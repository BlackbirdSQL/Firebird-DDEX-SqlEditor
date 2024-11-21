#pragma once
#include "pch.h"
#include "AbstractParser.h"

using namespace C5;


namespace BlackbirdDsl {



ref class CreateParser : public AbstractParser
{


public:



	CreateParser() : AbstractParser()
	{
		_Key = "CREATE";
	};

	CreateParser(EnParserOptions options) : AbstractParser(options)
	{
		_Key = "CREATE";
	};

protected:





public:

	virtual StringCell^ Parse(StringCell^ root) override;

};

}