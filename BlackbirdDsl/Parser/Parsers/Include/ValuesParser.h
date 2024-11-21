#pragma once
#include "pch.h"
#include "AbstractParser.h"

using namespace C5;


namespace BlackbirdDsl {



ref class ValuesParser : public AbstractParser
{


public:



	ValuesParser() : AbstractParser()
	{
		_Key = "VALUES";
	};

	ValuesParser(EnParserOptions options) : AbstractParser(options)
	{
		_Key = "VALUES";
	};

protected:





public:

	virtual StringCell^ Parse(StringCell^ root) override;

};

}