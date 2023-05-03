#pragma once
#include "pch.h"
#include "AbstractParser.h"

using namespace C5;


namespace BlackbirdDsl {



ref class TLimitParser : public AbstractParser
{


public:



	TLimitParser() : AbstractParser()
	{
		_Key = "LIMIT";
	};

	TLimitParser(DslOptions options) : AbstractParser(options)
	{
		_Key = "LIMIT";
	};

protected:





public:

	virtual StringCell^ Parse(StringCell^ root) override;

};

}