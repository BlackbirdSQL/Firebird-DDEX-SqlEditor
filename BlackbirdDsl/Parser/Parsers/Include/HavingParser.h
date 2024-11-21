#pragma once
#include "pch.h"
#include "AbstractParser.h"

using namespace C5;


namespace BlackbirdDsl {



ref class HavingParser : public AbstractParser
{


public:



	HavingParser() : AbstractParser()
	{
		_Key = "HAVING";
	};

	HavingParser(EnParserOptions options) : AbstractParser(options)
	{
		_Key = "HAVING";
	};

protected:





public:

	virtual StringCell^ Parse(StringCell^ root) override;

};

}