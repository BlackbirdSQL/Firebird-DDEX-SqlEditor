// Microsoft.SqlServer.DataStorage, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.MemDataStorage

using System;
using System.Collections;
using BlackbirdSql.Common.Controls.Grid;
using BlackbirdSql.Common.Interfaces;
using BlackbirdSql.Common.Model.Interfaces;


// using Microsoft.SqlServer.Management.UI.Grid;




namespace BlackbirdSql.Common.Model;


public abstract class AbstractMemDataStorage : IMemDataStorage, IDataStorage, IDisposable
{
	protected ArrayList _ColumnsArray;

	protected ArrayList _RowsArray;

	public virtual IStorageView GetStorageView()
	{
		return new MemStorageView(this);
	}

	public ISortView GetSortView()
	{
		return new SortView(GetStorageView());
	}

	public AbstractMemDataStorage()
	{
		_ColumnsArray = new ArrayList();
		_RowsArray = new ArrayList();
	}

	public virtual void Dispose()
	{
	}

	public virtual void InitStorage()
	{
		_ColumnsArray.Clear();
		_RowsArray.Clear();
	}

	public virtual void AddColumn(string name)
	{
		_ColumnsArray.Add(new ColumnInfo(name));
	}

	public virtual void AddRow(object[] values)
	{
		_RowsArray.Add(values);
	}

	public virtual void InsertRow(int iRow, object[] values)
	{
		_RowsArray.Insert(iRow, values);
	}

	public virtual void DeleteRow(int iRow)
	{
		_RowsArray.RemoveAt(iRow);
	}

	public virtual object[] GetRow(int i)
	{
		return (object[])_RowsArray[i];
	}

	public long RowCount => _RowsArray.Count;


	public int ColumnCount => _ColumnsArray.Count;

	public IColumnInfo GetColumnInfo(int iCol)
	{
		return (IColumnInfo)_ColumnsArray[iCol];
	}

	public bool IsClosed()
	{
		return true;
	}
}
