#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;

// namespace Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution
namespace BlackbirdSql.Common.Model.Events
{
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
}
