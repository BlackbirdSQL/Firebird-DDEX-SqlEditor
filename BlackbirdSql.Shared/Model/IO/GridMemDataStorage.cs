// Microsoft.SqlServer.DlgGrid, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.DlgGridMemDataStorage

using System;
using System.Collections;
using BlackbirdSql.Shared.Controls.Grid;
using BlackbirdSql.Shared.Interfaces;



namespace BlackbirdSql.Shared.Model.IO;


internal class GridMemDataStorage : AbstractMemDataStorage, IBsGridMemDataStorage, IBsMemDataStorage, IBsDataStorage, IDisposable
{
	private class ComparerWrapper : IComparer
	{
		private readonly int _ColumnIndex;

		private readonly IComparer _InnerComparer;

		private readonly bool _Descending;

		private object[] _CellsLhs;

		private object[] _CellsRhs;

		private ComparerWrapper()
		{
		}

		public ComparerWrapper(int colIndex, IComparer comparer, bool descend)
		{
			_ColumnIndex = colIndex;
			_InnerComparer = comparer;
			_Descending = descend;
		}

		public int Compare(object a, object b)
		{
			if (a == null)
			{
				return -1;
			}
			if (b == null)
			{
				return 1;
			}
			_CellsLhs = (object[])a;
			_CellsRhs = (object[])b;
			int result = _InnerComparer.Compare(_CellsLhs[_ColumnIndex], _CellsRhs[_ColumnIndex]);
			if (!_Descending)
			{
				return result;
			}
			return -result;
		}
	}

	public void InsertColumn(int nIndex, string name)
	{
		_ColumnInfoArray.Insert(nIndex, new ColumnInfo(name));
	}

	public void DeleteColumn(int colIndex)
	{
		int count = _RowsArray.Count;
		for (int i = 0; i < count; i++)
		{
			object[] array = GetRow(i);
			if (array != null && array.Length >= 1)
			{
				GridCell[] array2 = new GridCell[array.Length - 1];
				if (colIndex > 0)
				{
					Array.Copy(array, 0, array2, 0, colIndex);
				}
				if (colIndex < ColumnCount - 1)
				{
					Array.Copy(array, colIndex + 1, array2, colIndex, ColumnCount - colIndex - 1);
				}
				_RowsArray[i] = array2;
			}
		}
		_ColumnInfoArray.RemoveAt(colIndex);
	}

	public void SortByColumn(int colIndex, IComparer comparer, bool descending)
	{
		ComparerWrapper comparer2 = new ComparerWrapper(colIndex, comparer, descending);
		_RowsArray.Sort(comparer2);
	}
}
