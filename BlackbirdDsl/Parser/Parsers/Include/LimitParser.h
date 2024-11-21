#pragma once
#include "pch.h"
#include "AbstractParser.h"

using namespace C5;


namespace BlackbirdDsl {



ref class LimitParser : public AbstractParser
{


public:



	LimitParser() : AbstractParser()
	{
		_Key = "LIMIT";
	};

	LimitParser(EnParserOptions options) : AbstractParser(options)
	{
		_Key = "LIMIT";
	};

protected:





public:

	virtual StringCell^ Parse(StringCell^ root) override;

};

}