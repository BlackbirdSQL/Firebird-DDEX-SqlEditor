// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)


using BlackbirdSql.Core;
using System.Runtime.InteropServices;

namespace BlackbirdSql.Common.Enums;


[Guid(LibraryData.SqlEditorCommandSetGuid)]


public enum SqlEditorCmdSet : uint
{
	MnuIdToplevelMenu = 0xc201, // 49665
	MenuIdScriptToolbar = 0x2000, // 8192
	MenuIdOnlineToolbar = 0x2001,

	CtlrIdResultsTo = 0x2101,

	MenuIdScriptToolbarGroup = 0x1000, // 4096
	MenuIdOnlineToolbarGroup = 0x1001,

	CtlrIdResultsToGroup = 0x1101,



	ContextIdExecutionSettings = 0x0100, // 256
	ContextIdConnection = 0x0102,
	ContextIdResultsTo = 0x0103,
	ContextIdResultsWindow = 0x0104,
	ContextIdMessageWindow = 0x0105,
	ContextIdShowPlanWindow = 0x0106,

	// MenuCtlrIdDatabases = 0x3000,
	// GroupIdDatabases = 0x3001,
	// AnchorIdDatabases = 0x3002,
	// SeedIdDatabases = 0x3003,

	CmbIdSQLDatabases = 0x4000,
	CmbIdSQLDatabasesGetList = 0x4001,

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


}