#pragma once
#include "pch.h"
#include "ExplainParser.h"

using namespace C5;


namespace BlackbirdDsl {



ref class DescParser : public ExplainParser
{


public:



	DescParser() : ExplainParser()
	{
		_Key = "DESC";
	};

	DescParser(EnParserOptions options) : ExplainParser(options)
	{
		_Key = "DESC";
	};

protected:





public:


};

}