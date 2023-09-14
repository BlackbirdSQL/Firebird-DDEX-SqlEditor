//
// Plagiarized from Community.VisualStudio.Toolkit extension
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Serialization;
using BlackbirdSql.Core.Ctl.Interfaces;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;
using Microsoft.VisualStudio.Threading;


namespace BlackbirdSql.Core.Ctl.Config;

public abstract class AbstractOptionModel<T> : IBOptionModel where T : AbstractOptionModel<T>, new()
{
	private readonly string _OptionGroup;

	private static readonly AsyncLazy<T> _LiveModel = new AsyncLazy<T>(new Func<Task<T>>(CreateAsync), ThreadHelper.JoinableTaskFactory);

	private static AsyncLazy<ShellSettingsManager> _SettingsManager = null;

	private static IReadOnlyList<IBOptionModelPropertyWrapper> _PropertyWrappers = new List<IBOptionModelPropertyWrapper>();

	private static readonly object _LockObject = new object();

	private static bool _PropertyWrappersLoaded;

	public static T Instance => ThreadHelper.JoinableTaskFactory.Run(new Func<Task<T>>(GetLiveInstanceAsync));


	protected internal virtual string CollectionName { get; } = typeof(T).FullName;


	private static AsyncLazy<ShellSettingsManager> SettingsManager
	{
		get
		{
			try
			{
				return _SettingsManager ??=
					new AsyncLazy<ShellSettingsManager>(new Func<Task<ShellSettingsManager>>(GetSettingsManagerAsync),
					ThreadHelper.JoinableTaskFactory);
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw ex;
			}
		}
	}

	public static event Action<T> Saved;

	protected AbstractOptionModel(string group)
	{
		_OptionGroup = group;
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
		T instance = new T();
		await instance.LoadAsync();
		return instance;
	}

	public virtual void Load()
	{
		ThreadHelper.JoinableTaskFactory.Run(new Func<Task>(LoadAsync));
	}

	public virtual async Task LoadAsync()
	{
		ShellSettingsManager obj = await SettingsManager.GetValueAsync();
		SettingsScope scope = SettingsScope.UserSettings;
		SettingsStore readOnlySettingsStore = obj.GetReadOnlySettingsStore(scope);
		foreach (IBOptionModelPropertyWrapper propertyWrapper in GetPropertyWrappers())
		{
			propertyWrapper.Load(this, readOnlySettingsStore);
		}
	}

	public virtual void Save()
	{
		ThreadHelper.JoinableTaskFactory.Run(new Func<Task>(SaveAsync));
	}

	public virtual async Task SaveAsync()
	{
		ShellSettingsManager obj = await SettingsManager.GetValueAsync();
		SettingsScope scope = SettingsScope.UserSettings;
		WritableSettingsStore writableSettingsStore = obj.GetWritableSettingsStore(scope);
		foreach (IBOptionModelPropertyWrapper propertyWrapper in GetPropertyWrappers())
		{
			propertyWrapper.Save(this, writableSettingsStore);
		}

		T liveModel = await GetLiveInstanceAsync();
		if (this != liveModel)
		{
			await liveModel.LoadAsync();
		}

		Saved?.Invoke(liveModel);
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
		try
		{
			return new ShellSettingsManager(ServiceProvider.GlobalProvider);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}
	}

	//
	// Summary:
	//     Returns an enumerable of System.Reflection.PropertyInfo for the properties of
	//     T that will be loaded and saved. Base implementation utilizes reflection.
	protected virtual IEnumerable<PropertyInfo> GetOptionProperties()
	{
		return from p in GetType().GetProperties()
			   where p.PropertyType.IsPublic && p.CanRead && p.CanWrite
			   select p;
	}

	protected virtual IEnumerable<IBOptionModelPropertyWrapper> GetPropertyWrappers()
	{
		if (_PropertyWrappersLoaded)
		{
			return _PropertyWrappers;
		}

		lock (_LockObject)
		{
			if (_PropertyWrappersLoaded)
			{
				return _PropertyWrappers;
			}

			List<IBOptionModelPropertyWrapper> list = new List<IBOptionModelPropertyWrapper>();
			_PropertyWrappers = list.AsReadOnly();
			foreach (PropertyInfo optionProperty in GetOptionProperties())
			{
				try
				{
					list.Add(new OptionModelPropertyWrapper(optionProperty));
				}
				catch (Exception ex)
				{
					Diag.Dug(ex, string.Format("AbstractOptionModel<{0}>.{1} Property:{2} PropertyType:{3} is not a valid property.", typeof(T).FullName, "GetPropertyWrappers", optionProperty.Name, optionProperty.PropertyType));
				}
			}

			_PropertyWrappersLoaded = true;
		}

		return _PropertyWrappers;
	}

	public virtual string GetGroup()
	{
		return _OptionGroup;
	}

}
