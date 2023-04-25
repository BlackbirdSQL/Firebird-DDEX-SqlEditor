#pragma once
#include "pch.h"
#include "AbstractParser.h"

using namespace C5;


namespace BlackbirdDsl {



ref class TFromParser : public AbstractParser
{


public:



	TFromParser() : AbstractParser()
	{
		_Key = "FROM";
	};

	TFromParser(FlagsOptions options) : AbstractParser(options)
	{
		_Key = "FROM";
	};

protected:

	StringCell^ InitParseInfo(StringCell^ parseInfo);
	StringCell^ ParseFromExpression(StringCell^ parseInfo);



public:

	virtual StringCell^ Parse(StringCell^ root) override;

};

}