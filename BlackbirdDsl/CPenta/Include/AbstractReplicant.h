#pragma once
#include "pch.h"
#include "AbstruseReplicant.h"

// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)



using namespace System::Collections::Generic;



namespace C5 {



// =========================================================================================================
//										AbstractReplicant Class
//
/// Method definitions class for Replicant.
/// Declares additional properties and operators as abtract not defined by AbstruseReplicant.
/// Defines utility and worker methods utilized by AbstruseReplicant.
/// T: The type of the value stored in the array. In descendant Cell classes this would by the descendant
/// Replicant itself, ie. Cell<Cell<T>>.
// =========================================================================================================
template<typename T> public ref class AbstractReplicant abstract : public AbstruseReplicant<T>
{



// =========================================================================================================
#pragma region Constructors/Destructors - AbstractReplicant
// =========================================================================================================

public:

	// ---------------------------------------------------------------------------------
	/// Default .ctor
	// ---------------------------------------------------------------------------------
	AbstractReplicant() : AbstruseReplicant()
	{
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// .ctor with initial and growth capacity.
	/// </summary>
	/// <param name="capacity">
	/// Sets the initial and growth capacity. If capacity is less than one, the default
	/// _InitialCapacity will be used.
	/// </param>
	// ---------------------------------------------------------------------------------
	AbstractReplicant(int capacity) : AbstruseReplicant(capacity)
	{
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// .ctor with an initial T value element.
	/// </summary>
	/// <param name="capacity">
	/// Sets the initial and growth capacity. If capacity is less than one, the default
	/// _InitialCapacity will be used.
	/// </param>
	/// <param name="element"></param>
	// ---------------------------------------------------------------------------------
	AbstractReplicant(int capacity, T element) : AbstruseReplicant(capacity, element)
	{
	};


	// ---------------------------------------------------------------------------------
	/// .ctor initialized with an unnamed element collection.
	// ---------------------------------------------------------------------------------
	AbstractReplicant(ICollection<T>^ collection) : AbstruseReplicant(collection)
	{
	};


	// ---------------------------------------------------------------------------------
	/// .ctor initialized with a key and value element.
	// ---------------------------------------------------------------------------------
	AbstractReplicant(SysStr^ key, T element) : AbstruseReplicant(key, element)
	{
	};


	// ---------------------------------------------------------------------------------
	/// .ctor initialized with a named element collection.
	// ---------------------------------------------------------------------------------
	AbstractReplicant(ICollection<KeyValuePair<SysStr^, T>>^ collection) : AbstruseReplicant(collection)
	{
	};

	// ---------------------------------------------------------------------------------
	/// .ctor shallow copy constructor.
	// ---------------------------------------------------------------------------------
	AbstractReplicant(AbstractReplicant^ collection) : AbstruseReplicant(collection)
	{
	};

#pragma endregion Constructors/Destructors





// =========================================================================================================
#pragma region Method Implementations - AbstractReplicant
// =========================================================================================================

protected:



	// ---------------------------------------------------------------------------------
	/// Creates the next valid dictionary key for elements without a key when the
	/// object is in a IsDictionary state. The hint index to convert to a string should
	/// be _CurrentSeed++.
	// ---------------------------------------------------------------------------------
	virtual SysStr^ CreateDictKey(int hint) override
	{
		int index = hint < _KeySeed ? _KeySeed : hint;

		SysStr^ key = System::Convert::ToString(index);
		_KeySeed = index + 1;

		if (Count == 0)
			return key;

		if (_UnaryKey == key)
		{
			key = System::Convert::ToString(++index);
			_KeySeed = index + 1;
			return key;
		}


		while (_Dict->ContainsKey(key))
			key = System::Convert::ToString(++index);

		_KeySeed = index + 1;

		return key;
	};



	// ---------------------------------------------------------------------------------
	/// Determines if a T value is null or empty.
	// ---------------------------------------------------------------------------------
	virtual bool GetIsNullOrEmpty(T value) override
	{
		return (IsNullPtr(value) || value->ToString() == "");
	};



	// ---------------------------------------------------------------------------------
	/// Given a string value, determines if it can be converted to a valid integer
	/// in the range of the current element collection or plus one to add it to
	/// the end of the collection.
	// ---------------------------------------------------------------------------------
	virtual int GetValidIndex(SysStr^ value) override
	{
		if (Count == 0)
			return -1;

		int index = -1;

		try
		{
			index = System::Convert::ToInt32(value);
		}
		catch (System::Exception^)
		{
			return -1;
		}

		if (index < 0 || index >= Count)
			return -1;

		return index;
	};



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Migrates from a unary container to a collection if newCapacity warrants it.
	/// Warning: If newCapacity is -1 OR is zero and Count is zero this method
	/// will clear all containers for garbage collect.
	/// </summary>
	/// <param name="newCapacity">
	/// The logic here is tight around 1 and 2:
	/// That means it depends on the requested capacity being no less then 1 or 2 if 1 or 2 is going to be
	/// required repectively. Not adhering to this will cause an exception.
	/// Further it depends on the requested capacity being no more than 1 if 1 is going to be required.
	/// Requesting more than 1 on a requirement of 1 will cause instantiation of arrays and affect performance. 
	/// </param>
	// ---------------------------------------------------------------------------------
	virtual void MigrateToArray(int newCapacity) override
	{
		// Sanity check.
		if (_UnaryKey != nullptr || _Dict != nullptr)
		{
			MigrateToDict(newCapacity);
			return;
		}

		// Calculate the final capacity required.
		if (newCapacity < Count)
			newCapacity = Count;

		// If it's zero we can delete all objects for garbage collect and exit.
		if (newCapacity <= 0)
		{
			Clear();
			return;
		}

		// Whatever the setting of newCapacity, if the capacity required is greater than 1
		// an existing _UnaryValue will have to move over to an array collection otherwise it can
		// remain.
		// If it's already an array we're also done.
		if (_Items != nullptr || newCapacity == 1)
			return;

		// At this point we have to create an array and possibly migrate any unary value.



		// Increase the capacity required by the initial capacity less one for the extra one the caller
		// possibly added.
		newCapacity += (_InitialCapacity - 1);

		// Create the array.
		_Items = gcnew List<T>(newCapacity);

		// If no unary value exists we're done.
		if (Count == 0)
			return;

		_CountFlag = 1;

		_Items->Add(_UnaryValue);
		_UnaryValue = nullptr;

		// And done.
		return;

	};



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Migrates the collection from an unnamed array to a dictionary.
	/// If the collection is already a named pair using _UnaryKey the pair will be moved
	/// to the dictionary collection if newCapacity > 1.
	/// Warning: If newCapacity is -1 OR is zero and Count is zero this method
	/// will clear all containers for garbage collect.
	/// </summary>
	/// <param name="newCapacity">
	/// The logic here is tight around 1 and 2:
	/// That means it depends on the requested capacity being no less then 1 or 2 if 1 or 2 is going to be
	/// required repectively. Not adhering to this will cause an exception.
	/// Further it depends on the requested capacity being no more than 1 if 1 is going to be required.
	/// Requesting more than 1 on a requirement of 1 will cause instantiation of arrays and affect performance. 
	/// </param>
	// ---------------------------------------------------------------------------------
	virtual void MigrateToDict(int newCapacity) override
	{
		// Calculate the final capacity required.
		if (newCapacity < Count)
			newCapacity = Count;

		// If it's zero we can delete all objects for garbage collect and exit.
		if (newCapacity <= 0)
		{
			Clear();
			return;
		}

		// If the newCapacity required is greater than 1 any existing _UnaryKey will have to move
		// over to a dictionary collection otherwise it can remain.
		// If newCapacity == 1 it means there was nothing before => no migration necessary
		// because the single slot required will be assigned to the unary containers.
		// If it's already a dict we're also done.
		if (_Dict != nullptr || newCapacity == 1)
			return;


		// At this point we have to create a dict and possibly migrate.


		// Increase the capacity required by the initial capacity less one for the extra one the caller
		// possibly added.
		newCapacity += (_InitialCapacity - 1);

		_Xref = gcnew List<SysStr^>(newCapacity);
		_Dict = gcnew Dictionary<SysStr^, T>(newCapacity, System::StringComparer::OrdinalIgnoreCase);

		// The running name of the dict key
		SysStr^ key;

		// First off move the unary key and value if they exist.
		if (IsUnary)
		{
			_CountFlag = 1;

			// The unary value was an oridinal
			if (_UnaryKey == nullptr)
				key = CreateDictKey(_CurrentSeed++);
			else
				key = _UnaryKey;

			_Xref->Add(key);
			_Dict->Add(key, _UnaryValue);

			_UnaryKey = nullptr;
			_UnaryValue = nullptr;

			// And done.
			return;
		}


		// If we got here it means original count > 0 and it's not IsDictionary and not IsUnary
		// therefore it can only be that _Items contains the one or more elements.
		// Ordinals (unnamed objects with an auto-created keys) always come first so that they can
		// still be accessed through their original ordinal index value.

		// _CountFlag == _Items->Count will remain.

		for (int i = 0; i < _CountFlag; i++)
		{
			key = CreateDictKey(_CurrentSeed++);

			_Xref->Add(key);
			_Dict->Add(key, _Items[i]);
		}

		_Items = nullptr;

	};



	// ---------------------------------------------------------------------------------
	/// Performs an element insert and assumes that all parameters passed are valid to
	/// perform an insert without any validation,
	// ---------------------------------------------------------------------------------
	virtual T UnsafeDictionaryInsert(int index, int previous, SysStr^ key, T value) override
	{
		// We have the insert position 'index'. Now whatever the case if the key == nullptr
		// it's an ordinal element so:
		// 1. if index == _CurrentSeed it's in the correct ordinal spot so _CurrentSeed++.
		// 2. if index < _CurrentSeed then it's bumbing up the ordinals so _CurrentSeed++.
		// 3. if index > _CurrentSeed it's messed up our ordinal sequence and we have to go
		//    _CurrentSeed = index + 1.
		// Let's do that...

		if (key == nullptr)
		{
			if (index <= _CurrentSeed)
				_CurrentSeed++;
			else if (index > _CurrentSeed)
				_CurrentSeed = index + 1;

		}

		if (previous == -1)
		{
			if (key == nullptr)
				key = CreateDictKey(index);

			if (Count == 0 && _Dict == nullptr)
			{
				_UnaryKey = key;
				_UnaryValue = value;
				_CountFlag = -1;
				return value;
			}

			_Xref->Add(key);
			_Dict->Add(key, value);
			_CountFlag++;
		}
		else
		{
			// There is an existing element

			// Same spot - it's been located on or just above itself.
			if (index == previous || index == previous + 1)
			{
				_Dict[key] = value;
				return value;
			}

			_Xref->RemoveAt(previous);

			// If the next ordinal index is above the deleted spot
			// decrement the ordinal.
			if (_CurrentSeed > previous)
				_CurrentSeed--;

			// Do the same for the insertion point
			if (index > previous)
				index--;

			if (key == nullptr)
				key = CreateDictKey(index);
			_Dict[key] = value;

		}

		// Now insert the xref
		if (index == _Xref->Count)
			_Xref->Add(key);
		else
			_Xref->Insert(index, key);

		return value;
	};



public:



	// ---------------------------------------------------------------------------------
	/// Performs a shallow (non-destructive) copy of a Replicant into a new Replicant.
	/// A shallow copy creates a mirror image of the source Replicant, preserving object
	/// references.
	// ---------------------------------------------------------------------------------
	virtual SysObj^ Clone() override
	{
		SysObj^ clone = CreateInstance();
		Clone(clone);

		return clone;
	};


	// ---------------------------------------------------------------------------------
	/// Performs an insert of the key value pair at the provided index.
	/// If the key already exists it is replaced if the index is the same or plus one,
	/// else it is removed.
	/// This object is migrated to a dictionary if required.
	// ---------------------------------------------------------------------------------
	virtual void Insert(int index, SysStr^ key, T value) override
	{
		// Sanity check.
		if (key == nullptr && !IsDictionary)
		{
			Insert(index, value);
			return;
		}

		// If there are no elements it can only be a unary operation
		if (Count == 0 && index == 0)
		{
			if (key == nullptr)
				key = CreateDictKey(_CurrentSeed++);

			_UnaryKey = key;
			_UnaryValue = value;
			_CountFlag = -1;
			return;
		}
		// If we have matching keys on a unary pair and index is 0 it's a unary value replacement
		if (index >= 0 && index < 2 && _UnaryKey != nullptr && key == _UnaryKey)
		{
			_UnaryValue = value;
			return;
		}


		// k is the previous index of 'key' if it exists. 
		int k = -1;

		if (key != nullptr)
		{
			if (_Xref != nullptr)
				k = (_Xref->Count > 0 ? _Xref->IndexOf(key) : -1);
			else if (_UnaryKey == key)
				k = 0;
		}

		int newCapacity = Count + (k == -1 ? 1 : 0);


		if (index < 0 || index >= newCapacity)
		{
			System::ArgumentOutOfRangeException^ ex = gcnew System::ArgumentOutOfRangeException(
				SysStr::Format("Attempted insert{0} at index {1} into array with {2} items.",
					k != -1 ? "-replacement" : "", index, Count));

			Diag::Dug(ex);
			throw ex;
		}

		// Prep dict if needed.
		MigrateToDict(newCapacity);


		// We have everything - perform the insert
		UnsafeDictionaryInsert(index, k, key, value);

	};


	virtual bool InArray(List<T>^ list) override
	{
		if (Count == 0 || list == nullptr || list->Count < Count)
			return false;


		T srcVal = nullptr;
		T destVal = nullptr;

		/*
		for each (srcVal in (IList<T>^)this)
		{
			for each (destVal in list)
			{
				if (srcVal == destVal)
					return true;
			}
		}
		*/
		return false;

	};


	// ---------------------------------------------------------------------------------
	/// Searches the string value of the T elements and inserts the new value
	/// at the correct ignore-case position. If the status is IsDictionary
	/// will call InsertAfter for a dictionary element with a nullptr key.
	// ---------------------------------------------------------------------------------
	virtual T InsertAfter(SysStr^ search, T value) override
	{
		// Sanity check.
		if (IsDictionary)
			return InsertAfter(search, nullptr, value);


		MigrateToArray(Count + 1);

		if (_Items == nullptr)
		{
			_UnaryValue = value;
			_CountFlag = -1;
			return value;
		}

		int index;
		SysStr^ str;

		for (index = 0; index < _Items->Count; index++)
		{
			str = System::Convert::ToString(_Items[index]);

			if (str == nullptr)
				continue;

			if (search == nullptr)
				break;

			if (str->CompareTo(search) < 0)
				break;
		}

		if (index == _Items->Count)
			_Items->Add(value);
		else
			_Items->Insert(index, value);

		_CountFlag++;

		return value;
	};



	// ---------------------------------------------------------------------------------
	/// Searches the string value of the T elements and inserts the new pair
	/// at the correct ignore-case position. If the status is not IsDictionary will
	/// migrate to a dictionary first.
	/// If the element is unnamed the value of 'key' will be nullptr and a key will be
	/// created based on _CurrentSeed and _KeySeed.
	// ---------------------------------------------------------------------------------
	virtual T InsertAfter(SysStr^ search, SysStr^ key, T value) override
	{
		// Sanity check.
		if (key == nullptr && !IsDictionary)
			return InsertAfter(search, value);

		// If we have no elements it's a unary operation
		if (Count == 0)
		{
			if (key == nullptr)
				key = CreateDictKey(_CurrentSeed++);
			_UnaryKey = key;
			_UnaryValue = value;
			_CountFlag = -1;
			return value;
		}
		// If we have matching keys on a unary pair it's a unary value replacement
		if (_UnaryKey != nullptr && key == _UnaryKey)
		{
			_UnaryValue = value;
			return value;
		}


		// k is the previous index of 'key' if it exists. 
		int k = -1;

		if (key != nullptr)
		{
			if (_Xref != nullptr)
				k = (_Xref->Count > 0 ? _Xref->IndexOf(key) : -1);
			else if (_UnaryKey == key)
				k = 0;
		}


		int index;
		SysStr^ str;

		// Now loop through each of the values and locate the position of the insert
		// The values aren't ordered and the keys are ordered in natural order,
		// so it's first come first serve.
		for (index = 0; index < _Xref->Count; index++)
		{
			if (index == k)
				continue;

			str = System::Convert::ToString(_Dict[_Xref[index]]);

			if (str == nullptr)
				continue;

			if (search == nullptr)
				break;

			if (str->CompareTo(search) < 0)
				break;
		}

		int newCapacity = Count + (k == -1 ? 1 : 0);

		// Prep dict if needed.
		MigrateToDict(newCapacity);

		// We have everything - perform the insert
		return UnsafeDictionaryInsert(index, k, key, value);

	};



	// ---------------------------------------------------------------------------------
	/// Returns the ordinal index of the key else -1. If the list is unnamed
	/// converts the key to an integer. If the converted key is within the index range
	/// of the list returns it else returns -1.
	// ---------------------------------------------------------------------------------
	virtual int KeyIndexOf(SysStr^ key) override
	{
		if (Count == 0)
			return -1;

		if (IsList)
		{
			int index = GetValidIndex(key);

			if (index < 0 || index >= Count)
				return -1;

			return index;
		}

		if (IsUnary)
		{
			if (key == _UnaryKey)
				return 0;

			return -1;
		}

		if (IsDictionary)
		{
			return _Xref->IndexOf(key);
		}

		return -1;
	}



	// ---------------------------------------------------------------------------------
	/// Returns the key of the value else nullptr.
	/// If the list is an unnamed list returns the index cast to SysStr.
	// ---------------------------------------------------------------------------------
	virtual SysStr^ KeyOf(SysStr^ value) override
	{
		if (Count == 0)
			return nullptr;

		if (IsUnary && IsDictionary)
			return (_UnaryValue->ToString() == value ? _UnaryKey : nullptr);

		if (IsDictionary)
		{
			for each (KeyValuePair<SysStr^, T> ^ pair in _Dict)
			{
				if (value == pair->Value->ToString())
					return pair->Key;
			}

			return nullptr;
		}

		int i = _Xref->IndexOf(value);

		if (i == -1)
			return nullptr;

		return System::Convert::ToString(i);

	};


	// ---------------------------------------------------------------------------------
	/// Performs a php array merge.
	// ---------------------------------------------------------------------------------
	virtual void Merge(IReplicant<T>^ value) override
	{
		AbstractReplicant^ replicant = (AbstractReplicant^)value;

		if (replicant->IsDictionary)
		{
			for each (KeyValuePair<SysStr^, T> pair in replicant)
				Add(pair);
		}
		else if (replicant->IsList)
		{
			for each (T item in replicant->Enumerator)
				Add(item);
		}
	};



	// ---------------------------------------------------------------------------------
	/// Removes the value of the key and or index and returns true if successful.
	/// Either index must not be -1 or key must not be nullptr or both may be specified.
	/// If the list is unnamed converts the key to an integer if it's not nullptr.
	/// If the converted key is within the index range of the list
	/// removes the value at that index and returns true.
	// ---------------------------------------------------------------------------------
	virtual bool Remove(int index, SysStr^ key) override
	{
		if (Count == 0 || !IsDictionary || index < -1 || index >= Count)
			return false;

		if (IsUnary)
		{
			if ((index == -1 && (key == nullptr || key != _UnaryKey))
				|| (index == 0 && key != nullptr && key != _UnaryKey))
			{
				return false;
			}

			Clear();

			return true;
		}

		if (index == -1)
			index = _Xref->IndexOf(key);

		if (index == -1)
			return false;

		if (key == nullptr)
			key = _Xref[index];


		_Dict->Remove(key);
		_Xref->RemoveAt(index);

		if (_Xref->Count == 0)
		{
			Clear();
			return true;
		}

		if (_CurrentSeed > index)
		{
			if (_CurrentSeed == _KeySeed)
				_KeySeed--;
			_CurrentSeed--;
		}

		_CountFlag = _Xref->Count;

		return true;
	};



	// ---------------------------------------------------------------------------------
	/// Tries to get the value at "index" and places it in "value" returning true else
	/// returns false.
	// ---------------------------------------------------------------------------------
	virtual bool TryGetIndexedValue(int index, T% value) override
	{
		if (index < 0 || index >= Count)
			return false;

		if (IsUnary)
			value = _UnaryValue;
		else if (_Xref != nullptr)
			value = _Dict[_Xref[index]];
		else if (_Items != nullptr)
			value = _Items[index];

		return true;
	};



	// ---------------------------------------------------------------------------------
	/// Tries to convert a string segment to an unsigned integer then tries to get the
	/// indexed value.
	// ---------------------------------------------------------------------------------
	virtual bool TryGetIndexedValue(SysStr^ segment, T% value) override
	{
		int index = GetValidIndex(segment);

		return TryGetIndexedValue(index, value);
	};


#pragma endregion Method Implementations


};

}