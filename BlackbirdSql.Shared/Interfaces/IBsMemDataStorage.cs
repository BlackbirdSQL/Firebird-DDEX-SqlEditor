// Microsoft.SqlServer.DataStorage, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.IMemDataStorage

using System;

// using Microsoft.SqlServer.Management.UI.Grid;




namespace BlackbirdSql.Shared.Interfaces;


internal interface IBsMemDataStorage : IBsDataStorage, IDisposable
{
	void InitStorage();

	void AddColumn(string name);

	void AddRow(object[] values);

	void InsertRow(int iRow, object[] values);

	void DeleteRow(int iRow);

	object[] GetRow(int iRow);
}
