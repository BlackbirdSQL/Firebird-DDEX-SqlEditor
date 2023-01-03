using System.ComponentModel.Design;
using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Services;

using BlackbirdSql.Common;


/*
 * This command provider sets a static indicator to filter the table list dialog presented on a 
 * new query to either user tables or system tables.
 * It is only invoked if the active node can be identified as a system or user object through the 
 * inherited classes SystemQueryCommandProvider and UserQueryCommandProvider respectively.
 * 
 * The command provider is specified at the local/node level using command id 0x3528.
 * At the top of the explorer tree we specify the global command using id 0x3513 on the built-in
 * command provider with guid 884DD964-5327-461f-9F06-6484DD540F8F
 * 
 * When invoked we simply set the static DataToolsCommands.ObjectType to DataObjectType.System or
 * DataObjectType.User then invoke the global command using id 0x3513 which will revert to the built-in provider.
 * Once the built-in command has been invoked and objects enumerated we set the static DataToolsCommands.ObjectType
 * back to DataObjectType.None.
 * 
 * When the datatable list dialog is instantiated by VSs DataQueryDesignerDocument and it requests a table list
 * our ddex services can then examine the DataToolsCommands.ObjectType indicator to determine if any filtering 
 * should occur.
 * If the VS user does a refresh he/she will get a full list irrelevant of the node type because the association with the
 * node is lost. 
*/



namespace BlackbirdSql.VisualStudio.Ddex.Extensions
{
	internal abstract class AbstractQueryCommandProvider : DataViewCommandProvider
	{

		protected abstract DataToolsCommands.DataObjectType ObjectType
		{
			get;
		}

		protected override MenuCommand CreateCommand(int itemId, CommandID commandId, object[] parameters)
		{
			Diag.Dug();

			MenuCommand command = null;

			if (commandId.Equals(DataToolsCommands.NewQuery))
			{
				int qualityMetric = 262144;
				if (parameters != null && parameters[0] is int)
				{
					qualityMetric = (int)parameters[0];
				}

				command = new DataViewMenuCommand(itemId, commandId, delegate
				{
					OnNewQuery(itemId, ObjectType, qualityMetric);
				});
			}
			else
			{
				command = base.CreateCommand(itemId, commandId, parameters);
			}

			return command;
		}

		private void OnNewQuery(int itemId, DataToolsCommands.DataObjectType objectType, int qualityMetricProvider)
		{
			Diag.Dug();

			IVsDataExplorerNode vsDataExplorerNode = base.Site.ExplorerConnection.FindNode(itemId);

			Diag.Dug();
			MenuCommand command = vsDataExplorerNode.GetCommand(DataToolsCommands.GlobalNewQuery);
			Diag.Dug();

			// This should be locked
			DataToolsCommands.ObjectType = objectType;
			command.Invoke();


			Diag.Dug();
		}

	}
}
