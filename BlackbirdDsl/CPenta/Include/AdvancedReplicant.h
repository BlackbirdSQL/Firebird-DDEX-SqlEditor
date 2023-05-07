#pragma once
#include "pch.h"
#include "CPentaCommon.h"
#include "Replicant.h"

// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)



using namespace System::Collections::Generic;



namespace C5 {



// =========================================================================================================
//											AdvancedReplicant Class
//
/// <summary>
/// Implements nested subscripting for classes that support IDictionary and IList subscripting.
/// T: The type of the value stored in the array. In descendant Cell classes this would by the descendant
/// Replicant itself, ie. Cell<Cell<T>>.
/// </summary>
// =========================================================================================================
template<typename T> public ref class AdvancedReplicant : public Replicant<T>
{


#pragma region Property Accessors - AdvancedReplicant

public:

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The nested indexed element of an indexed element.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property T default[int, int]
	{
		virtual T get(int index0, int index1)
		{
			AdvancedReplicant^ item = (AdvancedReplicant^)this[index0];

			if (IsNullPtr(item))
				return nullptr;

			return item[index1];
		}

		virtual void set(int index0, int index1, T value)
		{
			AdvancedReplicant^ item = (AdvancedReplicant^)this[index0];

			if (IsNullPtr(item))
			{
				item = (AdvancedReplicant^)CreateInstance();
				this[index0] = (T)item;
			}

			item[index1] = value;
		}
	};



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The nested named element of a named element.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property T default[SysStr^, SysStr^]
	{
		virtual T get(SysStr ^ key0, SysStr ^ key1)
		{
			AdvancedReplicant^ item = (AdvancedReplicant^)this[key0];

			if (IsNullPtr(item))
				return nullptr;

			return item[key1];
		}

		virtual void set(SysStr ^ key0, SysStr ^ key1, T value)
		{
			AdvancedReplicant^ item = (AdvancedReplicant^)this[key0];

			if (IsNullPtr(item))
			{
				item = (AdvancedReplicant^)CreateInstance();
				this[key0] = (T)item;
			}

			item[key1] = value;
		}
	};



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The nested named element of an indexed element.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property T default[int, SysStr^]
	{
		virtual T get(int index, SysStr ^ key)
		{
			AdvancedReplicant^ item = (AdvancedReplicant^)this[index];

			if (IsNullPtr(item))
				return nullptr;

			return item[key];
		}

		virtual void set(int index, SysStr^ key, T value)
		{
			AdvancedReplicant^ item = (AdvancedReplicant^)this[index];

			if (IsNullPtr(item))
			{
				item = (AdvancedReplicant^)CreateInstance();
				this[index] = (T)item;
			}

			item[key] = value;
		}
	};



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The nested indexed element of a named element.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property T default[SysStr^, int]
	{
		virtual T get(SysStr ^ key, int index)
		{
			AdvancedReplicant^ item = (AdvancedReplicant^)this[key];

			if (IsNullPtr(item))
				return nullptr;

			return item[index];
		}

		virtual void set(SysStr ^ key, int index, T value)
		{
			AdvancedReplicant^ item = (AdvancedReplicant^)this[key];

			if (IsNullPtr(item))
			{
				item = (AdvancedReplicant^)CreateInstance();
				this[key] = (T)item;
			}

			item[index] = value;
		}
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs an IsNullPtr check on the element for the nested keys if it exist, else 
	/// returns true.
	/// IsNull: Level 1 existence check. Containers don't exist (are all nullptr).
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property bool IsNull[SysStr^, SysStr^]
	{
		virtual bool get(SysStr ^ key0, SysStr^ key1)
		{
			return IsNullPtr(this[key0, key1]);
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
		virtual bool get(SysStr ^ key0, SysStr ^ key1)
		{
			return GetIsNullOrEmpty(this[key0, key1]);
		}
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs an IsNullPtr check on the element for the nested keys, else
	/// returns true.
	/// IsUnpopulated: Level 2 existence check. Element doesn't exist or nullptr.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual property bool IsUnpopulated[SysStr^, SysStr^]
	{
		virtual bool get(SysStr ^ key0, SysStr^ key1)
		{
			return IsNullPtr(this[key0, key1]);
		}
	};




// =========================================================================================================
#pragma region Constructors/Destructors - AdvancedReplicant
// =========================================================================================================

public:

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Default .ctor
	/// </summary>
	// ---------------------------------------------------------------------------------
	AdvancedReplicant() : Replicant()
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
	AdvancedReplicant(int capacity) : Replicant(capacity)
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
	AdvancedReplicant(int capacity, T element) : Replicant(capacity, element)
	{
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// .ctor initialized with an unnamed element collection.
	/// </summary>
	// ---------------------------------------------------------------------------------
	AdvancedReplicant(ICollection<T>^ collection) : Replicant(collection)
	{
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// .ctor initialized with a key and value element.
	/// </summary>
	// ---------------------------------------------------------------------------------
	AdvancedReplicant(SysStr^ key, T element) : Replicant(key, element)
	{
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// .ctor initialized with a named element collection.
	/// </summary>
	// ---------------------------------------------------------------------------------
	AdvancedReplicant(ICollection<KeyValuePair<SysStr^, T>>^ collection) : Replicant(collection)
	{
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// .ctor shallow copy constructor.
	/// </summary>
	// ---------------------------------------------------------------------------------
	AdvancedReplicant(AdvancedReplicant^ collection) : Replicant(collection)
	{
	};




	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Creates an instance of the current type.
	/// </summary>
	// ---------------------------------------------------------------------------------
	virtual SysObj^ CreateInstance() override
	{
		return gcnew AdvancedReplicant();
	}

};


}