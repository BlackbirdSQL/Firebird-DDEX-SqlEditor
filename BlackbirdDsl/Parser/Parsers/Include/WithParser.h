#pragma once
#include "pch.h"
#include "AbstractParser.h"

using namespace C5;


namespace BlackbirdDsl {



ref class WithParser : public AbstractParser
{


public:



	WithParser() : AbstractParser()
	{
		_Key = "WITH";
	};

	WithParser(EnParserOptions options) : AbstractParser(options)
	{
		_Key = "WITH";
	};

protected:





public:

	virtual StringCell^ Parse(StringCell^ root) override;

};

}