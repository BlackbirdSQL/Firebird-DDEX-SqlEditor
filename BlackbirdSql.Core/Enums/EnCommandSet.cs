// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System.Runtime.InteropServices;
using BlackbirdSql.Core.Ctl.CommandProviders;


namespace BlackbirdSql.Core.Enums;


/// <summary>
/// When specific, suffix is the parent/owner.
/// </summary>
[Guid(CommandProperties.CommandSetGuid)]
public enum EnCommandSet
{
	MenuIdToplevel = 0xC201, // 49665
	ToolbarIdEditorWindow = 0x5000, // 20480
	ToolbarIdOnlineWindow = 0x5001, // 20481

	GrpIdToolbarExecute = 0x5100, // 20736
	GrpIdOnlineToolbar = 0x5101, // 20737
	GrpIdReusableWell = 0x5102, // 20738
	GrpIdToolbarConnection = 0x5103, // 20738
	GrpIdToolbarNew = 0x5104, // 20739
	GrpIdToolbarOutput = 0x5105, // 20740
	GrpIdToolbarAdditional = 0x5106, // 20741

	GrpIdSeRootContext = 0x5200, // 20992
	GrpIdSeNodeContext = 0x5201, // 20993
	GrpIdSeStaticContext = 0x5202, // 20994
	CtlrIdResultsToToolbar = 0x5203, // 20995
	CtlrIdExecuteToolbar = 0x5205, // 20997

	GrpIdResultsToController = 0x5300,

	GrpIdEditorWindowContext = 0x5400,
	GrpIdResultsWindowContextSelect = 0x5401,
	GrpIdResultsWindowContextPrint = 0x5402,
	ContextIdResultsWindow = 0x5403,
	ContextIdMessageWindow = 0x5404,
	ContextIdExecutionPlanWindow = 0x5405,

	CmbIdDatabaseSelect = 0x5500,
	CmbIdDatabaseList = 0x5501,

	CmdIdConnect = 0x0300, // 768
	CmdIdDisconnect = 0x0301, // 769
	CmdIdCloneQuery = 0x0302, // 770
	CmdIdExecuteQuery = 0x0304, // 772
	CmdIdCancelQuery = 0x0305, // 773
	CmdIdShowEstimatedPlan = 0x0306, // 774
	CmdIdToggleIntellisense = 0x0307, // 775
	CmdIdToggleClientStatistics = 0x0309, // 777
	CmdIdToggleExecutionPlan = 0x030A, // 778
	CmdIdResultsAsText = 0x030B, // 779
	CmdIdResultsAsGrid = 0x030C, // 780
	CmdIdResultsAsFile = 0x030D, // 781
	CmdIdQuerySettings = 0x030E, // 782
	CmdIdToggleResultsPane = 0x030F, // 783
	CmdIdNewQuery = 0x0310, // 784
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
	CmdIdOleCycleToNextTab = 0x0323, // 803
	CmdIdOleCycleToPreviousTab = 0x0324, // 804
	CmdIdDisconnectAllQueries = 0x0327, // 807
	CmdIdModifyConnection = 0x0328, // 808

	CmdIdOpenTextObject = 0x0334, // *820
	CmdIdOpenAlterTextObject = 0x0335, // 821
	CmdIdRetrieveDesignerData = 0x0336, // 822
	CmdIdResetPageOptions = 0x0337, // 823
	CmdIdNewDesignerQuery = 0x0338, // 824
	CmdIdTraceRct = 0x0339, // 825
	CmdIdValidateSolution = 0x033A, // 826
	CmdIdTransactionCommit = 0x033B, // 827
	CmdIdTransactionRollback = 0x033C, // 829
	CmdIdToggleTTS = 0x033D, // 830
	CmdIdQuerySettingsToolbar = 0x033E // 831
}