
using System;
using System.Security;
using BlackbirdSql.Sys.Enums;



namespace BlackbirdSql;


// =========================================================================================================
//											CoreConstants Class
//
/// <summary>
/// Core db constants class.
/// </summary>
// =========================================================================================================
internal static class CoreConstants
{

	// ---------------------------------------------------------------------------------
	#region Engine Miscellaneous constants - CoreConstants
	// ---------------------------------------------------------------------------------


	#endregion Engine Miscellaneous constants





	// ---------------------------------------------------------------------------------
	#region DbConnectionString Property Names - CoreConstants
	// ---------------------------------------------------------------------------------


	// External (non-paramameter) property defaults 
	internal const string C_KeyExClientVersion = "ClientVersion";
	internal const string C_KeyExMemoryUsage = "MemoryUsage";
	internal const string C_KeyExActiveUsers = "ActiveUsers";


	// Extended property descriptor keys
	internal const string C_KeyExDataset = "Dataset";
	internal const string C_KeyExDatasetKey = "DatasetKey";
	internal const string C_KeyExConnectionKey = "ConnectionKey";
	internal const string C_KeyExConnectionSource = "ConnectionSource";
	internal const string C_KeyExAdornedDisplayName = "AdornedDisplayName";
	internal const string C_KeyExAdornedQualifiedName = "AdornedQualifiedName";
	internal const string C_KeyExAdornedQualifiedTitle = "AdornedQualifiedTitle";


	// External (non-paramameter) property descriptor
	internal const string C_KeyExServerVersion = "ServerVersion";
	internal const string C_KeyExPersistPassword = "PersistPassword";
	internal const string C_KeyExEdmx = "edmx";
	internal const string C_KeyExEdmu = "edmu";

	// Internal (hidden) property keys
	internal const string C_KeyExInMemoryPassword = "InMemoryPassword";


	// Other connection constants.
	internal const string C_KeyExConnectionUrl = "ConnectionUrl";
	internal const string C_KeyExConnectionString = "ConnectionString";


	#endregion DbConnectionString Property Names




	// ---------------------------------------------------------------------------------
	#region DbConnectionString Property Default Values - CoreConstants
	// ---------------------------------------------------------------------------------


	// External (non-paramameter) property defaults 
	internal const Version C_DefaultExClientVersion = null;
	internal const string C_DefaultExMemoryUsage = null;
	internal const int C_DefaultExActiveUsers = int.MinValue;


	// Extended property defaults
	internal const string C_DefaultExDataset = "";
	internal const string C_DefaultExDatasetKey = "";
	internal const string C_DefaultExConnectionKey = "";
	internal const EnConnectionSource C_DefaultExConnectionSource = EnConnectionSource.Unknown;

	// External (non-paramameter) property defaults 
	internal const Version C_DefaultExServerVersion = null;
	internal const bool C_DefaultExPersistPassword = false;

	// Internal (hidden) property defaults
	internal const SecureString C_DefaultExInMemoryPassword = null;


	#endregion DbConnectionString Property Default Values

}
