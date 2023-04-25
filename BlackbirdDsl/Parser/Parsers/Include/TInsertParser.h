#pragma once
#include "pch.h"
#include "AbstractParser.h"

using namespace C5;


namespace BlackbirdDsl {



ref class TInsertParser : public AbstractParser
{


public:



	TInsertParser() : AbstractParser()
	{
		_Key = "INSERT";
	};

	TInsertParser(FlagsOptions options) : AbstractParser(options)
	{
		_Key = "INSERT";
	};

protected:





public:

	virtual StringCell^ Parse(StringCell^ root) override;

};

}