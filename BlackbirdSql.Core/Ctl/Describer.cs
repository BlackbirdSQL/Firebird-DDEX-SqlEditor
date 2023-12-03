
using System;
using System.ComponentModel;
using System.Reflection;
using BlackbirdSql.Core.Model;


namespace BlackbirdSql.Core.Ctl;

// =========================================================================================================
//
//												Describer Class
//
// =========================================================================================================
/// <summary>
/// A Describer is a detailed description of a property descriptor as
/// defined in the Firebird csb, or an alias for properties used outside
/// of the csb, or a nova external property that is not used to create
/// an actual connection.
/// </summary>
/// <param name="name">
/// The TitleCased property name as defined in the Firebird csb or
/// or a title-cased nova name for external properties.
/// If ConnectionParameter is not null and does not match Name, then
/// Name is considered a synonym of ConnectionParameter.
/// </param>
/// <param name="connectionParameter">
/// The connection property/parameter name as defined in the Firebird csb. If
/// ConnectionParameter is not null and does not match the Descriptor Name,
/// then the Descriptor Name is considered a synonym of ConnectionParameter.
/// </param>
/// <param name="propertyType">The property's system type.</param>
/// <param name="defaultValue">
/// The property default value. For properties where the default value
/// must be determined at runtime, for strings use null and for
/// cardinals use int.MinValue.
/// </param>
/// <param name="isConnectionProperty">
/// True if this describer represents a Firebird connection property/parameter.
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
/// <param name="isEquivalency">
/// Determines if changes to the underlying connection property value will produce
/// differing results from the database. For example UserID and NoDatabaseTriggers
/// are equivalency properties whereas PacketSize is not.
/// </param>
// =========================================================================================================
public class Describer(string name, string connectionParameter, Type propertyType, object defaultValue = null,
	bool isConnectionProperty = false, bool isAdvanced = true, bool isPublic = true, bool isMandatory = false,
	bool isEquivalency = false)
{

	/// <summary>
	/// Shortened .ctor.
	/// </summary>
	public Describer(string name, Type propertyType, object defaultValue = null, bool isConnectionProperty = false,
		bool isAdvanced = true, bool isPublic = true, bool isMandatory = false, bool isEquivalency = false)
		: this(name, null, propertyType, defaultValue, isConnectionProperty, isAdvanced, isPublic, isMandatory, isEquivalency)
	{
	}




	private string _ConnectionParameter = connectionParameter;
	public static PropertyDescriptorCollection _Descriptors = null;



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the property descriptor collection for the current connection string.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected static PropertyDescriptorCollection Descriptors
	{
		get
		{
			return _Descriptors ??= TypeDescriptor.GetProperties(typeof(CsbAgent));
		}
	}



	/// <summary>
	/// The TitleCased property name as defined in the Firebird csb or
	/// or a title-cased nova name for external properties.
	/// If ConnectionParameter is not null and does not match Name, then
	/// Name is considered a synonym of ConnectionParameter.
	/// </summary>
	public string Name { get; set; } = name;

	/// <summary>
	/// The property's system type.
	/// </summary>
	public Type PropertyType { get; set; } = propertyType;

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
	public object DefaultValue { get; set; } = defaultValue;

	/// <summary>
	/// True if this describer represents a Firebird connection property/parameter.
	/// If PropertyName is not null than the describer defined by it's Name
	/// is a pseudonym for PropertyNanme.
	/// </summary>
	public bool IsConnectionParameter { get; set; } = isConnectionProperty;

	/// <summary>
	/// Returns false if the describer is a connection property/parameter and appears in
	/// connection dialog front-ends (ie. a basic connection property/parameter) else
	/// true in all other cases.
	/// </summary>
	public bool IsAdvanced { get; set; } = isAdvanced;

	/// <summary>
	/// Returns false if the descripber is a secure value else false.
	/// </summary>
	public bool IsPublic { get; set; } = isPublic;

	/// <summary>
	/// Returns true if the describer represents a connection property/parameter and is required.
	/// </summary>
	public bool IsMandatory { get; set; } = isMandatory;

	/// <summary>
	/// Determines if changes to the underlying connection property/parameter value will produce
	/// differing results from the database. For example UserID and NoDatabaseTriggers
	/// are equivalency properties whereas PacketSize is not.
	/// </summary>
	public bool IsEquivalency { get; set; } = isEquivalency;


	/// <summary>
	/// The ConnectionParameter name else the Descriptor Name if ConnectionParameter
	/// is null. .
	/// </summary>
	public string DerivedConnectionParameter
	{
		get
		{

			if (_ConnectionParameter != null)
				return _ConnectionParameter;

			return Key;
		}
	}


	public PropertyDescriptor Descriptor => ConnectionParameter == null ? null : Descriptors.Find(ConnectionParameter, true);


	public string DisplayName
	{
		get
		{
			if (!IsConnectionParameter)
				return null;

			Type csbType = typeof(CsbAgent);

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
	/// Returns true is this describer is a connection property/parameter and is a mandatory
	/// property and is a public property.
	/// </summary>
	public bool IsPublicMandatory => IsMandatory && !IsPublic;


	/// <summary>
	/// The connection property/parameter name as defined in the Firebird csb. If ConnectionParameter
	/// is not null and does not match the Descriptor Name, then the Descriptor Name is
	/// considered a synonym of ConnectionParameter.
	/// </summary>
	public string ConnectionParameter
	{
		get { return _ConnectionParameter; }
		set { _ConnectionParameter = value; }
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
			if (rhs == null || rhs == DBNull.Value)
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
			if (rhs == null || rhs == DBNull.Value)
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
	/// empty string for tyes convertable to string types.
	/// </summary>
	/// <param name="rhs"></param>
	/// <returns></returns>
	public bool DefaultEqualsOrEmptyString(object rhs)
	{
		if (DefaultValue == null)
		{
			if (rhs == null || rhs == DBNull.Value || rhs.ToString() == "")
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
	public bool MatchesConnectionProperty(string connectionParameter)
	{
		return IsConnectionParameter && SynonymMatches(connectionParameter);
	}

	/// <summary>
	/// Checks if this descriptor is a connection property/parameter and if true
	/// then checks if it's parameter field or name matches the given parameter name.
	/// </summary>
	public bool SynonymMatches(string synonym)
	{
		return _ConnectionParameter != null && _ConnectionParameter.Equals(synonym, StringComparison.OrdinalIgnoreCase)
				|| Name.Equals(synonym, StringComparison.OrdinalIgnoreCase);
	}


	public override string ToString()
	{
		return Name;
	}
}
