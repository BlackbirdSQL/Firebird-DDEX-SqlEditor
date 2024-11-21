#pragma once
#include "pch.h"
#include "AbstractParser.h"

using namespace C5;


namespace BlackbirdDsl {



ref class OptionsParser : public AbstractParser
{


public:



	OptionsParser() : AbstractParser()
	{
		_Key = "OPTIONS";
	};

	OptionsParser(EnParserOptions options) : AbstractParser(options)
	{
		_Key = "OPTIONS";
	};

protected:





public:

	virtual StringCell^ Parse(StringCell^ root) override;

};

}