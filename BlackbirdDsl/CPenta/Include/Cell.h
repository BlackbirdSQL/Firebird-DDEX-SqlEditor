#pragma once
#include "pch.h"
#include "CPentaCommon.h"
#include "AbstractCell.h"

// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)


 
// Replicant base classes.
#define ARBase AdvancedReplicant
#define RBase Replicant


using namespace System::Runtime::InteropServices;




namespace C5 {




/*
* Note: Wherever possible Abstract classes define properties and operators and declare all
* methods abstract and final classes define methods.
* The exception is AbstruseReplicant which implements all it's interfaces and declares any unhandled
* members abstract.
* 
* Overview of the Cell/Replicant hierarchy
* ----------------------------------------
*
* A Cell is loosely based on the LISP con-cell, List, Node structure because the original intention
* was to do a port from the pgsql parser.
* A cell either holds a T (Tvalue) value or it is a collection of elements of its own type. 
* Tvalue must be a reference managed type, String^, Object^ etc., so for primitives use type^, eg int^.
* If a Cell is a collection, it's Cell elements can be in an unnamed or named list. The ordinal index of
* unnamed elements in a named list is tracked.
* If there is only a single element, it's key, if it exists, and value are stored in unary variables to
* avoid the overhead of managing collections.
* So at any point in a Cell's hierarchy it can have a Tvalue value or be a collection of other Cells, which
* means that at any point it can be a value or a list or a dictionary or a mixed array.
* When a Cell is a value, no collections are instantiated, so other than the class's global signature, it
* carries no greater overhead than any unary value object may carry.
* The class suite uses templates and templates are resolved at build time, not dynamically, making them as
* fast as non-templated classes.
*
* IReplicant
* ----------
* IReplicant is the root interface in the hierarchy. It introduces the IDictionary and IList interfaces
* and a few properties to support, in addition to the default KeyValuePair, a special KeyValuePair where
* the Key is ReplicaKey, a key that provides information for both IDictionary and IList enumerations.
* 
* AbstruseReplicant<T>
* --------------------
* This is the root class in the hierarchy.
* It implements all the interface methods required by IReplicant which includes IDict and IList. As far
* as is possible it avoids the inner workings of a Replicant which it leaves as abstract to descendant
* classes.
* No collections are physically instantiated unless there is more than a single element.
*
* AbstractReplicant<T>
* --------------------
* Implements all properties required by AbstruseReplicant that were declared abstract.
*
* Replicant<T>
* ------------
* Implements all the methods required by AbstractReplicant and AbstruseReplicant that were declared
* abstract.
*
* AdvancedReplicant<T>
* --------------------
* Introduces nested subscripting for T types that themselves support IDictionary and IList
* subscripting. This class 'Cell<Tvalue>', is such a class.
*
* AbstractCell<T, Tvalue> [where T = Cell<Tvalue>^]
* ------------------------------------------------
* Overrides and defines or declares as 'override abstract' the members in the Replicant
* classes to support the holding of a physical Tvalue value instead of an IList or
* IDictionary. Declares as abstract some overriden and all new properties and operators
* required to fulfill the dual role of being a physical unary Tvalue value or a named/unnamed list.
* AbstractCell<T, Tvalue> will inherit AdvancedReplicant<T> as AdvancedReplicant<Cell<Tvalue>^>
*
* Cell<Tvalue>
* ------------
* Implements the abstract members declared in AbstractCell with Tvalue being the actual physical
* value type that will be stored as an Object^.
*
* In short Cell<Tvalue> pretty much functionally emulates a PHP array while still adhering to
* the principle of strong typing.
* As it stands a Cell is strongly typed around Tvalue and String^. It cannot mutate to
* another Tvalue type within it's Cell tree /chain hierarchy.
*
* A note on the storage location of a Cell's value:
* When accessing the value of a cell the value may be stored in it's _Value member,
* but if the cell is a collection, and the collection consists of only a single element,
* then that element will be considered the StorageCell of the _Value. Recursively if that
* element is a collection with only a single element, then that element is considered and so
* on recursively.
* However when setting the local value of a Cell, it's chain/tree is voided and it becomes its
* own StorageCell. Any Cells further down the chain are garbage collected unless they are
* referenced elsewhere.
* To disable recursive storage for a cell set it's RecursiveStorage property to false. To set the default
* for new cells, set the static GlobalRecursiveStorage to false.
*/



// =========================================================================================================
//												Cell Class
//
/// <summary>
/// Cell property and operator definitions.
/// A Cell<Tvalue> can be a Tvalue value, held as an Object^ in _Value or a collection of child Cells in
/// in the Replicant parent class AdvancedReplicant<Cell<Tvalue>>.
/// This class exposes the abstract members declared in AbstractCell<Cell<Tvalue>, Tvalue>.
/// Tvalue: The type of the member stored in a Cell when the Cell is a value container.
// =========================================================================================================
template<typename T, typename Tvalue> public ref class Cell : public AbstractCell<T, Tvalue>
{


// =========================================================================================================
#pragma region Overridden Property Accessors - Cell
// =========================================================================================================

public:


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets or sets the element located at index.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property T^ default[int]
	{
		virtual T^ get(int index) override { return RBase::default[index]; }

		virtual void set(int index, T ^ value) override
		{
			// Void any stored value, we're working with a collection now
			_Value = nullptr;
			RBase::default[index] = value;
		}
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets or sets the named element for key or key cast to int for unnamed elements.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property T^ default[SysStr^]
	{
		virtual T^ get(SysStr^ key) override { return RBase::default[key]; }

		virtual void set(SysStr ^ key, T ^ value) override
		{
			// Void any stored value, we're working with a collection now
			_Value = nullptr;
			RBase::default[key] = value;
		}
	};

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The nested indexed element of an indexed element.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property T^ default[int, int]
	{
		virtual T^ get(int index0, int index1) override { return ARBase::default[index0, index1]; }

		virtual void set(int index0, int index1, T ^ value) override
		{
			// Void any stored value, we're working with a collection now
			_Value = nullptr;
			ARBase::default[index0, index1] = value;
		}
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The nested named element of a named element.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property T^ default[SysStr^, SysStr^]
	{
		virtual T^ get(SysStr^ key0, SysStr^ key1) override { return ARBase::default[key0, key1]; }

		virtual void set(SysStr ^ key0, SysStr ^ key1, T ^ value) override
		{
			// Void any stored value, we're working with a collection now
			_Value = nullptr;
			ARBase::default[key0, key1] = value;
		}
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The nested named element of an indexed element.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property T^ default[SysStr^, int]
	{
		virtual T^ get(SysStr^ key, int index) override { return ARBase::default[key, index]; }

		virtual void set(SysStr ^ key, int index, T ^ value) override
		{
			// Void any stored value, we're working with a collection now
			_Value = nullptr;
			ARBase::default[key, index] = value;
		}
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The nested indexed element of a named element.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property T^ default[int, SysStr^]
	{
		virtual T^ get(int index, SysStr^ key) override { return ARBase::default[index, key]; }

		virtual void set(int index, SysStr ^ key, T ^ value) override
		{
			// Void any stored value, we're working with a collection now
			_Value = nullptr;
			ARBase::default[index, key] = value;
		}
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
			return (StorageObject == nullptr && RBase::IsNull);
		}
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs an IsNull check on the element at index if it exist, else
	/// returns true.
	/// IsNull: Level 1 existence check. Containers don't exist (are all nullptr).
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property bool IsNull[int]
	{
		virtual bool get(int index) override
		{
			Cell^ cell = this[index];

			if (IsNullPtr(cell))
				return true;

			return cell->IsNull;
		}
	};



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs an IsNull check on the element for 'key' if it exist, else 
	/// returns true.
	/// IsNull: Level 1 existence check. Containers don't exist (are all nullptr).
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property bool IsNull[SysStr^]
	{
		virtual bool get(SysStr ^ key) override
		{
			Cell^ cell = this[key];

			if (IsNullPtr(cell))
				return true;

			return cell->IsNull;
		}
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs an IsNull check on the element for the nested keys if it exist, else 
	/// returns true.
	/// IsNull: Level 1 existence check. Containers don't exist (are all nullptr).
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property bool IsNull[SysStr^, SysStr^]
	{
		virtual bool get(SysStr^ key0, SysStr^ key1) override
		{
			Cell^ cell = this[key0, key1];

			if (IsNullPtr(cell))
				return true;

			return cell->IsNull;
		}
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Checks value container for IsNullOrEmpty and collections for Count == 0.
	/// IsNullOrEmpty: Level 3 existence check. Value container IsNullOrEmpty and
	/// collection Count == 0.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property bool IsNullOrEmpty
	{
		virtual bool get() override
		{
			return (GetIsNullOrEmpty(StorageObject) && RBase::IsNullOrEmpty);
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
			Cell^ cell = this[index];

			if (IsNullPtr(cell))
				return true;

			return cell->IsNullOrEmpty;
		}
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs an IsNullOrEmpty check on the element for 'key' if it exist, else 
	/// returns true.
	/// IsNullOrEmpty: Level 3 existence check. Value container IsNullOrEmpty and
	/// collection Count == 0.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property bool IsNullOrEmpty[SysStr^]
	{
		virtual bool get(SysStr ^ key) override
		{
			Cell^ cell = this[key];

			if (IsNullPtr(cell))
				return true;

			return cell->IsNullOrEmpty;
		}
	};



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs an IsNullOrEmpty check on the element for the nested keys if it exist,
	/// else returns true.
	/// IsNullOrEmpty: Level 3 existence check. Value container IsNullOrEmpty and
	/// collection Count == 0.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property bool IsNullOrEmpty[SysStr^, SysStr^]
	{
		virtual bool get(SysStr ^ key0, SysStr^ key1) override
		{
			Cell^ cell = this[key0, key1];

			if (IsNullPtr(cell))
				return true;

			return cell->IsNullOrEmpty;
		}
	};

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns true if no containers have a value (== nullptr). If there is a
	///  collection will still return true if Count == 0.
	/// IsUnpopulated: Level 2 existence check. Containers don't exist or, for a
	/// collection, Count == 0.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property bool IsUnpopulated
	{
		virtual bool get() override
		{
			return (StorageObject == nullptr && RBase::IsUnpopulated);
		}
	};

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs an IsUnpopulated check on the element at index if it exist, else
	/// returns true.
	/// IsUnpopulated: Level 2 existence check. Containers don't exist or, for a
	/// collection, Count == 0.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property bool IsUnpopulated[int]
	{
		virtual bool get(int index) override
		{
			Cell^ cell = this[index];

			if (IsNullPtr(cell))
				return true;

			return cell->IsUnpopulated;
		}
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs an IsUnpopulated check on the element for 'key' if it exist, else 
	/// returns true.
	/// IsUnpopulated: Level 2 existence check. Containers don't exist or, for a
	/// collection, Count == 0.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property bool IsUnpopulated[SysStr^]
	{
		virtual bool get(SysStr^ key) override
		{
			Cell^ cell = this[key];

			if (IsNullPtr(cell))
				return true;

			return cell->IsUnpopulated;
		}
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs an IsUnpopulated check on the element for the nested key2 if it exist,
	/// else  returns true.
	/// IsUnpopulated: Level 2 existence check. Containers don't exist or, for a
	/// collection, Count == 0.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property bool IsUnpopulated[SysStr^, SysStr^]
	{
		virtual bool get(SysStr^ key0, SysStr^ key1) override
		{
			Cell^ cell = this[key0, key1];

			if (IsNullPtr(cell))
				return true;

			return cell->IsUnpopulated;
		}
	};

#pragma endregion Overriden Property Accessors





	// =========================================================================================================
#pragma region Property Accessors - Cell
// =========================================================================================================

public:

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns the length of the stored value if it exists else -1 if this object
	/// IsCollection else 0.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property int Length
	{
		virtual int get() override
		{
			if (IsCollection)
				return -1;
			if (StorageObject == nullptr)
				return 0;

			SysStr^ str = TransientString;

			if (str == nullptr)
				return 0;

			return str->Length;
		}
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets or sets the physical locally stored object. To get the latest real object
	/// if it exists use StorageObject and TransientObject instead.
	/// Setting the local object will clear the node chain / tree.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property SysObj^ LocalObject
	{
		virtual SysObj^ get() override
		{
			return _Value;
		}
		virtual void set(SysObj^ value) override
		{
			Clear();
			_Value = value;
			_StorageCell = (T^)this;
		}
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets or sets the physical locally stored object cast as Tvalue. To get the latest
	/// real value if it exists use StorageValue and TransientValue instead.
	/// Setting the local value will clear the node chain / tree.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property Tvalue LocalValue
	{
		virtual Tvalue get() override
		{
			if (LocalObject == nullptr)
				return nullptr;

			return safe_cast<Tvalue>(_Value);
		}
		virtual void set(Tvalue value) override
		{
			Clear();
			_Value = safe_cast<SysObj^>(value);
			_StorageCell = (T^)this;
		}
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets or sets the physical locally stored object cast as SysStr^. To get the
	/// latest real value if it exists use StorageString and TransientString instead.
	/// Setting the local value will clear the node chain / tree.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property SysStr^ LocalString
	{
		virtual SysStr^ get() override
		{
			if (LocalObject == nullptr)
				return nullptr;

			SysStr^ str = (safe_cast<SysStr^>(_Value));
			if (str == nullptr)
				str = gcnew SysStr("");

			return str;
		}
		virtual void set(SysStr^ value) override
		{
			Clear();
			_Value = safe_cast<SysObj^>(value);
			_StorageCell = (T^)this;
		}
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Enables / disables recursive storage
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property bool RecursiveStorage
	{
		virtual bool get() override
		{
			return _RecursiveStorage;
		}
		virtual void set(bool value) override
		{
			_RecursiveStorage = value;
		}
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the cell in the chain that holds the unique Cell _Value for this cell if it
	/// exists. It exists if this cell's _Value is not null or, if it has a single
	/// element in it's collections then that cell will be recursively inspected.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property T^ StorageCell
	{
		virtual T^ get() override
		{
			if (LocalObject != nullptr)
				_StorageCell = (T^)this;
			else if (_RecursiveStorage && Count == 1)
				_StorageCell = Replicant::default[0]->StorageCell;
			else
				_StorageCell = nullptr;

			return _StorageCell;
		}
	};



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets or Sets the stored object of the Cell if it exists. 
	/// ie. StorageCell != nullptr. For the Setter if StorageCell == nullptr, clears all
	/// containers for garbage collect and sets the LocalValue.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property SysObj^ StorageObject
	{
		virtual SysObj^ get() override
		{
			return (IsNullPtr(StorageCell) ? nullptr : TransientCell->_Value);
		}
		void set(SysObj^ value) override
		{
			if (IsNullPtr(StorageCell))
				LocalObject = value;
			else
				TransientCell->LocalObject = value;
		}
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets or Sets the stored value of the Cell if it exists.
	/// ie. StorageCell != nullptr. For the Setter if StorageCell == nullptr, clears all
	/// containers for garbage collect and sets the LocalValue.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property Tvalue StorageValue
	{
		virtual Tvalue get() override
		{
			if (StorageObject == nullptr)
				return nullptr;

			return safe_cast<Tvalue>(TransientCell->_Value);
		}
		void set(Tvalue value) override
		{
			if (IsNullPtr(StorageCell))
				LocalValue = value;
			else
				TransientCell->LocalValue = value;
		}
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets or Sets the stored value of the Cell cast to SysStr^ if it exists.
	/// ie. StorageCell != nullptr. For the Setter if StorageCell == nullptr, clears all
	/// containers for garbage collect and sets the LocalString.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property SysStr^ StorageString
	{
		virtual SysStr^ get() override
		{
			if (StorageObject == nullptr)
				return nullptr;

			SysStr^ str = (safe_cast<SysStr^>(TransientCell->_Value));
			if (str == nullptr)
				str = gcnew SysStr("");

			return str;
		}
		void set(SysStr^ value) override
		{
			if (IsNullPtr(StorageCell))
				LocalString = value;
			else
				TransientCell->LocalString = value;
		}
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Converts the stored value of the Cell to SysStr^ upprcase, if it exists.
	/// else returns "". 
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property SysStr^ ToUpper
	{
		SysStr^ get() override
		{
			if (IsCollection)
				return "";
			if (StorageObject == nullptr)
				return "";

			SysStr^ str = TransientString;

			if (str == nullptr)
				return "";

			return str->ToUpper();
		}
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns the cell that was stored in the last get or set of the StorageCell
	/// property. See <see ref="TransientValue"/> for further information.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property T^ TransientCell
	{
		virtual T^ get() override
		{
			return _StorageCell;
		}
	};



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the last value known since a call to StorageValue, StorageObject or
	/// StorageCell. To guarantee you are getting the latest stored object use
	/// StorageObject instead, which inspects the cell tree.
	/// The purpose of TransientObject is for inline code, eg:
	/// myVariable = (StorageObject == nullptr ? "SysObj is null" : TransientObject);
	/// In the above example StorageObject is used first which guarantees TransientObject
	/// or TransientValue will be accessing the latest StorageCell.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property SysObj^ TransientObject
	{
		virtual SysObj^ get() override
		{
			return (IsNullPtr(_StorageCell) ? nullptr : TransientCell->_Value);
		}
		virtual void set(SysObj^ value) override
		{
			if (!IsNullPtr(_StorageCell))
				TransientCell->LocalObject = value;
			else
				LocalObject = value;
		}
	};



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the last value known since a call to StorageValue, StorageObject or
	/// StorageCell. To guarantee you are getting the latest stored value use
	/// StorageValue instead, which inspects the cell tree.
	/// The purpose of TransientValue is for inline code, eg:
	/// myVariable = (StorageValue == "Me" ? "Value is Mine" : TransientValue);
	/// In the above example StorageValue is used first which guarantees TransientValue
	/// or TransientObject will be accessing the latest StorageCell.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property Tvalue TransientValue
	{
		virtual Tvalue get() override
		{
			if (IsNullPtr(_StorageCell))
				return nullptr;

			return (safe_cast<Tvalue>(TransientCell->_Value));
		}
		virtual void set(Tvalue value) override
		{
			if (!IsNullPtr(_StorageCell))
				TransientCell->LocalValue = value;
			else
				LocalValue = value;
		}
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the last object (as SysStr^) known since a call to StorageValue,
	/// StorageString, StorageObject or StorageCell. To guarantee you are getting the
	/// latest stored object use StorageString instead, which inspects the cell tree.
	/// The purpose of TransientString is for inline code, eg:
	/// myVariable = (StorageString == nullptr ? "SysStr is null" : TransientString);
	/// In the above example StorageString is used first which guarantees
	/// TransientString or Transient... will be accessing the latest StorageCell.
	/// </summary>
	// ---------------------------------------------------------------------------------
	property SysStr^ TransientString
	{
		virtual SysStr^ get() override
		{
			if (IsNullPtr(_StorageCell))
				return nullptr;

			SysStr^ str;

			str = (safe_cast<SysStr^>(TransientCell->_Value));

			if (str == nullptr)
				str = gcnew SysStr("");

			return str;
		}
		virtual void set(SysStr^ value) override
		{
			if (!IsNullPtr(_StorageCell))
				TransientCell->LocalString = value;
			else
				LocalString = value;
		}
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets or Sets the char located at the index/offset of the (SysStr^) cast stored
	/// value of the cell. If the StorageValue is nullptr (and IsCollection for Get) or the
	/// offset is out of the bounds 0 to Length an exception is thrown. On Set if the
	/// offset is equal to the Length the char will be appended.
	/// If offset is zero and this object IsCollection, collections will be garbage collected
	/// and the Local value set to (SysStr^)value; 
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property XCHAR Value[int]
	{
		virtual XCHAR get(int index) override
		{
			int len = Length;

			if (index < 0 || index > len)
			{
				System::IndexOutOfRangeException^ ex = gcnew System::IndexOutOfRangeException(
					SysStr::Format("Offset {0} requested on string length of {1}.", index,
						TransientObject == nullptr ? "nullptr" : len.ToString()));

				Diag::Ex(ex);
				throw ex;
			}

			if (index == len)
				return '\0';

			SysStr^ str = TransientString;

			return str[index];
		}

		virtual void set(int index, XCHAR value) override
		{
			int len = Length;

			if (index < 0 || index > len)
			{
				System::IndexOutOfRangeException^ ex = gcnew System::IndexOutOfRangeException(
					SysStr::Format("Offset {0} requested on string length of {1}.", index,
						(TransientObject == nullptr) ? "nullptr" : len.ToString()));

				Diag::Ex(ex);
				throw ex;
			}


			SysStr^ str;

			if (index == 0 && value == '\0')
			{
				_Value = safe_cast<SysObj^>("");
			}
			else if (IsNullPtr(_StorageCell))
			{
				LocalString = System::Char::ToString(value);
			}
			else if (index == Length)
			{
				str = TransientString;

				LocalString = (str + System::Char::ToString(value));
			}
			else
			{
				str = TransientString;
				if (str == nullptr)
				{
					System::InvalidCastException^ ex = gcnew System::InvalidCastException("Could not cast stored typr to SysStr^");
					Diag::Ex(ex);
					throw ex;
				}
				LocalString = MarshalSetChar(str, index, value);
			}
		}
	};


	// ***********----------------------------------------------------------------------
	/// <summary>
	/// Implements Values getter. Returns list of elements. If IsUnary returns wrapped
	/// _UnaryElement else if IsList (!IsDictionay) returns _Items else if IsDictionary
	/// will return _Dict Values.
	/// </summary>
	// ***********----------------------------------------------------------------------
	virtual property ICollection<T^>^ Values
	{
		ICollection<T^>^ get() override { return Replicant::Values; }
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Casts the stored object to SysStr^ else returns nullptr if the stored value does
	/// not exist.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual operator SysStr ^ () override
	{
		return ToString();
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns false when IsNull else true for all other cases.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual operator bool () override
	{
		if (IsNull)
			return false;
		return true;
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns 0 when IsNull else ToInt32 if StorageObject is set else 1 for all other
	/// cases.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual operator int() override
	{
		if (IsNull)
			return 0;
		else if (StorageObject != nullptr)
			return System::Convert::ToInt32(TransientObject);

		return 1;
	};

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Casts the stored object to wchar_t* else returns nullptr if the stored value does
	/// not exist.
	/// </summary>
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
	/// <summary>
	/// Casts the stored object to char* else returns nullptr if the stored value does
	/// not exist.
	/// </summary>
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


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Sets the LocalObject to (SysObj^)value where value is type SysStr^ and returns the
	/// updated Cell<Tvalue>^.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual T% operator=(const SysStr^ rhs) override
	{
		LocalString = (SysStr^)rhs;
		return (T%)*this;
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Sets the LocalObject to 1 for true or calls Clear() for false and returns the
	/// updated Cell<Tvalue>^.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual T% operator=(const bool rhs) override
	{
		if (rhs)
			LocalObject = 1;
		else
			Clear();

		return (T%)*this;
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Sets the LocalObject to value for != 0 or calls Clear() for 0 and returns the
	/// updated Cell<Tvalue>^.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual T% operator=(const int rhs) override
	{
		if (rhs != 0)
			LocalObject = rhs;
		else
			Clear();

		return (T%)*this;
	};

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Sets the LocalObject to (SysObj^)value where value is a null terminated type
	/// wchar_t* and returns the updated Cell<Tvalue>^.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual T% operator=(PCXSTR rhs) override
	{
		LocalString = gcnew SysStr(rhs);
		return (T%)*this;
	};

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Sets the LocalObject to (SysObj^)value where value is a null terminated type
	/// char* and returns the updated Cell<Tvalue>^.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual T% operator=(PCYSTR rhs) override
	{
		LocalString = gcnew SysStr(rhs);
		return (T%)*this;
	};

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Clears this and then performs a shallow (non-destructive) copy of Cell value
	/// into this returning the updated Cell<Tvalue>^. To perform a deep copy use the
	/// copy constructor.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual T% operator=(const T^ rhs) override
	{
		((Cell^)rhs)->Clone(this);

		return (T%)*this;
	};

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Clears this and then performs a shallow (non-destructive) copy of Cell value
	/// into this returning the updated Cell<Tvalue>^. To perform a deep copy use the
	/// copy constructor.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual T^ operator=(const T% rhs) override
	{
		((Cell^)(%rhs))->Clone(this);

		return (T^)this;
	};

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// If lhs is a collection rhs will be added as an element otherwise it will be
	/// concatenated, returning the updated Cell<Tvalue>^.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual T^ operator+= (const SysStr^ rhs) override
	{
		if (IsCollection)
		{
			Add((SysStr^)rhs);
			return (T^)this;
		}

		SysStr^ str = "";

		if (StorageObject != nullptr)
			str = TransientString;

		TransientString = (str + (SysStr^)rhs);

		return (T^)this;
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// If lhs is a collection rhs will be added as an element otherwise it will be
	/// concatenated, returning the updated Cell<Tvalue>^.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual T^ operator+=(PCXSTR rhs) override
	{
		return operator+=(gcnew SysStr(rhs));
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// If either lhs or rhs is a collection both will be treated as collections and
	/// returning the updated collection. If neither are collections they will be
	/// concatenated returning the updated Cell<Tvalue>^.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual T^ operator+=(const T^ rhs) override
	{
		Cell^ cell = (Cell^)rhs;

		if (!IsCollection && !cell->IsCollection)
		{
			if (cell->StorageObject != nullptr)
				operator+=(cell->TransientString);
			return (T^)this;
		}

		Append((T^)rhs);

		return (T^)this;
	};


	static Cell^ operator++(Cell^ lhs)
	{
		int value = 0;

		if (lhs->StorageObject != nullptr)
			value = safe_cast<System::Int32>(lhs->TransientObject);

		lhs->LocalObject = ++value;

		return lhs;
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Adds lhs String to rhs Cell^ returning a new String.
	/// </summary>
	// ---------------------------------------------------------------------------------
	static SysStr^ operator+(SysStr^ lhs, Cell^ rhs)
	{
		SysStr^ str = gcnew SysStr(lhs);

		str += (IsNullPtr(rhs) ? "" : (rhs->StorageObject == nullptr ? "" : rhs->TransientString));

		return str;
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Adds lhs wchar_t* to rhs Cell^ returning a new wchar_t*.
	/// </summary>
	// ---------------------------------------------------------------------------------
	static PXSTR operator+(PXSTR lhs, Cell^ rhs)
	{
		SysStr^ str = gcnew SysStr(lhs);

		str += (IsNullPtr(rhs) ? "" : (rhs->StorageObject == nullptr ? "" : rhs->TransientString));

		System::IntPtr intptr = Marshal::StringToBSTR(str);

		return (PXSTR)intptr.ToPointer();
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Adds lhs char* to rhs Cell^ returning a new char*.
	/// </summary>
	// ---------------------------------------------------------------------------------
	static PYSTR operator+(PYSTR lhs, Cell^ rhs)
	{
		SysStr^ str = gcnew SysStr(lhs);

		str += (IsNullPtr(rhs) ? "" : (rhs->StorageObject == nullptr ? "" : rhs->TransientString));

		System::IntPtr intptr = Marshal::StringToBSTR(str);

		return (PYSTR)intptr.ToPointer();
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Adds lhs to rhs returning a new Cell.
	/// If lhs is a collection both will be treated as collections and a new collection
	/// returned else they will be concatenated. The new cell will be created using
	/// a shallow copy, preserving Cell references.
	/// </summary>
	// ---------------------------------------------------------------------------------
	static T^ operator+ (Cell^ lhs, SysStr^ rhs)
	{
		T^ cell;

		if (!IsNullPtr(lhs))
		{
			cell = (T^)lhs->CreateInstance();
		}
		else
		{
			cell = gcnew T();
		}

		lhs->Clone(cell);

		cell += rhs;

		return cell;
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// If lhs is a collection both will be treated as collections and a new collection
	/// returned else they will be concatenated. The new cell will be created using
	/// a shallow copy, preserving Cell references.
	/// </summary>
	// ---------------------------------------------------------------------------------
	static T^ operator+(Cell^ lhs, PXSTR rhs)
	{
		return operator+(lhs, gcnew SysStr(rhs));
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// If lhs is a collection both will be treated as collections and a new collection
	/// returned else they will be concatenated. The new cell will be created using
	/// a shallow copy, preserving Cell references.
	/// </summary>
	// ---------------------------------------------------------------------------------
	static T^ operator+(Cell^ lhs, PYSTR rhs)
	{
		return operator+(lhs, gcnew SysStr(rhs));
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Converts lhs to Int32 and subtracts rhs returning result.
	/// </summary>
	// ---------------------------------------------------------------------------------
	static int operator-(Cell^ lhs, int rhs)
	{
		int result = 0;

		if (!IsNullPtr(lhs))
			result = lhs->operator int();

		return result - rhs;
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Concatenates rhs to lhs.
	/// </summary>
	// ---------------------------------------------------------------------------------
	/*
	static PXSTR operator+(PXSTR lhs, Cell^ rhs)
	{
		return (gcnew SysStr(lhs)) + rhs->ToString();
	}
	*/


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Concatenates rhs to lhs.
	/// </summary>
	// ---------------------------------------------------------------------------------
	/*
	static PYSTR operator+(PYSTR lhs, Cell^ rhs)
	{
		return (gcnew SysStr(lhs)) + rhs->ToString();
	}
	*/


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// If either lhs or rhs is a collection both will be treated as collections and
	/// a new collection returned. If neither are collections they will be concatenated
	/// a new Cell<Tvalue>^ object. The new cell will be created using a shallow Clone()
	/// copy, preserving Cell references.
	/// </summary>
	// ---------------------------------------------------------------------------------
	static T^ operator+(Cell^ lhs, T^ rhs)
	{
		T^ cell;

		if (!IsNullPtr(rhs))
		{
			cell = (T^)rhs->CreateInstance();
		}
		else if (!IsNullPtr(lhs))
		{
			cell = (T^)lhs->CreateInstance();
		}
		else
		{
			return nullptr;
		}

		lhs->CopyTo(cell);
		cell += rhs;

		return cell;
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Strictly not-equal deep negative equivalancy - Considers nullptr's to be equal
	/// and objects to be equal if their values are equal, even if they are
	/// referentially different objects.
	/// To consider nullptr's unequal and equal values unequal if they are referentially
	/// different objects use operator==.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual bool operator!= (SysStr^ rhs) override
	{
		if (Count > 1)
			return true;

		bool isNullOrEmpty = IsNullOrEmpty;

		if (isNullOrEmpty && SysStr::IsNullOrEmpty(rhs))
			return false;

		if (isNullOrEmpty || SysStr::IsNullOrEmpty(rhs))
			return true;


		return (TransientString != rhs);
	};



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Strictly not-equal deep negative equivalancy - Considers nullptr's to be equal
	/// and objects to be equal if their values are equal, even if they are
	/// referentially different objects.
	/// To consider nullptr's unequal and equal values unequal if they are referentially
	/// different objects use operator==.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual bool operator!= (PXSTR rhs) override
	{
		if (Count > 1)
			return true;

		return operator!=(gcnew SysStr(rhs));
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs a full tree strictly not-equal deep negative equivalancy check.
	/// Considers objects to be equal if their values are equal, even if they are
	/// referentially different objects.
	/// To consider equal values unequal if they are referentially different objects
	/// use operator==.
	/// </summary>
	// ---------------------------------------------------------------------------------
	static bool operator!=(Cell^ lhs, Cell^ rhs)
	{
		// The only time the Count's could be different is when one Cell has
		// a Count == 0 and the other Count == 1 because we could still compare a collection"s
		// single element value against the other's value, so..
		if ((lhs->Count != rhs->Count)
			&& (lhs->Count > 1 || rhs->Count > 1))
		{
			return true;
		}

		// Check for differing stored object status. operator!= is strictly unequal
		// so we don't care if the actual values are stored at different physical locations.
		if ((lhs->StorageObject != nullptr && rhs->StorageObject == nullptr)
			|| (lhs->TransientObject == nullptr && rhs->StorageObject != nullptr))
		{
			return true;
		}

		// After the above conditional logic both objects' Transients will be active
		// and either both are null or both are not null.
		// If they're not null we can check the Transients.
		if (lhs->TransientObject != nullptr)
		{
			return (lhs->TransientString != rhs->TransientString);
		}

		// At this point the StorageObjects are both null.

		// Both Counts are equal and a Count of 0 has been caught in the StorageCell validation net so...
		if (lhs->Count < 1)
			return false;

		// Compare collections
		if (lhs->IsDictionary != rhs->IsDictionary)
			return true;

		int i = 0;
		Cell^ cell;

		// Loop through collection
		if (lhs->IsDictionary)
		{
			for each (KeyValuePair<SysStr^, T^> pair in lhs)
			{
				if (pair.Key != rhs->Key[i])
					return true;

				cell = rhs[pair.Key];

				// If the two objects are one and the same, continue.
				if ((Object^)(pair.Value) == (Object^)cell)
				{
					i++;
					continue;
				}

				// If one is null and the other not.
				if (IsNullPtr(pair.Value) || IsNullPtr(cell))
					return true;

				// Perform a recursive check on the two objects.
				if (cell->operator!=(pair.Value))
					return true;

				i++;
			}

			return false;
		}

		i = 0;

		for each (SysObj ^ item in lhs)
		{
			Cell^ cell = rhs[i];

			// If the two objects are one and the same, continue.
			if (item == cell)
			{
				i++;
				continue;
			}

			// If one is null and the other not.
			if (IsNullPtr(item) || IsNullPtr(cell))
				return true;

			// Perform a recursive check on the two objects.
			if (cell->operator!=((Cell^)item))
				return true;

			i++;
		}

		return false;
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Strictly equal shallow equivalancy - Considers nullptr's unequal and objects
	/// with equal values to be unequal if they are referentially different objects.
	/// To consider nullptr's equal and equal values to be equal even if they are
	/// referentially different objects use operator!=.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual bool operator== (SysStr^ value) override
	{
		if (Count != 0)
			return false;

		if (LocalObject == nullptr || value == nullptr)
			return false;

		return (LocalString == value);
	};



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Strictly equal shallow equivalancy - Considers nullptr's unequal and objects
	/// with equal values to be unequal if they are referentially different objects.
	/// To consider nullptr's equal and equal values to be equal even if they are
	/// referentially different objects use operator!=.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual bool operator==(PXSTR value) override
	{
		if (Count != 0)
			return false;

		return operator==(gcnew SysStr(value));
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs a strictly equal shallow equivalancy check.
	/// Considers with equal values to be unequal if they are referentially different
	/// objects.
	/// To consider equal values to be equal even if they are referentially different
	/// objects use operator!=.
	/// </summary>
	// ---------------------------------------------------------------------------------
	static bool operator==(Cell^ lhs, T^ rhsT)
	{
		if (IsNullPtr(lhs) && IsNullPtr(rhsT))
			return true;

		if (IsNullPtr(lhs) || IsNullPtr(rhsT))
			return false;

		Cell^ rhs = (Cell^)rhsT;

		if (lhs->IsNull != rhs->IsNull || lhs->Count != rhs->Count || lhs->LocalObject != rhs->LocalObject)
			return false;

		if (lhs->IsNull)
			return true;

		int i = 0;

		// Loop through collection
		if (lhs->IsDictionary)
		{
			for each (KeyValuePair<SysStr^, T^> pair in lhs)
			{
				if (pair.Key != rhs->Key[i])
					return false;

				// If the two objects are not one and the same => not equal.
				if (pair.Value != rhs[pair.Key])
					return false;

				i++;
			}

			return true;
		}

		i = 0;

		for each (SysObj ^ item in lhs)
		{
			// If the two objects are not one and the same => not equal.
			if ((Cell^)item != rhs[i])
				return false;

			i++;
		}

		return true;

	};

	static bool operator==(Cell^ lhs, int rhs)
	{
		int value = 0;

		if (!IsNullPtr(lhs) && lhs->StorageObject != nullptr)
			value = safe_cast<System::Int32>(lhs->TransientObject);

		return (value == rhs);
	}


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
	Cell() : AbstractCell()
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
	Cell(int capacity) : AbstractCell(capacity)
	{
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// .ctor with an initial Cell element.
	/// </summary>
	/// <param name="capacity">
	/// Sets the initial and growth capacity. If capacity is less than one, the default
	/// _InitialCapacity will be used.
	/// </param>
	// ---------------------------------------------------------------------------------
	Cell(int capacity, T^ element) : AbstractCell(capacity, element)
	{
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// .ctor initialized with an unnamed element collection.
	/// </summary>
	// ---------------------------------------------------------------------------------
	Cell(ICollection<T^>^ collection) : AbstractCell(collection)
	{
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// .ctor initialized with an string collection.
	/// </summary>
	// ---------------------------------------------------------------------------------
	Cell(ICollection<SysStr^>^ collection) : AbstractCell(collection)
	{
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// .ctor initialized with a key and Cell^ element.
	/// </summary>
	// ---------------------------------------------------------------------------------
	Cell(SysStr^ key, T^ element) : AbstractCell(key, element)
	{
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// .ctor initialized with a named element collection.
	/// </summary>
	// ---------------------------------------------------------------------------------
	Cell(ICollection<KeyValuePair<SysStr^, T^>>^ collection) : AbstractCell(collection)
	{
	};



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// .ctor deep (destructive) copy constructor. To perform a shallow copy use the
	/// assignment (operator=) operator.
	/// </summary>
	// ---------------------------------------------------------------------------------
	Cell(T^ cell) : AbstractCell(cell)
	{
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// .ctor with unary string value initializer.
	/// </summary>
	// ---------------------------------------------------------------------------------
	Cell(SysStr^ value) : AbstractCell(value)
	{
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// .ctor with unary PCXSTR value initializer.
	/// </summary>
	// ---------------------------------------------------------------------------------
	Cell(PCXSTR value) : AbstractCell(value)
	{
	};

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// .ctor with unary PXSTR value initializer.
	/// </summary>
	// ---------------------------------------------------------------------------------
	Cell(PCYSTR value) : AbstractCell(value)
	{
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// .ctor with unary SysObj^ value initializer. Reference is preserved.
	/// </summary>
	// ---------------------------------------------------------------------------------
	Cell(SysObj^ value) : AbstractCell(value)
	{
	};



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Creates an instance of the current type.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual SysObj^ CreateInstance() override
	{
		return gcnew Cell();
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs a shallow (non-destructive) copy of this into into clone. A shallow
	/// copy creates a mirror image of the source Cell, preserving object refrences.
	/// To create an entirely new set of objects use the deep copy CopyTo() method or
	/// the copy constructor.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual void Clone(SysObj^ cloneObject) override
	{
		Cell^ clone = (Cell^)cloneObject;

		clone->Clear();

		if (!IsCollection)
		{
			clone->_Value = _Value;
			return;
		}


		ARBase::Clone(clone);

	};


#pragma endregion Constructors/Destructors


};



}

#undef ARBase
#undef RBase
