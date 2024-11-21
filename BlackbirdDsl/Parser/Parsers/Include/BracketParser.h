#pragma once
#include "pch.h"
#include "AbstractParser.h"


using namespace C5;


namespace BlackbirdDsl {



ref class BracketParser : public AbstractParser
{


public:



	BracketParser() : AbstractParser()
	{
		_Key = "BRACKET";
	};

	BracketParser(EnParserOptions options) : AbstractParser(options)
	{
		_Key = "BRACKET";
	};

protected:

	StringCell^ GetRemainingNodes(StringCell^ parserNode);




public:

	virtual StringCell^ Parse(StringCell^ root) override;

};

}