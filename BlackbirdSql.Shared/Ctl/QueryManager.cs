// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.DataModel.QueryExecutor

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BlackbirdSql.Core.Enums;
using BlackbirdSql.Core.Events;
using BlackbirdSql.Shared.Ctl.Config;
using BlackbirdSql.Shared.Ctl.QueryExecution;
using BlackbirdSql.Shared.Enums;
using BlackbirdSql.Shared.Events;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Shared.Model;
using BlackbirdSql.Shared.Properties;
using BlackbirdSql.Sys.Enums;
using BlackbirdSql.Sys.Interfaces;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.Utilities;



namespace BlackbirdSql.Shared.Ctl;


// =============================================================================================================
//										QueryManager Class
//
/// <summary>
/// Manages query execution of an active document.
/// </summary>
// =============================================================================================================
public sealed class QueryManager : IBsQueryManager
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - QueryManager
	// ---------------------------------------------------------------------------------


	/// <summary>
	/// Default .ctor.
	/// </summary>
	public QueryManager(ConnectionStrategy strategy, uint docCookie)
	{
		_Strategy = strategy;
		_DocCookie = docCookie;

		_SqlExec = new QESQLExec(this);

		RegisterEvenHandlers();
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
			ExecutionStartedEventAsync = null;
			ExecutionCompletedEventAsync = null;
			BatchStatementCompletedEvent = null;
			BatchExecutionCompletedEventAsync = null;
			ErrorMessageEvent = null;
			StatusChangedEvent = null;

			try
			{
				if (IsExecuting)
					Cancel(true);
			}
			catch (Exception ex)
			{
				Diag.Ex(ex);
			}

			UnRegisterEventHandlers();

			_SqlExec.Dispose();
			_SqlExec = null;

			_Strategy.Dispose();
			_Strategy = null;
		}
	}



	void IBsQueryManager.DisposeTransaction() => Strategy?.DisposeTransaction();


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Constants - QueryManager
	// =========================================================================================================


	private const int _SleepWaitTimeout = 50;


	#endregion Constants





	// =========================================================================================================
	#region Fields & Constants - QueryManager
	// =========================================================================================================


	// A private 'this' object lock
	private readonly object _LockLocal = new object();

	private CancellationToken _AsyncCancelToken;
	private CancellationTokenSource _AsyncCancelTokenSource = null;
	private uint _DocCookie = 0;
	private QESQLExec.GetCurrentWorkingDirectoryPath _CurrentWorkingDirectoryPath;
	private int _EventExecutingCardinal = 0;
	private bool _HadTransactions = false;
	private TransientSettings _LiveSettings;
	private bool _LiveSettingsApplied;
	private long _RowsAffected;
	private int _StatementCount;
	private readonly IList<EnQueryState> _StateStack = [];
	private QESQLExec _SqlExec;
	private uint _StateFlags;
	private ConnectionStrategy _Strategy;
	private CancellationToken _SyncCancelToken;
	private CancellationTokenSource _SyncCancelTokenSource = null;
	private TimeSpan _SyncCancelTimeout = new TimeSpan(0, 0, 30);


	#endregion Fields & Constants





	// =========================================================================================================
	#region Property Accessors- QueryManager
	// =========================================================================================================


	private CancellationToken AsyncCancelToken
	{
		get { lock (_LockLocal) return _AsyncCancelTokenSource?.Token ?? default; }
	}


	public CancellationTokenSource AsyncCancelTokenSource
	{
		get { lock (_LockLocal) return _AsyncCancelTokenSource; }
	}


	private EnLauncherPayloadLaunchState AsyncExecState =>
		_SqlExec?.AsyncExecState ?? EnLauncherPayloadLaunchState.Inactive;

	private Task<bool> AsyncExecTask => _SqlExec?.AsyncExecTask;

	public IDbConnection DataConnection => _Strategy?.Connection;

	public ConnectionState DataConnectionState => DataConnection?.State ?? ConnectionState.Closed;


	public uint DocCookie
	{
		get
		{
			lock (_LockLocal)
				return _DocCookie;
		}
		set
		{
			lock (_LockLocal)
			{
				_DocCookie = value;
				if (Strategy != null)
					Strategy.DocCookie = value;
			}
		}
	}


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


	public string BatchSeparator => LiveSettings.EditorExecutionBatchSeparator;

	public long ExecutionTimeout => LiveSettings.EditorExecutionTimeout * 60000L;

	public bool HadTransactions => _HadTransactions;


	private	bool HasActiveActions => _StateStack.Count > 0
		&& _StateStack[^1] > EnQueryState.ActionMarker;


	public bool HasTransactions => Strategy?.HasTransactions ?? false;


	public bool IsCancelling => GetStateValue(EnQueryState.Cancelling);

	public bool IsPrompting => GetStateValue(EnQueryState.Prompting);

	public bool IsConnected => GetStateValue(EnQueryState.Connected);

	public bool IsConnecting => GetStateValue(EnQueryState.Connecting);

	public bool IsDatabaseChanged => GetStateValue(EnQueryState.DatabaseChanged);

	public bool IsExecuting => GetStateValue(EnQueryState.Executing);

	public bool IsFaulted => GetStateValue(EnQueryState.Faulted);

	public bool IsLocked => !EventLockEnter(true);

	public bool IsOnline => GetStateValue(EnQueryState.Online);



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
				_LiveSettings.TtsEnabled = PersistentSettings.EditorTtsDefault;

				return _LiveSettings;
			}
		}
	}


	public bool LiveSettingsApplied
	{
		get { lock (_LockLocal) return _LiveSettingsApplied; }
		set { lock (_LockLocal) _LiveSettingsApplied = value; }
	}


	public bool LiveTransactions
	{
		get
		{
			if (!EventLockEnter(false, true))
				return false;

			try
			{
				return Strategy.LiveTransactions;
			}
			finally
			{
				EventLockExit();
			}
		}
	}


	public bool PeekTransactions => Strategy?.PeekTransactions ?? false;


	public DateTime? QueryExecutionEndTime { get; set; }

	public DateTime? QueryExecutionStartTime { get; set; }

	public IBsQESQLBatchConsumer ResultsConsumer { get; set; }


	public long RowsAffected
	{
		get { lock (_LockLocal) return _RowsAffected; }
	}


	public int StatementCount
	{
		get { lock (_LockLocal) return _StatementCount; }
	}


	public uint StateFlags => _StateFlags;



	public ConnectionStrategy Strategy
	{
		get { lock (_LockLocal) return _Strategy; }
	}


	public CancellationToken SyncCancelToken
	{
		get { lock (_LockLocal) return _SyncCancelTokenSource?.Token ?? default; }
	}


	public CancellationTokenSource SyncCancelTokenSource
	{
		get { lock (_LockLocal) return _SyncCancelTokenSource; }
	}


	public IDbTransaction Transaction => Strategy?.Transaction;

	public IsolationLevel TtsIsolationLevel => LiveSettings.EditorExecutionIsolationLevel;


	public event QueryStateChangedEventHandler StatusChangedEvent;
	public event ExecutionStartedEventHandler ExecutionStartedEventAsync;
	public event ExecutionCompletedEventHandler ExecutionCompletedEventAsync;
	public event BatchExecutionCompletedEventHandler BatchExecutionCompletedEventAsync;
	public event ErrorMessageEventHandler ErrorMessageEvent;
	public event BatchStatementCompletedEventHandler BatchStatementCompletedEvent;

	// Added for StaticsPanel.RetrieveStatisticsIfNeeded();
	public event QueryDataEventHandler BatchDataLoadedEvent;
	public event QueryDataEventHandler BatchScriptParsedEvent;

	public event NotifyConnectionStateEventHandler NotifyConnectionStateEvent;


	#endregion Property Accessors





	// =========================================================================================================
	#region Methods- QueryManager
	// =========================================================================================================


	/// <summary>
	/// <see cref="ErrorHandler.Succeeded"/> token.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Future usage")]
	private bool __(int hr) => ErrorHandler.Succeeded(hr);



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// [Launch async]: Executes a query.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public bool ExecuteAsyin(IBsTextSpan textSpan, EnSqlExecutionType executionType)
	{
		// Evs.Trace(GetType(), nameof(AsyncExecute), " Enter. : ExecutionOptions.EstimatedPlanOnly: " + LiveSettings.EstimatedPlanOnly);

		_HadTransactions = HasTransactions;

		if (!EventExecutingEnter())
		{
			InvalidOperationException ex = new(Resources.ExceptionExecutionNotCompleted);
			Diag.Ex(ex);
			throw ex;
		}

		if (textSpan == null || string.IsNullOrWhiteSpace(textSpan.Text))
		{
			SetStateValue(EnQueryState.Faulted, true);
			SetStateValue(EnQueryState.Prompting, true);
			ArgumentException ex = new("Query is empty");
			MessageCtl.ShowX(ex, Resources.ExceptionDatabaseNotAccessible, null, MessageBoxButtons.OK, MessageBoxIcon.Hand);
			SetStateValue(EnQueryState.Prompting, false);

			EventExecutingExit();
			return false;
		}

		_AsyncCancelToken = default;
		_AsyncCancelTokenSource?.Dispose();
		_AsyncCancelTokenSource = new();
		_AsyncCancelToken = _AsyncCancelTokenSource.Token;

		_SyncCancelToken = default;
		_SyncCancelTokenSource?.Dispose();
		_SyncCancelTokenSource = new();
		_SyncCancelToken = _SyncCancelTokenSource.Token;

		// Fire and forget.
		Task.Factory.StartNew(() => ExecuteAsync(textSpan, executionType, _AsyncCancelToken, _SyncCancelToken),
			default, TaskCreationOptions.PreferFairness | TaskCreationOptions.DenyChildAttach,
			TaskScheduler.Default).Forget();

		return true;
	}



	private async Task<bool> ExecuteAsync(IBsTextSpan textSpan, EnSqlExecutionType executionType,
		CancellationToken cancelToken, CancellationToken syncToken)
	{
		IDbConnection connection = null;
		TransientSettings liveSettings;


		if (!cancelToken.Cancelled())
		{
			if (Strategy.Connection != null && Strategy.Connection.State == ConnectionState.Open)
				await Strategy.VerifyOpenConnectionAsync(cancelToken);
		}

		if (cancelToken.Cancelled())
		{
			await RaiseExecutionStartedAsync(textSpan.Text, executionType, false, connection,
				cancelToken, syncToken);
			await RaiseExecutionCompletedAsync(EnScriptExecutionResult.Halted, executionType,
				LiveSettings.EditorResultsOutputMode, false, LiveSettings.WithClientStats,
				cancelToken, syncToken);

			return false;
		}

		connection = Strategy.Connection;

		if (connection == null || connection.State != ConnectionState.Open)
		{
			// Connect on the fly.

			IDbConnection existingConnection = connection;

			SetStateValue(EnQueryState.Connecting, true);
			await RdtManager.InvalidateToolbarAsync(DocCookie);

			try
			{
				connection = await Strategy.EnsureVerifiedOpenConnectionAsync(cancelToken);
			}
			catch (Exception ex)
			{
				await RaiseExecutionStartedAsync(textSpan.Text, executionType, false, connection, cancelToken, syncToken);

				try
				{
					SetStateValue(EnQueryState.Cancelling, true);
					SetStateValue(EnQueryState.Faulted, true);
					SetStateValue(EnQueryState.Prompting, true);

					await RdtManager.InvalidateToolbarAsync(DocCookie);
					await RdtManager.ShowWindowFrameAsync(DocCookie);

					string str = Strategy?.DisplayServerName;
					if (!string.IsNullOrWhiteSpace(str))
					{
						ex.SetServer(str);
						str = Strategy?.DatasetDisplayName;
						if (!string.IsNullOrWhiteSpace(str))
							ex.SetDatabase(str);
					}

					MessageCtl.ShowX(ex, Resources.ExceptionDatabaseNotAccessible, null, MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
				finally
				{
					SetStateValue(EnQueryState.Prompting, false);
					SetStateValue(EnQueryState.Connecting, false);

					await RaiseExecutionCompletedAsync(EnScriptExecutionResult.Failure, executionType,
						LiveSettings.EditorResultsOutputMode, false, LiveSettings.WithClientStats,
						cancelToken, syncToken);
				}

				return false;
			}


			if (connection == null || cancelToken.Cancelled())
			{
				SetStateValue(EnQueryState.Connecting, false);
				SetStateValue(EnQueryState.Cancelling, true);

				await RaiseExecutionStartedAsync(textSpan.Text, executionType, false, connection,
					cancelToken, syncToken);
				await RaiseExecutionCompletedAsync(EnScriptExecutionResult.Cancel, executionType,
					LiveSettings.EditorResultsOutputMode, false, LiveSettings.WithClientStats,
					cancelToken, syncToken);

				return false;
			}

			// If it's the same connection, IsConnecting will not be reset, so we have to do it.

			if (existingConnection != null && ReferenceEquals(existingConnection, connection))
				SetStateValue(EnQueryState.Connecting, false);
		}

		_HadTransactions = LiveTransactions;

		// Evs.Trace(GetType(), nameof(ExecuteAsync), "Ensured connection");


		// Evs.Trace(GetType(), nameof(ExecuteAsync), "execTimeout = {0}.", execTimeout);

		if (!LiveSettingsApplied)
			LiveSettingsApplied = true;

		LiveSettings.ExecutionType = executionType;


		liveSettings = (TransientSettings)LiveSettings.Clone();

		// Evs.Trace(GetType(), nameof(ExecuteAsync), "executionType: {0},  LiveSettings.ExecutionType: {1}, liveSettings.ExecutionType: {2}.",
		//	executionType,  LiveSettings.ExecutionType, liveSettings.ExecutionType);


		if (!await RaiseExecutionStartedAsync(textSpan.Text, executionType, true, connection,
			cancelToken, syncToken))
		{
			await RaiseExecutionCompletedAsync(EnScriptExecutionResult.Halted, executionType,
				liveSettings.EditorResultsOutputMode, false, liveSettings.WithClientStats, cancelToken, syncToken);

			return false;
		}


		// -------------------------------------------------------------------------------- //
		// *************** Execution Point (2) - QueryManager.ExecuteAsync() ************** //
		// -------------------------------------------------------------------------------- //

		bool result = false;

		try
		{
			result = await _SqlExec.ExecuteAsyinAsync(textSpan, executionType, connection, ResultsConsumer, liveSettings, cancelToken, syncToken);
		}
		finally
		{
			if (!result)
			{
				await RaiseExecutionCompletedAsync(EnScriptExecutionResult.Failure, executionType,
					liveSettings.EditorResultsOutputMode, false, liveSettings.WithClientStats,
					cancelToken, syncToken);
			}
		}

		return result;
	}



	void IBsQueryManager.BeginTransaction()
	{
		Strategy?.BeginTransaction(LiveSettings.EditorExecutionIsolationLevel);
	}



	[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD003:Avoid awaiting foreign Tasks", Justification = "<Pending>")]
	public bool Cancel(bool synchronous)
	{
		if (!IsExecuting && !IsConnecting)
			return true;

		// ExecutionCompletedEventHandler executionCompletedHandler = null;

		SetStateValue(EnQueryState.Cancelling, true);

		Task<bool> task = null;

		try
		{
			// Evs.Trace(GetType(), "QESQLExec.Cancel", "", null);
			if ((AsyncExecTask?.IsCompleted ?? true)
				|| (AsyncCancelTokenSource?.IsCancellationRequested ?? true))
			{
				return true;
			}

			AsyncCancelTokenSource.Cancel();

			if (synchronous)
				SyncCancelTokenSource.Cancel();

			long startTimeEpoch = DateTime.Now.UnixMilliseconds();
			long currentTimeEpoch = startTimeEpoch;
			long maxTimeout = (long)_SyncCancelTimeout.TotalMilliseconds;

			TimeSpan sleepWait = new(0, 0, 0, 0, _SleepWaitTimeout);

			// Evs.Trace(GetType(), "QESQLExec.Cancel", "maxTimeOut: {0}.", maxTimeout);

			task = Task.Run(async delegate
			{
				while (AsyncExecState != EnLauncherPayloadLaunchState.Inactive && !(AsyncExecTask?.IsCompleted ?? true))
				{
					/*
					if (currentTimeEpoch - startTimeEpoch > maxTimeout)
					{
						Evs.Warning(GetType(), "Cancel()",
							$"Timed out waiting for AsyncExecTask to complete. " +
							$"Forgetting about it. Timeout (ms): {(currentTimeEpoch - startTimeEpoch)}.");

						HookupBatchConsumer(_CurBatch, _BatchConsumer, false);

						_AsyncExecState = EnLauncherPayloadLaunchState.Discarded;
						// _AsyncCancelToken = default;
						// _AsyncCancelTokenSource = null;
						_AsyncExecTask = null;

						return false;
					}
					*/

					Application.DoEvents();
					Thread.Sleep(sleepWait);

					try
					{
						if (!(AsyncExecTask?.IsCompleted ?? true))
							await AsyncExecTask.WithTimeout(sleepWait);
					}
					catch { }

					currentTimeEpoch = DateTime.Now.UnixMilliseconds();

				}

				// Evs.Trace(GetType(), nameof(Cancel), "Thread stopped gracefully");

				return true;

			});
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


			// IsCancelling = false;

			/*
			if (executionCompletedHandler != null && synchronous)
			{
				executionCompletedHandler(this, new ExecutionCompletedEventArgs(EnScriptExecutionResult.Cancel, false));
			}
			*/
		}

		// Evs.Trace(GetType(), "Cancel", "CANCELLATION LAUNCHED.");

		bool result = true;

		if (synchronous)
			result = task.AwaiterResult();

		// IsCancelling = false;

		return result;

	}



	void IBsQueryManager.CloseConnection() => Strategy?.CloseConnection();



	public bool CommitTransactions(bool validate)
	{
		if (!IsConnected || !EventLockEnter())
			return false;

		SetStateValue(EnQueryState.Connecting, true);
		bool result = false;

		try
		{
			result = Strategy.CommitTransactions(validate);
		}
		finally
		{
			SetStateValue(EnQueryState.Connecting, false);
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
			_ = LiveTransactions;
		}
		finally
		{
			EventLockExit();
		}
	}



	public bool DsqlCommit() => Strategy.CommitTransactions(true);



	public bool DsqlRollback() => Strategy.RollbackTransactions(true);



	public bool EnsureVerifiedOpenConnection()
	{
		if (!EventLockEnter())
			return false;

		SetStateValue(EnQueryState.Connecting, true);

		IDbConnection connection = null;

		// Fire and forget

		Task.Factory.StartNew(
			async () =>
			{
				try
				{
					connection = await Strategy?.EnsureVerifiedOpenConnectionAsync(default);
				}
				catch (Exception ex)
				{
					SetStateValue(EnQueryState.Faulted, true);
					SetStateValue(EnQueryState.Prompting, true);
					await RdtManager.ShowWindowFrameAsync(DocCookie);

					string server = Strategy?.MdlCsb?.DataSource;
					if (!string.IsNullOrWhiteSpace(server))
						ex.SetServer(server);

					MessageCtl.ShowX(ex, Resources.ExceptionDatabaseNotAccessible, null, MessageBoxButtons.OK, MessageBoxIcon.Hand);
					SetStateValue(EnQueryState.Prompting, false);
				}
				finally
				{
					if (connection == null)
						SetStateValue(EnQueryState.Connecting, false);

					EventLockExit();
				}
			},
			default, TaskCreationOptions.PreferFairness, TaskScheduler.Default).Forget();

		return true;
	}



	private bool GetStateValue(EnQueryState statusType)
	{
		lock (_LockLocal)
			return (_StateFlags & (uint)statusType) == (uint)statusType;
	}


	public bool ModifyConnection()
	{
		if (!EventLockEnter())
			return false;

		SetStateValue(EnQueryState.Connecting, true);

		bool result = false;

		try
		{
			result = Strategy.ModifyConnection();
		}
		catch (Exception ex)
		{
			SetStateValue(EnQueryState.Faulted, true);
			SetStateValue(EnQueryState.Prompting, true);

			string str = Strategy?.DisplayServerName;
			if (!string.IsNullOrWhiteSpace(str))
			{
				ex.SetServer(str);
				str = Strategy?.DatasetDisplayName;
				if (!string.IsNullOrWhiteSpace(str))
					ex.SetDatabase(str);
			}

			MessageCtl.ShowX(ex, Resources.ExceptionDatabaseNotAccessible, null, MessageBoxButtons.OK, MessageBoxIcon.Hand);
			SetStateValue(EnQueryState.Prompting, false);
		}
		finally
		{
			if (!result)
				SetStateValue(EnQueryState.Connecting, false);
			EventLockExit();
		}

		return false;
	}



	public bool NotifyConnectionState(EnNotifyConnectionState state)
	{
		if (SyncCancelToken.Cancelled())
			return false;

		if (!IsConnected && state == EnNotifyConnectionState.ConfirmedOpen)
		{
			SetStateValue(EnQueryState.Connected, true);
			RdtManager.InvalidateToolbarAsyeu(DocCookie);
		}
		else if (IsConnected && state == EnNotifyConnectionState.ConfirmedClosed)
		{
			SetStateValue(EnQueryState.Connected, false);
			RdtManager.InvalidateToolbarAsyeu(DocCookie);
		}


		// else if (state == EnNotifyConnectionState.RequestIsUnlocked) is always honored.

		return !IsLocked;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Pops a disabled state off the stack and ensures syncronicity
	/// between the changing states of a query document.
	/// Also ensures that program logic does not violate the single push
	/// (enable) to single pop (disable) pairing of a state.
	/// </summary>
	/// <param name="value">The state being disabled.</param>
	/// <returns>
	/// A tuple of the two previous states that are queued to be popped.
	/// </returns>
	// ---------------------------------------------------------------------------------
	private (EnQueryState, EnQueryState) PopStateStack(EnQueryState value)
	{
		EnQueryState prevState = EnQueryState.None;
		EnQueryState prevPrevState = EnQueryState.None;


		if (value < EnQueryState.VirtualMarker)
			return (prevState, prevPrevState);


		if (_StateStack.Count == 0)
			Diag.ThrowException(new ApplicationException($"No more States to Pop. Requested: {value}."));

		int i;
		int j = _StateStack.Count - 1;

		for (i = j; i >= 0 && _StateStack[i] != value; i--) ;

		if (i < 0)
			Diag.ThrowException(new ApplicationException($"Requested State to pop could not be found in the stack. . Requested: {value}."));

		if (i == j)
			j--;

		if (j >= 0)
			prevState = _StateStack[j];

		j--;

		if (i == j)
			j--;

		if (j >= 0)
			prevPrevState = _StateStack[j];

		_StateStack.RemoveAt(i);

		// If there are no more actions (besides Executing) in the stack, clear the virtual flags.

		if (value > EnQueryState.OperationMarker && !HasActiveActions)
		{
			for (uint state = 1; state < (uint)EnQueryState.VirtualMarker; state *= 2)
					_StateFlags &= ~state;
		}

		/*
		string str = "";

		foreach (EnQueryState state in _StateStack)
			str += state + ", ";

		// Evs.Trace(GetType(), nameof(PopStateStack),
			"\nPopped: {0}, prevState: {1}, prevPrevState: {2}, DatabaseChanged: {3}, Stack: {4}.",
			value, prevState, prevPrevState, IsDatabaseChanged, str);
		*/

		return (prevState, prevPrevState);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Pushes an enabled state onto the stack and ensures syncronicity
	/// between the changing states of a query document.
	/// Also ensures that program logic does not violate the single push
	/// (enable) to single pop (disable) pairing of a state.
	/// </summary>
	/// <param name="value">The state being enabled.</param>
	/// <returns>
	/// A tuple of the two previously enabled states that are queued to be
	/// popped.
	/// </returns>
	// ---------------------------------------------------------------------------------
	private (EnQueryState, EnQueryState) PushStateStack(EnQueryState value)
	{
		EnQueryState prevState = EnQueryState.None;
		EnQueryState prevPrevState = EnQueryState.None;

		if (value < EnQueryState.VirtualMarker)
			return (prevState, prevPrevState);


		foreach (EnQueryState queryState in _StateStack)
		{
			if (queryState == value)
				Diag.ThrowException(new ApplicationException($"Requested State to push is already on the stack. Requested: {value}."));
		}


		int i;

		if (value > EnQueryState.ActionMarker)
		{
			i = _StateStack.Count;

			_StateStack.Add(value);

		}
		else if (value > EnQueryState.OperationMarker)
		{
			// Pure Operation states before any Action operation states.

			for (i = _StateStack.Count; i > 0 && _StateStack[i - 1] > EnQueryState.ActionMarker; i--) ;

			_StateStack.Insert(i, value);
		}
		else
		{
			// Fixed states before any Operation/Action states.

			for (i = _StateStack.Count; i > 0 && _StateStack[i - 1] > EnQueryState.OperationMarker; i--) ;

			_StateStack.Insert(i, value);
		}

		int j = _StateStack.Count - 1;

		if (i == j)
			j--;

		if (j >= 0)
			prevState = _StateStack[j];

		j--;

		if (i == j)
			j--;

		if (j >= 0)
			prevPrevState = _StateStack[j];

		/*
		string str = "";

		foreach (EnQueryState state in _StateStack)
			str += state + ", ";

		// Evs.Trace(GetType(), nameof(PushStateStack),
			"\nPushed: {0}, prevState: {1}, prevPrevState: {2}, DatabaseChanged: {3}, Stack: {4}.",
			value, prevState, prevPrevState, IsDatabaseChanged, str);
		*/

		return (prevState, prevPrevState);
	}



	public bool RollbackTransactions(bool validate)
	{
		if (!IsConnected || !EventLockEnter())
			return false;

		SetStateValue(EnQueryState.Connecting, true);
		bool result = false;

		try
		{
			result = Strategy.RollbackTransactions(validate);
		}
		finally
		{
			SetStateValue(EnQueryState.Connecting, false);
			EventLockExit();
		}

		return result;
	}



	public bool SetDatasetKeyOnConnection(string selectedQualifiedName)
	{
		// Evs.Trace(GetType(), nameof(SetDatasetKeyOnConnection), "selectedQualifiedName: {0}.", selectedQualifiedName);

		if (!EventLockEnter(invalidateToolbar: false))
			return false;

		try
		{
			return Strategy?.SetDatasetKeyOnConnection(selectedQualifiedName) ?? false;
		}
		catch (DbException ex)
		{
			Diag.Expected(ex);

			SetStateValue(EnQueryState.Faulted, true);
			SetStateValue(EnQueryState.Connecting, true);
			SetStateValue(EnQueryState.Prompting, true);

			MessageCtl.ShowX(ex, Resources.ExceptionDatabaseNotAccessibleEx.Fmt(selectedQualifiedName), null, MessageBoxButtons.OK, MessageBoxIcon.Hand);

			SetStateValue(EnQueryState.Prompting, false);
			SetStateValue(EnQueryState.Connecting, false);
			return true;
		}
		catch (Exception ex)
		{
			Diag.Ex(ex);
		}
		finally
		{
			EventLockExit();
		}

		return false;
	}


	public void SetState(EnQueryState state, bool value) => SetStateValue(state, value);



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Sets the status flag of a status setting. If a secondary flag is provided the
	/// QueryStatusChangedEventArgs will include it in a single OnStatusChanged event.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void SetStateValue(EnQueryState stateFlag, bool value)
	{
		if (stateFlag == EnQueryState.Online && value && !IsOnline)
			SetStateValue(EnQueryState.DatabaseChanged, true);

		bool current;
		uint currentFlags = 0u;
		EnQueryState prevState = EnQueryState.None;
		EnQueryState prevPrevState = EnQueryState.None;

		try
		{
			lock (_LockLocal)
			{
				currentFlags = _StateFlags;

				current = (_StateFlags & (uint)stateFlag) == (uint)stateFlag;

				if (current == value)
					return;

				if (current != value)
				{
					if (value)
					{
						_StateFlags |= (uint)stateFlag;
						(prevState, prevPrevState) = PushStateStack(stateFlag);
					}
					else
					{
						_StateFlags &= ~(uint)stateFlag;
						(prevState, prevPrevState) = PopStateStack(stateFlag);
					}
				}
			}

			if (stateFlag > EnQueryState.VirtualMarker)
				RaiseStateChanged(currentFlags, stateFlag, value, prevState, prevPrevState);
		}
		finally
		{
			if (stateFlag == EnQueryState.Connected && value)
				SetStateValue(EnQueryState.Connecting, false);
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
	private bool EventExecutingEnter(bool test = false, bool force = false)
	{
		lock (_LockLocal)
		{
			if (!test && !force)
				Strategy.ResetKeepAliveTimeEpoch();

			if (_EventExecutingCardinal != 0 && !force)
				return false;

			if (test)
				return true;

			_EventExecutingCardinal++;

			if (_EventExecutingCardinal != 1)
				return true;
		}

		SetStateValue(EnQueryState.Executing, true);
		RdtManager.InvalidateToolbarAsyeu(DocCookie);

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
				Diag.Ex(ex);
				throw ex;
			}

			_EventExecutingCardinal--;

			if (_EventExecutingCardinal != 0)
				return;
		}

		SetStateValue(EnQueryState.Executing, false);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Increments the <see cref="_EventExecutingCardinal"/> counter when any action
	/// involves the database.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public bool EventLockEnter(bool test = false, bool force = false, bool invalidateToolbar = true)
	{
		lock (_LockLocal)
		{
			if (!test && !force)
				Strategy.ResetKeepAliveTimeEpoch();

			if (Strategy == null)
				return false;

			if (_EventExecutingCardinal != 0 && !force)
				return false;

			if (test)
				return true;

			_EventExecutingCardinal++;

			if (force || _EventExecutingCardinal != 1)
				return true;
		}

		if (invalidateToolbar)
			RdtManager.InvalidateToolbarAsyeu(DocCookie);

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
				Diag.Ex(ex);
				throw ex;
			}

			_EventExecutingCardinal--;
		}
	}



	private async Task OnBatchExecutionCompletedAsync(object sender, BatchExecutionCompletedEventArgs args)
	{
		// Evs.Trace(GetType(), nameof(OnBatchExecutionCompletedAsync), "", null);

		await Cmd.AwaitableAsync();

		_RowsAffected += args.Batch.RowsAffected;

		BatchExecutionCompletedEventAsync?.RaiseEventAsync(sender, args);
	}



	private void OnBatchExecutionStart(object sender, BatchExecutionStartEventArgs args)
	{
		// Evs.Trace(GetType(), nameof(OnBatchExecutionStart), "", null);
	}



	private void OnConnectionChanged(object sender, ConnectionChangedEventArgs args)
	{
		// Evs.Trace(GetType(), nameof(OnConnectionChanged), "Current: {0}, Previous: {1}.", args.CurrentConnection != null, args.PreviousConnection != null);

		if (SyncCancelToken.Cancelled())
			return;

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

			if (previousConnection != null && previousConnection.State == ConnectionState.Open)
			{
				SetStateValue(EnQueryState.Connected, false);
			}

			if (currentConnection != null && currentConnection.State == ConnectionState.Open)
			{
				SetStateValue(EnQueryState.Connected, true);
			}

			/*
			SetStateForConnection(Strategy.MdlCsb, currentConnection);

			bool open = DataConnectionState == ConnectionState.Open;

			EnQueryState state = open
				? EnQueryState.Connected : EnQueryState.Connection;

			(EnQueryState prevState, EnQueryState prevPrevState) = PopStateStack(state);

			RaiseStateChanged(_StateFlags, state, open, prevState, prevPrevState);
			*/
		}
	}



	private void OnConnectionStateChanged(object sender, StateChangeEventArgs args)
	{
		if (SyncCancelToken.Cancelled())
			return;

		// Evs.Trace(GetType(), nameof(OnConnectionStateChanged), "IN");

		bool hasTransactions = false;

		lock (_LockLocal)
		{
			using (DpiAwareness.EnterDpiScope(DpiAwarenessContext.SystemAware))
			{
				ConnectionState currentState = args.CurrentState;
				ConnectionState originalState = args.OriginalState;

				if (currentState == ConnectionState.Connecting)
					SetStateValue(EnQueryState.Connecting, true);

				switch (currentState)
				{
					case ConnectionState.Broken:

						hasTransactions = Strategy?.HasTransactions ?? false;
						SetStateValue(EnQueryState.Connected, false);
						LiveSettingsApplied = false;

						break;

					case ConnectionState.Closed:
						SetStateValue(EnQueryState.Connected, false);
						LiveSettingsApplied = false;

						// if (IsExecuting)
						//	IsExecuting = false;

						break;

					case ConnectionState.Open:
						if (originalState == ConnectionState.Closed || originalState == ConnectionState.Connecting)
						{
							SetStateValue(EnQueryState.Connected, true);
							LiveSettingsApplied = false;
						}

						break;
				}
			}
		}

		if (args.CurrentState == ConnectionState.Broken)
			RaiseNotifyConnectionState(EnNotifyConnectionState.NotifyBroken, hasTransactions);
	}



	private void OnDatabaseChanged(object sender, DatabaseChangeEventArgs args)
	{
		// Evs.Trace(GetType(), nameof(OnDatabaseChanged), "IN");

		if (args.PreviousCsb != null)
			SetStateValue(EnQueryState.Online, false);

		SetStateValue(EnQueryState.Online, args.CurrentCsb != null);
	}



	private void OnBatchDataLoaded(object sender, QueryDataEventArgs eventArgs)
	{
		BatchDataLoadedEvent?.Invoke(sender, eventArgs);
	}



	private void OnBatchScriptParsed(object sender, QueryDataEventArgs eventArgs)
	{
		_StatementCount = eventArgs.StatementCount;

		BatchScriptParsedEvent?.Invoke(sender, eventArgs);
	}



	private async Task<bool> OnExecutionCompletedAsync(object sender, ExecutionCompletedEventArgs args)
	{
		QueryExecutionEndTime = DateTime.Now;

		// Evs.Trace(GetType(), nameof(OnExecutionCompletedAsync), "Calling ExecutionCompletedEventAsync. args.ExecResult: {0}.", args.ExecutionResult);

		bool result = true;

		try
		{
			await ExecutionCompletedEventAsync?.RaiseEventAsync(this, args);

		}
		catch (Exception ex)
		{
			result = false;
			Diag.Ex(ex);
		}


		try
		{
			if (args.Launched && LiveSettings.EditorExecutionDisconnectOnCompletion)
				Strategy.CloseConnection();
		}
		catch (Exception ex)
		{
			result = false;
			Diag.Ex(ex);
		}

		if (args.Launched && !args.SyncToken.Cancelled())
			await Strategy.VerifyOpenConnectionAsync(default);

		args.Result &= result;

		EventExecutingExit();
		SetStateValue(EnQueryState.Cancelling, false);

		RdtManager.InvalidateToolbarAsync(DocCookie).Forget();

		return result;
	}



	private void OnErrorMessage(object sender, ErrorMessageEventArgs args)
	{
		if (SyncCancelToken.Cancelled())
			return;

		ErrorMessageEvent?.Invoke(sender, args);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Added for StaticsPanel.RetrieveStatisticsIfNeeded();
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void OnBatchStatementCompleted(object sender, BatchStatementCompletedEventArgs args) =>
		BatchStatementCompletedEvent?.Invoke(sender, args);



	private async Task<bool> RaiseExecutionCompletedAsync(EnScriptExecutionResult executionResult,
			EnSqlExecutionType executionType, EnSqlOutputMode outputMode, bool launched, bool withClientStats,
			CancellationToken cancelToken, CancellationToken syncToken)
	{
		// Evs.Trace(GetType(), nameof(RaiseExecutionCompletedAsync));

		ExecutionCompletedEventArgs args = new(executionResult, executionType, outputMode, launched,
			withClientStats, cancelToken, syncToken);

		return await OnExecutionCompletedAsync(this, args);
	}



	private async Task<bool> RaiseExecutionStartedAsync(string queryText, EnSqlExecutionType executionType,
		bool launched, IDbConnection connection, CancellationToken cancelToken, CancellationToken syncToken)
	{
		if (AsyncCancelToken.Cancelled())
			return false;

		// await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

		_RowsAffected = 0L;
		_StatementCount = 0;

		QueryExecutionStartTime = DateTime.Now;
		QueryExecutionEndTime = DateTime.Now;


		ExecutionStartedEventArgs args = new(queryText, executionType, launched, connection, cancelToken, syncToken);

		bool result = true;

		try
		{
			await ExecutionStartedEventAsync?.RaiseEventAsync(this, args);
		}
		catch (Exception ex)
		{
			result = false;
			Diag.Ex(ex);
		}

		args.Result &= result;

		// args.Result == false will cancel the launch.

		return args.Result;
	}



	private void RaiseNotifyConnectionState(EnNotifyConnectionState state, bool ttsDiscarded)
	{
		if (SyncCancelToken.Cancelled())
			return;

		NotifyConnectionStateEvent?.Invoke(this, new NotifyConnectionStateEventArgs(state, ttsDiscarded));
	}



	private void RaiseStateChanged(uint stateFlags, EnQueryState stateFlag, bool newValue,
		EnQueryState prevState, EnQueryState prevPrevState)
	{
		if (SyncCancelToken.Cancelled())
			return;

		StatusChangedEvent?.Invoke(this, new(stateFlags, stateFlag, newValue, prevState, prevPrevState));
	}



	private void RegisterEvenHandlers()
	{
		lock (_LockLocal)
		{
			_Strategy.ConnectionChangedEvent += OnConnectionChanged;
			_Strategy.ConnectionStateChangedEvent += OnConnectionStateChanged;
			_Strategy.DatabaseChangedEvent += OnDatabaseChanged;

			_SqlExec.ExecutionCompletedEventAsync += OnExecutionCompletedAsync;
			_SqlExec.BatchExecutionCompletedEventAsync += OnBatchExecutionCompletedAsync;
			_SqlExec.BatchExecutionStartEvent += OnBatchExecutionStart;
			_SqlExec.BatchDataLoadedEvent += OnBatchDataLoaded;
			_SqlExec.BatchScriptParsedEvent += OnBatchScriptParsed;
			_SqlExec.BatchStatementCompletedEvent += OnBatchStatementCompleted;
			_SqlExec.ErrorMessageEvent += OnErrorMessage;
		}
	}



	private void UnRegisterEventHandlers()
	{
		lock (_LockLocal)
		{
			_Strategy.ConnectionChangedEvent -= OnConnectionChanged;
			_Strategy.ConnectionStateChangedEvent -= OnConnectionStateChanged;
			_Strategy.DatabaseChangedEvent -= OnDatabaseChanged;

			_SqlExec.ExecutionCompletedEventAsync -= OnExecutionCompletedAsync;
			_SqlExec.BatchExecutionCompletedEventAsync -= OnBatchExecutionCompletedAsync;
			_SqlExec.BatchExecutionStartEvent -= OnBatchExecutionStart;
			_SqlExec.BatchDataLoadedEvent -= OnBatchDataLoaded;
			_SqlExec.BatchScriptParsedEvent -= OnBatchScriptParsed;
			_SqlExec.BatchStatementCompletedEvent -= OnBatchStatementCompleted;
			_SqlExec.ErrorMessageEvent -= OnErrorMessage;
		}
	}


	#endregion Event Handling





	// =========================================================================================================
	#region Sub-Classes- QueryManager
	// =========================================================================================================


	#endregion Sub-Classes

}
