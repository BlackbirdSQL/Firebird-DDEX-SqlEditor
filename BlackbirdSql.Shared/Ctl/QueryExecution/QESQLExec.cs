// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution.QEOLESQLExec
using System;
using System.Data;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using BlackbirdSql.Core.Enums;
using BlackbirdSql.Shared.Enums;
using BlackbirdSql.Shared.Events;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Shared.Model.Parsers;
using BlackbirdSql.Shared.Properties;
using BlackbirdSql.Sys;
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
public class QESQLExec : AbstractQESQLExec, IBCommandExecuter
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

	public event QEOLESQLErrorMessageEventHandler SqlErrorMessageEvent;
	public event QESQLQueryDataEventHandler DataLoadedEvent;


	#endregion Property Accessors





	// =========================================================================================================
	#region Methods - QESQLExec
	// =========================================================================================================


	public async Task<EnParserAction> BatchStatementCallbackAsync(IBsNativeDbStatementWrapper sqlStatement, int numberOfTimes, CancellationToken cancelToken)
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
			// ******************** Execution Point (7) - QESQLExec.BatchStatementCallbackAsync() ******************** //
			// --------------------------------------------------------------------------------------------- //
			bool continueProcessing = await ProcessBatchStatementAsync(sqlStatement, cancelToken);

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
			*/

			_CurrentConn.Close();
			_CurrentConn = null;
			// _CurrentConnInfo = null;
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			OnQEOLESQLErrorMessage(ControlsResources.ErrUnableToCloseCon, ex.Message, EnQESQLScriptProcessingMessageType.Error);
		}
	}



	protected override async Task<EnScriptExecutionResult> ExecuteBatchStatementAsync(QESQLBatch batch, CancellationToken cancelToken)
	{
		// Tracer.Trace(GetType(), "ExecuteBatchStatementAsync()", " _ExecOptions.EstimatedPlanOnly: " + ExecLiveSettings.EstimatedPlanOnly);

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
					// ******************** Execution Point (9) - QESQLExec.ExecuteBatchStatementAsync() ******************** //
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



	protected override async Task<bool> ExecuteScriptAsync(CancellationToken cancelToken)
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

			if (ExecLiveSettings.ExecutionType != EnSqlExecutionType.PlanOnly
				&& ExecLiveSettings.TtsEnabled && !QryMgr.Strategy.TtsActive)
			{
				IDbTransaction transaction = _Conn.BeginTransaction(ExecLiveSettings.EditorExecutionIsolationLevel);
				QryMgr.Strategy.Transaction = transaction;
			}

			// ------------------------------------------------------------------------------- //
			// ******************** Execution Point (5) - QESQLExec.ExecuteScriptAsync() ******************** //
			// ------------------------------------------------------------------------------- //
			await _SqlCmdParser.ParseAsync(cancelToken);

			// Tracer.Trace(GetType(), "ExecuteScriptAsync()", "ParseAsync() Completed");

		}
		catch (ThreadAbortException e)
		{
			Diag.ThrowException(e);
		}
		catch (Exception ex)
		{
#if DEBUG
			Diag.Dug(ex);
#endif
			_ExecResult = EnScriptExecutionResult.Failure;
			string info = ex.Message;
			OnScriptProcessingError(ControlsResources.ErrScriptingParseFailure.FmtRes(info),
				EnQESQLScriptProcessingMessageType.FatalError);
		}

		return !cancelToken.IsCancellationRequested;
	}


	#endregion Methods





	// =========================================================================================================
	#region Event Handling - QESQLExec
	// =========================================================================================================


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


	private void OnInfoMessage(string message)
	{
		_BatchConsumer?.OnMessage(this, new QESQLBatchMessageEventArgs(message));
	}



	private void OnQEOLESQLErrorMessage(string errorLine, string msg, EnQESQLScriptProcessingMessageType msgType)
	{
		// Tracer.Trace(GetType(), "OnQEOLESQLErrorMessage", "msg = {0}", msg);
		SqlErrorMessageEvent?.Invoke(this, new QEOLESQLErrorMessageEventArgs(errorLine, msg, msgType));
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


	#endregion Event Handling





	// =========================================================================================================
	#region Sub-Classes - QESQLExec
	// =========================================================================================================


	public delegate string GetCurrentWorkingDirectoryPath();


	#endregion Sub-Classes

}
