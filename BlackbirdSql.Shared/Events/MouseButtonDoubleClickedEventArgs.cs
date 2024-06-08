// Microsoft.SqlServer.GridControl, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.MouseButtonDoubleClickedEventArgs

using System;
using System.Drawing;
using System.Windows.Forms;
using BlackbirdSql.Shared.Enums;


namespace BlackbirdSql.Shared.Events;

public class MouseButtonDoubleClickedEventArgs : EventArgs
{
	private readonly EnHitTestResult m_htRes;

	private readonly long m_RowIndex;

	private readonly int m_ColumnIndex;

	private Rectangle m_CellRect;

	private readonly MouseButtons m_Button;

	private readonly EnGridButtonArea m_headerArea;

	public EnHitTestResult HitTest => m_htRes;

	public long RowIndex => m_RowIndex;

	public int ColumnIndex => m_ColumnIndex;

	public Rectangle CellRect => m_CellRect;

	public MouseButtons Button => m_Button;

	public EnGridButtonArea HeaderArea => m_headerArea;

	public MouseButtonDoubleClickedEventArgs(EnHitTestResult htRes, long nRowIndex, int nColIndex, Rectangle rCellRect, MouseButtons btn, EnGridButtonArea headerArea)
	{
		m_htRes = htRes;
		m_RowIndex = nRowIndex;
		m_ColumnIndex = nColIndex;
		m_CellRect = rCellRect;
		m_Button = btn;
		m_headerArea = headerArea;
	}

	protected MouseButtonDoubleClickedEventArgs()
	{
	}
}
