#pragma once
#include "pch.h"
#include "AbstractParser.h"

using namespace C5;


namespace BlackbirdDsl {



ref class TCreateParser : public AbstractParser
{


public:



	TCreateParser() : AbstractParser()
	{
		_Key = "CREATE";
	};

	TCreateParser(DslOptions options) : AbstractParser(options)
	{
		_Key = "CREATE";
	};

protected:





public:

	virtual StringCell^ Parse(StringCell^ root) override;

};

}