
using System;




namespace BlackbirdSql.Core;


public class Describer
{
	private string _Parameter;

	public string Name { get; set; }
	public Type PropertyType { get; set; }
	public object DefaultValue { get; set; }
	public bool IsParameter { get; set; }
	public bool IsAdvanced { get; set; }
	public bool IsPublic { get; set; }
	public bool IsMandatory { get; set; }
	public bool IsEquivalency { get; set; }


	/// <summary>
	/// The Parameter name else the Descriptor Name if the Parameter
	/// field is null. Returns null if the Descriptor is not a Parameter.
	/// </summary>
	public string DerivedParameter
	{
		get
		{
			if (!IsParameter)
				return null;

			if (_Parameter != null)
				return _Parameter;

			return Name.ToLower();
		}
	}

	public bool IsPublicMandatory
	{
		get
		{
			return IsMandatory && Name != "Password";
		}
	}


	/// <summary>
	/// The stored Parameter name. If Parameter is not null then the Descriptor
	/// Name is considered a synonym of Parameter.
	/// </summary>
	public string Parameter
	{
		get { return _Parameter; }
		set { _Parameter = value; }
	}


	public Describer(string name, Type propertyType, object defaultValue = null, bool isParameter = false,
		bool isAdvanced = true, bool isPublic = true, bool isMandatory = false, bool isEquivalency = false)
		: this(name, null, propertyType, defaultValue, isParameter, isAdvanced, isPublic, isMandatory, isEquivalency)
	{
	}

	public Describer(string name, string parameter, Type propertyType, object defaultValue = null,
		bool isParameter = false, bool isAdvanced = true, bool isPublic = true, bool isMandatory = false,
		bool isEquivalency = false)
	{
		Name = name;
		_Parameter = parameter;
		PropertyType = propertyType;
		DefaultValue = defaultValue;
		IsParameter = isParameter;
		IsAdvanced = isAdvanced;
		IsPublic = isPublic;
		IsMandatory = isMandatory;
		IsEquivalency = isEquivalency;
	}


	/// <summary>
	/// Checks if this descriptor is a parameter and if true then checks if
	/// it's parameter field or name matches the given parameter name.
	/// </summary>
	public bool ParameterMatches(string parameter)
	{
		return IsParameter && SynonymMatches(parameter);
	}

	/// <summary>
	/// Checks if this descriptor is a parameter and if true then checks if
	/// it's parameter field or name matches the given parameter name.
	/// </summary>
	public bool SynonymMatches(string synonym)
	{
		return (_Parameter != null && _Parameter.Equals(synonym, StringComparison.OrdinalIgnoreCase))
				|| Name.Equals(synonym, StringComparison.OrdinalIgnoreCase);
	}


	public override string ToString()
	{
		return Name;
	}
}
