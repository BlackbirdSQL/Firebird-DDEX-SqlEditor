// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.ComponentModel.Design;

using BlackbirdSql.Core.Ctl.Enums;


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


	/// <summary>
	/// The package-wide flag indicating whether or not the current node in the SE is a 'IsSystemObject'
	/// </summary>
	public static DataObjectType CommandObjectType = DataObjectType.None;

	/// <summary>
	/// The package-wide flag indicating the last command type
	/// </summary>
	// public static DataObjectType CommandLastObjectType = DataObjectType.None;

	#endregion Statics





	// =========================================================================================================
	#region GUIDs - CommandProperties
	// =========================================================================================================


	public const string DetachCommandProviderGuid = "8C591813-BB90-4B5C-BD7B-5A286D130D2E";
	public const string SystemQueryCommandProviderGuid = "C253F0FC-D24B-4BE4-A2DE-57502D677A09";
	public const string UserQueryCommandProviderGuid = "B07EDD71-7F1E-4A14-8BB0-38A30C72251D";
	public const string UniversalCommandProviderGuid = "CD332A3B-B404-45B7-988F-587672086727";
	public const string SystemOpenTextCommandProviderGuid = "CC8C9523-2143-4CDA-B2BC-129A8B2D0809";


	public const string MenuGroupGuid = "501822E1-B5AF-11d0-B4DC-00A0C91506EF";
	public const string MenuGroupDavGuid = "732ABE75-CD80-11d0-A2DB-00AA00A3EFFF";

	public const string ServerExplorerObjectNodesGuid = "d4f02a6a-c5ae-4bf2-938d-f1625bdca0e2";
	public const string CommandSetGuid = "13CD7876-FC84-4DDA-91BF-4CDBF893B134";


	#endregion GUIDs





	// =========================================================================================================
	#region Built-in IDs - CommandProperties
	// =========================================================================================================


	private const int _CmdIdSENewQuery = 13587;       // 0x3513
	private const int _CmdIdSELocalNewQuery = 13608;  // 0x3528
	private const int _CmdIdAddTableViewForQRY = 39; // 0x0027
	private const int _CmdIdSERetrieveData = 12384;   // 0x3060
	private const int _CmdIdSERun = 12386;            // 0x3062
	private const int _CmdIdSEDetachDatabase = 13591; // 0x3517
	private const int _CmdIdUpdateScript = 12345;     // 0x3039
	private const int _CmdIdNewTrigger = 13607;       // 0x3527
	private const int _CmdIdAddTrigger = 12362;       // 0x304A


	#endregion IDs





	// =========================================================================================================
	#region Command IDs - CommandProperties
	// =========================================================================================================


	public static CommandID DetachDatabase = new CommandID(new Guid(MenuGroupGuid), _CmdIdSEDetachDatabase);
	public static CommandID GlobalNewQuery = new CommandID(new Guid(MenuGroupGuid), _CmdIdSENewQuery);
	public static CommandID NewQuery = new CommandID(new Guid(MenuGroupGuid), _CmdIdSELocalNewQuery);
	public static CommandID ShowAddTableDialog = new CommandID(new Guid(MenuGroupDavGuid), _CmdIdAddTableViewForQRY);
	public static CommandID OpenTextObject = new CommandID(new Guid(CommandSetGuid), (int)EnCommandSet.CmdIdOpenTextObject);
	public static CommandID OpenAlterTextObject = new CommandID(new Guid(CommandSetGuid), (int)EnCommandSet.CmdIdOpenAlterTextObject);
	public static CommandID RetrieveData = new CommandID(new Guid(MenuGroupGuid), _CmdIdSERetrieveData);
	public static CommandID ExecuteTextObject = new CommandID(new Guid(MenuGroupGuid), _CmdIdSERun);


	// Unsupported
	public static CommandID GlobalNewDiagram = new CommandID(new Guid(MenuGroupGuid), 12352);
	public static CommandID GlobalNewTable = new CommandID(new Guid(MenuGroupGuid), 12353);
	public static CommandID GlobalNewView = new CommandID(new Guid(MenuGroupGuid), 12355);
	public static CommandID GlobalNewStoredProcedure = new CommandID(new Guid(MenuGroupGuid), 12356);
	public static CommandID GlobalNewScalarFunction = new CommandID(new Guid(MenuGroupGuid), 12357);
	public static CommandID GlobalNewTableValuedFunction = new CommandID(new Guid(MenuGroupGuid), 12358);
	public static CommandID GlobalNewInlineTableValuedFunction = new CommandID(new Guid(MenuGroupGuid), 12359);
	public static CommandID UpdateScript = new CommandID(new Guid(MenuGroupGuid), _CmdIdUpdateScript);
	public static CommandID NewTrigger = new CommandID(new Guid(MenuGroupGuid), _CmdIdNewTrigger);
	public static CommandID GlobalNewTrigger = new CommandID(new Guid(MenuGroupGuid), _CmdIdAddTrigger);
	public static CommandID ApplicationDebugging = new CommandID(new Guid(MenuGroupGuid), 13589);
	public static CommandID AllowSqlClrDebugging = new CommandID(new Guid(MenuGroupGuid), 13590);
	public static CommandID EndApplicationDebugging = new CommandID(new Guid(MenuGroupGuid), 13632);
	public static CommandID NewDiagram = new CommandID(new Guid(MenuGroupGuid), 13610);
	public static CommandID ShowDiagramAddTableDialog = new CommandID(new Guid(MenuGroupDavGuid), 33);
	public static CommandID NewTable = new CommandID(new Guid(MenuGroupGuid), 13600);
	public static CommandID NewView = new CommandID(new Guid(MenuGroupGuid), 13601);
	public static CommandID NewStoredProcedure = new CommandID(new Guid(MenuGroupGuid), 13602);
	public static CommandID NewScalarFunction = new CommandID(new Guid(MenuGroupGuid), 13605);
	public static CommandID NewTableValuedFunction = new CommandID(new Guid(MenuGroupGuid), 13604);
	public static CommandID NewInlineTableValuedFunction = new CommandID(new Guid(MenuGroupGuid), 13603);
	public static CommandID DesignDiagram = new CommandID(new Guid(MenuGroupGuid), 12291);
	public static CommandID DesignTabularObject = new CommandID(new Guid(MenuGroupGuid), 12291);
	public static CommandID DebugTextObject = new CommandID(new Guid(MenuGroupGuid), 12306);
	public static CommandID ScriptObject = new CommandID(new Guid(MenuGroupGuid), 12347);
	public static CommandID BrowseIntoSSDT = new CommandID(new Guid(MenuGroupGuid), 13592);

	#endregion Command IDs





	// =========================================================================================================
	#region Child classes - CommandProperties
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Enumerator for flagging the current SE node as 'IsSystemObject' or not
	/// </summary>
	// ---------------------------------------------------------------------------------
	public enum DataObjectType
	{
		None = 0,
		Global = 1,
		User = 2,
		System = 3
	};


	#endregion Child classes

}