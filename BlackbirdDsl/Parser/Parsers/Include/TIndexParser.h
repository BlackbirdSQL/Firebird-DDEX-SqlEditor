#pragma once
#include "pch.h"
#include "AbstractParser.h"

using namespace C5;


namespace BlackbirdDsl {



ref class TIndexParser : public AbstractParser
{


public:



	TIndexParser() : AbstractParser()
	{
		_Key = "INDEX";
	};

	TIndexParser(DslOptions options) : AbstractParser(options)
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