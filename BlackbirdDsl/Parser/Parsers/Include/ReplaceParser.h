#pragma once
#include "pch.h"
#include "AbstractParser.h"

using namespace C5;


namespace BlackbirdDsl {



ref class ReplaceParser : public AbstractParser
{


public:



	ReplaceParser() : AbstractParser()
	{
		_Key = "REPLACE";
	};

	ReplaceParser(EnParserOptions options) : AbstractParser(options)
	{
		_Key = "REPLACE";
	};

protected:





public:

	virtual StringCell^ Parse(StringCell^ root) override;

};

}