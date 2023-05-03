#pragma once
#include "pch.h"
#include "AbstractParser.h"

using namespace C5;


namespace BlackbirdDsl {



ref class TUpdateParser : public AbstractParser
{


public:



	TUpdateParser() : AbstractParser()
	{
		_Key = "UPDATE";
	};

	TUpdateParser(DslOptions options) : AbstractParser(options)
	{
		_Key = "UPDATE";
	};

protected:





public:

	virtual StringCell^ Parse(StringCell^ root) override;

};

}