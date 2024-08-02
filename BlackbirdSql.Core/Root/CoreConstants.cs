
using System;
using System.Data;
using System.Security;
using BlackbirdSql.Sys.Enums;
using BlackbirdSql.Sys.Properties;



namespace BlackbirdSql;


// =========================================================================================================
//											CoreConstants Class
//
/// <summary>
/// Core db constants class.
/// </summary>
// =========================================================================================================
public static class CoreConstants
{
	// ---------------------------------------------------------------------------------
	#region DbConnectionString Property Names - CoreConstants
	// ---------------------------------------------------------------------------------


	// External (non-paramameter) property defaults 
	public const string C_KeyExClientVersion = "ClientVersion";
	public const string C_KeyExMemoryUsage = "MemoryUsage";
	public const string C_KeyExActiveUsers = "ActiveUsers";


	// Extended property descriptor keys
	public const string C_KeyExDataset = "Dataset";
	public const string C_KeyExDatasetKey = "DatasetKey";
	public const string C_KeyExConnectionKey = "ConnectionKey";
	public const string C_KeyExConnectionSource = "ConnectionSource";
	public const string C_KeyExAdornedDisplayName = "AdornedDisplayName";
	public const string C_KeyExAdornedQualifiedName = "AdornedQualifiedName";
	public const string C_KeyExAdornedQualifiedTitle = "AdornedQualifiedTitle";


	// External (non-paramameter) property descriptor
	public const string C_KeyExServerVersion = "ServerVersion";
	public const string C_KeyExPersistPassword = "PersistPassword";
	public const string C_KeyExEdmx = "edmx";
	public const string C_KeyExEdmu = "edmu";
	public const string C_KeyExCreationFlags = "CreationFlags";

	// Internal (hidden) property keys
	public const string C_KeyExInMemoryPassword = "InMemoryPassword";


	// Other connection constants.
	public const string C_KeyExConnectionUrl = "ConnectionUrl";
	public const string C_KeyExConnectionString = "ConnectionString";


	#endregion DbConnectionString Property Names




	// ---------------------------------------------------------------------------------
	#region DbConnectionString Property Default Values - CoreConstants
	// ---------------------------------------------------------------------------------


	// External (non-paramameter) property defaults 
	public const Version C_DefaultExClientVersion = null;
	public const string C_DefaultExMemoryUsage = null;
	public const int C_DefaultExActiveUsers = int.MinValue;


	// Extended property defaults
	public const string C_DefaultExDataset = "";
	public const string C_DefaultExDatasetKey = "";
	public const string C_DefaultExConnectionKey = "";
	public const EnConnectionSource C_DefaultExConnectionSource = EnConnectionSource.Unknown;

	// External (non-paramameter) property defaults 
	public const Version C_DefaultExServerVersion = null;
	public const bool C_DefaultExPersistPassword = false;

	// Internal (hidden) property defaults
	public const SecureString C_DefaultExInMemoryPassword = null;


	#endregion DbConnectionString Property Default Values





	// ---------------------------------------------------------------------------------
	#region Model Engine Miscellaneous keys and defaults - CoreConstants
	// ---------------------------------------------------------------------------------


	#endregion Model Engine Miscellaneous keys and defaults

}
