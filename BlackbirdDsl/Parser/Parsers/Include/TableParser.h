#pragma once
#include "pch.h"
#include "AbstractParser.h"


using namespace C5;


namespace BlackbirdDsl {



ref class TableParser : public AbstractParser
{
protected:

	StringCell^ GetReservedType(StringCell^ token);

	StringCell^ GetConstantType(StringCell^ token);

	StringCell^ GetOperatorType(StringCell^ token);


	void Clear(StringCell^% expr, SysStr^% base_expr, SysStr^% category);

	StringCell^ MoveLike(StringCell^ root);


public:



	TableParser() : AbstractParser()
	{
		_Key = "TABLE";
	};

	TableParser(EnParserOptions options) : AbstractParser(options)
	{
		_Key = "TABLE";
	};

protected:





public:

	virtual StringCell^ Parse(StringCell^ root) override;

};

}