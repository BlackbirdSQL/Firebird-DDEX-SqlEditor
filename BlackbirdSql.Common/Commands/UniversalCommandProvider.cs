// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System.Runtime.InteropServices;

namespace BlackbirdSql.Common.Commands
{
	[Guid(DataToolsCommands.UniversalCommandProviderGuid)]


	// =========================================================================================================
	//										UniversalQueryCommandProvider Class
	//
	/// <summary>
	/// Implements the new query command on System Object nodes
	/// </summary>
	// =========================================================================================================
	internal class UniversalCommandProvider : AbstractCommandProvider
	{

		/// <summary>
		/// Identifies this <see cref="AbstractCommandProvider"/> as spawned off of a General Object SE node
		/// </summary>
		protected override DataToolsCommands.DataObjectType CommandObjectType
		{
			get { return DataToolsCommands.DataObjectType.Global; }
		}

	}
}
