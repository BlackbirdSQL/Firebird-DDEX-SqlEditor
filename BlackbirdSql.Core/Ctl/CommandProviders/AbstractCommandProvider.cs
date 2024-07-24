// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.ComponentModel.Design;
using BlackbirdSql.Core.Ctl.Config;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Core.Properties;
using BlackbirdSql.Sys.Enums;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;



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


		if (commandId.Equals(CommandProperties.ValidateSolution))
		{
			cmd = new DataViewMenuCommand(itemId, commandId, delegate
			{
				// if (!command.Properties.Contains("GotText"))
				//	Tracer.Trace(GetType(), "CreateCommand()", "ValidateSolution");

				cmd.Visible = true;
				cmd.Enabled = ApcManager.CanValidateSolution && !ApcManager.SolutionValidating;

				if (cmd.Visible && !command.Properties.Contains("GotText")
					&& (label = GlobalizeLabel(cmd, EnNodeSystemType.Global)) != string.Empty)
				{
					cmd.Properties["GotText"] = true;
					cmd.Properties["Text"] = label;
				}

			}, delegate
			{
				OnValidateSolution(itemId);
			});
			command = cmd;
		}
		else if (commandId.Equals(CommandProperties.TraceRct))
		{
			cmd = new DataViewMenuCommand(itemId, commandId, delegate
			{
				// if (!command.Properties.Contains("GotText"))
				//	Tracer.Trace(GetType(), "CreateCommand()", "TraceRct");

#if DEBUG
				cmd.Visible = cmd.Enabled = true;
#else
				cmd.Visible = cmd.Enabled = false;
#endif

				if (cmd.Visible && !command.Properties.Contains("GotText"))
				{
					cmd.Properties["GotText"] = true;
					cmd.Properties["Text"] = "Debug Trace Rct";
				}

				return;
			}, delegate
			{
				OnTraceRct(itemId);
			});
			command = cmd;
		}
		else if (commandId.Equals(CommandProperties.NewQuery))
		{
			cmd = new DataViewMenuCommand(itemId, commandId, delegate
			{
				// if (!command.Properties.Contains("GotText"))
				//	Tracer.Trace(GetType(), "CreateCommand()", "NewQuery");

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
				OnNewQuery(Resources.NewQueryBaseName);
			});
			command = cmd;
		}
		else if (commandId.Equals(CommandProperties.NewDesignerQuery))
		{
			cmd = new DataViewMenuCommand(itemId, commandId, delegate
			{
				// if (!command.Properties.Contains("GotText"))
				//	Tracer.Trace(GetType(), "CreateCommand()", "NewDesignerQuery");

				cmd.Visible = cmd.Enabled = true;

				node = Site.ExplorerConnection.FindNode(itemId);
				nodeSystemType = node.NodeSystemType();

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
				// if (!command.Properties.Contains("GotText"))
				//	Tracer.Trace(GetType(), "CreateCommand()", "OpenTextObject");

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
				// if (!command.Properties.Contains("GotText"))
				//	Tracer.Trace(GetType(), "CreateCommand()", "OpenAlterTextObject");

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
		else if (commandId.Equals(CommandProperties.OverrideRetrieveDataLocal))
		{

			// We're hiding the retrieve data command at the local node level so that we can use
			// our retrieve data command with the correct text.

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
		else if (commandId.Equals(CommandProperties.RetrieveDesignerData))
		{
			// if (!command.Properties.Contains("GotText"))
			//	Tracer.Trace(GetType(), "CreateCommand()", "RetrieveDesignerData");

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


	/*
	private IVsDataViewSupport GetViewSupport(IVsDataExplorerConnection root)
	{
		lock (_LockLocal)
		{
			return Reflect.GetField(root, "viewSupport",
				BindingFlags.NonPublic | BindingFlags.Instance) as IVsDataViewSupport;
		}
	}
	*/



	public static string GlobalizeLabel(DataViewMenuCommand cmd, IVsDataExplorerNode node)
	{
		if (!cmd.Visible)
			return string.Empty;

		string text = string.Empty;
		EnModelObjectType type = node.NodeBaseType();

		if (cmd.CommandID.Equals(CommandProperties.NewQuery))
			text = GetResourceString("New", "Sql", "Query");
		else if (cmd.CommandID.Equals(CommandProperties.OpenTextObject))
			text = GetResourceString("Open", type.ToString(), "Script");
		else if (cmd.CommandID.Equals(CommandProperties.OpenAlterTextObject))
			text = GetResourceString("Alter", type.ToString(), "Script");
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
		else if (cmd.CommandID.Equals(CommandProperties.ValidateSolution))
			text = GetResourceString("Validate", type, "Solution");

		// Tracer.Trace(typeof(AbstractCommandProvider), "GlobalizeLabel(EnNodeSystemType)", "text: {0}", text);

		return text;

	}


	#endregion Methods





	// =========================================================================================================
	#region Event handlers - AbstractCommandProvider
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Exposes the connection node RetrieveData DataViewMenuCommand's private field
	/// '_commandProvider' and uses it to create a new RetrieveData command for the
	/// handler's current node.
	/// Retrieve data intercept system command event handler.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected virtual void OnInterceptorDesignRetrieveData(int itemId)
	{
		// Tracer.Trace(GetType(), "OnInterceptorDesignRetrieveData()", "itemId: {0}.", itemId);

		Hostess host = new(Site.ServiceProvider);
		IVsDataExplorerNode node = Site.ExplorerConnection.FindNode(itemId);

		if (host.ActivateDocumentIfOpen("DataExplorer://{0}/{1}/{2}".FmtRes(node.ExplorerConnection.DisplayName, node.Object.Type.Name, node.Object.Identifier.ToString())))
		{
			return;
		}

		// Get the retrieve data command at the connection node level. This will be the original
		// built-in retrieve data command and the only access we have to the original because we
		// have overridden the local node with a hidden OverrideRetrieveDataLocal command.
		IVsDataExplorerNode connectionNode = Site.ExplorerConnection.ConnectionNode;
		MenuCommand globalCommand = connectionNode.GetCommand(CommandProperties.OverrideRetrieveDataLocal);

		if (globalCommand == null)
		{
			ArgumentException ex = new("GetCommand() GlobalNewQuery returned null");
			Diag.Dug(ex);
			return;
		}



		try
		{
			// The retrieve data command we've retrieved belongs to the connection node so we use reflection
			// to create a new command using it's command provider.

			// Get the connection node RetrieveData command's command provider.
			IVsDataViewCommandProvider commandProvider = (IVsDataViewCommandProvider)Reflect.GetFieldValue(globalCommand, "_commandProvider");

			if (commandProvider == null)
				return;

			// Use the provider to create a new retrieve data command at the local node level.
			DataViewMenuCommand command = (DataViewMenuCommand)commandProvider.CreateCommand(itemId, CommandProperties.OverrideRetrieveDataLocal);

			// Invoking the new command will now retrieve the correct node.
			command.Invoke();
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
		}


		// Show the diagram pane if enabled.
		if (PersistentSettings.ShowDiagramPane)
		{
			CommandID cmd = new CommandID(VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.ShowGraphicalPane);
			// Delay 10 ms to give Editor WindowFrame and QueryDesignerDocument time to breath.
			host.PostExecuteCommand(cmd, 10);
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
	protected void OnNewQuery(string baseName)
	{
		// Tracer.Trace(GetType(), "OnNewQuery()");

		if (SystemData.MandatedSqlEditorFactoryGuid.Equals(SystemData.EditorFactoryGuid, StringComparison.OrdinalIgnoreCase))
		{
			if (RctManager.ShutdownState)
				return;

			IBsDesignerExplorerServices service = ApcManager.EnsureService<IBsDesignerExplorerServices>();

			Csb csa = RctManager.CloneRegistered(Site.ExplorerConnection.ConnectionNode);

			// Tracer.Trace(GetType(), "OnNewQuery()", "csa.DatasetKey: {0}.", csa.DatasetKey);

			service.NewQuery(csa.DatasetKey, baseName, null);
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Open text object command event handler.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected void OnOpenScript(int itemId, EnModelTargetType targetType)
	{
		// Tracer.Trace(GetType(), "OnOpen()", "itemId: {0}.", itemId);

		if (RctManager.ShutdownState)
			return;

		IVsDataExplorerNode node = Site.ExplorerConnection.FindNode(itemId);

		if (SystemData.MandatedSqlEditorFactoryGuid.Equals(SystemData.EditorFactoryGuid, StringComparison.OrdinalIgnoreCase))
		{
			IBsDesignerExplorerServices service = ApcManager.EnsureService<IBsDesignerExplorerServices>();

			service.ViewCode(node, targetType);
		}
		else
		{
			Diag.ThrowException(new Exception("DesignerOnlineServices are not yet available"));

			/*
			IBDesignerOnlineServices service = Controller.EnsureService<IBDesignerOnlineServices>();

			Moniker moniker = new(node, targetType);

			IList<string> identifierList = moniker.Identifier.ToArray();
			EnModelObjectType objectType = moniker.ObjectType;
			string script = Moniker.GetDecoratedDdlSource(node, targetType);

			Csb csa = RctManager.CloneRegistered(node);

			service.ViewCode(csa, objectType, identifierList, targetType, script);
			*/
		}

		// Hostess.ActivateOrOpenVirtualDocument(node, false);
	}


	protected virtual void OnTraceRct(int itemId)
	{
		RctManager.TraceRct();
	}


	protected virtual void OnValidateSolution(int itemId)
	{
		ApcManager.ValidateSolution();
	}


	#endregion Event handlers

}
