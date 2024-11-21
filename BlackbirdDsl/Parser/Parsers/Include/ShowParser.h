#pragma once
#include "pch.h"
#include "AbstractParser.h"

using namespace C5;


namespace BlackbirdDsl {



ref class ShowParser : public AbstractParser
{


public:



	ShowParser() : AbstractParser()
	{
		_Key = "SHOW";
	};

	ShowParser(EnParserOptions options) : AbstractParser(options)
	{
		_Key = "SHOW";
	};

protected:





public:

	virtual StringCell^ Parse(StringCell^ root) override;

};

}