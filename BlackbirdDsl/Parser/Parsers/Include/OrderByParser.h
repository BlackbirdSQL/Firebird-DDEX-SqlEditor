#pragma once
#include "pch.h"
#include "AbstractParser.h"

using namespace C5;


namespace BlackbirdDsl {



ref class OrderByParser : public AbstractParser
{


public:



	OrderByParser() : AbstractParser()
	{
		_Key = "ORDER";
	};

	OrderByParser(EnParserOptions options) : AbstractParser(options)
	{
		_Key = "ORDER";
	};

protected:





public:

	virtual StringCell^ Parse(StringCell^ root) override;

};

}