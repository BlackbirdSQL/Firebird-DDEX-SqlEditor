#pragma once
#include "pch.h"
#include "IReplicant.h"
#include "ReplicaEnumerator.h"

// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)



using namespace System::Collections::Generic;



namespace C5 {



// =========================================================================================================
//										AbstruseReplicant Class
//
/// <summary>
/// The root class of the Replicant/Cell classes.
/// Declares all interface methods required by IDictionary, IList, IEnumerable<ReplicaKey> and ICloneable
/// and defines interface members as abstract.
/// T: The type of the value stored in the array. In descendant Cell classes this would by the descendant
/// Replicant itself, ie. Cell<Cell<Tvalue>>.
/// </summary>
// =========================================================================================================
template<typename T> public ref class AbstruseReplicant abstract : IReplicant<T>
{

#pragma region Fields

protected:

	/// <summary>
	/// The signed item count. This value is -1 when contents is being held in the unary variables _UnaryKey and/or
	/// _UnaryValue.
	/// </summary>
	int _CountFlag = 0;

	/// <summary>
	/// Initial capacity of collections.
	/// </summary>
	int _InitialCapacity = 10;

	/// <summary>
	/// The next index position in the natural order of the dictionary that an unnamed item will be placed.
	/// Because this class can hold a mixed collection of named pairs and unnamed elements, the unnamed elements
	/// will be placed in their natural order at the beginning of the dict if the collection is mixed so that they can still
	/// use an indexer. 
	/// </summary>
	int _CurrentSeed = 0;

	/// <summary>
	/// The next viable seed value that can be used to create the key for unnamed elements that are added to a mixed collection.
	/// _KeySeed may not match _CurrentSeed if an unnamed element was inserted amongst named elements. In that case _KeySeed will
	/// become the insertion index + 1.
	/// </summary>
	int _KeySeed = 0;

	/// <summary>
	/// The Dictionary container if there is more than a single named element.
	/// </summary>
	Dictionary<SysStr^, T>^ _Dict = nullptr;

	/// <summary>
	/// The Dictionary Xref container if there is more than a single named element.
	/// </summary>
	List<SysStr^>^ _Xref = nullptr;

	/// <summary>
	/// The one-dimensional array container if there is more than a single unnamed element.
	/// </summary>
	List<T>^ _Items = nullptr;

	/// <summary>
	///  The Dictionary key if there is a single named element.
	/// </summary>
	SysStr^ _UnaryKey = nullptr;

	/// <summary>
	/// The element container if there is only a single element.
	/// </summary>
	T _UnaryValue = nullptr;


#pragma endregion Fields




// =========================================================================================================
#pragma region Default Accessors - AbstruseReplicant
// =========================================================================================================

public:

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets or sets the element located at index.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property T default[int]
	{
		virtual T get(int index)
		{
			// T value = Activator::CreateInstance<T>();
			T value = nullptr;

			TryGetIndexedValue(index, value);

			return value;
		}

		virtual void set(int index, T value)
		{
			ReplaceAt(index, value);
		}
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets or sets the named element for key, or key cast to int for unnamed elements.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property T default[SysStr^]
	{
		virtual T get(SysStr ^ key)
		{
			// T value = Activator::CreateInstance<T>();
			T value = nullptr;

			TryGetValue(key, value);

			return value;
		}

		virtual void set(SysStr ^ key, T value)
		{
			Add(KeyValuePair<SysStr^, T>(key, value));
		}
	};


#pragma endregion Default Accessors




// =========================================================================================================
#pragma region Property Accessors - AbstruseReplicant
// =========================================================================================================

public:

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns true if Count == 0, else performs a single level traversal of each
	/// element in the collecton for a unanimous logical AND IsNullOrEmpty.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property bool ArrayNullOrEmpty { virtual bool get() abstract; };


	// ***********----------------------------------------------------------------------
	/// <summary>
	/// Implements the Count getter which is Abs(_CountFlag).
	/// </summary>
	// ***********----------------------------------------------------------------------
	virtual property int Count
	{
		virtual int get() { return (_CountFlag < 0 ? -_CountFlag : _CountFlag); }
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// True if the object has a unary or physical collections initialized else false.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property bool IsCollection { virtual bool get() abstract; };


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The object has named elements / is in a Dictionary state
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property bool IsDictionary { virtual bool get() abstract; }


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// True if the object has physical collections initialized else false. It may still
	/// be IsUnary.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property bool IsICollection { virtual bool get() abstract; };


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The object has unnamed elements / is in a one-dimensional list state
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property bool IsList { virtual bool get() abstract; };


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Checks if any containers have a value (not nullptr). No further validation.
	/// IsNull: Level 1 existence check. Containers don't exist (are all nullptr).
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property bool IsNull { virtual bool get() abstract; };


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Checks element at index for nullptr if it exists else returns true.
	/// IsNull: Level 1 existence check. Containers don't exist (are all nullptr).
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property bool IsNull[int]{ virtual bool get(int index) abstract; };


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Checks if 'key' exists. No further validation.
	/// IsNull: Level 1 existence check. Containers don't exist (are all nullptr).
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property bool IsNull[SysStr^]{ virtual bool get(SysStr ^ key) abstract; };


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns true if Count == 0.
	/// IsNullOrEmpty: Level 3 existence check. Value container IsNullOrEmpty and
	/// collection Count == 0.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property bool IsNullOrEmpty { virtual bool get() abstract; };


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs an IsNullOrEmpty check on the element at index if it exist, else
	/// returns true.
	/// IsNullOrEmpty: Level 3 existence check. Value container IsNullOrEmpty and
	/// collection Count == 0.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property bool IsNullOrEmpty[int]{ virtual bool get(int index) abstract; };


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs an IsNullOrEmpty check on the element for 'key' if it exist, else 
	/// returns true.
	/// IsNullOrEmpty: Level 3 existence check. Value container IsNullOrEmpty and
	/// collection Count == 0.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property bool IsNullOrEmpty[SysStr^]{ virtual bool get(SysStr ^ key) abstract; };

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns true if Count == 0.
	/// IsUnpopulated: Level 2 existence check. Containers don't exist or, for a
	/// collection, Count == 0.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property bool IsUnpopulated { virtual bool get() abstract; };


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs an IsUnpopulated check on the element at index if it exist, else
	/// returns true.
	/// IsUnpopulated: Level 2 existence check. Containers don't exist or, for a
	/// collection, Count == 0.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property bool IsUnpopulated[int]{ virtual bool get(int index) abstract; };


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs an IsUnpopulated check on the element for 'key' if it exist, else 
	/// returns true.
	/// IsUnpopulated: Level 2 existence check. Containers don't exist or, for a
	/// collection, Count == 0.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property bool IsUnpopulated[SysStr^]{ virtual bool get(SysStr ^ key) abstract; };


	// ***********----------------------------------------------------------------------
	/// <summary>
	/// Implements IsReadOnly getter. Underlying containers are read only.
	/// </summary>
	// ***********----------------------------------------------------------------------
	virtual property bool IsReadOnly
	{
		virtual bool get() { return true; }
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Checks if the object contains a single named or unnamed element stored in 
	/// _UnaryKey and/or _UnaryValue.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property bool IsUnary { virtual bool get() abstract; };


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns the key at index. If IsList (!IsDictionary), returns index.ToString()
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property SysStr^ Key[int]{ virtual SysStr ^ get(int index) abstract; };


	// ***********----------------------------------------------------------------------
	/// <summary>
	/// Implements the Keys collection getter. If IsList (!IsDictionay) will convert
	/// index range to list of Strings^.
	/// </summary>
	// ***********----------------------------------------------------------------------
	virtual property ICollection<SysStr^>^ Keys
	{
		virtual ICollection<SysStr^>^ get()
		{
			if (_UnaryKey != nullptr)
				return (ICollection<SysStr^>^) gcnew array<SysStr^> { _UnaryKey };

			if (_Xref != nullptr)
				return _Xref;

			List<SysStr^>^ keys = gcnew List<SysStr^>(Count == 0 ? 1 : _Items->Count);

			

			if (_Items != nullptr)
			{
				for (int index = 0; index < _Items->Count; keys->Add(index.ToString()), index++);
			}

			return (ICollection<SysStr^>^) keys;
		}
	};


	// ***********----------------------------------------------------------------------
	/// <summary>
	/// Implements the List enumerator object for IList<T> as abstract.
	/// </summary>
	// ***********----------------------------------------------------------------------
	virtual property IList<T>^ Enumerator { virtual IList<T>^ get() abstract; };


	// ***********----------------------------------------------------------------------
	/// <summary>
	/// Implements the ReplicaKey Dictionary enumerator object for #define
	/// ReplicaKeyPair(T) = KeyValuePair<ReplicaKey, T> as abstract.
	/// </summary>
	// ***********----------------------------------------------------------------------
	virtual property IEnumerable<ReplicaKeyPair(T)>^ ReplicaKeyEnumerator
	{
		virtual IEnumerable<ReplicaKeyPair(T)>^ get() abstract;
	};


	// ***********----------------------------------------------------------------------
	/// <summary>
	/// Implements Values getter. Returns list of elements. If IsUnary returns wrapped
	/// _UnaryElement else if IsList (!IsDictionay) returns _Items else if IsDictionary
	/// will return _Dict Values.
	/// </summary>
	// ***********----------------------------------------------------------------------
	virtual property ICollection<T>^ Values
	{
		ICollection<T>^ get()
		{
			if (IsUnary)
				return (ICollection<T>^) gcnew array<T> { _UnaryValue };

			if (IsDictionary)
				return _Dict->Values;

			if (IsList)
				return _Items;

			return (ICollection<T>^) gcnew array<T> {};
		}
	};


#pragma endregion Property Accessors




// =========================================================================================================
#pragma region Operators - AbstruseReplicant
// =========================================================================================================

public:

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Adds an unnamed element to the collection returning the updated collection.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual IReplicant<T>^ operator+= (const T element) abstract;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Adds a named element to the collection returning the updated collection.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual IReplicant<T>^ operator+= (const KeyValuePair<SysStr^, T> pair) abstract;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Adds an unnamed element to the collection returning a new collection. Static.
	/// </summary>
	// ---------------------------------------------------------------------------------
	// static AbstruseReplicant^ operator+ (AbstruseReplicant^ lhs, T element);


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Adds a named element (KeyValuePair) to the collection returning a new
	/// collection. Static.
	/// </summary>
	// ---------------------------------------------------------------------------------
	// virtual AbstruseReplicant operator+ (AbstruseReplicant^ lhs, KeyValuePair<SysStr^, T> pair);


#pragma endregion Operators




// =========================================================================================================
#pragma region Constructors/Destructors - AbstruseReplicant
// =========================================================================================================

public:

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Default .ctor
	/// </summary>
	// ---------------------------------------------------------------------------------
	AbstruseReplicant()
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
	AbstruseReplicant(int capacity)
	{
		if (capacity > 0)
			_InitialCapacity = capacity;
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
	AbstruseReplicant(int capacity, T element)
	{
		if (capacity > 0)
			_InitialCapacity = capacity;
		Add(element);
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// .ctor initialized with an unnamed element collection.
	/// </summary>
	// ---------------------------------------------------------------------------------
	AbstruseReplicant(ICollection<T>^ collection)
	{
		if (collection->Count == 0)
			return;

		_InitialCapacity = collection->Count;

		for each (T item in collection)
		{
			Add(item);
		}
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// .ctor initialized with a key and value element.
	/// </summary>
	// ---------------------------------------------------------------------------------
	AbstruseReplicant(SysStr^ key, T element)
	{
		Add(KeyValuePair<SysStr^, T>(key, element));
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// .ctor initialized with a named element collection.
	/// </summary>
	// ---------------------------------------------------------------------------------
	AbstruseReplicant(ICollection<KeyValuePair<SysStr^, T>>^ collection)
	{
		if (collection->Count == 0)
			return;

		_InitialCapacity = collection->Count;

		for each (KeyValuePair<SysStr^, T> pair in collection)
		{
			Add(pair);
		}
	};



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// .ctor shallow copy constructor.
	/// </summary>
	// ---------------------------------------------------------------------------------
	AbstruseReplicant(AbstruseReplicant^ collection)
	{
		collection->Clone(this);
	};


#pragma endregion Constructos/Destructors




// =========================================================================================================
#pragma region Methods - AbstruseReplicant
// =========================================================================================================

protected:

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Creates the next valid dictionary key for elements without a key when the
	/// object is in a IsDictionary state. The hint index to convert to a string should
	/// be _CurrentSeed++.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual SysStr^ CreateDictKey(int hint) abstract;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Determines if a T value is null or empty.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual bool GetIsNullOrEmpty(T value) abstract;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Given a string value, determines if it can be converted to a valid integer
	/// in the range of the current element collection or plus one to add it to
	/// the end of the collection.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual int GetValidIndex(SysStr^ value) abstract;


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
	virtual void MigrateToArray(int newCapacity) abstract;



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
	virtual void MigrateToDict(int newCapacity) abstract;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs an element insert and assumes that all parameters passed are valid to
	/// perform an insert without any validation,
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual T UnsafeDictionaryInsert(int index, int previous, SysStr^ key, T value) abstract;


public:

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs a shallow (non-destructive) copy of a Replicant into a new Replicant.
	/// A shallow copy creates a mirror image of the source Replicant, preserving object
	/// refrences.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual SysObj^ Clone() abstract;



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs a shallow (non-destructive) copy of a Replicant into a clone.
	/// A shallow copy creates a mirror image of the source Replicant, preserving object
	/// references.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual void Clone(SysObj^ cloneObject) abstract;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Creates an instance of the current type.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual SysObj^ CreateInstance() abstract;


	virtual bool InArray(List<T>^ list) abstract;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs an insert of the key value pair at the provided index.
	/// If the key already exists it is replaced if the index is the same or plus one,
	/// else it is removed.
	/// This object is migrated to a dictionary if required.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual void Insert(int index, SysStr^ key, T value) abstract;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Searches the string value of the T elements and inserts the new value
	/// at the correct ignore-case position. If the status is IsDictionary
	/// will call InsertAfter for a dictionary element with a nullptr key.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual T InsertAfter(SysStr^ search, T value) abstract;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Searches the string value of the T elements and inserts the new pair
	/// at the correct ignore-case position. If the status is not IsDictionary will
	/// migrate to a dictionary first.
	/// If the element is unnamed the value of 'key' will be nullptr and a key will be
	/// created based on _CurrentSeed and _KeySeed.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual T InsertAfter(SysStr^ search, SysStr^ key, T value) abstract;



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns the ordinal index of the key else nullptr. If the list is unnamed
	/// converts the key to an integer. If the converted key is within the index range
	/// of the list returns it else returns nullptr.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual int KeyIndexOf(SysStr^ key) abstract;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns the key of the value else nullptr.
	/// If the list is an unnamed list returns the index cast to SysStr.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual SysStr^ KeyOf(SysStr^ value) abstract;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs a php array merge.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual void Merge(IReplicant<T>^ value) abstract;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Removes the value of the key and or index and returns true if successful.
	/// Either index must not be -1 or key must not be nullptr or both may be specified.
	/// If the list is unnamed converts the key to an integer if it's not nullptr.
	/// If the converted key is within the index range of the list
	/// removes the value at that index and returns true.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual bool Remove(int index, SysStr^ key) abstract;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Tries to get the value at 'index' and places it in 'value' returning true else
	/// returns false.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual bool TryGetIndexedValue(int index, T% value) abstract;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Tries to convert a string segment to an unsigned integer then tries to get the
	/// indexed value.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual bool TryGetIndexedValue(SysStr^ segment, T% value) abstract;


#pragma endregion Methods




// =========================================================================================================
#pragma region Method Implementations - AbstruseReplicant
// =========================================================================================================

public:

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Adds value as an element.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual void Add(T value)
	{
		// Sanity check.
		if (IsDictionary)
		{
			Add(nullptr, value);
			return;
		}

		// We"re adding an element so check if we need to migrate to an array.
		MigrateToArray(Count + 1);


		// If there is space in the unary values, use them if _Items is null.
		// MigrateToArray guarantees there would be an array if the unary containers
		// are not available.
		if (!IsICollection)
		{
			_UnaryValue = value;
			// Set the count flag to the special case of one unary element.
			_CountFlag = -1;
			return;
		}


		// Add the item to the list
		_Items->Add(value);
		_CountFlag++;

	};

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Adds a KeyValuePair dictionary element.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual void Add(SysStr^ key, T value)
	{
		Add(KeyValuePair<SysStr^, T>(key, value));
	};

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Adds a KeyValuePair dictionary element.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual void Additty(SysStr^ key, SysObj^ value)
	{
		Add(KeyValuePair<SysStr^, T>(key, (T)value));
	};

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Adds a KeyValuePair dictionary element.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual void Add(KeyValuePair<SysStr^, T> pair)
	{
		// Sanity check
		// If we're adding an unnamed element and there isn"t an existing named element
		// then in reality we shouldn't be here.
		if (pair.Key == nullptr && !IsDictionary) // 2
		{
			Add(pair.Value);
			return;
		}


		// Check the hard key first. 
		if (_UnaryKey != nullptr && _UnaryKey == pair.Key)
		{
			_UnaryValue = pair.Value;
			return;
		}

		// Check for migrating.
		// If there are items or an _UnaryValue there's def going to be at least two if we got here.
		// Let's go through the possibilities.
		// 1. If Count is zero we're requesting to move to dict status for 1 element.
		//	Nothing will happen because _UnaryKey is free.
		// 2. If Count is 1 or more we're requesting for 2 or more elements.
		//	This will force migration to a physical dict.
		MigrateToDict(Count + 1);


		// Get the index position we will be inserting at.
		int index = (pair.Key == nullptr ? _CurrentSeed : Count);
		// Get the previous index of the key.
		int previous = (pair.Key == nullptr ? -1 : KeyIndexOf(pair.Key));

		UnsafeDictionaryInsert(index, previous, pair.Key, pair.Value);
	};


	virtual T ArrayPop()
	{
		if (Count == 0)
			return nullptr;

		T value = nullptr;
		TryGetIndexedValue(Count - 1, value);

		RemoveAt(Count - 1);

		return value;
	}


	virtual IReplicant<T>^ ArraySlice(int offset, int length, bool preserveKeys)
	{
		AbstruseReplicant^ replicant = (AbstruseReplicant^)CreateInstance();

		if (Count == 0 || offset < Count - 1)
			return replicant;

		if (length == -1)
			length = Count - offset;

		SysStr^ key;
		for (int i = offset; i < Count && i < offset + length; i++)
		{
			if (preserveKeys && IsDictionary && i >= _CurrentSeed)
			{
				key = Key[i];
				replicant->Add(key, default[key]);
			}
			else
			{
				replicant->Add(default[i]);
			}
		}
		return replicant;
	}

	virtual void Clear()
	{
		_Items = nullptr;
		_Xref = nullptr;
		_Dict = nullptr;
		_UnaryKey = nullptr;
		_UnaryValue = nullptr;
		_CountFlag = _CurrentSeed = _KeySeed = 0;
	};

	/// <summary>
	/// Searches for 'value' returning true if found.
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	virtual bool Contains(T value)
	{
		if (Count == 0)
			return false;

		if (IsUnary)
			return (_UnaryValue == value);

		if (IsDictionary)
			return _Dict->ContainsValue(value);

		return _Items->Contains(value);
	};

	virtual bool Contains(KeyValuePair<SysStr^, T> pair)
	{
		// T value = Activator::CreateInstance<T>();
		T value = nullptr;

		if (!TryGetValue(pair.Key, value))
			return false;

		return (value == pair.Value);
	};

	/// <summary>
	/// Checks if the value for the given key and returns boolean true if successful.
	/// If the list is unnamed converts the key to an integer.
	/// If the converted key is within the index range of the list
	/// returns true.
	/// </summary>
	virtual bool ContainsKey(SysStr^ key)
	{
		if (Count == 0)
			return false;

		if (_UnaryKey != nullptr)
		{
			return (_UnaryKey == key);
		}

		if (_Dict != nullptr)
		{
			return (_Dict->ContainsKey(key));
		}

		int i = GetValidIndex(key);

		return (i >= 0 && i < Count);
	};

	virtual void CopyTo(array<T, 1>^ value, int index)
	{
		if (value == nullptr)
		{
			System::ArgumentNullException^ ex = gcnew System::ArgumentNullException("Destination array");
			Diag::Dug(ex);
			throw ex;
		}

		if (index < 0 || index >= value->Length)
		{
			System::ArgumentOutOfRangeException^ ex = gcnew System::ArgumentOutOfRangeException
			(SysStr::Format("Destination array index {0} with length of {1}.", index, value->Length));
			Diag::Dug(ex);
			throw ex;
		}

		if (value->Length - index < Count)
		{
			System::ArgumentException^ ex = gcnew System::ArgumentException(SysStr::Format("Destination array size {0} too small for copy to index {1} from source with size {2}.", value->Length, index, Count));
			Diag::Dug(ex);
			throw ex;
		}

		for each (SysObj^ item in this)
			value[index++] = (T)item;
	};

	virtual void CopyTo(array<KeyValuePair<SysStr^, T>, 1>^ value, int index)
	{
		if (value == nullptr)
		{
			System::ArgumentNullException^ ex = gcnew System::ArgumentNullException("Destination array");
			Diag::Dug(ex);
			throw ex;
		}

		if (index < 0 || index >= value->Length)
		{
			System::ArgumentOutOfRangeException^ ex = gcnew System::ArgumentOutOfRangeException
				(SysStr::Format("Destination array index {0} with length of {1}.", index, value->Length));
			Diag::Dug(ex);
			throw ex;
		}

		if (value->Length - index < Count)
		{
			System::ArgumentException^ ex = gcnew System::ArgumentException(SysStr::Format("Destination array size {0} too small for copy to index {1} from source with size {2}.", value->Length, index, Count));
			Diag::Dug(ex);
			throw ex;
		}

		for each (KeyValuePair<SysStr^, T> pair in this)
			value[index++] = pair;
	};


	/// <summary>
	/// Returns the ordinal index of the key else nullptr.
	/// If the list is unnamed converts the key to an integer.
	/// If the converted key is within the index range of the list
	/// returns it else returns nullptr.
	/// </summary>
	/// <param name='key'></param>
	virtual int Index(SysStr^ key)
	{
		if (Count == 0)
			return -1;

		if (IsUnary && IsDictionary)
			return (_UnaryKey == key ? 0 : -1);

		if (IsDictionary)
			return _Xref->IndexOf(key);

		return GetValidIndex(key);
	};


	/// <summary>
	/// Returns the index of the element else -1.
	/// </summary>
	/// <param name="value"></param>
	virtual int IndexOf(T element)
	{
		if (Count == 0)
			return -1;

		if (IsUnary)
			return (_UnaryValue == element) ? 0 : -1;


		if (!IsDictionary)
			return _Items->IndexOf(element);

		for each (KeyValuePair<SysStr^, T> ^ pair in _Dict)
		{
			if (element == pair->Value)
				return _Xref->IndexOf(pair->Key);
		}

		return -1;
	};




	virtual void Insert(int index, T value)
	{
		if (IsDictionary)
		{
			Insert(index, nullptr, value);
			return;
		}


		if (index < 0 || index > Count)
		{
			System::ArgumentOutOfRangeException^ ex = gcnew System::ArgumentOutOfRangeException(
				SysStr::Format("Attempted insert at index {0} into Nodes array with {1} items.",
					index, _Items == nullptr ? 0 : Count));

			Diag::Dug(ex);
			throw ex;
		}

		// Check and prepare if we have to move to a physical array.
		MigrateToArray(Count + 1);

		if (!IsICollection)
		{
			_UnaryValue = value;
			_CountFlag = -1;
			return;
		}

		if (_Items->Count == index)
			_Items->Add(value);
		else
			_Items->Insert(index, value);

		_CountFlag++;

	};


	/// <summary>
	/// Removes the first occurrence of a specific object.
	virtual bool RemoveValue(T element) = IList<T>::Remove
	{
		if (Count == 0)
			return false;

		if (_Items != nullptr)
			return _Items->Remove(element);

		int index = IndexOf(element);

		if (index == -1)
			return false;

		RemoveAt(index);

		return true;
	};


	virtual bool Remove(SysStr^ key) = IDictionary<SysStr^, T>::Remove
	{
		return Remove(-1, key);
	};


	virtual bool Remove(KeyValuePair<SysStr^, T> pair)
	{
		if (Contains(pair))
			return Remove(-1, pair.Key);

		return false;
	};


	virtual void RemoveAt(int index)
	{
		if (Count == 0 || index < 0 || index >= Count)
			return;

		if (IsDictionary)
		{
			Remove(index, nullptr);
		}

		if (_CountFlag == -1)
		{
			_UnaryKey = nullptr;
			_UnaryValue = nullptr;
			_CountFlag = 0;
			return;
		}

		if (_Xref != nullptr)
		{
			_Dict->Remove(_Xref[index]);
			_Xref->RemoveAt(index);

			if (_Xref->Count == 0)
			{
				_KeySeed = 0;
				_CurrentSeed = 0;
			}
			else if (_CurrentSeed > index)
			{
				if (_CurrentSeed == _KeySeed)
					_KeySeed--;
				_CurrentSeed--;
			}

			_CountFlag = _Xref->Count;

			return;
		}

		_Items->RemoveAt(index);

		_CountFlag = _Items->Count;

		return;
	};


	virtual void ReplaceAt(int index, T value)
	{
		if (IsDictionary)
		{
			Insert(index, nullptr, value);
			return;
		}

		if (index == Count)
		{
			Insert(index, value);
			return;
		}

		if (index < 0 || index > Count)
		{
			System::ArgumentOutOfRangeException^ ex = gcnew System::ArgumentOutOfRangeException(
				SysStr::Format("Attempted replace at index {0} into Nodes array with {1} items.",
					index, _Items == nullptr ? 0 : Count));

			Diag::Dug(ex);
			throw ex;
		}

		if (!IsICollection)
		{
			_UnaryValue = value;
			_CountFlag = -1;
			return;
		}

		_Items[index] = value;
	};



	/// <summary>
	/// Gets the value of the key and returns true if successful.
	/// If the list is unnamed converts the key to an integer.
	/// If the converted key is within the index range of the list
	/// gets the value at the index.
	/// </summary>
	virtual bool TryGetValue(SysStr^ key, T% value)
	{
		if (Count == 0)
			return false;

		if (IsUnary && IsDictionary)
		{
			if (key == _UnaryKey)
			{
				value = _UnaryValue;
				return true;
			}

			return false;
		}

		if (IsDictionary)
			return _Dict->TryGetValue(key, value);

		int i = GetValidIndex(key);

		if (i == -1)
			return false;

		value = (IsUnary ? _UnaryValue : _Items[i]);

		return true;
	};


#pragma endregion Method Implementations




// =========================================================================================================
#pragma region Enumerators - AbstruseReplicant
// =========================================================================================================

public:

	virtual IEnumerator<KeyValuePair<SysStr^, T>>^ GetEnumerator() = IEnumerable<KeyValuePair<SysStr^, T>>::GetEnumerator
	{
		return gcnew ReplicaEnumerator<SysStr^, T>(this, false);
	};


	virtual System::Collections::IEnumerator^ GetArrayEnumerator() = System::Collections::IEnumerable::GetEnumerator
	{
		return gcnew ReplicaEnumerator<SysStr^, T>(this, false);
	};

	virtual IEnumerator<ReplicaKeyPair(T)>^ GetReplicaEnumerator() = IEnumerable<ReplicaKeyPair(T)>::GetEnumerator
	{
		return gcnew ReplicaEnumerator<ReplicaKey, T>(this, true);
	};

	virtual IEnumerator<T>^ GetListEnumerator() = IEnumerable<T>::GetEnumerator
	{
		return gcnew ReplicaEnumerator<SysStr^, T>(this, false);
	};

#pragma endregion Enumerators

};




}

