#pragma once
#include "pch.h"
#include "AdvancedReplicant.h"

// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)



// Replicant base classes.
#define ARBase AdvancedReplicant
#define RBase Replicant



using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;



namespace C5 {




// =========================================================================================================
//											AbstractCell Class
//
/// <summary>
/// Method definitions class for the Cell class.
/// Declares additional and overriden properties and operators as abtract.
/// Defines utility and worker methods utilized by the Cell class.
/// T: The dereferenced type of the value stored in the array. In descendant Cell classes this would be the
/// descendant itself, ie. Cell<Cell<Tvalue>>.
/// Tvalue: The type of the member stored in a Cell when the Cell is a value container.
/// </summary>
// =========================================================================================================
template<typename T, typename Tvalue> public ref class AbstractCell abstract : public AdvancedReplicant<T^>
{

protected:

	static bool GlobalRecursiveStorage = false;

	bool _RecursiveStorage = false;

	/// <summary>
	/// The actual Tvalue^ value if this cell physically stores a value.
	/// </summary>
	SysObj^ _Value = nullptr;

	/// <summary>
	/// The reference to the cell that actually stores the physical value. This is set after a call to the
	/// proerties StoredCell or StoredValue.
	/// </summary>
	T^ _StorageCell = nullptr;


// =========================================================================================================
#pragma region Overridden Property Accessors Declarations - AbstractCell
// =========================================================================================================

public:

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets or sets the element located at index.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property T^ default[int]
	{
		virtual T^ get(int index) override abstract; virtual void set(int index, T ^ value) override abstract;
	};

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets or sets the named element for key or key cast to int for unnamed elements.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property T^ default[SysStr^]
	{
		virtual T^ get(SysStr^ key) override abstract; virtual void set(SysStr ^ key, T ^ value) override abstract;
	};

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The nested indexed element of an indexed element.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property T^ default[int, int] { virtual void set(int index0, int index1, T ^ value) override abstract; };

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The nested named element of a named element.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property T^ default[SysStr^, SysStr^] { virtual void set(SysStr ^ key0, SysStr ^ key1, T ^ value) override abstract; };

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The nested named element of an indexed element.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property T^ default[SysStr^, int] { virtual void set(SysStr ^ key, int index, T ^ value) override abstract; };

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The nested indexed element of a named element.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property T^ default[int, SysStr^] { virtual void set(int index, SysStr ^ key, T ^ value) override abstract; };

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Checks if any containers have a value (not nullptr). No further validation.
	/// IsNull: Level 1 existence check. Containers don't exist (are all nullptr).
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property bool IsNull { virtual bool get() override abstract; };


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Checks element at index for nullptr if it exists else returns true.
	/// IsNull: Level 1 existence check. Containers don't exist (are all nullptr).
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property bool IsNull[int]{ virtual bool get(int index) override abstract; };


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Checks if 'key' exists. No further validation.
	/// IsNull: Level 1 existence check. Containers don't exist (are all nullptr).
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property bool IsNull[SysStr^]{ virtual bool get(SysStr ^ key) override abstract; };

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs an IsNull check on the element for the nested keys if it exist, else 
	/// returns true.
	/// IsNull: Level 1 existence check. Containers don't exist (are all nullptr).
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property bool IsNull[SysStr^, SysStr^]{ virtual bool get(SysStr ^ key0, SysStr ^ key1) override abstract; }

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Checks value container for IsNullOrEmpty and collections for Count == 0.
	/// IsNullOrEmpty: Level 3 existence check. Value container IsNullOrEmpty and
	/// collection Count == 0.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property bool IsNullOrEmpty { virtual bool get() override abstract; };

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs an IsNullOrEmpty check on the element at index if it exist, else
	/// returns true.
	/// IsNullOrEmpty: Level 3 existence check. Value container IsNullOrEmpty and
	/// collection Count == 0.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property bool IsNullOrEmpty[int]{ virtual bool get(int index) override abstract; };

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs an IsNullOrEmpty check on the element for 'key' if it exist, else 
	/// returns true.
	/// IsNullOrEmpty: Level 3 existence check. Value container IsNullOrEmpty and
	/// collection Count == 0.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property bool IsNullOrEmpty[SysStr^]{ virtual bool get(SysStr ^ key) override abstract; };

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs an IsNullOrEmpty check on the element for the nested keys if it exist,
	/// else returns true.
	/// IsNullOrEmpty: Level 3 existence check. Value container IsNullOrEmpty and
	/// collection Count == 0.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property bool IsNullOrEmpty[SysStr^, SysStr^]{ virtual bool get(SysStr ^ key0, SysStr^ key1) override abstract; };

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns true if no containers have a value (== nullptr). If there is a
	///  collection will still return true if Count == 0.
	/// IsUnpopulated: Level 2 existence check. Containers don't exist or, for a
	/// collection, Count == 0.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property bool IsUnpopulated { virtual bool get() override abstract; };


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs an IsUnpopulated check on the element at index if it exist, else
	/// returns true.
	/// IsUnpopulated: Level 2 existence check. Containers don't exist or, for a
	/// collection, Count == 0.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property bool IsUnpopulated[int]{ virtual bool get(int index) override abstract; };


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs an IsUnpopulated check on the element for 'key' if it exist, else 
	/// returns true.
	/// IsUnpopulated: Level 2 existence check. Containers don't exist or, for a
	/// collection, Count == 0.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property bool IsUnpopulated[SysStr^]{ virtual bool get(SysStr ^ key) override abstract; };


	// ***********----------------------------------------------------------------------
	/// <summary>
	/// Implements Values getter. Returns list of elements. If IsUnary returns wrapped
	/// _UnaryElement else if IsList (!IsDictionay) returns _Items else if IsDictionary
	/// will return _Dict Values.
	/// </summary>
	// ***********----------------------------------------------------------------------
	virtual property ICollection<T^>^ Values { ICollection<T^>^ get() override abstract; };


#pragma endregion Overriden Property Accessors Declarations


// =========================================================================================================
#pragma region Property Accessor Declarations - AbstractCell
// =========================================================================================================


public:

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets or sets the named element using a T^ to derive the key.
	/// </summary>
	// ---------------------------------------------------------------------------------
	/*
	virtual property T^ default[T^] 
	{
		virtual T^ get(T ^ key) abstract; virtual void set(T ^ key, T ^ value) abstract;
	};
	*/

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The nested named element of a named element, using type T to derive the key.
	/// </summary>
	// ---------------------------------------------------------------------------------
	/*
	virtual property T^ default[T^, T^]
	{
		virtual T^ get(T^ key0, T^ key1) bastract; virtual void set(T^ key0, T^ key1, T^ value) abstract;
	};
	*/

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns the length of the stored value else 0 else -1 if it's IsCollection.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property int Length { virtual int get() abstract; };

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets or sets the physical locally stored object.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property SysObj^ LocalObject { virtual SysObj^ get() abstract; virtual void set(SysObj^ value) abstract; };

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets or sets the physical locally stored object cast as Tvalue.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property Tvalue LocalValue { virtual Tvalue get() abstract; virtual void set(Tvalue value) abstract; };

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets or sets the physical locally stored object cast as SysStr^.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property SysStr^ LocalString { virtual SysStr^ get() abstract; virtual void set(SysStr^ value) abstract; };

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Enables / disables recursive storage
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property bool RecursiveStorage { virtual bool get() abstract; virtual void set(bool value) abstract; };

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the cell in the chain that holds the unique _Value for this cell.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property T^ StorageCell { virtual T^ get() abstract; };

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets or Sets the stored object of the T if it exists.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property SysObj^ StorageObject { virtual SysObj^ get() abstract; void set(SysObj^ value) abstract; };

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets or Sets the stored value of the T if it exists.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property Tvalue StorageValue { virtual Tvalue get() abstract; void set(Tvalue value) abstract; };

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets or Sets the stored value of the T cast to SysStr^ if it exists.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property SysStr^ StorageString { virtual SysStr^ get() abstract; void set(SysStr^ value) abstract; };

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Converts the stored value of the T to SysStr^ uppercase, else returns "". 
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property SysStr^ ToUpper { SysStr^ get() abstract; };

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns the cell that was stored in the last get or set of the StorageCell.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property T^ TransientCell { virtual T^ get() abstract; };

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the last object value as since a call to StorageValue, StorageObject...
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property SysObj^ TransientObject { virtual SysObj^ get() abstract; virtual void set(SysObj^ value) abstract; };

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the last value as (T) since a call to StorageValue, StorageObject...
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property Tvalue TransientValue { virtual Tvalue get() abstract; virtual void set(Tvalue value) abstract; };

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the last value as SysStr^since a call to StorageValue, StorageString...
	/// </summary>
	// ---------------------------------------------------------------------------------
	property SysStr^ TransientString { virtual SysStr^ get() abstract; virtual void set(SysStr^ value) abstract; };

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets or Sets the char at index/offset of the stored value cast to SysStr^.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property XCHAR Value[int] { virtual XCHAR get(int index) abstract; virtual void set(int index, XCHAR value) abstract; };


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Casts the stored object to SysStr^ else returns nullptr.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual operator SysStr ^ ()  abstract;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Casts the stored object to Int32 and returns the boolean else returns false.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual operator bool() abstract;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns 0 when IsNull else ToInt32 if StorageObject is set else 1 for all other
	/// cases.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual operator int() abstract;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Casts the stored object to wchar_t* else returns nullptr.
	/// </summary>
	// ---------------------------------------------------------------------------------
	// virtual operator PCXSTR() abstract;

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Casts the stored object to char* else returns nullptr.
	/// </summary>
	// ---------------------------------------------------------------------------------
	// virtual operator PYSTR() abstract;

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Sets the LocalString to rhs and returns the updated Cell<Tvalue>^.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual T% operator=(const SysStr^ rhs) abstract;

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Sets the LocalObject to (SysObj^)value where value is type bool and returns the
	/// updated Cell<Tvalue>^.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual T% operator=(const bool rhs) abstract;

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Sets the LocalObject to value for != 0 or calls Clear() for 0 and returns the
	/// updated Cell<Tvalue>^.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual T% operator=(const int rhs) abstract;

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Sets the LocalObject to null-terminated rhs and returns updated Cell<Tvalue>^.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual T% operator=(PCXSTR rhs) abstract;

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Sets the LocalObject to null-terminated rhs and returns updated Cell<Tvalue>^.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual T% operator=(PCYSTR rhs) abstract;
	
	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs a shallow copy of Cell rhs returning the updated Cell<Tvalue>^.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual T% operator=(const T^ rhs) abstract;

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs a shallow copy of Cell rhs returning the updated Cell<Tvalue>^.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual T^ operator=(const T% rhs) abstract;

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Concatenates or appends as element the rhs, returning the updated Cell<Tvalue>^.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual T^ operator+= (const SysStr^ rhs) abstract;

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Concatenates or appends as element the rhs, returning the updated Cell<Tvalue>^.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual T^ operator+=(PCXSTR rhs) abstract;

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Concatenates or appends as element the rhs, returning the updated Cell<Tvalue>^.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual T^ operator+=(const T^ rhs) abstract;

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Creates a new Cell, concatenating or appending rhs to lhs.
	/// </summary>
	// ---------------------------------------------------------------------------------
	// static Cell^ operator+ (Cell^ lhs, SysStr^ rhs);

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Creates a new Cell, concatenating or appending rhs to lhs.
	/// </summary>
	// ---------------------------------------------------------------------------------
	// static Cell^ operator+(Cell^ lhs, PXSTR rhs);

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Creates a new Cell, concatenating or appending rhs to lhs.
	/// </summary>
	// ---------------------------------------------------------------------------------
	// static Cell^ operator+(Cell^ lhs, PYSTR rhs);

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Creates a new Cell, concatenating or appending rhs to lhs.
	/// </summary>
	// ---------------------------------------------------------------------------------
	// static Cell^ operator+(Cell^ lhs, Cell^ rhs);

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Strictly not-equal deep negative equivalancy.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual bool operator!= (SysStr^ rhs) abstract;

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Strictly not-equal deep negative equivalancy.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual bool operator!= (PXSTR rhs) abstract;

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs a full tree strictly not-equal deep negative equivalancy check.
	/// </summary>
	// ---------------------------------------------------------------------------------
	// static bool operator!=(T^ lhs, T^ rhs);

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Strictly equal shallow equivalancy.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual bool operator== (SysStr^ value) abstract;

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Strictly equal shallow equivalancy.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual bool operator==(PXSTR value) abstract;

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs a strictly equal shallow equivalancy check. Static no declaration
	/// </summary>
	// ---------------------------------------------------------------------------------
	// static bool operator==(T^ lhs, T^ rhs);


#pragma endregion Property Accessors




// =========================================================================================================
#pragma region Constructors/Destructors - AbstractCell
// =========================================================================================================

public:

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Default .ctor
	/// </summary>
	// ---------------------------------------------------------------------------------
	AbstractCell() : AdvancedReplicant()
	{
		_RecursiveStorage = GlobalRecursiveStorage;
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// <summary>
	/// .ctor with initial and growth capacity.
	/// </summary>
	/// <param name="capacity">
	/// Sets the initial and growth capacity. If capacity is less than one, the default
	/// _InitialCapacity will be used.
	/// </param>
	// ---------------------------------------------------------------------------------
	AbstractCell(int capacity) : AdvancedReplicant(capacity)
	{
		_RecursiveStorage = GlobalRecursiveStorage;
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// <summary>
	/// .ctor with an initial T^ value element.
	/// </summary>
	/// <param name="capacity">
	/// Sets the initial and growth capacity. If capacity is less than one, the default
	/// _InitialCapacity will be used.
	/// </param>
	/// <param name="element"></param>
	AbstractCell(int capacity, T^ element) : AdvancedReplicant(capacity, element)
	{
		_RecursiveStorage = GlobalRecursiveStorage;
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// .ctor initialized with an unnamed element collection.
	/// </summary>
	// ---------------------------------------------------------------------------------
	AbstractCell(ICollection<T^>^ collection) : AdvancedReplicant(collection)
	{
		_RecursiveStorage = GlobalRecursiveStorage;
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// .ctor initialized with an string collection.
	/// </summary>
	// ---------------------------------------------------------------------------------
	AbstractCell(ICollection<SysStr^>^ collection) : AdvancedReplicant(collection->Count)
	{
		for each (SysStr ^ str in collection)
			Add(str);
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// .ctor initialized with a key and value element.
	/// </summary>
	// ---------------------------------------------------------------------------------
	AbstractCell(SysStr^ key, T^ element) : AdvancedReplicant(key, element)
	{
		_RecursiveStorage = GlobalRecursiveStorage;
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// .ctor initialized with a named element collection.
	/// </summary>
	// ---------------------------------------------------------------------------------
	AbstractCell(ICollection<KeyValuePair<SysStr^, T^>>^ collection) : AdvancedReplicant(collection)
	{
		_RecursiveStorage = GlobalRecursiveStorage;
	};



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// .ctor deep copy (destructive) constructor. To perform a shallow copy use the
	/// assignment (operator=) operator or Clone() method.
	/// </summary>
	// ---------------------------------------------------------------------------------
	AbstractCell(T^ cell) : AdvancedReplicant()
	{
		_RecursiveStorage = GlobalRecursiveStorage;

		cell->CopyTo((T^)this);
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// .ctor with unary string value initializer.
	/// </summary>
	// ---------------------------------------------------------------------------------
	AbstractCell(SysStr^ value) : AdvancedReplicant()
	{
		_RecursiveStorage = GlobalRecursiveStorage;

		LocalString = value;
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// .ctor with unary PCXSTR value initializer.
	/// </summary>
	// ---------------------------------------------------------------------------------
	AbstractCell(PCXSTR value) : AdvancedReplicant()
	{
		_RecursiveStorage = GlobalRecursiveStorage;

		LocalString = gcnew SysStr(value);
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// .ctor with unary PCYSTR value initializer.
	/// </summary>
	// ---------------------------------------------------------------------------------
	AbstractCell(PCYSTR value) : AdvancedReplicant()
	{
		_RecursiveStorage = GlobalRecursiveStorage;

		LocalString = gcnew SysStr(value);
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// .ctor with unary SysObj^ value initializer. Reference is preserved.
	/// </summary>
	// ---------------------------------------------------------------------------------
	AbstractCell(SysObj^ value) : AdvancedReplicant()
	{
		_RecursiveStorage = GlobalRecursiveStorage;
		LocalObject = value;
	};



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Reimplements CreateInstance.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual SysObj^ CreateInstance() override abstract;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Reimplements Clone(SysObj^%).
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual void Clone(SysObj^ cloneObject) override abstract;


#pragma endregion Constructors/Destructors





// =========================================================================================================
#pragma region Methods - AbstractCell
// =========================================================================================================


protected:


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Creates a cell initialized with a value.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual SysObj^ CreateInstance(SysObj^ value)
	{
		T^ cell = (T^)CreateInstance();
		cell->LocalObject = value;

		return cell;
	}




	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Determines if a Tvalue value is null or empty.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual bool GetIsNullOrEmpty(SysObj^ value)
	{
		return (value == nullptr || SysStr::IsNullOrEmpty(safe_cast<SysStr^>(value)));
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Converts to char* and sets char value at index. Defined in
	/// AbstruseReplicant.cpp to avoid header conflicts by marshal.h
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual SysStr^ MarshalSetChar(SysStr^ input, int index, XCHAR value)
	{
		System::IntPtr intptr = Marshal::StringToBSTR(input);
		PXSTR pxstr = (PXSTR)intptr.ToPointer();

		pxstr[index] = value;

		input = gcnew SysStr(pxstr);

		return input;
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Converts a cell that holds a value to a collection cell with a single element.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual void MigrateToCollection()
	{
		if (LocalObject == nullptr)
			return;

		T^ cell = (T^)CreateInstance(LocalObject);
		LocalObject = nullptr;

		Add(cell);
	}



public:


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Adds value as an element.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual void Add(T^ value) override
	{
		// Void any stored value, we're working with a collection now
		_Value = nullptr;

		// Rule 1: Never use a constructor to cast internally, it may cause recursion.
		// Use CreateInstance().
		RBase::Add(value);
	};



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Adds value as an element.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual void Add(SysStr^ value)
	{
		// Void any stored value, we're working with a collection now
		_Value = nullptr;

		// Rule 1: Never use a constructor to cast internally, it may cause recursion.
		// Use CreateInstance().
		Add((T^)CreateInstance(value));
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Adds key object if it doesn't exist then adds value as an element to it's
	/// collection. To add value as a KeyValuePair use Add(KeyValuePair).
	/// If key is a nullptr adds the ordinal as a KeyValuePair.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual void Add(SysStr^ key, T^ value) override
	{
		// Void any stored value, we're working with a collection now
		_Value = nullptr;

		// If it's an ordinal add it as a KeyValuePair.
		if (key == nullptr)
		{
			Add(KeyValuePair<SysStr^, T^>(key, value));
			return;
		}

		T^ lhs;

		if (!TryGetValue(key, lhs))
		{
			lhs = (T^)CreateInstance();
			Add(KeyValuePair<SysStr^, T^>(key, lhs));
		}

		lhs->Add(value);

	};



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Adds key object if it doesn't exist then adds value as an element to it's
	/// collection. To add value as a KeyValuePair use Add(KeyValuePair).
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual void Add(SysStr^ key, SysStr^ value) 
	{
		// Void any stored value, we're working with a collection now
		_Value = nullptr;


		Add(key, (T^)CreateInstance(value));
	};



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Adds a KeyValuePair dictionary element.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual void Add(KeyValuePair<SysStr^, T^> pair) override
	{
		// Void any stored value, we're working with a collection now
		_Value = nullptr;

		RBase::Add(pair);
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Converts the current object to a collection the adds value as an element if it
	/// is not a collection else appends the collection elements to this collection.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual void Append(T^ value)
	{

		// Void any stored value, we're working with a collection now
		if (LocalObject != nullptr)
		{
			ARBase::Add((T^)CreateInstance(LocalObject));
			_Value = nullptr;
		}

		if (value->IsCollection)
		{
			if (value->IsDictionary)
			{
				for each (KeyValuePair<SysStr^, T^> pair in value)
					ARBase::Add(pair);
				return;
			}

			for each (SysObj ^ item in value)
				ARBase::Add((T^)item);
			return;
		}

		ARBase::Add(value);
	};



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Creates the key object else ensures it's a collection then adds value as an
	/// element if it is not a collection else appends the collection elements to the
	/// key object collection.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual void Append(SysStr^ key, T^ value)
	{
		T^ lhs;

		if (!TryGetValue(key, lhs))
		{
			lhs = (T^)CreateInstance();
			this[key] = lhs;
		}

		((AbstractCell^)lhs)->Append(value);
	}




	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Full reset of containers.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual void Clear() override
	{
		_Value = nullptr;
		_StorageCell = nullptr;

		ARBase::Clear();
	}


	virtual bool Contains(SysStr^ value)
	{
		return Contains((T^)CreateInstance(value));
	};



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs a deep (destructive) copy of Cell^ cell into this. A deep copy
	/// replicates each object in the tree creating an entirely new set of objects. To
	/// create a mirror image of the source Cell and preserve object references perform
	/// a shallow Clone() copy or use the assignment (operator=) operator.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual void CopyTo(T^ cell)
	{
		AbstractCell^ clone = (AbstractCell^)cell;

		clone->Clear();

		if (!IsCollection)
		{
			clone->_Value = _Value;
			return;
		}

		int capacity = (Count == 0 ? clone->_InitialCapacity : Count);

		T^ copyToken = nullptr;

		if (_Items != nullptr)
		{
			clone->_Items = gcnew List<T^>(capacity);

			for each (T ^ item in _Items)
			{
				copyToken = (T^)CreateInstance();
				item->CopyTo(copyToken);

				clone->_Items->Add(copyToken);
			}

		}
		else if (_Xref != nullptr)
		{
			clone->_Xref = gcnew List<SysStr^>(capacity);
			clone->_Dict = gcnew Dictionary<SysStr^, T^>(capacity);

			for each (SysStr ^ key in _Xref)
			{
				clone->_Xref->Add(key);
			}

			for each (KeyValuePair<SysStr^, T^> pair in _Dict)
			{
				copyToken = (T^)CreateInstance();
				pair.Value->CopyTo(copyToken);

				clone->_Items->Add(copyToken);
				clone->_Dict->Add(pair.Key, copyToken);
			}

		}

		clone->_CountFlag = _CountFlag;
		clone->_CurrentSeed = _CurrentSeed;
		clone->_KeySeed = _KeySeed;
	};


	virtual SysStr^ Implode()
	{
		return Implode("");
	}


	virtual SysStr^ Implode(SysStr^ separator)
	{
		if (IsNull)
			return "";

		if (!IsCollection)
		{
			if (StorageObject != nullptr)
				return TransientString;

			return "";
		}

		SysStr^ str;
		T^ cell = nullptr;

		SysStr^ retval = "";

		for each (T^ cell in Enumerator)
		{
			// T^ cell = safe_cast<T^>(obj);

			if (IsNullPtr(cell))
				continue;

			str = cell->ToString();

			if (str == "")
				continue;

			retval += (retval == "" ? "" : separator) + str;
		}

		return retval;
	};



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns the index of the value cast to Cell^ else -1.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual int IndexOf(SysStr^ value)
	{
		if (!IsCollection)
			return -1;

		return IndexOf((T^)value);
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs an insert of the key and String^ cast to Cell value pair at7 the provided index.
	/// If the key already exists it is replaced if the index is the same or plus one,
	/// else it is removed.
	/// This object is migrated to a dictionary if required.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual void Insert(int index, SysStr^ key, SysStr^ value)
	{
		Insert(index, key, (T^)CreateInstance(value));
	};



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Searches the string value of the Cell^ elements and inserts the new SysStr^
	/// value at the correct ignore-case position. If the status is IsDictionary
	/// will call InsertAfter for a dictionary element with a nullptr key.
	/// </summary>
	// ---------------------------------------------------------------------------------


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs a php array merge.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual void Merge(IReplicant<T^>^ value) override
	{
		// Void any stored value, we're working with a collection now
		if (LocalObject != nullptr)
		{
			ARBase::Add((T^)CreateInstance(LocalObject));
			_Value = nullptr;
		}

		AbstractCell^ cell = (AbstractCell^)value;

		if (cell->LocalObject != nullptr)
		{
			Add(cell);
		}
		else if (cell->IsDictionary)
		{
			for each (KeyValuePair<SysStr^, T^> pair in cell)
				Add(pair);
		}
		else if (cell->IsList)
		{
			for each (AbstractCell^ item in cell->Enumerator)
				Add(item);
		}
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs a full tree printout.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual void PrintCell(SysStr^ title)
	{
		PrintCell(title, (T^)this, 0);
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs a full tree printout.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual void PrintCell(SysStr^ key, T^ value, int level)
	{
		int i;
		SysStr^ str;
		System::Text::StringBuilder^ sb = gcnew System::Text::StringBuilder("\n", level+20);
		sb->Append(' ', level*3);

		if (IsNullPtr(value) || !value->IsCollection)
		{
			str = (IsNullPtr(value) ? "nullptr" : value->ToString());

			if (str == nullptr)
				str = "";

			if (str->Trim()->Length == 0)
			{
				if (str->Length > 1)
					str = "\"" + str + "\"";
				else
					str = "'" + str + "'";
			}

			str = "[" + key + "] => " + str;

			System::Console::WriteLine(sb + str);

			return;
		}


		// Loop through collection
		if (value->IsDictionary)
		{
			str = "[" + key + "] => Dictionary(" + value->Count + ")";

			System::Console::WriteLine(sb + str);
			level++;

			for each (KeyValuePair<SysStr^, T^> pair in value)
			{
				PrintCell(pair.Key, pair.Value, level);
			}

			return;
		}

		// Cell^ cell;

		str = "[" + key + "] => List(" + value->Count + ")";

		System::Console::WriteLine(sb + str);

		level++;

		i = 0;

		for each (T^ cell in value->Enumerator)
		{
			PrintCell(System::Convert::ToString(i), cell, level);
			i++;
		}

		return;
	};



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Searches the string value of the T elements and inserts the new SysStr^, SysStr^
	/// pair at the correct ignore-case position. If the status is not IsDictionary will
	/// migrate to a dictionary first.
	/// If the element is unnamed the value of 'key' will be nullptr and a key will be
	/// created based on _CurrentSeed and _KeySeed.
	/// </summary>
	// ---------------------------------------------------------------------------------
	// virtual T^ InsertAfter(SysStr^ search, SysStr^ key, SysStr^ value) override
	// {
	//	return InsertAfter(search, key, (T^)CreateInstance(value));
	// };



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Overrides SysObj^ ToString() method.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual SysStr^ ToString() override
	{
		if (StorageObject == nullptr)
			return nullptr;

		return TransientString;
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs a destructive Trim on the stored value and returns the trimmed string.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual SysStr^ Trimmed()
	{
		if (StorageObject == nullptr)
			return nullptr;

		SysStr^ str = TransientString->Trim();

		if (str != nullptr)
			TransientString = str;

		return str;
	};

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs a non-destructive Trim on the stored value and returns the trimmed
	/// string.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual SysStr^ Trim()
	{
		if (StorageObject == nullptr)
			return nullptr;

		return TransientString->Trim();
	};


#pragma endregion Methods

};

}

#undef ARBase
#undef RBase
