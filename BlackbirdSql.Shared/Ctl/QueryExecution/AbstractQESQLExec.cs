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
using BlackbirdSql.Shared.Model.Parsers;
using BlackbirdSql.Shared.Properties;
using BlackbirdSql.Sys.Enums;
using BlackbirdSql.Sys.Interfaces;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;



namespace BlackbirdSql.Shared.Ctl.QueryExecution;


// =========================================================================================================
//										AbstractQESQLExec Class
//
/// <summary>
/// Abstract query executor class. Handles script processing. Post script multi-statement batch processing
/// is handled in descendent.
/// </summary>
// =========================================================================================================
public abstract class AbstractQESQLExec : IBsCommandExecuter, IDisposable
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
		// Evs.Trace(GetType(), "QESQLExec.Dispose", "", null);
		Dispose(true);
	}


	protected virtual bool Dispose(bool disposing)
	{
		// Evs.Trace(GetType(), "QESQLExec.Dispose", "bDisposing = {0}", bDisposing);
		if (!disposing)
			return false;

		Cleanup();

		if (_CurBatch != null)
		{
			_CurBatch.Dispose();
			_CurBatch = null;
		}


		if (_SqlCmdParser != null)
		{
			try
			{
				_SqlCmdParser.Dispose();
			}
			catch
			{
			}

			_SqlCmdParser = null;
			_CurrentConn = null;
			_Conn = null;
			// _CurrentConnInfo = null;
		}

		return true;

	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - AbstractQESQLExec
	// =========================================================================================================


	// A protected 'this' object lock
	protected readonly object _LockObject = new object();

	protected EnLauncherPayloadLaunchState _AsyncExecState = EnLauncherPayloadLaunchState.Inactive;
	private Task<bool> _AsyncExecTask;
	protected IBsQESQLBatchConsumer _BatchConsumer;
	protected IDbConnection _Conn;
	protected QESQLBatch _CurBatch;
	protected int _CurBatchIndex = -1;
	protected IDbConnection _CurrentConn;
	private bool _ExecOptionHasChanged;
	private EnSqlExecutionType _ExecutionType;
	private TransientSettings _ExecLiveSettings;
	protected EnScriptExecutionResult _ExecResult = EnScriptExecutionResult.Failure;
	private int _ExecTimeout;
	private readonly QueryManager _QryMgr = null;
	protected EnSpecialActions _SpecialActions;
	private ManagedBatchParser _SqlCmdParser;
	private IBsTextSpan _TextSpan;


	#endregion Fields





	// =========================================================================================================
	#region Property Accessors - AbstractQESQLExec
	// =========================================================================================================


	public EnLauncherPayloadLaunchState AsyncExecState => _AsyncExecState;
	public Task<bool> AsyncExecTask => _AsyncExecTask;
	protected TransientSettings ExecLiveSettings => _ExecLiveSettings;

	protected QueryManager QryMgr => _QryMgr;


	public event QueryDataEventHandler BatchScriptParsedEvent;
	public event ErrorMessageEventHandler ErrorMessageEvent;
	public event ExecutionCompletedEventHandler ExecutionCompletedEventAsync;


	#endregion Property Accessors





	// =========================================================================================================
	#region Methods - AbstractQESQLExec
	// =========================================================================================================


	public async Task<EnScriptExecutionResult> BatchParseCallbackAsync(IBsNativeDbBatchParser batchParser,
		CancellationToken cancelToken, CancellationToken syncToken)
	{
		// ------------------------------------------------------------------------------- //
		// ************ Execution Point (7) - QESQLExec.BatchParseCallback() ************* //
		// ------------------------------------------------------------------------------- //

		EnScriptExecutionResult result = _CurBatch.Parse(batchParser);

		if (result != EnScriptExecutionResult.Success)
			_ExecResult = result;

		RaiseScriptParsedEvent(cancelToken.Cancelled()
			? EnSqlStatementAction.Cancelled : batchParser.CurrentAction,
			batchParser.TotalRowsSelected, batchParser.StatementCount, cancelToken);

		return await Task.FromResult(result);
	}


	public abstract Task<EnParserAction> BatchStatementCallbackAsync(IBsNativeDbStatementWrapper sqlStatement,
		int numberOfTimes, CancellationToken cancelToken, CancellationToken syncToken);



	protected virtual void Cleanup()
	{
		// Evs.Trace(GetType(), nameof(Cleanup));

		lock (_LockObject)
			_AsyncExecState = EnLauncherPayloadLaunchState.Inactive;

		// CleanupBatchCollection(_SetConnectionOptionsBatches);
		// CleanupBatchCollection(_RestoreConnectionOptionsBatches);

		_ExecLiveSettings = null;
	}



	private async Task<bool> ExecuteAsync(CancellationToken cancelToken, CancellationToken syncToken)
	{
		// Evs.Trace(GetType(), nameof(ExecuteAsync));

		bool result = false;

		try
		{
			result = await ExecuteImplAsync(cancelToken, syncToken);
		}
		catch (Exception ex)
		{
			Diag.Ex(ex);

			_ExecResult = EnScriptExecutionResult.Failure;
		}
		finally
		{
			if (!syncToken.Cancelled())
				await QryMgr.Strategy.VerifyOpenConnectionAsync(default);

			await RaiseExecutionCompletedAsync(_ExecResult, true, cancelToken, syncToken);

			_AsyncExecState = EnLauncherPayloadLaunchState.Inactive;
		}

		return result;
	}



	public async Task<bool> ExecuteAsyinAsync(IBsTextSpan textSpan, EnSqlExecutionType executionType,
		IDbConnection conn, IBsQESQLBatchConsumer batchConsumer, TransientSettings sqlLiveSettings,
		CancellationToken cancelToken, CancellationToken syncToken)
	{
		if (_AsyncExecState != EnLauncherPayloadLaunchState.Inactive)
		{
			InvalidOperationException ex = new(Resources.ExceptionExecutionNotCompleted);
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


		// Evs.Trace(GetType(), nameof(ExecuteAsync), "ExecuteAsync() Launched.");

		return await Task.FromResult(true);
	}



	private async Task<bool> ExecuteImplAsync(CancellationToken cancelToken, CancellationToken syncToken)
	{
		// Evs.Trace(GetType(), nameof(ExecuteAsync));
		if (cancelToken.Cancelled())
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

		HookupBatchConsumer(_CurBatch, _BatchConsumer);

		_ExecResult = EnScriptExecutionResult.Success;



		// ------------------------------------------------------------------------------- //
		// ************* Execution Point (4) - AbstractQESQLExec.ExecuteAsync() ********** //
		// ------------------------------------------------------------------------------- //
		if (_ExecResult == EnScriptExecutionResult.Success)
			await ExecuteScriptAsync(cancelToken, syncToken);

		// Evs.Trace(GetType(), nameof(ExecuteAsync), "ExecuteScriptAsync() Completed. _ExecResult: {0}, _AsyncExecState: {1}.", _ExecResult, _AsyncExecState);


		bool discarded = false;

		lock (_LockObject)
			discarded = _AsyncExecState == EnLauncherPayloadLaunchState.Discarded;


		if (discarded)
		{
			Evs.Warning(GetType(), "ExecuteScriptAsync()", "Execution was discarded.");
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


		if (_ExecOptionHasChanged && !syncToken.Cancelled())
			ProcessExecOptions(_Conn);

		if (_ExecResult == EnScriptExecutionResult.Halted)
			_ExecResult = EnScriptExecutionResult.Failure;



		// Evs.Trace(GetType(), nameof(ExecuteAsync), "Completed.");

		return true;
	}



	protected abstract Task<EnScriptExecutionResult> ExecuteBatchStatementAsync(QESQLBatch batch,
		CancellationToken cancelToken, CancellationToken syncToken);



	// protected abstract Task<bool> ExecuteScriptAsync(CancellationToken cancelToken, CancellationToken syncToken);
	private async Task<bool> ExecuteScriptAsync(CancellationToken cancelToken, CancellationToken syncToken)
	{
		// Evs.Trace(GetType(), nameof(ExecuteScriptAsync));

		if (cancelToken.Cancelled())
			return false;

		_CurrentConn = _Conn;

		try
		{
			if (_SqlCmdParser == null)
				_SqlCmdParser = new ManagedBatchParser(QryMgr, _ExecutionType, _TextSpan.Text);
			else
				_SqlCmdParser.Cleanup(QryMgr, /* _BatchConsumer, */ _ExecutionType, /* ExecLiveSettings.EditorResultsOutputMode, */ _TextSpan.Text);


			string batchDelimiter = ExecLiveSettings.EditorExecutionBatchSeparator;


			_SqlCmdParser.SetBatchDelimiter(batchDelimiter);
			_SqlCmdParser.SetParseMode(EnParseMode.RecognizeOnlyBatchDelimiter);

			_SqlCmdParser.SetCommandExecuter((IBsCommandExecuter)this);

			if (ExecLiveSettings.ExecutionType != EnSqlExecutionType.PlanOnly
				&& ExecLiveSettings.TtsEnabled && !QryMgr.Strategy.TtsActive)
			{
				QryMgr.Strategy.BeginTransaction(ExecLiveSettings.EditorExecutionIsolationLevel);
			}

			// ------------------------------------------------------------------------------- //
			// ************ Execution Point (5) - QESQLExec.ExecuteScriptAsync() ************* //
			// ------------------------------------------------------------------------------- //
			await _SqlCmdParser.ParseAsync(cancelToken, syncToken);

			// Evs.Trace(GetType(), nameof(ExecuteScriptAsync), "ParseAsync() Completed");

		}
		catch (ThreadAbortException e)
		{
			Diag.ThrowException(e);
		}
		catch (Exception ex)
		{
			Diag.Expected(ex);

			_ExecResult = EnScriptExecutionResult.Failure;
			string info = ex.Message;

			RaiseScriptProcessingError(Resources.ExceptionScriptingParseFailure.Fmt(info),
				EnQESQLScriptProcessingMessageType.FatalError);
		}

		return !cancelToken.Cancelled();
	}



	/// <summary>
	/// We handle execution plan settings programmatically through FBCommand so these
	/// SET commands won't apply even though they exist in SqlResources.
	/// We just clear the batch collection after setting them for brevity.
	/// </summary>
	/// <param name="dbConnection"></param>
	private void ProcessExecOptions(IDbConnection dbConnection)
	{
		// Evs.Trace(GetType(), nameof(ProcessExecOptions));

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


	protected abstract void HookupBatchConsumer(QESQLBatch batch, IBsQESQLBatchConsumer batchConsumer);



	// Call statistics output
	public abstract void OnBatchDataLoaded(object sender, QueryDataEventArgs eventArgs);



	protected void RaiseErrorMessage(string errorLine, string msg, EnQESQLScriptProcessingMessageType msgType)
	{
		// Evs.Trace(GetType(), nameof(OnErrorMessage), "msg = {0}", msg);
		ErrorMessageEvent?.Invoke(this, new ErrorMessageEventArgs(errorLine, msg, msgType));
	}



	protected async virtual Task<bool> RaiseExecutionCompletedAsync(EnScriptExecutionResult execResult,
		bool launched, CancellationToken cancelToken, CancellationToken syncToken)
	{
		// Evs.Trace(GetType(), nameof(OnExecutionCompletedAsync), "execResult = {0}", execResult);

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
			Diag.Ex(ex);
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
			Diag.Ex(ex);
		}

		args.Result &= result;

		return await Task.FromResult(result);
	}



	private void RaiseScriptParsedEvent(EnSqlStatementAction statementAction, long totalRowsSelected,
		int statementCount, CancellationToken cancelToken)
	{
		QueryDataEventArgs args = new(_ExecutionType,
			cancelToken.Cancelled() ? EnSqlStatementAction.Cancelled : EnSqlStatementAction.ProcessQuery,
			_QryMgr.IsWithClientStats, totalRowsSelected, statementCount, _QryMgr.QueryExecutionStartTime, DateTime.Now);

		BatchScriptParsedEvent?.Invoke(this, args);
	}



	private void RaiseScriptProcessingError(string msg, EnQESQLScriptProcessingMessageType msgType)
	{
		// Evs.Trace(GetType(), nameof(OnScriptProcessingError), "msg = {0}", msg);

		switch (msgType)
		{
			case EnQESQLScriptProcessingMessageType.FatalError:
				RaiseErrorMessage(Resources.ErrorFatalScriptingNoParam, msg, msgType);
				break;
			case EnQESQLScriptProcessingMessageType.Error:
				RaiseErrorMessage(Resources.ErrorScriptingNoParam, msg, msgType);
				break;
			default:
				RaiseErrorMessage(Resources.WarnScriptingNoParam, msg, msgType);
				break;
		}
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
