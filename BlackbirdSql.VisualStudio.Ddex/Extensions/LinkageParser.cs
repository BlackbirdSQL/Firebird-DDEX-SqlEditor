// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)


using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TaskStatusCenter;

using FirebirdSql.Data.FirebirdClient;

using BlackbirdSql.Core;
using System.Diagnostics;

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
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Retrieves or creates the parser instance of a connection derived from Site
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static LinkageParser Instance(IVsDataConnection site, bool canCreate = true)
	{
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
	/// Retrieves or creates a distinct unique parser for a connection.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static new LinkageParser Instance(FbConnection connection, bool canCreate = true)
	{
		LinkageParser parser = (LinkageParser)AbstractLinkageParser.Instance(connection);

		if (canCreate)
			parser ??= new(connection);

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
				_Enabled = false;

			if (!_Enabled || asyncToken.IsCancellationRequested || !ConnectionActive)
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

		UpdateStatusBar(_LinkStage, AsyncActive);

		IVsTaskStatusCenterService tsc = ServiceProvider.GetGlobalServiceAsync<SVsTaskStatusCenterService,
			IVsTaskStatusCenterService>(swallowExceptions: false).Result;

		TaskHandlerOptions options = default;
		options.Title = $"{_Connection.DataSource} ({Path.GetFileNameWithoutExtension(_Connection.Database)}) sequence linkage";
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
		if (!_Enabled)
			UpdateStatusBar(_LinkStage, AsyncActive);

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

		if (!SyncActive && _LinkStage < EnumLinkStage.Completed && _Enabled)
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
		_Enabled = false;

		try
		{
			if (_AsyncTask == null || _AsyncTask.IsCompleted)
				return true;

			_AsyncTokenSource.Cancel();

			// _AsyncCardinal < 2: Async is still waiting in thread queue managed by UI thread so we flag
			// it to cancel and leave.
			// If we wait we'll deadlock because the launch is behind us.
			if (_AsyncCardinal < 2)
				return true;

			_SyncTokenSource = new();
			_SyncToken = _SyncTokenSource.Token;

			try
			{
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
				_AsyncTask.Wait(_SyncToken);
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
			}
			catch (Exception) { }

			_SyncTokenSource.Dispose();
			_SyncTokenSource = null;
			_SyncToken = default;

		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}

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
			Microsoft.VisualStudio.Data.DataProviderException ex = new("Connection disposed");
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
				Microsoft.VisualStudio.Data.DataProviderException ex = new("Connection closed");
				Diag.Dug(ex);
				throw ex;
			}
		}

		SyncEnter();

		UpdateStatusBar(_LinkStage, AsyncActive);

		IVsTaskStatusCenterService tsc = ServiceProvider.GetGlobalServiceAsync<SVsTaskStatusCenterService, IVsTaskStatusCenterService>(swallowExceptions: false).Result;

		TaskHandlerOptions options = default;
		options.Title = $"{_Connection.DataSource} ({Path.GetFileNameWithoutExtension(_Connection.Database)}) sequence linkage";
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
			ObjectDisposedException ex = new("Connection is null");
			Diag.Dug(ex);
			throw ex;
		}

		if (!ConnectionActive)
			return false;

		TaskHandlerProgress(GetPercentageComplete(_LinkStage), _LinkStage == EnumLinkStage.Start ? 0 : -1);

		if (_LinkStage < EnumLinkStage.GeneratorsLoaded)
		{
			GetRawGeneratorSchema();

			if (AsyncActive && userToken.IsCancellationRequested)
				_Enabled = false;

			TaskHandlerProgress("SELECT Generators", GetPercentageComplete(_LinkStage), Stopwatch.ElapsedMilliseconds);
		}


		if (AsyncActive && (!_Enabled || asyncToken.IsCancellationRequested))
			return false;

		if (!ConnectionActive)
			return false;


		if (_LinkStage < EnumLinkStage.TriggerDependenciesLoaded)
		{
			GetRawTriggerDependenciesSchema();

			if (AsyncActive && userToken.IsCancellationRequested)
				_Enabled = false;

			TaskHandlerProgress("SELECT TriggerDependencies", GetPercentageComplete(_LinkStage), Stopwatch.ElapsedMilliseconds);
		}

		if (AsyncActive && (!_Enabled || asyncToken.IsCancellationRequested))
			return false;

		if (!ConnectionActive)
			return false;

		if (_LinkStage < EnumLinkStage.TriggersLoaded)
		{
			GetRawTriggerSchema();

			if (AsyncActive && userToken.IsCancellationRequested)
				_Enabled = false;

			TaskHandlerProgress("SELECT Triggers", GetPercentageComplete(_LinkStage), Stopwatch.ElapsedMilliseconds);
		}

		if (AsyncActive && (!_Enabled || asyncToken.IsCancellationRequested))
			return false;


		if (!SequencesPopulated)
		{
			BuildSequenceTable();

			if (AsyncActive && userToken.IsCancellationRequested)
				_Enabled = false;

			TaskHandlerProgress("Populating Sequence Table", GetPercentageComplete(_LinkStage), Stopwatch.ElapsedMilliseconds);
		}


		if (AsyncActive && (!_Enabled || asyncToken.IsCancellationRequested))
			return false;

		if (_LinkStage < EnumLinkStage.Completed)
			BuildTriggerTable();

		TaskHandlerProgress($"Parsing and linking of {_Triggers.Rows.Count} Triggers", GetPercentageComplete(_LinkStage), Stopwatch.ElapsedMilliseconds);

		_Stopwatch = null;

		UpdateStatusBar(_LinkStage, AsyncActive);

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
	// ---------------------------------------------------------------------------------
	public bool SyncEnter()
	{
		_SyncCardinal++;

		try
		{
			if (_AsyncTask == null || _AsyncTask.IsCompleted)
				return true;

			_AsyncTokenSource.Cancel();

			// _AsyncCardinal < 2: Async is still waiting in thread queue managed by UI thread so we flag
			// it to cancel and leave.
			// If we wait we'll deadlock because the launch is behind us.
			if (_AsyncCardinal < 2)
				return true;

			_SyncTokenSource = new();
			_SyncToken = _SyncTokenSource.Token;

			try
			{
				_AsyncTask.Wait(_SyncToken);
			}
			catch (Exception) {}

			_SyncTokenSource.Dispose();
			_SyncTokenSource = null;
			_SyncToken = default;

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
		_SyncCardinal--;

		if (_SyncCardinal < 0)
		{
			InvalidOperationException ex = new("Attempt to exit a synchronous state when the object was not in a synchronous state");
			Diag.Dug(ex);
			throw ex;
		}

		if (_SyncCardinal > 0)
			return;

		_TaskHandler = null;
		_ProgressData = default;


		if (!AsyncActive && _LinkStage < EnumLinkStage.Completed && _Enabled)
		{
			if (_AsyncToken.IsCancellationRequested)
			{
				_AsyncTokenSource.Dispose();
				_AsyncTokenSource = new();
				_AsyncToken = _AsyncTokenSource.Token;
			}

			AsyncExecute();
		}

	}


	#endregion Methods


}
