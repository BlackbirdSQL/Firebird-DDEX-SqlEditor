// Microsoft.SqlServer.DlgGrid, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.DlgStorage

using System;
using System.Drawing;
using System.Globalization;
using BlackbirdSql.Core;
using BlackbirdSql.Common.Controls.Enums;
using BlackbirdSql.Common.Controls.Events;
using BlackbirdSql.Common.Controls.Interfaces;
using BlackbirdSql.Common.Model.Interfaces;

// using Microsoft.SqlServer.Management.UI.Grid;




namespace BlackbirdSql.Common.Controls.Grid;


public class DlgStorage : IDlgStorage, IGridStorage, IDisposable
{
	protected IGridMemDataStorage _GridMemStorage;

	// [CLSCompliant(false)]
	protected IStorageView _StorageView;

	// [CLSCompliant(false)]
	protected IDlgGridControl _DlgGridCtl;

	protected CustomizeCellGDIObjectsEventHandler m_OnCustCellGDIObjects;

	// [CLSCompliant(false)]
	public virtual IMemDataStorage Storage => _GridMemStorage;

	// [CLSCompliant(false)]
	public virtual IStorageView StorageView => _StorageView;

	public event FillControlWithDataEventHandler FillControlWithDataEvent;

	public event SetCellDataFromControlEventHandler SetCellDataFromControlEvent;

	// [CLSCompliant(false)]
	public DlgStorage(IDlgGridControl grid)
	{
		_GridMemStorage = new GridMemDataStorage();
		_GridMemStorage.InitStorage();
		_StorageView = _GridMemStorage.GetStorageView();
		_DlgGridCtl = grid;
		m_OnCustCellGDIObjects = OnCustCellGDIObjects;
		_DlgGridCtl.CustomizeCellGDIObjects += m_OnCustCellGDIObjects;
	}

	public void Dispose()
	{
		Dispose(bDisposing: true);
	}

	protected virtual void Dispose(bool bDisposing)
	{
		if (_StorageView != null)
		{
			_StorageView.Dispose();
			_StorageView = null;
		}
		if (_GridMemStorage != null)
		{
			_GridMemStorage = null;
		}
		if (_DlgGridCtl != null)
		{
			if (m_OnCustCellGDIObjects != null)
			{
				_DlgGridCtl.CustomizeCellGDIObjects -= m_OnCustCellGDIObjects;
			}
			_DlgGridCtl = null;
		}
	}

	public virtual long RowCount => _StorageView.RowCount;

	public virtual long EnsureRowsInBuf(long FirstRowIndex, long LastRowIndex)
	{
		return RowCount;
	}

	public virtual string GetCellDataAsString(long nRowIndex, int nColIndex)
	{
		GridCell cell = GetCell(nRowIndex, nColIndex);
		if (cell.CellData is string data)
		{
			return data;
		}
		return UIGridResources.InvalidNonStringCellType;
	}

	public virtual int IsCellEditable(long nRowIndex, int nColIndex)
	{
		return GetCell(nRowIndex, nColIndex).TextCellType;
	}

	public virtual Bitmap GetCellDataAsBitmap(long nRowIndex, int nColIndex)
	{
		GridCell cell = GetCell(nRowIndex, nColIndex);
		if (cell.CellData == null)
		{
			return null;
		}
		if (cell.CellData is Bitmap bitmap)
		{
			return bitmap;
		}
		return null;
	}

	public virtual void GetCellDataForButton(long nRowIndex, int nColIndex, out EnButtonCellState state, out Bitmap image, out string buttonLabel)
	{
		GridCell cell = GetCell(nRowIndex, nColIndex);
		if (cell.CellData is ButtonInfo buttonInfo)
		{
			state = buttonInfo.State;
			image = buttonInfo.Bmp;
			buttonLabel = buttonInfo.Label;
		}
		else
		{
			image = null;
			state = EnButtonCellState.Normal;
			buttonLabel = UIGridResources.InvalidNonButtonCellType;
		}
	}

	public virtual EnGridCheckBoxState GetCellDataForCheckBox(long nRowIndex, int nColIndex)
	{
		GridCell cell = GetCell(nRowIndex, nColIndex);
		if (cell.CellData is EnGridCheckBoxState state)
		{
			return state;
		}
		return EnGridCheckBoxState.None;
	}

	public virtual void FillControlWithData(long nRowIndex, int nColIndex, IGridEmbeddedControl control)
	{
		if (FillControlWithDataEvent != null)
		{
			FillControlWithDataEventArgs e = new FillControlWithDataEventArgs((int)nRowIndex, nColIndex, control);
			FillControlWithDataEvent(_DlgGridCtl, e);
		}
		else
		{
			string cellDataAsString = GetCellDataAsString(nRowIndex, nColIndex);
			control.AddDataAsString(cellDataAsString);
			control.SetCurSelectionAsString(cellDataAsString);
		}
	}

	public virtual bool SetCellDataFromControl(long nRowIndex, int nColIndex, IGridEmbeddedControl control)
	{
		if (SetCellDataFromControlEvent != null)
		{
			SetCellDataFromControlEventArgs setCellDataFromControlEventArgs = new SetCellDataFromControlEventArgs((int)nRowIndex, nColIndex, control);
			SetCellDataFromControlEvent(_DlgGridCtl, setCellDataFromControlEventArgs);
			if (!setCellDataFromControlEventArgs.Valid)
			{
				return false;
			}
		}
		GridCell cell = GetCell(nRowIndex, nColIndex);
		if (!(cell is not null))
		{
			Exception ex = new("Unexpected cell type");
			Diag.Dug(ex);
			throw ex;
		}
		GridCell obj = cell;
		string strA = (string)obj.CellData;
		string curSelectionAsString = control.GetCurSelectionAsString();
		if (string.Compare(strA, curSelectionAsString, ignoreCase: false, CultureInfo.InvariantCulture) != 0)
		{
			_DlgGridCtl.SetCellDirtyState((int)nRowIndex, nColIndex, bDirty: true);
		}
		obj.CellData = curSelectionAsString;
		return true;
	}

	protected GridCell GetCell(long nRow, int nCol)
	{
		try
		{
			object cellData = _StorageView.GetCellData(nRow, nCol);
			if (cellData is not GridCell)
			{
				Exception ex = new("DlgStorage.GetCell: unexpected data type");
				throw ex;
			}
			return cellData as GridCell;
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}
	}

	private void OnCustCellGDIObjects(object sender, CustomizeCellGDIObjectsEventArgs args)
	{
		GridCell cell = GetCell((int)args.RowIndex, args.ColumnIndex);
		if (cell.BkBrush != null)
		{
			args.BKBrush = cell.BkBrush;
		}
		if (cell.TextBrush != null)
		{
			args.TextBrush = cell.TextBrush;
		}
	}
}
