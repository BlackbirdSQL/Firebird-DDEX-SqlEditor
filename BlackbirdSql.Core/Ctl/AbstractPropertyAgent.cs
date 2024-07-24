
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using System.Xml;
using BlackbirdSql.Core.Ctl;
using BlackbirdSql.Core.Events;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Sys;
using BlackbirdSql.Sys.Ctl;
using BlackbirdSql.Sys.Enums;

using static BlackbirdSql.Sys.SysConstants;



namespace BlackbirdSql.Core;


// =========================================================================================================
//
//									AbstractPropertyAgent Class
//
/// <summary>
/// The base class for all property based dispatcher and connection classes used in conjunction with
/// PropertySet static classes.
/// A conglomerate base class that supports dynamic addition of properties and parsing functionality for
/// <see cref="DbConnectionStringBuilder"/>, <see cref="Csb"/> and property strings as
/// well as support for the <see cref="IDisposable"/>, <see cref="ICustomTypeDescriptor"/>,
/// <see cref="IDataConnectionProperties"/>, <see cref="IComparable{T}"/>, <see cref="IEquatable{T}"/>,
/// <see cref="INotifyPropertyChanged"/>, <see cref="INotifyDataErrorInfo"/> and
/// <see cref="IWeakEventListener"/> interfaces.
/// </summary>
/// <remarks>
/// Steps for adding an additional property in descendents:
/// 1. Add the core property descriptor support using
/// <see cref="Add(string, Type, object)"/>.
/// 2. Add the property accessor using <see cref="GetProperty(string)"/> and 
/// <see cref="SetProperty(string, object)"/>.
/// 3. If the property type cannot be supported by the built-in types in 
/// <see cref="SetProperty(string, object)"/>, overload the method to support the new type.
/// 4. If (3) applies include a Set_[NewType]Property() method to support the new type using the builtin
/// Set_[Type]Property() methods as a template.
/// 5. If (4) applies add a Will[NewType]Change() method if an existing builtin method cannot be used.  
/// Note: Additional properties will not be included in the connection string or have actual property
/// descriptors. <see cref="ICustomTypeDescriptor"/> members will still need to be overloaded.
/// </remarks>
// =========================================================================================================
public abstract class AbstractPropertyAgent : IBsPropertyAgent
{

	// -------------------------------------------------------
	#region Constructors / Destructors - AbstractPropertyAgent
	// -------------------------------------------------------


	/// <summary>
	/// Universal .ctor
	/// </summary>
	public AbstractPropertyAgent(IBsPropertyAgent rhs, bool generateNewId)
	{
		rhs?.CopyTo(this);

		if (generateNewId)
			_Id = NewId();
	}


	public AbstractPropertyAgent(bool generateNewId) : this(null, generateNewId)
	{
	}


	public AbstractPropertyAgent() : this(null, false)
	{
	}


	public AbstractPropertyAgent(IBsPropertyAgent rhs) : this(rhs, false)
	{
	}



	static AbstractPropertyAgent()
	{
		_Seed = -1L;
	}



	public virtual void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}


	protected virtual bool Dispose(bool disposing)
	{
		if (_Disposed || !disposing)
			return false;

		_Disposed = true;

		return true;
	}



	public abstract IBsPropertyAgent Copy();



	public virtual void CopyTo(IBsPropertyAgent lhs)
	{
		lhs.Clear();

		foreach (KeyValuePair<string, object> pair in _AssignedConnectionProperties)
		{
			((AbstractPropertyAgent)lhs).SetProperty(pair.Key, pair.Value);
		}
	}



	protected static void CreateAndPopulatePropertySet(DescriberDictionary describers = null)
	{
		// This property agent does not have it's own private describers so if called
		// from .ctor, exit.
		if (describers == null)
		{
			if (_Describers == null)
			{
				_Describers = [];

				// Initializers for property sets are held externally for this class
				CorePropertySet.CreateAndPopulatePropertySetFromStatic(_Describers);
			}

			return;
		}

		// Initializers for property sets are held externally for this class

		if (_Describers != null)
			describers?.AddRange(_Describers);
		else
			CorePropertySet.CreateAndPopulatePropertySetFromStatic(describers);
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - AbstractPropertyAgent
	// =========================================================================================================


	// A protected 'this' object lock
	protected object _LockObject = new object();



	protected Csb _Csa;
	protected bool _Disposed;
	private int _EventUpdateCardinal = 0;
	protected int _GetSetCardinal = 0;
	protected bool _GetSetConnectionOpened = false;
	private readonly long _Id;
	protected string _Moniker = null;
	protected bool _ParametersChanged = false;
	private static long _Seed = -1L;

	protected readonly IDictionary<string, object> _AssignedConnectionProperties
		= new Dictionary<string, object>(34, StringComparer.CurrentCultureIgnoreCase);
	protected static DescriberDictionary _Describers = null;
	protected IDictionary<string, string> _ValidationErrors = null;



	protected ConnectionChangedDelegate _ConnectionChangedEvent;
	protected PropertyChangedEventHandler _DispatcherPropertyChangedEvent;
	protected EventHandler<DataErrorsChangedEventArgs> _ErrorsChanged;


	#endregion Fields





	// =========================================================================================================
	#region Property Accessors - AbstractPropertyAgent
	// =========================================================================================================


	public virtual object this[string key]
	{
		get { return GetProperty(key); }
		set { SetProperty(key, value); }
	}


	/// <summary>
	/// Accessor to parsing and assembly of a DbConnectionStringBuilder object 
	/// whose properties represent a <see cref="DbConnection"/>
	/// parameters.
	/// </summary>
	protected Csb Csa
	{
		set { Set_Csb(value); }
		get { return Get_Csb(false); }
	}



	protected virtual DescriberDictionary Describers
	{
		get
		{
			if (_Describers == null)
				CreateAndPopulatePropertySet();

			return _Describers;
		}
	}


	public bool HasErrors => _ValidationErrors != null && _ValidationErrors.Any();


	public long Id => _Id;


	public bool IsComplete
	{
		get
		{
			foreach (Describer describer in Describers.MandatoryKeys)
			{
				bool exists = _AssignedConnectionProperties.TryGetValue(describer.Key, out object value);

				if (describer.Key == "Password" && (!exists || string.IsNullOrEmpty((string)value)))
				{
					exists = _AssignedConnectionProperties.TryGetValue("InMemoryPassword", out value);

					if (exists)
						value = ((SecureString)value).ToReadable();
				}

				if (!exists || string.IsNullOrEmpty((string)value))
					return false;
			}

			return true;
		}
	}


	public bool IsCompletePublic
	{
		get
		{
			foreach (Describer describer in Describers.PublicMandatoryKeys)
			{
				bool exists = _AssignedConnectionProperties.TryGetValue(describer.Key, out object value);

				if (!exists || string.IsNullOrEmpty((string)value))
					return false;
			}

			return true;
		}
	}


	public bool IsExtensible => true;



	public IDictionary<string, string> ValidationErrors
		=> _ValidationErrors ??= new Dictionary<string, string>();


	#endregion Property Accessors





	// =========================================================================================================
	#region External (non-connection) Property Accessors - AbstractPropertyAgent
	// =========================================================================================================


	public Version ClientVersion
	{
		get { return (Version)GetProperty("ClientVersion"); }
		set { SetProperty("ClientVersion", value); }
	}


	public string ConnectionKey
	{
		get { return (string)GetProperty("ConnectionKey"); }
		set { SetProperty("ConnectionKey", value); }
	}

	public string ConnectionName
	{
		get { return (string)GetProperty(C_KeyExConnectionName); }
		set { SetProperty(C_KeyExConnectionName, value); }
	}


	public EnConnectionSource ConnectionSource
	{
		get { return (EnConnectionSource)GetProperty("ConnectionSource"); }
		set { SetProperty("ConnectionSource", value); }
	}


	public string Dataset
	{
		get
		{
			string value = (string)GetProperty("Dataset");
			if (string.IsNullOrWhiteSpace(value))
			{
				value = Database;
				if (!string.IsNullOrWhiteSpace(value))
					value = Path.GetFileNameWithoutExtension(value);
			}
			return value;
		}
		set
		{
			SetProperty("Dataset", value);
		}
	}


	public string DatasetId
	{
		get { return (string)GetProperty(C_KeyExDatasetId); }
		set { SetProperty(C_KeyExDatasetId, value); }
	}


	public string DatasetKey
	{
		get { return (string)GetProperty("DatasetKey"); }
		set { SetProperty("DatasetKey", value); }
	}


	public bool PersistPassword
	{
		get { return (bool)GetProperty("PersistPassword"); }
		set { SetProperty("PersistPassword", value); }
	}


	#endregion External (non-connection) Property Accessors





	// =========================================================================================================
	#region Descriptors Property Accessors - AbstractPropertyAgent
	// =========================================================================================================


	public string Database
	{
		get { return (string)GetProperty("Database"); }
		set { SetProperty("Database", value); }
	}


	public string DataSource
	{
		get { return (string)GetProperty("DataSource"); }
		set { SetProperty("DataSource", value); }
	}


	public string Password
	{
		get { return (string)GetProperty("Password"); }
		set { SetProperty("Password", value); }
	}


	public int Port
	{
		get { return (int)GetProperty("Port"); }
		set { SetProperty("Port", value); }
	}


	public EnServerType ServerType
	{
		get { return (EnServerType)GetProperty("ServerType"); }
		set { SetProperty("ServerType", value); }
	}


	public string UserID
	{
		get { return (string)GetProperty("UserID"); }
		set { SetProperty("UserID", value); }
	}


	#endregion 	Descriptors Property Accessors





	// =========================================================================================================
	#region Event Accessors - AbstractPropertyAgent
	// =========================================================================================================


	public virtual event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged
	{
		add { _ErrorsChanged += value; }
		remove { _ErrorsChanged -= value; }
	}


	public virtual event EventHandler PropertyChanged;

	event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
	{
		add { _DispatcherPropertyChangedEvent += value; }
		remove { _DispatcherPropertyChangedEvent -= value; }
	}


	#endregion Event Accessors





	// =========================================================================================================
	#region Operator Overloads - AbstractPropertyAgent
	// =========================================================================================================


	public static bool operator ==(AbstractPropertyAgent infoA, AbstractPropertyAgent infoB)
	{
		bool result = false;
		bool flag = infoA is null;
		bool flag2 = infoB is null;
		if (flag && flag2)
		{
			result = true;
		}
		else if (!flag && !flag2)
		{
			result = infoA.Equals(infoB);
		}

		return result;
	}

	public static bool operator ==(AbstractPropertyAgent infoA, object infoB)
	{
		bool result = false;
		bool flag = infoA is null;
		bool flag2 = infoB == null;
		if (flag && flag2)
		{
			result = true;
		}
		else if (!flag && !flag2)
		{
			result = infoA.Equals(infoB);
		}

		return result;
	}

	public static bool operator ==(object infoA, AbstractPropertyAgent infoB)
	{
		bool result = false;
		bool flag = infoA == null;
		bool flag2 = infoB is null;
		if (flag && flag2)
		{
			result = true;
		}
		else if (!flag && !flag2)
		{
			result = infoB.Equals(infoA);
		}

		return result;
	}

	public static bool operator !=(AbstractPropertyAgent infoA, AbstractPropertyAgent infoB)
	{
		return !(infoA == infoB);
	}

	public static bool operator !=(AbstractPropertyAgent infoA, object infoB)
	{
		return !(infoA == infoB);
	}

	public static bool operator !=(object infoA, AbstractPropertyAgent infoB)
	{
		return !(infoA == infoB);
	}


	#endregion Operator Overloads





	// =========================================================================================================
	#region Property Getters/Setters - AbstractPropertyAgent
	// =========================================================================================================


	public virtual void Set_Csb(Csb csb)
	{
		Parse(csb);
	}

	public virtual Csb Get_Csb(bool secure)
	{
		if (_Csa != null && !_ParametersChanged)
			return _Csa;

		_ParametersChanged = false;

		_Csa ??= [];

		PopulateConnectionStringBuilder(_Csa, secure);

		_Moniker = null;

		return _Csa;
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the DatsetKey property and sets and registers it if successful.
	/// </summary>
	/// <returns>Returns the value tuple of the derived DatasetKey else null and
	/// a boolean indicating wether or not a connection was opened.</returns>
	// ---------------------------------------------------------------------------------
	public virtual (string, bool) GetSet_DatasetKey()
	{
		if (!IsComplete)
			return (null, false);

		Csb csa = RctManager.ShutdownState ? null : RctManager.CloneRegistered(this);
		if (csa == null)
			return (null, false);

		string datasetKey = csa.DatasetKey;

		if (string.IsNullOrEmpty(datasetKey))
			return (null, false);

		DatasetKey = datasetKey;
		Dataset = csa.Dataset;
		DatasetId = csa.DatasetId;

		return (datasetKey, false);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the ClientVersion property and sets it if successful.
	/// </summary>
	/// <returns>Returns the value tuple of the derived ClientVersion else null and
	/// a boolean indicating wehther or not a connection was opened.</returns>
	// ---------------------------------------------------------------------------------
	public (Version, bool) GetSet_ClientVersion()
	{

		bool opened = false;
		Version version = new(NativeDb.ClientVersion);

		if (version != null)
			ClientVersion = version;

		return (version, opened);
	}



	protected virtual bool Set_StringProperty(string name, string value)
	{
		if (!WillStringPropertyChange(name, value, false))
			return false;

		if (Describers[name].DefaultEqualsOrEmpty(value))
			_AssignedConnectionProperties.Remove(name);
		else
			_AssignedConnectionProperties[name] = value;

		RaisePropertyChanged(name);

		return true;
	}

	protected virtual bool Set_ObjectProperty(string name, object value)
	{
		if (!WillObjectPropertyChange(name, value, false))
			return false;

		if (Describers[name].DefaultEquals(value))
			_AssignedConnectionProperties.Remove(name);
		else
			_AssignedConnectionProperties[name] = value;

		RaisePropertyChanged(name);

		return true;
	}



	protected virtual bool Set_IntProperty(string name, int value)
	{
		if (!WillIntPropertyChange(name, value, false))
			return false;

		if (value == Convert.ToInt32(Describers[name].DefaultValue))
			_AssignedConnectionProperties.Remove(name);
		else
			_AssignedConnectionProperties[name] = value;

		RaisePropertyChanged(name);

		return true;
	}



	protected virtual bool Set_BoolProperty(string name, bool value)
	{
		if (!WillBoolPropertyChange(name, value, false))
			return false;

		if (value == Convert.ToBoolean(Describers[name].DefaultValue))
			_AssignedConnectionProperties.Remove(name);
		else
			_AssignedConnectionProperties[name] = value;

		RaisePropertyChanged(name);

		return true;
	}



	protected virtual bool Set_EnumProperty(string name, object enumValue)
	{
		// We will always store as int32.

		object value = enumValue;

		if (enumValue != null)
		{
			if (enumValue is Enum enumType)
				value = enumType;
			else if (value is string s)
				value = Enum.Parse(Describers[name].PropertyType, s, true);
			else
				value = (Enum)value;
		}


		if (!WillEnumPropertyChange(name, value, false))
			return false;

		if (enumValue == null || Convert.ToInt32(value) == Convert.ToInt32(Describers[name].DefaultValue))
			_AssignedConnectionProperties.Remove(name);
		else
			_AssignedConnectionProperties[name] = value;

		RaisePropertyChanged(name);

		return true;
	}



	protected virtual bool Set_ByteProperty(string name, object value)
	{
		if (!WillBytePropertyChange(name, value, false))
			return false;

		// We will always store as string.
		if (Cmd.IsNullValue(value))
			_AssignedConnectionProperties.Remove(name);
		else if (value.GetType() == typeof(byte[]))
			_AssignedConnectionProperties[name] = Encoding.Default.GetString((byte[])value);
		else
			_AssignedConnectionProperties[name] = (string)value;

		RaisePropertyChanged(name);

		return true;
	}



	protected virtual bool Set_PasswordProperty(string name, string value)
	{
		bool changed = WillPasswordPropertyChange(name, value, false);

		if (string.IsNullOrEmpty(value))
		{
			_AssignedConnectionProperties.Remove("InMemoryPassword");
			_AssignedConnectionProperties["InMemoryPassword"] = string.Empty;
		}
		else
		{
			_AssignedConnectionProperties["InMemoryPassword"] = value.ToSecure();
			_AssignedConnectionProperties.Remove("Password");
		}

		if (changed) RaisePropertyChanged(name);

		return changed;
	}



	protected virtual bool Set_VersionProperty(string name, Version value)
	{
		if (!WillStringPropertyChange(name, value?.ToString(), false))
			return false;

		if (Describers[name].DefaultEqualsOrEmptyString(value))
			_AssignedConnectionProperties.Remove(name);
		else
			_AssignedConnectionProperties[name] = (Version)value.Clone();

		RaisePropertyChanged(name);

		return true;
	}


	#endregion Property Getters/Setters





	// =========================================================================================================
	#region Accessor Methods - AbstractPropertyAgent
	// =========================================================================================================


	public virtual object GetProperty(string name)
	{
		if (name == "Password" && _AssignedConnectionProperties.TryGetValue("InMemoryPassword", out object secure))
			return ((SecureString)secure).ToReadable();


		if (_AssignedConnectionProperties.TryGetValue(name, out object value))
			return value;

		if (TryGetSetDerivedProperty(name, out value))
			return value;

		if (!TryGetDefaultValue(name, out object defaultValue))
		{
			ArgumentException ex = new($"Could not find a default value for property {name}.");
			Diag.Dug(ex);
			throw ex;
		}

		return defaultValue;
	}


	public virtual bool Isset(string property)
	{
		return _AssignedConnectionProperties.ContainsKey(property);
	}


	protected virtual bool SetProperty(string name, object value)
	{

		bool changed;
		string propertyTypeName;


		if (GetPropertyType(name).IsSubclassOf(typeof(Enum)))
			propertyTypeName = "enum";
		else
			propertyTypeName = GetPropertyTypeName(name).ToLower();


		switch (propertyTypeName)
		{
			case "password":
				changed = Set_PasswordProperty(name, Convert.ToString(value));
				break;
			case "version":
				changed = Set_VersionProperty(name, (Version)value);
				break;
			case "enum":
				changed = Set_EnumProperty(name, value);
				break;
			case "int32":
				changed = Set_IntProperty(name, Convert.ToInt32(value));
				break;
			case "boolean":
				changed = Set_BoolProperty(name, Convert.ToBoolean(value));
				break;
			case "byte[]":
				changed = Set_ByteProperty(name, value);
				break;
			case "string":
				changed = Set_StringProperty(name, Convert.ToString(value));
				break;
			case "object":
				changed = Set_ObjectProperty(name, value);
				break;
			default:
				ArgumentException ex = new($"Property type {propertyTypeName} for property {name} is not supported");
				Diag.Dug(ex);
				throw ex;
		}


		return changed;
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Attempts to derive a value for a property and sets it if successful.
	/// </summary>
	/// <param name="name">The property name</param>
	/// <param name="value">The derived value or fallback value else null</param>
	/// <returns>
	/// Returns true if a useable value was derived, even if the value could not be used
	/// to set the property, else false if no useable property could be derived. An
	/// example of a useable derived property that will not be used to set the property
	/// is the ServerError Icon property when a connection cannot be established.
	/// </returns>
	/// <remarks>
	/// This method may be recursively called because a derived property may be dependent
	/// on other derived properties, any of which may require access to an open database
	/// connection. Each get/set method returns a boolean indicating whether or not it
	/// opened the connection. We track that and only close the connection (if necessary)
	/// when we exit the recursion to avoid repetitively opening and closing the connection. 
	/// </remarks>
	// ---------------------------------------------------------------------------------
	protected virtual bool TryGetSetDerivedProperty(string name, out object value)
	{
		bool connectionOpened = false;
		bool result = false;

		_GetSetCardinal++;

		try
		{
			switch (name)
			{
				case "DatasetKey":
					(value, connectionOpened) = GetSet_DatasetKey();
					result = true;
					break;
				case "ClientVersion":
					(value, connectionOpened) = GetSet_ClientVersion();
					result = value != null;
					break;
				default:
					value = null;
					break;
			}
		}
		finally
		{
			_GetSetConnectionOpened |= connectionOpened;
			_GetSetCardinal--;

			if (_GetSetCardinal == 0 && _GetSetConnectionOpened)
			{
				CloseConnection();


				_GetSetConnectionOpened = false;
			}
		}

		return result;

	}



	protected virtual bool WillPasswordPropertyChange(string name, string newValue, bool removing)
	{
		bool isSet = Isset(name);
		bool isSetInMemory = Isset("InMemoryPassword");

		if (removing)
		{
			if (!isSet && !isSetInMemory)
				return false;

			if (isSet)
				_AssignedConnectionProperties.Remove(name);
			if (isSetInMemory)
				_AssignedConnectionProperties.Remove("InMemoryPassword");

			_ParametersChanged = true;
			return true;
		}


		bool changed;
		string currValue = isSetInMemory
			? ((SecureString)_AssignedConnectionProperties["InMemoryPassword"]).ToReadable()
			: (isSet ? (string)_AssignedConnectionProperties[name] : null);

		if (Cmd.NullEquality(newValue, currValue) <= EnNullEquality.Equal)
		{
			changed = Cmd.NullEquality(newValue, currValue) == EnNullEquality.UnEqual;

			if (changed && IsParameter(name))
				_ParametersChanged = true;

			return changed;
		}

		changed = currValue != newValue;
		_ParametersChanged = true;

		return changed;
	}




	#endregion Accessor Methods





	// =========================================================================================================
	#region Methods - AbstractPropertyAgent
	// =========================================================================================================


	public void Add(string propertyName)
	{
		Add(propertyName, typeof(object), null);
	}


	public void Add(string name, string parameter, Type propertyType, object defaultValue = null,
		bool isParameter = false, bool isAdvanced = true, bool isPublic = true, bool isMandatory = false)
	{
		if (!IsExtensible)
		{
			NotSupportedException ex = new(Properties.Resources.Properties_NotExtensible);
			Diag.Dug(ex);
			throw ex;
		}

		Describers.Add(name, parameter, propertyType, defaultValue, isParameter,
			isAdvanced, isPublic, isMandatory);
	}



	public void Add(string name, Type propertyType, object defaultValue = null,
		bool isParameter = false, bool isAdvanced = true, bool isPublic = true, bool isMandatory = false)
	{
		Add(name, null, propertyType, defaultValue, isParameter,
			isAdvanced, isPublic, isMandatory);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Checks whether or not property objects are equivalent
	/// </summary>
	/// <remarks>
	/// We consider property agents equivalent if they will produce the same results.
	/// The properties that determine this equivalency are defined in
	/// <see cref="EquivalencyKeys"/>.
	/// </remarks>
	// ---------------------------------------------------------------------------------
	public virtual bool AreEquivalent(IBsPropertyAgent other)
	{
		EnNullEquality nullEquality;
		string typeName;
		Type type;
		object value1, value2;

		// If neither are complete return true
		if (!IsComplete && !other.IsComplete)
			return true;

		// Is one is complete and another not return false
		if (IsComplete != other.IsComplete)
			return false;


		try
		{
			// Keep it simple. Loop through the equivalency keys and compare.
			// If it's not in the other or null use default



			foreach (Describer describer in Describers.EquivalencyKeys)
			{

				value1 = this[describer.Name];
				value2 = other[describer.Name];

				nullEquality = Cmd.NullEquality(value1, value2);

				if (nullEquality == EnNullEquality.Equal)
					continue;

				if (nullEquality == EnNullEquality.UnEqual)
					return false;


				type = describer.PropertyType;

				if (type.IsSubclassOf(typeof(Enum)))
					typeName = "int";
				else
					typeName = type.Name;


				switch (typeName)
				{
					case "int":
						if ((int)value1 != (int)value2)
							return false;
						break;
					case "boolean":
						if ((bool)value1 != (bool)value2)
							return false;
						break;
					case "string":
						if (!((string)value1).Equals((string)value2, StringComparison.CurrentCultureIgnoreCase))
							return false;
						break;
					case "byte[]":
						return Compare(value1, value2) == 0;
					default:
						ArgumentException ex = new($"Property type {typeName} for property {describer.Name} is not supported");
						Diag.Dug(ex);
						throw ex;
				}

			}

		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			return false;
		}

		return true;
	}



	public virtual void Clear()
	{
		_AssignedConnectionProperties.Clear();
		_Csa = null;
		_Moniker = null;
	}



	public virtual void ClearAllErrors()
	{
		if (_ValidationErrors == null)
			return;

		foreach (string key in _ValidationErrors.Keys)
		{
			_ValidationErrors.Remove(key);
			_ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(key));
		}
	}



	public abstract bool CloseConnection();


	public static int Compare(object lhs, object rhs)
	{
		if (lhs == null)
			return rhs == null ? 0 : -1;

		if (rhs == null)
			return 1;

		int result;

		if (lhs.GetType().IsEnum)
		{
			result = (int)lhs == (int)rhs ? 1
				: ((int)lhs > (int)rhs ? 1 : -1);
		}
		else if (lhs is SecureString secureValue)
		{
			result = string.CompareOrdinal(secureValue.ToReadable(),
				((SecureString)rhs).ToReadable());
		}
		else if (lhs is byte[] byteValue)
		{
			result = Compare(byteValue, rhs as byte[]);
		}
		else if (lhs is bool boolValue)
		{
			if (boolValue)
				result = ((bool)rhs) ? 0 : -1;
			else
				result = ((bool)rhs) ? 1 : 0;
		}
		else if (lhs is long longValue)
		{
			result = longValue == (long)rhs ? 0 : (longValue > (long)rhs ? 1 : -1);
		}
		else if (lhs is int || lhs.GetType().IsSubclassOf(typeof(Enum)))
		{
			int intValue = (int)lhs;
			result = intValue == (int)rhs ? 0 : (intValue > (int)rhs ? 1 : -1);
		}
		else if (lhs is string stringValue)
		{
			result = string.CompareOrdinal(stringValue, (string)rhs);
		}
		else
		{
			result = lhs.Equals(rhs) ? 0 : 1;
		}

		return result;

	}



	public static int Compare(byte[] lhs, byte[] rhs)
	{
		if (lhs.Length == 0)
			return rhs.Length == 0 ? 0 : -1;

		if (rhs.Length == 0)
			return 1;

		// Loop all values and compare
		for (int i = 0; i < lhs.Length; i++)
		{
			if (i == rhs.Length)
				return 1;

			int result = lhs[i].CompareTo(rhs[i]);

			if (result != 0)
				return result;
		}

		return rhs.Length > lhs.Length ? -1 : 0;
	}



	public virtual int CompareTo(IBsPropertyAgent rhs)
	{
		if (rhs == null)
			return 1;
		AbstractPropertyAgent rhsObject = rhs as AbstractPropertyAgent;

		foreach (KeyValuePair<string, object> pair in _AssignedConnectionProperties)
		{
			if (!rhsObject._AssignedConnectionProperties.TryGetValue(pair.Key, out object value))
				return 1;

			int result = Compare(pair.Value, value);
			if (result != 0)
				return result;
		}

		foreach (KeyValuePair<string, object> pair in rhsObject._AssignedConnectionProperties)
		{
			if (!_AssignedConnectionProperties.ContainsKey(pair.Key))
				return -1;
		}

		return 0;
	}



	public virtual bool Contains(string propertyName)
	{
		return _AssignedConnectionProperties.ContainsKey(propertyName);
	}



	public virtual bool Equals(IBsPropertyAgent rhs)
	{
		return CompareTo(rhs) == 0;
	}



	public override bool Equals(object obj)
	{
		bool result = false;


		if (obj is IBsPropertyAgent propertyAgent)
			result = Id == propertyAgent.Id;

		return result;
	}



	public virtual AttributeCollection GetAttributes()
	{
		return TypeDescriptor.GetAttributes(Csa, noCustomTypeDesc: true);
	}



	public virtual string GetClassName()
	{
		return TypeDescriptor.GetClassName(Csa, noCustomTypeDesc: true);
	}



	public virtual string GetComponentName()
	{
		return TypeDescriptor.GetComponentName(Csa, noCustomTypeDesc: true);
	}



	public virtual TypeConverter GetConverter()
	{
		return TypeDescriptor.GetConverter(Csa, noCustomTypeDesc: true);
	}



	public virtual EventDescriptor GetDefaultEvent()
	{
		return TypeDescriptor.GetDefaultEvent(Csa, noCustomTypeDesc: true);
	}



	public virtual System.ComponentModel.PropertyDescriptor GetDefaultProperty()
	{
		return TypeDescriptor.GetDefaultProperty(Csa, noCustomTypeDesc: true);
	}



	public virtual Describer GetDescriber(string key)
	{
		if (string.IsNullOrEmpty(key))
			return null;

		return Describers[key];
	}



	public virtual object GetEditor(Type editorBaseType)
	{
		return TypeDescriptor.GetEditor(Csa, editorBaseType, noCustomTypeDesc: true);
	}



	public virtual IEnumerable GetErrors(string propertyName)
	{
		if (string.IsNullOrWhiteSpace(propertyName.Trim())
			|| _ValidationErrors == null || !_ValidationErrors.ContainsKey(propertyName))
		{
			return Enumerable.Empty<string>();
		}

		return (from p in _ValidationErrors
				where p.Key.Equals(propertyName)
				select p.Value).ToArray();
	}



	public override int GetHashCode()
	{
		return (int)(Id % int.MaxValue);
	}



	public virtual Describer GetParameterDescriber(string parameter)
	{
		return Describers.GetParameterDescriber(parameter);
	}


	public virtual PropertyDescriptorCollection GetProperties()
	{
		return TypeDescriptor.GetProperties(Csa);
	}



	public virtual PropertyDescriptorCollection GetProperties(Attribute[] attributes)
	{
		return TypeDescriptor.GetProperties(Csa, attributes);
	}



	public virtual object GetPropertyOwner(System.ComponentModel.PropertyDescriptor pd)
	{
		return Csa;
	}



	public virtual Type GetPropertyType(string name)
	{
		return Describers[name].PropertyType;
	}



	public virtual string GetPropertyTypeName(string name)
	{
		if (name.ToLower() == "password")
			return "password";

		return Describers[name].PropertyType.Name;
	}


	public virtual Describer GetSynonymDescriber(string synonym)
	{
		return Describers.GetSynonymDescriber(synonym);
	}




	protected abstract (Version, bool) GetServerVersion();



	public virtual bool IsParameter(string propertyName)
	{
		if (propertyName.Equals(SysConstants.C_KeyPassword, StringComparison.OrdinalIgnoreCase)
			|| propertyName.Equals(SysConstants.C_KeyExInMemoryPassword, StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}

		return Describers.IsParameter(propertyName);
	}



	public virtual void LoadFromStream(XmlReader reader)
	{
		XmlTextReader xmlTextReader = new XmlTextReader(new StringReader(reader.ReadOuterXml()))
		{
			DtdProcessing = DtdProcessing.Prohibit
		};

		while (xmlTextReader.Read())
		{
			if (xmlTextReader.NodeType == XmlNodeType.Element && xmlTextReader.LocalName == "DatasetKey")
			{
				DatasetKey = xmlTextReader.ReadString();
				break;
			}
		}
		string name;

		while (xmlTextReader.Read())
		{
			if (xmlTextReader.NodeType == XmlNodeType.Element && (name = xmlTextReader.LocalName) != "")
			{
				Type type = GetPropertyType(name);
				object obj;

				string value = xmlTextReader.ReadString();

				if (type == typeof(long))
				{
					obj = Convert.ToInt64(value);
				}
				else if (type == typeof(int))
				{
					obj = Convert.ToInt32(value);
				}
				else if (type == typeof(bool))
				{
					obj = Convert.ToBoolean(value);
				}
				// else if (type == typeof(byte[]))
				// {
				//	obj = Encoding.Default.GetBytes(value);
				// }
				else if (type == typeof(Version))
				{
					obj = new Version(value);
				}
				else
				{
					obj = value;
				}


				SetProperty(name, obj);
			}
		}

	}



	private static long NewId()
	{
		_Seed++;

		return Interlocked.Add(ref _Seed, 0L);
	}



	public virtual void Parse(Csb csb)
	{
		bool changed = false;
		Describer describer;

		Clear();

		foreach (KeyValuePair<string, object> pair in csb)
		{
			string lckey = pair.Key.ToLower();

			describer = Describers.GetSynonymDescriber(pair.Key);

			if (describer == null)
			{
				// It could be a connection dataset which includes DatasetKey, DatasetId etc. so don't
				// report an exception if it is.
				NotSupportedException ex = new($"Connection parameter '{pair.Key}' has no descriptor property configured.");

				if (Csb.Describers[pair.Key] == null)
				{
					Diag.Debug(ex);
				}
				else
				{
					Diag.Expected(ex);
				}
				continue;
			}

			if (SetProperty(describer.Name, pair.Value))
				changed = true;
		}

		if (changed && !_ParametersChanged)
		{
			_ParametersChanged = true;
			_Csa = null;
			_Moniker = null;
		}

	}



	public virtual void Parse(string s)
	{
		Csb csb = Csa;

		csb.Clear();
		csb.ConnectionString = s;

		Parse(csb);
	}



	public void PopulateConnectionStringBuilder(Csb csb, bool secure)
	{
		csb.Clear();
				
		foreach (KeyValuePair<string, object> pair in _AssignedConnectionProperties)
		{
			if (secure && (pair.Key == SysConstants.C_KeyPassword || pair.Key == SysConstants.C_KeyExInMemoryPassword))
			{
				continue;
			}

			if (pair.Key == SysConstants.C_KeyExInMemoryPassword)
			{
				csb[SysConstants.C_KeyPassword] = GetProperty("Password");
				continue;
			}

			if (Cmd.IsNullValueOrEmpty(pair.Value))
				continue;

			csb[pair.Key] = pair.Value;
		}

	}



	public virtual void Remove(string propertyName)
	{
		if (propertyName == null)
		{
			ArgumentNullException ex = new("propertyName");
			Diag.Dug(ex);
			throw ex;
		}

		if (!IsExtensible)
		{
			NotSupportedException ex = new(Core.Properties.Resources.Properties_NotExtensible);
			Diag.Dug(ex);
			throw ex;
		}

		if (Isset(propertyName))
		{
			if (IsParameter(propertyName))
				_ParametersChanged = true;

			RaisePropertyChanged(propertyName);
		}

		Describers.Remove(propertyName);
	}



	public virtual void Reset()
	{
		if (_AssignedConnectionProperties.Count == 0)
			return;

		if (!_ParametersChanged)
		{
			foreach (string name in _AssignedConnectionProperties.Keys)
			{
				if (IsParameter(name))
				{
					_ParametersChanged = true;
					break;
				}
			}
		}

		Clear();

		RaisePropertyChanged(string.Empty);
	}



	public virtual void Reset(string propertyName)
	{
		if (!Isset(propertyName))
			return;

		if (IsParameter(propertyName))
			_ParametersChanged = true;
		_AssignedConnectionProperties.Remove(propertyName);

		RaisePropertyChanged(propertyName);
	}



	public virtual void ResetConnectionInfo()
	{
		Reset();
		ClearAllErrors();
	}



	public virtual void SaveToStream(XmlWriter writer, bool unsecured)
	{
		writer.WriteStartElement("ConnectionInformation");
		writer.WriteElementString("DatasetKey", null, DatasetKey);

		string key;
		object value;

		foreach (KeyValuePair<string, object> pair in _AssignedConnectionProperties)
		{
			if (!unsecured && (pair.Key == "Password" || pair.Key == "InMemoryPassword"))
				continue;

			if (Describers.IsAdvanced(pair.Key))
				continue;

			if (pair.Key == "InMemoryPassword")
			{
				key = "Password";
				value = ((SecureString)pair.Value).ToReadable();
			}
			else
			{
				key = pair.Key;
				value = pair.Value ?? string.Empty;
			}

			writer.WriteElementString(key, null, value.ToString());
		}

		writer.WriteStartElement("AdvancedOptions", null);

		foreach (KeyValuePair<string, object> pair in _AssignedConnectionProperties)
		{
			if (!Describers.IsAdvanced(pair.Key))
				continue;

			writer.WriteElementString(pair.Key, null, pair.Value.ToString());
		}

		writer.WriteEndElement();
		writer.WriteEndElement();
	}



	protected void Set(string connectionString)
	{
		Parse(connectionString);
	}



	public abstract void Test();



	public virtual string ToDisplayString()
	{
		DbConnectionStringBuilder csb = Csa;

		if (csb.ContainsKey("Password"))
		{
			_ParametersChanged = true;
			csb[SysConstants.C_KeyPassword] = "*****";
		}

		return csb.ConnectionString;
	}



	public virtual string ToFullString()
	{
		return ToFullString(false);
	}



	private string ToFullString(bool secure)
	{
		return Get_Csb(secure).ConnectionString;
	}



	private bool TryGetDefaultValue(string name, out object value)
	{
		if (!Describers.TryGetDefaultValue(name, out value))
		{
			ArgumentException ex = new($"Could not find a default value for descriptor {name}.");
			Diag.Dug(ex);
			throw ex;
		}

		return true;
	}



	protected bool WillObjectPropertyChange(string name, object newValue, bool removing)
	{
		bool isSet = Isset(name);

		if (removing)
		{
			if (!isSet)
				return false;

			_AssignedConnectionProperties.Remove(name);

			if (IsParameter(name))
				_ParametersChanged = true;

			return true;
		}


		bool changed;
		object assignedValue = isSet ? _AssignedConnectionProperties[name] : null;
		object currValue = isSet 
			? assignedValue
			: (Cmd.IsNullValue(newValue)
				? newValue
				: Describers[name].DefaultValue);

		if (Cmd.NullEquality(newValue, currValue) <= EnNullEquality.Equal)
		{
			changed = Cmd.NullEquality(newValue, currValue) == EnNullEquality.UnEqual;

			if (changed && IsParameter(name))
				_ParametersChanged = true;

			return changed;
		}

		changed = Equals(newValue, currValue);

		if (changed && IsParameter(name))
			_ParametersChanged = true;

		return changed;
	}



	protected virtual bool WillIntPropertyChange(string name, int newValue, bool removing)
	{
		bool isSet = Isset(name);

		if (removing)
		{
			if (!isSet)
				return false;

			_AssignedConnectionProperties.Remove(name);

			if (IsParameter(name))
				_ParametersChanged = true;

			return true;
		}

		bool changed;
		int defaultValue = Convert.ToInt32(Describers[name].DefaultValue);
		int currValue = isSet ? Convert.ToInt32(_AssignedConnectionProperties[name]) : defaultValue;

		changed = newValue != currValue;

		if (changed && IsParameter(name))
			_ParametersChanged |= changed;

		return changed;
	}



	protected virtual bool WillEnumPropertyChange(string name, object newValue, bool removing)
	{
		bool isSet = Isset(name);

		if (removing)
		{
			if (!isSet)
				return false;

			_AssignedConnectionProperties.Remove(name);

			if (IsParameter(name))
				_ParametersChanged = true;

			return true;
		}


		bool changed;
		int defaultValue = Convert.ToInt32(Describers[name].DefaultValue);
		int currValue = isSet ? Convert.ToInt32(_AssignedConnectionProperties[name]) : defaultValue;
		int value = newValue != null ? Convert.ToInt32(newValue) : defaultValue;

		changed = value != currValue;

		if (changed && IsParameter(name))
			_ParametersChanged |= changed;

		return changed;
	}



	protected virtual bool WillBoolPropertyChange(string name, bool newValue, bool removing)
	{
		bool isSet = Isset(name);

		if (removing)
		{
			if (!isSet)
				return false;

			_AssignedConnectionProperties.Remove(name);

			if (IsParameter(name))
				_ParametersChanged = true;

			return true;
		}


		bool changed;
		bool defaultValue = Convert.ToBoolean(Describers[name].DefaultValue);
		bool currValue = isSet ? Convert.ToBoolean(_AssignedConnectionProperties[name]) : defaultValue;

		changed = newValue != currValue;

		if (changed && IsParameter(name))
			_ParametersChanged |= changed;

		return changed;
	}



	protected virtual bool WillBytePropertyChange(string name, object newObjValue, bool removing)
	{
		bool isSet = Isset(name);

		if (removing)
		{
			if (!isSet)
				return false;

			_AssignedConnectionProperties.Remove(name);

			if (IsParameter(name))
				_ParametersChanged = true;

			return true;
		}

		string newValue;

		if (Cmd.IsNullValue(newObjValue))
		{
			newValue = (string)newObjValue;
		}
		else
		{
			newValue = newObjValue.GetType() == typeof(byte[])
				? Encoding.Default.GetString((byte[])newObjValue)
				: (string)newObjValue;
		}

		bool changed;
		object assignedValue = isSet ? _AssignedConnectionProperties[name] : null;
		object obj = isSet
			? assignedValue
			: (Cmd.IsNullValue(newValue)
				? newValue
				: Describers[name].DefaultValue);

		string currValue = obj?.ToString();

		if (Cmd.NullEquality(newValue, currValue) <= EnNullEquality.Equal)
		{
			changed = Cmd.NullEquality(newValue, currValue) == EnNullEquality.UnEqual;

			if (changed && IsParameter(name))
				_ParametersChanged = true;

			return changed;
		}

		changed = !newValue.Equals(currValue);

		if (changed && IsParameter(name))
			_ParametersChanged |= changed;

		return changed;
	}



	protected virtual bool WillStringPropertyChange(string name, string newValue, bool removing)
	{
		bool isSet = Isset(name);

		if (removing)
		{
			if (!isSet)
				return false;

			_AssignedConnectionProperties.Remove(name);

			if (IsParameter(name))
				_ParametersChanged = true;

			return true;
		}


		bool changed;
		object assignedValue = isSet ? _AssignedConnectionProperties[name] : null;
		object obj = isSet
			? assignedValue
			: (Cmd.IsNullValue(newValue)
				? newValue
				: Describers[name].DefaultValue);

		string currValue = obj?.ToString();

		if (Cmd.NullEquality(newValue, currValue) <= EnNullEquality.Equal)
		{
			changed = Cmd.NullEquality(newValue, currValue) == EnNullEquality.UnEqual;

			if (changed && IsParameter(name))
				_ParametersChanged = true;

			return changed;
		}

		changed = !newValue.Equals(currValue);

		if (changed && IsParameter(name))
			_ParametersChanged |= changed;

		return changed;
	}


	#endregion Methods





	// =========================================================================================================
	#region Event Methods - AbstractPropertyAgent
	// =========================================================================================================


	public virtual EventDescriptorCollection GetEvents()
	{
		return TypeDescriptor.GetEvents(Csa, noCustomTypeDesc: true);
	}



	public virtual EventDescriptorCollection GetEvents(Attribute[] attributes)
	{
		return TypeDescriptor.GetEvents(Csa, attributes, noCustomTypeDesc: true);
	}



	private void RaisePropertyChanged(string propertyName)
	{
		if (!EventUpdateEnter())
			return;

		try
		{
			OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
		}
		finally
		{
			EventUpdateExit();
		}
	}



	protected void RaisePropertyChanged<TKey, TValue>(Dictionary<TKey, TValue> dictionary,
		TKey key, params string[] propertiesToExclude) where TValue : IEnumerable<string>
	{
		if (!dictionary.TryGetValue(key, out var value))
		{
			return;
		}

		foreach (string item in value.Except(propertiesToExclude))
		{
			RaisePropertyChanged(item);
		}
	}


	#endregion Event Methods





	// =========================================================================================================
	#region Event Handling - AbstractPropertyAgent
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Increments the <see cref="_EventUpdateCardinal"/> counter when execution
	/// enters an update section that requires hands of.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected bool EventUpdateEnter(bool increment = true, bool force = false)
	{
		lock (_LockObject)
		{
			if (_EventUpdateCardinal != 0 && !force)
				return false;

			if (increment)
				_EventUpdateCardinal++;
		}

		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Decrements the <see cref="_EventUpdateCardinal"/> counter that was previously
	/// incremented by <see cref="EventUpdateEnter"/>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected void EventUpdateExit()
	{
		lock (_LockObject)
		{
			if (_EventUpdateCardinal <= 0)
			{
				ApplicationException ex = new($"Attempt to exit event when not in an event. _EventCardinal: {_EventUpdateCardinal}");
				Diag.Dug(ex);
				throw ex;
			}
			_EventUpdateCardinal--;
		}
	}



	protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
	{
		bool all = string.IsNullOrEmpty(e.PropertyName);

		Describer describer = all ? null : GetDescriber(e.PropertyName);

		if (all || (describer != null && describer.IsEquivalency))
		{
			_Moniker = null;
		}

		PropertyChanged?.Invoke(this, e);
	}



	public virtual bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
	{
		if (!ReceiveWeakEvent(managerType, sender, e))
		{
			Diag.Dug(new ArgumentException("Weak event was not handled"));
			return false;
		}

		return true;
	}


	#endregion Event Handling

}
