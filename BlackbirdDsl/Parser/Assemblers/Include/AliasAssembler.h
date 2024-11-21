#pragma once
#include "AbstractAssembler.h"

using namespace C5;



namespace BlackbirdDsl {


ref class AliasAssembler : public AbstractAssembler
{

public:

	AliasAssembler(StringCell^ node) : AbstractAssembler(node)
	{
	}


	virtual SysStr^ Assemble() override;


};

}