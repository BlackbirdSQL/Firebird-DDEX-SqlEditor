using System.Runtime.InteropServices;




namespace BlackbirdSql.Common.Extensions
{
	[Guid(DataToolsCommands.SystemQueryCommandProviderGuid)]
	internal class SystemQueryCommandProvider : AbstractQueryCommandProvider
	{
		protected override DataToolsCommands.DataObjectType ObjectType
		{
			get { return DataToolsCommands.DataObjectType.System; }
		}

	}
}
