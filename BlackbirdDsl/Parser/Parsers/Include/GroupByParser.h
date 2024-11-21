#pragma once
#include "pch.h"
#include "AbstractParser.h"

using namespace C5;


namespace BlackbirdDsl {



ref class GroupByParser : public AbstractParser
{


public:



	GroupByParser() : AbstractParser()
	{
		_Key = "GROUP";
	};

	GroupByParser(EnParserOptions options) : AbstractParser(options)
	{
		_Key = "GROUP";
	};

protected:





public:

	virtual StringCell^ Parse(StringCell^ root) override;

};

}