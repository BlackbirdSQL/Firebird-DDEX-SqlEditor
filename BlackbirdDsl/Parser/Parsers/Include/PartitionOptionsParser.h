#pragma once
#include "pch.h"
#include "AbstractParser.h"

using namespace C5;


namespace BlackbirdDsl {



ref class PartitionOptionsParser : public AbstractParser
{


public:



	PartitionOptionsParser() : AbstractParser()
	{
	};

	PartitionOptionsParser(EnParserOptions options) : AbstractParser(options)
	{
	};

protected:





public:

	virtual StringCell^ Parse(StringCell^ root) override;

};

}