// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System.Runtime.InteropServices;
using BlackbirdSql.Core.Ctl.CommandProviders;


namespace BlackbirdSql.VisualStudio.Ddex.Ctl.CommandProviders;

[Guid(CommandProperties.UserQueryCommandProviderGuid)]


// =========================================================================================================
//										UserQueryCommandProvider Class
//
/// <summary>
/// Implements commands on User Object SE nodes
/// </summary>
// =========================================================================================================
internal class UserCommandProvider : AbstractCommandProvider
{
	public UserCommandProvider() : base(CommandProperties.EnNodeSystemType.User)
	{
	}

}
