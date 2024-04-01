// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl.Events;
using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.LanguageExtension.Properties;


namespace BlackbirdSql.LanguageExtension.Ctl.Config;

// =========================================================================================================
//										PersistentSettings Class
//
/// <summary>
/// Consolidated single access point for daisy-chained packages settings models (IBSettingsModel).
/// As a rule we name descendent classes PersistentSettings as well. We hardcode bind the PersistentSettings
/// descendent tree from the top-level extension lib down to the Core.
/// PersistentSettings can be either consumers or providers of options, or both.
/// There is no point using services as this configuration is fixed. ie:
/// VisualStudio.Ddex > Controller > EditorExtension > LanguageExtension > Common > Core.
/// </summary>
// =========================================================================================================
public abstract class PersistentSettings : Common.Ctl.Config.PersistentSettings
{

	// ---------------------------------------------------------------------------------
	#region Fields
	// ---------------------------------------------------------------------------------

	// Editor GeneralSettingsModel
	// static bool _PromptToSaveKey = false;


	#endregion Fields





	// =========================================================================================================
	#region Property Accessors - PersistentSettings
	// =========================================================================================================



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
	/// Only implemented by packages that have settings models, ie. are options providers.
	/// </summary>
	public override void RegisterSettingsEventHandlers(IBPersistentSettings.SettingsSavedDelegate onSettingsSavedDelegate)
	{
		// There is no base. We're the first.
		// base.RegisterSettingsEventHandlers(onSettingsSavedDelegate);

		try
		{
			AdvancedPreferencesModel.SettingsSavedEvent += new Action<AdvancedPreferencesModel>(onSettingsSavedDelegate);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex, Resources.ExceptionFailedToSubscribeSettingsEvents);
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

		// There is no base. We're the first ancestor that creates new settings.
		// if (e.Package == null || e.Package != "LanguageService")
		//	result = base.PopulateSettingsEventArgs(ref e);


		if (e.Package == null || e.Package == "LanguageService")
		{
			if (e.Group == null || e.Group == "Advanced")
			{
				// TODO: Temp removal while testing.
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


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Settings saved event handler - only the final descendent class implements this.
	/// </summary>
	// ---------------------------------------------------------------------------------
	// public override void OnSettingsSaved(object sender);


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