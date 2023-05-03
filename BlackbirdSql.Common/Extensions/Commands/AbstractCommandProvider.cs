// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System.ComponentModel.Design;

using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Services;


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
 * When invoked we simply set the static DataToolsCommands.CommandObjectType to DataObjectType.System or
 * DataObjectType.User then invoke the global command using id 0x3513 which will revert to the built-in provider.
 * Once the built-in command has been invoked and objects enumerated we set the static DataToolsCommands.CommandObjectType
 * back to DataObjectType.None.
 * 
 * When the datatable list dialog is instantiated by VSs DataQueryDesignerDocument and it requests a table list
 * our ddex services can then examine the DataToolsCommands.CommandObjectType indicator to determine if any filtering 
 * should occur.
 * If the VS user does a refresh he/she will get a full list irrelevant of the node type because the association with the
 * node is lost. 
*/



namespace BlackbirdSql.Common.Extensions.Commands
{
	internal abstract class AbstractCommandProvider : DataViewCommandProvider
	{

		protected abstract DataToolsCommands.DataObjectType CommandObjectType
		{
			get;
		}

		protected override MenuCommand CreateCommand(int itemId, CommandID commandId, object[] parameters)
		{
			// Diag.Trace();

			MenuCommand command = null;

			if (commandId.Equals(DataToolsCommands.NewQuery))
			{
				command = new DataViewMenuCommand(itemId, commandId, delegate
				{
					OnNewQuery(itemId, CommandObjectType);
				});
			}
			else if (commandId.Equals(DataToolsCommands.OpenTextObject))
			{
				command = new DataViewMenuCommand(itemId, commandId, delegate
				{
					command.Enabled = true;
					command.Visible = true;
					if (command.Visible)
					{
						command.Properties["Text"] = Properties.Resources.TextObjectCommandProvider_Open;
					}

				}, delegate
				{
					OnOpen(itemId, CommandObjectType);
				});
			}
			else
			{
				command = base.CreateCommand(itemId, commandId, parameters);
			}

			return command;
		}

		private void OnNewQuery(int itemId, DataToolsCommands.DataObjectType objectType)
		{
			// Diag.Trace();

			IVsDataExplorerNode vsDataExplorerNode = Site.ExplorerConnection.FindNode(itemId);

			MenuCommand command = vsDataExplorerNode.GetCommand(DataToolsCommands.GlobalNewQuery);

			// This should be locked
			// Diag.Trace("SETTNG CONNECTION COMMANDTYPE TO: " + objectType + " for command in assembly: " + command.GetType().AssemblyQualifiedName);
			DataToolsCommands.CommandObjectType = objectType;
			
			command.Invoke();

			// Diag.Trace("COMMAND INVOKED");
		}

		private void OnOpen(int itemId, DataToolsCommands.DataObjectType objectType)
		{
			// Disabled until debugged.
			if (itemId == 123456789)
			{
				IVsDataExplorerNode vsDataExplorerNode = Site.ExplorerConnection.FindNode(itemId);

				MenuCommand command = vsDataExplorerNode.GetCommand(DataToolsCommands.OpenTextObject);

				// This should be locked
				// Diag.Trace("SETTNG CONNECTION COMMANDTYPE TO: " + objectType + " for command in assembly: " + command.GetType().AssemblyQualifiedName);
				// DataToolsCommands.CommandObjectType = objectType;

				command.Invoke();
			}

		}


	}
}
