#pragma once
#include "pch.h"
#include "AbstractParser.h"

using namespace C5;


namespace BlackbirdDsl {



ref class TRenameParser : public AbstractParser
{


public:



	TRenameParser() : AbstractParser()
	{
		_Key = "RENAME";
	};

	TRenameParser(DslOptions options) : AbstractParser(options)
	{
		_Key = "RENAME";
	};

protected:





public:

	virtual StringCell^ Parse(StringCell^ root) override;

};

}