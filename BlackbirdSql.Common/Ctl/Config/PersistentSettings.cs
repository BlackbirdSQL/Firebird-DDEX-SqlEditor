// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Resources;
using BlackbirdSql.Common.Ctl.Enums;
using BlackbirdSql.Common.Properties;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl.Enums;
using BlackbirdSql.Core.Ctl.Events;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Core.Model.Enums;


namespace BlackbirdSql.Common.Ctl.Config;

// =========================================================================================================
//										PersistentSettings Class
//
/// <summary>
/// Consolidated single access point for daisy-chained packages settings models (IBSettingsModel).
/// As a convention we name descendent classes PersistentSettings as well. We hardcode bind the PersistentSettings
/// descendent tree from the top-level extension lib down to the Core. There is no point using services as
/// this configuration is fixed. ie:
/// VisualStudio.Ddex > Controller > EditorExtension > Common > Core.
/// </summary>
// =========================================================================================================
public abstract class PersistentSettings : Core.Ctl.Config.PersistentSettings
{

	// ---------------------------------------------------------------------------------
	#region Sub-classes - PersistentSettings
	// ---------------------------------------------------------------------------------


	public class CommandObject(ResourceManager resMgr)
	{
		public ResourceManager ResMgr { get; } = resMgr;
		public string Name { get; set; }
	}


	#endregion Sub-classes




	// =========================================================================================================
	#region Fields - PersistentSettings
	// =========================================================================================================


	// Tracking variables
	protected static object _LockGlobal = new object();
	private static bool _LayoutPropertyChanged = false;
	private static CommandObject _CmdObject = null;


	#endregion Fields





	// =========================================================================================================
	#region Property Accessors - PersistentSettings
	// =========================================================================================================


	public static CommandObject CmdObject => _CmdObject ??= new(SqlResources.ResourceManager);

	// Editor GeneralSettingsModel

	public static bool EditorEnableIntellisense => (bool)GetSetting("EditorGeneralEnableIntellisense", true);
	public static bool EditorExecuteQueryOnOpen => (bool)GetSetting("EditorGeneralExecuteQueryOnOpen", true);
	public static bool EditorPromptToSave => (bool)GetSetting("EditorGeneralPromptToSave", false);



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
	public static string EditorContextBatchSeparator =>
		(string)GetSetting("EditorContextBatchSeparator", ModelConstants.C_DefaultBatchSeparator);


	// Editor ExecutionSettingsModel

	public static int EditorExecutionSetRowCount => (int)GetSetting("EditorExecutionGeneralSetRowCount", ModelConstants.C_DefaultSetRowCount);
	public static EnBlobSubType EditorExecutionSetBlobDisplay => (EnBlobSubType)GetSetting("EditorExecutionGeneralSetBlobDisplay", ModelConstants.C_DefaultSetBlobDisplay);
	public static bool EditorExecutionDefaultOleScripting => (bool)GetSetting("EditorExecutionGeneralDefaultOleScripting",
		ModelConstants.C_DefaultDefaultOleScripting);
	public static int EditorExecutionTimeout => (int)GetSetting("EditorExecutionGeneralTimeout", ModelConstants.C_DefaultCommandTimeout);

	// Editor ExecutionAdvancedSettingsModel
	public static bool EditorExecutionSetCount => (bool)GetSetting("EditorExecutionAdvancedSetCount", ModelConstants.C_DefaultSetCount);
	public static bool EditorExecutionSetNoExec => (bool)GetSetting("EditorExecutionAdvancedSetNoExec", ModelConstants.C_DefaultSetPlanOnly);
	public static bool EditorExecutionSetShowplanText => (bool)GetSetting("EditorExecutionAdvancedSetShowplanText", ModelConstants.C_DefaultSetPlan);
	public static bool EditorExecutionSetPlanXml => (bool)GetSetting("EditorExecutionAdvancedSetPlanXml", ModelConstants.C_DefaultSetExplain);
	public static bool EditorExecutionSetParseOnly =>
		(bool)GetSetting("EditorExecutionAdvancedSetParseOnly", ModelConstants.C_DefaultSetParseOnly);
	public static bool EditorExecutionSetConcatenationNull =>
		(bool)GetSetting("EditorExecutionAdvancedSetConcatenationNull", ModelConstants.C_DefaultSetConcatenationNull);
	public static bool EditorExecutionSetBail => (bool)GetSetting("EditorExecutionAdvancedSetBail", ModelConstants.C_DefaultSetBail);
	public static bool EditorExecutionSetPlanText => (bool)GetSetting("EditorExecutionAdvancedSetPlanText", ModelConstants.C_DefaultSetPlanText);
	public static bool EditorExecutionSetStats => (bool)GetSetting("EditorExecutionAdvancedSetStats", ModelConstants.C_DefaultSetStats);
	public static bool EditorExecutionSetStatisticsIO =>
		(bool)GetSetting("EditorExecutionAdvancedSetStatisticsIO", ModelConstants.C_DefaultSetStatisticsIO);
	public static bool EditorExecutionSetWarnings => (bool)GetSetting("EditorExecutionAdvancedSetWarnings", ModelConstants.C_DefaultSetWarnings);
	public static IsolationLevel EditorExecutionIsolationLevel
		=> (IsolationLevel)(int)GetSetting("EditorExecutionAdvancedIsolationLevel", ModelConstants.C_DefaultIsolationLevel);
	public static EnDeadlockPriority EditorExecutionDeadlockPriority
		=> (EnDeadlockPriority)(int)GetSetting("EditorExecutionAdvancedIsolationLevel", EnDeadlockPriority.Low);
	public static bool EditorExecutionDeadlockPriorityLow => EditorExecutionDeadlockPriority == EnDeadlockPriority.Low;
	public static int EditorExecutionLockTimeout => (int)GetSetting("EditorExecutionAdvancedLockTimeout", ModelConstants.C_DefaultLockTimeout);
	public static int EditorExecutionCostLimit => (int)GetSetting("EditorExecutionAdvancedCostLimit", 0);
	public static bool EditorExecutionSuppressHeaders => (bool)GetSetting("EditorExecutionAdvancedSuppressHeaders", ModelConstants.C_DefaultSuppressHeaders);
	public static bool EditorExecutionDisconnectOnCompletion => (bool)GetSetting("EditorExecutionAdvancedDisconnectOnCompletion", false);


	// Editor ResultsSettingsModel
	public static EnSqlOutputMode EditorResultsOutputMode
		=> (EnSqlOutputMode)(int)GetSetting("EditorResultsGeneralIsolationLevel", EnSqlOutputMode.ToGrid);
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
		(int)GetSetting("EditorResultsGridMaxCharsPerColumnStd", ModelConstants.C_DefaultGridMaxCharsPerColumnStd);
	public static int EditorResultsGridMaxCharsPerColumnXml =>
		(int)GetSetting("EditorResultsGridMaxCharsPerColumnXml", ModelConstants.C_DefaultGridMaxCharsPerColumnXml);



	// Text results options.

	public static bool EditorResultsTextIncludeHeaders => (bool)GetSetting("EditorResultsTextIncludeHeaders", true);
	public static bool EditorResultsTextOutputQuery => (bool)GetSetting("EditorResultsTextOutputQuery", false);
	public static bool EditorResultsTextScrollingResults => (bool)GetSetting("EditorResultsTextScrollingResults", false);
	public static bool EditorResultsTextAlignRightNumerics => (bool)GetSetting("EditorResultsTextAlignRightNumerics", false);
	public static bool EditorResultsTextDiscardResults => (bool)GetSetting("EditorResultsTextDiscardResults", false);
	public static int EditorResultsTextMaxCharsPerColumnStd =>
		(int)GetSetting("EditorResultsTextMaxCharsPerColumnStd", ModelConstants.C_DefaultTextMaxCharsPerColumnStd);
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
	#region Constructors / Destructors - PersistentSettings
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Private singleton .ctor
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
	/// Only implemented by packages that have settings models.
	/// </summary>
	// public override void RegisterSettingsEventHandlers(IBPersistentSettings.SettingsSavedDelegate onSettingsSavedDelegate);


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



public static class PersistentSettingsExtensions
{
	/// <summary>
	/// Gets the SQL command string for a property that exists within the PersistentSettings
	/// hierarchy. The SQL statement is accessed from ResMgr using the property's name
	/// as stored in LiveStore; which is the PropertyName prefixed with the LivePrefix
	/// constant of the settings model. The default for ResMgr is
	/// Properties\SqlResources.resx but this resource can be overriden.
	/// </summary>
	/// <param name="property">
	/// The PersistentSettings instance property accessed directly by name or through the
	/// PersistentSettings instance index accessor. This overload extension expects that
	/// the 'property' value passed is the result of a direct accessor get. Any
	/// unsupported commands must either have string.Empty in the resource file or
	/// nothing at all.
	/// </param>
	/// <param name="value">
	/// An optional value to use in place of the 'property' value for building the sql
	/// command/statement for 'property'.
	/// </param>
	/// <returns>
	/// The complete SQL statement for property otherwise string.Empty if the statement
	/// format string does not exist in ResMgr. 
	/// </returns>
	public static string SqlCmd(this object property, object value = null)
	{

		string name = PersistentSettings.CmdObject.Name;
		Type type = property.GetType();

		if (name == null)
		{
			Diag.Dug(true, $"Could not retrieve name of object type {type.Name}.");
			return string.Empty;
		}

		string cmd = PersistentSettings.CmdObject.ResMgr.GetString(name);

		if (cmd == null)
			return string.Empty;

		value ??= property;


		string result;

		if (type.IsEnum)
			result = cmd.FmtSqlEnum((System.Enum)value);
		else if (type == typeof(bool))
			result = cmd.FmtSqlOnOff((bool)value);
		else
			result = cmd.FmtRes(value);

		return result;
	}


	/// <summary>
	/// Formats an sql command using the string value of the enum
	/// if it's int.MinValue or int.MaxValue.
	/// </summary>
	public static string FmtSqlEnum(this string value, Enum arg0)
	{
		try
		{
			int num = Convert.ToInt32(arg0);

			if (num == int.MinValue || num == int.MaxValue)
				return string.Format(CultureInfo.InvariantCulture, value, arg0.ToString());

			return string.Format(CultureInfo.InvariantCulture, value, num);
		}
		catch { }

		return string.Format(CultureInfo.InvariantCulture, value, arg0);
	}


	/// <summary>
	/// Formats an sql command using the boolean value as an On/Off
	/// value.
	/// </summary>
	/// <param name="value"></param>
	/// <param name="arg0"></param>
	/// <returns></returns>
	public static string FmtSqlOnOff(this string value, bool arg0)
	{
		return string.Format(CultureInfo.InvariantCulture, value, arg0 ? "On" : "Off");
	}


}
