// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System.Runtime.InteropServices;
using BlackbirdSql.Core.Ctl.CommandProviders;


namespace BlackbirdSql.Core.Ctl.Enums;

[Guid(CommandProperties.CommandSetGuid)]

public enum EnCommandSet
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
	ContextIdExecutionPlanWindow = 0x3105,

	CmbIdSqlDatabases = 0x3200,
	CmbIdSqlDatabasesGetList = 0x3201,

	CmdIdConnect = 0x0300, // 768
	CmdIdDisconnect = 0x0301, // 769
	CmdIdCloneQuery = 0x0302, // 770
	CmdIdExecuteQuery = 0x0304, // 772
	CmdIdCancelQuery = 0x0305, // 773
	CmdIdShowEstimatedPlan = 0x0306, // 774
	CmdIdToggleIntellisense = 0x0307, // 775
	CmdIdToggleSQLCMDMode = 0x0308, // 776
	CmdIdToggleClientStatistics = 0x0309, // 777
	CmdIdToggleExecutionPlan = 0x030A, // 778
	CmdIdResultsAsText = 0x030B, // 779
	CmdIdResultsAsGrid = 0x030C, // 780
	CmdIdResultsAsFile = 0x030D, // 781
	CmdIdQueryOptions = 0x030E, // 782
	CmdIdToggleResultsPane = 0x030F, // 783
	CmdIdNewQueryConnection = 0x0310, // 784
	CmdIdSaveResultsAs = 0x0311, // 785
	CmdIdExecutionPlanXml = 0x0314, // 788
	CmdIdExecutionPlanMissingIndex = 0x0315, // 789
	CmdIdPrintPreview = 0x0316, // 790
	CmdIdExecutionPlanSave = 0x0317, // 791
	CmdIdExecutionPlanZoomIn = 0x0318, // 792
	CmdIdExecutionPlanZoomOut = 0x0319, // 793
	CmdIdExecutionPlanZoomCustom = 0x0320, // 800
	CmdIdExecutionPlanZoomToFit = 0x0321, // 801
	CmdIdCopyWithHeaders = 0x0322, // 802
	CmdIdCycleToNextTab = 0x0323, // 803
	CmdIdCycleToPrevious = 0x0324, // 804
	CmdIdParseQuery = 0x0326, // 806
	CmdIdDisconnectAllQueries = 0x0327, // 807
	CmdIdChangeConnection = 0x0328, // 808

	CmdIdOpenTextObject = 0x0334, // *820
	CmdIdOpenAlterTextObject = 0x0335 // 821

}