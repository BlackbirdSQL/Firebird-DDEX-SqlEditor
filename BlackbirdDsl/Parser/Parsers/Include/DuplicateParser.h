#pragma once
#include "pch.h"
#include "AbstractParser.h"

using namespace C5;


namespace BlackbirdDsl {



ref class DuplicateParser : public AbstractParser
{


public:



	DuplicateParser() : AbstractParser()
	{
		_Key = "DUPLICATE";
	};

	DuplicateParser(EnParserOptions options) : AbstractParser(options)
	{
		_Key = "DUPLICATE";
	};

protected:





public:

	virtual StringCell^ Parse(StringCell^ root) override;

};

}