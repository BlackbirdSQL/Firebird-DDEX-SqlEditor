/*
 *	Replica to expose the FirebirdClient ConnectionString constantsas well as additional members
 *	to support SourceInformation, Root ObjectSelector and SqlServer style ConnectionInfo classes
 */

//$OriginalAuthors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)

using System.Data;

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

	public const int C_TestConnectionTimeout = 15;

	// ---------------------------------------------------------------------------------
	#region Default Parameter Names - ModelConstants
	// ---------------------------------------------------------------------------------


	// Firbird default parameters
	internal const string C_KeyRoleName = "role name";
	internal const string C_KeyCharacterSet = "character set";
	internal const string C_KeyDialect = "dialect";
	internal const string C_KeyPacketSize = "packet size";
	internal const string C_KeyPooling = "pooling";
	internal const string C_KeyConnectionLifetime = "connection lifetime";
	internal const string C_KeyMinPoolSize = "min pool size";
	internal const string C_KeyMaxPoolSize = "max pool size";
	internal const string C_KeyConnectionTimeout = "connection timeout";
	internal const string C_KeyFetchSize = "fetch size";
	internal const string C_KeyIsolationLevel = "isolation level";
	internal const string C_KeyRecordsAffected = "records affected";
	internal const string C_KeyEnlist = "enlist";
	internal const string C_KeyClientLibrary = "client library";
	internal const string C_KeyDbCachePages = "cache pages";
	internal const string C_KeyNoDbTriggers = "no db triggers";
	internal const string C_KeyNoGarbageCollect = "no garbage collect";
	internal const string C_KeyCompression = "compression";
	internal const string C_KeyCryptKey = "crypt key";
	internal const string C_KeyWireCrypt = "wire crypt";
	internal const string C_KeyApplicationName = "application name";
	internal const string C_KeyCommandTimeout = "command timeout";
	internal const string C_KeyParallelWorkers = "parallel workers";



	// SourceInformation properties
	internal const string C_KeySIDataSourceName = "DataSourceName";
	internal const string C_KeySIDataSourceProduct = "DataSourceProduct";
	internal const string C_KeySIDataSourceVersion = "DataSourceVersion";


	// SourceInformation additional titlecased connection properties for Root
	internal const string C_KeySICatalog = "InitialCatalog";
	internal const string C_KeySIPortNumber = "PortNumber"; // Titelcased Synonym for C_KeyPortNumber
	internal const string C_KeySIServerType = "ServerType"; // Titelcased C_KeyServerType
	internal const string C_KeySIUserId = "UserId"; // Titlecased C_KeyUserId
	internal const string C_KeySIPassword = "Password"; // Titlecased C_KeyPassword

	// SourceInformation additional connection derived properties 
	internal const string C_KeySIDataset = "Dataset"; // New abbreviated InitialCatalog

	// SourceInformation additional new properties for root
	internal const string C_KeySIMemoryUsage = "MemoryUsage"; // New
	internal const string C_KeySIActiveUsers = "ActiveUsers"; // New


	// Root translated properties from source information
	internal const string C_KeyRootDataSourceName = "Server";
	internal const string C_KeyRootDataset = "Database";


	#endregion Default Parameter Names




	// ---------------------------------------------------------------------------------
	#region Property Default Values - ModelConstants
	// ---------------------------------------------------------------------------------


	internal const string C_DefaultRoleName = "";
	internal const string C_DefaultCharacterSet = "UTF8";
	internal const int C_DefaultDialect = 3;
	internal const int C_DefaultPacketSize = 8192;
	internal const bool C_DefaultPooling = true;
	internal const int C_DefaultConnectionLifetime = 0;
	internal const int C_DefaultMinPoolSize = 0;
	internal const int C_DefaultMaxPoolSize = 100;
	internal const int C_DefaultConnectionTimeout = 15;
	internal const int C_DefaultFetchSize = 200;
	internal const IsolationLevel C_DefaultIsolationLevel = IsolationLevel.ReadCommitted;
	internal const bool C_DefaultRecordsAffected = true;
	internal const bool C_DefaultEnlist = true;
	internal const string C_DefaultClientLibrary = "fbembed";
	internal const int C_DefaultDbCachePages = 0;
	internal const bool C_DefaultNoDbTriggers = false;
	internal const bool C_DefaultNoGarbageCollect = false;
	internal const bool C_DefaultCompression = false;
	internal const byte[] C_DefaultCryptKey = null;
	internal const FbWireCrypt C_DefaultWireCrypt = FbWireCrypt.Enabled;
	internal const string C_DefaultApplicationName = "";
	internal const int C_DefaultCommandTimeout = 0;
	internal const int C_DefaultParallelWorkers = 0;



	// SourceInformation properties
	internal const string C_DefaultSIDataSourceName = null;
	internal const string C_DefaultSIDataSourceProduct = null;
	internal const string C_DefaultSIDataSourceVersion = null;


	// SourceInformation additional titlecased connection properties for Root
	internal const string C_DefaultSICatalog = null;
	internal const int C_DefaultSIPortNumber = int.MinValue;
	internal const int C_DefaultSIServerType = int.MinValue;
	internal const string C_DefaultSIUserId = null;
	internal const string C_DefaultSIPassword = null;

	// SourceInformation additional connection derived properties for Root
	internal const string C_DefaultSIDataset = null;

	// SourceInformation additional new properties for root
	internal const int C_DefaultSIMemoryUsage = int.MinValue;
	internal const int C_DefaultSIActiveUsers = int.MinValue;


	#endregion Property Default Values


}
