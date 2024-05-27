// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Runtime.InteropServices;



namespace BlackbirdSql.Sys;

[Guid(LibraryData.NativeDatabaseEngineServiceGuid)]


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
	string ClientFactoryName_ { get; }
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
	string SchemaMetaDataXml_ { get; }
	string SqlLanguageName_ { get; }
	string ExternalUtilityConfigurationPath_ { get; }


	string Protocol_ { get; }
	string Scheme_ { get; }
	string Extension_ { get; }

	string XmlActualPlanColumn_ { get; }
	string XmlEstimatedPlanColumn_ { get; }





	DbConnection CastToAssemblyConnection_(object connection);
	string ConvertDataTypeToSql_(string type, int length, int precision, int scale);
	string ConvertDataTypeToSql_(object type, object length, object precision, object scale);
	IBsNativeDbBatchParser CreateDbBatchParser_(EnSqlExecutionType executionType, IBQueryManager qryMgr, string script);
	DbCommand CreateDbCommand_(string cmdText = null);
	IDbConnection CreateDbConnection_(string connectionString);
	DbConnectionStringBuilder CreateDbConnectionStringBuilder_();
	DbConnectionStringBuilder CreateDbConnectionStringBuilder_(string connectionString);
	IBsNativeDbConnectionWrapper CreateDbConnectionWrapper_(IDbConnection connection, Action<DbConnection> sqlConnectionCreatedObserver = null);
	IBsNativeDbStatementWrapper CreateDbStatementWrapper_(IBsNativeDbBatchParser owner, object statement);
	string GetDataTypeName_(EnDbDataType type);
	EnDbDataType GetDbDataTypeFromBlrType_(int type, int subType, int scale);
	byte GetErrorClass_(object error);
	int GetErrorLineNumber_(object error);
	string GetErrorMessage_(object error);
	int GetErrorNumber_(object error);
	IList<object> GetInfoMessageEventArgsErrors_(DbInfoMessageEventArgs e);
	ICollection<object> GetErrorEnumerator_(IList<object> errors);
	bool HasTransactions_(IDbTransaction @this);
	bool IsSupportedCommandType_(object command);
	bool IsSupportedConnection_(object connection);
	bool TransactionCompleted_(IDbTransaction transacttion);
}