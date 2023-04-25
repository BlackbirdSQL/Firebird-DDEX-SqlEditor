#pragma once
#include "pch.h"
#include "AbstractParser.h"

using namespace C5;


namespace BlackbirdDsl {



ref class TShowParser : public AbstractParser
{


public:



	TShowParser() : AbstractParser()
	{
		_Key = "SHOW";
	};

	TShowParser(FlagsOptions options) : AbstractParser(options)
	{
		_Key = "SHOW";
	};

protected:





public:

	virtual StringCell^ Parse(StringCell^ root) override;

};

}