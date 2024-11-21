#pragma once
#include "pch.h"
#include "AbstractParser.h"

using namespace C5;


namespace BlackbirdDsl {



ref class InsertParser : public AbstractParser
{


public:



	InsertParser() : AbstractParser()
	{
		_Key = "INSERT";
	};

	InsertParser(EnParserOptions options) : AbstractParser(options)
	{
		_Key = "INSERT";
	};

protected:





public:

	virtual StringCell^ Parse(StringCell^ root) override;

};

}