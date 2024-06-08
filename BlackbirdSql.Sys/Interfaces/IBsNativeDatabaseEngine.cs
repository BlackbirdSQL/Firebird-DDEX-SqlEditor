// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Threading;
using BlackbirdSql.Sys.Ctl;
using BlackbirdSql.Sys.Enums;
using BlackbirdSql.Sys.Events;
using Microsoft.VisualStudio.Data.Services;
using System.Diagnostics.CodeAnalysis;

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
	Type ConnectionType_ { get; }
	DescriberDictionary Describers_ { get; }
	Type ExceptionType_ { get; }


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


	void AsyncEnsureLinkageLoading_(IDbConnection connection, int delay = 0, int multiplier = 1);
	void AsyncEnsureLinkageLoading_(IVsDataConnection site, int delay = 0, int multiplier = 1);
	void AsyncEnsureLinkageLoading_(IVsDataExplorerNode node, int delay = 0, int multiplier = 1);
	void AsyncRequestLinkageLoading_(IVsDataConnection site, int delay = 0, int multiplier = 1);

	DbConnection CastToAssemblyConnection_(object connection);
	string ConvertDataTypeToSql_(string type, int length, int precision, int scale);
	string ConvertDataTypeToSql_(object type, object length, object precision, object scale);
	IBsNativeDbBatchParser CreateDbBatchParser_(EnSqlExecutionType executionType, IBQueryManager qryMgr, string script);
	DbCommand CreateDbCommand_(string cmdText = null);
	IDbConnection CreateDbConnection_(string connectionString);
	DbConnectionStringBuilder CreateDbConnectionStringBuilder_();
	DbConnectionStringBuilder CreateDbConnectionStringBuilder_(string connectionString);
	IBsNativeDbConnectionWrapper CreateDbConnectionWrapper_(IDbConnection connection, Action<DbConnection> sqlConnectionCreatedObserver = null);
	IBsNativeDbStatementWrapper CreateDbStatementWrapper_(IBsNativeDbBatchParser owner, object statement, int index);
	bool DisposeLinkageParserInstance_(IVsDataConnection site);
	bool DisposeLinkageParserInstance_(IDbConnection connection);
	bool DisposeLinkageParsers_();
	IBsLinkageParser EnsureLinkageParserInstance_(IDbConnection connection);
	IBsLinkageParser EnsureLinkageParserLoaded_(IDbConnection connection);

	byte GetErrorClass_(object error);
	int GetErrorLineNumber_(object error);
	string GetErrorMessage_(object error);
	int GetErrorNumber_(object error);
	int GetObjectTypeIdentifierLength_(string typeName);
	IList<object> GetInfoMessageEventArgsErrors_(DbInfoMessageEventArgs e);
	ICollection<object> GetErrorEnumerator_(IList<object> errors);
	IBsLinkageParser GetLinkageParserInstance_(IVsDataConnection site);
	IBsLinkageParser GetLinkageParserInstance_(IDbConnection connection);
	bool HasTransactions_(IDbTransaction @this);

	bool IsSupportedCommandType_(object command);
	bool IsSupportedConnection_(object connection);
	void LockLoadedParser_(string updatedString);
	bool TransactionCompleted_(IDbTransaction transacttion);
	void UnlockLoadedParser_();

	Task<bool> ReaderCloseAsync_(IDataReader @this, CancellationToken cancelToken);
	Task<DataTable> ReaderGetSchemaTableAsync_(IDataReader @this, CancellationToken cancelToken);
	Task<bool> ReaderNextResultAsync_(IDataReader @this, CancellationToken cancelToken);
	Task<bool> ReaderReadAsync_(IDataReader @this, CancellationToken cancelToken);

}