// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ResultPane.GridResultsGrid

using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using BlackbirdSql.Shared.Controls.Grid;
using BlackbirdSql.Shared.Ctl;
using BlackbirdSql.Shared.Enums;
using BlackbirdSql.Shared.Events;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Shared.Properties;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;



namespace BlackbirdSql.Shared.Controls.Results;


// =========================================================================================================
//
//										GridResultsGrid Class
//
/// <summary>
/// Grid results control.
/// </summary>
// =========================================================================================================
public class GridResultsGrid : GridControl, IBsGridControl2, IBsGridControl, IBsStatusBarContributer
{

	// ------------------------------------------
	#region Constructors / Destructors - GridResultsGrid
	// ------------------------------------------


	public GridResultsGrid()
	{
		// Tracer.Trace(GetType(), "GridResultsGrid.GridResultsGrid", "", null);
	}



	static GridResultsGrid()
	{
		_S_XmlCellTooltip = "";
		_S_SelectAllCellsTooltip = "";
		_S_SelectWholeRowTooltip = "";
		_S_SelectWholeColumnTooltip = "";

		PreloadStringsFromResources();
	}



	protected override void Dispose(bool bDisposing)
	{
		base.Dispose(bDisposing);
		DisposeBrushes();
	}



	private void DisposeBrushes()
	{
		if (_BgBrush != null)
		{
			_BgBrush.Dispose();
			_BgBrush = null;
		}

		if (_FgBrush != null)
		{
			_FgBrush.Dispose();
			_FgBrush = null;
		}

		if (_SelectedCellFocusedBrush != null)
		{
			_SelectedCellFocusedBrush.Dispose();
			_SelectedCellFocusedBrush = null;
		}

		if (_InactiveSelectedCellBrush != null)
		{
			_InactiveSelectedCellBrush.Dispose();
			_InactiveSelectedCellBrush = null;
		}
	}


	#endregion Constructors / Destructors




	// =========================================================================================================
	#region Fields - GridResultsGrid
	// =========================================================================================================


	private SolidBrush _BgBrush = new SolidBrush(SystemColors.Window);
	private readonly StringBuilder _CellMetricsString = new(50);
	private SolidBrush _FgBrush = new SolidBrush(SystemColors.WindowText);
	private bool _ForceHeadersOnDragAndDrop;
	private SolidBrush _InactiveSelectedCellBrush = new SolidBrush(SystemColors.InactiveCaption);
	private bool _IncludeHeadersOnDragAndDrop;
	private int _NumberOfCharsToShow = SharedConstants.C_DefaultMaxCharsPerColumnForGrid;
	private readonly StringBuilder _SbCustomClipboardText = new(SharedConstants.C_DefaultMaxCharsPerColumnForGrid);
	private SolidBrush _SelectedCellFocusedBrush = new SolidBrush(SystemColors.Highlight);

	private static string _S_SelectAllCellsTooltip;
	private static string _S_SelectWholeColumnTooltip;
	private static string _S_SelectWholeRowTooltip;
	private static string _S_XmlCellTooltip;


	#endregion Fields





	// =========================================================================================================
	#region Property Accessors - GridResultsGrid
	// =========================================================================================================


	public SolidBrush BgBrush => _BgBrush;


	public BlockOfCells CurrentSelectedBlock
	{
		get
		{
			if (_SelectionMgr.CurrentSelectionBlockIndex < 0)
			{
				return new BlockOfCells(0L, 0)
				{
					Width = 0,
					Height = 0L
				};
			}

			return _SelectionMgr.SelectedBlocks[_SelectionMgr.CurrentSelectionBlockIndex];
		}
	}


	public bool ForceHeadersOnDragAndDrop
	{
		get { return _ForceHeadersOnDragAndDrop; }
		set { _ForceHeadersOnDragAndDrop = value; }
	}


	public bool IncludeHeadersOnDragAndDrop
	{
		get { return _IncludeHeadersOnDragAndDrop; }
		set { _IncludeHeadersOnDragAndDrop = value; }
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
			foreach (AbstractGridColumn column in m_Columns)
			{
				(column as GridHyperlinkColumnWithLimit)?.SetMaxNumOfChars(_NumberOfCharsToShow);
			}
		}
	}



	// bool IBsGridControl2.ContainsFocus => ContainsFocus;



	public event AdjustSelectionForButtonClickEventHandler AdjustSelectionForButtonClickEvent;



	#endregion Property Accessors





	// =========================================================================================================
	#region Methods - GridResultsGrid
	// =========================================================================================================


	private static void PreloadStringsFromResources()
	{
		_S_XmlCellTooltip = ControlsResources.Grid_XMLCellTooltip;
		_S_SelectAllCellsTooltip = ControlsResources.Grid_SelectAllCellsTooltip;
		_S_SelectWholeRowTooltip = ControlsResources.Grid_SelectWholeColumnTooltip;
		_S_SelectWholeColumnTooltip = ControlsResources.Grid_SelectWholeRowTooltip;
	}



	void IBsStatusBarContributer.GetColumnAndRowNumber(out long rowNumber, out long columnNumber)
	{
		GetCurrentCell(out rowNumber, out var columnIndex);
		columnNumber = columnIndex;
		rowNumber++;
	}



	public override Size GetPreferredSize(Size proposedSize)
	{
		long rowCount = GridStorage != null ? GridStorage.RowCount : 0;

		if (rowCount == 0L)
			rowCount = 1L;

		if (rowCount > SharedConstants.C_DefaultInitialMinNumberOfVisibleRows)
			rowCount = SharedConstants.C_DefaultInitialMinNumberOfVisibleRows;

		if (rowCount <= SharedConstants.C_DefaultInitialMinNumberOfVisibleRows)
		{
			bool flag = false;
			if (IsHandleCreated)
			{
				Native.SCROLLINFOEx sCROLLINFO = new (bInitWithAllMask: true);
				Native.GetScrollInfo(Handle, 0, sCROLLINFO);
				flag = sCROLLINFO.nMax > sCROLLINFO.nPage;
			}

			proposedSize.Height = (int)rowCount * (RowHeight + 1) + HeaderHeight + 1 + Cmd.GetExtraSizeForBorderStyle(BorderStyle);
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
		// Tracer.Trace(GetType(), "GridHyperlinkColumn.AllocateHyperlinkColumn", "ci = {0}, nWidthInPixels = {1}, colIndex = {2}", ci, nWidthInPixels, colIndex);
		GridHyperlinkColumnWithLimit gridHyperlinkColumnWithLimit = new GridHyperlinkColumnWithLimit(ci, nWidthInPixels, colIndex, isRealHyperlink: true);
		LinkLabel linkLabel = new LinkLabel();
		gridHyperlinkColumnWithLimit.TextBrush = new SolidBrush(linkLabel.LinkColor);
		gridHyperlinkColumnWithLimit.SetMaxNumOfChars(_NumberOfCharsToShow);
		return gridHyperlinkColumnWithLimit;
	}



	protected override void GetCellGDIObjects(AbstractGridColumn gridColumn, long nRow, int nCol, ref SolidBrush bkBrush, ref SolidBrush textBrush)
	{
		base.GetCellGDIObjects(gridColumn, nRow, nCol, ref bkBrush, ref textBrush);
		if (!SystemInformation.HighContrast && nCol > 0)
		{
			if (m_Columns[nCol].ColumnType != 5)
			{
				textBrush = _FgBrush;
			}

			if (_SelectionMgr.IsCellSelected(nRow, nCol) && ContainsFocus)
			{
				bkBrush = _SelectedCellFocusedBrush;
			}
			else if (_SelectionMgr.IsCellSelected(nRow, nCol) && !ContainsFocus)
			{
				bkBrush = _InactiveSelectedCellBrush;
			}
		}
	}



	protected override string GetCellStringForResizeToShowAll(long rowIndex, int storageColIndex, out TextFormatFlags tff)
	{
		string cellStringForResizeToShowAll = base.GetCellStringForResizeToShowAll(rowIndex, storageColIndex, out tff);
		if (cellStringForResizeToShowAll == null)
		{
			return cellStringForResizeToShowAll;
		}

		_CellMetricsString.Length = 0;
		_CellMetricsString.Append(cellStringForResizeToShowAll);
		GridHyperlinkColumnWithLimit.AdjustCellString(_CellMetricsString, removeCR: true);
		if (_CellMetricsString.Length > _NumberOfCharsToShow)
		{
			_CellMetricsString.Length = _NumberOfCharsToShow;
		}

		return _CellMetricsString.ToString().TrimEnd(null);
	}



	protected override string GetTextBasedColumnStringForClipboardText(long rowIndex, int colIndex)
	{
		_SbCustomClipboardText.Length = 0;
		_SbCustomClipboardText.Append(m_gridStorage.GetCellDataAsString(rowIndex, m_Columns[colIndex].ColumnIndex));
		if (m_Columns[colIndex].ColumnType != 5)
		{
			GridHyperlinkColumnWithLimit.AdjustCellString(_SbCustomClipboardText, removeCR: true);
			if (_SbCustomClipboardText.Length > _NumberOfCharsToShow)
			{
				_SbCustomClipboardText.Length = _NumberOfCharsToShow;
			}
		}

		return _SbCustomClipboardText.ToString();
	}



	protected override bool AdjustSelectionForButtonCellMouseClick()
	{
		if (AdjustSelectionForButtonClickEvent != null)
		{
			AdjustSelectionForButtonClickEvent(this, new(m_captureTracker.RowIndex, m_captureTracker.ColumnIndex));
			return true;
		}

		return base.AdjustSelectionForButtonCellMouseClick();
	}



	public void SetSelectedCellsAndCurrentCell(BlockOfCellsCollection cells, long currentRow, int currentColumn)
	{
		SelectedCellsInternal(cells, bSet: true);
		_SelectionMgr.SetCurrentCell(currentRow, currentColumn);
	}



	public void HandleMouseWheelDirectly(IntPtr wParam, IntPtr lParam)
	{
		Point p = new Point((short)(int)lParam, (int)lParam >> 16);
		p = PointToClient(p);
		OnMouseWheel(new MouseEventArgs(MouseButtons.None, 0, p.X, p.Y, (int)wParam >> 16));
	}



	public void SetBgAndFgColors(Color bkColor, Color foreColor)
	{
		_BgBrush?.Dispose();

		_FgBrush?.Dispose();

		_BgBrush = new SolidBrush(bkColor);
		_FgBrush = new SolidBrush(foreColor);
		BackColor = bkColor;
		foreach (AbstractGridColumn column in m_Columns)
		{
			(column as GridHyperlinkColumnWithLimit)?.SetNewTextBrush(_FgBrush);
		}

		if (IsHandleCreated)
		{
			Invalidate();
		}
	}



	public void SetSelectedCellColor(Color selectedCellFocusedColor)
	{
		_SelectedCellFocusedBrush?.Dispose();

		_SelectedCellFocusedBrush = new SolidBrush(selectedCellFocusedColor);
		if (IsHandleCreated)
		{
			Invalidate();
		}
	}



	public void SetInactiveSelectedCellColor(Color inactiveSelectedCellFocusedColor)
	{
		_InactiveSelectedCellBrush?.Dispose();

		_InactiveSelectedCellBrush = new SolidBrush(inactiveSelectedCellFocusedColor);
		if (IsHandleCreated)
		{
			Invalidate();
		}
	}



	public void SetIncludeHeadersOnDragAndDrop(bool includeHeaders)
	{
		_IncludeHeadersOnDragAndDrop = includeHeaders;
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
		if (Visible)
			return;

		try
		{
			if (!IsHandleCreated)
			{
				CreateHandle();
			}

			int numberOfCharsToShow = NumberOfCharsToShow;
			NumberOfCharsToShow = SharedConstants.C_DefaultInitialMaxCharsPerColumnForGrid;
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
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;
		}
	}


	#endregion Methods





	// =========================================================================================================
	#region Event Handling - GridResultsGrid
	// =========================================================================================================


	protected override bool OnBeforeGetClipboardTextForCells(StringBuilder clipboardText, long startRow, long endRow, int startCol, int endCol)
	{
		if (_ForceHeadersOnDragAndDrop || _IncludeHeadersOnDragAndDrop && startCol >= 0 && startCol < endCol)
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



	protected override void OnKeyPressedOnCell(long nCurRow, int nCurCol, Keys key, Keys mod)
	{
		base.OnKeyPressedOnCell(nCurRow, nCurCol, key, mod);
		if (mod == Keys.Control && key == Keys.C)
		{
			Clipboard.SetDataObject(GetDataObject(bOnlyCurrentSelBlock: true));
		}
	}



	protected override void OnMouseButtonDoubleClicked(EnHitTestResult htArea, long rowIndex, int colIndex, Rectangle cellRect, MouseButtons btn, EnGridButtonArea headerArea)
	{
		// Tracer.Trace(GetType(), "SqlManagerUIDlgGrid.OnMouseButtonDoubleClicked", "", null);
		if (m_gridStorage != null && htArea == EnHitTestResult.ColumnResize && btn == MouseButtons.Left && (m_Columns[colIndex].ColumnType == 2 || m_Columns[colIndex].ColumnType == 1))
		{
			// Tracer.Trace(GetType(), Tracer.EnLevel.Verbose, "GridResultsGrid.OnMouseButtonDoubleClicked", "auto resizing column {0}", colIndex);
			ResizeColumnToShowAllContentsInternal(colIndex);
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



	protected override bool OnTooltipDataNeeded(EnHitTestResult ht, long rowNumber, int colNumber, ref string toolTipText)
	{
		switch (ht)
		{
			case EnHitTestResult.HyperlinkCell:
				toolTipText = _S_XmlCellTooltip;
				return true;
			case EnHitTestResult.HeaderButton:
				if (colNumber == 0)
				{
					toolTipText = _S_SelectAllCellsTooltip;
					return true;
				}

				toolTipText = _S_SelectWholeRowTooltip;
				return true;
			case EnHitTestResult.ButtonCell:
				if (colNumber == 0)
				{
					toolTipText = _S_SelectWholeColumnTooltip;
					return true;
				}

				break;
		}

		return false;
	}


	#endregion Event Handling





	// =========================================================================================================
	#region Sub-Classes - GridResultsGrid
	// =========================================================================================================


	private class GridHyperlinkColumnWithLimit : GridHyperlinkColumn
	{
		protected int m_maxNumOfChars = SharedConstants.C_DefaultMaxCharsPerColumnForGrid;

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

		public override void DrawCell(Graphics g, Brush bkBrush, SolidBrush textBrush, Font textFont, Rectangle rect, IBsGridStorage storage, long nRowIndex)
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

		public override void PrintCell(Graphics g, Brush bkBrush, SolidBrush textBrush, Font textFont, Rectangle rect, IBsGridStorage storage, long nRowIndex)
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

		protected override string GetCellStringToMeasure(long rowIndex, IBsGridStorage storage)
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
			Diag.ThrowIfNotOnUIThread();

			if (Package.GetGlobalService(typeof(SVsUIShell)) is IVsUIShell2 vsUIShell
				&& __(vsUIShell.GetVSSysColorEx((int)__VSSYSCOLOREX.VSCOLOR_CONTROL_LINK_TEXT, out var pdwRGBval)))
			{
				return ColorTranslator.FromWin32((int)pdwRGBval);
			}

			return Color.Empty;
		}
	}


	#endregion Sub-Classes
}
