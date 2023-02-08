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


internal static class DslConnectionString
{
	#region Constants
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
			{ DefaultKeyApplicationName },
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
		Diag.Trace();
		return tryGetValue(key, out var value)
			? Convert.ToInt16(value, CultureInfo.InvariantCulture)
			: defaultValue;
	}

	internal static int GetInt32(string key, TryGetValueDelegate tryGetValue, int defaultValue = default)
	{
		Diag.Trace();
		return tryGetValue(key, out var value)
			? Convert.ToInt32(value, CultureInfo.InvariantCulture)
			: defaultValue;
	}

	internal static long GetInt64(string key, TryGetValueDelegate tryGetValue, long defaultValue = default)
	{
		Diag.Trace();
		return tryGetValue(key, out var value)
			? Convert.ToInt64(value, CultureInfo.InvariantCulture)
			: defaultValue;
	}

	internal static string GetString(string key, TryGetValueDelegate tryGetValue, string defaultValue = default)
	{
		Diag.Trace();
		return tryGetValue(key, out var value)
			? Convert.ToString(value, CultureInfo.InvariantCulture)
			: defaultValue;
	}

	internal static bool GetBoolean(string key, TryGetValueDelegate tryGetValue, bool defaultValue = default)
	{
		Diag.Trace();
		return tryGetValue(key, out var value)
			? Convert.ToBoolean(value, CultureInfo.InvariantCulture)
			: defaultValue;
	}

	internal static byte[] GetBytes(string key, TryGetValueDelegate tryGetValue, byte[] defaultValue = default)
	{
		Diag.Trace();
		return tryGetValue(key, out var value)
			? (byte[])value
			: defaultValue;
	}

	internal static FbServerType GetServerType(string key, TryGetValueDelegate tryGetValue, FbServerType defaultValue = default)
	{
		Diag.Trace();
		return tryGetValue(key, out var value)
			? (FbServerType)value
			: defaultValue;
	}

	internal static IsolationLevel GetIsolationLevel(string key, TryGetValueDelegate tryGetValue, IsolationLevel defaultValue = default)
	{
		Diag.Trace();
		return tryGetValue(key, out var value)
			? (IsolationLevel)value
			: defaultValue;
	}

	internal static FbWireCrypt GetWireCrypt(string key, TryGetValueDelegate tryGetValue, FbWireCrypt defaultValue = default)
	{
		Diag.Trace();
		return tryGetValue(key, out var value)
			? (FbWireCrypt)value
			: defaultValue;
	}

	#endregion

	#region Private Static Methods

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
	private static string ExpandDataDirectory(string s)
	{
		Diag.Trace();
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
		Diag.Trace();
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


	#endregion
}
