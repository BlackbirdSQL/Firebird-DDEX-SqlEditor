// Microsoft.SqlServer.DataStorage, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.IDiskDataStorage
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using BlackbirdSql.Shared.Events;

namespace BlackbirdSql.Shared.Interfaces;

public interface IBsDiskDataStorage : IBsDataStorage, IDisposable
{
	event StorageNotifyDelegate StorageNotifyEventAsync;

	Task<bool> InitStorageAsync(IDataReader reader, CancellationToken cancelToken);

	Task<bool> AddToStorageAsync(IDataReader reader, CancellationToken cancelToken);

	Task<bool> StartStoringDataAsync(CancellationToken cancelToken);

	void StopStoringData();

	Task<bool> StoreDataAsync(CancellationToken cancelToken);

	Task<bool> SerializeDataAsync(CancellationToken cancelToken);

	string GetFileName();

	long GetRowOffset(long i64Row);

	void DeleteRow(long i64Row);
}
