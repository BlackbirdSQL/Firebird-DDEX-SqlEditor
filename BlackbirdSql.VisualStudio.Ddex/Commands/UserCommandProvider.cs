// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System.Runtime.InteropServices;

using Microsoft.VisualStudio.Shell;

using BlackbirdSql.Common.Commands;
using BlackbirdSql.VisualStudio.Ddex.Configuration;



namespace BlackbirdSql.VisualStudio.Ddex.Commands
{
	[Guid(CommandProperties.UserQueryCommandProviderGuid)]


	// =========================================================================================================
	//										UserQueryCommandProvider Class
	//
	/// <summary>
	/// Implements the new query command on User Object SE nodes
	/// </summary>
	// =========================================================================================================
	internal class UserCommandProvider : AbstractCommandProvider
	{

		protected override Package DdexPackage => PackageController.Instance().DdexPackage;

		/// <summary>
		/// Identifies this <see cref="AbstractCommandProvider"/> as spawned off of a User Object SE node
		/// </summary>
		protected override CommandProperties.DataObjectType CommandObjectType
		{
			get { return CommandProperties.DataObjectType.User; }
		}

	}
}
