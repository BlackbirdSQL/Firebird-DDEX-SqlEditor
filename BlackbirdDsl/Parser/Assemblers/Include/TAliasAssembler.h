#pragma once
#include "AbstractAssembler.h"

using namespace C5;



namespace BlackbirdDsl {


ref class TAliasAssembler : public AbstractAssembler
{

public:

	TAliasAssembler(StringCell^ node) : AbstractAssembler(node)
	{
	}


	virtual SysStr^ Assemble() override;


};

}