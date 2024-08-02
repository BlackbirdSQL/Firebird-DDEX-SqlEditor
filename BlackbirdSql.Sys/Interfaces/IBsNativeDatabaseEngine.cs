// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using BlackbirdSql.Sys.Ctl;
using BlackbirdSql.Sys.Enums;
using BlackbirdSql.Sys.Events;
using Microsoft.VisualStudio.Data.Services;



namespace BlackbirdSql.Sys.Interfaces;

[Guid(LibraryData.NativeDatabaseEngineServiceGuid)]
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
	string[] EntityFrameworkVersions_ { get; }
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


	void AsyncEnsureLinkageLoading_(IVsDataExplorerConnection root, int delay = 0, int multiplier = 1);

	DbConnection CastToNativeConnection_(object connection);
	string ConvertDataTypeToSql_(object type, object length, object precision, object scale);
	IBsNativeDbBatchParser CreateDbBatchParser_(EnSqlExecutionType executionType, IBsQueryManager qryMgr, string script);
	DbCommand CreateDbCommand_(string cmdText = null);
	IDbConnection CreateDbConnection_(string connectionString);
	IBsNativeDbStatementWrapper CreateDbStatementWrapper_(IBsNativeDbBatchParser owner, object statement, int index);
	bool DisposeLinkageParserInstance_(IVsDataExplorerConnection root, bool disposing);
	byte GetErrorClass_(object error);
	int GetErrorLineNumber_(object error);
	string GetErrorMessage_(object error);
	int GetErrorNumber_(object error);
	string GetDecoratedDdlSource_(IVsDataExplorerNode node, EnModelTargetType targetType);
	ICollection<object> GetErrorEnumerator_(IList<object> errors);
	IBsNativeDbLinkageParser GetLinkageParserInstance_(IVsDataExplorerConnection root);
	int GetObjectTypeIdentifierLength_(string typeName);
	bool HasTransactions_(IDbTransaction @this);
	bool IsSupportedCommandType_(object command);
	bool IsSupportedConnection_(IDbConnection connection);
	bool LockLoadedParser_(string originalString, string updatedString);
	void OpenConnection_(DbConnection connection);
	Task<bool> ReaderCloseAsync_(IDataReader @this, CancellationToken cancelToken);
	Task<DataTable> ReaderGetSchemaTableAsync_(IDataReader @this, CancellationToken cancelToken);
	Task<bool> ReaderNextResultAsync_(IDataReader @this, CancellationToken cancelToken);
	Task<bool> ReaderReadAsync_(IDataReader @this, CancellationToken cancelToken);
	bool TransactionCompleted_(IDbTransaction transacttion);
	void UnlockLoadedParser_();

}