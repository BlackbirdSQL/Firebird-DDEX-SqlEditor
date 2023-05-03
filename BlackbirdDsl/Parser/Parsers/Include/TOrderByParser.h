#pragma once
#include "pch.h"
#include "AbstractParser.h"

using namespace C5;


namespace BlackbirdDsl {



ref class TOrderByParser : public AbstractParser
{


public:



	TOrderByParser() : AbstractParser()
	{
		_Key = "ORDER";
	};

	TOrderByParser(DslOptions options) : AbstractParser(options)
	{
		_Key = "ORDER";
	};

protected:





public:

	virtual StringCell^ Parse(StringCell^ root) override;

};

}