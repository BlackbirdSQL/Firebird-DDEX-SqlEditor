// Microsoft.SqlServer.GridControl, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.MouseButtonClickedEventArgs

using System;
using System.Drawing;
using System.Windows.Forms;



namespace BlackbirdSql.Shared.Events;


public delegate void MouseButtonClickedEventHandler(object sender, MouseButtonClickedEventArgs args);


public class MouseButtonClickedEventArgs : EventArgs
{
	private readonly long m_RowIndex;

	private readonly int m_ColumnIndex;

	private Rectangle m_CellRect;

	private readonly MouseButtons m_Button;

	private bool m_ShouldRedraw = true;

	public long RowIndex => m_RowIndex;

	public int ColumnIndex => m_ColumnIndex;

	public Rectangle CellRect => m_CellRect;

	public MouseButtons Button => m_Button;

	public bool ShouldRedraw
	{
		get
		{
			return m_ShouldRedraw;
		}
		set
		{
			m_ShouldRedraw = value;
		}
	}

	public MouseButtonClickedEventArgs(long nRowIndex, int nColIndex, Rectangle rCellRect, MouseButtons btn)
	{
		m_RowIndex = nRowIndex;
		m_ColumnIndex = nColIndex;
		m_CellRect = rCellRect;
		m_Button = btn;
	}

	protected MouseButtonClickedEventArgs()
	{
	}
}
