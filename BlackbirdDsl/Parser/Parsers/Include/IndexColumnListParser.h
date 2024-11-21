#pragma once
#include "pch.h"
#include "AbstractParser.h"

using namespace C5;


namespace BlackbirdDsl {



ref class IndexColumnListParser : public AbstractParser
{


public:



	IndexColumnListParser() : AbstractParser()
	{
	};

	IndexColumnListParser(EnParserOptions options) : AbstractParser(options)
	{
	};

protected:
	StringCell^ InitExpression();




public:

	virtual StringCell^ Parse(StringCell^ root) override;

};

}