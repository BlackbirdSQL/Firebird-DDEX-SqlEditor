#pragma once
#include "pch.h"
#include "AbstractParser.h"

using namespace C5;


namespace BlackbirdDsl {



ref class WSelectExpressionParser : public AbstractParser
{


public:



	WSelectExpressionParser() : AbstractParser()
	{
	};

	WSelectExpressionParser(DslOptions options) : AbstractParser(options)
	{
	};

protected:





public:


	virtual StringCell^ Parse(StringCell^ root) override;

};

}