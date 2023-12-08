
using System.Security;
using System;

using BlackbirdSql.Core.Ctl.Enums;
using BlackbirdSql.Core.Ctl.Interfaces;

using FirebirdSql.Data.FirebirdClient;


namespace BlackbirdSql.Core.Ctl;

// =========================================================================================================
//											CoreConstants Class
//
/// <summary>
/// Core constants class.
/// </summary>
// =========================================================================================================
public static class CoreConstants
{

	// ---------------------------------------------------------------------------------
	#region Property Names - CoreConstants
	// ---------------------------------------------------------------------------------


	// Built-in default parameter keys
	public const string C_KeyFbDataSource = "data source";
	public const string C_KeyFbPort = "port number";
	public const string C_KeyFbServerType = "server type";
	public const string C_KeyFbDatabase = "initial catalog";
	public const string C_KeyFbUserID = "user id";
	public const string C_KeyFbPassword = "password";

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
	public const string C_KeyExDatasetId = "DatasetId";
	public const string C_KeyExExternalKey = "ExternalKey";


	// External (non-paramameter) property descriptor
	public const string C_KeyExServerEngine = "ServerEngine";
	public const string C_KeyExServerVersion = "ServerVersion";
	public const string C_KeyExPersistPassword = "PersistPassword";
	public const string C_KeyExAdministratorLogin = "AdministratorLogin";
	public const string C_KeyExServerFullyQualifiedDomainName = "ServerFullyQualifiedDomainName";
	public const string C_KeyExOtherParams = "OtherParams";

	// Internal (hidden) property keys
	public const string C_KeyExInMemoryPassword = "InMemoryPassword";


	#endregion Property Names




	// ---------------------------------------------------------------------------------
	#region Property Default Values - CoreConstants
	// ---------------------------------------------------------------------------------


	// Built-in property defaults
	public const string C_DefaultDataSource = "";
	public const int C_DefaultPort = 3050;
	public const FbServerType C_DefaultServerType = FbServerType.Default;
	public const string C_DefaultDatabase = "";
	public const string C_DefaultUserID = "";
	public const string C_DefaultPassword = "";


	// Extended property defaults
	public const IBIconType C_DefaultExIcon = null;
	public const string C_DefaultExDataset = "";
	public const string C_DefaultExDatasetKey = "";
	public const string C_DefaultExDatasetId = "";
	public const string C_DefaultExExternalKey = "";

	// External (non-paramameter) property defaults 
	public const EnEngineType C_DefaultExServerEngine = EnEngineType.Unknown;
	public const Version C_DefaultExServerVersion = null;
	public const bool C_DefaultExPersistPassword = false;
	public const string C_DefaultExAdministratorLogin = "";
	public const string C_DefaultExServerFullyQualifiedDomainName = "localhost";
	public const string C_DefaultExOtherParams = null;

	// Internal (hidden) property defaults
	public const SecureString C_DefaultExInMemoryPassword = null;


	#endregion Property Default Values


}
