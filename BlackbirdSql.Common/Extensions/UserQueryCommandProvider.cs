using System.Runtime.InteropServices;






namespace BlackbirdSql.Common.Extensions
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
