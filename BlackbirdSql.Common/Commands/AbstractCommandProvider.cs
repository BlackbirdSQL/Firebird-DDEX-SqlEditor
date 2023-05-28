// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System.ComponentModel.Design;

using BlackbirdSql.Common.Providers;
using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Shell;


using DataObjectType = BlackbirdSql.Common.Commands.CommandProperties.DataObjectType;


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
 * When invoked we simply set the static CommandProperties.CommandObjectType to DataObjectType.System or
 * DataObjectType.User then invoke the global command using id 0x3513 which will revert to the built-in provider.
 * Once the built-in command has been invoked and objects enumerated we set the static CommandProperties.CommandObjectType
 * back to DataObjectType.None.
 * 
 * When the datatable list dialog is instantiated by VSs DataQueryDesignerDocument and it requests a table list
 * our ddex services can then examine the CommandProperties.CommandObjectType indicator to determine if any filtering 
 * should occur.
 * If the VS user does a refresh he/she will get a full list irrelevant of the node type because the association with the
 * node is lost. 
*/



namespace BlackbirdSql.Common.Commands;



// =========================================================================================================
//										AbstractCommandProvider Class
//
/// <summary>
/// The base IVsDataViewCommandProvider class
/// </summary>
// =========================================================================================================
public abstract class AbstractCommandProvider : DataViewCommandProvider
{

	// ---------------------------------------------------------------------------------
	#region Variables - AbstractCommandProvider
	// ---------------------------------------------------------------------------------


	private Hostess _Host;


	#endregion Variables





	// =========================================================================================================
	#region Property Accessors - AbstractCommandProvider
	// =========================================================================================================


	protected abstract Package DdexPackage { get; }

	/// <summary>
	/// Abstract accessor to the command <see cref="DataObjectType"/>.
	/// Identifies whether the target SE node is is a User, System or Global node.
	/// </summary>
	protected abstract DataObjectType CommandObjectType { get; }


	/// <summary>
	/// IDE host access class object
	/// </summary>
	protected Hostess Host
	{
		get
		{
			_Host ??= new(Site.ServiceProvider);

			return _Host;
		}
	}


	#endregion Property Accessors





	// =========================================================================================================
	#region Methods - AbstractCommandProvider
	// =========================================================================================================



	protected static bool CanExecute(IVsDataExplorerNode node)
	{
		if (node != null)
		{
			IVsDataObject @object = node.Object;
			if (@object == null || !SqlMonikerHelper.TypeNameIn(@object.Type.Name, "StoredProcedure", "Function"))
			{
				return false;
			}
		}

		return true;
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// True if the node has an expression or source that can be opened in an IDE
	/// editor window else false.
	/// </summary>
	// ---------------------------------------------------------------------------------
	internal static bool CanOpen(IVsDataExplorerNode node)
	{
		if (node != null && node.Object != null)
		{
			IVsDataObject @object = node.Object;

			if (@object.Type.Name.EndsWith("Column") || @object.Type.Name.EndsWith("Parameter")
				|| SqlMonikerHelper.TypeNameIn(@object.Type.Name, "Index", "ForeignKey"))
			{
				if ((bool)@object.Properties["IS_COMPUTED"])
					return true;
			}
			else if (@object.Type.Name.EndsWith("Trigger")
				|| SqlMonikerHelper.TypeNameIn(@object.Type.Name, "View", "StoredProcedure", "Function"))
			{
				return true;
			}

		}

		return false;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Creates a command and delegate based on the SE itemId and commandId
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected override MenuCommand CreateCommand(int itemId, CommandID commandId, object[] parameters)
	{
		ThreadHelper.ThrowIfNotOnUIThread();
		// Diag.Trace("Command entry point");

		MenuCommand command = null;
		IVsDataExplorerNode node;

		// TemplateTextCommandProvider contains samples of future commands to be added

		if (commandId.Equals(CommandProperties.NewQuery))
		{
			command = new DataViewMenuCommand(itemId, commandId, delegate
			{
				if (command.Visible)
				{
					node = Site.ExplorerConnection.FindNode(itemId);

					command.Properties["Text"] = SqlMonikerHelper.GetString(
						$"CommandProvider_New{SqlMonikerHelper.GetNodeObjectType(CommandObjectType)}Query");
				}
			}, delegate
			{
				OnNewQuery(itemId, CommandObjectType);
			});
		}
		else if (commandId.Equals(CommandProperties.OpenTextObject))
		{
			command = new DataViewMenuCommand(itemId, commandId, delegate
			{
				IVsDataExplorerNode node = Site.ExplorerConnection.FindNode(itemId);
				command.Enabled = CanOpen(node);

				if (command.Visible)
				{
					node = Site.ExplorerConnection.FindNode(itemId);
					command.Properties["Text"] = SqlMonikerHelper.GetString(
						$"CommandProvider_Open{SqlMonikerHelper.GetNodeBaseType(node)}Script");
				}
			}, delegate
			{
				OnOpen(itemId);
			});
		}
		else
		{
			command = base.CreateCommand(itemId, commandId, parameters);
		}

		return command;
	}




	#endregion Methods





	// =========================================================================================================
	#region Event handlers - AbstractCommandProvider
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// New query command event handler.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected void OnNewQuery(int itemId, DataObjectType objectType)
	{
		// Diag.Trace("On new query");

		IVsDataExplorerNode vsDataExplorerNode = Site.ExplorerConnection.FindNode(itemId);

		MenuCommand command = vsDataExplorerNode.GetCommand(CommandProperties.GlobalNewQuery);

		// This should be locked
		// Diag.Trace("SETTNG CONNECTION COMMANDTYPE TO: " + objectType + " for command in assembly: " + command.GetType().AssemblyQualifiedName);
		CommandProperties.CommandObjectType = objectType;

		command.Invoke();

		// Diag.Trace("COMMAND INVOKED");
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Open text object command event handler.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected void OnOpen(int itemId)
	{
		ThreadHelper.ThrowIfNotOnUIThread();

		IVsDataExplorerNode node = Site.ExplorerConnection.FindNode(itemId);

		// CommandProperties.CommandObjectType = objectType;

		_ = Host.ActivateOrOpenDocument(node, false);

		// _ = Host.ActivateOrOpenVirtualDocument(DdexPackage, node, false);

	}


	#endregion Event handlers

}
