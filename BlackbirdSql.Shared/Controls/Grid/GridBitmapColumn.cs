// Microsoft.SqlServer.GridControl, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.GridBitmapColumn

using System.Drawing;
using System.Windows.Forms;
using BlackbirdSql.Shared.Interfaces;



namespace BlackbirdSql.Shared.Controls.Grid;


public class GridBitmapColumn : AbstractGridColumn
{
	protected bool m_isRTL = s_defaultRTL;

	public GridBitmapColumn(GridColumnInfo ci, int nWidthInPixels, int colIndex)
		: base(ci, nWidthInPixels, colIndex)
	{
	}

	public override void DrawCell(Graphics g, Brush bkBrush, SolidBrush textBrush, Font textFont, Rectangle rect, IBsGridStorage storage, long nRowIndex)
	{
		Bitmap cellDataAsBitmap = storage.GetCellDataAsBitmap(nRowIndex, m_myColumnIndex);
		g.FillRectangle(bkBrush, rect);
		DrawBitmap(g, bkBrush, rect, cellDataAsBitmap, bEnabled: true);
	}

	public override void PrintCell(Graphics g, Brush bkBrush, SolidBrush textBrush, Font textFont, Rectangle rect, IBsGridStorage storage, long nRowIndex)
	{
		Bitmap cellDataAsBitmap = storage.GetCellDataAsBitmap(nRowIndex, m_myColumnIndex);
		g.FillRectangle(bkBrush, rect.X - 1, rect.Y, rect.Width, rect.Height);
		DrawBitmap(g, bkBrush, rect, cellDataAsBitmap, bEnabled: true);
	}

	public override void DrawDisabledCell(Graphics g, Font textFont, Rectangle rect, IBsGridStorage storage, long nRowIndex)
	{
		Bitmap cellDataAsBitmap = storage.GetCellDataAsBitmap(nRowIndex, m_myColumnIndex);
		g.FillRectangle(SDisabledCellBKBrush, rect);
		DrawBitmap(g, SDisabledCellBKBrush, rect, cellDataAsBitmap, bEnabled: false);
	}

	public override void SetRTL(bool bRightToLeft)
	{
		m_isRTL = bRightToLeft;
	}

	protected virtual void DrawBitmap(Graphics g, Brush bkBrush, Rectangle rect, Bitmap myBmp, bool bEnabled)
	{
		if (myBmp == null)
		{
			return;
		}

		Rectangle rect2 = rect;
		if (myBmp.Width < rect.Width)
		{
			if (m_myAlign == HorizontalAlignment.Center)
			{
				rect2.X = rect.X + (rect.Width - myBmp.Width) / 2;
			}
			else if (m_myAlign == HorizontalAlignment.Left && !m_isRTL || m_myAlign == HorizontalAlignment.Right && m_isRTL)
			{
				rect2.X = rect.X;
			}
			else
			{
				rect2.X = rect.Right - myBmp.Width;
			}

			rect2.Width = myBmp.Width;
		}

		if (myBmp.Height < rect.Height)
		{
			rect2.Y = rect.Y + (rect.Height - myBmp.Height) / 2 + 1;
			rect2.Height = myBmp.Height;
		}

		if (bEnabled)
		{
			g.DrawImage(myBmp, rect2);
		}
		else
		{
			ControlPaint.DrawImageDisabled(g, myBmp, rect2.X, rect2.Y, ((SolidBrush)bkBrush).Color);
		}
	}

	protected GridBitmapColumn()
	{
	}
}
