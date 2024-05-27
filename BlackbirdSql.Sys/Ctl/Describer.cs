
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;


namespace BlackbirdSql.Sys;

// =========================================================================================================
//
//												Describer Class
//
// =========================================================================================================
/// <summary>
/// A Describer is a detailed description of a property descriptor as
/// defined in the native database csb, or an alias for properties used outside
/// of the csb, or a nova external property that is not used to create
/// an actual connection.
/// </summary>
/// <param name="name">
/// The TitleCased property name as defined in the native database csb or
/// or a title-cased nova name for external properties.
/// If ConnectionParameterKey is not null and does not match Name, then
/// Name is considered a synonym of ConnectionParameterKey.
/// </param>
/// <param name="connectionParameterKey">
/// The connection property/parameter name as defined in the native db csb. If
/// ConnectionParameterKey is not null and does not match the Descriptor Name,
/// then the Descriptor Name is considered a synonym of ConnectionParameterKey.
/// </param>
/// <param name="propertyType">The property's system type.</param>
/// <param name="defaultValue">
/// The property default value. For properties where the default value
/// must be determined at runtime, for strings use null and for
/// cardinals use int.MinValue.
/// </param>
/// <param name="isConnectionProperty">
/// True if this describer represents a native database connection property/parameter.
/// If PropertyName is not null than the describer defined by it's Name
/// is a pseudonym for PropertyNanme.
/// </param>
/// <param name="isAdvanced">
/// false if the describer is a connection property/parameter and appears in
/// connection dialog front-ends (ie. a basic cconnection property/parameter) else
/// true in all other cases.
/// </param>
/// <param name="isPublic">
/// false if the describer is a secure value else true.
/// </param>
/// <param name="isMandatory">
/// Returns true if the describer represents a connection property/parameter and is required.
/// </param>
// =========================================================================================================
public class Describer
{

	public Describer(string name, string connectionParameterKey, Type propertyType, object defaultValue = null,
	bool isConnectionProperty = false, bool isAdvanced = true, bool isPublic = true,
	bool isMandatory = false, bool isInternalStore = false)
	{
		Name = name;

		_ConnectionParameterKey = connectionParameterKey;

		PropertyType = propertyType;
		DefaultValue = defaultValue;
		IsConnectionParameter = isConnectionProperty;
		IsAdvanced = isAdvanced;
		IsPublic = isPublic;
		IsMandatory = isMandatory;
		IsInternalStore = isInternalStore;
	}

	/// <summary>
	/// Shortened .ctor.
	/// </summary>
	public Describer(string name, Type propertyType, object defaultValue = null, bool isConnectionProperty = false,
		bool isAdvanced = true, bool isPublic = true, bool isMandatory = false)
		: this(name, null, propertyType, defaultValue, isConnectionProperty, isAdvanced, isPublic, isMandatory)
	{
	}






	private string _ConnectionParameterKey = null;
	private static PropertyDescriptorCollection _Descriptors = null;
	private bool? _IsEquivalency;




	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the property descriptor collection for the current connection string.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected static PropertyDescriptorCollection Descriptors
	{
		get
		{
			return _Descriptors ??= TypeDescriptor.GetProperties(DbNative.CsbType);
		}
	}



	/// <summary>
	/// The TitleCased property name as defined in the native database csb or
	/// or a title-cased nova name for external properties.
	/// If ConnectionParameterKey is not null and does not match Name, then
	/// Name is considered a synonym of ConnectionParameterKey.
	/// </summary>
	public string Name { get; set; } = null;

	/// <summary>
	/// The property's system type.
	/// </summary>
	public Type PropertyType { get; set; } = null;

	/// <summary>
	/// The property's system data type.
	/// </summary>
	public Type DataType => PropertyType.IsSubclassOf(typeof(Enum))
		? typeof(int)
		: (PropertyType == typeof(byte[])
			? typeof(string) 
			: (PropertyType == typeof(Version)
				? typeof(string) : PropertyType));

	/// <summary>
	/// The property default value. For properties where the default value
	/// must be determined at runtime, for strings use null and for
	/// cardinals use int.MinValue.
	/// </summary>
	public object DefaultValue { get; set; } = null;

	/// <summary>
	/// True if this describer is a valid browsable property else false
	/// if it's for internal storage.
	/// </summary>
	public bool IsInternalStore { get; set; } = false;

	/// <summary>
	/// True if this describer represents a native db connection property/parameter.
	/// If PropertyName is not null than the describer defined by it's Name
	/// is a pseudonym for PropertyName.
	/// </summary>
	public bool IsConnectionParameter { get; set; } = false;

	/// <summary>
	/// Returns false if the describer is a connection property/parameter and appears in
	/// connection dialog front-ends (ie. a basic connection property/parameter) else
	/// true in all other cases.
	/// </summary>
	public bool IsAdvanced { get; set; } = false;

	/// <summary>
	/// Returns false if the describer is a secure value else true.
	/// </summary>
	public bool IsPublic { get; set; } = false;

	/// <summary>
	/// Returns true if the describer represents a connection property/parameter and is required.
	/// </summary>
	public bool IsMandatory { get; set; } = false;

	/// <summary>
	/// Determines if changes to the underlying connection property/parameter value will produce
	/// differing results from the database. For example UserID and NoDatabaseTriggers
	/// are equivalency properties whereas PacketSize is not.
	/// </summary>
	public bool IsEquivalency
	{
		get
		{
			if (_IsEquivalency.HasValue)
				return _IsEquivalency.Value;

			_IsEquivalency = DbNative.EquivalencyKeys.Contains(Name);

			return _IsEquivalency.Value;
		}
	}


	/// <summary>
	/// The key used in the connection string. The ConnectionParameterKey else the
	/// Descriptor Name if ConnectionParameterKey is null.
	/// </summary>
	public string ConnectionStringKey
	{
		get
		{

			if (_ConnectionParameterKey != null)
				return _ConnectionParameterKey;

			return Key;
		}
	}


	public PropertyDescriptor Descriptor => ConnectionParameterKey == null ? null : Descriptors.Find(ConnectionParameterKey, true);


	public string DisplayName
	{
		get
		{
			if (!IsConnectionParameter)
				return null;

			Type csbType = DbNative.CsbType;

			PropertyInfo pinfo = csbType.GetProperty(Name);

			if (pinfo == null)
			{
				ArgumentNullException ex = new ArgumentNullException($"Property {Name} not found in csb.");
				Diag.Dug(ex);
				return null;
			}

			DisplayNameAttribute attr = pinfo.GetCustomAttribute<DisplayNameAttribute>();
			if (attr == null)
			{
				ArgumentNullException ex = new ArgumentNullException($"Property {Name} DisplayNameAttribute not found in csb.");
				Diag.Dug(ex);
				return null;
			}

			return attr.DisplayName;
		}
	}

	public string Key
	{
		get
		{
			// if (!IsConnectionParameter)
			// 	return null;
			return Name;
		}
	}

	/// <summary>
	/// Returns true if this describer is a connection property/parameter and is a mandatory
	/// property and is a public property.
	/// </summary>
	public bool IsPublicMandatory => IsMandatory && IsPublic;


	/// <summary>
	/// The connection property/parameter name as defined in the native database csb. If ConnectionParameterKey
	/// is not null and does not match the Descriptor Name, then the Descriptor Name is
	/// considered a synonym of ConnectionParameterKey.
	/// </summary>
	public string ConnectionParameterKey
	{
		get { return _ConnectionParameterKey; }
		set { _ConnectionParameterKey = value; }
	}




	/// <summary>
	/// Compares equivalency to DefaultValue, accounting for null and DBNull.
	/// </summary>
	/// <param name="rhs"></param>
	/// <returns></returns>
	public bool DefaultEquals(object rhs)
	{
		if (DefaultValue == null)
		{
			if (Cmd.IsNullValue(rhs))
				return true;

			return false;
		}

		if (PropertyType.IsEnum)
			return Convert.ToInt32(DefaultValue) == Convert.ToInt32(rhs);

		return DefaultValue.Equals(rhs);
	}


	/// <summary>
	/// Compares equivalency to DefaultValue, accounting for null, DBNull and
	/// empty string for string types.
	/// </summary>
	/// <param name="rhs"></param>
	/// <returns></returns>
	public bool DefaultEqualsOrEmpty(object rhs)
	{
		if (DefaultValue == null)
		{
			if (Cmd.IsNullValue(rhs))
				return true;

			return false;
		}

		if (DataType == typeof(string) && string.IsNullOrWhiteSpace((string)DefaultValue))
		{
			if (string.IsNullOrWhiteSpace((string)rhs) || rhs == DBNull.Value)
				return true;

			return false;
		}

		if (PropertyType.IsEnum)
			return Convert.ToInt32(DefaultValue) == Convert.ToInt32(rhs);

		return DefaultValue.Equals(rhs);
	}

	/// <summary>
	/// Compares equivalency to DefaultValue, accounting for null, DBNull and
	/// empty string for types convertable to string types.
	/// </summary>
	/// <param name="rhs"></param>
	public bool DefaultEqualsOrEmptyString(object rhs)
	{
		if (DefaultValue == null)
		{
			if (Cmd.IsNullValue(rhs) || rhs.ToString() == "")
				return true;

			return false;
		}

		if (string.IsNullOrWhiteSpace(DefaultValue.ToString()))
		{
			if (string.IsNullOrWhiteSpace(rhs.ToString()))
				return true;

			return false;
		}

		if (PropertyType.IsEnum)
			return Convert.ToInt32(DefaultValue) == Convert.ToInt32(rhs);

		return DefaultValue.Equals(rhs);
	}


	/// <summary>
	/// Checks if this descriptor is a parameter and if true then checks if
	/// it's parameter field or name matches the given parameter name.
	/// Used to locate a describer in a DesciberDictionary that can be used
	/// as a connection property/parameter.
	/// </summary>
	public bool MatchesConnectionProperty(string connectionParameterKey)
	{
		return IsConnectionParameter && SynonymMatches(connectionParameterKey);
	}

	/// <summary>
	/// Checks if this descriptor is a connection property/parameter and if true
	/// then checks if it's parameter field or name matches the given parameter name.
	/// </summary>
	public bool SynonymMatches(string synonym)
	{
		return _ConnectionParameterKey != null && _ConnectionParameterKey.Equals(synonym, StringComparison.OrdinalIgnoreCase)
				|| Name.Equals(synonym, StringComparison.OrdinalIgnoreCase);
	}


	public override string ToString()
	{
		return Name;
	}
}
