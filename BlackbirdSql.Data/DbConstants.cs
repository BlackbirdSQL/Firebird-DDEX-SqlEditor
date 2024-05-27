
using System;
using System.Data;
using System.Security;



namespace BlackbirdSql.Data;


// =========================================================================================================
//											DbConstants Class
//
/// <summary>
/// Core db constants class.
/// </summary>
// =========================================================================================================
public static class DbConstants
{

	// ---------------------------------------------------------------------------------
	#region Property Names - DbConstants
	// ---------------------------------------------------------------------------------


	// Built-in default parameter keys
	public const string C_KeyDbDataSource = "data source";
	public const string C_KeyDbPort = "port number";
	public const string C_KeyDbServerType = "server type";
	public const string C_KeyDbDatabase = "initial catalog";
	public const string C_KeyDbUserID = "user id";
	public const string C_KeyDbPassword = "password";

	public const string C_KeyDbRole = "role name";
	public const string C_KeyDbDialect = "dialect";
	public const string C_KeyDbCharset = "character set";
	public const string C_KeyDbNoDatabaseTriggers = "no db triggers";
	public const string C_KeyDbPacketSize = "packet size";
	public const string C_KeyDbConnectionTimeout = "connection timeout";
	public const string C_KeyDbPooling = "pooling";
	public const string C_KeyDbConnectionLifeTime = "connection lifetime";
	public const string C_KeyDbMinPoolSize = "min pool size";
	public const string C_KeyDbMaxPoolSize = "max pool size";
	public const string C_KeyDbFetchSize = "fetch size";
	public const string C_KeyDbIsolationLevel = "isolation level";
	public const string C_KeyDbReturnRecordsAffected = "records affected";
	public const string C_KeyDbEnlist = "enlist";
	public const string C_KeyDbClientLibrary = "client library";
	public const string C_KeyDbDbCachePages = "cache pages";
	public const string C_KeyDbNoGarbageCollect = "no garbage collect";
	public const string C_KeyDbCompression = "compression";
	public const string C_KeyDbCryptKey = "crypt key";
	public const string C_KeyDbWireCrypt = "wire crypt";
	public const string C_KeyDbApplicationName = "application name";
	public const string C_KeyDbCommandTimeout = "command timeout";
	public const string C_KeyDbParallelWorkers = "parallel workers";



	#endregion Property Names

}
