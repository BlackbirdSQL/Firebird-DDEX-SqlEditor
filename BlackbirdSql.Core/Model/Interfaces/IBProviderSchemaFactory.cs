
using System;
using System.Data;
using System.Data.Common;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;



namespace BlackbirdSql.Core.Model.Interfaces;

[ComImport]
[Guid(SystemData.ProviderSchemaFactoryGuid)]


// =========================================================================================================
//										IBProviderSchemaFactory Interface
//
// =========================================================================================================
public interface IBProviderSchemaFactory
{
	// Schema factory to handle custom collections
	DataTable GetSchema(DbConnection connection, string collectionName, string[] restrictions);


	Task<DataTable> GetSchemaAsync(DbConnection connection, string collectionName,
		string[] restrictions, CancellationToken cancellationToken );

}
