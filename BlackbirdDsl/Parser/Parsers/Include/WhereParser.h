#pragma once
#include "pch.h"
#include "AbstractParser.h"

using namespace C5;


namespace BlackbirdDsl {



ref class WhereParser : public AbstractParser
{


public:



	WhereParser() : AbstractParser()
	{
		_Key = "WHERE";
	};

	WhereParser(EnParserOptions options) : AbstractParser(options)
	{
		_Key = "WHERE";
	};

protected:





public:

	virtual StringCell^ Parse(StringCell^ root) override;

};

}