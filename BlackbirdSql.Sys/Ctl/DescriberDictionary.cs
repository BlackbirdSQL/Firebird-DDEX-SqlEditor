
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using BlackbirdSql.Sys.Extensions;
using BlackbirdSql.Sys.Interfaces;



namespace BlackbirdSql.Sys.Ctl;


// =========================================================================================================
//
//											DescriberDictionary Class
//
/// <summary>
/// Extended dictionary class for managing <see cref="DbConnectionStringBuilder"/> property descriptors and
/// additional information accessible in a <see cref="Describer"/>.
/// </summary>
// =========================================================================================================
public class DescriberDictionary : PublicDictionary<string, Describer>
{

	private IDictionary<string, Describer> _Synonyms;


	/// <summary>
	/// Returns an enumerable of all connection describers of parameters that appear in the 'Advanced'
	/// dialog of a connection dialog.
	/// See the <seealso cref="DescriberKeys"/> enumerable for further information.
	/// </summary>
	public IEnumerable<Describer> AdvancedKeys => new EnumerableAdvanced(this);

	/// <summary>
	/// Returns an enumerable of all connection describers that are valid connection parameters
	/// for the underlying native database engine.
	/// See the <seealso cref="DescriberKeys"/> enumerable for further information.
	/// </summary>
	public IEnumerable<Describer> ConnectionKeys => new EnumerableConnection(this);

	/// <summary>
	/// Returns an enumerable of all connection key describers in the <see cref="DescriberDictionary"/>.
	/// A <see cref="Describer"/> is a detailed class equivalent of a descriptor. The database engine's
	/// <see cref="DescriberDictionary"/> is defined in the native database
	/// <see cref="IBsNativeDatabaseEngine"/> service.
	/// </summary>
	public IEnumerable<Describer> DescriberKeys => new EnumerableDescribers(this);

	/// <summary>
	/// Returns a <see cref="Describer"/> enumerable of all equivalency connection parameters as defined in the User
	/// Options. Equivalency parameters are connection parameters that could produce differing
	/// result sets. See the <seealso cref="DescriberKeys"/> enumerable for further information.
	/// </summary>
	public IEnumerable<Describer> EquivalencyKeys => new EnumerableEquivalencyDescribers(this);

	/// <summary>
	/// MandatoryKeys include the minimum set of connection parameters required to establish
	/// a connection including unsafe (non-public) parameters. eg. Passwords.
	/// See the <seealso cref="DescriberKeys"/> enumerable for further information.
	/// </summary>
	public IEnumerable<Describer> MandatoryKeys => new EnumerableMandatory(this);

	/// <summary>
	/// PublicMandatoryKeys is a subset of <see cref="MandatoryKeys"/> that includes the minimum set
	/// of connection parameters required to establish a connection excluding unsafe (non-public)
	/// parameters. eg. Passwords.
	/// See the <seealso cref="DescriberKeys"/> enumerable for further information.
	/// </summary>
	public IEnumerable<Describer> PublicMandatoryKeys => new EnumerablePublicMandatory(this);

	/// <summary>
	/// The weak equivalency keys enumerable is a subset of <see cref="EquivalencyKeys"/> and does
	/// not include the Application Name equivalency key if it has been included as an equivalency
	/// key in User options.
	/// This enumerable is used by the running connection table (Rct) and LinkageParser to identify
	/// equivalent connections that are differentiated by the application name parameter only, and
	/// are therefore functionally equivalent.
	/// </summary>
	public IEnumerable<Describer> WeakEquivalencyKeys => new EnumerableWeakEquivalencyDescribers(this);


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



	public IDictionary<string, Describer> Synonyms => _Synonyms;

	public DescriberDictionary() : base(StringComparer.OrdinalIgnoreCase)
	{
	}

	public DescriberDictionary(Describer[] describers, KeyValuePair<string, string>[] synonyms) : base(StringComparer.OrdinalIgnoreCase)
	{
		AddRange(describers);
		AddSynonyms(synonyms);
	}



	public DescriberDictionary(IDictionary<string, string> synonyms) : base(StringComparer.OrdinalIgnoreCase)
	{
		AddSynonyms(synonyms);
	}



	public void Add(string name, string parameter, Type propertyType, object defaultValue = null,
		bool isParameter = false, bool isAdvanced = true, bool isPublic = true, bool isMandatory = false)
	{
		if (this[name] != null)
		{
			ArgumentException ex = new($"Unable to add Descriptor '{name}'. Already exists.");
			Diag.Dug(ex);
			throw ex;
		}

		Describer describer = new(name, parameter, propertyType, defaultValue, isParameter,
			isAdvanced, isPublic, isMandatory);

		Add(name, describer);
		if (describer.ConnectionParameterKey != null
			&& describer.Name.ToLowerInvariant() != describer.ConnectionParameterKey.ToLowerInvariant())
		{
			AddSynonym(describer.ConnectionParameterKey.ToLowerInvariant(), describer.Name);
		}
	}



	public void Add(string name, Type propertyType, object defaultValue = null,
		bool isParameter = false, bool isAdvanced = true, bool isPublic = true, bool isMandatory = false)
	{
		Add(name, null, propertyType, defaultValue, isParameter,
			isAdvanced, isPublic, isMandatory);
	}


	public void AddRange(Describer[] describers)
	{
		base.AddRange(describers);


		foreach (Describer describer in describers)
		{
			AddSynonym(describer.Name.ToLowerInvariant(), describer.Name);

			if (describer.ConnectionParameterKey != null
				&& describer.Name.ToLowerInvariant() != describer.ConnectionParameterKey.ToLowerInvariant())
			{
				AddSynonym(describer.ConnectionParameterKey.ToLowerInvariant(), describer.Name);
			}
		}

	}



	public void AddRange(DescriberDictionary rhs)
	{
		base.AddRange(rhs);

		if (rhs._Synonyms != null)
			AddSynonyms(rhs._Synonyms);
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
			if (!_Synonyms.ContainsKey(synonym))
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



	/// <summary>
	/// Gets a describer array given a desciber name array.
	/// </summary>
	public Describer[] GetDescribers(string[] names)
	{
		List<Describer> describers = [];

		foreach (string name in names)
		{
			try
			{
				Describer describer = this[name] ?? throw new ArgumentException($"Describer {name} not found.");
				describers.Add(describer);
			}
			catch (Exception ex)
			{
				Diag.Dug(ex, $"Describer {name} not found or unable to add to array.");
				throw;
			}
		}

		return [.. describers];

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

		return value.ConnectionStringKey;

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
		IList<string> list = [];

		Describer describer = this[name];

		if (describer == null)
			return list;

		string lcname = describer.Name.ToLowerInvariant();
		string lcsynonym = name.ToLowerInvariant();

		if (lcname != lcsynonym)
		{
			list.Add(describer.Name);
		}

		string lcparam = describer.ConnectionParameterKey?.ToLowerInvariant();


		if (lcparam != null && lcparam != lcname && lcparam != lcsynonym)
		{
			list.Add(describer.ConnectionParameterKey);
		}

		foreach (KeyValuePair<string, Describer> pair in Synonyms)
		{
			if (pair.Value.Name.ToLowerInvariant() == describer.Name.ToLowerInvariant() && pair.Key.ToLowerInvariant() != lcsynonym)
				list.Add(pair.Key);
		}

		return list;
	}


	/// <summary>
	/// Kets the KeyValuePair pure synonyms array of a Describer array.
	/// </summary>
	public KeyValuePair<string, string>[] GetSynonyms(Describer[] describers)
	{
		try
		{
			List<KeyValuePair<string, string>> synonyms = [];

			foreach (KeyValuePair<string, Describer> pair in _Synonyms)
			{
				if (pair.Key == pair.Value.Name)
					continue;

				foreach (Describer describer in describers)
				{
					try
					{
						if (describer.Name == pair.Value.Name)
						{
							synonyms.Add(new KeyValuePair<string, string>(pair.Key, pair.Value.Name));
							break;
						}
					}
					catch (Exception ex)
					{
						Diag.Dug(ex, $"Failed to add synonym for {pair.Key}.");
						throw;
					}
				}
			}

			return [.. synonyms];
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;
		}

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



	public class EnumerableAdvanced(DescriberDictionary owner)
	: IBEnumerableDescribers<EnumeratorAdvanced>
	{
		private readonly DescriberDictionary _Owner = owner;

		// public IEnumerable<Describer> DescriberEnumerator => _Owner;

		public IEnumerator<Describer> GetEnumerator()
		{
			return new EnumeratorAdvanced(_Owner.Values);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new EnumeratorAdvanced(_Owner.Values);
		}
	}



	public class EnumerableEquivalencyDescribers(DescriberDictionary owner)
	: IBEnumerableDescribers<EnumeratorEquivalency>
	{
		private readonly DescriberDictionary _Owner = owner;

		// public IEnumerable<Describer> DescriberEnumerator => _Owner;

		public IEnumerator<Describer> GetEnumerator()
		{
			return new EnumeratorEquivalency(_Owner.Values);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new EnumeratorEquivalency(_Owner.Values);
		}
	}


	public class EnumerableWeakEquivalencyDescribers(DescriberDictionary owner)
		: IBEnumerableDescribers<EnumeratorWeakEquivalency>
	{
		private readonly DescriberDictionary _Owner = owner;

		public IEnumerator<Describer> GetEnumerator()
		{
			return new EnumeratorWeakEquivalency(_Owner.Values);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new EnumeratorWeakEquivalency(_Owner.Values);
		}
	}



	public class EnumerableMandatory(DescriberDictionary owner)
	: IBEnumerableDescribers<EnumeratorMandatory>
	{
		private readonly DescriberDictionary _Owner = owner;

		// public IEnumerable<Describer> DescriberEnumerator => _Owner;

		public IEnumerator<Describer> GetEnumerator()
		{
			return new EnumeratorMandatory(_Owner.Values);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new EnumeratorMandatory(_Owner.Values);
		}
	}



	public class EnumerablePublicMandatory(DescriberDictionary owner)
		: IBEnumerableDescribers<EnumeratorPublicMandatory>
	{
		private readonly DescriberDictionary _Owner = owner;

		// public IEnumerable<Describer> DescriberEnumerator => _Owner;

		public IEnumerator<Describer> GetEnumerator()
		{
			return new EnumeratorPublicMandatory(_Owner.Values);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new EnumeratorPublicMandatory(_Owner.Values);
		}
	}



	public class EnumerableConnection(DescriberDictionary owner)
	: IBEnumerableDescribers<EnumeratorConnection>
	{
		private readonly DescriberDictionary _Owner = owner;

		// public IEnumerable<Describer> DescriberEnumerator => _Owner;

		public IEnumerator<Describer> GetEnumerator()
		{
			return new EnumeratorConnection(_Owner.Values);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new EnumeratorConnection(_Owner.Values);
		}
	}

	public class EnumerableDescribers(DescriberDictionary owner)
		: IBEnumerableDescribers<EnumeratorDescribers>
	{
		private readonly DescriberDictionary _Owner = owner;

		// public IEnumerable<Describer> DescriberEnumerator => _Owner;

		public IEnumerator<Describer> GetEnumerator()
		{
			return new EnumeratorDescribers(_Owner.Values);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new EnumeratorDescribers(_Owner.Values);
		}
	}



	public class EnumeratorAdvanced(PublicValueCollection<string, Describer> values)
		: EnumeratorDescribers(values)
	{
		public override bool IsValid(Describer describer)
		{
			return describer.IsAdvanced && !describer.IsInternalStore;
		}

	}

	public class EnumeratorConnection(PublicValueCollection<string, Describer> values)
		: EnumeratorDescribers(values)
	{
		public override bool IsValid(Describer describer)
		{
			return describer.IsConnectionParameter && !describer.IsInternalStore;
		}

	}


	public class EnumeratorEquivalency(PublicValueCollection<string, Describer> values)
		: EnumeratorDescribers(values)
	{
		public override bool IsValid(Describer describer)
		{
			return describer.IsEquivalency && !describer.IsInternalStore;
		}

	}

	public class EnumeratorWeakEquivalency(PublicValueCollection<string, Describer> values)
		: EnumeratorDescribers(values)
	{
		public override bool IsValid(Describer describer)
		{
			return describer.IsEquivalency && !describer.IsInternalStore
				&& describer.Name != SysConstants.C_KeyApplicationName;
		}

	}


	public class EnumeratorMandatory(PublicValueCollection<string, Describer> values)
		: EnumeratorDescribers(values)
	{
		public override bool IsValid(Describer describer)
		{
			return describer.IsMandatory && !describer.IsInternalStore;
		}

	}

	public class EnumeratorPublicMandatory(PublicValueCollection<string, Describer> values)
		: EnumeratorDescribers(values)
	{
		public override bool IsValid(Describer describer)
		{
			return describer.IsPublicMandatory && !describer.IsInternalStore;
		}

	}

}

