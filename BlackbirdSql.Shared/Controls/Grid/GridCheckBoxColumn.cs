#region Assembly Microsoft.SqlServer.GridControl, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLCommon\Microsoft.SqlServer.GridControl.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System.Drawing;
using System.Windows.Forms;
using BlackbirdSql.Shared.Enums;
using BlackbirdSql.Shared.Interfaces;

// namespace Microsoft.SqlServer.Management.UI.Grid
namespace BlackbirdSql.Shared.Controls.Grid
{
	public class GridCheckBoxColumn : GridBitmapColumn
	{
		protected Bitmap m_CheckedBitmap;

		protected Bitmap m_UncheckedBitmap;

		protected Bitmap m_IntermidiateBitmap;

		protected Bitmap m_DisabledBitmap;

		protected int m_CurrentCheckSize = 13;

		public int CheckBoxHeight => m_CurrentCheckSize;

		protected GridCheckBoxColumn()
		{
		}

		public GridCheckBoxColumn(GridColumnInfo ci, int nWidthInPixels, int colIndex)
			: base(ci, nWidthInPixels, colIndex)
		{
			m_CheckedBitmap = GridConstants.CheckedCheckBoxBitmap;
			m_UncheckedBitmap = GridConstants.UncheckedCheckBoxBitmap;
			m_IntermidiateBitmap = GridConstants.IntermediateCheckBoxBitmap;
			m_DisabledBitmap = GridConstants.DisabledCheckBoxBitmap;
			CalcCheckboxSize();
		}

		private void CalcCheckboxSize()
		{
			m_CurrentCheckSize = m_CheckedBitmap.Size.Height;
			if (m_UncheckedBitmap.Size.Height > m_CurrentCheckSize)
			{
				m_CurrentCheckSize = m_UncheckedBitmap.Size.Height;
			}

			if (m_IntermidiateBitmap.Size.Height > m_CurrentCheckSize)
			{
				m_CurrentCheckSize = m_IntermidiateBitmap.Size.Height;
			}

			if (m_DisabledBitmap.Size.Height > m_CurrentCheckSize)
			{
				m_CurrentCheckSize = m_DisabledBitmap.Size.Height;
			}
		}

		public Bitmap BitmapFromGridCheckBoxState(EnGridCheckBoxState state)
		{
			Bitmap result = null;
			switch (state)
			{
				case EnGridCheckBoxState.Checked:
					result = m_CheckedBitmap;
					break;
				case EnGridCheckBoxState.Unchecked:
					result = m_UncheckedBitmap;
					break;
				case EnGridCheckBoxState.Indeterminate:
					result = m_IntermidiateBitmap;
					break;
				case EnGridCheckBoxState.Disabled:
					result = m_DisabledBitmap;
					break;
				default:
					_ = 4;
					break;
			}

			return result;
		}

		public void SetCheckboxBitmaps(Bitmap checkedState, Bitmap uncheckedState, Bitmap indeterminateState, Bitmap disabledState)
		{
			if (checkedState != null)
			{
				m_CheckedBitmap = checkedState;
			}

			if (uncheckedState != null)
			{
				m_UncheckedBitmap = uncheckedState;
			}

			if (indeterminateState != null)
			{
				m_IntermidiateBitmap = indeterminateState;
			}

			if (disabledState != null)
			{
				m_DisabledBitmap = disabledState;
			}

			CalcCheckboxSize();
		}

		public override void DrawCell(Graphics g, Brush bkBrush, SolidBrush textBrush, Font textFont, Rectangle rect, IBGridStorage storage, long nRowIndex)
		{
			EnGridCheckBoxState cellDataForCheckBox = storage.GetCellDataForCheckBox(nRowIndex, m_myColumnIndex);
			g.FillRectangle(bkBrush, rect);
			GridCheckBox.DrawCheckbox(g, rect, m_myAlign, m_isRTL, cellDataForCheckBox, bEnabled: true);
		}

		public override void PrintCell(Graphics g, Brush bkBrush, SolidBrush textBrush, Font textFont, Rectangle rect, IBGridStorage storage, long nRowIndex)
		{
			EnGridCheckBoxState cellDataForCheckBox = storage.GetCellDataForCheckBox(nRowIndex, m_myColumnIndex);
			g.FillRectangle(bkBrush, rect.X - 1, rect.Y, rect.Width, rect.Height);
			GridCheckBox.DrawCheckbox(g, rect, m_myAlign, m_isRTL, cellDataForCheckBox, bEnabled: true);
		}

		public override void DrawDisabledCell(Graphics g, Font textFont, Rectangle rect, IBGridStorage storage, long nRowIndex)
		{
			EnGridCheckBoxState cellDataForCheckBox = storage.GetCellDataForCheckBox(nRowIndex, m_myColumnIndex);
			g.FillRectangle(SDisabledCellBKBrush, rect);
			GridCheckBox.DrawCheckbox(g, rect, m_myAlign, m_isRTL, cellDataForCheckBox, bEnabled: false);
		}

		public override AccessibleStates GetAccessibleState(long nRowIndex, IBGridStorage storage)
		{
			return storage.GetCellDataForCheckBox(nRowIndex, m_myColumnIndex) switch
			{
				EnGridCheckBoxState.Checked => AccessibleStates.Checked,
				EnGridCheckBoxState.Unchecked => AccessibleStates.None,
				EnGridCheckBoxState.Indeterminate => AccessibleStates.Mixed,
				_ => base.GetAccessibleState(nRowIndex, storage),
			};
		}

		public override string GetAccessibleValue(long nRowIndex, IBGridStorage storage)
		{
			return storage.GetCellDataForCheckBox(nRowIndex, m_myColumnIndex).ToString();
		}
	}
}
