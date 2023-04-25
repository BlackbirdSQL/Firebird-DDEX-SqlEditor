#pragma once
#include "pch.h"
#include "IAssembler.h"

using namespace C5;



namespace BlackbirdDsl {




ref class AbstractAssembler abstract : public IAssembler
{

protected:

	StringCell^ _RootNode = nullptr;

public:

	virtual property bool HasAlias
	{
		virtual bool get()
		{
			if (IsNullPtr(_RootNode) || _RootNode->Count == 0)
				return false;

			return _RootNode->ContainsKey("alias");
		}
	}


private:
	AbstractAssembler()
	{
	}


public:



	AbstractAssembler(StringCell^ node)
	{
		_RootNode = node;
	}


	virtual SysStr^ Assemble() abstract;

};


}