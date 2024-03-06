// Microsoft.SqlServer.DataStorage, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.MemStorageView

using System;
using System.Globalization;
using BlackbirdSql.Common.Controls.Grid;
using BlackbirdSql.Common.Model.Interfaces;
using BlackbirdSql.Core;
using BlackbirdSql.Common.Properties;
using BlackbirdSql.Common.Ctl.Interfaces;

namespace BlackbirdSql.Common.Model.IO;


public class MemStorageView : AbstractStorageView
{
	protected IBMemDataStorage _MemStorage;

	public MemStorageView()
	{
	}

	public MemStorageView(IBMemDataStorage storage)
	{
		_MemStorage = storage;
	}

	public override long EnsureRowsInBuf(long startRow, long totalRowCount)
	{
		return RowCount;
	}

	public override long RowCount => _MemStorage.RowCount;


	public override int ColumnCount => _MemStorage.ColumnCount;


	public override IBColumnInfo GetColumnInfo(int iCol)
	{
		return _MemStorage.GetColumnInfo(iCol);
	}

	public override object GetCellData(long i64Row, int iCol)
	{
		if (i64Row > int.MaxValue)
		{
			Exception ex = new(string.Format(CultureInfo.CurrentCulture, ControlsResources.MemoryBasedStorageIsLimitedToNRows, int.MaxValue));
			Diag.Dug(ex);
			throw ex;
		}
		int iRow = Convert.ToInt32(i64Row);
		return _MemStorage.GetRow(iRow)[iCol];
	}

	public override void DeleteRow(long iRow)
	{
		_MemStorage.DeleteRow(Convert.ToInt32(iRow));
	}

	public override bool IsStorageClosed()
	{
		return _MemStorage.IsClosed();
	}
}
