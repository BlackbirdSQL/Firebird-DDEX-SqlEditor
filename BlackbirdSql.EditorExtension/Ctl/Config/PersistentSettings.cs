// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;

using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl.Events;
using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.EditorExtension.Model.Config;
using BlackbirdSql.EditorExtension.Properties;


namespace BlackbirdSql.EditorExtension.Ctl.Config;

// =========================================================================================================
//										PersistentSettings Class
//
/// <summary>
/// Consolidated single access point for daisy-chained packages settings models (IBSettingsModel).
/// As a rule we name descendent classes PersistentSettings as well. We hardcode bind the PersistentSettings descendent
/// tree from the top-level extension lib down to the Core. There is no point using services as this
/// configuration is fixed. ie:
/// VisualStudio.Ddex > Controller > EditorExtension > Common > Core.
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


	// public static bool PromptToSaveKey => _PromptToSaveKey;


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
	public override void RegisterSettingsEventHandlers(IBPersistentSettings.SettingsSavedDelegate onSettingsSavedDelegate)
	{
		// There is no base. We're the first.
		// base.RegisterSettingsEventHandlers(onSettingsSavedDelegate);

		try
		{
			GeneralSettingsModel.SettingsSavedEvent += new Action<GeneralSettingsModel>(onSettingsSavedDelegate);
			ContextSettingsModel.SettingsSavedEvent += new Action<ContextSettingsModel>(onSettingsSavedDelegate);
			TabAndStatusBarSettingsModel.SettingsSavedEvent += new Action<TabAndStatusBarSettingsModel>(onSettingsSavedDelegate);
			ExecutionSettingsModel.SettingsSavedEvent += new Action<ExecutionSettingsModel>(onSettingsSavedDelegate);
			ExecutionAdvancedSettingsModel.SettingsSavedEvent += new Action<ExecutionAdvancedSettingsModel>(onSettingsSavedDelegate);
			ResultsSettingsModel.SettingsSavedEvent += new Action<ResultsSettingsModel>(onSettingsSavedDelegate);
			ResultsGridSettingsModel.SettingsSavedEvent += new Action<ResultsGridSettingsModel>(onSettingsSavedDelegate);
			ResultsTextSettingsModel.SettingsSavedEvent += new Action<ResultsTextSettingsModel>(onSettingsSavedDelegate);
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

		// There is no base. We're the first.
		// if (args.Package == null || args.Package != "Editor")
		//	result = base.PopulateSettingsEventArgs(ref args);


		if (e.Package == null || e.Package == "Editor")
		{
			if (e.Group == null || e.Group == "General")
			{
				e.AddRange(GeneralSettingsModel.Instance);
				result = true;
			}

			if (e.Group == null || e.Group == "Context")
			{
				e.AddRange(ContextSettingsModel.Instance);
				result = true;
			}

			if (e.Group == null || e.Group == "TabAndStatusBar")
			{
				e.AddRange(TabAndStatusBarSettingsModel.Instance);
				result = true;
			}

			if (e.Group == null || e.Group == "Execution")
			{
				e.AddRange(ExecutionSettingsModel.Instance);
				result = true;
			}

			if (e.Group == null || e.Group == "ExecutionAdvanced")
			{
				e.AddRange(ExecutionAdvancedSettingsModel.Instance);
				result = true;
			}

			if (e.Group == null || e.Group == "Results")
			{
				e.AddRange(ResultsSettingsModel.Instance);
				result = true;
			}

			if (e.Group == null || e.Group == "ResultsGrid")
			{
				e.AddRange(ResultsGridSettingsModel.Instance);
				result = true;
			}

			if (e.Group == null || e.Group == "ResultsText")
			{
				e.AddRange(ResultsTextSettingsModel.Instance);
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
	}


	#endregion Event handlers


}