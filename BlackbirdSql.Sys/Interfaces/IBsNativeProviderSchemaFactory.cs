
using System;
using System.Data;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;



namespace BlackbirdSql.Sys.Interfaces;

[ComImport]
[Guid(LibraryData.NativeProviderSchemaFactoryGuid)]


// =========================================================================================================
//										IBsProviderSchemaFactory Interface
//
// =========================================================================================================
public interface IBsNativeProviderSchemaFactory
{
	// Schema factory to handle custom collections
	DataTable GetSchema(IDbConnection connection, string collectionName, string[] restrictions);


	Task<DataTable> GetSchemaAsync(IDbConnection connection, string collectionName,
		string[] restrictions, CancellationToken cancellationToken);

}
