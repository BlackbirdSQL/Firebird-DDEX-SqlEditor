#pragma once
#include <pch.h>
#include "MemoryPtr.h"

// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)



namespace BlackbirdSql {
namespace DslParser {
namespace Common {






// =========================================================================================================
//									MemoryPtr Class Template Definitions
//
// A lightweight memory pointer class that handles pointers between managed and unmanaged code and performs
// it's own cleanup.
// =========================================================================================================




// =========================================================================================================
#pragma region Property Accessors - MemoryPtr
// =========================================================================================================


// ---------------------------------------------------------------------------------
// Gets a pinned pointer to the underlying memory block.
// ---------------------------------------------------------------------------------
template<typename T> T* MemoryPtr<T>::Ptr::get()
{
	if (_MemBlock == __nullptr)
		return __nullptr;

	pin_ptr<T> pin = _MemBlock;
	T* ptr = pin;

	return ptr;
}



// ---------------------------------------------------------------------------------
/// Gets the allocated memory size.
// ---------------------------------------------------------------------------------
template<typename T> size_t MemoryPtr<T>::Size::get()
{
	return _MemSize;
}

// ---------------------------------------------------------------------------------
/// Provides pinned indexed access to the underlying memory block.
/// On get() if the index is out of bounds an exception is thrown.
/// On set() sufficient memory will be allocated.
// Linker hack.
// --------------------------------------
template<typename T> T MemoryPtr<T>::GetDefault(int i)
{
	if (_MemBlock == __nullptr)
	{
		NullReferenceException^ ex = gcnew NullReferenceException(String::Format("Attempt to access a null pointer array at offset {0}.", i));
		throw ex;
	}
	if (i < 0 || i >= _MemSize)
	{
		IndexOutOfRangeException^ rex;
		if (i < 0)
			rex = gcnew IndexOutOfRangeException(String::Format("Array index may not be less than zero: [{0}].", i));
		else
			rex = gcnew IndexOutOfRangeException(String::Format("Array index [{0}] exceeds memory pointer length of {1}.", i, _MemSize));
		throw rex;
	}

	pin_ptr<T> pin = _MemBlock;
	T* ptr = pin;

	return *(ptr + i);
}

template<typename T> void MemoryPtr<T>::SetDefault(int i, T value)
{
	if (i < 0)
	{
		IndexOutOfRangeException^ ex = gcnew IndexOutOfRangeException(String::Format("Attempt to set array index less than zero: [{0}].", i));
		throw ex;
	}

	if (_MemBlock == __nullptr)
		Malloc((size_t)i + OverSize);
	else if (i >= _MemSize)
		Realloc((size_t)i + OverSize);

	_MemBlock[i] = value;
}


// ---------------------------------------------------------------------------------
/// Gets or Sets a block of memory given the MemoryPtr memory block offset and
/// length.
// Linker hack.
// ---------------------------------------------------------------------------------
template<typename T> T* MemoryPtr<T>::GetDefault(int offset, size_t len)
{
	if (_MemBlock == __nullptr)
	{
		NullReferenceException^ ex = gcnew NullReferenceException(String::Format("Attempt to access a null pointer array at offset {0}.", offset));
		throw ex;
	}
	if (offset < 0 || offset + len > _MemSize)
	{
		IndexOutOfRangeException^ rex;
		if (offset < 0)
			rex = gcnew IndexOutOfRangeException(String::Format("Offset may not be less than zero: [{0}].", offset));
		else
			rex = gcnew IndexOutOfRangeException(
				String::Format("Memory block requested at offset {0} and length {1} exceeds memory pointer length of {2}.", offset, len, _MemSize));
		throw rex;
	}

	T* value = (T*)malloc(len + OverSize);
	if (value == (T*)0)
	{
		OutOfMemoryException^ ex = gcnew OutOfMemoryException(String::Format("Could not allocate {0} of memory.", len + OverSize));
		throw ex;
	}

	pin_ptr<T> pin = _MemBlock;
	T* ptr = pin;

	memcpy(value, ptr, len);

	return value;
}

template<typename T> void MemoryPtr<T>::SetDefault(int offset, size_t len, T* value)
{
	if (offset < 0)
	{
		IndexOutOfRangeException^ ex = gcnew IndexOutOfRangeException(String::Format("Attempt to set memory at offset less than zero: [{0}].", offset));
		throw ex;
	}
	if (len < 0)
	{
		ArgumentOutOfRangeException^ rex = gcnew ArgumentOutOfRangeException(String::Format("Attempt to set memory block with length less than zero: [{0}].", len));
		throw rex;
	}

	MemCpy(value, len);
}

#pragma endregion Property Accessors




// =========================================================================================================
#pragma region Constructors / Destructors - MemoryPtr
// =========================================================================================================


// ---------------------------------------------------------------------------------
/// Default .ctor.
// ---------------------------------------------------------------------------------
template<typename T> MemoryPtr<T>::MemoryPtr()
{
}


// ---------------------------------------------------------------------------------
/// Constructor that pre-allocates memory of 'size'.
// ---------------------------------------------------------------------------------
template<typename T> MemoryPtr<T>::MemoryPtr(size_t size)
{
	Malloc(size);
}


// ---------------------------------------------------------------------------------
/// Constructor that pre-allocates memory enough for 'len' and copies in 'len' of src.
// ---------------------------------------------------------------------------------
template<typename T> MemoryPtr<T>::MemoryPtr(T* src, size_t len)
{
	MemCpy(src, len);
}


// ---------------------------------------------------------------------------------
/// Constructor with System::String initialization.
// ---------------------------------------------------------------------------------
template<typename T> MemoryPtr<T>::MemoryPtr(String^ src)
{
	MemCpy(src);
}


// ---------------------------------------------------------------------------------
/// Destructor.
// ---------------------------------------------------------------------------------
template<typename T> MemoryPtr<T>::~MemoryPtr()
{
	Free();
}


#pragma endregion Constructors / Destructors





// =========================================================================================================
#pragma region Methods - MemoryPtr
// =========================================================================================================


// ---------------------------------------------------------------------------------
/// Allocates memory to the underlying memory block. If the underlying block has
/// been allocated, does a realloc if the block is too small, then clears the block
// ---------------------------------------------------------------------------------
template<typename T> void MemoryPtr<T>::Malloc(size_t size)
{
	Realloc(size);
}


// ---------------------------------------------------------------------------------
/// Frees the memory block if it exists else does nothing.
// ---------------------------------------------------------------------------------
template<typename T> void MemoryPtr<T>::Free()
{
	if (_MemBlock != __nullptr)
	{
		free(_MemBlock);
		_MemBlock = __nullptr;
		_MemSize = 0;
	}
}


// ---------------------------------------------------------------------------------
/// Reallocates memory to the underlying memory block if it is smaller than 'size'.
/// If the underlying block is null performs a malloc.
// ---------------------------------------------------------------------------------
template<typename T> void MemoryPtr<T>::Realloc(size_t size)
{
	if (_MemBlock != __nullptr)
	{
		if (_MemSize < size)
		{
			T* memBlock = (T*)realloc(_MemBlock, size);
			if (memBlock == __nullptr)
			{
				Free();
				OutOfMemoryException^ ex = gcnew OutOfMemoryException(String::Format("Could not increase memory allocation from {0} to {1}.", _MemSize, size));
				throw ex;
			}
			_MemBlock = memBlock;
			_MemSize = size;
		}
	}
	else
	{
		_MemBlock = (T*)malloc(size);
		if (_MemBlock == __nullptr)
		{
			OutOfMemoryException^ ex = gcnew OutOfMemoryException(String::Format("Could not allocate {0} of memory.", size));
			throw ex;
		}
		_MemSize = size;
	}
}


// ---------------------------------------------------------------------------------
/// Performs a safe memcpy. Performs a Malloc or Realloc if needed.
// ---------------------------------------------------------------------------------
template<typename T> void MemoryPtr<T>::MemCpy(T* src, size_t len)
{
	if (_MemBlock == __nullptr || _MemSize < len)
	{
		Realloc(len);
	}

	if (_MemBlock == __nullptr)
	{
		OutOfMemoryException^ ex = gcnew OutOfMemoryException(String::Format("Could not allocate {0} of memory.", len));
		throw ex;
	}


	memcpy(_MemBlock, src, len);
}


// ---------------------------------------------------------------------------------
/// Performs a memcpy of a String. Performs a Malloc or Realloc if needed.
// ---------------------------------------------------------------------------------
template<typename T> void MemoryPtr<T>::MemCpy(String^ src)
{
	size_t len = src->Length;

	if (_MemBlock == __nullptr || _MemSize < len)
	{
		Realloc(len);
	}

	// For IntelliSense because Realloc() would have raised exception already
	if (_MemBlock == __nullptr)
		throw gcnew OutOfMemoryException();


	marshal_context^ context = gcnew marshal_context();
	const char* ptrSrc = context->marshal_as<const char*>(src);

	memcpy(_MemBlock, ptrSrc, len);

	delete context;
}


#pragma endregion Methods




}
}
}
