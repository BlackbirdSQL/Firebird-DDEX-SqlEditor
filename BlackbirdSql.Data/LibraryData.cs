// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)



using System;
using System.Collections.Generic;
using System.Data;
using BlackbirdSql.Sys.Ctl;
using BlackbirdSql.Sys.Enums;

using static BlackbirdSql.Data.DataConstants;
using static BlackbirdSql.SysConstants;



namespace BlackbirdSql.Data;

// =========================================================================================================
//											LibraryData Class
//
/// <summary>
/// System wide and the current data provider (in this case Firebird) specific constants and statics
/// </summary>
// =========================================================================================================
public static class LibraryData
{
	public const string C_Invariant = "FirebirdSql.Data.FirebirdClient";
	public const string C_ProviderFactoryName = "FirebirdClient Data Provider";
	public const string C_ProviderFactoryClassName = "FirebirdSql.Data.FirebirdClient.FirebirdClientFactory";
	public const string C_ProviderFactoryDescription = ".NET Framework Data Provider for Firebird";

	public const string C_EFProvider = "EntityFramework.Firebird";
	public const string C_EFProviderServices = "EntityFramework.Firebird.FbProviderServices";
	public const string C_EFConnectionFactory = "EntityFramework.Firebird.FbConnectionFactory";

	public const string C_DataProviderName = "Firebird Server"; // Firebird
	public const string C_DbEngineName = "Firebird";
	public const string C_SchemaMetaDataXml = "BlackbirdSql.Data.Model.Schema.DslMetaData.xml";

	/// <summary>
	/// The path to the provider's configured connections xml (in this case FlameRobin for Firebird).
	/// </summary>
	public static string S_ExternalUtilityConfigurationPath
		=> Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
		+ "\\flamerobin\\fr_databases.conf";


	public const string C_Protocol = "fbsql++";
	public static string S_Scheme => C_Protocol + "://";
	public const string C_Extension = ".fbsql";

	public const string C_SqlLanguageName = "FB-SQL";

	public const string C_XmlActualPlanColumn = "Firebird_SQL_Server_XML_ActualPlan";
	public const string C_XmlEstimatedPlanColumn = "Firebird_SQL_Server_XML_EstimatedPlan";


	public const string C_InvariantAssemblyFullName = "FirebirdSql.Data.FirebirdClient, Version={0}, Culture=neutral, PublicKeyToken=3750abcc3150b00c";
	public const string C_InvariantAssemblyPrefix = "FirebirdSql.Data.FirebirdClient,";
	public const string C_InvariantAssemblySuffix = ", PublicKeyToken=3750abcc3150b00c";

	public const string C_EntityFrameworkAssemblyFullName = "EntityFramework.Firebird, Version={0}, Culture=neutral, PublicKeyToken=42d22d092898e5f8";
	public const string C_EntityFrameworkAssemblyPrefix = "EntityFramework.Firebird,";
	public const string C_EntityFrameworkAssemblySuffix = ", PublicKeyToken=42d22d092898e5f8";

	public const string C_EFProviderServicesTypeFullName = "EntityFramework.Firebird.FbProviderServices, EntityFramework.Firebird, Version={0}, Culture=neutral, PublicKeyToken=42d22d092898e5f8";



	// Deprecated. Known previous version of FirebirdSql.Data.FirebirdClient that could cause conflicts.
	/*
	public static readonly string[] S_InvariantVersions =
		[
			"10.3.1.0", "10.3.0.0", "10.2.0.0", "10.1.0.0", "10.0.0.0", "9.1.1.0", "9.1.0.0", "9.0.2.0",
			"9.0.1.0", "9.0.0.0", "8.5.4.0", "8.5.3.0", "8.5.2.0", "8.5.1.1", "8.5.1.0", "8.5.0.0",
			"8.0.1.0", "8.0.0.0", "7.10.1.0", "7.10.0.0", "7.5.0.0", "7.1.1.0", "7.1.0.0", "7.0.0.0",
			"6.7.0.0", "6.6.0.0", "6.5.0.0", "6.4.0.0", "6.3.0.0", "6.2.0.1", "6.1.0.0", "6.0.0.0",
			"5.12.1.0", "5.12.0.0", "5.11.0.0", "5.10.0.0", "5.9.1.0", "5.9.0.1", "5.8.1.0", "5.8.0.0",
			"5.7.0.0", "5.6.0.0", "5.5.0.0", "5.1.1.0", "5.1.0.0", "5.0.5.0", "5.0.0.0", "4.10.0.0",
			"4.9.0.0", "4.8.1.1", "4.8.0.0", "4.7.0.0", "4.6.4.0", "4.6.3.0", "4.6.2.0", "4.6.1.0",
			"4.6.0.0", "4.5.2.0", "4.5.1.0", "4.5.0.0", "4.2.0.0", "4.1.5.0", "4.1.0.0", "3.2.0.0",
			"3.1.1.0", "3.1.0.0", "3.0.2.1", "3.0.2.0", "3.0.1.0", "3.0.0.0", "2.7.7.0", "2.7.5.0",
			"2.7.0.0", "2.6.5.0"
		];
	*/


	// Deprecated. Known previous version of EntityFramework.Firebird that could cause conflicts.
	/*
	public static readonly string[] S_EntityFrameworkVersions =
		[
			"10.0.1.0", "10.0.0.0", "9.1.1.0", "9.1.0.0", "9.0.2.0", "9.0.1.0", "9.0.0.0", "8.5.4.0",
			"8.5.3.0", "8.5.2.0", "8.5.1.1", "8.5.1.0", "8.5.0.0", "8.0.1.0", "8.0.0.0", "7.10.1.0",
			"7.10.0.0", "7.5.0.0", "7.1.1.0", "7.1.0.0", "7.0.0.0", "6.7.0.0", "6.6.0.0", "6.5.0.0",
			"6.4.0.0", "6.3.0.0", "6.2.0.1", "6.1.0.0", "6.0.0.0", "5.12.1.0", "5.12.0.0", "5.11.0.0",
			"5.10.0.0", "5.9.1.0", "5.9.0.1", "5.8.1.0", "5.8.0.0", "5.7.0.0", "5.6.0.0", "5.5.0.0",
			"5.1.1.0", "5.1.0.0", "5.0.5.0", "5.0.0.0", "4.10.0.0", "4.9.0.0", "4.8.1.0", "4.8.0.0",
			"4.7.0.0", "4.6.4.0", "4.6.3.0", "4.6.2.0", "4.6.1.0", "4.6.0.0", "4.5.2.0", "4.5.1.0",
			"4.5.0.0", "4.2.0.0", "4.1.5.0", "4.1.0.0"
		];
	*/



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Describer collection of uniform properties used by the FbConnectionStringBuilder
	/// replacement, Csb.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static readonly Describer[] Describers =
	[
		new Describer(C_KeyDataSource, C_KeyDbDataSource, typeof(string), C_DefaultDataSource, D_Connection | D_Public | D_Mandatory), // *
		new Describer(C_KeyPort, C_KeyDbPort, typeof(int), C_DefaultPort, D_Connection | D_Public), // *
		new Describer(C_KeyServerType, C_KeyDbServerType, typeof(EnServerType), C_DefaultServerType, D_Connection | D_Public), // *
		new Describer(C_KeyDatabase, C_KeyDbDatabase, typeof(string), C_DefaultDatabase, D_Connection | D_Public | D_Mandatory), // *
		new Describer(C_KeyUserID, C_KeyDbUserID, typeof(string), C_DefaultUserID, D_Connection | D_Public | D_Mandatory), // *
		new Describer(C_KeyPassword, C_KeyDbPassword, typeof(string), C_DefaultPassword, D_Connection | D_Mandatory),

		new Describer(C_KeyRole, C_KeyDbRole, typeof(string), C_DefaultRole, D_Connection | D_Public), // *
		new Describer(C_KeyDialect, C_KeyDbDialect, typeof(int), C_DefaultDialect, D_Connection | D_Public), // *
		new Describer(C_KeyCharset, C_KeyDbCharset, typeof(string), C_DefaultCharset, D_Connection | D_Public), // *
		new Describer(C_KeyNoDatabaseTriggers, C_KeyDbNoDatabaseTriggers, typeof(bool), C_DefaultNoDatabaseTriggers, D_Default | D_Connection), // *
		new Describer(C_KeyPacketSize, C_KeyDbPacketSize, typeof(int), C_DefaultPacketSize, D_Default | D_Connection),
		new Describer(C_KeyConnectionTimeout, C_KeyDbConnectionTimeout, typeof(int), C_DefaultConnectionTimeout, D_Default | D_Connection),
		new Describer(C_KeyPooling, C_KeyDbPooling, typeof(bool), C_DefaultPooling, D_Default | D_Connection),
		new Describer(C_KeyConnectionLifeTime, C_KeyDbConnectionLifeTime, typeof(int), C_DefaultConnectionLifeTime, D_Default | D_Connection),
		new Describer(C_KeyMinPoolSize, C_KeyDbMinPoolSize, typeof(int), C_DefaultMinPoolSize, D_Default | D_Connection),
		new Describer(C_KeyMaxPoolSize, C_KeyDbMaxPoolSize, typeof(int), C_DefaultMaxPoolSize, D_Default | D_Connection),
		new Describer(C_KeyFetchSize, C_KeyDbFetchSize, typeof(int), C_DefaultFetchSize, D_Default | D_Connection),
		new Describer(C_KeyIsolationLevel, C_KeyDbIsolationLevel, typeof(IsolationLevel), C_DefaultIsolationLevel, D_Default | D_Connection),
		new Describer(C_KeyReturnRecordsAffected, C_KeyDbReturnRecordsAffected, typeof(bool), C_DefaultReturnRecordsAffected, D_Default | D_Connection),
		new Describer(C_KeyEnlist, C_KeyDbEnlist, typeof(bool), C_DefaultEnlist, D_Default | D_Connection),
		new Describer(C_KeyClientLibrary, C_KeyDbClientLibrary, typeof(string), C_DefaultClientLibrary, D_Default | D_Connection),
		new Describer(C_KeyDbCachePages, C_KeyDbDbCachePages, typeof(int), C_DefaultDbCachePages, D_Default | D_Connection),
		new Describer(C_KeyNoGarbageCollect, C_KeyDbNoGarbageCollect, typeof(bool), C_DefaultNoGarbageCollect, D_Default | D_Connection),
		new Describer(C_KeyCompression, C_KeyDbCompression, typeof(bool), C_DefaultCompression, D_Default | D_Connection),
		new Describer(C_KeyCryptKey, C_KeyDbCryptKey, typeof(byte[]), C_DefaultCryptKey, D_Default | D_Connection),
		new Describer(C_KeyWireCrypt, C_KeyDbWireCrypt, typeof(EnWireCrypt), C_DefaultWireCrypt, D_Default | D_Connection),
		new Describer(C_KeyApplicationName, C_KeyDbApplicationName, typeof(string), C_DefaultApplicationName, D_Default | D_Connection),
		new Describer(C_KeyCommandTimeout, C_KeyDbCommandTimeout, typeof(int), C_DefaultCommandTimeout, D_Default | D_Connection),
		new Describer(C_KeyParallelWorkers, C_KeyDbParallelWorkers, typeof(int), C_DefaultParallelWorkers, D_Default | D_Connection),
	];

	public static KeyValuePair<string, string>[] DescriberSynonyms =
	[
		Pair("server", C_KeyDataSource),
		Pair("host", C_KeyDataSource),
		Pair("uid", C_KeyUserID),
		Pair("user", C_KeyUserID),
		Pair("username", C_KeyUserID),
		Pair("user name", C_KeyUserID),
		Pair("userpassword", C_KeyPassword),
		Pair("user password", C_KeyPassword),
		Pair("no triggers", C_KeyNoDatabaseTriggers),
		Pair("nodbtriggers", C_KeyNoDatabaseTriggers),
		Pair("no dbtriggers", C_KeyNoDatabaseTriggers),
		Pair("no database triggers", C_KeyNoDatabaseTriggers),
		Pair("timeout", C_KeyConnectionTimeout),
		Pair("db cache pages", C_KeyDbCachePages),
		Pair("cachepages", C_KeyDbCachePages),
		Pair("pagebuffers", C_KeyDbCachePages),
		Pair("page buffers", C_KeyDbCachePages),
		Pair("wire compression", C_KeyCompression),
		Pair("app", C_KeyApplicationName),
		Pair("parallel", C_KeyParallelWorkers)
	];



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Converts a key and value to a KeyValuePair<string, string>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private static KeyValuePair<string, string> Pair(string key, string value)
	{
		return new KeyValuePair<string, string>(key, value);
	}

}