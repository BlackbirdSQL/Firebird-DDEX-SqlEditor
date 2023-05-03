#pragma once
#include "pch.h"
#include "AbstractParser.h"

using namespace C5;


namespace BlackbirdDsl {



ref class WExpressionListParser : public AbstractParser
{


public:



	WExpressionListParser() : AbstractParser()
	{
	};

	WExpressionListParser(DslOptions options) : AbstractParser(options)
	{
	};

protected:





public:

	virtual StringCell^ Parse(StringCell^ root) override;

};

}