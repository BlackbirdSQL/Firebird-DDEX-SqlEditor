#region Assembly Microsoft.SqlServer.GridControl, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLCommon\Microsoft.SqlServer.GridControl.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System.Drawing;
using System.Windows.Forms;
using BlackbirdSql.Common.Controls.Enums;
using BlackbirdSql.Common.Controls.Interfaces;




// namespace Microsoft.SqlServer.Management.UI.Grid
namespace BlackbirdSql.Common.Controls.Grid
{
	public class GridButtonColumn : GridTextColumn
	{
		private class ButtonWithForcedState
		{
			public long RowIndex = -1L;

			public ButtonState State;
		}

		private bool bGridHasLines = true;

		private bool bLineIndex;

		private readonly ButtonWithForcedState m_forcedButton = new();

		public bool IsLineIndexButton
		{
			get
			{
				return bLineIndex;
			}
			set
			{
				bLineIndex = value;
			}
		}

		public GridButtonColumn(GridColumnInfo ci, int nWidthInPixels, int colIndex)
			: base(ci, nWidthInPixels, colIndex)
		{
		}

		public void SetGridLinesMode(bool withLines)
		{
			bGridHasLines = withLines;
		}

		public void SetForcedButtonState(long rowIndex, ButtonState state)
		{
			if (rowIndex == -1)
			{
				m_forcedButton.RowIndex = -1L;
				return;
			}

			m_forcedButton.RowIndex = rowIndex;
			m_forcedButton.State = state;
		}

		public override void DrawCell(Graphics g, Brush bkBrush, SolidBrush textBrush, Font textFont, Rectangle rect, IBGridStorage storage, long nRowIndex)
		{
			DrawCellCommon(g, bkBrush, textBrush, textFont, rect, storage, nRowIndex, bEnabled: true, useGdiPlus: false);
		}

		public override void PrintCell(Graphics g, Brush bkBrush, SolidBrush textBrush, Font textFont, Rectangle rect, IBGridStorage storage, long nRowIndex)
		{
			DrawCellCommon(g, bkBrush, textBrush, textFont, rect, storage, nRowIndex, bEnabled: true, useGdiPlus: true);
		}

		public override void DrawDisabledCell(Graphics g, Font textFont, Rectangle rect, IBGridStorage storage, long nRowIndex)
		{
			DrawCellCommon(g, SDisabledCellBKBrush, SDisabledCellForeBrush, textFont, rect, storage, nRowIndex, bEnabled: false);
		}

		public override string GetAccessibleValue(long nRowIndex, IBGridStorage storage)
		{
			storage.GetCellDataForButton(nRowIndex, m_myColumnIndex, out _, out _, out string buttonLabel);
			return buttonLabel;
		}

		public void DrawButton(Graphics g, Brush bkBrush, SolidBrush textBrush, Font textFont, Rectangle rect, Bitmap buttomBmp, string buttonLabel, ButtonState btnState, bool bEnabled, bool useGdiPlus)
		{
			AdjustButtonRect(ref rect);
			g.FillRectangle(bkBrush, rect);
			if (useGdiPlus)
			{
				GridButton.Paint(g, rect, btnState, buttonLabel, buttomBmp, m_myAlign, m_myTextBmpLayout, bEnabled, textFont, textBrush, m_myStringFormat, (m_myStringFormat.FormatFlags & StringFormatFlags.DirectionRightToLeft) > 0, IsLineIndexButton ? EnGridButtonType.LineNumber : EnGridButtonType.Normal);
			}
			else
			{
				GridButton.Paint(g, rect, btnState, buttonLabel, buttomBmp, m_myAlign, m_myTextBmpLayout, bEnabled, textFont, textBrush, m_textFormat, (m_textFormat & TextFormatFlags.RightToLeft) > TextFormatFlags.Default, IsLineIndexButton ? EnGridButtonType.LineNumber : EnGridButtonType.Normal);
			}
		}

		public void DrawButton(Graphics g, Brush bkBrush, SolidBrush textBrush, Font textFont, Rectangle rect, Bitmap buttomBmp, string buttonLabel, ButtonState btnState, bool bEnabled)
		{
			DrawButton(g, bkBrush, textBrush, textFont, rect, buttomBmp, buttonLabel, btnState, bEnabled, useGdiPlus: false);
		}

		public void DrawButton(Graphics g, Brush bkBrush, Brush textBrush, Font textFont, Rectangle rect, Bitmap buttomBmp, string buttonLabel, ButtonState btnState, bool bEnabled)
		{
			DrawButton(g, bkBrush, (SolidBrush)textBrush, textFont, rect, buttomBmp, buttonLabel, btnState, bEnabled);
		}

		protected void DrawCellCommon(Graphics g, Brush bkBrush, Brush textBrush, Font textFont, Rectangle rect, IBGridStorage storage, long nRowIndex, bool bEnabled)
		{
			DrawCellCommon(g, bkBrush, (SolidBrush)textBrush, textFont, rect, storage, nRowIndex, bEnabled);
		}

		protected void DrawCellCommon(Graphics g, Brush bkBrush, SolidBrush textBrush, Font textFont, Rectangle rect, IBGridStorage storage, long nRowIndex, bool bEnabled)
		{
			DrawCellCommon(g, bkBrush, textBrush, textFont, rect, storage, nRowIndex, bEnabled, useGdiPlus: false);
		}

		protected void DrawCellCommon(Graphics g, Brush bkBrush, SolidBrush textBrush, Font textFont, Rectangle rect, IBGridStorage storage, long nRowIndex, bool bEnabled, bool useGdiPlus)
		{
			ButtonState btnState = ButtonState.Normal;
			storage.GetCellDataForButton(nRowIndex, m_myColumnIndex, out var state, out Bitmap image, out string buttonLabel);
			if (state != EnButtonCellState.Empty)
			{
				switch (state)
				{
					case EnButtonCellState.Pushed:
						btnState = ButtonState.Pushed;
						break;
					case EnButtonCellState.Disabled:
						btnState = ButtonState.Inactive;
						bEnabled = false;
						break;
				}

				if (nRowIndex == m_forcedButton.RowIndex)
				{
					btnState = m_forcedButton.State;
				}

				DrawButton(g, bkBrush, textBrush, textFont, rect, image, buttonLabel, btnState, bEnabled, useGdiPlus);
			}
			else
			{
				AdjustButtonRect(ref rect);
				g.FillRectangle(bkBrush, rect);
			}
		}

		public override void ProcessNewGridFont(Font gridFont)
		{
		}

		private void AdjustButtonRect(ref Rectangle rect)
		{
			if (bGridHasLines)
			{
				rect.Width--;
				rect.Y++;
				rect.Height--;
			}
		}

		protected GridButtonColumn()
		{
		}
	}
}
