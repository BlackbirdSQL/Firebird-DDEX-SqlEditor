// Microsoft.SqlServer.GridControl, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.GridControl
#define TRACE

using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Windows.Forms;
using Accessibility;
using BlackbirdSql.Shared.Ctl;
using BlackbirdSql.Shared.Enums;
using BlackbirdSql.Shared.Events;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Shared.Properties;
using Microsoft.VisualStudio;
using Microsoft.Win32;

using ColumnWidthChangedEventHandler = BlackbirdSql.Shared.Events.ColumnWidthChangedEventHandler;



namespace BlackbirdSql.Shared.Controls.Grid;

[Designer("System.Windows.Forms.Design.ControlDesigner, System.Design")]
[DefaultProperty("SelectionType")]
[DefaultEvent("MouseButtonClicked")]


public class GridControl : Control, ISupportInitialize, IBsGridControl
{

	static GridControl()
	{
		s_nMaxNumOfVisibleRows = 80;
	}

	public GridControl()
	{
		SetStyle(ControlStyles.Opaque, value: true);
		SetStyle(ControlStyles.UserMouse, value: true);
		SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
		BackColor = SystemColors.Window;
		m_scrollMgr.SetColumns(m_Columns);
		m_scrollMgr.RowCount = 0L;
		m_gridTooltip = new ToolTip
		{
			InitialDelay = 1000,
			ShowAlways = true,
			Active = false
		};
		m_gridTooltip.SetToolTip(this, "");
		ResetHeaderFont();
		InitializeCachedGDIObjects();
		_UpdateGridInternalDelegate = UpdateGridInternal;
		_OnEmbeddedControlContentsChangedHandler = OnEmbeddedControlContentsChanged;
		_OnEmbeddedControlLostFocusHandler = OnEmbeddedControlLostFocusInternal;
		HandleCreated += delegate
		{
			BeginInvoke(new VoidInvoker(InitDefaultEmbeddedControls));
		};
		GetFontInfo(Font, out var height, out m_cAvCharWidth);
		m_scrollMgr.CellHeight = height + GridButton.ButtonAdditionalHeight;
		m_scrollMgr.SetHorizontalScrollUnitForArrows((int)m_cAvCharWidth);
		m_autoScrollTimer.Interval = 75;
		m_autoScrollTimer.Tick += AutoscrollTimerProcessor;
		m_linkFont = new Font(Font, FontStyle.Underline | Font.Style);
	}





	private class InvokerInOutArgs
	{
		public object InOutParam;

		public object InOutParam2;

		public object InOutParam3;
	}




	private delegate void UpdateGridInvoker(bool bRecalcRows);

	private delegate void InsertColumnInvoker(int nIndex, GridColumnInfo ci);

	private delegate void SetHeaderInfoInvoker(int nIndex, string strText, Bitmap bmp, EnGridCheckBoxState checkboxState);

	private delegate void GetHeaderInfoInvoker(int nIndex, InvokerInOutArgs outArgs);

	private delegate void AddColumnInvoker(GridColumnInfo ci);

	private delegate void DeleteColumnInvoker(int nIndex);

	private delegate void EnsureCellIsVisibleInvoker(long nRowIndex, int nColIndex);

	private delegate void RegisterEmbeddedControlInternalInvoker(int editableCellType, Control embeddedControl);

	private delegate void SelectedCellsInternalInvoker(InvokerInOutArgs args, bool bSet);

	private delegate void StartCellEditInternalInvoker(long nRowIndex, int nColIndex, InvokerInOutArgs args);

	private delegate void SetColumnWidthInternalInvoker(int nColIndex, EnGridColumnWidthType widthType, int nWidth);

	private delegate void GetColumnWidthInternalInvoker(int nColIndex, InvokerInOutArgs args);

	private delegate void SetMergedHeaderResizeProportionInternalInvoker(int colIndex, float proportion);

	private delegate void IsACellBeingEditedInternalInvoker(InvokerInOutArgs args);

	private delegate void StopCellEditInternalInvoker(InvokerInOutArgs args, bool bCommitIntoStorage);

	private delegate void AlwaysHighlightSelectionIntInvoker(bool bAlwaysHighlight);

	private delegate void ResizeColumnToShowAllContentsInternalInvoker(int uiColumnIndex);

	private delegate void ResizeColumnToShowAllContentsInternalInvoker2(int uiColumnIndex, bool considerAllRows);

	private delegate void GetColumnsNumberInternalInvoker(InvokerInOutArgs args);

	private delegate void VoidInvoker();

	protected class TooltipInfo
	{
		public long RowNumber = -1L;

		public int ColumnNumber = -1;

		public EnHitTestResult HitTest;

		public void Reset()
		{
			RowNumber = -1L;
			ColumnNumber = -1;
			HitTest = EnHitTestResult.Nothing;
		}
	}

	public delegate void BitmapCellAccessibilityInfoNeededHandler(object sender, BitmapCellAccessibilityInfoNeededEventArgs args);

	public class GridPrinter
	{
		private readonly GridControl _GridCtl;

		private readonly int m_gridColNumber;

		private readonly long m_gridRowNumber;

		private readonly EnGridLineType m_linesType = EnGridLineType.Solid;

		private long m_firstRowNum;

		private int m_firstColNum;

		public GridPrinter()
		{
		}

		public GridPrinter(GridControl grid)
		{
			if (grid.ColumnsNumber <= 0)
			{
				ArgumentException ex = new(ControlsResources.ExGridColumnNumberShouldBeAboveZero, "grid");
				Diag.Dug(ex);
				throw ex;
			}

			_GridCtl = grid;
			m_gridColNumber = grid.ColumnsNumber;
			m_gridRowNumber = grid.GridStorage.RowCount;
			m_linesType = grid.m_lineType;
		}

		public virtual void PrintPage(PrintPageEventArgs ev)
		{
			int num = CalculateLastColumn(ev.MarginBounds.Width, out int nRealWidth);
			_ = nRealWidth;
			long num2 = CalculateLastRow(ev.MarginBounds.Height);
			int num3 = ev.MarginBounds.Left + 1;
			Graphics graphics = ev.Graphics;
			Region clip = graphics.Clip;
			try
			{
				graphics.SetClip(ev.MarginBounds);
				bool flag = m_firstRowNum == 0L && _GridCtl.m_withHeader;
				if (flag)
				{
					_GridCtl.PaintHeaderHelper(graphics, m_firstColNum, num, num3, ev.MarginBounds.Top + 1, useGdiPlus: true);
				}

				if (m_gridRowNumber > 0)
				{
					int gRID_LINE_WIDTH = ScrollManager.GRID_LINE_WIDTH;
					int cellHeight = _GridCtl.m_scrollMgr.CellHeight;
					int top = ev.MarginBounds.Top;
					top = !flag ? top + 1 : top + _GridCtl.HeaderHeight + 1;
					Rectangle rCell = default;
					rCell.Y = top;
					rCell.Height = cellHeight + gRID_LINE_WIDTH;
					for (long num4 = m_firstRowNum; num4 <= num2; num4++)
					{
						rCell.X = num3;
						for (int i = m_firstColNum; i <= num; i++)
						{
							PrintOneCell(graphics, i, num4, ref rCell);
						}

						rCell.Offset(0, cellHeight + gRID_LINE_WIDTH);
					}

					if (m_linesType != 0)
					{
						_GridCtl.PaintHorizGridLines(graphics, num2 - m_firstRowNum + 1, top, num3 - 1, rCell.X - gRID_LINE_WIDTH, bAdjust: false);
						_GridCtl.PaintVertGridLines(graphics, m_firstColNum, num, num3 - 1, top, rCell.Y);
						graphics.DrawLine(_GridCtl.GridLinesPen, num3 - 1, top, num3 - 1, rCell.Y);
					}
				}

				ev.HasMorePages = AdjustRowAndColumnIndeces(num, num2);
			}
			finally
			{
				graphics.Clip = clip;
			}
		}

		protected virtual void PrintOneCell(Graphics g, int nCol, long nRow, ref Rectangle rCell)
		{
			AbstractGridColumn gridColumn = _GridCtl.m_Columns[nCol];
			int widthInPixels = gridColumn.WidthInPixels;
			int gRID_LINE_WIDTH = ScrollManager.GRID_LINE_WIDTH;
			SolidBrush bkBrush = null;
			SolidBrush textBrush = null;
			rCell.Width = widthInPixels + gRID_LINE_WIDTH;
			_GridCtl.GetCellGDIObjects(gridColumn, nRow, nCol, ref bkBrush, ref textBrush);
			if (gridColumn.WithSelectionBk && _GridCtl.m_selMgr.IsCellSelected(nRow, nCol))
			{
				bkBrush = _GridCtl.m_highlightBrush;
			}

			_GridCtl.DoCellPrinting(g, bkBrush, textBrush, _GridCtl.GetCellFont(nRow, gridColumn), rCell, gridColumn, nRow);
			rCell.X = rCell.Right;
		}

		private bool AdjustRowAndColumnIndeces(int nLastColNumber, long nLastRowNumber)
		{
			bool result = true;
			if (nLastColNumber == m_gridColNumber - 1)
			{
				m_firstColNum = 0;
			}
			else
			{
				m_firstColNum = nLastColNumber + 1;
			}

			if (nLastRowNumber == m_gridRowNumber - 1 || m_gridRowNumber == 0L)
			{
				if (nLastColNumber == m_gridColNumber - 1)
				{
					result = false;
				}
			}
			else if (nLastColNumber == m_gridColNumber - 1)
			{
				m_firstRowNum = nLastRowNumber + 1;
			}

			return result;
		}

		private long CalculateLastRow(int nPageHeight)
		{
			if (m_gridRowNumber == 0L)
			{
				return 0L;
			}

			long firstRowNum = m_firstRowNum;
			int num = _GridCtl.m_scrollMgr.CellHeight + ScrollManager.GRID_LINE_WIDTH;
			firstRowNum = m_firstRowNum != 0L || !_GridCtl.m_withHeader ? firstRowNum + (nPageHeight / num - 1) : firstRowNum + ((nPageHeight - _GridCtl.HeaderHeight) / num - 1);
			if (firstRowNum < m_firstRowNum)
			{
				firstRowNum = m_firstRowNum;
			}

			if (firstRowNum >= m_gridRowNumber)
			{
				firstRowNum = m_gridRowNumber - 1;
			}

			return firstRowNum;
		}

		private int CalculateLastColumn(int nPageWidth, out int nRealWidth)
		{
			int i = m_firstColNum;
			nRealWidth = ScrollManager.GRID_LINE_WIDTH;
			for (; i < m_gridColNumber; i++)
			{
				int num = _GridCtl.m_Columns[i].WidthInPixels + ScrollManager.GRID_LINE_WIDTH;
				if (num + nRealWidth >= nPageWidth)
				{
					if (i == m_firstColNum)
					{
						nRealWidth = nPageWidth;
					}
					else
					{
						i--;
					}

					break;
				}

				nRealWidth += num;
			}

			if (i == m_gridColNumber)
			{
				i--;
			}

			return i;
		}
	}

	protected class GridControlAccessibleObject(GridControl owner) : ControlAccessibleObject(owner)
	{

		protected class ColumnAccessibleObject : AccessibleObject
		{

			protected class CellAccessibleObject(ColumnAccessibleObject parent,
					long nRowIndex, int nColIndex)
				: AccessibleObject
			{

				protected class EmbeddedEditOperationAccessibleObject(CellAccessibleObject parent,
						long nRowIndex, int nColIndex, bool bCommit)
					: AccessibleObject
				{
					protected bool m_commit = bCommit;

					protected long m_rowIndex = nRowIndex;

					protected int m_colIndex = nColIndex;

					protected CellAccessibleObject m_parent = parent;

					public override string Name
					{
						get
						{
							if (!m_commit)
							{
								return ControlsResources.Grid_EmbeddedEditCancelAAName;
							}

							return ControlsResources.Grid_EmbeddedEditCommitAAName;
						}
					}

					public override AccessibleObject Parent => m_parent;

					public override string DefaultAction
					{
						get
						{
							if (m_commit)
							{
								return ControlsResources.Grid_EmbeddedEditCommitDefaultAction;
							}

							return ControlsResources.Grid_EmbeddedEditCancelDefaultAction;
						}
					}

					public override void DoDefaultAction()
					{
						if (m_parent.IsCellBeingEdited)
						{
							m_parent.Grid.StopCellEdit(m_commit);
						}
					}
				}

				private readonly ColumnAccessibleObject m_parent = parent;

				protected int m_colIndex = nColIndex;

				protected long m_rowIndex = nRowIndex;

				protected GridControl Grid => m_parent.Grid;

				protected int GridColumnsNumber => Grid.m_Columns.Count;

				protected long GridRowsNumber => Grid.GridStorage.RowCount;

				protected bool IsCellBeingEdited
				{
					get
					{
						Grid.IsACellBeingEdited(out var nRowNum, out var nColNum);
						if (m_rowIndex == nRowNum)
						{
							return m_colIndex == nColNum;
						}

						return false;
					}
				}

				public override AccessibleRole Role
				{
					get
					{
						switch (Grid.GetGridColumnInfo(m_colIndex).ColumnType)
						{
							case 2:
								if (EnButtonCellState.Empty != Grid.GetButtonCellState(m_rowIndex, m_colIndex))
								{
									return AccessibleRole.PushButton;
								}

								return AccessibleRole.Cell;
							case 4:
								if (EnGridCheckBoxState.None != Grid.m_gridStorage.GetCellDataForCheckBox(m_rowIndex, m_colIndex))
								{
									return AccessibleRole.CheckButton;
								}

								return AccessibleRole.Cell;
							case 5:
								return AccessibleRole.Link;
							case 1:
								switch (Grid.m_gridStorage.IsCellEditable(m_rowIndex, m_colIndex))
								{
									case 0:
										return AccessibleRole.Text;
									case 2:
									case 3:
										return AccessibleRole.ComboBox;
									case 4:
										return AccessibleRole.SpinButton;
									default:
										return AccessibleRole.StaticText;
								}
							case 3:
								if (Grid.BitmapCellAccessibilityInfoNeeded != null)
								{
									BitmapCellAccessibilityInfoNeededEventArgs bitmapCellAccessibilityInfoNeededEventArgs = new BitmapCellAccessibilityInfoNeededEventArgs(m_colIndex, m_rowIndex);
									Grid.BitmapCellAccessibilityInfoNeeded(Grid, bitmapCellAccessibilityInfoNeededEventArgs);
									return bitmapCellAccessibilityInfoNeededEventArgs.AccessibleRole;
								}

								return AccessibleRole.Graphic;
							default:
								return AccessibleRole.Cell;
						}
					}
				}

				public override AccessibleObject Parent => m_parent;

				public override Rectangle Bounds => Grid.RectangleToScreen(Grid.m_scrollMgr.GetCellRectangle(m_rowIndex, m_colIndex));

				public override string Name => Grid.GetGridCellAccessibleName(m_rowIndex, m_colIndex);

				public override string DefaultAction
				{
					get
					{
						if (Grid.m_gridStorage.IsCellEditable(m_rowIndex, Grid.m_Columns[m_colIndex].ColumnIndex) != 0)
						{
							return ControlsResources.Grid_StartEditCell;
						}

						return ControlsResources.Grid_ClearSelAndSelectCell;
					}
				}

				public override AccessibleStates State
				{
					get
					{
						if (m_colIndex < GridColumnsNumber && m_rowIndex < GridRowsNumber)
						{
							AccessibleStates accessibleState = Grid.m_Columns[m_colIndex].GetAccessibleState(m_rowIndex, Grid.GridStorage);
							AccessibleStates accessibleStates = AccessibleStates.Focusable | AccessibleStates.Selectable;
							if (Grid.m_selMgr.CurrentColumn == m_colIndex && Grid.m_selMgr.CurrentRow == m_rowIndex)
							{
								accessibleStates |= AccessibleStates.Focused;
							}

							if (Grid.m_selMgr.IsCellSelected(m_rowIndex, m_colIndex))
							{
								accessibleStates |= AccessibleStates.Selected;
							}

							if (Grid.SelectionType != EnGridSelectionType.SingleCell)
							{
								accessibleStates |= AccessibleStates.MultiSelectable;
							}

							Rectangle empty = Rectangle.Empty;
							if (empty.Equals(Grid.m_scrollMgr.GetCellRectangle(m_rowIndex, m_colIndex)))
							{
								accessibleStates |= AccessibleStates.Invisible;
							}

							if (Grid.m_Columns[m_colIndex].ColumnType == 1 && Grid.m_gridStorage.IsCellEditable(m_rowIndex, m_colIndex) == 0)
							{
								accessibleStates |= AccessibleStates.ReadOnly;
							}

							if (Grid.m_Columns[m_colIndex].ColumnType == 3 && Grid.BitmapCellAccessibilityInfoNeeded != null)
							{
								BitmapCellAccessibilityInfoNeededEventArgs bitmapCellAccessibilityInfoNeededEventArgs = new BitmapCellAccessibilityInfoNeededEventArgs(m_colIndex, m_rowIndex);
								Grid.BitmapCellAccessibilityInfoNeeded(Grid, bitmapCellAccessibilityInfoNeededEventArgs);
								if (bitmapCellAccessibilityInfoNeededEventArgs.AdditionalAccessibleStates != 0)
								{
									accessibleStates |= bitmapCellAccessibilityInfoNeededEventArgs.AdditionalAccessibleStates;
								}
							}

							if (accessibleStates != 0)
							{
								return accessibleState | accessibleStates;
							}

							return accessibleStates;
						}

						return AccessibleStates.None;
					}
				}

				public override string Value
				{
					get
					{
						if (m_colIndex < GridColumnsNumber && m_rowIndex < GridRowsNumber)
						{
							return Grid.m_Columns[m_colIndex].GetAccessibleValue(m_rowIndex, Grid.GridStorage);
						}

						return null;
					}
					set
					{
						if (m_colIndex >= GridColumnsNumber || m_rowIndex >= GridRowsNumber || value == null || Grid.IsEditing)
						{
							return;
						}

						int num = Grid.GridStorage.IsCellEditable(m_rowIndex, Grid.m_Columns[m_colIndex].ColumnIndex);
						if (num == 0)
						{
							return;
						}

						Control control = (Control)Grid.m_EmbeddedControls[num];
						if (control == null)
						{
							return;
						}

						IBsGridEmbeddedControl gridEmbeddedControl = (IBsGridEmbeddedControl)control;
						gridEmbeddedControl.AddDataAsString(value);
						gridEmbeddedControl.SetCurSelectionAsString(value);
						try
						{
							Grid.m_bInGridStorageCall = true;
							if (Grid.GridStorage.SetCellDataFromControl(m_rowIndex, Grid.m_Columns[m_colIndex].ColumnIndex, gridEmbeddedControl))
							{
								Grid.Refresh();
							}
						}
						finally
						{
							Grid.m_bInGridStorageCall = false;
						}
					}
				}

				public override void Select(AccessibleSelection accessibleSelection)
				{
					if ((AccessibleSelection.TakeSelection & accessibleSelection) != 0)
					{
						Grid.m_selMgr.Clear();
						if (AccessibleSelection.TakeSelection == accessibleSelection)
						{
							Grid.m_selMgr.StartNewBlock(m_rowIndex, m_colIndex);
							Grid.Refresh();
							return;
						}
					}

					if ((AccessibleSelection.TakeFocus & accessibleSelection) != 0)
					{
						if ((AccessibleSelection.TakeSelection & accessibleSelection) != 0 || (AccessibleSelection.AddSelection & accessibleSelection) != 0)
						{
							Grid.m_selMgr.StartNewBlock(m_rowIndex, m_colIndex);
						}
						else if ((AccessibleSelection.ExtendSelection & accessibleSelection) != 0)
						{
							Grid.m_selMgr.UpdateCurrentBlock(m_rowIndex, m_colIndex);
							Grid.m_selMgr.CurrentRow = m_rowIndex;
							Grid.m_selMgr.CurrentColumn = m_colIndex;
						}
						else if ((AccessibleSelection.RemoveSelection & accessibleSelection) == 0)
						{
							Grid.m_selMgr.StartNewBlock(m_rowIndex, m_colIndex);
						}

						Grid.Refresh();
						return;
					}

					switch (accessibleSelection)
					{
						case AccessibleSelection.AddSelection:
							Grid.m_selMgr.StartNewBlock(m_rowIndex, m_colIndex);
							break;
						case AccessibleSelection.RemoveSelection:
							Grid.m_selMgr.StartNewBlockOrExcludeCell(m_rowIndex, m_colIndex);
							break;
						case AccessibleSelection.ExtendSelection:
							Grid.m_selMgr.UpdateCurrentBlock(m_rowIndex, m_colIndex);
							break;
					}

					Grid.Refresh();
				}

				public override AccessibleObject HitTest(int xLeft, int yTop)
				{
					if (IsCellBeingEdited)
					{
						return Grid.m_curEmbeddedControl.AccessibilityObject;
					}

					return null;
				}

				public override void DoDefaultAction()
				{
					if (m_colIndex < 0 || m_colIndex >= GridColumnsNumber || m_rowIndex < 0 || m_rowIndex >= GridRowsNumber)
					{
						return;
					}

					Rectangle cellRectangle = Grid.m_scrollMgr.GetCellRectangle(m_rowIndex, m_colIndex);
					if (cellRectangle.Equals(Rectangle.Empty))
					{
						Grid.EnsureCellIsVisible(m_rowIndex, m_colIndex);
						cellRectangle = Grid.m_scrollMgr.GetCellRectangle(m_rowIndex, m_colIndex);
					}

					if (Grid.OnMouseButtonClicking(m_rowIndex, m_colIndex, cellRectangle, ModifierKeys, MouseButtons.Left))
					{
						Grid.m_selMgr.Clear();
						int num = Grid.m_gridStorage.IsCellEditable(m_rowIndex, Grid.m_Columns[m_colIndex].ColumnIndex);
						if (num != 0)
						{
							bool bSendMouseClick = false;
							Grid.StartEditingCell(m_rowIndex, m_colIndex, cellRectangle, num, ref bSendMouseClick);
						}

						Grid.m_selMgr.StartNewBlock(m_rowIndex, m_colIndex);
						Grid.OnMouseButtonClicked(m_rowIndex, m_colIndex, cellRectangle, MouseButtons.Left);
						Grid.Refresh();
						Grid.OnSelectionChanged(Grid.m_selMgr.SelectedBlocks);
					}
				}

				public override int GetChildCount()
				{
					if (IsCellBeingEdited)
					{
						return 3;
					}

					return 0;
				}

				public override AccessibleObject GetChild(int childID)
				{
					if (IsCellBeingEdited)
					{
						if (childID == 0)
						{
							return Grid.m_curEmbeddedControl.AccessibilityObject;
						}

						return new EmbeddedEditOperationAccessibleObject(this, m_rowIndex, m_colIndex, childID == 1);
					}

					return null;
				}
			}

			protected class HeaderAccessibleObject(ColumnAccessibleObject parent, int nColIndex)
				: AccessibleObject
			{
				private readonly ColumnAccessibleObject m_parent = parent;

				private readonly int m_colIndex = nColIndex;

				protected GridControl Grid => m_parent.Grid;

				public override AccessibleObject Parent => m_parent;

				public override string Name
				{
					get
					{
						if (Grid.m_gridHeader[m_colIndex].AccessibleName != null)
						{
							return ControlsResources.Grid_ColumnHeaderAAName.FmtRes(m_colIndex, Grid.m_gridHeader[m_colIndex].AccessibleName);
						}

						return ControlsResources.Grid_ColumnHeaderAAName.FmtRes(m_colIndex, Grid.m_gridHeader[m_colIndex].Text);
					}
				}

				public override string DefaultAction => ControlsResources.Grid_ColumnHeaderAADefaultAction;

				public override string Value
				{
					get
					{
						if (m_colIndex >= 0 && m_colIndex < Grid.m_Columns.Count)
						{
							return Grid.m_gridHeader[m_colIndex].Text;
						}

						return null;
					}
					set
					{
					}
				}

				public override Rectangle Bounds
				{
					get
					{
						Rectangle cellRectangle = Grid.m_scrollMgr.GetCellRectangle(Grid.m_scrollMgr.FirstRowIndex, m_colIndex);
						int num = 0;
						int num2 = m_colIndex;
						while (num2 > 0 && Grid.m_gridHeader[num2 - 1].MergedWithRight)
						{
							num += Grid.m_scrollMgr.GetCellRectangle(Grid.m_scrollMgr.FirstRowIndex, num2 - 1).Width;
							num2--;
						}

						if (!cellRectangle.Equals(Rectangle.Empty))
						{
							cellRectangle.Width += num;
							cellRectangle.X -= num;
							cellRectangle.Y = 0;
							cellRectangle.Height = Grid.HeaderHeight;
							return Grid.RectangleToScreen(cellRectangle);
						}

						return Rectangle.Empty;
					}
				}

				public override AccessibleRole Role => AccessibleRole.ColumnHeader;

				public override AccessibleStates State
				{
					get
					{
						AccessibleStates accessibleStates = AccessibleStates.None;
						if (m_colIndex >= Grid.m_scrollMgr.FirstScrollableColumnIndex && (m_colIndex < Grid.m_scrollMgr.FirstColumnIndex || m_colIndex > Grid.m_scrollMgr.LastColumnIndex))
						{
							accessibleStates |= AccessibleStates.Invisible;
						}

						return accessibleStates;
					}
				}

				public override void DoDefaultAction()
				{
					Grid.OnHeaderButtonClicked(m_colIndex, MouseButtons.Left, EnGridButtonArea.Nothing);
					Grid.Refresh();
				}
			}

			protected int m_colIndex = -1;

			private readonly GridControlAccessibleObject m_parent;

			protected GridControl Grid => m_parent.Grid;

			public override AccessibleRole Role => AccessibleRole.Column;

			public override string Name
			{
				get
				{
					return ControlsResources.Grid_ColumnNumber.FmtRes(m_colIndex);
				}
				set
				{
				}
			}

			public override AccessibleObject Parent => m_parent;

			public override AccessibleStates State
			{
				get
				{
					AccessibleStates accessibleStates = AccessibleStates.Focusable | AccessibleStates.Selectable;
					if (m_colIndex >= Grid.m_scrollMgr.FirstScrollableColumnIndex && (m_colIndex < Grid.m_scrollMgr.FirstColumnIndex || m_colIndex > Grid.m_scrollMgr.LastColumnIndex))
					{
						accessibleStates |= AccessibleStates.Invisible;
					}

					if (Grid.m_selMgr.CurrentColumn == m_colIndex)
					{
						accessibleStates |= AccessibleStates.Focused;
					}

					return accessibleStates;
				}
			}

			public override Rectangle Bounds
			{
				get
				{
					Rectangle cellRectangle = Grid.m_scrollMgr.GetCellRectangle(Grid.m_scrollMgr.FirstRowIndex, m_colIndex);
					if (!cellRectangle.Equals(Rectangle.Empty))
					{
						cellRectangle.Y = 0;
						cellRectangle.Height = Grid.ClientRectangle.Height;
						return Grid.RectangleToScreen(cellRectangle);
					}

					return Rectangle.Empty;
				}
			}

			private ColumnAccessibleObject()
			{
			}

			public ColumnAccessibleObject(GridControlAccessibleObject parent, int nColIndex)
			{
				m_colIndex = nColIndex;
				m_parent = parent;
			}

			public override int GetChildCount()
			{
				int num = 0;
				if (Grid.WithHeader)
				{
					num = 1;
				}

				return num + (int)Grid.GridStorage.RowCount;
			}

			public override AccessibleObject GetChild(int index)
			{
				if (Grid.WithHeader)
				{
					if (index == 0)
					{
						return new HeaderAccessibleObject(this, m_colIndex);
					}

					index--;
				}

				if (index < Grid.GridStorage.RowCount)
				{
					return new CellAccessibleObject(this, index, m_colIndex);
				}

				return null;
			}

			public override AccessibleObject GetFocused()
			{
				if (Grid.m_selMgr.CurrentColumn == m_colIndex)
				{
					return new CellAccessibleObject(this, Grid.m_selMgr.CurrentRow, m_colIndex);
				}

				return null;
			}

			public override AccessibleObject HitTest(int xLeft, int yTop)
			{
				Point point = Grid.PointToClient(new Point(xLeft, yTop));
				HitTestInfo hitTestInfo = Grid.HitTestInternal(point.X, point.Y);
				if (hitTestInfo.HitTestResult == EnHitTestResult.BitmapCell || hitTestInfo.HitTestResult == EnHitTestResult.ButtonCell || hitTestInfo.HitTestResult == EnHitTestResult.TextCell || hitTestInfo.HitTestResult == EnHitTestResult.CustomCell || hitTestInfo.HitTestResult == EnHitTestResult.HyperlinkCell)
				{
					if (hitTestInfo.ColumnIndex == m_colIndex)
					{
						return new CellAccessibleObject(this, hitTestInfo.RowIndex, hitTestInfo.ColumnIndex);
					}
				}
				else if (hitTestInfo.HitTestResult == EnHitTestResult.HeaderButton && hitTestInfo.ColumnIndex == m_colIndex)
				{
					return new HeaderAccessibleObject(this, hitTestInfo.ColumnIndex);
				}

				return null;
			}
		}

		protected class AccessibleObjectOnIAccessible(IAccessible acc, AccessibleObject parent, int nChildId)
			: AccessibleObject
		{

			public AccessibleObjectOnIAccessible(IAccessible acc, AccessibleObject parent)
				: this(acc, parent, 0)
			{
			}



			private readonly IAccessible m_acc = acc;

			private readonly AccessibleObject m_parent = parent;

			private readonly int m_childID = nChildId;

			public override Rectangle Bounds
			{
				get
				{
					if (m_acc != null)
					{
						try
						{
							m_acc.accLocation(out int x, out int y, out int width, out int height, m_childID);
							return new Rectangle(x, y, width, height);
						}
						catch (COMException ex)
						{
							if (ex.ErrorCode != VSConstants.DISP_E_MEMBERNOTFOUND)
							{
								Diag.Dug(ex);
								throw ex;
							}
						}
					}

					return Rectangle.Empty;
				}
			}

			public override string DefaultAction
			{
				get
				{
					if (m_acc != null)
					{
						try
						{
							return m_acc.get_accDefaultAction(m_childID);
						}
						catch (COMException ex)
						{
							if (ex.ErrorCode != VSConstants.DISP_E_MEMBERNOTFOUND)
							{
								Diag.Dug(ex);
								throw ex;
							}
						}
					}

					return null;
				}
			}

			public override string Description
			{
				get
				{
					if (m_acc != null)
					{
						try
						{
							return m_acc.get_accDescription(m_childID);
						}
						catch (COMException ex)
						{
							if (ex.ErrorCode != VSConstants.DISP_E_MEMBERNOTFOUND)
							{
								Diag.Dug(ex);
								throw ex;
							}
						}
					}

					return null;
				}
			}

			public override string Help
			{
				get
				{
					if (m_acc != null)
					{
						try
						{
							return m_acc.get_accHelp(m_childID);
						}
						catch (COMException ex)
						{
							if (ex.ErrorCode != VSConstants.DISP_E_MEMBERNOTFOUND)
							{
								Diag.Dug(ex);
								throw ex;
							}
						}
					}

					return null;
				}
			}

			public override string KeyboardShortcut
			{
				get
				{
					if (m_acc != null)
					{
						try
						{
							return m_acc.get_accKeyboardShortcut(m_childID);
						}
						catch (COMException ex)
						{
							if (ex.ErrorCode != VSConstants.DISP_E_MEMBERNOTFOUND)
							{
								Diag.Dug(ex);
								throw ex;
							}
						}
					}

					return null;
				}
			}

			public override string Name
			{
				get
				{
					if (m_acc != null)
					{
						try
						{
							return m_acc.get_accName(m_childID);
						}
						catch (COMException ex)
						{
							if (ex.ErrorCode != VSConstants.DISP_E_MEMBERNOTFOUND)
							{
								Diag.Dug(ex);
								throw ex;
							}
						}
					}

					return null;
				}
				set
				{
					if (m_acc == null)
					{
						return;
					}

					try
					{
						m_acc.set_accName(m_childID, value);
					}
					catch (COMException ex)
					{
						if (ex.ErrorCode != VSConstants.DISP_E_MEMBERNOTFOUND)
						{
							Diag.Dug(ex);
							throw ex;
						}
					}
				}
			}

			public override AccessibleObject Parent => m_parent;

			public override AccessibleRole Role
			{
				get
				{
					if (m_acc != null)
					{
						return (AccessibleRole)m_acc.get_accRole(m_childID);
					}

					return AccessibleRole.None;
				}
			}

			public override AccessibleStates State
			{
				get
				{
					if (m_acc != null)
					{
						return (AccessibleStates)m_acc.get_accState(m_childID);
					}

					return AccessibleStates.None;
				}
			}

			public override string Value
			{
				get
				{
					if (m_acc != null)
					{
						try
						{
							return m_acc.get_accValue(m_childID);
						}
						catch (COMException ex)
						{
							if (ex.ErrorCode != VSConstants.DISP_E_MEMBERNOTFOUND)
							{
								Diag.Dug(ex);
								throw ex;
							}
						}
					}

					return "";
				}
				set
				{
					if (m_acc == null)
					{
						return;
					}

					try
					{
						m_acc.set_accValue(m_childID, value);
					}
					catch (COMException ex)
					{
						if (ex.ErrorCode != VSConstants.DISP_E_MEMBERNOTFOUND)
						{
							Diag.Dug(ex);
							throw ex;
						}
					}
				}
			}


			public override int GetHelpTopic(out string fileName)
			{
				if (m_acc != null)
				{
					try
					{
						return m_acc.get_accHelpTopic(out fileName, m_childID);
					}
					catch (COMException ex)
					{
						if (ex.ErrorCode != VSConstants.DISP_E_MEMBERNOTFOUND)
						{
							Diag.Dug(ex);
							throw ex;
						}
					}
				}

				fileName = null;
				return -1;
			}

			public override void DoDefaultAction()
			{
				if (m_acc == null)
				{
					return;
				}

				try
				{
					m_acc.accDoDefaultAction(m_childID);
				}
				catch (COMException ex)
				{
					if (ex.ErrorCode != VSConstants.DISP_E_MEMBERNOTFOUND)
					{
						Diag.Dug(ex);
						throw ex;
					}
				}
			}

			public override AccessibleObject Navigate(AccessibleNavigation navdir)
			{
				//IL_0045: Unknown result type (might be due to invalid IL or missing references)
				//IL_004b: Expected O, but got Unknown
				if (m_acc != null)
				{
					int childCount = GetChildCount();
					if (childCount > 0)
					{
						switch (navdir)
						{
							case AccessibleNavigation.FirstChild:
								return GetChild(0);
							case AccessibleNavigation.LastChild:
								return GetChild(childCount - 1);
						}
					}

					try
					{
						IAccessible val = (IAccessible)m_acc.accNavigate((int)navdir, m_childID);
						if (val != null)
						{
							return new AccessibleObjectOnIAccessible(val, this);
						}

						return null;
					}
					catch (COMException ex)
					{
						if (ex.ErrorCode != VSConstants.DISP_E_MEMBERNOTFOUND)
						{
							Diag.Dug(ex);
							throw ex;
						}
					}
				}

				return null;
			}

			public override void Select(AccessibleSelection flags)
			{
				if (m_acc == null)
				{
					return;
				}

				try
				{
					m_acc.accSelect((int)flags, m_childID);
				}
				catch (COMException ex)
				{
					if (ex.ErrorCode != VSConstants.DISP_E_MEMBERNOTFOUND)
					{
						Diag.Dug(ex);
						throw ex;
					}
				}
			}

			public override AccessibleObject GetChild(int index)
			{
				//IL_001b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0021: Expected O, but got Unknown
				if (m_acc != null)
				{
					IAccessible val = (IAccessible)m_acc.get_accChild(index + 1);
					if (val != null)
					{
						return new AccessibleObjectOnIAccessible(val, this);
					}

					return new AccessibleObjectOnIAccessible(m_acc, this, index + 1);
				}

				return null;
			}

			public override int GetChildCount()
			{
				if (m_acc != null)
				{
					if (m_childID == 0)
					{
						return m_acc.accChildCount;
					}

					return -1;
				}

				return -1;
			}
		}

		protected static int s_HorizSBChildIndex = 0;

		protected static int s_VertSBChildIndex = 1;

		private IntPtr m_cachedHandle = IntPtr.Zero;

		private AccessibleObjectOnIAccessible m_cachedHScrollIAccessible;

		private AccessibleObjectOnIAccessible m_cachedVScrollIAccessible;

		protected GridControl Grid => (GridControl)Owner;

		public override Rectangle Bounds => Grid.RectangleToScreen(Grid.ClientRectangle);

		protected AccessibleObject GetAccessibleForScrollBar(bool bHoriz)
		{
			if (m_cachedHandle != Handle)
			{
				m_cachedHandle = Handle;
				Guid refiid = Native.IID_IAccessible;
				object pAcc = null;
				Native.CreateStdAccessibleObject(Handle, -5, ref refiid, ref pAcc);
				if (pAcc != null)
				{
					m_cachedVScrollIAccessible = new AccessibleObjectOnIAccessible((IAccessible)pAcc, this);
				}

				pAcc = null;
				Native.CreateStdAccessibleObject(Handle, -6, ref refiid, ref pAcc);
				if (pAcc != null)
				{
					m_cachedHScrollIAccessible = new AccessibleObjectOnIAccessible((IAccessible)pAcc, this);
				}
			}

			if (!bHoriz)
			{
				return m_cachedVScrollIAccessible;
			}

			return m_cachedHScrollIAccessible;
		}

		public override int GetChildCount()
		{
			return 2 + Grid.m_Columns.Count;
		}

		public override AccessibleObject GetChild(int index)
		{
			if (index == s_HorizSBChildIndex)
			{
				return GetAccessibleForScrollBar(bHoriz: true);
			}

			if (index == s_VertSBChildIndex)
			{
				return GetAccessibleForScrollBar(bHoriz: false);
			}

			index -= 2;
			if (index < Grid.m_Columns.Count)
			{
				return new ColumnAccessibleObject(this, index);
			}

			return null;
		}

		public override AccessibleObject HitTest(int xLeft, int yTop)
		{
			Point point = Grid.PointToClient(new Point(xLeft, yTop));
			HitTestInfo hitTestInfo = Grid.HitTestInternal(point.X, point.Y);
			if (hitTestInfo.HitTestResult == EnHitTestResult.BitmapCell || hitTestInfo.HitTestResult == EnHitTestResult.ButtonCell || hitTestInfo.HitTestResult == EnHitTestResult.TextCell || hitTestInfo.HitTestResult == EnHitTestResult.CustomCell || hitTestInfo.HitTestResult == EnHitTestResult.ColumnResize || hitTestInfo.HitTestResult == EnHitTestResult.HeaderButton || hitTestInfo.HitTestResult == EnHitTestResult.HyperlinkCell)
			{
				return new ColumnAccessibleObject(this, hitTestInfo.ColumnIndex);
			}

			return null;
		}
	}

	private const double C_HyperlinkSelectionDelay = 0.5;

	private static readonly int s_nMaxNumOfVisibleRows;

	private bool focusOnNav = true;

	private bool m_withHeader = true;

	private int m_nIsInitializingCount;

	private double m_cAvCharWidth;

	private BorderStyle m_borderStyle = BorderStyle.Fixed3D;

	private int m_headerHeight;

	private EnGridLineType m_lineType = EnGridLineType.Solid;

	private Pen m_colInsertionPen;

	private Timer m_autoScrollTimer = new Timer();

	private bool m_alwaysHighlightSelection = true;

	private bool m_bInGridStorageCall;

	private bool m_bColumnsReorderableByDefault;

	private int m_wheelDelta;

	private readonly CustomizeCellGDIObjectsEventArgs m_CustomizeCellGDIObjectsArgs = new CustomizeCellGDIObjectsEventArgs();

	private const int C_MaximumColumnWidth = 20000;

	private Hashtable m_EmbeddedControls = [];

	private bool shouldRenderFocusRectangle;

	protected Font m_linkFont;

	private readonly UpdateGridInvoker _UpdateGridInternalDelegate;

	private readonly ContentsChangedEventHandler _OnEmbeddedControlContentsChangedHandler;

	private readonly EventHandler _OnEmbeddedControlLostFocusHandler;

	protected const string C_GridEventsCategory = "Grid Events";

	protected const string C_GridPropsCategory = "Appearance";

	protected Brush m_backBrush = new SolidBrush(DefaultBackColor);

	protected SolidBrush m_highlightBrush;

	protected SolidBrush m_highlightTextBrush;

	private SolidBrush highlightNonFocusedBrush;

	private SolidBrush highlightNonFocusedTextBrush;

	protected Font m_gridFont = DefaultFont;

	protected Rectangle m_scrollableArea;

	protected GridColumnCollection m_Columns = [];

	protected ScrollManager m_scrollMgr = new ScrollManager();

	protected SelectionManager m_selMgr = new SelectionManager();

	protected GridHeader m_gridHeader = new GridHeader();

	protected CaptureTracker m_captureTracker = new CaptureTracker();

	protected IBsGridStorage m_gridStorage;

	protected Pen m_gridLinesPen;

	protected ToolTip m_gridTooltip;

	protected TooltipInfo m_hooverOverArea = new TooltipInfo();

	protected Control m_curEmbeddedControl;

	public const int C_MaxDisplayableChars = 43679;

	[Description("Whether or not the grid should pre-create cell content editor controls")]
	[Category(C_GridPropsCategory)]
	[DefaultValue(true)]
	private bool m_preCreateEmbeddedControls = true;

	private GridColumnMapper<int> gridColumnMapper;

	[Browsable(true)]
	[Category(C_GridPropsCategory)]
	[Description("Whether or not Embedded Controls will receive focus when navigated to via keyboard")]
	[DefaultValue(true)]
	public bool FocusEditorOnNavigation
	{
		get
		{
			return focusOnNav;
		}
		set
		{
			focusOnNav = value;
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public IBsGridStorage GridStorage
	{
		get
		{
			return m_gridStorage;
		}
		set
		{
			if (m_gridStorage != value)
			{
				m_gridStorage = value;
				if (IsHandleCreated && m_nIsInitializingCount == 0)
				{
					UpdateGrid();
				}
			}
		}
	}

	[Browsable(false)]
	public int ColumnsNumber
	{
		get
		{
			if (!InvokeRequired)
			{
				return m_Columns.Count;
			}

			InvokerInOutArgs invokerInOutArgs = new InvokerInOutArgs();
			Invoke(new GetColumnsNumberInternalInvoker(GetColumnsNumberInternalForInvoke), invokerInOutArgs);
			return (int)invokerInOutArgs.InOutParam;
		}
	}

	public bool PreCreateEmbeddedControls
	{
		get
		{
			return m_preCreateEmbeddedControls;
		}
		set
		{
			m_preCreateEmbeddedControls = value;
		}
	}

	[Description("Whether or not the header will be shown")]
	[Category(C_GridPropsCategory)]
	[DefaultValue(true)]
	public bool WithHeader
	{
		get
		{
			return m_withHeader;
		}
		set
		{
			if (m_withHeader != value)
			{
				m_withHeader = value;
				UpdateScrollableAreaRect();
			}
		}
	}

	[Description("Font for the header of the grid")]
	[Category(C_GridPropsCategory)]
	[Localizable(true)]
	[AmbientValue(null)]
	public Font HeaderFont
	{
		get
		{
			if (m_gridHeader.Font != null)
			{
				return m_gridHeader.Font;
			}

			return DefaultHeaderFont;
		}
		set
		{
			value ??= DefaultHeaderFont;

			m_gridHeader.Font = value;
			TuneToHeaderFont(value);
		}
	}

	[Description("Type of grid lines")]
	[Category(C_GridPropsCategory)]
	[DefaultValue(EnGridLineType.Solid)]
	public EnGridLineType GridLineType
	{
		get
		{
			return m_lineType;
		}
		set
		{
			if (value == m_lineType)
			{
				return;
			}

			if (!Enum.IsDefined(typeof(EnGridLineType), value))
			{
				InvalidEnumArgumentException ex = new("value", (int)value, typeof(EnGridLineType));
				Diag.Dug(ex);
				throw ex;
			}

			m_lineType = value;
			foreach (AbstractGridColumn column in m_Columns)
			{
				if (column is GridButtonColumn gridButtonColumn)
				{
					gridButtonColumn.SetGridLinesMode(m_lineType != EnGridLineType.None);
				}
			}

			if (IsHandleCreated && m_nIsInitializingCount == 0)
			{
				Invalidate();
			}
		}
	}

	[Description("Selection type of the grid")]
	[Category(C_GridPropsCategory)]
	[DefaultValue(EnGridSelectionType.SingleCell)]
	public EnGridSelectionType SelectionType
	{
		get
		{
			return m_selMgr.SelectionType;
		}
		set
		{
			if (m_selMgr.SelectionType != value)
			{
				if (!Enum.IsDefined(typeof(EnGridSelectionType), value))
				{
					InvalidEnumArgumentException ex = new("value", (int)value, typeof(EnGridSelectionType));
					Diag.Dug(ex);
					throw ex;
				}

				m_selMgr.SelectionType = value;
				if (IsHandleCreated && m_nIsInitializingCount == 0)
				{
					m_selMgr.Clear();
					OnSelectionChanged(m_selMgr.SelectedBlocks);
					Invalidate();
				}
			}
		}
	}

	[Description("Border style of the grid")]
	[Category(C_GridPropsCategory)]
	[DefaultValue(BorderStyle.Fixed3D)]
	public BorderStyle BorderStyle
	{
		get
		{
			return m_borderStyle;
		}
		set
		{
			if (!Enum.IsDefined(typeof(BorderStyle), value))
			{
				InvalidEnumArgumentException ex = new("value", (int)value, typeof(BorderStyle));
				Diag.Dug(ex);
				throw ex;
			}

			if (m_borderStyle != value)
			{
				m_borderStyle = value;
				RecreateHandle();
			}
		}
	}

	[Description("Index of the first scrollable column")]
	[Category(C_GridPropsCategory)]
	[DefaultValue(0)]
	public int FirstScrollableColumn
	{
		get
		{
			return m_scrollMgr.FirstScrollableColumnIndex;
		}
		set
		{
			if (m_scrollMgr.FirstScrollableColumnIndex != value)
			{
				if (value < 0)
				{
					ArgumentException ex = new(ControlsResources.ExFirstScrollableColumnShouldBeValid, "value");
					Diag.Dug(ex);
					throw ex;
				}

				SetFirstScrollableColumnInt(value, recalcGrid: true);
			}
		}
	}

	[DefaultValue(0)]
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public uint FirstScrollableRow
	{
		get
		{
			return m_scrollMgr.FirstScrollableRowIndex;
		}
		set
		{
			if (m_scrollMgr.FirstScrollableRowIndex != value)
			{
				m_scrollMgr.FirstScrollableRowIndex = value;
				UpdateScrollableAreaRect();
				UpdateGridInternal(bRecalcRows: true);
			}
		}
	}

	[Description("Interval in miliseconds for autoscroll")]
	[Category(C_GridPropsCategory)]
	[DefaultValue(75)]
	public int AutoScrollInterval
	{
		get
		{
			return m_autoScrollTimer.Interval;
		}
		set
		{
			if (value <= 0)
			{
				ArgumentException ex = new(ControlsResources.Grid_AutoScrollMoreThanZero, "value");
				Diag.Dug(ex);
				throw ex;
			}

			m_autoScrollTimer.Interval = value;
		}
	}

	[Browsable(false)]
	public int VisibleRowsNum
	{
		get
		{
			int num = m_scrollMgr.CalcVertPageSize(m_scrollableArea);
			if (num <= 0)
			{
				num = s_nMaxNumOfVisibleRows;
			}

			return num;
		}
	}

	[Description("Widths of the left and right margins for an embedded control")]
	[Category(C_GridPropsCategory)]
	public int MarginsWidth => AbstractGridColumn.CELL_CONTENT_OFFSET;

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public BlockOfCellsCollection SelectedCells
	{
		get
		{
			BlockOfCellsCollection blockOfCellsCollection = [];
			if (InvokeRequired)
			{
				InvokerInOutArgs invokerInOutArgs = new InvokerInOutArgs
				{
					InOutParam = blockOfCellsCollection
				};
				Invoke(new SelectedCellsInternalInvoker(SelectedCellsInternalForInvoke), invokerInOutArgs, false);
			}
			else
			{
				SelectedCellsInternal(blockOfCellsCollection, bSet: false);
			}

			return AdjustColumnIndexesInSelectedCells(blockOfCellsCollection, bFromUIToStorage: true);
		}
		set
		{
			if (InvokeRequired)
			{
				InvokerInOutArgs invokerInOutArgs = new InvokerInOutArgs
				{
					InOutParam = AdjustColumnIndexesInSelectedCells(value, bFromUIToStorage: false)
				};
				Invoke(new SelectedCellsInternalInvoker(SelectedCellsInternalForInvoke), invokerInOutArgs, true);
			}
			else
			{
				SelectedCellsInternal(AdjustColumnIndexesInSelectedCells(value, bFromUIToStorage: false), bSet: true);
			}
		}
	}

	[Browsable(false)]
	public int HeaderHeight
	{
		get
		{
			if (!m_withHeader)
			{
				return 0;
			}

			return m_headerHeight;
		}
	}

	[Browsable(false)]
	public int RowHeight
	{
		get
		{
			if (m_scrollMgr != null)
			{
				return m_scrollMgr.CellHeight;
			}

			return -1;
		}
	}

	[Browsable(false)]
	public Color HighlightColor
	{
		get
		{
			if (m_highlightBrush != null)
			{
				return m_highlightBrush.Color;
			}

			return SystemColors.Highlight;
		}
	}

	[DefaultValue(true)]
	[Category(C_GridPropsCategory)]
	public bool AlwaysHighlightSelection
	{
		get
		{
			return m_alwaysHighlightSelection;
		}
		set
		{
			if (InvokeRequired)
			{
				Invoke(new AlwaysHighlightSelectionIntInvoker(AlwaysHighlightSelectionInt), value);
			}
			else
			{
				AlwaysHighlightSelectionInt(value);
			}
		}
	}

	[Browsable(false)]
	public PrintDocument PrintDocument
	{
		get
		{
			if (IsEmpty)
			{
				InvalidOperationException ex = new(ControlsResources.ExCannotPrintEmptyGrid);
				Diag.Dug(ex);
				throw ex;
			}

			if (RowCount == 0L && !m_withHeader)
			{
				InvalidOperationException ex = new(ControlsResources.ExCannotPrintEmptyGrid);
				Diag.Dug(ex);
				throw ex;
			}

			return new GridPrintDocument(AllocateGridPrinter());
		}
	}

	[DefaultValue(false)]
	[Category(C_GridPropsCategory)]
	public bool ColumnsReorderableByDefault
	{
		get
		{
			return m_bColumnsReorderableByDefault;
		}
		set
		{
			m_bColumnsReorderableByDefault = value;
		}
	}

	[Browsable(false)]
	public GridColumnInfoCollection GridColumnsInfo
	{
		get
		{
			GridColumnInfoCollection gridColumnInfoCollection = [];
			for (int i = 0; i < m_Columns.Count; i++)
			{
				gridColumnInfoCollection.Add(GetGridColumnInfo(i));
			}

			return gridColumnInfoCollection;
		}
	}

	[Browsable(false)]
	public static int StandardGridCheckBoxSize => 13;

	[Browsable(false)]
	public static int GridButtonHorizOffset => GridButton.ExtraHorizSpace;

	protected override CreateParams CreateParams
	{
		[SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		get
		{
			CreateParams createParams = base.CreateParams;
			switch (m_borderStyle)
			{
				case BorderStyle.FixedSingle:
					createParams.Style |= 8388608;
					break;
				case BorderStyle.Fixed3D:
					if (Application.RenderWithVisualStyles)
					{
						createParams.Style |= 8388608;
					}
					else
					{
						createParams.ExStyle |= 512;
					}

					break;
			}

			createParams.Style |= 1177550848;
			return createParams;
		}
	}

	protected virtual Pen GridLinesPen
	{
		get
		{
			if (!ActAsEnabled)
			{
				return SystemPens.ControlDark;
			}

			return m_gridLinesPen;
		}
	}

	protected virtual bool NeedToHighlightCurrentCell => m_selMgr.CurrentColumn >= 0;

	protected virtual string NewLineCharacters => "\r\n";

	protected virtual string ColumnsSeparator => "\t";

	protected virtual string StringForBitmapData => string.Empty;

	protected virtual string StringForButtonsWithBmpOnly => "<button>";

	protected virtual bool ActAsEnabled => Enabled;

	protected virtual Font DefaultHeaderFont
	{
		get
		{
			if (Parent != null && Parent.Font != null)
			{
				return Parent.Font;
			}

			if (Site != null)
			{
				AmbientProperties ambientProperties = (AmbientProperties)Site.GetService(typeof(AmbientProperties));
				if (ambientProperties != null)
				{
					return ambientProperties.Font;
				}
			}

			return DefaultFont;
		}
	}

	protected virtual bool ShouldCommitEmbeddedControlOnLostFocus => !m_bInGridStorageCall;

	private bool IsRTL => RightToLeft == RightToLeft.Yes;

	private int NumColInt => m_Columns.Count;

	private bool HasNonScrollableColumns
	{
		get
		{
			if (m_scrollMgr.FirstScrollableColumnIndex > 0)
			{
				return m_scrollMgr.FirstScrollableColumnIndex < m_Columns.Count;
			}

			return false;
		}
	}

	private long RowCount => m_scrollMgr.RowCount;

	private bool IsEmpty
	{
		get
		{
			if (NumColInt != 0)
			{
				return m_gridStorage == null;
			}

			return true;
		}
	}

	protected bool IsInitializing => m_nIsInitializingCount > 0;

	protected bool IsEditing => m_curEmbeddedControl != null;

	private bool InHyperlinkTimer
	{
		get
		{
			if (m_captureTracker.HyperLinkSelectionTimer == null)
			{
				return false;
			}

			return m_captureTracker.HyperLinkSelectionTimer.Enabled;
		}
	}

	private bool ShouldForwardCharToEmbeddedControl
	{
		get
		{
			if (IsEditing && !FocusEditorOnNavigation && IsCellEditableFromKeyboardNav())
			{
				return !m_curEmbeddedControl.ContainsFocus;
			}

			return false;
		}
	}

	public event BitmapCellAccessibilityInfoNeededHandler BitmapCellAccessibilityInfoNeeded;

	[Description("Occurs when the grid is about to draw given cell. The background brush will be used only if the cell is in NON-SELECTED state.")]
	[Category(C_GridEventsCategory)]
	public event CustomizeCellGDIObjectsEventHandler CustomizeCellGDIObjectsEvent;

	[Description("Called when user clicked on some cell BEFORE any processing is done.")]
	[Category(C_GridEventsCategory)]
	public event MouseButtonClickingEventHandler MouseButtonClickingEvent;

	[Description("Called when user clicked on some cell AFTER all the processing was done but BEFORE the grid is redrawn")]
	[Category(C_GridEventsCategory)]
	public event MouseButtonClickedEventHandler MouseButtonClickedEvent;

	[Description("Called when user double clicked on some part of the grid")]
	[Category(C_GridEventsCategory)]
	public event MouseButtonDoubleClickedEventHandler MouseButtonDoubleClickedEvent;

	[Description("Called when user clicked on some header button")]
	[Category(C_GridEventsCategory)]
	public event HeaderButtonClickedEventHandler HeaderButtonClickedEvent;

	[Description("Called when width of a column has changed")]
	[Category(C_GridEventsCategory)]
	public event ColumnWidthChangedEventHandler ColumnWidthChangedEvent;

	[Description("Called when selection has changed")]
	[Category(C_GridEventsCategory)]
	public event SelectionChangedEventHandler SelectionChangedEvent;

	[Description("Called BEFORE grid does standard processing of some keys (arrows, Tab etc)")]
	[Category(C_GridEventsCategory)]
	public event StandardKeyProcessingEventHandler StandardKeyProcessingEvent;

	[Description("Occurs when a user pressed some keyboard key while the grid had the focus and a current cell")]
	[Category(C_GridEventsCategory)]
	public event KeyPressedOnCellEventHandler KeyPressedOnCellEvent;

	[Description("Occurs when contents of the currently active embedded control has changed")]
	[Category(C_GridEventsCategory)]
	public event EmbeddedControlContentsChangedEventHandler EmbeddedControlContentsChangedEvent;

	[Description("Occurs when grid detects that it is time to show tooltip")]
	[Category(C_GridEventsCategory)]
	public event TooltipDataNeededEventHandler TooltipDataNeededEvent;

	[Description("Occurs when user wants to initiate drag operation of the header of given column")]
	[Category(C_GridEventsCategory)]
	public event ColumnReorderRequestedEventHandler ColumnReorderRequestedEvent;

	[Description("Occurs when user used drag and drop operation to move a column to a new location within the grid")]
	[Category(C_GridEventsCategory)]
	public event ColumnsReorderedEventHandler ColumnsReorderedEvent;

	[Description("Occurs in response to some custom behavior within the grid control itself")]
	public event GridSpecialEventHandler GridSpecialEvent;

	protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
	{
		base.ScaleControl(factor, specified);
		GridConstants.ScaleFactor = factor.Width;
	}






	/// <summary>
	/// <see cref="ErrorHandler.Succeeded"/> token.
	/// </summary>
	protected static bool __(int hr) => ErrorHandler.Succeeded(hr);



	private void InitDefaultEmbeddedControls()
	{
		if (PreCreateEmbeddedControls)
		{
			RegisterEmbeddedControlInternal(1, new EmbeddedTextBox(this, MarginsWidth));
			RegisterEmbeddedControlInternal(2, new EmbeddedComboBox(this, MarginsWidth, ComboBoxStyle.DropDown));
			RegisterEmbeddedControlInternal(3, new EmbeddedComboBox(this, MarginsWidth, ComboBoxStyle.DropDownList));
			RegisterEmbeddedControlInternal(4, new EmbeddedSpinBox(this, MarginsWidth));
			OnFontChanged(EventArgs.Empty);
		}
	}

	public void UpdateGrid()
	{
		UpdateGrid(bRecalcRows: true);
	}

	public void UpdateGrid(bool bRecalcRows)
	{
		if (InvokeRequired)
		{
			BeginInvoke(_UpdateGridInternalDelegate, bRecalcRows);
		}
		else
		{
			UpdateGridInternal(bRecalcRows);
		}
	}

	public void InsertColumn(int nIndex, GridColumnInfo ci)
	{
		if (InvokeRequired)
		{
			Invoke(new InsertColumnInvoker(InsertColumnInternal), nIndex, ci);
		}
		else
		{
			InsertColumnInternal(nIndex, ci);
		}
	}

	public void AddColumn(GridColumnInfo ci)
	{
		if (InvokeRequired)
		{
			Invoke(new AddColumnInvoker(AddColumnInternal), ci);
		}
		else
		{
			AddColumnInternal(ci);
		}
	}

	public void DeleteColumn(int nIndex)
	{
		if (InvokeRequired)
		{
			Invoke(new DeleteColumnInvoker(DeleteColumnInternal), nIndex);
		}
		else
		{
			DeleteColumnInternal(nIndex);
		}
	}

	public void SetHeaderInfo(int nColIndex, string strText, Bitmap bmp)
	{
		int uIColumnIndexByStorageIndex = GetUIColumnIndexByStorageIndex(nColIndex);
		if (m_gridHeader[uIColumnIndexByStorageIndex].Type == EnGridColumnHeaderType.CheckBox || m_gridHeader[uIColumnIndexByStorageIndex].Type == EnGridColumnHeaderType.TextAndCheckBox)
		{
			InvalidOperationException ex = new(string.Format(ControlsResources.ExShouldSetHeaderStateForCheckBox, nColIndex));
			Diag.Dug(ex);
			throw ex;
		}

		if (InvokeRequired)
		{
			Invoke(new SetHeaderInfoInvoker(SetHeaderInfoInternal), uIColumnIndexByStorageIndex, strText, bmp, EnGridCheckBoxState.None);
		}
		else
		{
			SetHeaderInfoInternal(uIColumnIndexByStorageIndex, strText, bmp, EnGridCheckBoxState.None);
		}
	}

	public void SetHeaderAccessibleName(int nColIndex, string accessibleName)
	{
		m_gridHeader[nColIndex].AccessibleName = accessibleName;
	}

	public void SetHeaderInfo(int colIndex, string strText, EnGridCheckBoxState checkboxState)
	{
		int uIColumnIndexByStorageIndex = GetUIColumnIndexByStorageIndex(colIndex);
		if (m_gridHeader[uIColumnIndexByStorageIndex].Type != EnGridColumnHeaderType.CheckBox && m_gridHeader[uIColumnIndexByStorageIndex].Type != EnGridColumnHeaderType.TextAndCheckBox)
		{
			InvalidOperationException ex = new(string.Format(ControlsResources.ExShouldSetHeaderStateForRegualrCol, colIndex));
			Diag.Dug(ex);
			throw ex;
		}

		if (InvokeRequired)
		{
			Invoke(new SetHeaderInfoInvoker(SetHeaderInfoInternal), uIColumnIndexByStorageIndex, strText, null, checkboxState);
		}
		else
		{
			SetHeaderInfoInternal(uIColumnIndexByStorageIndex, strText, null, checkboxState);
		}
	}

	public void GetHeaderInfo(int colIndex, out string headerText, out Bitmap headerBitmap)
	{
		int uIColumnIndexByStorageIndex = GetUIColumnIndexByStorageIndex(colIndex);
		if (m_gridHeader[uIColumnIndexByStorageIndex].Type == EnGridColumnHeaderType.CheckBox || m_gridHeader[uIColumnIndexByStorageIndex].Type == EnGridColumnHeaderType.TextAndCheckBox)
		{
			InvalidOperationException ex = new(string.Format(ControlsResources.ExShouldSetHeaderStateForCheckBox, colIndex));
			Diag.Dug(ex);
			throw ex;
		}

		headerBitmap = GetHeaderInfoCommon(uIColumnIndexByStorageIndex, bitmapHeader: true, out headerText) as Bitmap;
	}

	public void GetHeaderInfo(int colIndex, out string headerText, out EnGridCheckBoxState headerCheckBox)
	{
		int uIColumnIndexByStorageIndex = GetUIColumnIndexByStorageIndex(colIndex);
		if (m_gridHeader[uIColumnIndexByStorageIndex].Type != EnGridColumnHeaderType.CheckBox && m_gridHeader[uIColumnIndexByStorageIndex].Type != EnGridColumnHeaderType.TextAndCheckBox)
		{
			InvalidOperationException ex = new(string.Format(ControlsResources.ExShouldSetHeaderStateForRegualrCol, colIndex));
			Diag.Dug(ex);
			throw ex;
		}

		headerCheckBox = (EnGridCheckBoxState)GetHeaderInfoCommon(uIColumnIndexByStorageIndex, bitmapHeader: false, out headerText);
	}

	public string GetHeadersText()
	{
		StringBuilder stringBuilder = new StringBuilder(512);
		int count = m_Columns.Count;
		GetHeaderInfo(1, out string headerText, out Bitmap headerBitmap);
		_ = headerBitmap;
		stringBuilder.Append(headerText ?? string.Empty);
		for (int i = 2; i < count; i++)
		{
			GetHeaderInfo(i, out headerText, out headerBitmap);
			_ = headerBitmap;
			stringBuilder.Append(ColumnsSeparator);
			stringBuilder.Append(headerText ?? string.Empty);
		}

		return stringBuilder.ToString();
	}

	public void ResetGrid()
	{
		if (InvokeRequired)
		{
			BeginInvoke(new MethodInvoker(ResetGridInternal));
		}
		else
		{
			ResetGridInternal();
		}
	}

	public void EnsureCellIsVisible(long nRowIndex, int nColIndex)
	{
		int uIColumnIndexByStorageIndex = GetUIColumnIndexByStorageIndex(nColIndex);
		if (InvokeRequired)
		{
			BeginInvoke(new EnsureCellIsVisibleInvoker(EnsureCellIsVisibleInternal), nRowIndex, uIColumnIndexByStorageIndex);
		}
		else
		{
			EnsureCellIsVisibleInternal(nRowIndex, uIColumnIndexByStorageIndex);
		}
	}

	public void RegisterEmbeddedControl(int editableCellType, Control embeddedControl)
	{
		if (editableCellType < 1024)
		{
			ArgumentException ex = new(ControlsResources.ExInvalidCellType, "editableCellType");
			Diag.Dug(ex);
			throw ex;
		}

		if (embeddedControl == null)
		{
			ArgumentNullException ex = new("embeddedControl");
			Diag.Dug(ex);
			throw ex;
		}

		if (embeddedControl is not IBsGridEmbeddedControlManagement)
		{
			ArgumentException ex = new(ControlsResources.ExNoIGridEmbeddedControlManagement, "embeddedControl");
			Diag.Dug(ex);
			throw ex;
		}

		if (embeddedControl is not IBsGridEmbeddedControl)
		{
			ArgumentException ex = new(ControlsResources.ExNoIGridEmbeddedControl, "embeddedControl");
			Diag.Dug(ex);
			throw ex;
		}

		if (InvokeRequired)
		{
			BeginInvoke(new RegisterEmbeddedControlInternalInvoker(RegisterEmbeddedControlInternal), editableCellType, embeddedControl);
		}
		else
		{
			RegisterEmbeddedControlInternal(editableCellType, embeddedControl);
		}
	}

	public bool StartCellEdit(long nRowIndex, int nColIndex)
	{
		int uIColumnIndexByStorageIndex = GetUIColumnIndexByStorageIndex(nColIndex);
		if (InvokeRequired)
		{
			InvokerInOutArgs invokerInOutArgs = new InvokerInOutArgs();
			Invoke(new StartCellEditInternalInvoker(StartCellEditInternalForInvoke), nRowIndex, uIColumnIndexByStorageIndex, invokerInOutArgs);
			return (bool)invokerInOutArgs.InOutParam;
		}

		return StartCellEditInternal(nRowIndex, uIColumnIndexByStorageIndex);
	}

	public void SetColumnWidth(int nColIndex, EnGridColumnWidthType widthType, int nWidth)
	{
		int uIColumnIndexByStorageIndex = GetUIColumnIndexByStorageIndex(nColIndex);
		if (InvokeRequired)
		{
			Invoke(new SetColumnWidthInternalInvoker(SetColumnWidthInternalForPublic), uIColumnIndexByStorageIndex, widthType, nWidth);
		}
		else
		{
			SetColumnWidthInternalForPublic(uIColumnIndexByStorageIndex, widthType, nWidth);
		}
	}

	public int GetColumnWidth(int nColIndex)
	{
		int uIColumnIndexByStorageIndex = GetUIColumnIndexByStorageIndex(nColIndex);
		if (InvokeRequired)
		{
			InvokerInOutArgs invokerInOutArgs = new InvokerInOutArgs();
			Invoke(new GetColumnWidthInternalInvoker(GetColumnWidthInternalForInvoke), uIColumnIndexByStorageIndex, invokerInOutArgs);
			return (int)invokerInOutArgs.InOutParam;
		}

		return GetColumnWidthInternal(uIColumnIndexByStorageIndex);
	}

	public void SetMergedHeaderResizeProportion(int colIndex, float proportion)
	{
		int uIColumnIndexByStorageIndex = GetUIColumnIndexByStorageIndex(colIndex);
		if (InvokeRequired)
		{
			Invoke(new SetMergedHeaderResizeProportionInternalInvoker(SetMergedHeaderResizeProportionInternal), uIColumnIndexByStorageIndex, proportion);
		}
		else
		{
			SetMergedHeaderResizeProportionInternal(uIColumnIndexByStorageIndex, proportion);
		}
	}

	public bool IsACellBeingEdited(out long nRowNum, out int nColNum)
	{
		if (InvokeRequired)
		{
			InvokerInOutArgs invokerInOutArgs = new InvokerInOutArgs();
			Invoke(new IsACellBeingEditedInternalInvoker(IsACellBeingEditedInternalForInvoke), invokerInOutArgs);
			bool num = (bool)invokerInOutArgs.InOutParam;
			if (num)
			{
				nRowNum = (long)invokerInOutArgs.InOutParam2;
				nColNum = m_Columns[(int)invokerInOutArgs.InOutParam3].ColumnIndex;
				return num;
			}

			nRowNum = -1L;
			nColNum = -1;
			return num;
		}

		bool num2 = IsACellBeingEditedInternal(out nRowNum, out nColNum);
		if (num2)
		{
			nColNum = m_Columns[nColNum].ColumnIndex;
		}

		return num2;
	}

	public bool StopCellEdit(bool bCommitIntoStorage)
	{
		if (InvokeRequired)
		{
			InvokerInOutArgs invokerInOutArgs = new InvokerInOutArgs();
			Invoke(new StopCellEditInternalInvoker(StopCellEditInternalForInvoke), invokerInOutArgs, bCommitIntoStorage);
			return (bool)invokerInOutArgs.InOutParam;
		}

		return StopCellEditInternal(bCommitIntoStorage);
	}

	public void SetBitmapsForCheckBoxColumn(int nColIndex, Bitmap checkedState, Bitmap uncheckedState, Bitmap indeterminateState, Bitmap disabledState)
	{
		if (InvokeRequired)
		{
			InvalidOperationException ex = new(ControlsResources.ExInvalidThreadForMethod);
			Diag.Dug(ex);
			throw ex;
		}

		if (nColIndex < 0 || nColIndex >= NumColInt)
		{
			IndexOutOfRangeException ex = new("nColIndex");
			Diag.Dug(ex);
			throw ex;
		}

		int uIColumnIndexByStorageIndex = GetUIColumnIndexByStorageIndex(nColIndex);
		if (m_Columns[uIColumnIndexByStorageIndex].ColumnType != 4)
		{
			InvalidOperationException ex = new(ControlsResources.ExColumnIsNotCheckBox.FmtRes(nColIndex));
			Diag.Dug(ex);
			throw ex;
		}

		((GridCheckBoxColumn)m_Columns[uIColumnIndexByStorageIndex]).SetCheckboxBitmaps(checkedState, uncheckedState, indeterminateState, disabledState);
	}

	public HitTestInfo HitTest(int mouseX, int mouseY)
	{
		HitTestInfo hitTestInfo = HitTestInternal(mouseX, mouseY);
		if (hitTestInfo.ColumnIndex >= 0)
		{
			return new HitTestInfo(hitTestInfo.HitTestResult, hitTestInfo.RowIndex, m_Columns[hitTestInfo.ColumnIndex].ColumnIndex, hitTestInfo.AreaRectangle);
		}

		return hitTestInfo;
	}

	public DataObject GetDataObject(bool bOnlyCurrentSelBlock)
	{
		if (InvokeRequired)
		{
			InvalidOperationException ex = new(ControlsResources.ExInvalidThreadForMethod);
			Diag.Dug(ex);
			throw ex;
		}

		return GetDataObjectInternal(bOnlyCurrentSelBlock);
	}

	public int GetUIColumnIndexByStorageIndex(int indexInStorage)
	{
		if (indexInStorage < 0)
		{
			ArgumentException ex = new();
			Diag.Dug(ex);
			throw ex;
		}

		for (int i = 0; i < m_Columns.Count; i++)
		{
			if (m_Columns[i].ColumnIndex == indexInStorage)
			{
				return i;
			}
		}

		return -1;
	}

	public int GetStorageColumnIndexByUIIndex(int indexInUI)
	{
		if (indexInUI < 0)
		{
			ArgumentException ex = new();
			Diag.Dug(ex);
			throw ex;
		}

		return m_Columns[indexInUI].ColumnIndex;
	}

	public void GetCurrentCell(out long rowIndex, out int columnIndex)
	{
		rowIndex = m_selMgr.CurrentRow;
		columnIndex = m_selMgr.CurrentColumn;
	}

	public Rectangle GetVisibleCellRectangle(long rowIndex, int columnIndex)
	{
		return m_scrollMgr.GetCellRectangle(rowIndex, columnIndex);
	}

	public void ResizeColumnToShowAllContents(int columnIndex)
	{
		if (columnIndex < 0 || columnIndex >= NumColInt)
		{
			IndexOutOfRangeException ex = new("nColIndex");
			Diag.Dug(ex);
			throw ex;
		}

		if (InvokeRequired)
		{
			Invoke(new ResizeColumnToShowAllContentsInternalInvoker(ResizeColumnToShowAllContentsInternal), GetUIColumnIndexByStorageIndex(columnIndex));
		}
		else
		{
			ResizeColumnToShowAllContentsInternal(GetUIColumnIndexByStorageIndex(columnIndex));
		}
	}

	public void ResizeColumnToShowAllContents(int columnIndex, bool considerAllRows)
	{
		if (columnIndex < 0 || columnIndex >= NumColInt)
		{
			IndexOutOfRangeException ex = new("nColIndex");
			Diag.Dug(ex);
			throw ex;
		}

		if (InvokeRequired)
		{
			Invoke(new ResizeColumnToShowAllContentsInternalInvoker2(ResizeColumnToShowAllContentsInternal), GetUIColumnIndexByStorageIndex(columnIndex), considerAllRows);
		}
		else
		{
			ResizeColumnToShowAllContentsInternal(GetUIColumnIndexByStorageIndex(columnIndex), considerAllRows);
		}
	}

	public GridColumnInfo GetGridColumnInfo(int columnStorageIndex)
	{
		int uIColumnIndexByStorageIndex = GetUIColumnIndexByStorageIndex(columnStorageIndex);
		return new GridColumnInfo
		{
			BackgroundColor = m_Columns[uIColumnIndexByStorageIndex].BackgroundBrush.Color,
			TextColor = m_Columns[uIColumnIndexByStorageIndex].TextBrush.Color,
			ColumnAlignment = m_Columns[uIColumnIndexByStorageIndex].TextAlign,
			ColumnType = m_Columns[uIColumnIndexByStorageIndex].ColumnType,
			ColumnWidth = m_Columns[uIColumnIndexByStorageIndex].WidthInPixels,
			HeaderAlignment = m_gridHeader[uIColumnIndexByStorageIndex].Align,
			HeaderType = m_gridHeader[uIColumnIndexByStorageIndex].Type,
			IsHeaderClickable = m_gridHeader[uIColumnIndexByStorageIndex].Clickable,
			IsHeaderMergedWithRight = m_gridHeader[uIColumnIndexByStorageIndex].MergedWithRight,
			IsUserResizable = m_gridHeader[uIColumnIndexByStorageIndex].Resizable,
			IsWithRightGridLine = m_Columns[uIColumnIndexByStorageIndex].RightGridLine,
			IsWithSelectionBackground = m_Columns[uIColumnIndexByStorageIndex].WithSelectionBk,
			MergedHeaderResizeProportion = m_gridHeader[uIColumnIndexByStorageIndex].MergedHeaderResizeProportion,
			TextBmpCellsLayout = m_Columns[uIColumnIndexByStorageIndex].TextBitmapLayout,
			TextBmpHeaderLayout = m_gridHeader[uIColumnIndexByStorageIndex].TextBmpLayout,
			WidthType = EnGridColumnWidthType.InPixels
		};
	}

	private void GetColumnsNumberInternalForInvoke(InvokerInOutArgs a)
	{
		a.InOutParam = m_Columns.Count;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected virtual bool ShouldSerializeHeaderFont()
	{
		return m_gridHeader.Font != null;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public virtual void ResetHeaderFont()
	{
		m_gridHeader.Font = null;
		TuneToHeaderFont(DefaultHeaderFont);
	}

	public virtual void BeginInit()
	{
		lock (this)
		{
			m_nIsInitializingCount++;
		}
	}

	public virtual void EndInit()
	{
		lock (this)
		{
			m_nIsInitializingCount--;
			if (m_nIsInitializingCount == 0 && IsHandleCreated)
			{
				UpdateScrollableAreaRect();
			}
		}
	}

	protected override void Dispose(bool bDisposing)
	{
		if (bDisposing)
		{
			if (m_backBrush != null)
			{
				m_backBrush.Dispose();
				m_backBrush = null;
			}

			DisposeCachedGDIObjects();
			if (m_gridHeader != null)
			{
				m_gridHeader.Dispose();
				m_gridHeader = null;
			}

			if (m_autoScrollTimer != null)
			{
				m_autoScrollTimer.Dispose();
				m_autoScrollTimer = null;
			}

			if (m_linkFont != null)
			{
				m_linkFont.Dispose();
				m_linkFont = null;
			}

			if (m_gridTooltip != null)
			{
				m_gridTooltip.Dispose();
				m_gridTooltip = null;
			}

			for (int i = 0; i < m_Columns.Count; i++)
			{
				m_Columns[i].Dispose();
			}

			if (m_EmbeddedControls != null)
			{
				foreach (DictionaryEntry embeddedControl in m_EmbeddedControls)
				{
					if (embeddedControl.Value is IDisposable)
					{
						(embeddedControl.Value as IDisposable).Dispose();
					}
				}

				m_EmbeddedControls.Clear();
				m_EmbeddedControls = null;
			}
		}

		base.Dispose(bDisposing);
	}

	protected override void OnRightToLeftChanged(EventArgs e)
	{
		m_Columns.SetRTL(IsRTL);
		m_gridHeader.HeaderGridButton.RTL = IsRTL;
		UpdateEmbeddedControlsRTL();
		base.OnRightToLeftChanged(e);
	}

	protected override void OnBackColorChanged(EventArgs e)
	{
		if (m_backBrush != null)
		{
			m_backBrush.Dispose();
			m_backBrush = null;
		}

		m_backBrush = new SolidBrush(BackColor);
		base.OnBackColorChanged(e);
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		base.OnSizeChanged(e);
		if (m_scrollMgr.FirstScrollableColumnIndex > 0)
		{
			if (ProcessNonScrollableVerticalAreaChange(recalcGridIfNeeded: false))
			{
				m_scrollMgr.RecalcAll(m_scrollableArea);
			}
		}
		else
		{
			UpdateScrollableAreaRect();
		}
	}

	protected override void OnFontChanged(EventArgs e)
	{
		double cAvCharWidth = m_cAvCharWidth;
		GetFontInfo(Font, out var height, out m_cAvCharWidth);
		m_scrollMgr.CellHeight = height + GridButton.ButtonAdditionalHeight;
		m_scrollMgr.SetHorizontalScrollUnitForArrows((int)m_cAvCharWidth);
		UpdateEmbeddedControlsFont();
		m_Columns.ProcessNewGridFont(Font);
		if (cAvCharWidth > 0.0)
		{
			foreach (AbstractGridColumn column in m_Columns)
			{
				if (column.IsWidthInChars)
				{
					double num = column.WidthInPixels / cAvCharWidth;
					if (num > 0.0)
					{
						column.WidthInPixels = (int)Math.Round(num * m_cAvCharWidth);
					}
				}
			}
		}

		if (m_scrollMgr.FirstScrollableColumnIndex > 0)
		{
			ProcessNonScrollableVerticalAreaChange(recalcGridIfNeeded: false);
		}
		else
		{
			UpdateScrollableAreaRect();
		}

		if (IsHandleCreated && m_nIsInitializingCount == 0)
		{
			m_scrollMgr.RecalcAll(m_scrollableArea);
		}

		m_linkFont?.Dispose();

		m_linkFont = new Font(Font, FontStyle.Underline | Font.Style);
		base.OnFontChanged(e);
	}

	protected override void OnHandleDestroyed(EventArgs e)
	{
		SystemEvents.UserPreferenceChanged -= OnUserPrefChanged;
		base.OnHandleDestroyed(e);
	}

	protected override void OnHandleCreated(EventArgs e)
	{
		SystemEvents.UserPreferenceChanged += OnUserPrefChanged;
		m_scrollMgr.SetGridWindowHandle(Handle);
		UpdateGridInternal(bRecalcRows: true);
		base.OnHandleCreated(e);
		Width--;
		Width++;
	}

	protected override void OnKeyUp(KeyEventArgs ke)
	{
		base.OnKeyUp(ke);
		if (ke.KeyCode == Keys.ShiftKey || ke.KeyCode == Keys.Menu || ke.KeyCode == Keys.ControlKey)
		{
			Point point = PointToClient(MousePosition);
			MouseEventArgs mevent = new MouseEventArgs(MouseButtons, 0, point.X, point.Y, 0);
			ProcessMouseMoveWithoutCapture(mevent);
		}
	}

	[UIPermission(SecurityAction.InheritanceDemand, Window = UIPermissionWindow.AllWindows)]
	protected override bool IsInputChar(char charCode)
	{
		return IsInputCharInternal(charCode);
	}

	protected override void OnKeyPress(KeyPressEventArgs e)
	{
		if (ShouldForwardCharToEmbeddedControl && IsInputCharInternal(e.KeyChar))
		{
			m_curEmbeddedControl.Focus();
			if (m_curEmbeddedControl is IBsGridEmbeddedControlManagement2 gridEmbeddedControlManagement)
			{
				gridEmbeddedControlManagement.ReceiveChar(e.KeyChar);
				e.Handled = true;
				return;
			}
		}

		base.OnKeyPress(e);
	}

	protected override void OnKeyDown(KeyEventArgs ke)
	{
		if (ke.KeyCode == Keys.ShiftKey || ke.KeyCode == Keys.Menu || ke.KeyCode == Keys.ControlKey)
		{
			if (InHyperlinkTimer)
			{
				TransitionHyperlinkToStd(m_captureTracker.HyperLinkSelectionTimer, null);
			}

			if (Cursor == Cursors.Hand)
			{
				Cursor = Cursors.Arrow;
				m_gridTooltip.Active = false;
			}
		}

		if (!OnStandardKeyProcessing(ke))
		{
			return;
		}

		HandleKeyboard(ke);
		if (IsEditing && !m_curEmbeddedControl.ContainsFocus && IsCellEditableFromKeyboardNav())
		{
			ForwardKeyStrokeToControl(ke);
			if (m_curEmbeddedControl.ContainsFocus)
			{
				ke.Handled = true;
				return;
			}
		}

		base.OnKeyDown(ke);
	}

	[UIPermission(SecurityAction.InheritanceDemand, Window = UIPermissionWindow.AllWindows)]
	[UIPermission(SecurityAction.LinkDemand, Window = UIPermissionWindow.AllWindows)]
	protected override bool ProcessDialogKey(Keys keyData)
	{
		switch (keyData & Keys.KeyCode)
		{
			case Keys.Tab:
			case Keys.Return:
			case Keys.Escape:
			case Keys.Prior:
			case Keys.Next:
			case Keys.End:
			case Keys.Home:
			case Keys.Left:
			case Keys.Up:
			case Keys.Right:
			case Keys.Down:
				{
					KeyEventArgs ke = new KeyEventArgs(keyData);
					if (OnStandardKeyProcessing(ke) && HandleKeyboard(ke))
					{
						return true;
					}

					break;
				}
		}

		return base.ProcessDialogKey(keyData);
	}

	[SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
	protected override bool ProcessKeyPreview(ref Message m)
	{
		if (m.Msg == Native.WM_KEYFIRST)
		{
			KeyEventArgs keyEventArgs = new KeyEventArgs((Keys)((int)m.WParam | (int)ModifierKeys));
			switch (keyEventArgs.KeyCode)
			{
				case Keys.Tab:
				case Keys.Return:
				case Keys.Escape:
				case Keys.Prior:
				case Keys.Next:
				case Keys.End:
				case Keys.Home:
				case Keys.Left:
				case Keys.Right:
					if (OnStandardKeyProcessing(keyEventArgs) && HandleKeyboard(keyEventArgs))
					{
						return true;
					}

					break;
				case Keys.Up:
				case Keys.Down:
					if (OnStandardKeyProcessing(keyEventArgs) && HandleKeyboard(keyEventArgs))
					{
						return true;
					}

					break;
			}
		}

		return base.ProcessKeyPreview(ref m);
	}

	protected override void OnPaint(PaintEventArgs pe)
	{
		Graphics graphics = pe.Graphics;
		if (m_gridStorage == null || RowCount == 0L || NumColInt == 0)
		{
			PaintEmptyGrid(graphics);
		}
		else
		{
			PaintGrid(graphics);
		}

		base.OnPaint(pe);
		if (shouldRenderFocusRectangle)
		{
			ControlPaint.DrawFocusRectangle(graphics, ClientRectangle);
		}
	}

	protected override void OnEnter(EventArgs e)
	{
		base.OnEnter(e);
		if (RowCount == 0L || ColumnsNumber == 0)
		{
			shouldRenderFocusRectangle = true;
			Refresh();
		}
		else if (SelectedCells == null || SelectedCells.Count == 0)
		{
			_ = SelectedCells = new BlockOfCellsCollection(new BlockOfCells[1]
			{
				new BlockOfCells(0L, 0)
			});
		}
	}

	protected override void OnLeave(EventArgs e)
	{
		base.OnLeave(e);
		if (shouldRenderFocusRectangle)
		{
			shouldRenderFocusRectangle = false;
			Refresh();
		}
	}

	protected override void OnMouseDown(MouseEventArgs mevent)
	{
		base.OnMouseDown(mevent);
		if (IsEmpty)
		{
			return;
		}

		try
		{
			if (m_captureTracker.CaptureHitTest != 0 && Capture)
			{
				ProcessLeftButtonUp(mevent.X, mevent.Y);
			}

			HitTestInfo infoFromHitTest = HitTestInternal(mevent.X, mevent.Y);
			m_captureTracker.SetInfoFromHitTest(infoFromHitTest);
			m_captureTracker.MouseCapturePoint = new Point(mevent.X, mevent.Y);
			if (mevent.Clicks == 2 && ActAsEnabled)
			{
				if (m_captureTracker.CaptureHitTest == EnHitTestResult.CustomCell)
				{
					HandleCustomCellDoubleClick(ModifierKeys, mevent.Button);
				}
				else
				{
					EnGridButtonArea headerArea = EnGridButtonArea.Nothing;
					if (m_captureTracker.CaptureHitTest == EnHitTestResult.HeaderButton)
					{
						headerArea = HitTestGridButton(m_captureTracker.RowIndex, m_captureTracker.ColumnIndex, m_captureTracker.CellRect, m_captureTracker.MouseCapturePoint);
					}

					OnMouseButtonDoubleClicked(m_captureTracker.CaptureHitTest, m_captureTracker.RowIndex, m_captureTracker.ColumnIndex, m_captureTracker.CellRect, mevent.Button, headerArea);
				}

				m_captureTracker.Reset();
				return;
			}

			if (mevent.Button == MouseButtons.Left && ActAsEnabled)
			{
				switch (m_captureTracker.CaptureHitTest)
				{
					case EnHitTestResult.HeaderButton:
					case EnHitTestResult.ButtonCell:
						if (!HandleButtonLBtnDown())
						{
							m_captureTracker.Reset();
							Capture = false;
						}

						if (m_captureTracker.CaptureHitTest == EnHitTestResult.ButtonCell && m_Columns[m_captureTracker.ColumnIndex] is GridButtonColumn column)
						{
							column.SetForcedButtonState(m_captureTracker.RowIndex, ButtonState.Pushed);
						}

						break;
					case EnHitTestResult.ColumnResize:
						HandleColResizeLBtnDown();
						break;
					case EnHitTestResult.TextCell:
					case EnHitTestResult.BitmapCell:
						if (!HandleStdCellLBtnDown(ModifierKeys))
						{
							m_captureTracker.Reset();
							Capture = false;
						}

						break;
					case EnHitTestResult.HyperlinkCell:
						if (ModifierKeys != 0)
						{
							if (!HandleStdCellLBtnDown(ModifierKeys))
							{
								m_captureTracker.Reset();
								Capture = false;
							}

							m_captureTracker.CaptureHitTest = EnHitTestResult.TextCell;
							break;
						}

						if (!HandleHyperlinkLBtnDown())
						{
							if (!HandleStdCellLBtnDown(ModifierKeys))
							{
								m_captureTracker.Reset();
								Capture = false;
							}

							m_captureTracker.CaptureHitTest = EnHitTestResult.TextCell;
							break;
						}

						m_captureTracker.Time = DateTime.Now;
						if (m_captureTracker.HyperLinkSelectionTimer == null)
						{
							m_captureTracker.HyperLinkSelectionTimer = new Timer();
							m_captureTracker.HyperLinkSelectionTimer.Tick += TransitionHyperlinkToStd;
							m_captureTracker.HyperLinkSelectionTimer.Interval = Convert.ToInt32(500.0);
						}

						m_captureTracker.HyperLinkSelectionTimer.Start();
						break;
					case EnHitTestResult.CustomCell:
						if (!HandleCustomCellMouseBtnDown(ModifierKeys, mevent.Button))
						{
							m_captureTracker.Reset();
							Capture = false;
						}

						break;
					case EnHitTestResult.Nothing:
					case EnHitTestResult.ColumnOnly:
					case EnHitTestResult.RowOnly:
						if (IsEditing && m_captureTracker.WasEmbeddedControlFocused && !StopCellEdit(bCommitIntoStorage: true) && m_curEmbeddedControl != null)
						{
							m_curEmbeddedControl.Focus();
						}

						break;
				}

				return;
			}

			switch (m_captureTracker.CaptureHitTest)
			{
				case EnHitTestResult.HeaderButton:
					if (m_gridHeader[m_captureTracker.ColumnIndex].Clickable && OnHeaderButtonClicked(m_captureTracker.ColumnIndex, mevent.Button, HitTestGridButton(-1L, m_captureTracker.ColumnIndex, m_captureTracker.CellRect, m_captureTracker.MouseCapturePoint)))
					{
						Refresh();
					}

					break;
				case EnHitTestResult.CustomCell:
					HandleCustomCellMouseBtnDown(ModifierKeys, mevent.Button);
					break;
				case EnHitTestResult.TextCell:
				case EnHitTestResult.ButtonCell:
				case EnHitTestResult.BitmapCell:
				case EnHitTestResult.HyperlinkCell:
					if (!OnMouseButtonClicked(m_captureTracker.RowIndex, m_captureTracker.ColumnIndex, m_captureTracker.CellRect, mevent.Button))
					{
						Refresh();
					}

					break;
			}

			m_captureTracker.Reset();
		}
		catch (Exception ex)
		{
			m_captureTracker.Reset();
			Diag.Dug(ex);
			throw ex;
		}
	}

	protected override void OnMouseUp(MouseEventArgs mevent)
	{
		if (Capture)
		{
			Capture = false;
		}

		if (mevent.Button == MouseButtons.Left)
		{
			ProcessLeftButtonUp(mevent.X, mevent.Y);
		}
		else if (m_captureTracker.CaptureHitTest == EnHitTestResult.CustomCell)
		{
			HandleCustomCellMouseBtnUp(mevent.X, mevent.Y, mevent.Button);
		}

		m_captureTracker.Reset();
		base.OnMouseUp(mevent);
	}

	protected override void OnMouseMove(MouseEventArgs mevent)
	{
		base.OnMouseMove(mevent);
		if (!Capture)
		{
			if (mevent.Button == MouseButtons.None && !IsEmpty)
			{
				ProcessMouseMoveWithoutCapture(mevent);
			}
		}
		else if (mevent.Button == MouseButtons.Left)
		{
			switch (m_captureTracker.CaptureHitTest)
			{
				case EnHitTestResult.HeaderButton:
				case EnHitTestResult.ButtonCell:
					HandleButtonMouseMove(mevent.X, mevent.Y);
					break;
				case EnHitTestResult.TextCell:
				case EnHitTestResult.BitmapCell:
				case EnHitTestResult.HyperlinkCell:
					HandleStdCellLBtnMouseMove(mevent.X, mevent.Y);
					break;
				case EnHitTestResult.ColumnResize:
					HandleColResizeMouseMove(mevent.X, mevent.Y, bLastUpdate: false);
					break;
				case EnHitTestResult.CustomCell:
					HandleCustomCellMouseMove(mevent.X, mevent.Y, mevent.Button);
					break;
			}
		}
	}

	protected override void OnGotFocus(EventArgs a)
	{
		if (!m_bInGridStorageCall && m_selMgr.SelectedBlocks.Count > 0 && !IsEditing)
		{
			Invalidate();
		}

		base.OnGotFocus(a);
	}

	protected override void OnLostFocus(EventArgs a)
	{
		if (ShouldCommitEmbeddedControlOnLostFocus && IsEditing && !ContainsFocus)
		{
			StopEditCell();
		}

		if (!m_bInGridStorageCall && !ContainsFocus && m_selMgr.SelectedBlocks.Count > 0)
		{
			Invalidate();
		}

		base.OnLostFocus(a);
	}

	[SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
	protected override void WndProc(ref Message m)
	{
		switch (m.Msg)
		{
			case 133:
				if (!DrawManager.DrawNCBorder(ref m))
				{
					base.WndProc(ref m);
				}

				break;
			case 276:
				m_scrollMgr.HandleHScroll(Native.Util.LOWORD(m.WParam));
				if (!m_withHeader)
				{
					CheckAndRePositionEmbeddedControlForSmallSizes();
				}

				break;
			case 277:
				m_scrollMgr.HandleVScroll(Native.Util.LOWORD(m.WParam));
				CheckAndRePositionEmbeddedControlForSmallSizes();
				break;
			case 526:
				OnMouseWheelHorizontal(new MouseEventArgs(MouseButtons.None, 0, 0, 0, (short)((long)m.WParam >> 16 & 0xFFFF)));
				break;
			case 513:
				m_captureTracker.WasEmbeddedControlFocused = IsEditing && m_curEmbeddedControl.ContainsFocus;
				base.WndProc(ref m);
				break;
			case 517:
				try
				{
					OnMouseUp(new MouseEventArgs(MouseButtons.Right, 1, (short)(int)m.LParam, (int)m.LParam >> 16, 0));
				}
				finally
				{
					if (Capture)
					{
						Capture = false;
					}

					DefWndProc(ref m);
				}

				break;
			default:
				base.WndProc(ref m);
				break;
		}
	}

	protected override AccessibleObject CreateAccessibilityInstance()
	{
		if (AccessibleName == null || AccessibleName == "")
		{
			AccessibleName = ControlsResources.Grid_GridControlAaName;
		}

		AccessibleRole = AccessibleRole.Table;
		return new GridControlAccessibleObject(this);
	}

	protected override void OnEnabledChanged(EventArgs e)
	{
		if (!ActAsEnabled)
		{
			if (IsEditing)
			{
				CancelEditCell();
			}

			m_hooverOverArea.Reset();
		}

		base.OnEnabledChanged(e);
	}

	protected override void OnMouseWheel(MouseEventArgs e)
	{
		base.OnMouseWheel(e);
		m_wheelDelta += e.Delta;
		float num = m_wheelDelta / 120f;
		int num2 = (int)(SystemInformation.MouseWheelScrollLines * num);
		if (num2 == 0)
		{
			return;
		}

		m_wheelDelta = 0;
		int num3 = Math.Abs(num2);
		bool flag = (ModifierKeys & Keys.Shift) == 0;
		for (int i = 0; i < num3; i++)
		{
			if (flag)
			{
				if (RowCount > 0 && RowCount > m_scrollMgr.FirstScrollableRowIndex)
				{
					if (num2 > 0)
					{
						m_scrollMgr.HandleVScroll(0);
					}
					else
					{
						m_scrollMgr.HandleVScroll(1);
					}
				}
			}
			else if (NumColInt > 0 && NumColInt > m_scrollMgr.FirstScrollableColumnIndex)
			{
				if (num2 > 0)
				{
					m_scrollMgr.HandleHScroll(0);
				}
				else
				{
					m_scrollMgr.HandleHScroll(1);
				}
			}
		}
	}

	protected void OnMouseWheelHorizontal(MouseEventArgs e)
	{
		m_wheelDelta += e.Delta;
		float num = m_wheelDelta / 120f;
		int num2 = (int)(SystemInformation.MouseWheelScrollLines * num);
		if (num2 == 0)
		{
			return;
		}

		m_wheelDelta = 0;
		int num3 = Math.Abs(num2);
		for (int i = 0; i < num3; i++)
		{
			if (NumColInt > 0 && NumColInt > m_scrollMgr.FirstScrollableColumnIndex)
			{
				m_scrollMgr.HandleHScroll(num2 > 0 ? 1 : 0);
			}
		}
	}

	protected override void OnParentChanged(EventArgs e)
	{
		if (Parent != null && Parent.Font != null && !ShouldSerializeHeaderFont())
		{
			ResetHeaderFont();
		}

		base.OnParentChanged(e);
	}

	protected override void OnParentFontChanged(EventArgs e)
	{
		if (Parent != null && Parent.Font != null && !ShouldSerializeHeaderFont())
		{
			ResetHeaderFont();
		}

		base.OnParentFontChanged(e);
	}

	protected override AccessibleObject GetAccessibilityObjectById(int objectId)
	{
		return AccessibilityObject.GetChild(objectId - 1);
	}

	protected virtual AbstractGridColumn AllocateCustomColumn(GridColumnInfo ci, int nWidthInPixels, int colIndex)
	{
		NotImplementedException ex = new(ControlsResources.ExDeriveToImplementCustomColumn);
		Diag.Dug(ex);
		throw ex;
	}

	protected virtual bool HandleCustomCellMouseBtnDown(Keys modKeys, MouseButtons btn)
	{
		NotImplementedException ex = new(ControlsResources.ExDeriveToImplementCustomColumn);
		Diag.Dug(ex);
		throw ex;
	}

	protected virtual void HandleCustomCellDoubleClick(Keys modKeys, MouseButtons btn)
	{
		NotImplementedException ex = new(ControlsResources.ExDeriveToImplementCustomColumn);
		Diag.Dug(ex);
		throw ex;
	}

	protected virtual void HandleCustomCellMouseBtnUp(int X, int Y, MouseButtons btn)
	{
		NotImplementedException ex = new(ControlsResources.ExDeriveToImplementCustomColumn);
		Diag.Dug(ex);
		throw ex;
	}

	protected virtual void SetCursorForCustomCell(long nRowIndex, int nColumnIndex, Rectangle r)
	{
		NotImplementedException ex = new(ControlsResources.ExDeriveToImplementCustomColumn);
		Diag.Dug(ex);
		throw ex;
	}

	protected virtual void HandleCustomCellMouseMove(int X, int Y, MouseButtons btn)
	{
		NotImplementedException ex = new(ControlsResources.ExDeriveToImplementCustomColumn);
		Diag.Dug(ex);
		throw ex;
	}

	protected virtual string GetCustomColumnStringForClipboardText(long rowIndex, int colIndex)
	{
		NotImplementedException ex = new(ControlsResources.ExDeriveToImplementCustomColumn);
		Diag.Dug(ex);
		throw ex;
	}

	protected virtual int MeasureWidthOfCustomColumnRows(int columnIndex, int columnType, long nFirstRow, long nLastRow, Graphics g)
	{
		return 0;
	}

	protected virtual HitTestInfo HitTestInternal(int nMouseX, int nMouseY)
	{
		long nRowIndex = -1L;
		Rectangle rCellRect = Rectangle.Empty;
		int columnIndex;
		if (NumColInt == 0)
		{
			columnIndex = -1;
			return new HitTestInfo(EnHitTestResult.Nothing, nRowIndex, columnIndex, rCellRect);
		}

		EnHitTestResult hitTestResult = HitTestColumns(nMouseX, nMouseY, out columnIndex, ref rCellRect);
		if (hitTestResult == EnHitTestResult.ColumnResize || hitTestResult == EnHitTestResult.HeaderButton)
		{
			bool flag = columnIndex > 0 && m_gridHeader[columnIndex - 1].MergedWithRight;
			if (!m_gridHeader[columnIndex].MergedWithRight && !flag)
			{
				return new HitTestInfo(hitTestResult, nRowIndex, columnIndex, rCellRect);
			}

			int nIndex = columnIndex;
			if (columnIndex > 0 && flag)
			{
				while (columnIndex > 0 && m_gridHeader[columnIndex - 1].MergedWithRight)
				{
					rCellRect.X -= m_Columns[columnIndex - 1].WidthInPixels + ScrollManager.GRID_LINE_WIDTH;
					columnIndex--;
				}

				rCellRect.X += ScrollManager.GRID_LINE_WIDTH;
				if (columnIndex == 0 && HasNonScrollableColumns)
				{
					rCellRect.X = 0;
				}
			}

			int num = NumColInt - 1;
			if (m_scrollMgr.FirstScrollableColumnIndex > 0 && columnIndex <= m_scrollMgr.FirstScrollableColumnIndex - 1)
			{
				num = Math.Min(num, m_scrollMgr.FirstScrollableColumnIndex - 1);
			}

			rCellRect.Width = 0;
			for (; columnIndex <= num; columnIndex++)
			{
				rCellRect.Width += m_Columns[columnIndex].WidthInPixels;
				if (!m_gridHeader[columnIndex].MergedWithRight || columnIndex == num)
				{
					break;
				}

				rCellRect.Width += ScrollManager.GRID_LINE_WIDTH;
			}

			if (hitTestResult == EnHitTestResult.ColumnResize && m_gridHeader[nIndex].MergedWithRight)
			{
				hitTestResult = EnHitTestResult.HeaderButton;
			}

			return new HitTestInfo(hitTestResult, nRowIndex, columnIndex, rCellRect);
		}

		if (RowCount <= 0)
		{
			return new HitTestInfo(EnHitTestResult.ColumnOnly, nRowIndex, columnIndex, rCellRect);
		}

		EnHitTestResult hitTestResult2 = HitTestRows(nMouseX, nMouseY, out nRowIndex, ref rCellRect);
		if (hitTestResult == EnHitTestResult.Nothing)
		{
			if (hitTestResult2 == EnHitTestResult.Nothing)
			{
				return new HitTestInfo(EnHitTestResult.Nothing, nRowIndex, columnIndex, rCellRect);
			}

			return new HitTestInfo(EnHitTestResult.RowOnly, nRowIndex, columnIndex, rCellRect);
		}

		if (hitTestResult2 == EnHitTestResult.Nothing)
		{
			return new HitTestInfo(EnHitTestResult.ColumnOnly, nRowIndex, columnIndex, rCellRect);
		}

		EnHitTestResult result;
		switch (m_Columns[columnIndex].ColumnType)
		{
			case 1:
				result = EnHitTestResult.TextCell;
				break;
			case 3:
			case 4:
				result = EnHitTestResult.BitmapCell;
				break;
			case 2:
				result = EnHitTestResult.ButtonCell;
				break;
			case 5:
				result = EnHitTestResult.HyperlinkCell;
				break;
			default:
				result = EnHitTestResult.CustomCell;
				break;
		}

		return new HitTestInfo(result, nRowIndex, columnIndex, rCellRect);
	}

	protected virtual void UpdateGridInternal(bool bRecalcRows)
	{
		ValidateFirstScrollableColumn();
		ValidateFirstScrollableRow();
		if (bRecalcRows)
		{
			if (m_gridStorage != null)
			{
				m_scrollMgr.RowCount = m_gridStorage.RowCount;
			}
			else
			{
				m_scrollMgr.RowCount = 0L;
				m_selMgr.Clear();
			}

			if (!m_scrollableArea.IsEmpty && IsHandleCreated)
			{
				m_scrollMgr.RecalcAll(m_scrollableArea);
			}
		}

		if (IsHandleCreated && m_nIsInitializingCount == 0)
		{
			Refresh();
		}
	}

	protected virtual void ResetGridInternal()
	{
		if (IsEditing)
		{
			CancelEditCell();
		}

		m_gridStorage = null;
		m_captureTracker.Reset();
		m_scrollMgr.Reset();
		m_Columns.Clear();
		m_gridHeader.Reset();
		m_selMgr.Clear();
		m_hooverOverArea.Reset();
		Refresh();
	}

	protected virtual void AddColumnInternal(GridColumnInfo ci)
	{
		InsertColumnInternal(m_Columns.Count, ci);
	}

	protected virtual void InsertColumnInternal(int nIndex, GridColumnInfo ci)
	{
		if (nIndex < 0 || nIndex > m_Columns.Count)
		{
			ArgumentOutOfRangeException ex = new("nIndex", nIndex, "");
			Diag.Dug(ex);
			throw ex;
		}

		if (ci.MergedHeaderResizeProportion < 0f || ci.MergedHeaderResizeProportion > 1f)
		{
			ArgumentException ex = new(ControlsResources.ExInvalidMergedHeaderResizeProportion, "GridColumnInfo.MergedHeaderResizeProportion");
			Diag.Dug(ex);
			throw ex;
		}

		ValidateColumnType(ci.ColumnType);
		int columnWidthInPixels = GetColumnWidthInPixels(ci.ColumnWidth, ci.WidthType);
		if (columnWidthInPixels > C_MaximumColumnWidth)
		{
			ArgumentException ex = new(ControlsResources.ExColumnWidthShouldBeLessThanMax.FmtRes(C_MaximumColumnWidth), "nWidth");
			Diag.Dug(ex);
			throw ex;
		}

		if (IsEditing)
		{
			CancelEditCell();
		}

		AbstractGridColumn gridColumn = AllocateColumn(ci.ColumnType, ci, columnWidthInPixels, nIndex);
		gridColumn.ProcessNewGridFont(Font);
		gridColumn.SetRTL(IsRTL);
		m_Columns.Insert(nIndex, gridColumn);
		m_gridHeader.InsertHeaderItem(nIndex, ci);
		if (m_nIsInitializingCount == 0)
		{
			m_scrollMgr.ProcessNewCol(nIndex);
			if (IsHandleCreated)
			{
				Refresh();
			}
		}
	}

	protected virtual void DeleteColumnInternal(int nIndex)
	{
		if (nIndex < 0 || nIndex >= m_Columns.Count)
		{
			ArgumentOutOfRangeException ex = new("nIndex", nIndex, "");
			Diag.Dug(ex);
			throw ex;
		}

		if (m_scrollMgr.FirstScrollableColumnIndex == m_Columns.Count - 1 && m_Columns.Count != 1)
		{
			ArgumentException ex = new(ControlsResources.ExFirstScrollableWillBeBad, "nIndex");
			Diag.Dug(ex);
			throw ex;
		}

		if (IsEditing)
		{
			CancelEditCell();
		}

		AbstractGridColumn gridColumn = m_Columns[nIndex];
		try
		{
			m_Columns.RemoveAtAndAdjust(nIndex);
			m_scrollMgr.ProcessDeleteCol(nIndex, gridColumn.WidthInPixels);
		}
		finally
		{
			gridColumn.Dispose();
		}

		m_gridHeader.DeleteItem(nIndex);
		if (IsHandleCreated && !IsInitializing)
		{
			Refresh();
		}
	}

	protected virtual void SetHeaderInfoInternal(int nIndex, string strText, Bitmap bmp, EnGridCheckBoxState checkboxState)
	{
		if (nIndex < 0 || nIndex >= m_Columns.Count)
		{
			ArgumentOutOfRangeException ex = new("nIndex", nIndex, "");
			Diag.Dug(ex);
			throw ex;
		}

		m_gridHeader.SetHeaderItemInfo(nIndex, strText, bmp, checkboxState);
	}

	private object GetHeaderInfoCommon(int colIndex, bool bitmapHeader, out string headerText)
	{
		if (InvokeRequired)
		{
			InvokerInOutArgs invokerInOutArgs = new InvokerInOutArgs();
			Invoke(new GetHeaderInfoInvoker(GetHeaderInfoInternalForInvoke), colIndex, invokerInOutArgs);
			headerText = null;
			if (invokerInOutArgs.InOutParam != null)
			{
				headerText = invokerInOutArgs.InOutParam as string;
			}

			if (bitmapHeader)
			{
				if (invokerInOutArgs.InOutParam2 == null)
				{
					return null;
				}

				return invokerInOutArgs.InOutParam2;
			}

			return invokerInOutArgs.InOutParam3;
		}

		GetHeaderInfoInternal(colIndex, out headerText, out var bmp, out var checkBoxState);
		if (bitmapHeader)
		{
			return bmp;
		}

		return checkBoxState;
	}

	protected virtual void GetHeaderInfoInternal(int colIndex, out string headerText, out Bitmap bmp, out EnGridCheckBoxState checkBoxState)
	{
		if (colIndex < 0 || colIndex >= m_Columns.Count)
		{
			ArgumentOutOfRangeException ex = new("colIndex", colIndex, "");
			Diag.Dug(ex);
			throw ex;
		}

		if (m_gridHeader[colIndex].Type == EnGridColumnHeaderType.CheckBox || m_gridHeader[colIndex].Type == EnGridColumnHeaderType.TextAndCheckBox)
		{
			bmp = null;
			checkBoxState = m_gridHeader[colIndex].CheckboxState;
		}
		else
		{
			bmp = m_gridHeader[colIndex].Bmp;
			checkBoxState = EnGridCheckBoxState.None;
		}

		headerText = m_gridHeader[colIndex].Text;
	}

	private void GetHeaderInfoInternalForInvoke(int colIndex, InvokerInOutArgs outArgs)
	{
		GetHeaderInfoInternal(colIndex, out var headerText, out var bmp, out var checkBoxState);
		outArgs.InOutParam = headerText;
		outArgs.InOutParam2 = bmp;
		outArgs.InOutParam3 = checkBoxState;
	}

	protected virtual void EnsureCellIsVisibleInternal(long nRowIndex, int nColIndex)
	{
		if (nRowIndex < 0 || nRowIndex >= RowCount)
		{
			ArgumentOutOfRangeException ex = new("nRowIndex", nRowIndex, ControlsResources.ExRowIndexShouldBeInRange);
			Diag.Dug(ex);
			throw ex;
		}

		if (nColIndex < 0 || nColIndex >= m_Columns.Count)
		{
			ArgumentOutOfRangeException ex = new("nColIndex", nColIndex, ControlsResources.ExColumnIndexShouldBeInRange);
			Diag.Dug(ex);
			throw ex;
		}

		if (nRowIndex < m_scrollMgr.FirstRowIndex || nRowIndex > m_scrollMgr.LastRowIndex || nColIndex < m_scrollMgr.FirstColumnIndex || nColIndex > m_scrollMgr.LastColumnIndex)
		{
			m_scrollMgr.EnsureCellIsVisible(nRowIndex, nColIndex);
		}
	}

	protected virtual void RegisterEmbeddedControlInternal(int editableCellType, Control embeddedControl)
	{
		if (!m_EmbeddedControls.ContainsKey(editableCellType))
		{
			m_EmbeddedControls[editableCellType] = embeddedControl;
			embeddedControl.Font = Font;
			embeddedControl.Height = m_scrollMgr.CellHeight + 2 * ScrollManager.GRID_LINE_WIDTH;
			embeddedControl.RightToLeft = IsRTL ? RightToLeft.Yes : RightToLeft.No;
		}
	}

	protected virtual void SelectedCellsInternal(BlockOfCellsCollection col, bool bSet)
	{
		if (!bSet)
		{
			col.Clear();
			col.AddRange(m_selMgr.SelectedBlocks);
			return;
		}

		if (col != null && col.Count > 1 && m_selMgr.OnlyOneSelItem)
		{
			ArgumentException ex = new(ControlsResources.ExNoMultiBlockSelInSingleSelMode, "SelectionBlocks");
			Diag.Dug(ex);
			throw ex;
		}

		if (col != null)
		{
			foreach (BlockOfCells item in col)
			{
				if (item.X < 0 || item.Y < 0)
				{
					ArgumentException ex = new(string.Format(ControlsResources.ExNonExistingGridSelectionBlock, item.X.ToString(), item.Y.ToString(), item.Right.ToString(), item.Bottom.ToString()), "SelectionBlocks");
					Diag.Dug(ex);
					throw ex;
				}

				if (item.Y >= RowCount || item.Bottom >= RowCount)
				{
					ArgumentException ex = new(string.Format(ControlsResources.ExNonExistingGridSelectionBlock, item.X.ToString(), item.Y.ToString(), item.Right.ToString(), item.Bottom.ToString()), "SelectionBlocks");
					Diag.Dug(ex);
					throw ex;
				}

				if (item.X >= NumColInt || item.Right >= NumColInt)
				{
					ArgumentException ex = new(string.Format(ControlsResources.ExNonExistingGridSelectionBlock, item.X.ToString(), item.Y.ToString(), item.Right.ToString(), item.Bottom.ToString()), "SelectionBlocks");
					Diag.Dug(ex);
					throw ex;
				}
			}
		}

		if (IsEditing)
		{
			CancelEditCell();
		}

		m_selMgr.Clear();
		if (col != null)
		{
			foreach (BlockOfCells item2 in col)
			{
				m_selMgr.StartNewBlock(item2.Y, item2.X);
				m_selMgr.UpdateCurrentBlock(item2.Bottom, item2.Right);
			}
		}

		OnSelectionChanged(m_selMgr.SelectedBlocks);
		Invalidate();
	}

	protected virtual bool OnBeforeGetClipboardTextForCells(StringBuilder clipboardText, long nStartRow, long nEndRow, int nStartCol, int nEndCol)
	{
		return false;
	}

	private void SelectedCellsInternalForInvoke(InvokerInOutArgs a, bool bSet)
	{
		BlockOfCellsCollection col = (BlockOfCellsCollection)a.InOutParam;
		SelectedCellsInternal(col, bSet);
	}

	protected virtual bool StartCellEditInternal(long nRowIndex, int nColIndex)
	{
		if (!IsHandleCreated)
		{
			InvalidOperationException ex = new();
			Diag.Dug(ex);
			throw ex;
		}

		if (nRowIndex < 0 || nRowIndex >= RowCount)
		{
			ArgumentOutOfRangeException ex = new("nRowIndex", nRowIndex, "");
			Diag.Dug(ex);
			throw ex;
		}

		if (nColIndex < 0 || nColIndex >= NumColInt)
		{
			ArgumentOutOfRangeException ex = new("nColIndex", nColIndex, "");
			Diag.Dug(ex);
			throw ex;
		}

		if (m_Columns[nColIndex].ColumnType == 2 || m_Columns[nColIndex].ColumnType == 4)
		{
			return false;
		}

		int num = m_gridStorage.IsCellEditable(nRowIndex, m_Columns[nColIndex].ColumnIndex);
		if (num == 0)
		{
			return false;
		}

		m_selMgr.Clear();
		EnsureCellIsVisibleInternal(nRowIndex, nColIndex);
		bool num2 = StartEditingCell(nRowIndex, nColIndex, num);
		if (num2)
		{
			m_selMgr.StartNewBlock(nRowIndex, nColIndex);
			OnSelectionChanged(m_selMgr.SelectedBlocks);
			Refresh();
		}

		return num2;
	}

	protected Graphics GraphicsFromHandle()
	{
		return Graphics.FromHwnd(Handle);
	}

	private void StartCellEditInternalForInvoke(long nRowIndex, int nColIndex, InvokerInOutArgs args)
	{
		args.InOutParam = StartCellEditInternal(nRowIndex, nColIndex);
	}

	protected virtual void SetColumnWidthInternal(int nColIndex, EnGridColumnWidthType widthType, int nWidth)
	{
		if (nColIndex < 0 || nColIndex >= NumColInt)
		{
			ArgumentOutOfRangeException ex = new("nColIndex", nColIndex, "");
			Diag.Dug(ex);
			throw ex;
		}

		if (nWidth <= 0)
		{
			ArgumentException ex = new(ControlsResources.ExColumnWidthShouldBeGreaterThanZero, "nWidth");
			Diag.Dug(ex);
			throw ex;
		}

		int columnWidthInPixels = GetColumnWidthInPixels(nWidth, widthType);
		if (columnWidthInPixels > C_MaximumColumnWidth)
		{
			ArgumentException ex = new(ControlsResources.ExColumnWidthShouldBeLessThanMax.FmtRes(C_MaximumColumnWidth), "nWidth");
			Diag.Dug(ex);
			throw ex;
		}

		int widthInPixels = m_Columns[nColIndex].WidthInPixels;
		m_Columns[nColIndex].WidthInPixels = columnWidthInPixels;
		bool flag = false;
		if (nColIndex < m_scrollMgr.FirstScrollableColumnIndex)
		{
			flag = ProcessNonScrollableVerticalAreaChange(recalcGridIfNeeded: false);
		}
		else
		{
			m_scrollMgr.UpdateColWidth(nColIndex, widthInPixels, columnWidthInPixels, bFinalUpdate: true);
		}

		if (m_nIsInitializingCount == 0)
		{
			if (flag)
			{
				m_scrollMgr.RecalcAll(m_scrollableArea);
			}
			else
			{
				Invalidate();
			}

			OnColumnWidthChanged(nColIndex, columnWidthInPixels);
		}
	}

	protected virtual void SetMergedHeaderResizeProportionInternal(int colIndex, float proportion)
	{
		if (proportion < 0f || proportion > 1f)
		{
			ArgumentException ex = new(ControlsResources.ExInvalidMergedHeaderResizeProportion, "proportion");
			Diag.Dug(ex);
			throw ex;
		}

		if (!m_gridHeader[colIndex].MergedWithRight)
		{
			ArgumentException ex = new(ControlsResources.ExInvalidColIndexForMergedResizeProp, "colIndex");
			Diag.Dug(ex);
			throw ex;
		}

		m_gridHeader[colIndex].MergedHeaderResizeProportion = proportion;
	}

	protected virtual int GetColumnWidthInternal(int nColIndex)
	{
		if (nColIndex < 0 || nColIndex >= NumColInt)
		{
			ArgumentOutOfRangeException ex = new("nColIndex", nColIndex, "");
			Diag.Dug(ex);
			throw ex;
		}

		return m_Columns[nColIndex].WidthInPixels;
	}

	private void GetColumnWidthInternalForInvoke(int nColIndex, InvokerInOutArgs args)
	{
		args.InOutParam = GetColumnWidthInternal(nColIndex);
	}

	protected virtual bool IsACellBeingEditedInternal(out long rowIndex, out int columnIndex)
	{
		if (m_curEmbeddedControl != null)
		{
			IBsGridEmbeddedControl gridEmbeddedControl = (IBsGridEmbeddedControl)m_curEmbeddedControl;
			rowIndex = gridEmbeddedControl.RowIndex;
			columnIndex = GetUIColumnIndexByStorageIndex(gridEmbeddedControl.ColumnIndex);
			return true;
		}

		rowIndex = -1L;
		columnIndex = -1;
		return false;
	}

	private void IsACellBeingEditedInternalForInvoke(InvokerInOutArgs args)
	{
		args.InOutParam = IsACellBeingEditedInternal(out var rowIndex, out var columnIndex);
		args.InOutParam2 = rowIndex;
		args.InOutParam3 = columnIndex;
	}

	protected virtual bool StopCellEditInternal(bool bCommitIntoStorage)
	{
		if (m_curEmbeddedControl == null)
		{
			return false;
		}

		if (bCommitIntoStorage)
		{
			return StopEditCell();
		}

		CancelEditCell();
		return true;
	}

	private void StopCellEditInternalForInvoke(InvokerInOutArgs args, bool bCommitIntoStorage)
	{
		args.InOutParam = StopCellEditInternal(bCommitIntoStorage);
	}

	protected virtual void AlwaysHighlightSelectionInt(bool bAlwaysHighlight)
	{
		if (m_alwaysHighlightSelection != bAlwaysHighlight)
		{
			m_alwaysHighlightSelection = bAlwaysHighlight;
			if (m_selMgr.SelectedBlocks.Count > 0)
			{
				Invalidate();
			}
		}
	}

	protected virtual void PaintVertGridLines(Graphics g, int nFirstCol, int nLastCol, int nFirstColPos, int nTopMostPoint, int nBottomMostPoint)
	{
		Pen gridLinesPen = GridLinesPen;
		int num = nFirstColPos;
		if (nFirstCol == 0)
		{
			g.DrawLine(gridLinesPen, num, nTopMostPoint, num, nBottomMostPoint);
		}

		for (int i = nFirstCol; i <= nLastCol; i++)
		{
			num += m_Columns[i].WidthInPixels + ScrollManager.GRID_LINE_WIDTH;
			if (m_Columns[i].RightGridLine)
			{
				g.DrawLine(gridLinesPen, num, nTopMostPoint, num, nBottomMostPoint);
			}
		}
	}

	protected virtual void PaintHorizGridLines(Graphics g, long rows, int nFirstRowPos, int nLeftMostPoint, int nRightMostPoint, bool bAdjust)
	{
		Pen gridLinesPen = GridLinesPen;
		if (bAdjust)
		{
			nRightMostPoint = Math.Min(m_scrollableArea.Right, nRightMostPoint);
		}

		int num = nFirstRowPos;
		int cellHeight = m_scrollMgr.CellHeight;
		for (long num2 = 0L; num2 <= rows; num2++)
		{
			g.DrawLine(gridLinesPen, nLeftMostPoint, num, nRightMostPoint, num);
			num += cellHeight + ScrollManager.GRID_LINE_WIDTH;
		}
	}

	protected virtual void PaintCurrentCellRect(Graphics g, Rectangle r)
	{
		r.X--;
		r.Width++;
		ControlPaint.DrawFocusRectangle(g, r);
	}

	protected virtual int CalculateHeaderHeight(Font headerFont)
	{
		GetFontInfo(headerFont, out var height, out var _);
		return height + GridButton.ButtonAdditionalHeight;
	}

	protected virtual void PaintHeader(Graphics g)
	{
		if (HasNonScrollableColumns)
		{
			PaintHeaderHelper(g, 0, m_scrollMgr.FirstScrollableColumnIndex - 1, 0, 0);
		}

		Region clip = g.Clip;
		try
		{
			Rectangle clip2 = new Rectangle(m_scrollableArea.X, 0, m_scrollableArea.Width, m_scrollableArea.Y);
			g.SetClip(clip2);
			PaintHeaderHelper(g, m_scrollMgr.FirstColumnIndex, m_scrollMgr.LastColumnIndex, m_scrollMgr.FirstColumnPos, 0);
		}
		finally
		{
			g.Clip = clip;
		}
	}

	protected virtual int GetMinWidthOfColumn(int colIndex)
	{
		return Math.Min((int)m_cAvCharWidth, m_Columns[colIndex].WidthInPixels);
	}

	protected virtual int CalcValidColWidth(int X)
	{
		int num = X - m_captureTracker.CellRect.Left - m_captureTracker.MouseOffsetForColResize;
		if (num < m_captureTracker.MinWidthDuringColResize)
		{
			num = m_captureTracker.MinWidthDuringColResize;
		}

		if (num > C_MaximumColumnWidth)
		{
			num = C_MaximumColumnWidth;
		}

		return num;
	}

	protected virtual void PaintGridBackground(Graphics g)
	{
		if (ActAsEnabled)
		{
			g.FillRectangle(m_backBrush, ClientRectangle);
		}
		else
		{
			g.FillRectangle(AbstractGridColumn.DisabledBackgroundBrush, ClientRectangle);
		}
	}

	protected virtual void OnStartedCellEdit()
	{
		m_curEmbeddedControl.LostFocus += _OnEmbeddedControlLostFocusHandler;
	}

	protected virtual void OnStoppedCellEdit()
	{
		m_curEmbeddedControl.LostFocus -= _OnEmbeddedControlLostFocusHandler;
	}

	protected virtual GridPrinter AllocateGridPrinter()
	{
		return new GridPrinter(this);
	}

	protected virtual void SetCursorFromHitTest(EnHitTestResult ht, long nRowIndex, int nColumnIndex, Rectangle cellRect)
	{
		if (ApcManager.DisposableWaitCursor != null)
		{
			// Tracer.Trace(GetType(), "SetCursorFromHitTest()", "DisposableWaitCursor IS active");
			Cursor = Cursors.WaitCursor;
			return;
		}

		// Tracer.Trace(GetType(), "SetCursorFromHitTest()", "DisposableWaitCursor is NOT active");


		switch (ht)
		{
			case EnHitTestResult.ColumnResize:
				Cursor = Cursors.VSplit;
				return;
			case EnHitTestResult.CustomCell:
				SetCursorForCustomCell(nRowIndex, nColumnIndex, cellRect);
				return;
			case EnHitTestResult.HyperlinkCell:
				{
					if (ModifierKeys != 0)
					{
						break;
					}

					using (Graphics g = GraphicsFromHandle())
					{
						if (m_Columns[nColumnIndex].IsPointOverTextInCell(PointToClient(MousePosition), cellRect, m_gridStorage, nRowIndex, g, m_linkFont))
						{
							Cursor = Cursors.Hand;
						}
						else
						{
							Cursor = Cursors.Arrow;
						}
					}

					return;
				}
		}

		Cursor = Cursors.Arrow;
	}

	protected virtual void ProcessForTooltip(EnHitTestResult ht, long nRowNumber, int nColNumber)
	{
		if (ht == m_hooverOverArea.HitTest && nRowNumber == m_hooverOverArea.RowNumber && nColNumber == m_hooverOverArea.ColumnNumber && m_gridTooltip.Active)
		{
			if (ht != EnHitTestResult.HyperlinkCell || ModifierKeys != 0)
			{
				return;
			}

			using Graphics g = GraphicsFromHandle();
			if (!m_Columns[nColNumber].IsPointOverTextInCell(PointToClient(MousePosition), m_scrollMgr.GetCellRectangle(nRowNumber, nColNumber), m_gridStorage, nRowNumber, g, m_linkFont))
			{
				m_gridTooltip.Active = false;
			}
			else if (Cursor == Cursors.Hand)
			{
				return;
			}
		}

		string toolTipText = string.Empty;
		if (ht == EnHitTestResult.HyperlinkCell && ModifierKeys == Keys.None && Cursor == Cursors.Hand)
		{
			try
			{
				using Graphics g2 = GraphicsFromHandle();
				if (m_Columns[nColNumber].IsPointOverTextInCell(PointToClient(MousePosition), m_scrollMgr.GetCellRectangle(nRowNumber, nColNumber), m_gridStorage, nRowNumber, g2, m_linkFont))
				{
					toolTipText = ControlsResources.Grid_ToolTipUrl.FmtRes(m_gridStorage.GetCellDataAsString(nRowNumber, m_Columns[nColNumber].ColumnIndex));
				}
			}
			catch (Exception ex)
			{
				Trace.TraceError(ex.Message);
			}
		}

		m_hooverOverArea.Reset();
		if (!OnTooltipDataNeeded(ht, nRowNumber, nColNumber, ref toolTipText) && !(toolTipText != string.Empty))
		{
			if (m_gridTooltip.Active)
			{
				m_gridTooltip.Active = false;
			}

			return;
		}

		if (m_gridTooltip.Active)
		{
			m_gridTooltip.Active = false;
		}

		m_hooverOverArea.HitTest = ht;
		m_hooverOverArea.RowNumber = nRowNumber;
		m_hooverOverArea.ColumnNumber = nColNumber;
		m_gridTooltip.SetToolTip(this, toolTipText);
		m_gridTooltip.Active = true;
	}

	protected virtual string GetTextBasedColumnStringForClipboardText(long rowIndex, int colIndex)
	{
		return m_gridStorage.GetCellDataAsString(rowIndex, m_Columns[colIndex].ColumnIndex);
	}

	protected virtual DataObject GetDataObjectInternal(bool bOnlyCurrentSelBlock)
	{
		if (m_selMgr.SelectedBlocks.Count > 0)
		{
			StringBuilder stringBuilder = new StringBuilder(256);
			if (bOnlyCurrentSelBlock)
			{
				if (m_selMgr.CurrentSelectionBlockIndex < 0)
				{
					InvalidOperationException ex = new(ControlsResources.ExInvalidCurrentSelBlockForClipboard);
					Diag.Dug(ex);
					throw ex;
				}

				stringBuilder.Append(GetClipboardTextForSelectionBlock(m_selMgr.CurrentSelectionBlockIndex));
			}
			else
			{
				int count = m_selMgr.SelectedBlocks.Count;
				for (int i = 0; i < count; i++)
				{
					stringBuilder.Append(GetClipboardTextForSelectionBlock(i));
					if (i < count - 1)
					{
						stringBuilder.Append(NewLineCharacters);
					}
				}
			}

			DataObject dataObject = new DataObject();
			dataObject.SetData(DataFormats.UnicodeText, autoConvert: true, stringBuilder.ToString());
			return dataObject;
		}

		return null;
	}

	protected virtual bool IsCellEditableFromKeyboardNav()
	{
		long currentRow = m_selMgr.CurrentRow;
		int currentColumn = m_selMgr.CurrentColumn;
		int columnType = m_Columns[currentColumn].ColumnType;
		if (columnType != 2 && columnType != 4)
		{
			return m_gridStorage.IsCellEditable(currentRow, m_Columns[currentColumn].ColumnIndex) != 0;
		}

		return false;
	}

	protected virtual string GetCellStringForResizeToShowAll(long rowIndex, int storageColIndex, out StringFormat sf)
	{
		sf = new StringFormat(StringFormatFlags.NoWrap | StringFormatFlags.LineLimit)
		{
			HotkeyPrefix = HotkeyPrefix.None,
			Trimming = StringTrimming.EllipsisCharacter,
			LineAlignment = StringAlignment.Center
		};
		return m_gridStorage.GetCellDataAsString(rowIndex, storageColIndex);
	}

	protected virtual string GetCellStringForResizeToShowAll(long rowIndex, int storageColIndex, out TextFormatFlags tff)
	{
		tff = TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter;
		return m_gridStorage.GetCellDataAsString(rowIndex, storageColIndex);
	}

	protected virtual bool AdjustSelectionForButtonCellMouseClick()
	{
		m_selMgr.Clear();
		m_selMgr.StartNewBlock(m_captureTracker.RowIndex, m_captureTracker.ColumnIndex);
		OnSelectionChanged(m_selMgr.SelectedBlocks);
		return true;
	}

	protected virtual void ResizeColumnToShowAllContentsInternal(int columnIndex)
	{
		ResizeColumnToShowAllContentsInternal(columnIndex, considerAllRows: false);
	}

	protected virtual void ResizeColumnToShowAllContentsInternal(int columnIndex, bool considerAllRows)
	{
		if (columnIndex < 0 || columnIndex >= NumColInt)
		{
			ArgumentOutOfRangeException ex = new("columnIndex");
			Diag.Dug(ex);
			throw ex;
		}

		if (!IsHandleCreated)
		{
			InvalidOperationException ex = new();
			Diag.Dug(ex);
			throw ex;
		}

		try
		{
			using Graphics g = GraphicsFromHandle();
			int num = 0;
			int i = columnIndex;
			while (i > 0 && m_gridHeader[i - 1].MergedWithRight)
			{
				i--;
			}

			int num2 = i;
			int num3 = 0;
			int[] array = new int[columnIndex - i + 1];
			for (; i <= columnIndex; i++)
			{
				array[i - num2] = m_Columns[i].WidthInPixels;
				if (RowCount > 0)
				{
					num = m_Columns[i].ColumnType >= 1024 ? !considerAllRows ? MeasureWidthOfCustomColumnRows(i, m_Columns[i].ColumnType, m_scrollMgr.FirstRowIndex, m_scrollMgr.LastRowIndex, g) : MeasureWidthOfCustomColumnRows(i, m_Columns[i].ColumnType, 0L, RowCount - 1, g) : !considerAllRows ? MeasureWidthOfRows(i, m_Columns[i].ColumnType, m_scrollMgr.FirstRowIndex, m_scrollMgr.LastRowIndex, g) : MeasureWidthOfRows(i, m_Columns[i].ColumnType, 0L, RowCount - 1, g);
				}

				if (m_scrollMgr.FirstScrollableRowIndex != 0)
				{
					num = m_Columns[i].ColumnType < 1024 ? Math.Max(num, MeasureWidthOfRows(i, m_Columns[i].ColumnType, 0L, m_scrollMgr.FirstScrollableRowIndex - 1, g)) : Math.Max(num, MeasureWidthOfCustomColumnRows(i, m_Columns[i].ColumnType, m_scrollMgr.FirstRowIndex, m_scrollMgr.LastRowIndex, g));
				}

				num3 += num;
				if (i < columnIndex)
				{
					num3 += ScrollManager.GRID_LINE_WIDTH;
				}

				if (num > 0)
				{
					array[i - num2] = num;
				}

				if (!WithHeader || i != columnIndex)
				{
					continue;
				}

				Rectangle r = new Rectangle(0, 0, 100000, 100000);
				Bitmap bmp = m_gridHeader[columnIndex].Bmp;
				string text = m_gridHeader[columnIndex].Text;
				TextFormatFlags sFormat = GridConstants.DefaultTextFormatFlags;
				int num4 = GridButton.CalculateInitialContentsRect(g, r, text, bmp, HorizontalAlignment.Left, HeaderFont, IsRTL, ref sFormat, out int nStringWidth).Width;
				if (num4 > 0)
				{
					num4 += 2 * MarginsWidth;
				}

				if (num4 <= num3)
				{
					continue;
				}

				int num5 = 0;
				int num6 = 0;
				for (num6 = num2; num6 <= columnIndex; num6++)
				{
					m_Columns[num6].OrigWidthInPixelsDuringResize = array[num6 - num2];
					num5 += array[num6 - num2];
				}

				int[] array2 = new int[array.Length];
				num5 += ScrollManager.GRID_LINE_WIDTH * (columnIndex - num2);
				ResizeMultipleColumns(num2, columnIndex, num4 - num5, bFinalUpdate: true, array2);
				for (num6 = 0; num6 <= columnIndex - num2; num6++)
				{
					if (RowCount > 0)
					{
						array[num6] = Math.Max(array[num6], array2[num6]);
					}
					else
					{
						array[num6] = Math.Max(array2[num6], GetMinWidthOfColumn(num6));
					}
				}
			}

			for (int j = 0; j <= columnIndex - num2; j++)
			{
				if (array[j] > 0)
				{
					SetColumnWidthInternal(j + num2, EnGridColumnWidthType.InPixels, Math.Min(array[j], 20000));
				}
			}
		}
		finally
		{
			Update();
		}
	}

	protected virtual bool HandleTabOnLastOrFirstCell(bool goingLeft)
	{
		return true;
	}

	protected virtual GridTextColumn AllocateTextColumn(GridColumnInfo ci, int nWidthInPixels, int colIndex)
	{
		return new GridTextColumn(ci, nWidthInPixels, colIndex);
	}

	protected virtual GridBitmapColumn AllocateBitmapColumn(GridColumnInfo ci, int nWidthInPixels, int colIndex)
	{
		return new GridBitmapColumn(ci, nWidthInPixels, colIndex);
	}

	protected virtual GridButtonColumn AllocateButtonColumn(GridColumnInfo ci, int nWidthInPixels, int colIndex)
	{
		return new GridButtonColumn(ci, nWidthInPixels, colIndex);
	}

	protected virtual GridCheckBoxColumn AllocateCheckBoxColumn(GridColumnInfo ci, int nWidthInPixels, int colIndex)
	{
		return new GridCheckBoxColumn(ci, nWidthInPixels, colIndex);
	}

	protected virtual GridHyperlinkColumn AllocateHyperlinkColumn(GridColumnInfo ci, int nWidthInPixels, int colIndex)
	{
		GridHyperlinkColumn gridHyperlinkColumn = new GridHyperlinkColumn(ci, nWidthInPixels, colIndex);
		LinkLabel linkLabel = new LinkLabel();
		if (gridHyperlinkColumn.TextBrush != null)
		{
			gridHyperlinkColumn.TextBrush.Dispose();
			gridHyperlinkColumn.TextBrush = null;
		}

		gridHyperlinkColumn.TextBrush = new SolidBrush(linkLabel.LinkColor);
		return gridHyperlinkColumn;
	}

	protected virtual void OnResetFirstScrollableColumn(int prevousFirstScrollableColumn, int newFirstScrollableColumn)
	{
	}

	private EnHitTestResult HitTestColumnsHelper(int nMouseX, int nMouseY, int nFirstCol, int nLastCol, int nPos, out int nColIndex, ref Rectangle rCellRect)
	{
		EnHitTestResult hitTestResult = EnHitTestResult.Nothing;
		int num = 0;
		nColIndex = -1;
		for (int i = nFirstCol; i <= nLastCol; i++)
		{
			AbstractGridColumn gridColumn = m_Columns[i];
			int num2 = num;
			num = gridColumn.WidthInPixels;
			if (nMouseY < HeaderHeight && nMouseY >= 0)
			{
				if (nMouseX <= nPos + AbstractGridColumn.CELL_CONTENT_OFFSET && nMouseX >= nPos && (nMouseX > AbstractGridColumn.CELL_CONTENT_OFFSET + ScrollManager.GRID_LINE_WIDTH || nPos > ScrollManager.GRID_LINE_WIDTH && i > 0))
				{
					nColIndex = i - 1;
					if (!m_gridHeader[nColIndex].Resizable)
					{
						hitTestResult = EnHitTestResult.HeaderButton;
						nColIndex = i;
						rCellRect.X = nPos + ScrollManager.GRID_LINE_WIDTH;
						rCellRect.Width = num;
						break;
					}

					if (num2 == 0)
					{
						num2 = m_Columns[nColIndex].WidthInPixels;
					}

					rCellRect.X = nPos - num2;
					rCellRect.Width = num2;
					hitTestResult = EnHitTestResult.ColumnResize;
					break;
				}

				if (nMouseX >= nPos + num - AbstractGridColumn.CELL_CONTENT_OFFSET && nMouseX < nPos + num + AbstractGridColumn.CELL_CONTENT_OFFSET)
				{
					nColIndex = i;
					rCellRect.X = nPos + ScrollManager.GRID_LINE_WIDTH;
					rCellRect.Width = num;
					hitTestResult = m_gridHeader[nColIndex].Resizable ? EnHitTestResult.ColumnResize : EnHitTestResult.HeaderButton;
					break;
				}

				if (nMouseX >= nPos && nMouseX < nPos + num + ScrollManager.GRID_LINE_WIDTH)
				{
					nColIndex = i;
					hitTestResult = EnHitTestResult.HeaderButton;
					break;
				}
			}
			else if (nMouseX >= nPos && nMouseX < nPos + num + ScrollManager.GRID_LINE_WIDTH)
			{
				nColIndex = i;
				hitTestResult = EnHitTestResult.TextCell;
				break;
			}

			nPos += num + ScrollManager.GRID_LINE_WIDTH;
		}

		if (hitTestResult == EnHitTestResult.Nothing)
		{
			return hitTestResult;
		}

		if (EnHitTestResult.ColumnResize != hitTestResult)
		{
			rCellRect.X = nPos;
			rCellRect.Width = num + ScrollManager.GRID_LINE_WIDTH;
		}

		if (EnHitTestResult.HeaderButton == hitTestResult || EnHitTestResult.ColumnResize == hitTestResult)
		{
			rCellRect.Y = 0;
			rCellRect.Height = HeaderHeight;
		}

		return hitTestResult;
	}

	private EnHitTestResult HitTestColumns(int nMouseX, int nMouseY, out int nColIndex, ref Rectangle rCellRect)
	{
		int num = CalcNonScrollableColumnsWidth();
		if (HasNonScrollableColumns && nMouseX <= num)
		{
			return HitTestColumnsHelper(nMouseX, nMouseY, 0, m_scrollMgr.FirstScrollableColumnIndex - 1, ScrollManager.GRID_LINE_WIDTH, out nColIndex, ref rCellRect);
		}

		return HitTestColumnsHelper(nMouseX, nMouseY, m_scrollMgr.FirstColumnIndex, m_scrollMgr.LastColumnIndex, m_scrollMgr.FirstColumnPos, out nColIndex, ref rCellRect);
	}

	private EnHitTestResult HitTestRows(int nMouseX, int nMouseY, out long nRowIndex, ref Rectangle rCellRect)
	{
		_ = nMouseX;
		nRowIndex = -1L;
		long firstRowIndex = m_scrollMgr.FirstRowIndex;
		long lastRowIndex = m_scrollMgr.LastRowIndex;
		if (nMouseY < NonScrollableRowsHeight() + HeaderHeight + ScrollManager.GRID_LINE_WIDTH)
		{
			nRowIndex = (nMouseY - HeaderHeight) / (m_scrollMgr.CellHeight + ScrollManager.GRID_LINE_WIDTH);
			if (nRowIndex < 0)
			{
				nRowIndex = 0L;
			}

			rCellRect.Y = (int)nRowIndex * (m_scrollMgr.CellHeight + ScrollManager.GRID_LINE_WIDTH) + HeaderHeight;
			rCellRect.Height = m_scrollMgr.CellHeight + ScrollManager.GRID_LINE_WIDTH;
			return EnHitTestResult.TextCell;
		}

		int num = m_scrollMgr.FirstRowPos;
		int cellHeight = m_scrollMgr.CellHeight;
		long num2;
		for (num2 = firstRowIndex; num2 <= lastRowIndex; num2++)
		{
			if (nMouseY >= num && nMouseY < num + cellHeight + ScrollManager.GRID_LINE_WIDTH)
			{
				nRowIndex = num2;
				rCellRect.Y = num;
				rCellRect.Height = cellHeight + ScrollManager.GRID_LINE_WIDTH;
				break;
			}

			num += cellHeight + ScrollManager.GRID_LINE_WIDTH;
		}

		if (num2 > lastRowIndex)
		{
			return EnHitTestResult.Nothing;
		}

		return EnHitTestResult.TextCell;
	}

	private void SetFocusToEmbeddedControl()
	{
		if (!m_curEmbeddedControl.ContainsFocus)
		{
			m_curEmbeddedControl.Focus();
		}
	}

	private void PaintEmptyHeader(Graphics g)
	{
		Rectangle rectangle = new Rectangle(0, 0, ClientRectangle.Width, HeaderHeight);
		ControlPaint.DrawButton(g, rectangle, ButtonState.Normal);
	}

	protected void PaintEmptyGrid(Graphics g)
	{
		PaintGridBackground(g);
		if (m_withHeader)
		{
			if (NumColInt <= 0)
			{
				PaintEmptyHeader(g);
			}
			else
			{
				PaintHeader(g);
			}
		}
	}

	protected void PaintGrid(Graphics g)
	{
		PaintGridBackground(g);
		if (m_gridStorage == null || NumColInt == 0 || RowCount == 0L)
		{
			return;
		}

		int firstColumnIndex = m_scrollMgr.FirstColumnIndex;
		int lastColumnIndex = m_scrollMgr.LastColumnIndex;
		long firstRowIndex = m_scrollMgr.FirstRowIndex;
		long num = Math.Min(m_scrollMgr.LastRowIndex, m_gridStorage.RowCount - 1);
		int cellHeight = m_scrollMgr.CellHeight;
		Rectangle rCurrentCellRect = Rectangle.Empty;
		Rectangle rCell = default;
		int gRID_LINE_WIDTH = ScrollManager.GRID_LINE_WIDTH;
		int num2 = -1;
		long num3 = -1L;
		Rectangle rEditingCellRect = Rectangle.Empty;
		if (IsEditing)
		{
			IBsGridEmbeddedControl gridEmbeddedControl = (IBsGridEmbeddedControl)m_curEmbeddedControl;
			num2 = GetUIColumnIndexByStorageIndex(gridEmbeddedControl.ColumnIndex);
			num3 = gridEmbeddedControl.RowIndex;
			if (!IsCellVisible(num2, num3) && m_curEmbeddedControl.Visible)
			{
				Focus();
				m_curEmbeddedControl.Visible = false;
			}
		}

		bool hasNonScrollableColumns = HasNonScrollableColumns;
		rCell.Y = HeaderHeight;
		rCell.Height = cellHeight + gRID_LINE_WIDTH;
		int num4;
		for (long num5 = 0L; num5 < m_scrollMgr.FirstScrollableRowIndex; num5++)
		{
			rCell.X = m_scrollMgr.FirstColumnPos;
			for (num4 = firstColumnIndex; num4 <= lastColumnIndex; num4++)
			{
				PaintOneCell(g, num4, num5, num2, num3, ref rCell, ref rCurrentCellRect, ref rEditingCellRect);
			}

			rCell.X = ScrollManager.GRID_LINE_WIDTH;
			for (num4 = 0; num4 < m_scrollMgr.FirstScrollableColumnIndex; num4++)
			{
				PaintOneCell(g, num4, num5, num2, num3, ref rCell, ref rCurrentCellRect, ref rEditingCellRect);
			}

			rCell.Offset(0, cellHeight + gRID_LINE_WIDTH);
		}

		int num6 = ScrollManager.GRID_LINE_WIDTH;
		for (num4 = 0; num4 < m_scrollMgr.FirstScrollableColumnIndex; num4++)
		{
			rCell.Y = m_scrollMgr.FirstRowPos;
			for (long num7 = firstRowIndex; num7 <= num; num7++)
			{
				rCell.X = num6;
				PaintOneCell(g, num4, num7, num2, num3, ref rCell, ref rCurrentCellRect, ref rEditingCellRect);
				rCell.Y += rCell.Height;
			}

			num6 += rCell.Width;
		}

		Region clip = g.Clip;
		g.IntersectClip(m_scrollableArea);
		rCell.Y = m_scrollMgr.FirstRowPos;
		rCell.Height = cellHeight + gRID_LINE_WIDTH;
		for (long num8 = firstRowIndex; num8 <= num; num8++)
		{
			rCell.X = m_scrollMgr.FirstColumnPos;
			for (num4 = firstColumnIndex; num4 <= lastColumnIndex; num4++)
			{
				PaintOneCell(g, num4, num8, num2, num3, ref rCell, ref rCurrentCellRect, ref rEditingCellRect);
			}

			rCell.Offset(0, cellHeight + gRID_LINE_WIDTH);
		}

		g.Clip = clip;
		if (m_withHeader)
		{
			PaintHeader(g);
		}

		if (m_lineType != 0)
		{
			PaintHorizGridLines(g, num - firstRowIndex + FirstScrollableRow + 1, HeaderHeight, 0, rCell.X - 2 * gRID_LINE_WIDTH, bAdjust: true);
			if (m_scrollMgr.FirstScrollableRowIndex != 0)
			{
				PaintVertGridLines(g, firstColumnIndex, lastColumnIndex, m_scrollMgr.FirstColumnPos - ScrollManager.GRID_LINE_WIDTH, HeaderHeight, HeaderHeight + m_scrollMgr.FirstScrollableColumnIndex * (cellHeight + gRID_LINE_WIDTH));
			}

			if (hasNonScrollableColumns)
			{
				PaintVertGridLines(g, 0, m_scrollMgr.FirstScrollableColumnIndex - 1, 0, HeaderHeight, rCell.Y - gRID_LINE_WIDTH);
				g.IntersectClip(new Rectangle(m_scrollableArea.X, HeaderHeight, m_scrollableArea.Width, ClientRectangle.Height - HeaderHeight + 1));
			}

			PaintVertGridLines(g, firstColumnIndex, lastColumnIndex, m_scrollMgr.FirstColumnPos - ScrollManager.GRID_LINE_WIDTH, HeaderHeight, rCell.Y - gRID_LINE_WIDTH);
		}
		else if (hasNonScrollableColumns)
		{
			g.IntersectClip(new Rectangle(m_scrollableArea.X, HeaderHeight, m_scrollableArea.Width, ClientRectangle.Height - HeaderHeight + 1));
		}

		if (!rCurrentCellRect.IsEmpty)
		{
			rCurrentCellRect.Height += gRID_LINE_WIDTH;
			if (m_selMgr.CurrentColumn >= m_scrollMgr.FirstScrollableColumnIndex && (m_alwaysHighlightSelection || ContainsFocus))
			{
				PaintCurrentCellRect(g, rCurrentCellRect);
			}
		}

		if (hasNonScrollableColumns)
		{
			g.Clip = clip;
		}

		if (!rCurrentCellRect.IsEmpty && (m_alwaysHighlightSelection || ContainsFocus) && m_selMgr.CurrentColumn < m_scrollMgr.FirstScrollableColumnIndex)
		{
			PaintCurrentCellRect(g, rCurrentCellRect);
		}

		if (IsEditing && !rEditingCellRect.IsEmpty)
		{
			if (m_Columns[num2].ColumnType >= 1024)
			{
				AdjustEmbeddedEditorBoundsForCustomColumn(ref rEditingCellRect, num2, num3);
			}

			PositionEmbeddedEditor(rEditingCellRect, num2);
		}

		m_scrollMgr.RowCount = m_gridStorage.RowCount;
	}

	private void GetFontInfo(Font font, out int height, out double aveWidth)
	{
		IntPtr hwnd = (IntPtr)0;

		if (IsHandleCreated)
		{
			hwnd = Handle;
		}

		using Graphics graphics = Graphics.FromHwnd(hwnd);
		height = (int)Math.Round(font.GetHeight(graphics));
		aveWidth = TextRenderer.MeasureText(graphics, "The quick brown fox jumped over the lazy dog.", font).Width / 44.549996948242189;
	}

	private int GetColumnWidthInPixels(int nWidth, EnGridColumnWidthType widthType)
	{
		double num = nWidth;
		if (widthType == EnGridColumnWidthType.InAverageFontChar)
		{
			num *= m_cAvCharWidth;
		}

		return (int)num;
	}

	private AbstractGridColumn AllocateColumn(int colType, GridColumnInfo ci, int nWidthInPixels, int colIndex)
	{
		return colType switch
		{
			1 => AllocateTextColumn(ci, nWidthInPixels, colIndex),
			3 => AllocateBitmapColumn(ci, nWidthInPixels, colIndex),
			2 => AllocateButtonColumn(ci, nWidthInPixels, colIndex),
			4 => AllocateCheckBoxColumn(ci, nWidthInPixels, colIndex),
			5 => AllocateHyperlinkColumn(ci, nWidthInPixels, colIndex),
			_ => AllocateCustomColumn(ci, nWidthInPixels, colIndex),
		};
	}

	public bool IsCellVisible(int column, long row)
	{
		if (column < FirstScrollableColumn)
		{
			if (row < FirstScrollableRow)
			{
				return true;
			}

			if (row < m_scrollMgr.FirstRowIndex)
			{
				return false;
			}

			if (row <= m_scrollMgr.LastRowIndex)
			{
				return true;
			}
		}
		else
		{
			if (column < m_scrollMgr.FirstColumnIndex)
			{
				return false;
			}

			if (column <= m_scrollMgr.LastColumnIndex)
			{
				if (row < FirstScrollableRow)
				{
					return true;
				}

				if (row < m_scrollMgr.FirstRowIndex)
				{
					return false;
				}

				if (row <= m_scrollMgr.LastRowIndex)
				{
					return true;
				}
			}
		}

		return false;
	}

	private Color ColorFromWin32(int nWin32ColorIndex)
	{
		int sysColor = (int)Native.GetSysColor(nWin32ColorIndex);
		return Color.FromArgb(Native.Util.RGB_GETRED(sysColor), Native.Util.RGB_GETGREEN(sysColor), Native.Util.RGB_GETBLUE(sysColor));
	}

	private SolidBrush GetSeeThroghBkBrush(Color systemBaseColor)
	{
		Color color = ColorFromWin32(5);
		Color color2 = Color.FromArgb((13 * color.R + systemBaseColor.R * 7) / 20, (13 * color.G + systemBaseColor.G * 7) / 20, (13 * color.B + systemBaseColor.B * 7) / 20);
		if (Math.Abs(color2.R - systemBaseColor.R) * 2 + Math.Abs(color2.G - systemBaseColor.G) * 5 + Math.Abs(color2.B - systemBaseColor.B) < (color == Color.Black ? 73 : 133))
		{
			color2 = (color2.R * 2 + color2.G * 5 + color2.B) / 8 < 128 ? Color.FromArgb(color2.R + 5 * (255 - color2.R) / 20, color2.G + 5 * (255 - color2.G) / 20, color2.B + 5 * (255 - color2.B) / 20) : Color.FromArgb(color2.R * 13 / 20, color2.G * 13 / 20, color2.B * 13 / 20);
		}

		return new SolidBrush(color2);
	}

	private void InitializeCachedGDIObjects()
	{
		m_colInsertionPen = new Pen(Color.Red);
		if (!SystemInformation.HighContrast)
		{
			m_gridLinesPen = new Pen(SystemColors.Control);
			m_highlightBrush = GetSeeThroghBkBrush(ColorFromWin32(13));
			highlightNonFocusedBrush = GetSeeThroghBkBrush(ColorFromWin32(3));
			m_highlightTextBrush = new SolidBrush(SystemColors.ControlText);
			highlightNonFocusedTextBrush = new SolidBrush(SystemColors.ControlText);
		}
		else
		{
			m_highlightBrush = new SolidBrush(ColorFromWin32(13));
			highlightNonFocusedBrush = new SolidBrush(SystemColors.Control);
			m_gridLinesPen = new Pen(SystemColors.WindowFrame);
			m_highlightTextBrush = new SolidBrush(SystemColors.HighlightText);
			highlightNonFocusedTextBrush = new SolidBrush(SystemColors.ControlText);
		}

		if (m_lineType == EnGridLineType.Solid)
		{
			m_gridLinesPen.Width = ScrollManager.GRID_LINE_WIDTH;
			m_gridLinesPen.DashStyle = DashStyle.Solid;
		}
	}

	private void DisposeCachedGDIObjects()
	{
		if (m_gridLinesPen != null)
		{
			m_gridLinesPen.Dispose();
			m_gridLinesPen = null;
		}

		if (m_highlightBrush != null)
		{
			m_highlightBrush.Dispose();
			m_highlightBrush = null;
		}

		if (m_highlightTextBrush != null)
		{
			m_highlightTextBrush.Dispose();
			m_highlightTextBrush = null;
		}

		if (highlightNonFocusedBrush != null)
		{
			highlightNonFocusedBrush.Dispose();
			highlightNonFocusedBrush = null;
		}

		if (highlightNonFocusedTextBrush != null)
		{
			highlightNonFocusedTextBrush.Dispose();
			highlightNonFocusedTextBrush = null;
		}

		if (m_colInsertionPen != null)
		{
			m_colInsertionPen.Dispose();
			m_colInsertionPen = null;
		}
	}

	private void RefreshHeader(int xPosForInsertionMark)
	{
		using Graphics graphics = GraphicsFromHandle();
		PaintHeader(graphics);
		if (xPosForInsertionMark >= 0)
		{
			graphics.DrawLine(m_colInsertionPen, xPosForInsertionMark, 0, xPosForInsertionMark, HeaderHeight - 1);
		}
	}

	protected virtual bool OnMouseButtonClicking(long nRowIndex, int nColIndex, Rectangle rCellRect, Keys modKeys, MouseButtons btn)
	{
		if (MouseButtonClickingEvent != null)
		{
			MouseButtonClickingEventArgs args = new (nRowIndex, m_Columns[nColIndex].ColumnIndex, rCellRect, modKeys, btn);
			MouseButtonClickingEvent(this, args);
			return args.ShouldHandle;
		}

		return true;
	}

	protected virtual bool OnMouseButtonClicked(long nRowIndex, int nColIndex, Rectangle rCellRect, MouseButtons btn)
	{
		if (MouseButtonClickedEvent != null)
		{
			MouseButtonClickedEventArgs args = new(nRowIndex, m_Columns[nColIndex].ColumnIndex, rCellRect, btn);
			MouseButtonClickedEvent(this, args);
			int childID = (int)nRowIndex + (WithHeader ? 1 : 0);
			int objectID = nColIndex + 2 + 1;
			AccessibilityNotifyClients(AccessibleEvents.StateChange, objectID, childID);
			AccessibilityNotifyClients(AccessibleEvents.NameChange, objectID, childID);
			return !args.ShouldRedraw;
		}

		return false;
	}

	protected virtual void OnMouseButtonDoubleClicked(EnHitTestResult htArea, long nRowIndex, int nColIndex, Rectangle rCellRect, MouseButtons btn, EnGridButtonArea headerArea)
	{
		if (MouseButtonDoubleClickedEvent != null)
		{
			int nColIndex2 = nColIndex >= 0 ? m_Columns[nColIndex].ColumnIndex : nColIndex;
			MouseButtonDoubleClickedEventArgs args = new(htArea, nRowIndex, nColIndex2, rCellRect, btn, headerArea);
			MouseButtonDoubleClickedEvent(this, args);
		}
	}

	protected virtual bool OnHeaderButtonClicked(int nColIndex, MouseButtons btn, EnGridButtonArea headerArea)
	{
		if (HeaderButtonClickedEvent != null)
		{
			HeaderButtonClickedEventArgs args = new(m_Columns[nColIndex].ColumnIndex, btn, headerArea);
			HeaderButtonClickedEvent(this, args);

			return args.RepaintWholeGrid;
		}

		return false;
	}

	protected virtual void OnColumnWidthChanged(int nColIndex, int nNewColWidth)
	{
		ColumnWidthChangedEvent?.Invoke(this, new(m_Columns[nColIndex].ColumnIndex, nNewColWidth));
	}

	protected virtual void OnSelectionChanged(BlockOfCellsCollection selectedCells)
	{
		if (!IsEditing)
			NotifyAccAboutNewSelection(notifySelection: true, notifyFocus: true);

		SelectionChangedEvent?.Invoke(this, new(AdjustColumnIndexesInSelectedCells(selectedCells, bFromUIToStorage: true)));
	}

	protected virtual bool OnStandardKeyProcessing(KeyEventArgs ke)
	{
		if (StandardKeyProcessingEvent != null)
		{
			StandardKeyProcessingEventArgs args = new(ke);
			StandardKeyProcessingEvent(this, args);
			return args.ShouldHandle;
		}

		return true;
	}

	protected virtual void OnKeyPressedOnCell(long nCurRow, int nCurCol, Keys key, Keys mod)
	{
		KeyPressedOnCellEvent?.Invoke(this, new(nCurRow, m_Columns[nCurCol].ColumnIndex, key, mod));
	}

	private void OnEmbeddedControlContentsChanged(object sender, EventArgs a)
	{
		OnEmbeddedControlContentsChanged((IBsGridEmbeddedControl)sender);
	}

	protected virtual void OnEmbeddedControlContentsChanged(IBsGridEmbeddedControl embeddedControl)
	{
		EmbeddedControlContentsChangedEvent?.Invoke(this, new EmbeddedControlContentsChangedEventArgs(embeddedControl));
	}

	protected virtual void OnEmbeddedControlLostFocus()
	{
		if (ShouldCommitEmbeddedControlOnLostFocus && !ContainsFocus)
		{
			StopEditCell();
			if (m_selMgr.SelectedBlocks.Count > 0)
			{
				Invalidate();
			}
		}
	}

	private void OnEmbeddedControlLostFocusInternal(object sender, EventArgs a)
	{
		OnEmbeddedControlLostFocus();
	}

	protected virtual void OnMouseWheelInEmbeddedControl(MouseEventArgs e)
	{
		OnMouseWheel(e);
	}

	protected virtual bool OnTooltipDataNeeded(EnHitTestResult ht, long rowNumber, int colNumber, ref string toolTipText)
	{
		if (colNumber >= 0 && TooltipDataNeededEvent != null)
		{
			TooltipDataNeededEventArgs args = new (ht, rowNumber, m_Columns[colNumber].ColumnIndex);
			TooltipDataNeededEvent(this, args);
			if (string.IsNullOrEmpty(args.TooltipText))
				return false;

			toolTipText = args.TooltipText;
			return true;
		}

		return false;
	}

	protected virtual bool OnCanInitiateDragFromCell(long rowIndex, int colIndex)
	{
		if (m_Columns[colIndex].ColumnType != 3)
		{
			return m_Columns[colIndex].ColumnType != 4;
		}

		return false;
	}

	protected virtual void OnStartCellDragOperation()
	{
		string clipboardTextForSelectionBlock = GetClipboardTextForSelectionBlock(m_captureTracker.SelectionBlockIndex);
		DataObject dataObject = new DataObject();
		dataObject.SetData(DataFormats.UnicodeText, autoConvert: true, clipboardTextForSelectionBlock);
		DoDragDrop(dataObject, DragDropEffects.Copy);
	}

	protected virtual bool IsColumnHeaderDraggable(int colIndex)
	{
		if (ColumnReorderRequestedEvent == null)
		{
			return m_bColumnsReorderableByDefault;
		}

		ColumnReorderRequestedEventArgs columnReorderRequestedEventArgs = new ColumnReorderRequestedEventArgs(m_Columns[colIndex].ColumnIndex, m_bColumnsReorderableByDefault);
		ColumnReorderRequestedEvent(this, columnReorderRequestedEventArgs);
		return columnReorderRequestedEventArgs.AllowReorder;
	}

	protected virtual void OnColumnsReordered(int oldIndex, int newIndex)
	{
		ColumnsReorderedEvent?.Invoke(this, new(oldIndex, newIndex));
	}

	protected virtual void OnColumnWasReordered(int nOldIndex, int nNewIndex)
	{
		if (IsEditing)
		{
			CancelEditCell();
		}

		int num = GetFirstNonMergedToTheLeft(nOldIndex);
		gridColumnMapper ??= new GridColumnMapper<int>(m_Columns.Count, (id) => id);

		gridColumnMapper.ShiftColumnIndexes(num, nNewIndex);
		bool flag = false;
		if (num > nNewIndex)
		{
			int num2 = GetFirstNonMergedToTheLeft(nNewIndex);
			flag = true;
			while (num <= nOldIndex)
			{
				m_Columns.Move(num, num2);
				m_gridHeader.Move(num, num2);
				num++;
				num2++;
			}
		}
		else
		{
			for (int num3 = nOldIndex - num + 1; num3 > 0; num3--)
			{
				m_Columns.Move(num, nNewIndex);
				m_gridHeader.Move(num, nNewIndex);
			}
		}

		if (flag)
		{
			m_scrollMgr.RecalcAll(m_scrollableArea);
		}

		Refresh();
		OnColumnsReordered(nOldIndex, nNewIndex);
	}

	public int GetOriginalColumnIndex(int index)
	{
		if (gridColumnMapper != null)
		{
			return gridColumnMapper[index];
		}

		return index;
	}

	protected virtual BlockOfCellsCollection AdjustColumnIndexesInSelectedCells(BlockOfCellsCollection originalCol, bool bFromUIToStorage)
	{
		if (m_selMgr.SelectionType == EnGridSelectionType.CellBlocks || m_selMgr.SelectionType == EnGridSelectionType.ColumnBlocks || m_selMgr.SelectionType == EnGridSelectionType.RowBlocks || m_selMgr.SelectionType == EnGridSelectionType.SingleRow)
		{
			return originalCol;
		}

		if (originalCol == null || originalCol.Count == 0)
		{
			return originalCol;
		}

		BlockOfCellsCollection blockOfCellsCollection = [];
		foreach (BlockOfCells item in originalCol)
		{
			BlockOfCells blockOfCells = !bFromUIToStorage ? new BlockOfCells(item.Y, GetUIColumnIndexByStorageIndex(item.X)) : new BlockOfCells(item.Y, m_Columns[item.X].ColumnIndex);
			blockOfCellsCollection.Add(blockOfCells);
		}

		return blockOfCellsCollection;
	}

	protected bool HandleStdCellLBtnDown(Keys modKeys)
	{
		long rowIndex = m_captureTracker.RowIndex;
		int columnIndex = m_captureTracker.ColumnIndex;
		if (!OnMouseButtonClicking(rowIndex, columnIndex, m_captureTracker.CellRect, modKeys, MouseButtons.Left))
		{
			return false;
		}

		int num = m_selMgr.GetSelecttionBlockNumberForCell(rowIndex, columnIndex);
		long num2 = -1L;
		int num3 = -1;
		if (IsEditing)
		{
			num2 = m_selMgr.CurrentRow;
			num3 = m_selMgr.CurrentColumn;
			if (!StopEditCell())
			{
				return false;
			}
		}

		if (rowIndex >= m_gridStorage.RowCount)
		{
			return false;
		}

		bool flag = ProcessSelAndEditingForLeftClickOnCell(modKeys, rowIndex, columnIndex, out int editType, out bool bShouldStartEditing, out bool bDragCancelled);
		bool flag2 = false;
		if (bShouldStartEditing)
		{
			flag2 = !HandleStartCellEditFromStdCellLBtnDown(editType, rowIndex, columnIndex, flag);
			if (!flag2)
			{
				return false;
			}
		}

		if (flag2)
		{
			if (num2 >= 0 && num3 >= 0)
			{
				m_selMgr.Clear();
				m_selMgr.StartNewBlock(num2, num3);
			}

			return false;
		}

		bool flag3 = true;
		if ((modKeys & Keys.Shift) != 0)
		{
			m_selMgr.UpdateCurrentBlock(rowIndex, columnIndex);
		}
		else if ((modKeys & Keys.Control) != 0)
		{
			if (m_selMgr.IsCellSelected(rowIndex, columnIndex) && m_selMgr.SingleRowOrColumnSelectedInMultiSelectionMode)
			{
				flag3 = false;
			}
			else
			{
				flag3 = m_selMgr.StartNewBlockOrExcludeCell(rowIndex, columnIndex);
				if (!flag3)
				{
					flag = true;
				}
			}
		}
		else
		{
			flag3 = true;
			if ((m_selMgr.OnlyOneSelItem && num == -1 || !m_selMgr.OnlyOneSelItem && columnIndex < m_scrollMgr.FirstScrollableColumnIndex || !m_selMgr.OnlyOneSelItem && rowIndex < m_scrollMgr.FirstScrollableRowIndex) && !bDragCancelled)
			{
				flag = true;
				if (num == -1)
				{
					num = 0;
					m_selMgr.StartNewBlock(rowIndex, columnIndex);
				}
			}

			if (num != -1 && !bDragCancelled)
			{
				m_captureTracker.SelectionBlockIndex = num;
				m_captureTracker.DragState = CaptureTracker.EnDragOperation.DragReady;
			}
			else
			{
				m_selMgr.StartNewBlock(rowIndex, columnIndex);
				if (bDragCancelled)
				{
					m_captureTracker.DragState = CaptureTracker.EnDragOperation.None;
					if (m_selMgr.OnlyOneSelItem)
					{
						flag3 = false;
					}
				}
			}
		}

		if (!flag3)
		{
			if (!OnMouseButtonClicked(rowIndex, columnIndex, m_captureTracker.CellRect, MouseButtons.Left))
			{
				Refresh();
			}
		}
		else
		{
			m_captureTracker.LastColumnIndex = columnIndex;
			m_captureTracker.LastRowIndex = rowIndex;
			Refresh();
		}

		if (flag)
		{
			OnSelectionChanged(m_selMgr.SelectedBlocks);
		}

		return flag3;
	}

	private void TransitionHyperlinkToStd(object sender, EventArgs e)
	{
		if (sender is Timer timer)
		{
			timer.Stop();

			Cursor = Cursors.Arrow;

			HandleStdCellLBtnDown(Keys.None);
			m_captureTracker.CaptureHitTest = EnHitTestResult.TextCell;
		}
	}

	private void ProcessLeftButtonUp(int x, int y)
	{
		switch (m_captureTracker.CaptureHitTest)
		{
			case EnHitTestResult.HeaderButton:
			case EnHitTestResult.ButtonCell:
				if (m_captureTracker.CaptureHitTest == EnHitTestResult.ButtonCell && m_Columns[m_captureTracker.ColumnIndex] is GridButtonColumn column)
				{
					column.SetForcedButtonState(-1L, ButtonState.Pushed);
				}

				HandleButtonLBtnUp(x, y);
				break;
			case EnHitTestResult.TextCell:
			case EnHitTestResult.BitmapCell:
				HandleStdCellLBtnUp(x, y);
				break;
			case EnHitTestResult.HyperlinkCell:
				HandleHyperlinkLBtnUp(x, y);
				break;
			case EnHitTestResult.ColumnResize:
				HandleColResizeLBtnUp(x, y);
				break;
			case EnHitTestResult.CustomCell:
				HandleCustomCellMouseBtnUp(x, y, MouseButtons.Left);
				break;
		}
	}

	private bool HandleHyperlinkLBtnDown()
	{
		using Graphics g = GraphicsFromHandle();
		return m_Columns[m_captureTracker.ColumnIndex].IsPointOverTextInCell(m_captureTracker.MouseCapturePoint, m_captureTracker.CellRect, m_gridStorage, m_captureTracker.RowIndex, g, m_linkFont);
	}

	private void HandleHyperlinkLBtnUp(int currentX, int currentY)
	{
		DateTime now = DateTime.Now;
		m_captureTracker.HyperLinkSelectionTimer.Stop();
		Point pt = new Point(currentX, currentY);
		bool flag = false;
		using (Graphics g = GraphicsFromHandle())
		{
			flag = m_Columns[m_captureTracker.ColumnIndex].IsPointOverTextInCell(pt, m_captureTracker.CellRect, m_gridStorage, m_captureTracker.RowIndex, g, m_linkFont);
		}

		if (flag)
		{
			if ((now - m_captureTracker.Time).TotalSeconds < C_HyperlinkSelectionDelay)
			{
				OnGridSpecialEvent(0, null, m_captureTracker.CaptureHitTest, m_captureTracker.RowIndex, m_captureTracker.ColumnIndex, m_captureTracker.CellRect, MouseButtons.None, m_captureTracker.ButtonArea);
				m_selMgr.Clear();
				m_selMgr.StartNewBlock(m_captureTracker.RowIndex, m_captureTracker.ColumnIndex);
				OnSelectionChanged(m_selMgr.SelectedBlocks);
				Invalidate();
			}
			else
			{
				HandleStdCellLBtnUp(currentX, currentY);
			}
		}
		else
		{
			HandleStdCellLBtnUp(currentX, currentY);
		}
	}

	protected void OnGridSpecialEvent(int eventType, object data, EnHitTestResult htResult, long rowIndex, int colIndex, Rectangle cellRect, MouseButtons mouseState, EnGridButtonArea headerArea)
	{
		GridSpecialEvent?.Invoke(this, new(eventType, data, htResult, rowIndex, colIndex, cellRect, mouseState, headerArea));
	}

	protected void HandleStdCellLBtnUp(int nCurrentMouseX, int nCurrentMouseY)
	{
		if (m_autoScrollTimer.Enabled)
		{
			m_autoScrollTimer.Stop();
		}

		if (m_captureTracker.DragState == CaptureTracker.EnDragOperation.StartedDrag)
		{
			return;
		}

		HitTestInfo hitTestInfo = HitTestInternal(nCurrentMouseX, nCurrentMouseY);
		if (hitTestInfo.RowIndex == m_captureTracker.RowIndex && hitTestInfo.ColumnIndex == m_captureTracker.ColumnIndex)
		{
			bool flag = false;
			if (m_captureTracker.DragState == CaptureTracker.EnDragOperation.DragReady)
			{
				int num = m_gridStorage.IsCellEditable(hitTestInfo.RowIndex, m_Columns[hitTestInfo.ColumnIndex].ColumnIndex);
				int columnType = m_Columns[hitTestInfo.ColumnIndex].ColumnType;
				if (columnType == 2 || columnType == 4)
				{
					num = 0;
				}

				if (m_selMgr.SelectionType != EnGridSelectionType.SingleCell || num != 0)
				{
					m_selMgr.Clear();
					if (num != 0)
					{
						HandleStartCellEditFromStdCellLBtnDown(num, hitTestInfo.RowIndex, hitTestInfo.ColumnIndex, bNotifySelChange: false);
					}
					else
					{
						m_selMgr.StartNewBlock(hitTestInfo.RowIndex, hitTestInfo.ColumnIndex);
					}

					flag = true;
				}
			}

			if (!OnMouseButtonClicked(hitTestInfo.RowIndex, hitTestInfo.ColumnIndex, m_captureTracker.CellRect, MouseButtons.Left) || flag)
			{
				Refresh();
			}
		}

		if (!m_selMgr.OnlyOneSelItem)
		{
			OnSelectionChanged(m_selMgr.SelectedBlocks);
		}
	}

	protected void HandleStdCellLBtnMouseMove(int nCurrentMouseX, int nCurrentMouseY)
	{
		if (m_captureTracker.DragState == CaptureTracker.EnDragOperation.DragReady)
		{
			int num = SystemInformation.DragSize.Width / 2;
			int num2 = SystemInformation.DragSize.Height / 2;
			if (Math.Abs(nCurrentMouseX - m_captureTracker.MouseCapturePoint.X) > num || Math.Abs(nCurrentMouseY - m_captureTracker.MouseCapturePoint.Y) > num2)
			{
				m_captureTracker.DragState = CaptureTracker.EnDragOperation.StartedDrag;
				try
				{
					OnStartCellDragOperation();
				}
				catch (Exception ex)
				{
					Trace.TraceError(ex.Message);
					HandleStdCellLBtnUp(nCurrentMouseX, nCurrentMouseY);
				}

				return;
			}
		}

		if (m_captureTracker.ColumnIndex < m_scrollMgr.FirstScrollableColumnIndex || m_selMgr.OnlyOneSelItem || m_captureTracker.DragState == CaptureTracker.EnDragOperation.DragReady)
		{
			return;
		}

		if (!m_scrollableArea.Contains(nCurrentMouseX, nCurrentMouseY))
		{
			if (!m_autoScrollTimer.Enabled)
			{
				m_autoScrollTimer.Start();
			}

			return;
		}

		HitTestInfo hitTestInfo = HitTestInternal(nCurrentMouseX, nCurrentMouseY);
		if ((hitTestInfo.HitTestResult == EnHitTestResult.ButtonCell || hitTestInfo.HitTestResult == EnHitTestResult.CustomCell || hitTestInfo.HitTestResult == EnHitTestResult.BitmapCell || hitTestInfo.HitTestResult == EnHitTestResult.TextCell || hitTestInfo.HitTestResult == EnHitTestResult.HyperlinkCell) && !InHyperlinkTimer)
		{
			UpdateSelectionBlockFromMouse(hitTestInfo.RowIndex, hitTestInfo.ColumnIndex);
		}
	}

	protected bool HandleButtonLBtnDown()
	{
		if (m_captureTracker.RowIndex >= 0 && !OnMouseButtonClicking(m_captureTracker.RowIndex, m_captureTracker.ColumnIndex, m_captureTracker.CellRect, ModifierKeys, MouseButtons.Left))
		{
			return false;
		}

		m_captureTracker.AdjustedCellRect = m_captureTracker.CellRect;
		int num = CalcNonScrollableColumnsWidth();
		if (num > 0 && m_captureTracker.ColumnIndex >= m_scrollMgr.FirstScrollableColumnIndex && m_captureTracker.CellRect.X <= num)
		{
			int right = m_captureTracker.AdjustedCellRect.Right;
			int num2 = num + ScrollManager.GRID_LINE_WIDTH;
			m_captureTracker.UpdateAdjustedRectHorizontally(num2, right - num2);
		}

		m_captureTracker.WasOverButton = true;
		if (m_captureTracker.RowIndex == -1)
		{
			m_captureTracker.DragState = CaptureTracker.EnDragOperation.DragReady;
			bool clickable = m_gridHeader[m_captureTracker.ColumnIndex].Clickable;
			if (clickable)
			{
				m_gridHeader[m_captureTracker.ColumnIndex].Pushed = true;
			}

			m_captureTracker.ButtonArea = HitTestGridButton(-1L, m_captureTracker.ColumnIndex, m_captureTracker.CellRect, m_captureTracker.MouseCapturePoint);
			if (clickable)
			{
				RefreshHeader(-1);
			}
		}
		else
		{
			EnButtonCellState buttonCellState = GetButtonCellState(m_captureTracker.RowIndex, m_captureTracker.ColumnIndex);
			if (buttonCellState == EnButtonCellState.Empty || buttonCellState == EnButtonCellState.Disabled)
			{
				return false;
			}

			m_captureTracker.ButtonWasPushed = buttonCellState == EnButtonCellState.Pushed;
			DrawOneButtonCell(m_captureTracker.RowIndex, m_captureTracker.ColumnIndex, bPushed: true);
		}

		return true;
	}

	protected bool HandleHeaderButtonMouseMove(int X, int Y)
	{
		int num = SystemInformation.DragSize.Width / 2;
		int num2 = SystemInformation.DragSize.Height / 2;
		if (Math.Abs(X - m_captureTracker.MouseCapturePoint.X) > num || Math.Abs(Y - m_captureTracker.MouseCapturePoint.Y) > num2)
		{
			bool flag = m_captureTracker.ColumnIndex >= m_scrollMgr.FirstScrollableColumnIndex;
			if (flag)
			{
				flag = IsColumnHeaderDraggable(m_captureTracker.ColumnIndex);
			}

			if (!flag)
			{
				m_captureTracker.DragState = CaptureTracker.EnDragOperation.None;
				return false;
			}

			Size size = new Size(0, 0);
			if (BorderStyle == BorderStyle.Fixed3D)
			{
				size = SystemInformation.Border3DSize;
			}
			else if (BorderStyle == BorderStyle.FixedSingle)
			{
				size = SystemInformation.BorderSize;
			}

			using (Bitmap bitmap = new Bitmap(m_captureTracker.CellRect.Width, m_captureTracker.CellRect.Height, Graphics.FromHwnd(Handle)))
			{
				using Graphics g = Graphics.FromImage(bitmap);
				int num3 = m_captureTracker.ColumnIndex;
				while (num3 > 0 && m_gridHeader[num3 - 1].MergedWithRight)
				{
					num3--;
				}

				PaintHeaderHelper(g, num3, m_captureTracker.ColumnIndex, 0, 0);
				GridDragImageList gridDragImageList = new GridDragImageList(bitmap.Width, bitmap.Height);
				gridDragImageList.Add(bitmap, SystemColors.Control);
				m_captureTracker.HeaderDragY = m_captureTracker.MouseCapturePoint.Y - m_captureTracker.CellRect.Top;
				m_captureTracker.DragImageOperation = new GridDragImageListOperation(gridDragImageList, new Point(m_captureTracker.MouseCapturePoint.X - m_captureTracker.CellRect.Left - size.Width, m_captureTracker.HeaderDragY - size.Height), Handle, m_captureTracker.MouseCapturePoint, bOwnDIL: true);
			}

			m_captureTracker.DragState = CaptureTracker.EnDragOperation.StartedDrag;
			return true;
		}

		return false;
	}

	protected void HandleButtonMouseMove(int X, int Y)
	{
		if (m_captureTracker.DragImageOperation == null)
		{
			if (m_captureTracker.RowIndex == -1 && m_captureTracker.DragState == CaptureTracker.EnDragOperation.DragReady && HandleHeaderButtonMouseMove(X, Y))
			{
				return;
			}

			if (m_captureTracker.AdjustedCellRect.Contains(X, Y))
			{
				if (m_captureTracker.WasOverButton)
				{
					return;
				}

				if (m_captureTracker.RowIndex == -1)
				{
					if (m_gridHeader[m_captureTracker.ColumnIndex].Clickable)
					{
						m_gridHeader[m_captureTracker.ColumnIndex].Pushed = true;
						RefreshHeader(-1);
					}
				}
				else
				{
					DrawOneButtonCell(m_captureTracker.RowIndex, m_captureTracker.ColumnIndex, bPushed: true);
				}

				m_captureTracker.WasOverButton = true;
			}
			else
			{
				if (!m_captureTracker.WasOverButton)
				{
					return;
				}

				if (m_captureTracker.RowIndex == -1)
				{
					if (m_gridHeader[m_captureTracker.ColumnIndex].Clickable)
					{
						m_gridHeader[m_captureTracker.ColumnIndex].Pushed = false;
						RefreshHeader(-1);
					}
				}
				else
				{
					DrawOneButtonCell(m_captureTracker.RowIndex, m_captureTracker.ColumnIndex, m_captureTracker.ButtonWasPushed);
				}

				m_captureTracker.WasOverButton = false;
			}
		}
		else
		{
			HandleButtonMouseMoveWhileDraggingHeader(X, Y);
		}
	}

	protected void HandleButtonMouseMoveWhileDraggingHeader(int X, int Y)
	{
		_ = Y;
		bool flag = false;
		int num = -1;
		if (X < m_scrollableArea.X || X >= m_scrollableArea.Right)
		{
			if (m_captureTracker.ColIndexToDragColAfter != CaptureTracker.NoColIndexToDragColAfter)
			{
				m_captureTracker.ColIndexToDragColAfter = CaptureTracker.NoColIndexToDragColAfter;
				flag = true;
			}
		}
		else
		{
			HitTestInfo hitTestInfo = HitTestInternal(X, 2);
			if (hitTestInfo.HitTestResult != EnHitTestResult.ColumnResize && hitTestInfo.HitTestResult != EnHitTestResult.HeaderButton)
			{
				if (m_captureTracker.ColIndexToDragColAfter != CaptureTracker.NoColIndexToDragColAfter)
				{
					m_captureTracker.ColIndexToDragColAfter = CaptureTracker.NoColIndexToDragColAfter;
					flag = true;
				}
			}
			else
			{
				int num2 = hitTestInfo.ColumnIndex;
				if (X < (hitTestInfo.AreaRectangle.Left + hitTestInfo.AreaRectangle.Right) / 2)
				{
					num2--;
					num = hitTestInfo.AreaRectangle.Left;
				}
				else
				{
					num = hitTestInfo.AreaRectangle.Right;
				}

				if (HasNonScrollableColumns && num < m_scrollableArea.X)
				{
					num = -1;
				}

				if (num2 != m_captureTracker.ColIndexToDragColAfter)
				{
					if (num2 == NumColInt - 1 && num != -1)
					{
						num -= 2;
					}

					m_captureTracker.ColIndexToDragColAfter = num2;
					flag = true;
				}
			}
		}

		if (flag)
		{
			GridDragImageList.DragShowNolock(bShow: false);
			RefreshHeader(num);
			GridDragImageList.DragShowNolock(bShow: true);
		}

		GridDragImageList.DragMove(new Point(X, m_captureTracker.HeaderDragY));
	}

	protected void HandleButtonLBtnUp(int X, int Y)
	{
		bool clickable = m_gridHeader[m_captureTracker.ColumnIndex].Clickable;
		if (m_captureTracker.RowIndex == -1)
		{
			if (clickable)
			{
				m_gridHeader[m_captureTracker.ColumnIndex].Pushed = false;
			}

			if (m_captureTracker.DragImageOperation != null)
			{
				m_captureTracker.DragImageOperation = null;
				if (m_captureTracker.ColIndexToDragColAfter != CaptureTracker.NoColIndexToDragColAfter && m_captureTracker.ColIndexToDragColAfter != m_captureTracker.ColumnIndex && m_captureTracker.ColIndexToDragColAfter != m_captureTracker.ColumnIndex - 1)
				{
					int num = m_captureTracker.ColIndexToDragColAfter;
					if (num < m_captureTracker.ColumnIndex)
					{
						num++;
					}

					OnColumnWasReordered(m_captureTracker.ColumnIndex, num);
				}
				else
				{
					RefreshHeader(-1);
				}

				return;
			}
		}

		if (m_captureTracker.AdjustedCellRect.Contains(X, Y))
		{
			if (m_captureTracker.RowIndex == -1)
			{
				if (clickable)
				{
					EnGridButtonArea gridButtonArea = HitTestGridButton(-1L, m_captureTracker.ColumnIndex, m_captureTracker.CellRect, new Point(X, Y));
					if (gridButtonArea != m_captureTracker.ButtonArea)
					{
						gridButtonArea = EnGridButtonArea.Nothing;
					}

					if (OnHeaderButtonClicked(m_captureTracker.ColumnIndex, MouseButtons.Left, gridButtonArea))
					{
						Refresh();
					}
					else
					{
						RefreshHeader(-1);
					}
				}
			}
			else
			{
				bool flag = false;
				if ((m_selMgr.SelectionType == EnGridSelectionType.SingleCell || m_selMgr.SelectionType == EnGridSelectionType.CellBlocks || m_selMgr.SelectionType == EnGridSelectionType.SingleRow) && !IsEditing)
				{
					flag = AdjustSelectionForButtonCellMouseClick();
				}

				OnMouseButtonClicked(m_captureTracker.RowIndex, m_captureTracker.ColumnIndex, m_captureTracker.AdjustedCellRect, MouseButtons.Left);
				if (flag)
				{
					Refresh();
					return;
				}
			}
		}

		if (m_captureTracker.RowIndex != -1)
		{
			EnButtonCellState buttonCellState = GetButtonCellState(m_captureTracker.RowIndex, m_captureTracker.ColumnIndex);
			DrawOneButtonCell(m_captureTracker.RowIndex, m_captureTracker.ColumnIndex, buttonCellState == EnButtonCellState.Pushed);
		}
		else if (clickable)
		{
			RefreshHeader(-1);
		}
	}

	protected void HandleColResizeLBtnDown()
	{
		int columnIndex = m_captureTracker.ColumnIndex;
		m_captureTracker.LastColumnWidth = m_Columns[columnIndex].WidthInPixels;
		m_captureTracker.OrigColumnWidth = m_captureTracker.CellRect.Width;
		m_captureTracker.MouseOffsetForColResize = m_captureTracker.MouseCapturePoint.X - m_captureTracker.CellRect.Left - m_captureTracker.OrigColumnWidth;
		m_captureTracker.MinWidthDuringColResize = GetMinWidthOfColumn(columnIndex);
		m_gridHeader[columnIndex].MergedHeaderResizeProportion = 1f;
		m_Columns[columnIndex].OrigWidthInPixelsDuringResize = m_Columns[columnIndex].WidthInPixels;
		m_captureTracker.LeftMostMergedColumnIndex = columnIndex;
		int num = columnIndex - 1;
		while (num >= 0 && m_gridHeader[num].MergedWithRight)
		{
			m_Columns[num].OrigWidthInPixelsDuringResize = m_Columns[num].WidthInPixels;
			m_captureTracker.LeftMostMergedColumnIndex = num;
			m_captureTracker.MinWidthDuringColResize += GetMinWidthOfColumn(num) + ScrollManager.GRID_LINE_WIDTH;
			num--;
		}

		m_captureTracker.TotalGridLineAdjDuringResize = ScrollManager.GRID_LINE_WIDTH * (columnIndex - m_captureTracker.LeftMostMergedColumnIndex);
	}

	protected void HandleColResizeMouseMove(int X, int Y, bool bLastUpdate)
	{
		_ = Y;
		int num = CalcValidColWidth(X);
		int sizeDelta = num - m_captureTracker.OrigColumnWidth;
		m_captureTracker.LastColumnWidth = num;
		ResizeMultipleColumns(m_captureTracker.LeftMostMergedColumnIndex, m_captureTracker.ColumnIndex, sizeDelta, bLastUpdate, null);
		bool flag = false;
		if (m_captureTracker.ColumnIndex < m_scrollMgr.FirstScrollableColumnIndex)
		{
			flag = ProcessNonScrollableVerticalAreaChange(recalcGridIfNeeded: false);
		}

		if (flag)
		{
			m_scrollMgr.RecalcAll(m_scrollableArea);
		}
		else
		{
			Refresh();
		}
	}

	protected void HandleColResizeLBtnUp(int X, int Y)
	{
		HandleColResizeMouseMove(X, Y, bLastUpdate: true);
		int num = 0;
		for (int i = m_captureTracker.LeftMostMergedColumnIndex; i <= m_captureTracker.ColumnIndex; i++)
		{
			num += m_Columns[i].WidthInPixels;
		}

		num += ScrollManager.GRID_LINE_WIDTH * (m_captureTracker.ColumnIndex - m_captureTracker.LeftMostMergedColumnIndex);
		if (num != m_captureTracker.OrigColumnWidth)
		{
			OnColumnWidthChanged(m_captureTracker.ColumnIndex, num);
		}

		if (m_captureTracker.WasEmbeddedControlFocused)
		{
			SetFocusToEmbeddedControl();
		}
	}

	protected void SendMouseClickToEmbeddedControl()
	{
		if (m_curEmbeddedControl != null)
		{
			Point mousePosition = MousePosition;
			mousePosition = m_curEmbeddedControl.PointToClient(mousePosition);
			if (m_curEmbeddedControl.ClientRectangle.Contains(mousePosition))
			{
				Native.SendMessage(m_curEmbeddedControl.Handle, 513, (IntPtr)0, Native.Util.MAKELPARAM(mousePosition.X, 2));
				Native.SendMessage(m_curEmbeddedControl.Handle, 514, (IntPtr)0, Native.Util.MAKELPARAM(mousePosition.X, 2));
			}
		}
	}

	protected Control GetRegisteredEmbeddedControl(int editableCellType)
	{
		return (Control)m_EmbeddedControls[editableCellType];
	}

	private void DrawOneButtonCell(long nRowIndex, int nColumnIndex, bool bPushed)
	{
		GridButtonColumn gridButtonColumn = (GridButtonColumn)m_Columns[nColumnIndex];
		m_gridStorage.GetCellDataForButton(nRowIndex, m_Columns[nColumnIndex].ColumnIndex, out var _, out Bitmap image, out string buttonLabel);
		SolidBrush bkBrush = null;
		SolidBrush textBrush = null;
		GetCellGDIObjects(m_Columns[nColumnIndex], nRowIndex, nColumnIndex, ref bkBrush, ref textBrush);
		Graphics graphics = GraphicsFromHandle();
		try
		{
			if (HasNonScrollableColumns && nColumnIndex >= m_scrollMgr.FirstScrollableColumnIndex)
			{
				graphics.SetClip(new Rectangle(m_scrollableArea.X, HeaderHeight, m_scrollableArea.Width, ClientRectangle.Height - HeaderHeight + 1));
			}

			gridButtonColumn.DrawButton(graphics, bkBrush, textBrush, GetCellFont(nRowIndex, m_Columns[nColumnIndex]), m_captureTracker.CellRect, image, buttonLabel, bPushed ? ButtonState.Pushed : ButtonState.Normal, ActAsEnabled);
		}
		finally
		{
			graphics.Dispose();
		}
	}

	private void ValidateFirstScrollableColumn()
	{
		int firstScrollableColumnIndex = m_scrollMgr.FirstScrollableColumnIndex;
		if (m_Columns.Count > 0)
		{
			if (firstScrollableColumnIndex < 0 || firstScrollableColumnIndex >= m_Columns.Count)
			{
				ArgumentException ex = new(ControlsResources.ExFirstScrollableColumnShouldBeValid, "FirstScrollalbeColumn");
				Diag.Dug(ex);
				throw ex;
			}

			if (firstScrollableColumnIndex > 0 && m_gridHeader[firstScrollableColumnIndex - 1].MergedWithRight)
			{
				ArgumentException ex = new(ControlsResources.ExLastNonScrollCannotBeMerged, "FirstScrollalbeColumn");
				Diag.Dug(ex);
				throw ex;
			}
		}
	}

	private void ValidateFirstScrollableRow()
	{
		if (m_scrollMgr.FirstScrollableRowIndex < 0)
		{
			ArgumentException ex = new(ControlsResources.ExFirstScrollableRowShouldBeValid, "FirstScrollalbeRow");
			Diag.Dug(ex);
			throw ex;
		}
	}

	private int CalcNonScrollableColumnsWidth()
	{
		return CalcNonScrollableColumnsWidth(m_scrollMgr.FirstScrollableColumnIndex - 1);
	}

	private int CalcNonScrollableColumnsWidth(int lastIndex)
	{
		int num = 0;
		if (lastIndex >= 0 && lastIndex < m_Columns.Count)
		{
			num = ScrollManager.GRID_LINE_WIDTH;
			for (int i = 0; i <= lastIndex; i++)
			{
				num += m_Columns[i].WidthInPixels + ScrollManager.GRID_LINE_WIDTH;
			}
		}

		return num;
	}

	private uint NonScrollableRowsHeight()
	{
		if (m_scrollMgr.FirstScrollableRowIndex == 0)
		{
			return 0u;
		}

		return (uint)((m_scrollMgr.CellHeight + ScrollManager.GRID_LINE_WIDTH) * m_scrollMgr.FirstScrollableRowIndex);
	}

	protected void PaintHeaderHelper(Graphics g, int nFirstCol, int nLastCol, int nFirstColPos, int nY)
	{
		PaintHeaderHelper(g, nFirstCol, nLastCol, nFirstColPos, nY, useGdiPlus: false);
	}

	protected void PaintHeaderHelper(Graphics g, int nFirstCol, int nLastCol, int nFirstColPos, int nY, bool useGdiPlus)
	{
		GridButton headerGridButton = m_gridHeader.HeaderGridButton;
		if (nFirstCol > 0 && m_gridHeader[nFirstCol - 1].MergedWithRight)
		{
			int num = nFirstCol - 1;
			while (num >= 0 && m_gridHeader[num].MergedWithRight)
			{
				nFirstColPos -= m_Columns[num].WidthInPixels + ScrollManager.GRID_LINE_WIDTH;
				nFirstCol--;
				num--;
			}
		}

		if (m_gridHeader[nLastCol].MergedWithRight)
		{
			nLastCol++;
			while (nLastCol < NumColInt && m_gridHeader[nLastCol].MergedWithRight)
			{
				nLastCol++;
			}

			nLastCol = Math.Min(nLastCol, NumColInt - 1);
		}

		Rectangle rectangle = new Rectangle(nFirstColPos, nY, 0, HeaderHeight);
		int num2 = NumColInt - 1;
		for (int i = nFirstCol; i <= nLastCol; i++)
		{
			rectangle.Width += m_Columns[i].WidthInPixels;
			if (!m_gridHeader[i].MergedWithRight || i == num2)
			{
				GridHeader.HeaderItem headerItem = m_gridHeader[i];
				if (g.IsVisible(rectangle))
				{
					ButtonState buttonState = headerItem.Pushed ? ButtonState.Pushed : ButtonState.Normal;
					if (i == m_scrollMgr.FirstScrollableColumnIndex - 1 || i == NumColInt - 1)
					{
						rectangle.Width++;
					}

					if (m_scrollMgr.FirstScrollableColumnIndex > 0 && i == nFirstCol && i == m_scrollMgr.FirstScrollableColumnIndex)
					{
						rectangle.X--;
						rectangle.Width++;
					}

					if (headerItem.Type == EnGridColumnHeaderType.CheckBox || headerItem.Type == EnGridColumnHeaderType.TextAndCheckBox)
					{
						new GridCheckBox().Paint(g, rectangle, buttonState, headerItem.Text, headerItem.CheckboxState, headerItem.Align, headerItem.TextBmpLayout, ActAsEnabled, useGdiPlus, isHeader: true);
					}
					else
					{
						headerGridButton.Paint(g, rectangle, buttonState, headerItem.Text, headerItem.Bmp, headerItem.Align, headerItem.TextBmpLayout, ActAsEnabled, useGdiPlus, EnGridButtonType.Header);
					}
				}

				rectangle.X = rectangle.Right + ScrollManager.GRID_LINE_WIDTH;
				rectangle.Width = 0;
			}
			else
			{
				rectangle.Width += ScrollManager.GRID_LINE_WIDTH;
			}
		}
	}

	protected virtual void DoCellPainting(Graphics g, SolidBrush bkBrush, SolidBrush textBrush, Font textFont, Rectangle cellRect, AbstractGridColumn gridColumn, long rowNumber, bool enabledState)
	{
		if (enabledState)
		{
			gridColumn.DrawCell(g, bkBrush, textBrush, textFont, cellRect, m_gridStorage, rowNumber);
		}
		else
		{
			gridColumn.DrawDisabledCell(g, textFont, cellRect, m_gridStorage, rowNumber);
		}
	}

	protected virtual void DoCellPrinting(Graphics g, SolidBrush bkBrush, SolidBrush textBrush, Font textFont, Rectangle cellRect, AbstractGridColumn gridColumn, long rowNumber)
	{
		gridColumn.PrintCell(g, bkBrush, textBrush, textFont, cellRect, m_gridStorage, rowNumber);
	}

	protected virtual void PaintOneCell(Graphics g, int nCol, long nRow, int nEditedCol, long nEditedRow, ref Rectangle rCell, ref Rectangle rCurrentCellRect, ref Rectangle rEditingCellRect)
	{
		AbstractGridColumn gridColumn = m_Columns[nCol];
		int widthInPixels = gridColumn.WidthInPixels;
		int gRID_LINE_WIDTH = ScrollManager.GRID_LINE_WIDTH;
		SolidBrush bkBrush = null;
		SolidBrush textBrush = null;
		_ = Font;
		if (m_scrollMgr.FirstScrollableColumnIndex > 0 && nCol == m_scrollMgr.FirstScrollableColumnIndex)
		{
			rCell.X -= gRID_LINE_WIDTH;
			rCell.Width = widthInPixels + 2 * gRID_LINE_WIDTH;
		}
		else
		{
			rCell.Width = widthInPixels + gRID_LINE_WIDTH;
		}

		if (nEditedCol == nCol && nEditedRow == nRow)
		{
			rEditingCellRect = rCell;
		}
		else if (g.IsVisible(rCell))
		{
			if (ActAsEnabled)
			{
				GetCellGDIObjects(gridColumn, nRow, nCol, ref bkBrush, ref textBrush);
				DoCellPainting(g, bkBrush, textBrush, GetCellFont(nRow, gridColumn), rCell, gridColumn, nRow, enabledState: true);
				if (NeedToHighlightCurrentCell && nCol == m_selMgr.CurrentColumn && nRow == m_selMgr.CurrentRow)
				{
					rCurrentCellRect = rCell;
					if (m_scrollMgr.FirstScrollableColumnIndex > 0 && nCol == m_scrollMgr.FirstScrollableColumnIndex)
					{
						rCurrentCellRect.X++;
						rCurrentCellRect.Width--;
					}
				}
			}
			else
			{
				DoCellPainting(g, bkBrush, textBrush, GetCellFont(nRow, gridColumn), rCell, gridColumn, nRow, enabledState: false);
			}
		}

		rCell.X = rCell.Right;
	}

	protected virtual void GetCellGDIObjects(AbstractGridColumn gridColumn, long nRow, int nCol, ref SolidBrush bkBrush, ref SolidBrush textBrush)
	{
		textBrush = gridColumn.TextBrush.Color != SystemColors.WindowText || textBrush == null ? gridColumn.TextBrush : textBrush;
		bkBrush = gridColumn.BackgroundBrush.Color != SystemColors.Window || bkBrush == null ? gridColumn.BackgroundBrush : bkBrush;
		if (CustomizeCellGDIObjectsEvent != null)
		{
			m_CustomizeCellGDIObjectsArgs.SetRowAndColumn(nRow, m_Columns[nCol].ColumnIndex);
			m_CustomizeCellGDIObjectsArgs.TextBrush = textBrush;
			m_CustomizeCellGDIObjectsArgs.BKBrush = bkBrush;
			CustomizeCellGDIObjectsEvent(this, m_CustomizeCellGDIObjectsArgs);
			textBrush = m_CustomizeCellGDIObjectsArgs.TextBrush;
			bkBrush = m_CustomizeCellGDIObjectsArgs.BKBrush;
		}

		if (gridColumn.WithSelectionBk && m_selMgr.IsCellSelected(nRow, nCol))
		{
			if (ContainsFocus)
			{
				bkBrush = m_highlightBrush;
				textBrush = m_highlightTextBrush;
			}
			else if (m_alwaysHighlightSelection)
			{
				bkBrush = highlightNonFocusedBrush;
				textBrush = highlightNonFocusedTextBrush;
			}
		}
	}

	protected virtual Font GetCellFont(long rowIndex, AbstractGridColumn gridColumn)
	{
		if (gridColumn.ColumnType == 5)
		{
			return m_linkFont;
		}

		return Font;
	}

	private void UpdateScrollableAreaRect()
	{
		m_scrollableArea = ClientRectangle;
		if (m_withHeader)
		{
			_ = m_scrollableArea.Height;
			m_scrollableArea.Y = HeaderHeight;
			m_scrollableArea.Height -= HeaderHeight;
		}

		if (m_scrollMgr.FirstScrollableColumnIndex != 0)
		{
			int num = CalcNonScrollableColumnsWidth();
			m_scrollableArea.X = num;
			m_scrollableArea.Width -= num;
		}

		if (m_scrollMgr.FirstScrollableRowIndex != 0)
		{
			uint num2 = NonScrollableRowsHeight();
			m_scrollableArea.Y += (int)num2;
			m_scrollableArea.Height -= (int)num2;
		}

		m_scrollMgr.OnSAChange(m_scrollableArea);
	}

	protected void CheckAndRePositionEmbeddedControlForSmallSizes()
	{
		if (!IsACellBeingEdited(out var nRowNum, out var nColNum))
		{
			return;
		}

		Rectangle bounds = m_scrollMgr.GetCellRectangle(nRowNum, nColNum);
		if (m_scrollableArea.Height > m_scrollMgr.CellHeight)
		{
			return;
		}

		if (bounds == Rectangle.Empty)
		{
			if (m_curEmbeddedControl.Visible)
			{
				Focus();
				m_curEmbeddedControl.Visible = false;
			}
		}
		else if (bounds.Width >= m_scrollableArea.Width)
		{
			if (m_Columns[nColNum].ColumnType >= 1024)
			{
				AdjustEmbeddedEditorBoundsForCustomColumn(ref bounds, nColNum, nRowNum);
			}

			PositionEmbeddedEditor(bounds, nColNum);
		}
	}

	private void AdjustEditingCellHorizontally(ref Rectangle rEditingCellRect, int nEditingCol)
	{
		rEditingCellRect.X -= ScrollManager.GRID_LINE_WIDTH;
		rEditingCellRect.Width += ScrollManager.GRID_LINE_WIDTH;
		if (HasNonScrollableColumns && nEditingCol >= m_scrollMgr.FirstScrollableColumnIndex && rEditingCellRect.X < m_scrollableArea.X)
		{
			rEditingCellRect.Width -= m_scrollableArea.X - rEditingCellRect.X - ScrollManager.GRID_LINE_WIDTH;
			rEditingCellRect.X = m_scrollableArea.X - ScrollManager.GRID_LINE_WIDTH;
		}

		if (rEditingCellRect.X < -ScrollManager.GRID_LINE_WIDTH)
		{
			rEditingCellRect.Width -= -rEditingCellRect.X - ScrollManager.GRID_LINE_WIDTH;
			rEditingCellRect.X = 0;
		}

		if (rEditingCellRect.Right > m_scrollableArea.Right)
		{
			rEditingCellRect.Width -= rEditingCellRect.Right - m_scrollableArea.Right;
		}
	}

	protected virtual void AdjustEmbeddedEditorBoundsForCustomColumn(ref Rectangle bounds, int uiColumnIndex, long nRowIndex)
	{
	}

	protected void PositionEmbeddedEditor(Rectangle rEditingCellRect, int nEditingCol)
	{
		AdjustEditingCellHorizontally(ref rEditingCellRect, nEditingCol);
		rEditingCellRect.Height = m_curEmbeddedControl.Bounds.Height;
		m_curEmbeddedControl.Bounds = rEditingCellRect;
		if (!m_curEmbeddedControl.Visible)
		{
			m_curEmbeddedControl.Visible = true;
		}

		if (ContainsFocus && !m_curEmbeddedControl.ContainsFocus && FocusEditorOnNavigation)
		{
			m_curEmbeddedControl.Focus();
		}
	}

	private bool StartEditingCell(long nRowIndex, int nColIndex, Rectangle rCellRect, int editType, ref bool bSendMouseClick)
	{
		return StartEditingCell(nRowIndex, nColIndex, rCellRect, editType, ref bSendMouseClick, focusEmbedded: true);
	}

	private bool StartEditingCell(long nRowIndex, int nColIndex, Rectangle rCellRect, int editType, ref bool bSendMouseClick, bool focusEmbedded)
	{
		if (!m_EmbeddedControls.Contains(editType))
		{
			return false;
		}

		if (IsEditing && !StopEditCell())
		{
			return false;
		}

		try
		{
			Control control = m_EmbeddedControls[editType] as Control;
			IBsGridEmbeddedControlManagement gridEmbeddedControlManagement = control as IBsGridEmbeddedControlManagement;
			bSendMouseClick = gridEmbeddedControlManagement.WantMouseClick;
			gridEmbeddedControlManagement.SetHorizontalAlignment(m_Columns[nColIndex].TextAlign);
			IBsGridEmbeddedControl gridEmbeddedControl = (IBsGridEmbeddedControl)control;
			gridEmbeddedControl.ClearData();
			gridEmbeddedControl.Enabled = true;
			try
			{
				m_bInGridStorageCall = true;
				m_gridStorage.FillControlWithData(nRowIndex, m_Columns[nColIndex].ColumnIndex, gridEmbeddedControl);
			}
			finally
			{
				m_bInGridStorageCall = false;
			}

			gridEmbeddedControl.ContentsChangedEvent += _OnEmbeddedControlContentsChangedHandler;
			gridEmbeddedControl.RowIndex = nRowIndex;
			gridEmbeddedControl.ColumnIndex = m_Columns[nColIndex].ColumnIndex;
			rCellRect.Height += ScrollManager.GRID_LINE_WIDTH;
			if (m_Columns[nColIndex].ColumnType >= 1024)
			{
				AdjustEmbeddedEditorBoundsForCustomColumn(ref rCellRect, nColIndex, nRowIndex);
			}

			AdjustEditingCellHorizontally(ref rCellRect, nColIndex);
			control.Bounds = rCellRect;
			SolidBrush bkBrush = null;
			SolidBrush textBrush = null;
			GetCellGDIObjects(m_Columns[nColIndex], nRowIndex, nColIndex, ref bkBrush, ref textBrush);
			control.BackColor = bkBrush.Color;
			control.ForeColor = textBrush.Color;
			m_selMgr.CurrentColumn = nColIndex;
			m_selMgr.CurrentRow = nRowIndex;
			m_curEmbeddedControl = control;
			m_curEmbeddedControl.Visible = true;
			m_curEmbeddedControl.AccessibleName = GetGridCellAccessibleName(nRowIndex, nColIndex);
			if (focusEmbedded)
			{
				SetFocusToEmbeddedControl();
			}

			OnStartedCellEdit();
			return true;
		}
		catch
		{
			m_curEmbeddedControl = null;
			return false;
		}
	}

	public string GetGridCellAccessibleName(long rowIndex, int columnIndex)
	{
		if (m_gridHeader[columnIndex].AccessibleName != null)
		{
			return ControlsResources.Grid_CellDefaultAccessibleName.FmtRes(rowIndex, columnIndex, m_gridHeader[columnIndex].AccessibleName);
		}

		return ControlsResources.Grid_CellDefaultAccessibleName.FmtRes(rowIndex, columnIndex, m_gridHeader[columnIndex].Text);
	}

	private bool StartEditingCell(long nRowIndex, int nColIndex, int editType, bool focusEmbedded)
	{
		Rectangle cellRectangle = m_scrollMgr.GetCellRectangle(nRowIndex, nColIndex);
		if (!cellRectangle.IsEmpty)
		{
			bool bSendMouseClick = false;
			return StartEditingCell(nRowIndex, nColIndex, cellRectangle, editType, ref bSendMouseClick, focusEmbedded);
		}

		return false;
	}

	private bool StartEditingCell(long nRowIndex, int nColIndex, int editType)
	{
		return StartEditingCell(nRowIndex, nColIndex, editType, focusEmbedded: true);
	}

	private bool StopEditCell()
	{
		IBsGridEmbeddedControl gridEmbeddedControl = (IBsGridEmbeddedControl)m_curEmbeddedControl;
		if (gridEmbeddedControl == null)
		{
			InvalidOperationException ex = new(ControlsResources.ExCurControlIsNotIGridEmbedded);
			Diag.Dug(ex);
			throw ex;
		}

		bool containsFocus = ContainsFocus;
		m_bInGridStorageCall = true;
		try
		{
			if (m_gridStorage.SetCellDataFromControl(gridEmbeddedControl.RowIndex, gridEmbeddedControl.ColumnIndex, gridEmbeddedControl))
			{
				m_bInGridStorageCall = false;
				gridEmbeddedControl.ContentsChangedEvent -= _OnEmbeddedControlContentsChangedHandler;
				OnStoppedCellEdit();
				m_curEmbeddedControl.Visible = false;
				m_curEmbeddedControl = null;
				if (containsFocus)
				{
					Focus();
				}

				return true;
			}

			if (containsFocus && m_curEmbeddedControl != null)
			{
				SetFocusToEmbeddedControl();
			}

			return false;
		}
		finally
		{
			m_bInGridStorageCall = false;
		}
	}

	private void CancelEditCell()
	{
		((IBsGridEmbeddedControl)m_curEmbeddedControl ?? throw new InvalidOperationException(ControlsResources.ExCurControlIsNotIGridEmbedded)).ContentsChangedEvent -= _OnEmbeddedControlContentsChangedHandler;
		OnStoppedCellEdit();
		m_curEmbeddedControl.Visible = false;
		m_curEmbeddedControl = null;
		Focus();
	}

	private void UpdateEmbeddedControlsFont()
	{
		if (m_scrollMgr.CellHeight <= 0)
		{
			return;
		}

		foreach (DictionaryEntry embeddedControl in m_EmbeddedControls)
		{
			Control obj = (Control)embeddedControl.Value;
			obj.Font = Font;
			obj.Height = m_scrollMgr.CellHeight + 2 * ScrollManager.GRID_LINE_WIDTH;
		}
	}

	private void UpdateEmbeddedControlsRTL()
	{
		RightToLeft rightToLeft = IsRTL ? RightToLeft.Yes : RightToLeft.No;
		foreach (DictionaryEntry embeddedControl in m_EmbeddedControls)
		{
			((Control)embeddedControl.Value).RightToLeft = rightToLeft;
		}
	}

	private void UpdateSelectionBlockFromMouse(long nRowIndex, int nColIndex)
	{
		EnGridSelectionType selectionType = m_selMgr.SelectionType;
		m_selMgr.UpdateCurrentBlock(nRowIndex, nColIndex);
		if ((selectionType != EnGridSelectionType.CellBlocks || m_captureTracker.LastColumnIndex != nColIndex || m_captureTracker.LastRowIndex != nRowIndex) && (selectionType != EnGridSelectionType.RowBlocks || nRowIndex != m_captureTracker.LastRowIndex) && (selectionType != EnGridSelectionType.ColumnBlocks || nColIndex != m_captureTracker.LastColumnIndex))
		{
			Refresh();
		}

		m_captureTracker.LastColumnIndex = nColIndex;
		m_captureTracker.LastRowIndex = nRowIndex;
	}

	private void AutoscrollTimerProcessor(object myObject, EventArgs myEventArgs)
	{
		Point mousePosition = MousePosition;
		mousePosition = PointToClient(mousePosition);
		if (!m_scrollableArea.Contains(mousePosition))
		{
			int yDelta = -1;
			Native.RECTEx scrollRect = new Native.RECTEx(0, 0, 0, 0);
			HitTestInfo hitTestInfo = HitTestInternal(mousePosition.X, mousePosition.Y);
			long nRowIndex = hitTestInfo.RowIndex;
			int nColIndex = hitTestInfo.ColumnIndex;
			if (hitTestInfo.HitTestResult != EnHitTestResult.ColumnOnly)
			{
				nColIndex = m_captureTracker.LastColumnIndex;
			}

			if (hitTestInfo.HitTestResult != EnHitTestResult.RowOnly)
			{
				nRowIndex = m_captureTracker.LastRowIndex;
			}

			if (mousePosition.Y < m_scrollableArea.Y)
			{
				m_scrollMgr.HandleVScrollWithoutClientRedraw(2, ref yDelta, ref scrollRect);
				nRowIndex = m_scrollMgr.FirstRowIndex;
			}
			else if (mousePosition.Y >= m_scrollableArea.Bottom)
			{
				m_scrollMgr.HandleVScrollWithoutClientRedraw(3, ref yDelta, ref scrollRect);
				nRowIndex = m_scrollMgr.LastRowIndex;
			}

			if (mousePosition.X < m_scrollableArea.X)
			{
				m_scrollMgr.HandleHScrollWithoutClientRedraw(2, ref yDelta, ref scrollRect);
				nColIndex = m_scrollMgr.FirstColumnIndex;
			}
			else if (mousePosition.X >= m_scrollableArea.Right)
			{
				m_scrollMgr.HandleHScrollWithoutClientRedraw(3, ref yDelta, ref scrollRect);
				nColIndex = m_scrollMgr.LastColumnIndex;
			}

			UpdateSelectionBlockFromMouse(nRowIndex, nColIndex);
		}
		else
		{
			if (m_autoScrollTimer.Enabled)
			{
				m_autoScrollTimer.Stop();
			}

			HandleStdCellLBtnMouseMove(mousePosition.X, mousePosition.Y);
		}
	}

	private bool ShouldMakeControlVisible(KeyEventArgs ke)
	{
		Keys[] array =
			[
				Keys.Escape,
				Keys.Tab,
				Keys.Capital,
				Keys.Menu,
				Keys.ShiftKey,
				Keys.ControlKey,
				Keys.Alt,
				Keys.Home,
				Keys.End,
				Keys.Next,
				Keys.Prior,
				Keys.Snapshot,
				Keys.Insert
			];
		if (!FocusEditorOnNavigation)
		{
			Keys[] obj =
				[
					Keys.Left,
					Keys.Right,
					Keys.Up,
					Keys.Down,
					Keys.Return
				];
			Keys[] array2 = new Keys[obj.Length + array.Length];
			array.CopyTo(array2, 0);
			obj.CopyTo(array2, array.Length - 1);
			array = array2;
		}

		for (int i = 0; i < array.Length; i++)
		{
			if (ke.KeyCode == array[i])
			{
				return false;
			}
		}

		return true;
	}

	protected virtual bool HandleKeyboard(KeyEventArgs ke)
	{
		if (!ActAsEnabled)
		{
			return false;
		}

		if (IsEditing && ShouldMakeControlVisible(ke))
		{
			IBsGridEmbeddedControl gridEmbeddedControl = (IBsGridEmbeddedControl)m_curEmbeddedControl;
			EnsureCellIsVisibleInternal(gridEmbeddedControl.RowIndex, GetUIColumnIndexByStorageIndex(gridEmbeddedControl.ColumnIndex));
		}

		switch (ke.KeyCode)
		{
			case Keys.F3:
				if (m_selMgr.CurrentColumn >= 0)
				{
					EnGridButtonArea headerArea = HitTestGridButton(0L, m_selMgr.CurrentColumn, m_captureTracker.CellRect, m_captureTracker.MouseCapturePoint);
					OnHeaderButtonClicked(m_selMgr.CurrentColumn, MouseButtons.Left, headerArea);
					return true;
				}

				return false;
			case Keys.F2:
				if (!IsEditing && m_selMgr.OnlyOneCellSelected)
				{
					int num = m_gridStorage.IsCellEditable(m_selMgr.CurrentRow, m_Columns[m_selMgr.CurrentColumn].ColumnIndex);
					if (num != 0)
					{
						long currentRow = m_selMgr.CurrentRow;
						int currentColumn = m_selMgr.CurrentColumn;
						m_selMgr.Clear();
						StartEditingCell(currentRow, currentColumn, num, focusEmbedded: true);
						m_selMgr.StartNewBlock(currentRow, currentColumn);
						ForwardKeyStrokeToControl(ke);
					}
				}

				break;
			case Keys.Escape:
				if (IsEditing)
				{
					CancelEditCell();
					NotifyAccAboutNewSelection(notifySelection: false, notifyFocus: true);
					return true;
				}

				break;
			case Keys.Return:
				if (m_selMgr.CurrentColumn >= 0 && m_selMgr.CurrentRow >= 0 && m_Columns[m_selMgr.CurrentColumn].ColumnType == 2 && GetButtonCellState(m_selMgr.CurrentRow, m_selMgr.CurrentColumn) == EnButtonCellState.Normal)
				{
					OnKeyPressedOnCell(m_selMgr.CurrentRow, m_selMgr.CurrentColumn, Keys.Space, ke.Modifiers);
					return true;
				}

				if (!IsEditing)
				{
					break;
				}

				if (IsEditing && m_curEmbeddedControl.ContainsFocus)
				{
					if (!StopEditCell())
					{
						return true;
					}
				}
				else if (IsEditing && m_selMgr.CurrentRow < RowCount - 1)
				{
					CancelEditCell();
				}

				if (m_selMgr.CurrentRow < RowCount - 1)
				{
					ProcessUpDownKeys(bDown: true, Keys.None);
				}

				return true;
			case Keys.Prior:
			case Keys.Next:
				return ProcessPageUpDownKeys(ke.KeyCode == Keys.Prior, ke.Modifiers);
			case Keys.Left:
			case Keys.Right:
				if (ke.Shift && !m_selMgr.OnlyOneSelItem && !IsEditing)
				{
					ProcessLeftRightUpDownKeysForBlockSel(ke.KeyCode);
					return true;
				}

				return ProcessLeftRightKeys(ke.KeyCode == Keys.Left, ke.Modifiers, bChangeRowIfNeeded: false);
			case Keys.Up:
			case Keys.Down:
				if (ke.Alt)
				{
					if (m_gridStorage.IsCellEditable(m_selMgr.CurrentRow, m_selMgr.CurrentColumn) == 2 && !m_curEmbeddedControl.Enabled)
					{
						OnMouseButtonClicked(m_selMgr.CurrentRow, m_selMgr.CurrentColumn, default, MouseButtons.Left);
						return true;
					}

					return false;
				}

				if (ke.Shift && !m_selMgr.OnlyOneSelItem && !IsEditing)
				{
					ProcessLeftRightUpDownKeysForBlockSel(ke.KeyCode);
					return true;
				}

				return ProcessUpDownKeys(ke.KeyCode == Keys.Down, ke.Modifiers);
			case Keys.Tab:
				if (!ke.Control)
				{
					return ProcessLeftRightKeys(ke.Shift, Keys.None, bChangeRowIfNeeded: true);
				}

				return false;
			case Keys.End:
			case Keys.Home:
				ProcessHomeEndKeys(ke.KeyCode == Keys.Home, ke.Modifiers);
				return true;
			default:
				if (m_selMgr.CurrentColumn >= 0 && m_selMgr.CurrentRow >= 0)
				{
					OnKeyPressedOnCell(m_selMgr.CurrentRow, m_selMgr.CurrentColumn, ke.KeyCode, ke.Modifiers);
				}

				break;
		}

		return false;
	}

	protected void ProcessHomeEndKeys(bool bHome, Keys mod)
	{
		long currentRow = m_selMgr.CurrentRow;
		int currentColumn = m_selMgr.CurrentColumn;
		long num;
		int num3;
		int num2;
		if (mod == Keys.Control)
		{
			if (bHome)
			{
				num = m_scrollMgr.FirstScrollableRowIndex;
				num3 = num2 = m_scrollMgr.FirstScrollableColumnIndex;
			}
			else
			{
				num = RowCount - 1;
				num2 = NumColInt;
				num3 = num2 - 1;
			}
		}
		else
		{
			if (bHome)
			{
				num2 = num3 = m_scrollMgr.FirstScrollableColumnIndex;
			}
			else
			{
				num2 = NumColInt;
				num3 = num2 - 1;
			}

			num = m_selMgr.CurrentRow;
			if (num < 0)
			{
				num = m_scrollMgr.FirstScrollableRowIndex;
			}
		}

		bool flag = currentRow != num || currentColumn != num3;
		if (flag)
		{
			if (!CheckAndProcessCurrentEditingCellForKeyboard())
			{
				return;
			}

			m_selMgr.Clear();
		}

		m_scrollMgr.EnsureCellIsVisible(num, num2, bMakeFirstColFullyVisible: true, bRedraw: false);
		if (flag)
		{
			int num4 = m_gridStorage.IsCellEditable(num, m_Columns[num3].ColumnIndex);
			if (num4 != 0)
			{
				StartEditingCell(num, num3, num4, FocusEditorOnNavigation);
			}

			m_selMgr.StartNewBlock(num, num3);
			OnSelectionChanged(m_selMgr.SelectedBlocks);
		}

		Refresh();
	}

	protected bool ProcessPageUpDownKeys(bool bPageUp, Keys mod)
	{
		_ = mod;
		long currentRow = m_selMgr.CurrentRow;
		int currentColumn = m_selMgr.CurrentColumn;
		if (currentRow < 0 || currentColumn < 0)
		{
			return false;
		}

		if (currentRow == 0 && bPageUp || currentRow == RowCount - 1 && !bPageUp)
		{
			return true;
		}

		int yDelta = -1;
		Native.RECTEx scrollRect = new Native.RECTEx(0, 0, 0, 0);
		_ = m_scrollMgr.FirstRowIndex;
		int num = m_scrollMgr.CalcVertPageSize(m_scrollableArea);
		if (!CheckAndProcessCurrentEditingCellForKeyboard())
		{
			return true;
		}

		if (bPageUp)
		{
			if (currentRow >= m_scrollMgr.FirstRowIndex && currentRow <= m_scrollMgr.LastRowIndex)
			{
				m_scrollMgr.HandleVScrollWithoutClientRedraw(2, ref yDelta, ref scrollRect);
			}
			else
			{
				m_scrollMgr.EnsureRowIsVisbleWithoutClientRedraw(currentRow - num, bMakeRowTheTopOne: true, out yDelta);
			}
		}
		else if (currentRow >= m_scrollMgr.FirstRowIndex && currentRow <= m_scrollMgr.LastRowIndex)
		{
			m_scrollMgr.HandleVScrollWithoutClientRedraw(3, ref yDelta, ref scrollRect);
		}
		else
		{
			m_scrollMgr.EnsureRowIsVisbleWithoutClientRedraw(currentRow + num, bMakeRowTheTopOne: true, out yDelta);
		}

		m_selMgr.Clear();
		currentRow += (!bPageUp ? 1 : -1) * num;
		if (currentRow < 0)
		{
			currentRow = 0L;
		}

		if (currentRow > RowCount - 1)
		{
			currentRow = RowCount - 1;
		}

		int num2 = m_gridStorage.IsCellEditable(currentRow, m_Columns[currentColumn].ColumnIndex);
		if (num2 != 0)
		{
			StartEditingCell(currentRow, currentColumn, num2, FocusEditorOnNavigation);
		}

		m_selMgr.StartNewBlock(currentRow, currentColumn);
		OnSelectionChanged(m_selMgr.SelectedBlocks);
		Refresh();
		return true;
	}

	protected void ProcessLeftRightUpDownKeysForBlockSel(Keys key)
	{
		long lastUpdatedRow = m_selMgr.LastUpdatedRow;
		int lastUpdatedColumn = m_selMgr.LastUpdatedColumn;
		switch (key)
		{
			case Keys.Up:
				if (m_selMgr.SelectionType == EnGridSelectionType.ColumnBlocks)
				{
					m_scrollMgr.HandleVScroll(0);
					return;
				}

				if (lastUpdatedRow != -1 && lastUpdatedRow > 0)
				{
					m_selMgr.UpdateCurrentBlock(lastUpdatedRow - 1, lastUpdatedColumn);
					break;
				}

				return;
			case Keys.Down:
				if (m_selMgr.SelectionType == EnGridSelectionType.ColumnBlocks)
				{
					m_scrollMgr.HandleVScroll(1);
					return;
				}

				if (lastUpdatedRow != -1 && lastUpdatedRow < RowCount - 1)
				{
					m_selMgr.UpdateCurrentBlock(lastUpdatedRow + 1, lastUpdatedColumn);
					break;
				}

				return;
			case Keys.Left:
				if (m_selMgr.SelectionType == EnGridSelectionType.RowBlocks)
				{
					m_scrollMgr.HandleHScroll(0);
					return;
				}

				if (lastUpdatedColumn != -1 && lastUpdatedColumn > 0)
				{
					m_selMgr.UpdateCurrentBlock(lastUpdatedRow, lastUpdatedColumn - 1);
					break;
				}

				return;
			default:
				if (m_selMgr.SelectionType == EnGridSelectionType.RowBlocks)
				{
					m_scrollMgr.HandleHScroll(1);
					return;
				}

				if (lastUpdatedColumn != -1 && lastUpdatedColumn < NumColInt - 1)
				{
					m_selMgr.UpdateCurrentBlock(lastUpdatedRow, lastUpdatedColumn + 1);
					break;
				}

				return;
		}

		OnSelectionChanged(m_selMgr.SelectedBlocks);
		if (m_selMgr.SelectionType == EnGridSelectionType.CellBlocks)
		{
			m_scrollMgr.EnsureCellIsVisible(m_selMgr.LastUpdatedRow, m_selMgr.LastUpdatedColumn, bMakeFirstColFullyVisible: true, bRedraw: false);
		}
		else if (m_selMgr.SelectionType == EnGridSelectionType.ColumnBlocks)
		{
			m_scrollMgr.EnsureColumnIsVisible(m_selMgr.LastUpdatedColumn, bMakeFirstFullyVisible: true, bRedraw: false);
		}
		else
		{
			m_scrollMgr.EnsureRowIsVisbleWithoutClientRedraw(m_selMgr.LastUpdatedRow, bMakeRowTheTopOne: true, out var _);
		}

		Refresh();
	}

	protected bool ProcessUpDownKeys(bool bDown, Keys mod)
	{
		if (Keys.Control == mod && !IsEditing)
		{
			m_scrollMgr.HandleVScroll(bDown ? 1 : 0);
			return true;
		}

		if (m_selMgr.CurrentColumn != -1 && m_selMgr.CurrentRow != -1 && RowCount > 0)
		{
			if (m_selMgr.CurrentRow == 0L && !bDown || m_selMgr.CurrentRow == RowCount - 1 && bDown)
			{
				return true;
			}

			if (IsEditing && !CheckAndProcessCurrentEditingCellForKeyboard())
			{
				return true;
			}

			if (bDown)
			{
				m_selMgr.CurrentRow = Math.Min(RowCount - 1, m_selMgr.CurrentRow + 1);
			}
			else
			{
				m_selMgr.CurrentRow = Math.Max(0L, m_selMgr.CurrentRow - 1);
			}

			m_selMgr.Clear(bClearCurrentCell: false);
			bool flag = IsCellEditableFromKeyboardNav();
			if (!flag)
			{
				m_selMgr.StartNewBlock(m_selMgr.CurrentRow, m_selMgr.CurrentColumn);
			}

			bool bMakeRowTheTopOne = !bDown;
			m_scrollMgr.EnsureRowIsVisbleWithoutClientRedraw(m_selMgr.CurrentRow, bMakeRowTheTopOne, out var _);
			m_scrollMgr.EnsureColumnIsVisible(m_selMgr.CurrentColumn, bMakeFirstFullyVisible: false, bRedraw: false);
			CompleteArrowsNatigation(flag, bDown ? Keys.Down : Keys.Up, mod);
			return true;
		}

		return false;
	}

	protected bool ProcessLeftRightKeys(bool bLeft, Keys mod, bool bChangeRowIfNeeded)
	{
		if (m_selMgr.CurrentColumn != -1 && m_selMgr.CurrentRow != -1 && RowCount > 0)
		{
			bool flag = m_selMgr.CurrentColumn == 0 && m_selMgr.CurrentRow == 0 && bLeft || m_selMgr.CurrentColumn == NumColInt - 1 && m_selMgr.CurrentRow == RowCount - 1 && !bLeft;
			if (flag && bChangeRowIfNeeded)
			{
				return HandleTabOnLastOrFirstCell(bLeft);
			}

			if (IsEditing && !flag && !CheckAndProcessCurrentEditingCellForKeyboard())
			{
				return true;
			}

			bool bMakeFirstColFullyVisible = bLeft;
			bool flag2 = false;
			m_scrollMgr.EnsureCellIsVisible(m_selMgr.CurrentRow, m_selMgr.CurrentColumn, bMakeFirstColFullyVisible, bRedraw: false);
			if (bLeft)
			{
				m_selMgr.CurrentColumn--;
				if (m_selMgr.CurrentColumn < 0)
				{
					if (bChangeRowIfNeeded && !flag)
					{
						m_selMgr.CurrentColumn = NumColInt - 1;
						m_selMgr.CurrentRow--;
						flag2 = true;
					}
					else
					{
						m_selMgr.CurrentColumn = 0;
					}
				}
			}
			else if (m_selMgr.CurrentColumn == m_scrollMgr.FirstScrollableColumnIndex - 1)
			{
				m_selMgr.CurrentColumn = m_scrollMgr.FirstScrollableColumnIndex;
			}
			else
			{
				m_selMgr.CurrentColumn++;
				if (m_selMgr.CurrentColumn > NumColInt - 1)
				{
					if (bChangeRowIfNeeded && !flag)
					{
						m_selMgr.CurrentColumn = 0;
						m_selMgr.CurrentRow++;
						flag2 = true;
					}
					else
					{
						m_selMgr.CurrentColumn = NumColInt - 1;
					}
				}
			}

			m_selMgr.Clear(bClearCurrentCell: false);
			bool flag3 = IsCellEditableFromKeyboardNav();
			if (!flag3)
			{
				m_selMgr.StartNewBlock(m_selMgr.CurrentRow, m_selMgr.CurrentColumn);
			}

			if (bLeft || !bLeft && flag2 || m_selMgr.CurrentColumn < m_scrollMgr.FirstColumnIndex)
			{
				m_scrollMgr.EnsureCellIsVisible(m_selMgr.CurrentRow, m_selMgr.CurrentColumn, bMakeFirstColFullyVisible: true, bRedraw: false);
			}
			else if (m_selMgr.CurrentColumn > m_scrollMgr.LastColumnIndex)
			{
				m_scrollMgr.MakeNextColumnVisible(bRedraw: false);
			}

			CompleteArrowsNatigation(flag3, bLeft ? Keys.Left : Keys.Right, mod);
			return true;
		}

		return false;
	}

	protected int MeasureWidthOfRows(int columnIndex, int columnType, long nFirstRow, long nLastRow, Graphics g)
	{
		int num = 0;
		Rectangle r = new Rectangle(0, 0, 100000, 100000);
		TextFormatFlags sFormat = GridConstants.DefaultTextFormatFlags;
		Size proposedSize = new Size(int.MaxValue, RowHeight);
		int columnIndex2 = m_Columns[columnIndex].ColumnIndex;
		GridCheckBoxColumn gridCheckBoxColumn = null;
		if (columnType == 4)
		{
			gridCheckBoxColumn = m_Columns[columnIndex] as GridCheckBoxColumn;
		}

		for (long num2 = nFirstRow; num2 <= nLastRow; num2++)
		{
			SizeF sizeF = SizeF.Empty;
			Bitmap image;
			string buttonLabel;
			switch (columnType)
			{
				case 2:
					{
						m_gridStorage.GetCellDataForButton(num2, columnIndex2, out _, out image, out buttonLabel);
						sizeF = GridButton.CalculateInitialContentsRect(g, r, buttonLabel, image, HorizontalAlignment.Left, Font, IsRTL, ref sFormat, out _).Size;
						break;
					}
				case 1:
				case 5:
					{
						buttonLabel = GetCellStringForResizeToShowAll(num2, columnIndex2, out TextFormatFlags tff);
						if (buttonLabel != null && buttonLabel != "")
						{
							sizeF = TextRenderer.MeasureText(g, buttonLabel, GetCellFont(num2, m_Columns[columnIndex]), proposedSize, tff);
						}

						break;
					}
				case 3:
					image = m_gridStorage.GetCellDataAsBitmap(num2, columnIndex2);
					if (image != null)
					{
						sizeF = image.Size;
					}

					break;
				case 4:
					image = gridCheckBoxColumn.BitmapFromGridCheckBoxState(EnGridCheckBoxState.Unchecked);
					if (image != null)
					{
						sizeF = image.Size;
					}

					break;
			}

			if (sizeF.Width > num)
			{
				num = (int)sizeF.Width;
			}
		}

		if (columnType != 3)
		{
			num += 2 * MarginsWidth;
		}

		return num;
	}

	protected void NotifyAccAboutNewSelection(bool notifySelection, bool notifyFocus)
	{
		int currentColumn = m_selMgr.CurrentColumn;
		int childID = (int)m_selMgr.CurrentRow + (WithHeader ? 1 : 0);
		int objectID = currentColumn + 2 + 1;
		if (notifySelection)
		{
			AccessibilityNotifyClients(AccessibleEvents.Selection, objectID, childID);
		}

		if (notifyFocus)
		{
			AccessibilityNotifyClients(AccessibleEvents.Focus, objectID, childID);
		}
	}

	private bool CheckAndProcessCurrentEditingCellForKeyboard()
	{
		if (IsEditing)
		{
			if (!m_curEmbeddedControl.ContainsFocus)
			{
				CancelEditCell();
			}
			else if (!StopEditCell())
			{
				return false;
			}
		}

		return true;
	}

	protected EnGridButtonArea HitTestGridButton(long rowIndex, int colIndex, Rectangle btnRect, Point ptToHitTest)
	{
		if (rowIndex >= 0)
		{
			return EnGridButtonArea.Background;
		}

		GridButton headerGridButton = m_gridHeader.HeaderGridButton;
		GridHeader.HeaderItem headerItem = m_gridHeader[colIndex];
		using Graphics g = GraphicsFromHandle();
		return headerGridButton.HitTest(g, ptToHitTest, btnRect, headerItem.Text, headerItem.Bmp, headerItem.Align, headerItem.TextBmpLayout);
	}

	private void CompleteArrowsNatigation(bool bStartEditNewCurrentCell, Keys keyPressed, Keys modifiers)
	{
		if (bStartEditNewCurrentCell)
		{
			if (StartEditingCell(m_selMgr.CurrentRow, m_selMgr.CurrentColumn, m_gridStorage.IsCellEditable(m_selMgr.CurrentRow, m_Columns[m_selMgr.CurrentColumn].ColumnIndex), FocusEditorOnNavigation) && m_curEmbeddedControl.ContainsFocus)
			{
				NotifyControlAboutFocusFromKeyboard(keyPressed, modifiers);
			}

			m_selMgr.StartNewBlock(m_selMgr.CurrentRow, m_selMgr.CurrentColumn);
		}

		OnSelectionChanged(m_selMgr.SelectedBlocks);
		Refresh();
	}

	protected EnButtonCellState GetButtonCellState(long nRowIndex, int nColIndex)
	{
		m_gridStorage.GetCellDataForButton(nRowIndex, m_Columns[nColIndex].ColumnIndex, out var state, out _, out _);
		return state;
	}

	private void ValidateColumnType(int nType)
	{
		if (nType == 3 || nType == 2 || nType == 1 || nType == 4 || nType == 5 || nType >= 1024)
		{
			return;
		}

		ArgumentException ex = new(ControlsResources.ExInvalidCustomColType, "ci.ColumnType");
		Diag.Dug(ex);
		throw ex;
	}

	private void ProcessMouseMoveWithoutCapture(MouseEventArgs mevent)
	{
		HitTestInfo hitTestInfo = HitTestInternal(mevent.X, mevent.Y);
		SetCursorFromHitTest(hitTestInfo.HitTestResult, hitTestInfo.RowIndex, hitTestInfo.ColumnIndex, hitTestInfo.AreaRectangle);
		if (mevent.Button == MouseButtons.None)
		{
			ProcessForTooltip(hitTestInfo.HitTestResult, hitTestInfo.RowIndex, hitTestInfo.ColumnIndex);
		}
	}

	private string GetClipboardTextForSelectionBlock(int nBlockNum)
	{
		long num;
		long num2;
		int num3;
		int num4;
		if (m_selMgr.SelectionType == EnGridSelectionType.ColumnBlocks || m_selMgr.SelectionType == EnGridSelectionType.SingleColumn)
		{
			num = 0L;
			num2 = RowCount - 1;
			num3 = m_selMgr.SelectedBlocks[nBlockNum].X;
			num4 = m_selMgr.SelectedBlocks[nBlockNum].Right;
		}
		else if (m_selMgr.SelectionType == EnGridSelectionType.RowBlocks || m_selMgr.SelectionType == EnGridSelectionType.SingleRow)
		{
			num3 = 0;
			num4 = NumColInt - 1;
			num = m_selMgr.SelectedBlocks[nBlockNum].Y;
			num2 = m_selMgr.SelectedBlocks[nBlockNum].Bottom;
		}
		else
		{
			num3 = m_selMgr.SelectedBlocks[nBlockNum].X;
			num4 = m_selMgr.SelectedBlocks[nBlockNum].Right;
			num = m_selMgr.SelectedBlocks[nBlockNum].Y;
			num2 = m_selMgr.SelectedBlocks[nBlockNum].Bottom;
		}

		if (num4 >= num3 && num2 >= num && num3 >= 0 && num >= 0)
		{
			return GetClipboardTextForCells(num, num2, num3, num4);
		}

		InvalidOperationException ex = new(ControlsResources.ExInvalidGridStateForClipboard);
		Diag.Dug(ex);
		throw ex;
	}

	private string GetClipboardTextForCells(long nStartRow, long nEndRow, int nStartCol, int nEndCol)
	{
		if (m_gridStorage == null)
		{
			InvalidOperationException ex = new();
			Diag.Dug(ex);
			throw ex;
		}

		StringBuilder stringBuilder = new StringBuilder(256);
		if (!OnBeforeGetClipboardTextForCells(stringBuilder, nStartRow, nEndRow, nStartCol, nEndCol))
		{
			for (long num = nStartRow; num <= nEndRow; num++)
			{
				for (int i = nStartCol; i <= nEndCol; i++)
				{
					string buttonLabel;
					if (m_Columns[i].ColumnType == 1 || m_Columns[i].ColumnType == 5)
					{
						buttonLabel = GetTextBasedColumnStringForClipboardText(num, i);
					}
					else if (m_Columns[i].ColumnType == 3)
					{
						buttonLabel = StringForBitmapData;
					}
					else if (m_Columns[i].ColumnType != 2)
					{
						buttonLabel = m_Columns[i].ColumnType != 4 ? GetCustomColumnStringForClipboardText(num, i) : m_gridStorage.GetCellDataForCheckBox(num, m_Columns[i].ColumnIndex).ToString();
					}
					else
					{
						m_gridStorage.GetCellDataForButton(num, m_Columns[i].ColumnIndex, out _, out _, out buttonLabel);
						buttonLabel ??= StringForButtonsWithBmpOnly;
					}

					if (i < nEndCol)
					{
						stringBuilder.Append(buttonLabel);
						stringBuilder.Append(ColumnsSeparator);
					}
					else if (i == nEndCol)
					{
						if (num < nEndRow)
						{
							stringBuilder.Append(EnsureStringEndsWithNewline(buttonLabel));
						}
						else
						{
							stringBuilder.Append(buttonLabel);
						}
					}
				}
			}
		}

		return stringBuilder.ToString();
	}

	private bool ProcessSelAndEditingForLeftClickOnCell(Keys modKeys, long nRowIndex, int nColIndex, out int editType, out bool bShouldStartEditing, out bool bDragCancelled)
	{
		bDragCancelled = false;
		editType = m_gridStorage.IsCellEditable(nRowIndex, m_Columns[nColIndex].ColumnIndex);
		int columnType = m_Columns[nColIndex].ColumnType;
		if (columnType == 2 || columnType == 4)
		{
			editType = 0;
		}

		bShouldStartEditing = false;
		bool flag = false;
		if ((modKeys & Keys.Control) == 0 && (modKeys & Keys.Shift) == 0)
		{
			bDragCancelled = !OnCanInitiateDragFromCell(m_captureTracker.RowIndex, m_captureTracker.ColumnIndex);
			if (!m_selMgr.IsCellSelected(nRowIndex, nColIndex))
			{
				bShouldStartEditing = editType != 0;
				m_selMgr.Clear();
				flag = true;
			}
			else if (bDragCancelled)
			{
				bShouldStartEditing = editType != 0;
				flag = !m_selMgr.OnlyOneSelItem;
				if (bShouldStartEditing)
				{
					m_selMgr.Clear();
				}
				else if (flag)
				{
					m_selMgr.Clear();
				}
			}
		}
		else
		{
			bShouldStartEditing = false;
			if (m_selMgr.OnlyOneSelItem)
			{
				flag = !m_selMgr.IsCellSelected(nRowIndex, nColIndex);
				m_selMgr.Clear();
			}
		}

		return flag;
	}

	private bool HandleStartCellEditFromStdCellLBtnDown(int editType, long nRowIndex, int nColIndex, bool bNotifySelChange)
	{
		bool bSendMouseClick = false;
		if (StartEditingCell(nRowIndex, nColIndex, m_captureTracker.CellRect, editType, ref bSendMouseClick))
		{
			if (bSendMouseClick)
			{
				SendMouseClickToEmbeddedControl();
			}

			m_selMgr.StartNewBlock(nRowIndex, nColIndex);
			if (!OnMouseButtonClicked(nRowIndex, nColIndex, m_captureTracker.CellRect, MouseButtons.Left))
			{
				Refresh();
			}

			if (bNotifySelChange)
			{
				OnSelectionChanged(m_selMgr.SelectedBlocks);
			}

			return true;
		}

		return false;
	}

	private int GetFirstNonMergedToTheLeft(int colIndex)
	{
		int num = colIndex;
		while (num > 0 && m_gridHeader[num - 1].MergedWithRight)
		{
			num--;
		}

		return num;
	}

	private void OnUserPrefChanged(object sender, UserPreferenceChangedEventArgs pref)
	{
		if (InvokeRequired)
		{
			BeginInvoke(new EventHandler<UserPreferenceChangedEventArgs>(OnUserPrefChanged), sender, pref);
		}
		else
		{
			if (pref.Category != UserPreferenceCategory.Color)
			{
				return;
			}

			DisposeCachedGDIObjects();
			InitializeCachedGDIObjects();
			if (IsHandleCreated)
			{
				Invalidate();
			}

			SolidBrush solidBrush = new SolidBrush(new LinkLabel().LinkColor);
			foreach (AbstractGridColumn column in m_Columns)
			{
				if (column.ColumnType == 5)
				{
					if (column.TextBrush != null)
					{
						column.TextBrush.Dispose();
						column.TextBrush = null;
					}

					column.TextBrush = solidBrush.Clone() as SolidBrush;
				}
			}
		}
	}

	private void TuneToHeaderFont(Font f)
	{
		if (!f.Equals(m_gridHeader.HeaderGridButton.TextFont) || m_headerHeight == 0)
		{
			m_gridHeader.HeaderGridButton.TextFont = f;
			m_headerHeight = CalculateHeaderHeight(f);
			if (IsHandleCreated)
			{
				UpdateScrollableAreaRect();
				Refresh();
			}
		}
	}

	private void ForwardKeyStrokeToControl(KeyEventArgs ke)
	{
		(m_curEmbeddedControl as IBsGridEmbeddedControlManagement2)?.ReceiveKeyboardEvent(ke);
	}

	private void NotifyControlAboutFocusFromKeyboard(Keys keyPressed, Keys modifiers)
	{
		(m_curEmbeddedControl as IBsGridEmbeddedControlManagement2)?.PostProcessFocusFromKeyboard(keyPressed, modifiers);
	}

	private void SetFirstScrollableColumnInt(int columnIndex, bool recalcGrid)
	{
		if (columnIndex <= 0 || CalcNonScrollableColumnsWidth(columnIndex - 1) < ClientRectangle.Width)
		{
			m_scrollMgr.FirstScrollableColumnIndex = columnIndex;
			UpdateScrollableAreaRect();
			if (recalcGrid)
			{
				UpdateGridInternal(bRecalcRows: true);
			}
		}
	}

	private bool ProcessNonScrollableVerticalAreaChange(bool recalcGridIfNeeded)
	{
		int num = CalcNonScrollableColumnsWidth();
		int width = ClientRectangle.Width;
		bool result = false;
		if (num >= width && IsHandleCreated)
		{
			int firstScrollableColumn = FirstScrollableColumn;
			SetFirstScrollableColumnInt(0, recalcGridIfNeeded);
			result = !recalcGridIfNeeded;
			m_scrollMgr.EnsureColumnIsVisible(0, bMakeFirstFullyVisible: true, recalcGridIfNeeded);
			OnResetFirstScrollableColumn(firstScrollableColumn, 0);
		}
		else
		{
			UpdateScrollableAreaRect();
		}

		return result;
	}

	private bool IsInputCharInternal(char charCode)
	{
		if (charCode >= '\u0001' && charCode <= '\u001a')
		{
			return false;
		}

		if (charCode != '\b' && charCode != 0 && charCode != '\n' && charCode != '\r' && charCode != '\u007f')
		{
			return true;
		}

		return false;
	}

	private void ResizeMultipleColumns(int firstColumnIndex, int lastColumnIndex, int sizeDelta, bool bFinalUpdate, int[] columnWidths)
	{
		float num2 = 1f;
		int num5 = sizeDelta;
		int num6 = firstColumnIndex;
		while (num6 <= lastColumnIndex)
		{
			if (num2 > 0f)
			{
				int num = m_Columns[num6].WidthInPixels;
				float num3 = m_gridHeader[num6].MergedHeaderResizeProportion;
				if (num3 > num2)
				{
					num3 = num2;
				}

				int num4;
				if (num6 == lastColumnIndex)
				{
					num4 = num5;
				}
				else
				{
					num4 = (int)(sizeDelta * num3);
					if (num4 == 0)
					{
						num6++;
						continue;
					}
				}

				int num7 = Math.Max(m_Columns[num6].OrigWidthInPixelsDuringResize + num4, GetMinWidthOfColumn(num6));
				num5 += -Math.Sign(sizeDelta) * Math.Abs(m_Columns[num6].OrigWidthInPixelsDuringResize - num7);
				num2 -= num3;
				if (columnWidths == null)
				{
					m_Columns[num6].WidthInPixels = num7;
					if ((num != m_Columns[num6].WidthInPixels || bFinalUpdate) && m_captureTracker.ColumnIndex >= m_scrollMgr.FirstScrollableColumnIndex)
					{
						m_scrollMgr.UpdateColWidth(num6, num, m_Columns[num6].WidthInPixels, bFinalUpdate);
					}
				}
				else
				{
					columnWidths[num6 - firstColumnIndex] = num7;
				}
			}

			num6++;
		}
	}

	private void SetColumnWidthInternalForPublic(int nColIndex, EnGridColumnWidthType widthType, int nWidth)
	{
		SetColumnWidthInternal(nColIndex, widthType, nWidth);
		if (m_nIsInitializingCount == 0)
		{
			Update();
		}
	}

	private string EnsureStringEndsWithNewline(string aString)
	{
		if (aString != null && !aString.EndsWith(NewLineCharacters))
		{
			return aString + NewLineCharacters;
		}

		return aString;
	}
}
