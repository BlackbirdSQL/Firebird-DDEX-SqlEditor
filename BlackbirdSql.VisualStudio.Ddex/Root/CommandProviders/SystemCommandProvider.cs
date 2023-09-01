// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System.Runtime.InteropServices;

using BlackbirdSql.Core.CommandProviders;




namespace BlackbirdSql.VisualStudio.Ddex.CommandProviders
{
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

		/// <summary>
		/// Identifies this <see cref="AbstractCommandProvider"/> as spawned off of a System Object SE node
		/// </summary>
		protected override CommandProperties.DataObjectType CommandObjectType
		{
			get { return CommandProperties.DataObjectType.System; }
		}

	}
}
