// Microsoft.SqlServer.GridControl, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.GridSpecialEventArgs

using System.Drawing;
using System.Windows.Forms;
using BlackbirdSql.Common.Controls.Enums;



namespace BlackbirdSql.Common.Controls.Events;

public sealed class GridSpecialEventArgs : MouseButtonDoubleClickedEventArgs
{
	public const int C_HyperlinkClick = 0;

	private readonly object customData;

	private readonly int eventType;

	public object Data => customData;

	public int EventType => eventType;

	public GridSpecialEventArgs(int eventType, object data, EnHitTestResult htRes, long nRowIndex, int nColIndex, Rectangle rCellRect, MouseButtons btn, EnGridButtonArea headerArea)
		: base(htRes, nRowIndex, nColIndex, rCellRect, btn, headerArea)
	{
		this.eventType = eventType;
		customData = data;
	}
}
