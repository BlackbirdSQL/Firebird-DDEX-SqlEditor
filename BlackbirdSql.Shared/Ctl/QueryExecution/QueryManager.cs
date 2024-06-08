// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.DataModel.QueryExecutor

using System;
using System.Data;
using System.Data.Common;
using BlackbirdSql.Core.Enums;
using BlackbirdSql.Shared.Ctl.Config;
using BlackbirdSql.Shared.Events;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Shared.Model;
using BlackbirdSql.Shared.Properties;
using BlackbirdSql.Sys.Enums;
using BlackbirdSql.Sys.Interfaces;
using Microsoft.VisualStudio.Utilities;



namespace BlackbirdSql.Shared.Ctl.QueryExecution;



public sealed class QueryManager : IBQueryManager
{

	public QueryManager(SqlConnectionStrategy connectionStrategy)
	{
		ConnectionStrategy = connectionStrategy;
		_SqlExec = new QEOLESQLExec(this);

		RegisterSqlExecWithEvenHandlers();
	}




	[Flags]
	public enum EnStatusType
	{
		Connected = 0x1,
		Executing = 0x2,
		Connection = 0x4,
		Connecting = 0x8,
		Cancelling = 0x10,
		ExecutionOptionsWithOleSqlChanged = 0x20,
		DatabaseChanged = 0x40
	}

	public delegate void StatusChangedEventHandler(object sender, StatusChangedEventArgs args);

	public class StatusChangedEventArgs(EnStatusType changeType) : EventArgs
	{
		public EnStatusType Change { get; private set; } = changeType;
	}

	public class ScriptExecutionStartedEventArgs : EventArgs
	{

		public ScriptExecutionStartedEventArgs(string queryText, EnSqlExecutionType executionType, IDbConnection connection)
		{
			QueryText = queryText;
			ExecutionType = executionType;
			Connection = connection;
		}

		public string QueryText { get; private set; }

		public EnSqlExecutionType ExecutionType { get; private set; }

		public IDbConnection Connection { get; private set; }
	}

	public delegate bool ScriptExecutionStartedEventHandler(object sender, ScriptExecutionStartedEventArgs args);

	private uint _Status;

	private long _RowsAffected;
	private long _TotalRowsAffected;



	// A private 'this' object lock
	private readonly object _LockLocal = new object();

	private QEOLESQLExec _SqlExec;

	private SqlConnectionStrategy _ConnectionStrategy;

	private TransientSettings _LiveSettings;

	private bool _LiveSettingsApplied;

	public const string C_TName = "QryMgr";

	public static TimeSpan SyncCancelTimeout = new TimeSpan(0, 0, 30);

	private QEOLESQLExec.GetCurrentWorkingDirectoryPath _CurrentWorkingDirectoryPath;

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

	public long ExecutionTimeout => _LiveSettings.EditorExecutionTimeout * 60000L;

	public bool HasTransactions => ConnectionStrategy != null && ConnectionStrategy.HasTransactions;

	public IDbTransaction Transaction => ConnectionStrategy?.Transaction;


	public IBQESQLBatchConsumer ResultsHandler { get; set; }

	public DateTime? QueryExecutionStartTime { get; set; }

	public DateTime? QueryExecutionEndTime { get; set; }

	public bool IsConnected
	{
		get
		{
			return GetStatusFlag(EnStatusType.Connected);
		}
		private set
		{
			SetStatusFlag(value, EnStatusType.Connected);
		}
	}

	public long RowsAffected
	{
		get
		{
			lock (_LockLocal)
			{
				return _RowsAffected;
			}
		}
	}

	public long TotalRowsAffected
	{
		get
		{
			lock (_LockLocal)
			{
				return _TotalRowsAffected;
			}
		}
	}



	public bool IsExecuting
	{
		get
		{
			return GetStatusFlag(EnStatusType.Executing);
		}
		private set
		{
			SetStatusFlag(value, EnStatusType.Executing);
		}
	}

	public bool IsConnecting
	{
		get
		{
			return GetStatusFlag(EnStatusType.Connecting);
		}
		set
		{
			SetStatusFlag(value, EnStatusType.Connecting);
		}
	}

	public bool IsCancelling
	{
		get
		{
			return GetStatusFlag(EnStatusType.Cancelling);
		}
		private set
		{
			SetStatusFlag(value, EnStatusType.Cancelling);
		}
	}

	public bool IsWithOleSQLScripting
	{
		get
		{
			lock (_LockLocal)
			{
				return LiveSettings.WithOleSqlScripting;
			}
		}
		set
		{
			lock (_LockLocal)
			{
				LiveSettings.WithOleSqlScripting = value;
				OnStatusChanged(new StatusChangedEventArgs(EnStatusType.ExecutionOptionsWithOleSqlChanged));
			}
		}
	}


	public bool IsWithActualPlan => LiveSettings.WithActualPlan;

	public bool IsWithClientStats => LiveSettings.WithClientStats;


	public QEOLESQLExec.GetCurrentWorkingDirectoryPath CurrentWorkingDirectoryPath
	{
		get
		{
			lock (_LockLocal)
			{
				return _CurrentWorkingDirectoryPath;
			}
		}
		set
		{
			lock (_LockLocal)
			{
				_CurrentWorkingDirectoryPath = value;
				_SqlExec.CurrentWorkingDirectoryPath = _CurrentWorkingDirectoryPath;
			}
		}
	}


	IDbConnection IBQueryManager.Connection => ConnectionStrategy.Connection;



	public SqlConnectionStrategy ConnectionStrategy
	{
		get
		{
			lock (_LockLocal)
			{
				return _ConnectionStrategy;
			}
		}
		set
		{
			lock (_LockLocal)
			{
				if (_ConnectionStrategy != null)
				{
					_ConnectionStrategy.Dispose();
					_ConnectionStrategy.ConnectionChanged -= ConnectionChanged;
					_ConnectionStrategy.DatabaseChanged -= HandleDatabaseChanged;
				}

				_ConnectionStrategy = value;
				_ConnectionStrategy.ConnectionChanged += ConnectionChanged;
				_ConnectionStrategy.DatabaseChanged += HandleDatabaseChanged;

				SetStateForConnection(ConnectionStrategy.Connection);
				OnStatusChanged(new StatusChangedEventArgs(EnStatusType.Connection));
			}
		}
	}


	public IsolationLevel TtsIsolationLevel => LiveSettings.EditorExecutionIsolationLevel;



	public bool LiveSettingsApplied
	{
		get
		{
			lock (_LockLocal)
			{
				return _LiveSettingsApplied;
			}
		}
		set
		{
			lock (_LockLocal)
			{
				_LiveSettingsApplied = value;
			}
		}
	}

	public event StatusChangedEventHandler StatusChangedEvent;

	public event ScriptExecutionStartedEventHandler ScriptExecutionStartedEvent;

	public event ScriptExecutionCompletedEventHandler ScriptExecutionCompletedEvent;

	public event QESQLBatchExecutedEventHandler BatchExecutionCompletedEvent;

	public event QEOLESQLErrorMessageEventHandler ScriptExecutionErrorMessageEvent;



	// Added for StaticsPanel.RetrieveStatisticsIfNeeded();
	public event QESQLQueryDataEventHandler DataLoadedEvent;

	public event QESQLStatementCompletedEventHandler StatementCompletedEvent;



	void IBQueryManager.BeginTransaction()
	{
		ConnectionStrategy?.BeginTransaction(LiveSettings.EditorExecutionIsolationLevel);
	}

	void IBQueryManager.CommitTransaction()
	{
		ConnectionStrategy?.CommitTransaction();
	}

	void IBQueryManager.RollbackTransaction()
	{
		ConnectionStrategy?.RollbackTransaction();
	}

	void IBQueryManager.DisposeTransaction(bool force)
	{
		ConnectionStrategy?.DisposeTransaction(force);
	}


	public bool AsyncExecute(IBTextSpan textSpan, EnSqlExecutionType executionType)
	{
		// Tracer.Trace(GetType(), "AsyncExecute()", " Enter. : ExecutionOptions.EstimatedPlanOnly: " + LiveSettings.EstimatedPlanOnly);

		ConnectionStrategy.EnsureConnection(true);
		IDbConnection connection = ConnectionStrategy.Connection;

		if (connection == null)
			return false;

		// Tracer.Trace(GetType(), "AsyncExecute()", "Ensured connection");

		if (connection.State != ConnectionState.Open)
		{
			DataException ex = new("Connection is not open");
			Diag.Dug(ex);
			return false;
		}

		// Tracer.Trace(GetType(), "AsyncExecute()", "execTimeout = {0}.", execTimeout);

		if (IsExecuting)
		{
			InvalidOperationException ex = new(ControlsResources.ExecutionNotCompleted);
			Diag.Dug(ex);
			throw ex;
		}

		if (!LiveSettingsApplied)
		{
			LiveSettingsApplied = true;
		}

		if (textSpan == null)
		{
			ArgumentNullException ex = new("textSpan");
			Diag.Dug(ex);
			throw ex;
		}

		LiveSettings.ExecutionType = executionType;


		TransientSettings liveSettings = CreateLiveSettingsObject();

		// Tracer.Trace(GetType(), "AsyncExecute()", "executionType: {0},  LiveSettings.ExecutionType: {1}, liveSettings.ExecutionType: {2}.",
		//	executionType,  LiveSettings.ExecutionType, liveSettings.ExecutionType);


		if (liveSettings.WithClientStats && liveSettings.ExecutionType != EnSqlExecutionType.PlanOnly)
		{
			ConnectionStrategy.ResetAndEnableConnectionStatistics();
		}


		if (!OnScriptExecutionStarted(textSpan.Text, executionType, connection))
		{
			// OperationCanceledException ex = new("OnScriptExecutionStarted returned false");
			// Diag.Dug(ex);
			return false;
		}



		IsExecuting = true;
		QueryExecutionStartTime = DateTime.Now;
		QueryExecutionEndTime = DateTime.Now;


		// -------------------------------------------------------------------------------- //
		// ******************** Execution Point (2) - QueryManager.AsyncExecute() ******************** //
		// --------------------------------------------------------------------------------

		_SqlExec.AsyncExecuteQuery(textSpan, executionType, connection, ResultsHandler, liveSettings);

		return true;
	}



	public void Cancel(bool synchronous)
	{
		ScriptExecutionCompletedEventHandler scriptExecutionCompletedHandler = null;
		try
		{
			lock (_LockLocal)
			{
				IsCancelling = true;

				try
				{
					_SqlExec.Cancel(SyncCancelTimeout);
				}
				finally
				{
					if (synchronous)
					{
						IsExecuting = false;
						UnRegisterSqlExecWithEventHandlers();
						_SqlExec = new QEOLESQLExec(this);
						RegisterSqlExecWithEvenHandlers();
						scriptExecutionCompletedHandler = ScriptExecutionCompletedEvent;
					}
				}
			}
		}
		finally
		{
			IsCancelling = false;
			if (scriptExecutionCompletedHandler != null && synchronous)
			{
				scriptExecutionCompletedHandler(this, new ScriptExecutionCompletedEventArgs(EnScriptExecutionResult.Cancel));
			}
		}
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
			ScriptExecutionStartedEvent = null;
			ScriptExecutionCompletedEvent = null;
			StatementCompletedEvent = null;
			BatchExecutionCompletedEvent = null;
			ScriptExecutionErrorMessageEvent = null;
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

			ConnectionStrategy?.Dispose();
		}
	}



	private void ConnectionStateChangedEventHandler(object sender, StateChangeEventArgs args)
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
					case ConnectionState.Closed:
					case ConnectionState.Broken:
						IsConnected = false;
						LiveSettingsApplied = false;

						if (IsExecuting)
							IsExecuting = false;

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

	private void RegisterSqlExecWithEvenHandlers()
	{
		lock (_LockLocal)
		{
			_SqlExec.ExecutionCompletedEvent += OnExecutionCompleted;
			_SqlExec.BatchExecutionCompletedEvent += OnBatchExecutionCompleted;
			_SqlExec.DataLoadedEvent += OnDataLoaded;
			_SqlExec.StartingBatchExecutionEvent += OnStartingBatchExecution;
			_SqlExec.StatementCompletedEvent += OnStatementCompleted;
			_SqlExec.SqlErrorMessageEvent += OnScriptingErrorMessage;
		}
	}

	private void UnRegisterSqlExecWithEventHandlers()
	{
		lock (_LockLocal)
		{
			_SqlExec.ExecutionCompletedEvent -= OnExecutionCompleted;
			_SqlExec.BatchExecutionCompletedEvent -= OnBatchExecutionCompleted;
			_SqlExec.DataLoadedEvent -= OnDataLoaded;
			_SqlExec.StartingBatchExecutionEvent -= OnStartingBatchExecution;
			_SqlExec.StatementCompletedEvent -= OnStatementCompleted;
			_SqlExec.SqlErrorMessageEvent -= OnScriptingErrorMessage;
		}
	}

	private void OnBatchExecutionCompleted(object sender, QESQLBatchExecutedEventArgs args)
	{
		// Tracer.Trace(GetType(), "QryMgr.OnBatchExecutionCompleted", "", null);
		_RowsAffected += args.Batch.RowsAffected;
		_TotalRowsAffected += args.Batch.TotalRowsAffected;
		BatchExecutionCompletedEvent?.Invoke(sender, args);
	}

	private void OnStartingBatchExecution(object sender, QESQLStartingBatchEventArgs args)
	{
		// Tracer.Trace(GetType(), "QryMgr.OnStartingBatchExecution", "", null);
	}


	private void OnScriptingErrorMessage(object sender, QEOLESQLErrorMessageEventArgs args)
	{
		ScriptExecutionErrorMessageEvent?.Invoke(sender, args);
	}



	private TransientSettings CreateLiveSettingsObject()
	{
		return (TransientSettings)LiveSettings.Clone();

	}

	private void OnExecutionCompleted(object sender, ScriptExecutionCompletedEventArgs args)
	{
		QueryExecutionEndTime = DateTime.Now;

		// Tracer.Trace(GetType(), "OnExecutionCompleted()", "Calling ScriptExecutionCompletedEvent. args.ExecResult: {0}.", args.ExecutionResult);

		try
		{
			ScriptExecutionCompletedEvent?.Invoke(this, args);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;
		}

		IsExecuting = false;
		IsCancelling = false;

		if (LiveSettings.EditorExecutionDisconnectOnCompletion)
			ConnectionStrategy.Connection.Close();
	}

	private bool OnScriptExecutionStarted(string text, EnSqlExecutionType executionType, IDbConnection connection)
	{
		_RowsAffected = 0L;
		_TotalRowsAffected = 0L;
		if (ScriptExecutionStartedEvent != null)
		{
			return ScriptExecutionStartedEvent(this, new(text, executionType, connection));
		}

		return true;
	}


	// Added for StaticsPanel.RetrieveStatisticsIfNeeded();
	private void OnStatementCompleted(object sender, QESQLStatementCompletedEventArgs args)
	{
		StatementCompletedEvent?.Invoke(sender, args);
	}

	private void OnDataLoaded(object sender, QESQLQueryDataEventArgs eventArgs)
	{
		DataLoadedEvent?.Invoke(sender, eventArgs);
	}


	private void OnStatusChanged(StatusChangedEventArgs args)
	{
		StatusChangedEvent?.Invoke(this, args);
	}

	private bool GetStatusFlag(EnStatusType statusType)
	{
		lock (_LockLocal)
		{
			return (_Status & (uint)statusType) == (uint)statusType;
		}
	}

	private void SetStatusFlag(bool enabled, EnStatusType statusType)
	{
		lock (_LockLocal)
		{
			if (!enabled)
			{
				if ((_Status & (uint)statusType) == (uint)statusType)
				{
					uint num = (uint)~statusType;
					_Status &= num;

					OnStatusChanged(new StatusChangedEventArgs(statusType));
				}
			}
			else if ((_Status & (uint)statusType) != (uint)statusType)
			{
				_Status |= (uint)statusType;

				OnStatusChanged(new StatusChangedEventArgs(statusType));
			}
		}
	}

	private void HandleDatabaseChanged(object sender, EventArgs args)
	{
		OnStatusChanged(new StatusChangedEventArgs(EnStatusType.DatabaseChanged));
	}

	private void ConnectionChanged(object sender, AbstractConnectionStrategy.ConnectionChangedEventArgs args)
	{
		lock (_LockLocal)
		{
			_RowsAffected = 0L;
			_TotalRowsAffected = 0L;
			QueryExecutionStartTime = null;
			QueryExecutionEndTime = null;
			LiveSettingsApplied = false;
			IDbConnection previousConnection = args.PreviousConnection;
			IDbConnection connection = ConnectionStrategy.Connection;
			LiveSettings.EditorExecutionTimeout = ConnectionStrategy.GetExecutionTimeout();
			if (previousConnection != null)
			{
				if (previousConnection is DbConnection dbConnection)
				{
					dbConnection.StateChange -= ConnectionStateChangedEventHandler;
				}
			}

			SetStateForConnection(connection);
			OnStatusChanged(new StatusChangedEventArgs(EnStatusType.Connection));
		}
	}

	private void SetStateForConnection(IDbConnection newConnection)
	{
		if (newConnection != null)
		{
			if (newConnection is DbConnection dbConnection)
			{
				dbConnection.StateChange += ConnectionStateChangedEventHandler;
			}

			IsExecuting = newConnection.State == ConnectionState.Executing;
			IsConnected = newConnection.State == ConnectionState.Open;
			IsConnecting = newConnection.State == ConnectionState.Connecting;
			IsCancelling = false;
		}
		else
		{
			IsExecuting = false;
			IsConnected = false;
			IsConnecting = false;
			IsCancelling = false;
		}
	}
}
