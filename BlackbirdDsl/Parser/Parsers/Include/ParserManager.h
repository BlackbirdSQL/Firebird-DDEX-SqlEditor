#pragma once
#include "pch.h"
#include "DslOptions.h"
#include "IParser.h"


using namespace C5;


namespace BlackbirdDsl {


ref class ParserManager abstract sealed
{
protected:



public:

	static StringCell^ Parse(StringCell^ root, FlagsOptions options);


	static IParser^ GetParser(SysStr^ parserType, FlagsOptions options);
};

}