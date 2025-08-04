// Microsoft.SqlServer.DlgGrid, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.DlgStorage

using System;
using System.Drawing;
using System.Globalization;
using BlackbirdSql.Shared.Enums;
using BlackbirdSql.Shared.Events;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Shared.Model.IO;
using BlackbirdSql.Shared.Properties;


namespace BlackbirdSql.Shared.Controls.Grid;


internal class DlgStorage : IBsDlgStorage, IBsGridStorage, IDisposable
{
	protected IBsGridMemDataStorage _GridMemStorage;

	// [CLSCompliant(false)]
	protected IBsStorageView _StorageView;

	// [CLSCompliant(false)]
	protected IBsDlgGridControl _DlgGridCtl;

	protected CustomizeCellGDIObjectsEventHandler _OnCustCellGDIObjectsHandler;

	// [CLSCompliant(false)]
	public virtual IBsMemDataStorage Storage => _GridMemStorage;

	// [CLSCompliant(false)]
	public virtual IBsStorageView StorageView => _StorageView;

	public event FillControlWithDataEventHandler FillControlWithDataEvent;

	public event SetCellDataFromControlEventHandler SetCellDataFromControlEvent;

	// [CLSCompliant(false)]
	public DlgStorage(IBsDlgGridControl grid)
	{
		_GridMemStorage = new GridMemDataStorage();
		_GridMemStorage.InitStorage();
		_StorageView = _GridMemStorage.GetStorageView();
		_DlgGridCtl = grid;
		_OnCustCellGDIObjectsHandler = OnCustCellGDIObjects;
		_DlgGridCtl.CustomizeCellGDIObjectsEvent += _OnCustCellGDIObjectsHandler;
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
			if (_OnCustCellGDIObjectsHandler != null)
			{
				_DlgGridCtl.CustomizeCellGDIObjectsEvent -= _OnCustCellGDIObjectsHandler;
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
		return ControlsResources.Grid_InvalidNonStringCellType;
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
			buttonLabel = ControlsResources.Grid_InvalidNonButtonCellType;
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

	public virtual void FillControlWithData(long nRowIndex, int nColIndex, IBsGridEmbeddedControl control)
	{
		if (FillControlWithDataEvent != null)
		{
			FillControlWithDataEvent(_DlgGridCtl, new((int)nRowIndex, nColIndex, control));
			return;
		}

		string cellDataAsString = GetCellDataAsString(nRowIndex, nColIndex);
		control.AddDataAsString(cellDataAsString);
		control.SetCurSelectionAsString(cellDataAsString);
	}

	public virtual bool SetCellDataFromControl(long nRowIndex, int nColIndex, IBsGridEmbeddedControl control)
	{
		if (SetCellDataFromControlEvent != null)
		{
			SetCellDataFromControlEventArgs args = new ((int)nRowIndex, nColIndex, control);
			SetCellDataFromControlEvent(_DlgGridCtl, args);

			if (!args.Valid)
				return false;
		}

		GridCell cell = GetCell(nRowIndex, nColIndex);
		if (!(cell is not null))
		{
			Exception ex = new("Unexpected cell type");
			Diag.Ex(ex);
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
			Diag.Ex(ex);
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
