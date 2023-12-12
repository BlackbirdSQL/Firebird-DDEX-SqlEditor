// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl.Events;
using BlackbirdSql.Core.Ctl.Extensions;

namespace BlackbirdSql.Controller.Ctl.Config;

// =========================================================================================================
//										PersistentSettings Class
//
/// <summary>
/// Consolidated single access point for daisy-chained packages settings models (IBSettingsModel).
/// As a rule we name descendent classes PersistentSettings as well. We hardcode bind the PersistentSettings descendent
/// tree from the top-level extension lib down to the Core. There is no point using services as this
/// configuration is fixed. ie:
/// VisualStudio.Ddex > Controller > [Intermediate Libs] > Core.
/// Current intermediate libs are: EditorExtension > Common.
/// </summary>
// =========================================================================================================
public abstract class PersistentSettings : EditorExtension.Ctl.Config.PersistentSettings
{

	// ---------------------------------------------------------------------------------
	#region Variables
	// ---------------------------------------------------------------------------------


	#endregion Variables




	// =========================================================================================================
	#region Property Accessors - PersistentSettings
	// =========================================================================================================


	// Ddex GeneralSettingsModel
	public static bool ValidateConfig => (bool)GetSetting("DdexValidateConfig", true);
	public static bool ValidateEdmx => (bool)GetSetting("DdexValidateEdmx", true);

	// Ddex DebugSettingsModel
#if DEBUG
	public static bool PersistentValidation => (bool)GetSetting("DdexDebugPersistentValidation", true);
#else
	public static bool PersistentValidation => (bool)GetSetting("DdexDebugPersistentValidation", false);
#endif


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


	#endregion Constructors / Destructors



	// =========================================================================================================
	#region Methods - PersistentSettings
	// =========================================================================================================



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
	/// extension.
	/// </summary>
	public override void PropagateSettings(PropagateSettingsEventArgs e)
	{
		base.PropagateSettings(e);
	}


	#endregion Event handlers


}