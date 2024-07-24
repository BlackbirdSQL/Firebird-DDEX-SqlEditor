// Microsoft.SqlServer.GridControl, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.CustomizeCellGDIObjectsEventHandler
// Microsoft.SqlServer.Management.UI.Grid.CustomizeCellGDIObjectsEventArgs

using System;
using System.Drawing;



namespace BlackbirdSql.Shared.Events;


public delegate void CustomizeCellGDIObjectsEventHandler(object sender, CustomizeCellGDIObjectsEventArgs args);


public class CustomizeCellGDIObjectsEventArgs : EventArgs
{
	private long m_rowIndex;

	private int m_columnIndex;

	private SolidBrush m_bkBrush;

	private SolidBrush m_textBrush;

	private Font m_cellFont;

	public long RowIndex => m_rowIndex;

	public int ColumnIndex => m_columnIndex;

	public SolidBrush BKBrush
	{
		get
		{
			return m_bkBrush;
		}
		set
		{
			m_bkBrush = value;
		}
	}

	public SolidBrush TextBrush
	{
		get
		{
			return m_textBrush;
		}
		set
		{
			m_textBrush = value;
		}
	}

	public Font CellFont
	{
		get
		{
			return m_cellFont;
		}
		set
		{
			m_cellFont = value;
		}
	}

	public void SetRowAndColumn(long nRowIndex, int nColIndex)
	{
		m_rowIndex = nRowIndex;
		m_columnIndex = nColIndex;
	}
}
