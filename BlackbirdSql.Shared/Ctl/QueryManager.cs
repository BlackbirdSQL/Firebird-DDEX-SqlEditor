// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.DataModel.QueryExecutor

using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using System.Windows.Controls;
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
	public QueryManager(ConnectionStrategy strategy)
	{
		_SqlExec = new QESQLExec(this);

		_Strategy = strategy;
		_Strategy.ConnectionChangedEvent += OnConnectionChanged;
		_Strategy.DatabaseChangedEvent += OnDatabaseChanged;

		SetStateForConnection(DataConnection);

		bool open = DataConnectionState == ConnectionState.Open;

		EnQueryStatusFlags status = open
			? EnQueryStatusFlags.Connected : EnQueryStatusFlags.Connection;

		RaiseStatusChanged(status, open, true);

		RegisterSqlExecWithEvenHandlers();
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
					Cancel(true);
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
			_Strategy = null;
		}
	}



	void IBsQueryManager.DisposeTransaction(bool force) => Strategy?.DisposeTransaction(force);


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields & Constants - QueryManager
	// =========================================================================================================


	// A private 'this' object lock
	private readonly object _LockLocal = new object();

	private QESQLExec.GetCurrentWorkingDirectoryPath _CurrentWorkingDirectoryPath;
	private int _EventExecutingCardinal = 0;


	private TransientSettings _LiveSettings;
	private bool _LiveSettingsApplied;
	private long _RowsAffected;
	private int _StatementCount;
	private QESQLExec _SqlExec;
	private uint _Status;
	private ConnectionStrategy _Strategy;
	private TimeSpan _SyncCancelTimeout = new TimeSpan(0, 0, 30);


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


	public bool IsLocked => !EventLockEnter(true);


	public bool IsWithActualPlan => LiveSettings.WithActualPlan;

	public bool IsWithClientStats => LiveSettings.WithClientStats;


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

	public event EventHandler InvalidateToolbarEvent;
	public event NotifyConnectionStateEventHandler NotifyConnectionStateEvent;
	public event EventHandler ShowWindowFrameEvent;


	#endregion Property Accessors





	// =========================================================================================================
	#region Methods- QueryManager
	// =========================================================================================================


	/// <summary>
	/// <see cref="ErrorHandler.Succeeded"/> token.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
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

		if (_SqlExec.SyncCancel)
		{
			RaiseExecutionCompleted(EnScriptExecutionResult.Halted, true, executionType,
				LiveSettings.EditorResultsOutputMode, LiveSettings.WithClientStats);
			EventExecutingExit();
			return false;
		}

		SetStatusFlag(false, EnQueryStatusFlags.Executing);
		SetStatusFlag(true, EnQueryStatusFlags.Connecting);
		RaiseInvalidateToolbar();
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
			RaiseInvalidateToolbar();
			RaiseShowWindowFrame();

			MessageCtl.ShowEx(ex, Resources.ExDatabaseNotAccessible, null, MessageBoxButtons.OK, MessageBoxIcon.Hand);

			RaiseExecutionCompleted(EnScriptExecutionResult.Failure, _SqlExec.SyncCancel, executionType, LiveSettings.EditorResultsOutputMode, LiveSettings.WithClientStats);
			EventExecutingExit();
			return false;
		}

		SetStatusFlag(false, EnQueryStatusFlags.Connecting);

		if (connection == null || _SqlExec.SyncCancel)
		{
			RaiseExecutionCompleted(EnScriptExecutionResult.Cancel, _SqlExec.SyncCancel, executionType, LiveSettings.EditorResultsOutputMode, LiveSettings.WithClientStats);
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


		try
		{
			if (!await RaiseExecutionStartedAsync(textSpan.Text, executionType, connection))
			{
				RaiseExecutionCompleted(EnScriptExecutionResult.Halted, _SqlExec.SyncCancel, executionType, liveSettings.EditorResultsOutputMode, liveSettings.WithClientStats);
				EventExecutingExit();
				return false;
			}
		}
		catch (Exception ex)
		{
			RaiseExecutionCompleted(EnScriptExecutionResult.Failure, _SqlExec.SyncCancel, executionType, liveSettings.EditorResultsOutputMode, liveSettings.WithClientStats);
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
			RaiseExecutionCompleted(EnScriptExecutionResult.Failure, _SqlExec.SyncCancel, executionType, liveSettings.EditorResultsOutputMode, liveSettings.WithClientStats);
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

		// QueryExecutionCompletedEventHandler executionCompletedHandler = null;

		IsCancelling = true;

		try
		{
			_SqlExec.Cancel(synchronous, _SyncCancelTimeout);
		}
		finally
		{
			/*
			if (synchronous)
			{
				// IsExecuting = false;
				UnRegisterSqlExecWithEventHandlers();
				_SqlExec = new QESQLExec(this);
				RegisterSqlExecWithEvenHandlers();
				executionCompletedHandler = ExecutionCompletedEvent;
			}
			*/

			IsCancelling = false;

			/*
			if (executionCompletedHandler != null && synchronous)
			{
				executionCompletedHandler(this, new QueryExecutionCompletedEventArgs(EnScriptExecutionResult.Cancel, false));
			}
			*/
		}
	}


	void IBsQueryManager.CloseConnection() => Strategy?.CloseConnection();



	public bool CommitTransactions(bool validate)
	{
		if (!IsConnected || !EventLockEnter())
			return false;

		IsConnecting = true;
		bool result = false;

		try
		{
			result = Strategy.CommitTransactions(validate);
		}
		finally
		{
			if (validate)
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
			GetUpdatedTransactionsStatus(true);
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

		long stamp = Strategy.RctStamp;

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
					RaiseShowWindowFrame();
					MessageCtl.ShowEx(ex, Resources.ExDatabaseNotAccessible, null, MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
				finally
				{
					SetStatusFlag(false, EnQueryStatusFlags.Connecting, stamp != Strategy.RctStamp);
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



	public bool GetUpdatedTransactionsStatus(bool supressExceptions)
	{
		if (!EventLockEnter(false, true))
			return false;

		try
		{
			return Strategy.GetUpdatedTransactionsStatus(supressExceptions);
		}
		finally
		{
			EventLockExit();
		}
	}




	public bool ModifyConnection()
	{
		if (!EventLockEnter())
			return false;

		IsConnecting = true;

		long stamp = Strategy.ConnectionId;

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
			SetStatusFlag(false, EnQueryStatusFlags.Connecting, stamp != Strategy.RctStamp);
			EventLockExit();
		}

		return false;
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



	public bool RollbackTransactions(bool validate)
	{
		if (!IsConnected || !EventLockEnter())
			return false;

		IsConnecting = true;
		bool result = false;

		try
		{
			result = Strategy.RollbackTransactions(validate);
		}
		finally
		{
			if (validate)
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
		}

		RaiseStatusChanged(statusFlag, enable, newConnection);

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
	public bool EventExecutingEnter(bool test = false, bool force = false)
	{
		lock (_LockLocal)
		{
			if (!test && !force)
				Strategy.ResetKeepAliveTimeEpoch();

			if (_EventExecutingCardinal != 0 && !force)
				return false;

			if (!test)
			{
				_EventExecutingCardinal++;

				if (_EventExecutingCardinal == 1)
				{
					SetStatusFlag(true, EnQueryStatusFlags.Executing);
					RaiseInvalidateToolbar();
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
	public bool EventLockEnter(bool test = false, bool force = false)
	{
		lock (_LockLocal)
		{
			if (!test && !force)
				Strategy.ResetKeepAliveTimeEpoch();

			if (Strategy == null)
				return false;

			if (_EventExecutingCardinal != 0 && !force)
				return false;

			if (!test)
			{
				_EventExecutingCardinal++;

				if (!force && _EventExecutingCardinal == 1)
					RaiseInvalidateToolbar();
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
						RaiseNotifyConnectionState(EnNotifyConnectionState.NotifyBroken, Strategy?.HadTransactions ?? false);

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


	public bool OnNotifyConnectionState(object sender, EventArgs args)
	{
		NotifyConnectionStateEventArgs stateArgs = args as NotifyConnectionStateEventArgs;

		if (!IsConnected && stateArgs.State == EnNotifyConnectionState.ConfirmedOpen)
		{
			SetStatusFlag(true, EnQueryStatusFlags.Connected, false);
			IsConnecting = true;
			SetStatusFlag(false, EnQueryStatusFlags.Connecting, false);
			GetUpdatedTransactionsStatus(true);
			RaiseInvalidateToolbar();
		}
		else if (IsConnected && stateArgs.State == EnNotifyConnectionState.ConfirmedClosed)
		{
			IsConnected = false;
			RaiseInvalidateToolbar();
		}


		// else if (stateArgs.State == EnNotifyConnectionState.RequestIsUnlocked) is always honored.

		return !IsLocked;
	}




	// Added for StaticsPanel.RetrieveStatisticsIfNeeded();
	private void OnBatchStatementCompleted(object sender, BatchStatementCompletedEventArgs args) =>
		BatchStatementCompletedEvent?.Invoke(sender, args);





	private void RaiseExecutionCompleted(EnScriptExecutionResult executionResult, bool syncCancel,
			EnSqlExecutionType executionType, EnSqlOutputMode outputMode, bool withClientStats)
	{
		QueryExecutionEndTime = DateTime.Now;

		// Tracer.Trace(GetType(), "RaiseExecutionCompleted()");

		QueryExecutionCompletedEventArgs args = new(executionResult, syncCancel, executionType, outputMode, withClientStats);

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
		if (_SqlExec.SyncCancel)
			return false;

		await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

		_RowsAffected = 0L;
		_StatementCount = 0;

		QueryExecutionStartTime = DateTime.Now;
		QueryExecutionEndTime = DateTime.Now;


		QueryExecutionStartedEventArgs args = new(queryText, executionType, connection);

		return ExecutionStartedEvent?.Invoke(this, args) ?? true;
	}



	private void RaiseInvalidateToolbar() =>
		InvalidateToolbarEvent?.Invoke(this, new());



	private void RaiseNotifyConnectionState(EnNotifyConnectionState state, bool ttsDiscarded)
	{
		NotifyConnectionStateEvent?.Invoke(this, new NotifyConnectionStateEventArgs(state, ttsDiscarded));
	}



	public void RaiseShowWindowFrame()
	{
		if (_SqlExec.SyncCancel)
			return;

		ShowWindowFrameEvent?.Invoke(this, new());
	}



	private void RaiseStatusChanged(EnQueryStatusFlags statusFlag, bool enabled, bool newConnection) =>
		StatusChangedEvent?.Invoke(this, new(statusFlag, enabled, newConnection));


	#endregion Event Handling





	// =========================================================================================================
	#region Sub-Classes- QueryManager
	// =========================================================================================================


	#endregion Sub-Classes

}
