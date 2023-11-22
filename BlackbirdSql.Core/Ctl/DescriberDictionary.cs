
using System;
using System.Collections;
using System.Collections.Generic;
using BlackbirdSql.Core.Ctl.Extensions;
using BlackbirdSql.Core.Ctl.Interfaces;


namespace BlackbirdSql.Core.Ctl;

public class DescriberDictionary : PublicDictionary<string, Describer>, IBEnumerableAdvancedDescriptors,
	IBEnumerableEquivalencyDescriptors, IBEnumerableMandatoryDescriptors, IBEnumerableConnectionDescriptors
{

	private int _EquivalencyCount = -1;
	private IDictionary<string, Describer> _Synonyms;
	



	public IBEnumerableAdvancedDescriptors Advanced => this;
	public IBEnumerableEquivalencyDescriptors Equivalency => this;
	public IBEnumerableMandatoryDescriptors Mandatory => this;
	public IBEnumerableConnectionDescriptors ConnectionProperties => this;


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

	public int EquivalencyCount
	{
		get
		{
			if (_EquivalencyCount == -1)
			{
				_EquivalencyCount = 0;

				foreach (Describer _ in Equivalency)
					_EquivalencyCount++;
			}
			return _EquivalencyCount;
		}
	}


	public IDictionary<string, Describer> Synonyms => _Synonyms;

	public DescriberDictionary() : base(StringComparer.OrdinalIgnoreCase)
	{
	}

	public DescriberDictionary(Describer[] describers, KeyValuePair<string, string>[] synonyms) : base(StringComparer.OrdinalIgnoreCase)
	{
		AddRange(describers);
		foreach (Describer describer in describers)
		{
			if (describer.ConnectionParameter != null
				&& describer.Name.ToLowerInvariant() != describer.ConnectionParameter.ToLowerInvariant())
			{
				AddSynonym(describer.ConnectionParameter.ToLowerInvariant(), describer.Name);
			}
		}
		AddSynonyms(synonyms);
	}



	public DescriberDictionary(IDictionary<string, string> synonyms) : base(StringComparer.OrdinalIgnoreCase)
	{
		AddSynonyms(synonyms);
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

		_EquivalencyCount = -1;

		Describer describer = new(name, parameter, propertyType, defaultValue, isParameter,
			isAdvanced, isPublic, isMandatory, isEquivalency);

		Add(name, describer);
		if (describer.ConnectionParameter != null
			&& describer.Name.ToLowerInvariant() != describer.ConnectionParameter.ToLowerInvariant())
		{
			AddSynonym(describer.ConnectionParameter.ToLowerInvariant(), describer.Name);
		}
	}



	public void Add(string name, Type propertyType, object defaultValue = null,
		bool isParameter = false, bool isAdvanced = true, bool isPublic = true, bool isMandatory = false,
		bool isEquivalency = false)
	{
		Add(name, null, propertyType, defaultValue, isParameter,
			isAdvanced, isPublic, isMandatory, isEquivalency);
	}



	public void AddRange(DescriberDictionary rhs)
	{
		base.AddRange(rhs);

		if (rhs._Synonyms != null)
			AddSynonyms(rhs._Synonyms);

		_EquivalencyCount = -1;
	}



	public void AddSynonym(string synonym, string key)
	{
		if (!TryGetValue(key, out Describer descriptor))
		{
			ArgumentException ex = new($"Descriptor '{key}' not found for synonym '{synonym}.");
			Diag.Dug(ex);
			throw ex;
		}

		_EquivalencyCount = -1;

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


	public void AddSynonyms(IDictionary<string, Describer> synonyms)
	{
		foreach (KeyValuePair<string, Describer> pair in synonyms)
			AddSynonym(pair.Key, pair.Value.Name);
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

		if (!value.IsConnectionParameter)
		{
			return null;
		}

		return value.DerivedConnectionParameter;

	}



	IEnumerator IBEnumerableAdvancedDescriptors.GetEnumerator()
	{
		return new AdvancedDescribersEnumerator(Values);
	}

	IEnumerator IBEnumerableEquivalencyDescriptors.GetEnumerator()
	{
		return new EquivalencyDescribersEnumerator(Values);
	}

	IEnumerator IBEnumerableMandatoryDescriptors.GetEnumerator()
	{
		return new MandatoryDescribersEnumerator(Values);
	}

	IEnumerator IBEnumerableConnectionDescriptors.GetEnumerator()
	{
		return new ConnectionDescribersEnumerator(Values);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the descriptor name given the default connection parameter name.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public Describer GetParameterDescriber(string parameter)
	{
		parameter = parameter.ToLower();

		for (int j = 0; j < _Count; j++)
		{
			if (_Entries[j].HashCode >= 0)
			{
				if (_Entries[j].Value.MatchesConnectionProperty(parameter))
					return _Entries[j].Value;
			}
		}

		return null;
	}


	public Describer GetSynonymDescriber(string synonym)
	{

		if (_Synonyms != null && _Synonyms.TryGetValue(synonym, out Describer value))
			return value;

		return GetParameterDescriber(synonym);
	}


	public string GetSynonymDescriptorName(string synonym)
	{
		Describer descriptor = GetSynonymDescriber(synonym);

		if (descriptor == null)
			return null;

		return descriptor.Name;
	}

	public IList<string> GetSynonyms(string name)
	{
		IList<string> list = new List<string>();

		Describer describer = this[name];

		if (describer == null)
			return list;

		string lcname = describer.Name.ToLowerInvariant();
		string lcsynonym = name.ToLowerInvariant();

		if (lcname != lcsynonym)
		{
			list.Add(describer.Name);
		}

		string lcparam = describer.ConnectionParameter?.ToLowerInvariant();


		if (lcparam != null && lcparam != lcname && lcparam != lcsynonym)
		{
			list.Add(describer.ConnectionParameter);
		}

		foreach (KeyValuePair<string, Describer> pair in Synonyms)
		{
			if (pair.Value.Name.ToLowerInvariant() == describer.Name.ToLowerInvariant() && pair.Key.ToLowerInvariant() != lcsynonym)
				list.Add(pair.Key);
		}

		return list;
	}

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the Type of any given property name synonym.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public Type GetSynonymType(string synonym)
	{
		Describer descriptor = GetSynonymDescriber(synonym);

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

		return descriptor.IsConnectionParameter;
	}



	public override bool Remove(string key)
	{
		if (!base.Remove(key))
			return false;

		_EquivalencyCount = -1;

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
