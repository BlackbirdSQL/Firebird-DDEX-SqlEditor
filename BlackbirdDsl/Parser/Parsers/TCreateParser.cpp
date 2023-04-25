#include "pch.h"
#include "TCreateParser.h"




namespace BlackbirdDsl {




StringCell^ TCreateParser::Parse(StringCell^ root)
{
	StringCell^ parserNode = root[_Key];

	if (parserNode->Count == 0)
	{
		root->Remove(_Key);
		return root;
	}


	StringCell^ retnode = gcnew StringCell();
	StringCell^ expr = gcnew StringCell();
	StringCell^ cell;

	SysStr^ baseExpr = "";
	SysStr^ trim;
	SysStr^ upper;

	for each(StringCell^ token in parserNode->Enumerator)
	{

		trim = token->Trim();

		baseExpr += token;

		if (trim == "")
			continue;

		upper = trim->ToUpper();


		if (upper == "TEMPORARY")
		{
			// CREATE TEMPORARY TABLE
			retnode["expr_type"] = Expressions::TEMPORARY_TABLE;
			retnode["not-exists"] = NullCell;
			cell = CellPairs(( CellPair("expr_type", Expressions::RESERVED),
				CellPair("base_expr", trim) ));
			expr->Add(cell);
		}
		else if (upper == "TRIGGER")
		{
			// CREATE TRIGGER
			retnode["expr_type"] = Expressions::TRIGGER;
			retnode["not-exists"] = NullCell;
			cell = CellPairs((CellPair("expr_type", Expressions::RESERVED),
				CellPair("base_expr", trim)));
			expr->Add(cell);
		}
		else if (upper == "TABLE")
		{
			// CREATE TABLE
			retnode["expr_type"] = Expressions::TABLE;
			retnode["not-exists"] = NullCell;
			cell = CellPairs(( CellPair("expr_type", Expressions::RESERVED),
				CellPair("base_expr", trim) ));
			expr->Add(cell);
		}
		else if (upper == "INDEX")
		{
			// CREATE INDEX
			retnode["expr_type"] = Expressions::INDEX;
			cell = CellPairs((( CellPair("expr_type", Expressions::RESERVED),
				CellPair("base_expr", trim) )));
			expr->Add(cell);
		}
		else if (upper == "UNIQUE" || upper == "FULLTEXT" || upper == "SPATIAL")
		{
			// options of CREATE INDEX
			retnode["base_expr"] = NullCell;
			retnode["expr_type"] = NullCell;
			retnode["constraint"] = upper;
			cell = CellPairs((( CellPair("expr_type", Expressions::RESERVED),
				CellPair("base_expr", trim) )));
			expr->Add(cell);
		}
		else if (upper == "IF")
		{
			// option of CREATE TABLE
			cell = CellPairs((( CellPair("expr_type", Expressions::RESERVED),
				CellPair("base_expr", trim) )));
			expr->Add(cell);
		}
		else if (upper == "NOT")      
		{
			// option of CREATE TABLE
			cell = CellPairs(( CellPair("expr_type", Expressions::RESERVED),
				CellPair("base_expr", trim) ));
			expr->Add(cell);
		}
		else if (upper == "EXISTS")
		{
			// option of CREATE TABLE
			retnode["not-exists"] = "1";
			cell = CellPairs(( CellPair("expr_type", Expressions::RESERVED),
				CellPair("base_expr", trim) ));
			expr->Add(cell);
		}
	}

	retnode["base_expr"] = baseExpr->Trim();
	retnode["sub_tree"] = expr;


	root[_Key] = retnode;

	return root;
}

}