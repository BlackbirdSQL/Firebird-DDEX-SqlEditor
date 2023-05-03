#pragma once
#include "pch.h"
#include "TExplainParser.h"

using namespace C5;


namespace BlackbirdDsl {



ref class TDescParser : public TExplainParser
{


public:



	TDescParser() : TExplainParser()
	{
		_Key = "DESC";
	};

	TDescParser(DslOptions options) : TExplainParser(options)
	{
		_Key = "DESC";
	};

protected:





public:


};

}