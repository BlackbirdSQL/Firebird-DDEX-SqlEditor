#pragma once
#include <msclr/marshal.h>
#include <malloc.h>

// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)



using namespace System;
using namespace msclr::interop;



namespace BlackbirdSql {
namespace DslParser {
namespace Common {


// =========================================================================================================
//											TMemoryPtr Class
//
/// A lightweight memory pointer class that handles pointers between managed and unmanaged code and performs
/// it's own cleanup.
// =========================================================================================================
template<typename T> ref class TMemoryPtr
{
private:

	#pragma region Variables

	T* _MemBlock = __nullptr;
	size_t _MemSize = 0;


public:
	size_t OverSize = 32;


	#pragma endregion Variables




	// =========================================================================================================
	#pragma region Property Accessors - TMemoryPtr
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// Gets a pinned pointer to the underlying memory block.
	// ---------------------------------------------------------------------------------
	property T* Ptr
	{
		T* get();
	}



	// ---------------------------------------------------------------------------------
	/// Gets the allocated memory size.
	// ---------------------------------------------------------------------------------
	property size_t Size
	{
		size_t get();
	}



	// ---------------------------------------------------------------------------------
	/// Provides pinned indexed access to the underlying memory block.
	/// On get() if the index is out of bounds an exception is thrown.
	/// On set() sufficient memory will be allocated.
	// ---------------------------------------------------------------------------------
	property T default[int]
	{
		T get(int i) { return GetDefault(i); }
		void set(int i, T value) { SetDefault(i, value); }
	}

	// Linker hack - External default get not working too well
	T GetDefault(int i);
	void SetDefault(int i, T value);





	// ---------------------------------------------------------------------------------
	/// Gets or Sets a block of memory given the TMemoryPtr memory block offset and
	/// length.
	// ---------------------------------------------------------------------------------
	property T* default[int, size_t]
	{
		T* get(int offset, size_t len) { return GetDefault(offset, len); }
		void set(int offset, size_t len, T * value) { SetDefault(offset, len, value); }
	}

	// Linker hack -  - External default get/set not working too well
	T* GetDefault(int offset, size_t len);
	void SetDefault(int offset, size_t len, T* value);





	#pragma endregion Property Accessors


	// =========================================================================================================
	#pragma region Constructors / Destructors - TMemoryPtr
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// Default .ctor.
	// ---------------------------------------------------------------------------------
	TMemoryPtr();


	// ---------------------------------------------------------------------------------
	/// Constructor that pre-allocates memory of 'size'.
	// ---------------------------------------------------------------------------------
	TMemoryPtr(size_t size);


	// ---------------------------------------------------------------------------------
	/// Constructor that pre-allocates memory of 'size' and copies in src.
	// ---------------------------------------------------------------------------------
	TMemoryPtr(T* src, size_t len);


	// ---------------------------------------------------------------------------------
	/// Constructor with System::String initialization.
	// ---------------------------------------------------------------------------------
	TMemoryPtr(String^ src);


	// ---------------------------------------------------------------------------------
	/// Destructor.
	// ---------------------------------------------------------------------------------
	~TMemoryPtr();


	#pragma endregion Constructors / Destructors





	// =========================================================================================================
	#pragma region Methods - TMemoryPtr
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// Allocates memory to the underlying memory block. If the underlying block has
	/// been allocated, does a realloc if the block is too small, then clears the block
	// ---------------------------------------------------------------------------------
	void Malloc(size_t size);


	// ---------------------------------------------------------------------------------
	/// Frees the memory block if it exists else does nothing.
	// ---------------------------------------------------------------------------------
	void Free();


	// ---------------------------------------------------------------------------------
	/// Reallocates memory to the underlying memory block if it is smaller than 'size'.
	/// If the underlying block is null performs a malloc.
	// ---------------------------------------------------------------------------------
	void Realloc(size_t size);


	// ---------------------------------------------------------------------------------
	/// Performs a safe memcpy. Performs a Malloc or Realloc if needed.
	// ---------------------------------------------------------------------------------
	void MemCpy(T* src, size_t len);


	// ---------------------------------------------------------------------------------
	/// Performs a memcpy of a String. Performs a Malloc or Realloc if needed.
	// ---------------------------------------------------------------------------------
	void MemCpy(String^ src);


	#pragma endregion Methods

};

}
}
}