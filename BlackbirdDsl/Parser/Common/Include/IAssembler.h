#pragma once
#include "pch.h"
#include "StringCell.h"


using namespace System::Collections::Generic;
using namespace C5;


namespace BlackbirdDsl {


public interface class IAssembler
{

public:

	virtual property bool HasAlias
	{
		virtual bool get();
	};


	/**
	 * Builds a part of an SQL statement.
	 *
	 * @param array $parsed a subtree of the PHPSQLParser output array
	 *
	 * @return A string, which contains a part of an SQL statement.
	 */
	SysStr^ Assemble();


};

}