//
// Plagiarized from Community.VisualStudio.Toolkit extension
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Serialization;
using BlackbirdSql.Core.Ctl.CommandProviders;
using BlackbirdSql.Core.Ctl.ComponentModel;
using BlackbirdSql.Core.Ctl.Config;
using BlackbirdSql.Core.Enums;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Sys.Events;
using BlackbirdSql.Sys.Interfaces;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.Utilities;



namespace BlackbirdSql.Core.Model.Config;


// =============================================================================================================
//										AbstractSettingsModel Class
//
/// <summary>
/// Base class template for the user options model. If created with transientSettings then uses a volatile
/// copy of the model that can be used in code and modified programatically or by using 
/// </summary>
// =============================================================================================================
public abstract class AbstractSettingsModel<TModel> : IBsSettingsModel where TModel : AbstractSettingsModel<TModel>
{

	// -------------------------------------------------------
	#region Constructors / Destructors - AbstractEventsManager
	// -------------------------------------------------------


	/// <summary>
	/// Persistent settings .ctor used by <see cref="CreateInstanceAsync()"/>.
	/// </summary>
	protected AbstractSettingsModel(string package, string group, string propertyPrefix)
	{
		_SettingsPackage = package;
		_SettingsGroup = group;
		_PropertyPrefix = propertyPrefix;
	}


	/// <summary>
	/// Transient settings .ctor used by <see cref="CreateInstanceAsync(IBsSettingsProvider)"/>.
	/// </summary>
	protected AbstractSettingsModel(string package, string group, string propertyPrefix, IBsSettingsProvider transientSettings)
		: this(package, group, propertyPrefix)
	{
		_TransientSettings = transientSettings;
	}


	/// <summary>
	/// The singleton instance of this options model.
	/// </summary>
	public static TModel Instance =>
		ThreadHelper.JoinableTaskFactory.Run(new Func<Task<TModel>>(LazyInstance.GetValueAsync));


	public static AsyncLazy<TModel> LazyInstance => _LazyInstance ??=
		new AsyncLazy<TModel>(new Func<Task<TModel>>(CreateInstanceAsync), ThreadHelper.JoinableTaskFactory);


	/// <summary>
	/// <see cref="IDisposable"/> implementation.
	/// </summary>
	public void Dispose()
	{
		// never called in this specific context with the PropertyGrid
		// but just reference the required Disposed event to avoid warnings
		Disposed?.Invoke(this, EventArgs.Empty);
	}



	/// <summary>
	/// Creates a new instance of the options class and loads the values from the store.
	/// This is for persistent settings and save operations save to the store.
	/// </summary>
	public static async Task<TModel> CreateInstanceAsync()
	{
		TModel instance = (TModel)Activator.CreateInstance(typeof(TModel));

		await instance.LoadAsync();

		return instance;
	}


	/// <summary>
	/// Creates a new instance of the options class and loads the values from the store.
	/// This is for transient settings. Save operations save to memory.
	/// </summary>
	public static async Task<TModel> CreateInstanceAsync(IBsSettingsProvider transientSettings)
	{
		object[] args = [transientSettings];

		TModel instance = (TModel)Activator.CreateInstance(typeof(TModel), args);

		await instance.LoadAsync();

		return instance;
	}


	#endregion Constructors / Destructors




	// =========================================================================================================
	#region Fields - AbstractSettingsModel
	// =========================================================================================================

	// A static class lock
	private static readonly object _LockGlobal = new object();

	private VerbSite _Site = null;
	private readonly string _SettingsPackage;
	private readonly string _SettingsGroup;
	private readonly string _PropertyPrefix;

	private readonly IBsSettingsProvider _TransientSettings = null;


	private static AsyncLazy<TModel> _LazyInstance = null;
	private static AsyncLazy<ShellSettingsManager> _SettingsManager = null;
	private static List<IBsModelPropertyWrapper> _PropertyWrappers = null;


	#endregion Fields





	// =========================================================================================================
	#region Property accessors - AbstractSettingsModel
	// =========================================================================================================


	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public IBsModelPropertyWrapper this[string propertyName]
	{
		get
		{
			return (PropertyWrapper)PropertyWrappers.Find((obj)
				=> obj.PropertyName.Equals(propertyName, StringComparison.OrdinalIgnoreCase));
		}
	}



	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public string PropertyPrefix => _PropertyPrefix;


	// ** Item of interest ** Return the site object that supports DesignerVerbs
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	ISite IComponent.Site
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
	public virtual string CollectionName { get; } = typeof(TModel).FullName;


	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public List<IBsModelPropertyWrapper> PropertyWrappers
	{
		get
		{
			lock (_LockGlobal)
			{
				if (_PropertyWrappers != null)
					return _PropertyWrappers;


				List<IBsModelPropertyWrapper> list = [];
				_PropertyWrappers = list;

				foreach (PropertyInfo property in GetOptionProperties())
				{
					try
					{
						list.Add(new PropertyWrapper(property));
					}
					catch (Exception ex)
					{
						Diag.Ex(ex, "AbstractSettingsModel<{0}>.{1} Property:{2} PropertyType:{3} is not a valid property.".Fmt(typeof(TModel).FullName, "GetPropertyWrappersEnumeration", property.Name, property.PropertyType));
					}
				}

				list.Sort(PropertyWrapper.OrderComparer.Default);

			}

			return _PropertyWrappers;

		}
	}



	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public IEnumerable<IBsModelPropertyWrapper> PropertyWrappersEnumeration => PropertyWrappers.AsReadOnly();


	private static AsyncLazy<ShellSettingsManager> SettingsManager =>
		_SettingsManager ??=
				new AsyncLazy<ShellSettingsManager>(new Func<Task<ShellSettingsManager>>(GetSettingsManagerAsync),
				ThreadHelper.JoinableTaskFactory);


	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public string SettingsGroup => _SettingsGroup;

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public string SettingsPackage => _SettingsPackage;



	public event IBsSettingsModel.AutomatorPropertyValueChangedEventHandler AutomatorPropertyValueChangedEvent;

	public event EventHandler BeforeLoadEvent;

	public event EventHandler Disposed;

	public event IBsSettingsModel.EditControlFocusEventHandler EditControlGotFocusEvent;

	public event IBsSettingsModel.EditControlFocusEventHandler EditControlLostFocusEvent;

	public event AutomationVerbEventHandler SettingsResetEvent;

	public static event Action<TModel> SettingsSavedEvent;



	#endregion Property accessors




	// =========================================================================================================
	#region Methods - AbstractSettingsModel
	// =========================================================================================================



	public object GetDefaultValue(string propertyName)
	{
		lock (_LockGlobal)
		{
			PropertyWrapper wrapper = (PropertyWrapper)this[propertyName];

			if (wrapper == null)
				return null;

			return wrapper.DefaultValue;
		}
	}



	public void Load()
	{
		ThreadHelper.JoinableTaskFactory.Run(new Func<Task>(LoadAsync));
	}



	private async Task LoadAsync()
	{
		BeforeLoadEvent?.Invoke(this, EventArgs.Empty);

		SettingsStore readOnlySettingsStore = null;

		if (_TransientSettings == null)
		{
			ShellSettingsManager manager = await SettingsManager.GetValueAsync();
			SettingsScope scope = SettingsScope.UserSettings;
			readOnlySettingsStore = manager.GetReadOnlySettingsStore(scope);
		}


		foreach (IBsModelPropertyWrapper propertyWrapper in PropertyWrappersEnumeration)
		{
			if (readOnlySettingsStore == null)
				((IBSettingsModelPropertyWrapper)propertyWrapper).Load(this, _TransientSettings);
			else
				((IBSettingsModelPropertyWrapper)propertyWrapper).Load(this, readOnlySettingsStore);
		}
	}



	public void LoadDefaults()
	{
		BeforeLoadEvent?.Invoke(this, EventArgs.Empty);

		lock (_LockGlobal)
		{
			foreach (IBsModelPropertyWrapper propertyWrapper in PropertyWrappersEnumeration)
			{
				((IBSettingsModelPropertyWrapper)propertyWrapper).LoadDefault(this);
			}
		}
	}



	public void Save()
	{
		ThreadHelper.JoinableTaskFactory.Run(new Func<Task>(SaveAsync));
	}



	private async Task SaveAsync()
	{
		WritableSettingsStore writableSettingsStore = null;

		if (_TransientSettings == null)
		{
			ShellSettingsManager manager = await SettingsManager.GetValueAsync();
			SettingsScope scope = SettingsScope.UserSettings;
			writableSettingsStore = manager.GetWritableSettingsStore(scope);
		}

		foreach (IBsModelPropertyWrapper propertyWrapper in PropertyWrappersEnumeration)
		{
			if (writableSettingsStore == null)
				((IBSettingsModelPropertyWrapper)propertyWrapper).Save(this, _TransientSettings);
			else
				((IBSettingsModelPropertyWrapper)propertyWrapper).Save(this, writableSettingsStore);
		}

		TModel liveModel = await LazyInstance.GetValueAsync();
		
		if (this != liveModel)
			await liveModel.LoadAsync();

		SettingsSavedEvent?.Invoke(liveModel);
	}



	public virtual string SerializeValue(object value, Type type, string propertyName)
	{
		if (value == null)
		{
			return "";
		}

		XmlSerializer val = new XmlSerializer(value!.GetType());
		using StringWriter stringWriter = new StringWriter();
		val.Serialize(stringWriter, value);
		return stringWriter.ToString();
	}



	public virtual object DeserializeValue(string serializedData, Type type, string propertyName)
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

		return new(ServiceProvider.GlobalProvider);
	}
	



	/// <summary>
	/// Returns an enumerable of System.Reflection.PropertyInfo for the properties of
	/// TModel that will be loaded and saved. Base implementation utilizes reflection.
	/// </summary>
	private IEnumerable<PropertyInfo> GetOptionProperties()
	{
		Attribute attribute;
		DesignerSerializationVisibilityAttribute visibility;

		PropertyInfo[] properties = GetType().GetProperties();
		List<PropertyInfo> settings = new(properties.Length);

		foreach (PropertyInfo property in properties)
		{
			// Evs.Debug(GetType(), "GetOptionProperties()",
			//	$"Validating property for model {typeof(TModel).Name} property: {property.Name}.");

			if ((!property.PropertyType.IsPublic && !property.PropertyType.IsNestedPublic)
				|| !property.CanWrite || !property.CanRead)
			{
				// Evs.Debug(GetType(), "GetOptionProperties()",
				//	$"Ignored property for model {typeof(TModel).Name} property {property.Name} - " +
				//	$"IsPublic: {property.PropertyType.IsPublic} CanWrite: {property.CanWrite} " +
				//	$"CanRead: {property.CanRead}.");

				continue;
			}

			attribute = property.GetCustomAttribute(typeof(DesignerSerializationVisibilityAttribute));

			if (attribute != null)
			{
				visibility = attribute as DesignerSerializationVisibilityAttribute;

				if (visibility.Visibility == DesignerSerializationVisibility.Hidden)
				{
					// Evs.Debug(GetType(), "GetOptionProperties()",
					//	$"IGNORED property for model {typeof(TModel).Name} " +
					//	$"property {property.Name} because Hidden.");

					continue;
				}
			}

			settings.Add(property);

			// Evs.Debug(GetType(), "GetOptionProperties()",
			//	$"ADDED property for model {typeof(TModel).Name} property: {property.Name}.");
		}

		return settings;
	}



	[GlobalizedVerbText("GlobalizedDesignerVerbReset")]
	[CommandId(CommandProperties.CommandSetGuid, (int)EnCommandSet.CmdIdResetPageOptions)]
	public void VerbMethodReset()
	{
		SettingsResetEvent?.Invoke(this, new EventArgs());
	}


	#endregion Methods





	// =========================================================================================================
	#region Event Handling - AbstractSettingsModel
	// =========================================================================================================

	public void OnAutomatorPropertyValueChanged(object sender, AutomatorPropertyValueChangedEventArgs e)
	{
		AutomatorPropertyValueChangedEvent?.Invoke(sender, e);
	}



	public void OnEditControlGotFocus(object sender, EditControlFocusEventArgs e)
	{
		EditControlGotFocusEvent?.Invoke(sender, e);
	}



	public void OnEditControlLostFocus(object sender, EditControlFocusEventArgs e)
	{
		EditControlLostFocusEvent?.Invoke(sender, e);
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


	#endregion Event Handling

}

