// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using BlackbirdSql.Core.Ctl.Config;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Core.Model.Enums;
using BlackbirdSql.Core.Properties;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Services;



namespace BlackbirdSql.Core.Ctl.CommandProviders;

// =========================================================================================================
//										AbstractCommandProvider Class
//
/// <summary>
/// The base IVsDataViewCommandProvider class
/// </summary>
// =========================================================================================================
public abstract class AbstractCommandProvider : DataViewCommandProvider
{

	// -----------------------------------------------------------------------------------------------------
	#region Constructors / Destructors - AbstractCommandProvider
	// -----------------------------------------------------------------------------------------------------


	public AbstractCommandProvider() : base()
	{
	}


	#endregion Constructors / Destructors




	// =========================================================================================================
	#region Methods - AbstractCommandProvider
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Creates a command and delegate based on the SE itemId and commandId
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected override MenuCommand CreateCommand(int itemId, CommandID commandId, object[] parameters)
	{
		// Tracer.Trace(GetType(), "AbstractCommandProvider.CreateCommand", "itemId: {0}, commandId: {1}", itemId, commandId);

		string label;
		MenuCommand command = null;
		DataViewMenuCommand cmd = null;
		EnNodeSystemType nodeSystemType = EnNodeSystemType.Undefined;
		IVsDataExplorerNode node;


		if (commandId.Equals(CommandProperties.NewSqlQuery))
		{
			cmd = new DataViewMenuCommand(itemId, commandId, delegate
			{
				// Tracer.Trace(GetType(), "CreateCommand()", "NewSqlQuery");
				cmd.Visible = cmd.Enabled = true;

				node = Site.ExplorerConnection.ConnectionNode;

				if (cmd.Visible && !command.Properties.Contains("GotText")
					&& (label = GlobalizeLabel(cmd, node)) != string.Empty)
				{
					cmd.Properties["GotText"] = true;
					cmd.Properties["Text"] = label;
				}

			}, delegate
			{
				OnNewSqlQuery();
			});
			command = cmd;
		}
		else if (commandId.Equals(CommandProperties.NewDesignerQuery))
		{
			cmd = new DataViewMenuCommand(itemId, commandId, delegate
			{
				// Tracer.Trace(GetType(), "CreateCommand()", "NewDesignerQuery");
				cmd.Visible = cmd.Enabled = true;

				node = Site.ExplorerConnection.FindNode(itemId);
				nodeSystemType = node.NodeSystemType();

				// Tracer.Trace(GetType(), "CreateCommand()", "NewDesignerQuery itemid: {0}, commandid: {1} systemType {2}.", itemId, commandId.ID, nodeSystemType.ToString());

				if (cmd.Visible && !command.Properties.Contains("GotText")
					&& (label = GlobalizeLabel(cmd, nodeSystemType)) != string.Empty)
				{
					cmd.Properties["GotText"] = true;
					cmd.Properties["Text"] = label;
				}

			}, delegate
					{
						OnInterceptorNewDesignerQuery(itemId, nodeSystemType);
					});
			command = cmd;
		}
		else if (commandId.Equals(CommandProperties.OverrideNewQueryLocal))
		{
			// We're hiding the new query command at the node level so that we can use our New Designer
			// with the correct text.
			node = Site.ExplorerConnection.FindNode(itemId);

			cmd = new DataViewMenuCommand(itemId, commandId,
				delegate
				{
					cmd.Enabled = cmd.Visible = false;
				},
				delegate
				{
					// Never called.
				});

			command = cmd;
		}
		else if (commandId.Equals(CommandProperties.OpenTextObject))
		{
			cmd = new DataViewMenuCommand(itemId, commandId, delegate
			{
				node = Site.ExplorerConnection.FindNode(itemId);

				IVsDataExplorerNode scriptNode = node.ScriptNode();

				cmd.Visible = scriptNode != null;
				cmd.Enabled = cmd.Visible && scriptNode.CanOpen();

				if (cmd.Visible && !command.Properties.Contains("GotText")
					&& (label = GlobalizeLabel(cmd, scriptNode)) != string.Empty)
				{
					cmd.Properties["GotText"] = true;
					cmd.Properties["Text"] = label;
				}

				if (scriptNode != null)
					itemId = scriptNode.ItemId;

			}, delegate
				{
					OnOpenScript(itemId, EnModelTargetType.QueryScript);
				});
			command = cmd;
		}
		else if (commandId.Equals(CommandProperties.OpenAlterTextObject))
		{
			cmd = new DataViewMenuCommand(itemId, commandId, delegate
			{
				node = Site.ExplorerConnection.FindNode(itemId);

				IVsDataExplorerNode scriptNode = node.ScriptNode();

				cmd.Visible = cmd.Enabled = (scriptNode != null) && scriptNode.CanAlter();

				if (cmd.Visible && !command.Properties.Contains("GotText")
					&& (label = GlobalizeLabel(cmd, scriptNode)) != string.Empty)
				{
					cmd.Properties["GotText"] = true;
					cmd.Properties["Text"] = label;
				}

				if (scriptNode != null)
					itemId = scriptNode.ItemId;

			}, delegate
					{
						OnOpenScript(itemId, EnModelTargetType.AlterScript);
					});
			command = cmd;
		}
		else if (commandId.Equals(CommandProperties.OverrideCopy))
		{
			// We're hiding the copy command at the node level so that we can use our copy with the correct text.
			node = Site.ExplorerConnection.FindNode(itemId);

			cmd = new DataViewMenuCommand(itemId, commandId,
				delegate
				{
					cmd.Enabled = cmd.Visible = false;
				},
				delegate
				{
					// Never called.
				});

			command = cmd;
		}
		else if (commandId.Equals(CommandProperties.CopyObject))
		{
			cmd = new DataViewMenuCommand(itemId, commandId, delegate
			{
				node = Site.ExplorerConnection.FindNode(itemId);
				cmd.Enabled = cmd.Visible = node.CanCopy();

				if (cmd.Visible && !command.Properties.Contains("GotText")
					&& (label = GlobalizeLabel(cmd, node)) != string.Empty)
				{
					cmd.Properties["GotText"] = true;
					cmd.Properties["Text"] = label;
				}

			}, delegate
					{
						OnInterceptorCopy(itemId);
					});
			command = cmd;
		}
		else if (commandId.Equals(CommandProperties.RetrieveDesignerData))
		{
			node = Site.ExplorerConnection.FindNode(itemId);

			cmd = new DataViewMenuCommand(itemId, commandId, delegate
			{
				IVsDataExplorerNode designerNode = node.DesignerNode();

				cmd.Enabled = cmd.Visible = designerNode != null;

				if (cmd.Visible && !command.Properties.Contains("GotText")
					&& (label = GlobalizeLabel(cmd, designerNode)) != string.Empty)
				{
					cmd.Properties["GotText"] = true;
					cmd.Properties["Text"] = label;
				}

				if (designerNode != null)
					itemId = designerNode.ItemId;

			}, delegate
			{
				OnInterceptorDesignRetrieveData(itemId);
			});

			command = cmd;
		}
		else
		{
			// Tracer.Trace(GetType(), "CreateCommand()", "itemid: {0}, commandid: {1}.", itemId, commandId.ID);
			command = base.CreateCommand(itemId, commandId, parameters);
		}


		return command;

	}



	public static string GetResourceString(string commandFunction, string nodeType, string targetType)
	{
		string resource = $"CommandProvider_{commandFunction}{nodeType}{targetType}";
		// Tracer.Trace(typeof(AbstractCommandProvider), "GetResourceString()", "resourceKey: {0}", resource);
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



	public static string GlobalizeLabel(DataViewMenuCommand cmd, IVsDataExplorerNode node)
	{
		if (!cmd.Visible)
			return string.Empty;

		string text = string.Empty;
		EnModelObjectType type = node.NodeBaseType();

		if (cmd.CommandID.Equals(CommandProperties.NewSqlQuery))
			text = GetResourceString("New", "Sql", "Query");
		if (cmd.CommandID.Equals(CommandProperties.OpenTextObject))
			text = GetResourceString("Open", type.ToString(), "Script");
		else if (cmd.CommandID.Equals(CommandProperties.OpenAlterTextObject))
			text = GetResourceString("Alter", type.ToString(), "Script");
		else if (cmd.CommandID.Equals(CommandProperties.CopyObject))
			text = GetResourceString("Copy", type.ToString(), "Node");
		else if (cmd.CommandID.Equals(CommandProperties.RetrieveDesignerData))
			text = GetResourceString("Retrieve", type.ToString(), "DesignerData");

		return text;
	}

	public static string GlobalizeLabel(DataViewMenuCommand cmd, EnNodeSystemType nodeSystemType)
	{
		if (!cmd.Visible)
			return string.Empty;

		string text = string.Empty;
		string type = nodeSystemType.ToString();

		if (cmd.CommandID.Equals(CommandProperties.NewDesignerQuery))
			text = GetResourceString("New", type, "Designer");

		// Tracer.Trace(typeof(AbstractCommandProvider), "GlobalizeLabel(EnNodeSystemType)", "text: {0}", text);

		return text;

	}


	#endregion Methods





	// =========================================================================================================
	#region Event handlers - AbstractCommandProvider
	// =========================================================================================================


	protected virtual void OnInterceptorDesignRetrieveData(int itemId)
	{
		// Tracer.Trace(GetType(), "OnInterceptorDesignRetrieveData()", "itemId: {0}.", itemId);

		IVsDataExplorerNode node = Site.ExplorerConnection.FindNode(itemId);
		MenuCommand command = node.GetCommand(CommandProperties.RetrieveDesignerLocal);

		if (command == null)
		{
			ArgumentException ex = new("GetCommand() GlobalNewQuery returned null");
			Diag.Dug(ex);
			return;
		}

		try
		{
			command.Invoke();
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
		}

		if (UserSettings.ShowDiagramPane)
		{
			Hostess host = new(Site.ServiceProvider);
			CommandID cmd = new CommandID(VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.ShowGraphicalPane);
			// Delay 10 ms just to give Editor WindowFrame and QueryDesignerDocument time to breath.
			host.PostExecuteCommand(cmd, 10);
		}

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Exposes CopyMenuCommand's private field '_explorerNode'.
	/// Copy intercept system command event handler.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected void OnInterceptorCopy(int itemId)
	{
		// Tracer.Trace(GetType(), "OnInterceptorCopy()", "itemId: {0}", itemId);

		// Get the copy command at the connection node level. This will be the original
		// built-in copy command and the only access we have to the original because we
		// have not implemented an override copy command at that level.
		IVsDataExplorerNode connectionNode = Site.ExplorerConnection.ConnectionNode;
		MenuCommand command = connectionNode.GetCommand(CommandProperties.OverrideCopy);

		if (command == null)
		{
			ArgumentException ex = new("GetCommand() Copy returned null");
			Diag.Dug(ex);
			return;
		}

		IVsDataExplorerNode node = Site.ExplorerConnection.FindNode(itemId);


		// The copy command we've retrieved belongs to the connection node so we use reflection
		// to change the command's node to the current node.

		Type typeCommand = command.GetType();

		FieldInfo explorerNodeInfo = typeCommand.GetField("_explorerNode",
			BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.IgnoreCase);

		if (explorerNodeInfo == null)
		{
			ArgumentException ex = new($"MenuCommand GetField(_explorerNode) returned null. Command type: {command.GetType().FullName}");
			Diag.Dug(ex);
			return;
		}


		try
		{
			// Set the copy command's node to the current node using Reflection.
			explorerNodeInfo.SetValue(command, node);
			// Invoking the copy will now copy the correct node.
			command.Invoke();
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// New query intercept system command event handler.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected void OnInterceptorNewDesignerQuery(int itemId, EnNodeSystemType nodeSystemType)
	{
		// Tracer.Trace(GetType(), "OnInterceptorNewDesigner()", "itemId: {0}, nodeSystemType: {1}", itemId, nodeSystemType);

		IVsDataExplorerNode node = Site.ExplorerConnection.FindNode(itemId);
		MenuCommand command = node.GetCommand(CommandProperties.NewQueryGlobal);

		if (command == null)
		{
			ArgumentException ex = new("GetCommand() NewQueryGlobal returned null");
			Diag.Dug(ex);
			return;
		}

		CommandProperties.CommandNodeSystemType = nodeSystemType;

		try
		{
			command.Invoke();
		}
		catch (Exception ex)
		{
			CommandProperties.CommandNodeSystemType = EnNodeSystemType.Undefined;
			Diag.Dug(ex);
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Open new SQL Query command event handler.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected void OnNewSqlQuery()
	{
		// Tracer.Trace(GetType(), "OnNewSqlQuery()");

		if (SystemData.MandatedSqlEditorFactoryGuid.Equals(SystemData.DslEditorFactoryGuid, StringComparison.OrdinalIgnoreCase))
		{
			IBDesignerExplorerServices service = Controller.GetService<IBDesignerExplorerServices>()
				?? throw Diag.ServiceUnavailable(typeof(IBDesignerExplorerServices));

			CsbAgent csa = new(Site.ExplorerConnection.ConnectionNode);
			csa.RegisterDataset();

			service.NewSqlQuery(csa.DatasetKey);
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Open text object command event handler.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected void OnOpenScript(int itemId, EnModelTargetType targetType)
	{
		// Tracer.Trace(GetType(), "OnOpen()", "itemId: {0}, alternate: {1}", itemId, alternate);

		IVsDataExplorerNode node = Site.ExplorerConnection.FindNode(itemId);

		if (SystemData.MandatedSqlEditorFactoryGuid.Equals(SystemData.DslEditorFactoryGuid, StringComparison.OrdinalIgnoreCase))
		{
			IBDesignerExplorerServices service = Controller.GetService<IBDesignerExplorerServices>()
				?? throw Diag.ServiceUnavailable(typeof(IBDesignerExplorerServices));

			service.ViewCode(node, targetType);
		}
		else
		{
			IBDesignerOnlineServices service = Controller.GetService<IBDesignerOnlineServices>()
				?? throw Diag.ServiceUnavailable(typeof(IBDesignerOnlineServices));

			MonikerAgent moniker = new(node, targetType);

			IList<string> identifierList = moniker.Identifier.ToArray();
			EnModelObjectType objectType = moniker.ObjectType;
			string script = MonikerAgent.GetDecoratedDdlSource(node, targetType);
			CsbAgent csb = new(node);

			service.ViewCode(csb, objectType, identifierList, targetType, script);

		}

		// Hostess.ActivateOrOpenVirtualDocument(node, false);
	}


	#endregion Event handlers

}
