// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)


using System.Runtime.InteropServices;
using BlackbirdSql.Core.CommandProviders;

namespace BlackbirdSql.Core.Enums;


[Guid(CommandProperties.CommandSetGuid)]


public enum EnCommandSet : uint
{
	MenuIdToplevelMenu = 0xC201, // 49665
	MenuIdScriptToolbar = 0x2000, // 8192
	MenuIdOnlineToolbar = 0x2001, // 8193

	MenuIdScriptToolbarGroup = 0x3000, // 12288
	MenuIdOnlineToolbarGroup = 0x3001, // 12289

	MenuIdSeNodeGroup = 0x3002, // 12290
	MenuIdSeRootGroup = 0x3003, // 12291
	MenuIdResultsTo = 0x3004,

	CtlrIdResultsToGroup = 0x3010,

	ContextIdExecutionSettings = 0x3100,
	ContextIdConnection = 0x3101,
	ContextIdResultsTo = 0x3102,
	ContextIdResultsWindow = 0x3103,
	ContextIdMessageWindow = 0x3104,
	ContextIdShowPlanWindow = 0x3105,

	CmbIdSqlDatabases = 0x3200,
	CmbIdSqlDatabasesGetList = 0x3201,

	CmdIdConnect = 0x0300, // 768
	CmdIdDisconnect = 0x0301,
	CmdIdCloneQuery = 0x0302,
	CmdIdExecuteQuery = 0x0303,
	CmdIdCancelQuery = 0x0304,
	CmdIdShowEstimatedPlan = 0x0305,
	CmdIdToggleIntellisense = 0x0306,
	CmdIdToggleSQLCMDMode = 0x0307,
	CmdIdToggleClientStatistics = 0x0308,
	CmdIdToggleExecutionPlan = 0x0309,
	CmdIdResultsAsText = 0x0310,
	CmdIdResultsAsGrid = 0x0311,
	CmdIdResultsAsFile = 0x0312,
	CmdIdQueryOptions = 0x0313,
	CmdIdToggleResultsPane = 0x0314,
	CmdIdNewQueryConnection = 0x0315,
	CmdidSaveResultsAs = 0x0316,
	CmdidShowPlanXml = 0x0319,
	CmdidShowPlanMissingIndex = 0x0320,
	CmdidPrintPreview = 0x0321,
	CmdidShowPlanSave = 0x0322,
	CmdidShowPlanZoomIn = 0x0323,
	CmdidShowPlanZoomOut = 0x0324,
	CmdidShowPlanZoomCustom = 0x0325,
	CmdidShowPlanZoomToFit = 0x0326,
	CmdidCopyWithHeaders = 0x0327,
	CmdidCycleToNextTab = 0x0328,
	CmdidCycleToPrevious = 0x0329,
	CmdIdParseQuery = 0x0330,
	CmdIdDisconnectAllQueries = 0x0331,
	CmdIdChangeConnection = 0x0332,


	CmdIdOpenTextObject = 0x0333, // 819
	CmdIdOpenAlterTextObject = 0x0334 // 820


}