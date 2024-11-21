#pragma once
#include "pch.h"
#include "AbstractParser.h"

using namespace C5;


namespace BlackbirdDsl {



ref class CreateDefinitionParser : public AbstractParser
{


public:



	CreateDefinitionParser() : AbstractParser()
	{
	};

	CreateDefinitionParser(EnParserOptions options) : AbstractParser(options)
	{
	};

protected:





public:

	virtual StringCell^ Parse(StringCell^ root) override;

};

}