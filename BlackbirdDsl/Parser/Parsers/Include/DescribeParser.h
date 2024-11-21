#pragma once
#include "pch.h"
#include "ExplainParser.h"

using namespace C5;


namespace BlackbirdDsl {



ref class DescribeParser : public ExplainParser
{


public:



	DescribeParser() : ExplainParser()
	{
		_Key = "DESCRIBE";
	};

	DescribeParser(EnParserOptions options) : ExplainParser(options)
	{
		_Key = "DESCRIBE";
	};


};

}