using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using BlackbirdSql.Sys.Properties;



namespace BlackbirdSql.Sys.Extensions;

[Serializable]
[DebuggerDisplay("Count = {Count}")]


//
// Summary:
//     Represents the collection of values in a BlackbirdSql.Sys.Extensions.PublicDictionary.
//     This class cannot be inherited.
public sealed class PublicValueCollection<TKey, TValue> : ICollection<TValue>, IEnumerable<TValue>, IEnumerable, ICollection, IReadOnlyCollection<TValue>
{

	//
	// Summary:
	//     Initializes a new instance of the BlackbirdSql.Sys.Extensions.PublicValueCollection
	//     class that reflects the values in the specified BlackbirdSql.Sys.Extensions.PublicDictionary.
	//
	// Parameters:
	//   _Dictionary:
	//     The BlackbirdSql.Sys.Extensions.PublicDictionary whose values are reflected in the
	//     new BlackbirdSql.Sys.Extensions.PublicValueCollection.
	//
	// Exceptions:
	//   T:System.ArgumentNullException:
	//     _Dictionary is null.
	public PublicValueCollection(PublicDictionary<TKey, TValue> dictionary)
	{
		if (dictionary == null)
		{
			ArgumentNullException ex = new(nameof(dictionary));
			Diag.Ex(ex);
			throw ex;
		}

		_Dictionary = dictionary;
	}



	//
	// Summary:
	//     Enumerates the elements of a BlackbirdSql.Sys.Extensions.PublicValueCollection.
	[Serializable]
	public struct Enumerator : IEnumerator<TValue>, IDisposable, IEnumerator
	{
		private readonly PublicDictionary<TKey, TValue> _Dictionary;
		private int _Index;

		private TValue _Current;




		//
		// Summary:
		//     Gets the element at the current position of the enumerator.
		//
		// Returns:
		//     The element in the BlackbirdSql.Sys.Extensions.PublicValueCollection at
		//     the current position of the enumerator.
#pragma warning disable IDE0251 // Make member 'readonly'
		public TValue Current => _Current;
#pragma warning restore IDE0251 // Make member 'readonly'


#pragma warning disable IDE0251 // Make member 'readonly'
		public int Index => _Index;
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
					InvalidOperationException ex = new(Resources.ExceptionEnumerationFailed);
					Diag.Ex(ex);
					throw ex;
				}
				return _Current;
			}
		}

		internal Enumerator(PublicDictionary<TKey, TValue> _Dictionary)
		{
			this._Dictionary = _Dictionary;
			_Index = 0;
			_Current = default;
		}

		//
		// Summary:
		//     Releases all resources used by the BlackbirdSql.Sys.Extensions.PublicValueCollection.Enumerator.
#pragma warning disable IDE0251 // Make member 'readonly'
		public void Dispose()
#pragma warning restore IDE0251 // Make member 'readonly'
		{
		}

		//
		// Summary:
		//     Advances the enumerator to the next element of the BlackbirdSql.Sys.Extensions.PublicValueCollection.
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
					_Current = _Dictionary.Entries[_Index].Value;
					_Index++;
					return true;
				}

				_Index++;
			}

			_Index = _Dictionary.RawCount + 1;
			_Current = default;
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
		public void Reset()
		{
			_Index = 0;
			_Current = default;
		}
	}

	private readonly PublicDictionary<TKey, TValue> _Dictionary;

	//
	// Summary:
	//     Gets the number of elements contained in the BlackbirdSql.Sys.Extensions.PublicValueCollection.
	//
	// Returns:
	//     The number of elements contained in the BlackbirdSql.Sys.Extensions.PublicValueCollection.
	public int Count
	{
		get
		{
			return _Dictionary.Count;
		}
	}

	bool ICollection<TValue>.IsReadOnly
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
	//     safe); otherwise, false. In the default implementation of BlackbirdSql.Sys.Extensions.PublicValueCollection,
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
	//     In the default implementation of BlackbirdSql.Sys.Extensions.PublicValueCollection,
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
	//     Returns an enumerator that iterates through the BlackbirdSql.Sys.Extensions.PublicValueCollection.
	//
	// Returns:
	//     A BlackbirdSql.Sys.Extensions.PublicValueCollection.Enumerator for the
	//     BlackbirdSql.Sys.Extensions.PublicValueCollection.
	public Enumerator GetEnumerator()
	{
		return new Enumerator(_Dictionary);
	}

	//
	// Summary:
	//     Copies the BlackbirdSql.Sys.Extensions.PublicValueCollection elements to
	//     an existing one-dimensional System.Array, starting at the specified array _Index.
	//
	// Parameters:
	//   array:
	//     The one-dimensional System.Array that is the destination of the elements copied
	//     from BlackbirdSql.Sys.Extensions.PublicValueCollection. The System.Array
	//     must have zero-based indexing.
	//
	//   _Index:
	//     The zero-based _Index in array at which copying begins.
	//
	// Exceptions:
	//   T:System.ArgumentNullException:
	//     array is null.
	//
	//   T:System.ArgumentOutOfRangeException:
	//     _Index is less than zero.
	//
	//   T:System.ArgumentException:
	//     The number of elements in the source BlackbirdSql.Sys.Extensions.PublicValueCollection
	//     is greater than the available space from _Index to the end of the destination
	//     array.
	public void CopyTo(TValue[] array, int index)
	{
		if (array == null)
		{
			ArgumentNullException ex = new(nameof(array));
			Diag.Ex(ex);
			throw ex;
		}

		if (index < 0 || index > array.Length)
		{
			ArgumentOutOfRangeException ex = new(Resources.ExceptionCopyToIndexOutOfRange.Fmt(index, array.Length));
			Diag.Ex(ex);
			throw ex;
		}

		if (array.Length - index < _Dictionary.Count)
		{
			ArgumentException ex = new(Resources.ExceptionCopyToArrayTooSmall.Fmt(array.Length, _Dictionary.Count, index));
			Diag.Ex(ex);
			throw ex;
		}

		int count = _Dictionary.RawCount;
		PublicDictionary<TKey, TValue>.Entry[] entries = _Dictionary.Entries;
		for (int i = 0; i < count; i++)
		{
			if (entries[i].HashCode >= 0)
				array[index++] = entries[i].Value;
		}
	}

	void ICollection<TValue>.Add(TValue item)
	{
		NotSupportedException ex = new("Add(TValue item)");
		Diag.Ex(ex);
	}

	bool ICollection<TValue>.Remove(TValue item)
	{
		NotSupportedException ex = new("Remove(TValue item)");
		Diag.Ex(ex);
		return false;
	}

	void ICollection<TValue>.Clear()
	{
		NotSupportedException ex = new("Clear()");
		Diag.Ex(ex);
	}

	bool ICollection<TValue>.Contains(TValue item)
	{
		return _Dictionary.ContainsValue(item);
	}

	IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
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
	//     starting at a particular System.Array _Index.
	//
	// Parameters:
	//   array:
	//     The one-dimensional System.Array that is the destination of the elements copied
	//     from System.Collections.ICollection. The System.Array must have zero-based indexing.
	//
	//   _Index:
	//     The zero-based _Index in array at which copying begins.
	//
	// Exceptions:
	//   T:System.ArgumentNullException:
	//     array is null.
	//
	//   T:System.ArgumentOutOfRangeException:
	//     _Index is less than zero.
	//
	//   T:System.ArgumentException:
	//     array is multidimensional.-or-array does not have zero-based indexing.-or-The
	//     number of elements in the source System.Collections.ICollection is greater than
	//     the available space from _Index to the end of the destination array.-or-The type
	//     of the source System.Collections.ICollection cannot be cast automatically to
	//     the type of the destination array.
	void ICollection.CopyTo(Array array, int index)
	{
		if (array == null)
		{
			ArgumentNullException ex = new(nameof(array));
			Diag.Ex(ex);
			throw ex;
		}

		if (array.Rank != 1)
		{
			ArgumentException ex = new(Resources.ExceptionCopyToRankMultiDimNotSupported);
			Diag.Ex(ex);
			throw ex;
		}

		if (array.GetLowerBound(0) != 0)
		{
			ArgumentException ex = new(Resources.ExceptionCopyToNonZeroLowerBound);
			Diag.Ex(ex);
			throw ex;
		}

		if (index < 0 || index > array.Length)
		{
			ArgumentOutOfRangeException ex = new(nameof(index), Resources.ExceptionCopyToIndexOutOfRange.Fmt(index, array.Length));
			Diag.Ex(ex);
			throw ex;
		}

		if (array.Length - index < _Dictionary.Count)
		{
			ArgumentException ex = new(Resources.ExceptionCopyToArrayTooSmall.Fmt(array.Length, _Dictionary.Count, index));
			Diag.Ex(ex);
			throw ex;
		}

		if (array is TValue[] array2)
		{
			CopyTo(array2, index);
			return;
		}

		if (array is not object[] array3)
		{
			ArgumentException ex = new(Resources.ExceptionCopyToInvalidArrayType.Fmt(array.GetType()), nameof(array));
			Diag.Ex(ex);
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
					array3[index++] = entries[i].Value;
				}
			}
		}
		catch (ArrayTypeMismatchException)
		{
			ArgumentException ex = new(Resources.ExceptionCopyToArrayTypeMismatch.Fmt(_Dictionary.Entries.GetType(), array.GetType()));
			Diag.Ex(ex);
			throw ex;
		}
	}
}
