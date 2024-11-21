// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using BlackbirdSql.Sys.Ctl;
using BlackbirdSql.Sys.Enums;
using Microsoft.VisualStudio.Data.Services;



namespace BlackbirdSql.Sys.Interfaces;

[Guid(LibraryData.C_NativeDatabaseEngineServiceGuid)]
[SuppressMessage("Style", "VSTHRD200:Use \"Async\" suffix for async methods", Justification = "<Pending>")]


// =========================================================================================================
//										IBsNativeDatabaseEngine Interface
/// <summary>
/// Interface for native DatabaseEngine service.
/// </summary>
// =========================================================================================================
public interface IBsNativeDatabaseEngine
{
	string AssemblyQualifiedName_ { get; }
	Assembly ClientFactoryAssembly_ { get; }
	string ClientVersion_ { get; }
	Type ClientFactoryType_ { get; }
	Type EFProviderServicesType_ { get; }
	string EFProviderServicesTypeFullName_ { get; }
	Assembly EntityFrameworkAssembly_ { get; }
	// string[] EntityFrameworkVersions_ { get; }
	Type ConnectionType_ { get; }
	DescriberDictionary Describers_ { get; }


	string Invariant_ { get; }
	string ProviderFactoryName_ { get; }
	string ProviderFactoryClassName_ { get; }
	string ProviderFactoryDescription_ { get; }

	string EFProvider_ { get; }
	string EFProviderServices_ { get; }
	string EFConnectionFactory_ { get; }

	string DataProviderName_ { get; }
	string DbEngineName_ { get; }
	string SqlLanguageName_ { get; }
	string ExternalUtilityConfigurationPath_ { get; }


	string Protocol_ { get; }
	string Scheme_ { get; }
	string Extension_ { get; }

	string XmlActualPlanColumn_ { get; }
	string XmlEstimatedPlanColumn_ { get; }

	string RootObjectTypeName_ { get; }



	int AddCommandParameter_(DbCommand @this, string name, int index, object value);
	DbConnection CastToNativeConnection_(object connection);
	object CreateDatabaseInfoObject_(DbConnection @this);
	IBsNativeDbBatchParser CreateDbBatchParser_(EnSqlExecutionType executionType, IBsQueryManager qryMgr, string script);
	IDbConnection CreateDbConnection_(string connectionString);
	DbCommand CreateDbCommand_(string cmdText = null);
	IBsNativeDbStatementWrapper CreateDbStatementWrapper_(IBsNativeDbBatchParser owner, object statement, int index);
	bool DisposeLinkageParserInstance_(IVsDataExplorerConnection root, bool disposing);
	void EnsureLinkageLoadingAsyin_(IVsDataExplorerConnection root, int delay = 0, int multiplier = 1);
	string GetConnectionDataSource_(IDbConnection @this);
	string GetConnectionDataSourceVersion_(IDbConnection @this);
	int GetConnectionPacketSize_(DbConnection @this);
	IBsNativeDbLinkageParser GetLinkageParserInstance_(IVsDataExplorerConnection root);
	bool HasTransactions_(IDbTransaction @this);
	bool LockLoadedParser_(string originalString, string updatedString);
	bool MatchesEntityFrameworkAssembly_(string assemblyName);
	bool MatchesInvariantAssembly_(string assemblyName);
	(bool, bool) OpenOrVerifyConnection_(IDbConnection @this);
	Task<(bool, bool)> OpenOrVerifyConnectionAsync_(IDbConnection @this,
		IDbTransaction transaction, CancellationToken cancelToken);
	Version ParseConnectionServerVersion_(IDbConnection @this);
	Task<bool> ReaderCloseAsync_(IDataReader @this, CancellationToken cancelToken);
	Task<DataTable> ReaderGetSchemaTableAsync_(IDataReader @this, CancellationToken cancelToken);
	Task<bool> ReaderNextResultAsync_(IDataReader @this, CancellationToken cancelToken);
	Task<bool> ReaderReadAsync_(IDataReader @this, CancellationToken cancelToken);

	bool TransactionCompleted_(IDbTransaction transacttion);
	void UnlockLoadedParser_();
}