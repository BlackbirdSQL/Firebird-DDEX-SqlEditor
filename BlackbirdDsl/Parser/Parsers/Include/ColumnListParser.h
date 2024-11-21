#pragma once
#include "pch.h"
#include "AbstractParser.h"

using namespace C5;


namespace BlackbirdDsl {



ref class ColumnListParser : public AbstractParser
{


public:



	ColumnListParser() : AbstractParser()
	{
	};

	ColumnListParser(EnParserOptions options) : AbstractParser(options)
	{
	};

protected:





public:
	virtual StringCell^ ColumnListParser::Execute(SysStr^ sql) override;

	virtual StringCell^ Parse(StringCell^ root) override
	{
		System::NotImplementedException^ ex = gcnew System::NotImplementedException();
		Diag::Ex(ex);
		throw ex;
	};

};

}