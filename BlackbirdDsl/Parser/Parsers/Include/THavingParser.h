#pragma once
#include "pch.h"
#include "AbstractParser.h"

using namespace C5;


namespace BlackbirdDsl {



ref class THavingParser : public AbstractParser
{


public:



	THavingParser() : AbstractParser()
	{
		_Key = "HAVING";
	};

	THavingParser(FlagsOptions options) : AbstractParser(options)
	{
		_Key = "HAVING";
	};

protected:





public:

	virtual StringCell^ Parse(StringCell^ root) override;

};

}