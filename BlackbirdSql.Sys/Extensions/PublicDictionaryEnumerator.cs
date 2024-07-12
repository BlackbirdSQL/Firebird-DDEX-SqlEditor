using System;
using System.Collections;
using System.Collections.Generic;



namespace BlackbirdSql.Sys.Extensions;


public struct PublicDictionaryEnumerator<TKey, TValue> : IEnumerator<KeyValuePair<TKey, TValue>>, IDisposable, IEnumerator, IDictionaryEnumerator
{

	internal PublicDictionaryEnumerator(PublicDictionary<TKey, TValue> dictionary, int getEnumeratorRetType)
	{
		_Dictionary = dictionary;
		_Index = 0;
		_GetEnumeratorRetType = getEnumeratorRetType;
		_Current = default;
	}




	private readonly PublicDictionary<TKey, TValue> _Dictionary;

	private int _Index;

	private KeyValuePair<TKey, TValue> _Current;

	private readonly int _GetEnumeratorRetType;

	internal const int C_DictEntry = 1;

	internal const int C_KeyValuePair = 2;

	//
	// Summary:
	//     Gets the element at the _Current position of the enumerator.
	//
	// Returns:
	//     The element in the BlackbirdSql.Sys.Extensions.PublicDictionary at the _Current position
	//     of the enumerator.
#pragma warning disable IDE0251 // Make member 'readonly'
	public KeyValuePair<TKey, TValue> Current => _Current;
#pragma warning restore IDE0251 // Make member 'readonly'


	//
	// Summary:
	//     Gets the element at the _Current position of the enumerator.
	//
	// Returns:
	//     The element in the collection at the _Current position of the enumerator, as an
	//     System.Object.
	//
	// Exceptions:
	//   T:System.InvalidOperationException:
	//     The enumerator is positioned before the first element of the collection or after
	//     the last element.
	object IEnumerator.Current
	{
		get
		{
			if (_Index == 0 || _Index == _Dictionary.RawCount + 1)
			{
				InvalidOperationException ex = new("InvalidOperation_EnumOpCantHappen");
				Diag.Dug(ex);
				throw ex;
			}

			if (_GetEnumeratorRetType == 1)
			{
				return new DictionaryEntry(_Current.Key, _Current.Value);
			}

			return new KeyValuePair<TKey, TValue>(_Current.Key, _Current.Value);
		}
	}

	//
	// Summary:
	//     Gets the element at the _Current position of the enumerator.
	//
	// Returns:
	//     The element in the _Dictionary at the _Current position of the enumerator, as a
	//     System.Collections.DictionaryEntry.
	//
	// Exceptions:
	//   T:System.InvalidOperationException:
	//     The enumerator is positioned before the first element of the collection or after
	//     the last element.
	DictionaryEntry IDictionaryEnumerator.Entry
	{
		get
		{
			if (_Index == 0 || _Index == _Dictionary.RawCount + 1)
			{
				InvalidOperationException ex = new("InvalidOperation_EnumOpCantHappen");
				Diag.Dug(ex);
				throw ex;
			}

			return new DictionaryEntry(_Current.Key, _Current.Value);
		}
	}

	//
	// Summary:
	//     Gets the key of the element at the _Current position of the enumerator.
	//
	// Returns:
	//     The key of the element in the _Dictionary at the _Current position of the enumerator.
	//
	// Exceptions:
	//   T:System.InvalidOperationException:
	//     The enumerator is positioned before the first element of the collection or after
	//     the last element.
	object IDictionaryEnumerator.Key
	{
		get
		{
			if (_Index == 0 || _Index == _Dictionary.RawCount + 1)
			{
				InvalidOperationException ex = new("InvalidOperation_EnumOpCantHappen");
				Diag.Dug(ex);
				throw ex;
			}

			return _Current.Key;
		}
	}

	//
	// Summary:
	//     Gets the value of the element at the _Current position of the enumerator.
	//
	// Returns:
	//     The value of the element in the _Dictionary at the _Current position of the enumerator.
	//
	// Exceptions:
	//   T:System.InvalidOperationException:
	//     The enumerator is positioned before the first element of the collection or after
	//     the last element.
	object IDictionaryEnumerator.Value
	{
		get
		{
			if (_Index == 0 || _Index == _Dictionary.RawCount + 1)
			{
				InvalidOperationException ex = new("InvalidOperation_EnumOpCantHappen");
				Diag.Dug(ex);
				throw ex;
			}
			return _Current.Value;
		}
	}





	//
	// Summary:
	//     Advances the enumerator to the next element of the BlackbirdSql.Sys.Extensions.PublicDictionary.
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
				_Current = new KeyValuePair<TKey, TValue>(_Dictionary.Entries[_Index].Key, _Dictionary.Entries[_Index].Value);
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
	//     Releases all resources used by the BlackbirdSql.Sys.Extensions.PublicDictionary.Enumerator.
#pragma warning disable IDE0251 // Make member 'readonly'
	public void Dispose()
#pragma warning restore IDE0251 // Make member 'readonly'
	{
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
		_Current = default;
	}
}
