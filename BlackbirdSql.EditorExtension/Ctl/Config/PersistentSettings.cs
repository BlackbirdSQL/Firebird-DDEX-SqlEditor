﻿// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using BlackbirdSql.EditorExtension.Model.Config;
using BlackbirdSql.EditorExtension.Properties;
using BlackbirdSql.Sys.Interfaces;
using BlackbirdSql.Sys.Events;
using BlackbirdSql.Shared.Enums;


namespace BlackbirdSql.EditorExtension.Ctl.Config;

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
public abstract class PersistentSettings : LanguageExtension.Ctl.Config.PersistentSettings
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


	private int _AssemblyId = -1;


	#endregion Fields





	// =========================================================================================================
	#region Property Accessors - PersistentSettings
	// =========================================================================================================


	public static string MandatedLanguageServiceGuid
	{
		get
		{
			switch (EditorLanguageService)
			{
				case EnLanguageService.USql:
					return VS.USqlLanguageServiceGuid;
				case EnLanguageService.TSql90:
					return VS.TSql90LanguageServiceGuid;
				case EnLanguageService.FbSql:
					return LanguageExtension.PackageData.C_LanguageServiceGuid;
				default:
					return VS.SSDTLanguageServiceGuid;
			}
		}

	}


	#endregion Property Accessors





	// =========================================================================================================
	#region Methods - PersistentSettings
	// =========================================================================================================


	public override int GetEvsAssemblyId(Type type)
	{
		int id = base.GetEvsAssemblyId(type);

		if (id > 0)
			return id;

		--id;

		if (type.Assembly.FullName == typeof(PersistentSettings).Assembly.FullName)
		{
			_AssemblyId = -id;
			return _AssemblyId;
		}

		return id;
	}



	/// <summary>
	/// Adds the extension's SettingsSavedDelegate to a package settings models SettingsSavedEvents.
	/// Only implemented by packages that have settings models, ie. are options providers.
	/// </summary>
	public override void RegisterSettingsEventHandlers(IBsSettingsProvider.SettingsSavedDelegate onSettingsSavedDelegate)
	{
		base.RegisterSettingsEventHandlers(onSettingsSavedDelegate);

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
			Diag.Ex(ex, Resources.ExFailedToSubscribeSettingsEvents);
		}
	}


	/// <summary>
	/// Only implemented by packages that have settings models. Whenever a package
	/// settings model is saved it fires the extension's OnSettingsSaved event.
	/// That event handler then requests each package to populate SettingsEventArgs
	/// if it has settings relevant to the model.
	/// PopulateSettingsEventArgs is also called on loading by the extension without
	/// a specific model specified for a universal request for settings. That effectively
	/// instantiates all option page models.
	/// </summary>
	public override bool PopulateSettingsEventArgs(ref PropagateSettingsEventArgs e)
	{
		bool result = false;

		if (e.Package == null || e.Package != "Editor")
			result |= base.PopulateSettingsEventArgs(ref e);


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