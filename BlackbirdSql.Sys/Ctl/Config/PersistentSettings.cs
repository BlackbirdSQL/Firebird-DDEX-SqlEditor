// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using BlackbirdSql.Sys.Properties;
using BlackbirdSql.Sys.Events;
using BlackbirdSql.Sys.Extensions;
using BlackbirdSql.Sys.Interfaces;
using BlackbirdSql.Sys.Enums;

namespace BlackbirdSql.Sys.Ctl.Config;


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
public abstract class PersistentSettings : IBsSettingsProvider
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - PersistentSettings
	// ---------------------------------------------------------------------------------


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Protected singleton .ctor
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected PersistentSettings()
	{
		Diag.ThrowIfInstanceExists(_Instance);

		_Instance = this;
	}

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Private live .ctor
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected PersistentSettings(bool transient)
	{
		_TransientStore = new(SettingsStore);
	}


	protected PersistentSettings(IDictionary<string, object> transientStore)
	{
		_TransientStore = new(transientStore);
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the singleton PersistentSettings instance
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static IBsSettingsProvider Instance
	{
		get
		{
			if (_Instance == null)
			{
				NullReferenceException ex = new(Resources.ExceptionCreateAbstractClass.Fmt(nameof(PersistentSettings)));
				Diag.Ex(ex);
				throw ex;
			}

			return _Instance;
		}
	}


	protected virtual void Initialize()
	{
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - PersistentSettings
	// =========================================================================================================

	protected static readonly object _LockGlobal = new object();
	protected readonly object _LockObject = new object();

	private static string[] _EquivalencyKeys = null;
	protected static IBsSettingsProvider _Instance = null;
	protected static Dictionary<string, object> _SettingsStore = null;
	protected Dictionary<string, object> _TransientStore = null;

	#endregion Fields





	// =========================================================================================================
	#region Property Accessors - PersistentSettings
	// =========================================================================================================

	public virtual object this[string name]
	{
		get
		{
			object value = null;
			Dictionary<string, object> store = _TransientStore ?? _SettingsStore;

			store?.TryGetValue(name, out value);

			return value;
		}

		set
		{
			Dictionary<string, object> store = _TransientStore ?? _SettingsStore;
			store[name] = value;
		}
	}


	public static Dictionary<string, object> SettingsStore
	{
		get
		{
			lock (_LockGlobal)
			{
				if (_SettingsStore == null)
					((PersistentSettings)Instance).Initialize();

				return _SettingsStore;
			}
		}
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Flag indicating whether or not <see cref="Diag"/> messages are logged
	/// </summary>
	/// <remarks>
	/// Exceptions are always logged.
	/// </remarks>
	// ---------------------------------------------------------------------------------
	public static bool EnableDiagnostics => (bool)GetPersistentSetting("DdexGeneralEnableDiagnostics", true);


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns a boolean indicating whether or not task results can be written to the
	/// output window.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static bool EnableTaskLog => (bool)GetPersistentSetting("DdexGeneralEnableTaskLog", true);


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Flag indicating whether or not EventSource ActivityTracing calls are logged.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static bool EnableActivityLogging => (bool)GetPersistentSetting("DdexDebugEnableActivityLogging", false);

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Enables Stop, Start, Suspend, Transfer and Resume event tracing.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static bool EnableActivityTracing => (bool)GetPersistentSetting("DdexDebugEnableActivityTracing", false);


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Flag indicating whether or not EventSource calls are logged
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static bool EnableEventSourceLogging => (bool)GetPersistentSetting("DdexDebugEnableEventSourceLogging", false);

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Flag indicating whether or not Evs Trace calls are logged
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static bool EnableTrace => (bool)GetPersistentSetting("DdexDebugEnableTrace", false)
		|| EnableActivityLogging || EnableEventSourceLogging;



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Flag indicating whether or not diagnostics calls are logged to the log file.
	/// </summary>
	/// <remarks>
	/// Only applies to the Debug configuration. Debug Exceptions are always logged.
	/// </remarks>
	// ---------------------------------------------------------------------------------
	public static bool EnableDiagnosticsLog => (bool)GetPersistentSetting("DdexDebugEnableDiagnosticsLog", false);


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Flag indicating whether or not <see cref="Diag.Debug"/> exceptions are output.
	/// Debug exceptions are always displayed in debug builds.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static bool EnableDebugExceptions => (bool)GetPersistentSetting("DdexDebugEnableDebugExceptions", false);


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Flag indicating whether or not <see cref="Diag.Expected"/> exceptions are output.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static bool EnableExpected => (bool)GetPersistentSetting("DdexDebugEnableExpected", false);


	public static string[] EquivalencyKeys
	{
		get
		{
			if (_EquivalencyKeys != null)
				return _EquivalencyKeys;

			List<string> equivalencyKeys = [];

			foreach (KeyValuePair<string, object> pair in SettingsStore)
			{
				if (pair.Key.StartsWith("DdexEquivalency") && (bool)pair.Value)
					equivalencyKeys.Add(pair.Key[15..]);
			}

			if (equivalencyKeys.Count == 0)
			{
				ApplicationException ex = new(Resources.ExceptionExtractEquivalencyKeys);
				Diag.Ex(ex);
				throw (ex);
			}

			_EquivalencyKeys = [.. equivalencyKeys];

			return _EquivalencyKeys;

		}
	}


	/// <summary>
	/// If enabled, disables asynchronous Trigger/Generator linkage and linkage will
	/// only performed when it is actually required.
	/// </summary>
	public static bool OnDemandLinkage => (bool)GetPersistentSetting("DdexGeneralOnDemandLinkage", false);

	/// <summary>
	/// The maximum time a trigger linkage parser will wait before requesting an extension.
	/// </summary>
	public static int LinkageTimeout => (int)GetPersistentSetting("DdexGeneralLinkageTimeout", 30);


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The log file path
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static string LogFile => (string)GetPersistentSetting("DdexDebugLogFile", "/temp/vsdiag.log");


	#endregion Property Accessors





	// =========================================================================================================
	#region Methods - PersistentSettings
	// =========================================================================================================


	protected static object GetPersistentSetting(string name, object defaultValue)
	{
		try
		{
			if (!SettingsStore.TryGetValue(name, out object value))
				value = defaultValue;

			return value;
		}
		catch (Exception ex)
		{
			Diag.Ex(ex);
			throw ex;
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Specifies the level of trace messages filtered by the source switch and event
	/// type filter.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static EnSourceLevels GetEvsLevel(string identifier) =>
		(EnSourceLevels)(int)GetPersistentSetting($"DdexDebugEvsLevel{identifier}", EnSourceLevels.Off)
			| (EnableActivityTracing ? EnSourceLevels.ActivityTracing : EnSourceLevels.Off);



	protected static bool HasSetting(string name)
	{
		return _SettingsStore != null && _SettingsStore.ContainsKey(name);
	}



	/// <summary>
	/// Adds the extension's SettingsSavedDelegate to a package settings models SettingsSavedEvents.
	/// Only implemented by packages that have settings models, ie. are options providers.
	/// </summary>
	public virtual void RegisterSettingsEventHandlers(IBsSettingsProvider.SettingsSavedDelegate onSettingsSavedDelegate)
	{
	}


	/// <summary>
	/// Only implemented by packages that have settings models. Whenever a package
	/// settings model is saved it fires the extension's OnSettingsSaved event.
	/// That event handler then requests each package to populate SettingsEventArgs
	/// if it has settings relevant to the model.
	/// PopulateSettingsEventArgs is also called on loading by the extension without
	/// a specific model specified for a universal request for settings.
	/// </summary>
	public virtual bool PopulateSettingsEventArgs(ref PropagateSettingsEventArgs args)
	{
		return false;
	}



	public bool PropertyExists(string name)
	{
		lock (_LockObject)
		{
			if (_TransientStore != null)
				return _TransientStore.ContainsKey(name);

			return _SettingsStore.ContainsKey(name);
		}
	}


	#endregion Methods




	// =========================================================================================================
	#region Event handlers - PersistentSettings
	// =========================================================================================================


	/// <summary>
	/// Updates settings used by a library. This method will be initiated by the
	/// extension package and passed down through the chain of dll's to the Core.
	/// A dll will update settings relevant to itself from here.
	/// IOW these are push notifications of any settings loaded or saved throughout the
	/// extension and an opportunity to update any live settings.
	/// </summary>
	public virtual void PropagateSettings(PropagateSettingsEventArgs e)
	{
		foreach (MutablePair<string, object> pair in e.Arguments)
			this[pair.Key] = pair.Value;
	}


	#endregion Event handlers

}