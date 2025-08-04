
using System;
using System.Data;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;



namespace BlackbirdSql.Sys.Interfaces;

[ComImport]
[Guid(LibraryData.C_NativeProviderSchemaFactoryGuid)]


// =========================================================================================================
//										IBsProviderSchemaFactory Interface
//
// =========================================================================================================
internal interface IBsNativeProviderSchemaFactory
{
	// Schema factory to handle custom collections
	DataTable GetSchema(IDbConnection connection, string collectionName, string[] restrictions);


	Task<DataTable> GetSchemaAsync(IDbConnection connection, string collectionName,
		string[] restrictions, CancellationToken cancelToken);

}
