// BlackbirdSql - separated from Builder for brevity
/*
 * This is a central point to change defaults for testing DDEX
 * which will pick them up from here for the UI
 * 
 */


using System;
using System.Collections.Generic;
using System.Data;


namespace BlackbirdSql.Data.Common
{
	internal static class ConnectionParameters
	{
		#region Constants
		internal const string DefaultValueDataSource = "MMEI-LT01"; // For debug
		internal const int DefaultValuePortNumber = 55504; // For debug
		internal const string DefaultValueUserId = "SYSDBA"; // For debug
		internal const string DefaultValuePassword = "masterkey"; // For debug
		internal const string DefaultValueRoleName = "";
		internal const string DefaultValueCatalog = "C:\\Server\\Data\\smartitplus_databases\\MMEI_SI_DB.FDB"; // For debug
		internal const string DefaultValueCharacterSet = "UTF8";
		internal const int DefaultValueDialect = 3;
		internal const int DefaultValuePacketSize = 8192;
		internal const bool DefaultValuePooling = true;
		internal const int DefaultValueConnectionLifetime = 0;
		internal const int DefaultValueMinPoolSize = 0;
		internal const int DefaultValueMaxPoolSize = 100;
		internal const int DefaultValueConnectionTimeout = 15;
		internal const int DefaultValueFetchSize = 200;
		internal const ServerType DefaultValueServerType = ServerType.Default;
		internal const IsolationLevel DefaultValueIsolationLevel = IsolationLevel.ReadCommitted;
		internal const bool DefaultValueRecordsAffected = true;
		internal const bool DefaultValueEnlist = true;
		internal const string DefaultValueClientLibrary = "fbembed";
		internal const int DefaultValueDbCachePages = 0;
		internal const bool DefaultValueNoDbTriggers = false;
		internal const bool DefaultValueNoGarbageCollect = false;
		internal const bool DefaultValueCompression = false;
		internal const byte[] DefaultValueCryptKey = null;
		internal const WireCrypt DefaultValueWireCrypt = WireCrypt.Enabled;
		internal const string DefaultValueApplicationName = "";
		internal const int DefaultValueCommandTimeout = 0;
		internal const int DefaultValueParallelWorkers = 0;

		internal const string DefaultKeyUserId = "user id";
		internal const string DefaultKeyPortNumber = "port number";
		internal const string DefaultKeyDataSource = "data source";
		internal const string DefaultKeyPassword = "password";
		internal const string DefaultKeyRoleName = "role name";
		internal const string DefaultKeyCatalog = "initial catalog";
		internal const string DefaultKeyCharacterSet = "character set";
		internal const string DefaultKeyDialect = "dialect";
		internal const string DefaultKeyPacketSize = "packet size";
		internal const string DefaultKeyPooling = "pooling";
		internal const string DefaultKeyConnectionLifetime = "connection lifetime";
		internal const string DefaultKeyMinPoolSize = "min pool size";
		internal const string DefaultKeyMaxPoolSize = "max pool size";
		internal const string DefaultKeyConnectionTimeout = "connection timeout";
		internal const string DefaultKeyFetchSize = "fetch size";
		internal const string DefaultKeyServerType = "server type";
		internal const string DefaultKeyIsolationLevel = "isolation level";
		internal const string DefaultKeyRecordsAffected = "records affected";
		internal const string DefaultKeyEnlist = "enlist";
		internal const string DefaultKeyClientLibrary = "client library";
		internal const string DefaultKeyDbCachePages = "cache pages";
		internal const string DefaultKeyNoDbTriggers = "no db triggers";
		internal const string DefaultKeyNoGarbageCollect = "no garbage collect";
		internal const string DefaultKeyCompression = "compression";
		internal const string DefaultKeyCryptKey = "crypt key";
		internal const string DefaultKeyWireCrypt = "wire crypt";
		internal const string DefaultKeyApplicationName = "application name";
		internal const string DefaultKeyCommandTimeout = "command timeout";
		internal const string DefaultKeyParallelWorkers = "parallel workers";
		#endregion

		#region Static Fields


		internal static readonly IDictionary<string, object> DefaultValues = new Dictionary<string, object>(StringComparer.Ordinal)
		{
			{ DefaultKeyDataSource, DefaultValueDataSource },
			{ DefaultKeyPortNumber, DefaultValuePortNumber },
			{ DefaultKeyUserId, DefaultValueUserId },
			{ DefaultKeyPassword, DefaultValuePassword },
			{ DefaultKeyRoleName, DefaultValueRoleName },
			{ DefaultKeyCatalog, DefaultValueCatalog },
			{ DefaultKeyCharacterSet, DefaultValueCharacterSet },
			{ DefaultKeyDialect, DefaultValueDialect },
			{ DefaultKeyPacketSize, DefaultValuePacketSize },
			{ DefaultKeyPooling, DefaultValuePooling },
			{ DefaultKeyConnectionLifetime, DefaultValueConnectionLifetime },
			{ DefaultKeyMinPoolSize, DefaultValueMinPoolSize },
			{ DefaultKeyMaxPoolSize, DefaultValueMaxPoolSize },
			{ DefaultKeyConnectionTimeout, DefaultValueConnectionTimeout },
			{ DefaultKeyFetchSize, DefaultValueFetchSize },
			{ DefaultKeyServerType, DefaultValueServerType },
			{ DefaultKeyIsolationLevel, DefaultValueIsolationLevel },
			{ DefaultKeyRecordsAffected, DefaultValueRecordsAffected },
			{ DefaultKeyEnlist, DefaultValueEnlist },
			{ DefaultKeyClientLibrary, DefaultValueClientLibrary },
			{ DefaultKeyDbCachePages, DefaultValueDbCachePages },
			{ DefaultKeyNoDbTriggers, DefaultValueNoDbTriggers },
			{ DefaultKeyNoGarbageCollect, DefaultValueNoGarbageCollect },
			{ DefaultKeyCompression, DefaultValueCompression },
			{ DefaultKeyCryptKey, DefaultValueCryptKey },
			{ DefaultKeyWireCrypt, DefaultValueWireCrypt },
			{ DefaultKeyApplicationName, DefaultValueApplicationName },
			{ DefaultKeyCommandTimeout, DefaultValueCommandTimeout },
			{ DefaultKeyParallelWorkers, DefaultValueParallelWorkers },
		};


		internal static readonly IDictionary<string, string> Synonyms = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
		{
			{ DefaultKeyDataSource, DefaultKeyDataSource },
			{ "datasource", DefaultKeyDataSource },
			{ "server", DefaultKeyDataSource },
			{ "host", DefaultKeyDataSource },
			{ "port", DefaultKeyPortNumber },
			{ DefaultKeyPortNumber, DefaultKeyPortNumber },
			{ "database", DefaultKeyCatalog },
			{ DefaultKeyCatalog, DefaultKeyCatalog },
			{ DefaultKeyUserId, DefaultKeyUserId },
			{ "userid", DefaultKeyUserId },
			{ "uid", DefaultKeyUserId },
			{ "user", DefaultKeyUserId },
			{ "user name", DefaultKeyUserId },
			{ "username", DefaultKeyUserId },
			{ DefaultKeyPassword, DefaultKeyPassword },
			{ "user password", DefaultKeyPassword },
			{ "userpassword", DefaultKeyPassword },
			{ DefaultKeyDialect, DefaultKeyDialect },
			{ DefaultKeyPooling, DefaultKeyPooling },
			{ DefaultKeyMaxPoolSize, DefaultKeyMaxPoolSize },
			{ "maxpoolsize", DefaultKeyMaxPoolSize },
			{ DefaultKeyMinPoolSize, DefaultKeyMinPoolSize },
			{ "minpoolsize", DefaultKeyMinPoolSize },
			{ DefaultKeyCharacterSet, DefaultKeyCharacterSet },
			{ "charset", DefaultKeyCharacterSet },
			{ DefaultKeyConnectionLifetime, DefaultKeyConnectionLifetime },
			{ "connectionlifetime", DefaultKeyConnectionLifetime },
			{ "timeout", DefaultKeyConnectionTimeout },
			{ DefaultKeyConnectionTimeout, DefaultKeyConnectionTimeout },
			{ "connectiontimeout", DefaultKeyConnectionTimeout },
			{ DefaultKeyPacketSize, DefaultKeyPacketSize },
			{ "packetsize", DefaultKeyPacketSize },
			{ "role", DefaultKeyRoleName },
			{ DefaultKeyRoleName, DefaultKeyRoleName },
			{ DefaultKeyFetchSize, DefaultKeyFetchSize },
			{ "fetchsize", DefaultKeyFetchSize },
			{ DefaultKeyServerType, DefaultKeyServerType },
			{ "servertype", DefaultKeyServerType },
			{ DefaultKeyIsolationLevel, DefaultKeyIsolationLevel },
			{ "isolationlevel", DefaultKeyIsolationLevel },
			{ DefaultKeyRecordsAffected, DefaultKeyRecordsAffected },
			{ DefaultKeyEnlist, DefaultKeyEnlist },
			{ "clientlibrary", DefaultKeyClientLibrary },
			{ DefaultKeyClientLibrary, DefaultKeyClientLibrary },
			{ DefaultKeyDbCachePages, DefaultKeyDbCachePages },
			{ "cachepages", DefaultKeyDbCachePages },
			{ "pagebuffers", DefaultKeyDbCachePages },
			{ "page buffers", DefaultKeyDbCachePages },
			{ DefaultKeyNoDbTriggers, DefaultKeyNoDbTriggers },
			{ "nodbtriggers", DefaultKeyNoDbTriggers },
			{ "no dbtriggers", DefaultKeyNoDbTriggers },
			{ "no database triggers", DefaultKeyNoDbTriggers },
			{ "nodatabasetriggers", DefaultKeyNoDbTriggers },
			{ DefaultKeyNoGarbageCollect, DefaultKeyNoGarbageCollect },
			{ "nogarbagecollect", DefaultKeyNoGarbageCollect },
			{ DefaultKeyCompression, DefaultKeyCompression },
			{ "wire compression", DefaultKeyCompression },
			{ DefaultKeyCryptKey, DefaultKeyCryptKey },
			{ "cryptkey", DefaultKeyCryptKey },
			{ DefaultKeyWireCrypt, DefaultKeyWireCrypt },
			{ "wirecrypt", DefaultKeyWireCrypt },
			{ DefaultKeyApplicationName, DefaultKeyApplicationName },
			{ "applicationname", DefaultKeyApplicationName },
			{ "app", DefaultKeyApplicationName },
			{ DefaultKeyCommandTimeout, DefaultKeyCommandTimeout },
			{ "commandtimeout", DefaultKeyCommandTimeout },
			{ DefaultKeyParallelWorkers, DefaultKeyParallelWorkers },
			{ "parallelworkers", DefaultKeyParallelWorkers },
			{ "parallel", DefaultKeyParallelWorkers },
		};

		#endregion
	}
}
