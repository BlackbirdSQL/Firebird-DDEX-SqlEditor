#pragma once
#include "pch.h"
#include "AbstractParser.h"

using namespace C5;


namespace BlackbirdDsl {



ref class SetParser : public AbstractParser
{


public:



	SetParser() : AbstractParser()
	{
		_Key = "SET";
	};

	SetParser(EnParserOptions options) : AbstractParser(options)
	{
		_Key = "SET";
	};

protected:





public:

	virtual StringCell^ Parse(StringCell^ root) override;

};

}