
using System;
using System.Collections;
using System.Collections.Generic;

using BlackbirdSql.Core.Extensions;
using BlackbirdSql.Core.Interfaces;




namespace BlackbirdSql.Core;


public class DescriberDictionary : PublicDictionary<string, Describer>, IEnumerableAdvancedDescriptors, IEnumerableEquivalencyDescriptors, IEnumerableMandatoryDescriptors
{

	private IDictionary<string, Describer> _Synonyms;

	public IEnumerableAdvancedDescriptors Advanced => this;
	public IEnumerableEquivalencyDescriptors Equivalency => this;
	public IEnumerableMandatoryDescriptors Mandatory => this;


	public override Describer this[string key]
	{
		get
		{
			if (key == null)
				return null;

			if (!TryGetValue(key, out Describer value))
			{
				_Synonyms?.TryGetValue(key, out value);
			}

			return value;
		}
		set
		{
			Insert(key, value, add: false);
		}

	}


	public DescriberDictionary() : base(StringComparer.OrdinalIgnoreCase)
	{
	}



	public DescriberDictionary(IDictionary<string, string> synonyms) : base(StringComparer.OrdinalIgnoreCase)
	{
		foreach (KeyValuePair<string, string> pair in synonyms)
			AddSynonym(pair.Key, pair.Value);
	}



	public void Add(string name, string parameter, Type propertyType, object defaultValue = null,
		bool isParameter = false, bool isAdvanced = true, bool isPublic = true, bool isMandatory = false,
		bool isEquivalency = false)
	{
		if (this[name] != null)
		{
			ArgumentException ex = new($"Unable to add Descriptor '{name}'. Already exists.");
			Diag.Dug(ex);
			throw ex;
		}

		Describer descriptor = new(name, parameter, propertyType, defaultValue, isParameter,
			isAdvanced, isPublic, isMandatory, isEquivalency);

		Add(name, descriptor);
	}



	public void Add(string name, Type propertyType, object defaultValue = null,
		bool isParameter = false, bool isAdvanced = true, bool isPublic = true, bool isMandatory = false,
		bool isEquivalency = false)
	{
		Add(name, null, propertyType, defaultValue, isParameter,
			isAdvanced, isPublic, isMandatory, isEquivalency);
	}



	public void AddSynonym(string synonym, string key)
	{
		if (!TryGetValue(key, out Describer descriptor))
		{
			ArgumentException ex = new($"Descriptor '{key}' not found for synonym '{synonym}.");
			Diag.Dug(ex);
			throw ex;
		}

		_Synonyms ??= new Dictionary<string, Describer>(StringComparer.OrdinalIgnoreCase);
		try
		{
			_Synonyms.Add(synonym, descriptor);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex, "Synonym: " + synonym);
			throw ex;
		}
	}


	public void AddSynonyms(IDictionary<string, string> synonyms)
	{
		foreach (KeyValuePair<string, string> pair in synonyms)
			AddSynonym(pair.Key, pair.Value);
	}


	protected void AddSynonyms(IDictionary<string, Describer> synonyms)
	{
		foreach (KeyValuePair<string, Describer> pair in synonyms)
			AddSynonym(pair.Key, pair.Value.Name);
	}


	public void AddRange(DescriberDictionary rhs)
	{
		base.AddRange(rhs);

		if (rhs._Synonyms != null)
			AddSynonyms(_Synonyms);
	}


	public void AddSynonyms(KeyValuePair<string, string>[] synonyms)
	{
		foreach (KeyValuePair<string, string> pair in synonyms)
			AddSynonym(pair.Key, pair.Value);
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the default parameter name for a given a descriptor.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public string GetDescriptorParameter(string name)
	{
		if (!TryGetValue(name, out Describer value))
		{
			if (_Synonyms == null || !_Synonyms.TryGetValue(name, out value))
			{
				return null;
			}
		}

		if (!value.IsParameter)
		{
			return null;
		}

		return value.DerivedParameter;

	}



	IEnumerator IEnumerableAdvancedDescriptors.GetEnumerator()
	{
		return new AdvancedDescribersEnumerator(Values);
	}

	IEnumerator IEnumerableEquivalencyDescriptors.GetEnumerator()
	{
		return new EquivalencyDescribersEnumerator(Values);
	}

	IEnumerator IEnumerableMandatoryDescriptors.GetEnumerator()
	{
		return new MandatoryDescribersEnumerator(Values);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the descriptor name given the default connection parameter name.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public Describer GetParameterDescriptor(string parameter)
	{
		parameter = parameter.ToLower();

		for (int j = 0; j < _Count; j++)
		{
			if (_Entries[j].HashCode >= 0)
			{
				if (_Entries[j].Value.ParameterMatches(parameter))
					return _Entries[j].Value;
			}
		}

		return null;
	}


	public Describer GetSynonymDescriptor(string synonym)
	{

		if (_Synonyms != null && _Synonyms.TryGetValue(synonym, out Describer value))
			return value;

		return GetParameterDescriptor(synonym);
	}


	public string GetSynonymDescriptorName(string synonym)
	{
		Describer descriptor = GetSynonymDescriptor(synonym);

		if (descriptor == null)
			return null;

		return descriptor.Name;
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the Type of any given property name synonym.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public Type GetSynonymType(string synonym)
	{
		Describer descriptor = GetSynonymDescriptor(synonym);

		if (descriptor == null)
		{
			ArgumentException ex = new($"The synonym '{synonym}' has no descriptor");
			Diag.Dug(ex);
			throw ex;
		}

		return descriptor.PropertyType;
	}


	public bool IsAdvanced(string key)
	{
		if (!TryGetValue(key, out Describer descriptor))
		{
			ArgumentException ex = new("Descriptors: " + key);
			Diag.Dug(ex);
			throw ex;
		}

		return descriptor.IsAdvanced;
	}


	public bool IsParameter(string key)
	{
		if (!TryGetValue(key, out Describer descriptor))
		{
			ArgumentException ex = new("Descriptors: " + key);
			Diag.Dug(ex);
			throw ex;
		}

		return descriptor.IsParameter;
	}



	public override bool Remove(string key)
	{
		if (!base.Remove(key))
			return false;

		if (_Synonyms == null)
			return true;

		foreach (KeyValuePair<string, Describer> pair in _Synonyms)
		{
			if (pair.Value.Name == key)
				_Synonyms.Remove(pair.Key);
		}

		return true;
	}


		public bool TryGetDefaultValue(string key, out object value)
	{
		if (!TryGetValue(key, out Describer descriptor))
		{
			ArgumentException ex = new("Descriptors: " + key);
			Diag.Dug(ex);
			throw ex;
		}

		value = descriptor.DefaultValue;

		return true;
	}

}
