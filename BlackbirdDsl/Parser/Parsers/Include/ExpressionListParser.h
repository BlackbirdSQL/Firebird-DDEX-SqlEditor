#pragma once
#include "pch.h"
#include "AbstractParser.h"

using namespace C5;


namespace BlackbirdDsl {



ref class ExpressionListParser : public AbstractParser
{


public:



	ExpressionListParser() : AbstractParser()
	{
	};

	ExpressionListParser(EnParserOptions options) : AbstractParser(options)
	{
	};

protected:





public:

	virtual StringCell^ Parse(StringCell^ root) override;

};

}