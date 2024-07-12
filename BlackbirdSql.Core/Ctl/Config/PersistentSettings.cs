// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Collections.Generic;
using BlackbirdSql.Core.Properties;
using BlackbirdSql.Sys.Events;
using BlackbirdSql.Sys.Extensions;
using BlackbirdSql.Sys.Interfaces;

namespace BlackbirdSql.Core.Ctl.Config;


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
public abstract class PersistentSettings : IBPersistentSettings
{

	// ---------------------------------------------------------------------------------
	#region Fields
	// ---------------------------------------------------------------------------------

	protected static IBPersistentSettings _Instance = null;
	protected static Dictionary<string, object> _SettingsStore;

	#endregion Fields





	// =========================================================================================================
	#region Property Accessors - PersistentSettings
	// =========================================================================================================

	public virtual object this[string name]
	{
		get
		{
			object value = null;

			_SettingsStore?.TryGetValue(name, out value);

			return value;
		}

		set
		{
			SettingsStore[name] = value;
		}
	}


	protected static Dictionary<string, object> SettingsStore => _SettingsStore ??= [];


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Flag indicating whether or not <see cref="Diag.Dug"/> calls are logged
	/// </summary>
	/// <remarks>
	/// Exceptions are always logged.
	/// </remarks>
	// ---------------------------------------------------------------------------------
	public static bool EnableDiagnostics => (bool)GetSetting("DdexGeneralEnableDiagnostics", true);


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns a boolean indicating whether or not task results can be written to the
	/// output window.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static bool EnableTaskLog => (bool)GetSetting("DdexGeneralEnableTaskLog", true);


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Flag indicating whether or not Tracer calls are logged
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static bool EnableTracer => (bool)GetSetting("DdexDebugEnableTracer", false);

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Flag indicating whether or not <see cref="Diag.Trace"/> calls are logged
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static bool EnableTrace => (bool)GetSetting("DdexDebugEnableTrace", false) | EnableTracer;



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Flag indicating whether or not diagnostics calls are logged to the log file.
	/// </summary>
	/// <remarks>
	/// Only applies to the Debug configuration. Debug Exceptions are always logged.
	/// </remarks>
	// ---------------------------------------------------------------------------------
	public static bool EnableDiagnosticsLog => (bool)GetSetting("DdexDebugEnableDiagnosticsLog", false);


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Flag indicating whether or not the extrapolated TObjectSupport.xml, after
	/// imports, must be saved to the diagnostics logfile directory.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static bool EnableSaveExtrapolatedXml => (bool)GetSetting("DdexDebugEnableSaveExtrapolatedXml", false);


	/// <summary>
	/// If enabled, closes edmx data models that have been left open when a solution closes.
	/// </summary>
	public static bool AutoCloseOffScreenEdmx => (bool)GetSetting("DdexGeneralAutoCloseOffScreenEdmx", true);

	/// <summary>
	/// If enabled, closes xsd datasets that have been left open when a solution closes.
	/// </summary>
	public static bool AutoCloseXsdDatasets => (bool)GetSetting("DdexGeneralAutoCloseXsdDatasets", false);


	/// <summary>
	/// Determines if configured connections in a solution's projects are included in selection lists when adding
	/// a new connnection.
	/// </summary>
	public static bool IncludeAppConnections => (bool)GetSetting("DdexGeneralIncludeAppConnections", true);

	/// <summary>
	/// If enabled, disables asynchronous Trigger/Generator linkage and linkage will
	/// only performed when it is actually required.
	/// </summary>
	public static bool OnDemandLinkage => (bool)GetSetting("DdexGeneralOnDemandLinkage", false);

	/// <summary>
	/// The maximum time a trigger linkage parser will wait before requesting an extension.
	/// </summary>
	public static int LinkageTimeout => (int)GetSetting("DdexGeneralLinkageTimeout", 30);


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The log file path
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static string LogFile => (string)GetSetting("DdexDebugLogFile", "/temp/vsdiag.log");


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Sets the query designer diagram pane to visible when a table or view's data
	/// is initially retrieved.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static bool ShowDiagramPane => (bool)GetSetting("DdexGeneralShowDiagramPane", true);


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// If enabled performs a recovery of late loading provider factories.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static bool ValidateProviderFactories => (bool)GetSetting("DdexGeneralValidateProviderFactories", false);


	#endregion Property Accessors





	// =========================================================================================================
	#region Constructors / Destructors - PersistentSettings
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Private singleton .ctor
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected PersistentSettings()
	{
		if (_Instance != null)
		{
			TypeAccessException ex = new(Resources.ExceptionDuplicateSingletonInstances.FmtRes(GetType().FullName));
			Diag.Dug(ex);
			throw ex;
		}


		_Instance = this;
	}

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Private live .ctor
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected PersistentSettings(bool live)
	{
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the singleton PersistentSettings instance
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static IBPersistentSettings Instance
	{
		get
		{
			if (_Instance == null)
			{
				NullReferenceException ex = new("Cannot instantiate PersistentSettings from abstract ancestor");
				Diag.Dug(ex);
				throw ex;
			}

			return _Instance;
		}
	}


	#endregion Constructors / Destructors



	// =========================================================================================================
	#region Methods - PersistentSettings
	// =========================================================================================================


	protected static object GetSetting(string name, object defaultValue)
	{
		try
		{
			if (_SettingsStore == null || !_SettingsStore.TryGetValue(name, out object value))
				value = defaultValue;

			return value;
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}
	}



	protected static bool HasSetting(string name)
	{
		return _SettingsStore != null && _SettingsStore.ContainsKey(name);
	}



	/// <summary>
	/// Adds the extension's SettingsSavedDelegate to a package settings models SettingsSavedEvents.
	/// Only implemented by packages that have settings models, ie. are options providers.
	/// </summary>
	public abstract void RegisterSettingsEventHandlers(IBPersistentSettings.SettingsSavedDelegate onSettingsSavedDelegate);


	/// <summary>
	/// Only implemented by packages that have settings models. Whenever a package
	/// settings model is saved it fires the extension's OnSettingsSaved event.
	/// That event handler then requests each package to populate SettingsEventArgs
	/// if it has settings relevant to the model.
	/// PopulateSettingsEventArgs is also called on loading by the extension without
	/// a specific model specified for a universal request for settings.
	/// </summary>
	public abstract bool PopulateSettingsEventArgs(ref PropagateSettingsEventArgs args);


	#endregion Methods




	// =========================================================================================================
	#region Event handlers - PersistentSettings
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Settings saved event handler - only the final descendent class implements this.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public abstract void OnSettingsSaved(object sender);



	/// <summary>
	/// Updates settings used by a library. This method will be initiated by the
	/// extension package and passed down through the chain of dll's to the Core.
	/// A dll will update settings relevant to itself from here.
	/// IOW these are push notifications of any settings loaded or saved throughout the
	/// extension and an opportunity to update any live settings.
	/// </summary>
	public virtual void PropagateSettings(PropagateSettingsEventArgs e)
	{
		PropagateEquivalencyKeys(e);
		PropagateDiagnosticsSettings(e);
	}


	private void PropagateEquivalencyKeys(PropagateSettingsEventArgs e)
	{
		if ((e.Package == null || e.Package == "Ddex") && (e.Group == null || e.Group == "Equivalency"))
		{
			List<string> equivalencyKeys = [];

			foreach (MutablePair<string, object> pair in e.Arguments)
			{
				this[pair.Key] = pair.Value;
				if (pair.Key.StartsWith("DdexEquivalency") && (bool)pair.Value)
					equivalencyKeys.Add(pair.Key[15..]);
			}

			if (equivalencyKeys.Count > 0)
				NativeDb.EquivalencyKeys = [.. equivalencyKeys];

		}
	}


	private void PropagateDiagnosticsSettings(PropagateSettingsEventArgs e)
	{
		if ((e.Package == null || e.Package == "Ddex") && (e.Group == null || e.Group == "General"))
		{
			Diag.SettingEnableTaskLog = EnableTaskLog;
			Diag.SettingEnableDiagnostics = EnableDiagnostics;

			NativeDb.OnDemandLinkage = OnDemandLinkage;
			NativeDb.LinkageTimeout = LinkageTimeout;
		}

		if ((e.Package == null || e.Package == "Ddex") && (e.Group == null || e.Group == "Debug"))
		{
			Diag.SettingEnableTrace = EnableTrace;
			Diag.SettingEnableTracer = EnableTracer;
			Diag.SettingEnableDiagnosticsLog = EnableDiagnosticsLog;
			Diag.SettingLogFile = LogFile;
		}
	}

	#endregion Event handlers


}