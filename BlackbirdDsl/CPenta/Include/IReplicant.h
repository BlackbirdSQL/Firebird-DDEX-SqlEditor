#pragma once
#include "pch.h"
#include "CPentaCommon.h"
#include "Diag.h"
#include "ReplicaKey.h"

// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)



using namespace System::Collections::Generic;




namespace C5 {


static bool IsNullPtr(SysObj^ obj)
{
	return (obj == nullptr);
};




// =========================================================================================================
//										IReplicant Interface
//
/// <summary>
/// This is the root interface of the Replicant and Cell classes.
/// Exposes IDictionary, IList and IEnumerable and declares the interface members required by the IList and
/// dual function enumerators, ListEnumerator and ReplicaKeyEnumerator.
/// T: The type of the value stored in the array. In descendant Cell classes this would by the descendant
/// Replicant itself, ie. Cell<Cell<T>>.
/// </summary>
// =========================================================================================================
template<typename T> public interface class IReplicant : IDictionary<SysStr^, T>, IList<T>,
	IEnumerable<KeyValuePair<ReplicaKey, T>>, System::ICloneable
{
	T ArrayPop();


	IReplicant<T>^ ArraySlice(int offset, int length, bool preserveKeys);

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs a shallow (non-destructive) copy of a Replicant or Cell into clone.
	/// A shallow copy creates a mirror image of the source Replicant, preserving object
	/// references.
	/// </summary>
	// ---------------------------------------------------------------------------------
	void Clone(SysObj^ cloneObject);


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Creates an instance of the current type.
	/// </summary>
	// ---------------------------------------------------------------------------------
	Object^ CreateInstance();


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The object has named elements or is in a Dictionary state
	/// </summary>
	// ---------------------------------------------------------------------------------
	property bool IsDictionary { bool get(); }


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns the key at index. If IsList (!IsDictionary) returns index.ToString()
	/// </summary>
	// ---------------------------------------------------------------------------------
	property SysStr^ Key[int] { SysStr ^ get(int index); };


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The List enumerator object for IList<T>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	property IList<T>^ Enumerator { IList<T>^ get(); };


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The ReplicaKey Ilist/IDictionary combination enumerator object for #define
	/// ReplicaKeyPair(T) = KeyValuePair<ReplicaKey, T>
	/// </summary>
	// ---------------------------------------------------------------------------------
	property IEnumerable<ReplicaKeyPair(T)>^ ReplicaKeyEnumerator
	{
		IEnumerable<ReplicaKeyPair(T)>^ get();
	};


};

}