//
// Plagiarized from Community.VisualStudio.Toolkit extension
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Serialization;
using BlackbirdSql.Core.Controls.Events;
using BlackbirdSql.Core.Ctl.CommandProviders;
using BlackbirdSql.Core.Ctl.ComponentModel;
using BlackbirdSql.Core.Ctl.Config;
using BlackbirdSql.Core.Ctl.Enums;
using BlackbirdSql.Core.Ctl.Events;
using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.Core.Model.Interfaces;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.Utilities;

using Tracer = BlackbirdSql.Core.Ctl.Diagnostics.Tracer;


namespace BlackbirdSql.Core.Model.Config;

public abstract class AbstractSettingsModel<T> : IBSettingsModel where T : AbstractSettingsModel<T>
{
	private VerbSite _Site = null;
	private readonly IBTransientSettings _LiveSettings = null;

	private readonly string _SettingsPackage;
	private readonly string _SettingsGroup;
	private readonly string _LivePrefix;

	private static readonly AsyncLazy<T> _LiveModel = new AsyncLazy<T>(new Func<Task<T>>(CreateAsync), ThreadHelper.JoinableTaskFactory);

	private static AsyncLazy<ShellSettingsManager> _SettingsManager = null;

	private static List<IBSettingsModelPropertyWrapper> _PropertyWrappers = null;

	// A static class lock
	private static readonly object _LockGlobal = new object();


	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public PropertyWrapper this[string propertyName]
	{
		get
		{
			return (PropertyWrapper)PropertyWrappers.Find((obj)
				=> obj.PropertyName.Equals(propertyName, StringComparison.OrdinalIgnoreCase));
		}
	}


	public static T Instance => ThreadHelper.JoinableTaskFactory.Run(new Func<Task<T>>(GetLiveInstanceAsync));


	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public string LivePrefix => _LivePrefix;

	// ** Item of interest ** Return the site object that supports DesignerVerbs
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public ISite Site
	{
		// return our "site" which connects back to us to expose our tagged methods
		get
		{
			if (_Site == null)
			{
				_Site = new(this);
				_Site.AutomationVerbExecutedEvent += OnVerbExecuted;
			}
			return _Site;
		}
		set { throw new NotImplementedException(); }
	}


	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public virtual string CollectionName { get; } = typeof(T).FullName;


	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public List<IBSettingsModelPropertyWrapper> PropertyWrappers
	{
		get
		{
			lock (_LockGlobal)
			{
				if (_PropertyWrappers != null)
					return _PropertyWrappers;


				List<IBSettingsModelPropertyWrapper> list = [];
				_PropertyWrappers = list;

				foreach (PropertyInfo property in GetOptionProperties())
				{
					try
					{
						list.Add(new PropertyWrapper(property));
					}
					catch (Exception ex)
					{
						Diag.Dug(ex, string.Format("AbstractSettingsModel<{0}>.{1} Property:{2} PropertyType:{3} is not a valid property.", typeof(T).FullName, "GetPropertyWrappersEnumeration", property.Name, property.PropertyType));
					}
				}

				list.Sort(PropertyWrapper.OrderComparer.Default);

			}

			return _PropertyWrappers;

		}
	}

	protected virtual IEnumerable<IBSettingsModelPropertyWrapper> PropertyWrappersEnumeration => PropertyWrappers.AsReadOnly();


	private static AsyncLazy<ShellSettingsManager> SettingsManager =>
		_SettingsManager ??=
			new AsyncLazy<ShellSettingsManager>(new Func<Task<ShellSettingsManager>>(GetSettingsManagerAsync),
			ThreadHelper.JoinableTaskFactory);


	public static event Action<T> SettingsSavedEvent;

	public event AutomationVerbEventHandler SettingsResetEvent;
	// public event IBSettingsModel.SelectedItemChangedEventHandler SelectedItemChangedEvent;
	public event IBSettingsModel.EditControlFocusEventHandler EditControlGotFocusEvent;
	public event IBSettingsModel.EditControlFocusEventHandler EditControlLostFocusEvent;
	public event IBSettingsModel.AutomationPropertyValueChangedEventHandler AutomationPropertyValueChangedEvent;

	public event EventHandler BeforeLoadEvent;
	public event EventHandler Disposed;



	protected AbstractSettingsModel(string package, string group, string livePrefix)
	{
		_SettingsPackage = package;
		_SettingsGroup = group;
		_LivePrefix = livePrefix;
	}

	protected AbstractSettingsModel(string package, string group, string livePrefix, IBTransientSettings transientSettings)
		: this(package, group, livePrefix)
	{
		_LiveSettings = transientSettings;
	}

	public void Dispose()
	{
		// never called in this specific context with the PropertyGrid
		// but just reference the required Disposed event to avoid warnings
		Disposed?.Invoke(this, EventArgs.Empty);
	}


	//
	// Summary:
	//     Get the singleton instance of the options. Thread safe.
	public static Task<T> GetLiveInstanceAsync()
	{
		return _LiveModel.GetValueAsync();
	}

	//
	// Summary:
	//     Creates a new instance of the options class and loads the values from the store.
	//     For internal use only
	public static async Task<T> CreateAsync()
	{
		T instance = (T)Activator.CreateInstance(typeof(T));
		await instance.LoadAsync();
		return instance;
	}

	//
	// Summary:
	//     Creates a new instance of the options class and loads the values from the store.
	//     For internal use only
	public static async Task<T> CreateAsync(IBTransientSettings transientSettings)
	{
		object[] args = [transientSettings];
		T instance = (T)Activator.CreateInstance(typeof(T), args);
		await instance.LoadAsync();
		return instance;
	}


	public virtual object GetDefaultValue(string propertyName)
	{
		lock (_LockGlobal)
		{
			PropertyWrapper wrapper = this[propertyName];

			if (wrapper == null)
				return null;

			return wrapper.DefaultValue;
		}
	}

	public virtual void Load()
	{
		ThreadHelper.JoinableTaskFactory.Run(new Func<Task>(LoadAsync));
	}

	/*
	public void OnSelectedItemChanged(object sender, SelectedGridItemChangedEventArgs e)
	{
		SelectedItemChangedEvent?.Invoke(sender, e);
	}
	*/



	public void OnEditControlGotFocus(object sender, EditControlFocusEventArgs e)
	{
		EditControlGotFocusEvent?.Invoke(sender, e);
	}



	public void OnEditControlLostFocus(object sender, EditControlFocusEventArgs e)
	{
		EditControlLostFocusEvent?.Invoke(sender, e);
	}

	public void OnAutomationPropertyValueChanged(object sender, AutomationPropertyValueChangedEventArgs e)
	{
		AutomationPropertyValueChangedEvent?.Invoke(sender, e);
	}


	public virtual async Task LoadAsync()
	{
		BeforeLoadEvent?.Invoke(this, EventArgs.Empty);

		ShellSettingsManager obj = await SettingsManager.GetValueAsync();
		SettingsScope scope = SettingsScope.UserSettings;
		SettingsStore readOnlySettingsStore = obj.GetReadOnlySettingsStore(scope);

		foreach (IBSettingsModelPropertyWrapper propertyWrapper in PropertyWrappersEnumeration)
		{
			if (_LiveSettings != null)
				propertyWrapper.Load(this, _LiveSettings);
			else
				propertyWrapper.Load(this, readOnlySettingsStore);
		}
	}

	public virtual void LoadDefaults()
	{
		BeforeLoadEvent?.Invoke(this, EventArgs.Empty);

		lock (_LockGlobal)
		{
			foreach (IBSettingsModelPropertyWrapper propertyWrapper in PropertyWrappersEnumeration)
			{
				propertyWrapper.LoadDefault(this);
			}
		}
	}


	public virtual void Save()
	{
		ThreadHelper.JoinableTaskFactory.Run(new Func<Task>(SaveAsync));
	}

	public virtual async Task SaveAsync()
	{
		ShellSettingsManager obj;
		SettingsScope scope;
		WritableSettingsStore writableSettingsStore = null;

		if (_LiveSettings == null)
		{
			obj = await SettingsManager.GetValueAsync();
			scope = SettingsScope.UserSettings;
			writableSettingsStore = obj.GetWritableSettingsStore(scope);
		}
		foreach (IBSettingsModelPropertyWrapper propertyWrapper in PropertyWrappersEnumeration)
		{
			if (_LiveSettings != null)
				propertyWrapper.Save(this, _LiveSettings);
			else
				propertyWrapper.Save(this, writableSettingsStore);
		}

		T liveModel = await GetLiveInstanceAsync();
		if (this != liveModel)
		{
			await liveModel.LoadAsync();
		}

		SettingsSavedEvent?.Invoke(liveModel);
	}


	protected internal virtual string SerializeValue(object value, Type type, string propertyName)
	{
		if (value == null)
		{
			return string.Empty;
		}

		XmlSerializer val = new XmlSerializer(value!.GetType());
		using StringWriter stringWriter = new StringWriter();
		val.Serialize(stringWriter, value);
		return stringWriter.ToString();
	}


	protected internal virtual object DeserializeValue(string serializedData, Type type, string propertyName)
	{
		if (serializedData.Length == 0)
		{
			if (type.IsValueType)
			{
				return Activator.CreateInstance(type);
			}

			return null;
		}

		return new XmlSerializer(type).Deserialize(new StringReader(serializedData));
	}


	private static async Task<ShellSettingsManager> GetSettingsManagerAsync()
	{
		await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
		return new ShellSettingsManager(ServiceProvider.GlobalProvider);
	}

	//
	// Summary:
	//     Returns an enumerable of System.Reflection.PropertyInfo for the properties of
	//     T that will be loaded and saved. Base implementation utilizes reflection.
	protected virtual IEnumerable<PropertyInfo> GetOptionProperties()
	{
		Attribute attribute;
		DesignerSerializationVisibilityAttribute visibility;

		PropertyInfo[] properties = GetType().GetProperties();
		List<PropertyInfo> settings = new(properties.Length);

		foreach (PropertyInfo property in properties)
		{
			// Tracer.Trace($"Validating property for model {typeof(T).Name} property: {property.Name}.");

			if ((!property.PropertyType.IsPublic && !property.PropertyType.IsNestedPublic)
				|| !property.CanWrite || !property.CanRead)
			{
				// Tracer.Trace($"Ignored property for model {typeof(T).Name} property {property.Name} - IsPublic: {property.PropertyType.IsPublic} CanWrite: {property.CanWrite} CanRead: {property.CanRead}.");
				continue;
			}

			attribute = property.GetCustomAttribute(typeof(DesignerSerializationVisibilityAttribute));

			if (attribute != null)
			{
				visibility = attribute as DesignerSerializationVisibilityAttribute;

				if (visibility.Visibility == DesignerSerializationVisibility.Hidden)
				{
					// Tracer.Trace($"IGNORED property for model {typeof(T).Name} property {property.Name} because Hidden.");
					continue;
				}
			}

			settings.Add(property);

			// Tracer.Trace($"ADDED property for model {typeof(T).Name} property: {property.Name}.");
		}

		return settings;
	}

	public virtual string GetPackage()
	{
		return _SettingsPackage;
	}

	public virtual string GetGroup()
	{
		return _SettingsGroup;
	}



	[GlobalizedVerbText("GlobalizedDesignerVerbReset")]
	[CommandId(CommandProperties.CommandSetGuid, (int)EnCommandSet.CmdIdResetPageOptions)]
	public void VerbMethodReset()
	{
		SettingsResetEvent?.Invoke(this, new EventArgs());
	}


	private void OnVerbExecuted(object sender, EventArgs e)
	{
		CommandIdAttribute cmdAttr;
		object[] cmdAttrs;

		// The verb is the sender
		DesignerVerb verb = sender as DesignerVerb;
		// Enumerate the methods again to find the one named by the verb
		MethodInfo[] mia = GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance);

		foreach (MethodInfo mi in mia)
		{
			cmdAttrs = mi.GetCustomAttributes(typeof(CommandIdAttribute), true);
			if (cmdAttrs == null || cmdAttrs.Length == 0)
				continue;

			cmdAttr = (CommandIdAttribute)cmdAttrs[0];

			if (verb.CommandID.Guid.ToString().Equals(cmdAttr.CommandSetGuid, StringComparison.OrdinalIgnoreCase)
				&& verb.CommandID.ID == cmdAttr.CommandId)
			{
				// Invoke the method on our object (no parameters)
				mi.Invoke(this, null);
				return;
			}
		}
	}

}
