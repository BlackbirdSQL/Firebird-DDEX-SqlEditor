// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution.QESQLExec
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BlackbirdSql.Core.Enums;
using BlackbirdSql.Shared.Ctl.Config;
using BlackbirdSql.Shared.Enums;
using BlackbirdSql.Shared.Events;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Shared.Properties;
using BlackbirdSql.Sys.Enums;
using BlackbirdSql.Sys.Interfaces;



namespace BlackbirdSql.Shared.Ctl.QueryExecution;


public abstract class AbstractQESQLExec : IDisposable
{
	protected enum EnExecState
	{
		Initial,
		Executing,
		ExecutingBatch,
		Cancelling,
		Discarded
	}



	public const int C_Int1MB = 1048576;
	public const int C_Int5MB = 5242880;
	public const int int2GB = int.MaxValue;
	// public const string C_PlanConnectionType = "FbConnection"; // "SqlCeConnection"





	// A protected 'this' object lock
	protected object _LockObject = new object();

	protected IBQESQLBatchConsumer _BatchConsumer;
	protected IDbConnection _Conn;
	protected EnSqlSpecialActions _SpecialActions;
	protected int _ExecTimeout;
	private TransientSettings _ExecLiveSettings;
	protected IBTextSpan _TextSpan;
	protected EnSqlExecutionType _ExecutionType;
	private bool _ExecOptionHasBeenChanged;
	protected int _CurBatchIndex = -1;
	protected QESQLBatch _CurBatch;
	protected string _TextPlan = null;
	protected EnScriptExecutionResult _ExecResult = EnScriptExecutionResult.Failure;
	// private Thread _ExecThread;
	// protected EnExecState _ExecState;


	protected EnLauncherPayloadLaunchState _AsyncExecState = EnLauncherPayloadLaunchState.Inactive;
	private CancellationTokenSource _AsyncExecCancelTokenSource = null;
	private CancellationToken _AsyncExecCancelToken;
	private CancellationTokenSource _AsyncLaunchCancelTokenSource = null;
	private CancellationToken _AsyncLaunchCancelToken;

	private Task<bool> _AsyncExecTask;




	private static readonly TimeSpan S_SleepTimeout = new TimeSpan(0, 0, 0, 0, 50);





	protected TransientSettings ExecLiveSettings => _ExecLiveSettings;

	protected QueryManager QryMgr { get; set; }


	public event ScriptExecutionCompletedEventHandler ExecutionCompletedEvent;
	public event QESQLBatchExecutedEventHandler BatchExecutionCompletedEvent;
	public event QESQLStartingBatchEventHandler StartingBatchExecutionEvent;

	public event QESQLStatementCompletedEventHandler StatementCompletedEvent;




	public AbstractQESQLExec(QueryManager qryMgr)
	{
		QryMgr = qryMgr;
		_CurBatch = new(QryMgr)
		{
			NoResultsExpected = false
		};
	}

	public void AsyncExecuteQuery(IBTextSpan textSpan, EnSqlExecutionType executionType, IDbConnection conn,
		IBQESQLBatchConsumer batchConsumer, TransientSettings sqlLiveSettings)
	{
		if (_AsyncExecState != EnLauncherPayloadLaunchState.Inactive)
		{
			InvalidOperationException ex = new(ControlsResources.ExecutionNotCompleted);
			Diag.ThrowException(ex);
		}


		_Conn = conn;
		_BatchConsumer = batchConsumer;
		_ExecTimeout = sqlLiveSettings.EditorExecutionTimeout;
		// Tracer.Trace(GetType(), "AsyncExecuteQuery()", " execOptions.EstimatedPlanOnly: " + execOptions.EstimatedPlanOnly);
		_ExecLiveSettings = sqlLiveSettings.Clone() as TransientSettings;
		_TextSpan = textSpan;
		_ExecutionType = executionType;
		_SpecialActions = EnSqlSpecialActions.None;


		_AsyncExecState = EnLauncherPayloadLaunchState.Pending;

		_AsyncExecCancelToken = default;
		_AsyncExecCancelTokenSource?.Dispose();
		_AsyncExecCancelTokenSource = new();
		_AsyncExecCancelToken = _AsyncExecCancelTokenSource.Token;

		_AsyncLaunchCancelToken = default;
		_AsyncLaunchCancelTokenSource?.Dispose();
		_AsyncLaunchCancelTokenSource = new();
		_AsyncLaunchCancelToken = _AsyncLaunchCancelTokenSource.Token;


		// The following for brevity.
		TaskCreationOptions creationOptions = TaskCreationOptions.PreferFairness
			| TaskCreationOptions.AttachedToParent;

		Task<bool> payloadAsync() =>
			ExecuteQueryAsync(_AsyncExecCancelToken);

		// Tracer.Trace(GetType(), "AsyncExecuteQuery()", "Launching ExecuteQueryAsync().");

		// Fire and remember

		// ------------------------------------------------------------------------- //
		// ******************** Execution Point (3) - AbstractQESQLExec.AsyncExecuteQuery() ******************** //
		// ------------------------------------------------------------------------- //

		_AsyncExecTask = Task.Factory.StartNew(payloadAsync, _AsyncLaunchCancelToken, creationOptions, TaskScheduler.Default).Unwrap();

		/*
		_ExecState = EnExecState.Initial;

		_ExecThread = new(ExecuteQueryThread)
		{
			CurrentCulture = CultureInfo.CurrentCulture,
			CurrentUICulture = CultureInfo.CurrentUICulture,
			Name = "Batch Execution"
		};


		_ExecThread.Start();
		*/

		// Tracer.Trace(GetType(), "AsyncExecuteQuery()", "ExecuteQueryAsync() Launched.");

	}


	[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD002:Avoid problematic synchronous waits", Justification = "<Pending>")]
	public void Cancel(TimeSpan maxTimeSpan)
	{
		// Tracer.Trace(GetType(), "QESQLExec.Cancel", "", null);
		if (_AsyncExecTask == null || _AsyncExecTask.IsCompleted
			|| _AsyncExecCancelTokenSource == null || _AsyncExecCancelTokenSource.IsCancellationRequested)
		{
			return;
		}

		_AsyncExecCancelTokenSource.Cancel();

		if (_AsyncExecState == EnLauncherPayloadLaunchState.Pending)
		{
			_AsyncLaunchCancelTokenSource.Cancel();
			_AsyncExecTask = null;
			return;
		}


		long startTimeEpoch = DateTime.Now.UnixMilliseconds();
		long currentTimeEpoch = startTimeEpoch;
		long maxTimeout = (long)maxTimeSpan.TotalMilliseconds;

		// Tracer.Trace(GetType(), "QESQLExec.Cancel", "maxTimeOut: {0}.", maxTimeout);

		_ = Task.Factory.StartNew(() =>
		{
			while (_AsyncExecState != EnLauncherPayloadLaunchState.Inactive && _AsyncExecTask != null && !_AsyncExecTask.IsCompleted)
			{
				if (currentTimeEpoch - startTimeEpoch > maxTimeout)
				{
					Tracer.Warning(GetType(), "Cancel()", "Timed out waiting for AsyncExecTask to complete. Forgetting about it. Timeout (ms): {0}.", currentTimeEpoch - startTimeEpoch);

					HookupBatchWithConsumer(_CurBatch, _BatchConsumer, false);

					_AsyncExecState = EnLauncherPayloadLaunchState.Discarded;
					// _AsyncExecCancelToken = default;
					// _AsyncExecCancelTokenSource = null;
					_AsyncExecTask = null;

					return;

				}

				Application.DoEvents();

				try
				{
					_AsyncExecTask.Wait(S_SleepTimeout);
				}
				catch { }

				currentTimeEpoch = DateTime.Now.UnixMilliseconds();

			}

			// Tracer.Trace(GetType(), Tracer.EnLevel.Information, "QESQLExec.Cancel: thread stopped gracefully", "", null);

			/*
			if (_Conn != null)
			{
				try
				{
					_Conn.Close();
				}
				catch
				{
				}
			}
			*/
		}, default, TaskCreationOptions.PreferFairness, TaskScheduler.Default);

		// Tracer.Trace(GetType(), "QESQLExec.Cancel", "CANCELLATION LAUNCHED.");

	}



	public void Dispose()
	{
		// Tracer.Trace(GetType(), "QESQLExec.Dispose", "", null);
		Dispose(bDisposing: true);
	}

	private async Task<bool> ExecuteQueryAsync(CancellationToken cancelToken)
	{
		// Tracer.Trace(GetType(), "ExecuteQueryAsync()");

		if (cancelToken == default || cancelToken.IsCancellationRequested)
			return false;

		_AsyncExecState = EnLauncherPayloadLaunchState.Launching;

		try
		{
			ProcessExecOptions(_Conn);

			_ExecOptionHasBeenChanged = false;
			_ExecResult = EnScriptExecutionResult.Failure;
			_CurBatchIndex = 0;
			_CurBatch.ExecTimeout = _ExecTimeout;
			_CurBatch.ResetTotal();

			_CurBatch.SetSuppressProviderMessageHeaders(ExecLiveSettings.SuppressProviderMessageHeaders);

			HookupBatchWithConsumer(_CurBatch, _BatchConsumer, true);

			lock (_LockObject)
			{
				if (cancelToken.IsCancellationRequested)
				{
					OnExecutionCompleted(EnScriptExecutionResult.Cancel);
					return false;
				}
			}

			_ExecResult = EnScriptExecutionResult.Success;



			// ------------------------------------------------------------------------------- //
			// ******************** Execution Point (4) - AbstractQESQLExec.ExecuteQueryAsync() ******************** //
			// ------------------------------------------------------------------------------- //
			if (_ExecResult == EnScriptExecutionResult.Success)
				await ExecuteScriptAsync(cancelToken);

			// Tracer.Trace(GetType(), "ExecuteQueryAsync()", "ExecuteScriptAsync() Completed. _ExecResult: {0}, _AsyncExecState: {1}.", _ExecResult, _AsyncExecState);


			bool discarded = false;

			lock (_LockObject)
				discarded = _AsyncExecState == EnLauncherPayloadLaunchState.Discarded;


			if (discarded)
			{
				Tracer.Warning(GetType(), "ExecuteScriptAsync()", "Execution was discarded.");
				Cleanup();

				if (_Conn != null && _Conn.State == ConnectionState.Open)
				{
					try
					{
						_Conn.Close();
					}
					catch
					{
					}
				}

				return false;
			}

			// Tracer.Trace(GetType(), "ExecuteQueryAsync()", "execution was NOT discarded");
			// EstimatedExecutionPlan = gets the plan without executing the script. MS still seem to execute the script.
			// We have bypassed that and get the plan at ExecuteReader above.
			// ActualExecutionPlan = the WithExecutionPlan toggle is latched so we can get the actual because
			// ExecuteReader has been called.
			if (_ExecResult == EnScriptExecutionResult.Success
				&& ExecLiveSettings.ExecutionType != EnSqlExecutionType.PlanOnly
				&& ExecLiveSettings.WithActualPlan)
			{
				PostProcessSqlCeExecutionPlan();
			}


			if (_ExecOptionHasBeenChanged)
				ProcessExecOptions(_Conn);

			if (_ExecResult == EnScriptExecutionResult.Halted)
				_ExecResult = EnScriptExecutionResult.Failure;
		}
		catch (Exception e)
		{
			Diag.Dug(e);
			_ExecResult = EnScriptExecutionResult.Failure;
		}


		try
		{
			OnExecutionCompleted(_ExecResult);
		}
		catch (Exception e)
		{
			Diag.Dug(e);
			_ExecResult = EnScriptExecutionResult.Failure;
			throw;
		}
		finally
		{
			_AsyncExecCancelToken = default;
			_AsyncExecCancelTokenSource?.Dispose();
			_AsyncExecCancelTokenSource = null;
			_AsyncExecState = EnLauncherPayloadLaunchState.Inactive;
		}

		// Tracer.Trace(GetType(), "ExecuteQueryAsync()", "Completed.");

		return true;
	}



	private void PostProcessSqlCeExecutionPlan()
	{
		// Tracer.Trace(GetType(), "QESQLExec.PostProcessSqlCeExecutionPlan", "", null);
	}



	/// <summary>
	/// We handle execution plan settings programmatically through FBCommand so these
	/// SET commands won't apply even though they exist in SqlResources.
	/// We just clear the batch collection after setting them for brevity.
	/// </summary>
	/// <param name="dbConnection"></param>
	protected void ProcessExecOptions(IDbConnection dbConnection)
	{
		// Tracer.Trace(GetType(), "ProcessExecOptions()");

		_ExecOptionHasBeenChanged = true;

		_SpecialActions = EnSqlSpecialActions.None;

		if (_ExecLiveSettings.ExecutionType == EnSqlExecutionType.PlanOnly)
			_SpecialActions |= EnSqlSpecialActions.EstimatedPlanOnly;
		else if (_ExecLiveSettings.ExecutionType == EnSqlExecutionType.QueryWithPlan)
			_SpecialActions |= EnSqlSpecialActions.ActualPlanIncluded;

		if (_ExecLiveSettings.ExecutionType != EnSqlExecutionType.PlanOnly
			&& _ExecLiveSettings.TtsEnabled && !QryMgr.ConnectionStrategy.TtsActive)
		{
			IDbTransaction transaction = dbConnection.BeginTransaction(_ExecLiveSettings.EditorExecutionIsolationLevel);
			QryMgr.ConnectionStrategy.Transaction = transaction;
		}

		/*
			This is a sample of what used to be here but isql is not available to us.

			CleanupBatchCollection(_SetConnectionOptionsBatches);
			CleanupBatchCollection(_RestoreConnectionOptionsBatches);
			...
			string cmd1, cmd2, cmd3, cmd4;

			if (ExecLiveSettings.EstimatedPlanOnly)
			{
				cmd1 = ExecLiveSettings.EditorExecutionSetPlanXml.SqlCmd(true);
				...
			}
			else if (ExecLiveSettings.WithExecutionPlan)
			{
				...
			}
			...
		*/
	}



	protected async Task<bool> ProcessBatchStatementRepetitionAsync(IBsNativeDbStatementWrapper sqlStatement, CancellationToken cancelToken)
	{
		// Tracer.Trace(GetType(), "ProcessBatchStatementRepetitionAsync()", " ExecLiveSettings.EstimatedPlanOnly: " + ExecLiveSettings.EstimatedPlanOnly);

		bool continueProcessing = true;

		if (sqlStatement == null)
		{
			_ExecResult |= EnScriptExecutionResult.Success;
			return continueProcessing;
		}


		lock (_LockObject)
		{
			if (cancelToken.IsCancellationRequested)
			{
				_ExecResult = EnScriptExecutionResult.Cancel;
			}
			else
			{
				_CurBatch.Reset();
				_CurBatch.SqlStatement = sqlStatement;
				// _CurBatch.SqlScript = sqlScript;
				// _CurBatch.TextSpan = textSpan;
				_CurBatch.BatchIndex = _CurBatchIndex;
				_CurBatchIndex++;
				_AsyncExecState = EnLauncherPayloadLaunchState.Launching;
			}
		}

		EnScriptExecutionResult scriptExecutionResult = EnScriptExecutionResult.Failure;
		if (_ExecResult != EnScriptExecutionResult.Cancel)
		{
			bool discarded = false;
			try
			{
				OnStartBatchExecution(_CurBatch);

				// ---------------------------------------------------------------------------------------------- //
				// ******************** Execution Point (8) - AbstractQESQLExec.ProcessBatchStatementRepetitionAsync() ******************** //
				// ---------------------------------------------------------------------------------------------- //
				scriptExecutionResult = await ExecuteBatchRepetitionAsync(_CurBatch, cancelToken);
			}
			finally
			{
				lock (_LockObject)
				{
					// Tracer.Trace(GetType(), "ProcessBatchStatementRepetitionAsync()", "execState = {0}", _ExecState);
					if (cancelToken.IsCancellationRequested || _AsyncExecState == EnLauncherPayloadLaunchState.Discarded)
					{
						scriptExecutionResult = EnScriptExecutionResult.Cancel;
					}
					else
					{
						_AsyncExecState = EnLauncherPayloadLaunchState.Launching;
					}

					discarded = _AsyncExecState == EnLauncherPayloadLaunchState.Discarded;
				}
			}

			if (!discarded)
			{
				OnBatchExecutionCompleted(_CurBatch, scriptExecutionResult);
			}
		}
		else
		{
			scriptExecutionResult = EnScriptExecutionResult.Cancel;
		}

		if (scriptExecutionResult == EnScriptExecutionResult.Cancel
			|| scriptExecutionResult == EnScriptExecutionResult.Halted)
		{
			_ExecResult = scriptExecutionResult;
			continueProcessing = false;
		}
		else
		{
			_ExecResult |= scriptExecutionResult;
		}

		return continueProcessing;
	}



	protected void HookupBatchWithConsumer(QESQLBatch batch, IBQESQLBatchConsumer batchConsumer, bool bHookUp)
	{
		// Tracer.Trace(GetType(), "QESQLExec.HookupBatchWithConsumer", "bHookUp = {0}", bHookUp);
		if (batch != null && batchConsumer != null)
		{
			if (!bHookUp)
			{
				batch.ErrorMessageEvent -= batchConsumer.OnErrorMessage;
				batch.MessageEvent -= batchConsumer.OnMessage;
				batch.NewResultSetEventAsync -= batchConsumer.OnNewResultSetAsync;
				batch.CancellingEvent -= batchConsumer.OnCancelling;
				batch.FinishedResultSetEvent -= batchConsumer.OnFinishedProcessingResultSet;
				batch.SpecialActionEvent -= batchConsumer.OnSpecialAction;
				batch.StatementCompletedEvent -= batchConsumer.OnStatementCompleted;
				batch.StatementCompletedEvent -= OnStatementCompleted;
			}
			else
			{
				batch.ErrorMessageEvent += batchConsumer.OnErrorMessage;
				batch.MessageEvent += batchConsumer.OnMessage;
				batch.NewResultSetEventAsync += batchConsumer.OnNewResultSetAsync;
				batch.CancellingEvent += batchConsumer.OnCancelling;
				batch.FinishedResultSetEvent += batchConsumer.OnFinishedProcessingResultSet;
				batch.SpecialActionEvent += batchConsumer.OnSpecialAction;
				batch.StatementCompletedEvent += OnStatementCompleted;
				batch.StatementCompletedEvent += batchConsumer.OnStatementCompleted;
			}
		}
	}

	protected abstract Task<bool> ExecuteScriptAsync(CancellationToken cancelToken);

	protected abstract Task<EnScriptExecutionResult> ExecuteBatchRepetitionAsync(QESQLBatch batch, CancellationToken cancelToken);

	protected virtual void OnStartBatchExecution(QESQLBatch batch)
	{
		// Tracer.Trace(GetType(), "QESQLExec.OnStartBatchExecution", "m_curBatchIndex = {0}", _CurBatchIndex);
		StartingBatchExecutionEvent?.Invoke(this, new QESQLStartingBatchEventArgs(-1, batch));
	}


	protected virtual void OnBatchExecutionCompleted(QESQLBatch batch, EnScriptExecutionResult batchResult)
	{
		// Tracer.Trace(GetType(), "QESQLExec.OnBatchExecutionCompleted", "m_curBatchIndex = {0}, batchResult = {1}, _ExecState = {2}", _CurBatchIndex, batchResult, _ExecState);
		if (BatchExecutionCompletedEvent != null)
		{
			EnSqlExecutionType executionType = EnSqlExecutionType.QueryOnly;
			if (ExecLiveSettings != null)
				executionType = ExecLiveSettings.ExecutionType;

			BatchExecutionCompletedEvent(this, new(batchResult, batch, executionType));
		}
	}


	public virtual void OnStatementCompleted(object sender, QESQLStatementCompletedEventArgs args)
	{
		// Tracer.Trace(GetType(), "AbstractQESQLExec.OnStatementCompleted", "sender: {0}, args.RecordCount: {1}", sender, args.RecordCount);

		// Added for StaticsPanel.RetrieveStatisticsIfNeeded();
		StatementCompletedEvent?.Invoke(sender, args);
	}


	protected virtual void OnExecutionCompleted(EnScriptExecutionResult execResult)
	{
		// Tracer.Trace(GetType(), "OnExecutionCompleted()", "execResult = {0}", execResult);

		EnSqlExecutionType executionType = EnSqlExecutionType.QueryOnly;

		if (ExecLiveSettings != null)
			executionType = ExecLiveSettings.ExecutionType;

		try
		{
			HookupBatchWithConsumer(_CurBatch, _BatchConsumer, bHookUp: false);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
		}

		try
		{
			Cleanup();
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
		}


		try
		{
			ExecutionCompletedEvent?.Invoke(this, new ScriptExecutionCompletedEventArgs(execResult, executionType));
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
		}
	}


	protected virtual void Cleanup()
	{
		// Tracer.Trace(GetType(), "QESQLExec.Cleanup", "", null);
		lock (_LockObject)
			_AsyncExecState = EnLauncherPayloadLaunchState.Inactive;

		// CleanupBatchCollection(_SetConnectionOptionsBatches);
		// CleanupBatchCollection(_RestoreConnectionOptionsBatches);

		_CurBatchIndex = -1;
		if (_CurBatch != null && _BatchConsumer != null)
		{
			HookupBatchWithConsumer(_CurBatch, _BatchConsumer, bHookUp: false);
		}

		_BatchConsumer = null;
		_ExecLiveSettings = null;

	}

	protected virtual void Dispose(bool bDisposing)
	{
		// Tracer.Trace(GetType(), "QESQLExec.Dispose", "bDisposing = {0}", bDisposing);
		if (bDisposing)
		{
			Cleanup();
			if (_CurBatch != null)
			{
				_CurBatch.Dispose();
				_CurBatch = null;
			}

			_LockObject = null;
		}
	}
}
