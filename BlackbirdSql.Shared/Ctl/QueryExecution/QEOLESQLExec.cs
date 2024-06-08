// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution.QEOLESQLExec
using System;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BlackbirdSql.Core.Enums;
using BlackbirdSql.Shared.Enums;
using BlackbirdSql.Shared.Events;
using BlackbirdSql.Shared.Exceptions;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Shared.Model;
using BlackbirdSql.Shared.Model.Parsers;
using BlackbirdSql.Shared.Properties;
using BlackbirdSql.Sys;
using BlackbirdSql.Sys.Enums;
using BlackbirdSql.Sys.Interfaces;



namespace BlackbirdSql.Shared.Ctl.QueryExecution;


public class QEOLESQLExec : AbstractQESQLExec, IBCommandExecuter
{

	public QEOLESQLExec(QueryManager qryMgr)
		: base(qryMgr)
	{
	}



	public const int C_ReadBufferSizeInBytes = 1024;

	public delegate string GetCurrentWorkingDirectoryPath();

	private ManagedBatchParser _SqlCmdParser;

	private int _LineNumOfLastBatchEnd = -1;

	private IDbConnection _CurrentConn;

	private DslConnectionInfo _CurrentConnInfo;


	private int _ExecBatchNumOfTimes = 1;

	public GetCurrentWorkingDirectoryPath CurrentWorkingDirectoryPath { get; set; }

	public event QEOLESQLErrorMessageEventHandler SqlErrorMessageEvent;
	public event QESQLQueryDataEventHandler DataLoadedEvent;




	protected override async Task<bool> ExecuteScriptAsync(CancellationToken cancelToken)
	{
		// Tracer.Trace(GetType(), "ExecuteScriptAsync()");

		if (cancelToken.IsCancellationRequested)
			return false;

		_ExecBatchNumOfTimes = 1;
		_CurrentConn = _Conn;

		if (_SqlCmdParser == null)
		{
			_SqlCmdParser = new ManagedBatchParser(QryMgr, _BatchConsumer, _ExecutionType, ExecLiveSettings.EditorResultsOutputMode, _TextSpan.Text);
		}
		else
		{
			_SqlCmdParser.Cleanup(QryMgr, _BatchConsumer, _ExecutionType, ExecLiveSettings.EditorResultsOutputMode, _TextSpan.Text);
		}

		string batchDelimiter = SysConstants.C_DefaultBatchSeparator;

		if (ExecLiveSettings.EditorContextBatchSeparator != null && ExecLiveSettings.EditorContextBatchSeparator.Length > 0)
		{
			batchDelimiter = ExecLiveSettings.EditorContextBatchSeparator;
		}

		_SqlCmdParser.SetBatchDelimiter(batchDelimiter);

		if (ExecLiveSettings.WithOleSqlScripting)
			_SqlCmdParser.SetParseMode(EnParseMode.RecognizeAll);
		else
			_SqlCmdParser.SetParseMode(EnParseMode.RecognizeOnlyBatchDelimiter);

		_SqlCmdParser.SetCommandExecuter(this);

		try
		{
			// ------------------------------------------------------------------------------- //
			// ******************** Execution Point (5) - QESQLExec.ExecuteScriptAsync() ******************** //
			// ------------------------------------------------------------------------------- //
			await _SqlCmdParser.ParseAsync(cancelToken);

			// Tracer.Trace(GetType(), "ExecuteScriptAsync()", "ParseAsync() Completed");

		}
		catch (ParserException ex)
		{
			Diag.Dug(ex);
			ParserState parserState = ex.ParserStateValue;
			if (parserState.ErrorTypeValue != ParserState.EnErrorType.CommandAborted)
			{
				_ExecResult = EnScriptExecutionResult.Failure;
				string info = parserState.Info;
				if (parserState.StatusValue == ParserState.EnStatus.Error
					&& parserState.ErrorTypeValue == ParserState.EnErrorType.SyntaxError &&
					info != null && info.Length > 0)
				{
					OnScriptProcessingError(string.Format(CultureInfo.CurrentCulture, ControlsResources.ErrScriptingIncorrectSyntax, info), EnQESQLScriptProcessingMessageType.FatalError);
				}
			}
		}
		catch (ThreadAbortException e)
		{
			Diag.ThrowException(e);
		}
		catch (Exception e2)
		{
			Diag.Dug(e2);
			_ExecResult = EnScriptExecutionResult.Failure;
		}

		return !cancelToken.IsCancellationRequested;
	}

	protected override async Task<EnScriptExecutionResult> ExecuteBatchRepetitionAsync(QESQLBatch batch, CancellationToken cancelToken)
	{
		// Tracer.Trace(GetType(), "ExecuteBatchRepetitionAsync()", " _ExecOptions.EstimatedPlanOnly: " + ExecLiveSettings.EstimatedPlanOnly);

		if (batch.SqlStatement == null)
			return EnScriptExecutionResult.Success;

		EnScriptExecutionResult scriptExecutionResult = EnScriptExecutionResult.Failure;
		try
		{
			bool multi = _ExecBatchNumOfTimes > 1;

			if (multi)
				OnInfoMessage(ControlsResources.BeginningBatchExec);


			for (int i = _ExecBatchNumOfTimes; i > 0; i--)
			{
				scriptExecutionResult = EnScriptExecutionResult.Failure;
				try
				{
					// ---------------------------------------------------------------------------------------------- //
					// ******************** Execution Point (9) - QEOLESQLExec.ExecuteBatchRepetitionAsync() ******************** //
					// ---------------------------------------------------------------------------------------------- //
					scriptExecutionResult = await batch.ProcessAsync(_CurrentConn, _SpecialActions, cancelToken);
				}
				catch (Exception e)
				{
					Diag.Dug(e);
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
							OnInfoMessage(ControlsResources.ErrBatchExecutionFailedIgnoring);

						break;
					case EnScriptExecutionResult.Success:
						break;
				}
			}

			if (multi)
				OnInfoMessage(string.Format(CultureInfo.CurrentCulture, ControlsResources.BatchExecCompleted, _ExecBatchNumOfTimes));

			return scriptExecutionResult;
		}
		catch (Exception e2)
		{
			Diag.Dug(e2);
			return EnScriptExecutionResult.Failure;
		}
	}


	// Call statistics output
	public void OnBatchDataLoaded(object sender, QESQLQueryDataEventArgs eventArgs)
	{
		DataLoadedEvent?.Invoke(this, eventArgs);
	}


	protected override void OnExecutionCompleted(EnScriptExecutionResult execResult)
	{
		// Tracer.Trace(GetType(), "OnExecutionCompleted()", "execResult = {0}", execResult);

		try
		{
			CloseCurrentConnIfNeeded();
		}
		catch (Exception ex)
		{
			Tracer.Warning(GetType(), "OnExecutionCompleted()", "Exception: {0}.", ex.Message);
		}

		base.OnExecutionCompleted(execResult);
	}


	protected override void Cleanup()
	{
		_LineNumOfLastBatchEnd = -1;

		CloseCurrentConnIfNeeded();

		base.Cleanup();
	}

	protected override void Dispose(bool bDisposing)
	{
		// Tracer.Trace(GetType(), "QEOLESQLExec.Dispose", "bDisposing = {0}", bDisposing);

		base.Dispose(bDisposing);

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
			_CurrentConnInfo = null;
		}
	}



	public async Task<EnParserAction> ProcessParsedBatchStatementAsync(IBsNativeDbStatementWrapper sqlStatement, int numberOfTimes, CancellationToken cancelToken)
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
			// ******************** Execution Point (7) - QESQLExec.ProcessParsedBatchStatementAsync() ******************** //
			// --------------------------------------------------------------------------------------------- //
			bool continueProcessing = await ProcessBatchStatementRepetitionAsync(sqlStatement, cancelToken);

			if (_ExecResult == EnScriptExecutionResult.Cancel)
				_BatchConsumer.CurrentMessageCount++;

			if (continueProcessing)
				return EnParserAction.Continue;
		}

		return EnParserAction.Abort;
	}




	private void OnQEOLESQLErrorMessage(string errorLine, string msg, EnQESQLScriptProcessingMessageType msgType)
	{
		// Tracer.Trace(GetType(), "QEOLESQLExec.OnQEOLESQLErrorMessage", "msg = {0}", msg);
		SqlErrorMessageEvent?.Invoke(this, new QEOLESQLErrorMessageEventArgs(errorLine, msg, msgType));
	}


	private void OnInfoMessage(string message)
	{
		_BatchConsumer?.OnMessage(this, new QESQLBatchMessageEventArgs(message));
	}


	private void CloseCurrentConnIfNeeded()
	{
		// Tracer.Trace(GetType(), "QEOLESQLExec.CloseCurrentConnIfNeeded", "", null);
		try
		{
			if (_CurrentConn == null || _CurrentConn == _Conn || _CurrentConn.State != ConnectionState.Open)
			{
				return;
			}

			if (_CurrentConnInfo != null)
			{
				if (_CurrentConnInfo.UserID != null && _CurrentConnInfo.UserID.Length != 0)
				{
					OnInfoMessage(string.Format(CultureInfo.CurrentCulture, ControlsResources.InfoDisconnectingFromSvrAsUser,
						string.IsNullOrWhiteSpace(_CurrentConnInfo.DatasetId) ? _CurrentConnInfo.Dataset : _CurrentConnInfo.DatasetId,
						_CurrentConnInfo.UserID));
				}
				else
				{
					OnInfoMessage(string.Format(CultureInfo.CurrentCulture, ControlsResources.InfoDisconnectingFromSvr,
						string.IsNullOrWhiteSpace(_CurrentConnInfo.DatasetId) ? _CurrentConnInfo.Dataset : _CurrentConnInfo.DatasetId));
				}
			}

			_CurrentConn.Close();
			_CurrentConn = null;
			_CurrentConnInfo = null;
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			OnQEOLESQLErrorMessage(ControlsResources.ErrUnableToCloseCon, ex.Message, EnQESQLScriptProcessingMessageType.Error);
		}
	}




	private void OnScriptProcessingError(string msg, EnQESQLScriptProcessingMessageType msgType)
	{
		// Tracer.Trace(GetType(), "OnScriptProcessingError()", "msg = {0}", msg);

		switch (msgType)
		{
			case EnQESQLScriptProcessingMessageType.FatalError:
				OnQEOLESQLErrorMessage(ControlsResources.ErrFatalScriptingErrorNoParam, msg, msgType);
				break;
			case EnQESQLScriptProcessingMessageType.Error:
				OnQEOLESQLErrorMessage(ControlsResources.ErrScriptingErrorNoParam, msg, msgType);
				break;
			default:
				OnQEOLESQLErrorMessage(ControlsResources.ErrScriptingWarningNoParam, msg, msgType);
				break;
		}
	}

}
