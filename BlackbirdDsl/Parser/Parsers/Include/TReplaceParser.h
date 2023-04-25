#pragma once
#include "pch.h"
#include "AbstractParser.h"

using namespace C5;


namespace BlackbirdDsl {



ref class TReplaceParser : public AbstractParser
{


public:



	TReplaceParser() : AbstractParser()
	{
		_Key = "REPLACE";
	};

	TReplaceParser(FlagsOptions options) : AbstractParser(options)
	{
		_Key = "REPLACE";
	};

protected:





public:

	virtual StringCell^ Parse(StringCell^ root) override;

};

}