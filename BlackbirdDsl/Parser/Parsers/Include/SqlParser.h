#pragma once
#include "pch.h"
#include "AbstractParser.h"

using namespace C5;


namespace BlackbirdDsl {



ref class SqlParser : public AbstractParser
{

public:

	SqlParser() : AbstractParser()
	{
	};

	SqlParser(EnParserOptions options) : AbstractParser(options)
	{
	};



	virtual StringCell^ Parse(StringCell^ root) override;

};

}