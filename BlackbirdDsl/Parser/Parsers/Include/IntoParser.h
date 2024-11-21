#pragma once
#include "pch.h"
#include "AbstractParser.h"

using namespace C5;


namespace BlackbirdDsl {



ref class IntoParser : public AbstractParser
{


public:



	IntoParser() : AbstractParser()
	{
		_Key = "INTO";
	};

	IntoParser(EnParserOptions options) : AbstractParser(options)
	{
		_Key = "INTO";
	};

protected:





public:

	virtual StringCell^ Parse(StringCell^ root) override;

};

}