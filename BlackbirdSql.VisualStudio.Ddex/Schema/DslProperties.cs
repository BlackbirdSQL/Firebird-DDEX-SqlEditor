/*
 *	This is an override of the FirebirdClient Schema
 *	We're maintaining the same structure so that it's easy to overload any GetSchema's that may need it.
 *	We still use the original Firebird metadata manifest pulled from the Firebird assembly
 *	
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/raw/master/license.txt.
 *
 *    Software distributed under the License is distributed on
 *    an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *    express or implied. See the License for the specific
 *    language governing rights and limitations under the License.
 *
 *    All Rights Reserved.
 */

//$OriginalAuthors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

using BlackbirdSql.Common;
using FirebirdSql.Data.FirebirdClient;



namespace BlackbirdSql.VisualStudio.Ddex.Schema;


internal static class DslProperties
{
	#region Constants

	internal static readonly object UnretrievedValue = null;
	internal static readonly object UnretrievedObject = new object();


	internal const string DefaultValueDataSource = "";
	internal const int DefaultValuePortNumber = 3050;
	internal const string DefaultValueUserId = "";
	internal const string DefaultValuePassword = "";
	internal const string DefaultValueRoleName = "";
	internal const string DefaultValueCatalog = "";
	internal const string DefaultValueCharacterSet = "UTF8";
	internal const int DefaultValueDialect = 3;
	internal const int DefaultValuePacketSize = 8192;
	internal const bool DefaultValuePooling = true;
	internal const int DefaultValueConnectionLifetime = 0;
	internal const int DefaultValueMinPoolSize = 0;
	internal const int DefaultValueMaxPoolSize = 100;
	internal const int DefaultValueConnectionTimeout = 15;
	internal const int DefaultValueFetchSize = 200;
	internal const FbServerType DefaultValueServerType = FbServerType.Default;
	internal const IsolationLevel DefaultValueIsolationLevel = IsolationLevel.ReadCommitted;
	internal const bool DefaultValueRecordsAffected = true;
	internal const bool DefaultValueEnlist = true;
	internal const string DefaultValueClientLibrary = "fbembed";
	internal const int DefaultValueDbCachePages = 0;
	internal const bool DefaultValueNoDbTriggers = false;
	internal const bool DefaultValueNoGarbageCollect = false;
	internal const bool DefaultValueCompression = false;
	internal const byte[] DefaultValueCryptKey = null;
	internal const FbWireCrypt DefaultValueWireCrypt = FbWireCrypt.Enabled;
	internal const string DefaultValueApplicationName = "";
	internal const int DefaultValueCommandTimeout = 0;
	internal const int DefaultValueParallelWorkers = 0;



	// SourceInformation properties
	internal const string DefaultValueDslDataSourceName = null;
	internal const string DefaultValueDslDataSourceProduct = null;
	internal const string DefaultValueDslDataSourceVersion = null;


	// SourceInformation additional titlecased connection properties for Root
	internal const string DefaultValueDslCatalog = null;
	internal const int DefaultValueDslPortNumber = int.MinValue;
	internal const int DefaultValueDslServerType = int.MinValue;
	internal const string DefaultValueDslUserId = null;

	// SourceInformation additional connection derived properties for Root
	internal const string DefaultValueDslDataset = null;

	// SourceInformation additional new properties for root
	internal const int DefaultValueDslMemoryUsage = int.MinValue;
	internal const int DefaultValueDslActiveUsers = int.MinValue;





	// Default descriptor keys
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



	// SourceInformation properties
	internal const string DefaultKeyDslDataSourceName = "DataSourceName"; 
	internal const string DefaultKeyDslDataSourceProduct = "DataSourceProduct"; 
	internal const string DefaultKeyDslDataSourceVersion = "DataSourceVersion"; 


	// SourceInformation additional titlecased connection properties for Root
	internal const string DefaultKeyDslCatalog = "InitialCatalog";
	internal const string DefaultKeyDslPortNumber = "PortNumber"; // Titelcased Synonym for DefaultKeyPortNumber
	internal const string DefaultKeyDslServerType = "ServerType"; // Titelcased DefaultKeyServerType
	internal const string DefaultKeyDslUserId = "UserId"; // Titlecased DefaultKeyUserId

	// SourceInformation additional connection derived properties for Root
	internal const string DefaultKeyDslDataset = "Dataset"; // New abbreviated InitialCatalog

	// SourceInformation additional new properties for root
	internal const string DefaultKeyDslMemoryUsage = "MemoryUsage"; // New
	internal const string DefaultKeyDslActiveUsers = "ActiveUsers"; // New


	#endregion



	#region Descriptors lookup

	internal static readonly KeyValuePair<string, string>[] Descriptors = new KeyValuePair<string, string>[28]
	{
		StringPair( DefaultKeyParallelWorkers, "ParallelWorkers" ),
		StringPair( DefaultKeyIsolationLevel, "IsolationLevel" ),
		StringPair( DefaultKeyApplicationName, "ApplicationName" ),
		StringPair( DefaultKeyFetchSize, "FetchSize" ),
		StringPair( DefaultKeyDbCachePages, "DbCachePages" ),
		StringPair( DefaultKeyCharacterSet, "Charset" ),
		StringPair( DefaultKeyRecordsAffected, "ReturnRecordsAffected" ),
		StringPair( DefaultKeyRoleName, "Role" ),
		StringPair( DefaultKeyCatalog, "initial catalog" ),
		StringPair( DefaultKeyNoGarbageCollect, "NoGarbageCollect" ),
		StringPair( DefaultKeyIsolationLevel, "Dialect" ),
		StringPair( DefaultKeyDataSource, "DataSource" ),
		StringPair( DefaultKeyMaxPoolSize, "MaxPoolSize" ),
		StringPair( DefaultKeyWireCrypt, "WireCrypt" ),
		StringPair( DefaultKeyConnectionTimeout, "ConnectionTimeout" ),
		StringPair( DefaultKeyPortNumber, "port number" ),
		StringPair( DefaultKeyConnectionLifetime, "ConnectionLifeTime" ),
		StringPair( DefaultKeyCompression, "Compression" ),
		StringPair( DefaultKeyMinPoolSize, "MinPoolSize" ),
		StringPair( DefaultKeyPooling, "Pooling" ),
		StringPair( DefaultKeyUserId, "UserID" ),
		StringPair( DefaultKeyCryptKey,  "CryptKey" ),
		StringPair( DefaultKeyClientLibrary, "ClientLibrary" ),
		StringPair( DefaultKeyPacketSize, "PacketSize" ),
		StringPair( DefaultKeyCommandTimeout, "CommandTimeout" ),
		StringPair( DefaultKeyNoDbTriggers, "NoDatabaseTriggers" ),
		StringPair( DefaultKeyEnlist, "Enlist" ),
		StringPair( DefaultKeyServerType, "ServerType" )
	};
	#endregion


	#region Synonyms lookup

	internal static readonly IDictionary<string, string> Synonyms = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
		{
			{ DefaultKeyDataSource, DefaultKeyDataSource },
			{ "datasource", DefaultKeyDataSource },
			{ "server", DefaultKeyDataSource },
			{ "host", DefaultKeyDataSource },
			{ DefaultKeyPortNumber, DefaultKeyPortNumber },
			{ "port", DefaultKeyPortNumber },
			{ DefaultKeyCatalog, DefaultKeyCatalog },
			{ "database", DefaultKeyCatalog },
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

			// Additional SourceInformation property synonyms
			{ DefaultKeyDslDataSourceName.ToLower(), DefaultKeyDataSource },
			{ DefaultKeyDslCatalog.ToLower(), DefaultKeyCatalog },
			{ DefaultKeyDslPortNumber.ToLower(), DefaultKeyPortNumber },
		};


	// SourceInformation descriptor key synonyms
	internal static readonly IDictionary<string, string> DslSynonyms = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
		{
			// Additional SourceInformation descriptor key synonyms
			{ DefaultKeyDslDataSourceName, DefaultKeyDslDataSourceName },
			{ DefaultKeyDataSource, DefaultKeyDslDataSourceName },
			{ "DataSource", DefaultKeyDslDataSourceName },
			{ "Server", DefaultKeyDslDataSourceName },
			{ "Host", DefaultKeyDslDataSourceName },

			{ DefaultKeyDslDataSourceProduct, DefaultKeyDslDataSourceProduct },
			{ "DataSourceProductName", DefaultKeyDslDataSourceProduct }, // FbMetaData

			{ DefaultKeyDslDataSourceVersion, DefaultKeyDslDataSourceVersion },
			{ "DataSourceProductVersion", DefaultKeyDslDataSourceVersion }, // FbMetaData

			// SourceInformation additional titlecased connection properties for Root
			{ DefaultKeyDslCatalog, DefaultKeyDslCatalog},
			{ DefaultKeyCatalog, DefaultKeyDslCatalog},
			{ "Database", DefaultKeyDslCatalog},

			{ DefaultKeyDslPortNumber, DefaultKeyDslPortNumber },
			{ DefaultKeyPortNumber, DefaultKeyDslPortNumber },
			{ "port", DefaultKeyDslPortNumber },

			{ DefaultKeyDslServerType, DefaultKeyDslServerType },
			{ DefaultKeyServerType, DefaultKeyDslServerType },

			{ DefaultKeyDslUserId, DefaultKeyDslUserId },
			{ DefaultKeyUserId, DefaultKeyDslUserId },
			{ "uid", DefaultKeyDslUserId },
			{ "user", DefaultKeyDslUserId },
			{ "user name", DefaultKeyDslUserId },
			{ "username", DefaultKeyDslUserId },

			// SourceInformation additional connection derived properties for Root
			{ DefaultKeyDslDataset, DefaultKeyDslDataset }, 

			// SourceInformation additional new properties for root
			{ DefaultKeyDslMemoryUsage, DefaultKeyDslMemoryUsage },
			{ DefaultKeyDslActiveUsers, DefaultKeyDslActiveUsers }
		};


	// Root synonyms conversion from DataSourceInformation
	internal static readonly KeyValuePair<string, string>[] RootSynonyms = new KeyValuePair<string, string>[2]
		{
			StringPair( DefaultKeyDslDataSourceName, "Server" ),
			StringPair( DefaultKeyDslDataset, "Database" )
		};



	// Default values lookup
	internal static readonly IDictionary<string, object> DefaultValues = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
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
			{ DefaultKeyCryptKey, DBNull.Value },
			{ DefaultKeyWireCrypt, DefaultValueWireCrypt },
			{ DefaultKeyApplicationName, DefaultValueApplicationName },
			{ DefaultKeyCommandTimeout, DefaultValueCommandTimeout },
			{ DefaultKeyParallelWorkers, DefaultValueParallelWorkers },


			// SourceInformation properties
			{ DefaultKeyDslDataSourceName, DefaultValueDslDataSourceName },
			{ DefaultKeyDslDataSourceProduct, DefaultValueDslDataSourceProduct },
			{ DefaultKeyDslDataSourceVersion, DefaultValueDslDataSourceVersion },

			// SourceInformation additional titlecased connection properties for Root
			{ DefaultKeyDslCatalog, DefaultValueDslCatalog },
			{ DefaultKeyDslPortNumber, DefaultValueDslPortNumber },
			{ DefaultKeyDslServerType, DefaultValueDslServerType },
			{ DefaultKeyDslUserId, DefaultValueDslUserId },

			// SourceInformation additional connection derived properties for Root
			{ DefaultKeyDslDataset, DefaultValueDslDataset },

			{ DefaultKeyDslMemoryUsage, DefaultValueDslMemoryUsage },
			{ DefaultKeyDslActiveUsers, DefaultValueDslActiveUsers }
		};


	// SourceInformation Types defaults
	internal static readonly KeyValuePair<string, object>[] DslDefaultValues = new KeyValuePair<string, object>[10]
		{
			// SourceInformation properties
			ValuePair( DefaultKeyDslDataSourceName, DefaultValueDslDataSourceName ),
			ValuePair( DefaultKeyDslDataSourceProduct, DefaultValueDslDataSourceProduct ),
			ValuePair( DefaultKeyDslDataSourceVersion, DefaultValueDslDataSourceVersion ), 

			// SourceInformation additional titlecased connection properties for Root
			ValuePair( DefaultKeyDslCatalog, DefaultValueDslCatalog ),
			ValuePair( DefaultKeyDslPortNumber, DefaultValueDslPortNumber ),
			ValuePair( DefaultKeyDslServerType, DefaultValueDslServerType ),
			ValuePair( DefaultKeyDslUserId, DefaultValueDslUserId),

			// SourceInformation additional connection derived properties for Root
			ValuePair( DefaultKeyDslDataset, DefaultValueDslDataset ),

			ValuePair( DefaultKeyDslMemoryUsage, DefaultValueDslMemoryUsage ),
			ValuePair( DefaultKeyDslActiveUsers, DefaultValueDslActiveUsers )
		};



	// Types lookup
	internal static readonly KeyValuePair<string, Type>[] SystemTypes = new KeyValuePair<string, Type>[39]
		{
			TypePair( DefaultKeyDataSource, typeof(string) ),
			TypePair( DefaultKeyPortNumber, typeof(int) ),
			TypePair( DefaultKeyCatalog, typeof(string) ),
			TypePair( DefaultKeyUserId, typeof(string) ),
			TypePair( DefaultKeyPassword, typeof(string) ),
			TypePair( DefaultKeyRoleName, typeof(string) ),
			TypePair( DefaultKeyCharacterSet, typeof(string) ),
			TypePair( DefaultKeyDialect, typeof(int) ),
			TypePair( DefaultKeyPacketSize, typeof(int) ),
			TypePair( DefaultKeyPooling, typeof(bool) ),
			TypePair( DefaultKeyConnectionLifetime, typeof(int) ),
			TypePair( DefaultKeyMinPoolSize, typeof(int) ),
			TypePair( DefaultKeyMaxPoolSize, typeof(int) ),
			TypePair( DefaultKeyConnectionTimeout, typeof(int) ),
			TypePair( DefaultKeyFetchSize, typeof(int) ),
			TypePair( DefaultKeyServerType, typeof(int) ),
			TypePair( DefaultKeyIsolationLevel, typeof(int) ),
			TypePair( DefaultKeyRecordsAffected, typeof(int) ),
			TypePair( DefaultKeyEnlist, typeof(bool) ),
			TypePair( DefaultKeyClientLibrary, typeof(string) ),
			TypePair( DefaultKeyDbCachePages, typeof(int) ),
			TypePair( DefaultKeyNoDbTriggers, typeof(bool) ),
			TypePair( DefaultKeyNoGarbageCollect, typeof(bool) ),
			TypePair( DefaultKeyCompression, typeof(bool) ),
			TypePair( DefaultKeyCryptKey, typeof(string) ),
			TypePair( DefaultKeyWireCrypt, typeof(int) ),
			TypePair( DefaultKeyApplicationName, typeof(string) ),
			TypePair( DefaultKeyCommandTimeout, typeof(int) ),
			TypePair( DefaultKeyParallelWorkers, typeof(int) ),


			// SourceInformation properties
			TypePair( DefaultKeyDslDataSourceName, typeof(string) ),
			TypePair( DefaultKeyDslDataSourceProduct, typeof(string) ),
			TypePair( DefaultKeyDslDataSourceVersion, typeof(string) ),

			// SourceInformation additional titlecased connection properties for Root
			TypePair( DefaultKeyDslCatalog, typeof(string) ),
			TypePair( DefaultKeyDslPortNumber, typeof(int) ),
			TypePair( DefaultKeyDslServerType, typeof(int) ),
			TypePair( DefaultKeyDslUserId, typeof(string) ),

			// SourceInformation additional connection derived properties for Root
			TypePair( DefaultKeyDslDataset, typeof(string) ),

			// SourceInformation additional new properties for root
			TypePair( DefaultKeyDslMemoryUsage, typeof(int) ),
			TypePair( DefaultKeyDslActiveUsers, typeof(int) )
		};

	// SourceInformation types lookup
	internal static readonly KeyValuePair<string, Type>[] DslTypes = new KeyValuePair<string, Type>[10]
		{
			// SourceInformation properties
			TypePair( DefaultKeyDslDataSourceName, typeof(string) ),
			TypePair( DefaultKeyDslDataSourceProduct, typeof(string) ),
			TypePair( DefaultKeyDslDataSourceVersion, typeof(string) ),

			// SourceInformation additional titlecased connection properties for Root
			TypePair( DefaultKeyDslCatalog, typeof(string) ),
			TypePair( DefaultKeyDslPortNumber, typeof(int) ),
			TypePair( DefaultKeyDslServerType, typeof(int) ),
			TypePair( DefaultKeyDslUserId, typeof(string) ),

			// SourceInformation additional connection derived properties for Root
			TypePair( DefaultKeyDslDataset, typeof(string) ),

			// SourceInformation additional new properties for root
			TypePair( DefaultKeyDslMemoryUsage, typeof(int) ),
			TypePair( DefaultKeyDslActiveUsers, typeof(int) )
		};



	internal static readonly IList<string> EquivalencyKeys = new List<string>()
		{
			{ DefaultKeyDataSource },
			{ DefaultKeyPortNumber },
			{ DefaultKeyUserId },
			{ DefaultKeyRoleName },
			{ DefaultKeyCatalog },
			{ DefaultKeyCharacterSet },
			{ DefaultKeyDialect },
			{ DefaultKeyServerType },
			{ DefaultKeyClientLibrary },
			{ DefaultKeyNoDbTriggers },
			{ DefaultKeyCryptKey },
			{ DefaultKeyWireCrypt },
			{ DefaultKeyApplicationName }
		};


	/// <summary>
	/// Mandatory connection properties excluding sensitive information (password)
	/// </summary>
	internal static readonly string[] PublicMandatoryProperties =
	{
		DefaultKeyDataSource,
		DefaultKeyCatalog,
		DefaultKeyUserId
	};


	/// <summary>
	/// Mandatory connection properties including sensitive information (password)
	/// </summary>
	internal static readonly string[] ProtectedMandatoryProperties =
	{
		DefaultKeyDataSource,
		DefaultKeyCatalog,
		DefaultKeyUserId,
		DefaultKeyPassword
	};

	#endregion



	#region Internal Static Methods

	internal delegate bool TryGetValueDelegate(string key, out object value);

	internal static short GetInt16(string key, TryGetValueDelegate tryGetValue, short defaultValue = default)
	{
		// Diag.Trace();
		return tryGetValue(key, out var value)
			? Convert.ToInt16(value, CultureInfo.InvariantCulture)
			: defaultValue;
	}

	internal static int GetInt32(string key, TryGetValueDelegate tryGetValue, int defaultValue = default)
	{
		// Diag.Trace();
		return tryGetValue(key, out var value)
			? Convert.ToInt32(value, CultureInfo.InvariantCulture)
			: defaultValue;
	}

	internal static long GetInt64(string key, TryGetValueDelegate tryGetValue, long defaultValue = default)
	{
		// Diag.Trace();
		return tryGetValue(key, out var value)
			? Convert.ToInt64(value, CultureInfo.InvariantCulture)
			: defaultValue;
	}

	internal static string GetString(string key, TryGetValueDelegate tryGetValue, string defaultValue = default)
	{
		// Diag.Trace();
		return tryGetValue(key, out var value)
			? Convert.ToString(value, CultureInfo.InvariantCulture)
			: defaultValue;
	}

	internal static bool GetBoolean(string key, TryGetValueDelegate tryGetValue, bool defaultValue = default)
	{
		// Diag.Trace();
		return tryGetValue(key, out var value)
			? Convert.ToBoolean(value, CultureInfo.InvariantCulture)
			: defaultValue;
	}

	internal static byte[] GetBytes(string key, TryGetValueDelegate tryGetValue, byte[] defaultValue = default)
	{
		// Diag.Trace();
		return tryGetValue(key, out var value)
			? (byte[])value
			: defaultValue;
	}

	internal static FbServerType GetServerType(string key, TryGetValueDelegate tryGetValue, FbServerType defaultValue = default)
	{
		// Diag.Trace();
		return tryGetValue(key, out var value)
			? (FbServerType)value
			: defaultValue;
	}

	internal static IsolationLevel GetIsolationLevel(string key, TryGetValueDelegate tryGetValue, IsolationLevel defaultValue = default)
	{
		// Diag.Trace();
		return tryGetValue(key, out var value)
			? (IsolationLevel)value
			: defaultValue;
	}

	internal static FbWireCrypt GetWireCrypt(string key, TryGetValueDelegate tryGetValue, FbWireCrypt defaultValue = default)
	{
		// Diag.Trace();
		return tryGetValue(key, out var value)
			? (FbWireCrypt)value
			: defaultValue;
	}

	#endregion

	#region Private Static Methods

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
	private static string ExpandDataDirectory(string s)
	{
		// Diag.Trace();
		const string DataDirectoryKeyword = "|DataDirectory|";
		if (s == null)
			return s;

		var dataDirectoryLocation = (AppDomain.CurrentDomain.GetData("DataDirectory") ?? string.Empty) as string;
		var pattern = string.Format("{0}{1}?", Regex.Escape(DataDirectoryKeyword), Regex.Escape(Path.DirectorySeparatorChar.ToString()));
		return Regex.Replace(s, pattern, dataDirectoryLocation + Path.DirectorySeparatorChar, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
	private static T ParseEnum<T>(string value, string name) where T : struct
	{
		// Diag.Trace();
		if (!Enum.TryParse<T>(value, true, out var result))
			throw NotSupported(name);
		return result;
	}

	private static Exception NotSupported(string name)
	{
		NotSupportedException ex = new NotSupportedException($"Not supported '{name}'.");
		Diag.Dug(ex);
		return ex;
	}



	private static KeyValuePair<string, Type> TypePair(string key, Type value)
	{
		return new KeyValuePair<string, Type>(key, value);
	}



	private static KeyValuePair<string, object> ValuePair(string key, object value)
	{
		return new KeyValuePair<string, object>(key, value);
	}

	private static KeyValuePair<string, string> StringPair(string key, string value)
	{
		return new KeyValuePair<string, string>(key, value);
	}

	public static Type GetType(string name)
	{
		KeyValuePair<string, Type> pair = Array.Find(SystemTypes,
			(KeyValuePair<string, Type> obj) => obj.Key.Equals(name, StringComparison.OrdinalIgnoreCase));

		if (pair.Key == null)
			return null;

		return pair.Value;
	}

	public static Type GetDslType(string name)
	{
		KeyValuePair<string, Type> pair = Array.Find(DslTypes,
			(KeyValuePair<string, Type> obj) => obj.Key.Equals(name, StringComparison.OrdinalIgnoreCase));

		if (pair.Key == null)
			return null;

		return pair.Value;
	}

	public static bool DslType(string name)
	{
		KeyValuePair<string, Type> pair = Array.Find(DslTypes,
			(KeyValuePair<string, Type> obj) => obj.Key.Equals(name, StringComparison.OrdinalIgnoreCase));

		return (pair.Key != null);
	}

	public static string Descriptor(string name)
	{
		KeyValuePair<string, string> pair = Array.Find(Descriptors,
			(KeyValuePair<string, string> obj) => obj.Key.Equals(name, StringComparison.OrdinalIgnoreCase));

		return pair.Value;
	}

	#endregion
}
