// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.DataModel.QueryExecutor

using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BlackbirdSql.Core.Enums;
using BlackbirdSql.Core.Events;
using BlackbirdSql.Shared.Ctl.Config;
using BlackbirdSql.Shared.Enums;
using BlackbirdSql.Shared.Events;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Shared.Model;
using BlackbirdSql.Shared.Properties;
using BlackbirdSql.Sys.Enums;
using BlackbirdSql.Sys.Interfaces;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.Utilities;



namespace BlackbirdSql.Shared.Ctl.QueryExecution;


// =========================================================================================================
//										QueryManager Class
//
/// <summary>
/// Manages query execution of an active document.
/// </summary>
// =========================================================================================================
public sealed class QueryManager : IBsQueryManager
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - QueryManager
	// ---------------------------------------------------------------------------------


	/// <summary>
	/// Default .ctor.
	/// </summary>
	public QueryManager(AuxilliaryDocData owner, ConnectionStrategy strategy)
	{
		_Owner = owner;
		_SqlExec = new QESQLExec(this);
		Strategy = strategy;
		RegisterSqlExecWithEvenHandlers();

		_KeepAliveCancelTokenSource = new();

		CancellationToken cancelToken = _KeepAliveCancelTokenSource.Token;

		// Fire and forget.
		Task.Factory.StartNew(() => KeepAliveMonitoringAsync(cancelToken),
			default, TaskCreationOptions.PreferFairness | TaskCreationOptions.DenyChildAttach,
			TaskScheduler.Default).Forget();
	}



	public void Dispose()
	{
		Dispose(bDisposing: true);
		GC.SuppressFinalize(this);
	}



	private void Dispose(bool bDisposing)
	{
		lock (_LockLocal)
		{
			_KeepAliveCancelTokenSource?.Cancel();
			_KeepAliveCancelTokenSource?.Dispose();
			_KeepAliveCancelTokenSource = null;

			ExecutionStartedEvent = null;
			ExecutionCompletedEvent = null;
			BatchStatementCompletedEvent = null;
			BatchExecutionCompletedEvent = null;
			ErrorMessageEvent = null;
			StatusChangedEvent = null;
			try
			{
				if (IsExecuting)
				{
					Cancel(synchronous: true);
				}
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
			}

			if (_SqlExec != null)
			{
				UnRegisterSqlExecWithEventHandlers();
				_SqlExec.Dispose();
				_SqlExec = null;
			}

			Strategy?.Dispose();
		}
	}



	void IBsQueryManager.DisposeTransaction(bool force) => Strategy?.DisposeTransaction(force);


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields & Constants - QueryManager
	// =========================================================================================================


	// A private 'this' object lock
	private readonly object _LockLocal = new object();

	private int _ConnectionValidationCardinal = 0;
	private QESQLExec.GetCurrentWorkingDirectoryPath _CurrentWorkingDirectoryPath;
	private int _EventExecutingCardinal = 0;

	private long _KeepAliveConnectionStartTimeEpoch = long.MinValue;

	private TransientSettings _LiveSettings;
	private bool _LiveSettingsApplied;
	private readonly AuxilliaryDocData _Owner = null;
	private long _RowsAffected;
	private int _StatementCount;
	private QESQLExec _SqlExec;
	private uint _Status;
	private ConnectionStrategy _Strategy;
	private TimeSpan _SyncCancelTimeout = new TimeSpan(0, 0, 30);

	private CancellationTokenSource _KeepAliveCancelTokenSource = null;


	#endregion Fields & Constants





	// =========================================================================================================
	#region Property Accessors- QueryManager
	// =========================================================================================================


	public IDbConnection DataConnection => _Strategy?.Connection;


	public ConnectionState DataConnectionState => DataConnection?.State ?? ConnectionState.Closed;

	public QESQLExec.GetCurrentWorkingDirectoryPath CurrentWorkingDirectoryPath
	{
		set
		{
			lock (_LockLocal)
			{
				_CurrentWorkingDirectoryPath = value;
				_SqlExec.CurrentWorkingDirectoryPath = _CurrentWorkingDirectoryPath;
			}
		}
	}


	public long ExecutionTimeout => _LiveSettings.EditorExecutionTimeout * 60000L;

	public bool HasTransactions => Strategy != null && Strategy.HasTransactions;


	public bool IsCancelling
	{
		get { return GetStatusFlag(EnQueryStatusFlags.Cancelling); }
		set { SetStatusFlag(value, EnQueryStatusFlags.Cancelling); }
	}


	public bool IsConnected
	{
		get { return GetStatusFlag(EnQueryStatusFlags.Connected); }
		private set { SetStatusFlag(value, EnQueryStatusFlags.Connected); }
	}



	public bool IsConnecting
	{
		get { return GetStatusFlag(EnQueryStatusFlags.Connecting); }
		set { SetStatusFlag(value, EnQueryStatusFlags.Connecting); }
	}


	public bool IsExecuting => GetStatusFlag(EnQueryStatusFlags.Executing);


	public bool IsLocked => !EventLockEnter(false);


	public bool IsWithActualPlan => LiveSettings.WithActualPlan;

	public bool IsWithClientStats => LiveSettings.WithClientStats;


	public AuxilliaryDocData Owner => _Owner;


	public TransientSettings LiveSettings
	{
		get
		{
			lock (_LockLocal)
			{
				if (_LiveSettings != null)
					return _LiveSettings;

				_LiveSettings = TransientSettings.CreateInstance();
				_LiveSettings.TtsEnabled = _LiveSettings.EditorExecutionTtsDefault;

				return _LiveSettings;
			}
		}
	}


	public bool LiveSettingsApplied
	{
		get { lock (_LockLocal) return _LiveSettingsApplied; }
		set { lock (_LockLocal) _LiveSettingsApplied = value; }
	}


	public DateTime? QueryExecutionEndTime { get; set; }

	public DateTime? QueryExecutionStartTime { get; set; }

	public IBsQESQLBatchConsumer ResultsHandler { get; set; }


	public long RowsAffected
	{
		get
		{
			lock (_LockLocal)
				return _RowsAffected;
		}
	}

	public int StatementCount
	{
		get
		{
			lock (_LockLocal)
				return _StatementCount;
		}
	}


	public ConnectionStrategy Strategy
	{
		get
		{
			lock (_LockLocal)
				return _Strategy;
		}
		set
		{
			lock (_LockLocal)
			{
				bool newConnection = !ReferenceEquals(value, _Strategy);

				if (!newConnection)
					return;

				if (_Strategy != null)
				{
					_Strategy.ConnectionChangedEvent -= OnConnectionChanged;
					_Strategy.DatabaseChangedEvent -= OnDatabaseChanged;
					_Strategy.Dispose();
				}

				_Strategy = value;
				_Strategy.ConnectionChangedEvent += OnConnectionChanged;
				_Strategy.DatabaseChangedEvent += OnDatabaseChanged;

				SetStateForConnection(DataConnection);

				bool open = DataConnectionState == ConnectionState.Open;

				EnQueryStatusFlags status = open
					? EnQueryStatusFlags.Connected : EnQueryStatusFlags.Connection;

				RaiseStatusChanged(status, open, true);
			}
		}
	}


	public IDbTransaction Transaction => Strategy?.Transaction;

	public IsolationLevel TtsIsolationLevel => LiveSettings.EditorExecutionIsolationLevel;



	public event QueryStatusChangedEventHandler StatusChangedEvent;
	public event QueryExecutionStartedEventHandler ExecutionStartedEvent;
	public event QueryExecutionCompletedEventHandler ExecutionCompletedEvent;
	public event BatchExecutionCompletedEventHandler BatchExecutionCompletedEvent;
	public event ErrorMessageEventHandler ErrorMessageEvent;
	public event BatchStatementCompletedEventHandler BatchStatementCompletedEvent;

	// Added for StaticsPanel.RetrieveStatisticsIfNeeded();
	public event QueryDataEventHandler BatchDataLoadedEvent;
	public event QueryDataEventHandler BatchScriptParsedEvent;


	#endregion Property Accessors





	// =========================================================================================================
	#region Methods- QueryManager
	// =========================================================================================================


	/// <summary>
	/// <see cref="ErrorHandler.Succeeded"/> token.
	/// </summary>
	private bool __(int hr) => ErrorHandler.Succeeded(hr);



	public bool AsyncExecute(IBsTextSpan textSpan, EnSqlExecutionType executionType)
	{
		// Tracer.Trace(GetType(), "AsyncExecute()", " Enter. : ExecutionOptions.EstimatedPlanOnly: " + LiveSettings.EstimatedPlanOnly);

		if (!EventExecutingEnter())
		{
			InvalidOperationException ex = new(Resources.ExExecutionNotCompleted);
			Diag.Dug(ex);
			throw ex;
		}

		if (textSpan == null || string.IsNullOrWhiteSpace(textSpan.Text))
		{
			ArgumentException ex = new("Query is empty");
			MessageCtl.ShowEx(ex, Resources.ExDatabaseNotAccessible, null, MessageBoxButtons.OK, MessageBoxIcon.Hand);
			EventExecutingExit();
			return false;
		}

		// Fire and forget.
		Task.Factory.StartNew(() => ExecuteAsync(textSpan, executionType),
			default, TaskCreationOptions.PreferFairness | TaskCreationOptions.DenyChildAttach,
			TaskScheduler.Default).Forget();

		return true;
	}


	private async Task<bool> ExecuteAsync(IBsTextSpan textSpan, EnSqlExecutionType executionType)
	{
		IDbConnection connection;


		SetStatusFlag(false, EnQueryStatusFlags.Executing);
		SetStatusFlag(true, EnQueryStatusFlags.Connecting);
		RefreshToolbar();

		try
		{
			connection = await Strategy.EnsureVerifiedOpenConnectionAsync();
		}
		catch (Exception ex)
		{
			Diag.Expected(ex);

			SetStatusFlag(false, EnQueryStatusFlags.Connecting);
			SetStatusFlag(true, EnQueryStatusFlags.Executing);

			IsCancelling = true;
			RefreshToolbar();
			ShowWindowFrame();

			MessageCtl.ShowEx(ex, Resources.ExDatabaseNotAccessible, null, MessageBoxButtons.OK, MessageBoxIcon.Hand);

			RaiseExecutionCompleted(EnScriptExecutionResult.Failure, executionType, LiveSettings.EditorResultsOutputMode, LiveSettings.WithClientStats);
			EventExecutingExit();
			return false;
		}

		SetStatusFlag(false, EnQueryStatusFlags.Connecting);

		if (connection == null)
		{
			RaiseExecutionCompleted(EnScriptExecutionResult.Cancel, executionType, LiveSettings.EditorResultsOutputMode, LiveSettings.WithClientStats);
			EventExecutingExit();
			return false;
		}

		SetStatusFlag(true, EnQueryStatusFlags.Executing);


		// Tracer.Trace(GetType(), "AsyncExecute()", "Ensured connection");


		// Tracer.Trace(GetType(), "AsyncExecute()", "execTimeout = {0}.", execTimeout);

		if (!LiveSettingsApplied)
			LiveSettingsApplied = true;

		LiveSettings.ExecutionType = executionType;


		TransientSettings liveSettings = (TransientSettings)LiveSettings.Clone();

		// Tracer.Trace(GetType(), "AsyncExecute()", "executionType: {0},  LiveSettings.ExecutionType: {1}, liveSettings.ExecutionType: {2}.",
		//	executionType,  LiveSettings.ExecutionType, liveSettings.ExecutionType);


		if (liveSettings.WithClientStats && liveSettings.ExecutionType != EnSqlExecutionType.PlanOnly)
		{
			Strategy.ResetAndEnableConnectionStatistics();
		}


		try
		{
			if (!await RaiseExecutionStartedAsync(textSpan.Text, executionType, connection))
			{
				RaiseExecutionCompleted(EnScriptExecutionResult.Halted, executionType, liveSettings.EditorResultsOutputMode, liveSettings.WithClientStats);
				EventExecutingExit();
				return false;
			}
		}
		catch (Exception ex)
		{
			RaiseExecutionCompleted(EnScriptExecutionResult.Failure, executionType, liveSettings.EditorResultsOutputMode, liveSettings.WithClientStats);
			EventExecutingExit();
			Diag.Dug(ex);
			throw;
		}



		// -------------------------------------------------------------------------------- //
		// ******************** Execution Point (2) - QueryManager.AsyncExecute() ******************** //
		// --------------------------------------------------------------------------------

		try
		{
			return await _SqlExec.ExecuteAsync(textSpan, executionType, connection, ResultsHandler, liveSettings);
		}
		catch
		{
			RaiseExecutionCompleted(EnScriptExecutionResult.Failure, executionType, liveSettings.EditorResultsOutputMode, liveSettings.WithClientStats);
			EventExecutingExit();
			throw;
		}

	}



	void IBsQueryManager.BeginTransaction()
	{
		Strategy?.BeginTransaction(LiveSettings.EditorExecutionIsolationLevel);
	}



	public void Cancel(bool synchronous)
	{
		if (!IsExecuting)
			return;

		QueryExecutionCompletedEventHandler executionCompletedHandler = null;
		try
		{
			lock (_LockLocal)
			{
				IsCancelling = true;

				try
				{
					_SqlExec.Cancel(_SyncCancelTimeout);
				}
				finally
				{
					if (synchronous)
					{
						// IsExecuting = false;
						UnRegisterSqlExecWithEventHandlers();
						_SqlExec = new QESQLExec(this);
						RegisterSqlExecWithEvenHandlers();
						executionCompletedHandler = ExecutionCompletedEvent;
					}
				}
			}
		}
		finally
		{
			IsCancelling = false;
			if (executionCompletedHandler != null && synchronous)
			{
				executionCompletedHandler(this, new QueryExecutionCompletedEventArgs(EnScriptExecutionResult.Cancel));
			}
		}
	}


	void IBsQueryManager.CloseConnection() => Strategy?.CloseConnection();



	public bool CommitTransactions()
	{
		if (!IsConnected || !EventLockEnter())
			return false;

		IsConnecting = true;
		bool result = false;

		try
		{
			result = Strategy.CommitTransactions();
		}
		finally
		{
			SetStatusFlag(false, EnQueryStatusFlags.Connecting, false);
			EventLockExit();
		}

		return result;
	}



	public void Disconnect()
	{
		if (!EventLockEnter())
			return;

		try
		{
			// Strategy.DisposeTransaction(true);
			Strategy.CloseConnection();
			Strategy.ResetConnection();
			GetUpdateTransactionsStatus(true);
		}
		finally
		{
			EventLockExit();
		}
	}


	public bool EnsureVerifiedOpenConnection()
	{
		if (!EventLockEnter())
			return false;

		IsConnecting = true;

		long stamp = Strategy.ConnectionStamp;

		// Fire and forget

		Task.Factory.StartNew(
			async () =>
			{
				try
				{
					await Strategy?.EnsureVerifiedOpenConnectionAsync();
				}
				catch (Exception ex)
				{
					IsConnecting = true;
					ShowWindowFrame();
					MessageCtl.ShowEx(ex, Resources.ExDatabaseNotAccessible, null, MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
				finally
				{
					SetStatusFlag(false, EnQueryStatusFlags.Connecting, stamp != Strategy.ConnectionStamp);
					EventLockExit();
				}
			},
			default, TaskCreationOptions.PreferFairness, TaskScheduler.Default).Forget();

		return true;
	}



	private bool GetStatusFlag(EnQueryStatusFlags statusType)
	{
		lock (_LockLocal)
			return (_Status & (uint)statusType) == (uint)statusType;
	}



	public bool GetUpdateTransactionsStatus(bool supressExceptions)
	{
		if (!EventLockEnter(true, true))
			return false;

		try
		{
			return Strategy.GetUpdateTransactionsStatus(supressExceptions);
		}
		finally {
			EventLockExit();
		}
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Low overhead asynchronous connection keep alive and monitoring task.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private async Task KeepAliveMonitoringAsync(CancellationToken cancelToken)
	{
		await Cmd.AwaitableAsync();


		while (_KeepAliveCancelTokenSource != null && !cancelToken.IsCancellationRequested)
		{
			bool connected = GetStatusFlag(EnQueryStatusFlags.Connected);

			if (connected && !IsLocked)
			{

				_ConnectionValidationCardinal++;

				bool hadTransactions = Strategy?.HadTransactions ?? false;

				if (DataConnection == null)
				{
					_ConnectionValidationCardinal = 0;
					_KeepAliveConnectionStartTimeEpoch = long.MinValue;

					// ConnectionChangedEventArgs args = new(null, null);
					// OnConnectionChanged(this, args);

					RefreshToolbar();

					string prefix = Resources.WarnQueryPrefix;
					string indent = new string(' ', prefix.Length);
					string msg = Resources.WarnQueryConnectionDead.FmtRes(prefix, Strategy.ConnInfo.DatasetKey, indent);

					if (hadTransactions)
						msg += "\n" + Resources.WarnQueryTransactionsDiscarded.FmtRes(indent);


					_ = Diag.OutputPaneWriteLineAsync(msg, true);
				}
				else if ((_ConnectionValidationCardinal % LibraryData.C_ConnectionValidationModulus) == 0)
				{
					_ConnectionValidationCardinal = 0;

					if (!hadTransactions)
					{
						int connectionLifeTime = Strategy.ConnInfo.ConnectionLifeTime;

						if (connectionLifeTime > 0)
						{
							if (_KeepAliveConnectionStartTimeEpoch == long.MinValue)
							{
								_KeepAliveConnectionStartTimeEpoch = DateTime.Now.UnixMilliseconds();
							}
							else
							{
								long currentTimeEpoch = DateTime.Now.UnixMilliseconds();

								if (currentTimeEpoch - _KeepAliveConnectionStartTimeEpoch > (connectionLifeTime * 1000))
								{
									_KeepAliveConnectionStartTimeEpoch = long.MinValue;
									connected = false;

									Strategy.CloseConnection();
									RefreshToolbar();

									_ = Diag.OutputPaneWriteLineAsync(Resources.InfoConnectionAutoClosed.FmtRes(Strategy.ConnInfo.DatasetKey, connectionLifeTime), true);
								}

							}
						}
						else
						{
							_KeepAliveConnectionStartTimeEpoch = long.MinValue;
						}
					}
					else
					{
						_KeepAliveConnectionStartTimeEpoch = long.MinValue;
					}


					if (connected)
					{
						long connectionId = Strategy.ConnInfo.ConnectionId;
						// IDbConnection connection = Connection;

						try
						{
							_ = await Strategy.ConnInfo.OpenOrVerifyConnectionAsync();
						}
						catch { }


						if (_KeepAliveCancelTokenSource == null || cancelToken.IsCancellationRequested || ApcManager.SolutionClosing)
							break;


						if (DataConnection == null && Strategy.ConnInfo != null
								&& connectionId == Strategy.ConnInfo.ConnectionId)
						{
							RaiseStatusChanged(EnQueryStatusFlags.Connected, false, false);
							// RefreshToolbar();

							string prefix = Resources.WarnQueryPrefix;
							string indent = new string(' ', prefix.Length);
							string msg = Resources.WarnQueryConnectionReset.FmtRes(prefix, Strategy.ConnInfo.DatasetKey, indent);

							if (hadTransactions)
								msg += "\n" + Resources.WarnQueryTransactionsDiscarded.FmtRes(indent);


							_ = Diag.OutputPaneWriteLineAsync(msg, true);
						}
					}
				}
			}
			else
			{
				if (!connected || IsExecuting)
					_KeepAliveConnectionStartTimeEpoch = long.MinValue;

				_ConnectionValidationCardinal = 0;
			}

			Thread.Sleep(100);
		}
	}



	public bool ModifyConnection()
	{
		if (!EventLockEnter())
			return false;

		IsConnecting = true;

		long stamp = Strategy.ConnectionStamp;

		try
		{
			Strategy.ModifyConnection();
		}
		catch (Exception ex)
		{
			MessageCtl.ShowEx(ex, Resources.ExDatabaseNotAccessible, null, MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
		finally
		{
			SetStatusFlag(false, EnQueryStatusFlags.Connecting, stamp != Strategy.ConnectionStamp);
			EventLockExit();
		}

		return false;
	}



	private void RefreshToolbar()
	{
		uint cookie = Owner.DocCookie;

		if (cookie == 0)
			return;

		if (!ThreadHelper.CheckAccess())
		{
			// Fire and wait.

			bool result = ThreadHelper.JoinableTaskFactory.Run(async delegate
			{
				await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

				IVsWindowFrame frame = RdtManager.GetWindowFrame(cookie);

				if (frame == null)
					return false;

				if (!__(frame.GetProperty((int)__VSFPROPID.VSFPROPID_ToolbarHost, out object pToolbar)))
					return false;

				if (pToolbar == null)
					return false;

				if (pToolbar is not IVsToolWindowToolbarHost toolbarHost)
					return false;

				toolbarHost.ForceUpdateUI();

				Application.DoEvents();

				return true;
			});

		}
		else
		{
			IVsWindowFrame frame = RdtManager.GetWindowFrame(cookie);

			if (frame == null)
				return;

			if (!__(frame.GetProperty((int)__VSFPROPID.VSFPROPID_ToolbarHost, out object pToolbar)))
				return;

			if (pToolbar is not IVsToolWindowToolbarHost toolbarHost)
				return;

			toolbarHost.ForceUpdateUI();

			Application.DoEvents();
		}
	}








	private void RegisterSqlExecWithEvenHandlers()
	{
		lock (_LockLocal)
		{
			_SqlExec.ExecutionCompletedEvent += OnExecutionCompleted;
			_SqlExec.BatchExecutionCompletedEvent += OnBatchExecutionCompleted;
			_SqlExec.BatchExecutionStartEvent += OnBatchExecutionStart;
			_SqlExec.BatchDataLoadedEvent += OnBatchDataLoaded;
			_SqlExec.BatchScriptParsedEvent += OnBatchScriptParsed;
			_SqlExec.BatchStatementCompletedEvent += OnBatchStatementCompleted;
			_SqlExec.ErrorMessageEvent += OnErrorMessage;
		}
	}



	public bool RollbackTransactions()
	{
		if (!IsConnected || !EventLockEnter())
			return false;

		IsConnecting = true;
		bool result = false;

		try
		{
			result = Strategy.RollbackTransactions();
		}
		finally
		{
			SetStatusFlag(false, EnQueryStatusFlags.Connecting, false);
			EventLockExit();
		}

		return result;
	}



	private void SetStateForConnection(IDbConnection newConnection)
	{
		if (newConnection != null)
		{
			if (newConnection is DbConnection dbConnection)
			{
				dbConnection.StateChange += OnConnectionStateChanged;
			}

			// IsExecuting = newConnection.State == ConnectionState.Executing;
			IsConnected = newConnection.State == ConnectionState.Open;
			IsConnecting = newConnection.State == ConnectionState.Connecting;
		}
		else
		{
			// IsExecuting = false;
			IsConnected = false;
			IsConnecting = false;
		}

		IsCancelling = false;
	}


	/// <summary>
	/// Sets the status flag of a status setting. If a secondary flag is provided the
	/// QueryStatusChangedEventArgs will include it in a single OnStatyusChanged event.
	/// </summary>
	private void SetStatusFlag(bool enable, EnQueryStatusFlags statusFlag, bool newConnection = true)
	{
		lock (_LockLocal)
		{
			bool current = (_Status & (uint)statusFlag) == (uint)statusFlag;

			if (current == enable)
				return;

			if (enable)
				_Status |= (uint)statusFlag;
			else
			_Status &= (uint)~statusFlag;

			RaiseStatusChanged(statusFlag, enable, newConnection);
		}
	}



	public void ShowWindowFrame()
	{
		uint cookie = Owner.DocCookie;
		if (cookie == 0)
			return;

		if (!ThreadHelper.CheckAccess())
		{
			// Fire and wait.

			bool result = ThreadHelper.JoinableTaskFactory.Run(async delegate
			{
				await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

				RdtManager.ShowFrame(cookie);

				return true;
			});

		}
		else
		{
			RdtManager.ShowFrame(cookie);
		}
	}



	private void UnRegisterSqlExecWithEventHandlers()
	{
		lock (_LockLocal)
		{
			_SqlExec.ExecutionCompletedEvent -= OnExecutionCompleted;
			_SqlExec.BatchExecutionCompletedEvent -= OnBatchExecutionCompleted;
			_SqlExec.BatchExecutionStartEvent -= OnBatchExecutionStart;
			_SqlExec.BatchDataLoadedEvent -= OnBatchDataLoaded;
			_SqlExec.BatchScriptParsedEvent -= OnBatchScriptParsed;
			_SqlExec.BatchStatementCompletedEvent -= OnBatchStatementCompleted;
			_SqlExec.ErrorMessageEvent -= OnErrorMessage;
		}
	}


	#endregion Methods





	// =========================================================================================================
	#region Event Handling- QueryManager
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Increments the <see cref="_EventExecutingCardinal"/> counter and sets the
	/// execution status flag on query execution.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public bool EventExecutingEnter(bool increment = true, bool force = false)
	{
		lock (_LockLocal)
		{
			if (increment && !force)
				_KeepAliveConnectionStartTimeEpoch = long.MinValue;

			if (_EventExecutingCardinal != 0 && !force)
				return false;

			if (increment)
			{
				_EventExecutingCardinal++;

				if (_EventExecutingCardinal == 1)
				{
					SetStatusFlag(true, EnQueryStatusFlags.Executing);
					RefreshToolbar();
				}
			}
		}

		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Decrements the <see cref="_EventExecutingCardinal"/> counter that was previously
	/// incremented by <see cref="EventExecutingEnter"/>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public void EventExecutingExit()
	{
		lock (_LockLocal)
		{
			if (_EventExecutingCardinal <= 0)
			{
				ApplicationException ex = new($"Attempt to exit event when not in an event. _EventCardinal: {_EventExecutingCardinal}");
				Diag.Dug(ex);
				throw ex;
			}

			_EventExecutingCardinal--;

			if (_EventExecutingCardinal == 0)
				SetStatusFlag(false, EnQueryStatusFlags.Executing);

		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Increments the <see cref="_EventExecutingCardinal"/> counter when any action
	/// involves the database.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public bool EventLockEnter(bool increment = true, bool force = false)
	{
		lock (_LockLocal)
		{
			if (increment && !force)
				_KeepAliveConnectionStartTimeEpoch = long.MinValue;

			if (Strategy == null)
				return false;

			if (_EventExecutingCardinal != 0 && !force)
				return false;

			if (increment)
			{
				_EventExecutingCardinal++;

				if (!force && _EventExecutingCardinal == 1)
					RefreshToolbar();
			}
		}

		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Decrements the <see cref="_EventExecutingCardinal"/> counter that was previously
	/// incremented by <see cref="EventLockEnter"/>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public void EventLockExit()
	{
		lock (_LockLocal)
		{
			if (_EventExecutingCardinal <= 0)
			{
				ApplicationException ex = new($"Attempt to exit event when not in an event. _EventCardinal: {_EventExecutingCardinal}");
				Diag.Dug(ex);
				throw ex;
			}

			_EventExecutingCardinal--;
		}
	}



	private void OnBatchExecutionCompleted(object sender, BatchExecutionCompletedEventArgs args)
	{
		// Tracer.Trace(GetType(), "OnBatchExecutionCompleted()", "", null);
		_RowsAffected += args.Batch.RowsAffected;

		BatchExecutionCompletedEvent?.Invoke(sender, args);
	}



	private void OnBatchExecutionStart(object sender, BatchExecutionStartEventArgs args)
	{
		// Tracer.Trace(GetType(), "OnBatchExecutionStart()", "", null);
	}



	private void OnConnectionChanged(object sender, ConnectionChangedEventArgs args)
	{
		// Tracer.Trace(GetType(), "OnConnectionChanged()", "Current: {0}, Previous: {1}.", args.CurrentConnection != null, args.PreviousConnection != null);

		lock (_LockLocal)
		{
			_RowsAffected = 0L;
			_StatementCount = 0;
			QueryExecutionStartTime = null;
			QueryExecutionEndTime = null;
			LiveSettingsApplied = false;
			IDbConnection previousConnection = args.PreviousConnection;
			IDbConnection currentConnection = args.CurrentConnection;
			LiveSettings.EditorExecutionTimeout = Strategy.GetExecutionTimeout();

			if (previousConnection != null && previousConnection is DbConnection dbConnection)
			{
				dbConnection.StateChange -= OnConnectionStateChanged;
			}

			SetStateForConnection(currentConnection);

			bool open = DataConnectionState == ConnectionState.Open;

			EnQueryStatusFlags status = open
				? EnQueryStatusFlags.Connected : EnQueryStatusFlags.Connection;

			RaiseStatusChanged(status, open, true);
		}
	}



	private void OnConnectionStateChanged(object sender, StateChangeEventArgs args)
	{
		lock (_LockLocal)
		{
			using (DpiAwareness.EnterDpiScope(DpiAwarenessContext.SystemAware))
			{
				ConnectionState currentState = args.CurrentState;
				ConnectionState originalState = args.OriginalState;
				if (currentState == ConnectionState.Connecting)
				{
					IsConnecting = true;
				}
				else
				{
					IsConnecting = false;
				}

				switch (currentState)
				{
					case ConnectionState.Broken:
						string prefix = Resources.WarnQueryPrefix;
						string indent = new string(' ', prefix.Length);
						string msg = Resources.WarnQueryConnectionBroken.FmtRes(prefix, Strategy.ConnInfo.DatasetKey, indent);
						if (Strategy?.HadTransactions ?? false)
							msg += "\n" + Resources.WarnQueryTransactionsDiscarded.FmtRes(indent);

						Diag.AsyncOutputPaneWriteLine(msg, true);

						IsConnected = false;
						LiveSettingsApplied = false;
						break;
					case ConnectionState.Closed:
						IsConnected = false;
						LiveSettingsApplied = false;

						// if (IsExecuting)
						//	IsExecuting = false;

						break;
					case ConnectionState.Open:
						if (originalState == ConnectionState.Closed || originalState == ConnectionState.Connecting)
						{
							IsConnected = true;
							LiveSettingsApplied = false;
						}

						break;
				}
			}
		}
	}


	private void OnDatabaseChanged(object sender, EventArgs args) =>
		RaiseStatusChanged(EnQueryStatusFlags.DatabaseChanged, true, true);



	private void OnBatchDataLoaded(object sender, QueryDataEventArgs eventArgs)
	{
		BatchDataLoadedEvent?.Invoke(sender, eventArgs);
	}

	private void OnBatchScriptParsed(object sender, QueryDataEventArgs eventArgs)
	{
		_StatementCount = eventArgs.StatementCount;

		BatchScriptParsedEvent?.Invoke(sender, eventArgs);
	}


	private void OnExecutionCompleted(object sender, QueryExecutionCompletedEventArgs args)
	{
		QueryExecutionEndTime = DateTime.Now;

		// Tracer.Trace(GetType(), "OnExecutionCompleted()", "Calling ExecutionCompletedEvent. args.ExecResult: {0}.", args.ExecutionResult);

		try
		{
			ExecutionCompletedEvent?.Invoke(this, args);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;
		}

		// IsExecuting = false;
		IsCancelling = false;

		if (LiveSettings.EditorExecutionDisconnectOnCompletion)
			Strategy.CloseConnection();
	}



	private void OnErrorMessage(object sender, ErrorMessageEventArgs args)
	{
		ErrorMessageEvent?.Invoke(sender, args);
	}






	// Added for StaticsPanel.RetrieveStatisticsIfNeeded();
	private void OnBatchStatementCompleted(object sender, BatchStatementCompletedEventArgs args) =>
		BatchStatementCompletedEvent?.Invoke(sender, args);



	private void RaiseStatusChanged(EnQueryStatusFlags statusFlag, bool enabled, bool newConnection) =>
		StatusChangedEvent?.Invoke(this, new(statusFlag, enabled, newConnection));




	private void RaiseExecutionCompleted(EnScriptExecutionResult executionResult,
			EnSqlExecutionType executionType, EnSqlOutputMode outputMode, bool withClientStats)
	{
		QueryExecutionEndTime = DateTime.Now;

		// Tracer.Trace(GetType(), "RaiseExecutionCompleted()");

		QueryExecutionCompletedEventArgs args = new(executionResult, executionType, outputMode, withClientStats);

		try
		{
			ExecutionCompletedEvent?.Invoke(this, args);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
		}

		// IsExecuting = false;
		IsCancelling = false;

		if (LiveSettings.EditorExecutionDisconnectOnCompletion)
			Strategy.CloseConnection();
	}


	private async Task<bool> RaiseExecutionStartedAsync(string queryText, EnSqlExecutionType executionType, IDbConnection connection)
	{
		await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

		_RowsAffected = 0L;
		_StatementCount = 0;

		QueryExecutionStartTime = DateTime.Now;
		QueryExecutionEndTime = DateTime.Now;


		QueryExecutionStartedEventArgs args = new(queryText, executionType, connection);

		return ExecutionStartedEvent?.Invoke(this, args) ?? true;
	}


	#endregion Event Handling





	// =========================================================================================================
	#region Sub-Classes- QueryManager
	// =========================================================================================================


	#endregion Sub-Classes

}
