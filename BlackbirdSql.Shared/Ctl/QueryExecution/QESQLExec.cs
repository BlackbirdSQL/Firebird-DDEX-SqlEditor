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
using BlackbirdSql.Shared.Model.Parsers;
using BlackbirdSql.Shared.Properties;
using BlackbirdSql.Sys.Enums;
using BlackbirdSql.Sys.Interfaces;



namespace BlackbirdSql.Shared.Ctl.QueryExecution;


// =========================================================================================================
//										QESQLExec Class
//
/// <summary>
/// Query executor class.
/// </summary>
// =========================================================================================================
public class QESQLExec : AbstractQESQLExec, IBsCommandExecuter
{

	// -------------------------------------------
	#region Constructors / Destructors - QESQLExec
	// -------------------------------------------


	public QESQLExec(QueryManager qryMgr)
		: base(qryMgr)
	{
	}


	protected override void Dispose(bool disposing)
	{
		// Tracer.Trace(GetType(), "Dispose(bool)", "disposing: {0}", disposing);

		base.Dispose(disposing);

		if (!disposing)
			return;

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
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - QESQLExec
	// =========================================================================================================


	private IDbConnection _CurrentConn;
	private int _ExecBatchNumOfTimes = 1;
	private int _LineNumOfLastBatchEnd = -1;
	private ManagedBatchParser _SqlCmdParser;


	#endregion Fields





	// =========================================================================================================
	#region Property Accessors - QESQLExec
	// =========================================================================================================


	public GetCurrentWorkingDirectoryPath CurrentWorkingDirectoryPath { get; set; }

	public event ErrorMessageEventHandler ErrorMessageEvent;
	public event QueryDataEventHandler BatchDataLoadedEvent;
	public event QueryDataEventHandler BatchScriptParsedEvent;


	#endregion Property Accessors





	// =========================================================================================================
	#region Methods - QESQLExec
	// =========================================================================================================


	public async Task<EnParserAction> BatchStatementCallbackAsync(IBsNativeDbStatementWrapper sqlStatement,
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
			// *************** Execution Point (7) - QESQLExec.BatchStatementCallbackAsync() *************** //
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

		CloseCurrentConnIfNeeded();

		base.Cleanup();
	}



	private void CloseCurrentConnIfNeeded()
	{
		// Tracer.Trace(GetType(), "CloseCurrentConnIfNeeded()");
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
					OnInfoMessage(Resources.InfoDisconnectingFromSvrAsUser.FmtRes(string.IsNullOrWhiteSpace(_CurrentConnInfo.DatasetId) ? _CurrentConnInfo.Dataset : _CurrentConnInfo.DatasetId,
						_CurrentConnInfo.UserID));
				}
				else
				{
					OnInfoMessage(Resources.InfoDisconnectingFromSvr.FmtRes(string.IsNullOrWhiteSpace(_CurrentConnInfo.DatasetId) ? _CurrentConnInfo.Dataset : _CurrentConnInfo.DatasetId));
				}
			}
			*/

			_CurrentConn.Close();
			_CurrentConn = null;
			// _CurrentConnInfo = null;
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			RaiseErrorMessage(Resources.ExUnableToCloseCon, ex.Message, EnQESQLScriptProcessingMessageType.Error);
		}
	}



	protected override async Task<EnScriptExecutionResult> ExecuteBatchStatementAsync(QESQLBatch batch,
		CancellationToken cancelToken, CancellationToken syncToken)
	{
		// Tracer.Trace(GetType(), "ExecuteBatchStatementAsync()", " _ExecOptions.EstimatedPlanOnly: " + ExecLiveSettings.EstimatedPlanOnly);

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
					// **************** Execution Point (9) - QESQLExec.ExecuteBatchStatementAsync() **************** //
					// ---------------------------------------------------------------------------------------------- //
					scriptExecutionResult = await batch.ExecuteAsync(_CurrentConn, _SpecialActions, cancelToken, syncToken);
				}
				catch (Exception e)
				{
					if (!cancelToken.IsCancellationRequested)
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
				RaiseInfoMessage(Resources.QueryBatchExecCompleted.FmtRes(_ExecBatchNumOfTimes));

			return scriptExecutionResult;
		}
		catch (Exception e2)
		{
			Diag.Expected(e2);
			return EnScriptExecutionResult.Failure;
		}
	}



	protected override async Task<bool> ExecuteScriptAsync(CancellationToken cancelToken, CancellationToken syncToken)
	{
		// Tracer.Trace(GetType(), "ExecuteScriptAsync()");

		if (cancelToken.IsCancellationRequested)
			return false;

		_ExecBatchNumOfTimes = 1;
		_CurrentConn = _Conn;

		try
		{
			if (_SqlCmdParser == null)
			{
				_SqlCmdParser = new ManagedBatchParser(QryMgr, _ExecutionType, _TextSpan.Text);
			}
			else
			{
				_SqlCmdParser.Cleanup(QryMgr, /* _BatchConsumer, */ _ExecutionType, /* ExecLiveSettings.EditorResultsOutputMode, */ _TextSpan.Text);
			}

			string batchDelimiter = SysConstants.C_DefaultBatchSeparator;

			if (ExecLiveSettings.EditorContextBatchSeparator != null && ExecLiveSettings.EditorContextBatchSeparator.Length > 0)
			{
				batchDelimiter = ExecLiveSettings.EditorContextBatchSeparator;
			}

			_SqlCmdParser.SetBatchDelimiter(batchDelimiter);
			_SqlCmdParser.SetParseMode(EnParseMode.RecognizeOnlyBatchDelimiter);

			_SqlCmdParser.SetCommandExecuter(this);

			if (ExecLiveSettings.ExecutionType != EnSqlExecutionType.PlanOnly
				&& ExecLiveSettings.TtsEnabled && !QryMgr.Strategy.TtsActive)
			{
				QryMgr.Strategy.BeginTransaction(ExecLiveSettings.EditorExecutionIsolationLevel);
			}

			// ------------------------------------------------------------------------------- //
			// ************ Execution Point (5) - QESQLExec.ExecuteScriptAsync() ************* //
			// ------------------------------------------------------------------------------- //
			await _SqlCmdParser.ParseAsync(cancelToken, syncToken);

			// Tracer.Trace(GetType(), "ExecuteScriptAsync()", "ParseAsync() Completed");

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

			RaiseScriptProcessingError(Resources.ExScriptingParseFailure.FmtRes(info),
				EnQESQLScriptProcessingMessageType.FatalError);
		}

		return !cancelToken.IsCancellationRequested;
	}


	#endregion Methods





	// =========================================================================================================
	#region Event Handling - QESQLExec
	// =========================================================================================================


	// Call statistics output
	public void OnBatchDataLoaded(object sender, QueryDataEventArgs eventArgs)
	{
		BatchDataLoadedEvent?.Invoke(this, eventArgs);
	}


	public void OnBatchScriptParsed(object sender, QueryDataEventArgs eventArgs)
	{
		BatchScriptParsedEvent?.Invoke(this, eventArgs);
	}



	private void RaiseErrorMessage(string errorLine, string msg, EnQESQLScriptProcessingMessageType msgType)
	{
		// Tracer.Trace(GetType(), "OnErrorMessage()", "msg = {0}", msg);
		ErrorMessageEvent?.Invoke(this, new ErrorMessageEventArgs(errorLine, msg, msgType));
	}



	protected override async Task<bool> RaiseExecutionCompletedAsync(EnScriptExecutionResult execResult, bool launched,
		CancellationToken cancelToken, CancellationToken syncToken)
	{
		// Tracer.Trace(GetType(), "OnExecutionCompletedAsync()", "execResult = {0}", execResult);

		try
		{
			CloseCurrentConnIfNeeded();
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
		}

		return await base.RaiseExecutionCompletedAsync(execResult, launched, cancelToken, syncToken);
	}


	private void RaiseInfoMessage(string message)
	{
		_BatchConsumer?.OnMessage(this, new BatchMessageEventArgs(message));
	}



	private void RaiseScriptProcessingError(string msg, EnQESQLScriptProcessingMessageType msgType)
	{
		// Tracer.Trace(GetType(), "OnScriptProcessingError()", "msg = {0}", msg);

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
	#region Sub-Classes - QESQLExec
	// =========================================================================================================


	public delegate string GetCurrentWorkingDirectoryPath();


	#endregion Sub-Classes

}
