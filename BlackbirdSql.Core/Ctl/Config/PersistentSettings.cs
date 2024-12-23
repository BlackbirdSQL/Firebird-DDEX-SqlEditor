﻿// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using BlackbirdSql.Core.Properties;
using BlackbirdSql.Sys.Events;
using BlackbirdSql.Sys.Extensions;
using BlackbirdSql.Sys.Interfaces;

namespace BlackbirdSql.Core.Ctl.Config;


// =========================================================================================================
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
// =========================================================================================================
public abstract class PersistentSettings : Sys.Ctl.Config.PersistentSettings
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - PersistentSettings
	// ---------------------------------------------------------------------------------


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Protected singleton .ctor
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected PersistentSettings() : base()
	{
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Private live .ctor
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected PersistentSettings(bool transient) : base(transient)
	{
	}

	protected PersistentSettings(IDictionary<string, object> transientStore) : base(transientStore)
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


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns a boolean indicating whether or not the extension's load statistics can
	/// be written to the output window.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static bool EnableLoadStatistics => (bool)GetPersistentSetting("DdexGeneralEnableLoadStatistics", false);


	/// <summary>
	/// Determines if configured connections in a solution's projects are included in selection lists when adding
	/// a new connnection.
	/// </summary>
	public static bool IncludeAppConnections => (bool)GetPersistentSetting("DdexGeneralIncludeAppConnections", true);


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Sets the query designer diagram pane to visible when a table or view's data
	/// is initially retrieved.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static bool ShowDiagramPane => (bool)GetPersistentSetting("DdexGeneralShowDiagramPane", true);


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// If enabled performs a recovery of late loading provider factories.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static bool ValidateProviderFactories => (bool)GetPersistentSetting("DdexGeneralValidateProviderFactories", false);


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



	/*
		/// <summary>
		/// Adds the extension's SettingsSavedDelegate to a package settings models SettingsSavedEvents.
		/// Only implemented by packages that have settings models, ie. are options providers.
		/// </summary>
		public abstract void RegisterSettingsEventHandlers(IBsPersistentSettings.SettingsSavedDelegate onSettingsSavedDelegate);


		/// <summary>
		/// Only implemented by packages that have settings models. Whenever a package
		/// settings model is saved it fires the extension's OnSettingsSaved event.
		/// That event handler then requests each package to populate SettingsEventArgs
		/// if it has settings relevant to the model.
		/// PopulateSettingsEventArgs is also called on loading by the extension without
		/// a specific model specified for a universal request for settings.
		/// </summary>
		public abstract bool PopulateSettingsEventArgs(ref PropagateSettingsEventArgs args);
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