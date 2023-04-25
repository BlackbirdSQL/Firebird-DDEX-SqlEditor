#pragma once
#include "pch.h"
#include "AbstractParser.h"

using namespace C5;


namespace BlackbirdDsl {



ref class WPartitionOptionsParser : public AbstractParser
{


public:



	WPartitionOptionsParser() : AbstractParser()
	{
	};

	WPartitionOptionsParser(FlagsOptions options) : AbstractParser(options)
	{
	};

protected:





public:

	virtual StringCell^ Parse(StringCell^ root) override;

};

}