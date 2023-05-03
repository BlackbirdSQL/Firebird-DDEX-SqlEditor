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
/// This is the root interface of the Replicant and Cell classes.
/// Exposes IDictionary, IList and IEnumerable and declares the interface members required by the IList and
/// dual function enumerators, ListEnumerator and ReplicaKeyEnumerator.
/// T: The type of the value stored in the array. In descendant Cell classes this would by the descendant
/// Replicant itself, ie. Cell<Cell<T>>.
// =========================================================================================================
template<typename T> public interface class IReplicant : IDictionary<SysStr^, T>, IList<T>,
	IEnumerable<KeyValuePair<ReplicaKey, T>>, System::ICloneable
{
	T ArrayPop();


	IReplicant<T>^ ArraySlice(int offset, int length, bool preserveKeys);

	// ---------------------------------------------------------------------------------
	/// Performs a shallow (non-destructive) copy of a Replicant or Cell into clone.
	/// A shallow copy creates a mirror image of the source Replicant, preserving object
	/// references.
	// ---------------------------------------------------------------------------------
	void Clone(SysObj^ cloneObject);


	// ---------------------------------------------------------------------------------
	/// Creates an instance of the current type.
	// ---------------------------------------------------------------------------------
	Object^ CreateInstance();


	// ---------------------------------------------------------------------------------
	/// The object has named elements or is in a Dictionary state
	// ---------------------------------------------------------------------------------
	property bool IsDictionary { bool get(); }


	// ---------------------------------------------------------------------------------
	/// Returns the key at index. If IsList (!IsDictionary) returns index.ToString()
	// ---------------------------------------------------------------------------------
	property SysStr^ Key[int] { SysStr ^ get(int index); };


	// ---------------------------------------------------------------------------------
	/// The List enumerator object for IList<T>.
	// ---------------------------------------------------------------------------------
	property IList<T>^ Enumerator { IList<T>^ get(); };


	// ---------------------------------------------------------------------------------
	/// The ReplicaKey Ilist/IDictionary combination enumerator object for #define
	/// ReplicaKeyPair(T) = KeyValuePair<ReplicaKey, T>
	// ---------------------------------------------------------------------------------
	property IEnumerable<ReplicaKeyPair(T)>^ ReplicaKeyEnumerator
	{
		IEnumerable<ReplicaKeyPair(T)>^ get();
	};


};

}