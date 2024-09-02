// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Resources;
using BlackbirdSql.Core.Enums;
using BlackbirdSql.Shared.Enums;
using BlackbirdSql.Shared.Properties;
using BlackbirdSql.Sys.Enums;
using BlackbirdSql.Sys.Events;



namespace BlackbirdSql.Shared.Ctl.Config;


// =============================================================================================================
//										PersistentSettings Class
//
/// <summary>
/// Consolidated single access point for daisy-chained packages settings models (IBsSettingsModel).
/// As a rule we name descendent classes PersistentSettings as well. We hardcode bind the PersistentSettings
/// descendent tree from the top-level extension lib down to the Core.
/// PersistentSettings can be either consumers or providers of options, or both.
/// There is no point using services as this configuration is fixed. ie:
/// VisualStudio.Ddex > Controller > EditorExtension > LanguageExtension > Common > Core.
/// </summary>
// =============================================================================================================
public abstract class PersistentSettings : Core.Ctl.Config.PersistentSettings
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - PersistentSettings
	// ---------------------------------------------------------------------------------


	/// <summary>
	/// Protected singleton .ctor
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected PersistentSettings() : base()
	{
	}

	protected PersistentSettings(bool live) : base(live)
	{
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - PersistentSettings
	// =========================================================================================================


	// Tracking variables
	private static bool _LayoutPropertyChanged = false;


	#endregion Fields





	// =========================================================================================================
	#region Property Accessors - PersistentSettings
	// =========================================================================================================


	// LanguageService AdvancedPreferencesModel

	public static bool LanguageServiceEnableIntellisense => (bool)GetSetting("LanguageServiceAdvancedEnableIntellisense", true);
	public static bool LanguageServiceAutoOutlining => LanguageServiceEnableIntellisense
		&& (bool)GetSetting("LanguageServiceAdvancedAutoOutlining", true);
	public static bool LanguageServiceUnderlineErrors => LanguageServiceEnableIntellisense
		&& (bool)GetSetting("LanguageServiceAdvancedUnderlineErrors", true);
	public static int LanguageServiceMaxScriptSize => (int)GetSetting("LanguageServiceAdvancedMaxScriptSize", 1048576);
	public static EnCasingStyle LanguageServiceTextCasing => (EnCasingStyle)GetSetting("LanguageServiceAdvancedTextCasing", EnCasingStyle.Uppercase);
	// _DisplayInfoProvider.BuiltInCasing = Prefs.TextCasing == 0 ? CasingStyle.Uppercase : CasingStyle.Lowercase;



	// Editor GeneralSettingsModel

	public static EnIntellisensePolicy EditorIntellisensePolicy => (EnIntellisensePolicy)GetSetting("EditorGeneralIntellisensePolicy", EnIntellisensePolicy.ActiveOnly);
	public static bool EditorExecuteQueryOnOpen => (bool)GetSetting("EditorGeneralExecuteQueryOnOpen", true);
	public static EnLanguageService EditorLanguageService => (EnLanguageService)GetSetting("EditorGeneralLanguageService",
		EnLanguageService.SSDT);
	public static bool EditorPromptSave => (bool)GetSetting("EditorGeneralPromptSave", true);
	public static bool EditorTtsDefault => (bool)GetSetting("EditorGeneralTtsDefault", true);



	// Editor TabAndStatusBarSettingsModel

	public static EnExecutionTimeMethod EditorStatusBarExecutionTimeMethod =>
		(EnExecutionTimeMethod)GetSetting("EditorStatusBarExecutionTimeMethod", EnExecutionTimeMethod.Elapsed);
	public static bool EditorStatusBarIncludeDatabaseName => (bool)GetSetting("EditorStatusBarIncludeDatabaseName", true);
	public static bool EditorStatusBarIncludeLoginName => (bool)GetSetting("EditorStatusBarIncludeLoginName", true);
	public static bool EditorStatusBarIncludeRowCount => (bool)GetSetting("EditorStatusBarIncludeRowCount", true);
	public static bool EditorStatusBarIncludeServerName => (bool)GetSetting("EditorStatusBarIncludeServerName", true);
	public static Color EditorStatusBarBackgroundColor => (Color)GetSetting("EditorStatusBarBackgroundColor", SystemColors.Control);
	public static bool EditorStatusTabTextIncludeDatabaseName => (bool)GetSetting("EditorStatusTabTextIncludeDatabaseName", false);
	public static bool EditorStatusTabTextIncludeLoginName => (bool)GetSetting("EditorStatusTabTextIncludeLoginName", false);
	public static bool EditorStatusTabTextIncludeFileName => (bool)GetSetting("EditorStatusTabTextIncludeFileName", true);
	public static bool EditorStatusTabTextIncludeServerName => (bool)GetSetting("EditorStatusTabTextIncludeServerName", false);

	// Editor ContextSettingsModel
	public static EnStatusBarPosition EditorContextStatusBarPosition =>
		(EnStatusBarPosition)GetSetting("EditorContextStatusBarPosition", EnStatusBarPosition.Bottom);


	// Editor ExecutionSettingsModel

	public static int EditorExecutionSetRowCount => (int)GetSetting("EditorExecutionGeneralSetRowCount", SharedConstants.C_DefaultSetRowCount);
	public static EnBlobSubType EditorExecutionSetBlobDisplay => (EnBlobSubType)GetSetting("EditorExecutionGeneralSetBlobDisplay", SharedConstants.C_DefaultSetBlobDisplay);
	public static int EditorExecutionTimeout => (int)GetSetting("EditorExecutionGeneralExecutionTimeout", SharedConstants.C_DefaultExecutionTimeout);

	public static string EditorExecutionBatchSeparator
	{
		get
		{
			string value = (string)GetSetting("EditorExecutionGeneralBatchSeparator", SharedConstants.C_DefaultBatchSeparator);

			value = value.Replace(" ", "");

			if (value == "")
				value = ";";

			return value;
		}
	}

	// Editor ExecutionAdvancedSettingsModel
	public static bool EditorExecutionSetCount => (bool)GetSetting("EditorExecutionAdvancedSetCount", SharedConstants.C_DefaultSetCount);
	public static bool EditorExecutionSetNoExec => (bool)GetSetting("EditorExecutionAdvancedSetNoExec", SharedConstants.C_DefaultSetPlanOnly);
	public static bool EditorExecutionSetShowplanText => (bool)GetSetting("EditorExecutionAdvancedSetShowplanText", SharedConstants.C_DefaultSetPlan);
	public static bool EditorExecutionSetPlanXml => (bool)GetSetting("EditorExecutionAdvancedSetPlanXml", SharedConstants.C_DefaultSetExplain);
	public static bool EditorExecutionSetParseOnly =>
		(bool)GetSetting("EditorExecutionAdvancedSetParseOnly", SharedConstants.C_DefaultSetParseOnly);
	public static bool EditorExecutionSetConcatenationNull =>
		(bool)GetSetting("EditorExecutionAdvancedSetConcatenationNull", SharedConstants.C_DefaultSetConcatenationNull);
	public static bool EditorExecutionSetBail => (bool)GetSetting("EditorExecutionAdvancedSetBail", SharedConstants.C_DefaultSetBail);
	public static bool EditorExecutionSetPlanText => (bool)GetSetting("EditorExecutionAdvancedSetPlanText", SharedConstants.C_DefaultSetPlanText);
	public static bool EditorExecutionSetStats => (bool)GetSetting("EditorExecutionAdvancedSetStats", SharedConstants.C_DefaultSetStats);
	public static bool EditorExecutionSetStatisticsIO =>
		(bool)GetSetting("EditorExecutionAdvancedSetStatisticsIO", SharedConstants.C_DefaultSetStatisticsIO);
	public static bool EditorExecutionSetWarnings => (bool)GetSetting("EditorExecutionAdvancedSetWarnings", SharedConstants.C_DefaultSetWarnings);
	public static IsolationLevel EditorExecutionIsolationLevel
		=> (IsolationLevel)(int)GetSetting("EditorExecutionAdvancedIsolationLevel", SysConstants.C_DefaultIsolationLevel);
	public static EnDeadlockPriority EditorExecutionDeadlockPriority
		=> (EnDeadlockPriority)(int)GetSetting("EditorExecutionAdvancedIsolationLevel", EnDeadlockPriority.Low);
	public static bool EditorExecutionDeadlockPriorityLow => EditorExecutionDeadlockPriority == EnDeadlockPriority.Low;
	public static int EditorExecutionLockTimeout => (int)GetSetting("EditorExecutionAdvancedLockTimeout", SharedConstants.C_DefaultLockTimeout);
	public static int EditorExecutionCostLimit => (int)GetSetting("EditorExecutionAdvancedCostLimit", 0);
	public static bool EditorExecutionSuppressHeaders => (bool)GetSetting("EditorExecutionAdvancedSuppressHeaders", SharedConstants.C_DefaultSuppressHeaders);
	public static bool EditorExecutionDisconnectOnCompletion => (bool)GetSetting("EditorExecutionAdvancedDisconnectOnCompletion", false);


	// Editor ResultsSettingsModel
	public static EnSqlOutputMode EditorResultsOutputMode
		=> (EnSqlOutputMode)(int)GetSetting("EditorResultsGeneralOutputMode", EnSqlOutputMode.ToGrid);
	public static string EditorResultsDirectory
		=> (string)GetSetting("EditorResultsGeneralDirectory", Environment.GetFolderPath(Environment.SpecialFolder.Personal));
	public static bool EditorResultsPlaySounds => (bool)GetSetting("EditorResultsGeneralPlaySounds", false);

	// Grid results options.

	public static bool EditorResultsGridOutputQuery => (bool)GetSetting("EditorResultsGridOutputQuery", false);
	public static bool EditorResultsGridSingleTab => (bool)GetSetting("EditorResultsGridSingleTab", true);
	public static bool EditorResultsGridSaveIncludeHeaders => (bool)GetSetting("EditorResultsGridSaveIncludeHeaders", false);
	public static bool EditorResultsGridCsvQuoteStringsCommas => (bool)GetSetting("EditorResultsGridCsvQuoteStringsCommas", false);
	public static bool EditorResultsGridDiscardResults => (bool)GetSetting("EditorResultsGridDiscardResults", false);
	public static bool EditorResultsGridSeparateTabs => (bool)GetSetting("EditorResultsGridSeparateTabs", false);
	public static bool EditorResultsGridSwitchToResults => (bool)GetSetting("EditorResultsGridSwitchToResults", false);
	public static int EditorResultsGridMaxCharsPerColumnStd =>
		(int)GetSetting("EditorResultsGridMaxCharsPerColumnStd", SharedConstants.C_DefaultGridMaxCharsPerColumnStd);
	public static int EditorResultsGridMaxCharsPerColumnXml =>
		(int)GetSetting("EditorResultsGridMaxCharsPerColumnXml", SharedConstants.C_DefaultGridMaxCharsPerColumnXml);



	// Text results options.

	public static bool EditorResultsTextIncludeHeaders => (bool)GetSetting("EditorResultsTextIncludeHeaders", true);
	public static bool EditorResultsTextOutputQuery => (bool)GetSetting("EditorResultsTextOutputQuery", false);
	public static bool EditorResultsTextScrollingResults => (bool)GetSetting("EditorResultsTextScrollingResults", false);
	public static bool EditorResultsTextAlignRightNumerics => (bool)GetSetting("EditorResultsTextAlignRightNumerics", false);
	public static bool EditorResultsTextDiscardResults => (bool)GetSetting("EditorResultsTextDiscardResults", false);
	public static int EditorResultsTextMaxCharsPerColumnStd =>
		(int)GetSetting("EditorResultsTextMaxCharsPerColumnStd", SharedConstants.C_DefaultTextMaxCharsPerColumnStd);
	public static bool EditorResultsTextSeparateTabs => (bool)GetSetting("EditorResultsTextSeparateTabs", false);
	public static bool EditorResultsTextSwitchToResults => (bool)GetSetting("EditorResultsTextSwitchToResults", false);
	public static EnSqlOutputFormat EditorResultsTextOutputFormat
		=> (EnSqlOutputFormat)(int)GetSetting("EditorResultsTextOutputFormat", EnSqlOutputFormat.ColAligned);
	public static char EditorResultsTextDelimiter => GetTextDelimiter(EditorResultsTextOutputFormat,
		GetSetting("EditorResultsTextDelimiter", ","));



	// Tracking properties
	public static bool LayoutPropertyChanged
	{
		get
		{
			return _LayoutPropertyChanged;
		}
		set
		{
			_LayoutPropertyChanged = value;
		}
	}

	#endregion Property Accessors





	// =========================================================================================================
	#region Methods - PersistentSettings
	// =========================================================================================================


	public static char GetTextDelimiter(object format, object delimiter) =>
		GetTextDelimiter((EnSqlOutputFormat)format, (string)delimiter);


	public static char GetTextDelimiter(EnSqlOutputFormat format, string delimiter)
	{
		switch (format)
		{
			case EnSqlOutputFormat.ColAligned:
				return '\0';
			case EnSqlOutputFormat.CommaDelim:
				return ',';
			case EnSqlOutputFormat.TabDelim:
				return '\t';
			case EnSqlOutputFormat.SpaceDelim:
				return ' ';
			default:
				return delimiter[0];
		}
	}



	/// <summary>
	/// Adds the extension's SettingsSavedDelegate to a package settings models SettingsSavedEvents.
	/// Only implemented by packages that have settings models, ie. are options providers.
	/// </summary>
	// public override void RegisterSettingsEventHandlers(IBsPersistentSettings.SettingsSavedDelegate onSettingsSavedDelegate);


	/// <summary>
	/// Only implemented by packages that have settings models. Whenever a package
	/// settings model is saved it fires the extension's OnSettingsSaved event.
	/// That event handler then requests each package to populate SettingsEventArgs
	/// if it has settings relevant to the model.
	/// PopulateSettingsEventArgs is also called on loading by the extension without
	/// a specific model specified for a universal request for settings.
	/// </summary>
	// public override bool PopulateSettingsEventArgs(ref SettingsEventArgs args);


	#endregion Methods





	// =========================================================================================================
	#region Event handlers - PersistentSettings
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Settings saved event handler - only the final descendent class implements this.
	/// </summary>
	// ---------------------------------------------------------------------------------
	// public abstract void OnSettingsSaved(object sender);


	/// <summary>
	/// Updates settings used by a library. This method will be initiated by the
	/// extension package and passed down through the chain of dll's to the Core.
	/// A dll will update settings relevant to itself from here.
	/// IOW these are push notifications of any settings loaded or saved throughout the
	/// extension and an opportunity to update any live settings.
	/// </summary>
	public override void PropagateSettings(PropagateSettingsEventArgs e)
	{

		if (e.Package == null || e.Package == "Editor")
		{
			if (e.Group == null || e.Group == "TabAndStatusBar")
			{
				if (!_LayoutPropertyChanged && HasSetting("EditorStatusBarIncludeDatabaseName"))
					_LayoutPropertyChanged |= EditorStatusBarIncludeDatabaseName != (bool)e["EditorStatusBarIncludeDatabaseName"];
				if (!_LayoutPropertyChanged && HasSetting("EditorStatusBarIncludeLoginName"))
					_LayoutPropertyChanged |= EditorStatusBarIncludeLoginName != (bool)e["EditorStatusBarIncludeLoginName"];
				if (!_LayoutPropertyChanged && HasSetting("EditorStatusBarIncludeRowCount"))
					_LayoutPropertyChanged |= EditorStatusBarIncludeRowCount != (bool)e["EditorStatusBarIncludeRowCount"];
				if (!_LayoutPropertyChanged && HasSetting("EditorStatusBarIncludeServerName"))
					_LayoutPropertyChanged |= EditorStatusBarIncludeServerName != (bool)e["EditorStatusBarIncludeServerName"];
			}
		}

		base.PropagateSettings(e);

	}


	#endregion Event handlers


}
