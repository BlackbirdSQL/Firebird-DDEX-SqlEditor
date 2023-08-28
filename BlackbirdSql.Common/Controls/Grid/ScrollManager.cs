#region Assembly Microsoft.SqlServer.GridControl, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLCommon\Microsoft.SqlServer.GridControl.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Drawing;
using BlackbirdSql.Common.Properties;
using BlackbirdSql.Core;


namespace BlackbirdSql.Common.Controls.Grid;

public sealed class ScrollManager
{
	private IntPtr m_hWnd = IntPtr.Zero;

	private GridColumnCollection m_Columns;

	private Rectangle m_SARect;

	private Rectangle m_cachedSA;

	private long m_firstRowIndex;

	private int m_firstRowPos;

	private long m_lastRowIndex;

	private int m_firstColIndex;

	private int m_firstColPos;

	private int m_lastColIndex;

	private long m_cRowsNum;

	private int m_nCellHeight;

	private int m_totalGridWidth;

	private const int C_HorizScrollUnit = 1;

	private int m_horizScrollUnitForArrows = C_HorizScrollUnit;

	private int m_firstScrollableColIndex;

	private uint m_firstScrollableRowIndex;

	public static readonly int GRID_LINE_WIDTH = 1;

	public long RowCount
	{
		get
		{
			return m_cRowsNum;
		}
		set
		{
			if (value < 0)
			{
				ArgumentException ex = new(ControlsResources.NumOfRowsShouldBeGreaterThanZero);
				Diag.Dug(ex);
				throw ex;
			}

			if (m_cRowsNum != value)
			{
				m_cRowsNum = value;
				_ = m_lastRowIndex;
				long maxFirstRowIndex = GetMaxFirstRowIndex(CalcVertPageSize());
				if (m_firstRowIndex > maxFirstRowIndex)
				{
					m_firstRowIndex = Math.Max(m_firstScrollableRowIndex, maxFirstRowIndex);
				}

				CalcLastRowIndex();
				ProcessVertChange();
			}
		}
	}

	public int CellHeight
	{
		get
		{
			return m_nCellHeight;
		}
		set
		{
			if (value <= 0)
			{
				ArgumentException ex = new(ControlsResources.CellHeightShouldBeGreaterThanZero);
				Diag.Dug(ex);
				throw ex;
			}

			m_nCellHeight = value;
		}
	}

	public int FirstScrollableColumnIndex
	{
		get
		{
			return m_firstScrollableColIndex;
		}
		set
		{
			if (value < 0)
			{
				ArgumentException ex = new(ControlsResources.FirstScrollableColumnShouldBeValid);
				Diag.Dug(ex);
				throw ex;
			}

			if (m_firstScrollableColIndex == value)
			{
				return;
			}

			m_firstScrollableColIndex = value;
			if (m_hWnd != IntPtr.Zero)
			{
				Native.SCROLLINFO sCROLLINFO = new(bInitWithAllMask: true);
				if (SafeNative.GetScrollInfo(m_hWnd, 0, sCROLLINFO))
				{
					CalcFirstColumnIndexAndPosFromScrollPos(sCROLLINFO.nPos);
				}
			}
		}
	}

	// [CLSCompliant(false)]
	public uint FirstScrollableRowIndex
	{
		get
		{
			return m_firstScrollableRowIndex;
		}
		set
		{
			if (m_firstScrollableRowIndex == value)
			{
				return;
			}

			m_firstScrollableRowIndex = value;
			m_firstRowIndex = m_firstScrollableRowIndex;
			if (!(m_hWnd == IntPtr.Zero))
			{
				Native.SCROLLINFO sCROLLINFO = new(bInitWithAllMask: true);
				if (SafeNative.GetScrollInfo(m_hWnd, 1, sCROLLINFO))
				{
					CalcFirstRowIndexAndPosFromScrollPos(sCROLLINFO.nPos);
				}
			}
		}
	}

	public long FirstRowIndex => m_firstRowIndex;

	public long LastRowIndex => m_lastRowIndex;

	public int FirstRowPos => m_firstRowPos;

	public int FirstColumnIndex => m_firstColIndex;

	public int LastColumnIndex => m_lastColIndex;

	public int FirstColumnPos => m_firstColPos;

	public ScrollManager()
	{
		m_SARect = Rectangle.Empty;
		m_cachedSA = Rectangle.Empty;
		Reset();
	}

	public void Reset()
	{
		m_firstRowIndex = 0L;
		m_firstRowPos = 0;
		m_lastRowIndex = 0L;
		m_firstColIndex = 0;
		m_firstColPos = 0;
		m_lastColIndex = 0;
		m_cRowsNum = 0L;
		m_totalGridWidth = GRID_LINE_WIDTH;
		m_firstScrollableColIndex = 0;
		m_firstScrollableRowIndex = 0u;
		ResetScrollBars();
	}

	public void SetColumns(GridColumnCollection columns)
	{
		if (m_Columns != columns)
		{
			m_Columns = columns;
		}
	}

	public void OnSAChange(Rectangle newSA)
	{
		if (m_hWnd == IntPtr.Zero)
		{
			return;
		}

		m_cachedSA = m_SARect;
		m_SARect = newSA;
		m_firstRowPos = m_SARect.Top;
		if (m_SARect.Height != m_cachedSA.Height)
		{
			ProcessVertChange();
			CalcLastRowIndex();
			SafeNative.InvalidateRect(m_hWnd, null, erase: true);
		}

		if (m_SARect.Width != m_cachedSA.Width)
		{
			if (m_Columns.Count > 0)
			{
				int nOldScrollPos = 0;
				int nPos = ProcessHorizChange(ref nOldScrollPos);
				CalcFirstColumnIndexAndPosFromScrollPos(nPos);
				CalcLastColumnIndex();
			}

			SafeNative.InvalidateRect(m_hWnd, null, erase: true);
		}
	}

	public void OnSAChange(int nLeft, int nRight, int nTop, int nBottom)
	{
		Rectangle newSA = new Rectangle(nLeft, nTop, nRight - nLeft, nBottom - nTop);
		OnSAChange(newSA);
	}

	public void RecalcAll(Rectangle scrollableArea)
	{
		if (m_Columns != null && m_Columns.Count != 0)
		{
			m_SARect = scrollableArea;
			m_totalGridWidth = GRID_LINE_WIDTH;
			int i = m_firstScrollableColIndex;
			for (int count = m_Columns.Count; i < count; i++)
			{
				m_totalGridWidth += m_Columns[i].WidthInPixels + GRID_LINE_WIDTH;
			}

			int nPos = ProcessHorizChange();
			CalcFirstColumnIndexAndPosFromScrollPos(nPos);
			CalcLastColumnIndex();
			m_firstRowPos = m_SARect.Top;
			ProcessVertChange();
			CalcLastRowIndex();
		}
	}

	public void ProcessNewCol(int nIndex)
	{
		CalcLastColumnIndex();
		m_totalGridWidth += m_Columns[nIndex].WidthInPixels + GRID_LINE_WIDTH;
		ProcessHorizChange();
	}

	public void ProcessDeleteCol(int nIndex, int nWidth)
	{
		_ = nIndex;
		m_totalGridWidth -= nWidth + GRID_LINE_WIDTH;
		if (m_lastColIndex >= m_Columns.Count)
		{
			m_lastColIndex = m_Columns.Count - C_HorizScrollUnit;
		}

		ProcessHorizChange();
		RecalcAll(m_SARect);
	}

	public bool EnsureRowIsVisbleWithoutClientRedraw(long nRowIndex, bool bMakeRowTheTopOne, out int yDelta)
	{
		yDelta = 0;
		if (nRowIndex < m_firstScrollableRowIndex)
		{
			return false;
		}

		if (nRowIndex < m_firstRowIndex || nRowIndex > m_lastRowIndex || nRowIndex == m_lastRowIndex && !bMakeRowTheTopOne)
		{
			Native.SCROLLINFO sCROLLINFO = new(bInitWithAllMask: true);
			SafeNative.GetScrollInfo(m_hWnd, 1, sCROLLINFO);
			long firstRowIndex = m_firstRowIndex;
			if (bMakeRowTheTopOne)
			{
				m_firstRowIndex = Math.Max(nRowIndex, m_firstScrollableRowIndex);
			}
			else if (sCROLLINFO.nPage > 0)
			{
				m_firstRowIndex = Math.Max(nRowIndex - sCROLLINFO.nPage + C_HorizScrollUnit, m_firstScrollableRowIndex);
			}
			else
			{
				m_firstRowIndex = Math.Max(nRowIndex, m_firstScrollableRowIndex);
			}

			m_firstRowIndex = Math.Min(m_firstRowIndex, GetMaxFirstRowIndex(sCROLLINFO.nPage));
			CalcLastRowIndex();
			sCROLLINFO.nPos = (int)(m_firstRowIndex - m_firstScrollableRowIndex);
			sCROLLINFO.fMask = 4;
			Native.SetScrollInfo(m_hWnd, 1, sCROLLINFO, redraw: true);
			yDelta = (int)(firstRowIndex - m_firstRowIndex) * (m_nCellHeight + GRID_LINE_WIDTH);
			return true;
		}

		return false;
	}

	public void EnsureRowIsVisible(long nRowIndex, bool bMakeRowTheTopOne)
	{
		if (EnsureRowIsVisbleWithoutClientRedraw(nRowIndex, bMakeRowTheTopOne, out var yDelta))
		{
			Native.RECT rectScrollRegion = Native.RECT.FromXYWH(0, m_SARect.Y, m_SARect.Right, m_SARect.Height);
			SafeNative.ScrollWindow(m_hWnd, 0, yDelta, ref rectScrollRegion, ref rectScrollRegion);
		}
	}

	public void EnsureColumnIsVisible(int nColIndex, bool bMakeFirstFullyVisible, bool bRedraw)
	{
		if (nColIndex < m_firstColIndex && nColIndex >= m_firstScrollableColIndex || nColIndex > m_lastColIndex || bMakeFirstFullyVisible && nColIndex == m_firstColIndex && m_firstColPos < m_SARect.X)
		{
			int num = 0;
			for (int i = m_firstScrollableColIndex; i < nColIndex; i++)
			{
				num += m_Columns[i].WidthInPixels + GRID_LINE_WIDTH;
			}

			if (bRedraw)
			{
				AdjustGridByHorizScrollBarPos(num / C_HorizScrollUnit);
			}
			else
			{
				AdjustGridByHorizScrollBarPosWithoutClientRedraw(num / 1, out var _);
			}
		}
	}

	public void EnsureCellIsVisible(long nRowIndex, int nColIndex, bool bMakeFirstColFullyVisible, bool bRedraw)
	{
		if (bRedraw)
		{
			EnsureRowIsVisible(nRowIndex, bMakeRowTheTopOne: true);
		}
		else
		{
			EnsureRowIsVisbleWithoutClientRedraw(nRowIndex, bMakeRowTheTopOne: true, out var _);
		}

		EnsureColumnIsVisible(nColIndex, bMakeFirstColFullyVisible, bRedraw);
	}

	public void EnsureCellIsVisible(long nRowIndex, int nColIndex)
	{
		EnsureCellIsVisible(nRowIndex, nColIndex, bMakeFirstColFullyVisible: true, bRedraw: true);
	}

	public void MakeNextColumnVisible(bool bRedraw)
	{
		if (m_lastColIndex < m_Columns.Count - 1)
		{
			Native.SCROLLINFO sCROLLINFO = new(bInitWithAllMask: true);
			SafeNative.GetScrollInfo(m_hWnd, 0, sCROLLINFO);
			int num = GRID_LINE_WIDTH + m_Columns[m_firstColIndex].WidthInPixels - Math.Abs(m_SARect.X + GRID_LINE_WIDTH - m_firstColPos);
			for (int i = m_firstColIndex + C_HorizScrollUnit; i <= m_lastColIndex; i++)
			{
				num += GRID_LINE_WIDTH + m_Columns[i].WidthInPixels;
			}

			int num2 = (int)Math.Floor((double)((num - m_SARect.Width + 2 * GRID_LINE_WIDTH + m_Columns[m_lastColIndex + 1].WidthInPixels) / 1));
			if (bRedraw)
			{
				AdjustGridByHorizScrollBarPos(sCROLLINFO.nPos + num2);
			}
			else
			{
				AdjustGridByHorizScrollBarPosWithoutClientRedraw(sCROLLINFO.nPos + num2, out var _);
			}
		}
	}

	public Rectangle GetCellRectangle(long nRowIndex, int nColIndex)
	{
		if (nRowIndex >= m_firstRowIndex && nRowIndex <= m_lastRowIndex || nRowIndex >= 0 && nRowIndex < FirstScrollableRowIndex)
		{
			int num = CellHeight + GRID_LINE_WIDTH;
			Rectangle result = new Rectangle(0, m_firstRowPos + (int)(nRowIndex - m_firstRowIndex) * num, 0, num)
			{
				X = GRID_LINE_WIDTH
			};
			int num2;
			int num3;
			if (nColIndex >= 0 && nColIndex < m_firstScrollableColIndex)
			{
				for (num2 = 0; num2 < FirstScrollableColumnIndex; num2++)
				{
					result.X -= GRID_LINE_WIDTH;
					num3 = m_Columns[num2].WidthInPixels;
					result.Width = num3 + GRID_LINE_WIDTH;
					if (num2 == nColIndex)
					{
						return result;
					}

					result.X += num3 + 2 * GRID_LINE_WIDTH;
				}
			}

			if (nColIndex >= m_firstColIndex && nColIndex <= m_lastColIndex)
			{
				result.X = m_firstColPos + GRID_LINE_WIDTH;
				for (num2 = m_firstColIndex; num2 <= m_lastColIndex; num2++)
				{
					result.X -= GRID_LINE_WIDTH;
					num3 = m_Columns[num2].WidthInPixels;
					result.Width = num3 + GRID_LINE_WIDTH;
					if (num2 == nColIndex)
					{
						return result;
					}

					result.X += num3 + 2 * GRID_LINE_WIDTH;
				}
			}
		}

		return Rectangle.Empty;
	}

	public void UpdateColWidth(int nIndex, int nOldWidth, int nNewWidth, bool bFinalUpdate)
	{
		_ = nIndex;
		m_totalGridWidth -= nOldWidth;
		m_totalGridWidth += nNewWidth;
		int nPos = ProcessHorizChange();
		if (bFinalUpdate)
		{
			CalcFirstColumnIndexAndPosFromScrollPos(nPos);
		}

		CalcLastColumnIndex();
	}

	public void HandleVScroll(int nScrollRequest)
	{
		int yDelta = -1;
		Native.RECT scrollRect = new(0, 0, 0, 0);
		if (HandleVScrollWithoutClientRedraw(nScrollRequest, ref yDelta, ref scrollRect))
		{
			SafeNative.ScrollWindow(m_hWnd, 0, yDelta, ref scrollRect, ref scrollRect);
		}
	}

	public bool HandleVScrollWithoutClientRedraw(int nScrollRequest, ref int yDelta, ref Native.RECT scrollRect)
	{
		bool result = false;
		Native.SCROLLINFO sCROLLINFO = new(bInitWithAllMask: true);
		SafeNative.GetScrollInfo(m_hWnd, 1, sCROLLINFO);
		long firstRowIndex = m_firstRowIndex;
		bool flag = true;
		switch (nScrollRequest)
		{
			case 6:
				m_firstRowIndex = m_firstScrollableRowIndex;
				sCROLLINFO.nPos = sCROLLINFO.nMin;
				flag = false;
				break;
			case 7:
				m_firstRowIndex = m_cRowsNum - m_firstScrollableRowIndex - Math.Max(sCROLLINFO.nPage - 1, 0);
				sCROLLINFO.nPos = sCROLLINFO.nMax - (int)m_firstScrollableRowIndex - Math.Max(sCROLLINFO.nPage - 1, 0);
				flag = false;
				break;
			case 0:
				m_firstRowIndex--;
				break;
			case 1:
				m_firstRowIndex++;
				break;
			case 2:
				m_firstRowIndex -= sCROLLINFO.nPage;
				break;
			case 3:
				m_firstRowIndex += sCROLLINFO.nPage;
				break;
			case 5:
				sCROLLINFO.nPos = sCROLLINFO.nTrackPos;
				m_firstRowIndex = FirstRowIndexFromThumbTrack(sCROLLINFO.nTrackPos);
				flag = false;
				break;
		}

		m_firstRowIndex = Math.Max(m_firstRowIndex, m_firstScrollableRowIndex);
		m_firstRowIndex = Math.Min(m_firstRowIndex, GetMaxFirstRowIndex(sCROLLINFO.nPage));
		if (flag)
		{
			sCROLLINFO.nPos += (int)(m_firstRowIndex - firstRowIndex);
		}

		sCROLLINFO.fMask = 4;
		Native.SetScrollInfo(m_hWnd, 1, sCROLLINFO, redraw: true);
		if (m_firstRowIndex != firstRowIndex)
		{
			result = true;
			CalcLastRowIndex();
			scrollRect = Native.RECT.FromXYWH(0, m_SARect.Y, m_SARect.Right, m_SARect.Height);
			yDelta = (int)(firstRowIndex - m_firstRowIndex) * (m_nCellHeight + GRID_LINE_WIDTH);
		}

		return result;
	}

	public void HandleHScroll(int nScrollRequest)
	{
		int xDelta = -1;
		Native.RECT scrollRect = new(0, 0, 0, 0);
		if (HandleHScrollWithoutClientRedraw(nScrollRequest, ref xDelta, ref scrollRect))
		{
			SafeNative.ScrollWindow(m_hWnd, xDelta, 0, ref scrollRect, ref scrollRect);
		}
	}

	public void SetGridWindowHandle(IntPtr handle)
	{
		if (handle != m_hWnd)
		{
			m_hWnd = handle;
			ResetScrollBars();
		}
	}

	public void SetHorizontalScrollUnitForArrows(int numUnits)
	{
		if (numUnits <= 0)
		{
			m_horizScrollUnitForArrows = 1;
		}
		else
		{
			m_horizScrollUnitForArrows = numUnits;
		}
	}

	public bool HandleHScrollWithoutClientRedraw(int nScrollRequest, ref int xDelta, ref Native.RECT scrollRect)
	{
		bool result = false;
		Native.SCROLLINFO sCROLLINFO = new(bInitWithAllMask: true);
		SafeNative.GetScrollInfo(m_hWnd, 0, sCROLLINFO);
		int nPos = sCROLLINFO.nPos;
		switch (nScrollRequest)
		{
			case 0:
				sCROLLINFO.nPos -= m_horizScrollUnitForArrows / 1;
				break;
			case 1:
				sCROLLINFO.nPos += m_horizScrollUnitForArrows / 1;
				break;
			case 2:
				sCROLLINFO.nPos -= sCROLLINFO.nPage;
				break;
			case 3:
				sCROLLINFO.nPos += sCROLLINFO.nPage;
				break;
			case 5:
				sCROLLINFO.nPos = sCROLLINFO.nTrackPos;
				break;
			case 6:
				sCROLLINFO.nPos = sCROLLINFO.nMin;
				break;
			case 7:
				sCROLLINFO.nPos = sCROLLINFO.nMax - Math.Max(sCROLLINFO.nPage - 1, 0);
				break;
		}

		sCROLLINFO.fMask = 4;
		Native.SetScrollInfo(m_hWnd, 0, sCROLLINFO, redraw: true);
		SafeNative.GetScrollInfo(m_hWnd, 0, sCROLLINFO);
		if (sCROLLINFO.nPos != nPos)
		{
			result = true;
			CalcFirstColumnIndexAndPosFromScrollPos(sCROLLINFO.nPos);
			CalcLastColumnIndex();
			scrollRect = Native.RECT.FromXYWH(m_SARect.X, 0, m_SARect.Width, m_SARect.Height + m_SARect.Y);
			xDelta = nPos - sCROLLINFO.nPos;
		}

		return result;
	}

	private long ProcessVertChange()
	{
		Native.SCROLLINFO sCROLLINFO = new(bInitWithAllMask: true);
		SafeNative.GetScrollInfo(m_hWnd, 1, sCROLLINFO);
		sCROLLINFO.nPage = CalcVertPageSize();
		sCROLLINFO.nMin = 0;
		sCROLLINFO.nMax = (int)Math.Max(m_cRowsNum - 1 - m_firstScrollableRowIndex, 0L);
		long maxFirstRowIndex = GetMaxFirstRowIndex(sCROLLINFO.nPage);
		if (m_firstRowIndex > maxFirstRowIndex)
		{
			m_firstRowIndex = Math.Max(m_firstScrollableRowIndex, maxFirstRowIndex);
		}

		sCROLLINFO.nPos = (int)(m_firstRowIndex - m_firstScrollableRowIndex);
		Native.SetScrollInfo(m_hWnd, 1, sCROLLINFO, redraw: true);
		return sCROLLINFO.nPos;
	}

	private int ProcessHorizChange(ref int nOldScrollPos)
	{
		Native.SCROLLINFO sCROLLINFO = new(bInitWithAllMask: true);
		SafeNative.GetScrollInfo(m_hWnd, 0, sCROLLINFO);
		nOldScrollPos = sCROLLINFO.nPos;
		sCROLLINFO.nMin = 0;
		sCROLLINFO.nMax = m_totalGridWidth / 1 - 1;
		sCROLLINFO.nPage = CalcHorizPageSize();
		return Native.SetScrollInfo(m_hWnd, 0, sCROLLINFO, redraw: true);
	}

	private int ProcessHorizChange()
	{
		int nOldScrollPos = 0;
		return ProcessHorizChange(ref nOldScrollPos);
	}

	private void CalcLastColumnIndex()
	{
		m_lastColIndex = m_firstColIndex;
		int count = m_Columns.Count;
		if (count != 0 && m_SARect.Right > 0)
		{
			int num = m_firstColPos + GRID_LINE_WIDTH;
			while (m_lastColIndex < count && num <= m_SARect.Right)
			{
				num += m_Columns[m_lastColIndex].WidthInPixels + GRID_LINE_WIDTH;
				m_lastColIndex++;
			}

			if (m_lastColIndex > m_firstColIndex)
			{
				m_lastColIndex--;
			}
		}
	}

	private void CalcLastRowIndex()
	{
		m_lastRowIndex = m_firstRowIndex;
		if (m_cRowsNum != 0L && m_SARect.Bottom > 0)
		{
			int num = m_firstRowPos + GRID_LINE_WIDTH;
			while (m_lastRowIndex < m_cRowsNum && num <= m_SARect.Bottom)
			{
				num += m_nCellHeight + GRID_LINE_WIDTH;
				m_lastRowIndex++;
			}

			if (m_lastRowIndex > 0)
			{
				m_lastRowIndex--;
			}
		}
	}

	private void CalcFirstColumnIndexAndPosFromScrollPos(int nPos)
	{
		int count = m_Columns.Count;
		if (m_firstScrollableColIndex < count)
		{
			int num = m_SARect.Left + nPos;
			int num2 = m_SARect.Left + GRID_LINE_WIDTH;
			m_firstColIndex = m_firstScrollableColIndex;
			int num3 = num2;
			while (m_firstColIndex < count && num2 < num)
			{
				num3 = num2;
				num2 += m_Columns[m_firstColIndex].WidthInPixels + GRID_LINE_WIDTH;
				m_firstColIndex++;
			}

			if (m_firstColIndex > m_firstScrollableColIndex)
			{
				m_firstColIndex--;
			}

			m_firstColPos = m_SARect.Left + num3 - num;
		}
	}

	private void CalcFirstRowIndexAndPosFromScrollPos(long nPos)
	{
		if (nPos < 0)
		{
			ArgumentException ex = new(ControlsResources.ScrollPosShouldBeMoreOrEqualZero, "nPos");
			Diag.Dug(ex);
			throw ex;
		}

		m_firstRowIndex = nPos + m_firstScrollableRowIndex;
		m_firstRowPos = m_SARect.Top + (int)nPos * (m_nCellHeight + GRID_LINE_WIDTH);
	}

	public int CalcVertPageSize(Rectangle ScrollableAreaRect)
	{
		if (ScrollableAreaRect.Height <= 0)
		{
			return -1;
		}

		return (ScrollableAreaRect.Height - GRID_LINE_WIDTH) / (m_nCellHeight + GRID_LINE_WIDTH);
	}

	private int CalcVertPageSize()
	{
		return CalcVertPageSize(m_SARect);
	}

	private int CalcHorizPageSize()
	{
		if (m_SARect.Width <= 0)
		{
			return -1;
		}

		return m_SARect.Width / 1;
	}

	private long FirstRowIndexFromThumbTrack(int nThumbPos)
	{
		return nThumbPos + m_firstScrollableRowIndex;
	}

	private void ResetScrollBars()
	{
		Native.SCROLLINFO sCROLLINFO = new(bInitWithAllMask: true);
		sCROLLINFO.nPage = sCROLLINFO.nMin = sCROLLINFO.nMax = 0;
		Native.SetScrollInfo(m_hWnd, 1, sCROLLINFO, redraw: true);
		Native.SetScrollInfo(m_hWnd, 0, sCROLLINFO, redraw: true);
	}

	private void AdjustGridByHorizScrollBarPosWithoutClientRedraw(int nHorizScrollPos, out int xDelta)
	{
		Native.SCROLLINFO sCROLLINFO = new(bInitWithAllMask: true);
		SafeNative.GetScrollInfo(m_hWnd, 0, sCROLLINFO);
		int nPos = sCROLLINFO.nPos;
		sCROLLINFO.nPos = nHorizScrollPos;
		sCROLLINFO.fMask = 4;
		Native.SetScrollInfo(m_hWnd, 0, sCROLLINFO, redraw: true);
		SafeNative.GetScrollInfo(m_hWnd, 0, sCROLLINFO);
		CalcFirstColumnIndexAndPosFromScrollPos(sCROLLINFO.nPos);
		CalcLastColumnIndex();
		xDelta = nPos - sCROLLINFO.nPos;
	}

	private void AdjustGridByHorizScrollBarPos(int nHorizScrollPos)
	{
		AdjustGridByHorizScrollBarPosWithoutClientRedraw(nHorizScrollPos, out var xDelta);
		Native.RECT rectScrollRegion = Native.RECT.FromXYWH(m_SARect.X, 0, m_SARect.Width, m_SARect.Height + m_SARect.Y);
		SafeNative.ScrollWindow(m_hWnd, xDelta, 0, ref rectScrollRegion, ref rectScrollRegion);
	}

	private long GetMaxFirstRowIndex(int pageSize)
	{
		if (pageSize > 0)
		{
			return m_cRowsNum - pageSize;
		}

		return m_cRowsNum - 1;
	}
}
