#pragma once
#include "pch.h"
#include "CPentaCommon.h"



namespace C5 {


/// <summary>
/// The key returned on ReplicaKeyEnumerator. Syntax for a Value type of 'Cell&lt;SysStr^&gt;':
/// <c>for each (ReplicaKeyPair(Cell&lt;SysStr^&gt;^) in cell->ReplicaKeyEnumerator)</c>
/// </summary>
/// <typeparam name="T">
/// Always 'void'. ReplicaKey has to be a template so it is
/// always declared as ReplicaKey&lt;void&gt;.
/// </typeparam>
public value struct ReplicaKey
{
public:
	bool Named;
	int Ordinal;
	SysStr^ Segment;

	ReplicaKey(SysStr^ segment)
	{
		Named = true;
		Ordinal = 0;
		Segment = segment;
	};


	ReplicaKey(bool named, int ordinal, SysStr^ segment)
	{
		Named = named;
		Ordinal = ordinal;
		Segment = segment;
	};

	operator SysStr ^ () { return Segment; };
};



}
