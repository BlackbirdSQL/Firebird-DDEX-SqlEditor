// Microsoft.SqlServer.DataStorage, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.IDiskDataStorage
using System;
using System.Data;
using BlackbirdSql.Common.Model.Events;
using BlackbirdSql.Sys;

namespace BlackbirdSql.Common.Model.Interfaces;

public interface IBDiskDataStorage : IBDataStorage, IDisposable
{
	event StorageNotifyDelegate StorageNotify;

	void InitStorage(IBsNativeDbStatementWrapper sqlStatement, IDataReader reader);

	void AddToStorage(IBsNativeDbStatementWrapper sqlStatement, IDataReader reader);

	void StartStoringData();

	void StopStoringData();

	void StoreData();

	void SerializeData();

	string GetFileName();

	long GetRowOffset(long i64Row);

	void DeleteRow(long i64Row);
}
