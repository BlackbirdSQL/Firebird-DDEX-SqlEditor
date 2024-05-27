namespace BlackbirdSql.Common.Model.Events;


public sealed class QESQLStatementCompletedEventArgs(long rowsSelected, long totalRowsSelected, bool isParseOnly)
{
	private readonly long _RowsSelected = rowsSelected;
	private readonly long _TotalRowsSelected = totalRowsSelected;
	private readonly bool _IsParseOnly = isParseOnly;

	public bool IsParseOnly => _IsParseOnly;
	public long RowsSelected => _RowsSelected;
	public long TotalRowsSelected => _TotalRowsSelected;
}
