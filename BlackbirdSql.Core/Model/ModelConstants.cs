/*
 *	Replica to expose the FirebirdClient ConnectionString constantsas well as additional members
 *	to support SourceInformation, Root ObjectSelector and SqlServer style ConnectionInfo classes
 */

//$OriginalAuthors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)

using System;
using System.Data;
using BlackbirdSql.Core.Model.Enums;
using FirebirdSql.Data.FirebirdClient;




namespace BlackbirdSql.Core.Model;


// =========================================================================================================
//											ModelConstants Class
//
/// <summary>
/// The Firebird schema constants class.
/// </summary>
// =========================================================================================================
public static class ModelConstants
{

	// ---------------------------------------------------------------------------------
	#region Property Names - ModelConstants
	// ---------------------------------------------------------------------------------


	// Built-in default parameter keys
	public const string C_KeyFbRole = "role name";
	public const string C_KeyFbDialect = "dialect";
	public const string C_KeyFbCharset = "character set";
	public const string C_KeyFbNoDatabaseTriggers = "no db triggers";
	public const string C_KeyFbPacketSize = "packet size";
	public const string C_KeyFbConnectionTimeout = "connection timeout";
	public const string C_KeyFbPooling = "pooling";
	public const string C_KeyFbConnectionLifeTime = "connection lifetime";
	public const string C_KeyFbMinPoolSize = "min pool size";
	public const string C_KeyFbMaxPoolSize = "max pool size";
	public const string C_KeyFbFetchSize = "fetch size";
	public const string C_KeyFbIsolationLevel = "isolation level";
	public const string C_KeyFbReturnRecordsAffected = "records affected";
	public const string C_KeyFbEnlist = "enlist";
	public const string C_KeyFbClientLibrary = "client library";
	public const string C_KeyFbDbCachePages = "cache pages";
	public const string C_KeyFbNoGarbageCollect = "no garbage collect";
	public const string C_KeyFbCompression = "compression";
	public const string C_KeyFbCryptKey = "crypt key";
	public const string C_KeyFbWireCrypt = "wire crypt";
	public const string C_KeyFbApplicationName = "application name";
	public const string C_KeyFbCommandTimeout = "command timeout";
	public const string C_KeyFbParallelWorkers = "parallel workers";


	// Built-in property descriptor keys
	public const string C_KeyRole = "Role";
	public const string C_KeyDialect = "Dialect";
	public const string C_KeyCharset = "Charset";
	public const string C_KeyNoDatabaseTriggers = "NoDatabaseTriggers";
	public const string C_KeyPacketSize = "PacketSize";
	public const string C_KeyConnectionTimeout = "ConnectionTimeout";
	public const string C_KeyPooling = "Pooling";
	public const string C_KeyConnectionLifeTime = "ConnectionLifeTime";
	public const string C_KeyMinPoolSize = "MinPoolSize";
	public const string C_KeyMaxPoolSize = "MaxPoolSize";
	public const string C_KeyFetchSize = "FetchSize";
	public const string C_KeyIsolationLevel = "IsolationLevel";
	public const string C_KeyReturnRecordsAffected = "ReturnRecordsAffected";
	public const string C_KeyEnlist = "Enlist";
	public const string C_KeyClientLibrary = "ClientLibrary";
	public const string C_KeyDbCachePages = "DbCachePages";
	public const string C_KeyNoGarbageCollect = "NoGarbageCollect";
	public const string C_KeyCompression = "Compression";
	public const string C_KeyCryptKey = "CryptKey";
	public const string C_KeyWireCrypt = "WireCrypt";
	public const string C_KeyApplicationName = "ApplicationName";
	public const string C_KeyCommandTimeout = "CommandTimeout";
	public const string C_KeyParallelWorkers = "ParallelWorkers";

	// Extended property descriptor keys
	public const string C_KeyExObjectType = "ObjectType";
	public const string C_KeyExExplorerTreeName = "ExplorerTreeName";
	public const string C_KeyExObjectName = "ObjectName";
	public const string C_KeyExTargetType = "TargetType";
	public const string C_KeyExIsUnique = "IsUnique";

	public const string C_KeyExClientVersion = "ClientVersion";
	public const string C_KeyExMemoryUsage = "MemoryUsage";
	public const string C_KeyExActiveUsers = "ActiveUsers";


	#endregion Property Names




	// ---------------------------------------------------------------------------------
	#region Property Default Values - ModelConstants
	// ---------------------------------------------------------------------------------


	// Built-in property defaults
	public const string C_DefaultRole = "";
	public const int C_DefaultDialect = 3;
	public const string C_DefaultCharset = "UTF8";
	public const bool C_DefaultNoDatabaseTriggers = false;
	public const int C_DefaultPacketSize = 8192;
	public const int C_DefaultConnectionTimeout = 15;
	public const bool C_DefaultPooling = true;
	public const int C_DefaultConnectionLifeTime = 0;
	public const int C_DefaultMinPoolSize = 0;
	public const int C_DefaultMaxPoolSize = 100;
	public const int C_DefaultFetchSize = 200;
	public const IsolationLevel C_DefaultIsolationLevel = IsolationLevel.ReadCommitted;
	public const bool C_DefaultReturnRecordsAffected = true;
	public const bool C_DefaultEnlist = true;
	public const string C_DefaultClientLibrary = "fbembed";
	public const int C_DefaultDbCachePages = 0;
	public const bool C_DefaultNoGarbageCollect = false;
	public const bool C_DefaultCompression = false;
	public const byte[] C_DefaultCryptKey = null;
	public const FbWireCrypt C_DefaultWireCrypt = FbWireCrypt.Enabled;
	public const string C_DefaultApplicationName = "";
	public const int C_DefaultCommandTimeout = 0;
	public const int C_DefaultParallelWorkers = 0;

	// Extended property defaults
	public const EnModelObjectType C_DefaultExObjectType = EnModelObjectType.Unknown;
	public const string C_DefaultExExplorerTreeName = "";
	public const string C_DefaultExObjectName = "";
	public const EnModelTargetType C_DefaultExTargetType = EnModelTargetType.Unknown;
	public const bool C_DefaultExIsUnique = false;

	// External (non-paramameter) property defaults 
	public const Version C_DefaultExClientVersion = null;
	public const string C_DefaultExMemoryUsage = null;
	public const int C_DefaultExActiveUsers = int.MinValue;


	#endregion Property Default Values




	// ---------------------------------------------------------------------------------
	#region Model Engine Miscellanoeus values and defaults - ModelConstants
	// ---------------------------------------------------------------------------------


	public const int C_TestConnectionTimeout = 15;
	public const string C_DefaultBatchSeparator = "GO";

	public const int C_DefaultSetRowCount = 0;
	public const EnBlobSubType C_DefaultSetBlobDisplay = EnBlobSubType.Text;
	public const bool C_DefaultDefaultOleScripting = false;
	public const bool C_DefaultSetCount = true;
	public const bool C_DefaultSetPlanOnly = false;
	public const bool C_DefaultSetPlan = false;
	public const bool C_DefaultSetExplain = false;
	public const bool C_DefaultSetParseOnly = false;
	public const bool C_DefaultSetConcatenationNull = true;
	public const bool C_DefaultSetBail = true;
	public const bool C_DefaultSetPlanText = false;
	public const bool C_DefaultSetStats = false;
	public const bool C_DefaultSetWarnings = false;
	public const bool C_DefaultSetStatisticsIO = false;
	public const int C_DefaultLockTimeout = 0;
	public const bool C_DefaultSuppressHeaders = false;
	public const int C_DefaultGridMaxCharsPerColumnStd = 65535;
	public const int C_DefaultGridMaxCharsPerColumnXml = 2097152;
	public const int C_DefaultTextMaxCharsPerColumnStd = 256;
	public const char C_DefaultTextDelimiter = '\0';


	#endregion Model Engine Miscellanoeus values and defaults




}
