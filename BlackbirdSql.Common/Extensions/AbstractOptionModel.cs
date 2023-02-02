
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;
using Microsoft.VisualStudio.Threading;


namespace BlackbirdSql.Common.Extensions
{
	public abstract class AbstractOptionModel<T> where T : AbstractOptionModel<T>, new()
	{
		private static readonly AsyncLazy<T> _liveModel = new AsyncLazy<T>(new Func<Task<T>>(CreateAsync), ThreadHelper.JoinableTaskFactory);

		private static readonly AsyncLazy<ShellSettingsManager> _settingsManager = new AsyncLazy<ShellSettingsManager>(new Func<Task<ShellSettingsManager>>(GetSettingsManagerAsync), ThreadHelper.JoinableTaskFactory);

		private static IReadOnlyList<IOptionModelPropertyWrapper> _propertyWrappers = new List<IOptionModelPropertyWrapper>();

		private static readonly object _propertyWrapperLock = new object();

		private static bool _propertyWrappersLoaded;

		public static T Instance => ThreadHelper.JoinableTaskFactory.Run(new Func<Task<T>>(GetLiveInstanceAsync));

		protected internal virtual string CollectionName { get; } = typeof(T).FullName;


#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
		public static event Action<T>? Saved;
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.

		protected AbstractOptionModel()
		{
		}

		//
		// Summary:
		//     Get the singleton instance of the options. Thread safe.
		public static Task<T> GetLiveInstanceAsync()
		{
			return _liveModel.GetValueAsync();
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
			ShellSettingsManager obj = await _settingsManager.GetValueAsync();
			SettingsScope scope = SettingsScope.UserSettings;
			SettingsStore readOnlySettingsStore = obj.GetReadOnlySettingsStore(scope);
			foreach (IOptionModelPropertyWrapper propertyWrapper in GetPropertyWrappers())
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
			ShellSettingsManager obj = await _settingsManager.GetValueAsync();
			SettingsScope scope = SettingsScope.UserSettings;
			WritableSettingsStore writableSettingsStore = obj.GetWritableSettingsStore(scope);
			foreach (IOptionModelPropertyWrapper propertyWrapper in GetPropertyWrappers())
			{
				propertyWrapper.Save(this, writableSettingsStore);
			}

			T liveModel = await GetLiveInstanceAsync();
			if (this != liveModel)
			{
				await liveModel.LoadAsync();
			}

			AbstractOptionModel<T>.Saved?.Invoke(liveModel);
		}

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
		protected internal virtual string SerializeValue(object? value, Type type, string propertyName)
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
		{
			if (value == null)
			{
				return string.Empty;
			}

			XmlSerializer val = new XmlSerializer(value!.GetType());
			using StringWriter stringWriter = new StringWriter();
			val.Serialize((TextWriter)stringWriter, value);
			return stringWriter.ToString();
		}

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
		protected internal virtual object? DeserializeValue(string serializedData, Type type, string propertyName)
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
		{
			if (serializedData.Length == 0)
			{
				if (type.IsValueType)
				{
					return Activator.CreateInstance(type);
				}

				return null;
			}

			return new XmlSerializer(type).Deserialize((TextReader)new StringReader(serializedData));
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
			return from p in GetType().GetProperties()
					 where p.PropertyType.IsPublic && p.CanRead && p.CanWrite
					 select p;
		}

		protected virtual IEnumerable<IOptionModelPropertyWrapper> GetPropertyWrappers()
		{
			if (_propertyWrappersLoaded)
			{
				return _propertyWrappers;
			}

			lock (_propertyWrapperLock)
			{
				if (_propertyWrappersLoaded)
				{
					return _propertyWrappers;
				}

				List<IOptionModelPropertyWrapper> list = new List<IOptionModelPropertyWrapper>();
				_propertyWrappers = list.AsReadOnly();
				foreach (PropertyInfo optionProperty in GetOptionProperties())
				{
					try
					{
						list.Add(new OptionModelPropertyWrapper(optionProperty));
					}
					catch (Exception ex)
					{
						Diag.Dug(ex, String.Format("AbstractOptionModel<{0}>.{1} Property:{2} PropertyType:{3} is not a valid property.", typeof(T).FullName, "GetPropertyWrappers", optionProperty.Name, optionProperty.PropertyType));
					}
				}

				_propertyWrappersLoaded = true;
			}

			return _propertyWrappers;
		}
	}
}
