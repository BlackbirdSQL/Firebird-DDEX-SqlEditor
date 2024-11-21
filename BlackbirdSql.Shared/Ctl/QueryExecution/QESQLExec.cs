// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution.QEOLESQLExec

using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using BlackbirdSql.Core.Enums;
using BlackbirdSql.Shared.Enums;
using BlackbirdSql.Shared.Events;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Shared.Properties;
using BlackbirdSql.Sys.Enums;
using BlackbirdSql.Sys.Interfaces;
using Microsoft.VisualStudio.PlatformUI;



namespace BlackbirdSql.Shared.Ctl.QueryExecution;


// =========================================================================================================
//										QESQLExec Class
//
/// <summary>
/// Query executor class. Handles post-script multi-statement batch processing. The ancestor class handles
/// script processing.
/// </summary>
// =========================================================================================================
public class QESQLExec : AbstractQESQLExec
{

	// -------------------------------------------
	#region Constructors / Destructors - QESQLExec
	// -------------------------------------------


	public QESQLExec(QueryManager qryMgr)
		: base(qryMgr)
	{
	}


	protected override bool Dispose(bool disposing)
	{
		// Evs.Trace(GetType(), "Dispose(bool)", "disposing: {0}", disposing);

		if (!base.Dispose(disposing))
			return false;

		return true;
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - QESQLExec
	// =========================================================================================================


	private int _ExecBatchNumOfTimes = 1;
	private bool _Hooked = false;
	private int _LineNumOfLastBatchEnd = -1;


	#endregion Fields





	// =========================================================================================================
	#region Property Accessors - QESQLExec
	// =========================================================================================================


	public GetCurrentWorkingDirectoryPath CurrentWorkingDirectoryPath { get; set; }

	public event QueryDataEventHandler BatchDataLoadedEvent;
	public event BatchStatementCompletedEventHandler BatchStatementCompletedEvent;
	public event BatchExecutionCompletedEventHandler BatchExecutionCompletedEventAsync;
	public event BatchExecutionStartEventHandler BatchExecutionStartEvent;


	#endregion Property Accessors





	// =========================================================================================================
	#region Methods - QESQLExec
	// =========================================================================================================


	public async override Task<EnParserAction> BatchStatementCallbackAsync(IBsNativeDbStatementWrapper sqlStatement,
		int numberOfTimes, CancellationToken cancelToken, CancellationToken syncToken)
	{
		int lineNumber = _LineNumOfLastBatchEnd + 1;
		if (_LineNumOfLastBatchEnd < -1)
			_LineNumOfLastBatchEnd = -1;
		if (lineNumber == -1)
			lineNumber = 0;
		_ = lineNumber;


		if (sqlStatement != null)
		{
			_ExecBatchNumOfTimes = numberOfTimes;

			// --------------------------------------------------------------------------------------------- //
			// *************** Execution Point (10) - QESQLExec.BatchStatementCallbackAsync() ************** //
			// --------------------------------------------------------------------------------------------- //
			bool continueProcessing = await ProcessBatchStatementAsync(sqlStatement, cancelToken, syncToken);

			if (_ExecResult == EnScriptExecutionResult.Cancel)
				_BatchConsumer.CurrentMessageCount++;

			if (continueProcessing)
				return EnParserAction.Continue;
		}

		return EnParserAction.Abort;
	}



	protected override void Cleanup()
	{
		_LineNumOfLastBatchEnd = -1;
		_CurBatchIndex = -1;

		CloseCurrentConnIfNeeded();
		UnhookBatchConsumer(_CurBatch, _BatchConsumer);

		_BatchConsumer = null;

		base.Cleanup();
	}



	private void CloseCurrentConnIfNeeded()
	{
		// Evs.Trace(GetType(), nameof(CloseCurrentConnIfNeeded));
		try
		{
			if (_CurrentConn == null || _CurrentConn == _Conn || _CurrentConn.State != ConnectionState.Open)
			{
				return;
			}

			/*
			if (_CurrentConnInfo != null)
			{
				if (_CurrentConnInfo.UserID != null && _CurrentConnInfo.UserID.Length != 0)
				{
					OnInfoMessage(Resources.InfoDisconnectingFromSvrAsUser.Fmt(string.IsNullOrWhiteSpace(_CurrentConnInfo.DatasetName) ? _CurrentConnInfo.Dataset : _CurrentConnInfo.DatasetName,
						_CurrentConnInfo.UserID));
				}
				else
				{
					OnInfoMessage(Resources.InfoDisconnectingFromSvr.Fmt(string.IsNullOrWhiteSpace(_CurrentConnInfo.DatasetName) ? _CurrentConnInfo.Dataset : _CurrentConnInfo.DatasetName));
				}
			}
			*/

			_CurrentConn.Close();
			_CurrentConn = null;
			// _CurrentConnInfo = null;
		}
		catch (Exception ex)
		{
			Diag.Ex(ex);
			RaiseErrorMessage(Resources.ExceptionUnableToCloseCon, ex.Message, EnQESQLScriptProcessingMessageType.Error);
		}
	}



	protected override async Task<EnScriptExecutionResult> ExecuteBatchStatementAsync(QESQLBatch batch,
		CancellationToken cancelToken, CancellationToken syncToken)
	{
		// Evs.Trace(GetType(), nameof(ExecuteBatchStatementAsync), " _ExecOptions.EstimatedPlanOnly: " + ExecLiveSettings.EstimatedPlanOnly);

		if (batch.SqlStatement == null)
			return EnScriptExecutionResult.Success;

		EnScriptExecutionResult scriptExecutionResult = EnScriptExecutionResult.Failure;
		try
		{
			bool multi = _ExecBatchNumOfTimes > 1;

			if (multi)
				RaiseInfoMessage(Resources.QueryBeginningBatchExec);


			for (int i = _ExecBatchNumOfTimes; i > 0; i--)
			{
				scriptExecutionResult = EnScriptExecutionResult.Failure;
				try
				{
					// ---------------------------------------------------------------------------------------------- //
					// **************** Execution Point (12) - QESQLExec.ExecuteBatchStatementAsync() *************** //
					// ---------------------------------------------------------------------------------------------- //
					scriptExecutionResult = await batch.ExecuteAsync(_CurrentConn, _SpecialActions, cancelToken, syncToken);
				}
				catch (Exception e)
				{
					if (!cancelToken.Cancelled())
						Diag.Expected(e);
					scriptExecutionResult = EnScriptExecutionResult.Failure;
				}

				if (_CurrentConn.State != ConnectionState.Open)
				{
					scriptExecutionResult = EnScriptExecutionResult.Halted;
				}

				switch (scriptExecutionResult)
				{
					case EnScriptExecutionResult.Cancel:
					case EnScriptExecutionResult.Halted:
						return scriptExecutionResult;
					default:
						if (multi)
							RaiseInfoMessage(Resources.WarnBatchExecutionFailedIgnoring);

						break;
					case EnScriptExecutionResult.Success:
						break;
				}
			}

			if (multi)
				RaiseInfoMessage(Resources.QueryBatchExecCompleted.Fmt(_ExecBatchNumOfTimes));

			return scriptExecutionResult;
		}
		catch (Exception e2)
		{
			Diag.Expected(e2);
			return EnScriptExecutionResult.Failure;
		}
	}



	private async Task<bool> ProcessBatchStatementAsync(IBsNativeDbStatementWrapper sqlStatement,
	CancellationToken cancelToken, CancellationToken syncToken)
	{
		// Evs.Trace(GetType(), nameof(ProcessBatchStatementAsync), " ExecLiveSettings.EstimatedPlanOnly: " + ExecLiveSettings.EstimatedPlanOnly);

		bool continueProcessing = true;

		if (sqlStatement == null)
		{
			_ExecResult |= EnScriptExecutionResult.Success;
			return continueProcessing;
		}


		lock (_LockObject)
		{
			if (cancelToken.Cancelled())
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
				RaiseBatchExecutionStart(_CurBatch);

				// ---------------------------------------------------------------------------------------------- //
				// *********** Execution Point (11) - AbstractQESQLExec.ProcessBatchStatementAsync() ************ //
				// ---------------------------------------------------------------------------------------------- //
				scriptExecutionResult = await ExecuteBatchStatementAsync(_CurBatch, cancelToken, syncToken);
			}
			finally
			{
				lock (_LockObject)
				{
					// Evs.Trace(GetType(), nameof(ProcessBatchStatementAsync), "execState = {0}", _ExecState);
					if (cancelToken.Cancelled() || _AsyncExecState == EnLauncherPayloadLaunchState.Discarded)
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
				await RaiseBatchExecutionCompletedAsync(_CurBatch, scriptExecutionResult, cancelToken, syncToken);
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


	#endregion Methods





	// =========================================================================================================
	#region Event Handling - QESQLExec
	// =========================================================================================================


	protected override void HookupBatchConsumer(QESQLBatch batch, IBsQESQLBatchConsumer batchConsumer)
	{
		// Evs.Trace(GetType(), nameof(HookupBatchConsumer));

		if (batch == null || batchConsumer == null || _Hooked)
			return;

		_Hooked = true;

		batch.ErrorMessageEvent += batchConsumer.OnErrorMessage;
		batch.MessageEvent += batchConsumer.OnMessage;
		batch.NewResultSetEventAsync += batchConsumer.OnNewResultSetAsync;
		batch.CancellingEvent += batchConsumer.OnCancelling;
		batch.FinishedResultSetEvent += batchConsumer.OnFinishedProcessingResultSet;
		batch.SpecialActionEvent += batchConsumer.OnSpecialAction;
		batch.StatementCompletedEvent += batchConsumer.OnStatementCompleted;

		batch.StatementCompletedEvent += OnBatchStatementCompleted;
	}



	// Call statistics output
	public override void OnBatchDataLoaded(object sender, QueryDataEventArgs eventArgs)
	{
		BatchDataLoadedEvent?.Invoke(this, eventArgs);
	}



	private void OnBatchStatementCompleted(object sender, BatchStatementCompletedEventArgs args)
	{
		// Evs.Trace(GetType(), nameof(OnBatchStatementCompleted), "sender: {0}, args.RecordCount: {1}", sender, args.RecordCount);

		// Added for StaticsPanel.RetrieveStatisticsIfNeeded();
		BatchStatementCompletedEvent?.Invoke(sender, args);
	}



	private async Task RaiseBatchExecutionCompletedAsync(QESQLBatch batch, EnScriptExecutionResult batchResult,
		CancellationToken cancelToken, CancellationToken syncToken)
	{
		// Evs.Trace(GetType(), nameof(OnBatchExecutionCompletedAsync), "m_curBatchIndex = {0}, batchResult = {1}, _ExecState = {2}", _CurBatchIndex, batchResult, _ExecState);
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



	private void RaiseBatchExecutionStart(QESQLBatch batch)
	{
		// Evs.Trace(GetType(), "QESQLExec.OnStartBatchExecution", "m_curBatchIndex = {0}", _CurBatchIndex);
		BatchExecutionStartEvent?.Invoke(this, new BatchExecutionStartEventArgs(-1, batch));
	}



	protected override async Task<bool> RaiseExecutionCompletedAsync(EnScriptExecutionResult execResult, bool launched,
		CancellationToken cancelToken, CancellationToken syncToken)
	{
		// Evs.Trace(GetType(), nameof(OnExecutionCompletedAsync), "execResult = {0}", execResult);

		try
		{
			CloseCurrentConnIfNeeded();
		}
		catch (Exception ex)
		{
			Diag.Ex(ex);
		}

		return await base.RaiseExecutionCompletedAsync(execResult, launched, cancelToken, syncToken);
	}


	private void RaiseInfoMessage(string message)
	{
		_BatchConsumer?.OnMessage(this, new BatchMessageEventArgs(message));
	}



	private void UnhookBatchConsumer(QESQLBatch batch, IBsQESQLBatchConsumer batchConsumer)
	{
		// Evs.Trace(GetType(), nameof(UnhookBatchConsumer));

		if (batch == null || batchConsumer == null || !_Hooked)
			return;

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


	#endregion Event Handling





	// =========================================================================================================
	#region Sub-Classes - QESQLExec
	// =========================================================================================================


	public delegate string GetCurrentWorkingDirectoryPath();


	#endregion Sub-Classes

}
