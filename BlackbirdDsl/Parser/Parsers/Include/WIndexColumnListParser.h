#pragma once
#include "pch.h"
#include "AbstractParser.h"

using namespace C5;


namespace BlackbirdDsl {



ref class WIndexColumnListParser : public AbstractParser
{


public:



	WIndexColumnListParser() : AbstractParser()
	{
	};

	WIndexColumnListParser(FlagsOptions options) : AbstractParser(options)
	{
	};

protected:
	StringCell^ InitExpression();




public:

	virtual StringCell^ Parse(StringCell^ root) override;

};

}