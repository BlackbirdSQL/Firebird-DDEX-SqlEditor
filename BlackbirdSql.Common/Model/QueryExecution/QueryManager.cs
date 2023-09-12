// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.DataModel.QueryExecutor

using System;
using System.Data;
using System.Data.Common;

using BlackbirdSql.Common.Config;
using BlackbirdSql.Common.Interfaces;
using BlackbirdSql.Common.Model.Enums;
using BlackbirdSql.Common.Model.Events;
using BlackbirdSql.Common.Model.Interfaces;
using BlackbirdSql.Common.Properties;
using BlackbirdSql.Core;
using FirebirdSql.Data.FirebirdClient;
using Microsoft.VisualStudio.Utilities;

using Tracer = BlackbirdSql.Core.Diagnostics.Tracer;


namespace BlackbirdSql.Common.Model.QueryExecution;

public sealed class QueryManager : IDisposable
{
	[Flags]
	public enum StatusType
	{
		Connected = 0x1,
		Executing = 0x2,
		Debugging = 0x4,
		Connection = 0x8,
		Connecting = 0x10,
		Cancelling = 0x20,
		ExecutionOptionsWithOleSqlChanged = 0x40,
		DatabaseChanged = 0x80
	}

	public delegate void StatusChangedEventHandler(object sender, StatusChangedEventArgs args);

	public class StatusChangedEventArgs : EventArgs
	{
		public StatusType Change { get; private set; }

		public StatusChangedEventArgs(StatusType changeType)
		{
			Change = changeType;
		}
	}

	public class ScriptExecutionStartedEventArgs : EventArgs
	{
		public string QueryText { get; private set; }

		public bool IsParseOnly { get; private set; }

		public IDbConnection Connection { get; private set; }

		public ScriptExecutionStartedEventArgs(string queryText, bool isParseOnly, IDbConnection connection)
		{
			QueryText = queryText;
			IsParseOnly = isParseOnly;
			Connection = connection;
		}
	}

	public delegate bool ScriptExecutionStartedEventHandler(object sender, ScriptExecutionStartedEventArgs args);

	private uint _Status;

	private long _RowsAffected;

	private readonly object _LockObject = new object();

	private QEOLESQLExec _SqlExec;

	private SqlConnectionStrategy _ConnectionStrategy;

	private IUserSettings _QueryExecutionSettings;

	private bool _QueryExecutionSettingsApplied;

	public const string C_TName = "QryMgr";

	public static TimeSpan SyncCancelTimeout = new TimeSpan(0, 0, 5);

	private QEOLESQLExec.ResolveSqlCmdVariable _SqlCmdVariableResolver;

	private QEOLESQLExec.GetCurrentWorkingDirectoryPath _currentWorkingDirectoryPath;

	public QESQLExecutionOptions ExecutionOptions { get; private set; }

	public IQESQLBatchConsumer ResultsHandler { get; set; }

	public DateTime? QueryExecutionStartTime { get; private set; }

	public DateTime? QueryExecutionEndTime { get; private set; }

	public bool IsConnected
	{
		get
		{
			return IsStatusFlagSet(StatusType.Connected);
		}
		private set
		{
			SetStatusFlag(value, StatusType.Connected);
		}
	}

	public long RowsAffected
	{
		get
		{
			lock (_LockObject)
			{
				return _RowsAffected;
			}
		}
	}

	public bool IsDebugging
	{
		get
		{
			return IsStatusFlagSet(StatusType.Debugging);
		}
		private set
		{
			SetStatusFlag(value, StatusType.Debugging);
		}
	}

	public bool IsAllowedToExecuteWhileDebugging { get; set; }

	public bool IsExecuting
	{
		get
		{
			return IsStatusFlagSet(StatusType.Executing);
		}
		private set
		{
			SetStatusFlag(value, StatusType.Executing);
		}
	}

	public bool IsConnecting
	{
		get
		{
			return IsStatusFlagSet(StatusType.Connecting);
		}
		set
		{
			SetStatusFlag(value, StatusType.Connecting);
		}
	}

	public bool IsCancelling
	{
		get
		{
			return IsStatusFlagSet(StatusType.Cancelling);
		}
		private set
		{
			SetStatusFlag(value, StatusType.Cancelling);
		}
	}

	public bool IsWithOleSQLScripting
	{
		get
		{
			lock (_LockObject)
			{
				return ExecutionOptions.WithOleSqlScripting;
			}
		}
		set
		{
			lock (_LockObject)
			{
				ExecutionOptions.WithOleSqlScripting = value;
				OnStatusChanged(new StatusChangedEventArgs(StatusType.ExecutionOptionsWithOleSqlChanged));
			}
		}
	}

	public QEOLESQLExec.ResolveSqlCmdVariable SqlCmdVariableResolver
	{
		get
		{
			lock (_LockObject)
			{
				return _SqlCmdVariableResolver;
			}
		}
		set
		{
			lock (_LockObject)
			{
				_SqlCmdVariableResolver = value;
				_SqlExec.SqlCmdVariableResolver = _SqlCmdVariableResolver;
			}
		}
	}

	public QEOLESQLExec.GetCurrentWorkingDirectoryPath CurrentWorkingDirectoryPath
	{
		get
		{
			lock (_LockObject)
			{
				return _currentWorkingDirectoryPath;
			}
		}
		set
		{
			lock (_LockObject)
			{
				_currentWorkingDirectoryPath = value;
				_SqlExec.CurrentWorkingDirectoryPath = _currentWorkingDirectoryPath;
			}
		}
	}

	public SqlConnectionStrategy ConnectionStrategy
	{
		get
		{
			lock (_LockObject)
			{
				return _ConnectionStrategy;
			}
		}
		set
		{
			lock (_LockObject)
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
				OnStatusChanged(new StatusChangedEventArgs(StatusType.Connection));
			}
		}
	}

	public IUserSettings QueryExecutionSettings
	{
		get
		{
			lock (_LockObject)
			{
				if (_QueryExecutionSettings == null)
				{
					IUserSettings current = UserSettings.Instance.Current;
					_QueryExecutionSettings = current.Clone() as IUserSettings;
				}

				return _QueryExecutionSettings;
			}
		}
		set
		{
			lock (_LockObject)
			{
				_QueryExecutionSettings = value;
			}
		}
	}

	public bool QueryExecutionSettingsApplied
	{
		get
		{
			lock (_LockObject)
			{
				return _QueryExecutionSettingsApplied;
			}
		}
		set
		{
			lock (_LockObject)
			{
				_QueryExecutionSettingsApplied = value;
			}
		}
	}

	public event StatusChangedEventHandler StatusChangedEvent;

	public event ScriptExecutionStartedEventHandler ScriptExecutionStartedEvent;

	public event ScriptExecutionCompletedEventHandler ScriptExecutionCompletedEvent;

	public event QESQLBatchExecutedEventHandler BatchExecutionCompletedEvent;

	public event QEOLESQLErrorMessageEventHandler ScriptExecutionErrorMessageEvent;

	public event QEOLESQLOutputRedirectionEventHandler SqlCmdOutputRedirectionEvent;

	public event QeSqlCmdMessageFromAppEventHandler SqlCmdMessageFromAppEvent;

	// Added for StaticsPanel.RetrieveStatisticsIfNeeded();
	public event QESQLDataLoadedEventHandler DataLoadedEvent;

	public event QESQLStatementCompletedEventHandler StatementCompletedEvent;


	public QueryManager(SqlConnectionStrategy connectionStrategy, QEOLESQLExec.ResolveSqlCmdVariable sqlCmdVarResolver)
	{
		ConnectionStrategy = connectionStrategy;
		_SqlExec = new QEOLESQLExec(SqlCmdVariableResolver, this);
		SqlCmdVariableResolver = sqlCmdVarResolver;
		RegisterSqlExecWithEvenHandlers();
		ExecutionOptions = new QESQLExecutionOptions();
		IsAllowedToExecuteWhileDebugging = true;
	}

	public bool Run(ITextSpan textSpan)
	{
		return ValidateAndRun(textSpan, 0, bParseOnly: false, withDebugging: false, useCustomExecutionTimeout: false);
	}

	public bool Run(ITextSpan textSpan, int execTimeout, bool withDebugging)
	{
		return ValidateAndRun(textSpan, execTimeout, bParseOnly: false, withDebugging, useCustomExecutionTimeout: true);
	}

	public void Parse(ITextSpan textSpan)
	{
		ValidateAndRun(textSpan, 0, bParseOnly: true, withDebugging: false, useCustomExecutionTimeout: false);
	}

	public void Cancel(bool bSync)
	{
		ScriptExecutionCompletedEventHandler scriptExecutionCompletedHandler = null;
		try
		{
			lock (_LockObject)
			{
				IsCancelling = true;
				if (IsDebugging)
				{
					// EditorDebugContext.DetachDebugger(stopExecution: true);
					return;
				}

				Tracer.Trace(GetType(), "QryMgr.Cancel", "bSync = {0}", bSync);
				Tracer.Trace(GetType(), "QryMgr.Cancel", "initiating Cancel");
				try
				{
					Tracer.Trace(GetType(), "QryMgr.Cancel", "Issuing cancel request");
					_SqlExec.Cancel(bSync, SyncCancelTimeout);
				}
				finally
				{
					if (bSync)
					{
						IsExecuting = false;
						UnRegisterSqlExecWithEventHandlers();
						_SqlExec = new QEOLESQLExec(SqlCmdVariableResolver, this);
						RegisterSqlExecWithEvenHandlers();
						scriptExecutionCompletedHandler = ScriptExecutionCompletedEvent;
					}

					Tracer.Trace(GetType(), "QryMgr.Cancel", "cancel completed");
				}
			}
		}
		finally
		{
			IsCancelling = false;
			if (scriptExecutionCompletedHandler != null && bSync)
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
		lock (_LockObject)
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
					Cancel(bSync: true);
				}
			}
			catch (Exception ex)
			{
				Tracer.Trace(GetType(), "QryMgr.Dispose", ex.ToString());
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
		lock (_LockObject)
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
						QueryExecutionSettingsApplied = false;
						if (IsExecuting)
						{
							IsExecuting = false;
						}

						if (IsDebugging)
						{
							IsDebugging = false;
						}

						break;
					case ConnectionState.Open:
						if (originalState == ConnectionState.Closed || originalState == ConnectionState.Connecting)
						{
							IsConnected = true;
							QueryExecutionSettingsApplied = false;
						}

						break;
				}
			}
		}
	}

	private void RegisterSqlExecWithEvenHandlers()
	{
		lock (_LockObject)
		{
			_SqlExec.ExecutionCompletedEvent += OnExecutionCompleted;
			_SqlExec.BatchExecutionCompletedEvent += OnBatchExecutionCompleted;
			_SqlExec.DataLoadedEvent += OnDataLoaded;
			_SqlExec.StartingBatchExecutionEvent += OnStartingBatchExecution;
			_SqlExec.StatementCompletedEvent += OnStatementCompleted;
			_SqlExec.SqlOutputRedirectionEvent += OnOutputRedirection;
			_SqlExec.SqlErrorMessageEvent += OnScriptingErrorMessage;
			_SqlExec.SqlCmdMessageFromAppEvent += OnSqlCmdMsgFromApp;
			_SqlExec.SqlCmdNewConnectionOpenedEvent += OnSqlCmdConnect;
		}
	}

	private void UnRegisterSqlExecWithEventHandlers()
	{
		lock (_LockObject)
		{
			_SqlExec.ExecutionCompletedEvent -= OnExecutionCompleted;
			_SqlExec.BatchExecutionCompletedEvent -= OnBatchExecutionCompleted;
			_SqlExec.DataLoadedEvent -= OnDataLoaded;
			_SqlExec.StartingBatchExecutionEvent -= OnStartingBatchExecution;
			_SqlExec.StatementCompletedEvent -= OnStatementCompleted;
			_SqlExec.SqlOutputRedirectionEvent -= OnOutputRedirection;
			_SqlExec.SqlErrorMessageEvent -= OnScriptingErrorMessage;
			_SqlExec.SqlCmdMessageFromAppEvent -= OnSqlCmdMsgFromApp;
			_SqlExec.SqlCmdNewConnectionOpenedEvent -= OnSqlCmdConnect;
		}
	}

	private void OnBatchExecutionCompleted(object sender, QESQLBatchExecutedEventArgs args)
	{
		Tracer.Trace(GetType(), "QryMgr.OnBatchExecutionCompleted", "", null);
		_RowsAffected += args.Batch.RowsAffected;
		BatchExecutionCompletedEvent?.Invoke(sender, args);
	}

	private void OnStartingBatchExecution(object sender, QESQLStartingBatchEventArgs args)
	{
		Tracer.Trace(GetType(), "QryMgr.OnStartingBatchExecution", "", null);
	}

	private void OnOutputRedirection(object sender, QEOLESQLOutputRedirectionEventArgs args)
	{
		SqlCmdOutputRedirectionEvent?.Invoke(sender, args);
	}

	private void OnScriptingErrorMessage(object sender, QEOLESQLErrorMessageEventArgs args)
	{
		ScriptExecutionErrorMessageEvent?.Invoke(sender, args);
	}

	private void OnSqlCmdMsgFromApp(object sender, QeSqlCmdMessageFromAppEventArgs args)
	{
		SqlCmdMessageFromAppEvent?.Invoke(sender, args);
	}

	private void OnSqlCmdConnect(object sender, QeSqlCmdNewConnectionOpenedEventArgs args)
	{
		ConnectionStrategy.ApplyConnectionOptions(args.Connection, QueryExecutionSettings.Execution);
	}

	private bool ValidateAndRun(ITextSpan textSpan, int execTimeout, bool bParseOnly, bool withDebugging, bool useCustomExecutionTimeout)
	{
		Tracer.Trace(GetType(), Tracer.Level.Verbose, "ValidateAndRun", " Enter. : ExecutionOptions.WithEstimatedExecutionPlan: " + ExecutionOptions.WithEstimatedExecutionPlan);
		
		ConnectionStrategy.EnsureConnection(tryOpenConnection: true);
		Tracer.Trace(GetType(), Tracer.Level.Verbose, "ValidateAndRun", "Ensured connection");
		IDbConnection connection = ConnectionStrategy.Connection;

		if (connection == null)
		{
			DataException ex = new("Connection is null");
			Diag.Dug(ex);
			return false;
		}

		if (connection.State != ConnectionState.Open)
		{
			DataException ex = new("Connection is not open");
			Diag.Dug(ex);
			return false;
		}

		Tracer.Trace(GetType(), "QryMgr.ValidateAndRun", "execTimeout = {0}, bParseOnly = {1}", execTimeout, bParseOnly);
		if (IsExecuting)
		{
			InvalidOperationException ex = new(ControlsResources.ExecutionNotCompleted);
			Diag.Dug(ex);
			throw ex;
		}

		if (!QueryExecutionSettingsApplied)
		{
			ConnectionStrategy.ApplyConnectionOptions(connection, QueryExecutionSettings.Execution);
			QueryExecutionSettingsApplied = true;
		}

		if (execTimeout < 0)
		{
			ArgumentOutOfRangeException ex = new("execTimeout");
			Diag.Dug(ex);
			throw ex;
		}

		if (textSpan == null)
		{
			ArgumentNullException ex = new("textSpan");
			Diag.Dug(ex);
			throw ex;
		}

		QESQLExecutionOptions qESQLExecutionOptions = CreateExecutionOptionsObject();
		if (bParseOnly)
		{
			qESQLExecutionOptions.ParseOnly = true;
			qESQLExecutionOptions.SuppressProviderMessageHeaders = true;
			qESQLExecutionOptions.WithOleSqlScripting = ExecutionOptions.WithOleSqlScripting;
		}
		else if (ExecutionOptions.WithClientStats)
		{
			ConnectionStrategy.ResetAndEnableConnectionStatistics();
		}

		if (!OnScriptExecutionStarted(textSpan.Text, bParseOnly, connection))
		{
			OperationCanceledException ex = new("OnScriptExecutionStarted return false");
			Diag.Dug(ex);
			return false;
		}



		IsExecuting = true;
		QueryExecutionStartTime = DateTime.Now;
		QueryExecutionEndTime = DateTime.Now;
		if (withDebugging)
		{
			IsDebugging = true;
			qESQLExecutionOptions.WithDebugging = true;
		}
		else
		{
			IsDebugging = false;
			qESQLExecutionOptions.WithDebugging = false;
		}

		if (!useCustomExecutionTimeout)
		{
			execTimeout = qESQLExecutionOptions.ExecutionTimeout;
		}

		_SqlExec.Execute(textSpan, connection, execTimeout, ResultsHandler, qESQLExecutionOptions);

		return true;
	}

	private QESQLExecutionOptions CreateExecutionOptionsObject()
	{
		return new QESQLExecutionOptions(ExecutionOptions)
		{
			ParseOnly = QueryExecutionSettings.Execution.SetParseOnly,
			WithNoExec = QueryExecutionSettings.Execution.SetNoExec,
			WithExecutionPlanText = QueryExecutionSettings.Execution.SetShowplanText,
			WithStatisticsProfile = QueryExecutionSettings.Execution.SetStatisticsProfile,
			WithStatisticsIO = QueryExecutionSettings.Execution.SetStatisticsIO,
			WithStatisticsTime = QueryExecutionSettings.Execution.SetStatisticsTime,
			BatchSeparator = QueryExecutionSettings.Execution.BatchSeparator,
			ExecutionTimeout = QueryExecutionSettings.Execution.ExecutionTimeout,
			SuppressProviderMessageHeaders = QueryExecutionSettings.Execution.SuppressProviderMessageHeaders
		};
	}

	private void OnExecutionCompleted(object sender, ScriptExecutionCompletedEventArgs args)
	{
		QueryExecutionEndTime = DateTime.Now;
		ScriptExecutionCompletedEvent?.Invoke(this, args);

		IsExecuting = false;
		IsDebugging = false;
		IsCancelling = false;
		if (QueryExecutionSettings.Execution.DisconnectAfterQueryExecutes)
		{
			ConnectionStrategy.Connection.Close();
			ConnectionStrategy.SetConnectionInfo(null);
		}
	}

	private bool OnScriptExecutionStarted(string text, bool isParseOnly, IDbConnection connection)
	{
		_RowsAffected = 0L;
		if (ScriptExecutionStartedEvent != null)
		{
			return ScriptExecutionStartedEvent(this, new(text, isParseOnly, connection));
		}

		return true;
	}


	// Added for StaticsPanel.RetrieveStatisticsIfNeeded();
	private void OnStatementCompleted(object sender, QESQLStatementCompletedEventArgs args)
	{
		StatementCompletedEvent?.Invoke(sender, args);
	}

	private void OnDataLoaded(object sender, QESQLDataLoadedEventArgs args)
	{
		DataLoadedEvent?.Invoke(sender, args);
	}


	private void OnStatusChanged(StatusChangedEventArgs args)
	{
		StatusChangedEvent?.Invoke(this, args);
	}

	private bool IsStatusFlagSet(StatusType statusType)
	{
		lock (_LockObject)
		{
			return (_Status & (uint)statusType) == (uint)statusType;
		}
	}

	private void SetStatusFlag(bool enabled, StatusType statusType)
	{
		lock (_LockObject)
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
		OnStatusChanged(new StatusChangedEventArgs(StatusType.DatabaseChanged));
	}

	private void ConnectionChanged(object sender, ConnectionStrategy.ConnectionChangedEventArgs args)
	{
		lock (_LockObject)
		{
			_RowsAffected = 0L;
			QueryExecutionStartTime = null;
			QueryExecutionEndTime = null;
			QueryExecutionSettingsApplied = false;
			IDbConnection previousConnection = args.PreviousConnection;
			IDbConnection connection = ConnectionStrategy.Connection;
			QueryExecutionSettings.Execution.ExecutionTimeout = ConnectionStrategy.GetExecutionTimeout();
			if (previousConnection != null)
			{
				if (previousConnection is DbConnection dbConnection)
				{
					dbConnection.StateChange -= ConnectionStateChangedEventHandler;
				}
			}

			SetStateForConnection(connection);
			OnStatusChanged(new StatusChangedEventArgs(StatusType.Connection));
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
			IsDebugging = false;
			IsCancelling = false;
		}
		else
		{
			IsExecuting = false;
			IsDebugging = false;
			IsConnected = false;
			IsConnecting = false;
			IsCancelling = false;
		}
	}
}
