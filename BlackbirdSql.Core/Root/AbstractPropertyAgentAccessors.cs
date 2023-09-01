
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Security;
using System.Windows;
using System.Windows.Media.Imaging;

using BlackbirdSql.Core.Enums;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Core.Model;

using Microsoft.Data.ConnectionUI;




namespace BlackbirdSql.Core;


// =========================================================================================================
//									AbstractPropertyAgent Class - Accessors
//
/// <summary>
/// A conglomerate base class that supports dynamic addition of properties and parsing functionality for
/// <see cref="DbConnectionStringBuilder"/>, <see cref="MonikerAgent"/> and property strings as
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
public abstract partial class AbstractPropertyAgent : IBPropertyAgent
{

	// ---------------------------------------------------------------------------------
	#region Accessor Variables - AbstractPropertyAgent
	// ---------------------------------------------------------------------------------


	protected bool _GetSetConnectionOpened = false;
	protected int _GetSetCardinal = 0;


	#endregion Accesor Variables





	// =========================================================================================================
	#region Property Accessors - AbstractPropertyAgent
	// =========================================================================================================


	public virtual object this[string key]
	{
		get { return GetProperty(key); }
		set { SetProperty(key, value); }
	}

	public virtual BitmapImage IconImage => CoreIconsCollection.Instance.GetImage(
		Isset("Icon") ? (IBIconType)GetProperty("Icon") : CoreIconsCollection.Instance.Error_16);


	public DbConnectionStringBuilder ConnectionStringBuilder
	{
		set { Set_ConnectionStringBuilder(value); }
		get { return Get_ConnectionStringBuilder(false); }
	}


	public virtual DbConnection DataConnection
	{
		get
		{
			if (_Connection == null)
			{
				NotImplementedException ex = new("DataConnection");
				Diag.Dug(ex);
				throw ex;
			}

			return _Connection;
		}
	}


	public virtual DescriberDictionary Describers
	{
		get
		{
			if (_Describers == null)
				CreateAndPopulatePropertySet();

			return _Describers;
		}
	}


	public virtual string DisplayName
	{
		get
		{
			if (_DisplayName == null)
			{
				string database = Database;

				if (database == "")
					return "";

				return $"{DataSource}({Dataset})";
			}

			return _DisplayName;
		}
		set
		{
			_DisplayName = value;
		}
	}

	// public abstract string[] EquivalencyKeys { get; }

	public bool HasErrors => _ValidationErrors != null && _ValidationErrors.Any();

	public long Id => _Id;

	public bool IsComplete
	{
		get
		{
			foreach (string name in Describers.Mandatory)
			{
				if (!_AssignedConnectionProperties.TryGetValue(name,
					out object value) || string.IsNullOrEmpty((string)value))
				{
					return false;
				}
			}

			return true;
		}
	}

	public bool IsExtensible => true;


	public virtual string PropertyString => ConnectionStringBuilder.ConnectionString;


	public virtual string Dataset => null;



	public IDictionary<string, string> ValidationErrors
		=> _ValidationErrors ??= new Dictionary<string, string>();


	#endregion Property Accessors





	// =========================================================================================================
	#region External (non-connection) Property Accessors - AbstractPropertyAgent
	// =========================================================================================================


	public string AdministratorLogin
	{
		get { return (string)GetProperty("AdministratorLogin"); }
		set { SetProperty("AdministratorLogin", value); }
	}


	public IBIconType Icon
	{
		get { return (IBIconType)GetProperty("Icon"); }
		set { SetProperty("Icon", value); }
	}


	public string OtherParams
	{
		get { return (string)GetProperty("OtherParams"); }
		set { SetProperty("OtherParams", value); }
	}


	public bool PersistPassword
	{
		get { return (bool)GetProperty("PersistPassword"); }
		set { SetProperty("PersistPassword", value); }
	}


	public ServerDefinition ServerDefinition
	{
		get { return (ServerDefinition)GetProperty("ServerDefinition"); }
		set { SetProperty("ServerDefinition", value); }
	}


	public string ServerFullyQualifiedDomainName
	{
		get { return (string)GetProperty("ServerFullyQualifiedDomainName"); }
		set { SetProperty("ServerFullyQualifiedDomainName", value); }
	}


	public Version ServerVersion
	{
		get { return (Version)GetProperty("ServerVersion"); }
		set { SetProperty("ServerVersion", value); }
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


	public EnDbServerType ServerType
	{
		get { return (EnDbServerType)GetProperty("ServerType"); }
		set { SetProperty("ServerType", value); }
	}


	public string UserID
	{
		get { return (string)GetProperty("UserID"); }
		set { SetProperty("UserID", value); }
	}


	#endregion 	Descriptors Property Accessors




	// =========================================================================================================
	#region Property Getters/Setters - AbstractPropertyAgent
	// =========================================================================================================


	public virtual void Set_ConnectionStringBuilder(DbConnectionStringBuilder csb)
	{
		Parse(csb);
	}
	public virtual DbConnectionStringBuilder Get_ConnectionStringBuilder(bool secure)
	{
		if (_ConnectionStringBuilder != null && !_ParametersChanged)
			return _ConnectionStringBuilder;

		_ParametersChanged = false;

		PopulateConnectionStringBuilder(_ConnectionStringBuilder, secure);

		return _ConnectionStringBuilder;
	}



	public virtual (IBIconType, bool) GetSet_Icon()
	{
		IBIconType iconType;

		if (ServerType == EnDbServerType.Embedded)
			iconType = CoreIconsCollection.Instance.EmbeddedDatabase_32;
		else
			iconType = CoreIconsCollection.Instance.ClassicServer_32;

		Icon = iconType;

		return (iconType, false);

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the ServerDefinition property and sets it if successful.
	/// </summary>
	/// <returns>Returns the value tuple of the derived ServerDefinition else null and
	/// a boolean indicating wehther or not a connection was opened.</returns>
	// ---------------------------------------------------------------------------------
	public virtual  (ServerDefinition, bool) GetSet_ServerDefinition()
	{
		ServerDefinition serverDefinition;

		if (ServerType == EnDbServerType.Embedded)
			serverDefinition = new("Firebird", EnEngineType.EmbeddedDatabase);
		else
			serverDefinition = new("Firebird", EnEngineType.ClassicServer);

		ServerDefinition = serverDefinition;

		return (serverDefinition, false);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the ServerVersion property and sets it if successful.
	/// </summary>
	/// <returns>Returns the value tuple of the derived ServerVersion else null and
	/// a boolean indicating wehther or not a connection was opened.</returns>
	// ---------------------------------------------------------------------------------
	public (Version, bool) GetSet_ServerVersion()
	{
		if (DataConnection.State != ConnectionState.Open && !IsComplete)
			return (null, false);

		bool opened;
		Version version;

		(version, opened) = GetServerVersion(DataConnection);

		if (version != null)
			ServerVersion = version;

		return (version, opened);
	}



	protected virtual bool Set_ServerDefinitionProperty(string name, ServerDefinition value)
	{
		if (!WillServerDefinitionPropertyChange(name, value, false))
			return false;

		_AssignedConnectionProperties[name] = new ServerDefinition(value.EngineProduct, value.EngineType);

		RaisePropertyChanged(name);

		return true;
	}



	protected virtual bool Set_StringProperty(string name, string value)
	{
		if (!WillStringPropertyChange(name, value, false))
			return false;

		_AssignedConnectionProperties[name] = value;

		RaisePropertyChanged(name);

		return true;
	}

	protected virtual bool Set_ObjectProperty(string name, object value)
	{
		if (!WillObjectPropertyChange(name, value, false))
			return false;

		_AssignedConnectionProperties[name] = value;

		RaisePropertyChanged(name);

		return true;
	}



	protected virtual bool Set_IntProperty(string name, int value)
	{
		if (!WillIntPropertyChange(name, value, false))
			return false;

		_AssignedConnectionProperties[name] = value;

		RaisePropertyChanged(name);

		return true;
	}



	protected virtual bool Set_BoolProperty(string name, bool value)
	{
		if (!WillBoolPropertyChange(name, value, false))
			return false;

		_AssignedConnectionProperties[name] = value;

		RaisePropertyChanged(name);

		return true;
	}



	protected virtual bool Set_ByteProperty(string name, byte[] value)
	{
		if (!WillBytePropertyChange(name, value, false))
			return false;

		_AssignedConnectionProperties[name] = value.Clone();

		RaisePropertyChanged(name);

		return true;
	}



	protected virtual bool Set_PasswordProperty(string name, string value)
	{
		bool changed = WillPasswordPropertyChange(name, value, false);

		if (string.IsNullOrEmpty(value))
		{
			_AssignedConnectionProperties.Remove("InMemoryPassword");
			_AssignedConnectionProperties[name] = string.Empty;
		}
		else
		{
			_AssignedConnectionProperties["InMemoryPassword"] = value.StringToSecureString();
			_AssignedConnectionProperties.Remove("Password");
		}

		if (changed) RaisePropertyChanged(name);

		return changed;
	}



	protected virtual bool Set_VersionProperty(string name, Version value)
	{
		if (!WillStringPropertyChange(name, value?.ToString(), false))
			return false;

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
			return ((SecureString)secure).SecureStringToString();


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


	public virtual bool SetProperty(string name, object value)
	{

		bool changed;
		string propertyTypeName;

		propertyTypeName = GetPropertyTypeName(name).ToLower();


		switch (propertyTypeName)
		{
			case "serverdefinition":
				changed = Set_ServerDefinitionProperty(name, (ServerDefinition)value);
				break;
			case "password":
				changed = Set_PasswordProperty(name, Convert.ToString(value));
				break;
			case "version":
				changed = Set_VersionProperty(name, (Version)value);
				break;
			case "int32":
				changed = Set_IntProperty(name, Convert.ToInt32(value));
				break;
			case "bool":
				changed = Set_BoolProperty(name, Convert.ToBoolean(value));
				break;
			case "byte[]":
				changed = Set_ByteProperty(name, (byte[])value);
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
	public virtual bool TryGetSetDerivedProperty(string name, out object value)
	{
		bool connectionOpened = false;
		bool result = false;

		_GetSetCardinal++;

		try
		{
			switch (name)
			{
				case "Icon":
					(value, connectionOpened) = GetSet_Icon();
					result = true;
					break;
				case "ServerDefinition":
					(value, connectionOpened) = GetSet_ServerDefinition();
					result = value != null;
					break;
				case "ServerVersion":
					(value, connectionOpened) = GetSet_ServerVersion();
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
				try
				{
					DataConnection.Close();
				}
				catch { }

				_GetSetConnectionOpened = false;
			}
		}

		return result;

	}



	protected virtual bool WillServerDefinitionPropertyChange(string name, IBServerDefinition newValue, bool removing)
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
		IBServerDefinition currValue = isSet ? (IBServerDefinition)_AssignedConnectionProperties[name] : null;

		if (Cmd.NullEquality(newValue, currValue) <= EnNullEquality.Equal)
		{
			changed = Cmd.NullEquality(newValue, currValue) == EnNullEquality.UnEqual;

			if (changed && IsParameter(name))
				_ParametersChanged = true;

			return changed;
		}

		changed = currValue.EqualsServerDefinition(newValue);


		// if (changed && IsParameter(name))
		// 	_ParametersChanged |= changed;

		return changed;
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
			? ((SecureString)_AssignedConnectionProperties["InMemoryPassword"]).SecureStringToString()
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


}
