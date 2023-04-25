#pragma once
#include "pch.h"
#include "AbstractParser.h"


using namespace C5;


namespace BlackbirdDsl {



ref class TTableParser : public AbstractParser
{
protected:

	StringCell^ GetReservedType(StringCell^ token);

	StringCell^ GetConstantType(StringCell^ token);

	StringCell^ GetOperatorType(StringCell^ token);


	void Clear(StringCell^% expr, SysStr^% base_expr, SysStr^% category);

	StringCell^ MoveLike(StringCell^ root);


public:



	TTableParser() : AbstractParser()
	{
		_Key = "TABLE";
	};

	TTableParser(FlagsOptions options) : AbstractParser(options)
	{
		_Key = "TABLE";
	};

protected:





public:

	virtual StringCell^ Parse(StringCell^ root) override;

};

}