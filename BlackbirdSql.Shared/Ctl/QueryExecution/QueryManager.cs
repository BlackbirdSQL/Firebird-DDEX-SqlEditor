// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.DataModel.QueryExecutor

using System;
using System.Data;
using System.Data.Common;
using System.Windows.Forms;
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


// =========================================================================================================
//										QueryManager Class
//
/// <summary>
/// Manages query execution of an active document.
/// </summary>
// =========================================================================================================
public sealed class QueryManager : IBQueryManager
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors- QueryManager
	// ---------------------------------------------------------------------------------


	/// <summary>
	/// Default .ctor.
	/// </summary>
	public QueryManager(AuxilliaryDocData owner, ConnectionStrategy connectionStrategy)
	{
		_Owner = owner;

		_Strategy = connectionStrategy;
		_SqlExec = new QESQLExec(this);

		_Strategy.ConnectionChanged += OnConnectionChanged;
		_Strategy.DatabaseChanged += OnDatabaseChanged;

		SetStateForConnection(_Strategy.Connection);
		OnStatusChanged(new StatusChangedEventArgs(EnStatusType.Connection));
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

			Strategy?.Dispose();
		}
	}



	void IBQueryManager.DisposeTransaction(bool force)
	{
		Strategy?.DisposeTransaction(force);
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields- QueryManager
	// =========================================================================================================


	// A private 'this' object lock
	private readonly object _LockLocal = new object();

	private QESQLExec.GetCurrentWorkingDirectoryPath _CurrentWorkingDirectoryPath;
	private TransientSettings _LiveSettings;
	private bool _LiveSettingsApplied;
	private readonly AuxilliaryDocData _Owner = null;
	private long _RowsAffected;
	private QESQLExec _SqlExec;
	private uint _Status;
	private ConnectionStrategy _Strategy;
	private TimeSpan _SyncCancelTimeout = new TimeSpan(0, 0, 30);


	#endregion Fields





	// =========================================================================================================
	#region Property Accessors- QueryManager
	// =========================================================================================================


	IDbConnection IBQueryManager.Connection => Strategy.Connection;


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
		get { return GetStatusFlag(EnStatusType.Cancelling); }
		set { SetStatusFlag(value, EnStatusType.Cancelling); }
	}


	public bool IsConnected
	{
		get { return GetStatusFlag(EnStatusType.Connected); }
		private set { SetStatusFlag(value, EnStatusType.Connected); }
	}


	public bool IsConnecting
	{
		get { return GetStatusFlag(EnStatusType.Connecting); }
		set { SetStatusFlag(value, EnStatusType.Connecting); }
	}


	public bool IsExecuting
	{
		get { return GetStatusFlag(EnStatusType.Executing); }
		private set { SetStatusFlag(value, EnStatusType.Executing); }
	}


	public bool IsWithActualPlan => LiveSettings.WithActualPlan;

	public bool IsWithClientStats => LiveSettings.WithClientStats;


	public bool IsWithOleSQLScripting
	{
		get
		{
			lock (_LockLocal)
				return LiveSettings.WithOleSqlScripting;
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

	public IBQESQLBatchConsumer ResultsHandler { get; set; }


	public long RowsAffected
	{
		get
		{
			lock (_LockLocal)
				return _RowsAffected;
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
				if (_Strategy != null)
				{
					_Strategy.Dispose();
					_Strategy.ConnectionChanged -= OnConnectionChanged;
					_Strategy.DatabaseChanged -= OnDatabaseChanged;
				}

				_Strategy = value;
				_Strategy.ConnectionChanged += OnConnectionChanged;
				_Strategy.DatabaseChanged += OnDatabaseChanged;

				SetStateForConnection(Strategy.Connection);
				OnStatusChanged(new StatusChangedEventArgs(EnStatusType.Connection));
			}
		}
	}


	public IDbTransaction Transaction => Strategy?.Transaction;

	public IsolationLevel TtsIsolationLevel => LiveSettings.EditorExecutionIsolationLevel;




	public delegate bool ScriptExecutionStartedEventHandler(object sender, ScriptExecutionStartedEventArgs args);

	public event StatusChangedEventHandler StatusChangedEvent;
	public event ScriptExecutionStartedEventHandler ScriptExecutionStartedEvent;
	public event ScriptExecutionCompletedEventHandler ScriptExecutionCompletedEvent;
	public event QESQLBatchExecutionCompletedEventHandler BatchExecutionCompletedEvent;
	public event QEOLESQLErrorMessageEventHandler ScriptExecutionErrorMessageEvent;
	public event QESQLStatementCompletedEventHandler StatementCompletedEvent;

	// Added for StaticsPanel.RetrieveStatisticsIfNeeded();
	public event QESQLQueryDataEventHandler DataLoadedEvent;


	#endregion Property Accessors





	// =========================================================================================================
	#region Methods- QueryManager
	// =========================================================================================================


	public bool AsyncExecute(IBTextSpan textSpan, EnSqlExecutionType executionType)
	{
		// Tracer.Trace(GetType(), "AsyncExecute()", " Enter. : ExecutionOptions.EstimatedPlanOnly: " + LiveSettings.EstimatedPlanOnly);

		IDbConnection connection = Strategy.EnsureConnection(true);

		if (connection == null)
			return false;

		// Tracer.Trace(GetType(), "AsyncExecute()", "Ensured connection");

		if (connection.State != ConnectionState.Open)
		{
			GetUpdateTransactionsStatus();

			DataException ex = new("Connection is not open");
#if DEBUG
			Diag.Dug(ex);
#endif
			MessageCtl.ShowEx(ex, ControlsResources.ErrDatabaseNotAccessible, null, MessageBoxButtons.OK, MessageBoxIcon.Hand);
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


		TransientSettings liveSettings = (TransientSettings)LiveSettings.Clone();

		// Tracer.Trace(GetType(), "AsyncExecute()", "executionType: {0},  LiveSettings.ExecutionType: {1}, liveSettings.ExecutionType: {2}.",
		//	executionType,  LiveSettings.ExecutionType, liveSettings.ExecutionType);


		if (liveSettings.WithClientStats && liveSettings.ExecutionType != EnSqlExecutionType.PlanOnly)
		{
			Strategy.ResetAndEnableConnectionStatistics();
		}

		if (!InitializeScriptExecution(textSpan.Text, executionType, connection))
		{
			GetUpdateTransactionsStatus();

			return false;
		}



		IsExecuting = true;
		QueryExecutionStartTime = DateTime.Now;
		QueryExecutionEndTime = DateTime.Now;


		// -------------------------------------------------------------------------------- //
		// ******************** Execution Point (2) - QueryManager.AsyncExecute() ******************** //
		// --------------------------------------------------------------------------------

		try
		{
			_SqlExec.AsyncExecute(textSpan, executionType, connection, ResultsHandler, liveSettings);
		}
		finally
		{
			GetUpdateTransactionsStatus();
		}

		return true;
	}



	void IBQueryManager.BeginTransaction()
	{
		Strategy?.BeginTransaction(LiveSettings.EditorExecutionIsolationLevel);
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
					_SqlExec.Cancel(_SyncCancelTimeout);
				}
				finally
				{
					if (synchronous)
					{
						IsExecuting = false;
						UnRegisterSqlExecWithEventHandlers();
						_SqlExec = new QESQLExec(this);
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



	void IBQueryManager.CommitTransaction()
	{
		Strategy?.CommitTransaction();
	}



	private bool GetStatusFlag(EnStatusType statusType)
	{
		lock (_LockLocal)
			return (_Status & (uint)statusType) == (uint)statusType;
	}



	public bool GetUpdateTransactionsStatus() => Strategy != null && Strategy.GetUpdateTransactionsStatus();



	private bool InitializeScriptExecution(string text, EnSqlExecutionType executionType, IDbConnection connection)
	{
		_RowsAffected = 0L;

		return ScriptExecutionStartedEvent?.Invoke(this, new(text, executionType, connection)) ?? true;
	}



	private void RegisterSqlExecWithEvenHandlers()
	{
		lock (_LockLocal)
		{
			_SqlExec.ExecutionCompletedEvent += OnExecutionCompleted;
			_SqlExec.BatchExecutionCompletedEvent += OnBatchExecutionCompleted;
			_SqlExec.BatchExecutionStartEvent += OnBatchExecutionStart;
			_SqlExec.DataLoadedEvent += OnDataLoaded;
			_SqlExec.StatementCompletedEvent += OnStatementCompleted;
			_SqlExec.SqlErrorMessageEvent += OnScriptingErrorMessage;
		}
	}



	void IBQueryManager.RollbackTransaction()
	{
		Strategy?.RollbackTransaction();
	}



	private void SetStateForConnection(IDbConnection newConnection)
	{
		if (newConnection != null)
		{
			if (newConnection is DbConnection dbConnection)
			{
				dbConnection.StateChange += OnConnectionStateChanged;
			}

			IsExecuting = newConnection.State == ConnectionState.Executing;
			IsConnected = newConnection.State == ConnectionState.Open;
			IsConnecting = newConnection.State == ConnectionState.Connecting;
		}
		else
		{
			IsExecuting = false;
			IsConnected = false;
			IsConnecting = false;
		}

		IsCancelling = false;
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



	private void UnRegisterSqlExecWithEventHandlers()
	{
		lock (_LockLocal)
		{
			_SqlExec.ExecutionCompletedEvent -= OnExecutionCompleted;
			_SqlExec.BatchExecutionCompletedEvent -= OnBatchExecutionCompleted;
			_SqlExec.BatchExecutionStartEvent -= OnBatchExecutionStart;
			_SqlExec.DataLoadedEvent -= OnDataLoaded;
			_SqlExec.StatementCompletedEvent -= OnStatementCompleted;
			_SqlExec.SqlErrorMessageEvent -= OnScriptingErrorMessage;
		}
	}


	#endregion Methods





	// =========================================================================================================
	#region Event Handling- QueryManager
	// =========================================================================================================


	private void OnBatchExecutionCompleted(object sender, QESQLBatchExecutedEventArgs args)
	{
		// Tracer.Trace(GetType(), "QryMgr.OnBatchExecutionCompleted", "", null);
		_RowsAffected += args.Batch.RowsAffected;

		BatchExecutionCompletedEvent?.Invoke(sender, args);
	}



	private void OnBatchExecutionStart(object sender, QESQLBatchExecutionStartEventArgs args)
	{
		// Tracer.Trace(GetType(), "QryMgr.OnStartingBatchExecution", "", null);
	}



	private void OnConnectionChanged(object sender, AbstractConnectionStrategy.ConnectionChangedEventArgs args)
	{
		lock (_LockLocal)
		{
			_RowsAffected = 0L;
			QueryExecutionStartTime = null;
			QueryExecutionEndTime = null;
			LiveSettingsApplied = false;
			IDbConnection previousConnection = args.PreviousConnection;
			IDbConnection connection = Strategy.Connection;
			LiveSettings.EditorExecutionTimeout = Strategy.GetExecutionTimeout();
			if (previousConnection != null)
			{
				if (previousConnection is DbConnection dbConnection)
				{
					dbConnection.StateChange -= OnConnectionStateChanged;
				}
			}

			SetStateForConnection(connection);
			OnStatusChanged(new StatusChangedEventArgs(EnStatusType.Connection));
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


	private void OnDatabaseChanged(object sender, EventArgs args) =>
		OnStatusChanged(new StatusChangedEventArgs(EnStatusType.DatabaseChanged));



	private void OnDataLoaded(object sender, QESQLQueryDataEventArgs eventArgs) =>
		DataLoadedEvent?.Invoke(sender, eventArgs);



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
			Strategy.Connection.Close();
	}



	private void OnScriptingErrorMessage(object sender, QEOLESQLErrorMessageEventArgs args)
	{
		ScriptExecutionErrorMessageEvent?.Invoke(sender, args);
	}



	// Added for StaticsPanel.RetrieveStatisticsIfNeeded();
	private void OnStatementCompleted(object sender, QESQLStatementCompletedEventArgs args) =>
		StatementCompletedEvent?.Invoke(sender, args);



	private void OnStatusChanged(StatusChangedEventArgs args) =>
		StatusChangedEvent?.Invoke(this, args);


	#endregion Event Handling





	// =========================================================================================================
	#region Sub-Classes- QueryManager
	// =========================================================================================================


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



	public delegate void StatusChangedEventHandler(object sender, StatusChangedEventArgs args);

	public class StatusChangedEventArgs(EnStatusType changeType) : EventArgs
	{
		public EnStatusType Change { get; private set; } = changeType;
	}


	#endregion Sub-Classes

}
