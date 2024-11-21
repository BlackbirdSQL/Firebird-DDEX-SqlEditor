#pragma once
#include "pch.h"
#include "AbstractParser.h"


using namespace System::Collections::Generic;
using namespace C5;


namespace BlackbirdDsl {



ref class UnionParser : public AbstractParser
{
protected:

	static array<SysStr^>^ UnionTypes = gcnew array<SysStr^> { "UNION", "UNION ALL" };


public:

	UnionParser() : AbstractParser()
	{
	};

	UnionParser(EnParserOptions options) : AbstractParser(options)
	{
	};




protected:

	void ParseMySql(StringCell^ root);

	void SplitRemainder(SysStr^ unionType, StringCell^ root, StringCell^ remainder);


public:

	virtual StringCell^ Parse(StringCell^ root) override;

};

}