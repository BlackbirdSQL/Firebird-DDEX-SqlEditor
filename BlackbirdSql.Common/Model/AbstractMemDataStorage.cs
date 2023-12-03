// Microsoft.SqlServer.DataStorage, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.MemDataStorage

using System;
using System.Collections;
using BlackbirdSql.Common.Controls.Grid;
using BlackbirdSql.Common.Model.Interfaces;
using BlackbirdSql.Common.Ctl.Interfaces;


// using Microsoft.SqlServer.Management.UI.Grid;




namespace BlackbirdSql.Common.Model;


public abstract class AbstractMemDataStorage : IBMemDataStorage, IBDataStorage, IDisposable
{
	protected ArrayList _ColumnInfoArray;

	protected ArrayList _RowsArray;

	public virtual IBStorageView GetStorageView()
	{
		return new MemStorageView(this);
	}

	public IBSortView GetSortView()
	{
		return new SortView(GetStorageView());
	}

	public AbstractMemDataStorage()
	{
		_ColumnInfoArray = [];
		_RowsArray = [];
	}

	public virtual void Dispose()
	{
	}

	public virtual void InitStorage()
	{
		_ColumnInfoArray.Clear();
		_RowsArray.Clear();
	}

	public virtual void AddColumn(string name)
	{
		_ColumnInfoArray.Add(new ColumnInfo(name));
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


	public int ColumnCount => _ColumnInfoArray.Count;

	public IBColumnInfo GetColumnInfo(int iCol)
	{
		return (IBColumnInfo)_ColumnInfoArray[iCol];
	}

	public bool IsClosed()
	{
		return true;
	}
}
