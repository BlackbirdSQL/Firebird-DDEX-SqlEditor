#include "pch.h"
#include "IndexColumnListParser.h"




namespace BlackbirdDsl {



StringCell^ IndexColumnListParser::InitExpression()
{
	return CellPairs(( CellPair("name", NullCell), CellPair("no_quotes", NullCell),
		CellPair("length", NullCell), CellPair("dir", NullCell)));
}


StringCell^ IndexColumnListParser::Parse(StringCell^ root)
{
	StringCell^ expr = InitExpression();

	StringCell^ retnode = gcnew StringCell();
	StringCell^ cell;
	SysStr^ base_expr = "";
	SysStr^ trim;
	SysStr^ upper;

	for each(StringCell^ token in root->Enumerator)
	{
		trim = token->Trim();
		base_expr += token;

		if (trim == "")
			continue;

		upper = trim->ToUpper();

		if (upper == "ASC" || upper == "DESC")
		{
			// the optional order
			expr["dir"] = trim;
		}
		else if (upper == ",")
		{
			// the next column
			cell = CellPairs(( CellPair("expr_type", Expressions::INDEX_COLUMN), CellPair("base_expr", base_expr) ));
			cell->Merge(expr);

			retnode->Add(cell);

			expr = InitExpression();
			base_expr = "";
		}
		else
		{
			if (upper->StartsWith("(") && upper->EndsWith(")"))
			{
				// the optional length
				expr["length"] = RemoveParenthesis(trim);
				continue;
			}
			// the col name
			expr["name"] = trim;
			expr["no_quotes"] = ExtractQuotesPairs(trim);
		}
	}

	cell = CellPairs(( CellPair("expr_type", Expressions::INDEX_COLUMN), CellPair("base_expr", base_expr) ));
	cell->Merge(expr);

	retnode->Add(cell);

	return retnode;
}

}