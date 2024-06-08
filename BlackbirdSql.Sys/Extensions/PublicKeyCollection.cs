using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;



namespace BlackbirdSql.Sys.Extensions;


//
// Summary:
//     Represents the collection of keys in a System.Collections.Generic.Dictionary`2.
//     This class cannot be inherited.
[Serializable]
[DebuggerDisplay("Count = {Count}")]
public sealed class PublicKeyCollection<TKey, TValue> : ICollection<TKey>, IEnumerable<TKey>, IEnumerable, ICollection, IReadOnlyCollection<TKey>
{
	//
	// Summary:
	//     Enumerates the elements of a System.Collections.Generic.Dictionary`2.KeyCollection.
	[Serializable]
	public struct Enumerator : IEnumerator<TKey>, IDisposable, IEnumerator
	{
		private readonly PublicDictionary<TKey, TValue> _Dictionary;

		private int _Index;


		private TKey _CurrentKey;

		//
		// Summary:
		//     Gets the element at the current position of the enumerator.
		//
		// Returns:
		//     The element in the System.Collections.Generic.Dictionary`2.KeyCollection at the
		//     current position of the enumerator.
#pragma warning disable IDE0251 // Make member 'readonly'
		public TKey Current => _CurrentKey;
#pragma warning restore IDE0251 // Make member 'readonly'


		//
		// Summary:
		//     Gets the element at the current position of the enumerator.
		//
		// Returns:
		//     The element in the collection at the current position of the enumerator.
		//
		// Exceptions:
		//   T:System.InvalidOperationException:
		//     The enumerator is positioned before the first element of the collection or after
		//     the last element.
		object IEnumerator.Current
		{
#pragma warning disable IDE0251 // Make member 'readonly'
			get
#pragma warning restore IDE0251 // Make member 'readonly'
			{
				if (_Index == 0 || _Index == _Dictionary.RawCount + 1)
				{
					IndexOutOfRangeException ex = new("PublicKeyCollection::Current");
					Diag.Dug(ex);
					throw ex;
				}

				return _CurrentKey;
			}
		}

		internal Enumerator(PublicDictionary<TKey, TValue> dictionary)
		{
			_Dictionary = dictionary;
			_Index = 0;
			_CurrentKey = default;
		}

		//
		// Summary:
		//     Releases all resources used by the System.Collections.Generic.Dictionary`2.KeyCollection.Enumerator.
#pragma warning disable IDE0251 // Make member 'readonly'
		public void Dispose()
#pragma warning restore IDE0251 // Make member 'readonly'
		{
		}

		//
		// Summary:
		//     Advances the enumerator to the next element of the System.Collections.Generic.Dictionary`2.KeyCollection.
		//
		// Returns:
		//     true if the enumerator was successfully advanced to the next element; false if
		//     the enumerator has passed the end of the collection.
		//
		// Exceptions:
		//   T:System.InvalidOperationException:
		//     The collection was modified after the enumerator was created.
		public bool MoveNext()
		{
			while ((uint)_Index < (uint)_Dictionary.RawCount)
			{
				if (_Dictionary.Entries[_Index].HashCode >= 0)
				{
					_CurrentKey = _Dictionary.Entries[_Index].Key;
					_Index++;
					return true;
				}

				_Index++;
			}

			_Index = _Dictionary.RawCount + 1;
			_CurrentKey = default;
			return false;
		}

		//
		// Summary:
		//     Sets the enumerator to its initial position, which is before the first element
		//     in the collection.
		//
		// Exceptions:
		//   T:System.InvalidOperationException:
		//     The collection was modified after the enumerator was created.
		void IEnumerator.Reset()
		{
			_Index = 0;
			_CurrentKey = default;
		}
	}

	private readonly PublicDictionary<TKey, TValue> _Dictionary;

	//
	// Summary:
	//     Gets the number of elements contained in the System.Collections.Generic.Dictionary`2.KeyCollection.
	//
	// Returns:
	//     The number of elements contained in the System.Collections.Generic.Dictionary`2.KeyCollection.Retrieving
	//     the value of this property is an O(1) operation.
	public int Count
	{
		get
		{
			return _Dictionary.Count;
		}
	}

	bool ICollection<TKey>.IsReadOnly
	{
		get
		{
			return true;
		}
	}

	//
	// Summary:
	//     Gets a value indicating whether access to the System.Collections.ICollection
	//     is synchronized (thread safe).
	//
	// Returns:
	//     true if access to the System.Collections.ICollection is synchronized (thread
	//     safe); otherwise, false. In the default implementation of System.Collections.Generic.Dictionary`2.KeyCollection,
	//     this property always returns false.
	bool ICollection.IsSynchronized
	{
		get
		{
			return false;
		}
	}

	//
	// Summary:
	//     Gets an object that can be used to synchronize access to the System.Collections.ICollection.
	//
	// Returns:
	//     An object that can be used to synchronize access to the System.Collections.ICollection.
	//     In the default implementation of System.Collections.Generic.Dictionary`2.KeyCollection,
	//     this property always returns the current instance.
	object ICollection.SyncRoot
	{
		get
		{
			return ((ICollection)_Dictionary).SyncRoot;
		}
	}

	//
	// Summary:
	//     Initializes a new instance of the System.Collections.Generic.Dictionary`2.KeyCollection
	//     class that reflects the keys in the specified System.Collections.Generic.Dictionary`2.
	//
	// Parameters:
	//   dictionary:
	//     The System.Collections.Generic.Dictionary`2 whose keys are reflected in the new
	//     System.Collections.Generic.Dictionary`2.KeyCollection.
	//
	// Exceptions:
	//   T:System.ArgumentNullException:
	//     dictionary is null.
	public PublicKeyCollection(PublicDictionary<TKey, TValue> dictionary)
	{
		if (dictionary == null)
		{
			ArgumentNullException ex = new("Dictionary");
			Diag.Dug(ex);
			throw ex;
		}

		_Dictionary = dictionary;
	}

	//
	// Summary:
	//     Returns an enumerator that iterates through the System.Collections.Generic.Dictionary`2.KeyCollection.
	//
	// Returns:
	//     A System.Collections.Generic.Dictionary`2.KeyCollection.Enumerator for the System.Collections.Generic.Dictionary`2.KeyCollection.
	public Enumerator GetEnumerator()
	{
		return new Enumerator(_Dictionary);
	}

	//
	// Summary:
	//     Copies the System.Collections.Generic.Dictionary`2.KeyCollection elements to
	//     an existing one-dimensional System.Array, starting at the specified array index.
	//
	// Parameters:
	//   array:
	//     The one-dimensional System.Array that is the destination of the elements copied
	//     from System.Collections.Generic.Dictionary`2.KeyCollection. The System.Array
	//     must have zero-based indexing.
	//
	//   index:
	//     The zero-based index in array at which copying begins.
	//
	// Exceptions:
	//   T:System.ArgumentNullException:
	//     array is null.
	//
	//   T:System.ArgumentOutOfRangeException:
	//     index is less than zero.
	//
	//   T:System.ArgumentException:
	//     The number of elements in the source System.Collections.Generic.Dictionary`2.KeyCollection
	//     is greater than the available space from index to the end of the destination
	//     array.
	public void CopyTo(TKey[] array, int index)
	{
		if (array == null)
		{
			ArgumentNullException ex = new("CollectionEx::CopyTo array");
			Diag.Dug(ex);
			throw ex;
		}

		if (index < 0 || index > array.Length)
		{
			ArgumentOutOfRangeException ex = new("CollectionEx::CopyTo index");
			Diag.Dug(ex);
			throw ex;
		}

		if (array.Length - index < _Dictionary.Count)
		{
			ArgumentException ex = new("CollectionEx::CopyTo array to small");
			Diag.Dug(ex);
			throw ex;
		}

		int count = _Dictionary.RawCount;
		PublicDictionary<TKey, TValue>.Entry[] entries = _Dictionary.Entries;
		for (int i = 0; i < count; i++)
		{
			if (entries[i].HashCode >= 0)
			{
				array[index++] = entries[i].Key;
			}
		}
	}

	void ICollection<TKey>.Add(TKey item)
	{
		NotSupportedException ex = new("Add(TKey item) Not Supported");
		Diag.Dug(ex);
		throw ex;
	}

	void ICollection<TKey>.Clear()
	{
		NotSupportedException ex = new("Clear() Not Supported");
		Diag.Dug(ex);
		throw ex;
	}

	bool ICollection<TKey>.Contains(TKey item)
	{
		return _Dictionary.ContainsKey(item);
	}

	bool ICollection<TKey>.Remove(TKey item)
	{
		NotSupportedException ex = new("Remove(TKey item) Not Supported");
		Diag.Dug(ex);
		return false;
	}

	IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator()
	{
		return new Enumerator(_Dictionary);
	}

	//
	// Summary:
	//     Returns an enumerator that iterates through a collection.
	//
	// Returns:
	//     An System.Collections.IEnumerator that can be used to iterate through the collection.
	IEnumerator IEnumerable.GetEnumerator()
	{
		return new Enumerator(_Dictionary);
	}

	//
	// Summary:
	//     Copies the elements of the System.Collections.ICollection to an System.Array,
	//     starting at a particular System.Array index.
	//
	// Parameters:
	//   array:
	//     The one-dimensional System.Array that is the destination of the elements copied
	//     from System.Collections.ICollection. The System.Array must have zero-based indexing.
	//
	//   index:
	//     The zero-based index in array at which copying begins.
	//
	// Exceptions:
	//   T:System.ArgumentNullException:
	//     array is null.
	//
	//   T:System.ArgumentOutOfRangeException:
	//     index is less than zero.
	//
	//   T:System.ArgumentException:
	//     array is multidimensional.-or-array does not have zero-based indexing.-or-The
	//     number of elements in the source System.Collections.ICollection is greater than
	//     the available space from index to the end of the destination array.-or-The type
	//     of the source System.Collections.ICollection cannot be cast automatically to
	//     the type of the destination array.
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
			ArgumentException ex = new("RankMultiDimNotSupported");
			Diag.Dug(ex);
			throw ex;
		}

		if (array.GetLowerBound(0) != 0)
		{
			ArgumentException ex = new("NonZeroLowerBound");
			Diag.Dug(ex);
			throw ex;
		}

		if (index < 0 || index > array.Length)
		{
			ArgumentOutOfRangeException ex = new("ArgumentOutOfRange_NeedNonNegNum");
			Diag.Dug(ex);
			throw ex;
		}

		if (array.Length - index < _Dictionary.Count)
		{
			ArgumentException ex = new("ArrayPlusOffTooSmall");
			Diag.Dug(ex);
			throw ex;
		}

		if (array is TKey[] array2)
		{
			CopyTo(array2, index);
			return;
		}

		if (array is not object[] array3)
		{
			ArgumentException ex = new("Argument_InvalidArrayType");
			Diag.Dug(ex);
			throw ex;
		}

		int count = _Dictionary.RawCount;
		PublicDictionary<TKey, TValue>.Entry[] entries = _Dictionary.Entries;
		try
		{
			for (int i = 0; i < count; i++)
			{
				if (entries[i].HashCode >= 0)
				{
					array3[index++] = entries[i].Key;
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
}

