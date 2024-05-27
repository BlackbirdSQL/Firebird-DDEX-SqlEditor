
using System;
using System.Collections.Generic;


namespace BlackbirdSql.Sys;

/// <summary>
/// Defines a mutable key/value pair that can be set or retrieved.
/// </summary>
/// <typeparam name="TKey">A mutable <see cref="KeyValuePair{TKey, TValue}.Key"/>.</typeparam>
/// <typeparam name="TValue">A mutable <see cref="KeyValuePair{TKey, TValue}.Value"/>.</typeparam>
[Serializable]
public struct MutablePair<TKey, TValue>
{
	private KeyValuePair<TKey, TValue> _Pair;

	//
	// Summary:
	//     Gets the key in the key/value pair.
	//
	// Returns:
	//     A TKey that is the key of the System.Collections.Generic.KeyValuePair`2.
	public TKey Key
	{
		get
		{
			return _Pair.Key;
		}
		set
		{
			_Pair = new KeyValuePair<TKey, TValue>(value, _Pair.Value);
		}
	}

	//
	// Summary:
	//     Gets the value in the key/value pair.
	//
	// Returns:
	//     A TValue that is the value of the System.Collections.Generic.KeyValuePair`2.
	public TValue Value
	{
		get
		{
			return _Pair.Value;
		}

		set
		{
			_Pair = new KeyValuePair<TKey, TValue>(_Pair.Key, value);
		}
	}

	//
	// Summary:
	//     Initializes a new instance of the System.Collections.Generic.KeyValuePair`2 structure
	//     with the specified key and value.
	//
	// Parameters:
	//   key:
	//     The object defined in each key/value pair.
	//
	//   value:
	//     The definition associated with key.

	/// <summary>
	/// MutablePair .ctor.
	/// </summary>
	/// <param name="key">A mutable <see cref="KeyValuePair{TKey, TValue}.Key"/>.</param>
	/// <param name="value">A mutable <see cref="KeyValuePair{TKey, TValue}.Value"/>.</param>
	public MutablePair(TKey key, TValue value)
	{
		_Pair = new KeyValuePair<TKey, TValue>(key, value);
	}

	public MutablePair(MutablePair<TKey, TValue> pair)
	{
		_Pair = new KeyValuePair<TKey, TValue>(pair.Key, pair.Value);
	}

	public MutablePair(KeyValuePair<TKey, TValue> pair)
	{
		_Pair = new KeyValuePair<TKey, TValue>(pair.Key, pair.Value);
	}


	public static MutablePair<TKey, TValue> Find(MutablePair<TKey, TValue>[] pairs, TKey key)
	{
		if (key is string str)
			return Find(pairs, str);

		return Array.Find(pairs,
			(obj) => obj.Key.Equals(key));
	}

	public static MutablePair<TKey, TValue> Find(MutablePair<TKey, TValue>[] pairs,
		string key, StringComparison stringComparison = StringComparison.CurrentCultureIgnoreCase)
	{
		return Array.Find(pairs,
			(obj) => obj.Key == null
				? key == null
				: obj.Key.ToString().Equals(key, StringComparison.OrdinalIgnoreCase));
	}


	public static MutablePair<TKey, TValue> Find(List<MutablePair<TKey, TValue>> list, TKey key)
	{
		if (key is string str)
			return Find(list, str);

		return list.Find((obj) => obj.Key.Equals(key));
	}

	public static MutablePair<TKey, TValue> Find(List<MutablePair<TKey, TValue>> list,
		string key, StringComparison stringComparison = StringComparison.CurrentCultureIgnoreCase)
	{
		return list.Find((obj) => (obj.Key as string).Equals(key, stringComparison));
	}

	public static MutablePair<TKey, TValue> ValuePair(TKey key, TValue value)
	{
		return new MutablePair<TKey, TValue>(key, value);
	}


	//
	// Summary:
	//     Returns a string representation of the System.Collections.Generic.KeyValuePair`2,
	//     using the string representations of the key and value.
	//
	// Returns:
	//     A string representation of the System.Collections.Generic.KeyValuePair`2, which
	//     includes the string representations of the key and value.
	public override string ToString()
	{
		return _Pair.ToString();
	}
}
