// Microsoft.SqlServer.DlgGrid, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.GridCell

using System.Drawing;
using BlackbirdSql.Shared.Enums;



namespace BlackbirdSql.Shared.Controls.Grid;


internal class GridCell
{
	private SolidBrush m_textBrush;

	private SolidBrush _BgBrush;

	private int m_textCellType;

	private object m_cellData;

	private object m_tag;

	internal SolidBrush TextBrush
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

	internal SolidBrush BkBrush
	{
		get
		{
			return _BgBrush;
		}
		set
		{
			_BgBrush = value;
		}
	}

	internal int TextCellType
	{
		get
		{
			return m_textCellType;
		}
		set
		{
			m_textCellType = value;
		}
	}

	internal object CellData
	{
		get
		{
			return m_cellData;
		}
		set
		{
			m_cellData = value;
		}
	}

	internal object Tag
	{
		get
		{
			return m_tag;
		}
		set
		{
			m_tag = value;
		}
	}

	public GridCell(int textCellType, string strCellText)
	{
		m_textCellType = textCellType;
		m_cellData = strCellText;
	}

	public GridCell(string strCellText)
	{
		m_cellData = strCellText;
	}

	public GridCell(Bitmap bmp)
	{
		m_cellData = bmp;
	}

	public GridCell(string strButtonText, Bitmap buttonImage)
	{
		ButtonInfo cellData = new ButtonInfo(buttonImage, strButtonText);
		m_cellData = cellData;
	}

	public GridCell(EnGridCheckBoxState state)
	{
		m_cellData = state;
	}

	internal void Assign(GridCell cell)
	{
		if (cell != this && cell != null)
		{
			TextBrush = cell.TextBrush;
			BkBrush = cell.BkBrush;
			TextCellType = cell.TextCellType;
			CellData = cell.CellData;
			Tag = cell.Tag;
		}
	}
}
