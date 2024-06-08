// Microsoft.SqlServer.DlgGrid, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.DlgGridControl

using System;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using BlackbirdSql.Core;
using BlackbirdSql.Shared.Ctl;
using BlackbirdSql.Shared.Enums;
using BlackbirdSql.Shared.Events;
using BlackbirdSql.Shared.Interfaces;

// using Microsoft.SqlServer.Management.UI.Grid;




namespace BlackbirdSql.Shared.Controls.Grid;


public class DlgGridControl : GridControl, IBDlgGridControl, IBGridControl
{
	private class PairOfObjects
	{
		public object object1;

		public object object2;
	}

	private delegate int GetSelectedRowIntHandler();

	private delegate void SetSelectedRowIntHandler(int rowIndex);

	private delegate int[] GetSelectedRowsIntHandler();

	private delegate void SetSelectedRowsIntHandler(int[] selectedRows);

	private delegate int GetRowsNumberIntHandler();

	private delegate IBDlgStorage GetDlgStorageIntHandler();

	private delegate void SetDlgStorageIntHandler(IBDlgStorage newStorage);

	private delegate void AddRowIntHandler(GridCellCollection row);

	private delegate void InsertRowIntHandler(int nRowIndex, GridCellCollection row);

	private delegate GridCellCollection GetRowInfoIntHandler(int rowNum);

	private delegate void SetRowInfoIntHandler(int rowNum, GridCellCollection row);

	private delegate GridCell GetCellInfoIntHandler(int rowNum, int colNum);

	private delegate void SetCellInfoIntHandler(int rowNum, int colNum, GridCell cell);

	private delegate PairOfObjects GetSelectedCellIntHandler();

	private delegate void SetSelectedCellIntHandler(int rowIndex, int colIndex);

	private delegate bool IsCellDirtyIntHandler(int rowIndex, int colIndex);

	private delegate void SetCellDirtyStateIntHandler(int rowIndex, int colIndex, bool dirty);

	private delegate bool IsRowDirtyIntHandler(int rowIndex);

	private delegate void OnDeleteRowHandler(int rowIndex);

	// [CLSCompliant(false)]
	protected IBDlgStorage m_Storage;

	public BitArrayCollection m_cellDirtyBits = [];

	private const int C_BitArrayGrowthChunk = 100;

	private int m_curBitArrayLen = 100;

	protected const string C_TName = "DlgGrid";

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int SelectedRow
	{
		get
		{
			if (InvokeRequired)
			{
				return (int)Invoke(new GetSelectedRowIntHandler(GetSelectedRowInt), new object[0]);
			}
			return GetSelectedRowInt();
		}
		set
		{
			if (InvokeRequired)
			{
				Invoke(new SetSelectedRowIntHandler(SetSelectedRowInt), value);
			}
			else
			{
				SetSelectedRowInt(value);
			}
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int[] SelectedRows
	{
		get
		{
			if (InvokeRequired)
			{
				return (int[])Invoke(new GetSelectedRowsIntHandler(GetSelectedRowsInt), new object[0]);
			}
			return GetSelectedRowsInt();
		}
		set
		{
			if (InvokeRequired)
			{
				Invoke(new SetSelectedRowsIntHandler(SetSelectedRowsInt), value);
			}
			else
			{
				SetSelectedRowsInt(value);
			}
		}
	}

	[Browsable(false)]
	public int RowCount
	{
		get
		{
			if (InvokeRequired)
			{
				return (int)Invoke(new GetRowsNumberIntHandler(GetRowCount), new object[0]);
			}
			return GetRowCount();
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	// [CLSCompliant(false)]
	public IBDlgStorage DlgStorage
	{
		get
		{
			if (InvokeRequired)
			{
				return (IBDlgStorage)Invoke(new GetDlgStorageIntHandler(GetDlgStorageInt), new object[0]);
			}
			return GetDlgStorageInt();
		}
		set
		{
			if (InvokeRequired)
			{
				Invoke(new SetDlgStorageIntHandler(SetDlgStorageInt), value);
			}
			else
			{
				SetDlgStorageInt(value);
			}
		}
	}

	[Description("Occurs when it is time to populate embedded control with data.")]
	[Category(C_GridEventsCategory)]
	public event FillControlWithDataEventHandler FillControlWithDataEvent
	{
		add
		{
			m_Storage.FillControlWithDataEvent += value;
		}
		remove
		{
			m_Storage.FillControlWithDataEvent -= value;
		}
	}

	[Description("Occurs when it is time to validate curernt contents of the embedded control.")]
	[Category(C_GridEventsCategory)]
	public event SetCellDataFromControlEventHandler SetCellDataFromControlEvent
	{
		add
		{
			m_Storage.SetCellDataFromControlEvent += value;
		}
		remove
		{
			m_Storage.SetCellDataFromControlEvent -= value;
		}
	}

	public DlgGridControl()
	{
		DlgStorage = new DlgStorage(this);
	}

	// [CLSCompliant(false)]
	public DlgGridControl(IBDlgStorage stg)
	{
		DlgStorage = stg;
	}

	public override void EndInit()
	{
		base.EndInit();
		UpdateGridIfNeeded(bRecalcRows: true);
	}

	protected override void DeleteColumnInternal(int nIndex)
	{
		if (m_Storage.Storage is IBGridMemDataStorage storage)
		{
			storage.DeleteColumn(m_Columns[nIndex].ColumnIndex);
		}
		base.DeleteColumnInternal(nIndex);
		m_cellDirtyBits.RemoveAt(nIndex);
	}

	protected override void InsertColumnInternal(int nIndex, GridColumnInfo ci)
	{
		base.InsertColumnInternal(nIndex, ci);
		m_cellDirtyBits.Insert(nIndex, new BitArray(m_curBitArrayLen));
		if (m_Storage.Storage is IBGridMemDataStorage storage)
		{
			storage.InsertColumn(nIndex, "");
		}
	}

	protected override void ResetGridInternal()
	{
		base.ResetGridInternal();
		m_cellDirtyBits.Clear();
	}

	protected override bool HandleTabOnLastOrFirstCell(bool goingLeft)
	{
		return false;
	}

	protected override void OnKeyPressedOnCell(long curRow, int curCol, Keys key, Keys mod)
	{
		if (ShouldRedirectKeyPressToMouseClick(curRow, curCol, key, mod))
		{
			if (!OnMouseButtonClicking(curRow, curCol, m_scrollMgr.GetCellRectangle(curRow, curCol), mod, MouseButtons.Left))
			{
				return;
			}
			if (!OnMouseButtonClicked(curRow, curCol, m_scrollMgr.GetCellRectangle(curRow, curCol), MouseButtons.Left))
			{
				Refresh();
				return;
			}
		}
		base.OnKeyPressedOnCell(curRow, curCol, key, mod);
	}

	public void AddRow(GridCellCollection row)
	{
		if (InvokeRequired)
		{
			Invoke(new AddRowIntHandler(AddRowInt), row);
		}
		else
		{
			AddRowInt(row);
		}
	}

	public void InsertRow(int rowIndex, GridCellCollection row)
	{
		if (InvokeRequired)
		{
			Invoke(new InsertRowIntHandler(InsertRowInt), rowIndex, row);
		}
		else
		{
			InsertRowInt(rowIndex, row);
		}
	}

	public void DeleteAllRows()
	{
		if (InvokeRequired)
		{
			Invoke(new MethodInvoker(OnDeleteAllRows));
		}
		else
		{
			OnDeleteAllRows();
		}
	}

	public void DeleteRow(int rowIndex)
	{
		if (InvokeRequired)
		{
			Invoke(new OnDeleteRowHandler(OnDeleteRow), rowIndex);
		}
		else
		{
			OnDeleteRow(rowIndex);
		}
	}

	public GridCellCollection GetRowInfo(int rowNum)
	{
		if (InvokeRequired)
		{
			return (GridCellCollection)Invoke(new GetRowInfoIntHandler(GetRowInfoInt), rowNum);
		}
		return GetRowInfoInt(rowNum);
	}

	public void SetRowInfo(int rowNum, GridCellCollection row)
	{
		if (InvokeRequired)
		{
			Invoke(new SetRowInfoIntHandler(SetRowInfoInt), rowNum, row);
		}
		else
		{
			SetRowInfoInt(rowNum, row);
		}
	}

	public GridCell GetCellInfo(int rowNum, int colNum)
	{
		if (InvokeRequired)
		{
			return (GridCell)Invoke(new GetCellInfoIntHandler(GetCellInfoInt), rowNum, colNum);
		}
		return GetCellInfoInt(rowNum, colNum);
	}

	public void SetCellInfo(int rowNum, int colNum, GridCell cell)
	{
		if (InvokeRequired)
		{
			Invoke(new SetCellInfoIntHandler(SetCellInfoInt), rowNum, colNum, cell);
		}
		else
		{
			SetCellInfoInt(rowNum, colNum, cell);
		}
	}

	public void GetSelectedCell(out int rowIndex, out int colIndex)
	{
		PairOfObjects pairOfObjects = !InvokeRequired ? GetSelectedCellInt() : Invoke(new GetSelectedCellIntHandler(GetSelectedCellInt), new object[0]) as PairOfObjects;
		rowIndex = (int)pairOfObjects.object1;
		colIndex = (int)pairOfObjects.object2;
	}

	public void SetSelectedCell(int rowIndex, int colIndex)
	{
		if (InvokeRequired)
		{
			Invoke(new SetSelectedCellIntHandler(SetSelectedCellInt), rowIndex, colIndex);
		}
		else
		{
			SetSelectedCellInt(rowIndex, colIndex);
		}
	}

	public bool IsCellDirty(int rowIndex, int colIndex)
	{
		if (InvokeRequired)
		{
			return (bool)Invoke(new IsCellDirtyIntHandler(IsCellDirtyInt), rowIndex, colIndex);
		}
		return IsCellDirtyInt(rowIndex, colIndex);
	}

	public void SetCellDirtyState(int rowIndex, int colIndex, bool dirty)
	{
		if (InvokeRequired)
		{
			Invoke(new SetCellDirtyStateIntHandler(SetCellDirtyStateInt), rowIndex, colIndex, dirty);
		}
		else
		{
			SetCellDirtyStateInt(rowIndex, colIndex, dirty);
		}
	}

	public bool IsRowDirty(int rowIndex)
	{
		if (InvokeRequired)
		{
			return (bool)Invoke(new IsRowDirtyIntHandler(IsRowDirtyInt), rowIndex);
		}
		return IsRowDirtyInt(rowIndex);
	}

	private int GetSelectedRowInt()
	{
		if (!ValidateRowsSel(bRowBLocksAllowed: false))
		{
			return -1;
		}
		BlockOfCellsCollection selectedCells = SelectedCells;
		if (selectedCells != null && selectedCells.Count > 0)
		{
			return (int)selectedCells[0].Y;
		}
		return -1;
	}

	private void SetSelectedRowInt(int rowIndex)
	{
		if (ValidateRowsSel(bRowBLocksAllowed: false))
		{
			SetSelectedCell(rowIndex, 0);
		}
	}

	private int[] GetSelectedRowsInt()
	{
		if (!ValidateRowsSel(bRowBLocksAllowed: true))
		{
			return null;
		}
		BlockOfCellsCollection selectedCells = SelectedCells;
		if (selectedCells != null && selectedCells.Count > 0)
		{
			int num = 0;
			int num2;
			for (num2 = 0; num2 < selectedCells.Count; num2++)
			{
				num += (int)selectedCells[num2].Height;
			}
			int[] array = new int[num];
			int num3 = 0;
			for (num2 = 0; num2 < selectedCells.Count; num2++)
			{
				for (int i = (int)selectedCells[num2].Y; i <= (int)selectedCells[num2].Bottom; i++)
				{
					array[num3] = i;
					num3++;
				}
			}
			return array;
		}
		return null;
	}

	private void SetSelectedRowsInt(int[] selectedRows)
	{
		if (selectedRows != null && ValidateRowsSel(bRowBLocksAllowed: true))
		{
			BlockOfCellsCollection blockOfCellsCollection = [];
			for (int i = 0; i < selectedRows.Length; i++)
			{
				BlockOfCells blockOfCells = new BlockOfCells(selectedRows[i], 0);
				blockOfCellsCollection.Add(blockOfCells);
			}
			SelectedCells = blockOfCellsCollection;
		}
	}

	private int GetRowCount()
	{
		if (m_Storage != null && m_Storage.StorageView != null)
		{
			return (int)m_Storage.StorageView.RowCount;
		}
		return 0;
	}

	private IBDlgStorage GetDlgStorageInt()
	{
		return m_Storage;
	}

	private void SetDlgStorageInt(IBDlgStorage newStorage)
	{
		if (m_Storage is not null and IDisposable)
		{
			((IDisposable)m_Storage).Dispose();
			m_Storage = null;
		}
		m_Storage = newStorage;
		GridStorage = m_Storage;
	}

	private void AddRowInt(GridCellCollection row)
	{
		IBMemDataStorage storage = m_Storage.Storage;
		object[] gridCellArrFromCol = GetGridCellArrFromCol(row);
		storage.AddRow(gridCellArrFromCol);
		CheckBitArraysLength();
		UpdateGridIfNeeded(bRecalcRows: true);
	}

	private void InsertRowInt(int nRowIndex, GridCellCollection row)
	{
		IBMemDataStorage storage = m_Storage.Storage;
		object[] gridCellArrFromCol = GetGridCellArrFromCol(row);
		storage.InsertRow(nRowIndex, gridCellArrFromCol);
		CheckBitArraysLength();
		foreach (BitArray cellDirtyBit in m_cellDirtyBits)
		{
			int i;
			for (i = RowCount - 1; i > nRowIndex; i--)
			{
				cellDirtyBit[i] = cellDirtyBit[i - 1];
			}
			cellDirtyBit[nRowIndex] = false;
		}
		UpdateGridIfNeeded(bRecalcRows: true);
	}

	private GridCellCollection GetRowInfoInt(int rowNum)
	{
		return new GridCellCollection(GetStorageRowAsGridCells(rowNum));
	}

	private void SetRowInfoInt(int rowNum, GridCellCollection row)
	{
		GridCell[] storageRowAsGridCells = GetStorageRowAsGridCells(rowNum);
		if (storageRowAsGridCells.Length != row.Count)
		{
			ArgumentException ex = new("row");
			Diag.Dug(ex);
			throw ex;
		}
		for (int i = 0; i < storageRowAsGridCells.Length; i++)
		{
			storageRowAsGridCells[i].Assign(row[i]);
		}
		UpdateGridIfNeeded(bRecalcRows: false);
	}

	private GridCell GetCellInfoInt(int rowNum, int colNum)
	{
		return GetStorageRowAsGridCells(rowNum)[colNum];
	}

	private void SetCellInfoInt(int rowNum, int colNum, GridCell cell)
	{
		GetCellInfo(rowNum, colNum).Assign(cell);
		UpdateGridIfNeeded(bRecalcRows: false);
	}

	private PairOfObjects GetSelectedCellInt()
	{
		int num = -1;
		int num2 = -1;
		BlockOfCellsCollection selectedCells = SelectedCells;
		if (selectedCells != null && selectedCells.Count > 0)
		{
			num = (int)selectedCells[0].Y;
			num2 = selectedCells[0].X;
			if (SelectionType == EnGridSelectionType.CellBlocks || SelectionType == EnGridSelectionType.ColumnBlocks)
			{
				num2 = m_Columns[num2].ColumnIndex;
			}
		}
		return new PairOfObjects
		{
			object1 = num,
			object2 = num2
		};
	}

	private void SetSelectedCellInt(int rowIndex, int colIndex)
	{
		if (SelectionType == EnGridSelectionType.CellBlocks || SelectionType == EnGridSelectionType.ColumnBlocks)
		{
			colIndex = GetUIColumnIndexByStorageIndex(colIndex);
		}
		BlockOfCells node = new BlockOfCells(rowIndex, colIndex);
		BlockOfCellsCollection blockOfCellsCollection =
		[
			node
		];
		SelectedCells = blockOfCellsCollection;
	}

	private bool IsCellDirtyInt(int rowIndex, int colIndex)
	{
		return m_cellDirtyBits[colIndex][rowIndex];
	}

	private void SetCellDirtyStateInt(int rowIndex, int colIndex, bool dirty)
	{
		m_cellDirtyBits[colIndex][rowIndex] = dirty;
	}

	private bool IsRowDirtyInt(int rowIndex)
	{
		foreach (BitArray cellDirtyBit in m_cellDirtyBits)
		{
			if (cellDirtyBit[rowIndex])
			{
				return true;
			}
		}
		return false;
	}

	protected GridCell[] GetStorageRowAsGridCells(int nRow)
	{
		object[] row = m_Storage.Storage.GetRow(nRow);
		if (row is not GridCell[])
		{
			return null;
		}
		return (GridCell[])row;
	}

	protected virtual bool ShouldRedirectKeyPressToMouseClick(long curRow, int curCol, Keys key, Keys mod)
	{
		if (key != Keys.Space)
		{
			return false;
		}
		int columnType = m_Columns[curCol].ColumnType;
		if (columnType != 2 && columnType != 4)
		{
			return false;
		}
		if (columnType == 2)
		{
			EnButtonCellState buttonCellState = GetButtonCellState(curRow, curCol);
			if (buttonCellState == EnButtonCellState.Empty || buttonCellState == EnButtonCellState.Disabled)
			{
				return false;
			}
		}
		return true;
	}

	protected virtual void OnDeleteAllRows()
	{
		BeginInit();
		for (int num = RowCount - 1; num >= 0; num--)
		{
			DeleteRow(num);
		}
		SelectedCells = null;
		EndInit();
		UpdateGridIfNeeded(bRecalcRows: true);
	}

	protected virtual void OnDeleteRow(int rowIndex)
	{
		m_Storage.Storage.DeleteRow(rowIndex);
		int rowCount = RowCount;
		foreach (BitArray cellDirtyBit in m_cellDirtyBits)
		{
			int i;
			for (i = rowIndex; i < rowCount; i++)
			{
				cellDirtyBit[i] = cellDirtyBit[i + 1];
			}
		}
		if (SelectionType == EnGridSelectionType.SingleRow && SelectedRow >= RowCount)
		{
			SelectedCells = null;
		}
		UpdateGridIfNeeded(bRecalcRows: true);
	}

	protected void UpdateGridIfNeeded(bool bRecalcRows)
	{
		if (!IsInitializing)
		{
			UpdateGrid(bRecalcRows);
		}
	}

	protected bool ValidateRowsSel(bool bRowBLocksAllowed)
	{
		if (SelectionType == EnGridSelectionType.SingleRow || bRowBLocksAllowed && SelectionType == EnGridSelectionType.RowBlocks)
		{
			return true;
		}
		return false;
	}

	protected GridCell[] GetGridCellArrFromCol(GridCellCollection row)
	{
		if (row == null)
		{
			ArgumentNullException ex = new("row");
			Diag.Dug(ex);
			throw ex;
		}
		GridCell[] array = new GridCell[row.Count];
		row.CopyTo(array, 0);
		return array;
	}

	protected void CheckBitArraysLength()
	{
		if (m_curBitArrayLen >= RowCount)
		{
			return;
		}
		int curBitArrayLen = m_curBitArrayLen;
		m_curBitArrayLen += C_BitArrayGrowthChunk;
		foreach (BitArray cellDirtyBit in m_cellDirtyBits)
		{
			cellDirtyBit.Length = m_curBitArrayLen;
			while (curBitArrayLen < m_curBitArrayLen)
			{
				cellDirtyBit[curBitArrayLen++] = false;
			}
		}
	}
}
