#pragma once
#include "pch.h"
#include "WSelectExpressionParser.h"

using namespace C5;


namespace BlackbirdDsl {



ref class TSelectParser : public WSelectExpressionParser
{


public:



	TSelectParser() : WSelectExpressionParser()
	{
		_Key = "SELECT";
	};

	TSelectParser(DslOptions options) : WSelectExpressionParser(options)
	{
		_Key = "SELECT";
	};

protected:





public:

	virtual StringCell^ Parse(StringCell^ root) override;

};

}