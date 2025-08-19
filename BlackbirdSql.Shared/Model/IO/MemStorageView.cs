// Microsoft.SqlServer.DataStorage, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.MemStorageView

using System;
using System.Globalization;
using BlackbirdSql.Shared.Controls.Grid;
using BlackbirdSql.Core;
using BlackbirdSql.Shared.Properties;
using BlackbirdSql.Shared.Interfaces;

namespace BlackbirdSql.Shared.Model.IO;


internal class MemStorageView : AbstractStorageView
{
	protected IBsMemDataStorage _MemStorage;

	public MemStorageView()
	{
	}

	public MemStorageView(IBsMemDataStorage storage)
	{
		_MemStorage = storage;
	}

	public override long EnsureRowsInBuf(long startRow, long totalRowCount)
	{
		return RowCount;
	}

	public override bool IsStorageClosed => _MemStorage.IsClosed;
	


	public override long RowCount => _MemStorage.RowCount;


	public override int ColumnCount => _MemStorage.ColumnCount;


	public override IBsColumnInfo GetColumnInfo(int iCol)
	{
		return _MemStorage.GetColumnInfo(iCol);
	}

	public override object GetCellData(long i64Row, int iCol)
	{
		if (i64Row > int.MaxValue)
		{
			Exception ex = new(ControlsResources.ExMemoryBasedStorageIsLimitedToNRows.Fmt(int.MaxValue));
			Diag.Ex(ex);
			throw ex;
		}
		int iRow = Convert.ToInt32(i64Row);
		return _MemStorage.GetRow(iRow)[iCol];
	}

	public override void DeleteRow(long iRow)
	{
		_MemStorage.DeleteRow(Convert.ToInt32(iRow));
	}

}
