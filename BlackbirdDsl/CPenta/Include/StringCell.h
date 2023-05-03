#pragma once
#include "pch.h"
#include "Cell.h"

// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)


#define CellClass StringCell
#define ValueType System::String

// Replicant base classes.
#define ARBase AdvancedReplicant
#define RBase Replicant

#define CellPair KeyValuePair<System::String^, StringCell^>
// #define CellPairs gcnew array<KeyValuePair<System::String^, StringCell^>>
#define CellPairs(__CELLPAIRS__) gcnew StringCell(gcnew cli::array<KeyValuePair<System::String^, StringCell^>> {__CELLPAIRS__})
#define NullCell gcnew StringCell()


using namespace System::Runtime::InteropServices;


// Resolved Cell class Cell<String^> type for C#


namespace C5 {



// =========================================================================================================
//										StringCell Class
//
/// Exposes a <see cref="Cell"/> with a storage type of <see cref="System::String"/> to C++/Cli and C# projects.
// =========================================================================================================
public ref class CellClass : public Cell<CellClass, ValueType^>
{
	// =========================================================================================================
	#pragma region Expose Cell Property Accessors - CellClass
	// =========================================================================================================

public:


	// ---------------------------------------------------------------------------------
	/// Gets or sets the element located at index.
	// ---------------------------------------------------------------------------------
	
	virtual property CellClass^ default[int]
	{
		virtual CellClass^ get(int index) override { return Cell::default[index]; }
		virtual void set(int index, CellClass^ value) override { Cell::default[index] = value; }
	};


	// ---------------------------------------------------------------------------------
	/// Gets or sets the named element for key or key cast to int for unnamed elements.
	// ---------------------------------------------------------------------------------
	virtual property CellClass^ default[SysStr^]
	{
		virtual CellClass^ get(SysStr^ key) override { return Cell::default[key]; }
		virtual void set(SysStr^ key, CellClass^ value) override { Cell::default[key] = value; }
	};

	// ---------------------------------------------------------------------------------
	/// The nested indexed element of an indexed element.
	// ---------------------------------------------------------------------------------
	virtual property CellClass^ default[int, int]
	{
		virtual CellClass^ get(int index0, int index1) override { return Cell::default[index0, index1]; }
		virtual void set(int index0, int index1, CellClass^ value) override { Cell::default[index0, index1] = value; }
	};


	// ---------------------------------------------------------------------------------
	/// The nested named element of a named element.
	// ---------------------------------------------------------------------------------
	virtual property CellClass^ default[SysStr^, SysStr^]
	{
		virtual CellClass^ get(SysStr^ key0, SysStr^ key1) override { return Cell::default[key0, key1]; }
		virtual void set(SysStr^ key0, SysStr^ key1, CellClass^ value) override { Cell::default[key0, key1] = value; }
	};


	// ---------------------------------------------------------------------------------
	/// The nested named element of an indexed element.
	// ---------------------------------------------------------------------------------
	virtual property CellClass^ default[SysStr^, int]
	{
		virtual CellClass^ get(SysStr^ key, int index) override { return Cell::default[key, index]; }
		virtual void set(SysStr^ key, int index, CellClass^ value) override { Cell::default[key, index] = value; }
	};


	// ---------------------------------------------------------------------------------
	/// The nested indexed element of a named element.
	// ---------------------------------------------------------------------------------
	virtual property CellClass^ default[int, SysStr^]
	{
		virtual CellClass^ get(int index, SysStr^ key) override { return Cell::default[index, key]; }
		virtual void set(int index, SysStr^ key, CellClass^ value) override { Cell::default[index, key] = value; }
	};

	// ---------------------------------------------------------------------------------
	/// Returns true if Count == 0, else performs a single level traversal of each
	/// element in the collecton for a unanimous logical AND IsNullOrEmpty.
	// ---------------------------------------------------------------------------------
	virtual property bool ArrayNullOrEmpty { virtual bool get() override { return Cell::ArrayNullOrEmpty; } };


	// ***********----------------------------------------------------------------------
	/// Implements the Count getter which is Abs(_CountFlag).
	// ***********----------------------------------------------------------------------
	virtual property int Count { virtual int get() override { return Cell::Count; } };


	// ---------------------------------------------------------------------------------
	/// Returns true if the object is a collection else false.
	// ---------------------------------------------------------------------------------
	virtual property bool IsCollection
	{
		virtual bool get() override { return Cell::IsCollection; }
	};


	// ---------------------------------------------------------------------------------
	/// The object has named elements / is in a Dictionary state
	// ---------------------------------------------------------------------------------
	virtual property bool IsDictionary { virtual bool get() override { return Cell::IsDictionary; } };


	// ---------------------------------------------------------------------------------
	/// Returns true if the object has a physical ICollection else false.
	// ---------------------------------------------------------------------------------
	virtual property bool IsICollection
	{
		virtual bool get() override { return Cell::IsICollection; }
	};


	// ---------------------------------------------------------------------------------
	/// The object has unnamed elements / is in a one-dimensional list state
	// ---------------------------------------------------------------------------------
	virtual property bool IsList { virtual bool get() override { return Cell::IsList; } };


	// ---------------------------------------------------------------------------------
	/// Checks if any containers have a value (not nullptr). No further validation.
	/// IsNull: Level 1 existence check. Containers don't exist (are all nullptr).
	// ---------------------------------------------------------------------------------
	virtual property bool IsNull
	{
		virtual bool get() override { return Cell::IsNull; }
	};


	// ---------------------------------------------------------------------------------
	/// Performs an IsNull check on the element at index if it exist, else
	/// returns true.
	/// IsNull: Level 1 existence check. Containers don't exist (are all nullptr).
	// ---------------------------------------------------------------------------------
	virtual property bool IsNull[int]
	{
		virtual bool get(int index) override { return Cell::IsNull[index]; }
	};


	// ---------------------------------------------------------------------------------
	/// Performs an IsNull check on the element for 'key' if it exist, else 
	/// returns true.
	/// IsNull: Level 1 existence check. Containers don't exist (are all nullptr).
	// ---------------------------------------------------------------------------------
	virtual property bool IsNull[SysStr^]
	{
		virtual bool get(SysStr^ key) override { return Cell::IsNull[key]; }
	};


	// ---------------------------------------------------------------------------------
	/// Performs an IsNullPtr check on the element for the nested keys if it exist, else 
	/// returns true.
	/// IsNull: Level 1 existence check. Containers don't exist (are all nullptr).
	// ---------------------------------------------------------------------------------
	virtual property bool IsNull[SysStr^, SysStr^]
	{
		virtual bool get(SysStr ^ key0, SysStr ^ key1) override { return ARBase::IsNull[key0, key1]; }
	};


	// ---------------------------------------------------------------------------------
	/// Checks value container for IsNullOrEmpty and collections for Count == 0.
	/// IsNullOrEmpty: Level 3 existence check. Value container IsNullOrEmpty and
	/// collection Count == 0.
	// ---------------------------------------------------------------------------------
	virtual property bool IsNullOrEmpty
	{
		virtual bool get() override { return Cell::IsNullOrEmpty; }
	};


	// ---------------------------------------------------------------------------------
	/// Performs an IsNullOrEmpty check on the element at index if it exist, else
	/// returns true.
	/// IsNullOrEmpty: Level 3 existence check. Value container IsNullOrEmpty and
	/// collection Count == 0.
	// ---------------------------------------------------------------------------------
	virtual property bool IsNullOrEmpty[int]
	{
		virtual bool get(int index) override { return Cell::IsNullOrEmpty[index]; }
	};


	// ---------------------------------------------------------------------------------
	/// Performs an IsNullOrEmpty check on the element for 'key' if it exist, else 
	/// returns true.
	/// IsNullOrEmpty: Level 3 existence check. Value container IsNullOrEmpty and
	/// collection Count == 0.
	// ---------------------------------------------------------------------------------
	virtual property bool IsNullOrEmpty[SysStr^]
	{
		virtual bool get(SysStr^ key) override { return Cell::IsNullOrEmpty[key]; }
	};

	/*
	// ---------------------------------------------------------------------------------
	/// Returns true if no containers have a value (== nullptr). If there is a
	///  collection will still return true if Count == 0.
	/// IsUnpopulated: Level 2 existence check. Containers don't exist or, for a
	/// collection, Count == 0.
	// ---------------------------------------------------------------------------------
	virtual property bool IsUnpopulated
	{
		virtual bool get() override { return Cell::IsUnpopulated; }
	};

	// ---------------------------------------------------------------------------------
	/// Performs an IsUnpopulated check on the element at index if it exist, else
	/// returns true.
	/// IsUnpopulated: Level 2 existence check. Containers don't exist or, for a
	/// collection, Count == 0.
	// ---------------------------------------------------------------------------------
	virtual property bool IsUnpopulated[int]
	{
		virtual bool get(int index) override { return Cell::IsUnpopulated[index]; }
	};


	// ---------------------------------------------------------------------------------
	/// Performs an IsUnpopulated check on the element for 'key' if it exist, else 
	/// returns true.
	/// IsUnpopulated: Level 2 existence check. Containers don't exist or, for a
	/// collection, Count == 0.
	// ---------------------------------------------------------------------------------
	virtual property bool IsUnpopulated[SysStr^]
	{
		virtual bool get(SysStr^ key) override { return Cell::IsUnpopulated[key]; }
	};
	*/

	// ***********----------------------------------------------------------------------
	/// Implements IsReadOnly getter. Underlying containers are read only.
	// ***********----------------------------------------------------------------------
	virtual property bool IsReadOnly { virtual bool get() override { return Cell::IsReadOnly; } };


	// ---------------------------------------------------------------------------------
	/// Checks if the object contains a single named or unnamed element stored in 
	/// _UnaryKey and/or _UnaryValue.
	// ---------------------------------------------------------------------------------
	virtual property bool IsUnary { virtual bool get() override { return Cell::IsUnary; } };



	// ---------------------------------------------------------------------------------
	/// Returns the key at index. If IsList (!IsDictionary), returns index.ToString()
	// ---------------------------------------------------------------------------------
	virtual property SysStr^ Key[int]
	{
		virtual SysStr ^ get(int index) override { return Cell::Key[index]; }
	};


	// ***********----------------------------------------------------------------------
	/// Implements the Keys collection getter. If IsList (!IsDictionay) will convert
	/// index range to list of Strings^.
	// ***********----------------------------------------------------------------------
	virtual property ICollection<SysStr^>^ Keys
	{
		virtual ICollection<SysStr^>^ get() override { return Cell::Keys; }
	};

	// ---------------------------------------------------------------------------------
	/// Returns the length of the stored value if it exists else -1 if this object
	/// IsCollection else 0.
	// ---------------------------------------------------------------------------------
	virtual property int Length
	{
		virtual int get() override { return Cell::Length; }
	};


	// ---------------------------------------------------------------------------------
	/// Gets or sets the physical locally stored object. To get the latest real object
	/// if it exists use StorageObject and TransientObject instead.
	/// Setting the local object will clear the node chain / tree.
	// ---------------------------------------------------------------------------------
	virtual property SysObj^ LocalObject 
	{
		virtual SysObj^ get() override { return Cell::LocalObject; }
		virtual void set(SysObj^ value) override { Cell::LocalObject = value; }
	};


	// ---------------------------------------------------------------------------------
	/// Gets or sets the physical locally stored object cast as ValueType^. To get the latest
	/// real value if it exists use StorageValue and TransientValue instead.
	/// Setting the local value will clear the node chain / tree.
	// ---------------------------------------------------------------------------------
	virtual property ValueType^ LocalValue
	{
		virtual ValueType^ get() override { return Cell::LocalValue; }
		virtual void set(ValueType^ value) override { Cell::LocalValue = value; }
	};


	// ---------------------------------------------------------------------------------
	/// Gets or sets the physical locally stored object cast as String^. To get the
	/// latest real value if it exists use StorageString and TransientString instead.
	/// Setting the local value will clear the node chain / tree.
	// ---------------------------------------------------------------------------------
	virtual property SysStr^ LocalString
	{
		virtual SysStr^ get() override { return Cell::LocalString; }
		virtual void set(SysStr^ value) override { Cell::LocalString = value; }
	};


	// ---------------------------------------------------------------------------------
	/// Enables / disables recursive storage
	// ---------------------------------------------------------------------------------
	virtual property bool RecursiveStorage
	{
		virtual bool get() override { return Cell::RecursiveStorage; }
		virtual void set(bool value) override { Cell::RecursiveStorage = value; }
	};


	// ---------------------------------------------------------------------------------
	/// Gets the cell in the chain that holds the unique CellClass _Value for this cell if it
	/// exists. It exists if this cell's _Value is not null or, if it has a single
	/// element in it's collections then that cell will be recursively inspected.
	// ---------------------------------------------------------------------------------
	virtual property CellClass^ StorageCell
	{
		virtual CellClass^ get() override { return Cell::StorageCell; }
	};


	// ---------------------------------------------------------------------------------
	/// Gets or Sets the stored object of the CellClass if it exists. 
	/// ie. StorageCell != nullptr. For the Setter if StorageCell == nullptr, clears all
	/// containers for garbage collect and sets the LocalValue.
	// ---------------------------------------------------------------------------------
	virtual property SysObj^ StorageObject
	{
		virtual SysObj^ get() override { return Cell::StorageObject; }
		void set(SysObj^ value) override { Cell::StorageObject = value; }
	};


	// ---------------------------------------------------------------------------------
	/// Gets or Sets the stored value of the CellClass if it exists.
	/// ie. StorageCell != nullptr. For the Setter if StorageCell == nullptr, clears all
	/// containers for garbage collect and sets the LocalValue.
	// ---------------------------------------------------------------------------------
	virtual property ValueType^ StorageValue
	{
		virtual ValueType^ get() override { return Cell::StorageValue; }
		void set(ValueType^ value) override { Cell::StorageValue = value; }
	};


	// ---------------------------------------------------------------------------------
	/// Gets or Sets the stored value of the CellClass cast to String^ if it exists.
	/// ie. StorageCell != nullptr. For the Setter if StorageCell == nullptr, clears all
	/// containers for garbage collect and sets the LocalString.
	// ---------------------------------------------------------------------------------
	virtual property SysStr^ StorageString
	{
		virtual SysStr^ get() override { return Cell::StorageString; }
		void set(SysStr^ value) override { Cell::StorageString = value; }
	};


	// ---------------------------------------------------------------------------------
	/// Converts the stored value of the CellClass to String^ upprcase, if it exists.
	/// else returns "". 
	// ---------------------------------------------------------------------------------
	virtual property SysStr^ ToUpper
	{
		SysStr^ get() override { return Cell::ToUpper; }
	};


	// ---------------------------------------------------------------------------------
	/// Returns the cell that was stored in the last get or set of the StorageCell
	/// property. See <see ref="TransientValue"/> for further information.
	// ---------------------------------------------------------------------------------
	virtual property CellClass^ TransientCell
	{
		virtual CellClass^ get() override { return Cell::TransientCell; }
	};



	// ---------------------------------------------------------------------------------
	/// Gets the last value known since a call to StorageValue, StorageObject or
	/// StorageCell. To guarantee you are getting the latest stored object use
	/// StorageObject instead, which inspects the cell tree.
	/// The purpose of TransientObject is for inline code, eg:
	/// myVariable = (StorageObject == nullptr ? "SysObj is null" : TransientObject);
	/// In the above example StorageObject is used first which guarantees TransientObject
	/// or TransientValue will be accessing the latest StorageCell.
	// ---------------------------------------------------------------------------------
	virtual property SysObj^ TransientObject
	{
		virtual SysObj^ get() override { return Cell::TransientObject; }
		virtual void set(SysObj^ value) override { Cell::TransientObject = value; }
	};



	// ---------------------------------------------------------------------------------
	/// Gets the last value known since a call to StorageValue, StorageObject or
	/// StorageCell. To guarantee you are getting the latest stored value use
	/// StorageValue instead, which inspects the cell tree.
	/// The purpose of TransientValue is for inline code, eg:
	/// myVariable = (StorageValue == "Me" ? "Value is Mine" : TransientValue);
	/// In the above example StorageValue is used first which guarantees TransientValue
	/// or TransientObject will be accessing the latest StorageCell.
	// ---------------------------------------------------------------------------------
	virtual property ValueType^ TransientValue
	{
		virtual ValueType^ get() override { return Cell::TransientValue; }
		virtual void set(ValueType^ value) override { Cell::TransientValue = value; }
	};


	// ---------------------------------------------------------------------------------
	/// Gets the last object (as String^) known since a call to StorageValue,
	/// StorageString, StorageObject or StorageCell. To guarantee you are getting the
	/// latest stored object use StorageString instead, which inspects the cell tree.
	/// The purpose of TransientString is for inline code, eg:
	/// myVariable = (StorageString == nullptr ? "String is null" : TransientString);
	/// In the above example StorageString is used first which guarantees
	/// TransientString or Transient... will be accessing the latest StorageCell.
	// ---------------------------------------------------------------------------------
	property SysStr^ TransientString
	{
		virtual SysStr^ get() override { return Cell::TransientString; }
		virtual void set(SysStr^ value) override { Cell::TransientString = value; }
	};


	// ---------------------------------------------------------------------------------
	/// Gets or Sets the char located at the index/offset of the (String^) cast stored
	/// value of the cell. If the StorageValue is nullptr (and IsCollection for Get) or the
	/// offset is out of the bounds 0 to Length an exception is thrown. On Set if the
	/// offset is equal to the Length the char will be appended.
	/// If offset is zero and this object IsCollection, collections will be garbage collected
	/// and the Local value set to (String^)value; 
	// ---------------------------------------------------------------------------------
	virtual property XCHAR Value[int]
	{
		virtual XCHAR get(int index) override { return Cell::Value[index]; }
		virtual void set(int index, XCHAR value) override { Cell::Value[index] = value; }
	};


	// ***********----------------------------------------------------------------------
	/// Implements Values getter. Returns list of elements. If IsUnary returns wrapped
	/// _UnaryElement else if IsList (!IsDictionay) returns _Items else if IsDictionary
	/// will return _Dict Values.
	// ***********----------------------------------------------------------------------
	virtual property ICollection<CellClass^>^ Values
	{
		ICollection<CellClass^>^ get() override { return (ICollection<CellClass^>^)Cell::Values; }
	};


	// ---------------------------------------------------------------------------------
	/// Casts the stored object to String^ else returns nullptr if the stored value does
	/// not exist.
	// ---------------------------------------------------------------------------------
	virtual operator SysStr^() override
	{
		return Cell::operator SysStr^();
	};


	// ---------------------------------------------------------------------------------
	/// Returns false when IsNull else true for all other cases.
	// ---------------------------------------------------------------------------------
	virtual operator bool() override
	{
		return Cell::operator bool ();
	};




	// ---------------------------------------------------------------------------------
	/// Returns 0 when IsNull else ToInt32 if StorageObject is set else 1 for all other
	/// cases.
	// ---------------------------------------------------------------------------------
	virtual operator int() override
	{
		return Cell::operator int();
	};


	// ---------------------------------------------------------------------------------
	// Casts the stored object to wchar_t* else returns nullptr if the stored value does
	// not exist.
	// ---------------------------------------------------------------------------------

	/*
	virtual operator PCXSTR() override
	{

		if (StorageObject == nullptr)
			return NULL;

		IntPtr intptr = Marshal::StringToBSTR(TransientString);

		return (PCXSTR)intptr.ToPointer();
	};
	*/


	// ---------------------------------------------------------------------------------
	// Casts the stored object to char* else returns nullptr if the stored value does
	// not exist.
	// ---------------------------------------------------------------------------------
	/*
	virtual operator PYSTR() override
	{

		if (StorageObject == nullptr)
			return NULL;

		IntPtr intptr = Marshal::StringToBSTR(TransientString);

		return (PCYSTR)intptr.ToPointer();
	};
	*/
	static operator CellClass^(PCXSTR rhs)
	{
		CellClass^ cell = gcnew CellClass();
		cell->LocalString = (gcnew SysStr(rhs));
		return cell;
	};

	static operator CellClass ^ (PCYSTR rhs)
	{
		CellClass^ cell = gcnew CellClass();
		cell->LocalString = (gcnew SysStr(rhs));
		return cell;
	};

	static operator CellClass ^ (const SysStr^ rhs)
	{
		CellClass^ cell = gcnew CellClass();
		cell->LocalString = (SysStr^)rhs;
		return cell;
	};

	static operator CellClass ^ (const bool rhs)
	{
		CellClass^ cell = gcnew CellClass();
		if (rhs)
			cell->LocalObject = 1;
		return cell;
	};

	static operator CellClass ^ (const int rhs)
	{
		CellClass^ cell = gcnew CellClass();
		if (rhs != 0)
			cell->LocalObject = rhs;

		return cell;
	};

	// ---------------------------------------------------------------------------------
	/// Sets the LocalObject to (String^)value where value is type String^ and returns the
	/// updated CellClass&lt;ValueType^&gt;^.
	// ---------------------------------------------------------------------------------
	virtual CellClass% operator=(const SysStr^ rhs) new
	{
		return Cell::operator=(rhs);
	};

	// ---------------------------------------------------------------------------------
	/// Sets the LocalObject to 1 for true or calls Clear() for false and returns the
	/// updated Cell&lt;Tvalue&gt;^.
	// ---------------------------------------------------------------------------------
	virtual CellClass% operator=(const bool rhs) new
	{
		return Cell::operator=(rhs);
	}

	// ---------------------------------------------------------------------------------
	/// Sets the LocalObject to value for != 0 or calls Clear() for 0 and returns the
	/// updated Cell&lt;Tvalue&gt;^.
	// ---------------------------------------------------------------------------------
	virtual CellClass% operator=(const int rhs) new
	{
		return Cell::operator=(rhs);
	};

	// ---------------------------------------------------------------------------------
	/// Sets the LocalObject to (SysObj^)value where value is a null terminated type
	/// wchar_t* and returns the updated CellClass&lt;ValueType^&gt;^.
	// ---------------------------------------------------------------------------------
	virtual CellClass% operator=(PCXSTR rhs) new
	{
		return (CellClass%)Cell::operator=(rhs);
	};

	// ---------------------------------------------------------------------------------
	/// Sets the LocalObject to (SysObj^)value where value is a null terminated type
	/// char* and returns the updated CellClass&lt;ValueType^&gt;^.
	// ---------------------------------------------------------------------------------
	virtual CellClass% operator=(PCYSTR rhs) new
	{
		return (CellClass%)Cell::operator=(rhs);
	};

	// ---------------------------------------------------------------------------------
	/// Clears this and then performs a shallow (non-destructive) copy of CellClass value
	/// into this returning the updated CellClass&lt;1SysStr^&gt;^. To perform a deep copy use the
	/// copy constructor.
	// ---------------------------------------------------------------------------------
	virtual CellClass% operator=(const CellClass^ rhs) override
	{
		return (CellClass%)Cell::operator=(rhs);
	};

	// ---------------------------------------------------------------------------------
	/// Clears this and then performs a shallow (non-destructive) copy of CellClass value
	/// into this returning the updated CellClass&lt;1SysStr^&gt;^. To perform a deep copy use the
	/// copy constructor.
	// ---------------------------------------------------------------------------------
	virtual CellClass^ operator=(const CellClass% rhs) override
	{
		return (CellClass^)Cell::operator=(rhs);
	};

	// ---------------------------------------------------------------------------------
	/// If lhs is a collection rhs will be added as an element otherwise it will be
	/// concatenated, returning the updated CellClass&lt;ValueType^&gt;^.
	// ---------------------------------------------------------------------------------
	virtual CellClass^ operator+= (const SysStr^ rhs) new
	{
		return (CellClass^)Cell::operator+=(rhs);
	};


	// ---------------------------------------------------------------------------------
	/// If lhs is a collection rhs will be added as an element otherwise it will be
	/// concatenated, returning the updated CellClass&lt;ValueType^&gt;^.
	// ---------------------------------------------------------------------------------
	virtual CellClass^ operator+=(PCXSTR rhs) new
	{
		return (CellClass^)Cell::operator+=(rhs);
	};


	// ---------------------------------------------------------------------------------
	/// If either lhs or rhs is a collection both will be treated as collections and
	/// a new appended collection returned. If neither are collections they will be
	/// concatenated returning the updated CellClass&lt;ValueType^&gt;^.
	// ---------------------------------------------------------------------------------
	virtual CellClass^ operator+=(const CellClass^ rhs) override
	{
		return Cell::operator+=(rhs);
	};

	static CellClass^ operator++(CellClass^ lhs)
	{
		return (CellClass^)Cell::operator++(lhs);
	};


	// ---------------------------------------------------------------------------------
	/// Adds lhs String to rhs Cell^ returning a new String.
	// ---------------------------------------------------------------------------------
	static SysStr^ operator+(SysStr^ lhs, CellClass^ rhs)
	{
		return Cell::operator+(lhs, rhs);
	}


	// ---------------------------------------------------------------------------------
	/// Adds lhs wchar_t* to rhs Cell^ returning a new wchar_t*.
	// ---------------------------------------------------------------------------------
	static PXSTR operator+(PXSTR lhs, CellClass^ rhs)
	{
		return Cell::operator+(lhs, rhs);
	}


	// ---------------------------------------------------------------------------------
	/// Adds lhs char* to rhs Cell^ returning a new char*.
	// ---------------------------------------------------------------------------------
	static PYSTR operator+(PYSTR lhs, CellClass^ rhs)
	{
		return Cell::operator+(lhs, rhs);
	}


	// ---------------------------------------------------------------------------------
	/// Adds lhs to rhs returning a new CellClass.
	/// If lhs is a collection both will be treated as collections and a new collection
	/// returned else they will be concatenated. The new cell will be created using
	/// a shallow copy, preserving CellClass references.
	// ---------------------------------------------------------------------------------
	static CellClass^ operator+ (CellClass^ lhs, SysStr^ rhs)
	{
		return Cell::operator+(lhs, rhs);
	};


	// ---------------------------------------------------------------------------------
	/// If lhs is a collection both will be treated as collections and a new collection
	/// returned else they will be concatenated. The new cell will be created using
	/// a shallow copy, preserving CellClass references.
	// ---------------------------------------------------------------------------------
	static CellClass^ operator+(CellClass^ lhs, PXSTR rhs)
	{
		return (CellClass^)Cell::operator+(lhs, rhs);
	}


	// ---------------------------------------------------------------------------------
	/// If lhs is a collection both will be treated as collections and a new collection
	/// returned else they will be concatenated. The new cell will be created using
	/// a shallow copy, preserving CellClass references.
	// ---------------------------------------------------------------------------------
	static CellClass^ operator+(CellClass^ lhs, PYSTR rhs)
	{
		return (CellClass^)Cell::operator+(lhs, rhs);
	}


	// ---------------------------------------------------------------------------------
	// Concatenates rhs to lhs.
	// ---------------------------------------------------------------------------------
	/*
	static PXSTR operator+(PXSTR lhs, CellClass^ rhs)
	{
		return (gcnew SysStr(lhs)) + rhs->ToString();
	}
	*/


	// ---------------------------------------------------------------------------------
	// Concatenates rhs to lhs.
	// ---------------------------------------------------------------------------------
	/*
	static PYSTR operator+(PYSTR lhs, CellClass^ rhs)
	{
		return (gcnew SysStr(lhs)) + rhs->ToString();
	}
	*/


	// ---------------------------------------------------------------------------------
	/// If either lhs or rhs is a collection both will be treated as collections and
	/// a new collection returned. If neither are collections they will be concatenated
	/// a new Cell&lt;String^&gt;^ object. The new cell will be created using a shallow Clone()
	/// copy, preserving CellClass references.
	// ---------------------------------------------------------------------------------
	static CellClass^ operator+(CellClass^ lhs, CellClass^ rhs)
	{
		return (CellClass^)Cell::operator+(lhs, rhs);
	}


	// ---------------------------------------------------------------------------------
	/// Strictly not-equal deep negative equivalancy - Considers nullptr's to be equal
	/// and objects to be equal if their values are equal, even if they are
	/// referentially different objects.
	/// To consider nullptr's unequal and equal values unequal if they are referentially
	/// different objects use operator==.
	// ---------------------------------------------------------------------------------
	virtual bool operator!= (SysStr^ rhs) override
	{
		return Cell::operator!=(rhs);
	};



	// ---------------------------------------------------------------------------------
	/// Strictly not-equal deep negative equivalancy - Considers nullptr's to be equal
	/// and objects to be equal if their values are equal, even if they are
	/// referentially different objects.
	/// To consider nullptr's unequal and equal values unequal if they are referentially
	/// different objects use operator==.
	// ---------------------------------------------------------------------------------
	virtual bool operator!= (PXSTR rhs) override
	{
		return Cell::operator!=(rhs);
	};


	// ---------------------------------------------------------------------------------
	/// Performs a full tree strictly not-equal deep negative equivalancy check.
	/// Considers objects to be equal if their values are equal, even if they are
	/// referentially different objects.
	/// To consider equal values unequal if they are referentially different objects
	/// use operator==.
	// ---------------------------------------------------------------------------------
	static bool operator!=(CellClass^ lhs, CellClass^ rhs)
	{
		return Cell::operator!=(lhs, rhs);
	};


	// ---------------------------------------------------------------------------------
	/// Strictly equal shallow equivalancy - Considers nullptr's unequal and objects
	/// with equal values to be unequal if they are referentially different objects.
	/// To consider nullptr's equal and equal values to be equal even if they are
	/// referentially different objects use operator!=.
	// ---------------------------------------------------------------------------------
	virtual bool operator== (SysStr^ value) override
	{
		return Cell::operator==(value);
	};



	// ---------------------------------------------------------------------------------
	/// Strictly equal shallow equivalancy - Considers nullptr's unequal and objects
	/// with equal values to be unequal if they are referentially different objects.
	/// To consider nullptr's equal and equal values to be equal even if they are
	/// referentially different objects use operator!=.
	// ---------------------------------------------------------------------------------
	virtual bool operator==(PXSTR value) override
	{
		return Cell::operator==(value);
	};


	// ---------------------------------------------------------------------------------
	/// Performs a strictly equal shallow equivalancy check.
	/// Considers with equal values to be unequal if they are referentially different
	/// objects.
	/// To consider equal values to be equal even if they are referentially different
	/// objects use operator!=.
	// ---------------------------------------------------------------------------------
	static bool operator==(CellClass^ lhs, CellClass^ rhs)
	{
		return Cell::operator==(lhs, rhs);
	};

	static bool operator==(CellClass^ lhs, int rhs)
	{
		return Cell::operator==(lhs, rhs);
	}

#pragma endregion Property Accessors




	/// Default .ctor
	CellClass() : Cell() {};
	/// .ctor with initial and growth capacity.
	/// <param name="capacity">
	/// Sets the initial and growth capacity. If capacity is less than one, the default
	/// _InitialCapacity will be used.
	/// </param>
	CellClass(int capacity) : Cell(capacity) {};
	// ---------------------------------------------------------------------------------
	/// .ctor with an initial CellClass element.
	/// <param name="capacity">
	/// Sets the initial and growth capacity. If capacity is less than one, the default
	/// _InitialCapacity will be used.
	/// </param>
	// ---------------------------------------------------------------------------------
	CellClass(int capacity, CellClass^ element) : Cell(capacity, element) {};
	// ---------------------------------------------------------------------------------
	/// .ctor initialized with an unnamed element collection.
	// ---------------------------------------------------------------------------------
	CellClass(ICollection<CellClass^>^ collection) : Cell(collection) {};
	// ---------------------------------------------------------------------------------
	/// .ctor initialized with a string collection.
	// ---------------------------------------------------------------------------------
	CellClass(ICollection<SysStr^>^ collection) : Cell(collection) {};


	// ---------------------------------------------------------------------------------
	/// .ctor initialized with collection.
	// ---------------------------------------------------------------------------------
	CellClass(System::Collections::ICollection^ collection, int start, bool excludeNullOrEmpty) : Cell(collection->Count)
	{
		for each (SysStr ^ str in collection)
		{
			if (start-- > 0)
				continue;

			if (excludeNullOrEmpty && SysStr::IsNullOrEmpty(str))
				continue;

			Add(str);
		}
	}


	/// .ctor initialized with a key and CellClass^ element.
	CellClass(SysStr^ key, CellClass^ element) : Cell(key, element) {};
	/// .ctor initialized with a named element collection.
	CellClass(ICollection<KeyValuePair<SysStr^, CellClass^>>^ collection) : Cell(collection) {};
	/// .ctor deep (destructive) copy constructor. To perform a shallow copy use the
	/// assignment (operator=) operator.
	CellClass(CellClass^ cell) : Cell(cell) {};
	/// .ctor with unary string value initializer.
	CellClass(SysStr^ value) : Cell(value) {};
	/// .ctor with unary System::Int32 value initializer.
	CellClass(System::Int32^ value) : Cell(value) {};
	/// .ctor with unary System::Double value initializer.
	CellClass(System::Double^ value) : Cell(value) {};
	/// .ctor with unary PCXSTR value initializer.
	CellClass(PCXSTR value) : Cell(value) {};
	/// .ctor with unary PXSTR value initializer.
	CellClass(PCYSTR value) : Cell(value) {};
	/// .ctor with unary SysObj^ value initializer. Reference is preserved.
	CellClass(SysObj^ value) : Cell(value) {};



	// ---------------------------------------------------------------------------------
	/// Creates an instance of the current type.
	// ---------------------------------------------------------------------------------
	virtual SysObj^ CreateInstance() override
	{
		return gcnew CellClass();
	}



	// ---------------------------------------------------------------------------------
	/// Performs a shallow (non-destructive) copy of this into into clone. A shallow
	/// copy creates a mirror image of the source Cell, preserving object refrences.
	/// To create an entirely new set of objects use the deep copy CopyTo() method or
	/// the copy constructor.
	// ---------------------------------------------------------------------------------
	virtual void Clone(SysObj^ cloneObject) override { Cell::Clone(cloneObject); };


	// ---------------------------------------------------------------------------------
	/// Adds value as an element.
	// ---------------------------------------------------------------------------------
	virtual void Add(CellClass^ value) override { AbstractCell::Add(value); };


	// ---------------------------------------------------------------------------------
	/// Adds value as an element.
	// ---------------------------------------------------------------------------------
	virtual void Add(SysStr^ value) override { AbstractCell::Add(value); };


	// ---------------------------------------------------------------------------------
	/// Adds key object if it doesn't exist then adds value as an element to it's
	/// collection. To add value as a KeyValuePair use Add(KeyValuePair).
	/// If key is a nullptr adds the ordinal as a KeyValuePair.
	// ---------------------------------------------------------------------------------
	virtual void Add(SysStr^ key, CellClass^ value) override { AbstractCell::Add(key, value); };

	// ---------------------------------------------------------------------------------
	/// Adds key object if it doesn't exist then adds value as an element to it's
	/// collection. To add value as a KeyValuePair use Add(KeyValuePair).
	// ---------------------------------------------------------------------------------
	virtual void Add(SysStr^ key, SysStr^ value) override { AbstractCell::Add(key, value); };


	// ---------------------------------------------------------------------------------
	/// Adds a KeyValue pair dictionary element.
	// ---------------------------------------------------------------------------------
	virtual void Add(KeyValuePair<SysStr^, CellClass^> pair) override { AbstractCell::Add(pair); };

	// ---------------------------------------------------------------------------------
	/// Converts the current object to a collection then adds value as an element if it
	/// is not a collection else appends the collection elements to this collection.
	// ---------------------------------------------------------------------------------
	virtual void Append(CellClass^ value) override { AbstractCell::Append(value); };

	// ---------------------------------------------------------------------------------
	/// Creates the key object else ensures it's a collection then adds value as an
	/// element if it is not a collection else appends the collection elements to the
	/// key object collection.
	// ---------------------------------------------------------------------------------
	virtual void Append(SysStr^ key, CellClass^ value) override { AbstractCell::Append(key, value); };


	virtual CellClass^ ArraySlice(int offset, int length, bool preserveKeys) new
	{
		return (CellClass^)AbstractCell::ArraySlice(offset, length, preserveKeys);
	}


	// ---------------------------------------------------------------------------------
	/// Full reset of containers.
	// ---------------------------------------------------------------------------------
	virtual void Clear() override { AbstractCell::Clear(); };



	virtual bool Contains(SysStr^ value) override { return AbstractCell::Contains(value); };

	// ---------------------------------------------------------------------------------
	/// Performs a deep (destructive) copy of CellClass^ cell into this. A deep copy
	/// replicates each object in the tree creating an entirely new set of objects. To
	/// create a mirror image of the source CellClass and preserve object references perform
	/// a shallow Clone() copy or use the assignment (operator=) operator.
	// ---------------------------------------------------------------------------------
	virtual void CopyTo(CellClass^ cell) override { AbstractCell::CopyTo(cell); };


	virtual SysStr^ Implode() override { return AbstractCell::Implode(); };

	virtual SysStr^ Implode(SysStr^ separator) override { return AbstractCell::Implode(separator); };


	virtual bool InArray(List<CellClass^>^ list) override { return Cell::InArray(list); }


	// ---------------------------------------------------------------------------------
	/// Returns the index of the value cast to CellClass^ else -1.
	// ---------------------------------------------------------------------------------
	virtual int IndexOf(SysStr^ value) override { return AbstractCell::IndexOf(value); };

	// ---------------------------------------------------------------------------------
	/// Performs an insert of the key and String^ cast to ValueType value pair at the provided index.
	/// If the key already exists it is replaced if the index is the same or plus one,
	/// else it is removed.
	/// This object is migrated to a dictionary if required.
	// ---------------------------------------------------------------------------------
	virtual void Insert(int index, SysStr^ key, SysStr^ value) override
	{
		AbstractCell::Insert(index, key, value);
	};


	// ---------------------------------------------------------------------------------
	/// Searches the string value of the T elements and inserts the new value
	/// at the correct ignore-case position. If the status is IsDictionary
	/// will call InsertAfter for a dictionary element with a nullptr key.
	// ---------------------------------------------------------------------------------
	virtual CellClass^ InsertAfter(SysStr^ search, CellClass^ value) override
	{
		return (CellClass^)Cell::InsertAfter(search, value);
	};



	// ---------------------------------------------------------------------------------
	/// Searches the string value of the T elements and inserts the new pair
	/// at the correct ignore-case position. If the status is not IsDictionary will
	/// migrate to a dictionary first.
	/// If the element is unnamed the value of 'key' will be nullptr and a key will be
	/// created based on _CurrentSeed and _KeySeed.
	// ---------------------------------------------------------------------------------
	virtual CellClass^ InsertAfter(SysStr^ search, SysStr^ key, CellClass^ value) override
	{
		return (CellClass^)Cell::InsertAfter(search, key, value);
	};



	// ---------------------------------------------------------------------------------
	/// Returns the ordinal index of the key else -1. If the list is unnamed
	/// converts the key to an integer. If the converted key is within the index range
	/// of the list returns it else returns -1.
	// ---------------------------------------------------------------------------------
	virtual int KeyIndexOf(SysStr^ key) override { return Cell::KeyIndexOf(key); };



	// ---------------------------------------------------------------------------------
	/// Returns the key of the value else nullptr.
	/// If the list is an unnamed list returns the index cast to SysStr.
	// ---------------------------------------------------------------------------------
	virtual SysStr^ KeyOf(SysStr^ value) override { return Cell::KeyOf(value); };


	// ---------------------------------------------------------------------------------
	/// Performs a php array merge.
	// ---------------------------------------------------------------------------------
	virtual void Merge(IReplicant<StringCell^>^ value) override
	{
		AbstractCell::Merge(value);
	}


	// ---------------------------------------------------------------------------------
	/// Performs a full tree printout.
	// ---------------------------------------------------------------------------------
	virtual void PrintCell(SysStr^ title) override { AbstractCell::PrintCell(title); };


	// ---------------------------------------------------------------------------------
	/// Performs a full tree printout.
	// ---------------------------------------------------------------------------------
	virtual void PrintCell(SysStr^ key, CellClass^ value, int level) override { AbstractCell::PrintCell(key, value, level); };


	// ---------------------------------------------------------------------------------
	/// Removes the value of the key and or index and returns true if successful.
	/// Either index must not be -1 or key must not be nullptr or both may be specified.
	/// If the list is unnamed converts the key to an integer if it's not nullptr.
	/// If the converted key is within the index range of the list
	/// removes the value at that index and returns true.
	// ---------------------------------------------------------------------------------
	virtual bool Remove(int index, SysStr^ key) override { return Cell::Remove(index, key); };


	// ---------------------------------------------------------------------------------
	/// Overrides SysObj^ ToString() method.
	// ---------------------------------------------------------------------------------
	virtual SysStr^ ToString() override { return AbstractCell::ToString(); };

	// ---------------------------------------------------------------------------------
	/// Performs a destructive Trim on the stored value and returns the trimmed string.
	// ---------------------------------------------------------------------------------
	virtual SysStr^ Trimmed() override { return AbstractCell::Trimmed(); };

	// ---------------------------------------------------------------------------------
	/// Performs a non-destructive Trim on the stored value and returns the trimmed
	/// string.
	// ---------------------------------------------------------------------------------
	virtual SysStr^ Trim() override { return AbstractCell::Trim(); };



	// ---------------------------------------------------------------------------------
	/// Tries to get the value at 'index' and places it in 'value' returning true else
	/// returns false.
	// ---------------------------------------------------------------------------------
	virtual bool TryGetIndexedValue(int index, CellClass^% value) override
	{
		return Cell::TryGetIndexedValue(index, value);
	};



	// ---------------------------------------------------------------------------------
	/// Tries to convert a string segment to an unsigned integer then tries to get the
	/// indexed value.
	// ---------------------------------------------------------------------------------
	virtual bool TryGetIndexedValue(SysStr^ segment, CellClass^% value) override
	{
		return Cell::TryGetIndexedValue(segment, value);
	};



	/// <summary>
	/// Gets the value of the key and returns true if successful.
	/// If the list is unnamed converts the key to an integer.
	/// If the converted key is within the index range of the list
	/// gets the value at the index.
	/// </summary>
	virtual bool TryGetValue(SysStr^ key, CellClass^% value) override
	{
		return Cell::TryGetValue(key, value);
	};



	// ---------------------------------------------------------------------------------
	/// Implements the List enumerator object for IList&lt;T&gt;.
	// ---------------------------------------------------------------------------------
	virtual property IList<StringCell^>^ Enumerator
	{
		virtual IList<StringCell^>^ get() override
		{
			return RBase::Enumerator;
		}
	};

	/*
	// ---------------------------------------------------------------------------------
	/// Implements the default KeyValueEnumerator Dictionary enumerator object for (#define CellClass)
	// ---------------------------------------------------------------------------------
	virtual IEnumerator<KeyValuePair<SysStr^, CellClass^>>^ GetEnumerator() new = IEnumerable<KeyValuePair<SysStr^, CellClass^>>::GetEnumerator
	{
		return gcnew ReplicaEnumerator<SysStr^, CellClass^>(this, false);
	};

	// ---------------------------------------------------------------------------------
	/// Implements the ReplicaKey Dictionary enumerator object for (#define CellClass)
	/// ReplicaKeyPair(T) = KeyValuePair&lt;ReplicaKey, T&gt;.
	// ---------------------------------------------------------------------------------
	virtual property IEnumerable<KeyValuePair<SysStr^, CellClass^>>^ KeyValueEnumerator
	{
		virtual IEnumerable<KeyValuePair<SysStr^, CellClass^>>^ get() 
		{
			return (IEnumerable<KeyValuePair<SysStr^, CellClass^>>^)this;
		}
	};

	// ---------------------------------------------------------------------------------
	/// Implements the ReplicaKey Dictionary enumerator object for (#define CellClass)
	/// ReplicaKeyPair(T) = KeyValuePair&lt;ReplicaKey, T&gt;.
	// ---------------------------------------------------------------------------------
	virtual IEnumerator<ReplicaKeyPair(CellClass^)>^ GetReplicaStringCellEnumerator() = IEnumerable<ReplicaKeyPair(CellClass^)>::GetEnumerator
	{
		return gcnew ReplicaEnumerator<ReplicaKey, CellClass^>(this, true);
	};
	*/

	// ---------------------------------------------------------------------------------
	/// Implements the ReplicaKey Dictionary enumerator object for (#define CellClass)
	/// ReplicaKeyPair(T) = KeyValuePair&lt;ReplicaKey, T&gt;.
	// ---------------------------------------------------------------------------------
	virtual property IEnumerable<ReplicaKeyPair(CellClass^)>^ ReplicaKeyEnumerator
	{
		virtual IEnumerable<ReplicaKeyPair(CellClass^)>^ get() override
		{
			return RBase::ReplicaKeyEnumerator;
		}
	};


	virtual IEnumerator<KeyValuePair<SysStr^, CellClass^>>^ GetEnumerator() override
	{
		return RBase::GetEnumerator();
	};

};


}


#undef ARBase
#undef RBase
#undef CellClass

