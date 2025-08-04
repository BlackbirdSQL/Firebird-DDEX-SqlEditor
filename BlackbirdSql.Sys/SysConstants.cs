
using System.Data;
using System.Diagnostics.CodeAnalysis;
using BlackbirdSql.Sys.Enums;
using BlackbirdSql.Sys.Properties;



namespace BlackbirdSql;

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Preference")]


// =========================================================================================================
//											SysConstants Class
//
/// <summary>
/// Built-in db constants class.
/// </summary>
// =========================================================================================================
internal static class SysConstants
{

	internal static string S_DatasetKeyFormat => Resources.DatasetKeyFormat;
	internal static string S_DatasetKeyAlternateFormat => Resources.DatasetKeyAlternateFormat;



	// ---------------------------------------------------------------------------------
	#region Describer Flags - SysConstants
	// ---------------------------------------------------------------------------------


	internal const int D_Connection = 0x1;
	internal const int D_Advanced = 0x2;
	internal const int D_Public = 0x4;
	internal const int D_Default = D_Advanced | D_Public;
	internal const int D_Mandatory = 0x8;
	internal const int D_Derived = 0x10;
	internal const int D_HasReadOnly = 0x20;
	internal const int D_DefaultReadOnly = D_Advanced | D_Public | D_HasReadOnly;
	internal const int D_ExtendedType = 0x40;
	internal const int D_Extended = D_Default | D_ExtendedType;
	internal const int D_InternalType = 0x80;
	internal const int D_Internal = D_Extended | D_InternalType;


	#endregion Describer Flags



	// ---------------------------------------------------------------------------------
	#region DbConnectionString Property Names - SysConstants
	// ---------------------------------------------------------------------------------

	// Built-in property descriptor keys
	internal const string C_KeyDataSource = "DataSource";
	internal const string C_KeyPort = "Port";
	internal const string C_KeyServerType = "ServerType";
	internal const string C_KeyDatabase = "Database";
	internal const string C_KeyUserID = "UserID";
	internal const string C_KeyPassword = "Password";
	internal const string C_KeyRole = "Role";
	internal const string C_KeyDialect = "Dialect";
	internal const string C_KeyCharset = "Charset";
	internal const string C_KeyNoDatabaseTriggers = "NoDatabaseTriggers";
	internal const string C_KeyPacketSize = "PacketSize";
	internal const string C_KeyConnectionTimeout = "ConnectionTimeout";
	internal const string C_KeyPooling = "Pooling";
	internal const string C_KeyConnectionLifeTime = "ConnectionLifeTime";
	internal const string C_KeyMinPoolSize = "MinPoolSize";
	internal const string C_KeyMaxPoolSize = "MaxPoolSize";
	internal const string C_KeyFetchSize = "FetchSize";
	internal const string C_KeyIsolationLevel = "IsolationLevel";
	internal const string C_KeyReturnRecordsAffected = "ReturnRecordsAffected";
	internal const string C_KeyEnlist = "Enlist";
	internal const string C_KeyClientLibrary = "ClientLibrary";
	internal const string C_KeyDbCachePages = "DbCachePages";
	internal const string C_KeyNoGarbageCollect = "NoGarbageCollect";
	internal const string C_KeyCompression = "Compression";
	internal const string C_KeyCryptKey = "CryptKey";
	internal const string C_KeyWireCrypt = "WireCrypt";
	internal const string C_KeyApplicationName = "ApplicationName";
	internal const string C_KeyCommandTimeout = "CommandTimeout";
	internal const string C_KeyParallelWorkers = "ParallelWorkers";


	// Extended property descriptor keys
	internal const string C_KeyExDatasetName = "DatasetName";
	internal const string C_KeyExConnectionName = "ConnectionName";


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
	internal const string C_DefaultDataSource = "";
	internal const int C_DefaultPort = 3050;
	internal const EnServerType C_DefaultServerType = EnServerType.Default;
	internal const string C_DefaultDatabase = "";
	internal const string C_DefaultUserID = "";
	internal const string C_DefaultPassword = "";
	internal const string C_DefaultRole = "";
	internal const int C_DefaultDialect = 3;
	internal const string C_DefaultCharset = "UTF8";
	internal const bool C_DefaultNoDatabaseTriggers = false;
	internal const int C_DefaultPacketSize = 8192;
	internal const int C_DefaultConnectionTimeout = 15;
	internal const bool C_DefaultPooling = true;
	internal const int C_DefaultConnectionLifeTime = 0;
	internal const int C_DefaultMinPoolSize = 0;
	internal const int C_DefaultMaxPoolSize = 100;
	internal const int C_DefaultFetchSize = 200;
	internal const IsolationLevel C_DefaultIsolationLevel = IsolationLevel.ReadCommitted;
	internal const bool C_DefaultReturnRecordsAffected = true;
	internal const bool C_DefaultEnlist = true;
	internal const string C_DefaultClientLibrary = "fbembed";
	internal const int C_DefaultDbCachePages = 0;
	internal const bool C_DefaultNoGarbageCollect = false;
	internal const bool C_DefaultCompression = false;
	internal const byte[] C_DefaultCryptKey = null;
	internal const EnWireCrypt C_DefaultWireCrypt = EnWireCrypt.Enabled;
	internal const string C_DefaultApplicationName = "";
	internal const int C_DefaultCommandTimeout = 0;
	internal const int C_DefaultParallelWorkers = 0;


	// Extended property defaults
	internal const string C_DefaultExDatasetName = "";
	internal const string C_DefaultExConnectionName = "";


	#endregion DbConnectionString Property Default Values

}
