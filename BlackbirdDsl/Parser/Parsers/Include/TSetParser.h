#pragma once
#include "pch.h"
#include "AbstractParser.h"

using namespace C5;


namespace BlackbirdDsl {



ref class TSetParser : public AbstractParser
{


public:



	TSetParser() : AbstractParser()
	{
		_Key = "SET";
	};

	TSetParser(FlagsOptions options) : AbstractParser(options)
	{
		_Key = "SET";
	};

protected:





public:

	virtual StringCell^ Parse(StringCell^ root) override;

};

}