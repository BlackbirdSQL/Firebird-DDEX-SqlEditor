#region Assembly Microsoft.SqlServer.GridControl, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLCommon\Microsoft.SqlServer.GridControl.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Drawing;
using System.Windows.Forms;
using BlackbirdSql.Common.Controls.Enums;

// namespace Microsoft.SqlServer.Management.UI.Grid
namespace BlackbirdSql.Common.Controls.Grid
{
	public sealed class CaptureTracker
	{
		public enum EnDragOperation
		{
			None,
			DragReady,
			StartedDrag
		}

		private EnHitTestResult m_captureHitTest;

		private Rectangle m_cellRect;

		private Rectangle m_adjustedCellRect;

		private long m_rowIndex;

		private int m_columnIndex;

		private bool m_wasOverButton;

		private EnGridButtonArea m_buttonArea;

		private int m_origColumnWidth;

		private int m_lastColumnWidth;

		private int m_mouseOffsetForColResize;

		private int m_leftMostMergedColumnIndex;

		private int m_minWidthDuringColResize;

		private int m_totalGridLineAdjDuringResize;

		private int m_lastColumnIndex;

		private long m_lastRowIndex;

		private bool m_buttonWasPushed;

		private Point m_mouseCapturePoint;

		private int m_selectionBlockIndex;

		private EnDragOperation m_dragState;

		private GridDragImageListOperation m_dragOper;

		private int m_headerDragY;

		private int m_colIndexToDragColAfter;

		public static int NoColIndexToDragColAfter = -2;

		private bool embContrlFocused;

		private DateTime timeEvent = DateTime.MinValue;

		private Timer hyperlinkSelTimer;

		public EnHitTestResult CaptureHitTest
		{
			get
			{
				return m_captureHitTest;
			}
			set
			{
				m_captureHitTest = value;
			}
		}

		public Rectangle CellRect
		{
			get
			{
				return m_cellRect;
			}
			set
			{
				m_cellRect = value;
			}
		}

		public Rectangle AdjustedCellRect
		{
			get
			{
				return m_adjustedCellRect;
			}
			set
			{
				m_adjustedCellRect = value;
			}
		}

		public long RowIndex
		{
			get
			{
				return m_rowIndex;
			}
			set
			{
				m_rowIndex = value;
			}
		}

		public int ColumnIndex
		{
			get
			{
				return m_columnIndex;
			}
			set
			{
				m_columnIndex = value;
			}
		}

		public bool WasOverButton
		{
			get
			{
				return m_wasOverButton;
			}
			set
			{
				m_wasOverButton = value;
			}
		}

		public EnGridButtonArea ButtonArea
		{
			get
			{
				return m_buttonArea;
			}
			set
			{
				m_buttonArea = value;
			}
		}

		public int OrigColumnWidth
		{
			get
			{
				return m_origColumnWidth;
			}
			set
			{
				m_origColumnWidth = value;
			}
		}

		public int LastColumnWidth
		{
			get
			{
				return m_lastColumnWidth;
			}
			set
			{
				m_lastColumnWidth = value;
			}
		}

		public int MouseOffsetForColResize
		{
			get
			{
				return m_mouseOffsetForColResize;
			}
			set
			{
				m_mouseOffsetForColResize = value;
			}
		}

		public int LeftMostMergedColumnIndex
		{
			get
			{
				return m_leftMostMergedColumnIndex;
			}
			set
			{
				m_leftMostMergedColumnIndex = value;
			}
		}

		public int MinWidthDuringColResize
		{
			get
			{
				return m_minWidthDuringColResize;
			}
			set
			{
				m_minWidthDuringColResize = value;
			}
		}

		public int TotalGridLineAdjDuringResize
		{
			get
			{
				return m_totalGridLineAdjDuringResize;
			}
			set
			{
				m_totalGridLineAdjDuringResize = value;
			}
		}

		public int LastColumnIndex
		{
			get
			{
				return m_lastColumnIndex;
			}
			set
			{
				m_lastColumnIndex = value;
			}
		}

		public long LastRowIndex
		{
			get
			{
				return m_lastRowIndex;
			}
			set
			{
				m_lastRowIndex = value;
			}
		}

		public bool ButtonWasPushed
		{
			get
			{
				return m_buttonWasPushed;
			}
			set
			{
				m_buttonWasPushed = value;
			}
		}

		public Point MouseCapturePoint
		{
			get
			{
				return m_mouseCapturePoint;
			}
			set
			{
				m_mouseCapturePoint = value;
			}
		}

		public int SelectionBlockIndex
		{
			get
			{
				return m_selectionBlockIndex;
			}
			set
			{
				m_selectionBlockIndex = value;
			}
		}

		public EnDragOperation DragState
		{
			get
			{
				return m_dragState;
			}
			set
			{
				m_dragState = value;
			}
		}

		public GridDragImageListOperation DragImageOperation
		{
			get
			{
				return m_dragOper;
			}
			set
			{
				if (m_dragOper != null && m_dragOper != value)
				{
					try
					{
						m_dragOper.Dispose();
					}
					catch
					{
					}

					m_dragOper = null;
				}

				m_dragOper = value;
			}
		}

		public int HeaderDragY
		{
			get
			{
				return m_headerDragY;
			}
			set
			{
				m_headerDragY = value;
			}
		}

		public int ColIndexToDragColAfter
		{
			get
			{
				return m_colIndexToDragColAfter;
			}
			set
			{
				m_colIndexToDragColAfter = value;
			}
		}

		public bool WasEmbeddedControlFocused
		{
			get
			{
				return embContrlFocused;
			}
			set
			{
				embContrlFocused = value;
			}
		}

		public DateTime Time
		{
			get
			{
				return timeEvent;
			}
			set
			{
				timeEvent = value;
			}
		}

		public Timer HyperLinkSelectionTimer
		{
			get
			{
				return hyperlinkSelTimer;
			}
			set
			{
				hyperlinkSelTimer = value;
			}
		}

		public void UpdateAdjustedRectHorizontally(int x, int width)
		{
			m_adjustedCellRect.X = x;
			m_adjustedCellRect.Width = width;
		}

		public void SetInfoFromHitTest(HitTestInfo htInfo)
		{
			m_rowIndex = htInfo.RowIndex;
			m_columnIndex = htInfo.ColumnIndex;
			m_cellRect = htInfo.AreaRectangle;
			m_captureHitTest = htInfo.HitTestResult;
		}

		public CaptureTracker()
		{
			Reset();
		}

		public void Reset()
		{
			m_captureHitTest = EnHitTestResult.Nothing;
			m_cellRect = Rectangle.Empty;
			m_adjustedCellRect = Rectangle.Empty;
			m_rowIndex = -1L;
			m_columnIndex = -1;
			m_wasOverButton = false;
			m_buttonArea = EnGridButtonArea.Background;
			m_origColumnWidth = -1;
			m_lastColumnWidth = -1;
			m_leftMostMergedColumnIndex = -1;
			m_minWidthDuringColResize = -1;
			m_mouseOffsetForColResize = 0;
			m_totalGridLineAdjDuringResize = 0;
			m_lastColumnIndex = -1;
			m_lastRowIndex = -1L;
			m_buttonWasPushed = false;
			m_mouseCapturePoint.X = -1;
			m_mouseCapturePoint.Y = -1;
			m_selectionBlockIndex = -1;
			m_dragState = EnDragOperation.None;
			DragImageOperation = null;
			m_headerDragY = -1;
			m_colIndexToDragColAfter = NoColIndexToDragColAfter;
			embContrlFocused = false;
			timeEvent = DateTime.MinValue;
			hyperlinkSelTimer?.Stop();
		}
	}
}
