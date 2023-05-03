#pragma once
#include "pch.h"
#include "AbstractParser.h"

using namespace C5;


namespace BlackbirdDsl {



ref class TGroupByParser : public AbstractParser
{


public:



	TGroupByParser() : AbstractParser()
	{
		_Key = "GROUP";
	};

	TGroupByParser(DslOptions options) : AbstractParser(options)
	{
		_Key = "GROUP";
	};

protected:





public:

	virtual StringCell^ Parse(StringCell^ root) override;

};

}