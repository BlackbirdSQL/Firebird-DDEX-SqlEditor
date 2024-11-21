#pragma once
#include "pch.h"
#include "SelectExpressionParser.h"

using namespace C5;


namespace BlackbirdDsl {



ref class SelectParser : public SelectExpressionParser
{


public:



	SelectParser() : SelectExpressionParser()
	{
		_Key = "SELECT";
	};

	SelectParser(EnParserOptions options) : SelectExpressionParser(options)
	{
		_Key = "SELECT";
	};

protected:





public:

	virtual StringCell^ Parse(StringCell^ root) override;

};

}