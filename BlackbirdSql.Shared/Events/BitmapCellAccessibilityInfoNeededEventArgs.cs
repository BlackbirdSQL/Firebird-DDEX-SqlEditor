// Microsoft.SqlServer.GridControl, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.BitmapCellAccessibilityInfoNeededEventArgs

using System;
using System.Windows.Forms;



namespace BlackbirdSql.Shared.Events;


public class BitmapCellAccessibilityInfoNeededEventArgs : EventArgs
{
	public int ColumnIndex { get; private set; }

	public long RowIndex { get; private set; }

	public AccessibleRole AccessibleRole { get; set; }

	public AccessibleStates AdditionalAccessibleStates { get; set; }

	public BitmapCellAccessibilityInfoNeededEventArgs(int columnIndex, long rowIndex)
	{
		ColumnIndex = columnIndex;
		RowIndex = rowIndex;
		AccessibleRole = AccessibleRole.Graphic;
		AdditionalAccessibleStates = AccessibleStates.None;
	}
}
