// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System.Runtime.InteropServices;
using BlackbirdSql.Core.Ctl.CommandProviders;


namespace BlackbirdSql.VisualStudio.Ddex.Ctl.CommandProviders;

[Guid(CommandProperties.UniversalCommandProviderGuid)]


// =========================================================================================================
//										UniversalQueryCommandProvider Class
//
/// <summary>
/// Implements commands on System Object nodes
/// </summary>
// =========================================================================================================
internal class UniversalCommandProvider : AbstractCommandProvider
{
	public UniversalCommandProvider() : base(CommandProperties.EnNodeSystemType.Global)
	{
	}

}
