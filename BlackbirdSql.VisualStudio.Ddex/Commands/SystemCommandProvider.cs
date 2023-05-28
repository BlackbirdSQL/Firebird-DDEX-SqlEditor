﻿// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System.Runtime.InteropServices;

using Microsoft.VisualStudio.Shell;

using BlackbirdSql.Common.Commands;
using BlackbirdSql.VisualStudio.Ddex.Configuration;



namespace BlackbirdSql.VisualStudio.Ddex.Commands
{
	[Guid(CommandProperties.SystemQueryCommandProviderGuid)]


	// =========================================================================================================
	//										SystemQueryCommandProvider Class
	//
	/// <summary>
	/// Implements the new query command on System Object nodes
	/// </summary>
	// =========================================================================================================
	internal class SystemCommandProvider : AbstractCommandProvider
	{

		protected override Package DdexPackage => PackageController.Instance().DdexPackage;


		/// <summary>
		/// Identifies this <see cref="AbstractCommandProvider"/> as spawned off of a System Object SE node
		/// </summary>
		protected override CommandProperties.DataObjectType CommandObjectType
		{
			get { return CommandProperties.DataObjectType.System; }
		}

	}
}