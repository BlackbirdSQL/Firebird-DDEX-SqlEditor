// Microsoft.SqlServer.BatchParser, Version=16.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// ManagedBatchParser.Parser
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using BlackbirdSql.Common.Model.Enums;
using BlackbirdSql.Common.Model.Interfaces;
using BlackbirdSql.Core.Ctl.Enums;
using BlackbirdSql.Sys;
using Microsoft.VisualStudio.Threading;



namespace BlackbirdSql.Common.Model.Parsers;



public class ManagedBatchParser : IDisposable
{
	public ManagedBatchParser(IBQueryManager qryMgr, IBQESQLBatchConsumer batchConsumer, EnSqlExecutionType executionType, EnSqlOutputMode outputMode, string script)
	{
		_QryMgr = qryMgr;
		_BatchConsumer = batchConsumer;
		_ExecutionType = executionType;
		_OutputMode = outputMode;
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



	public void Cleanup(IBQueryManager qryMgr = null, IBQESQLBatchConsumer batchConsumer = null, EnSqlExecutionType executionType = EnSqlExecutionType.QueryOnly,
		EnSqlOutputMode outputMode = EnSqlOutputMode.ToGrid, string script = null)
	{
		_QryMgr = qryMgr;
		_BatchConsumer = batchConsumer;
		_ExecutionType = executionType;
		_OutputMode = outputMode;
		_Script = script;
		_BatchParser?.Dispose();
		_BatchParser = null;
		_Current = -1;
	}





	EnParseMode _ParseMode = EnParseMode.RecognizeAll;

	private string _Script;
	private IBCommandExecuter _Executor = null;
	private IBVariableResolver _Resolver = null;
	private EnSqlExecutionType _ExecutionType;
	private EnSqlOutputMode _OutputMode;
	private IBsNativeDbBatchParser _BatchParser = null;

	private IBQueryManager _QryMgr;
	IBQESQLBatchConsumer _BatchConsumer;

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

	public void SetCommandExecuter(IBCommandExecuter pICommandExecuter)
	{
		_Executor = pICommandExecuter;
	}

	public void SetVariableResolver(IBVariableResolver pIVariableResolver)
	{
		_Resolver = pIVariableResolver;
		_ = _Resolver;
	}


	public bool Parse()
	{
		_BatchParser = NativeDbBatchParserProxy.CreateInstance(_ExecutionType, _QryMgr, _Script);
		_BatchParser.Parse();

		IBsNativeDbStatementWrapper sqlStatement = null;
		EnParserAction result = EnParserAction.Continue;

		while (_BatchParser.CurrentAction != EnSqlStatementAction.Completed && !_BatchParser.Cancelled)
		{

			while (result == EnParserAction.Continue && !_BatchParser.Cancelled)
			{
				result = _BatchParser.GetNextStatement(ref sqlStatement);

				if (result != EnParserAction.Continue)
					break;


				_Current++;

				// ------------------------------------------------------------------------------- //
				// ******************** Execution Point (6) - ManagedBatchParser.ExecuteScript() ******************** //
				// ------------------------------------------------------------------------------- //
				_Executor.ProcessParsedBatchStatement(sqlStatement, 1);
			}

			// Call statistics output
			_Executor.OnBatchDataLoaded(this, new(_ExecutionType,
				_BatchParser.Cancelled ? EnSqlStatementAction.Cancelled : _BatchParser.CurrentAction,
				_OutputMode, _QryMgr.IsWithActualPlan, _QryMgr.IsWithClientStats, _BatchParser.TotalRowsSelected,
				_BatchConsumer.CurrentErrorCount, _BatchConsumer.CurrentMessageCount,
				_QryMgr.QueryExecutionStartTime, DateTime.Now));

			if (_BatchParser.Cancelled)
				break;

			_BatchParser.AdvanceToNextAction();


			result = EnParserAction.Continue;
		}


		return true;
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
}
