#pragma once
#include "pch.h"
#include "Cell.h"




// Resolved Tvalue Cell^ types for C#


namespace C5 {


/*
/// <summary>
/// Exposes a <see cref="Cell"/> with a storage type of <see cref="System::Int32"/> to C#.
/// </summary>
public ref class IntegerCell : public Cell<System::Int32^>
{
public:
	/// Default .ctor
	IntegerCell() : Cell() {};
	/// <summary>
	/// .ctor with initial and growth capacity.
	/// </summary>
	/// <param name="capacity">
	/// Sets the initial and growth capacity. If capacity is less than one, the default
	/// _InitialCapacity will be used.
	/// </param>
	IntegerCell(System::Int32 capacity) : Cell(capacity) {};
	/// .ctor with an initial Cell element.
	/// <param name="capacity">
	/// Sets the initial and growth capacity. If capacity is less than one, the default
	/// _InitialCapacity will be used.
	/// </param>
	IntegerCell(System::Int32 capacity, IntegerCell^ element) : Cell(capacity, element) {};
	/// .ctor initialized with an unnamed element collection.
	IntegerCell(ICollection<Cell<System::Int32^>^>^ collection) : Cell(collection) {};
	/// .ctor initialized with a key and Cell^ element.
	IntegerCell(SysStr^ key, IntegerCell^ element) : Cell(key, element) {};
	/// .ctor initialized with a named element collection.
	IntegerCell(ICollection<KeyValuePair<SysStr^, Cell<System::Int32^>^>>^ collection) : Cell(collection) {};
	/// .ctor deep (destructive) copy constructor. To perform a shallow copy use the
	/// assignment (operator=) operator.
	IntegerCell(IntegerCell^ cell) : Cell(cell) {};
	/// .ctor with unary string value initializer.
	IntegerCell(SysStr^ value) : Cell(value) {};
	/// .ctor with unary System::Int32 value initializer.
	IntegerCell(System::Int32^ value) : Cell((SysObj^)value) {};
	/// .ctor with unary System::Double value initializer.
	IntegerCell(System::Double^ value) : Cell((SysObj^)value) {};
	/// .ctor with unary PCXSTR value initializer.
	IntegerCell(PCXSTR value) : Cell(value) {};
	/// .ctor with unary PXSTR value initializer.
	IntegerCell(PCYSTR value) : Cell(value) {};
	/// .ctor with unary SysObj^ value initializer. Reference is preserved.
	IntegerCell(SysObj^ value) : Cell(value) {};
};

/// <summary>
/// Exposes a <see cref="Cell"/> with a storage type of <see cref="System::Double"/> to C#.
/// </summary>
public ref class DoubleCell : public Cell<DoubleCell, System::Double^>
{
public:
	/// Default .ctor
	DoubleCell() : Cell() {};
	/// <summary>
	/// .ctor with initial and growth capacity.
	/// </summary>
	/// <param name="capacity">
	/// Sets the initial and growth capacity. If capacity is less than one, the default
	/// _InitialCapacity will be used.
	/// </param>
	DoubleCell(System::Int32 capacity) : Cell(capacity) {};
	/// .ctor with an initial Cell element.
	/// <param name="capacity">
	/// Sets the initial and growth capacity. If capacity is less than one, the default
	/// _InitialCapacity will be used.
	/// </param>
	DoubleCell(System::Int32 capacity, DoubleCell^ element) : Cell(capacity, element) {};
	/// .ctor initialized with an unnamed element collection.
	DoubleCell(ICollection<DoubleCell^>^ collection) : Cell(collection) {};
	/// .ctor initialized with a key and Cell^ element.
	DoubleCell(SysStr^ key, DoubleCell^ element) : Cell(key, element) {};
	/// .ctor initialized with a named element collection.
	DoubleCell(ICollection<KeyValuePair<SysStr^, DoubleCell^>>^ collection) : Cell(collection) {};
	/// .ctor deep (destructive) copy constructor. To perform a shallow copy use the
	/// assignment (operator=) operator.
	DoubleCell(DoubleCell^ cell) : Cell(cell) {};
	/// .ctor with unary string value initializer.
	DoubleCell(SysStr^ value) : Cell(value) {};
	/// .ctor with unary System::Int32 value initializer.
	DoubleCell(System::Int32^ value) : Cell((SysObj^)value) {};
	/// .ctor with unary System::Double value initializer.
	DoubleCell(System::Double^ value) : Cell((SysObj^)value) {};
	/// .ctor with unary PCXSTR value initializer.
	DoubleCell(PCXSTR value) : Cell(value) {};
	/// .ctor with unary PXSTR value initializer.
	DoubleCell(PCYSTR value) : Cell(value) {};
	/// .ctor with unary SysObj^ value initializer. Reference is preserved.
	DoubleCell(SysObj^ value) : Cell(value) {};
};
*/


}