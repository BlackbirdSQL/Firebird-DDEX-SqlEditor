// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ResultPane.StatisticsDlgGridControl

using System.Drawing;
using BlackbirdSql.Shared.Controls.Grid;



namespace BlackbirdSql.Shared.Controls.Results;


internal class StatisticsDlgGridControl : DlgGridControl
{
	private SolidBrush _bkBrush;

	private SolidBrush _fgBrush;

	private SolidBrush _selectedCellBrush;

	private SolidBrush _InactiveSelectedCellBrush;

	private SolidBrush _highlightedCellBrush;

	public StatisticsDlgGridControl(Color bkColor, Color fgColor, Color selectedCellColor, Color inactiveSelectedCellColor, Color highlightedCellColor)
	{
		_bkBrush = new SolidBrush(bkColor);
		_fgBrush = new SolidBrush(fgColor);
		_selectedCellBrush = new SolidBrush(selectedCellColor);
		_InactiveSelectedCellBrush = new SolidBrush(inactiveSelectedCellColor);
		_highlightedCellBrush = new SolidBrush(highlightedCellColor);
		BackColor = bkColor;
	}

	protected override void GetCellGDIObjects(AbstractGridColumn gridColumn, long nRow, int nCol, ref SolidBrush bkBrush, ref SolidBrush textBrush)
	{
		base.GetCellGDIObjects(gridColumn, nRow, nCol, ref bkBrush, ref textBrush);
		bkBrush = _bkBrush;
		textBrush = _fgBrush;
		GridCell cellInfo = GetCellInfo((int)nRow, nCol);
		if (cellInfo.BkBrush != null && cellInfo.BkBrush.Color == SystemColors.Control)
		{
			bkBrush = _highlightedCellBrush;
		}
		if (_SelectionMgr.IsCellSelected(nRow, nCol) && ContainsFocus)
		{
			bkBrush = _selectedCellBrush;
		}
		else if (_SelectionMgr.IsCellSelected(nRow, nCol) && !ContainsFocus)
		{
			bkBrush = _InactiveSelectedCellBrush;
		}
	}

	internal void SetBkAndForeColors(Color bkColor, Color foreColor)
	{
		if (_bkBrush != null)
		{
			_bkBrush.Dispose();
			_bkBrush = null;
		}
		if (_fgBrush != null)
		{
			_fgBrush.Dispose();
			_fgBrush = null;
		}
		_bkBrush = new SolidBrush(bkColor);
		_fgBrush = new SolidBrush(foreColor);
		BackColor = bkColor;
		if (IsHandleCreated)
		{
			Invalidate();
		}
	}

	internal void SetSelectedCellColor(Color selectedCellFocusedColor)
	{
		_selectedCellBrush?.Dispose();
		_selectedCellBrush = new SolidBrush(selectedCellFocusedColor);
		if (IsHandleCreated)
		{
			Invalidate();
		}
	}

	internal void SetInactiveSelectedCellColor(Color inactiveSelectedCellFocusedColor)
	{
		_InactiveSelectedCellBrush?.Dispose();
		_InactiveSelectedCellBrush = new SolidBrush(inactiveSelectedCellFocusedColor);
		if (IsHandleCreated)
		{
			Invalidate();
		}
	}

	internal void SetHighlightedCellColor(Color highlightedCellColor)
	{
		_highlightedCellBrush?.Dispose();
		_highlightedCellBrush = new SolidBrush(highlightedCellColor);
		if (IsHandleCreated)
		{
			Invalidate();
		}
	}

	protected override void Dispose(bool bDisposing)
	{
		base.Dispose(bDisposing);
		if (bDisposing)
		{
			if (_bkBrush != null)
			{
				_bkBrush.Dispose();
				_bkBrush = null;
			}
			if (_fgBrush != null)
			{
				_fgBrush.Dispose();
				_fgBrush = null;
			}
			if (_selectedCellBrush != null)
			{
				_selectedCellBrush.Dispose();
				_selectedCellBrush = null;
			}
			if (_InactiveSelectedCellBrush != null)
			{
				_InactiveSelectedCellBrush.Dispose();
				_InactiveSelectedCellBrush = null;
			}
			if (_highlightedCellBrush != null)
			{
				_highlightedCellBrush.Dispose();
				_highlightedCellBrush = null;
			}
		}
	}
}
