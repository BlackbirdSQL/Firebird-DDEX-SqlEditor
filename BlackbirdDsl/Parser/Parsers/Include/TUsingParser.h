#pragma once
#include "pch.h"
#include "AbstractParser.h"

using namespace C5;


namespace BlackbirdDsl {



ref class TUsingParser : public AbstractParser
{


public:



	TUsingParser() : AbstractParser()
	{
		_Key = "USING";
	};

	TUsingParser(DslOptions options) : AbstractParser(options)
	{
		_Key = "USING";
	};

protected:





public:

	virtual StringCell^ Parse(StringCell^ root) override;

};

}