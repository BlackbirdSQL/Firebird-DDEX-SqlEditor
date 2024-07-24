// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System.Data;
using System.Drawing;
using BlackbirdSql.Sys.Enums;
using BlackbirdSql.Core.Enums;
using BlackbirdSql.Shared.Enums;



namespace BlackbirdSql.Shared.Interfaces;


// =========================================================================================================
//									IBsEditorTransientSettings Interface
//
// =========================================================================================================
public interface IBsEditorTransientSettings
{


	// ----------------------------------------------------
	#region Property Accessors - IBsEditorTransientSettings
	// ----------------------------------------------------


	bool EditorEnableIntellisense { get; set; }
	bool EditorExecuteQueryOnOpen { get; set; }
	bool EditorPromptToSave { get; set; }
	EnLanguageService EditorLanguageService { get; set; }


	// Editor ContextSettingsModel
	EnStatusBarPosition EditorContextStatusBarPosition { get; set; }
	string EditorContextBatchSeparator { get; set; }


	// Editor TabAndStatusBarSettingsModel
	EnExecutionTimeMethod EditorStatusBarExecutionTimeMethod { get; set; }
	bool EditorStatusBarIncludeDatabaseName { get; set; }
	bool EditorStatusBarIncludeLoginName { get; set; }
	bool EditorStatusBarIncludeRowCount { get; set; }
	bool EditorStatusBarIncludeServerName { get; set; }
	Color EditorStatusBarBackgroundColor { get; set; }
	bool EditorStatusTabTextIncludeDatabaseName { get; set; }
	bool EditorStatusTabTextIncludeLoginName { get; set; }
	bool EditorStatusTabTextIncludeFileName { get; set; }
	bool EditorStatusTabTextIncludeServerName { get; set; }


	// Editor ExecutionSettingsModel
	bool EditorExecutionTtsDefault { get; set; }
	int EditorExecutionSetRowCount { get; set; }
	EnBlobSubType EditorExecutionSetBlobDisplay { get; set; }
	int EditorExecutionTimeout { get; set; }

	// Editor ExecutionAdvancedSettingsModel
	bool EditorExecutionSetCount { get; set; }
	bool EditorExecutionSetNoExec { get; set; }
	bool EditorExecutionSetShowplanText { get; set; }
	bool EditorExecutionSetPlanXml { get; set; }
	bool EditorExecutionSetParseOnly { get; set; }
	bool EditorExecutionSetConcatenationNull { get; set; }
	bool EditorExecutionSetBail { get; set; }
	bool EditorExecutionSetPlanText { get; set; }
	bool EditorExecutionSetStats { get; set; }
	bool EditorExecutionSetStatisticsIO { get; set; }
	bool EditorExecutionSetWarnings { get; set; }
	IsolationLevel EditorExecutionIsolationLevel { get; set; }
	EnDeadlockPriority EditorExecutionDeadlockPriority { get; set; }
	bool EditorExecutionDeadlockPriorityLow { get; set; }
	int EditorExecutionLockTimeout { get; set; }
	int EditorExecutionCostLimit { get; set; }
	bool EditorExecutionSuppressHeaders { get; set; }
	bool EditorExecutionDisconnectOnCompletion { get; set; }


	// Editor ResultsSettingsModel
	EnSqlOutputMode EditorResultsOutputMode { get; set; }
	string EditorResultsDirectory { get; set; }
	bool EditorResultsPlaySounds { get; set; }

	// Grid results options.
	bool EditorResultsGridOutputQuery { get; set; }
	bool EditorResultsGridSingleTab { get; set; }
	bool EditorResultsGridSaveIncludeHeaders { get; set; }
	bool EditorResultsGridCsvQuoteStringsCommas { get; set; }
	bool EditorResultsGridDiscardResults { get; set; }
	bool EditorResultsGridSeparateTabs { get; set; }
	bool EditorResultsGridSwitchToResults { get; set; }
	int EditorResultsGridMaxCharsPerColumnStd { get; set; }
	int EditorResultsGridMaxCharsPerColumnXml { get; set; }

	// Text results options.
	bool EditorResultsTextIncludeHeaders { get; set; }
	bool EditorResultsTextOutputQuery { get; set; }
	bool EditorResultsTextScrollingResults { get; set; }
	bool EditorResultsTextAlignRightNumerics { get; set; }
	bool EditorResultsTextDiscardResults { get; set; }
	int EditorResultsTextMaxCharsPerColumnStd { get; set; }
	bool EditorResultsTextSeparateTabs { get; set; }
	bool EditorResultsTextSwitchToResults { get; set; }
	EnSqlOutputFormat EditorResultsTextOutputFormat { get; set; }
	char EditorResultsTextDelimiter { get; set; }



	EnSqlExecutionType ExecutionType { get; set; }
	bool WithActualPlan { get; set; }
	bool WithClientStats { get; set; }
	bool WithProfiling { get; set; }
	bool WithNoExec { get; set; }
	bool WithStatisticsTime { get; set; }
	bool WithStatisticsIO { get; set; }
	bool WithStatisticsProfile { get; set; }
	bool TtsEnabled { get; set; }
	public bool SuppressProviderMessageHeaders { get; set; }


	#endregion Property Accessors

}