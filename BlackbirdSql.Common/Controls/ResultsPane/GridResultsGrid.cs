#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using BlackbirdSql.Common.Ctl;
using BlackbirdSql.Common.Controls.Grid;
using BlackbirdSql.Common.Controls.Enums;
using BlackbirdSql.Common.Controls.Interfaces;
using BlackbirdSql.Common.Model.Events;
using BlackbirdSql.Common.Model.Interfaces;
using BlackbirdSql.Common.Controls.QueryExecution;
using BlackbirdSql.Common.Properties;

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using BlackbirdSql.Core.Ctl.Diagnostics;

namespace BlackbirdSql.Common.Controls.ResultsPane;


public class GridResultsGrid : GridControl, IGridControl2, IGridControl, IStatusBarContributer
{
	private class GridHyperlinkColumnWithLimit : GridHyperlinkColumn
	{
		protected int m_maxNumOfChars = CommonUtils.DefaultMaxCharsPerColumnForGrid;

		protected StringBuilder m_cellSB = new StringBuilder(256);

		protected string m_cellValue;

		protected SolidBrush originalTextBrush;

		protected bool isRealHyperlink;

		protected Font cachedGridFont;

		public GridHyperlinkColumnWithLimit(GridColumnInfo ci, int nWidthInPixels, int colIndex, bool isRealHyperlink)
			: base(ci, nWidthInPixels, colIndex)
		{
			this.isRealHyperlink = isRealHyperlink;
			if (this.isRealHyperlink)
			{
				originalTextBrush = TextBrush;
			}
		}

		public void SetMaxNumOfChars(int numOfChars)
		{
			if (numOfChars > 0)
			{
				m_maxNumOfChars = Math.Min(numOfChars, 43679);
			}
		}

		protected void AdjustCellRect(ref Rectangle r)
		{
			r.Inflate(-CELL_CONTENT_OFFSET, 0);
		}

		protected static bool ShouldTreatXmlStringAsHyperlink(string xmlValue)
		{
			if (xmlValue != null && xmlValue.Length == 4 && string.Compare(xmlValue, "null", StringComparison.OrdinalIgnoreCase) == 0)
			{
				return false;
			}

			return true;
		}

		public static void AdjustCellString(StringBuilder cellString, bool removeCR)
		{
			int length = cellString.Length;
			for (int i = 0; i < length; i++)
			{
				if (removeCR && (cellString[i] == '\r' || cellString[i] == '\n' || cellString[i] == '\t'))
				{
					cellString[i] = ' ';
				}
				else if (cellString[i] == '\0')
				{
					cellString.Length = i;
					break;
				}
			}
		}

		public override void ProcessNewGridFont(Font gridFont)
		{
			base.ProcessNewGridFont(gridFont);
			cachedGridFont = gridFont;
		}

		public void SetNewTextBrush(SolidBrush br)
		{
			if (isRealHyperlink)
			{
				originalTextBrush = br;
			}
		}

		public override void DrawCell(Graphics g, Brush bkBrush, SolidBrush textBrush, Font textFont, Rectangle rect, IGridStorage storage, long nRowIndex)
		{
			g.FillRectangle(bkBrush, rect);
			AdjustCellRect(ref rect);
			if (rect.Width <= 0)
			{
				return;
			}

			m_cellSB.Length = 0;
			m_cellSB.Append(storage.GetCellDataAsString(nRowIndex, m_myColumnIndex));
			if (m_cellSB.Length > m_maxNumOfChars)
			{
				m_cellSB.Length = m_maxNumOfChars;
			}

			AdjustCellString(m_cellSB, removeCR: true);
			m_cellValue = m_cellSB.ToString().TrimEnd(null);
			if (isRealHyperlink)
			{
				if (!ShouldTreatXmlStringAsHyperlink(m_cellValue))
				{
					TextRenderer.DrawText(g, m_cellValue, cachedGridFont, rect, originalTextBrush.Color, m_textFormat);
					return;
				}

				Color color = GetVSHyperLinkColor();
				if (color == Color.Empty)
				{
					color = textBrush.Color;
				}

				TextRenderer.DrawText(g, m_cellValue, textFont, rect, color, m_textFormat);
			}
			else
			{
				TextRenderer.DrawText(g, m_cellValue, textFont, rect, textBrush.Color, m_textFormat);
			}
		}

		public override void PrintCell(Graphics g, Brush bkBrush, SolidBrush textBrush, Font textFont, Rectangle rect, IGridStorage storage, long nRowIndex)
		{
			g.FillRectangle(bkBrush, rect.X - 1, rect.Y, rect.Width, rect.Height);
			AdjustCellRect(ref rect);
			if (rect.Width <= 0)
			{
				return;
			}

			m_cellSB.Length = 0;
			m_cellSB.Append(storage.GetCellDataAsString(nRowIndex, m_myColumnIndex));
			if (m_cellSB.Length > m_maxNumOfChars)
			{
				m_cellSB.Length = m_maxNumOfChars;
			}

			AdjustCellString(m_cellSB, removeCR: true);
			m_cellValue = m_cellSB.ToString().TrimEnd(null);
			if (isRealHyperlink)
			{
				if (!ShouldTreatXmlStringAsHyperlink(m_cellValue))
				{
					g.DrawString(m_cellValue, cachedGridFont, originalTextBrush, rect, m_myStringFormat);
					return;
				}

				Color vSHyperLinkColor = GetVSHyperLinkColor();
				SolidBrush solidBrush = !(vSHyperLinkColor == Color.Empty) ? new SolidBrush(vSHyperLinkColor) : textBrush;
				g.DrawString(m_cellValue, textFont, solidBrush, rect, m_myStringFormat);
				solidBrush.Dispose();
			}
			else
			{
				g.DrawString(m_cellSB.ToString(), textFont, textBrush, rect, m_myStringFormat);
			}
		}

		protected override string GetCellStringToMeasure(long rowIndex, IGridStorage storage)
		{
			m_cellSB.Length = 0;
			m_cellSB.Append(storage.GetCellDataAsString(rowIndex, ColumnIndex));
			if (m_cellSB.Length > m_maxNumOfChars)
			{
				m_cellSB.Length = m_maxNumOfChars;
			}

			AdjustCellString(m_cellSB, removeCR: true);
			m_cellValue = m_cellSB.ToString().TrimEnd(null);
			if (isRealHyperlink && ShouldTreatXmlStringAsHyperlink(m_cellValue))
			{
				return m_cellValue;
			}

			return null;
		}

		private Color GetVSHyperLinkColor()
		{
			if (Package.GetGlobalService(typeof(SVsUIShell)) is IVsUIShell2 vsUIShell
				&& Native.Succeeded(vsUIShell.GetVSSysColorEx(-29, out var pdwRGBval)))
			{
				return ColorTranslator.FromWin32((int)pdwRGBval);
			}

			return Color.Empty;
		}
	}

	private SolidBrush m_bkBrush = new SolidBrush(SystemColors.Window);

	private SolidBrush m_foreBrush = new SolidBrush(SystemColors.WindowText);

	private SolidBrush _selectedCellFocusedBrush = new SolidBrush(SystemColors.Highlight);

	private SolidBrush _inactiveSelectedCellBrush = new SolidBrush(SystemColors.InactiveCaption);

	private readonly StringBuilder cellStringForMeasue = new(50);

	private static string s_xmlCellTooltip;

	private static string s_selectAllCellsTooltip;

	private static string s_selectWholeRowTooltip;

	private static string s_selectWholeColumnTooltip;

	private int _NumberOfCharsToShow = CommonUtils.DefaultMaxCharsPerColumnForGrid;

	private readonly StringBuilder m_sbCustomClipboardText = new(CommonUtils.DefaultMaxCharsPerColumnForGrid);

	private bool m_includeColumnHeadersForDnD;

	private bool m_forceHeaders;

	public bool IncludeHeadersOnDragAndDrop
	{
		get
		{
			return m_includeColumnHeadersForDnD;
		}
		set
		{
			m_includeColumnHeadersForDnD = value;
		}
	}

	public bool ForceHeadersOnDragAndDrop
	{
		get
		{
			return m_forceHeaders;
		}
		set
		{
			m_forceHeaders = value;
		}
	}

	public int NumberOfCharsToShow
	{
		get
		{
			return _NumberOfCharsToShow;
		}
		set
		{
			_NumberOfCharsToShow = Math.Min(value, 43679);
			foreach (GridColumn column in m_Columns)
			{
				(column as GridHyperlinkColumnWithLimit)?.SetMaxNumOfChars(_NumberOfCharsToShow);
			}
		}
	}

	public BlockOfCells CurrentSelectedBlock
	{
		get
		{
			if (m_selMgr.CurrentSelectionBlockIndex < 0)
			{
				return new BlockOfCells(0L, 0)
				{
					Width = 0,
					Height = 0L
				};
			}

			return m_selMgr.SelectedBlocks[m_selMgr.CurrentSelectionBlockIndex];
		}
	}

	public SolidBrush BackGroundBrush => m_bkBrush;

	public event AdjustSelectionForButtonClickEventHandler AdjustSelectionForButtonClickEvent;

	private static void PreloadStringsFromResources()
	{
		s_xmlCellTooltip = ControlsResources.GridXMLCellTooltip;
		s_selectAllCellsTooltip = ControlsResources.GridSelectAllCellsTooltip;
		s_selectWholeRowTooltip = ControlsResources.GridSelectWholeColumnTooltip;
		s_selectWholeColumnTooltip = ControlsResources.GridSelectWholeRowTooltip;
	}

	static GridResultsGrid()
	{
		s_xmlCellTooltip = "";
		s_selectAllCellsTooltip = "";
		s_selectWholeRowTooltip = "";
		s_selectWholeColumnTooltip = "";
		PreloadStringsFromResources();
	}

	public GridResultsGrid()
	{
		Tracer.Trace(GetType(), "GridResultsGrid.GridResultsGrid", "", null);
	}

	void IStatusBarContributer.GetColumnAndRowNumber(out long rowNumber, out long columnNumber)
	{
		GetCurrentCell(out rowNumber, out var columnIndex);
		columnNumber = columnIndex;
		rowNumber++;
	}

	public override Size GetPreferredSize(Size proposedSize)
	{
		long rowCount = GridStorage != null ? GridStorage.RowCount : 0;
		if (rowCount == 0L)
		{
			rowCount = 1L;
		}

		if (rowCount > CommonUtils.DefaultInitialMinNumberOfVisibleRows)
		{
			rowCount = CommonUtils.DefaultInitialMinNumberOfVisibleRows;
		}

		if (rowCount <= CommonUtils.DefaultInitialMinNumberOfVisibleRows)
		{
			bool flag = false;
			if (IsHandleCreated)
			{
				Native.SCROLLINFO sCROLLINFO = new Native.SCROLLINFO(bInitWithAllMask: true);
				Native.GetScrollInfo(Handle, 0, sCROLLINFO);
				flag = sCROLLINFO.nMax > sCROLLINFO.nPage;
			}

			proposedSize.Height = (int)rowCount * (RowHeight + 1) + HeaderHeight + 1 + CommonUtils.GetExtraSizeForBorderStyle(BorderStyle);
			if (flag)
			{
				proposedSize.Height += SystemInformation.HorizontalScrollBarHeight;
			}
		}

		return proposedSize;
	}

	protected override GridTextColumn AllocateTextColumn(GridColumnInfo ci, int nWidthInPixels, int colIndex)
	{
		GridHyperlinkColumnWithLimit gridHyperlinkColumnWithLimit = new GridHyperlinkColumnWithLimit(ci, nWidthInPixels, colIndex, isRealHyperlink: false);
		gridHyperlinkColumnWithLimit.SetMaxNumOfChars(_NumberOfCharsToShow);
		return gridHyperlinkColumnWithLimit;
	}

	protected override GridButtonColumn AllocateButtonColumn(GridColumnInfo ci, int nWidthInPixels, int colIndex)
	{
		GridButtonColumn gridButtonColumn = base.AllocateButtonColumn(ci, nWidthInPixels, colIndex);
		if (gridButtonColumn != null && colIndex == 0)
		{
			gridButtonColumn.IsLineIndexButton = true;
		}

		return gridButtonColumn;
	}

	protected override GridHyperlinkColumn AllocateHyperlinkColumn(GridColumnInfo ci, int nWidthInPixels, int colIndex)
	{
		Tracer.Trace(GetType(), "GridHyperlinkColumn.AllocateHyperlinkColumn", "ci = {0}, nWidthInPixels = {1}, colIndex = {2}", ci, nWidthInPixels, colIndex);
		GridHyperlinkColumnWithLimit gridHyperlinkColumnWithLimit = new GridHyperlinkColumnWithLimit(ci, nWidthInPixels, colIndex, isRealHyperlink: true);
		LinkLabel linkLabel = new LinkLabel();
		gridHyperlinkColumnWithLimit.TextBrush = new SolidBrush(linkLabel.LinkColor);
		gridHyperlinkColumnWithLimit.SetMaxNumOfChars(_NumberOfCharsToShow);
		return gridHyperlinkColumnWithLimit;
	}

	protected override bool OnTooltipDataNeeded(EnHitTestResult ht, long rowNumber, int colNumber, ref string toolTipText)
	{
		switch (ht)
		{
			case EnHitTestResult.HyperlinkCell:
				toolTipText = s_xmlCellTooltip;
				return true;
			case EnHitTestResult.HeaderButton:
				if (colNumber == 0)
				{
					toolTipText = s_selectAllCellsTooltip;
					return true;
				}

				toolTipText = s_selectWholeRowTooltip;
				return true;
			case EnHitTestResult.ButtonCell:
				if (colNumber == 0)
				{
					toolTipText = s_selectWholeColumnTooltip;
					return true;
				}

				break;
		}

		return false;
	}

	protected override void Dispose(bool bDisposing)
	{
		base.Dispose(bDisposing);
		DisposeBrushes();
	}

	protected override void GetCellGDIObjects(GridColumn gridColumn, long nRow, int nCol, ref SolidBrush bkBrush, ref SolidBrush textBrush)
	{
		base.GetCellGDIObjects(gridColumn, nRow, nCol, ref bkBrush, ref textBrush);
		if (!SystemInformation.HighContrast && nCol > 0)
		{
			if (m_Columns[nCol].ColumnType != 5)
			{
				textBrush = m_foreBrush;
			}

			if (m_selMgr.IsCellSelected(nRow, nCol) && ContainsFocus)
			{
				bkBrush = _selectedCellFocusedBrush;
			}
			else if (m_selMgr.IsCellSelected(nRow, nCol) && !ContainsFocus)
			{
				bkBrush = _inactiveSelectedCellBrush;
			}
		}
	}

	protected override void OnMouseDown(MouseEventArgs mevent)
	{
		if (mevent.Button == MouseButtons.Right && !ContainsFocus)
		{
			Focus();
		}

		base.OnMouseDown(mevent);
	}

	protected override string GetCellStringForResizeToShowAll(long rowIndex, int storageColIndex, out TextFormatFlags tff)
	{
		string cellStringForResizeToShowAll = base.GetCellStringForResizeToShowAll(rowIndex, storageColIndex, out tff);
		if (cellStringForResizeToShowAll == null)
		{
			return cellStringForResizeToShowAll;
		}

		cellStringForMeasue.Length = 0;
		cellStringForMeasue.Append(cellStringForResizeToShowAll);
		GridHyperlinkColumnWithLimit.AdjustCellString(cellStringForMeasue, removeCR: true);
		if (cellStringForMeasue.Length > _NumberOfCharsToShow)
		{
			cellStringForMeasue.Length = _NumberOfCharsToShow;
		}

		return cellStringForMeasue.ToString().TrimEnd(null);
	}

	protected override void OnMouseButtonDoubleClicked(EnHitTestResult htArea, long rowIndex, int colIndex, Rectangle cellRect, MouseButtons btn, EnGridButtonArea headerArea)
	{
		Tracer.Trace(GetType(), "SqlManagerUIDlgGrid.OnMouseButtonDoubleClicked", "", null);
		if (m_gridStorage != null && htArea == EnHitTestResult.ColumnResize && btn == MouseButtons.Left && (m_Columns[colIndex].ColumnType == 2 || m_Columns[colIndex].ColumnType == 1))
		{
			Tracer.Trace(GetType(), Tracer.Level.Verbose, "GridResultsGrid.OnMouseButtonDoubleClicked", "auto resizing column {0}", colIndex);
			ResizeColumnToShowAllContentsInternal(colIndex);
		}
	}

	protected override void OnKeyPressedOnCell(long nCurRow, int nCurCol, Keys key, Keys mod)
	{
		base.OnKeyPressedOnCell(nCurRow, nCurCol, key, mod);
		if (mod == Keys.Control && key == Keys.C)
		{
			Clipboard.SetDataObject(GetDataObject(bOnlyCurrentSelBlock: true));
		}
	}

	protected override string GetTextBasedColumnStringForClipboardText(long rowIndex, int colIndex)
	{
		m_sbCustomClipboardText.Length = 0;
		m_sbCustomClipboardText.Append(m_gridStorage.GetCellDataAsString(rowIndex, m_Columns[colIndex].ColumnIndex));
		if (m_Columns[colIndex].ColumnType != 5)
		{
			GridHyperlinkColumnWithLimit.AdjustCellString(m_sbCustomClipboardText, removeCR: true);
			if (m_sbCustomClipboardText.Length > _NumberOfCharsToShow)
			{
				m_sbCustomClipboardText.Length = _NumberOfCharsToShow;
			}
		}

		return m_sbCustomClipboardText.ToString();
	}

	protected override bool OnBeforeGetClipboardTextForCells(StringBuilder clipboardText, long startRow, long endRow, int startCol, int endCol)
	{
		if (m_forceHeaders || m_includeColumnHeadersForDnD && startCol >= 0 && startCol < endCol)
		{
			for (int i = startCol; i <= endCol; i++)
			{
				if (i == startCol)
				{
					clipboardText.Append(m_gridHeader[i].Text);
				}
				else
				{
					clipboardText.AppendFormat("{0}{1}", ColumnsSeparator, m_gridHeader[i].Text);
				}
			}

			clipboardText.Append(NewLineCharacters);
		}

		return false;
	}

	protected override bool AdjustSelectionForButtonCellMouseClick()
	{
		if (AdjustSelectionForButtonClickEvent != null)
		{
			AdjustSelectionForButtonClickEvent(this, new (m_captureTracker.RowIndex, m_captureTracker.ColumnIndex));
			return true;
		}

		return base.AdjustSelectionForButtonCellMouseClick();
	}

	public void SetSelectedCellsAndCurrentCell(BlockOfCellsCollection cells, long currentRow, int currentColumn)
	{
		SelectedCellsInternal(cells, bSet: true);
		m_selMgr.SetCurrentCell(currentRow, currentColumn);
	}

	public void HandleMouseWheelDirectly(IntPtr wParam, IntPtr lParam)
	{
		Point p = new Point((short)(int)lParam, (int)lParam >> 16);
		p = PointToClient(p);
		OnMouseWheel(new MouseEventArgs(MouseButtons.None, 0, p.X, p.Y, (int)wParam >> 16));
	}

	public void SetBkAndForeColors(Color bkColor, Color foreColor)
	{
		m_bkBrush?.Dispose();

		m_foreBrush?.Dispose();

		m_bkBrush = new SolidBrush(bkColor);
		m_foreBrush = new SolidBrush(foreColor);
		BackColor = bkColor;
		foreach (GridColumn column in m_Columns)
		{
			(column as GridHyperlinkColumnWithLimit)?.SetNewTextBrush(m_foreBrush);
		}

		if (IsHandleCreated)
		{
			Invalidate();
		}
	}

	public void SetSelectedCellColor(Color selectedCellFocusedColor)
	{
		_selectedCellFocusedBrush?.Dispose();

		_selectedCellFocusedBrush = new SolidBrush(selectedCellFocusedColor);
		if (IsHandleCreated)
		{
			Invalidate();
		}
	}

	public void SetInactiveSelectedCellColor(Color inactiveSelectedCellFocusedColor)
	{
		_inactiveSelectedCellBrush?.Dispose();

		_inactiveSelectedCellBrush = new SolidBrush(inactiveSelectedCellFocusedColor);
		if (IsHandleCreated)
		{
			Invalidate();
		}
	}

	public void SetIncludeHeadersOnDragAndDrop(bool includeHeaders)
	{
		m_includeColumnHeadersForDnD = includeHeaders;
	}

	private void DisposeBrushes()
	{
		if (m_bkBrush != null)
		{
			m_bkBrush.Dispose();
			m_bkBrush = null;
		}

		if (m_foreBrush != null)
		{
			m_foreBrush.Dispose();
			m_foreBrush = null;
		}

		if (_selectedCellFocusedBrush != null)
		{
			_selectedCellFocusedBrush.Dispose();
			_selectedCellFocusedBrush = null;
		}

		if (_inactiveSelectedCellBrush != null)
		{
			_inactiveSelectedCellBrush.Dispose();
			_inactiveSelectedCellBrush = null;
		}
	}

	public void InitialColumnResize()
	{
		if (InvokeRequired)
		{
			Invoke(new MethodInvoker(InitialColumnResizeInternal));
		}
		else
		{
			InitialColumnResizeInternal();
		}
	}

	private void InitialColumnResizeInternal()
	{
		if (!Visible)
		{
			if (!IsHandleCreated)
			{
				CreateHandle();
			}

			int numberOfCharsToShow = NumberOfCharsToShow;
			NumberOfCharsToShow = CommonUtils.DefaultInitialMaxCharsPerColumnForGrid;
			for (int i = 1; i < ColumnsNumber; i++)
			{
				ResizeColumnToShowAllContents(i);
			}

			NumberOfCharsToShow = numberOfCharsToShow;
			if (Parent != null && GetPreferredSize(Size).Height != Height)
			{
				(Parent.Parent as MultiControlPanel)?.ResizeControlsToPreferredHeight(limitMaxControlHeightToClientArea: false);
			}

			Show();
		}
	}

	bool IGridControl2.Focus()
	{
		return Focus();
	}

	bool IGridControl2.ContainsFocus => ContainsFocus;
}
