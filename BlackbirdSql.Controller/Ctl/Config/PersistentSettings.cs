// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using BlackbirdSql.Sys.Events;

namespace BlackbirdSql.Controller.Ctl.Config;

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
public abstract class PersistentSettings : EditorExtension.Ctl.Config.PersistentSettings
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - PersistentSettings
	// ---------------------------------------------------------------------------------


	/// <summary>
	/// Private singleton .ctor
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


	/// <summary>
	/// If enabled, closes edmx data models that have been left open when a solution closes.
	/// </summary>
	public static bool AutoCloseOffScreenEdmx => (bool)GetPersistentSetting("DdexGeneralAutoCloseOffScreenEdmx", true);


	/// <summary>
	/// If enabled, closes xsd datasets that have been left open when a solution closes.
	/// </summary>
	public static bool AutoCloseXsdDatasets => (bool)GetPersistentSetting("DdexGeneralAutoCloseXsdDatasets", false);


	#endregion Property Accessors





	// =========================================================================================================
	#region Methods - PersistentSettings
	// =========================================================================================================


	/*
	/// <summary>
	/// Adds the extension's SettingsSavedDelegate to a package settings models SettingsSavedEvents.
	/// Only implemented by packages that have settings models, ie. are options providers.
	/// </summary>
	public override void RegisterSettingsEventHandlers(IBsSettingsProvider.SettingsSavedDelegate onSettingsSavedDelegate)


	/// <summary>
	/// Only implemented by packages that have settings models. Whenever a package
	/// settings model is saved it fires the extension's OnSettingsSaved event.
	/// That event handler then requests each package to populate SettingsEventArgs
	/// if it has settings relevant to the model.
	/// PopulateSettingsEventArgs is also called on loading by the extension without
	/// a specific model specified for a universal request for settings.
	/// </summary>
	public override bool PopulateSettingsEventArgs(ref PropagateSettingsEventArgs e)
	*/


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
	}


	#endregion Event handlers


}