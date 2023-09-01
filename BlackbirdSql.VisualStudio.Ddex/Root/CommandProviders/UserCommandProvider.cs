// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System.Runtime.InteropServices;

using BlackbirdSql.Core.CommandProviders;




namespace BlackbirdSql.VisualStudio.Ddex.CommandProviders
{
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

		/// <summary>
		/// Identifies this <see cref="AbstractCommandProvider"/> as spawned off of a User Object SE node
		/// </summary>
		protected override CommandProperties.DataObjectType CommandObjectType
		{
			get { return CommandProperties.DataObjectType.User; }
		}

	}
}
