#pragma once
#include "pch.h"
#include "AbstractParser.h"

using namespace C5;


namespace BlackbirdDsl {



ref class TIntoParser : public AbstractParser
{


public:



	TIntoParser() : AbstractParser()
	{
		_Key = "INTO";
	};

	TIntoParser(FlagsOptions options) : AbstractParser(options)
	{
		_Key = "INTO";
	};

protected:





public:

	virtual StringCell^ Parse(StringCell^ root) override;

};

}