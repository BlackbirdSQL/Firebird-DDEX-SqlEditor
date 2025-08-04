// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.ComponentModel.Design;
using BlackbirdSql.Core.Enums;
using BlackbirdSql.Sys.Enums;



namespace BlackbirdSql.Core.Ctl.CommandProviders;


// =========================================================================================================
//											CommandProperties Class
//
/// <summary>
/// Class containing constants and statics for use in data tools commands.
/// Some IDs and Guids from Microsoft.VisualStudio.Data.Providers.SqlServer.SqlDataToolsCommands.
/// </summary>
// =========================================================================================================
internal static class CommandProperties
{
	#region Statics

	// A static class lock
	private static readonly object _LockGlobal = new();

	/// <summary>
	/// The package-wide flag indicating whether or not the current node in the SE is a 'IsSystemObject'
	/// </summary>
	private static EnNodeSystemType _CommandNodeSystemType = EnNodeSystemType.Undefined;

	internal static EnNodeSystemType CommandNodeSystemType
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
	internal static Guid ClsidProvider = new Guid(SystemData.C_ProviderGuid);

	// VS DataToolsCommands providers
	internal const string UniversalCommandProviderGuid = "CD332A3B-B404-45B7-988F-587672086727";

	// Command sets
	internal const string CommandSetGuid = "C6972FD9-9586-438A-800E-1E72AC1FE4E6";
	internal static readonly Guid ClsidCommandSet = new(CommandSetGuid);


	#endregion GUIDs





	// =========================================================================================================
	#region Built-in IDs - CommandProperties
	// =========================================================================================================


	private const int C_CmdIdSENewQueryGlobal = 0x3513; // 13587;
	private const int C_CmdIdSENewQueryLocal = 0x3528; // 13608;
	private const int C_CmdIdAddTableViewForQRY = 0x0027; // 39;
	internal const int C_CmdIdSERetrieveData = 0x3060; // 12384;
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
	internal static CommandID NewQueryGlobal = new CommandID(new Guid(VS.SeDataCommandSetGuid), C_CmdIdSENewQueryGlobal);
	internal static CommandID OverrideNewQueryLocal = new CommandID(new Guid(VS.SeDataCommandSetGuid), C_CmdIdSENewQueryLocal);
	internal static CommandID NewQuery = new CommandID(new Guid(CommandSetGuid), (int)EnCommandSet.CmdIdNewQuery);
	internal static CommandID NewDesignerQuery = new CommandID(new Guid(CommandSetGuid), (int)EnCommandSet.CmdIdNewDesignerQuery);
	internal static CommandID ShowAddTableDialog = new CommandID(new Guid(VS.DavCommandSetGuid), C_CmdIdAddTableViewForQRY);
	internal static CommandID OpenTextObject = new CommandID(new Guid(CommandSetGuid), (int)EnCommandSet.CmdIdOpenTextObject);
	internal static CommandID CreateTextObject = new CommandID(new Guid(CommandSetGuid), (int)EnCommandSet.CmdIdCreateTextObject);
	internal static CommandID AlterTextObject = new CommandID(new Guid(CommandSetGuid), (int)EnCommandSet.CmdIdAlterTextObject);
	internal static CommandID OverrideRetrieveDataLocal = new CommandID(new Guid(VS.SeDataCommandSetGuid), C_CmdIdSERetrieveData);
	internal static CommandID RetrieveDesignerData = new CommandID(new Guid(CommandSetGuid), (int)EnCommandSet.CmdIdRetrieveDesignerData);
	internal static CommandID TraceRct = new CommandID(new Guid(CommandSetGuid), (int)EnCommandSet.CmdIdTraceRct);
	internal static CommandID ValidateSolution = new CommandID(new Guid(CommandSetGuid), (int)EnCommandSet.CmdIdValidateSolution);

	// Unsupported
	/*
	internal static CommandID StartLabelEdit = new CommandID(new Guid(VSConstants.CMDSETID.UIHierarchyWindowCommandSet_string),
		(int)VSConstants.VsUIHierarchyWindowCmdIds.UIHWCMDID_StartLabelEdit);
	internal static CommandID CommitLabelEdit = new CommandID(new Guid(VSConstants.CMDSETID.UIHierarchyWindowCommandSet_string),
		(int)VSConstants.VsUIHierarchyWindowCmdIds.UIHWCMDID_CommitLabelEdit);
	internal static CommandID CancelLabelEdit = new CommandID(new Guid(VSConstants.CMDSETID.UIHierarchyWindowCommandSet_string),
		(int)VSConstants.VsUIHierarchyWindowCmdIds.UIHWCMDID_CancelLabelEdit);

	internal static CommandID ExecuteTextObject = new CommandID(new Guid(VS.SeDataCommandSetGuid), _CmdIdSERun);
	internal static CommandID GlobalNewDiagram = new CommandID(new Guid(VS.SeDataCommandSetGuid), 12352);
	internal static CommandID GlobalNewTable = new CommandID(new Guid(VS.SeDataCommandSetGuid), 12353);
	internal static CommandID GlobalNewView = new CommandID(new Guid(VS.SeDataCommandSetGuid), 12355);
	internal static CommandID GlobalNewStoredProcedure = new CommandID(new Guid(VS.SeDataCommandSetGuid), 12356);
	internal static CommandID GlobalNewScalarFunction = new CommandID(new Guid(VS.SeDataCommandSetGuid), 12357);
	internal static CommandID GlobalNewTableValuedFunction = new CommandID(new Guid(VS.SeDataCommandSetGuid), 12358);
	internal static CommandID GlobalNewInlineTableValuedFunction = new CommandID(new Guid(VS.SeDataCommandSetGuid), 12359);
	internal static CommandID UpdateScript = new CommandID(new Guid(VS.SeDataCommandSetGuid), _CmdIdUpdateScript);
	internal static CommandID NewTrigger = new CommandID(new Guid(VS.SeDataCommandSetGuid), _CmdIdNewTrigger);
	internal static CommandID GlobalNewTrigger = new CommandID(new Guid(VS.SeDataCommandSetGuid), _CmdIdAddTrigger);
	internal static CommandID NewDiagram = new CommandID(new Guid(VS.SeDataCommandSetGuid), 13610);
	internal static CommandID ShowDiagramAddTableDialog = new CommandID(new Guid(VS.DavCommandSetGuid), 33);
	internal static CommandID NewTable = new CommandID(new Guid(VS.SeDataCommandSetGuid), 13600);
	internal static CommandID NewView = new CommandID(new Guid(VS.SeDataCommandSetGuid), 13601);
	internal static CommandID NewStoredProcedure = new CommandID(new Guid(VS.SeDataCommandSetGuid), 13602);
	internal static CommandID NewScalarFunction = new CommandID(new Guid(VS.SeDataCommandSetGuid), 13605);
	internal static CommandID NewTableValuedFunction = new CommandID(new Guid(VS.SeDataCommandSetGuid), 13604);
	internal static CommandID NewInlineTableValuedFunction = new CommandID(new Guid(VS.SeDataCommandSetGuid), 13603);
	internal static CommandID DesignDiagram = new CommandID(new Guid(VS.SeDataCommandSetGuid), 12291);
	internal static CommandID DesignTabularObject = new CommandID(new Guid(VS.SeDataCommandSetGuid), 12291);
	internal static CommandID DebugTextObject = new CommandID(new Guid(VS.SeDataCommandSetGuid), 12306);
	internal static CommandID ScriptObject = new CommandID(new Guid(VS.SeDataCommandSetGuid), 12347);
	internal static CommandID BrowseIntoSSDT = new CommandID(new Guid(VS.SeDataCommandSetGuid), 13592);
	*/

	#endregion Command IDs


}