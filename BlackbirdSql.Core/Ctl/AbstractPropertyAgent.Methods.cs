

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using System.Xml;
using BlackbirdSql.Core.Ctl;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Core.Properties;
using BlackbirdSql.Sys;
using BlackbirdSql.Sys.Ctl;
using BlackbirdSql.Sys.Enums;

namespace BlackbirdSql.Core;

// =========================================================================================================
//
//									AbstractPropertyAgent Class - Methods
//
// The base class for all property based dispatcher and connection classes used in conjunction with
// PropertySet static classes.
// =========================================================================================================
public abstract partial class AbstractPropertyAgent : IBPropertyAgent
{

	// ---------------------------------------------------------------------------------
	#region Fields - AbstractPropertyAgent
	// ---------------------------------------------------------------------------------

	protected static DescriberDictionary _Describers = null;

	protected DbConnection _DataConnection = null;
	protected DbConnectionStringBuilder _ConnectionStringBuilder;
	protected string _DatasetKey = null;
	protected long _Id;
	protected bool _IsDisposed;
	// A protected 'this' object lock
	protected object _LockObject = new object();
	protected static long _Seed = -1L;
	protected object _Null = new();

	protected readonly IDictionary<string, object> _AssignedConnectionProperties
		= new Dictionary<string, object>(34, StringComparer.CurrentCultureIgnoreCase);

	protected IDictionary<string, string> _ValidationErrors = null;


	#endregion Fields





	// =========================================================================================================
	#region Constructors / Destructors - AbstractPropertyAgent
	// =========================================================================================================


	/// <summary>
	/// Universal .ctor
	/// </summary>
	public AbstractPropertyAgent(IBPropertyAgent rhs, bool generateNewId)
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


	public AbstractPropertyAgent(IBPropertyAgent rhs) : this(rhs, false)
	{
	}



	static AbstractPropertyAgent()
	{
		_Seed = -1L;
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




	public virtual void Dispose()
	{
		Dispose(isDisposing: true);
		GC.SuppressFinalize(this);
	}

	public virtual void Dispose(bool isDisposing)
	{
		if (!_IsDisposed && isDisposing)
		{
			_IsDisposed = true;
		}
	}


	#endregion Constructors / Destructors





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
	public virtual bool AreEquivalent(IBPropertyAgent other)
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



	public virtual int CompareTo(IBPropertyAgent rhs)
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



	public abstract IBPropertyAgent Copy();


	public virtual void CopyTo(IBPropertyAgent lhs)
	{
		lhs.Clear();

		foreach (KeyValuePair<string, object> pair in _AssignedConnectionProperties)
		{
			lhs.SetProperty(pair.Key, pair.Value);
		}
	}



	public virtual DbCommand CreateCommand(string cmd = null)
	{
		NotImplementedException ex = new("CreateCommand");
		Diag.Dug(ex);
		throw ex;
	}



	public virtual bool Equals(IBPropertyAgent rhs)
	{
		return CompareTo(rhs) == 0;
	}



	public override bool Equals(object obj)
	{
		bool result = false;


		if (obj is IBPropertyAgent propertyAgent)
			result = Id == propertyAgent.Id;

		return result;
	}



	public virtual AttributeCollection GetAttributes()
	{
		return TypeDescriptor.GetAttributes(ConnectionStringBuilder, noCustomTypeDesc: true);
	}


	public virtual string GetClassName()
	{
		return TypeDescriptor.GetClassName(ConnectionStringBuilder, noCustomTypeDesc: true);
	}


	public virtual string GetComponentName()
	{
		return TypeDescriptor.GetComponentName(ConnectionStringBuilder, noCustomTypeDesc: true);
	}

	public virtual TypeConverter GetConverter()
	{
		return TypeDescriptor.GetConverter(ConnectionStringBuilder, noCustomTypeDesc: true);
	}



	public virtual EventDescriptor GetDefaultEvent()
	{
		return TypeDescriptor.GetDefaultEvent(ConnectionStringBuilder, noCustomTypeDesc: true);
	}



	public virtual System.ComponentModel.PropertyDescriptor GetDefaultProperty()
	{
		return TypeDescriptor.GetDefaultProperty(ConnectionStringBuilder, noCustomTypeDesc: true);
	}



	public virtual object GetEditor(Type editorBaseType)
	{
		return TypeDescriptor.GetEditor(ConnectionStringBuilder, editorBaseType, noCustomTypeDesc: true);
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



	public virtual PropertyDescriptorCollection GetProperties()
	{
		return TypeDescriptor.GetProperties(ConnectionStringBuilder);
	}

	public virtual PropertyDescriptorCollection GetProperties(Attribute[] attributes)
	{
		return TypeDescriptor.GetProperties(ConnectionStringBuilder, attributes);
	}



	public virtual Describer GetParameterDescriber(string parameter)
	{
		return Describers.GetParameterDescriber(parameter);
	}

	public virtual Describer GetSynonymDescriber(string synonym)
	{
		return Describers.GetSynonymDescriber(synonym);
	}



	public virtual object GetPropertyOwner(System.ComponentModel.PropertyDescriptor pd)
	{
		return ConnectionStringBuilder;
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



	public (Version, bool) GetServerVersion(IDbConnection connection)
	{
		if (connection is not DbConnection dbConnection)
			return (null, false);

		bool opened = false;

		if (dbConnection.State != ConnectionState.Open)
		{
			try
			{
				dbConnection.Open();
			}
			catch
			{ }

			if (dbConnection.State != ConnectionState.Open)
				return (null, false);

			opened = true;
		}


		Version version = new(dbConnection.ServerVersion);

		return (version, opened);
	}



	public virtual bool IsParameter(string propertyName)
	{
		if (propertyName.ToLower() == SysConstants.C_KeyPassword.ToLower() || propertyName.ToLower() == SysConstants.C_KeyExInMemoryPassword.ToLower())
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



	public virtual void Parse(DbConnectionStringBuilder csb)
	{
		bool changed = false;
		Describer describer;

		// Tracer.Trace(GetType(), "Parse()", "csa.ConnectionString: {0}", csb.ConnectionString);

		Clear();

		foreach (KeyValuePair<string, object> pair in csb)
		{
			string lckey = pair.Key.ToLower();
			// if (lckey == "displaymember") // || lckey == "datasetkey" || lckey == "dataset")
			//	continue;

			describer = Describers.GetSynonymDescriber(pair.Key);

			if (describer == null)
			{
				// It could be a connection dataset which includes DatasetKey, DatasetId etc. so don't
				// report an exception if it is.
				if (Csb.Describers[pair.Key] == null)
				{
					NotSupportedException ex = new($"Connection parameter '{pair.Key}' has no descriptor property configured.");
					Diag.Dug(ex);
				}
				continue;
			}

			if (SetProperty(describer.Name, pair.Value))
				changed = true;
		}

		if (changed && !_ParametersChanged)
		{
			_ParametersChanged = true;
			_ConnectionStringBuilder = null;
		}

	}



	public virtual void Parse(string s)
	{
		DbConnectionStringBuilder csb = ConnectionStringBuilder;

		csb.Clear();
		csb.ConnectionString = s;

		Parse(csb);
	}



	public virtual void PopulateConnectionStringBuilder(DbConnectionStringBuilder dbcsb, bool secure)
	{
		Csb csa = (Csb)dbcsb;

		csa.Clear();
				
		foreach (KeyValuePair<string, object> pair in _AssignedConnectionProperties)
		{
			if ((secure && (pair.Key.ToLower() == SysConstants.C_KeyPassword.ToLower() || pair.Key.ToLower() == SysConstants.C_KeyExInMemoryPassword.ToLower()))
				|| !IsParameter(pair.Key))
			{
				continue;
			}

			if (pair.Key == SysConstants.C_KeyExInMemoryPassword)
			{
				csa[SysConstants.C_KeyPassword] = GetProperty("Password");
				continue;
			}

			if (Cmd.IsNullValue(pair.Value))
				continue;

			if (pair.Value.GetType() == typeof(byte[]))
				csa[pair.Key] = Encoding.Default.GetString((byte[])pair.Value);
			else
				csa[pair.Key] = pair.Value;
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



	public virtual void Set(string connectionString)
	{
		Parse(connectionString);
	}



	public virtual void Test()
	{
		DbConnection conn = DataConnection;

		conn.Open();
		conn.Close();
		conn.Dispose();
	}



	public virtual string ToDisplayString()
	{
		DbConnectionStringBuilder csb = ConnectionStringBuilder;

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



	public virtual string ToFullString(bool secure)
	{
		return Get_ConnectionStringBuilder(secure).ConnectionString;
	}



	public virtual IBPropertyAgent ToConnectionInfo()
	{
		return new ConnectionPropertyAgent(this);
	}



	public virtual bool TryGetDefaultValue(string name, out object value)
	{
		if (!Describers.TryGetDefaultValue(name, out value))
		{
			ArgumentException ex = new($"Could not find a default value for descriptor {name}.");
			Diag.Dug(ex);
			throw ex;
		}

		return true;
	}



	public virtual void UpdatePropertyInfo(IBPropertyAgent rhs)
	{
		rhs.CopyTo(this);
	}


	public void ValidateTextBoxField(string value, string propertyName)
	{
		if (string.IsNullOrWhiteSpace(value) && !string.IsNullOrWhiteSpace(propertyName.Trim()))
		{
			ValidationErrors[propertyName] = Resources.TextBoxRequiredFieldToolTip;
		}
		else
		{
			_ValidationErrors?.Remove(propertyName);
		}

		_ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
	}



	public virtual bool WillObjectPropertyChange(string name, object newValue, bool removing)
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


}
