/*
 *	Replica to expose the FirebirdClient ConnectionString constantsas well as additional members
 *	to support SourceInformation, Root ObjectSelector and SqlServer style ConnectionInfo classes
 */

//$OriginalAuthors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)

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




	// ---------------------------------------------------------------------------------
	#region Default Parameter Names - ModelConstants
	// ---------------------------------------------------------------------------------


	// Firbird default parameters
	public const string C_KeyRoleName = "role name";
	public const string C_KeyCharacterSet = "character set";
	public const string C_KeyDialect = "dialect";
	public const string C_KeyPacketSize = "packet size";
	public const string C_KeyPooling = "pooling";
	public const string C_KeyConnectionLifetime = "connection lifetime";
	public const string C_KeyMinPoolSize = "min pool size";
	public const string C_KeyMaxPoolSize = "max pool size";
	public const string C_KeyConnectionTimeout = "connection timeout";
	public const string C_KeyFetchSize = "fetch size";
	public const string C_KeyIsolationLevel = "isolation level";
	public const string C_KeyRecordsAffected = "records affected";
	public const string C_KeyEnlist = "enlist";
	public const string C_KeyClientLibrary = "client library";
	public const string C_KeyDbCachePages = "cache pages";
	public const string C_KeyNoDbTriggers = "no db triggers";
	public const string C_KeyNoGarbageCollect = "no garbage collect";
	public const string C_KeyCompression = "compression";
	public const string C_KeyCryptKey = "crypt key";
	public const string C_KeyWireCrypt = "wire crypt";
	public const string C_KeyApplicationName = "application name";
	public const string C_KeyCommandTimeout = "command timeout";
	public const string C_KeyParallelWorkers = "parallel workers";


	// Root and Connection node keys
	public const string C_KeyNodeDatasetKey = "DatasetKey";
	public const string C_KeyNodeDataSource = "DataSource";
	public const string C_KeyNodePort = "Port";
	public const string C_KeyNodeDatabase = "Database";
	public const string C_KeyNodeDisplayMember = "DisplayMember";
	public const string C_KeyNodeServerType = "ServerType";
	public const string C_KeyNodeUserId = "UserID";
	public const string C_KeyNodePassword = "Password";
	public const string C_KeyNodeRole = "Role";
	public const string C_KeyNodeCharset = "Charset";
	public const string C_KeyNodeDialect = "Dialect";
	public const string C_KeyNodeNoDbTriggers = "NoDatabaseTriggers";
	public const string C_KeyNodeMemoryUsage = "MemoryUsage";
	public const string C_KeyNodeActiveUsers = "ActiveUsers";


	#endregion Default Parameter Names




	// ---------------------------------------------------------------------------------
	#region Parameter Default Values - ModelConstants
	// ---------------------------------------------------------------------------------


	public const string C_DefaultRoleName = "";
	public const string C_DefaultCharacterSet = "UTF8";
	public const int C_DefaultDialect = 3;
	public const int C_DefaultPacketSize = 8192;
	public const bool C_DefaultPooling = true;
	public const int C_DefaultConnectionLifetime = 0;
	public const int C_DefaultMinPoolSize = 0;
	public const int C_DefaultMaxPoolSize = 100;
	public const int C_DefaultConnectionTimeout = 15;
	public const int C_DefaultFetchSize = 200;
	public const IsolationLevel C_DefaultIsolationLevel = IsolationLevel.ReadCommitted;
	public const bool C_DefaultRecordsAffected = true;
	public const bool C_DefaultEnlist = true;
	public const string C_DefaultClientLibrary = "fbembed";
	public const int C_DefaultDbCachePages = 0;
	public const bool C_DefaultNoDbTriggers = false;
	public const bool C_DefaultNoGarbageCollect = false;
	public const bool C_DefaultCompression = false;
	public const byte[] C_DefaultCryptKey = null;
	public const FbWireCrypt C_DefaultWireCrypt = FbWireCrypt.Enabled;
	public const string C_DefaultApplicationName = "";
	public const int C_DefaultCommandTimeout = 0;
	public const int C_DefaultParallelWorkers = 0;


	// Connection node defaults
	public const string C_DefaultNodeMemoryUsage = null;
	public const int C_DefaultNodeActiveUsers = int.MinValue;


	#endregion Property Default Values


}
