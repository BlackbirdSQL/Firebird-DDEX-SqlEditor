// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)


using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using BlackbirdSql.Core;
using BlackbirdSql.VisualStudio.Ddex.Properties;

using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TaskStatusCenter;

using FirebirdSql.Data.FirebirdClient;
using BlackbirdSql.Core.Ctl.Extensions;
using BlackbirdSql.Core.Ctl.Diagnostics;

namespace BlackbirdSql.VisualStudio.Ddex.Extensions;




// =========================================================================================================
//										LinkageParser Class
//
/// <summary>
/// Builds the Trigger / Generator linkage tables.
/// </summary>
/// <remarks>
/// The class is split into 3 separate classes:
/// 1. AbstruseLinkageParser - handles the parsing,
/// 2. AbstractLinkageParser - handles the actual data table building and progess reporting, and
/// 3. LinkageParser - manages the sync/async build tasks.
/// The building task is executed async as <see cref="TaskCreationOptions.LongRunning"/> and
/// suspended whenever any UI main thread database tasks are requested for a particular connection,
/// and then resumed once the request is completed.
/// If a UI request requires the completed Trigger / Generator linkage tables the UI thread takes over
/// from the async thread to complete the build.
/// </remarks>
// =========================================================================================================
internal class LinkageParser : AbstractLinkageParser
{


	// -----------------------------------------------------------------------------------------------------
	#region Constructors - LinkageParser
	// -----------------------------------------------------------------------------------------------------


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Protected .ctor. LinkageParser's are uniquely distinct to a connection. Use the
	/// Instance() static to create or retrieve a parser for a connection or Site.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected LinkageParser(FbConnection connection) : base(connection)
	{
		Tracer.Trace(GetType(), "LinkageParser.LinkageParser(FbConnection)");
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// .ctor that clones a parser.
	/// Protected .ctor. LinkageParser's are uniquely distinct to a connection. Use the
	/// Instance() static to create or retrieve a parser for a connection or Site.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected LinkageParser(FbConnection connection, LinkageParser rhs) : base(connection, rhs)
	{
		Tracer.Trace(GetType(), "LinkageParser.LinkageParser(FbConnection, LinkageParser)");
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Retrieves or creates the parser instance of a connection derived from Site
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static LinkageParser Instance(IVsDataConnection site, bool canCreate)
	{
		Tracer.Trace(typeof(LinkageParser), "LinkageParser.Instance(IVsDataConnection, bool)", "canCreate: ", canCreate);

		if (site == null)
			return null;

		if (site.GetService(typeof(IVsDataConnectionSupport)) is not IVsDataConnectionSupport vsDataConnectionSupport)
			return null;

		if (vsDataConnectionSupport.ProviderObject is not FbConnection connection)
			return null;

		return Instance(connection, canCreate);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Finds a matching connection existing parser for a connection.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static LinkageParser EnsureInstance(FbConnection connection)
	{
		Tracer.Trace(typeof(LinkageParser), "LinkageParser.EnsureInstance(FbConnection)");
		return Instance(connection, true);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Retrieves an existing parser for a connection.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static new LinkageParser Instance(FbConnection connection)
	{
		Tracer.Trace(typeof(LinkageParser), "LinkageParser.Instance(FbConnection)");
		return Instance(connection, false);
	}


	// ---------------------------------------------------------------------------------
		/// <summary>
		/// Retrieves or creates a distinct unique parser for a connection.
		/// </summary>
		// ---------------------------------------------------------------------------------
	protected static LinkageParser Instance(FbConnection connection, bool canCreate)
	{
		Tracer.Trace(typeof(LinkageParser), "LinkageParser.Instance(FbConnection, bool)", "canCreate: ", canCreate);

		LinkageParser parser;

		parser = (LinkageParser)AbstractLinkageParser.Instance(connection);

		if (parser == null)
		{
			LinkageParser rhs = (LinkageParser)FindEquivalentParser(connection);

			if (rhs != null)
				parser = new(connection, rhs);
			else if (canCreate)
				parser = new(connection);
		}

		return parser;
	}


	#endregion Constructors





	// =========================================================================================================
	#region Methods - LinkageParser
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs delay in small timeslices to intermittently check if the user has not
	/// cancelled an async operation during the overall delay period of delay *
	/// multiplier.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected bool AsyncDelay(int delay, int multiplier, CancellationToken asyncToken,
		CancellationToken userToken)
	{
		if (delay == 0 || multiplier == 0)
			return true;


		for (int i = 0; i < multiplier; i++)
		{
			Thread.Sleep(delay);

			if (userToken.IsCancellationRequested)
				Enabled = false;

			if (!Enabled || asyncToken.IsCancellationRequested || !ConnectionActive)
			{
				return false;
			}
		}

		return true;
	}



	// Deadlock warning message suppression
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD002:Avoid problematic synchronous waits",
		Justification = "Code logic ensures a deadlock cannot occur")]

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Begins or resumes asynchronous linkage build operations.
	/// </summary>
	/// <param name="delay">
	/// The delay in milliseconds before beginning operations if an async build was
	/// initiated through the creation of a new data connection in the SE. A delay is
	/// required to allow the SE time to render the initial root node.
	/// </param>
	/// <param name="multiplier">
	/// A multiplier to split the delay into smaller timeslices and allow checking of
	/// cancellation tokens during the total delay time of 'delay * multiplier'.
	/// </param>
	/// <returns>True if the linkage was successfully completed, else false.</returns>
	// ---------------------------------------------------------------------------------
	public override bool AsyncExecute(int delay = 0, int multiplier = 1)
	{
		if (!ClearToLoadAsync)
			return false;

		_AsyncCardinal = 1;

		UpdateStatusBar(LinkStage, AsyncActive);

		IVsTaskStatusCenterService tsc = ServiceProvider.GetGlobalServiceAsync<SVsTaskStatusCenterService,
			IVsTaskStatusCenterService>(swallowExceptions: false).Result;

		TaskHandlerOptions options = default;
		options.Title = Resources.LinkageParserTaskHandlerTitle.Res($"{_Connection.DataSource} ({Path.GetFileNameWithoutExtension(_Connection.Database)})");
		options.ActionsAfterCompletion = CompletionActions.None;

		_ProgressData = default;
		_ProgressData.CanBeCanceled = true;

		_TaskHandler = tsc.PreRegister(options, _ProgressData);

		_AsyncTokenSource?.Dispose();
		_AsyncTokenSource = new();
		_AsyncToken = _AsyncTokenSource.Token;


		_AsyncTask = Task.Factory.StartNew(() =>
		{
			if (!_AsyncTokenSource.IsCancellationRequested)
			{
				try
				{
					_AsyncCardinal++;
					if (AsyncDelay(delay, multiplier, _AsyncToken, _TaskHandler.UserCancellation))
						PopulateLinkageTables(_AsyncToken, _TaskHandler.UserCancellation);
				}
				catch (Exception ex)
				{
					Diag.Dug(ex);
					throw;
				}
			}

			return AsyncExit();
		},
		default, TaskCreationOptions.LongRunning | TaskCreationOptions.AttachedToParent, TaskScheduler.Default);


		_TaskHandler.RegisterTask(_AsyncTask);


		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Tags the parser status as out of an asynchronous state.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected bool AsyncExit()
	{
		// User requested this cancellation so UpdateStatusBar will be placed on UI thread
		// queue, but as fire and forget so we're okay here.
		if (!Enabled)
			UpdateStatusBar(LinkStage, AsyncActive);

		_AsyncCardinal = 0;

		// _TaskHandler = null;
		// _ProgressData = default;


		// Ok so a little nasty here. Even though we (the async task) are run on a thread in the pool,
		// the actual thread manager process that carries us as a payload and launches us, does so on
		// the UI thread. So if a sync operation is executed it may wait for an async operation (us)
		// that has not yet launched, and the thread manager 'launcher' carrying us as a payload may be
		// behind the sync op on the UI thread.
		// So we may have been cancelled before we were actually launched and running and in the interim
		// a sync operation entered, cancelled us, executed and then on the way out should have restarted
		// us.
		// But on the way out the completed sync op could not relaunch us asynchronously because we were
		// a payload waiting to be launched by the thread manager on the ui thread.
		// So we were already cancelled before we were launched, and the sync op may be done and dusted.
		// In that case in reality we were a go to run. So in this case we'll launch ourselves again if
		// it's an all clear.
		// This scenario can easily be reproduced when we perform a GetCurrentMemory() or
		// GetActiveUsers() db sync request.

		if (!SyncActive && LinkStage < EnumLinkStage.Completed && Enabled)
		{
			if (_AsyncToken.IsCancellationRequested)
			{
				_AsyncTokenSource.Dispose();
				_AsyncTokenSource = new();
				_AsyncToken = _AsyncTokenSource.Token;
			}
			AsyncExecute();
		}

		_SyncTokenSource?.Cancel();

		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Disable future async operations and suspends any current async tasks.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public bool Disable()
	{
		SyncEnter(false);

		Enabled = false;

		SyncExit();

		return true;
	}



	// Deadlock warning message suppression
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD002:Avoid problematic synchronous waits",
		Justification = "Code logic ensures a deadlock cannot occur")]

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Launches the UI thread build of the linkage tables if the UI requires them. If
	/// an async build is in progress, waits for the active operation to complete and
	/// then switches over to a UI thread build for the remaining tasks.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override bool Execute()
	{
		if (!ClearToLoad)
			return false;


		if (_Connection == null)
		{
			Microsoft.VisualStudio.Data.DataProviderException ex = new(Resources.ExceptionConnectionDisposed);
			Diag.Dug(ex);
			throw ex;
		}

		if (!ConnectionActive)
		{
			// Try reset
			try
			{
				_Connection.Close();
				_Connection.Open();
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw;
			}

			if (!ConnectionActive)
			{
				Microsoft.VisualStudio.Data.DataProviderException ex = new(Resources.ExceptionConnectionClosed);
				Diag.Dug(ex);
				throw ex;
			}
		}

		SyncEnter(false);

		UpdateStatusBar(LinkStage, AsyncActive);

		IVsTaskStatusCenterService tsc = ServiceProvider.GetGlobalServiceAsync<SVsTaskStatusCenterService, IVsTaskStatusCenterService>(swallowExceptions: false).Result;

		TaskHandlerOptions options = default;
		options.Title = Resources.LinkageParserTaskHandlerTitle.Res($"{_Connection.DataSource} ({Path.GetFileNameWithoutExtension(_Connection.Database)})");
		options.ActionsAfterCompletion = CompletionActions.None;

		_ProgressData = default;
		_ProgressData.CanBeCanceled = false;

		_TaskHandler = tsc.PreRegister(options, _ProgressData);


		Task<bool> task = Task.Factory.StartNew(() =>
		{
			try
			{
				return PopulateLinkageTables();
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw;
			}
		},
			default, TaskCreationOptions.PreferFairness, TaskScheduler.Current);

		_TaskHandler.RegisterTask(task);

		task.Wait();

		SyncExit();


		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs the actual linkage build process.
	/// </summary>
	/// <param name="asyncLockedToken">
	/// Passing a cancellation token indicates that the call has been made asynchronously.
	/// </param>
	/// <returns>
	/// True if the method ran it's course else false.
	/// </returns>
	// ----------------------------------------------------------------------------------
	protected bool PopulateLinkageTables(CancellationToken asyncToken = default,
		CancellationToken userToken = default)
	{
		if (_Connection == null)
		{
			ObjectDisposedException ex = new(Resources.ExceptionConnectionNull);
			Diag.Dug(ex);
			throw ex;
		}

		if (!ConnectionActive)
			return false;

		TaskHandlerProgress(GetPercentageComplete(LinkStage), LinkStage == EnumLinkStage.Start ? 0 : -1);

		if (LinkStage < EnumLinkStage.GeneratorsLoaded)
		{
			TaskHandlerProgress(Resources.LinkageParserStageGeneratorsBegin, GetPercentageComplete(LinkStage + 1, true), -3);

			GetRawGeneratorSchema();

			if (AsyncActive && userToken.IsCancellationRequested)
				Enabled = false;

			TaskHandlerProgress(Resources.LinkageParserStageGeneratorsEnd, GetPercentageComplete(LinkStage), Stopwatch.ElapsedMilliseconds);
		}


		if (AsyncActive && (!Enabled || asyncToken.IsCancellationRequested))
			return false;

		if (!ConnectionActive)
			return false;


		if (LinkStage < EnumLinkStage.TriggerDependenciesLoaded)
		{
			TaskHandlerProgress(Resources.LinkageParserStageTriggerDependenciesStart, GetPercentageComplete(LinkStage + 1, true), -3);

			GetRawTriggerDependenciesSchema();

			if (AsyncActive && userToken.IsCancellationRequested)
				Enabled = false;

			TaskHandlerProgress(Resources.LinkageParserStageTriggerDependenciesEnd, GetPercentageComplete(LinkStage), Stopwatch.ElapsedMilliseconds);
		}

		if (AsyncActive && (!_Enabled || asyncToken.IsCancellationRequested))
			return false;

		if (!ConnectionActive)
			return false;

		if (LinkStage < EnumLinkStage.TriggersLoaded)
		{
			TaskHandlerProgress(Resources.LinkageParserStageTriggersBegin, GetPercentageComplete(LinkStage + 1, true), -3);

			GetRawTriggerSchema();

			if (AsyncActive && userToken.IsCancellationRequested)
				_Enabled = false;

			TaskHandlerProgress(Resources.LinkageParserStageTriggersEnd, GetPercentageComplete(LinkStage), Stopwatch.ElapsedMilliseconds);
		}

		if (AsyncActive && (!_Enabled || asyncToken.IsCancellationRequested))
			return false;


		if (!SequencesPopulated)
		{
			TaskHandlerProgress(Resources.LinkageParserStageSequencesBegin, GetPercentageComplete(LinkStage + 1, true), -3);

			BuildSequenceTable();

			if (AsyncActive && userToken.IsCancellationRequested)
				_Enabled = false;

			TaskHandlerProgress(Resources.LinkageParserStageSequencesEnd, GetPercentageComplete(LinkStage), Stopwatch.ElapsedMilliseconds);
		}


		if (AsyncActive && (!Enabled || asyncToken.IsCancellationRequested))
			return false;

		if (LinkStage < EnumLinkStage.Completed)
		{
			TaskHandlerProgress(Resources.LinkageParserStageLinkingBegin, GetPercentageComplete(LinkStage + 1, true), -3);

			BuildTriggerTable();

			TaskHandlerProgress(Resources.LinkageParserStageLinkingEnd, GetPercentageComplete(LinkStage), Stopwatch.ElapsedMilliseconds);
		}

		LinkStage = EnumLinkStage.Completed;

		TaskHandlerProgress(Resources.LinkageParserStageCompleted.Res(_Triggers.Rows.Count), GetPercentageComplete(LinkStage), Stopwatch.ElapsedMilliseconds);

		_Stopwatch = null;

		UpdateStatusBar(LinkStage, AsyncActive);

		return true;
	}



	// Deadlock warning message suppression
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD002:Avoid problematic synchronous waits",
		Justification = "Code logic ensures a deadlock cannot occur")]

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Increments the sync call counter and suspends any async tasks. This should be
	/// called on every occasion that a UI thread db call is made in the package.
	/// </summary>
	/// <remarks>
	/// This is tricky because although we're entering a sandboxed synchronous tunnel
	/// everything outside is async, including our own async process. This means we have
	/// to handle additional sync requests coming in. At any point the parsing might be
	/// completed. Further, if the linkage tables are required by a process, it will
	/// synchronously complete the linkage, so any new processes in the queue must check
	/// if their task has not been completed synchronously or asynchronously.
	/// </remarks>
	// ---------------------------------------------------------------------------------
	public bool SyncEnter(bool pausing)
	{
		lock (_LockObject)
		{
			_SyncCardinal++;

			if (Loaded)
				return true;

		}

		try
		{
			// There is another sync request ahead of us so we'll wait until it's done
			if (_SyncTask != null && !_SyncTask.IsCompleted)
			{
				try
				{
					_SyncTask.Wait(_SyncToken);
				}
				catch (Exception) { }
			}
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}

		try
		{
			lock (_LockObject)
			{
				// Again check if we're loaded
				if (Loaded)
					return true;

				// If there's no async running we have exclusive sync control
				if (_AsyncTask == null || _AsyncTask.IsCompleted)
					return true;

				// Async is active so request cancellation
				_AsyncTokenSource.Cancel();


				// Hold any other sync calls coming in
				_SyncTokenSource = new();
				_SyncToken = _SyncTokenSource.Token;
				_SyncTask = Task.FromCanceled<bool>(_SyncToken);
			}

			if (pausing)
				TaskHandlerProgress(GetPercentageComplete(LinkStage), -2);

			lock (_LockObject)
			{
				// _AsyncCardinal < 2: Async is still waiting in thread queue managed by UI thread so we flag
				// it to cancel and leave. Basically async is done but has not completed updated it's status.
				// If we wait we'll deadlock because the launch is behind us.
				if (_AsyncCardinal < 2)
					return true;
			}


			// Create a dummy while we wait
			_DummyTokenSource = new();
			_DummyToken = _DummyTokenSource.Token;

			try
			{
				_AsyncTask.Wait(_DummyToken);
			}
			catch (Exception) {}

			// Dispose of the dummy
			_DummyTokenSource.Dispose();
			_DummyTokenSource = null;

		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}

		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Decrements the sync call counter. This should be called on every occasion that
	/// a UI thread db call is exited in the package.
	/// When the counter reaches zero, outstanding async tasks are resumed.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public void SyncExit()
	{
		lock (_LockObject)
		{
			_SyncCardinal--;

			if (_SyncCardinal < 0)
			{
				InvalidOperationException ex = new(Resources.ExceptionAttemptToExitSyncFromAsync);
				Diag.Dug(ex);
				throw ex;
			}

			// If we're loaded and no token, exit
			if (Loaded && _SyncTokenSource == null)
				return;

			// If we're the only process in sync + entering sync
			if ((Loaded || _SyncCardinal == 0) && _SyncTokenSource != null)
			{
				// This is a cleanup.
				// If the token was previously cancelled any processes waiting on it
				// have gone through, so we can dispose, otherwise cancel it.
				if (_SyncTokenSource.IsCancellationRequested)
				{
					_SyncTokenSource.Dispose();
					_SyncTokenSource = null;
				}
				else
				{
					_SyncTokenSource.Cancel();
				}
			}

			// If there are others waiting behing us or we're loaded, we're done.
			if (_SyncCardinal > 0 || Loaded)
			{
				// In all other case we are the process that enabled the token so we can cancel
				// to let the next through.
				_SyncTokenSource?.Cancel();
				return;
			}
		}


		// If we reached here it means we must restart the async process.


		_TaskHandler = null;
		_ProgressData = default;


		if (!AsyncActive && LinkStage < EnumLinkStage.Completed && Enabled)
		{
			if (_AsyncToken.IsCancellationRequested)
			{
				_AsyncTokenSource.Dispose();
				_AsyncTokenSource = new();
				_AsyncToken = _AsyncTokenSource.Token;
			}

			// Should be no one behind us
			AsyncExecute();
		}

		// Lastly we can green light any processes that may just have come in
		// behind us.
		_SyncTokenSource?.Cancel();


	}


	#endregion Methods


}
