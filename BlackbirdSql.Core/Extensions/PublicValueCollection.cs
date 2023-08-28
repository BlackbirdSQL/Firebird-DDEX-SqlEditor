using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;




namespace BlackbirdSql.Core.Extensions;


//
// Summary:
//     Represents the collection of values in a BlackbirdSql.Core.Extensions.PublicDictionary.
//     This class cannot be inherited.
[Serializable]
[DebuggerDisplay("Count = {Count}")]
public sealed class PublicValueCollection<TKey, TValue> : ICollection<TValue>, IEnumerable<TValue>, IEnumerable, ICollection, IReadOnlyCollection<TValue>
{
	//
	// Summary:
	//     Enumerates the elements of a BlackbirdSql.Core.Extensions.PublicValueCollection.
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
		//     The element in the BlackbirdSql.Core.Extensions.PublicValueCollection at
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
					InvalidOperationException ex = new("EnumOpCantHappen");
					Diag.Dug(ex);
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
		//     Releases all resources used by the BlackbirdSql.Core.Extensions.PublicValueCollection.Enumerator.
#pragma warning disable IDE0251 // Make member 'readonly'
		public void Dispose()
#pragma warning restore IDE0251 // Make member 'readonly'
		{
		}

		//
		// Summary:
		//     Advances the enumerator to the next element of the BlackbirdSql.Core.Extensions.PublicValueCollection.
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
	//     Gets the number of elements contained in the BlackbirdSql.Core.Extensions.PublicValueCollection.
	//
	// Returns:
	//     The number of elements contained in the BlackbirdSql.Core.Extensions.PublicValueCollection.
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
	//     safe); otherwise, false. In the default implementation of BlackbirdSql.Core.Extensions.PublicValueCollection,
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
	//     In the default implementation of BlackbirdSql.Core.Extensions.PublicValueCollection,
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
	//     Initializes a new instance of the BlackbirdSql.Core.Extensions.PublicValueCollection
	//     class that reflects the values in the specified BlackbirdSql.Core.Extensions.PublicDictionary.
	//
	// Parameters:
	//   _Dictionary:
	//     The BlackbirdSql.Core.Extensions.PublicDictionary whose values are reflected in the
	//     new BlackbirdSql.Core.Extensions.PublicValueCollection.
	//
	// Exceptions:
	//   T:System.ArgumentNullException:
	//     _Dictionary is null.
	public PublicValueCollection(PublicDictionary<TKey, TValue> _Dictionary)
	{
		if (_Dictionary == null)
		{
			ArgumentNullException ex = new("_Dictionary");
			Diag.Dug(ex);
			throw ex;
		}

		this._Dictionary = _Dictionary;
	}

	//
	// Summary:
	//     Returns an enumerator that iterates through the BlackbirdSql.Core.Extensions.PublicValueCollection.
	//
	// Returns:
	//     A BlackbirdSql.Core.Extensions.PublicValueCollection.Enumerator for the
	//     BlackbirdSql.Core.Extensions.PublicValueCollection.
	public Enumerator GetEnumerator()
	{
		return new Enumerator(_Dictionary);
	}

	//
	// Summary:
	//     Copies the BlackbirdSql.Core.Extensions.PublicValueCollection elements to
	//     an existing one-dimensional System.Array, starting at the specified array _Index.
	//
	// Parameters:
	//   array:
	//     The one-dimensional System.Array that is the destination of the elements copied
	//     from BlackbirdSql.Core.Extensions.PublicValueCollection. The System.Array
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
	//     The number of elements in the source BlackbirdSql.Core.Extensions.PublicValueCollection
	//     is greater than the available space from _Index to the end of the destination
	//     array.
	public void CopyTo(TValue[] array, int _Index)
	{
		if (array == null)
		{
			ArgumentNullException ex = new("array");
			Diag.Dug(ex);
			throw ex;
		}

		if (_Index < 0 || _Index > array.Length)
		{
			ArgumentOutOfRangeException ex = new("_Index is negative");
			Diag.Dug(ex);
			throw ex;
		}

		if (array.Length - _Index < _Dictionary.Count)
		{
			ArgumentException ex = new("Arg_ArrayPlusOffTooSmall");
			Diag.Dug(ex);
			throw ex;
		}

		int count = _Dictionary.RawCount;
		PublicDictionary<TKey, TValue>.Entry[] entries = _Dictionary.Entries;
		for (int i = 0; i < count; i++)
		{
			if (entries[i].HashCode >= 0)
			{
				array[_Index++] = entries[i].Value;
			}
		}
	}

	void ICollection<TValue>.Add(TValue item)
	{
		NotSupportedException ex = new("NotSupported_ValueCollectionSet");
		Diag.Dug(ex);
	}

	bool ICollection<TValue>.Remove(TValue item)
	{
		NotSupportedException ex = new("NotSupported_ValueCollectionSet");
		Diag.Dug(ex);
		return false;
	}

	void ICollection<TValue>.Clear()
	{
		NotSupportedException ex = new("NotSupported_ValueCollectionSet");
		Diag.Dug(ex);
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
	void ICollection.CopyTo(Array array, int _Index)
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

		if (_Index < 0 || _Index > array.Length)
		{
			ArgumentOutOfRangeException ex = new("_Index is negative");
			Diag.Dug(ex);
			throw ex;
		}

		if (array.Length - _Index < _Dictionary.Count)
		{
			ArgumentException ex = new("Arg_ArrayPlusOffTooSmall");
			Diag.Dug(ex);
			throw ex;
		}

		if (array is TValue[] array2)
		{
			CopyTo(array2, _Index);
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
					array3[_Index++] = entries[i].Value;
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
