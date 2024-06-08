
using System;
using System.Data;
using System.Security;
using BlackbirdSql.Sys.Enums;
using BlackbirdSql.Sys.Interfaces;
using BlackbirdSql.Sys.Properties;

namespace BlackbirdSql.Sys;


// =========================================================================================================
//											SysConstants Class
//
/// <summary>
/// Core db constants class.
/// </summary>
// =========================================================================================================
public static class SysConstants
{
	public static string DatasetKeyFormat => Resources.DatasetKeyFormat;
	public static string DatasetKeyAlternateFormat => Resources.DatasetKeyAlternateFormat;


	// ---------------------------------------------------------------------------------
	#region DbConnectionString Property Names - SysConstants
	// ---------------------------------------------------------------------------------

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




	// Built-in property descriptor keys
	public const string C_KeyDataSource = "DataSource";
	public const string C_KeyPort = "Port";
	public const string C_KeyServerType = "ServerType";
	public const string C_KeyDatabase = "Database";
	public const string C_KeyUserID = "UserID";
	public const string C_KeyPassword = "Password";

	// Extended property descriptor keys
	public const string C_KeyExIcon = "Icon";
	public const string C_KeyExDataset = "Dataset";
	public const string C_KeyExDatasetKey = "DatasetKey";
	public const string C_KeyExConnectionKey = "ConnectionKey";
	public const string C_KeyExDatasetId = "DatasetId";
	public const string C_KeyExConnectionName = "ConnectionName";
	public const string C_KeyExConnectionSource = "ConnectionSource";
	public const string C_KeyExDisplayName = "DisplayName";


	// External (non-paramameter) property descriptor
	public const string C_KeyExServerEngine = "ServerEngine";
	public const string C_KeyExServerVersion = "ServerVersion";
	public const string C_KeyExPersistPassword = "PersistPassword";
	public const string C_KeyExAdministratorLogin = "AdministratorLogin";
	public const string C_KeyExServerFullyQualifiedDomainName = "ServerFullyQualifiedDomainName";
	public const string C_KeyExOtherParams = "OtherParams";
	public const string C_KeyExEdmx = "edmx";
	public const string C_KeyExEdmu = "edmu";

	// Internal (hidden) property keys
	public const string C_KeyExInMemoryPassword = "InMemoryPassword";


	// Other connection constants.
	public const string C_KeyExConnectionUrl = "ConnectionUrl";
	public const string C_KeyExConnectionString = "ConnectionString";

	#endregion DbConnectionString Property Names




	// ---------------------------------------------------------------------------------
	#region DbConnectionString Property Default Values - SysConstants
	// ---------------------------------------------------------------------------------

	// This is problematic because really the native ".Data.dll" should hold these
	// values, which we should then access through the DescriberDictionary in
	// Csb.Describers, but they're currently being used as part of
	// NativeDbCsbProxy's property type descriptors in the DefaultValue attribute.
	// These are in turn used by the Ddex's IVsDataConnectionUIProperties and
	// IVsDataConnectionProperties implementations when those classes implement the
	// ICustomTypeDescriptor interface, which in turn is needed by the connection
	// dialogs.
	// This is easily remedied by constructing type descriptor collections in the native
	// data library; but we probably shouldn't have any properties defined in
	// NativeDbCsbProxy except for properties that are common across the different
	// database engines we may cater for in the future, such as PostgreSQL.
	// The same applies to the Language service which should also get it's native db
	// metdata from the native data library. We'll gradually untagle everything.
	// The intention is to ultimately be able to have multiple db engine ".Data.dll"
	// libraries as services.


	// Built-in property defaults
	public const string C_DefaultDataSource = "";
	public const int C_DefaultPort = 3050;
	public const EnServerType C_DefaultServerType = EnServerType.Default;
	public const string C_DefaultDatabase = "";
	public const string C_DefaultUserID = "";
	public const string C_DefaultPassword = "";


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
	public const EnWireCrypt C_DefaultWireCrypt = EnWireCrypt.Enabled;
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




	// Extended property defaults
	public const IBIconType C_DefaultExIcon = null;
	public const string C_DefaultExDataset = "";
	public const string C_DefaultExDatasetKey = "";
	public const string C_DefaultExConnectionKey = "";
	public const string C_DefaultExDatasetId = "";
	public const string C_DefaultExConnectionName = "";
	public const EnConnectionSource C_DefaultExConnectionSource = EnConnectionSource.None;

	// External (non-paramameter) property defaults 
	public const EnEngineType C_DefaultExServerEngine = EnEngineType.Unknown;
	public const Version C_DefaultExServerVersion = null;
	public const bool C_DefaultExPersistPassword = false;
	public const string C_DefaultExAdministratorLogin = "";
	public const string C_DefaultExServerFullyQualifiedDomainName = "localhost";
	public const string C_DefaultExOtherParams = null;

	// Internal (hidden) property defaults
	public const SecureString C_DefaultExInMemoryPassword = null;


	#endregion DbConnectionString Property Default Values





	// ---------------------------------------------------------------------------------
	#region Model Engine Miscellanoeus values and defaults - SysConstants
	// ---------------------------------------------------------------------------------


	public const string C_DefaultBatchSeparator = ";";

	public const int C_DefaultSetRowCount = 0;
	public const EnBlobSubType C_DefaultSetBlobDisplay = EnBlobSubType.Text;
	public const int C_DefaultExecutionTimeout = 0;
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
