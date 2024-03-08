// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Data;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BlackbirdSql.Core.Controls;
using BlackbirdSql.Core.Ctl.Config;
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


	#region Constructors - LinkageParser


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Protected .ctor. for creating an unregistered clone.
	/// Callers must make a call to EnsureLoadedImpl() for the rhs beforehand.
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
	private LinkageParser(IDbConnection connection) : this(connection, null)
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
	private LinkageParser(IDbConnection connection, LinkageParser rhs) : base(connection, rhs)
	{
		// Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}] _LinkageParser(FbConnection, LinkageParser)");
		_TaskHandler = new(_InstanceConnection);
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Creates an unregistered clone of a parser.
	/// </summary>
	/// <returns></returns>
	// ---------------------------------------------------------------------------------
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
	private static LinkageParser CreateInstance(IDbConnection connection, bool canCreate)
	{
		// Tracer.Trace(typeof(LinkageParser), "CreateInstance(FbConnection, bool)", "canCreate: {0}.", canCreate);

		LinkageParser parser;

		lock (_LockGlobal)
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
	public static LinkageParser EnsureInstance(IDbConnection connection)
	{
		// Tracer.Trace(typeof(LinkageParser), "EnsureInstance(FbConnection)");

		return CreateInstance(connection, true);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Retrieves or creates the parser instance of a Site.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static LinkageParser EnsureInstance(IVsDataConnection site)
	{
		// Tracer.Trace(typeof(LinkageParser), "EnsureInstance(IVsDataConnection, Type)");

		if (site == null)
			return null;

		if (site.GetService(typeof(IVsDataConnectionSupport)) is not IVsDataConnectionSupport vsDataConnectionSupport)
			return null;

		if (vsDataConnectionSupport.ProviderObject is not FbConnection connection)
			return null;

		return EnsureInstance(connection);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Retrieves or creates the parser instance of a connection, Waiting to ensure it
	/// has linked.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static LinkageParser EnsureLoaded(IDbConnection connection)
	{
		// Diag.Stack();
		// Tracer.Trace(typeof(LinkageParser), "EnsureLoaded(IDbConnection)");

		LinkageParser parser = CreateInstance(connection, true);

		parser.EnsureLoadedImpl();

		return parser;
	}




	// ---------------------------------------------------------------------------------
	/// <summary>1
	/// Retrieves or creates the parser instance of a connection, Waiting to ensure it
	/// has linked.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static LinkageParser EnsureLoaded(IVsDataConnection site)
	{
		// Tracer.Trace(typeof(LinkageParser), "EnsureInstance(FbConnection, Type)");

		if (site == null)
			return null;

		if (site.GetService(typeof(IVsDataConnectionSupport)) is not IVsDataConnectionSupport vsDataConnectionSupport)
			return null;

		if (vsDataConnectionSupport.ProviderObject is not FbConnection connection)
			return null;

		return EnsureLoaded(connection);
	}


	protected override bool EnsureLoadedImpl()
	{
		// Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}] EnsureLoadedImpl() not pausing, calling SyncExecute()");

		AsyncExecute(0, 0);

		return AsyncWait();
	}




	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Retrieves an existing parser for a connection else null if no LinkageParser
	/// exists.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static new LinkageParser GetInstance(IDbConnection connection)
	{
		// Tracer.Trace(typeof(LinkageParser), "GetInstance(FbConnection)");

		return CreateInstance(connection, false);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Retrieves an existing parser for a Site else null if no LinkageParser exists.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static LinkageParser GetInstance(IVsDataConnection site)
	{
		// Tracer.Trace(typeof(LinkageParser), "GetInstance(IVsDataConnection)");

		if (site == null)
			return null;

		if (site.GetService(typeof(IVsDataConnectionSupport)) is not IVsDataConnectionSupport vsDataConnectionSupport)
			return null;

		if (vsDataConnectionSupport.ProviderObject is not FbConnection connection)
			return null;

		return GetInstance(connection);
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
		_TransientParser = null;
	}


	#endregion Constructors





	// =========================================================================================================
	#region Constants - LinkageParser
	// =========================================================================================================


	public const int C_Elapsed_Resuming = -1;
	public const int C_Elapsed_Disabling = -2;
	public const int C_Elapsed_StageCompleted = -3;



	#endregion Constants





	// =========================================================================================================
	#region Fields - LinkageParser
	// =========================================================================================================


	// Threading / multi process control variables.

	// The async process thread id / index for tracing
	private static int _AsyncProcessSeed = 90000;

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


	#endregion Fields





	// =========================================================================================================
	#region Property accessors - LinkageParser
	// =========================================================================================================


	private bool AsyncActive
	{
		get
		{
			lock (_LockObject)
				return _AsyncPayloadLaunchState != EnLauncherPayloadLaunchState.Inactive;
		}
	}


	/// <summary>
	/// True if an async payload is queued in the thread pool but has not yet been launched.
	/// </summary>
	private bool AsyncPending
	{
		get
		{
			lock (_LockObject)
				return _AsyncPayloadLaunchState == EnLauncherPayloadLaunchState.Pending;
		}
	}



	/// <summary>
	/// Getter inidicating whether or not async linkage can and should begin or resume operations.
	/// </summary>
	private bool ClearToLoadAsync => _Enabled && !AsyncActive && !Loaded;


	/// <summary>
	/// Getter indicating whether or not linkage is still required. Completed
	/// differs from Loaded in that Completed may be because the linker
	/// has been cancelled/disabled by the user.
	/// </summary>
	private bool Completed => _LinkStage == EnLinkStage.Completed || !_Enabled;

	public bool Loading => AsyncActive;


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

		// Tracer.Trace(GetType(), "AsyncDelay()", "Exiting - delay: {0}, multiplier: {1}, UIThread? {2}.", delay, multiplier, ThreadHelper.CheckAccess());


		return !userToken.IsCancellationRequested && !asyncCancellationToken.IsCancellationRequested
			&& ConnectionActive && _AsyncPayloadLauncher != null;
	}


	/*
	 * GetInstance - Gets instance else null if no exist.
	 * EnsureInstance - GetInstance else create if no exist.
	 * 
	 * AsyncRequestLoading - GetsInstance then initiate asyncloading if !PersistentSettings.OnDemandLinkage.
	 * AsyncEnsureLoading - EnsureInstance then initiate asyncloading if !PersistentSettings.OnDemandLinkage.
	 * 
	 * EnsureLoaded - AsyncEnsureLoading and wait for completion.
	 * 
	 */


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Ensures an instance is retrieved or else created and then initiates loading if
	/// !PersistentSettings.OnDemandLinkage.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static LinkageParser AsyncEnsureLoading(IDbConnection connection, int delay = 0, int multiplier = 1)
	{
		// Tracer.Trace(typeof(LinkageParser), "EnsureInstance(FbConnection, Type)");

		LinkageParser parser = CreateInstance(connection, true);

		if (parser == null)
			return null;

		if (!parser.Loading && !parser.Loaded && parser._Enabled && !PersistentSettings.OnDemandLinkage)
			parser.AsyncExecute(delay, multiplier);

		return parser;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>1
	/// Ensures an instance is retrieved or else created and then initiates loading if
	/// !PersistentSettings.OnDemandLinkage.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static LinkageParser AsyncEnsureLoading(IVsDataConnection site, int delay = 0, int multiplier = 1)
	{
		// Tracer.Trace(typeof(LinkageParser), "EnsureInstance(FbConnection, Type)");

		if (site == null)
			return null;

		if (site.GetService(typeof(IVsDataConnectionSupport)) is not IVsDataConnectionSupport vsDataConnectionSupport)
			return null;

		if (vsDataConnectionSupport.ProviderObject is not FbConnection connection)
			return null;

		return AsyncEnsureLoading(connection, delay, multiplier);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Ensures an instance is retrieved or else created and then initiates loading if
	/// !PersistentSettings.OnDemandLinkage.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static LinkageParser AsyncEnsureLoading(IVsDataExplorerNode node, int delay = 0, int multiplier = 1)
	{
		// Tracer.Trace(typeof(LinkageParser), "EnsureInstance(FbConnection, Type)");

		if (node == null || node.Object == null || node.ExplorerConnection.Connection == null)
		{
			return null;
		}

		return AsyncEnsureLoading(node.ExplorerConnection.Connection, delay, multiplier);
	}




	// ---------------------------------------------------------------------------------
	/// <summary>
	/// GetInstance and initiates loading if !PersistentSettings.OnDemandLinkage.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static LinkageParser AsyncRequestLoading(IDbConnection connection, int delay = 0, int multiplier = 1)
	{
		// Tracer.Trace(typeof(LinkageParser), "EnsureInstance(FbConnection, Type)");

		try
		{

			LinkageParser parser = CreateInstance(connection, false);

			if (parser == null)
				return null;

			if (!parser.Loading && !parser.Loaded && parser._Enabled && !PersistentSettings.OnDemandLinkage)
				parser.AsyncExecute(delay, multiplier);

			return parser;
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// GetInstance and initiates loading if !PersistentSettings.OnDemandLinkage.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static LinkageParser AsyncRequestLoading(IVsDataConnection site, int delay = 0, int multiplier = 1)
	{
		// Tracer.Trace(typeof(LinkageParser), "EnsureInstance(FbConnection, Type)");

		if (site == null)
			return null;

		if (site.GetService(typeof(IVsDataConnectionSupport)) is not IVsDataConnectionSupport vsDataConnectionSupport)
			return null;

		if (vsDataConnectionSupport.ProviderObject is not FbConnection connection)
			return null;

		return AsyncRequestLoading(connection, delay, multiplier);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// GetInstance and initiates loading if !PersistentSettings.OnDemandLinkage.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static LinkageParser AsyncRequestLoading(IVsDataExplorerNode node, int delay = 0, int multiplier = 1)
	{
		// Tracer.Trace(typeof(LinkageParser), "EnsureInstance(FbConnection, Type)");

		if (node == null || node.Object == null || node.ExplorerConnection.Connection == null)
		{
			return null;
		}

		return AsyncRequestLoading(node.ExplorerConnection.Connection, delay, multiplier);
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
	protected override bool AsyncExecute(int delay, int multiplier)
	{
		if (!ClearToLoadAsync)
			return false;

		int asyncProcessId = _AsyncProcessSeed < 99990 ? ++_AsyncProcessSeed : 90001;
		_AsyncProcessSeed = asyncProcessId;

		// Tracer.Trace(GetType(), "AsyncExecute()");


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
		// Fire and remember.
		_AsyncPayloadLauncher = Task.Factory.StartNew(payload, default, creationOptions, scheduler);

		_TaskHandler.RegisterTask(_AsyncPayloadLauncher);

		// Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}->A{asyncProcessId}] AsyncExecute()", "EXIT - AsyncTask registered - _AsyncPayloadLaunchState: {0}", _AsyncPayloadLaunchState);


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

		AsyncWait(true);

		_Enabled = false;

		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Disposes of a parser given an IVsDataConnection site.
	/// </summary>
	/// <param name="site">
	/// The IVsDataConnection explorer connection object
	/// </param>
	/// <param name="isValidTransient">
	/// False if this is a user refresh and a transient parser should not be stored else
	/// True.
	/// </param>
	/// <returns>True of the parser was found and disposed else false.</returns>
	// ---------------------------------------------------------------------------------
	public static bool DisposeInstance(IVsDataConnection site, bool isValidTransient)
	{
		// Tracer.Trace(typeof(LinkageParser), "DisposeInstance(IVsDataConnection)", "!Refreshing = disposing: {0}.", disposing);

		if (site == null)
			return false;

		if (site.GetService(typeof(IVsDataConnectionSupport)) is not IVsDataConnectionSupport vsDataConnectionSupport)
			return false;

		if (vsDataConnectionSupport.ProviderObject is not FbConnection connection)
			return false;

		return DisposeInstance(connection, isValidTransient);
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
			if (userCancellationToken.IsCancellationRequested)
			{
				_Enabled = false;
			}
			else if (!asyncCancellationToken.IsCancellationRequested)
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
		}
		finally
		{
			// Tracer.Trace(GetType(), "PayloadAsyncExecute()", "Executing finally");

			lock (_LockObject)
			{
				if (_MessageBoxDlg != null)
				{
					CloseMessageBox();
				}
			}

			if (!_Enabled)
				_TaskHandler.Status(_LinkStage, _TotalElapsed, _Enabled, AsyncActive);

			lock (_LockObject)
				_AsyncPayloadLaunchState = EnLauncherPayloadLaunchState.Inactive;
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

			_TaskHandler.Progress(Percent(_LinkStage), _LinkStage == EnLinkStage.Start ? 0 : C_Elapsed_Resuming, _TotalElapsed, _Enabled, AsyncActive);

			Thread.Yield();

			if (_LinkStage < EnLinkStage.GeneratorsLoaded)
			{
				_TaskHandler.Progress(Resources.LinkageParserStageGeneratorsBegin,
					Percent(_LinkStage + 1, true), C_Elapsed_StageCompleted, _TotalElapsed, _Enabled, AsyncActive);

				GetRawGeneratorSchema();

				if (userToken.IsCancellationRequested)
					_Enabled = false;

				_TaskHandler.Progress(Resources.LinkageParserStageGeneratorsEnd,
					Percent(_LinkStage), Stopwatch.ElapsedMilliseconds, _TotalElapsed, _Enabled, AsyncActive);
			}

			Thread.Yield();

			if (!_Enabled || asyncCancellationToken.IsCancellationRequested)
				return false;

			if (!ConnectionActive)
				return false;


			if (_LinkStage < EnLinkStage.TriggerDependenciesLoaded)
			{
				_TaskHandler.Progress(Resources.LinkageParserStageTriggerDependenciesStart,
					Percent(_LinkStage + 1, true), C_Elapsed_StageCompleted, _TotalElapsed, _Enabled, AsyncActive);

				GetRawTriggerDependenciesSchema();

				if (AsyncActive && userToken.IsCancellationRequested)
					_Enabled = false;

				_TaskHandler.Progress(Resources.LinkageParserStageTriggerDependenciesEnd,
					Percent(_LinkStage), Stopwatch.ElapsedMilliseconds, _TotalElapsed, _Enabled, AsyncActive);
			}

			Thread.Yield();

			if (!_Enabled || asyncCancellationToken.IsCancellationRequested)
				return false;

			if (!ConnectionActive)
				return false;

			if (_LinkStage < EnLinkStage.TriggersLoaded)
			{
				_TaskHandler.Progress(Resources.LinkageParserStageTriggersBegin, Percent(_LinkStage + 1, true),
					C_Elapsed_StageCompleted, _TotalElapsed, _Enabled, AsyncActive);

				GetRawTriggerSchema();

				if (userToken.IsCancellationRequested)
					_Enabled = false;

				_TaskHandler.Progress(Resources.LinkageParserStageTriggersEnd, Percent(_LinkStage),
					Stopwatch.ElapsedMilliseconds, _TotalElapsed, _Enabled, AsyncActive);
			}

			if (!_Enabled || asyncCancellationToken.IsCancellationRequested)
				return false;

			Thread.Yield();


			if (!SequencesPopulated)
			{
				_TaskHandler.Progress(Resources.LinkageParserStageSequencesBegin, Percent(_LinkStage + 1, true),
					C_Elapsed_StageCompleted, _TotalElapsed, _Enabled, AsyncActive);

				BuildSequenceTable();

				if (userToken.IsCancellationRequested)
					_Enabled = false;

				_TaskHandler.Progress(Resources.LinkageParserStageSequencesEnd, Percent(_LinkStage),
					Stopwatch.ElapsedMilliseconds, _TotalElapsed, _Enabled, AsyncActive);
			}

			Thread.Yield();


			if (!_Enabled || asyncCancellationToken.IsCancellationRequested)
				return false;

			if (_LinkStage < EnLinkStage.Completed)
			{
				_TaskHandler.Progress(Resources.LinkageParserStageLinkingBegin, Percent(_LinkStage + 1, true),
					C_Elapsed_StageCompleted, _TotalElapsed, _Enabled, AsyncActive);

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
	/// Returns true if an SE node's collection requires completed Trigger/Generator
	/// linkage tables in order to render, else false.
	/// If true and linkage is incomplete or non-existent, the caller will first
	/// initiate linkage, if required, and wait for the owning Explorer ConnectionNode's
	/// linkage tables to be prepared before allowing a node to be rendered.
	/// </summary>
	// ----------------------------------------------------------------------------------
	public static bool RequiresTriggers(string collection)
	{
		// Tracer.Trace(typeof(LinkageParser), "RequiresTriggers()", "Collection: {0}.", collection);

		switch (collection)
		{
			case "ForeignKeys":
			case "Functions":
			case "Indexes":
			case "Procedures":
			case "Tables":
			case "RawGenerators":
			case "RawTriggerDependencies":
			case "RawTriggers":
			case "Views":
			case "ViewColumns":
				return false;
			default:
				break;
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
	public bool AsyncWait(bool disabling = false)
	{

		lock (_LockObject)
		{

			// If there's no one home just exit.

			if (Completed)
			{
				if (!AsyncActive && _AsyncPayloadLauncher != null && _AsyncPayloadLauncher.IsCompleted)
				{
					_AsyncPayloadLauncher.Dispose();
					_AsyncLauncherTokenSource.Dispose();
					_AsyncLauncherTokenSource = null;
					_AsyncPayloadLauncher = null;
				}

				// Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}] SyncEnter()", "ENTER/EXIT SyncEnter - _SyncCardinal: {0} pausing: {1}, IsUiThread: {2}.", _SyncCardinal, pausing, ThreadHelper.CheckAccess());

				return _Enabled && !disabling;
			}

		}

		// Async is active. We have to optionally disable and wait.
		// This is a deadlock trap if AsyncPending is true, so we have to take
		// the state of the launcher task's state (EnLauncherPayloadLaunchState)
		// into account...


		try
		{
			if (disabling)
			{
				// Async is active so request cancellation. 
				lock (_LockObject)
					_AsyncLauncherTokenSource.Cancel();

				_TaskHandler.Progress(Percent(_LinkStage), C_Elapsed_Disabling, _TotalElapsed, _Enabled, AsyncActive);
			}

			int waitTime = 0;
			int timeout = PersistentSettings.LinkageTimeout * 1000;

			// Wait maximum of 3 seconds for async task to launch.
			int pendingTimeout = 3000;


			while (AsyncActive)
			{
				if (waitTime >= timeout && !disabling && _Enabled)
				{
					// Make a non-blocking request to the user for a time extension.
					// Message prompt will automatically disappear if linkage completes before
					// the user has responded.

					string caption = ControlsResources.LinkageParser_CaptionLinkageTimeout;
					string msg = ControlsResources.LinkageParser_TextLinkageParser.FmtRes(PersistentSettings.LinkageTimeout);
					
					if (MessageCtl.ShowEx(msg, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Error, OnMessageBoxShown) == DialogResult.Yes)
					{
						waitTime = 0;

					}
					else
					{
						if (AsyncActive)
						{
							lock (_LockObject)
							{
								disabling = true;
								_Enabled = false;
								_AsyncLauncherTokenSource.Cancel();
							}

							_TaskHandler.Progress(Percent(_LinkStage), C_Elapsed_Disabling, _TotalElapsed, _Enabled, AsyncActive);

							TimeoutException ex = new($"Timed out waiting for AsyncPayloadLauncher to complete. Timeout (ms): {waitTime}.");
							Diag.Dug(ex);
							// throw ex;
						}
					}

					continue;
				}


				try
				{
					_AsyncPayloadLauncher.Wait(50, _AsyncLauncherToken);
				}
				catch // (OperationCanceledException ex)
				{
					// Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}->S {_SyncCardinal}] SyncEnter()", "_SyncWaitAsyncTask.Cancelled "); //, ex.Message);
				}

				if ((_AsyncLauncherTokenSource.IsCancellationRequested || disabling) && AsyncPending)
				{
					pendingTimeout -= 100;

					if (pendingTimeout <= 0)
					{
						// Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}->S {_SyncCardinal}] SyncEnter()", "_AsyncPayloadLauncher Timeout");
						_AsyncPayloadLauncher = null;

						if (!_Enabled || disabling)
							_TaskHandler.Status(_LinkStage, _TotalElapsed, _Enabled && !disabling, AsyncActive);

						lock (_LockObject)
							_AsyncPayloadLaunchState = EnLauncherPayloadLaunchState.Inactive;

						return false;
					}
				}

				Thread.Sleep(50);
				if ((waitTime % 500) == 0)
					Thread.Yield();
				
				waitTime += 100;

			}
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}

		// Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}->S{_SyncCardinal}] SyncEnter()", "EXIT after Wait and exiting - _AsyncPayloadLaunchState: {0}", _AsyncPayloadLaunchState);


		return _Enabled && !disabling;

	}

	private MessageBoxDialog _MessageBoxDlg = null;

	private void OnMessageBoxShown(object sender, EventArgs e)
	{
		// Tracer.Trace(GetType(), "OnMessageBoxShown()", "Sender type: {0}", sender);

		ThreadStart threadParameters = new ThreadStart(delegate { ThreadSafeInitializeMessageBox(sender); });
		Thread thread = new Thread(threadParameters);
		thread.Start();
	}


	private void CloseMessageBox()
	{
		lock (_LockObject)
		{
			if (_MessageBoxDlg == null)
				return;
		}

		ThreadStart threadParameters = new ThreadStart(delegate { ThreadSafeCloseMessageBox(); });
		Thread thread = new Thread(threadParameters);
		thread.Start();
	}

	public void ThreadSafeCloseMessageBox()
	{
			lock (_LockObject)
			{
				if (_MessageBoxDlg == null)
					return;
			}

		// Tracer.Trace(GetType(), "OnMessageBoxShown()", "Sender type: {0}", sender);

		if (_MessageBoxDlg.InvokeRequired)
		{
			Action safeClose = delegate { ThreadSafeCloseMessageBox(); };
			_MessageBoxDlg.Invoke(safeClose);
		}
		else
		{
			lock (_LockObject)
			{
				_MessageBoxDlg.DialogResult = DialogResult.Yes;
				_MessageBoxDlg?.Close();
			}
		}
	}



	public void ThreadSafeInitializeMessageBox(object sender)
	{
		lock (_LockObject)
		{
			if (_MessageBoxDlg != null)
				return;
		}

		// Tracer.Trace(GetType(), "OnMessageBoxShown()", "Sender type: {0}", sender);

		if (((MessageBoxDialog)sender).InvokeRequired)
		{
			Action safeInitialize = delegate { ThreadSafeInitializeMessageBox(sender); };
			((MessageBoxDialog)sender).Invoke(safeInitialize);
		}
		else
		{
			lock (_LockObject)
			{
				_MessageBoxDlg = (MessageBoxDialog)sender;
				_MessageBoxDlg.FormClosed += OnMessageBoxClose;

				_MessageBoxDlg.Activate();
			}
		}
	}


	public void OnMessageBoxClose(object sender, FormClosedEventArgs e)
	{
		lock (_LockObject)
			_MessageBoxDlg = null;
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
