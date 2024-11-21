#pragma once
#include "pch.h"
#include "AbstractParser.h"

using namespace C5;


namespace BlackbirdDsl {



ref class DeleteParser : public AbstractParser
{


public:



	DeleteParser() : AbstractParser()
	{
		_Key = "DELETE";
	};

	DeleteParser(EnParserOptions options) : AbstractParser(options)
	{
		_Key = "DELETE";
	};

protected:





public:

	virtual StringCell^ Parse(StringCell^ root) override;

};

}