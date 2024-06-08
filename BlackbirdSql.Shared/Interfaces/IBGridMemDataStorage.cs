// Microsoft.SqlServer.DlgGrid, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.IDlgGridMemDataStorage

using System;
using System.Collections;

// using Microsoft.SqlServer.Management.UI.Grid;




namespace BlackbirdSql.Shared.Interfaces;


public interface IBGridMemDataStorage : IBMemDataStorage, IBDataStorage, IDisposable
{
	void InsertColumn(int nIndex, string name);

	void DeleteColumn(int colIndex);

	void SortByColumn(int colIndex, IComparer comparer, bool descending);
}
