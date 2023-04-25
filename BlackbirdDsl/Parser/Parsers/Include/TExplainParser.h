#pragma once
#include "pch.h"
#include "AbstractParser.h"

using namespace C5;


namespace BlackbirdDsl {



ref class TExplainParser : public AbstractParser
{

public:



	TExplainParser() : AbstractParser()
	{
		_Key = "EXPLAIN";
	};

	TExplainParser(FlagsOptions options) : AbstractParser(options)
	{
		_Key = "EXPLAIN";
	};

protected:

	bool IsStatement(List<SysStr^>^ keys);




public:

	virtual StringCell^ Parse(StringCell^ root) override;

};

}