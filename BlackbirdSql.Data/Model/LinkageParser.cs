// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BlackbirdSql.Data.Ctl;
using BlackbirdSql.Data.Properties;
using BlackbirdSql.Sys.Controls;
using BlackbirdSql.Sys.Enums;
using FirebirdSql.Data.FirebirdClient;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Shell;



namespace BlackbirdSql.Data.Model;


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
	/// Protected default .ctor for creating a Transient instance with restrictions.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private LinkageParser(string connectionString, string[] restrictions) : base(connectionString, restrictions)
	{
		// Tracer.Trace(typeof(LinkageParser), ".ctor(string, string[]");
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Protected .ctor. for creating an unregistered clone.
	/// Callers must make a call to EnsureLoaded() for the rhs beforehand.
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
	private LinkageParser(IVsDataExplorerConnection root) : this(root, null)
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
	private LinkageParser(IVsDataExplorerConnection root, LinkageParser rhs) : base(root, rhs)
	{
		// Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}] _LinkageParser(FbConnection, LinkageParser)");
		_TaskHandler = new(_ConnectionString);
	}



	protected override bool Dispose(bool disposing)
	{
		// Tracer.Trace(typeof(LinkageParser), "Dispose(bool)");

		lock (_LockGlobal)
		{
			if (!base.Dispose(disposing))
				return false;

			_TaskHandler = null;

			return true;
		}
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
			return new LinkageParser(_InstanceRoot);
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// This is the universal _Instance access point.
	/// Retrieves or creates a distinct unique parser for a connection.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private static LinkageParser CreateInstance(IVsDataExplorerConnection root, bool canCreate)
	{
		// Tracer.Trace(typeof(LinkageParser), "CreateInstance(FbConnection, bool)", "canCreate: {0}.", canCreate);

		LinkageParser parser;

		parser = (LinkageParser)GetInstanceImpl(root);


		if (parser == null)
		{
			LinkageParser rhs = (LinkageParser)FindEquivalentParser(root);

			if (rhs != null)
				parser = new(root, rhs);
			else if (canCreate)
				parser = new(root);
		}

		return parser;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>1
	/// Retrieves or creates the parser instance of a connection, Waiting to ensure it
	/// has linked.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static LinkageParser EnsureLoaded(string connectionString, string[] restrictions)
	{
		// Tracer.Trace(typeof(LinkageParser), "EnsureLoaded()");

		
		LinkageParser parser;

		IVsDataExplorerConnectionManager manager = ApcManager.ExplorerConnectionManager;

		(_, IVsDataExplorerConnection root) = manager.SearchExplorerConnectionEntry(connectionString, false, false);

		if (root != null)
		{
			if (restrictions == null)
				return EnsureLoaded(root);

			parser = CreateInstance(root, true);

			if (parser.Loaded)
				return parser;

			if (!parser.Loading && parser._Enabled)
				parser.AsyncExecute(0, 0);
		}


		(_, root) = manager.SearchExplorerConnectionEntry(connectionString, false, true);

		if (root != null)
		{
			parser = CreateInstance(root, false);

			if (parser != null && parser.Loaded)
				return parser;
		}


		parser = (LinkageParser)FindEquivalentParser(connectionString, true);

		if (parser != null)
			return parser;

		lock (_LockGlobal)
		{
			if (_TransientInstance != null && ApcManager.IsWeakConnectionEquivalency(connectionString, _TransientInstance.ConnectionString)
			&& ((LinkageParser)_TransientInstance)._TransientRestrictions[2].Equals(restrictions[2], StringComparison.InvariantCultureIgnoreCase))
			{
				return (LinkageParser)_TransientInstance;
			}
		}

		parser = new(connectionString, restrictions);

		parser.EnsureLoadedRestricted();

		return parser;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>1
	/// Retrieves or creates the parser instance of a connection, Waiting to ensure it
	/// has linked.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static LinkageParser EnsureLoaded(IDbConnection connection)
	{
		// Tracer.Trace(typeof(LinkageParser), "EnsureLoaded()");

		IVsDataExplorerConnection root = connection.ExplorerConnection(true);

		if (root == null)
			return null;

		LinkageParser parser = CreateInstance(root, true);

		parser.EnsureLoaded();

		return parser;

	}




	// ---------------------------------------------------------------------------------
	/// <summary>1
	/// Retrieves or creates the parser instance of a connection, Waiting to ensure it
	/// has linked.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static LinkageParser EnsureLoaded(IVsDataExplorerConnection root)
	{
		// Tracer.Trace(typeof(LinkageParser), "EnsureLoaded()");

		LinkageParser parser = CreateInstance(root, true);

		parser.EnsureLoaded();

		return parser;

	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Launches the UI thread build of the linkage tables if the UI requires them.
	/// If an async build is in progress, waits for the active operation to complete and
	/// then switches over to a UI thread build for the remaining tasks.
	/// </summary>
	/// <returns>True if successfully loaded or already loaded else false</returns>
	// ---------------------------------------------------------------------------------
	public override bool EnsureLoaded()
	{
		// Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}] EnsureLoaded() not pausing, calling SyncExecute()");

		if (IsTransient)
			return true;


		AsyncExecute(0, 0);

		return AsyncWait();
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Launches a arser load with restrictions.
	/// </summary>
	/// <returns>True if successfully loaded or already loaded else false</returns>
	// ---------------------------------------------------------------------------------
	[SuppressMessage("Usage", "VSTHRD104:Offer async methods")]
	public override bool EnsureLoadedRestricted()
	{
		// Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}] EnsureLoaded() not pausing, calling SyncExecute()");

		bool result = false;
		// Fire and wait

		ThreadHelper.JoinableTaskFactory.Run(async delegate
		{
			result = await PopulateLinkageTablesAsync(default, "SyncId", 0);
		});

		return result;
	}




	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Retrieves an existing parser for a Site else null if no LinkageParser exists.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static LinkageParser GetInstance(IVsDataExplorerConnection root)
	{
		// Tracer.Trace(typeof(LinkageParser), "GetInstance(IVsDataConnection)");

		return CreateInstance(root, false);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Retrieves an existing parser or transient for a connection else null if no
	/// LinkageParser exists.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static LinkageParser GetInstanceOrTransient(string connectionString)
	{
		// Tracer.Trace(typeof(LinkageParser), "GetInstance(connectionString)", "connectionString: {0}.", connectionString);

		return (LinkageParser)FindInstanceOrTransient(connectionString);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Destructor.
	/// </summary>
	// ---------------------------------------------------------------------------------
	~LinkageParser()
	{
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

	private static string _LockedLoadedConnectionString = null;
	private AdvancedMessageBox _MessageBoxDlg = null;
	private LinkageParserTaskHandler _TaskHandler = null;

	/// <summary>
	/// The async process state, 0 = No async process, 1 = Async launch queued, 2 = Async Active.
	/// </summary>
	private EnLauncherPayloadLaunchState _AsyncPayloadLaunchState = EnLauncherPayloadLaunchState.Inactive;

	/// <summary>
	/// The async launcher task if it exists.
	/// </summary>
	private Task<bool> _AsyncPayloadLauncher;


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




	// ---------------------------------------------------------------------------------
	/// Single pass - clears a connection that has been locked against disposal by 
	/// <see cref="LockLoadedParser"/> and returns true if the parser is the locked
	/// parser.
	// ---------------------------------------------------------------------------------
	public override bool IsLockedLoaded
	{
		get
		{
			if (_LockedLoadedConnectionString == null)
				return false;

			if (!Loaded)
			{
				_LockedLoadedConnectionString = null;
				return false;
			}

			string lockedConnectionUrl = ApcManager.CreateConnectionUrl(_LockedLoadedConnectionString);
			string disposingConnectionUrl = ApcManager.CreateConnectionUrl(ConnectionString);

			// Tracer.Trace(typeof(RctManager), "IsLockedLoaded", "\nlockedConnectionUrl: {0}, disposingConnectionUrl: {1}.", lockedConnectionUrl, disposingConnectionUrl);
			_LockedLoadedConnectionString = null;

			if (lockedConnectionUrl.Equals(disposingConnectionUrl))
				return true;

			return false;
		}
	}


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
	private bool AsyncDelay(int delay, int multiplier, CancellationToken userToken)
	{
		// Tracer.Trace(GetType(), "AsyncDelay()", "Entering - delay: {0}, multiplier: {1}, UIThread? {2}.", delay, multiplier, ThreadHelper.CheckAccess());

		lock (_LockObject)
			_AsyncPayloadLaunchState = EnLauncherPayloadLaunchState.Launching;

		int i;

		for (i = 0; i < multiplier; i++)
		{
			if (userToken.IsCancellationRequested || _AsyncPayloadLauncher == null)
			{
				break;
			}


			try
			{
				_AsyncPayloadLauncher.Wait(delay, userToken);
			}
			catch // (OperationCanceledException ex)
			{
			}
		}

		// Tracer.Trace(GetType(), "AsyncDelay()", "Exiting - delay: {0}, multiplier: {1}, UIThread? {2}.", delay, multiplier, ThreadHelper.CheckAccess());


		return !userToken.IsCancellationRequested && _AsyncPayloadLauncher != null;
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
	/// <summary>1
	/// Ensures an instance is retrieved or else created and then initiates loading if
	/// !PersistentSettings.OnDemandLinkage.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static LinkageParser AsyncEnsureLoading(IVsDataExplorerConnection root, int delay = 0, int multiplier = 1)
	{
		// Tracer.Trace(typeof(LinkageParser), "EnsureInstance(FbConnection, Type)");

		if (root == null)
			return null;

		LinkageParser parser = CreateInstance(root, true);

		if (parser == null)
			return null;

		if (!parser.Loading && !parser.Loaded /* && parser._TaskHandler != null */
			&& parser._Enabled && !NativeDb.OnDemandLinkage)
		{
			parser.AsyncExecute(delay, multiplier);
		}

		return parser;


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

		// Tracer.Trace(GetType(), "AsyncExecute()");

		int asyncProcessId = _AsyncProcessSeed < 99990 ? ++_AsyncProcessSeed : 90001;
		_AsyncProcessSeed = asyncProcessId;

		lock (_LockObject)
		{
			_AsyncPayloadLaunchState = EnLauncherPayloadLaunchState.Pending;
		}


		_TaskHandler.Status(_LinkStage, _TotalElapsed, _Enabled, AsyncActive);
		_TaskHandler.PreRegister(true);


		// The following for brevity.
		CancellationToken userCancellationToken = _TaskHandler.UserCancellation;
		TaskCreationOptions creationOptions = TaskCreationOptions.LongRunning | TaskCreationOptions.AttachedToParent;
		TaskScheduler scheduler = TaskScheduler.Default;

		// Tracer.Trace(GetType(), "AsyncExecute()", "delay: {0}, multiplier: {1}, UIThread? {2}.", delay, multiplier, ThreadHelper.CheckAccess());

		// For brevity.
		Task<bool> payloadAsync() =>
			ExecuteAsync(asyncProcessId, userCancellationToken, delay, multiplier);

		// Start up the payload launcher with tracking.
		// Fire and remember.
		_AsyncPayloadLauncher = Task.Factory.StartNew(payloadAsync, default, creationOptions, scheduler).Unwrap();

		_TaskHandler.RegisterTask(_AsyncPayloadLauncher);

		// Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}->A{asyncProcessId}] AsyncExecute()", "EXIT - AsyncTask registered - _AsyncPayloadLaunchState: {0}", _AsyncPayloadLaunchState);


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
				{
					CancellationTokenSource tokenSource = (CancellationTokenSource)Reflect.GetField(_TaskHandler.UserCancellation, "m_source");
					tokenSource?.Cancel();
				}

				_TaskHandler.Progress(Percent(_LinkStage), C_Elapsed_Disabling, _TotalElapsed, _Enabled, AsyncActive);
			}

			int waitTime = 0;
			int timeout = NativeDb.LinkageTimeout * 1000;

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
					string msg = ControlsResources.LinkageParser_TextLinkageParser.FmtRes(NativeDb.LinkageTimeout);

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

								CancellationTokenSource tokenSource = (CancellationTokenSource)Reflect.GetField(_TaskHandler.UserCancellation, "m_source");
								tokenSource?.Cancel();
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
					_AsyncPayloadLauncher.Wait(50, _TaskHandler.UserCancellation);
				}
				catch // (OperationCanceledException ex)
				{
					// Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}->S {_SyncCardinal}] SyncEnter()", "_SyncWaitAsyncTask.Cancelled "); //, ex.Message);
				}

				if ((_TaskHandler.UserCancellation.IsCancellationRequested || disabling) && AsyncPending)
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



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Disable future async operations and suspends any current async tasks.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected override bool Disable()
	{
		// Tracer.Trace(GetType(), "Disable()");

		if (_Disabling)
			return false;

		if (Completed || !Loading)
		{
			_Enabled = false;
			return false;
		}

		_Disabling = true; 

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
	/// <param name="disposing">
	/// If disposing is set to true, then all parsers with weak equivalency will
	/// be tagged as intransient, meaning their trigger linkage databases cannot
	/// be copied to another parser with weak equivalency. 
	/// </param>
	/// <returns>True of the parser was found and disposed else false.</returns>
	// -------------------------------------------------------------------------
	public static bool DisposeInstance(IVsDataExplorerConnection root, bool disposing)
	{

		if (root == null)
			return false;

		return DisposeInstanceImpl(root, disposing);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Stores the ConnectionString of a connection changed by the SE that already has
	/// it's linkage tables loaded. We don't want linkage tables disposed if The
	/// ConnectionUrl has not changed when ServerExplorer has updated other properties.
	/// The stored value will be retrieved as a single pass in IVsDataViewSupport
	/// and cleared using <see cref="UnlockLoadedParser"/> or
	/// <see cref="IsLockedLoaded"/>.
	/// Returns true if the connection strings are equivalent and a parser was locked.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static bool LockLoadedParser(string originalString, string updatedString)
	{
		// Tracer.Trace(typeof(RctManager), "LockLoadedParser()");

		_LockedLoadedConnectionString = null;

		// Get the site's parser.
		LinkageParser parser = GetInstanceOrTransient(originalString);


		if (parser == null || !parser.Loaded)
			return false;


		if (!ApcManager.IsConnectionEquivalency(originalString, updatedString))
			return false;

		// Tracer.Trace(typeof(RctManager), "LockLoadedParser()", "LOCKING!!!\nParser TransientString: {0}\nupdatedString: {1}", parser.TransientString, updatedString);

		_LockedLoadedConnectionString = updatedString;

		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The _AsyncPayloadLauncher payload.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private async Task<bool> ExecuteAsync(int asyncProcessId, CancellationToken userCancellationToken, int delay, int multiplier)
	{
		bool result = false;

		// Tracer.Trace(GetType(), "AsyncPayloadTask()", "delay: {0}, multiplier: {1}, UIThread? {2}.", delay, multiplier, ThreadHelper.CheckAccess());

		try
		{
			if (userCancellationToken.IsCancellationRequested)
			{
				_Enabled = false;
			}
			else
			{
				if (AsyncDelay(delay, multiplier, userCancellationToken))
				{
					await PopulateLinkageTablesAsync(userCancellationToken, "AsyncId", asyncProcessId);
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
	private async Task<bool> PopulateLinkageTablesAsync(CancellationToken cancelToken, string idType, int id)
	{
		// Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}] {idType}[{id}] PopulateLinkageTables()", "SyncCardinal: {0}, UIThread: {1}", _SyncCardinal, ThreadHelper.CheckAccess());

		if (_ConnectionString == null)
		{
			// Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}] {idType}[{id}] PopulateLinkageTables()", "Connection null SyncCardinal: {0}", _SyncCardinal);
			ObjectDisposedException ex = new(Resources.ExceptionConnectionStringNull);
			Diag.Dug(ex);
			return false;
		}

		FbConnection connection = null;

		try
		{

			// Tracer.Trace(GetType(), $"ParserId:[{_InstanceId}] {idType}[{id}] PopulateLinkageTables()", "ENTER - _LinkStage: {0}", _LinkStage);
			if (!IsTransient)
			{
				_TaskHandler.Progress(Percent(_LinkStage), _LinkStage == EnLinkStage.Start ? 0 : C_Elapsed_Resuming, _TotalElapsed, _Enabled, AsyncActive);

				Thread.Yield();
			}

			if (_LinkStage < EnLinkStage.GeneratorsLoaded)
			{
				if (!IsTransient)
				{
					_TaskHandler.Progress(Resources.LinkageParserStageGeneratorsBegin,
						Percent(_LinkStage + 1, true), C_Elapsed_StageCompleted, _TotalElapsed, _Enabled, AsyncActive);
				}

				if (connection == null)
				{
					connection = new(ConnectionString);
					await connection.OpenAsync(cancelToken);
				}
				await GetRawGeneratorSchemaAsync(connection, cancelToken);

				if (cancelToken.IsCancellationRequested)
					_Enabled = false;

				if (!IsTransient)
				{
					_TaskHandler.Progress(Resources.LinkageParserStageGeneratorsEnd,
						Percent(_LinkStage), Stopwatch.ElapsedMilliseconds, _TotalElapsed, _Enabled, AsyncActive);
				}
			}

			if (!IsTransient)
				Thread.Yield();

			if (!_Enabled)
				return false;



			if (_LinkStage < EnLinkStage.TriggerDependenciesLoaded)
			{
				if (!IsTransient)
				{
					_TaskHandler.Progress(Resources.LinkageParserStageTriggerDependenciesStart,
						Percent(_LinkStage + 1, true), C_Elapsed_StageCompleted, _TotalElapsed, _Enabled, AsyncActive);
				}

				if (connection == null)
				{
					connection = new(ConnectionString);
					await connection.OpenAsync(cancelToken);
				}
				await GetRawTriggerDependenciesSchemaAsync(connection, cancelToken);

				if (AsyncActive && cancelToken.IsCancellationRequested)
					_Enabled = false;

				if (!IsTransient)
				{
					_TaskHandler.Progress(Resources.LinkageParserStageTriggerDependenciesEnd,
						Percent(_LinkStage), Stopwatch.ElapsedMilliseconds, _TotalElapsed, _Enabled, AsyncActive);
				}
			}

			if (!IsTransient)
				Thread.Yield();

			if (!_Enabled)
				return false;




			if (_LinkStage < EnLinkStage.TriggersLoaded)
			{
				if (!IsTransient)
				{
					_TaskHandler.Progress(Resources.LinkageParserStageTriggersBegin, Percent(_LinkStage + 1, true),
						C_Elapsed_StageCompleted, _TotalElapsed, _Enabled, AsyncActive);
				}

				if (connection == null)
				{
					connection = new(ConnectionString);
					await connection.OpenAsync(cancelToken);
				}
				await GetRawTriggerSchemaAsync(connection, cancelToken);

				if (cancelToken.IsCancellationRequested)
					_Enabled = false;

				if (!IsTransient)
				{
					_TaskHandler.Progress(Resources.LinkageParserStageTriggersEnd, Percent(_LinkStage),
						Stopwatch.ElapsedMilliseconds, _TotalElapsed, _Enabled, AsyncActive);
				}
			}

			if (!_Enabled)
				return false;

			if (!IsTransient)
				Thread.Yield();
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}
		finally
		{
			if (connection != null)
			{
				try
				{
					await connection.CloseAsync();
					connection.Dispose();
				}
				catch { }
			}

		}


		try
		{
			if (!SequencesPopulated)
			{
				if (!IsTransient)
				{
					_TaskHandler.Progress(Resources.LinkageParserStageSequencesBegin, Percent(_LinkStage + 1, true),
						C_Elapsed_StageCompleted, _TotalElapsed, _Enabled, AsyncActive);
				}

				BuildSequenceTable();

				if (cancelToken.IsCancellationRequested)
					_Enabled = false;

				if (!IsTransient)
				{
					_TaskHandler.Progress(Resources.LinkageParserStageSequencesEnd, Percent(_LinkStage),
						Stopwatch.ElapsedMilliseconds, _TotalElapsed, _Enabled, AsyncActive);
				}
			}

			if (!IsTransient)
				Thread.Yield();


			if (!_Enabled)
				return false;
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}


		try
		{
			if (_LinkStage < EnLinkStage.Completed)
			{
				if (!IsTransient)
				{
					_TaskHandler.Progress(Resources.LinkageParserStageLinkingBegin, Percent(_LinkStage + 1, true),
						C_Elapsed_StageCompleted, _TotalElapsed, _Enabled, AsyncActive);
				}

				BuildTriggerTable();

				if (!IsTransient)
				{
					_TaskHandler.Progress(Resources.LinkageParserStageLinkingEnd, Percent(_LinkStage),
						Stopwatch.ElapsedMilliseconds, _TotalElapsed, _Enabled, AsyncActive);
				}
			}

			_LinkStage = EnLinkStage.Completed;
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}


		try
		{
			if (!IsTransient)
			{
				_TaskHandler.Progress(Resources.LinkageParserStageCompleted.FmtRes(_Triggers.Rows.Count),
					Percent(_LinkStage), Stopwatch.ElapsedMilliseconds, _TotalElapsed, _Enabled, AsyncActive);
			}

			_Stopwatch = null;

			if (!IsTransient)
				_TaskHandler.Status(_LinkStage, _TotalElapsed, _Enabled, AsyncActive);

			lock (_LockGlobal)
			{
				if (!IsTransient && _TransientInstance != null
					&& ApcManager.IsWeakConnectionEquivalency(_ConnectionString, _TransientInstance.ConnectionString))
				{
					_TransientInstance = null;
				}

				if (IsTransient)
				{
					_Sequences = null;

					/*
					 * Uncomment this routine if we ever want to have sequences by table.
					 * 
					object @object;
					string tableName = _TransientRestrictions[2];

					List<DataRow> rows = new(_Sequences.Rows.Count);


					foreach (DataRow seqRow in _Sequences.Rows)
					{
						@object = seqRow["DEPENDENCY_TABLE"];

						if (!Cmd.IsNullValueOrEmpty(@object) && tableName.Equals(@object.ToString(), StringComparison.InvariantCultureIgnoreCase))
							continue;

						rows.Add(seqRow);
					}

					_Sequences.BeginLoadData();

					foreach(DataRow row in rows)
						row.Delete();

					_Sequences.EndLoadData();
					_Sequences.AcceptChanges();
					*/
				}
			}

		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}

		return true;
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

		if (((AdvancedMessageBox)sender).InvokeRequired)
		{
			Action safeInitialize = delegate { ThreadSafeInitializeMessageBox(sender); };
			((AdvancedMessageBox)sender).Invoke(safeInitialize);
		}
		else
		{
			lock (_LockObject)
			{
				_MessageBoxDlg = (AdvancedMessageBox)sender;
				_MessageBoxDlg.FormClosed += OnMessageBoxClose;

				_MessageBoxDlg.Activate();
			}
		}
	}



	// ---------------------------------------------------------------------------------
	/// Clears a connection that has been locked against disposal by
	/// <see cref="LockLoadedParser"/>.
	// ---------------------------------------------------------------------------------
	public static void UnlockLoadedParser()
	{
		// Tracer.Trace(typeof(RctManager), "UnlockLoadedParser()", "\n_LockedLoadedConnectionString: {0}", _LockedLoadedConnectionString);

		_LockedLoadedConnectionString = null;
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



	public void OnMessageBoxClose(object sender, FormClosedEventArgs e)
	{
		lock (_LockObject)
			_MessageBoxDlg = null;
	}



	private void OnMessageBoxShown(object sender, EventArgs e)
	{
		// Tracer.Trace(GetType(), "OnMessageBoxShown()", "Sender type: {0}", sender);

		ThreadStart threadParameters = new ThreadStart(delegate { ThreadSafeInitializeMessageBox(sender); });
		Thread thread = new Thread(threadParameters);
		thread.Start();
	}


	#endregion Event handlers


}
