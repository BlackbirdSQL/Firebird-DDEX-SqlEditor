#include "pch.h"
#include "ExplainParser.h"




namespace BlackbirdDsl {



bool ExplainParser::IsStatement(List<SysStr^>^ keys)
{
	int pos = keys->IndexOf(_Key);

	if (pos != -1 && pos + 1 < keys->Count)
	{
		SysStr^ key = keys[pos + 1];

		return (key == "SELECT" || key == "DELETE" || key == "INSERT" || key == "REPLACE" || key == "UPDATE");
	}
	return false;
}



StringCell^ ExplainParser::Parse(StringCell^ root)
{
	StringCell^ parserNode = root[_Key];

	if (parserNode->Count == 0)
	{
		root->Remove(_Key);
		return root;
	}

	List<SysStr^>^ keys = (List<SysStr^>^)root->Keys;


	StringCell^ retnode = NullCell;
	StringCell^ cell;

	SysStr^ baseExpr = "";
	SysStr^ currCategory = "";
	SysStr^ trim;
	SysStr^ upper;

	if (IsStatement(keys))
	{
		for each(StringCell^ token in parserNode->Enumerator)
		{

			trim = token->Trim();
			baseExpr += token;

			if (trim == "")
				continue;

			upper = trim->ToUpper();

			if (upper == "EXTENDED" || upper == "PARTITIONS")
			{
				cell = CellPairs(( CellPair("expr_type", Expressions::RESERVED),
					CellPair("base_expr", token) ));
				return cell;
			}
			else if (upper == "FORMAT")
			{
				if (currCategory == "")
				{
					currCategory = upper;
					cell = CellPairs(( CellPair("expr_type", Expressions::RESERVED),
						CellPair("base_expr", trim) ));
					retnode->Add(cell);
				}
				// else?
			}
			else if (upper == "=")
			{
				if (currCategory == "FORMAT")
				{
					cell = CellPairs(( CellPair("expr_type", Expressions::OPERATOR),
						CellPair("base_expr", trim) ));
					retnode->Add(cell);
				}
				// else?
			}
			else if (upper == "TRADITIONAL" || upper == "JSON")
			{
				if (currCategory == "FORMAT") {
					cell = CellPairs(( CellPair("expr_type", Expressions::RESERVED),
						CellPair("base_expr", trim) ));
					retnode->Add(cell);

					cell = CellPairs(( CellPair("expr_type", Expressions::EXPRESSION),
						CellPair("base_expr", baseExpr->Trim()), CellPair("sub_tree", retnode)));

					root[_Key] = cell;
					return root;
				}
				// else?
			}
			// ignore the other stuff
		}

		if (retnode->Count == 0)
			root->Remove(_Key);
		else
			root[_Key] = retnode;

		return root;
	}

	for each (StringCell ^ token in parserNode->Enumerator)
	{

		trim = token->Trim();

		if (trim == "")
			continue;


		if (currCategory == "TABLENAME")
		{
			currCategory = "WILD";
			cell = CellPairs(( CellPair("expr_type", Expressions::COLREF),
				CellPair("base_expr", trim), CellPair("no_quotes", ExtractQuotesPairs(trim)) ));
			retnode->Add(cell);
		}
		else if (currCategory == "")
		{
			currCategory = "TABLENAME";
			cell = CellPairs(( CellPair("expr_type", Expressions::TABLE),
				CellPair("table", trim), CellPair("no_quotes", ExtractQuotesPairs(trim)),
				CellPair("alias", NullCell), CellPair("base_expr", trim) ));
			retnode->Add(cell);
		}
	}

	if (retnode->Count == 0)
		root->Remove(_Key);
	else
		root[_Key] = retnode;

	return root;
}

}