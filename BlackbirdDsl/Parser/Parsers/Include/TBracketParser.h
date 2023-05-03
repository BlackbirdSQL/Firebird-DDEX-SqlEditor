#pragma once
#include "pch.h"
#include "AbstractParser.h"


using namespace C5;


namespace BlackbirdDsl {



ref class TBracketParser : public AbstractParser
{


public:



	TBracketParser() : AbstractParser()
	{
		_Key = "BRACKET";
	};

	TBracketParser(DslOptions options) : AbstractParser(options)
	{
		_Key = "BRACKET";
	};

protected:

	StringCell^ GetRemainingNodes(StringCell^ parserNode);




public:

	virtual StringCell^ Parse(StringCell^ root) override;

};

}