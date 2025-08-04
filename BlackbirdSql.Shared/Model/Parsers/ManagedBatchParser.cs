// Microsoft.SqlServer.BatchParser, Version=16.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// ManagedBatchParser.Parser
using System;
using System.Threading;
using System.Threading.Tasks;
using BlackbirdSql.Core.Enums;
using BlackbirdSql.Shared.Enums;
using BlackbirdSql.Shared.Events;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Sys.Enums;
using BlackbirdSql.Sys.Interfaces;
using BlackbirdSql.Sys.Model;
using Microsoft.VisualStudio.Threading;



namespace BlackbirdSql.Shared.Model.Parsers;



internal class ManagedBatchParser : IDisposable
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


	internal void ParserDispose()
	{
		Cleanup();
	}



	internal void Cleanup(IBsQueryManager qryMgr = null, /* IBsQESQLBatchConsumer batchConsumer = null, */
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
	private string _Delimiter = ";";




	internal void SetParseMode(EnParseMode pm)
	{
		_ParseMode = pm;
		_ = _ParseMode;
	}

	internal void SetRecognizeOnlyVariables(bool bRecognizeOnlyVariables)
	{
		_RecognizeOnlyVariables = bRecognizeOnlyVariables;
		_ = _RecognizeOnlyVariables;
	}

	internal void SetBatchDelimiter(string strBatchDelimiter)
	{
		_Delimiter = strBatchDelimiter;
		_ = _Delimiter;
	}

	internal void DisableVariableSubstitution()
	{
		_SubstitutionEnabled = false;
		_ = _SubstitutionEnabled;
	}

	internal void SetCommandExecuter(IBsCommandExecuter pICommandExecuter)
	{
		_Executor = pICommandExecuter;
	}


	internal async Task<bool> ParseAsync(CancellationToken cancelToken, CancellationToken syncToken)
	{
		// Evs.Trace(GetType(), nameof(ParseAsync));

		_BatchParser = NativeDbBatchParserProxy.CreateInstance(_ExecutionType, _QryMgr, _Script);

		// ------------------------------------------------------------------------------- //
		// ************ Execution Point (6) - ManagedBatchParser.ParseAsync() ************ //
		// ------------------------------------------------------------------------------- //

		if (await _Executor.BatchParseCallbackAsync(_BatchParser, cancelToken, syncToken) != EnScriptExecutionResult.Success)
			return !cancelToken.Cancelled();


		IBsNativeDbStatementWrapper sqlStatement = null;
		EnParserAction result = EnParserAction.Continue;

		while (_BatchParser.CurrentAction != EnSqlStatementAction.Completed && !cancelToken.Cancelled())
		{

			while (result == EnParserAction.Continue && !cancelToken.Cancelled())
			{
				result = _BatchParser.GetNextStatement(ref sqlStatement);

				if (result != EnParserAction.Continue)
					break;


				_Current++;

				// Evs.Trace(GetType(), nameof(ParseAsync), "Calling BatchStatementCallbackAsync() for ParserAction: {0}, Statement: {1}.",
				//	_BatchParser.CurrentAction, _BatchParser.Current);

				// ------------------------------------------------------------------------------- //
				// ************ Execution Point (9) - ManagedBatchParser.ParseAsync() ************ //
				// ------------------------------------------------------------------------------- //
				result = await _Executor.BatchStatementCallbackAsync(sqlStatement, 1, cancelToken, syncToken);

				// Evs.Trace(GetType(), nameof(ParseAsync), "Done Calling BatchStatementCallbackAsync() for ParserAction: {0}, Statement: {1}.",
				//	_BatchParser.CurrentAction, _BatchParser.Current);

			}


			// Evs.Trace(GetType(), nameof(ParseAsync), "Calling OnBatchDataLoaded() for ParserAction: {0}, Statement: {1}.",
			//	_BatchParser.CurrentAction, _BatchParser.Current);

			if (result == EnParserAction.Abort)
				break;

			// Call statistics output
			if (!syncToken.Cancelled())
				RaiseDataLoadedEvent(cancelToken);

			if (cancelToken.Cancelled())
				break;

			// Evs.Trace(GetType(), nameof(Parse), "AdvanceToNextAction for ParserAction: {0}, Statement: {1}.",
			//	_BatchParser.CurrentAction, _BatchParser.Current);

			_BatchParser.AdvanceToNextAction();


			result = EnParserAction.Continue;
		}

		// Evs.Trace(GetType(), nameof(ParseAsync), "Completed");

		return !cancelToken.Cancelled();
	}



	internal int GetLastCommandLineNumber()
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


	private void RaiseDataLoadedEvent(CancellationToken cancelToken)
	{
		QueryDataEventArgs args = new(_ExecutionType, cancelToken.Cancelled() ? EnSqlStatementAction.Cancelled : _BatchParser.CurrentAction,
			/* _OutputMode, _QryMgr.IsWithActualPlan, */ _QryMgr.IsWithClientStats, _BatchParser.TotalRowsSelected,
			_BatchParser.StatementCount, /* _BatchConsumer.CurrentErrorCount, _BatchConsumer.CurrentMessageCount, */
			_QryMgr.QueryExecutionStartTime, DateTime.Now);

		_Executor.OnBatchDataLoaded(this, args);
	}

}
