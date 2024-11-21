#pragma once
#include "pch.h"
#include "AbstractParser.h"

using namespace C5;


namespace BlackbirdDsl {



ref class RenameParser : public AbstractParser
{


public:



	RenameParser() : AbstractParser()
	{
		_Key = "RENAME";
	};

	RenameParser(EnParserOptions options) : AbstractParser(options)
	{
		_Key = "RENAME";
	};

protected:





public:

	virtual StringCell^ Parse(StringCell^ root) override;

};

}