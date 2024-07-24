// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution.AdjustSelectionForButtonClickEventHandler
// Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution.AdjustSelectionForButtonClickEventArgs

using System;



namespace BlackbirdSql.Shared.Events;


public delegate void AdjustSelectionForButtonClickEventHandler(object sender, AdjustSelectionForButtonClickEventArgs e);


public sealed class AdjustSelectionForButtonClickEventArgs : EventArgs
{
	private readonly int _columnIndex = -1;

	private readonly long _rowIndex = -1L;

	public long RowIndex => _rowIndex;

	public int ColumnIndex => _columnIndex;

	public AdjustSelectionForButtonClickEventArgs(long rowIndex, int columnIndex)
	{
		_rowIndex = rowIndex;
		_columnIndex = columnIndex;
	}
}
