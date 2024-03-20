// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.ComponentModel.Design;
using BlackbirdSql.Core.Ctl.Enums;
using BlackbirdSql.Core.Model.Enums;

namespace BlackbirdSql.Core.Ctl.CommandProviders;

// =========================================================================================================
//											CommandProperties Class
//
/// <summary>
/// Class containing constants and statics for use in data tools commands.
/// Some IDs and Guids from Microsoft.VisualStudio.Data.Providers.SqlServer.SqlDataToolsCommands.
/// </summary>
// =========================================================================================================
public static class CommandProperties
{
	#region Statics

	// A static class lock
	private static readonly object _LockGlobal = new();

	/// <summary>
	/// The package-wide flag indicating whether or not the current node in the SE is a 'IsSystemObject'
	/// </summary>
	private static EnNodeSystemType _CommandNodeSystemType = EnNodeSystemType.Undefined;

	public static EnNodeSystemType CommandNodeSystemType
	{
		get { lock(_LockGlobal) { return _CommandNodeSystemType; } }
		set { lock (_LockGlobal) { _CommandNodeSystemType = value; } }
	}

	/// <summary>
	/// The package-wide flag indicating the last command type
	/// </summary>
	// public static DataObjectType CommandLastObjectType = DataObjectType.None;

	#endregion Statics





	// =========================================================================================================
	#region GUIDs - CommandProperties
	// =========================================================================================================

	// Provider Clsid.
	public static Guid ClsidProvider = new Guid(SystemData.ProviderGuid);

	// VS DataToolsCommands providers
	public const string UniversalCommandProviderGuid = "CD332A3B-B404-45B7-988F-587672086727";

	// Command sets
	public const string CommandSetGuid = "C6972FD9-9586-438A-800E-1E72AC1FE4E6";
	public static readonly Guid ClsidCommandSet = new(CommandSetGuid);


	#endregion GUIDs





	// =========================================================================================================
	#region Built-in IDs - CommandProperties
	// =========================================================================================================


	private const int C_CmdIdSENewQueryGlobal = 0x3513; // 13587;
	private const int C_CmdIdSENewQueryLocal = 0x3528; // 13608;
	private const int C_CmdIdAddTableViewForQRY = 0x0027; // 39;
	public const int C_CmdIdSERetrieveData = 0x3060; // 12384;
	// private const int _CmdIdSERun = 0x3062; // 12386;
	// private const int _CmdIdSEDetachDatabase = 0x3517; // 13591;
	// private const int _CmdIdUpdateScript = 0x3039; // 12345;
	// private const int _CmdIdNewTrigger = 0x3527; // 13607;
	// private const int _CmdIdAddTrigger = 0x304A; // 12362;
	// private const int _CmdIdCopy = 0x000F; // 15;


	#endregion IDs





	// =========================================================================================================
	#region Command IDs - CommandProperties
	// =========================================================================================================


	// public static CommandID DetachDatabase = new CommandID(new Guid(VS.SeDataCommandSetGuid), _CmdIdSEDetachDatabase);
	public static CommandID NewQueryGlobal = new CommandID(new Guid(VS.SeDataCommandSetGuid), C_CmdIdSENewQueryGlobal);
	public static CommandID OverrideNewQueryLocal = new CommandID(new Guid(VS.SeDataCommandSetGuid), C_CmdIdSENewQueryLocal);
	public static CommandID NewSqlQuery = new CommandID(new Guid(CommandSetGuid), (int)EnCommandSet.CmdIdNewSqlQuery);
	public static CommandID NewDesignerQuery = new CommandID(new Guid(CommandSetGuid), (int)EnCommandSet.CmdIdNewDesignerQuery);
	public static CommandID ShowAddTableDialog = new CommandID(new Guid(VS.DavCommandSetGuid), C_CmdIdAddTableViewForQRY);
	public static CommandID OpenTextObject = new CommandID(new Guid(CommandSetGuid), (int)EnCommandSet.CmdIdOpenTextObject);
	public static CommandID OpenAlterTextObject = new CommandID(new Guid(CommandSetGuid), (int)EnCommandSet.CmdIdOpenAlterTextObject);
	public static CommandID OverrideRetrieveDataLocal = new CommandID(new Guid(VS.SeDataCommandSetGuid), C_CmdIdSERetrieveData);
	public static CommandID RetrieveDesignerData = new CommandID(new Guid(CommandSetGuid), (int)EnCommandSet.CmdIdRetrieveDesignerData);
	public static CommandID TraceRct = new CommandID(new Guid(CommandSetGuid), (int)EnCommandSet.CmdIdTraceRct);
	public static CommandID ValidateSolution = new CommandID(new Guid(CommandSetGuid), (int)EnCommandSet.CmdIdValidateSolution);

	// Unsupported
	/*
	public static CommandID StartLabelEdit = new CommandID(new Guid(VSConstants.CMDSETID.UIHierarchyWindowCommandSet_string),
		(int)VSConstants.VsUIHierarchyWindowCmdIds.UIHWCMDID_StartLabelEdit);
	public static CommandID CommitLabelEdit = new CommandID(new Guid(VSConstants.CMDSETID.UIHierarchyWindowCommandSet_string),
		(int)VSConstants.VsUIHierarchyWindowCmdIds.UIHWCMDID_CommitLabelEdit);
	public static CommandID CancelLabelEdit = new CommandID(new Guid(VSConstants.CMDSETID.UIHierarchyWindowCommandSet_string),
		(int)VSConstants.VsUIHierarchyWindowCmdIds.UIHWCMDID_CancelLabelEdit);

	public static CommandID ExecuteTextObject = new CommandID(new Guid(VS.SeDataCommandSetGuid), _CmdIdSERun);
	public static CommandID GlobalNewDiagram = new CommandID(new Guid(VS.SeDataCommandSetGuid), 12352);
	public static CommandID GlobalNewTable = new CommandID(new Guid(VS.SeDataCommandSetGuid), 12353);
	public static CommandID GlobalNewView = new CommandID(new Guid(VS.SeDataCommandSetGuid), 12355);
	public static CommandID GlobalNewStoredProcedure = new CommandID(new Guid(VS.SeDataCommandSetGuid), 12356);
	public static CommandID GlobalNewScalarFunction = new CommandID(new Guid(VS.SeDataCommandSetGuid), 12357);
	public static CommandID GlobalNewTableValuedFunction = new CommandID(new Guid(VS.SeDataCommandSetGuid), 12358);
	public static CommandID GlobalNewInlineTableValuedFunction = new CommandID(new Guid(VS.SeDataCommandSetGuid), 12359);
	public static CommandID UpdateScript = new CommandID(new Guid(VS.SeDataCommandSetGuid), _CmdIdUpdateScript);
	public static CommandID NewTrigger = new CommandID(new Guid(VS.SeDataCommandSetGuid), _CmdIdNewTrigger);
	public static CommandID GlobalNewTrigger = new CommandID(new Guid(VS.SeDataCommandSetGuid), _CmdIdAddTrigger);
	public static CommandID NewDiagram = new CommandID(new Guid(VS.SeDataCommandSetGuid), 13610);
	public static CommandID ShowDiagramAddTableDialog = new CommandID(new Guid(VS.DavCommandSetGuid), 33);
	public static CommandID NewTable = new CommandID(new Guid(VS.SeDataCommandSetGuid), 13600);
	public static CommandID NewView = new CommandID(new Guid(VS.SeDataCommandSetGuid), 13601);
	public static CommandID NewStoredProcedure = new CommandID(new Guid(VS.SeDataCommandSetGuid), 13602);
	public static CommandID NewScalarFunction = new CommandID(new Guid(VS.SeDataCommandSetGuid), 13605);
	public static CommandID NewTableValuedFunction = new CommandID(new Guid(VS.SeDataCommandSetGuid), 13604);
	public static CommandID NewInlineTableValuedFunction = new CommandID(new Guid(VS.SeDataCommandSetGuid), 13603);
	public static CommandID DesignDiagram = new CommandID(new Guid(VS.SeDataCommandSetGuid), 12291);
	public static CommandID DesignTabularObject = new CommandID(new Guid(VS.SeDataCommandSetGuid), 12291);
	public static CommandID DebugTextObject = new CommandID(new Guid(VS.SeDataCommandSetGuid), 12306);
	public static CommandID ScriptObject = new CommandID(new Guid(VS.SeDataCommandSetGuid), 12347);
	public static CommandID BrowseIntoSSDT = new CommandID(new Guid(VS.SeDataCommandSetGuid), 13592);
	*/

	#endregion Command IDs


}