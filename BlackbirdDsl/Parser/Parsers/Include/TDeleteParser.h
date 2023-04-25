#pragma once
#include "pch.h"
#include "AbstractParser.h"

using namespace C5;


namespace BlackbirdDsl {



ref class TDeleteParser : public AbstractParser
{


public:



	TDeleteParser() : AbstractParser()
	{
		_Key = "DELETE";
	};

	TDeleteParser(FlagsOptions options) : AbstractParser(options)
	{
		_Key = "DELETE";
	};

protected:





public:

	virtual StringCell^ Parse(StringCell^ root) override;

};

}