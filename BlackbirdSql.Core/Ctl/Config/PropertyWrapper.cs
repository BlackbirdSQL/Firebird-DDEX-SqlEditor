//
// Plagiarized from Community.VisualStudio.Toolkit extension
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;

using BlackbirdSql.Core.Ctl.ComponentModel;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Ctl.Enums;
using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.Core.Model.Config;

using Microsoft.VisualStudio.Settings;


namespace BlackbirdSql.Core.Ctl.Config;

//
// Summary:
//     Wraps an instance property member with public getter and setters from a Community.VisualStudio.Toolkit.AbstractSettingsModel`1,
//     and exposes the ability to load and save the value of the property to the Microsoft.VisualStudio.Settings.SettingsStore.
//
// Remarks:
//     The instance of the Community.VisualStudio.Toolkit.AbstractSettingsModel`1 provides
//     the default collection path that is used to store the property values. Adding
//     Community.VisualStudio.Toolkit.OverrideCollectionNameAttribute to the property
//     will override this only for the attributed property. This also will infer the
//     proper data type to store the property value as for common types to avoid serialization.
//     See remarks at Community.VisualStudio.Toolkit.OptionModelPropertyWrapper.ConvertPropertyTypeToStorageType``1(System.Object,Community.VisualStudio.Toolkit.AbstractSettingsModel{``0})
//     for specifics.
//     For types not supported by default, the overridable serialization methods in
//     Community.VisualStudio.Toolkit.AbstractSettingsModel`1 will be used, and the output
//     of that will be stored. Alternatively, you can apply the Community.VisualStudio.Toolkit.OverrideDataTypeAttribute
//     specifying the storage type/methodology. By default, that will use System.Convert.ChangeType(System.Object,System.Type,System.IFormatProvider)
//     with System.Globalization.CultureInfo.InvariantCulture, though that attribute
//     can also include a flag to use the type's System.ComponentModel.TypeConverterAttribute
//     instead.
//     The implementation of this class uses reflection, only in the ctor, to create
//     an open delegate that is used to get and set the property values. This initial
//     hit using reflection happens once, and subsequent load and saves of the value
//     are therefore as performant as possible. This is the technique used by Jon Skeet
//     in Google's Protocol Buffers.
public class PropertyWrapper : IBSettingsModelPropertyWrapper
{
	public sealed class OrderComparer : IComparer<IBSettingsModelPropertyWrapper>
	{
		public static readonly OrderComparer Default = new OrderComparer();

		public int Compare(IBSettingsModelPropertyWrapper x, IBSettingsModelPropertyWrapper y)
		{
			return x.DisplayOrder - y.DisplayOrder;
		}
	}



	//
	// Summary:
	//     (Immutable) Dictionary of types, limited to the types available in Microsoft.VisualStudio.Settings.SettingsStore,
	//     to a delegate with a signature of WritableSettingsStore targetSettingsStore,
	//     string collectionPath, string propertyPath, object value. This is initialized
	//     in the static ctor.
	protected static IReadOnlyDictionary<EnNativeSettingsType, Action<WritableSettingsStore, string, string, object>> SettingStoreSetMethodsDict { get; }

	//
	// Summary:
	//     (Immutable) Dictionary of types, limited to the types available in Microsoft.VisualStudio.Settings.SettingsStore,
	//     to a delegate with a signature of SettingsStore targetSettingsStore, string collectionPath,
	//     string propertyPath, which returns the value of the setting as an object. This
	//     is initialized in the static ctor.
	protected static IReadOnlyDictionary<EnNativeSettingsType, Func<SettingsStore, string, string, object>> SettingStoreGetMethodsDict { get; }

	//
	// Summary:
	//     (Immutable) The property being wrapped.
	public PropertyInfo PropInfo { get; }

	//
	// Summary:
	//     (Immutable) Either specified via Community.VisualStudio.Toolkit.OverrideDataTypeAttribute
	//     or inferred from the System.Reflection.PropertyInfo.PropertyType of the wrapped
	//     property in the Community.VisualStudio.Toolkit.OptionModelPropertyWrapper.InferDataType(System.Type)
	//     method. This serves a dual purpose - it specifies how the wrapped value is converted
	//     to the storage type as well as the native type that is stored.
	protected EnSettingDataType DataType { get; }

	//
	// Summary:
	//     (Immutable) A delegate to the method to set a value in Microsoft.VisualStudio.Settings.WritableSettingsStore.
	//     This delegate signature is WritableSettingsStore settingsStore, string collectionPath,
	//     string propertyPath, object value.
	protected Action<WritableSettingsStore, string, string, object> SettingStoreSetMethod { get; }

	//
	// Summary:
	//     (Immutable) A delegate to the method to get a value from the Microsoft.VisualStudio.Settings.SettingsStore.
	//     This delegate signature is SettingsStore settingsStore, string collectionPath,
	//     string propertyPath and returns the value stored as an object.
	protected Func<SettingsStore, string, string, object> SettingStoreGetMethod { get; }

	//
	// Summary:
	//     (Immutable) A delegate to set the value of the wrapped property from the Community.VisualStudio.Toolkit.AbstractSettingsModel`1
	//     instance. These are explicitly object types in the signature but must be of the
	//     proper types when they are called. The signature is AbstractSettingsModel{T} targetObject,
	//     object value, where the type of value must be assignable to the System.Reflection.PropertyInfo.PropertyType
	//     of the wrapped property.
	protected Action<object, object> WrappedPropertySetMethod { get; }

	//
	// Summary:
	//     (Immutable) A delegate to get the value of the wrapped property from the Community.VisualStudio.Toolkit.AbstractSettingsModel`1
	//     instance. These are explicitly object types in the signature but must be of the
	//     proper types when they are called. The signature is AbstractSettingsModel{T} targetObject,
	//     where the type that is returned will be the System.Reflection.PropertyInfo.PropertyType
	//     of the wrapped property.
	public Func<object, object> WrappedPropertyGetMethod { get; }

	//
	// Summary:
	//     (Immutable) If not null the CollectionPath the value of this property should
	//     be loaded/saved to, which is set via the optional Community.VisualStudio.Toolkit.OverrideCollectionNameAttribute
	//     on the property. If null, the Community.VisualStudio.Toolkit.AbstractSettingsModel`1.CollectionName
	//     should be used instead.
	protected string OverrideCollectionName { get; }

	//
	// Summary:
	//     (Immutable) The PropertyName in the Microsoft.VisualStudio.Settings.SettingsStore
	//     where the value of this property is stored. By default, this is the actual name
	//     of the property that this instance wraps. This can be overridden via the optional
	//     Community.VisualStudio.Toolkit.OverridePropertyNameAttribute on the property.
	public string PropertyName { get; }

	public string Automator { get; }
	public bool IsAutomator { get; }
	public bool InvertAutomation { get; }
	public int AutomationEnableValue { get; }

	//
	// Summary:
	//     (Immutable) The data type the property will be stored as, which is limited to
	//     the types available in Microsoft.VisualStudio.Settings.SettingsStore. Set via
	//     Community.VisualStudio.Toolkit.OptionModelPropertyWrapper.GetNativeSettingsType(Community.VisualStudio.Toolkit.SettingDataType).
	//     See also the summary of Community.VisualStudio.Toolkit.OptionModelPropertyWrapper.DataType.
	protected EnNativeSettingsType NativeStorageType { get; }

	//
	// Summary:
	//     (Immutable) If Community.VisualStudio.Toolkit.OverrideDataTypeAttribute is applied,
	//     and the Community.VisualStudio.Toolkit.OptionModelPropertyWrapper.PropertyInfo
	//     PropertyType has a System.ComponentModel.TypeConverterAttribute applied that
	//     is compatible with its declared storage data type, this System.ComponentModel.TypeConverter
	//     will be non-null and used to convert the property value to and from the Microsoft.VisualStudio.Settings.SettingsStore
	//     Community.VisualStudio.Toolkit.OptionModelPropertyWrapper.NativeStorageType.
	protected TypeConverter Converter { get; }


	public object DefaultValue { get; }

	public int DisplayOrder { get; } = int.MaxValue;

	//
	// Summary:
	//     One-time static initialization of delegates to interact with the Microsoft.VisualStudio.Settings.SettingsStore
	//     and Microsoft.VisualStudio.Settings.WritableSettingsStore.
	static PropertyWrapper()
	{
		Dictionary<EnNativeSettingsType, Action<WritableSettingsStore, string, string, object>> setDictionary = (Dictionary<EnNativeSettingsType, Action<WritableSettingsStore, string, string, object>>)(SettingStoreSetMethodsDict = new Dictionary<EnNativeSettingsType, Action<WritableSettingsStore, string, string, object>>(7));
		Dictionary<EnNativeSettingsType, Func<SettingsStore, string, string, object>> getDictionary = (Dictionary<EnNativeSettingsType, Func<SettingsStore, string, string, object>>)(SettingStoreGetMethodsDict = new Dictionary<EnNativeSettingsType, Func<SettingsStore, string, string, object>>(7));
		Type writableSettingsStoreType = typeof(WritableSettingsStore);

		setDictionary[EnNativeSettingsType.String] = CreateSettingsStoreSetMethod<string>(writableSettingsStoreType.GetMethod("SetString",
		[
			typeof(string),
			typeof(string),
			typeof(string)
		]));

		setDictionary[EnNativeSettingsType.Int32] = CreateSettingsStoreSetMethod<int>(writableSettingsStoreType.GetMethod("SetInt32",
		[
			typeof(string),
			typeof(string),
			typeof(int)
		]));

		setDictionary[EnNativeSettingsType.UInt32] = CreateSettingsStoreSetMethod<uint>(writableSettingsStoreType.GetMethod("SetUInt32",
		[
			typeof(string),
			typeof(string),
			typeof(uint)
		]));

		setDictionary[EnNativeSettingsType.Int64] = CreateSettingsStoreSetMethod<long>(writableSettingsStoreType.GetMethod("SetInt64",
		[
			typeof(string),
			typeof(string),
			typeof(long)
		]));

		setDictionary[EnNativeSettingsType.UInt64] = CreateSettingsStoreSetMethod<ulong>(writableSettingsStoreType.GetMethod("SetUInt64",
		[
			typeof(string),
			typeof(string),
			typeof(ulong)
		]));

		setDictionary[EnNativeSettingsType.Binary] = CreateSettingsStoreSetMethod<MemoryStream>(writableSettingsStoreType.GetMethod("SetMemoryStream",
		[
			typeof(string),
			typeof(string),
			typeof(MemoryStream)
		]));

		Type settingsStoreType = typeof(SettingsStore);

		getDictionary[EnNativeSettingsType.String] = CreateSettingsStoreGetMethod<string>(settingsStoreType.GetMethod("GetString",
		[
			typeof(string),
			typeof(string)
		]));

		getDictionary[EnNativeSettingsType.Int32] = CreateSettingsStoreGetMethod<int>(settingsStoreType.GetMethod("GetInt32",
		[
			typeof(string),
			typeof(string)
		]));

		getDictionary[EnNativeSettingsType.UInt32] = CreateSettingsStoreGetMethod<uint>(settingsStoreType.GetMethod("GetUInt32",
		[
			typeof(string),
			typeof(string)
		]));

		getDictionary[EnNativeSettingsType.Int64] = CreateSettingsStoreGetMethod<long>(settingsStoreType.GetMethod("GetInt64",
		[
			typeof(string),
			typeof(string)
		]));

		getDictionary[EnNativeSettingsType.UInt64] = CreateSettingsStoreGetMethod<ulong>(settingsStoreType.GetMethod("GetUInt64",
		[
			typeof(string),
			typeof(string)
		]));

		getDictionary[EnNativeSettingsType.Binary] = CreateSettingsStoreGetMethod<MemoryStream>(settingsStoreType.GetMethod("GetMemoryStream",
		[
			typeof(string),
			typeof(string)
		]));
	}

	//
	// Summary:
	//     Creates a delegate to the settings store that sets a value in the settings store.
	//     The delegate exposes a common signature that intentionally makes the type to
	//     be set an object to simplify code.
	//
	// Parameters:
	//   mi:
	//     The method info of the typed set method for the Microsoft.VisualStudio.Settings.WritableSettingsStore.
	//
	// Type parameters:
	//   T:
	//     The actual type being stored in the Microsoft.VisualStudio.Settings.WritableSettingsStore.
	//
	// Returns:
	//     The delegate to set a value, as described above.
	private static Action<WritableSettingsStore, string, string, object> CreateSettingsStoreSetMethod<T>(MethodInfo mi)
	{
		Action<WritableSettingsStore, string, string, T> action = (Action<WritableSettingsStore, string, string, T>)Delegate.CreateDelegate(typeof(Action<WritableSettingsStore, string, string, T>), mi, throwOnBindFailure: true);
		return delegate (WritableSettingsStore settingsStore, string collectionName, string propertyName, object value)
		{
			action(settingsStore, collectionName, propertyName, (T)value);
		};
	}

	//
	// Summary:
	//     Creates a delegate to the settings store that gets a value from the settings
	//     store. The delegate exposes a common signature that intentionally makes the return
	//     type an object to simplify code.
	//
	// Parameters:
	//   mi:
	//     The method info of the typed get method for the Microsoft.VisualStudio.Settings.SettingsStore.
	//
	// Type parameters:
	//   T:
	//     The actual type that is stored in the Microsoft.VisualStudio.Settings.SettingsStore.
	//
	// Returns:
	//     The delegate to get a value, as described above.
	private static Func<SettingsStore, string, string, object> CreateSettingsStoreGetMethod<T>(MethodInfo mi)
	{
		Func<SettingsStore, string, string, T> func = (Func<SettingsStore, string, string, T>)Delegate.CreateDelegate(typeof(Func<SettingsStore, string, string, T>), mi, throwOnBindFailure: true);
		return (settingsStore, collectionName, propertyName) => func(settingsStore, collectionName, propertyName);
	}

	//
	// Summary:
	//     Initializes a new instance of the class.
	//
	// Parameters:
	//   propertyInfo:
	//     The property being wrapped.
	public PropertyWrapper(PropertyInfo propInfo)
	{
		PropInfo = propInfo;
		PropertyName = propInfo.Name;

		WrappedPropertySetMethod = CreateWrappedPropertySetDelegate(propInfo);
		WrappedPropertyGetMethod = CreateWrappedPropertyGetDelegate(propInfo);

		OverrideDataTypeAttribute overrideDataTypeAttribute = null;

		foreach (Attribute customAttribute in propInfo.GetCustomAttributes())
		{
			// Diag.Trace($"Validating {PropertyName} custom attribute: {customAttribute.GetType().FullName}.");

			if (customAttribute is DefaultValueAttribute defaultValue)
			{
				DefaultValue = defaultValue.Value;
				// Diag.Trace($"Loaded default for {PropertyName}: {DefaultValue}.");
				continue;
			}
			if (customAttribute is AutomationAttribute autoAttr)
			{
				Automator = autoAttr.Automator;
				IsAutomator = Automator == null;
				InvertAutomation = autoAttr.Invert;
				AutomationEnableValue = autoAttr.EnableValue;

				// Diag.Trace($"Loaded automation for {PropertyName}: Automator: {Automator} IsAutomator: {IsAutomator} InvertAutomation: {InvertAutomation}.");
				continue;
			}
			if (customAttribute is DisplayOrderAttribute displayOrder)
			{
				DisplayOrder = displayOrder.DisplayOrder;
				continue;
			}
			if (customAttribute is OverrideCollectionNameAttribute overrideCollectionNameAttribute)
			{
				string text = overrideCollectionNameAttribute.CollectionName.Trim();
				if (text.Length > 0)
				{
					OverrideCollectionName = text;
				}
				continue;
			}
			if (customAttribute is OverridePropertyNameAttribute overridePropertyNameAttribute)
			{
				string text2 = overridePropertyNameAttribute.PropertyName.Trim();
				if (text2.Length > 0)
				{
					PropertyName = text2;
				}
			}
			else
			{
				if (customAttribute is OverrideDataTypeAttribute overrideDataTypeAttribute2)
				{
					overrideDataTypeAttribute = overrideDataTypeAttribute2;
				}
			}
		}

		if (overrideDataTypeAttribute != null)
		{
			DataType = overrideDataTypeAttribute.SettingDataType;
			if (overrideDataTypeAttribute.UseTypeConverter && DataType != EnSettingDataType.Legacy && DataType != EnSettingDataType.Serialized)
			{
				Converter = TypeDescriptor.GetConverter(propInfo.PropertyType);
			}
		}
		else
		{
			DataType = InferDataType(propInfo.PropertyType);
		}

		NativeStorageType = GetNativeSettingsType(DataType);
		SettingStoreSetMethod = SettingStoreSetMethodsDict[NativeStorageType];
		SettingStoreGetMethod = SettingStoreGetMethodsDict[NativeStorageType];
	}

	//
	// Summary:
	//     Creates a delegate that can get the value of a property with object signatures.
	//     This is for both performance reasons and ease of implementation as types are
	//     not known until runtime.
	//
	// Parameters:
	//   propertyInfo:
	//     The property for which to create the delegate.
	//
	// Returns:
	//     A delegate as described above.
	protected static Func<object, object> CreateWrappedPropertyGetDelegate(PropertyInfo propertyInfo)
	{
		MethodInfo method = typeof(PropertyWrapper).GetMethod("PropertyGetHelper", BindingFlags.Static | BindingFlags.NonPublic);

		if (method == null)
		{
			InvalidOperationException ex = new("Could not get method PropertyGetHelper");
			Diag.Dug(ex);
			throw ex;
		}

		return (Func<object, object>)method.MakeGenericMethod(propertyInfo.DeclaringType, propertyInfo.PropertyType).Invoke(null, [propertyInfo.GetGetMethod(nonPublic: false)]);
	}



	//
	// Summary:
	//     Gets a delegate that ultimately will get value of a property. The real types
	//     are not known at compile-time, so this is called via reflection. The returned
	//     delegate has the signature using objects, which at runtime are cast to the proper
	//     types.
	//
	// Parameters:
	//   method:
	//     The property get method.
	//
	// Type parameters:
	//   TTarget:
	//     Type for the target object on which the property get method will be called.
	//
	//   TReturn:
	//     Type returned by the property get method.
	//
	// Returns:
	//     A delegate as described above.
	private static Func<object, object> PropertyGetHelper<TTarget, TReturn>(MethodInfo method)
	{
		Func<TTarget, TReturn> func = (Func<TTarget, TReturn>)Delegate.CreateDelegate(typeof(Func<TTarget, TReturn>), method);
		return (target) => func((TTarget)target);
	}

	//
	// Summary:
	//     Creates a delegate that can set the value of a property with object signatures.
	//     This is for both performance reasons and ease of implementation as types are
	//     not known until runtime.
	//
	// Parameters:
	//   propertyInfo:
	//     The property for which to create the delegate.
	//
	// Returns:
	//     A delegate as described above.
	protected static Action<object, object> CreateWrappedPropertySetDelegate(PropertyInfo propertyInfo)
	{
		MethodInfo method = typeof(PropertyWrapper).GetMethod("PropertySetHelper", BindingFlags.Static | BindingFlags.NonPublic);

		if (method == null)
		{
			InvalidOperationException ex = new("Could not get method PropertySetHelper for " + propertyInfo.Name);
			Diag.Dug(ex);
			throw ex;
		}

		return (Action<object, object>)method.MakeGenericMethod(propertyInfo.DeclaringType, propertyInfo.PropertyType).Invoke(null, [propertyInfo.GetSetMethod(nonPublic: false)]);
	}



	//
	// Summary:
	//     Gets a delegate that ultimately will set value of a property. The real types
	//     are not known at compile-time, so this is called via reflection. The returned
	//     delegate has the signature using objects, which at runtime are cast to the proper
	//     types.
	//
	// Parameters:
	//   method:
	//     The property set method.
	//
	// Type parameters:
	//   TTarget:
	//     Type for the target object on which the property set method will be called.
	//
	//   TParam:
	//     Type expected by the property set method.
	//
	// Returns:
	//     A delegate as described above.
	private static Action<object, object> PropertySetHelper<TTarget, TParam>(MethodInfo method)
	{
		Action<TTarget, TParam> action = (Action<TTarget, TParam>)Delegate.CreateDelegate(typeof(Action<TTarget, TParam>), method);
		return delegate (object target, object value)
		{
			action((TTarget)target, (TParam)value);
		};
	}



	//
	// Summary:
	//     The value of the wrapped property is retrieved by calling the property get method
	//     on baseOptionModel. This value is converted or serialized to a native type supported
	//     by the settingsStore, then persisted to the store, assuring the collection exists
	//     first. No exceptions should be thrown from this method.
	//
	// Parameters:
	//   baseOptionModel:
	//     The base option model which is used as the target object from which the property
	//     value will be retrieved. It also can be used for serialization of stored data.
	//
	//   settingsStore:
	//     The settings store to set the setting value in.
	//
	// Type parameters:
	//   TOptMdl:
	//     Type of the base option model.
	//
	// Returns:
	//     True if we were able to persist the value in the store. However, if the serialization
	//     results in a null value, it cannot be persisted in the settings store and false
	//     will be returned. False is also returned if any step of the process failed, and
	//     these are logged.
	public virtual bool Save<TOptMdl>(AbstractSettingsModel<TOptMdl> baseOptionModel, WritableSettingsStore settingsStore) where TOptMdl : AbstractSettingsModel<TOptMdl>
	{
		string collectionName = OverrideCollectionName ?? baseOptionModel.CollectionName;
		object obj = null;

		try
		{
			obj = WrappedPropertyGetMethod(baseOptionModel);
			obj = ConvertPropertyTypeToStorageType(obj, baseOptionModel);
			if (obj == null)
			{
				Diag.StackException(string.Format("Cannot store null in settings store. AbstractSettingsModel<{0}>.{1} CollectionName:{2} PropertyName:{3} dataType:{4} PropertyType:{5} Value:{6}", baseOptionModel.GetType().FullName, "Load", collectionName, PropertyName, DataType, PropInfo.PropertyType, obj ?? "[NULL]"));
				return false;
			}

			settingsStore.CreateCollection(collectionName);
			SettingStoreSetMethod(settingsStore, collectionName, PropertyName, obj);
			return true;
		}
		catch (Exception ex)
		{
			Diag.Dug(ex, string.Format("AbstractSettingsModel<{0}>.{1} CollectionName:{2} PropertyName:{3} dataType:{4} PropertyType:{5} Value:{6}", baseOptionModel.GetType().FullName, "Save", collectionName, PropertyName, DataType, PropInfo.PropertyType, obj ?? "[NULL]"));
		}

		return false;
	}

	public virtual bool Save<TOptMdl>(AbstractSettingsModel<TOptMdl> baseOptionModel, IBTransientSettings transientSettings) where TOptMdl : AbstractSettingsModel<TOptMdl>
	{
		string collectionName = OverrideCollectionName ?? baseOptionModel.CollectionName;
		object obj = null;

		try
		{
			obj = WrappedPropertyGetMethod(baseOptionModel);
			if (obj == null)
			{
				Diag.StackException(string.Format("Cannot store null in settings store. AbstractSettingsModel<{0}>.{1} CollectionName:{2} PropertyName:{3} dataType:{4} PropertyType:{5} Value:{6}", baseOptionModel.GetType().FullName, "Save", collectionName, PropertyName, DataType, PropInfo.PropertyType, obj ?? "[NULL]"));
				return false;
			}

			transientSettings[baseOptionModel.LivePrefix + PropertyName] = obj;
			return true;
		}
		catch (Exception ex)
		{
			Diag.Dug(ex, string.Format("AbstractSettingsModel<{0}>.{1} CollectionName:{2} PropertyName:{3} dataType:{4} PropertyType:{5} Value:{6}", baseOptionModel.GetType().FullName, "Load", collectionName, PropertyName, DataType, PropInfo.PropertyType, obj ?? "[NULL]"));
		}

		return false;
	}

	//
	// Summary:
	//     If the setting is found in the settingsStore, retrieves the value of the setting,
	//     converts or deserializes it to the type of the wrapped property, and calls the
	//     property set method on the baseOptionModel. No exceptions should be thrown from
	//     this method. No changes to the property will be made if the setting does not
	//     exist.
	//
	// Parameters:
	//   baseOptionModel:
	//     The base option model which is used as the target object on which the property
	//     will be set. It also can be used for deserialization of stored data.
	//
	//   settingsStore:
	//     The settings store to retrieve the setting value from.
	//
	// Type parameters:
	//   TOptMdl:
	//     Type of the base option model.
	//
	// Returns:
	//     True if the value exists in the settingsStore, and the property was updated in
	//     baseOptionModel, false if setting does not exist or any step of the process failed.
	public virtual bool Load<TOptMdl>(AbstractSettingsModel<TOptMdl> baseOptionModel, SettingsStore settingsStore) where TOptMdl : AbstractSettingsModel<TOptMdl>
	{
		string text = OverrideCollectionName ?? baseOptionModel.CollectionName;
		object obj = null;

		try
		{
			if (!settingsStore.PropertyExists(text, PropertyName))
			{
				return false;
			}

			obj = SettingStoreGetMethod(settingsStore, text, PropertyName);
			obj = ConvertStorageTypeToPropertyType(obj, baseOptionModel);

			WrappedPropertySetMethod(baseOptionModel, obj);
			return true;
		}
		catch (Exception ex)
		{
			Diag.Dug(ex, string.Format("AbstractSettingsModel<{0}>.{1} CollectionName:{2} PropertyName:{3} dataType:{4} PropertyType:{5} Value:{6}", baseOptionModel.GetType().FullName, "Load", text, PropertyName, DataType, PropInfo.PropertyType, obj ?? "[NULL]"));
		}

		return false;
	}

	public virtual bool Load<TOptMdl>(AbstractSettingsModel<TOptMdl> baseOptionModel, IBTransientSettings transientSettings) where TOptMdl : AbstractSettingsModel<TOptMdl>
	{
		object obj = null;

		try
		{
			if (!transientSettings.PropertyExists(baseOptionModel.LivePrefix + PropertyName))
			{
				return false;
			}

			obj = transientSettings[baseOptionModel.LivePrefix + PropertyName];

			WrappedPropertySetMethod(baseOptionModel, obj);
			return true;
		}
		catch (Exception ex)
		{
			Diag.Dug(ex, string.Format("AbstractSettingsModel<{0}>.{1} PropertyName:{2} dataType:{3} PropertyType:{4} Value:{5}", baseOptionModel.GetType().FullName, "Load", baseOptionModel.LivePrefix + PropertyName, DataType, PropInfo.PropertyType, obj ?? "[NULL]"));
		}

		return false;
	}


	public virtual bool LoadDefault<TOptMdl>(AbstractSettingsModel<TOptMdl> baseOptionModel) where TOptMdl : AbstractSettingsModel<TOptMdl>
	{
		string text = OverrideCollectionName ?? baseOptionModel.CollectionName;
		object obj = DefaultValue;

		try
		{
			if (obj == null)
				return false;

			WrappedPropertySetMethod(baseOptionModel, obj);
			return true;
		}
		catch (Exception ex)
		{
			Diag.Dug(ex, string.Format("AbstractSettingsModel<{0}>.{1} CollectionName:{2} PropertyName:{3} dataType:{4} PropertyType:{5} Value:{6}", baseOptionModel.GetType().FullName, "LoadDefault", text, PropertyName, DataType, PropInfo.PropertyType, obj ?? "[NULL]"));
		}

		return false;
	}

	//
	// Summary:
	//     Gets the native data type that the property value will be stored as in the Microsoft.VisualStudio.Settings.SettingsStore.
	//
	// Parameters:
	//   settingDataType:
	//     The type/mechanism by which the value will be converted for storage.
	//
	// Returns:
	//     The native data type that the property value will be stored as in the Microsoft.VisualStudio.Settings.SettingsStore.
	protected virtual EnNativeSettingsType GetNativeSettingsType(EnSettingDataType settingDataType)
	{
		switch (settingDataType)
		{
			case EnSettingDataType.String:
			case EnSettingDataType.Legacy:
			case EnSettingDataType.Serialized:
				return EnNativeSettingsType.String;
			case EnSettingDataType.Int32:
				return EnNativeSettingsType.Int32;
			case EnSettingDataType.UInt32:
				return EnNativeSettingsType.UInt32;
			case EnSettingDataType.Int64:
				return EnNativeSettingsType.Int64;
			case EnSettingDataType.UInt64:
				return EnNativeSettingsType.UInt64;
			case EnSettingDataType.Binary:
				return EnNativeSettingsType.Binary;
			default:
				InvalidOperationException ex = new($"GetNativeDataType for SettingDataType {settingDataType} is not supported.");
				Diag.Dug(ex);
				throw ex;
		}
	}

	//
	// Summary:
	//     Infers the underlying type (or mechanism for unknown types) by which we will
	//     set the native data type. See remarks at Community.VisualStudio.Toolkit.OptionModelPropertyWrapper.ConvertPropertyTypeToStorageType``1(System.Object,Community.VisualStudio.Toolkit.AbstractSettingsModel{``0})
	//
	// Parameters:
	//   propertyType:
	//     The type of the property being wrapped.
	//
	// Returns:
	//     A SettingDataType.
	protected virtual EnSettingDataType InferDataType(Type propertyType)
	{
		if (propertyType.IsEnum)
		{
			propertyType = propertyType.GetEnumUnderlyingType();
		}

		if (propertyType == typeof(string) || propertyType == typeof(float) || propertyType == typeof(double) || propertyType == typeof(decimal) || propertyType == typeof(char) || propertyType == typeof(Guid) || propertyType == typeof(DateTimeOffset))
		{
			return EnSettingDataType.String;
		}

		if (propertyType == typeof(bool) || propertyType == typeof(sbyte) || propertyType == typeof(byte) || propertyType == typeof(short) || propertyType == typeof(ushort) || propertyType == typeof(int) || propertyType == typeof(Color))
		{
			return EnSettingDataType.Int32;
		}

		if (propertyType == typeof(uint))
		{
			return EnSettingDataType.UInt32;
		}

		if (propertyType == typeof(long) || propertyType == typeof(DateTime))
		{
			return EnSettingDataType.Int64;
		}

		if (propertyType == typeof(ulong))
		{
			return EnSettingDataType.UInt64;
		}

		if (propertyType == typeof(byte[]) || propertyType == typeof(MemoryStream))
		{
			return EnSettingDataType.Binary;
		}

		return EnSettingDataType.Serialized;
	}


	[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0038:Use pattern matching", Justification = "<Pending>")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0019:Use pattern matching", Justification = "<Pending>")]
	//
	// Summary:
	//     Convert the propertyValue retrieved from the property to the type it will be
	//     stored as in the Microsoft.VisualStudio.Settings.SettingsStore.
	//
	// Parameters:
	//   propertyValue:
	//     The value retrieved from the wrapped property, as an object.
	//
	//   baseOptionModel:
	//     Instance of Community.VisualStudio.Toolkit.AbstractSettingsModel`1. For types requiring
	//     serialization, methods in this object are used.
	//
	// Type parameters:
	//   TOptMdl:
	//     Type of Community.VisualStudio.Toolkit.AbstractSettingsModel`1.
	//
	// Returns:
	//     propertyValue, converted to one of the types supported by Microsoft.VisualStudio.Settings.SettingsStore.
	//
	// Remarks:
	//     The methods Community.VisualStudio.Toolkit.OptionModelPropertyWrapper.ConvertPropertyTypeToStorageType``1(System.Object,Community.VisualStudio.Toolkit.AbstractSettingsModel{``0}),
	//     Community.VisualStudio.Toolkit.OptionModelPropertyWrapper.ConvertStorageTypeToPropertyType``1(System.Object,Community.VisualStudio.Toolkit.AbstractSettingsModel{``0}),
	//     and Community.VisualStudio.Toolkit.OptionModelPropertyWrapper.InferDataType(System.Type)
	//     are designed to work in tandem, and are therefore tightly coupled. The Microsoft.VisualStudio.Settings.SettingsStore
	//     cannot store null values, therefore any property that is converted to a reference
	//     type cannot round-trip successfully if that conversion yields System.String,
	//     System.IO.MemoryStream, and arrays of System.Byte - in these cases the equivalent
	//     of empty is stored, therefore when loaded the result will not match.
	//     The method Community.VisualStudio.Toolkit.OptionModelPropertyWrapper.InferDataType(System.Type)
	//     returns an enumeration that identifies both the native storage type, and method
	//     of conversion, that will be used when storing the property value. These defaults
	//     can be overridden via the Community.VisualStudio.Toolkit.OverrideDataTypeAttribute.
	//     The method Community.VisualStudio.Toolkit.OptionModelPropertyWrapper.ConvertPropertyTypeToStorageType``1(System.Object,Community.VisualStudio.Toolkit.AbstractSettingsModel{``0})
	//     is provided the current value of the property. It's job is to convert this value
	//     to the native storage type based on Community.VisualStudio.Toolkit.OptionModelPropertyWrapper.DataType
	//     which is set via Community.VisualStudio.Toolkit.OptionModelPropertyWrapper.InferDataType(System.Type).
	//     The method Community.VisualStudio.Toolkit.OptionModelPropertyWrapper.ConvertStorageTypeToPropertyType``1(System.Object,Community.VisualStudio.Toolkit.AbstractSettingsModel{``0})
	//     is the reverse of the above. Given an instance of the native storage type, it's
	//     job is to convert it to an instance the property type.
	//     The conversions between types in the default implementation follows this:
	//     • A property with a setting data type of Community.VisualStudio.Toolkit.SettingDataType.Legacy
	//     uses System.Runtime.Serialization.Formatters.Binary.BinaryFormatter and stores
	//     it as a base64 encoded string. null values are stored as an empty string.
	//     • Array of System.Byte is wrapped in a System.IO.MemoryStream. null values are
	//     converted to an empty System.IO.MemoryStream.
	//     • System.Drawing.Color, with setting data type Community.VisualStudio.Toolkit.SettingDataType.Int32
	//     uses To[From]Argb to store it as an Int32.
	//     • System.Guid, with setting data type Community.VisualStudio.Toolkit.SettingDataType.String
	//     uses System.Guid.ToString and System.Guid.Parse(System.String) to convert to
	//     and from a string.
	//     • System.DateTime, with setting data type Community.VisualStudio.Toolkit.SettingDataType.Int64
	//     uses To[From]Binary to store it as an Int64.
	//     • System.DateTimeOffset, with setting data type Community.VisualStudio.Toolkit.SettingDataType.String
	//     uses the round-trip 'o' specifier to store as a string.
	//     • System.Single and System.Double, with setting data type Community.VisualStudio.Toolkit.SettingDataType.String
	//     uses the round-trip 'G9' and 'G17' specifier to store as a string, and is parsed
	//     via the standard Convert method.
	//     • System.String, if null, is stored as an empty string.
	//     • Enumerations are converted to/from their underlying type.
	//     • Integral numeric types, System.Single, System.Double, System.Decimal, and System.Char
	//     use System.Convert.ChangeType(System.Object,System.Type,System.IFormatProvider),
	//     using System.Globalization.CultureInfo.InvariantCulture. Enumerations are stored
	//     as their underlying integral numeric type.
	//     • Any type not described above, or a property with a setting data type of Community.VisualStudio.Toolkit.SettingDataType.Serialized
	//     uses Community.VisualStudio.Toolkit.AbstractSettingsModel`1.SerializeValue(System.Object,System.Type,System.String)
	//     and Community.VisualStudio.Toolkit.AbstractSettingsModel`1.DeserializeValue(System.String,System.Type,System.String)
	//     and stores it as binary, refer to those overridable methods for details.
	protected virtual object ConvertPropertyTypeToStorageType<TOptMdl>(object propertyValue, AbstractSettingsModel<TOptMdl> baseOptionModel) where TOptMdl : AbstractSettingsModel<TOptMdl>
	{
		switch (DataType)
		{
			case EnSettingDataType.Serialized:
				if (NativeStorageType != EnNativeSettingsType.String)
				{
					InvalidOperationException ex = new($"The SettingDataType of Serialized is not capable of supporting native storage type {NativeStorageType}");
					Diag.Dug(ex);
					throw ex;
				}

				string serialized = baseOptionModel.SerializeValue(propertyValue, PropInfo.PropertyType, PropertyName);

				if (serialized == null)
				{
					InvalidOperationException ex = new("The SerializeValue method of " + baseOptionModel.GetType().FullName + " returned  a null value. This method cannot return null.");
					Diag.Dug(ex);
					throw ex;
				}

				return serialized;
			case EnSettingDataType.Legacy:
				if (NativeStorageType != EnNativeSettingsType.String)
				{
					InvalidOperationException ex = new($"The SettingDataType of Legacy is not capable of supporting native storage type {NativeStorageType}");
					Diag.Dug(ex);
					throw ex;
				}

				return StringUtils.SerializeBinary64(propertyValue);
			default:
				{
					Type type = NativeStorageType.GetDotNetTypeX();
					if (Converter != null)
					{
						bool flag = false;
						if (NativeStorageType == EnNativeSettingsType.Binary)
						{
							flag = true;
							type = typeof(byte[]);
						}

						if (!Converter!.CanConvertTo(type))
						{
							InvalidOperationException ex = new($"TypeConverter {Converter!.GetType().FullName} can not convert {PropInfo.PropertyType.FullName} to {NativeStorageType} ({type.Name})");
							Diag.Dug(ex);
							throw ex;
						}

						object obj = Converter!.ConvertTo(null, CultureInfo.InvariantCulture, propertyValue, type);
						if (obj == null)
						{
							InvalidOperationException ex = new($"TypeConverter {Converter!.GetType().FullName} returned null converting from {PropInfo.PropertyType.FullName} to {NativeStorageType} ({type.Name}), which is not supported.");
							Diag.Dug(ex);
							throw ex;
						}

						if (!type.IsInstanceOfType(obj))
						{
							InvalidOperationException ex = new($"TypeConverter {Converter!.GetType().FullName} returned type {obj.GetType().FullName} when converting from {PropInfo.PropertyType.FullName} to {NativeStorageType} ({type.Name}).");
							Diag.Dug(ex);
							throw ex;
						}

						if (flag)
						{
							return new MemoryStream((byte[])obj);
						}

						return obj;
					}

					switch (NativeStorageType)
					{
						case EnNativeSettingsType.Int32:

							break;
						case EnNativeSettingsType.Int64:
							if (propertyValue is DateTime)
							{
								return ((DateTime)propertyValue).ToBinary();
							}

							break;
						case EnNativeSettingsType.String:
							if (propertyValue is Color color)
							{
								if (color.IsNamedColor)
									return "##" + color.Name;
								else
									return ((Color)propertyValue).ToArgb().ToString(CultureInfo.InvariantCulture);
							}
							if (propertyValue is Guid)
							{
								return ((Guid)propertyValue).ToString();
							}

							if (propertyValue is DateTimeOffset)
							{
								return ((DateTimeOffset)propertyValue).ToString("o", CultureInfo.InvariantCulture);
							}

							if (propertyValue is float)
							{
								return ((float)propertyValue).ToString("G9", CultureInfo.InvariantCulture);
							}

							if (propertyValue is double)
							{
								return ((double)propertyValue).ToString("G17", CultureInfo.InvariantCulture);
							}

							if (propertyValue == null)
							{
								return string.Empty;
							}

							break;
						case EnNativeSettingsType.Binary:
							{
								byte[] array = propertyValue as byte[];
								if (array != null)
								{
									return new MemoryStream(array);
								}

								MemoryStream memoryStream = propertyValue as MemoryStream;
								if (memoryStream != null)
								{
									return memoryStream;
								}

								if (propertyValue == null)
								{
									return new MemoryStream();
								}

								InvalidOperationException ex = new("Can not convert NativeStorageType of Binary to " + propertyValue!.GetType().FullName + " - property type must be byte[] or MemoryStream.");
								Diag.Dug(ex);
								throw ex;
							}
					}

					if (propertyValue == null)
					{
						InvalidOperationException ex = new($"A null property value with SettingDataType of {DataType} is not supported.");
						Diag.Dug(ex);
						throw ex;
					}

					if (type.IsInstanceOfType(propertyValue))
					{
						return propertyValue;
					}

					return Convert.ChangeType(propertyValue, type, CultureInfo.InvariantCulture);
				}
		}
	}

	//
	// Summary:
	//     Convert the settingsStoreValue retrieved from the settings store to the type
	//     of the property we are wrapping. See remarks at Community.VisualStudio.Toolkit.OptionModelPropertyWrapper.ConvertPropertyTypeToStorageType``1(System.Object,Community.VisualStudio.Toolkit.AbstractSettingsModel{``0})
	//
	// Parameters:
	//   settingsStoreValue:
	//     The value retrieved from the settings store, as an object. This will not be null.
	//
	//   baseOptionModel:
	//     Instance of Community.VisualStudio.Toolkit.AbstractSettingsModel`1. For types requiring
	//     deserialization, methods in this object are used.
	//
	// Type parameters:
	//   TOptMdl:
	//     Type of Community.VisualStudio.Toolkit.AbstractSettingsModel`1.
	//
	// Returns:
	//     settingsStoreValue, converted to the property type.
	protected virtual object ConvertStorageTypeToPropertyType<TOptMdl>(object settingsStoreValue, AbstractSettingsModel<TOptMdl> baseOptionModel) where TOptMdl : AbstractSettingsModel<TOptMdl>
	{
		Type type = PropInfo.PropertyType;
		if (type.IsEnum)
		{
			type = type.GetEnumUnderlyingType();
		}

		switch (DataType)
		{
			case EnSettingDataType.Serialized:
				if (NativeStorageType != EnNativeSettingsType.String)
				{
					InvalidOperationException ex = new($"The SettingDataType of Serialized must be SettingsType.String. Was: {NativeStorageType}");
					Diag.Dug(ex);
					throw ex;
				}

				return baseOptionModel.DeserializeValue((string)settingsStoreValue, type, PropertyName);
			case EnSettingDataType.Legacy:
				if (NativeStorageType != EnNativeSettingsType.String)
				{
					InvalidOperationException ex = new($"The SettingDataType of Legacy must be SettingsType.String. Was: {NativeStorageType}");
					Diag.Dug(ex);
					throw ex;
				}

				return StringUtils.DeserializeBinary64((string)settingsStoreValue, type);
			default:
				if (Converter != null)
				{
					Type type2 = settingsStoreValue.GetType();
					if (NativeStorageType == EnNativeSettingsType.Binary)
					{
						type2 = typeof(byte[]);
						settingsStoreValue = ((MemoryStream)settingsStoreValue).ToArray();
					}

					if (!Converter!.CanConvertFrom(type2))
					{
						InvalidOperationException ex = new("TypeConverter " + Converter!.GetType().FullName + " can not convert from " + type2.Name + " to " + type.FullName + ".");
						Diag.Dug(ex);
						throw ex;
					}

					object obj = Converter!.ConvertFrom(null, CultureInfo.InvariantCulture, settingsStoreValue);
					if (obj == null)
					{
						if (type.IsValueType)
						{
							InvalidOperationException ex = new("TypeConverter " + Converter!.GetType().FullName + " attempt to convert from " + type2.Name + " to " + type.FullName + " returned null for a value type.");
							Diag.Dug(ex);
							throw ex;
						}

						return obj;
					}

					if (!type.IsInstanceOfType(obj))
					{
						InvalidOperationException ex = new("TypeConverter " + Converter!.GetType().FullName + " attempt to convert from " + type2.Name + " to " + type.FullName + " returned incompatible type " + obj.GetType().FullName + ".");
						Diag.Dug(ex);
						throw ex;
					}

					return obj;
				}

				switch (NativeStorageType)
				{
					case EnNativeSettingsType.Int32:

						break;
					case EnNativeSettingsType.Int64:
						if (type == typeof(DateTime))
						{
							return DateTime.FromBinary((long)settingsStoreValue);
						}

						break;
					case EnNativeSettingsType.String:
						if (type == typeof(Color))
						{
							string color = (string)settingsStoreValue;

							if (color[..2] == "##")
								return Color.FromName(color[2..]);
							else
								return Color.FromArgb(Convert.ToInt32(color));
						}
						if (type == typeof(Guid))
						{
							return Guid.Parse((string)settingsStoreValue);
						}

						if (type == typeof(DateTimeOffset))
						{
							return DateTimeOffset.Parse((string)settingsStoreValue, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
						}

						break;
					case EnNativeSettingsType.Binary:
						if (type == typeof(MemoryStream))
						{
							return (MemoryStream)settingsStoreValue;
						}

						if (type == typeof(byte[]))
						{
							return ((MemoryStream)settingsStoreValue).ToArray();
						}

						InvalidCastException ex = new("Can not convert SettingsType.Binary to " + type.FullName + " - property type must be byte[] or MemoryStream.");
						Diag.Dug(ex);
						throw ex;
				}

				if (type.IsInstanceOfType(settingsStoreValue))
				{
					return settingsStoreValue;
				}

				return Convert.ChangeType(settingsStoreValue, type, CultureInfo.InvariantCulture);
		}
	}
}
