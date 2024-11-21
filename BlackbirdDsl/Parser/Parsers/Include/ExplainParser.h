#pragma once
#include "pch.h"
#include "AbstractParser.h"

using namespace C5;


namespace BlackbirdDsl {



ref class ExplainParser : public AbstractParser
{

public:



	ExplainParser() : AbstractParser()
	{
		_Key = "EXPLAIN";
	};

	ExplainParser(EnParserOptions options) : AbstractParser(options)
	{
		_Key = "EXPLAIN";
	};

protected:

	bool IsStatement(List<SysStr^>^ keys);




public:

	virtual StringCell^ Parse(StringCell^ root) override;

};

}