
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using BlackbirdSql.Sys.Properties;
using static BlackbirdSql.SysConstants;


namespace BlackbirdSql.Sys.Ctl;


// =========================================================================================================
//
//												Describer Class
//
/// <summary>
/// A Describer is a detailed description of a property descriptor as defined in the native database csb,
/// or an alias for properties used outside of the csb, or a nova external property that is not used to
/// create an actual connection.
/// </summary>
// =========================================================================================================
public class Describer
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - Describer
	// ---------------------------------------------------------------------------------


	/// <summary>
	/// Full .ctor.
	/// </summary>
	/// <param name="name">
	/// The TitleCased property name as defined in the native database csb or or a title-cased nova name
	/// for external properties. If ConnectionParameterKey is not null and does not match Name, then
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
	/// <param name="isInternalStore">Internal storage property. For example an encrypted version of a password.</param>
	public Describer(string name, string connectionParameterKey, Type propertyType, object defaultValue, int dtype)
	{
		_Name = name;
		_ConnectionParameterKey = connectionParameterKey;
		_PropertyType = propertyType;
		_DefaultValue = defaultValue;
		_DType = dtype;
	}

	/// <summary>
	/// Shortened .ctor.
	/// </summary>
	public Describer(string name, Type propertyType, object defaultValue, int dtype)
		: this(name, null, propertyType, defaultValue, dtype)
	{
	}


	public Describer(string name, string connectionParameterKey, Type propertyType, int dtype)
		: this(name, connectionParameterKey, propertyType, null, dtype)
	{
	}


	public Describer(string name, Type propertyType, int dtype)
	: this(name, null, propertyType, null, dtype)
	{
	}


	#endregion Constructors / Destructors




	// =========================================================================================================
	#region Fields - Describer
	// =========================================================================================================


	private readonly string _Name = null;
	private readonly string _ConnectionParameterKey = null;
	private readonly Type _PropertyType = null;
	private readonly object _DefaultValue = null;
	private readonly int _DType = 6;

	private bool? _IsEquivalency;
	private static PropertyDescriptorCollection _Descriptors = null;


	#endregion Fields





	// =========================================================================================================
	#region Property accessors - Describer
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the property descriptor collection for the current connection string.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected static PropertyDescriptorCollection Descriptors
	{
		get
		{
			return _Descriptors ??= TypeDescriptor.GetProperties(NativeDb.CsbType);
		}
	}


	/// <summary>
	/// The TitleCased property name as defined in the native database csb or
	/// or a title-cased nova name for external properties.
	/// If ConnectionParameterKey is not null and does not match Name, then
	/// Name is considered a synonym of ConnectionParameterKey.
	/// </summary>
	public string Name => _Name;


	/// <summary>
	/// The property's system type.
	/// </summary>
	public Type PropertyType => _PropertyType;


	/// <summary>
	/// The property's system data type.
	/// </summary>
	public Type DataType => _PropertyType.IsSubclassOf(typeof(Enum))
		? typeof(int)
		: _PropertyType == typeof(byte[])
			? typeof(string)
			: _PropertyType == typeof(Version)
				? typeof(string) : _PropertyType;

	/// <summary>
	/// The property default value. For properties where the default value
	/// must be determined at runtime, for strings use null and for
	/// cardinals use int.MinValue.
	/// </summary>
	public object DefaultValue => _DefaultValue;


	/// <summary>
	/// Returns true if the the connection property/parameter has a ReadOnlyAttribute(true) attribute.
	/// </summary>
	public bool HasReadOnly => (_DType & D_HasReadOnly) > 0;


	/// <summary>
	/// True if this describer is derived / calculated else false.
	/// </summary>
	public bool IsDerived => (_DType & D_Derived) > 0;

	/// <summary>
	/// True if this describer is an additional describer no in the original native
	/// describer table else false.
	/// </summary>
	public bool IsExtended => (_DType & D_ExtendedType) > 0;

	/// <summary>
	/// True if this describer is a valid browsable property else false
	/// if it's for internal storage.
	/// </summary>
	public bool IsInternalStore => (_DType & D_InternalType) > 0;


	/// <summary>
	/// True if this describer represents a native db connection property/parameter.
	/// If PropertyName is not null than the describer defined by it's Name
	/// is a pseudonym for PropertyName.
	/// </summary>
	public bool IsConnectionParameter => (_DType & D_Connection) > 0;


	/// <summary>
	/// Returns false if the describer is a connection property/parameter and appears in
	/// connection dialog front-ends (ie. a basic connection property/parameter) else
	/// true in all other cases.
	/// </summary>
	public bool IsAdvanced => (_DType & D_Advanced) > 0;


	/// <summary>
	/// Returns false if the describer is a secure value else true.
	/// </summary>
	public bool IsPublic => (_DType & D_Public) > 0;


	/// <summary>
	/// Returns true if the describer represents a connection property/parameter and is required.
	/// </summary>
	public bool IsMandatory => (_DType & D_Mandatory) > 0;


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

			_IsEquivalency = NativeDb.EquivalencyKeys.Contains(_Name);

			return _IsEquivalency.Value;
		}
	}


	/// <summary>
	/// The key used in the connection string. The ConnectionParameterKey else the
	/// Descriptor Name if ConnectionParameterKey is null.
	/// </summary>
	public string ConnectionStringKey => _ConnectionParameterKey ?? _Name;



	public PropertyDescriptor Descriptor => _ConnectionParameterKey == null ? null : Descriptors.Find(_ConnectionParameterKey, true);


	public string DisplayName
	{
		get
		{
			if (!IsConnectionParameter)
				return null;

			Type csbType = NativeDb.CsbType;

			PropertyInfo pinfo = csbType.GetProperty(_Name);

			if (pinfo == null)
			{
				ArgumentNullException ex = new ArgumentNullException(Resources.ExceptionCsbPropertyNotFound.Fmt(_Name));
				Diag.Ex(ex);
				return null;
			}

			DisplayNameAttribute attr = pinfo.GetCustomAttribute<DisplayNameAttribute>();
			if (attr == null)
			{
				ArgumentNullException ex = new ArgumentNullException(Resources.ExceptionCsbAttributeNotFound.Fmt(_Name, nameof(DisplayNameAttribute)));
				Diag.Ex(ex);
				return null;
			}

			return attr.DisplayName;
		}
	}


	public string Key => _Name;


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
	public string ConnectionParameterKey => _ConnectionParameterKey;


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - Describer
	// =========================================================================================================


	/// <summary>
	/// Compares equivalency to DefaultValue, accounting for null and DBNull.
	/// </summary>
	/// <param name="rhs"></param>
	/// <returns></returns>
	public bool DefaultEquals(object rhs)
	{
		if (_DefaultValue == null)
		{
			if (Cmd.IsNullValue(rhs))
				return true;

			return false;
		}

		if (_PropertyType.IsEnum)
			return Convert.ToInt32(_DefaultValue) == Convert.ToInt32(rhs);

		return _DefaultValue.Equals(rhs);
	}


	/// <summary>
	/// Compares equivalency to DefaultValue, accounting for null, DBNull and
	/// empty string for string types.
	/// </summary>
	/// <param name="rhs"></param>
	/// <returns></returns>
	public bool DefaultEqualsOrEmpty(object rhs)
	{
		if (_DefaultValue == null)
		{
			if (Cmd.IsNullValue(rhs))
				return true;

			return false;
		}

		if (DataType == typeof(string) && string.IsNullOrWhiteSpace((string)_DefaultValue))
		{
			if (string.IsNullOrWhiteSpace((string)rhs) || rhs == DBNull.Value)
				return true;

			return false;
		}

		if (_PropertyType.IsEnum)
			return Convert.ToInt32(_DefaultValue) == Convert.ToInt32(rhs);

		return _DefaultValue.Equals(rhs);
	}

	/// <summary>
	/// Compares equivalency to DefaultValue, accounting for null, DBNull and
	/// empty string for types convertable to string types.
	/// </summary>
	/// <param name="rhs"></param>
	public bool DefaultEqualsOrEmptyString(object rhs)
	{
		if (_DefaultValue == null)
		{
			if (Cmd.IsNullValue(rhs) || rhs.ToString() == "")
				return true;

			return false;
		}

		if (string.IsNullOrWhiteSpace(_DefaultValue.ToString()))
		{
			if (string.IsNullOrWhiteSpace(rhs.ToString()))
				return true;

			return false;
		}

		if (_PropertyType.IsEnum)
			return Convert.ToInt32(_DefaultValue) == Convert.ToInt32(rhs);

		return _DefaultValue.Equals(rhs);
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
				|| _Name.Equals(synonym, StringComparison.OrdinalIgnoreCase);
	}


	public override string ToString()
	{
		return _Name;
	}


	#endregion Methods

}
