// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using BlackbirdSql.Core.Controls;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Model.Enums;
using BlackbirdSql.Core.Properties;
using FirebirdSql.Data.FirebirdClient;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;


namespace BlackbirdSql.Core.Model;

// =========================================================================================================
//										LinkageParser Class
//
/// <summary>
/// Builds the Trigger / Generator linkage tables.
/// </summary>
/// <remarks>
/// The class is split into 3 separate classes:
/// 1. AbstruseLinkageParser - handles the parsing,
/// 2. AbstractLinkageParser - handles the actual data table building, and
/// 3. LinkageParser - manages the sync/async build tasks and progess reporting.
/// The building task is executed async as <see cref="TaskCreationOptions.LongRunning"/> and
/// suspended whenever any UI main thread database tasks are requested for a particular connection,
/// and then resumed once the request is completed.
/// If a UI request requires the completed Trigger / Generator linkage tables the UI thread takes over
/// from the async thread to complete the build.
/// </remarks>
// =========================================================================================================
public class LinkageParser : AbstractLinkageParser
{




	// -----------------------------------------------------------------------------------------------------
	#region Constructors - LinkageParser
	// -----------------------------------------------------------------------------------------------------


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Protected .ctor. for creating an unregistered clone.
	/// Callers must make a call to EnsureLoaded() for rhs beforehand.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private LinkageParser(LinkageParser rhs) : base(rhs)
	{
		// Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}] _LinkageParser(FbConnection)");
	}

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Protected .ctor. LinkageParser's are uniquely distinct to a connection. Use the
	/// Instance() static to create or retrieve a parser for a connection.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private LinkageParser(FbConnection connection) : this(connection, null)
	{
		// Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}] _LinkageParser(FbConnection)");
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// .ctor that clones a parser.
	/// Protected .ctor. LinkageParser's are uniquely distinct to a connection. Use the
	/// Instance() static to create or retrieve a parser for a connection.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private LinkageParser(FbConnection connection, LinkageParser rhs) : base(connection, rhs)
	{
		// Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}] _LinkageParser(FbConnection, LinkageParser)");
		_TaskHandler = new(_InstanceConnection);
	}



	/// <summary>
	/// Creates an unregistered clone of a parser.
	/// </summary>
	/// <returns></returns>
	protected override object Clone()
	{
		if (_Enabled && Loaded)
			return new LinkageParser(this);
		else
			return new LinkageParser(_InstanceConnection);
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// This is the universal _Instance access point.
	/// Retrieves or creates a distinct unique parser for a connection.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private static LinkageParser CreateInstance(FbConnection connection, bool canCreate)
	{
		// Tracer.Trace(typeof(LinkageParser), "CreateInstance(FbConnection, bool)", "canCreate: {0}. This static or GetInstance() will always provide a trace of it's result.", canCreate);

		LinkageParser parser;

		lock (_LockClass)
		{
			parser = (LinkageParser)AbstractLinkageParser.GetInstance(connection);


			if (parser == null)
			{
				LinkageParser rhs = (LinkageParser)FindEquivalentParser(connection);

				if (rhs != null)
					parser = new(connection, rhs);
				else if (canCreate)
					parser = new(connection);
			}
		}

		return parser;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Retrieves or creates the parser instance of a connection.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static LinkageParser EnsureInstance(FbConnection connection, Type schemaFactoryType)
	{
		// Tracer.Trace(typeof(LinkageParser), "EnsureInstance(FbConnection, Type)");

		_SchemaFactoryType = schemaFactoryType;
		return CreateInstance(connection, true);
	}




	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Retrieves an existing parser for a connection.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static new LinkageParser GetInstance(FbConnection connection)
	{
		// Tracer.Trace(typeof(LinkageParser), "GetInstance(FbConnection)");

		return CreateInstance(connection, false);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Destructor.
	/// </summary>
	// ---------------------------------------------------------------------------------
	~LinkageParser()
	{
		_AsyncLauncherTokenSource?.Cancel();
		_AsyncLauncherTokenSource?.Dispose();
		_SyncWaitOnAsyncTokenSource?.Cancel();
		_SyncWaitOnAsyncTokenSource?.Dispose();
		_TransientParser = null;
	}


	#endregion Constructors





	// =========================================================================================================
	#region Fields - LinkageParser
	// =========================================================================================================


	// Threading / multi process control variables.
	// A quick double tap on an unopened SE connection can cause a deadlong hang.
	// This happens when the async task (payload) launcher task, _AsyncPayloadLauncher, is created, and then
	// a sync process returns and needs to terminate it in order to execute sync db commands. (Note: The Fb
	// client cannot execute sync commands simultaneously with an async process.) Cancelling and placing a
	// wait on the async launcher process will cause a deadlock because _AsyncPayloadLauncher queues the launching
	// of it's payload in behind the current sync process.
	// To overcome this we signal a cancel to the launcher, and then treat it as "launch cancelled", because we
	// know it will cancel the payload launch once it is clear to execute.


	// The async process thread id / index for tracing
	private static int _AsyncProcessSeed = 90000;

	private int _TransientDelay = 0, _TransientMultiplier = 0;

	/// <summary>
	/// The async process state, 0 = No async process, 1 = Async launch queued, 2 = Async Active.
	/// </summary>
	private EnLauncherPayloadLaunchState _AsyncPayloadLaunchState = EnLauncherPayloadLaunchState.Inactive;

	/// <summary>
	/// The async launcher task if it exists.
	/// </summary>
	private Task<bool> _AsyncPayloadLauncher;
	private CancellationToken _AsyncLauncherToken;
	private CancellationTokenSource _AsyncLauncherTokenSource = null;

	private readonly LinkageParserTaskHandler _TaskHandler = null;

	/// <summary>
	/// The number of sync tasks that have requested and been granted exclusive sync access.
	/// This will be the initial task + subsequent recursive sync requests.
	/// </summary>
	private int _SyncCardinal = 0;

	/// <summary>
	/// This is the token the async payload launcher task/process signals when it exits.
	/// </summary>
	private CancellationToken _SyncWaitOnAsyncToken;
	private CancellationTokenSource _SyncWaitOnAsyncTokenSource = null;



	#endregion Fields




	// =========================================================================================================
	#region Property accessors - LinkageParser
	// =========================================================================================================


	private bool AsyncActive => _AsyncPayloadLaunchState != EnLauncherPayloadLaunchState.Inactive;

	/// <summary>
	/// Getter inidicating whether or not async linkage can and should begin or resume operations.
	/// </summary>
	private bool ClearToLoadAsync => _Enabled && !AsyncActive && !Loaded && !SyncActive;

	/// <summary>
	/// Getter indicating wether or not the UI thread can and should resume linkage operations.
	/// </summary>
	private bool ClearToLoadSync => !Loaded && _Enabled;

	/// <summary>
	/// Getter indicating whether or not linkage is still required. Incomplete
	/// differs from !Loaded in that in that !Incomplete may be because the linker
	/// has been disabled.
	/// </summary>
	private bool Incomplete => _LinkStage < EnLinkStage.Completed && _Enabled;

	private bool SyncActive => _SyncCardinal != 0;


	#endregion Property accessors





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
	private bool AsyncDelay(int delay, int multiplier, CancellationToken asyncCancellationToken,
		CancellationToken userToken)
	{
		// Tracer.Trace(GetType(), "AsyncDelay()", "Entering - delay: {0}, multiplier: {1}, UIThread? {2}.", delay, multiplier, ThreadHelper.CheckAccess());

		lock (_LockObject)
			_AsyncPayloadLaunchState = EnLauncherPayloadLaunchState.Launching;

		int i;

		for (i = 0; i < multiplier; i++)
		{
			if (userToken.IsCancellationRequested || asyncCancellationToken.IsCancellationRequested
				|| !ConnectionActive || _AsyncPayloadLauncher == null)
			{
				break;
			}


			try
			{
				_AsyncPayloadLauncher.Wait(delay, asyncCancellationToken);
			}
			catch // (OperationCanceledException ex)
			{
			}
		}

		if (i < multiplier)
		{
			_TransientDelay = delay;
			_TransientMultiplier = multiplier;
		}
		else
		{
			_TransientDelay = 0;
			_TransientMultiplier = 0;
		}

		// Tracer.Trace(GetType(), "AsyncDelay()", "Exiting - delay: {0}, multiplier: {1}, UIThread? {2}.", delay, multiplier, ThreadHelper.CheckAccess());


		return !userToken.IsCancellationRequested && !asyncCancellationToken.IsCancellationRequested
			&& ConnectionActive && _AsyncPayloadLauncher != null;
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
	public override bool AsyncExecute(int delay = 0, int multiplier = 0)
	{
		_TransientDelay = 0;
		_TransientMultiplier = 0;

		if (!ClearToLoadAsync)
			return false;

		int asyncProcessId = _AsyncProcessSeed < 99990 ? ++_AsyncProcessSeed : 90001;
		_AsyncProcessSeed = asyncProcessId;

		// Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}->A{asyncProcessId}] AsyncExecute()", "ENTER - ClearToLoadAsync - _AsyncPayloadLaunchState: {0}, IsUiThread: {1}.", _AsyncPayloadLaunchState, ThreadHelper.CheckAccess());


		lock (_LockObject)
		{
			_AsyncPayloadLaunchState = EnLauncherPayloadLaunchState.Pending;
			_AsyncLauncherTokenSource?.Dispose();
			_AsyncLauncherTokenSource = new();
			_AsyncLauncherToken = _AsyncLauncherTokenSource.Token;
		}


		_TaskHandler.Status(_LinkStage, _TotalElapsed, _Enabled, AsyncActive);
		_TaskHandler.PreRegister(true);


		// The following for brevity.
		CancellationToken asyncCancellationToken = _AsyncLauncherToken;
		CancellationToken userCancellationToken = _TaskHandler.UserCancellation;
		TaskCreationOptions creationOptions = TaskCreationOptions.LongRunning | TaskCreationOptions.AttachedToParent;
		TaskScheduler scheduler = TaskScheduler.Default;

		// Tracer.Trace(GetType(), "AsyncExecute()", "delay: {0}, multiplier: {1}, UIThread? {2}.", delay, multiplier, ThreadHelper.CheckAccess());


		// For brevity.
		bool payload() =>
			PayloadAsyncExecute(asyncProcessId, asyncCancellationToken, userCancellationToken, delay, multiplier);

		// Start up the payload launcher with tracking.
		_AsyncPayloadLauncher = Task.Factory.StartNew(payload, default, creationOptions, scheduler);

		_TaskHandler.RegisterTask(_AsyncPayloadLauncher);

		// Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}->A{asyncProcessId}] AsyncExecute()", "EXIT - AsyncTask registered - _AsyncPayloadLaunchState: {0}", _AsyncPayloadLaunchState);


		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Tags the parser status as out of an asynchronous state.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private bool AsyncExit(int asyncProcessId, CancellationToken asyncCancellationToken, CancellationToken userCancellationToken)
	{
		// User requested this cancellation so UpdateStatusBar will be placed on UI thread
		// queue, but as fire and forget so we're okay here.
		if (!_Enabled)
		{
			// Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}->A{asyncProcessId}] AsyncExit()", "ENTER and !_Enabled - _SyncWaitOnAsyncTokenSource?.Cancel - _AsyncPayloadLaunchState: {0}", _AsyncPayloadLaunchState);
			_TaskHandler.Status(_LinkStage, _TotalElapsed, _Enabled, AsyncActive);
		}
		else
		{
			// Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}->A{asyncProcessId}] AsyncExit()", "ENTER and _Enabled - _SyncWaitOnAsyncTokenSource?.Cancel - _AsyncPayloadLaunchState: {0}", _AsyncPayloadLaunchState);
		}


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

		if (!SyncActive && Incomplete)
		{
			lock (_LockObject)
			{
				if (asyncCancellationToken.IsCancellationRequested)
				{
					// Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}->A{asyncProcessId}] AsyncExit()", "_AsyncLauncherToken.IsCancellationRequested - _AsyncLauncherTokenSource renew - _AsyncPayloadLaunchState: {0}", _AsyncPayloadLaunchState);
					_AsyncLauncherTokenSource.Dispose();
					_AsyncLauncherTokenSource = new();
					_AsyncLauncherToken = _AsyncLauncherTokenSource.Token;
				}
				// Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}->A{asyncProcessId}] AsyncExit()", "Re-executing AsyncExecute() - _AsyncPayloadLaunchState: {0}", _AsyncPayloadLaunchState);
				_AsyncPayloadLaunchState = EnLauncherPayloadLaunchState.Inactive;
			}

			AsyncExecute(_TransientDelay, _TransientMultiplier);
		}
		else
		{
			// Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}->A{asyncProcessId}] AsyncExit()", "EXIT resetting and _SyncWaitOnAsyncTokenSource?.Cancel() - _AsyncPayloadLaunchState: {0}", _AsyncPayloadLaunchState);
			lock (_LockObject)
			{
				_AsyncPayloadLaunchState = EnLauncherPayloadLaunchState.Inactive;
				_SyncWaitOnAsyncTokenSource?.Cancel();
			}
		}

		// Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}->A{asyncProcessId}] AsyncExit()", "EXIT - _AsyncPayloadLaunchState: {0}", _AsyncPayloadLaunchState);

		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Disable future async operations and suspends any current async tasks.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected override bool Disable()
	{
		// Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}] Disable()");

		if (!_Enabled)
			return false;

		int syncCardinal = SyncEnter();

		_Enabled = false;

		SyncExit(syncCardinal);

		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Disposes of a parser given an IVsDataConnection site.
	/// </summary>
	/// <param name="site">
	/// The IVsDataConnection explorer connection object
	/// </param>
	/// <param name="disposing">
	/// True if this is a permanent disposal and a transient parser should not
	/// be stored else false.
	/// </param>
	/// <returns>True of the parser was found and disposed else false.</returns>
	// ---------------------------------------------------------------------------------
	public static bool DisposeInstance(IVsDataConnection site, bool disposing)
	{
		// Tracer.Trace(typeof(LinkageParser), "DisposeInstance(IVsDataConnection)", "!Refreshing = disposing: {0}.", disposing);

		if (site == null)
			return false;

		if (site.GetService(typeof(IVsDataConnectionSupport)) is not IVsDataConnectionSupport vsDataConnectionSupport)
			return false;

		if (vsDataConnectionSupport.ProviderObject is not FbConnection connection)
			return false;

		return AbstractLinkageParser.DisposeInstance(connection, disposing);
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
		// Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}] EnsureLoaded() not pausing, calling SyncExecute()");

		return SyncExecute();
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The _AsyncPayloadLauncher payload.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private bool PayloadAsyncExecute(int asyncProcessId, CancellationToken asyncCancellationToken,
		CancellationToken userCancellationToken, int delay, int multiplier)
	{
		bool result = false;

		// Tracer.Trace(GetType(), "AsyncPayloadTask()", "delay: {0}, multiplier: {1}, UIThread? {2}.", delay, multiplier, ThreadHelper.CheckAccess());

		try
		{
			// Tracer.Trace("<AsyncExecute>._AsyncPayloadLauncher", $"ParserId:[{_InstanceId}->A{asyncProcessId}] AsyncExecute>AsyncPayloadTask()", "ENTER - userCancellationToken.IsCancellationRequested: {0}", userCancellationToken.IsCancellationRequested);

			if (userCancellationToken.IsCancellationRequested)
			{
				_TransientDelay = delay;
				_TransientMultiplier = multiplier;
				_Enabled = false;
			}
			else if (asyncCancellationToken.IsCancellationRequested)
			{
				_TransientDelay = delay;
				_TransientMultiplier = multiplier;
			}
			else
			{
				if (AsyncDelay(delay, multiplier, asyncCancellationToken, userCancellationToken))
				{
					_ = PopulateLinkageTables(asyncCancellationToken, userCancellationToken, "AsyncId", asyncProcessId);
				}
				else if (userCancellationToken.IsCancellationRequested)
				{
					_Enabled = false;
				}
			}
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			// throw;
		}
		finally
		{
			// Tracer.Trace("<AsyncExecute>._AsyncPayloadLauncher", $"ParserId:[{_InstanceId}->A{asyncProcessId}] AsyncExecute>AsyncPayloadTask()", "Finally calling AsyncExit - _AsyncPayloadLaunchState: {0}", _AsyncPayloadLaunchState);
			AsyncExit(asyncProcessId, asyncCancellationToken, userCancellationToken);
		}


		return result;
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
	private bool PopulateLinkageTables(CancellationToken asyncCancellationToken,
		CancellationToken userToken, string idType, int id)
	{
		// Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}] {idType}[{id}] PopulateLinkageTables()", "SyncCardinal: {0}, UIThread: {1}", _SyncCardinal, ThreadHelper.CheckAccess());

		if (_DbConnection == null)
		{
			// Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}] {idType}[{id}] PopulateLinkageTables()", "Connection null SyncCardinal: {0}", _SyncCardinal);
			ObjectDisposedException ex = new(Resources.ExceptionConnectionNull);
			Diag.Dug(ex);
			return false;
		}

		if (!ConnectionActive)
		{
			// Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}] {idType}[{id}] PopulateLinkageTables()", "ENTER and exit - !ConnectionActive - SyncCardinal: {0}", _SyncCardinal);
			return false;
		}

		try
		{

			// Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}] {idType}[{id}] PopulateLinkageTables()", "ENTER - _LinkStage: {0}", _LinkStage);

			_TaskHandler.Progress(Percent(_LinkStage), _LinkStage == EnLinkStage.Start ? 0 : -1, _TotalElapsed, _Enabled, AsyncActive);

			if (_LinkStage < EnLinkStage.GeneratorsLoaded)
			{
				_TaskHandler.Progress(Resources.LinkageParserStageGeneratorsBegin,
					Percent(_LinkStage + 1, true), -3, _TotalElapsed, _Enabled, AsyncActive);

				GetRawGeneratorSchema();

				if (AsyncActive && userToken.IsCancellationRequested)
					_Enabled = false;

				_TaskHandler.Progress(Resources.LinkageParserStageGeneratorsEnd,
					Percent(_LinkStage), Stopwatch.ElapsedMilliseconds, _TotalElapsed, _Enabled, AsyncActive);
			}


			if (AsyncActive && (!_Enabled || asyncCancellationToken.IsCancellationRequested))
				return false;

			if (!ConnectionActive)
				return false;


			if (_LinkStage < EnLinkStage.TriggerDependenciesLoaded)
			{
				_TaskHandler.Progress(Resources.LinkageParserStageTriggerDependenciesStart,
					Percent(_LinkStage + 1, true), -3, _TotalElapsed, _Enabled, AsyncActive);

				GetRawTriggerDependenciesSchema();

				if (AsyncActive && userToken.IsCancellationRequested)
					_Enabled = false;

				_TaskHandler.Progress(Resources.LinkageParserStageTriggerDependenciesEnd,
					Percent(_LinkStage), Stopwatch.ElapsedMilliseconds, _TotalElapsed, _Enabled, AsyncActive);
			}

			if (AsyncActive && (!_Enabled || asyncCancellationToken.IsCancellationRequested))
				return false;

			if (!ConnectionActive)
				return false;

			if (_LinkStage < EnLinkStage.TriggersLoaded)
			{
				_TaskHandler.Progress(Resources.LinkageParserStageTriggersBegin, Percent(_LinkStage + 1, true),
					-3, _TotalElapsed, _Enabled, AsyncActive);

				GetRawTriggerSchema();

				if (AsyncActive && userToken.IsCancellationRequested)
					_Enabled = false;

				_TaskHandler.Progress(Resources.LinkageParserStageTriggersEnd, Percent(_LinkStage),
					Stopwatch.ElapsedMilliseconds, _TotalElapsed, _Enabled, AsyncActive);
			}

			if (AsyncActive && (!_Enabled || asyncCancellationToken.IsCancellationRequested))
				return false;


			if (!SequencesPopulated)
			{
				_TaskHandler.Progress(Resources.LinkageParserStageSequencesBegin, Percent(_LinkStage + 1, true),
					-3, _TotalElapsed, _Enabled, AsyncActive);

				BuildSequenceTable();

				if (AsyncActive && userToken.IsCancellationRequested)
					_Enabled = false;

				_TaskHandler.Progress(Resources.LinkageParserStageSequencesEnd, Percent(_LinkStage),
					Stopwatch.ElapsedMilliseconds, _TotalElapsed, _Enabled, AsyncActive);
			}


			if (AsyncActive && (!_Enabled || asyncCancellationToken.IsCancellationRequested))
				return false;

			if (_LinkStage < EnLinkStage.Completed)
			{
				_TaskHandler.Progress(Resources.LinkageParserStageLinkingBegin, Percent(_LinkStage + 1, true),
					-3, _TotalElapsed, _Enabled, AsyncActive);

				BuildTriggerTable();

				_TaskHandler.Progress(Resources.LinkageParserStageLinkingEnd, Percent(_LinkStage),
					Stopwatch.ElapsedMilliseconds, _TotalElapsed, _Enabled, AsyncActive);
			}

			_LinkStage = EnLinkStage.Completed;

			_TaskHandler.Progress(Resources.LinkageParserStageCompleted.FmtRes(_Triggers.Rows.Count),
				Percent(_LinkStage), Stopwatch.ElapsedMilliseconds, _TotalElapsed, _Enabled, AsyncActive);

			_Stopwatch = null;

			_TaskHandler.Status(_LinkStage, _TotalElapsed, _Enabled, AsyncActive);

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
	public int SyncEnter(bool pausing = true)
	{
		// Sanity check.
		// If async is active and _SyncCardinal > 0 it means we have a sync process ahead of
		// us waiting for async to complete.
		// We're the sync process so how can we be ahead of us???
		if (_SyncCardinal > 0 && _AsyncPayloadLauncher != null && !_AsyncPayloadLauncher.IsCompleted && _AsyncPayloadLaunchState > EnLauncherPayloadLaunchState.Pending)
		{
			InvalidOperationException ex = new($"A deadlock has occured: _SyncCardinal: {_SyncCardinal}");
			Diag.Dug(ex);
			throw ex;
		}


		lock (_LockObject)
		{

			// If there's no one home just exit.

			if (_SyncCardinal == 0 && !Incomplete)
			{

				if (!AsyncActive && _AsyncPayloadLauncher != null && _AsyncPayloadLauncher.IsCompleted)
				{
					_AsyncPayloadLauncher.Dispose();
					_AsyncLauncherTokenSource.Dispose();
					_AsyncLauncherTokenSource = null;
					_AsyncPayloadLauncher = null;
				}

				// Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}] SyncEnter()", "ENTER/EXIT SyncEnter - _SyncCardinal: {0} pausing: {1}, IsUiThread: {2}.", _SyncCardinal, pausing, ThreadHelper.CheckAccess());

				return 0;
			}

			// Increment the sync processes count.
			_SyncCardinal++;

			// If this is a recursive sync call or async is not active we have exclusive sync control
			// and can exit.

			if (_SyncCardinal > 1 || !AsyncActive)
			{
				// Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}->S{_SyncCardinal}] SyncEnter()", "Cleared - Loaded or no _AsyncPayloadLauncher or _AsyncPayloadLauncher.IsCompleted");
				return !Incomplete ? _SyncCardinal : -_SyncCardinal;
			}
		}

		// Async is active. We have to wait.
		// This is a deadlock trap so we have to take the state of the launcher task's state
		// EnLauncherPayloadLaunchState into account...


		try
		{
			// If TriggerDependenciesLoaded it means Raw Trigger are busy selecting so no point cancelling.
			if (pausing && _LinkStage < EnLinkStage.TriggerDependenciesLoaded)
			{
				// Async is active so request cancellation. 
				lock (_LockObject)
					_AsyncLauncherTokenSource.Cancel();


				_TaskHandler.Progress(Percent(_LinkStage), -2, _TotalElapsed, _Enabled, AsyncActive);
			}

			lock (_LockObject)
			{
				if (_AsyncPayloadLaunchState < EnLauncherPayloadLaunchState.Launching)
				{
					// _AsyncPayloadLaunchState < 2: _AsyncPayloadLauncher is still waiting in thread queue managed by
					// UI thread so we flag it to cancel and leave.
					// If we wait we'll deadlock because the launcher's payload launch task/process will be behind us.

					// Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}->S{_SyncCardinal}] SyncEnter()", "Async task has not launched it's payload yet. We can exit.");

					return !Incomplete ? _SyncCardinal : -_SyncCardinal;
				}

				// Create a wait-on-async while we wait
				_SyncWaitOnAsyncTokenSource?.Dispose();
				_SyncWaitOnAsyncTokenSource = new();
				_SyncWaitOnAsyncToken = _SyncWaitOnAsyncTokenSource.Token;
			}

			// Tracer.Trace(GetType(), "SyncEnter()", "Entering possible wait. AsyncActive: {0}, UIThread? {1}.", AsyncActive, ThreadHelper.CheckAccess());

			int pendingTimeout = 3000;

			while (AsyncActive)
			{
				try
				{
					_AsyncPayloadLauncher.Wait(50, _SyncWaitOnAsyncToken);
				}
				catch // (OperationCanceledException ex)
				{
					// Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}->S {_SyncCardinal}] SyncEnter()", "_SyncWaitAsyncTask.Cancelled "); //, ex.Message);
				}

				if (!_SyncWaitOnAsyncToken.IsCancellationRequested)
				{
					Thread.Sleep(50);

					pendingTimeout -= 100;

					if (pendingTimeout <= 0 && _AsyncPayloadLaunchState == EnLauncherPayloadLaunchState.Pending)
					{
						// Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}->S {_SyncCardinal}] SyncEnter()", "_AsyncPayloadLauncher Timeout");
						_AsyncPayloadLauncher.Dispose();
						_AsyncPayloadLauncher = null;
						AsyncExit(_AsyncProcessSeed, _AsyncLauncherToken, _TaskHandler.UserCancellation);
					}
				}
				// Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}->S {_SyncCardinal}] SyncEnter()", "Wait loop _AsyncPayloadLaunchState: {0}, _SyncWaitOnAsyncToken.IsCancellationRequested: {1}, UIThread: {2}.", _AsyncPayloadLaunchState, _SyncWaitOnAsyncToken.IsCancellationRequested, ThreadHelper.CheckAccess());
			}

			// Tracer.Trace(GetType(), "SyncEnter()", "Finished possible wait. AsyncActive: {0}, UIThread? {1}.", AsyncActive, ThreadHelper.CheckAccess());

			// Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}->S {_SyncCardinal}] SyncEnter()", "Released _AsyncPayloadLauncher.Wait - _AsyncPayloadLaunchState: {0}", _AsyncPayloadLaunchState);

			// We have exclusive control.
			// Dispose of the wait-on-async token
			lock (_LockObject)
			{
				_SyncWaitOnAsyncTokenSource?.Dispose();
				_SyncWaitOnAsyncTokenSource = null;
				_SyncWaitOnAsyncToken = default;
			}

		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}

		// Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}->S{_SyncCardinal}] SyncEnter()", "EXIT after Wait and exiting - _AsyncPayloadLaunchState: {0}", _AsyncPayloadLaunchState);


		return !Incomplete ? _SyncCardinal : -_SyncCardinal;

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
			return _Enabled;

		if (_DbConnection == null)
		{
			ObjectDisposedException ex = new(Resources.ExceptionConnectionDisposed);
			Diag.Dug(ex);
			throw ex;
		}

		if (!ConnectionActive)
		{
			// Try reset
			try
			{
				_DbConnection.Close();
				_DbConnection.Open();
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
			return true;

		// Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}->S{syncCardinal}]  SyncExecute()", "Ready to prepare for sync execution");


		_TaskHandler.Status(_LinkStage, _TotalElapsed, _Enabled, AsyncActive);
		_TaskHandler.PreRegister(false);


		Task<bool> task = Task.Factory.StartNew(() =>
		{
			try
			{
				if (!ClearToLoadSync)
					return false;

				return PopulateLinkageTables(default, default, "SyncCardinal", syncCardinal);
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw;
			}
		},
		default, TaskCreationOptions.PreferFairness, TaskScheduler.Current);

		_TaskHandler.RegisterTask(task);

		// Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}->S{syncCardinal}]  SyncExecute()", "Sync thread for PopulateLinkageTables() loaded. Waiting for it to complete...");

		task.Wait();

		// Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}->S{syncCardinal}]  SyncExecute()", "Done waiting for thread - calling SyncExit.");

		SyncExit(syncCardinal);

		// Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}->S{syncCardinal}]  SyncExecute()", "EXIT - SyncExit done.");


		return true;
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
		// Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}->S{syncCardinal}]  SyncExit()", "ENTER SyncExit");


		lock (_LockObject)
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

			if (_AsyncPayloadLauncher != null && _AsyncPayloadLauncher.IsCompleted)
			{
				_AsyncPayloadLauncher?.Dispose();
				_AsyncLauncherTokenSource?.Dispose();
				_AsyncLauncherTokenSource = null;
				_AsyncPayloadLauncher = null;
			}


			_SyncCardinal--;
		}

		// If we don't have to restart the async exit.
		if (!Incomplete)
			return;
		

		// Should be no one behind us
		// Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}->S{syncCardinal}]  SyncExit()", "Restarting AsyncExecute");
		AsyncExecute(_TransientDelay, _TransientMultiplier);
		// Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}->S7{syncCardinal}]  SyncExit()", "EXIT");


	}


	#endregion Methods




		// =========================================================================================================
		#region Taskhandler and Status Bar - LinkageParser
		// =========================================================================================================


	private int Percent(EnLinkStage stage, bool starting = false)
	{
		return LinkageParserTaskHandler.GetPercentageComplete(stage, starting);
	}


	#endregion Taskhandler and Status Bar




	// =========================================================================================================
	#region Event handlers - LinkageParser
	// =========================================================================================================


	protected override void OnConnectionDisposed(object sender, EventArgs e)
	{
		// Tracer.Trace(GetType(), "OnConnectionDisposed()", "Sender: {0}.", sender.GetType().FullName);

		if (_InstanceConnection == null)
			return;


		Dispose(true);
	}


	#endregion Event handlers


}
