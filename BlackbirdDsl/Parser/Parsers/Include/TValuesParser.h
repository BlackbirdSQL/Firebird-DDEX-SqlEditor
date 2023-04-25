#pragma once
#include "pch.h"
#include "AbstractParser.h"

using namespace C5;


namespace BlackbirdDsl {



ref class TValuesParser : public AbstractParser
{


public:



	TValuesParser() : AbstractParser()
	{
		_Key = "VALUES";
	};

	TValuesParser(FlagsOptions options) : AbstractParser(options)
	{
		_Key = "VALUES";
	};

protected:





public:

	virtual StringCell^ Parse(StringCell^ root) override;

};

}