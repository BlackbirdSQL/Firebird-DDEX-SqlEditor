#pragma once
#include "pch.h"
#include "AbstractParser.h"

using namespace C5;


namespace BlackbirdDsl {



ref class TDuplicateParser : public AbstractParser
{


public:



	TDuplicateParser() : AbstractParser()
	{
		_Key = "DUPLICATE";
	};

	TDuplicateParser(FlagsOptions options) : AbstractParser(options)
	{
		_Key = "DUPLICATE";
	};

protected:





public:

	virtual StringCell^ Parse(StringCell^ root) override;

};

}