using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
using System.Threading;


namespace BlackbirdSql.Sys;


/// <summary>
/// Exposed replica of <see cref="Dictionary{TKey, TValue}"/>.
/// </summary>
[Serializable]
[DebuggerDisplay("Count = {Count}")]
[ComVisible(false)]
public class PublicDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, IDictionary, ICollection, IReadOnlyDictionary<TKey, TValue>, IReadOnlyCollection<KeyValuePair<TKey, TValue>>, ISerializable, IDeserializationCallback
{
	public struct Entry
	{
		public int HashCode;

		public int Next;

		public TKey Key;

		public TValue Value;
	}


	private int[] _Buckets;

	protected Entry[] _Entries;

	protected int _Count;

	internal int _Version;

	private int _FreeList;

	private int _FreeCount;

	private IEqualityComparer<TKey> _Comparer;

	private PublicKeyCollection<TKey, TValue> _Keys;

	private PublicValueCollection<TKey, TValue> _Values;

	private object _SyncRoot;

	private const string C_VersionName = "Version";
	private const string C_HashSizeName = "HashSize";
	private const string C_KeyValuePairsName = "KeyValuePairs";
	private const string C_ComparerName = "Comparer";




	//
	// Summary:
	//     Gets the System.Collections.Generic.IEqualityComparer`1 that is used to determine
	//     equality of _Keys for the dictionary.
	//
	// Returns:
	//     The System.Collections.Generic.IEqualityComparer`1 generic interface implementation
	//     that is used to determine equality of _Keys for the current System.Collections.Generic.Dictionary`2
	//     and to provide hash _Values for the _Keys.
	public IEqualityComparer<TKey> Comparer => _Comparer;


	public Entry[] Entries => _Entries;


	public int RawCount => _Count;

	//
	// Summary:
	//     Gets the number of key/value pairs contained in the System.Collections.Generic.Dictionary`2.
	//
	// Returns:
	//     The number of key/value pairs contained in the System.Collections.Generic.Dictionary`2.
	public int Count => _Count - _FreeCount;


	//
	// Summary:
	//     Gets a collection containing the _Keys in the System.Collections.Generic.Dictionary`2.
	//
	// Returns:
	//     A System.Collections.Generic.Dictionary`2.KeyCollection containing the _Keys in
	//     the System.Collections.Generic.Dictionary`2.
	public PublicKeyCollection<TKey, TValue> Keys => _Keys ??= new(this);


	ICollection<TKey> IDictionary<TKey, TValue>.Keys => _Keys ??= new(this);


	IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => _Keys ??= new(this);


	//
	// Summary:
	//     Gets a collection containing the _Values in the System.Collections.Generic.Dictionary`2.
	//
	// Returns:
	//     A BlackbirdSql.Core.Extensions.PublicValueCollection<TKey, TValue> containing the _Values
	//     in the System.Collections.Generic.Dictionary`2.
	public PublicValueCollection<TKey, TValue> Values => _Values ??= new(this);


	ICollection<TValue> IDictionary<TKey, TValue>.Values => _Values ??= new(this);


	IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => _Values ??= new(this);


	//
	// Summary:
	//     Gets or sets the value associated with the specified key.
	//
	// Parameters:
	//   key:
	//     The key of the value to get or set.
	//
	// Returns:
	//     The value associated with the specified key. If the specified key is not found,
	//     a get operation throws a System.Collections.Generic.KeyNotFoundException, and
	//     a set operation creates a new element with the specified key.
	//
	// Exceptions:
	//   T:System.ArgumentNullException:
	//     key is null.
	//
	//   T:System.Collections.Generic.KeyNotFoundException:
	//     The property is retrieved and key does not exist in the collection.
	public virtual TValue this[TKey key]
	{
		get
		{
			int num = FindEntry(key);

			if (num >= 0)
				return _Entries[num].Value;

			KeyNotFoundException ex = new();
			Diag.Dug(ex);
			throw ex;
		}
		set
		{
			Insert(key, value, add: false);
		}
	}

	bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;


	//
	// Summary:
	//     Gets a value indicating whether access to the System.Collections.ICollection
	//     is synchronized (thread safe).
	//
	// Returns:
	//     true if access to the System.Collections.ICollection is synchronized (thread
	//     safe); otherwise, false. In the default implementation of System.Collections.Generic.Dictionary`2,
	//     this property always returns false.
	bool ICollection.IsSynchronized => false;


	//
	// Summary:
	//     Gets an object that can be used to synchronize access to the System.Collections.ICollection.
	//
	// Returns:
	//     An object that can be used to synchronize access to the System.Collections.ICollection.
	object ICollection.SyncRoot =>
		_SyncRoot ??= Interlocked.CompareExchange<object>(ref _SyncRoot, new object(), null);


	//
	// Summary:
	//     Gets a value indicating whether the System.Collections.IDictionary has a fixed
	//     size.
	//
	// Returns:
	//     true if the System.Collections.IDictionary has a fixed size; otherwise, false.
	//     In the default implementation of System.Collections.Generic.Dictionary`2, this
	//     property always returns false.
	bool IDictionary.IsFixedSize => false;



	//
	// Summary:
	//     Gets a value indicating whether the System.Collections.IDictionary is read-only.
	//
	// Returns:
	//     true if the System.Collections.IDictionary is read-only; otherwise, false. In
	//     the default implementation of System.Collections.Generic.Dictionary`2, this property
	//     always returns false.
	bool IDictionary.IsReadOnly => false;


	//
	// Summary:
	//     Gets an System.Collections.ICollection containing the _Keys of the System.Collections.IDictionary.
	//
	// Returns:
	//     An System.Collections.ICollection containing the _Keys of the System.Collections.IDictionary.
	ICollection IDictionary.Keys => Keys;


	//
	// Summary:
	//     Gets an System.Collections.ICollection containing the _Values in the System.Collections.IDictionary.
	//
	// Returns:
	//     An System.Collections.ICollection containing the _Values in the System.Collections.IDictionary.
	ICollection IDictionary.Values => Values;


	//
	// Summary:
	//     Gets or sets the value with the specified key.
	//
	// Parameters:
	//   key:
	//     The key of the value to get.
	//
	// Returns:
	//     The value associated with the specified key, or null if key is not in the dictionary
	//     or key is of a type that is not assignable to the key type TKey of the System.Collections.Generic.Dictionary`2.
	//
	// Exceptions:
	//   T:System.ArgumentNullException:
	//     key is null.
	//
	//   T:System.ArgumentException:
	//     A value is being assigned, and key is of a type that is not assignable to the
	//     key type TKey of the System.Collections.Generic.Dictionary`2.-or-A value is being
	//     assigned, and value is of a type that is not assignable to the value type TValue
	//     of the System.Collections.Generic.Dictionary`2.
	object IDictionary.this[object key]
	{
		get
		{
			if (IsCompatibleKey(key))
			{
				int num = FindEntry((TKey)key);

				if (num >= 0)
					return _Entries[num].Value;
			}

			return null;
		}
		set
		{
			if (key == null)
			{
				ArgumentNullException ex = new("key");
				Diag.Dug(ex);
				throw ex;
			}

			if (value == null && default(TValue) != null)
			{
				ArgumentNullException ex = new("value");
				Diag.Dug(ex);
				throw ex;
			}

			try
			{
				TKey key2 = (TKey)key;
				try
				{
					this[key2] = (TValue)value;
				}
				catch (InvalidCastException)
				{
					ArgumentException ex = new($"Arg_WrongType: value - {typeof(TValue)}");
					Diag.Dug(ex);
					throw ex;
				}
			}
			catch (InvalidCastException)
			{
				ArgumentException ex = new($"Arg_WrongType: key - {typeof(TKey)}");
				Diag.Dug(ex);
				throw ex;
			}
		}
	}

	//
	// Summary:
	//     Initializes a new instance of the System.Collections.Generic.Dictionary`2 class
	//     that is empty, has the default initial capacity, and uses the default equality
	//     _Comparer for the key type.
	public PublicDictionary()
	: this(0, null)
	{
	}

	//
	// Summary:
	//     Initializes a new instance of the System.Collections.Generic.Dictionary`2 class
	//     that is empty, has the specified initial capacity, and uses the default equality
	//     _Comparer for the key type.
	//
	// Parameters:
	//   capacity:
	//     The initial number of elements that the System.Collections.Generic.Dictionary`2
	//     can contain.
	//
	// Exceptions:
	//   T:System.ArgumentOutOfRangeException:
	//     capacity is less than 0.
	public PublicDictionary(int capacity)
	: this(capacity, null)
	{
	}

	//
	// Summary:
	//     Initializes a new instance of the System.Collections.Generic.Dictionary`2 class
	//     that is empty, has the default initial capacity, and uses the specified System.Collections.Generic.IEqualityComparer`1.
	//
	// Parameters:
	//   _Comparer:
	//     The System.Collections.Generic.IEqualityComparer`1 implementation to use when
	//     comparing _Keys, or null to use the default System.Collections.Generic.EqualityComparer`1
	//     for the type of the key.
	public PublicDictionary(IEqualityComparer<TKey> comparer)
	: this(0, comparer)
	{
	}

	//
	// Summary:
	//     Initializes a new instance of the System.Collections.Generic.Dictionary`2 class
	//     that is empty, has the specified initial capacity, and uses the specified System.Collections.Generic.IEqualityComparer`1.
	//
	// Parameters:
	//   capacity:
	//     The initial number of elements that the System.Collections.Generic.Dictionary`2
	//     can contain.
	//
	//   _Comparer:
	//     The System.Collections.Generic.IEqualityComparer`1 implementation to use when
	//     comparing _Keys, or null to use the default System.Collections.Generic.EqualityComparer`1
	//     for the type of the key.
	//
	// Exceptions:
	//   T:System.ArgumentOutOfRangeException:
	//     capacity is less than 0.
	public PublicDictionary(int capacity, IEqualityComparer<TKey> comparer)
	{
		if (capacity < 0)
		{
			ArgumentOutOfRangeException ex = new("capacity");
			Diag.Dug(ex);
			throw ex;
		}

		if (capacity > 0)
		{
			Initialize(capacity);
		}

		_Comparer = comparer ?? EqualityComparer<TKey>.Default;
	}

	//
	// Summary:
	//     Initializes a new instance of the System.Collections.Generic.Dictionary`2 class
	//     that contains elements copied from the specified System.Collections.Generic.IDictionary`2
	//     and uses the default equality _Comparer for the key type.
	//
	// Parameters:
	//   dictionary:
	//     The System.Collections.Generic.IDictionary`2 whose elements are copied to the
	//     new System.Collections.Generic.Dictionary`2.
	//
	// Exceptions:
	//   T:System.ArgumentNullException:
	//     dictionary is null.
	//
	//   T:System.ArgumentException:
	//     dictionary contains one or more duplicate _Keys.
	public PublicDictionary(IDictionary<TKey, TValue> dictionary)
	: this(dictionary, null)
	{
	}

	//
	// Summary:
	//     Initializes a new instance of the BlackbirdSql.Core.Extensions.PublicDictionary class
	//     that contains elements copied from the specified System.Collections.Generic.IDictionary`2
	//     and uses the specified System.Collections.Generic.IEqualityComparer`1.
	//
	// Parameters:
	//   dictionary:
	//     The System.Collections.Generic.IDictionary`2 whose elements are copied to the
	//     new BlackbirdSql.Core.Extensions.PublicDictionary.
	//
	//   _Comparer:
	//     The System.Collections.Generic.IEqualityComparer`1 implementation to use when
	//     comparing _Keys, or null to use the default System.Collections.Generic.EqualityComparer`1
	//     for the type of the key.
	//
	// Exceptions:
	//   T:System.ArgumentNullException:
	//     dictionary is null.
	//
	//   T:System.ArgumentException:
	//     dictionary contains one or more duplicate _Keys.
	public PublicDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> _Comparer)
	: this(dictionary?.Count ?? 0, _Comparer)
	{
		if (dictionary == null)
		{
			ArgumentNullException ex = new("dictionary");
			Diag.Dug(ex);
			throw ex;
		}

		foreach (KeyValuePair<TKey, TValue> item in dictionary)
		{
			Add(item.Key, item.Value);
		}
	}

	//
	// Summary:
	//     Initializes a new instance of the System.Collections.Generic.Dictionary`2 class
	//     with serialized data.
	//
	// Parameters:
	//   info:
	//     A System.Runtime.Serialization.SerializationInfo object containing the information
	//     required to serialize the System.Collections.Generic.Dictionary`2.
	//
	//   context:
	//     A System.Runtime.Serialization.StreamingContext structure containing the source
	//     and destination of the serialized stream associated with the System.Collections.Generic.Dictionary`2.
	protected PublicDictionary(SerializationInfo info, StreamingContext context)
	{
		HashHelpersEx.SerializationInfoTable.Add(this, info);
	}

	//
	// Summary:
	//     Adds the specified key and value to the dictionary.
	//
	// Parameters:
	//   key:
	//     The key of the element to add.
	//
	//   value:
	//     The value of the element to add. The value can be null for reference types.
	//
	// Exceptions:
	//   T:System.ArgumentNullException:
	//     key is null.
	//
	//   T:System.ArgumentException:
	//     An element with the same key already exists in the System.Collections.Generic.Dictionary`2.
	public void Add(TKey key, TValue value)
	{
		Insert(key, value, add: true);
	}

	void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> keyValuePair)
	{
		Add(keyValuePair.Key, keyValuePair.Value);
	}

	bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> keyValuePair)
	{
		int num = FindEntry(keyValuePair.Key);
		if (num >= 0 && EqualityComparer<TValue>.Default.Equals(_Entries[num].Value, keyValuePair.Value))
		{
			return true;
		}

		return false;
	}

	bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> keyValuePair)
	{
		int num = FindEntry(keyValuePair.Key);
		if (num >= 0 && EqualityComparer<TValue>.Default.Equals(_Entries[num].Value, keyValuePair.Value))
		{
			Remove(keyValuePair.Key);
			return true;
		}

		return false;
	}

	//
	// Summary:
	//     Removes all _Keys and _Values from the System.Collections.Generic.Dictionary`2.
	public void Clear()
	{
		if (_Count > 0)
		{
			for (int i = 0; i < _Buckets.Length; i++)
			{
				_Buckets[i] = -1;
			}

			Array.Clear(_Entries, 0, _Count);
			_FreeList = -1;
			_Count = 0;
			_FreeCount = 0;
			_Version++;
		}
	}


	//
	// Summary:
	//		Non-overridable ContainsKey() method.
	//     Determines whether the System.Collections.Generic.Dictionary`2 contains the specified
	//     key.
	//
	// Parameters:
	//   key:
	//     The key to locate in the System.Collections.Generic.Dictionary`2.
	//
	// Returns:
	//     true if the System.Collections.Generic.Dictionary`2 contains an element with
	//     the specified key; otherwise, false.
	//
	// Exceptions:
	//   T:System.ArgumentNullException:
	//     key is null.
	public bool ContainsEntry(TKey key)
	{
		return FindEntry(key) >= 0;
	}

	//
	// Summary:
	//     Determines whether the System.Collections.Generic.Dictionary`2 contains the specified
	//     key.
	//
	// Parameters:
	//   key:
	//     The key to locate in the System.Collections.Generic.Dictionary`2.
	//
	// Returns:
	//     true if the System.Collections.Generic.Dictionary`2 contains an element with
	//     the specified key; otherwise, false.
	//
	// Exceptions:
	//   T:System.ArgumentNullException:
	//     key is null.
	public virtual bool ContainsKey(TKey key)
	{
		return FindEntry(key) >= 0;
	}

	//
	// Summary:
	//     Determines whether the System.Collections.Generic.Dictionary`2 contains a specific
	//     value.
	//
	// Parameters:
	//   value:
	//     The value to locate in the System.Collections.Generic.Dictionary`2. The value
	//     can be null for reference types.
	//
	// Returns:
	//     true if the System.Collections.Generic.Dictionary`2 contains an element with
	//     the specified value; otherwise, false.
	public bool ContainsValue(TValue value)
	{
		if (value == null)
		{
			for (int i = 0; i < _Count; i++)
			{
				if (_Entries[i].HashCode >= 0 && _Entries[i].Value == null)
				{
					return true;
				}
			}
		}
		else
		{
			EqualityComparer<TValue> @default = EqualityComparer<TValue>.Default;
			for (int j = 0; j < _Count; j++)
			{
				if (_Entries[j].HashCode >= 0 && @default.Equals(_Entries[j].Value, value))
				{
					return true;
				}
			}
		}

		return false;
	}

	private void CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
	{
		if (array == null)
		{
			ArgumentNullException ex = new("array");
			Diag.Dug(ex);
			throw ex;
		}

		if (index < 0 || index > array.Length)
		{
			ArgumentOutOfRangeException ex = new("index cannot be negative");
			Diag.Dug(ex);
			throw ex;
		}

		if (array.Length - index < Count)
		{
			ArgumentException ex = new("Arg_ArrayPlusOffTooSmall");
			Diag.Dug(ex);
			throw ex;
		}

		int num = _Count;
		Entry[] array2 = _Entries;
		for (int i = 0; i < num; i++)
		{
			if (array2[i].HashCode >= 0)
			{
				array[index++] = new KeyValuePair<TKey, TValue>(array2[i].Key, array2[i].Value);
			}
		}
	}

	//
	// Summary:
	//     Returns an enumerator that iterates through the System.Collections.Generic.Dictionary`2.
	//
	// Returns:
	//     A System.Collections.Generic.Dictionary`2.Enumerator structure for the System.Collections.Generic.Dictionary`2.
	public PublicDictionaryEnumerator<TKey, TValue> GetEnumerator()
	{
		return new PublicDictionaryEnumerator<TKey, TValue>(this, 2);
	}

	IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
	{
		return new PublicDictionaryEnumerator<TKey, TValue>(this, 2);
	}

	//
	// Summary:
	//     Implements the System.Runtime.Serialization.ISerializable interface and returns
	//     the data needed to serialize the System.Collections.Generic.Dictionary`2 instance.
	//
	// Parameters:
	//   info:
	//     A System.Runtime.Serialization.SerializationInfo object that contains the information
	//     required to serialize the System.Collections.Generic.Dictionary`2 instance.
	//
	//   context:
	//     A System.Runtime.Serialization.StreamingContext structure that contains the source
	//     and destination of the serialized stream associated with the System.Collections.Generic.Dictionary`2
	//     instance.
	//
	// Exceptions:
	//   T:System.ArgumentNullException:
	//     info is null.
	[SecurityCritical]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			ArgumentNullException ex = new("info");
			Diag.Dug(ex);
			throw ex;
		}

		info.AddValue(C_VersionName, _Version);
		info.AddValue(C_ComparerName, HashHelpersEx.GetEqualityComparerForSerialization(_Comparer), typeof(IEqualityComparer<TKey>));
		info.AddValue(C_HashSizeName, _Buckets != null ? _Buckets.Length : 0);
		if (_Buckets != null)
		{
			KeyValuePair<TKey, TValue>[] array = new KeyValuePair<TKey, TValue>[Count];
			CopyTo(array, 0);
			info.AddValue(C_KeyValuePairsName, array, typeof(KeyValuePair<TKey, TValue>[]));
		}
	}

	protected int FindEntry(TKey key)
	{
		if (key == null)
		{
			ArgumentNullException ex = new("key");
			Diag.Dug(ex);
			throw ex;
		}

		if (_Buckets != null)
		{
			int num = _Comparer.GetHashCode(key) & 0x7FFFFFFF;
			for (int num2 = _Buckets[num % _Buckets.Length]; num2 >= 0; num2 = _Entries[num2].Next)
			{
				if (_Entries[num2].HashCode == num && _Comparer.Equals(_Entries[num2].Key, key))
				{
					return num2;
				}
			}
		}

		return -1;
	}

	private void Initialize(int capacity)
	{
		int prime = HashHelpersEx.GetPrime(capacity);
		_Buckets = new int[prime];
		for (int i = 0; i < _Buckets.Length; i++)
		{
			_Buckets[i] = -1;
		}

		_Entries = new Entry[prime];
		_FreeList = -1;
	}

	protected void Insert(TKey key, TValue value, bool add)
	{
		if (key == null)
		{
			ArgumentNullException ex = new("key");
			Diag.Dug(ex);
			throw ex;
		}

		if (_Buckets == null)
		{
			Initialize(0);
		}

		int num = _Comparer.GetHashCode(key) & 0x7FFFFFFF;
		int num2 = num % _Buckets.Length;
		int num3 = 0;
		for (int num4 = _Buckets[num2]; num4 >= 0; num4 = _Entries[num4].Next)
		{
			if (_Entries[num4].HashCode == num && _Comparer.Equals(_Entries[num4].Key, key))
			{
				if (add)
				{
					ArgumentException ex = new("Argument_AddingDuplicate");
					Diag.Dug(ex);
					throw ex;
				}

				_Entries[num4].Value = value;
				_Version++;
				return;
			}

			num3++;
		}

		int num5;
		if (_FreeCount > 0)
		{
			num5 = _FreeList;
			_FreeList = _Entries[num5].Next;
			_FreeCount--;
		}
		else
		{
			if (_Count == _Entries.Length)
			{
				Resize();
				num2 = num % _Buckets.Length;
			}

			num5 = _Count;
			_Count++;
		}

		_Entries[num5].HashCode = num;
		_Entries[num5].Next = _Buckets[num2];
		_Entries[num5].Key = key;
		_Entries[num5].Value = value;
		_Buckets[num2] = num5;
		_Version++;
		/*
		if (num3 > HashHelpersEx.C_HashCollisionThreshold && HashHelpersEx.IsWellKnownEqualityComparer(_Comparer))
		{
			_Comparer = (IEqualityComparer<TKey>)HashHelpersEx.GetRandomizedEqualityComparer(_Comparer);
			Resize(_Entries.Length, forceNewHashCodes: true);
		}
		*/
	}

	//
	// Summary:
	//     Implements the System.Runtime.Serialization.ISerializable interface and raises
	//     the deserialization event when the deserialization is complete.
	//
	// Parameters:
	//   sender:
	//     The source of the deserialization event.
	//
	// Exceptions:
	//   T:System.Runtime.Serialization.SerializationException:
	//     The System.Runtime.Serialization.SerializationInfo object associated with the
	//     current System.Collections.Generic.Dictionary`2 instance is invalid.
	public virtual void OnDeserialization(object sender)
	{
		HashHelpersEx.SerializationInfoTable.TryGetValue(this, out var value);
		if (value == null)
		{
			return;
		}

		int @int = value.GetInt32(C_VersionName);
		int int2 = value.GetInt32(C_HashSizeName);
		_Comparer = (IEqualityComparer<TKey>)value.GetValue(C_ComparerName, typeof(IEqualityComparer<TKey>));
		if (int2 != 0)
		{
			_Buckets = new int[int2];
			for (int i = 0; i < _Buckets.Length; i++)
			{
				_Buckets[i] = -1;
			}

			_Entries = new Entry[int2];
			_FreeList = -1;
			KeyValuePair<TKey, TValue>[] array = (KeyValuePair<TKey, TValue>[])value.GetValue(C_KeyValuePairsName, typeof(KeyValuePair<TKey, TValue>[]));
			if (array == null)
			{
				SerializationException ex = new("Serialization_MissingKeys");
				Diag.Dug(ex);
				throw ex;
			}

			for (int j = 0; j < array.Length; j++)
			{
				if (array[j].Key == null)
				{
					SerializationException ex = new("Serialization_NullKey");
					Diag.Dug(ex);
					throw ex;
				}

				Insert(array[j].Key, array[j].Value, add: true);
			}
		}
		else
		{
			_Buckets = null;
		}

		_Version = @int;
		HashHelpersEx.SerializationInfoTable.Remove(this);
	}

	private void Resize()
	{
		Resize(HashHelpersEx.ExpandPrime(_Count), forceNewHashCodes: false);
	}

	private void Resize(int newSize, bool forceNewHashCodes)
	{
		int[] array = new int[newSize];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = -1;
		}

		Entry[] array2 = new Entry[newSize];
		Array.Copy(_Entries, 0, array2, 0, _Count);
		if (forceNewHashCodes)
		{
			for (int j = 0; j < _Count; j++)
			{
				if (array2[j].HashCode != -1)
				{
					array2[j].HashCode = _Comparer.GetHashCode(array2[j].Key) & 0x7FFFFFFF;
				}
			}
		}

		for (int k = 0; k < _Count; k++)
		{
			if (array2[k].HashCode >= 0)
			{
				int num = array2[k].HashCode % newSize;
				array2[k].Next = array[num];
				array[num] = k;
			}
		}

		_Buckets = array;
		_Entries = array2;
	}

	//
	// Summary:
	//     Non-overridable Remove.
	//     Removes the value with the specified key from the System.Collections.Generic.Dictionary`2.
	//
	// Parameters:
	//   key:
	//     The key of the element to remove.
	//
	// Returns:
	//     true if the element is successfully found and removed; otherwise, false. This
	//     method returns false if key is not found in the System.Collections.Generic.Dictionary`2.
	//
	// Exceptions:
	//   T:System.ArgumentNullException:
	//     key is null.
	public bool RemoveEntry(TKey key)
	{
		if (key == null)
		{
			ArgumentNullException ex = new("key");
			Diag.Dug(ex);
			throw ex;
		}

		if (_Buckets != null)
		{
			int num = _Comparer.GetHashCode(key) & 0x7FFFFFFF;
			int num2 = num % _Buckets.Length;
			int num3 = -1;
			for (int num4 = _Buckets[num2]; num4 >= 0; num4 = _Entries[num4].Next)
			{
				if (_Entries[num4].HashCode == num && _Comparer.Equals(_Entries[num4].Key, key))
				{
					if (num3 < 0)
					{
						_Buckets[num2] = _Entries[num4].Next;
					}
					else
					{
						_Entries[num3].Next = _Entries[num4].Next;
					}

					_Entries[num4].HashCode = -1;
					_Entries[num4].Next = _FreeList;
					_Entries[num4].Key = default;
					_Entries[num4].Value = default;
					_FreeList = num4;
					_FreeCount++;
					_Version++;
					return true;
				}

				num3 = num4;
			}
		}

		return false;
	}

	//
	// Summary:
	//     Removes the value with the specified key from the System.Collections.Generic.Dictionary`2.
	//
	// Parameters:
	//   key:
	//     The key of the element to remove.
	//
	// Returns:
	//     true if the element is successfully found and removed; otherwise, false. This
	//     method returns false if key is not found in the System.Collections.Generic.Dictionary`2.
	//
	// Exceptions:
	//   T:System.ArgumentNullException:
	//     key is null.
	public virtual bool Remove(TKey key)
	{
		return RemoveEntry(key);
	}


	//
	// Summary:
	//		Non-overridable TryGetValue method.
	//     Gets the value associated with the specified key.
	//
	// Parameters:
	//   key:
	//     The key of the value to get.
	//
	//   value:
	//     When this method returns, contains the value associated with the specified key,
	//     if the key is found; otherwise, the default value for the type of the value parameter.
	//     This parameter is passed uninitialized.
	//
	// Returns:
	//     true if the System.Collections.Generic.Dictionary`2 contains an element with
	//     the specified key; otherwise, false.
	//
	// Exceptions:
	//   T:System.ArgumentNullException:
	//     key is null.
	public bool TryGetEntry(TKey key, out TValue value)
	{
		int num = FindEntry(key);
		if (num >= 0)
		{
			value = _Entries[num].Value;
			return true;
		}

		value = default;
		return false;
	}

	//
	// Summary:
	//     Gets the value associated with the specified key.
	//
	// Parameters:
	//   key:
	//     The key of the value to get.
	//
	//   value:
	//     When this method returns, contains the value associated with the specified key,
	//     if the key is found; otherwise, the default value for the type of the value parameter.
	//     This parameter is passed uninitialized.
	//
	// Returns:
	//     true if the System.Collections.Generic.Dictionary`2 contains an element with
	//     the specified key; otherwise, false.
	//
	// Exceptions:
	//   T:System.ArgumentNullException:
	//     key is null.
	public virtual bool TryGetValue(TKey key, out TValue value)
	{
		return TryGetEntry(key, out value);
	}

	internal TValue GetValueOrDefault(TKey key)
	{
		int num = FindEntry(key);
		if (num >= 0)
		{
			return _Entries[num].Value;
		}

		return default;
	}

	void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
	{
		CopyTo(array, index);
	}

	//
	// Summary:
	//     Copies the elements of the System.Collections.Generic.ICollection`1 to an array,
	//     starting at the specified array index.
	//
	// Parameters:
	//   array:
	//     The one-dimensional array that is the destination of the elements copied from
	//     System.Collections.Generic.ICollection`1. The array must have zero-based indexing.
	//
	//   index:
	//     The zero-based index in array at which copying begins.
	//
	// Exceptions:
	//   T:System.ArgumentNullException:
	//     array is null.
	//
	//   T:System.ArgumentOutOfRangeException:
	//     index is less than 0.
	//
	//   T:System.ArgumentException:
	//     array is multidimensional.-or-array does not have zero-based indexing.-or-The
	//     number of elements in the source System.Collections.Generic.ICollection`1 is
	//     greater than the available space from index to the end of the destination array.-or-The
	//     type of the source System.Collections.Generic.ICollection`1 cannot be cast automatically
	//     to the type of the destination array.
	void ICollection.CopyTo(Array array, int index)
	{
		if (array == null)
		{
			ArgumentNullException ex = new("array");
			Diag.Dug(ex);
			throw ex;
		}

		if (array.Rank != 1)
		{
			ArgumentException ex = new("Arg_RankMultiDimNotSupported");
			Diag.Dug(ex);
			throw ex;
		}

		if (array.GetLowerBound(0) != 0)
		{
			ArgumentException ex = new("Arg_NonZeroLowerBound");
			Diag.Dug(ex);
			throw ex;
		}

		if (index < 0 || index > array.Length)
		{
			ArgumentOutOfRangeException ex = new("index cannot be negative");
			Diag.Dug(ex);
			throw ex;
		}

		if (array.Length - index < Count)
		{
			ArgumentException ex = new("Arg_ArrayPlusOffTooSmall");
			Diag.Dug(ex);
			throw ex;
		}


		if (array is KeyValuePair<TKey, TValue>[] array2)
		{
			CopyTo(array2, index);
			return;
		}

		if (array is DictionaryEntry[])
		{
			DictionaryEntry[] array3 = array as DictionaryEntry[];
			Entry[] array4 = _Entries;
			for (int i = 0; i < _Count; i++)
			{
				if (array4[i].HashCode >= 0)
				{
					array3[index++] = new DictionaryEntry(array4[i].Key, array4[i].Value);
				}
			}

			return;
		}


		if (array is not object[] array5)
		{
			ArgumentException ex = new("Argument_InvalidArrayType");
			Diag.Dug(ex);
			throw ex;
		}

		try
		{
			int num = _Count;
			Entry[] array6 = _Entries;
			for (int j = 0; j < num; j++)
			{
				if (array6[j].HashCode >= 0)
				{
					array5[index++] = new KeyValuePair<TKey, TValue>(array6[j].Key, array6[j].Value);
				}
			}
		}
		catch (ArrayTypeMismatchException)
		{
			ArgumentException ex = new("Argument_InvalidArrayType");
			Diag.Dug(ex);
			throw ex;
		}
	}

	//
	// Summary:
	//     Returns an enumerator that iterates through the collection.
	//
	// Returns:
	//     An System.Collections.IEnumerator that can be used to iterate through the collection.
	IEnumerator IEnumerable.GetEnumerator()
	{
		return new PublicDictionaryEnumerator<TKey, TValue>(this, 2);
	}

	private static bool IsCompatibleKey(object key)
	{
		if (key == null)
		{
			ArgumentNullException ex = new("key");
			Diag.Dug(ex);
			throw ex;
		}

		return key is TKey;
	}

	//
	// Summary:
	//     Adds the specified key and value to the dictionary.
	//
	// Parameters:
	//   key:
	//     The object to use as the key.
	//
	//   value:
	//     The object to use as the value.
	//
	// Exceptions:
	//   T:System.ArgumentNullException:
	//     key is null.
	//
	//   T:System.ArgumentException:
	//     key is of a type that is not assignable to the key type TKey of the System.Collections.Generic.Dictionary`2.-or-value
	//     is of a type that is not assignable to TValue, the type of _Values in the System.Collections.Generic.Dictionary`2.-or-A
	//     value with the same key already exists in the System.Collections.Generic.Dictionary`2.
	void IDictionary.Add(object key, object value)
	{
		if (key == null)
		{
			ArgumentNullException ex = new("key");
			Diag.Dug(ex);
			throw ex;
		}

		if (value == null && default(TValue) != null)
		{
			ArgumentNullException ex = new("value");
			Diag.Dug(ex);
			throw ex;
		}

		try
		{
			TKey key2 = (TKey)key;
			try
			{
				Add(key2, (TValue)value);
			}
			catch (InvalidCastException)
			{
				ArgumentException ex = new($"Arg_WrongType: value - {typeof(TValue)}");
				Diag.Dug(ex);
				throw ex;
			}
		}
		catch (InvalidCastException)
		{
			ArgumentException ex = new($"Arg_WrongType: key - {typeof(TKey)}");
			Diag.Dug(ex);
			throw ex;
		}
	}


	public void AddRange(PublicDictionary<TKey, TValue> collection)
	{
		foreach (KeyValuePair<TKey, TValue> pair in collection)
		{
			Add(pair.Key, pair.Value);
		}
	}


	public void AddRange(IEnumerable<TValue> collection)
	{
		TKey key;

		foreach (TValue value in collection)
		{
			key = (TKey)(value.ToString() as object);

			Add(key, value);
		}
	}

	//
	// Summary:
	//     Determines whether the System.Collections.IDictionary contains an element with
	//     the specified key.
	//
	// Parameters:
	//   key:
	//     The key to locate in the System.Collections.IDictionary.
	//
	// Returns:
	//     true if the System.Collections.IDictionary contains an element with the specified
	//     key; otherwise, false.
	//
	// Exceptions:
	//   T:System.ArgumentNullException:
	//     key is null.
	bool IDictionary.Contains(object key)
	{
		if (IsCompatibleKey(key))
		{
			return ContainsKey((TKey)key);
		}

		return false;
	}

	//
	// Summary:
	//     Returns an System.Collections.IDictionaryEnumerator for the System.Collections.IDictionary.
	//
	// Returns:
	//     An System.Collections.IDictionaryEnumerator for the System.Collections.IDictionary.
	IDictionaryEnumerator IDictionary.GetEnumerator()
	{
		return new PublicDictionaryEnumerator<TKey, TValue>(this, 1);
	}

	//
	// Summary:
	//     Removes the element with the specified key from the System.Collections.IDictionary.
	//
	// Parameters:
	//   key:
	//     The key of the element to remove.
	//
	// Exceptions:
	//   T:System.ArgumentNullException:
	//     key is null.
	void IDictionary.Remove(object key)
	{
		if (IsCompatibleKey(key))
		{
			Remove((TKey)key);
		}
	}


}
