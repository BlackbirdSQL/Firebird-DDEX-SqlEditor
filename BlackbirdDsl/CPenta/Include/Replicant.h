#pragma once
#include "pch.h"
#include "AbstractReplicant.h"

// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)



using namespace System::Collections::Generic;



namespace C5 {



// =========================================================================================================
//											Replicant Class
//
/// <summary>
/// Property and operator definitions for the Replicant class.
/// Defines utility and worker methods utilized by AbstruseReplicant in implementing IDictionary
/// and IList interface methods.
/// T: The type of the value stored in the array. In descendant Cell classes this would by the descendant
/// Replicant itself, ie. Cell<Cell<T>>.
/// </summary>
// =========================================================================================================
template<typename T> public ref class Replicant : public AbstractReplicant<T>
{


#pragma region Property Accessors - Replicant

public:

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// In addition to IsNull, traverses elements for null or empty
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property bool ArrayNullOrEmpty
	{
		virtual bool get() override
		{
			if (Count == 0)
				return true;

			if (IsUnary)
				return GetIsNullOrEmpty(_UnaryValue);

			if (!IsICollection)
				return true;


			if (IsDictionary)
			{
				for each (T item in _Dict->Values)
				{
					if (!GetIsNullOrEmpty(item))
						return false;
				}

				return true;
			}

			for each (T item in _Items)
			{
				if (!GetIsNullOrEmpty(item))
					return false;
			}

			return true;
		}
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// True if the object has physical collections initialized else false. It may still
	/// be IsUnary
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property bool IsCollection
	{
		virtual bool get() override { return (Count != 0 || _Xref != nullptr || _Items != nullptr); }
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The object has named elements or is in a Dictionary state
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property bool IsDictionary
	{
		virtual bool get() override { return (_UnaryKey != nullptr || _Xref != nullptr); }
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// True if the object has physical collections initialized else false. It may still
	/// be IsUnary
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property bool IsICollection
	{
		virtual bool get() override { return (_Xref != nullptr || _Items != nullptr); }
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The object has unnamed elements or is in a one-dimensional list state
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property bool IsList
	{
		virtual bool get() override { return (_Items != nullptr || (IsUnary && _UnaryKey == nullptr)); }
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Checks if any containers have a value (not nullptr). No further validation.
	/// IsNull: Level 1 existence check. Containers don't exist (are all nullptr).
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property bool IsNull
	{
		virtual bool get() override
		{
			return (!IsUnary && _Items == nullptr && _Dict == nullptr);
		}
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Checks element at index for nullptr if it exists else returns true.
	/// IsNull: Level 1 existence check. Containers don't exist (are all nullptr).
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property bool IsNull[int]
	{
		virtual bool get(int index) override
		{
			return IsNullPtr(this[index]);
		}
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Checks element of 'key' for nullptr if it exists else returns true.
	/// IsNull: Level 1 existence check. Containers don't exist (are all nullptr).
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property bool IsNull[SysStr^]
	{
		virtual bool get(SysStr ^ key) override
		{
			if (Count == 0 || IsList)
				return true;

			return IsNullPtr(this[key]);
	
		}
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns true if Count == 0.
	/// IsNullOrEmpty: Level 3 existence check. Value container IsNullOrEmpty and
	/// collection Count == 0.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property bool IsNullOrEmpty
	{
		virtual bool get() override
		{
			return (Count == 0);
		}
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs an IsNullOrEmpty check on the element at index if it exist, else
	/// returns true.
	/// IsNullOrEmpty: Level 3 existence check. Value container IsNullOrEmpty and
	/// collection Count == 0.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property bool IsNullOrEmpty[int]
	{
		virtual bool get(int index) override
		{
			return GetIsNullOrEmpty(this[index]);
		}
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Checks that key does not exist else checks if element of key is null or empty.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property bool IsNullOrEmpty[SysStr^]
	{
		virtual bool get(SysStr ^ key) override
		{
			if (Count == 0 || IsList)
				return true;

			return GetIsNullOrEmpty(this[key]);
		}
	};

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns true if Count == 0.
	/// IsUnpopulated: Level 2 existence check. Containers don't exist or Count == 0.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property bool IsUnpopulated
	{
		virtual bool get() override
		{
			return (Count == 0);
		}
	};

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs an IsNullPtr check on the element at index if it exist, else
	/// returns true.
	/// IsUnpopulated: Level 2 existence check. Element doesn't exist or nullptr.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property bool IsUnpopulated[int]
	{
		virtual bool get(int index) override
		{
			return  IsNullPtr(this[index]);
		}
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs an IsNullPtr check on the element for 'key', else
	/// returns true.
	/// IsUnpopulated: Level 2 existence check. Element doesn't exist or nullptr.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property bool IsUnpopulated[SysStr^]
	{
		virtual bool get(SysStr ^ key) override
		{
			if (Count == 0 || IsList)
				return true;

			return IsNullPtr(this[key]);
		}
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Checks if the object contains a single named or unnamed element stored in 
	/// _UnaryKey and/or _UnaryValue.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property bool IsUnary
	{
		virtual bool get() override { return _CountFlag == -1; };
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns the key at index. If IsList (!IsDictionary), returns index.ToString()
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property SysStr^ Key[int]
	{
		virtual SysStr ^ get(int index) override
		{
			if (index < 0 || index >= Count)
				return nullptr;

			if (!IsDictionary)
			{
				return System::Convert::ToString(index);
			}
			else if (_Xref != nullptr)
			{
				return _Xref[index];
			}

			return _UnaryKey;
		}
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Implements the List enumerator object for IList<T>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property IList<T>^ Enumerator
	{
		virtual IList<T>^ get() override
		{
			return (IList<T>^)this;
		}
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Implements the ReplicaKey Dictionary enumerator object for Replicant.
	/// ReplicaKeyPair(T) = KeyValuePair<ReplicaKey, T>
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property IEnumerable<ReplicaKeyPair(T)>^ ReplicaKeyEnumerator
	{
		virtual IEnumerable<ReplicaKeyPair(T)>^ get() override
		{
			return (IEnumerable<ReplicaKeyPair(T)>^)this;
		}
	};


#pragma endregion Property Accessors




// =========================================================================================================
#pragma region Operators - Replicant
// =========================================================================================================

public:

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Adds an unnamed element to the collection returning the updated collection.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual IReplicant<T>^ operator+= (const T element) override
	{
		Add(element);
		return this;
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Adds a named element to the collection returning the updated collection.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual IReplicant<T>^ operator+= (const KeyValuePair<SysStr^, T> pair) override
	{
		Add(pair);
		return this;
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Adds an unnamed element to the collection returning a new collection. 
	/// </summary>
	// ---------------------------------------------------------------------------------
	static Replicant^ operator+ (Replicant^ lhs, T element)
	{
		Replicant^ collection = lhs->CreateInstance();

		collection->Clone(lhs);
		collection->Add(element);

		return collection;
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Adds a named element to the collection. The reference of a referenced Value
	/// element will be stored.
	/// </summary>
	/// <remarks>
	/// Note that this will append to the lhs original and return it.
	/// It doesn't create a new item. That's because it may have been called from a
	/// += operator on an indexer. To add to a new item use: 
	/// 'gcnew NewItem; newItem += lhsNode; newItem += rhsNode'.
	/// </remarks>
	// ---------------------------------------------------------------------------------
	static Replicant^ operator+ (Replicant^ lhs, KeyValuePair<SysStr^, T> pair) 
	{
		Replicant^ collection = lhs->CreateInstance();

		collection->Clone(lhs);
		collection->Add(pair);

		return collection;
	}


#pragma endregion Operators





// =========================================================================================================
#pragma region Constructors/Destructors - Replicant
// =========================================================================================================

public:

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Default .ctor
	/// </summary>
	// ---------------------------------------------------------------------------------
	Replicant() : AbstractReplicant()
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
	Replicant(int capacity) : AbstractReplicant(capacity)
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
	Replicant(int capacity, T element) : AbstractReplicant(capacity, element)
	{
	};



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// .ctor initialized with an unnamed element collection.
	/// </summary>
	// ---------------------------------------------------------------------------------
	Replicant(ICollection<T>^ collection) : AbstractReplicant(collection)
	{
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// .ctor initialized with a key and value element.
	/// </summary>
	// ---------------------------------------------------------------------------------
	Replicant(SysStr^ key, T element) : AbstractReplicant(key, element)
	{
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// .ctor initialized with a named element collection.
	/// </summary>
	// ---------------------------------------------------------------------------------
	Replicant(ICollection<KeyValuePair<SysStr^, T>>^ collection) : AbstractReplicant(collection)
	{
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// .ctor shallow copy constructor.
	/// </summary>
	// ---------------------------------------------------------------------------------
	Replicant(Replicant^ collection) : AbstractReplicant(collection)
	{
	};



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Creates an instance of the current type.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual SysObj^ CreateInstance() override
	{
		return gcnew Replicant();
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs a shallow (non-destructive) copy of a Replicant into clone.
	/// A shallow copy creates a mirror image of the source Replicant, preserving object
	/// references.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual void Clone(SysObj^ cloneObject) override
	{
		Replicant^ clone = (Replicant^)cloneObject;

		if (IsUnary)
		{
			clone->_UnaryKey = _UnaryKey;
			clone->_UnaryValue = _UnaryValue;
			return;
		}

		int capacity = (Count == 0 ? clone->_InitialCapacity : Count);

		if (_Items != nullptr)
		{

			clone->_Items = gcnew List<T>(capacity);

			for each (T item in _Items)
			{
				clone->_Items->Add(item);
			}

			clone->_CountFlag = _CountFlag;
			clone->_CurrentSeed = _CurrentSeed;
			clone->_KeySeed = _KeySeed;
		}
		else if (_Xref != nullptr)
		{
			clone->_Xref = gcnew List<SysStr^>(capacity);

			for each (SysStr ^ key in _Xref)
			{
				clone->_Xref->Add(key);
			}


			clone->_Dict = gcnew Dictionary<SysStr^, T>(capacity);

			for each (KeyValuePair<SysStr^, T> pair in _Dict)
			{
				clone->_Dict->Add(pair.Key, pair.Value);
			}

			clone->_CountFlag = _CountFlag;
			clone->_CurrentSeed = _CurrentSeed;
			clone->_KeySeed = _KeySeed;
		}
	};

};


}
