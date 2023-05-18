// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using BlackbirdSql.Common.Properties;
using BlackbirdSql.Common.Providers;
using Microsoft;
using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;


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



namespace BlackbirdSql.Common.Commands;



// =========================================================================================================
//										AbstractCommandProvider Class
//
/// <summary>
/// The base IVsDataViewCommandProvider class
/// </summary>
// =========================================================================================================
internal abstract class AbstractCommandProvider : DataViewCommandProvider
{

	// ---------------------------------------------------------------------------------
	#region Variables - AbstractCommandProvider
	// ---------------------------------------------------------------------------------


	private Hostess _Host;


	#endregion Variables





	// =========================================================================================================
	#region Property Accessors - AbstractCommandProvider
	// =========================================================================================================


	/// <summary>
	/// Abstract accessor to the command <see cref="DataToolsCommands.DataObjectType"/>.
	/// Identifies whether the target SE node is is a User, System or Global node.
	/// </summary>
	protected abstract DataToolsCommands.DataObjectType CommandObjectType
	{
		get;
	}


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


	/// <summary>
	/// Activates or opens a virtual file for editing
	/// </summary>
	/// <param name="fileIdentifier"></param>
	/// <param name="doNotShowWindowFrame"></param>
	/// <returns></returns>
	internal IVsWindowFrame ActivateOrOpenVirtualFile(string moniker, object source, Guid editorGuid, bool doNotShowWindowFrame)
	{
		string path;

		bool isProjectDocument = false;

		moniker = moniker.Replace("\\", "{backslash}");
		moniker = moniker.Replace("/", "{slash}");
		moniker = moniker.Replace(":", "{colon}");
		moniker = moniker.Replace("*", "{asterisk}");
		moniker = moniker.Replace("?", "{questionmark}");
		moniker = moniker.Replace("\"", "{quote}");
		moniker = moniker.Replace("<", "{openbracket}");
		moniker = moniker.Replace(">", "{closebracket}");
		moniker = moniker.Replace("|", "{bar}");

#pragma warning disable VSTHRD010 // Invoke single-threaded types on Main thread
		path = Host.ApplicationDataDirectory;
#pragma warning restore VSTHRD010 // Invoke single-threaded types on Main thread
		path = Path.Combine(path, "SqlFiles");
		path = Path.Combine(path, moniker);

		/*
		if (path.Length > 260)
		{
			NotSupportedException ex = new(Resources.SqlViewTextObjectCommandProvider_PathTooLong);
			Diag.Dug(ex);
			throw ex;
		}
		*/
		Diag.Trace(path);

		if (!Directory.Exists(Path.GetDirectoryName(path)))
			Directory.CreateDirectory(Path.GetDirectoryName(path));

		IVsWindowFrame vsWindowFrame = Host.ActivateDocumentIfOpen(path, doNotShowWindowFrame);

		if (vsWindowFrame != null)
			return vsWindowFrame;

		if (!isProjectDocument)
		{
			if (File.Exists(path))
				File.SetAttributes(path, FileAttributes.Normal);

			FileStream fileStream = File.Open(path, FileMode.Create);
			using (fileStream)
			{
				byte[] bytes = Encoding.ASCII.GetBytes((string)source);
				fileStream.Write(bytes, 0, bytes.Length);
			}

			File.SetAttributes(path, FileAttributes.ReadOnly);
		}

		vsWindowFrame = Host.OpenDocumentViaProject(path, editorGuid);

		if (!isProjectDocument)
		{
#pragma warning disable VSTHRD010 // Invoke single-threaded types on Main thread
			NativeMethods.WrapComCall(vsWindowFrame.GetProperty(-4004, out object pvar));
#pragma warning restore VSTHRD010 // Invoke single-threaded types on Main thread

			IVsTextLines vsTextLines = pvar as IVsTextLines;
			if (vsTextLines == null)
			{
#pragma warning disable VSTHRD010 // Invoke single-threaded types on Main thread
				(pvar as IVsTextBufferProvider)?.GetTextBuffer(out vsTextLines);
#pragma warning restore VSTHRD010 // Invoke single-threaded types on Main thread
			}

			if (vsTextLines != null)
			{
				NativeMethods.WrapComCall(vsTextLines.GetStateFlags(out uint pdwReadOnlyFlags));
				pdwReadOnlyFlags |= 1u;
				NativeMethods.WrapComCall(vsTextLines.SetStateFlags(pdwReadOnlyFlags));
			}
		}

		if (vsWindowFrame != null && !doNotShowWindowFrame)
		{
#pragma warning disable VSTHRD010 // Invoke single-threaded types on Main thread
			NativeMethods.WrapComCall(vsWindowFrame.Show());
#pragma warning restore VSTHRD010 // Invoke single-threaded types on Main thread
		}

		return vsWindowFrame;
	}

	protected static bool CanExecute(IVsDataExplorerNode node)
	{
		if (node != null)
		{
			IVsDataObject @object = node.Object;
			if (@object == null || !TypeNameIn(@object.Type.Name, "StoredProcedure", "Function"))
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
				|| TypeNameIn(@object.Type.Name, "Index", "ForeignKey"))
			{
				if ((bool)@object.Properties["IS_COMPUTED"])
					return true;
			}
			else if (@object.Type.Name.EndsWith("Trigger")
				|| TypeNameIn(@object.Type.Name, "View", "StoredProcedure", "Function"))
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
		Diag.Trace("Entry point");

		MenuCommand command = null;

		// TemplateTextCommandProvider contains samples of future commands to be added

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
				command.Enabled = CanOpen(Site.ExplorerConnection.FindNode(itemId));

				if (command.Visible)
					command.Properties["Text"] = Resources.SqlViewTextObjectCommandProvider_Open;

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



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns true if typeName exists in the values array else false.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected static bool TypeNameIn(string typeName, params string[] values)
	{
		foreach (string value in values)
		{
			if (typeName.Equals(value, StringComparison.Ordinal))
			{
				return true;
			}
		}

		return false;
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
	protected void OnNewQuery(int itemId, DataToolsCommands.DataObjectType objectType)
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



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Open text object command event handler.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected void OnOpen(int itemId, DataToolsCommands.DataObjectType objectType)
	{
		// In production. Just send a msg to the outpt window and exit.

		if (itemId != 1234567898)
		{
			Diag.OutputPaneWriteLine("Open script command is not operational yet.");
			return;
		}

		IVsDataExplorerNode node = Site.ExplorerConnection.FindNode(itemId);

		SqlMonikerHelper sqlMoniker = new(node);
		string moniker = sqlMoniker.ToString();

		Diag.Trace(node.Name + ": " + moniker);

		string prop = SqlMonikerHelper.GetNodeScriptProperty(node.Object);
		object source = node.Object.Properties[prop];
		Guid guid = new(DataToolsCommands.SqlEditorGuid);


		DataToolsCommands.CommandObjectType = objectType;

		_ = ActivateOrOpenVirtualFile(moniker, source, guid, false);

	}


	#endregion Event handlers

}
