
using System;
using System.Threading;
using System.Threading.Tasks;
using BlackbirdSql.Core.Enums;
using BlackbirdSql.Sys.Enums;



namespace BlackbirdSql.Shared.Events;



public delegate Task<bool> ExecutionCompletedEventHandler(object sender, ExecutionCompletedEventArgs args);


public class ExecutionCompletedEventArgs : EventArgs
{
	public ExecutionCompletedEventArgs(EnScriptExecutionResult executionResult,
		EnSqlExecutionType executionType, EnSqlOutputMode outputMode, bool launched,
		bool withClientStats, long totalRowsSelected, int statementCount, int errorCount,
		int messageCount, CancellationToken cancelToken, CancellationToken syncToken)
	{
		_ExecutionResult = executionResult;
		_ExecutionType = executionType;
		_OutputMode = outputMode;
		_Launched = launched;
		_WithClientStats = withClientStats;
		_TotalRowsSelected = totalRowsSelected;
		_StatementCount = statementCount;
		_ErrorCount = errorCount;
		_MessageCount = messageCount;
		_CancelToken = cancelToken;
		_SyncToken = syncToken;
	}


	public ExecutionCompletedEventArgs(EnScriptExecutionResult executionResult,
			EnSqlExecutionType executionType, EnSqlOutputMode outputMode, bool launched,
			bool withClientStats, CancellationToken cancelToken, CancellationToken syncToken)
		: this(executionResult, executionType, outputMode, launched, withClientStats, 0L, 0, 0, 0,
			cancelToken, syncToken)
	{
	}

	public ExecutionCompletedEventArgs(EnScriptExecutionResult executionResult,
			EnSqlExecutionType executionType, bool launched, CancellationToken cancelToken,
			CancellationToken syncToken)
		: this(executionResult, executionType, EnSqlOutputMode.ToGrid, launched, false, 0L, 0, 0, 0,
			  cancelToken, syncToken)
	{
	}

	public ExecutionCompletedEventArgs(EnScriptExecutionResult executionResult, bool launched,
			CancellationToken cancelToken, CancellationToken syncToken)
		: this(executionResult, EnSqlExecutionType.QueryOnly, EnSqlOutputMode.ToGrid,
			launched, false, 0L, 0, 0, 0, cancelToken, syncToken)
	{
	}

	private readonly EnScriptExecutionResult _ExecutionResult;
	private readonly EnSqlExecutionType _ExecutionType;
	private readonly EnSqlOutputMode _OutputMode;
	private readonly bool _WithClientStats;
	private readonly bool _Launched;
	private readonly long _TotalRowsSelected;
	private readonly int _StatementCount;
	private readonly int _ErrorCount;
	private readonly int _MessageCount;
	private readonly CancellationToken _CancelToken;
	private readonly CancellationToken _SyncToken;
	private bool _Result = true;




	public EnScriptExecutionResult ExecutionResult => _ExecutionResult;
	public EnSqlExecutionType ExecutionType => _ExecutionType;
	public EnSqlOutputMode OutputMode => _OutputMode;
	public bool WithClientStats => _WithClientStats;
	public bool Launched => _Launched;
	public long TotalRowsSelected => _TotalRowsSelected;
	public int StatementCount => _StatementCount;
	public int ErrorCount => _ErrorCount;
	public int MessageCount => _MessageCount;
	public CancellationToken CancelToken => _CancelToken;
	public CancellationToken SyncToken => _SyncToken;

	public bool Result
	{
		get { return _Result; }
		set { _Result = value; }
	}
}
