
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection;
using BlackbirdSql.Data.Model;
using BlackbirdSql.Sys;
using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.Data.Isql;

using static BlackbirdSql.Data.DbConstants;
using static BlackbirdSql.SysConstants;



namespace BlackbirdSql.Data;


// =========================================================================================================
//											Firebird DatabaseEngineService Class
//
/// <summary>
/// Central class for database specific class methods. The intention is ultimately provide these as a
/// service so that BlackbirdSql can provide support for additional database engines.
/// </summary>
// =========================================================================================================
public class DatabaseEngineService : SBsNativeDatabaseEngine, IBsNativeDatabaseEngine
{
	private DatabaseEngineService()
	{
	}

	public static DatabaseEngineService CreateInstance() => new();


	public string AssemblyQualifiedName_ => typeof(FirebirdClientFactory).AssemblyQualifiedName;

	public Assembly ClientFactoryAssembly_ => typeof(FirebirdClientFactory).Assembly;

	public string ClientFactoryName_ => typeof(FirebirdClientFactory).Name;

	public string ClientVersion_ => $"FirebirdSql {typeof(FbConnection).Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>().Version}";

	public Type ClientFactoryType_ => typeof(FirebirdClientFactory);

	public Type ConnectionType_ => typeof(FbConnection);

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Describer collection of uniform properties used by the FbConnectionStringBuilder
	/// wrapper, Csb, as well as PropertyAgent and it's descendent SqlEditor
	/// ConnectionInfo and Dispatcher connection classes, and also the SE root nodes,
	/// Root and Database.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public DescriberDictionary Describers_ => new(
		[
			new Describer(C_KeyExDatasetKey, typeof(string), C_DefaultExDatasetKey),
			new Describer(C_KeyExConnectionKey, typeof(string), C_DefaultExConnectionKey),
			new Describer(C_KeyExDatasetId, typeof(string), C_DefaultExDatasetId),
			new Describer(C_KeyExDataset, typeof(string), C_DefaultExDataset),
			new Describer(C_KeyExConnectionName, typeof(string), C_DefaultExConnectionName),
			new Describer(C_KeyExConnectionSource, typeof(EnConnectionSource), C_DefaultExConnectionSource),

			new Describer(C_KeyDataSource, C_KeyDbDataSource, typeof(string), C_DefaultDataSource, true, false, true, true), // *
			new Describer(C_KeyPort, C_KeyDbPort, typeof(int), C_DefaultPort, true, false), // *
			new Describer(C_KeyServerType, C_KeyDbServerType, typeof(EnServerType), C_DefaultServerType, true, false), // *
			new Describer(C_KeyDatabase, C_KeyDbDatabase, typeof(string), C_DefaultDatabase, true, false, true, true), // *
			new Describer(C_KeyUserID, C_KeyDbUserID, typeof(string), C_DefaultUserID, true, false, true, true), // *
			new Describer(C_KeyPassword, C_KeyDbPassword, typeof(string), C_DefaultPassword, true, false, false, true),

			new Describer(C_KeyRole, C_KeyDbRole, typeof(string), C_DefaultRole, true, false), // *
			new Describer(C_KeyDialect, C_KeyDbDialect, typeof(int), C_DefaultDialect, true, false), // *
			new Describer(C_KeyCharset, C_KeyDbCharset, typeof(string), C_DefaultCharset, true, false), // *
			new Describer(C_KeyNoDatabaseTriggers, C_KeyDbNoDatabaseTriggers, typeof(bool), C_DefaultNoDatabaseTriggers, true), // *
			new Describer(C_KeyPacketSize, C_KeyDbPacketSize, typeof(int), C_DefaultPacketSize, true),
			new Describer(C_KeyConnectionTimeout, C_KeyDbConnectionTimeout, typeof(int), C_DefaultConnectionTimeout, true),
			new Describer(C_KeyPooling, C_KeyDbPooling, typeof(bool), C_DefaultPooling, true),
			new Describer(C_KeyConnectionLifeTime, C_KeyDbConnectionLifeTime, typeof(int), C_DefaultConnectionLifeTime, true),
			new Describer(C_KeyMinPoolSize, C_KeyDbMinPoolSize, typeof(int), C_DefaultMinPoolSize, true),
			new Describer(C_KeyMaxPoolSize, C_KeyDbMaxPoolSize, typeof(int), C_DefaultMaxPoolSize, true),
			new Describer(C_KeyFetchSize, C_KeyDbFetchSize, typeof(int), C_DefaultFetchSize, true),
			new Describer(C_KeyIsolationLevel, C_KeyDbIsolationLevel, typeof(IsolationLevel), C_DefaultIsolationLevel, true),
			new Describer(C_KeyReturnRecordsAffected, C_KeyDbReturnRecordsAffected, typeof(bool), C_DefaultReturnRecordsAffected, true),
			new Describer(C_KeyEnlist, C_KeyDbEnlist, typeof(bool), C_DefaultEnlist, true),
			new Describer(C_KeyClientLibrary, C_KeyDbClientLibrary, typeof(string), C_DefaultClientLibrary, true),
			new Describer(C_KeyDbCachePages, C_KeyDbDbCachePages, typeof(int), C_DefaultDbCachePages, true),
			new Describer(C_KeyNoGarbageCollect, C_KeyDbNoGarbageCollect, typeof(bool), C_DefaultNoGarbageCollect, true),
			new Describer(C_KeyCompression, C_KeyDbCompression, typeof(bool), C_DefaultCompression, true),
			new Describer(C_KeyCryptKey, C_KeyDbCryptKey, typeof(byte[]), C_DefaultCryptKey, true),
			new Describer(C_KeyWireCrypt, C_KeyDbWireCrypt, typeof(EnWireCrypt), C_DefaultWireCrypt, true),
			new Describer(C_KeyApplicationName, C_KeyDbApplicationName, typeof(string), C_DefaultApplicationName, true),
			new Describer(C_KeyCommandTimeout, C_KeyDbCommandTimeout, typeof(int), C_DefaultCommandTimeout, true),
			new Describer(C_KeyParallelWorkers, C_KeyDbParallelWorkers, typeof(int), C_DefaultParallelWorkers, true),

			new Describer(C_KeyExClientVersion, typeof(Version), C_DefaultExClientVersion, false, false),
			new Describer(C_KeyExMemoryUsage, typeof(string), C_DefaultExMemoryUsage, false, false),
			new Describer(C_KeyExActiveUsers, typeof(int), C_DefaultExActiveUsers, false, false),

			new Describer(C_KeyExServerEngine, typeof(EnEngineType), C_DefaultExServerEngine),
			new Describer(C_KeyExServerVersion, typeof(Version), C_DefaultExServerVersion),
			new Describer(C_KeyExPersistPassword, typeof(bool), C_DefaultExPersistPassword, false, false, false),
			new Describer(C_KeyExAdministratorLogin, typeof(string), C_DefaultExAdministratorLogin),
			new Describer(C_KeyExServerFullyQualifiedDomainName, typeof(string), C_DefaultExServerFullyQualifiedDomainName),
			new Describer(C_KeyExOtherParams, typeof(string)),
			new Describer(C_KeyExIcon, typeof(object)),
			new Describer(C_KeyExEdmx, typeof(string)),
			new Describer(C_KeyExEdmu, typeof(string))
		],
		[
			StringPair("server", C_KeyDataSource),
			StringPair("host", C_KeyDataSource),
			StringPair("uid", C_KeyUserID),
			StringPair("user", C_KeyUserID),
			StringPair("username", C_KeyUserID),
			StringPair("user name", C_KeyUserID),
			StringPair("userpassword", C_KeyPassword),
			StringPair("user password", C_KeyPassword),
			StringPair("no triggers", C_KeyNoDatabaseTriggers),
			StringPair("nodbtriggers", C_KeyNoDatabaseTriggers),
			StringPair("no dbtriggers", C_KeyNoDatabaseTriggers),
			StringPair("no database triggers", C_KeyNoDatabaseTriggers),
			StringPair("timeout", C_KeyConnectionTimeout),
			StringPair("db cache pages", C_KeyDbCachePages),
			StringPair("cachepages", C_KeyDbCachePages),
			StringPair("pagebuffers", C_KeyDbCachePages),
			StringPair("page buffers", C_KeyDbCachePages),
			StringPair("wire compression", C_KeyCompression),
			StringPair("app", C_KeyApplicationName),
			StringPair("parallel", C_KeyParallelWorkers)
		]
	);

	public Type ExceptionType_ => typeof(FbException);



	public string Invariant_ => LibraryData.Invariant;
	public string ProviderFactoryName_ => LibraryData.ProviderFactoryName;
	public string ProviderFactoryClassName_ => LibraryData.ProviderFactoryClassName;
	public string ProviderFactoryDescription_ => LibraryData.ProviderFactoryDescription;

	public string EFProvider_ => LibraryData.EFProvider;
	public string EFProviderServices_ => LibraryData.EFProviderServices;
	public string EFConnectionFactory_ => LibraryData.EFConnectionFactory;

	public string DataProviderName_ => LibraryData.DataProviderName;
	public string DbEngineName_ => LibraryData.DbEngineName;
	public string SchemaMetaDataXml_ => LibraryData.SchemaMetaDataXml;

	public string SqlLanguageName_ => LibraryData.SqlLanguageName;

	/// <summary>
	/// The path to the provider's configured connections xml (in this case FlameRobin for Firebird).
	/// </summary>
	public string ExternalUtilityConfigurationPath_ => LibraryData.ExternalUtilityConfigurationPath;


	public string Protocol_ => LibraryData.Protocol;
	public string Scheme_ => LibraryData.Scheme;
	public string Extension_ => LibraryData.Extension;

	public string XmlActualPlanColumn_ => LibraryData.XmlActualPlanColumn;
	public string XmlEstimatedPlanColumn_ => LibraryData.XmlEstimatedPlanColumn;



	public DbConnection CastToAssemblyConnection_(object connection)
	{
		return connection as FbConnection;
	}


	public string ConvertDataTypeToSql_(string type, int length, int precision, int scale)
	{
		return DbTypeHelper.ConvertDataTypeToSql(type, length, precision, scale);
	}

	public string ConvertDataTypeToSql_(object type, object length, object precision, object scale)
	{
		return DbTypeHelper.ConvertDataTypeToSql(type, length, precision, scale);
	}

	public DbCommand CreateDbCommand_(string cmdText = null)
	{
		return new FbCommand(cmdText);
	}


	/// <summary>
	/// Creates a Firebird connection using a connection string.
	/// </summary>
	public IDbConnection CreateDbConnection_(string  connectionString)
	{
		return new FbConnection(connectionString);
	}

	public DbConnectionStringBuilder CreateDbConnectionStringBuilder_()
	{
		return new FbConnectionStringBuilder();
	}

	public DbConnectionStringBuilder CreateDbConnectionStringBuilder_(string connectionString)
	{
		return new FbConnectionStringBuilder(connectionString);
	}

	public IBsNativeDbConnectionWrapper CreateDbConnectionWrapper_(IDbConnection connection, Action<DbConnection> sqlConnectionCreatedObserver = null)
	{
		return new DbConnectionWrapper(connection, sqlConnectionCreatedObserver);
	}


	public IBsNativeDbBatchParser CreateDbBatchParser_(EnSqlExecutionType executionType, IBQueryManager qryMgr, string script)
	{
		return new DbBatchParser(executionType, qryMgr, script);
	}


	public IBsNativeDbStatementWrapper CreateDbStatementWrapper_(IBsNativeDbBatchParser owner, object statement)
	{
		return new DbStatementWrapper(owner, statement);
	}

	public EnDbDataType GetDbDataTypeFromBlrType_(int type, int subType, int scale)
	{
		return DbTypeHelper.GetDbDataTypeFromBlrType(type, subType, scale);
	}

	public string GetDataTypeName_(EnDbDataType type)
	{
		return DbTypeHelper.GetDataTypeName(type);
	}

	public byte GetErrorClass_(object error)
	{
		return ((FbError)error).Class;
	}

	public int GetErrorLineNumber_(object error)
	{
		return ((FbError)error).LineNumber;
	}



	public string GetErrorMessage_(object error)
	{
		return ((FbError)error).Message;
	}


	public int GetErrorNumber_(object error)
	{
		return ((FbError)error).Number;
	}


	public IList<object> GetInfoMessageEventArgsErrors_(DbInfoMessageEventArgs e)
	{
		if (e.InternalEventArgs is not FbInfoMessageEventArgs fbe)
			return new List<object>();

		IList<object> objects = new List<object>(fbe.Errors.Count);

		foreach (FbError error in fbe.Errors)
			objects.Add(error);

		return objects;
	}

	public ICollection<object> GetErrorEnumerator_(IList<object> errors)
	{
		if (errors == null)
			return null;

		return errors;
	}

	public bool HasTransactions_(IDbTransaction @this)
	{
		if (@this == null)
			return false;

		FbConnection connection = (FbConnection)@this.Connection;

		FbDatabaseInfo dbInfo = new(connection);

		return dbInfo.GetActiveTransactionsCount() > 0;
	}



	public bool IsSupportedCommandType_(object command)
	{
		return command is IBsNativeDbStatementWrapper || command is FbCommand || command is FbBatchExecution;
	}

	public bool IsSupportedConnection_(object connection)
	{
		return connection is FbConnection;
	}

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Converts a key and value to a KeyValuePair<string, string>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private static KeyValuePair<string, string> StringPair(string key, string value)
	{
		return new KeyValuePair<string, string>(key, value);
	}

	public bool TransactionCompleted_(IDbTransaction @this)
	{
		if (@this == null || @this.Connection == null)
			return true;

		return (bool)Reflect.GetPropertyValue(@this, "IsCompleted");
	}


}
