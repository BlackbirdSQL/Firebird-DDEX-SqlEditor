
using System;
using System.Data;
using System.Data.Common;

namespace BlackbirdSql.Common.Model.Events;


public sealed class QESQLDataLoadedEventArgs
{
	private readonly IDbCommand _Command;

	private readonly long _RecordCount;

	private readonly long _RecordsAffected;

	private readonly DateTime _ExecutionEndTime;

	private readonly bool _IsDebugging;

	private readonly bool _IsParseOnly;

	public IDbCommand Command => _Command;

	public bool IsParseOnly => _IsParseOnly;

	public long RecordCount => _RecordCount;

	public long RecordsAffected => _RecordsAffected;

	public DateTime ExecutionEndTime => _ExecutionEndTime;

	public bool IsDebugging => _IsDebugging;


	public QESQLDataLoadedEventArgs(IDbCommand command, long recordCount, long recordsAffected, DateTime executionEndTime, bool isParseOnly, bool isDebugging)
	{
		_Command = command;
		_RecordCount = recordCount;
		_RecordsAffected = recordsAffected;
		_ExecutionEndTime = executionEndTime;
		_IsDebugging = isDebugging;
		_IsParseOnly = isParseOnly;
	}
}
