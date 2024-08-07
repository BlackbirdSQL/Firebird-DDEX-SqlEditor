﻿// Microsoft.SqlServer.GridControl, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.GridHyperlinkColumn

using System.Drawing;
using System.Windows.Forms;
using BlackbirdSql.Shared.Interfaces;



namespace BlackbirdSql.Shared.Controls.Grid;


public class GridHyperlinkColumn : GridTextColumn
{
	public GridHyperlinkColumn(GridColumnInfo ci, int nWidthInPixels, int colIndex)
		: base(ci, nWidthInPixels, colIndex)
	{
	}

	public override bool IsPointOverTextInCell(Point pt, Rectangle cellRect, IBsGridStorage storage, long row, Graphics g, Font f)
	{
		string cellStringToMeasure = GetCellStringToMeasure(row, storage);
		if (cellStringToMeasure != null && cellStringToMeasure.Length > 0)
		{
			cellRect.Inflate(-CELL_CONTENT_OFFSET, 0);
			Size size = TextRenderer.MeasureText(g, cellStringToMeasure, f, cellRect.Size, m_textFormat);
			pt.Y -= cellRect.Top + (cellRect.Height - size.Height) / 2;
			if (m_myAlign == HorizontalAlignment.Left)
			{
				pt.X -= cellRect.Left;
			}
			else if (m_myAlign == HorizontalAlignment.Center)
			{
				int num = cellRect.Left + (cellRect.Width - size.Width) / 2;
				pt.X -= num;
			}
			else
			{
				int num2 = cellRect.Left + (cellRect.Width - size.Width);
				pt.X -= num2;
			}

			if (pt.X >= 0 && pt.X <= size.Width && pt.Y >= 0)
			{
				return pt.Y <= size.Height;
			}

			return false;
		}

		return false;
	}

	protected virtual string GetCellStringToMeasure(long rowIndex, IBsGridStorage storage)
	{
		return string.Intern(storage.GetCellDataAsString(rowIndex, ColumnIndex));
	}
}
