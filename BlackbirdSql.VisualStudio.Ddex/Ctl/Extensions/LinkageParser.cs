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
using BlackbirdDsl;
using BlackbirdSql.VisualStudio.Ddex.Model;
using System.Threading.Tasks.Dataflow;
using Microsoft.VisualStudio.Shell.Interop;
using System.Windows.Forms;
using System.Windows.Media.Animation;

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
		// Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}] _LinkageParser(FbConnection)");
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
		// Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}] _LinkageParser(FbConnection, LinkageParser)");
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Retrieves or creates the parser instance of a connection derived from Site.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static LinkageParser EnsureInstance(IVsDataConnection site)
	{
		// Tracer.Trace(typeof(LinkageParser), $"StaticId:[{"0000"}] EnsureInstance(IVsDataConnection)");
		return Instance(site, true);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Retrieves an existing parser for a connection derived from Site.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static LinkageParser Instance(IVsDataConnection site)
	{
		// Tracer.Trace(typeof(LinkageParser), $"StaticId:[{"0000"}] Instance(FbConnection)");
		return Instance(site, false);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Retrieves or creates the parser instance of a connection derived from Site
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected static LinkageParser Instance(IVsDataConnection site, bool canCreate)
	{
		// Tracer.Trace(typeof(LinkageParser), $"StaticId:[{"0000"}] Instance(IVsDataConnection, bool)", "canCreate: {0}", canCreate);

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
	/// Retrieves or creates the parser instance of a connection.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static LinkageParser EnsureInstance(FbConnection connection)
	{
		// Tracer.Trace(typeof(LinkageParser), $"StaticId:[{"0000"}] EnsureInstance(FbConnection)");
		return Instance(connection, true);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Retrieves an existing parser for a connection.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static new LinkageParser Instance(FbConnection connection)
	{
		// Tracer.Trace(typeof(LinkageParser), $"StaticId:[{"0000"}] Instance(FbConnection)");
		return Instance(connection, false);
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Retrieves or creates a distinct unique parser for a connection.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected static LinkageParser Instance(FbConnection connection, bool canCreate)
	{
		// Tracer.Trace(typeof(LinkageParser), $"StaticId:[{"0000"}] Instance(FbConnection, bool)", "canCreate: {0}", canCreate);

		LinkageParser parser;

		lock (_LockObject)
		{
			parser = (LinkageParser)AbstractLinkageParser.Instance(connection);

			if (parser == null)
			{
				LinkageParser rhs = (LinkageParser)FindEquivalentParser(connection);

				if (rhs != null)
				{
					parser = new(connection, rhs)
					{
						_InstanceId = _InstanceSeed < 1990 ? ++_InstanceSeed : 1001
					};
					Tracer.Trace(typeof(LinkageParser), $"StaticId:[{"0000"}] Instance(FbConnection, bool)", "Found parser to clone. canCreate: {0}", canCreate);
				}
				else if (canCreate)
				{
					Tracer.Trace(typeof(LinkageParser), $"StaticId:[{"0000"}] Instance(FbConnection, bool)", "Created new parser. canCreate: {0}", canCreate);
					parser = new(connection)
					{
						_InstanceId = _InstanceSeed < 1990 ? ++_InstanceSeed : 1001
					};
					Tracer.Trace(typeof(LinkageParser), $"StaticId:[{"0000"}] Instance(FbConnection, bool)", "Created new Parser. canCreate: {0}", canCreate);
				}

				if (parser != null && S_SeToolHWnd == IntPtr.Zero)
				{
					try
					{
						parser.SetSeToolWindow();
					}
					catch (Exception ex)
					{
						Diag.Dug(ex);
						throw;
					}
				}
			}
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
		for (int i = 0; i < multiplier; i++)
		{
			Thread.Sleep(delay);


			if (userToken.IsCancellationRequested || asyncToken.IsCancellationRequested || !ConnectionActive)
			{
				return false;
			}
		}

		return true;
	}




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
		{
			Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}] _AsyncExecute", "ENTER and exit - !ClearToLoadAsync - _AsyncCardinal: {0}", _AsyncCardinal);
			return false;
		}

		int asyncProcessId = _AsyncProcessSeed < 9990 ? ++_AsyncProcessSeed : 9001;

		Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}->A{asyncProcessId}] _AsyncExecute", "ENTER - ClearToLoadAsync - _AsyncCardinal: {0}, IsUiThread: {1}.", _AsyncCardinal, ThreadHelper.CheckAccess());

		_AsyncCardinal = 1;


		UpdateStatusBar(_LinkStage, AsyncActive);

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
			ExecuteAsyncTask(asyncProcessId, _AsyncToken, _TaskHandler.UserCancellation, delay, multiplier),
			default, TaskCreationOptions.LongRunning | TaskCreationOptions.AttachedToParent,
			TaskScheduler.Default);


		_TaskHandler.RegisterTask(_AsyncTask);

		Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}->A{asyncProcessId}] _AsyncExecute", "EXIT - AsyncTask registered - _AsyncCardinal: {0}", _AsyncCardinal);


		return true;
	}

	protected bool ExecuteAsyncTask(int asyncProcessId, CancellationToken asyncToken,
		CancellationToken userCancellation, int delay, int multiplier)
	{
		bool result = false;

		try
		{
			Tracer.Trace("<AsyncExecute>._AsyncTask", $"ParserId:[{_InstanceId}->A{asyncProcessId}] _AsyncExecute:AsyncTask", "ENTER - userCancellation.IsCancellationRequested: {0}", userCancellation.IsCancellationRequested);

			if (!asyncToken.IsCancellationRequested && !userCancellation.IsCancellationRequested)
			{
				Tracer.Trace("<AsyncExecute>._AsyncTask", $"ParserId:[{_InstanceId}->A{asyncProcessId}] _AsyncExecute:AsyncTask", "Ready to populate - _AsyncCardinal: {0}, IsUiThread: {1}.", _AsyncCardinal, ThreadHelper.CheckAccess());
				_AsyncCardinal++;

				if (AsyncDelay(delay, multiplier, asyncToken, userCancellation))
					_ = PopulateLinkageTables(asyncToken, userCancellation, "AsyncId", asyncProcessId);
				else if (userCancellation.IsCancellationRequested)
					_Enabled = false;
			}
			else
			{
				Tracer.Trace("<AsyncExecute>._AsyncTask", $"ParserId:[{_InstanceId}->A{asyncProcessId}] _AsyncExecute:AsyncTask", "Populate failed because _AsyncTokenSource.IsCancellationRequested - _AsyncCardinal: {0}", _AsyncCardinal);
			}

		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;
		}
		finally
		{
			Tracer.Trace("<AsyncExecute>._AsyncTask", $"ParserId:[{_InstanceId}->A{asyncProcessId}] _AsyncExecute:AsyncTask", "Finally calling AsyncExit - _AsyncCardinal: {0}", _AsyncCardinal);
			AsyncExit(asyncProcessId, asyncToken, userCancellation);
		}


		return result;
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Tags the parser status as out of an asynchronous state.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected bool AsyncExit(int asyncProcessId, CancellationToken asyncToken, CancellationToken userCancellation)
	{
		// User requested this cancellation so UpdateStatusBar will be placed on UI thread
		// queue, but as fire and forget so we're okay here.
		if (!_Enabled)
		{
			Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}->A{asyncProcessId}] _AsyncExit", "ENTER and !_Enabled - _SyncWaitAsyncTokenSource?.Cancel - _AsyncCardinal: {0}", _AsyncCardinal);
			UpdateStatusBar(_LinkStage, AsyncActive);
		}
		else
			Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}->A{asyncProcessId}] _AsyncExit", "ENTER and _Enabled - _SyncWaitAsyncTokenSource?.Cancel - _AsyncCardinal: {0}", _AsyncCardinal);


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
			if (asyncToken.IsCancellationRequested)
			{
				Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}->A{asyncProcessId}] _AsyncExit", "_AsyncToken.IsCancellationRequested - _AsyncTokenSource renew - _AsyncCardinal: {0}", _AsyncCardinal);
				_AsyncTokenSource.Dispose();
				_AsyncTokenSource = new();
				_AsyncToken = _AsyncTokenSource.Token;
			}
			Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}->A{asyncProcessId}] _AsyncExit", "Re-executing AsyncExecute() - _AsyncCardinal: {0}", _AsyncCardinal);
			_AsyncCardinal = 0;
			AsyncExecute();
		}
		else
		{
			Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}->A{asyncProcessId}] _AsyncExit", "EXIT resetting and _SyncWaitAsyncTokenSource?.Cancel() - _AsyncCardinal: {0}", _AsyncCardinal);
			_AsyncCardinal = 0;
			_SyncWaitAsyncTokenSource?.Cancel();
		}

		Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}->A{asyncProcessId}] _AsyncExit", "EXIT - _AsyncCardinal: {0}", _AsyncCardinal);

		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Disable future async operations and suspends any current async tasks.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public bool Disable()
	{
		Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}] _Disable()");

		int syncCardinal = SyncEnter(true);

		_Enabled = false;

		SyncExit(syncCardinal);

		return true;
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Launches the UI thread build of the linkage tables if the UI requires them.
	/// If an async build is in progress, waits for the active operation to complete and
	/// then switches over to a UI thread build for the remaining tasks.
	/// </summary>
	/// <returns>True if successfully loaded or already loaded else false</returns>
	// ---------------------------------------------------------------------------------
	protected override bool EnsureLoaded()
	{
		Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}] _EnsureLoaded not pausing, executing");
		return SyncExecute();
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Launches the UI thread build of the linkage tables if the UI requires them. If
	/// an async build is in progress, waits for the active operation to complete and
	/// then switches over to a UI thread build for the remaining tasks.
	/// </summary>
	/// <returns>True if successfully executed or already loaded else false</returns>
	// ---------------------------------------------------------------------------------
	protected override bool SyncExecute()
	{
		if (!ClearToLoadSync)
		{
			Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}] _SyncExecute", "ENTER and EXIT - Loaded or !_Enabled - _SyncCardinal: {0}", _SyncCardinal);
			return true;
		}

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


		int syncCardinal = SyncEnter(false);

		if (syncCardinal >= 0)
		{
			Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}->S{syncCardinal}]  _SyncExecute", "Exiting - SyncEnter returned loaded}");
			return true;
		}

		Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}->S{syncCardinal}]  _SyncExecute", "Ready to prepare for sync execution");


		UpdateStatusBar(_LinkStage, AsyncActive);

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
				if (!ClearToLoadSync)
				{
					Tracer.Trace("<AsyncExecute>.task", $"ParserId:[{_InstanceId}->S {syncCardinal}]  _SyncExecute", "Thread aborted PopulateLinkageTables() because ClearToLoadSync state change");
					return false;
				}
				Tracer.Trace("<AsyncExecute>.task", $"ParserId:[{_InstanceId}->S {syncCardinal}]  _SyncExecute", "Calling PopulateLinkageTables()");
				bool result = PopulateLinkageTables(default, default, "SyncCardinal", syncCardinal);
				Tracer.Trace("<AsyncExecute>.task", $"ParserId:[{_InstanceId}->S {syncCardinal}] _SyncExecute", "Done PopulateLinkageTables() Success: {0}", result.ToString());
				return result;
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw;
			}
		},
		default, TaskCreationOptions.PreferFairness, TaskScheduler.Current);

		_TaskHandler.RegisterTask(task);

		Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}->S{syncCardinal}]  _SyncExecute", "Sync thread for PopulateLinkageTables() loaded. Waiting for it to complete...");

		task.Wait();

		Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}->S{syncCardinal}]  _SyncExecute", "Done waiting for thread - calling SyncExit.");

		SyncExit(syncCardinal);

		Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}->S{syncCardinal}]  _SyncExecute", "EXIT - SyncExit done.");


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
	protected bool PopulateLinkageTables(CancellationToken asyncToken,
		CancellationToken userToken, string idType, int id)
	{
		if (_Connection == null)
		{
			ObjectDisposedException ex = new(Resources.ExceptionConnectionNull);
			Diag.Dug(ex);
			throw ex;
		}

		if (!ConnectionActive)
		{
			Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}] {idType}[{id}] _PopulateLinkageTables", "ENTER and exit - !ConnectionActive - SyncCardinal: {0}", _SyncCardinal);
			return false;
		}


		Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}] {idType}[{id}] _PopulateLinkageTables", "ENTER - _LinkStage: {0}", _LinkStage);
		TaskHandlerProgress(GetPercentageComplete(_LinkStage), _LinkStage == EnumLinkStage.Start ? 0 : -1);

		if (_LinkStage < EnumLinkStage.GeneratorsLoaded)
		{
			TaskHandlerProgress(Resources.LinkageParserStageGeneratorsBegin, GetPercentageComplete(_LinkStage + 1, true), -3);

			GetRawGeneratorSchema();

			if (AsyncActive && userToken.IsCancellationRequested)
				_Enabled = false;

			TaskHandlerProgress(Resources.LinkageParserStageGeneratorsEnd, GetPercentageComplete(_LinkStage), Stopwatch.ElapsedMilliseconds);
		}


		if (AsyncActive && (!_Enabled || asyncToken.IsCancellationRequested))
			return false;

		if (!ConnectionActive)
			return false;


		if (_LinkStage < EnumLinkStage.TriggerDependenciesLoaded)
		{
			TaskHandlerProgress(Resources.LinkageParserStageTriggerDependenciesStart, GetPercentageComplete(_LinkStage + 1, true), -3);

			GetRawTriggerDependenciesSchema();

			if (AsyncActive && userToken.IsCancellationRequested)
				_Enabled = false;

			TaskHandlerProgress(Resources.LinkageParserStageTriggerDependenciesEnd, GetPercentageComplete(_LinkStage), Stopwatch.ElapsedMilliseconds);
		}

		if (AsyncActive && (!_Enabled || asyncToken.IsCancellationRequested))
			return false;

		if (!ConnectionActive)
			return false;

		if (_LinkStage < EnumLinkStage.TriggersLoaded)
		{
			TaskHandlerProgress(Resources.LinkageParserStageTriggersBegin, GetPercentageComplete(_LinkStage + 1, true), -3);

			GetRawTriggerSchema();

			if (AsyncActive && userToken.IsCancellationRequested)
				_Enabled = false;

			TaskHandlerProgress(Resources.LinkageParserStageTriggersEnd, GetPercentageComplete(_LinkStage), Stopwatch.ElapsedMilliseconds);
		}

		if (AsyncActive && (!_Enabled || asyncToken.IsCancellationRequested))
			return false;


		if (!SequencesPopulated)
		{
			TaskHandlerProgress(Resources.LinkageParserStageSequencesBegin, GetPercentageComplete(_LinkStage + 1, true), -3);

			BuildSequenceTable();

			if (AsyncActive && userToken.IsCancellationRequested)
				_Enabled = false;

			TaskHandlerProgress(Resources.LinkageParserStageSequencesEnd, GetPercentageComplete(_LinkStage), Stopwatch.ElapsedMilliseconds);
		}


		if (AsyncActive && (!_Enabled || asyncToken.IsCancellationRequested))
			return false;

		if (_LinkStage < EnumLinkStage.Completed)
		{
			TaskHandlerProgress(Resources.LinkageParserStageLinkingBegin, GetPercentageComplete(_LinkStage + 1, true), -3);

			BuildTriggerTable();

			TaskHandlerProgress(Resources.LinkageParserStageLinkingEnd, GetPercentageComplete(_LinkStage), Stopwatch.ElapsedMilliseconds);
		}

		_LinkStage = EnumLinkStage.Completed;

		TaskHandlerProgress(Resources.LinkageParserStageCompleted.Res(_Triggers.Rows.Count), GetPercentageComplete(_LinkStage), Stopwatch.ElapsedMilliseconds);

		_Stopwatch = null;

		UpdateStatusBar(_LinkStage, AsyncActive);

		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Increments the sync call counter and suspends any async tasks. This should be
	/// called on every occasion that a UI thread db call is made in the package.
	/// </summary>
	/// <param name="pausing">
	/// Updates output pane and status bar with "pausing" message. Set this to true if
	/// linkage tables will not be required.
	/// </param>
	/// <returns>
	/// Negative thread id if trigger tables must still be loaded else positive if complete
	/// </returns>
	/// <remarks>
	/// This is tricky because although we're entering a sandboxed synchronous tunnel
	/// everything outside is async, including our own async process. This means we have
	/// to handle additional sync requests coming in. At any point the parsing might be
	/// completed. Further, if the linkage tables are required by a process, it will
	/// synchronously complete the linkage, so any new processes in the queue must check
	/// if their task has not been completed synchronously or asynchronously.
	/// </remarks>
	// ---------------------------------------------------------------------------------
	public int SyncEnter(bool pausing)
	{
		// Sanity check.
		// If async is active and _SyncCardinal > 0 it means we have a sync process ahead of
		// us waiting for async to complete.
		// We're the sync process so how can we be ahead of us???
		if (_SyncCardinal > 0 && _AsyncTask != null && !_AsyncTask.IsCompleted && _AsyncCardinal > 1)
		{
			InvalidOperationException ex = new($"A deadlock has occured: _SyncCardinal: {_SyncCardinal}");
			Diag.Dug(ex);
			throw ex;
		}


		lock (_LocalObject)
		{

			// If there's no one home just exit.

			if (_SyncCardinal == 0 && !Incomplete)
			{

				if (!AsyncActive && _AsyncTask != null && _AsyncTask.IsCompleted)
				{
					_AsyncTask.Dispose();
					_AsyncTokenSource.Dispose();
					_AsyncTokenSource = null;
					_AsyncTask = null;
				}

				Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}] _SyncEnter", "ENTER/EXIT SyncEnter - _SyncCardinal: {0} pausing: {1}, IsUiThread: {2}.", _SyncCardinal, pausing, ThreadHelper.CheckAccess());

				return 0;
			}

			// Increment the sync processes count.
			_SyncCardinal++;

			// If this is a recursive sync call or async is not active we have exclusive sync control
			// and can exit.

			if (_SyncCardinal > 1 || !AsyncActive)
			{
				Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}->S{_SyncCardinal}] _SyncEnter", "Cleared - Loaded or no _AsyncTask or _AsyncTask.IsCompleted");
				return !Incomplete ? _SyncCardinal : -_SyncCardinal;
			}
		}

		// Async is active. We have to wait.

		try
		{
			// Async is active so request cancellation. 
			_AsyncTokenSource.Cancel();

			if (pausing)
				TaskHandlerProgress(GetPercentageComplete(_LinkStage), -2);

			if (_AsyncCardinal < 2)
			{
				// _AsyncCardinal < 2: Async is still waiting in thread queue managed by UI thread so we flag
				// it to cancel and leave.
				// If we wait we'll deadlock because the launch is behind us.

				Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}->S{_SyncCardinal}] _SyncEnter", "Async task has not launched it's payload yet. We can exit.");

				return !Incomplete ? _SyncCardinal : -_SyncCardinal;
			}

			// Create a wait-on-async while we wait
			_SyncWaitAsyncTokenSource?.Dispose();
			_SyncWaitAsyncTokenSource = new();
			_SyncWaitAsyncToken = _SyncWaitAsyncTokenSource.Token;

			Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}->S {_SyncCardinal}] _SyncEnter", "Waiting _AsyncTask.Wait - _AsyncCardinal: {0} isFaulted: {1}.", _AsyncCardinal, _AsyncTask.IsFaulted.ToString());

			try
			{
				_AsyncTask.Wait(_SyncWaitAsyncToken);
			}
			catch (OperationCanceledException ex)
			{
				Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}->S {_SyncCardinal}] _SyncEnter", "_SyncWaitAsyncTask.Cancelled {0}", ex.Message);
			}

			Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}->S {_SyncCardinal}] _SyncEnter", "Released _AsyncTask.Wait - _AsyncCardinal: {0}", _AsyncCardinal);

			// We have exclusive control.
			// Dispose of the wait-on-async token
			_SyncWaitAsyncTokenSource?.Dispose();
			_SyncWaitAsyncTokenSource = null;
			_SyncWaitAsyncToken = default;

		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}

		Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}->S{_SyncCardinal}] _SyncEnter", "EXIT after Wait and exiting - _AsyncCardinal: {0}", _AsyncCardinal);


		return !Incomplete ? _SyncCardinal : -_SyncCardinal;

		// Once we're done with our sync processing we'll release the next sync process if it
		// exists, else restart the async process if it's not completed.
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Decrements the sync call counter. This should be called on every occasion that
	/// a UI thread db call is exited in the package.
	/// When the counter reaches zero, outstanding async tasks are resumed.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public void SyncExit(int syncCardinal)
	{
		Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}->S{syncCardinal}]  _SyncExit", "ENTER SyncExit");

		EnableSeWindow(true);


		lock (_LocalObject)
		{
			// If this is zero we know for sure everything was loaded or disabled so
			// we never actually entered into sync mode because it wasn't necessary.
			if (_SyncCardinal == 0)
				return;

			// If there are others in the queue release and exit
			if (_SyncCardinal > 1)
			{
				_SyncCardinal--;
				return;
			}

			if (_AsyncTask != null && _AsyncTask.IsCompleted)
			{
				_AsyncTask?.Dispose();
				_AsyncTokenSource?.Dispose();
				_AsyncTokenSource = null;
				_AsyncTask = null;
			}


			lock (_LocalObject)
			{
				_SyncCardinal--;

				// If we don't have to restart the async exit.
				if (!Incomplete)
					return;
			}


			// Should be no one behind us
			Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}->S{syncCardinal}]  _SyncExit", "Restarting AsyncExecute");
			AsyncExecute();
			Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}->S{syncCardinal}]  _SyncExi4t", "EXIT");
		}


	}


	#endregion Methods


}
