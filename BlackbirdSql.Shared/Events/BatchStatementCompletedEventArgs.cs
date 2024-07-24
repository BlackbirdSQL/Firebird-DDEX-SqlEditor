// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution.QESQLBatchStatementCompletedEventHandler
// Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution.QESQLBatchStatementCompletedEventArgs

namespace BlackbirdSql.Shared.Events;



public delegate void BatchStatementCompletedEventHandler(object sender, BatchStatementCompletedEventArgs e);


public sealed class BatchStatementCompletedEventArgs(int statementIndex, long rowsSelected, long totalRowsSelected, bool isParseOnly)
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
