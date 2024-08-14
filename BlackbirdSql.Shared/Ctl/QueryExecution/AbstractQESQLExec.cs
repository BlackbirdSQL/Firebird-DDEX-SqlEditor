// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution.QESQLExec

using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using BlackbirdSql.Core.Enums;
using BlackbirdSql.Shared.Ctl.Config;
using BlackbirdSql.Shared.Enums;
using BlackbirdSql.Shared.Events;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Shared.Model;
using BlackbirdSql.Shared.Properties;
using BlackbirdSql.Sys.Enums;
using BlackbirdSql.Sys.Interfaces;
using EnvDTE;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;



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
		_QryMgr = qryMgr;

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

	private EnLauncherPayloadLaunchState _AsyncExecState = EnLauncherPayloadLaunchState.Inactive;
	private Task<bool> _AsyncExecTask;
	protected IBsQESQLBatchConsumer _BatchConsumer;
	protected IDbConnection _Conn;
	private QESQLBatch _CurBatch;
	private int _CurBatchIndex = -1;
	private bool _ExecOptionHasChanged;
	protected EnSqlExecutionType _ExecutionType;
	private TransientSettings _ExecLiveSettings;
	protected EnScriptExecutionResult _ExecResult = EnScriptExecutionResult.Failure;
	private int _ExecTimeout;
	private bool _Hooked = false;
	private readonly QueryManager _QryMgr = null;
	protected EnSpecialActions _SpecialActions;
	protected IBsTextSpan _TextSpan;


	#endregion Fields





	// =========================================================================================================
	#region Property Accessors - AbstractQESQLExec
	// =========================================================================================================


	public EnLauncherPayloadLaunchState AsyncExecState => _AsyncExecState;
	public Task<bool> AsyncExecTask => _AsyncExecTask;
	protected TransientSettings ExecLiveSettings => _ExecLiveSettings;

	protected QueryManager QryMgr => _QryMgr;


	public event BatchExecutionCompletedEventHandler BatchExecutionCompletedEventAsync;
	public event BatchExecutionStartEventHandler BatchExecutionStartEvent;
	public event ExecutionCompletedEventHandler ExecutionCompletedEventAsync;
	public event BatchStatementCompletedEventHandler BatchStatementCompletedEvent;


	#endregion Property Accessors





	// =========================================================================================================
	#region Methods - AbstractQESQLExec
	// =========================================================================================================


	public async Task<bool> AsyncExecuteAsync(IBsTextSpan textSpan, EnSqlExecutionType executionType,
		IDbConnection conn, IBsQESQLBatchConsumer batchConsumer, TransientSettings sqlLiveSettings,
		CancellationToken cancelToken, CancellationToken syncToken)
	{
		if (_AsyncExecState != EnLauncherPayloadLaunchState.Inactive)
		{
			InvalidOperationException ex = new(Resources.ExExecutionNotCompleted);
			Diag.ThrowException(ex);
		}

		_Conn = conn;
		_BatchConsumer = batchConsumer;
		_ExecTimeout = sqlLiveSettings.EditorExecutionTimeout;
		_ExecLiveSettings = sqlLiveSettings.Clone() as TransientSettings;
		_TextSpan = textSpan;
		_ExecutionType = executionType;
		_SpecialActions = EnSpecialActions.None;


		_AsyncExecState = EnLauncherPayloadLaunchState.Pending;


		// Fire and remember

		// ------------------------------------------------------------------------- //
		// ******** Execution Point (3) - AbstractQESQLExec.ExecuteAsync() ********* //
		// ------------------------------------------------------------------------- //


		_AsyncExecTask = Task.Run(() => ExecuteAsync(cancelToken, syncToken));


		// Tracer.Trace(GetType(), "ExecuteAsync()", "ExecuteAsync() Launched.");

		return await Cmd.AwaitableAsync(true);
	}



	protected virtual void Cleanup()
	{
		// Tracer.Trace(GetType(), "QESQLExec.Cleanup", "", null);
		lock (_LockObject)
			_AsyncExecState = EnLauncherPayloadLaunchState.Inactive;

		// CleanupBatchCollection(_SetConnectionOptionsBatches);
		// CleanupBatchCollection(_RestoreConnectionOptionsBatches);

		_CurBatchIndex = -1;

		HookupBatchConsumer(_CurBatch, _BatchConsumer, false);

		_BatchConsumer = null;
		_ExecLiveSettings = null;

	}



	private async Task<bool> ExecuteAsync(CancellationToken cancelToken, CancellationToken syncToken)
	{
		// Tracer.Trace(GetType(), "ExecuteAsync()");

		bool result = false;

		try
		{
			result = await ExecuteImplAsync(cancelToken, syncToken);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);

			_ExecResult = EnScriptExecutionResult.Failure;
		}
		finally
		{
			if (!syncToken.IsCancellationRequested)
				await QryMgr.Strategy.VerifyOpenConnectionAsync(default);

			await RaiseExecutionCompletedAsync(_ExecResult, true, cancelToken, syncToken);

			_AsyncExecState = EnLauncherPayloadLaunchState.Inactive;
		}

		return result;
	}



	private async Task<bool> ExecuteImplAsync(CancellationToken cancelToken, CancellationToken syncToken)
	{
		// Tracer.Trace(GetType(), "ExecuteAsync()");
		if (cancelToken.IsCancellationRequested)
		{
			_ExecResult = EnScriptExecutionResult.Cancel;

			return false;
		}

		_AsyncExecState = EnLauncherPayloadLaunchState.Launching;

		ProcessExecOptions(_Conn);

		_ExecOptionHasChanged = false;
		_ExecResult = EnScriptExecutionResult.Failure;
		_CurBatchIndex = 0;
		_CurBatch.ExecTimeout = _ExecTimeout;
		_CurBatch.ResetTotal();

		_CurBatch.SetSuppressProviderMessageHeaders(ExecLiveSettings.SuppressProviderMessageHeaders);

		HookupBatchConsumer(_CurBatch, _BatchConsumer, true);

		if (cancelToken.IsCancellationRequested)
		{
			_ExecResult = EnScriptExecutionResult.Cancel;

			return false;
		}

		_ExecResult = EnScriptExecutionResult.Success;



		// ------------------------------------------------------------------------------- //
		// ************* Execution Point (4) - AbstractQESQLExec.ExecuteAsync() ********** //
		// ------------------------------------------------------------------------------- //
		if (_ExecResult == EnScriptExecutionResult.Success)
			await ExecuteScriptAsync(cancelToken, syncToken);

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

			_ExecResult = EnScriptExecutionResult.Halted;

			return false;
		}


		if (_ExecOptionHasChanged && !syncToken.IsCancellationRequested)
			ProcessExecOptions(_Conn);

		if (_ExecResult == EnScriptExecutionResult.Halted)
			_ExecResult = EnScriptExecutionResult.Failure;



		// Tracer.Trace(GetType(), "ExecuteAsync()", "Completed.");

		return true;
	}



	protected abstract Task<EnScriptExecutionResult> ExecuteBatchStatementAsync(QESQLBatch batch,
		CancellationToken cancelToken, CancellationToken syncToken);



	protected abstract Task<bool> ExecuteScriptAsync(CancellationToken cancelToken, CancellationToken syncToken);



	private void HookupBatchConsumer(QESQLBatch batch, IBsQESQLBatchConsumer batchConsumer, bool hookup)
	{
		// Tracer.Trace(GetType(), "QESQLExec.HookupBatchWithConsumer", "bHookUp = {0}", bHookUp);
		if (batch != null && batchConsumer != null)
		{
			if (!hookup && _Hooked)
			{
				_Hooked = false;

				batch.ErrorMessageEvent -= batchConsumer.OnErrorMessage;
				batch.MessageEvent -= batchConsumer.OnMessage;
				batch.NewResultSetEventAsync -= batchConsumer.OnNewResultSetAsync;
				batch.CancellingEvent -= batchConsumer.OnCancelling;
				batch.FinishedResultSetEvent -= batchConsumer.OnFinishedProcessingResultSet;
				batch.SpecialActionEvent -= batchConsumer.OnSpecialAction;
				batch.StatementCompletedEvent -= batchConsumer.OnStatementCompleted;
				batch.StatementCompletedEvent -= OnBatchStatementCompleted;
			}
			else if (!_Hooked)
			{
				_Hooked = true;

				batch.ErrorMessageEvent += batchConsumer.OnErrorMessage;
				batch.MessageEvent += batchConsumer.OnMessage;
				batch.NewResultSetEventAsync += batchConsumer.OnNewResultSetAsync;
				batch.CancellingEvent += batchConsumer.OnCancelling;
				batch.FinishedResultSetEvent += batchConsumer.OnFinishedProcessingResultSet;
				batch.SpecialActionEvent += batchConsumer.OnSpecialAction;
				batch.StatementCompletedEvent += OnBatchStatementCompleted;
				batch.StatementCompletedEvent += batchConsumer.OnStatementCompleted;
			}
		}
	}



	protected async Task<bool> ProcessBatchStatementAsync(IBsNativeDbStatementWrapper sqlStatement,
		CancellationToken cancelToken, CancellationToken syncToken)
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
				// *********** Execution Point (8) - AbstractQESQLExec.ProcessBatchStatementAsync() ************* //
				// ---------------------------------------------------------------------------------------------- //
				scriptExecutionResult = await ExecuteBatchStatementAsync(_CurBatch, cancelToken, syncToken);
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
				await OnBatchExecutionCompletedAsync(_CurBatch, scriptExecutionResult, cancelToken, syncToken);
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

		_SpecialActions = EnSpecialActions.None;

		if (_ExecLiveSettings.ExecutionType == EnSqlExecutionType.PlanOnly)
			_SpecialActions |= EnSpecialActions.EstimatedPlanOnly;
		else if (_ExecLiveSettings.ExecutionType == EnSqlExecutionType.QueryWithPlan)
			_SpecialActions |= EnSpecialActions.ActualPlanIncluded;
	}


	#endregion Methods





	// =========================================================================================================
	#region Event Handling - AbstractQESQLExec
	// =========================================================================================================



	private async Task OnBatchExecutionCompletedAsync(QESQLBatch batch, EnScriptExecutionResult batchResult,
		CancellationToken cancelToken, CancellationToken syncToken)
	{
		// Tracer.Trace(GetType(), "OnBatchExecutionCompletedAsync()", "m_curBatchIndex = {0}, batchResult = {1}, _ExecState = {2}", _CurBatchIndex, batchResult, _ExecState);
		if (BatchExecutionCompletedEventAsync != null)
		{
			EnSqlExecutionType executionType = EnSqlExecutionType.QueryOnly;
			if (ExecLiveSettings != null)
				executionType = ExecLiveSettings.ExecutionType;

			await Cmd.AwaitableAsync();

			if (batchResult == EnScriptExecutionResult.Cancel)
				QryMgr.AsyncCancelTokenSource.Cancel();

			BatchExecutionCompletedEventArgs args = new(batchResult, batch, executionType, cancelToken, syncToken);

			_ = BatchExecutionCompletedEventAsync.RaiseEventAsync(this, args);
		}
	}


	public virtual void OnBatchStatementCompleted(object sender, BatchStatementCompletedEventArgs args)
	{
		// Tracer.Trace(GetType(), "OnBatchStatementCompleted()", "sender: {0}, args.RecordCount: {1}", sender, args.RecordCount);

		// Added for StaticsPanel.RetrieveStatisticsIfNeeded();
		BatchStatementCompletedEvent?.Invoke(sender, args);
	}


	private void OnBatchExecutionStart(QESQLBatch batch)
	{
		// Tracer.Trace(GetType(), "QESQLExec.OnStartBatchExecution", "m_curBatchIndex = {0}", _CurBatchIndex);
		BatchExecutionStartEvent?.Invoke(this, new BatchExecutionStartEventArgs(-1, batch));
	}



	protected async virtual Task<bool> RaiseExecutionCompletedAsync(EnScriptExecutionResult execResult,
		bool launched, CancellationToken cancelToken, CancellationToken syncToken)
	{
		// Tracer.Trace(GetType(), "OnExecutionCompletedAsync()", "execResult = {0}", execResult);

		EnSqlExecutionType executionType = EnSqlExecutionType.QueryOnly;
		EnSqlOutputMode outputMode = EnSqlOutputMode.ToGrid;

		if (ExecLiveSettings != null)
		{
			executionType = ExecLiveSettings.ExecutionType;
			outputMode = ExecLiveSettings.EditorResultsOutputMode;
		}

		int errorCount = _BatchConsumer?.CurrentErrorCount ?? 0;
		int messageCount = _BatchConsumer?.CurrentMessageCount ?? 0;

		bool result = true;

		try
		{
			Cleanup();
		}
		catch (Exception ex)
		{
			result = false;
			Diag.Dug(ex);
		}


		ExecutionCompletedEventArgs args = new(execResult, executionType, outputMode, launched,
			QryMgr.IsWithClientStats, QryMgr.RowsAffected, QryMgr.StatementCount, errorCount,
			messageCount, cancelToken, syncToken);

		try
		{
			ExecutionCompletedEventAsync?.RaiseEventAsync(this, args);
		}
		catch (Exception ex)
		{
			result = false;
			Diag.Dug(ex);
		}

		args.Result &= result;

		return await Cmd.AwaitableAsync(result);
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
