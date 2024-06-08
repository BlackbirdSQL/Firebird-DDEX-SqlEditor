namespace BlackbirdSql.Shared.Events;


public sealed class QESQLStatementCompletedEventArgs(int statementIndex, long rowsSelected, long totalRowsSelected, bool isParseOnly)
{
	private readonly int _StatementIndex = statementIndex;
	private readonly long _RowsSelected = rowsSelected;
	private readonly long _TotalRowsSelected = totalRowsSelected;
	private readonly bool _IsParseOnly = isParseOnly;

	public bool IsParseOnly => _IsParseOnly;
	public int StatementIndex => _StatementIndex;
	public long RowsSelected => _RowsSelected;
	public long TotalRowsSelected => _TotalRowsSelected;
}
