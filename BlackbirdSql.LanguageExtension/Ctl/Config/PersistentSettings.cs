// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using BlackbirdSql.LanguageExtension.Enums;
using BlackbirdSql.LanguageExtension.Properties;
using BlackbirdSql.Sys.Events;
using BlackbirdSql.Sys.Interfaces;



namespace BlackbirdSql.LanguageExtension.Ctl.Config;


// =============================================================================================================
//										PersistentSettings Class
//
/// <summary>
/// Consolidated single access point for daisy-chained packages settings models (IBsSettingsModel).
/// As a rule we name descendent classes PersistentSettings as well. We hardcode bind the PersistentSettings
/// descendent tree from the top-level extension lib down to Sys.
/// PersistentSettings can be either consumers or providers of options, or both.
/// Property accessors should only be declared at the hierarchy level that they are first required. They do
/// not need to be declared in PeristentSettings if they're only required in TransientSettings.
/// There is no point using services as this hierarchy is fixed. ie:
/// VisualStudio.Ddex > Controller > EditorExtension > LanguageExtension > Shared > Core > Sys.
/// </summary>
// =============================================================================================================
public abstract class PersistentSettings : Shared.Ctl.Config.PersistentSettings
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - PersistentSettings
	// ---------------------------------------------------------------------------------


	/// <summary>
	/// Protected singleton .ctor
	/// </summary>
	protected PersistentSettings() : base()
	{
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - PersistentSettings
	// =========================================================================================================



	#endregion Fields





	// =========================================================================================================
	#region Property Accessors - PersistentSettings
	// =========================================================================================================


	// LanguageService AdvancedPreferencesModel
	public static bool LanguageServiceEnableIntellisense => (bool)GetPersistentSetting("LanguageServiceAdvancedEnableIntellisense", true);
	public static bool LanguageServiceAutoOutlining => LanguageServiceEnableIntellisense
		&& (bool)GetPersistentSetting("LanguageServiceAdvancedAutoOutlining", true);
	public static bool LanguageServiceUnderlineErrors => LanguageServiceEnableIntellisense
		&& (bool)GetPersistentSetting("LanguageServiceAdvancedUnderlineErrors", true);
	public static int LanguageServiceMaxScriptSize => (int)GetPersistentSetting("LanguageServiceAdvancedMaxScriptSize", 1048576);
	public static EnCasingStyle LanguageServiceTextCasing => (EnCasingStyle)GetPersistentSetting("LanguageServiceAdvancedTextCasing", EnCasingStyle.Uppercase);
	// _DisplayInfoProvider.BuiltInCasing = Prefs.TextCasing == 0 ? CasingStyle.Uppercase : CasingStyle.Lowercase;



	// Editor ExecutionSettingsModel

	public static string EditorExecutionBatchSeparator
	{
		get
		{
			string value = (string)GetPersistentSetting("EditorExecutionGeneralBatchSeparator", SharedConstants.C_DefaultBatchSeparator);

			value = value.Replace(" ", "");

			if (value == "")
				value = ";";

			return value;
		}
	}



	#endregion Property Accessors





	// =========================================================================================================
	#region Methods - PersistentSettings
	// =========================================================================================================


	/// <summary>
	/// Adds the extension's SettingsSavedDelegate to a package settings models SettingsSavedEvents.
	/// Only implemented by packages that have settings models, ie. are options providers.
	/// </summary>
	public override void RegisterSettingsEventHandlers(IBsSettingsProvider.SettingsSavedDelegate onSettingsSavedDelegate)
	{
		base.RegisterSettingsEventHandlers(onSettingsSavedDelegate);

		try
		{
			AdvancedPreferencesModel.SettingsSavedEvent += new Action<AdvancedPreferencesModel>(onSettingsSavedDelegate);
		}
		catch (Exception ex)
		{
			Diag.Ex(ex, Resources.ExFailedToSubscribeSettingsEvents);
		}
	}


	/// <summary>
	/// Only implemented by packages that have settings models. Whenever a package
	/// settings model is saved it fires the extension's OnSettingsSaved event.
	/// That event handler then requests each package to populate SettingsEventArgs
	/// if it has settings relevant to the model.
	/// PopulateSettingsEventArgs is also called on loading by the extension without
	/// a specific model specified for a universal request for settings.
	/// </summary>
	public override bool PopulateSettingsEventArgs(ref PropagateSettingsEventArgs e)
	{
		bool result = false;

		if (e.Package == null || e.Package != "LanguageService")
			result |= base.PopulateSettingsEventArgs(ref e);


		if (e.Package == null || e.Package == "LanguageService")
		{
			if (e.Group == null || e.Group == "Advanced")
			{
				e.AddRange(AdvancedPreferencesModel.Instance);
				result = true;
			}
		}

		return result;
	}


	#endregion Methods




	// =========================================================================================================
	#region Event handlers - PersistentSettings
	// =========================================================================================================


	/*
	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Settings saved event handler - only the final descendent class implements this.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public abstract void OnSettingsSaved(object sender);
	*/


	/// <summary>
	/// Updates settings used by a library. This method will be initiated by the
	/// extension package and passed down through the chain of dll's to the Core.
	/// A dll will update settings relevant to itself from here.
	/// IOW these are push notifications of any settings loaded or saved throughout the
	/// extension and an opportunity to update any live settings.
	/// </summary>
	public override void PropagateSettings(PropagateSettingsEventArgs e)
	{
		base.PropagateSettings(e);

		if (e.Package == null || e.Package == "LanguageService")
		{
			if (e.Group == null || e.Group == "Advanced")
			{
				LanguageExtensionPackage.Instance.UpdateUserPreferences();
			}
		}
	}


	#endregion Event handlers


}