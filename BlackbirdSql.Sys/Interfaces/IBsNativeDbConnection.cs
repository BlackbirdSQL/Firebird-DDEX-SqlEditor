// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Data;
using System.Data.Common;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Threading;



namespace BlackbirdSql.Sys;

[Guid(LibraryData.NativeDbConnectionServiceGuid)]


// =========================================================================================================
//										IBsNativeDbConnection Interface
/// <summary>
/// Interface for native DbException extension methods service.
/// </summary>
// =========================================================================================================
public interface IBsNativeDbConnection
{
	object CreateDatabaseInfoObject(DbConnection @this);
	DbCommand CreateDbCommand(DbConnection @this, string cmdText = null, object transaction = null);

	string GetDataSource(DbConnection @this);
	string GetDataSourceVersion(IDbConnection @this);
	int GetPacketSize(DbConnection @this);
	Task<DataTable> GetSchemaAsync(DbConnection @this, string collectionName,
		string[] restrictions, CancellationToken cancellationToken);
	Version GetVersion(IDbConnection @this);
}