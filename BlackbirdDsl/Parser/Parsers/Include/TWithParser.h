#pragma once
#include "pch.h"
#include "AbstractParser.h"

using namespace C5;


namespace BlackbirdDsl {



ref class TWithParser : public AbstractParser
{


public:



	TWithParser() : AbstractParser()
	{
		_Key = "WITH";
	};

	TWithParser(FlagsOptions options) : AbstractParser(options)
	{
		_Key = "WITH";
	};

protected:





public:

	virtual StringCell^ Parse(StringCell^ root) override;

};

}