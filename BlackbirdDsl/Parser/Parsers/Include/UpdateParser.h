#pragma once
#include "pch.h"
#include "AbstractParser.h"

using namespace C5;


namespace BlackbirdDsl {



ref class UpdateParser : public AbstractParser
{


public:



	UpdateParser() : AbstractParser()
	{
		_Key = "UPDATE";
	};

	UpdateParser(EnParserOptions options) : AbstractParser(options)
	{
		_Key = "UPDATE";
	};

protected:





public:

	virtual StringCell^ Parse(StringCell^ root) override;

};

}