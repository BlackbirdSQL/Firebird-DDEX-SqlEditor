
using System;
using BlackbirdSql.Core.Enums;
using BlackbirdSql.Sys.Enums;



namespace BlackbirdSql.Shared.Events;


public delegate void QueryExecutionCompletedEventHandler(object sender, QueryExecutionCompletedEventArgs args);


public class QueryExecutionCompletedEventArgs : EventArgs
{
	public QueryExecutionCompletedEventArgs(EnScriptExecutionResult executionResult,
		EnSqlExecutionType executionType, EnSqlOutputMode outputMode, bool withClientStats,
		long totalRowsSelected, int statementCount, int errorCount, int messageCount)
	{
		_ExecutionResult = executionResult;
		_ExecutionType = executionType;
		_OutputMode = outputMode;
		_WithClientStats = withClientStats;
		_TotalRowsSelected = totalRowsSelected;
		_StatementCount = statementCount;
		_ErrorCount = errorCount;
		_MessageCount = messageCount;
	}


	public QueryExecutionCompletedEventArgs(EnScriptExecutionResult executionResult,
			EnSqlExecutionType executionType, EnSqlOutputMode outputMode, bool withClientStats)
		: this(executionResult, executionType, outputMode, withClientStats, 0L, 0, 0, 0)
	{
	}

	public QueryExecutionCompletedEventArgs(EnScriptExecutionResult executionResult, EnSqlExecutionType executionType)
	: this(executionResult, executionType, EnSqlOutputMode.ToGrid, false, 0L, 0, 0, 0)
	{
	}

	public QueryExecutionCompletedEventArgs(EnScriptExecutionResult executionResult)
	: this(executionResult, EnSqlExecutionType.QueryOnly, EnSqlOutputMode.ToGrid, false, 0L, 0, 0, 0)
	{
	}

	private readonly EnScriptExecutionResult _ExecutionResult;
	private readonly EnSqlExecutionType _ExecutionType;
	private readonly EnSqlOutputMode _OutputMode;
	private readonly bool _WithClientStats;
	private readonly long _TotalRowsSelected;
	private readonly int _StatementCount;
	private readonly int _ErrorCount;
	private readonly int _MessageCount;




	public EnScriptExecutionResult ExecutionResult => _ExecutionResult;
	public EnSqlExecutionType ExecutionType => _ExecutionType;
	public EnSqlOutputMode OutputMode => _OutputMode;
	public long TotalRowsSelected => _TotalRowsSelected;
	public bool WithClientStats => _WithClientStats;
	public int StatementCount => _StatementCount;
	public int ErrorCount => _ErrorCount;
	public int MessageCount => _MessageCount;

}
