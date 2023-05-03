#pragma once
#include "pch.h"
#include "TExplainParser.h"

using namespace C5;


namespace BlackbirdDsl {



ref class TDescribeParser : public TExplainParser
{


public:



	TDescribeParser() : TExplainParser()
	{
		_Key = "DESCRIBE";
	};

	TDescribeParser(DslOptions options) : TExplainParser(options)
	{
		_Key = "DESCRIBE";
	};


};

}