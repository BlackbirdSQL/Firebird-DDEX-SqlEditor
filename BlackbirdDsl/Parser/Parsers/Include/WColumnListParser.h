#pragma once
#include "pch.h"
#include "AbstractParser.h"

using namespace C5;


namespace BlackbirdDsl {



ref class WColumnListParser : public AbstractParser
{


public:



	WColumnListParser() : AbstractParser()
	{
	};

	WColumnListParser(FlagsOptions options) : AbstractParser(options)
	{
	};

protected:





public:
	virtual StringCell^ WColumnListParser::Execute(SysStr^ sql) override;

	virtual StringCell^ Parse(StringCell^ root) override
	{
		System::NotImplementedException^ ex = gcnew System::NotImplementedException();
		Diag::Dug(ex);
		throw ex;
	};

};

}