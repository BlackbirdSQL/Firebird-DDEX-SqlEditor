
using System;
using System.Data;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Threading;

using FirebirdSql.Data.FirebirdClient;


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
	DataTable GetSchema(FbConnection connection, string collectionName, string[] restrictions);


	Task<DataTable> GetSchemaAsync(FbConnection connection, string collectionName,
		string[] restrictions, CancellationToken cancellationToken );

}
