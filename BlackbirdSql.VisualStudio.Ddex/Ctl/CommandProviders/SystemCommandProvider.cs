// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System.Runtime.InteropServices;
using BlackbirdSql.Core.Ctl.CommandProviders;


namespace BlackbirdSql.VisualStudio.Ddex.Ctl.CommandProviders;

[Guid(CommandProperties.SystemQueryCommandProviderGuid)]


// =========================================================================================================
//										SystemQueryCommandProvider Class
//
/// <summary>
/// Implements the commands on System Object nodes
/// </summary>
// =========================================================================================================
internal class SystemCommandProvider : AbstractCommandProvider
{

	public SystemCommandProvider() :base(CommandProperties.EnNodeSystemType.System)
	{
	}
}
