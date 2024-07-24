// Microsoft.SqlServer.GridControl, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.HeaderButtonClickedEventArgs

using System;
using System.Windows.Forms;
using BlackbirdSql.Shared.Enums;



namespace BlackbirdSql.Shared.Events;


public delegate void HeaderButtonClickedEventHandler(object sender, HeaderButtonClickedEventArgs args);


public class HeaderButtonClickedEventArgs : EventArgs
{
	private readonly int m_ColumnIndex;

	private readonly MouseButtons m_Button;

	private bool m_RepaintWholeGrid;

	private readonly EnGridButtonArea m_headerArea;

	public int ColumnIndex => m_ColumnIndex;

	public MouseButtons Button => m_Button;

	public EnGridButtonArea HeaderArea => m_headerArea;

	public bool RepaintWholeGrid
	{
		get
		{
			return m_RepaintWholeGrid;
		}
		set
		{
			m_RepaintWholeGrid = value;
		}
	}

	public HeaderButtonClickedEventArgs(int nColIndex, MouseButtons btn, EnGridButtonArea headerArea)
	{
		m_ColumnIndex = nColIndex;
		m_Button = btn;
		m_headerArea = headerArea;
	}

	protected HeaderButtonClickedEventArgs()
	{
	}
}
