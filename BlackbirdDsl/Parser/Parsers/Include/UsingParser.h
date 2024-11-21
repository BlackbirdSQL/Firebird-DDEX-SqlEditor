#pragma once
#include "pch.h"
#include "AbstractParser.h"

using namespace C5;


namespace BlackbirdDsl {



ref class UsingParser : public AbstractParser
{


public:



	UsingParser() : AbstractParser()
	{
		_Key = "USING";
	};

	UsingParser(EnParserOptions options) : AbstractParser(options)
	{
		_Key = "USING";
	};

protected:





public:

	virtual StringCell^ Parse(StringCell^ root) override;

};

}