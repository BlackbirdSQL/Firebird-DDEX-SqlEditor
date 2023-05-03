#pragma once
#include "pch.h"
#include "AbstractParser.h"

using namespace C5;


namespace BlackbirdDsl {



ref class TDropParser : public AbstractParser
{


public:



	TDropParser() : AbstractParser()
	{
		_Key = "DROP";
	};

	TDropParser(DslOptions options) : AbstractParser(options)
	{
		_Key = "DROP";
	};

protected:





public:

	virtual StringCell^ Parse(StringCell^ root) override;

};

}