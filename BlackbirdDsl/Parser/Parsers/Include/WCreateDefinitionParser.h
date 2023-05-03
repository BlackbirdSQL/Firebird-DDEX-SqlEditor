#pragma once
#include "pch.h"
#include "AbstractParser.h"

using namespace C5;


namespace BlackbirdDsl {



ref class WCreateDefinitionParser : public AbstractParser
{


public:



	WCreateDefinitionParser() : AbstractParser()
	{
	};

	WCreateDefinitionParser(DslOptions options) : AbstractParser(options)
	{
	};

protected:





public:

	virtual StringCell^ Parse(StringCell^ root) override;

};

}