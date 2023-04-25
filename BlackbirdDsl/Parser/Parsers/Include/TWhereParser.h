#pragma once
#include "pch.h"
#include "AbstractParser.h"

using namespace C5;


namespace BlackbirdDsl {



ref class TWhereParser : public AbstractParser
{


public:



	TWhereParser() : AbstractParser()
	{
		_Key = "WHERE";
	};

	TWhereParser(FlagsOptions options) : AbstractParser(options)
	{
		_Key = "WHERE";
	};

protected:





public:

	virtual StringCell^ Parse(StringCell^ root) override;

};

}