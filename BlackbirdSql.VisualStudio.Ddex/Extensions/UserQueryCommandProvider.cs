using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Data.Framework;

using BlackbirdSql.Common;
using Microsoft.VisualStudio.Data.Services;





namespace BlackbirdSql.VisualStudio.Ddex.Extensions
{

	[Guid(DataToolsCommands.UserQueryCommandProviderGuid)]
	internal class UserQueryCommandProvider : AbstractQueryCommandProvider
	{
		protected override DataToolsCommands.DataObjectType ObjectType
		{
			get { return DataToolsCommands.DataObjectType.User; }
		}

	}
}
