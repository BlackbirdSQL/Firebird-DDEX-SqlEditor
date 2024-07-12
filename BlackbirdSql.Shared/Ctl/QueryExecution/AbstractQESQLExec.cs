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


// =========================================================================================================
//										AbstractQESQLExec Class
//
/// <summary>
/// Abstract query executor class.
/// </summary>
// =========================================================================================================
public abstract class AbstractQESQLExec : IDisposable
{

	// ---------------------------------------------------
	#region Constructors / Destructors - AbstractQESQLExec
	// ---------------------------------------------------


	public AbstractQESQLExec(QueryManager qryMgr)
	{
		QryMgr = qryMgr;
		_CurBatch = new(QryMgr)
		{
			NoResultsExpected = false
		};
	}


	public void Dispose()
	{
		// Tracer.Trace(GetType(), "QESQLExec.Dispose", "", null);
		Dispose(true);
	}


	protected virtual void Dispose(bool disposing)
	{
		// Tracer.Trace(GetType(), "QESQLExec.Dispose", "bDisposing = {0}", bDisposing);
		if (!disposing)
			return;

		Cleanup();
		if (_CurBatch != null)
		{
			_CurBatch.Dispose();
			_CurBatch = null;
		}
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - AbstractQESQLExec
	// =========================================================================================================


	// A protected 'this' object lock
	protected readonly object _LockObject = new object();

	private const int _SleepWaitTimeout = 50;

	private CancellationToken _AsyncExecCancelToken;
	private CancellationTokenSource _AsyncExecCancelTokenSource = null;
	private EnLauncherPayloadLaunchState _AsyncExecState = EnLauncherPayloadLaunchState.Inactive;
	private Task<bool> _AsyncExecTask;
	private CancellationToken _AsyncLaunchCancelToken;
	private CancellationTokenSource _AsyncLaunchCancelTokenSource = null;
	protected IBQESQLBatchConsumer _BatchConsumer;
	protected IDbConnection _Conn;
	private QESQLBatch _CurBatch;
	private int _CurBatchIndex = -1;
	private bool _ExecOptionHasChanged;
	protected EnSqlExecutionType _ExecutionType;
	private TransientSettings _ExecLiveSettings;
	protected EnScriptExecutionResult _ExecResult = EnScriptExecutionResult.Failure;
	private int _ExecTimeout;
	protected EnSqlSpecialActions _SpecialActions;
	protected IBTextSpan _TextSpan;


	#endregion Fields





	// =========================================================================================================
	#region Property Accessors - AbstractQESQLExec
	// =========================================================================================================


	protected TransientSettings ExecLiveSettings => _ExecLiveSettings;

	protected QueryManager QryMgr { get; set; }



	public event QESQLBatchExecutionCompletedEventHandler BatchExecutionCompletedEvent;
	public event QESQLBatchExecutionStartEventHandler BatchExecutionStartEvent;
	public event ScriptExecutionCompletedEventHandler ExecutionCompletedEvent;
	public event QESQLStatementCompletedEventHandler StatementCompletedEvent;


	#endregion Property Accessors





	// =========================================================================================================
	#region Methods - AbstractQESQLExec
	// =========================================================================================================


	public void AsyncExecute(IBTextSpan textSpan, EnSqlExecutionType executionType, IDbConnection conn,
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
			ExecuteAsync(_AsyncExecCancelToken);

		// Tracer.Trace(GetType(), "AsyncExecute()", "Launching ExecuteAsync().");

		// Fire and remember

		// ------------------------------------------------------------------------- //
		// ******************** Execution Point (3) - AbstractQESQLExec.AsyncExecute() ******************** //
		// ------------------------------------------------------------------------- //

		_AsyncExecTask = Task.Factory.StartNew(payloadAsync, _AsyncLaunchCancelToken, creationOptions, TaskScheduler.Default).Unwrap();


		// Tracer.Trace(GetType(), "AsyncExecute()", "ExecuteAsync() Launched.");

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
			_AsyncLaunchCancelTokenSource?.Cancel();
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

					HookupBatchConsumer(_CurBatch, _BatchConsumer, false);

					_AsyncExecState = EnLauncherPayloadLaunchState.Discarded;
					// _AsyncExecCancelToken = default;
					// _AsyncExecCancelTokenSource = null;
					_AsyncExecTask = null;

					return;

				}

				Application.DoEvents();

				try
				{
					_AsyncExecTask.Wait(_SleepWaitTimeout);
				}
				catch { }

				currentTimeEpoch = DateTime.Now.UnixMilliseconds();

			}

			// Tracer.Trace(GetType(), Tracer.EnLevel.Information, "QESQLExec.Cancel: thread stopped gracefully", "", null);

		}, default, TaskCreationOptions.PreferFairness, TaskScheduler.Default);

		// Tracer.Trace(GetType(), "QESQLExec.Cancel", "CANCELLATION LAUNCHED.");

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
			HookupBatchConsumer(_CurBatch, _BatchConsumer, false);
		}

		_BatchConsumer = null;
		_ExecLiveSettings = null;

	}



	private async Task<bool> ExecuteAsync(CancellationToken cancelToken)
	{
		// Tracer.Trace(GetType(), "ExecuteAsync()");

		try
		{
			if (cancelToken == default || cancelToken.IsCancellationRequested)
				return false;

			_AsyncExecState = EnLauncherPayloadLaunchState.Launching;

			ProcessExecOptions(_Conn);

			_ExecOptionHasChanged = false;
			_ExecResult = EnScriptExecutionResult.Failure;
			_CurBatchIndex = 0;
			_CurBatch.ExecTimeout = _ExecTimeout;
			_CurBatch.ResetTotal();

			_CurBatch.SetSuppressProviderMessageHeaders(ExecLiveSettings.SuppressProviderMessageHeaders);

			HookupBatchConsumer(_CurBatch, _BatchConsumer, true);

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
			// ******************** Execution Point (4) - AbstractQESQLExec.ExecuteAsync() ******************** //
			// ------------------------------------------------------------------------------- //
			if (_ExecResult == EnScriptExecutionResult.Success)
				await ExecuteScriptAsync(cancelToken);

			// Tracer.Trace(GetType(), "ExecuteAsync()", "ExecuteScriptAsync() Completed. _ExecResult: {0}, _AsyncExecState: {1}.", _ExecResult, _AsyncExecState);


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


			if (_ExecOptionHasChanged)
				ProcessExecOptions(_Conn);

			if (_ExecResult == EnScriptExecutionResult.Halted)
				_ExecResult = EnScriptExecutionResult.Failure;
		}
		catch (Exception e)
		{
			Diag.Dug(e);
			_ExecResult = EnScriptExecutionResult.Failure;
		}
		finally
		{
			QryMgr.GetUpdateTransactionsStatus();
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

			QryMgr.GetUpdateTransactionsStatus();
		}

		// Tracer.Trace(GetType(), "ExecuteAsync()", "Completed.");

		return true;
	}



	protected abstract Task<EnScriptExecutionResult> ExecuteBatchStatementAsync(QESQLBatch batch, CancellationToken cancelToken);



	protected abstract Task<bool> ExecuteScriptAsync(CancellationToken cancelToken);



	private void HookupBatchConsumer(QESQLBatch batch, IBQESQLBatchConsumer batchConsumer, bool hookup)
	{
		// Tracer.Trace(GetType(), "QESQLExec.HookupBatchWithConsumer", "bHookUp = {0}", bHookUp);
		if (batch != null && batchConsumer != null)
		{
			if (!hookup)
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



	protected async Task<bool> ProcessBatchStatementAsync(IBsNativeDbStatementWrapper sqlStatement, CancellationToken cancelToken)
	{
		// Tracer.Trace(GetType(), "ProcessBatchStatementAsync()", " ExecLiveSettings.EstimatedPlanOnly: " + ExecLiveSettings.EstimatedPlanOnly);

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
				OnBatchExecutionStart(_CurBatch);

				// ---------------------------------------------------------------------------------------------- //
				// ******************** Execution Point (8) - AbstractQESQLExec.ProcessBatchStatementAsync() ******************** //
				// ---------------------------------------------------------------------------------------------- //
				scriptExecutionResult = await ExecuteBatchStatementAsync(_CurBatch, cancelToken);
			}
			finally
			{
				lock (_LockObject)
				{
					// Tracer.Trace(GetType(), "ProcessBatchStatementAsync()", "execState = {0}", _ExecState);
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
			|| scriptExecutionResult == EnScriptExecutionResult.Halted
			|| scriptExecutionResult == EnScriptExecutionResult.Failure)
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



	/// <summary>
	/// We handle execution plan settings programmatically through FBCommand so these
	/// SET commands won't apply even though they exist in SqlResources.
	/// We just clear the batch collection after setting them for brevity.
	/// </summary>
	/// <param name="dbConnection"></param>
	private void ProcessExecOptions(IDbConnection dbConnection)
	{
		// Tracer.Trace(GetType(), "ProcessExecOptions()");

		_ExecOptionHasChanged = true;

		_SpecialActions = EnSqlSpecialActions.None;

		if (_ExecLiveSettings.ExecutionType == EnSqlExecutionType.PlanOnly)
			_SpecialActions |= EnSqlSpecialActions.EstimatedPlanOnly;
		else if (_ExecLiveSettings.ExecutionType == EnSqlExecutionType.QueryWithPlan)
			_SpecialActions |= EnSqlSpecialActions.ActualPlanIncluded;
	}


	#endregion Methods





	// =========================================================================================================
	#region Event Handling - AbstractQESQLExec
	// =========================================================================================================


	private void OnBatchExecutionCompleted(QESQLBatch batch, EnScriptExecutionResult batchResult)
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


	private void OnBatchExecutionStart(QESQLBatch batch)
	{
		// Tracer.Trace(GetType(), "QESQLExec.OnStartBatchExecution", "m_curBatchIndex = {0}", _CurBatchIndex);
		BatchExecutionStartEvent?.Invoke(this, new QESQLBatchExecutionStartEventArgs(-1, batch));
	}



	protected virtual void OnExecutionCompleted(EnScriptExecutionResult execResult)
	{
		// Tracer.Trace(GetType(), "OnExecutionCompleted()", "execResult = {0}", execResult);

		EnSqlExecutionType executionType = EnSqlExecutionType.QueryOnly;

		if (ExecLiveSettings != null)
			executionType = ExecLiveSettings.ExecutionType;

		try
		{
			HookupBatchConsumer(_CurBatch, _BatchConsumer, false);
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



	public virtual void OnStatementCompleted(object sender, QESQLStatementCompletedEventArgs args)
	{
		// Tracer.Trace(GetType(), "AbstractQESQLExec.OnStatementCompleted", "sender: {0}, args.RecordCount: {1}", sender, args.RecordCount);

		// Added for StaticsPanel.RetrieveStatisticsIfNeeded();
		StatementCompletedEvent?.Invoke(sender, args);
	}


	#endregion Event Handling





	// =========================================================================================================
	#region Sub-Classes - AbstractQESQLExec
	// =========================================================================================================


	protected enum EnExecState
	{
		Initial,
		Executing,
		ExecutingBatch,
		Cancelling,
		Discarded
	}


	#endregion Sub-Classes
}
