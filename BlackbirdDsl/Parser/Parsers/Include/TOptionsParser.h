#pragma once
#include "pch.h"
#include "AbstractParser.h"

using namespace C5;


namespace BlackbirdDsl {



ref class TOptionsParser : public AbstractParser
{


public:



	TOptionsParser() : AbstractParser()
	{
		_Key = "OPTIONS";
	};

	TOptionsParser(DslOptions options) : AbstractParser(options)
	{
		_Key = "OPTIONS";
	};

protected:





public:

	virtual StringCell^ Parse(StringCell^ root) override;

};

}