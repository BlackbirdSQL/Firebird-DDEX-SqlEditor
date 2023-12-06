
using System;
using System.Data;
using System.Data.Common;

namespace BlackbirdSql.Common.Model.Events;


public sealed class QESQLDataLoadedEventArgs(IDbCommand command, long recordCount,
	long recordsAffected, DateTime executionEndTime, bool isParseOnly)
{
	private readonly IDbCommand _Command = command;
	private readonly long _RecordCount = recordCount;
	private readonly long _RecordsAffected = recordsAffected;
	private readonly DateTime _ExecutionEndTime = executionEndTime;
	private readonly bool _IsParseOnly = isParseOnly;



	public IDbCommand Command => _Command;
	public bool IsParseOnly => _IsParseOnly;
	public long RecordCount => _RecordCount;
	public long RecordsAffected => _RecordsAffected;
	public DateTime ExecutionEndTime => _ExecutionEndTime;
}
