// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution.QEOLESQLExec
using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;

using BlackbirdSql.Common.Ctl;
using BlackbirdSql.Common.Ctl.Exceptions;
using BlackbirdSql.Common.Ctl.Interfaces;
using BlackbirdSql.Common.Model.Enums;
using BlackbirdSql.Common.Model.Events;
using BlackbirdSql.Common.Model.Interfaces;
using BlackbirdSql.Common.Model.Parsers;
using BlackbirdSql.Common.Properties;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Model;

using FirebirdSql.Data.FirebirdClient;

using Microsoft.VisualStudio;




namespace BlackbirdSql.Common.Model.QueryExecution;


public class QEOLESQLExec(QEOLESQLExec.ResolveSqlCmdVariable sqlCmdVariableResolver,
		QueryManager qryMgr)
	: AbstractQESQLExec(qryMgr), IBBatchSource, IBCommandExecuter, IBVariableResolver
{
	public const int C_ReadBufferSizeInBytes = 1024;

	public delegate string ResolveSqlCmdVariable(string variableName);

	public delegate string GetCurrentWorkingDirectoryPath();

	private class AsyncRedirectedOutputState
	{
		private readonly AutoResetEvent _DoneEvent;

		private readonly byte[] _ReadOutputBuffer;

		public Stream OutputStream;

		public bool IsStdOut;

		public Encoding OutputEncoding;

		public AutoResetEvent DoneEvent => _DoneEvent;

		public byte[] ReadOutputBuffer => _ReadOutputBuffer;

		private AsyncRedirectedOutputState()
		{
		}

		public AsyncRedirectedOutputState(Stream outputStream, Encoding outputEncoding, bool isStdOut)
		{
			OutputStream = outputStream;
			OutputEncoding = outputEncoding;
			IsStdOut = isStdOut;
			_DoneEvent = new AutoResetEvent(initialState: false);
			_ReadOutputBuffer = new byte[C_ReadBufferSizeInBytes];
		}
	}

	public class BatchSourceString : IBBatchSource
	{
		private string str;

		private BatchSourceString()
		{
		}

		public BatchSourceString(string str)
		{
			this.str = str;
		}

		public EnParserAction GetMoreData(ref string str)
		{
			str = this.str;
			this.str = null;
			return VSConstants.S_OK;
		}

		EnParserAction IBBatchSource.GetMoreData(ref string str)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			return GetMoreData(ref str);
		}
	}

	private ManagedBatchParser _SqlCmdParser;

	private int _LineNumOfLastBatchEnd = -1;

	private IDbConnection _CurrentConn;
	// private IDbConnection _currentSSConn;

	private DslConnectionInfo _CurrentConnInfo;

	// Microsoft.SqlServer.Management.Common.SqlConnectionInfo _currentSSConnInfo;

	private EnErrorAction _ErrorAction = EnErrorAction.Ignore;

	private int _ExecBatchNumOfTimes = 1;

	private Process _runningProcess;

	private AsyncRedirectedOutputState _stdOutRedirState;

	private AsyncRedirectedOutputState _stdErrorRedirState;

	private object _AsyncLockLocal = new object();

	private AsyncCallback _readBufferCallback;

	public ResolveSqlCmdVariable SqlCmdVariableResolver { get; set; } = sqlCmdVariableResolver;

	public GetCurrentWorkingDirectoryPath CurrentWorkingDirectoryPath { get; set; }

	public event QEOLESQLOutputRedirectionEventHandler SqlOutputRedirectionEvent;

	public event QEOLESQLErrorMessageEventHandler SqlErrorMessageEvent;

	public event QeSqlCmdMessageFromAppEventHandler SqlCmdMessageFromAppEvent;

	public event QeSqlCmdNewConnectionOpenedEventHandler SqlCmdNewConnectionOpenedEvent;

	protected override void ExecuteScript(IBTextSpan textSpan)
	{
		// Tracer.Trace(GetType(), "QEOLESQLExec.ExecuteScript", " _ExecOptions.WithEstimatedExecutionPlan: " + ExecLiveSettings.WithEstimatedExecutionPlan);
		_ErrorAction = EnErrorAction.Ignore;
		_ExecBatchNumOfTimes = 1;
		_CurrentConn = _Conn;

		if (_SqlCmdParser == null)
			_SqlCmdParser = new ManagedBatchParser();
		else
			_SqlCmdParser.Cleanup();

		string batchDelimiter = ModelConstants.C_DefaultBatchSeparator;

		if (ExecLiveSettings.EditorContextBatchSeparator != null && ExecLiveSettings.EditorContextBatchSeparator.Length > 0)
		{
			batchDelimiter = ExecLiveSettings.EditorContextBatchSeparator;
		}

		_SqlCmdParser.SetBatchDelimiter(batchDelimiter);

		if (ExecLiveSettings.WithOleSqlScripting)
			_SqlCmdParser.SetParseMode(EnParseMode.RecognizeAll);
		else
			_SqlCmdParser.SetParseMode(EnParseMode.RecognizeOnlyBatchDelimiter);

		_SqlCmdParser.SetBatchSource(new BatchSourceString(_TextSpan.Text));
		_SqlCmdParser.SetCommandExecuter(this);
		_SqlCmdParser.SetVariableResolver(this);

		try
		{
			// ------------------------------------------------------------------------------- //
			// ******************** Execution Point (7) - ExecuteScript() ******************** //
			// ------------------------------------------------------------------------------- //
			_SqlCmdParser.Parse();
		}
		catch (ParserException ex)
		{
			Diag.Dug(ex);
			ParserState parserState = ex.ParserStateValue;
			if (parserState.ErrorTypeValue != ParserState.EnErrorType.CommandAborted)
			{
				_ExecResult = EnScriptExecutionResult.Failure;
				string info = parserState.Info;
				if (parserState.StatusValue == ParserState.EnStatus.Error && parserState.ErrorTypeValue == ParserState.EnErrorType.SyntaxError && info != null && info.Length > 0)
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
	}

	protected override EnScriptExecutionResult ExecuteBatchRepeatStatement(QESQLBatch batch)
	{
		// Tracer.Trace(GetType(), "QEOLESQLExec.ExecuteBatchRepeatStatement", " _ExecOptions.WithEstimatedExecutionPlan: " + ExecLiveSettings.WithEstimatedExecutionPlan);
		if (batch.SqlScript == null || batch.SqlScript != null && batch.SqlScript.Length == 0)
		{
			return EnScriptExecutionResult.Success;
		}

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
					if (ExecLiveSettings.WithEstimatedExecutionPlan)
						_SpecialActions |= EnQESQLBatchSpecialAction.ExpectEstimatedYukonXmlExecutionPlan;

					// ---------------------------------------------------------------------------------------------- //
					// ******************** Execution Point (11) - ExecuteBatchRepeatStatement() ******************** //
					// ---------------------------------------------------------------------------------------------- //
					scriptExecutionResult = batch.ProcessStatement(_CurrentConn, _SpecialActions);
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
						if (_ErrorAction == EnErrorAction.Ignore)
						{
							if (multi)
								OnInfoMessage(ControlsResources.ErrBatchExecutionFailedIgnoring);
						}
						else if (_ErrorAction == EnErrorAction.Exit)
						{
							OnInfoMessage(ControlsResources.ErrBatchExecutionFailedExiting);
							return EnScriptExecutionResult.Halted;
						}

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

	protected override void CompleteAsyncCancelOperation(EnExecState stateBeforeCancelOp)
	{
		if (stateBeforeCancelOp != EnExecState.Executing)
		{
			return;
		}

		lock (_LockObject)
		{
			_runningProcess?.Kill();
		}

		lock (_AsyncLockLocal)
		{
			_stdOutRedirState?.DoneEvent.Set();

			_stdErrorRedirState?.DoneEvent.Set();
		}
	}

	protected override void Cleanup()
	{
		_ErrorAction = EnErrorAction.Ignore;
		_LineNumOfLastBatchEnd = -1;
		CloseCurrentConnIfNeeded();
		lock (_LockObject)
		{
			if (_runningProcess != null)
			{
				_runningProcess.Dispose();
				_runningProcess = null;
			}
		}

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
			_SSconn = null;
			_CurrentConnInfo = null;
			// _currentSSConnInfo = null;
		}

		_AsyncLockLocal = null;
	}

	public EnParserAction GetMoreData(ref string str)
	{
		// Tracer.Trace(GetType(), "QEOLESQLExec.GetMoreData", "", null);
		str = _TextSpan.Text;
		return 0;
	}

	public EnParserAction ResolveVariable(string varName, ref string varValue)
	{
		varValue = null;
		if (SqlCmdVariableResolver != null)
		{
			varValue = SqlCmdVariableResolver(varName);
		}

		if (varValue == null)
		{
			OnScriptProcessingError(string.Format(CultureInfo.CurrentCulture, ControlsResources.ErrOLESQLVarNotDefined, varName), EnQESQLScriptProcessingMessageType.FatalError);
			_ExecResult = EnScriptExecutionResult.Halted;
			return (EnParserAction)1;
		}

		return 0;
	}

	public EnParserAction DeleteVariable(string varName)
	{
		return 0;
	}

	public EnParserAction ResolveVariableOwnership(string varName, string varValue, ref bool bTakeOwmership)
	{
		bTakeOwmership = false;
		return 0;
	}

	public EnParserAction ProcessParsedBatchStatement(string statementString, int numberOfTimes)
	{
		// Tracer.Trace(GetType(), "QEOLESQLExec.ProcessParsedBatchStatement", "str = {0}, numberOfTimes = {1}", str ?? "null", numberOfTimes);
		int lineNumber = _LineNumOfLastBatchEnd + 1;
		// _LineNumOfLastBatchEnd = _SqlCmdParser.GetLastCommandLineNumber() - 1;

		if (_LineNumOfLastBatchEnd < -1)
			_LineNumOfLastBatchEnd = -1;


		if (lineNumber == -1)
			lineNumber = 0;

		IBTextSpan textSpan = new SqlTextSpan(_TextSpan.AnchorLine, _TextSpan.AnchorCol,
			_TextSpan.EndLine, _TextSpan.EndCol, -1, lineNumber, statementString,
			((SqlTextSpan)_TextSpan).VsTextView);

		if (statementString != null)
		{
			_ExecBatchNumOfTimes = numberOfTimes;

			// --------------------------------------------------------------------------------------------- //
			// ******************** Execution Point (9) - ProcessParsedBatchStatement() ******************** //
			// --------------------------------------------------------------------------------------------- //
			ProcessBatchRepeatStatement(statementString, textSpan, out bool continueProcessing);

			if (continueProcessing)
				return EnParserAction.Continue;
		}

		return EnParserAction.Abort;
	}

	public EnParserAction Exit(string batch, string exitBatch)
	{
		// Tracer.Trace(GetType(), "QEOLESQLExec.Exit", "batch = {0}, exitBatch = {1}", batch ?? "null", exitBatch ?? "null");
		StringBuilder stringBuilder = new StringBuilder(batch);
		if (exitBatch != null)
		{
			stringBuilder.AppendFormat(" {0}", exitBatch);
		}

		// ExecuteParsedBatchLine(stringBuilder.ToString(), 1, _SqlCmdParser.GetLastCommandLineNumber());
		return (EnParserAction)1;
	}

	public EnParserAction IncludeFileName(string fileName, ref IBBatchSource pIBatchSource)
	{
		string text = ReadFileContent(fileName);
		if (text == null)
		{
			_ExecResult = EnScriptExecutionResult.Halted;
			return (EnParserAction)1;
		}

		pIBatchSource = (IBBatchSource)(object)new BatchSourceString(text);
		return 0;
	}

	public EnParserAction Quit()
	{
		return (EnParserAction)1;
	}

	public EnParserAction Reset()
	{
		OnScriptProcessingError(string.Format(CultureInfo.CurrentCulture, ControlsResources.ErrNotSupportedSqlCmdCommand, "Reset"), EnQESQLScriptProcessingMessageType.Warning);
		return 0;
	}

	public EnParserAction Ed(string batch, ref IBBatchSource batchSource)
	{
		OnScriptProcessingError(string.Format(CultureInfo.CurrentCulture, ControlsResources.ErrNotSupportedSqlCmdCommand, "Ed"), EnQESQLScriptProcessingMessageType.Warning);
		return 0;
	}

	public EnParserAction ExecuteShellCommand(string command)
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		// Tracer.Trace(GetType(), "QEOLESQLExec.ExecuteShellCommand", "command = {0}", command ?? "null");
		ExecuteACommand(command, null);
		lock (_LockObject)
		{
			if (_ExecState == EnExecState.Cancelling)
			{
				_ExecResult = EnScriptExecutionResult.Cancel;
				return (EnParserAction)1;
			}

			return 0;
		}
	}

	public EnParserAction ServerList()
	{
		OnScriptProcessingError(string.Format(CultureInfo.CurrentCulture, ControlsResources.ErrNotSupportedSqlCmdCommand, "ServerList"), EnQESQLScriptProcessingMessageType.Warning);
		return 0;
	}

	public EnParserAction List(string batch)
	{
		OnScriptProcessingError(string.Format(CultureInfo.CurrentCulture, ControlsResources.ErrNotSupportedSqlCmdCommand, "List"), EnQESQLScriptProcessingMessageType.Warning);
		return 0;
	}

	public EnParserAction ListVar(string varList)
	{
		OnScriptProcessingError(string.Format(CultureInfo.CurrentCulture, ControlsResources.ErrNotSupportedSqlCmdCommand, "ListVar"), EnQESQLScriptProcessingMessageType.Warning);
		return 0;
	}

	public EnParserAction Error(EnOutputDestination od, string fileName)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		// Tracer.Trace(GetType(), "QEOLESQLExec.Error", "od = {0}", od);
		CheckRedirectionCategoty(od, fileName, EnQEOLESQLOutputCategory.Errors);
		return 0;
	}

	public EnParserAction Out(EnOutputDestination od, string fileName)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		// Tracer.Trace(GetType(), "QEOLESQLExec.Out", "od = {0}", od);
		CheckRedirectionCategoty(od, fileName, EnQEOLESQLOutputCategory.Results);
		return 0;
	}

	public EnParserAction PerfTrace(EnOutputDestination od, string fileName)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		// Tracer.Trace(GetType(), "QEOLESQLExec.PerfTrace", "od = {0}", od);
		OnScriptProcessingError(string.Format(CultureInfo.CurrentCulture, ControlsResources.ErrNotSupportedSqlCmdCommand, "perftrace"), EnQESQLScriptProcessingMessageType.Warning);
		return 0;
	}




	public EnParserAction Connect(int timeout, string server, int port, string database, string user, string password)
	{
		CloseCurrentConnIfNeeded();

		DslConnectionInfo sqlConnectionInfo = new(server, port, database, user, password)
		{
			ConnectionTimeout = timeout
		};

		_CurrentConn = AttemptToEstablishCurConnection(sqlConnectionInfo);
		if (_CurrentConn == null)
		{
			_ExecResult = EnScriptExecutionResult.Halted;
			return (EnParserAction)1;
		}

		_CurrentConnInfo = sqlConnectionInfo;
		return 0;
	}

	public EnParserAction Connect(int timeout, string server, string user, string password)
	{
		/*
		CloseCurrentSSConnIfNeeded();
		Microsoft.SqlServer.Management.Common.SqlConnectionInfo sqlConnectionInfo = new(server, user, password)
		{
			ConnectionTimeout = timeout
		};

		_currentSSConn = AttemptToEstablishCurConnection(sqlConnectionInfo);
		if (_currentSSConn == null)
		{
			_ExecResult = EnScriptExecutionResult.Halted;
			return (EnParserAction)1;
		}

		_currentSSConnInfo = sqlConnectionInfo;
		*/
		return 0;
	}



	public EnParserAction OnError(EnErrorAction ea)
	{
		// Tracer.Trace(GetType(), "QEOLESQLExec.OnError", "ea = {0}", ea);
		_ErrorAction = ea;

		return EnParserAction.Continue;
	}

	public EnParserAction Xml(EnXmlStatus xs)
	{
		OnScriptProcessingError(string.Format(CultureInfo.CurrentCulture, ControlsResources.ErrNotSupportedSqlCmdCommand, "Xml"), EnQESQLScriptProcessingMessageType.Warning);
		return 0;
	}

	public EnParserAction Help()
	{
		OnScriptProcessingError(string.Format(CultureInfo.CurrentCulture, ControlsResources.ErrNotSupportedSqlCmdCommand, "Help"), EnQESQLScriptProcessingMessageType.Warning);
		return 0;
	}

	private void OnQEOLESQLErrorMessage(string errorLine, string msg, EnQESQLScriptProcessingMessageType msgType)
	{
		// Tracer.Trace(GetType(), "QEOLESQLExec.OnQEOLESQLErrorMessage", "msg = {0}", msg);
		SqlErrorMessageEvent?.Invoke(this, new QEOLESQLErrorMessageEventArgs(errorLine, msg, msgType));
	}

	private void OnQeSqlCmdMessageFromApp(string message, bool stdOut)
	{
		if (SqlCmdMessageFromAppEvent != null)
		{
			lock (_AsyncLockLocal)
			{
				SqlCmdMessageFromAppEvent(this, new (message, stdOut));
			}
		}
	}

	private void OnQEOLESQLOutputRedirection(EnQEOLESQLOutputCategory category, string fullFileName)
	{
		// Tracer.Trace(GetType(), "QEOLESQLExec.OnQEOLESQLOutputRedirection", "", null);
		if (SqlOutputRedirectionEvent != null)
		{
			QEOLESQLOutputRedirectionEventArgs qEOLESQLOutputRedirectionEventArgs = new (category, fullFileName, _BatchConsumer);
			SqlOutputRedirectionEvent(this, qEOLESQLOutputRedirectionEventArgs);
			if (qEOLESQLOutputRedirectionEventArgs.BatchConsumer != null && qEOLESQLOutputRedirectionEventArgs.BatchConsumer != _BatchConsumer)
			{
				HookupBatchWithConsumer(_CurBatch, _BatchConsumer, bHookUp: false);
				_BatchConsumer = qEOLESQLOutputRedirectionEventArgs.BatchConsumer;
				HookupBatchWithConsumer(_CurBatch, _BatchConsumer, bHookUp: true);
			}
		}
	}

	private void OnInfoMessage(string message)
	{
		_BatchConsumer?.OnMessage(this, new QESQLBatchMessageEventArgs(message));
	}

	private void CheckRedirectionCategoty(EnOutputDestination od, string fileName, EnQEOLESQLOutputCategory categoty)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Invalid comparison between Unknown and I4
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Invalid comparison between Unknown and I4
		// Tracer.Trace(GetType(), "QEOLESQLExec.CheckRedirectionCategoty", "", null);
		string text = fileName;
		if ((int)od == 2)
		{
			text = "stderr";
		}
		else if ((int)od == 1)
		{
			text = "stdout";
		}

		try
		{
			OnQEOLESQLOutputRedirection(categoty, text);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			OnQEOLESQLErrorMessage(string.Format(CultureInfo.CurrentCulture, ControlsResources.ErrUnableToRedirOutput, text), ex.Message, EnQESQLScriptProcessingMessageType.Error);
		}
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



	/*
	private void CloseCurrentSSConnIfNeeded()
	{
		// Tracer.Trace(GetType(), "QEOLESQLExec.CloseCurrentConnIfNeeded", "", null);
		try
		{
			if (_currentSSConn == null || _currentSSConn == _SSconn || _currentSSConn.State != ConnectionState.Open)
			{
				return;
			}
			if (_currentSSConnInfo != null)
			{
				if (_currentSSConnInfo.UserID != null && _currentSSConnInfo.UserID.Length != 0)
				{
					OnInfoMessage(string.Format(CultureInfo.CurrentCulture, ControlsResources.InfoDisconnectingFromSvrAsUser, _currentSSConnInfo.DataSource, _currentSSConnInfo.UserID));
				}
				else
				{
					OnInfoMessage(string.Format(CultureInfo.CurrentCulture, ControlsResources.InfoDisconnectingFromSvr, _currentSSConnInfo.DataSource));
				}
			}
			_currentSSConn.Close();
			_currentSSConn = null;
			_currentSSConnInfo = null;
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			OnQEOLESQLErrorMessage(ControlsResources.ErrUnableToCloseCon, ex.Message, EnQESQLScriptProcessingMessageType.Error);
		}
	}
	*/

	private IDbConnection AttemptToEstablishCurConnection(DslConnectionInfo ci)
	{
		// Tracer.Trace(GetType(), "QEOLESQLExec.AttemptToEstablishCurConnection", "ci = {0}", ci.ToString());
		try
		{
			if (ci.DataSource == null || ci.DataSource.Length == 0 && _Conn != null)
			{
				ci.DataSource = ((FbConnection)_Conn).DataSource;
			}

			if (ci.UserID != null && ci.UserID.Length != 0)
			{
				OnInfoMessage(string.Format(CultureInfo.CurrentCulture, ControlsResources.InfoConnectingToSvrAsUser,
					string.IsNullOrWhiteSpace(ci.DatasetId) ? ci.Dataset : ci.DatasetId, ci.UserID));
			}
			else
			{
				OnInfoMessage(string.Format(CultureInfo.CurrentCulture, ControlsResources.InfoConnectingToSvr,
					string.IsNullOrWhiteSpace(ci.DatasetId) ? ci.Dataset : ci.DatasetId));
			}

			// ci.Pooling = false;
			// ci.ApplicationName = LibraryData.ApplicationName;
			string connectionString = ci.PropertyString;
			// connectionString += ";Pooling=false";
			/*
			if (_encryptConnectionOptions)
			{
				// connectionString += ";Encrypt=true";
			}

			if (_trustServerCertificateOption)
			{
				// connectionString += ";TrustServerCertificate=true";
			}
			*/

			// Tracer.Trace(GetType(), Tracer.EnLevel.Information, "QEOLESQLExec.AttemptToEstablishCurConnection: final connection string is \"{0}\"", connectionString);
			IDbConnection dbConnection = new FbConnection(connectionString);
			dbConnection.Open();
			SqlCmdNewConnectionOpenedEvent?.Invoke(this, new QeSqlCmdNewConnectionOpenedEventArgs(dbConnection));

			ProcessExecOptions(dbConnection);
			SetRestoreConnectionOptions(bSet: true, dbConnection);
			return dbConnection;
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			OnQEOLESQLErrorMessage(ControlsResources.ErrUnableToConnect, ex.Message, EnQESQLScriptProcessingMessageType.FatalError);
			return null;
		}
	}

	/*
	private IDbConnection AttemptToEstablishCurConnection(Microsoft.SqlServer.Management.Common.SqlConnectionInfo ci)
	{
		// Tracer.Trace(GetType(), "QEOLESQLExec.AttemptToEstablishCurConnection", "ci = {0}", ci.ToString());
		try
		{
			if (ci.ServerName == null || ci.ServerName.Length == 0 && _SSconn != null)
			{
				ci.DataSource = ((SqlConnection)_SSconn).DataSource;
			}

			if (ci.UserID != null && ci.UserID.Length != 0)
			{
				OnInfoMessage(string.Format(CultureInfo.CurrentCulture, ControlsResources.InfoConnectingToSvrAsUser, ci.DataSource, ci.UserID));
			}
			else
			{
				OnInfoMessage(string.Format(CultureInfo.CurrentCulture, ControlsResources.InfoConnectingToSvr, ci.DataSource));
			}

			ci.ApplicationName = "Microsoft SQL Server Data Tools, T-SQL Editor";
			string connectionString = ci.ConnectionString;
			connectionString += ";Pooling=false";
			if (_encryptConnectionOptions)
			{
				connectionString += ";Encrypt=true";
			}

			if (_trustServerCertificateOption)
			{
				connectionString += ";TrustServerCertificate=true";
			}

			// Tracer.Warning(GetType(), "QEOLESQLExec.AttemptToEstablishCurConnection: final connection string is \"{0}\"", connectionString);
			IDbConnection dbConnection = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
			dbConnection.Open();
			SqlCmdNewConnectionOpenedEvent?.Invoke(this, new QeSqlCmdNewConnectionOpenedEventArgs(dbConnection));

			ProcessExecOptions(dbConnection);
			SetRestoreConnectionOptions(bSet: true, dbConnection);
			return dbConnection;
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			OnQEOLESQLErrorMessage(ControlsResources.ErrUnableToConnect, ex.Message, EnQESQLScriptProcessingMessageType.FatalError);
			return null;
		}
	}
	*/


	private void RedirectOutputCallback(IAsyncResult ar)
	{
		AsyncRedirectedOutputState asyncRedirectedOutputState = ar.AsyncState as AsyncRedirectedOutputState;
		Stream stream = asyncRedirectedOutputState.OutputStream;
		int num = 0;
		try
		{
			num = stream.EndRead(ar);
		}
		catch (IOException)
		{
		}

		lock (_LockObject)
		{
			if (_ExecState != EnExecState.Executing)
			{
				return;
			}
		}

		if (num > 0)
		{
			string @string = asyncRedirectedOutputState.OutputEncoding.GetString(asyncRedirectedOutputState.ReadOutputBuffer, 0, num);
			OnQeSqlCmdMessageFromApp(@string, asyncRedirectedOutputState.IsStdOut);
			stream.BeginRead(asyncRedirectedOutputState.ReadOutputBuffer, 0, C_ReadBufferSizeInBytes, _readBufferCallback, asyncRedirectedOutputState);
		}
		else
		{
			asyncRedirectedOutputState.DoneEvent.Set();
		}
	}

	private void ExecuteACommand(string fileName, string commandLine)
	{
		// Tracer.Trace(GetType(), "QEOLESQLExec.ExecuteACommand", "fileName = {0}, commandLine = {1}", fileName, commandLine);
		Process process = null;
		try
		{
			process = new Process();
			string fileName2 = string.Format(CultureInfo.InvariantCulture, "{0}\\cmd.exe", Environment.SystemDirectory);
			string text = string.Format(CultureInfo.InvariantCulture, "/C {0}", fileName);
			if (commandLine != null && commandLine.Length > 0)
			{
				text += string.Format(CultureInfo.InvariantCulture, " {0}", commandLine, CultureInfo.InvariantCulture);
			}

			ProcessStartInfo processStartInfo = new(fileName2, text)
			{
				CreateNoWindow = true,
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardInput = true,
				RedirectStandardError = true
			};
			process.StartInfo = processStartInfo;
			process.Start();
			lock (_LockObject)
			{
				if (_ExecState == EnExecState.Cancelling)
				{
					process.Kill();
					process.Dispose();
					process = null;
					return;
				}

				_runningProcess = process;
				process = null;
			}

			_readBufferCallback ??= RedirectOutputCallback;

			Stream baseStream;
			Stream baseStream2;
			lock (_AsyncLockLocal)
			{
				baseStream = _runningProcess.StandardOutput.BaseStream;
				_stdOutRedirState = new AsyncRedirectedOutputState(baseStream, _runningProcess.StandardOutput.CurrentEncoding, true);
				baseStream2 = _runningProcess.StandardError.BaseStream;
				_stdErrorRedirState = new AsyncRedirectedOutputState(baseStream2, _runningProcess.StandardError.CurrentEncoding, false);
			}

			baseStream.BeginRead(_stdOutRedirState.ReadOutputBuffer, 0, C_ReadBufferSizeInBytes, _readBufferCallback, _stdOutRedirState);
			baseStream2.BeginRead(_stdErrorRedirState.ReadOutputBuffer, 0, C_ReadBufferSizeInBytes, _readBufferCallback, _stdErrorRedirState);
			_runningProcess.WaitForExit();
			_stdOutRedirState.DoneEvent.WaitOne();
			_stdErrorRedirState.DoneEvent.WaitOne();
		}
		catch (Win32Exception ex)
		{
			Diag.Dug(ex);
			OnQEOLESQLErrorMessage(string.Format(CultureInfo.CurrentCulture, ControlsResources.ErrUnableToLaunchProcesss, fileName, commandLine), ex.Message, EnQESQLScriptProcessingMessageType.Error);
		}
		catch (Exception ex2)
		{
			Diag.Dug(ex2);
			OnQEOLESQLErrorMessage(string.Format(CultureInfo.CurrentCulture, ControlsResources.ErrUnableToExecProcess, fileName, commandLine), ex2.Message, EnQESQLScriptProcessingMessageType.Error);
		}
		finally
		{
			lock (_LockObject)
			{
				if (_runningProcess != null)
				{
					_runningProcess.Dispose();
					_runningProcess = null;
				}
			}

			lock (_AsyncLockLocal)
			{
				_stdOutRedirState = null;
				_stdErrorRedirState = null;
			}
		}
	}

	private void OnScriptProcessingError(string msg, EnQESQLScriptProcessingMessageType msgType)
	{
		// Tracer.Trace(GetType(), "QEOLESQLExec.OnScriptProcessingError", "msg = {0}", msg);
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

	private string ReadFileContent(string fileName)
	{
		// Tracer.Trace(GetType(), "QEOLESQLExec.ReadFileContent", "fileName = \"{0}\"", fileName);
		string text = null;
		if (CurrentWorkingDirectoryPath != null && !File.Exists(fileName))
		{
			try
			{
				string text2 = CurrentWorkingDirectoryPath();
				if (text2 != null)
				{
					text = Path.GetFullPath(Path.Combine(text2, fileName));
					if (!File.Exists(text))
					{
						text = null;
					}
				}
			}
			catch
			{
			}
		}

		try
		{
			text ??= Path.GetFullPath(fileName);
		}
		catch (ArgumentException)
		{
			OnScriptProcessingError(ControlsResources.ErrInvalidPathInRCmd, EnQESQLScriptProcessingMessageType.FatalError);
			return null;
		}
		catch (PathTooLongException)
		{
			OnScriptProcessingError(ControlsResources.ErrPathIsTooLongForRCmd, EnQESQLScriptProcessingMessageType.FatalError);
			return null;
		}
		catch (Exception)
		{
			OnScriptProcessingError(ControlsResources.ErrUnableToProcessRCmd, EnQESQLScriptProcessingMessageType.FatalError);
			return null;
		}

		// Tracer.Trace(GetType(), Tracer.EnLevel.Information, "ReadFileContent: full file name is \"{0}\"", text);
		StreamReader streamReader = null;
		try
		{
			streamReader = new StreamReader(text);
			string text3 = streamReader.ReadToEnd();
			if (!text3.EndsWith(Environment.NewLine, StringComparison.OrdinalIgnoreCase))
			{
				text3 += Environment.NewLine;
			}

			return text3;
		}
		catch (FileNotFoundException e)
		{
			Diag.Dug(e);
			OnScriptProcessingError(ControlsResources.ErrFileWasnotFoundForRCmd, EnQESQLScriptProcessingMessageType.FatalError);
		}
		catch (DirectoryNotFoundException e2)
		{
			Diag.Dug(e2);
			OnScriptProcessingError(ControlsResources.ErrDirWasnotFoundForRCmd, EnQESQLScriptProcessingMessageType.FatalError);
		}
		catch (Exception ex4)
		{
			Diag.Dug(ex4);
			OnScriptProcessingError(string.Format(CultureInfo.CurrentCulture, ControlsResources.ErrGenericRCmdError, ex4.Message), EnQESQLScriptProcessingMessageType.FatalError);
		}
		finally
		{
			streamReader?.Close();
		}

		return null;
	}




	EnParserAction IBBatchSource.GetMoreData(ref string str)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return GetMoreData(ref str);
	}

	EnParserAction IBCommandExecuter.Ed(string batch, ref IBBatchSource pIBatchSource)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		return Ed(batch, ref pIBatchSource);
	}

	EnParserAction IBCommandExecuter.IncludeFileName(string fileName, ref IBBatchSource ppIBatchSource)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		return IncludeFileName(fileName, ref ppIBatchSource);
	}

	EnParserAction IBVariableResolver.ResolveVariable(string varName, ref string varValue)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		return ResolveVariable(varName, ref varValue);
	}

	EnParserAction IBVariableResolver.ResolveVariableOwnership(string varName, string varValue, ref bool bTakeOwmership)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		return ResolveVariableOwnership(varName, varValue, ref bTakeOwmership);
	}
}
