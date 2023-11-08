// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;

using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl.Events;
using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.Core.Model.Interfaces;
using BlackbirdSql.VisualStudio.Ddex.Model.Config;
using BlackbirdSql.VisualStudio.Ddex.Properties;


namespace BlackbirdSql.VisualStudio.Ddex.Ctl.Config;

// =========================================================================================================
//										UserSettings Class
//
/// <summary>
/// Consolidated single access point for daisy-chained packages settings models (IBSettingsModel).
/// As a rule we name descendent classes UserSettings as well. We hardcode bind the UserSettings descendent
/// tree from the top-level extension lib (this lib) down to the Core. There is no point using services as
/// this configuration is fixed. ie:
/// VisualStudio.Ddex > Controller > [Intermediate Libs] > Core.
/// Current intermediate libs are: EditorExtension > Common.
/// </summary>
// =========================================================================================================
public class UserSettings : Controller.Ctl.Config.UserSettings
{


	// =========================================================================================================
	#region Property Accessors - UserSettings
	// =========================================================================================================


	public static bool ShowDiagramPane => (bool)GetSetting("DdexGeneralShowDiagramPane", true);


	#endregion Property Accessors





	// =========================================================================================================
	#region Constructors / Destructors - UserSettings
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Private singleton .ctor
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected UserSettings() : base()
	{
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the singleton UserSettings instance
	/// </summary>
	// ---------------------------------------------------------------------------------
	public new static IBUserSettings Instance => _Instance ??= new UserSettings();


	#endregion Constructors / Destructors



	// =========================================================================================================
	#region Methods - UserSettings
	// =========================================================================================================



	/// <summary>
	/// Adds the extension's SettingsSavedDelegate to a package settings models SettingsSavedEvents.
	/// Only implemented by packages that have settings models.
	/// </summary>
	public override void RegisterSettingsEventHandlers(IBUserSettings.SettingsSavedDelegate onSettingsSavedDelegate)
	{
		base.RegisterSettingsEventHandlers(onSettingsSavedDelegate);

		try
		{
			GeneralSettingsModel.SettingsSavedEvent += new Action<GeneralSettingsModel>(onSettingsSavedDelegate);
			DebugSettingsModel.SettingsSavedEvent += new Action<DebugSettingsModel>(onSettingsSavedDelegate);
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

		if (e.Package == null || e.Package != "Ddex")
			result = base.PopulateSettingsEventArgs(ref e);


		if (e.Package == null || e.Package == "Ddex")
		{
			if (e.Group == null || e.Group == "General")
			{
				e.AddRange(GeneralSettingsModel.Instance);
				result = true;
			}

			if (e.Group == null || e.Group == "Debug")
			{
				e.AddRange(DebugSettingsModel.Instance);
				result = true;
			}
		}

		return result;
	}


	#endregion Methods




	// =========================================================================================================
	#region Event handlers - UserSettings
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Settings saved event handler - only the final descendent class implements this.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override void OnSettingsSaved(object sender)
	{
		IBSettingsModel model = (IBSettingsModel)sender;

		PropagateSettingsEventArgs e = new(model.GetPackage(), model.GetGroup());

		PopulateSettingsEventArgs(ref e);
		PropagateSettings(e);
	}


	/// <summary>
	/// Updates settings used by a library. This method will be initiated by the
	/// extension package and passed down through the chain of dll's to the Core.
	/// A dll will update settings relevant to itself from here.
	/// IOW these are push notifications of any settings loaded or saved throughout the
	/// extension.
	/// </summary>
	public override void PropagateSettings(PropagateSettingsEventArgs e)
	{
		try
		{
			base.PropagateSettings(e);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
		}
	}


	#endregion Event handlers


}