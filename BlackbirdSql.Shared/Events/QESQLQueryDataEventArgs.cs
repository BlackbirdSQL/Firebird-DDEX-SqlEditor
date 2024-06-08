
using System;
using BlackbirdSql.Core.Enums;
using BlackbirdSql.Sys.Enums;

namespace BlackbirdSql.Shared.Events;


public sealed class QESQLQueryDataEventArgs
{
	public QESQLQueryDataEventArgs(EnSqlExecutionType executionType,
		EnSqlStatementAction statementAction, EnSqlOutputMode outputMode,
		bool withActualPlan, bool withClientStats, long totalRowsSelected,
		int statementCount, int errorCount, int messageCount,
		DateTime? executionStartTime, DateTime? executionEndTime)
	{
		_ExecutionType = executionType;
		_StatementAction = statementAction;
		_OutputMode = outputMode;
		_WithActualPlan = withActualPlan;
		_WithClientStats = withClientStats;
		_TotalRowsSelected = totalRowsSelected;
		_StatementCount = statementCount;
		_ErrorCount = errorCount;
		_MessageCount = messageCount;
		_ExecutionStartTime = executionStartTime;
		_ExecutionEndTime = executionEndTime;
	}


	private readonly EnSqlExecutionType _ExecutionType;
	private readonly EnSqlStatementAction _StatementAction;
	private readonly EnSqlOutputMode _OutputMode;
	private readonly bool _WithActualPlan;
	private readonly bool _WithClientStats;
	private readonly long _TotalRowsSelected;
	private readonly int _ErrorCount;
	private readonly int _StatementCount;
	private readonly int _MessageCount;

	private readonly DateTime? _ExecutionStartTime;
	private readonly DateTime? _ExecutionEndTime;




	public EnSqlExecutionType ExecutionType => _ExecutionType;
	public EnSqlStatementAction StatementAction => _StatementAction;
	public EnSqlOutputMode OutputMode => _OutputMode;
	public bool WithActualPlan => _WithActualPlan;
	public bool WithClientStats => _WithClientStats;
	public long TotalRowsSelected => _TotalRowsSelected;
	public int StatementCount => _StatementCount;
	public int ErrorCount => _ErrorCount;
	public int MessageCount => _MessageCount;
	public DateTime? ExecutionStartTime => _ExecutionStartTime;
	public DateTime? ExecutionEndTime => _ExecutionEndTime;
}
