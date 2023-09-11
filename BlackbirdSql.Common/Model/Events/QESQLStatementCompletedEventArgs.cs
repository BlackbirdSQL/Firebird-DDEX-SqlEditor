
using System;
using System.Data;

namespace BlackbirdSql.Common.Model.Events;


public sealed class QESQLStatementCompletedEventArgs
{
	private readonly long _RecordCount;

	private readonly bool _IsDebugging;

	private readonly bool _IsParseOnly;

	public bool IsParseOnly => _IsParseOnly;

	public long RecordCount => _RecordCount;

	public bool IsDebugging => _IsDebugging;


	public QESQLStatementCompletedEventArgs(long recordCount, bool isParseOnly, bool isDebugging)
	{
		_RecordCount = recordCount;
		_IsDebugging = isDebugging;
		_IsParseOnly = isParseOnly;
	}
}
