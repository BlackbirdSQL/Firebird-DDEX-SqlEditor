#pragma once
#include "pch.h"
#include "EnParserOptions.h"
#include "IParser.h"


using namespace C5;


namespace BlackbirdDsl {


ref class ParserManager abstract sealed
{
protected:



public:

	static StringCell^ Parse(StringCell^ root, EnParserOptions options);


	static IParser^ GetParser(SysStr^ parserType, EnParserOptions options);
};

}