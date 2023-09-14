
using System.Security;
using System;

using BlackbirdSql.Core.Ctl.Enums;
using BlackbirdSql.Core.Ctl.Interfaces;

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


	// Built-in default parameters
	internal const string C_KeyExIcon = "Icon";
	internal const string C_KeyDataSource = "data source";
	internal const string C_KeyPortNumber = "port number";
	internal const string C_KeyServerType = "server type";
	internal const string C_KeyCatalog = "initial catalog";
	internal const string C_KeyUserId = "user id";
	internal const string C_KeyPassword = "password";


	// External (non-paramameter) properties 
	internal const string C_KeyExServerDefinition = "ServerDefinition";
	internal const string C_KeyExServerVersion = "ServerVersion";
	internal const string C_KeyExPersistPassword = "PersistPassword";
	internal const string C_KeyExAdministratorLogin = "AdministratorLogin";
	internal const string C_KeyExServerFullyQualifiedDomainName = "ServerFullyQualifiedDomainName";
	internal const string C_KeyExOtherParams = "OtherParams";

	// Internal (hidden) properties
	internal const string C_KeyExInMemoryPassword = "InMemoryPassword";


	#endregion Property Names




	// ---------------------------------------------------------------------------------
	#region Property Default Values - CoreConstants
	// ---------------------------------------------------------------------------------


	// Built-in property defaults
	internal const IBIconType C_DefaultExIcon = null;
	internal const string C_DefaultDataSource = "";
	internal const int C_DefaultPortNumber = 3050;
	internal const EnDbServerType C_DefaultServerType = EnDbServerType.Default;
	internal const string C_DefaultCatalog = "";
	internal const string C_DefaultUserId = "";
	internal const string C_DefaultPassword = "";
	internal const string C_DefaultExOtherParams = null;


	// Other external properties
	internal const string C_DefaultExDatasetKey = "";
	internal const ServerDefinition C_DefaultExServerDefinition = null;
	internal const string C_DefaultExAdministratorLogin = "";
	internal const string C_DefaultExServerFullyQualifiedDomainName = "localhost";
	internal const SecureString C_DefaultExInMemoryPassword = null;
	internal const Version C_DefaultExServerVersion = null;
	internal const bool C_DefaultExPersistPassword = false;


	#endregion Property Default Values


}
