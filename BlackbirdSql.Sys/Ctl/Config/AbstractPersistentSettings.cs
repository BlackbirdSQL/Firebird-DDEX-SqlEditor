// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BlackbirdSql.Sys.Events;
using BlackbirdSql.Sys.Extensions;
using BlackbirdSql.Sys.Interfaces;
using BlackbirdSql.Sys.Properties;
using Microsoft.VisualStudio.Shell;



namespace BlackbirdSql.Sys.Ctl.Config;


// =========================================================================================================
//										AbstractPersistentSettings Class
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
public abstract class AbstractPersistentSettings : IBsSettingsProvider
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - AbstractPersistentSettings
	// ---------------------------------------------------------------------------------


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Protected singleton .ctor
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected AbstractPersistentSettings()
	{
		Diag.ThrowIfInstanceExists(_Instance);

		_Instance = this;
	}

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Private live .ctor
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected AbstractPersistentSettings(bool transient)
	{
		_TransientStore = new(SettingsStore);
	}


	protected AbstractPersistentSettings(IDictionary<string, object> transientStore)
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



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// [Launch ensure UI thread]: Initializes the _SettingsStore on the UI thread.
	/// Pushes through any request already on the UI thread ahead of any waiting for
	/// the main thread.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private bool InitializeEui()
	{
		// Fire and wait.

		if (!ThreadHelper.CheckAccess())
		{
			bool result = Task.Run(async delegate
			{
				await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

				if (_SettingsStore != null)
					return false;

				((PersistentSettings)Instance).Initialize();

				return true;

			}).AwaiterResult();

			return result;
		}

		((PersistentSettings)Instance).Initialize();

		return true;
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - PersistentSettings
	// =========================================================================================================

	protected static readonly object _LockGlobal = new object();
	protected readonly object _LockObject = new object();

	protected static string[] _EquivalencyKeys = null;
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
			if (_SettingsStore == null)
				((PersistentSettings)Instance).InitializeEui();

			return _SettingsStore;
		}
	}




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



	protected static bool HasSetting(string name)
	{
		lock (_LockGlobal)
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
	public abstract bool PopulateSettingsEventArgs(ref PropagateSettingsEventArgs e);



	public bool PropertyExists(string name)
	{
		if (_TransientStore != null)
			return _TransientStore.ContainsKey(name);

		return _SettingsStore.ContainsKey(name);
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