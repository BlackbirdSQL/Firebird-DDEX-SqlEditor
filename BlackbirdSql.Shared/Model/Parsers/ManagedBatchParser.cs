// Microsoft.SqlServer.BatchParser, Version=16.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// ManagedBatchParser.Parser
using System;
using System.Threading;
using System.Threading.Tasks;
using BlackbirdSql.Shared.Enums;
using BlackbirdSql.Shared.Events;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Sys.Enums;
using BlackbirdSql.Sys.Interfaces;
using BlackbirdSql.Sys.Model;
using Microsoft.VisualStudio.Threading;



namespace BlackbirdSql.Shared.Model.Parsers;



public class ManagedBatchParser : IDisposable
{
	public ManagedBatchParser(IBsQueryManager qryMgr, /* IBsQESQLBatchConsumer batchConsumer, */
		EnSqlExecutionType executionType, /* EnSqlOutputMode outputMode, */ string script)
	{
		_QryMgr = qryMgr;
		// _BatchConsumer = batchConsumer;
		_ExecutionType = executionType;
		// _OutputMode = outputMode;
		_Script = script;
	}

	~ManagedBatchParser()
	{
		ParserDispose();
	}

	/*
	private bool IsDisposed()
	{
		return true;
	}
	*/


	public void ParserDispose()
	{
		Cleanup();
	}



	public void Cleanup(IBsQueryManager qryMgr = null, /* IBsQESQLBatchConsumer batchConsumer = null, */
		EnSqlExecutionType executionType = EnSqlExecutionType.QueryOnly,
		/* EnSqlOutputMode outputMode = EnSqlOutputMode.ToGrid , */ string script = null)
	{
		_QryMgr = qryMgr;
		// _BatchConsumer = batchConsumer;
		_ExecutionType = executionType;
		// _OutputMode = outputMode;
		_Script = script;
		_BatchParser?.Dispose();
		_BatchParser = null;
		_Current = -1;
	}





	EnParseMode _ParseMode = EnParseMode.RecognizeAll;

	private string _Script;
	private IBsCommandExecuter _Executor = null;
	private EnSqlExecutionType _ExecutionType;
	// private EnSqlOutputMode _OutputMode;
	private IBsNativeDbBatchParser _BatchParser = null;

	private IBsQueryManager _QryMgr;
	//m IBsQESQLBatchConsumer _BatchConsumer;

	private int _Current = -1;
	private bool _SubstitutionEnabled = true;
	private bool _RecognizeOnlyVariables = false;
	private string _Delimiter = null;




	public void SetParseMode(EnParseMode pm)
	{
		_ParseMode = pm;
		_ = _ParseMode;
	}

	public void SetRecognizeOnlyVariables(bool bRecognizeOnlyVariables)
	{
		_RecognizeOnlyVariables = bRecognizeOnlyVariables;
		_ = _RecognizeOnlyVariables;
	}

	public void SetBatchDelimiter(string strBatchDelimiter)
	{
		_Delimiter = strBatchDelimiter;
		_ = _Delimiter;
	}

	public void DisableVariableSubstitution()
	{
		_SubstitutionEnabled = false;
		_ = _SubstitutionEnabled;
	}

	public void SetCommandExecuter(IBsCommandExecuter pICommandExecuter)
	{
		_Executor = pICommandExecuter;
	}


	public async Task<bool> ParseAsync(CancellationToken cancelToken)
	{
		// Tracer.Trace(GetType(), "ParseAsync()");

		_BatchParser = NativeDbBatchParserProxy.CreateInstance(_ExecutionType, _QryMgr, _Script);
		_BatchParser.Parse();

		RaiseScriptParsedEvent(cancelToken);

		IBsNativeDbStatementWrapper sqlStatement = null;
		EnParserAction result = EnParserAction.Continue;

		while (_BatchParser.CurrentAction != EnSqlStatementAction.Completed && !cancelToken.IsCancellationRequested)
		{

			while (result == EnParserAction.Continue && !cancelToken.IsCancellationRequested)
			{
				result = _BatchParser.GetNextStatement(ref sqlStatement);

				if (result != EnParserAction.Continue)
					break;


				_Current++;

				// Tracer.Trace(GetType(), "ParseAsync()", "Calling BatchStatementCallbackAsync() for ParserAction: {0}, Statement: {1}.",
				//	_BatchParser.CurrentAction, _BatchParser.Current);

				// ------------------------------------------------------------------------------- //
				// ******************** Execution Point (6) - ManagedBatchParser.ParseAsync() ******************** //
				// ------------------------------------------------------------------------------- //
				result = await _Executor.BatchStatementCallbackAsync(sqlStatement, 1, cancelToken);

				// Tracer.Trace(GetType(), "ParseAsync()", "Done Calling BatchStatementCallbackAsync() for ParserAction: {0}, Statement: {1}.",
				//	_BatchParser.CurrentAction, _BatchParser.Current);

			}


			// Tracer.Trace(GetType(), "ParseAsync()", "Calling OnBatchDataLoaded() for ParserAction: {0}, Statement: {1}.",
			//	_BatchParser.CurrentAction, _BatchParser.Current);

			if (result == EnParserAction.Abort)
				break;

			// Call statistics output
			RaiseDataLoadedEvent(cancelToken);

			if (cancelToken.IsCancellationRequested)
				break;

			// Tracer.Trace(GetType(), "Parse()", "AdvanceToNextAction for ParserAction: {0}, Statement: {1}.",
			//	_BatchParser.CurrentAction, _BatchParser.Current);

			_BatchParser.AdvanceToNextAction();


			result = EnParserAction.Continue;
		}

		// Tracer.Trace(GetType(), "ParseAsync()", "Completed");

		return !cancelToken.IsCancellationRequested;
	}



	public int GetLastCommandLineNumber()
	{
		return _Current;
	}

	protected virtual void Dispose(bool isDisposing)
	{
		if (isDisposing)
			ParserDispose();
	}

	public virtual void Dispose()
	{
		Dispose(true);
		// GC.SuppressFinalize(this);
	}

	private void RaiseScriptParsedEvent(CancellationToken cancelToken)
	{
		QueryDataEventArgs args = new(_ExecutionType,
			cancelToken.IsCancellationRequested ? EnSqlStatementAction.Cancelled : _BatchParser.CurrentAction,
			_QryMgr.IsWithClientStats, _BatchParser.TotalRowsSelected, _BatchParser.StatementCount,
			_QryMgr.QueryExecutionStartTime, DateTime.Now);

		_Executor.OnBatchScriptParsed(this, args);
	}

	private void RaiseDataLoadedEvent(CancellationToken cancelToken)
	{
		QueryDataEventArgs args = new(_ExecutionType, cancelToken.IsCancellationRequested ? EnSqlStatementAction.Cancelled : _BatchParser.CurrentAction,
			/* _OutputMode, _QryMgr.IsWithActualPlan, */ _QryMgr.IsWithClientStats, _BatchParser.TotalRowsSelected,
			_BatchParser.StatementCount, /* _BatchConsumer.CurrentErrorCount, _BatchConsumer.CurrentMessageCount, */
			_QryMgr.QueryExecutionStartTime, DateTime.Now);

		_Executor.OnBatchDataLoaded(this, args);
	}

}
