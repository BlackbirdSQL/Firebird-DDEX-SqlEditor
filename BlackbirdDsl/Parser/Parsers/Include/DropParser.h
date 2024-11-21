#pragma once
#include "pch.h"
#include "AbstractParser.h"

using namespace C5;


namespace BlackbirdDsl {



ref class DropParser : public AbstractParser
{


public:



	DropParser() : AbstractParser()
	{
		_Key = "DROP";
	};

	DropParser(EnParserOptions options) : AbstractParser(options)
	{
		_Key = "DROP";
	};

protected:





public:

	virtual StringCell^ Parse(StringCell^ root) override;

};

}