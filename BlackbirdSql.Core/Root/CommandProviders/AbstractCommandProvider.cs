// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.Linq;
using BlackbirdSql.Core.Enums;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Core.Properties;
using BlackbirdSql.Core.Providers;
using FirebirdSql.Data.FirebirdClient;
using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Shell;


using DataObjectType = BlackbirdSql.Core.CommandProviders.CommandProperties.DataObjectType;


/*
 * For New Query commands, this command provider sets a static indicator to filter the table list 
 * dialog presented on a new query to either user tables or system tables or both.
 * We don't want to mess with the internal storage manager so we force a new connection for table
 * lists for new query command requests.
 * If the active node can be identified as a system or user object the descendent classes 
 * SystemCommandProvider and UserQueryCommandProvider are invoked respectively, otherwise 
 * UniversalCommandProvider is used.
 * 
 * For New Query commands, the command provider is specified at the local/node level using command id 0x3528.
 * At the top of the explorer tree we specify the UniversalCommander and directly below it the global 
 * command using id 0x3513 on the built-in command provider with guid 884DD964-5327-461f-9F06-6484DD540F8F.
 * 
 * When invoked we simply set the static CommandProperties.CommandObjectType to DataObjectType.System or
 * DataObjectType.User DataObjectType.Global and then invoke the global command using id 0x3513 which will 
 * revert to the built-in provider.
 * Once the built-in command has been invoked and objects have been enumerated we set the static 
 * CommandProperties.CommandObjectType back to DataObjectType.None.
 * 
 * When the datatable list dialog is instantiated by VSs DataQueryDesignerDocument and it requests a table 
 * list our ddex services can then examine the CommandProperties.CommandObjectType indicator to determine 
 * if any filtering should occur.
 * If the VS user does a refresh he/she will get a full list irrelevant of the node type because the 
 * association with the node is lost. The built in DataQueryDesignerDocument only makes a single call back to
 * our package when it does a connection equivalency check. This is where we intercept it and return false,
 * knowing that the next call back to us will be for a new connection and then a new table schema. It is at
 * this point that we provide provide it with the appropriate table list.
*/



namespace BlackbirdSql.Core.CommandProviders;



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


	// protected abstract Package DdexPackage { get; }
	protected IBAsyncPackage DdexPackage => Controller.DdexPackage;

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
			if (@object == null || !MonikerAgent.ModelObjectTypeIn(@object, EnModelObjectType.StoredProcedure,
				EnModelObjectType.Function))
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
				|| MonikerAgent.ModelObjectTypeIn(@object, EnModelObjectType.Index, EnModelObjectType.ForeignKey))
			{
				if ((bool)@object.Properties["IS_COMPUTED"])
				{
					return true;
				}
			}
			else if (@object.Type.Name.EndsWith("Trigger")
				|| MonikerAgent.ModelObjectTypeIn(@object, EnModelObjectType.View, EnModelObjectType.StoredProcedure, EnModelObjectType.Function))
			{
				return true;
			}

		}

		return false;
	}





	// ---------------------------------------------------------------------------------
	/// <summary>
	/// True if the node has an expression or source node object can be altered in an
	/// IDE editor window else false.
	/// </summary>
	// ---------------------------------------------------------------------------------
	internal static bool CanAlter(IVsDataExplorerNode node)
	{
		if (node != null && node.Object != null)
		{
			IVsDataObject @object = node.Object;

			if (@object.Type.Name.EndsWith("Trigger")
				|| MonikerAgent.ModelObjectTypeIn(@object, EnModelObjectType.View, EnModelObjectType.StoredProcedure, EnModelObjectType.Function))
			{
				return true;
			}
		}

		return false;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// True if the node has an expression or source that can be opened in an IDE
	/// editor window else false.
	/// </summary>
	// ---------------------------------------------------------------------------------
	internal static bool HasScript(IVsDataExplorerNode node)
	{
		if (node != null && node.Object != null)
		{
			IVsDataObject @object = node.Object;

			if (@object.Type.Name.EndsWith("Column") || @object.Type.Name.EndsWith("Parameter")
				|| MonikerAgent.ModelObjectTypeIn(@object, EnModelObjectType.Index, EnModelObjectType.ForeignKey))
			{
				if ((bool)@object.Properties["IS_COMPUTED"])
				{
					return true;
				}
			}
			else if (@object.Type.Name.EndsWith("Trigger")
				|| MonikerAgent.ModelObjectTypeIn(@object, EnModelObjectType.View, EnModelObjectType.StoredProcedure, EnModelObjectType.Function))
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

		MenuCommand command = null;
		DataViewMenuCommand cmd = null;
		IVsDataExplorerNode node;


		if (commandId.Equals(CommandProperties.NewQuery))
		{
			cmd = new DataViewMenuCommand(itemId, commandId, delegate
			{
				if (cmd.Visible)
				{
					node = Site.ExplorerConnection.FindNode(itemId);

					cmd.Properties["Text"] = GetResourceString("New", "Query", CommandObjectType);
				}
			}, delegate
			{
				OnNewQuery(itemId, CommandObjectType);
			});
			command = cmd;
		}
		else if (commandId.Equals(CommandProperties.OpenTextObject))
		{
			cmd = new DataViewMenuCommand(itemId, commandId, delegate
			{
				IVsDataExplorerNode node = Site.ExplorerConnection.FindNode(itemId);
				cmd.Visible = HasScript(node);
				cmd.Enabled = CanOpen(node);

				if (cmd.Visible)
				{
					node = Site.ExplorerConnection.FindNode(itemId);
					cmd.Properties["Text"] = GetResourceString("Open", "Script", node);
				}
			}, delegate
			{
				OnOpen(itemId, false);
			});
			command = cmd;
		}
		else if (commandId.Equals(CommandProperties.OpenAlterTextObject))
		{
			cmd = new DataViewMenuCommand(itemId, commandId, delegate
			{
				IVsDataExplorerNode node = Site.ExplorerConnection.FindNode(itemId);
				cmd.Visible = HasScript(node);
				cmd.Enabled = CanAlter(node);

				if (cmd.Visible)
				{
					node = Site.ExplorerConnection.FindNode(itemId);

					cmd.Properties["Text"] = GetResourceString("Alter", "Script", node);
				}
			}, delegate
			{
				OnOpen(itemId, true);
			});
			command = cmd;
		}
		else
		{
			command = base.CreateCommand(itemId, commandId, parameters);
		}


		return command;

	}


	public static string GetResourceString(string commandFunction, string scriptType, IVsDataExplorerNode node)
	{
		return GetResourceString(commandFunction, scriptType, MonikerAgent.GetNodeBaseType(node));

	}

	public static string GetResourceString(string commandFunction, string scriptType, DataObjectType nodeSystemType)
	{
		return GetResourceString(commandFunction, scriptType, MonikerAgent.GetNodeObjectType(nodeSystemType));

	}


	public static string GetResourceString(string commandFunction, string scriptType, string nodeType)
	{
		string resource = $"CommandProvider_{commandFunction}{nodeType}{scriptType}";
		try
		{
			return Resources.ResourceManager.GetString(resource);
		}
		catch (Exception ex)

		{
			Diag.Dug(ex);
			return "Resource not found: " + resource;
		}
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
	protected void OnOpen(int itemId, bool alternate)
	{
		ThreadHelper.ThrowIfNotOnUIThread();

		IVsDataExplorerNode node = Site.ExplorerConnection.FindNode(itemId);

		// CommandProperties.CommandObjectType = objectType;


		if (SystemData.MandatedSqlEditorFactoryGuid.Equals(SystemData.DslEditorFactoryGuid, StringComparison.OrdinalIgnoreCase))
		{
			IBDesignerExplorerServices service = ((AsyncPackage)Controller.Instance.DdexPackage).GetService<IBDesignerExplorerServices, IBDesignerExplorerServices>();

			if (service == null)
			{
				InvalidOperationException ex = new("IBDesignerExplorerServices service not found");
				Diag.Dug(ex);
				throw ex;
			}
			service.ViewCode(node, alternate);
		}
		else
		{
			IBDesignerOnlineServices service = ((AsyncPackage)Controller.Instance.DdexPackage).GetService<IBDesignerOnlineServices,
				IBDesignerOnlineServices>();

			if (service == null)
			{
				InvalidOperationException ex = new("IBDesignerOnlineServices service not found");
				Diag.Dug(ex);
				throw ex;
			}

			FbConnectionStringBuilder csb = new();

			MonikerAgent moniker = new(node);

			csb.DataSource = moniker.Server;
			csb.Port = moniker.Port;
			csb.UserID = moniker.User;
			csb.Database = moniker.Database;
			csb.Password = moniker.Password;

			IList<string> identifierList = moniker.Identifier.ToArray();
			EnModelObjectType objectType = moniker.ObjectType;

			string script = MonikerAgent.GetDecoratedDdlSource(node, alternate);
			service.ViewCode(csb, objectType, alternate, identifierList, script);

		}

		// Host.ActivateOrOpenVirtualDocument(node, false);
	}


	#endregion Event handlers

}
