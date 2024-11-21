#pragma once
#include "pch.h"
#include "AbstractParser.h"

using namespace C5;


namespace BlackbirdDsl {



ref class IndexParser : public AbstractParser
{


public:



	IndexParser() : AbstractParser()
	{
		_Key = "INDEX";
	};

	IndexParser(EnParserOptions options) : AbstractParser(options)
	{
		_Key = "INDEX";
	};

protected:

	StringCell^ GetReservedType(StringCell^ token);
	StringCell^ GetConstantType(StringCell^ token);
	StringCell^ GetOperatorType(StringCell^ token);




public:

	virtual StringCell^ Parse(StringCell^ root) override;

};

}