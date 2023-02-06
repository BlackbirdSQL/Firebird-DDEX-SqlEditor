using System.Runtime.InteropServices;
using BlackbirdSql.Common.Extensions.Commands;

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
