#region Assembly Microsoft.SqlServer.GridControl, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLCommon\Microsoft.SqlServer.GridControl.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using BlackbirdSql.Shared.Interfaces;




// namespace Microsoft.SqlServer.Management.UI.Grid
namespace BlackbirdSql.Shared.Controls.Grid
{
	public class GridTextColumn : AbstractGridColumn
	{
		protected bool m_bVertical;

		protected StringFormat m_myStringFormat = new StringFormat(StringFormatFlags.NoWrap | StringFormatFlags.LineLimit);

		protected TextFormatFlags m_textFormat = GridConstants.DefaultTextFormatFlags;

		public GridTextColumn(GridColumnInfo ci, int nWidthInPixels, int colIndex)
			: base(ci, nWidthInPixels, colIndex)
		{
			m_myStringFormat.HotkeyPrefix = HotkeyPrefix.None;
			m_myStringFormat.Trimming = StringTrimming.EllipsisCharacter;
			m_myStringFormat.LineAlignment = StringAlignment.Center;
			SetStringFormatRTL(s_defaultRTL);
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (m_myStringFormat != null)
			{
				m_myStringFormat.Dispose();
				m_myStringFormat = null;
			}
		}

		public override void DrawCell(Graphics g, Brush bkBrush, SolidBrush textBrush, Font textFont, Rectangle rect, IBGridStorage storage, long nRowIndex)
		{
			g.FillRectangle(bkBrush, rect);
			rect.Inflate(-CELL_CONTENT_OFFSET, 0);
			if (rect.Width > 0)
			{
				if (m_bVertical)
				{
					DrawTextStringForVerticalFonts(g, textBrush, textFont, rect, storage, nRowIndex, useGdiPlus: false);
				}
				else
				{
					TextRenderer.DrawText(g, storage.GetCellDataAsString(nRowIndex, m_myColumnIndex), textFont, rect, textBrush.Color, m_textFormat);
				}
			}
		}

		public override void PrintCell(Graphics g, Brush bkBrush, SolidBrush textBrush, Font textFont, Rectangle rect, IBGridStorage storage, long nRowIndex)
		{
			g.FillRectangle(bkBrush, rect.X - 1, rect.Y, rect.Width, rect.Height);
			rect.Inflate(-CELL_CONTENT_OFFSET, 0);
			if (rect.Width > 0)
			{
				if (m_bVertical)
				{
					DrawTextStringForVerticalFonts(g, textBrush, textFont, rect, storage, nRowIndex, useGdiPlus: true);
				}
				else
				{
					g.DrawString(storage.GetCellDataAsString(nRowIndex, m_myColumnIndex), textFont, textBrush, rect, m_myStringFormat);
				}
			}
		}

		public override string GetAccessibleValue(long nRowIndex, IBGridStorage storage)
		{
			return storage.GetCellDataAsString(nRowIndex, m_myColumnIndex);
		}

		public override void SetRTL(bool bRightToLeft)
		{
			SetStringFormatRTL(bRightToLeft);
		}

		public override void ProcessNewGridFont(Font gridFont)
		{
			if (gridFont.GdiVerticalFont)
			{
				m_myStringFormat.FormatFlags |= StringFormatFlags.DirectionVertical;
				m_bVertical = true;
			}
			else
			{
				m_myStringFormat.FormatFlags &= ~StringFormatFlags.DirectionVertical;
				m_bVertical = false;
			}
		}

		private void DrawTextStringForVerticalFonts(Graphics g, SolidBrush textBrush, Font textFont, Rectangle rect, IBGridStorage storage, long nRowIndex, bool useGdiPlus)
		{
			using Matrix transform = new Matrix(0f, -1f, 1f, 0f, rect.X - rect.Y, rect.X + rect.Y + rect.Height);
			new Rectangle(rect.X, rect.Y, rect.Height, rect.Width);
			g.Transform = transform;
			if (useGdiPlus)
			{
				g.DrawString(storage.GetCellDataAsString(nRowIndex, m_myColumnIndex), textFont, textBrush, rect, m_myStringFormat);
			}
			else
			{
				TextRenderer.DrawText(g, storage.GetCellDataAsString(nRowIndex, m_myColumnIndex), textFont, rect, textBrush.Color, m_textFormat);
			}

			g.ResetTransform();
		}

		private void SetStringFormatRTL(bool bRTL)
		{
			if (bRTL)
			{
				m_myStringFormat.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
				m_textFormat |= TextFormatFlags.RightToLeft;
			}
			else
			{
				m_myStringFormat.FormatFlags &= ~StringFormatFlags.DirectionRightToLeft;
				m_textFormat &= ~TextFormatFlags.RightToLeft;
			}

			GridConstants.AdjustFormatFlagsForAlignment(ref m_textFormat, m_myAlign);
			if (m_myAlign == HorizontalAlignment.Left)
			{
				m_myStringFormat.Alignment = StringAlignment.Near;
			}
			else if (m_myAlign == HorizontalAlignment.Center)
			{
				m_myStringFormat.Alignment = StringAlignment.Center;
			}
			else
			{
				m_myStringFormat.Alignment = StringAlignment.Far;
			}
		}

		protected GridTextColumn()
		{
		}
	}
}
