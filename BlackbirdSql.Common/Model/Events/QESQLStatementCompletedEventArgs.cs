
using System;
using System.Data;

namespace BlackbirdSql.Common.Model.Events;


public sealed class QESQLStatementCompletedEventArgs(long recordCount, bool isParseOnly)
{
	private readonly long _RecordCount = recordCount;
	private readonly bool _IsParseOnly = isParseOnly;

	public bool IsParseOnly => _IsParseOnly;
	public long RecordCount => _RecordCount;
}
