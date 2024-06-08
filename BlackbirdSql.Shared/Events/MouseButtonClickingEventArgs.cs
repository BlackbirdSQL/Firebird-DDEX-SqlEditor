// Microsoft.SqlServer.GridControl, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.MouseButtonClickingEventArgs

using System;
using System.Drawing;
using System.Windows.Forms;


namespace BlackbirdSql.Shared.Events;

public class MouseButtonClickingEventArgs : EventArgs
{
	private readonly long m_RowIndex;

	private readonly int m_ColumnIndex;

	private Rectangle m_CellRect;

	private readonly Keys m_Modifiers;

	private readonly MouseButtons m_Button;

	private bool m_ShouldHandle = true;

	public long RowIndex => m_RowIndex;

	public int ColumnIndex => m_ColumnIndex;

	public Rectangle CellRect => m_CellRect;

	public Keys Modifiers => m_Modifiers;

	public MouseButtons Button => m_Button;

	public bool ShouldHandle
	{
		get
		{
			return m_ShouldHandle;
		}
		set
		{
			m_ShouldHandle = value;
		}
	}

	public MouseButtonClickingEventArgs(long nRowIndex, int nColIndex, Rectangle rCellRect, Keys mod, MouseButtons btn)
	{
		m_RowIndex = nRowIndex;
		m_ColumnIndex = nColIndex;
		m_CellRect = rCellRect;
		m_Modifiers = mod;
		m_Button = btn;
	}

	protected MouseButtonClickingEventArgs()
	{
	}
}
