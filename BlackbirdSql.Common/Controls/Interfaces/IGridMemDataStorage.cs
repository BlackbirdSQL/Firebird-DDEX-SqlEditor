// Microsoft.SqlServer.DlgGrid, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.IDlgGridMemDataStorage

using System;
using System.Collections;
using BlackbirdSql.Common.Model.Interfaces;

// using Microsoft.SqlServer.Management.UI.Grid;




namespace BlackbirdSql.Common.Controls.Interfaces;


public interface IGridMemDataStorage : IMemDataStorage, IDataStorage, IDisposable
{
	void InsertColumn(int nIndex, string name);

	void DeleteColumn(int colIndex);

	void SortByColumn(int colIndex, IComparer comparer, bool descending);
}
