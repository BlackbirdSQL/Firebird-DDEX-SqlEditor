#pragma once
#include "pch.h"
#include "AbstractParser.h"

using namespace C5;


namespace BlackbirdDsl {



ref class SelectExpressionParser : public AbstractParser
{


public:



	SelectExpressionParser() : AbstractParser()
	{
	};

	SelectExpressionParser(EnParserOptions options) : AbstractParser(options)
	{
	};

protected:





public:


	virtual StringCell^ Parse(StringCell^ root) override;

};

}