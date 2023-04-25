#include "pch.h"
#include "TIndexParser.h"

#include "WIndexColumnListParser.h"




namespace BlackbirdDsl {


StringCell^ TIndexParser::GetReservedType(StringCell^ token)
{
	return CellPairs(( CellPair("expr_type", Expressions::RESERVED), CellPair("base_expr", token) ));
}

StringCell^ TIndexParser::GetConstantType(StringCell^ token)
{
	return CellPairs(( CellPair("expr_type", Expressions::CONSTANT), CellPair("base_expr", token) ));
}

StringCell^ TIndexParser::GetOperatorType(StringCell^ token)
{
	return CellPairs(( CellPair("expr_type", Expressions::OPERATOR), CellPair("base_expr", token) ));
}


StringCell^ TIndexParser::Parse(StringCell^ root)
{
	StringCell^ parserNode = root[_Key];

	if (parserNode->Count == 0)
	{
		root->Remove(_Key);
		return root;
	}

	SysStr^ currCategory = "INDEX_NAME";
	SysStr^ prevCategory;

	StringCell^ retnode = CellPairs(( CellPair("base_expr", NullCell), CellPair("name", NullCell),
		CellPair("no_quotes", NullCell), CellPair("index-type", NullCell), CellPair("on", NullCell),
		CellPair("options", NullCell) ));

	StringCell^ expr = gcnew StringCell();
	StringCell^ cols;
	StringCell^ cell;

	SysStr^ base_expr = "";
	SysStr^ trim;
	SysStr^ upper;

	int skip = 0;

	for each (StringCell ^ token in parserNode->Enumerator)
	{
		trim = token->Trim();
		base_expr += token;

		if (skip > 0)
		{
			skip--;
			continue;
		}
		if (skip < 0)
			break;
		if (trim == "")
			continue;

		upper = trim->ToUpper();

		if (upper == "USING")
		{
			if (prevCategory == "CREATE_DEF")
			{
				expr->Add(GetReservedType(trim));
				currCategory = "TYPE_OPTION";
				continue;
			}
			if (prevCategory == "TYPE_DEF")
			{
				expr->Add(GetReservedType(trim));
				currCategory = "INDEX_TYPE";
				continue;
			}
			// else ?
		}
		else if (upper == "KEY_BLOCK_SIZE")
		{
			if (prevCategory == "CREATE_DEF")
			{
				expr->Add(GetReservedType(trim));
				currCategory = "INDEX_OPTION";
				continue;
			}
			// else ?
		}
		else if (upper == "WITH")
		{
			if (prevCategory == "CREATE_DEF")
			{
				expr->Add(GetReservedType(trim));
				currCategory = "INDEX_PARSER";
				continue;
			}
			// else ?
		}
		else if (upper == "PARSER")
		{
			if (currCategory == "INDEX_PARSER")
			{
				expr->Add(GetReservedType(trim));
				continue;
			}
			// else ?
		}
		else if (upper == "COMMENT")
		{
			if (prevCategory == "CREATE_DEF")
			{
				expr->Add(GetReservedType(trim));
				currCategory = "INDEX_COMMENT";
				continue;
			}
			// else ?
		}
		else if (upper == "ALGORITHM" || upper == "LOCK")
		{
			if (prevCategory == "CREATE_DEF")
			{
				expr->Add(GetReservedType(trim));
				currCategory = upper + "_OPTION";
				continue;
			}
			// else ?
		}
		else if (upper == "=")
		{
			// the optional operator
			if (currCategory->EndsWith("_OPTION"))
			{
				expr->Add(GetOperatorType(trim));
				continue; // don"t change the category
			}
			// else ?
		}
		else if (upper == "ON")
		{
			if (prevCategory == "CREATE_DEF" || prevCategory == "TYPE_DEF")
			{
				expr->Add(GetReservedType(trim));
				currCategory = "TABLE_DEF";
				continue;
			}
			// else ?
		}
		else
		{
			if (currCategory == "COLUMN_DEF")
			{
				if (upper->StartsWith("(") && upper->EndsWith(")"))
				{
					retnode["on", "base_expr"] += base_expr;

					cols = (gcnew WIndexColumnListParser(_Options))->Execute(RemoveParenthesis(trim));
					cell = CellPairs(( CellPair("expr_type", Expressions::COLUMN_LIST),
						CellPair("base_expr", trim), CellPair("sub_tree", cols) ));
					retnode["on", "sub_tree"] = cell;
				}

				expr = gcnew StringCell();
				base_expr = "";
				currCategory = "CREATE_DEF";
			}
			else if (currCategory == "TABLE_DEF")
			{
				// the table name
				expr->Add(GetConstantType(trim));
				// TODO: the base_expr should contain the column-def too
				cell = CellPairs(( CellPair("expr_type", Expressions::TABLE),
					CellPair("base_expr", base_expr), CellPair("name", trim),
					CellPair("no_quotes", ExtractQuotesPairs(trim)), CellPair("sub_tree", NullCell) ));
				retnode["on"] = cell;

				expr = gcnew StringCell();
				base_expr = "";
				currCategory = "COLUMN_DEF";
				continue;
			}
			else if (currCategory == "INDEX_NAME")
			{
				retnode["name"] = trim;
				retnode["base_expr"] = trim;
				retnode["no_quotes"] = ExtractQuotesPairs(trim);

				expr = gcnew StringCell();
				base_expr = "";
				currCategory = "TYPE_DEF";
			}
			else if (currCategory == "INDEX_PARSER")
			{
				// the parser name
				expr->Add(GetConstantType(trim));
				cell = CellPairs(( CellPair("expr_type", Expressions::INDEX_PARSER),
					CellPair("base_expr", base_expr->Trim()), CellPair("sub_tree", expr) ));
				retnode["options"]->Add(cell);
				expr = gcnew StringCell();
				base_expr = "";
				currCategory = "CREATE_DEF";
			}
			else if (currCategory == "INDEX_COMMENT")
			{
				// the index comment
				expr->Add(GetConstantType(trim));
				cell = CellPairs(( CellPair("expr_type" , Expressions::COMMENT),
					CellPair("base_expr", base_expr->Trim()), CellPair("sub_tree", expr) ));
				retnode["options"]->Add(cell);
				expr = gcnew StringCell();
				base_expr = "";
				currCategory = "CREATE_DEF";
			}
			else if (currCategory == "INDEX_OPTION")
			{
				// the key_block_size
				expr->Add(GetConstantType(trim));
				cell = CellPairs(( CellPair("expr_type", Expressions::INDEX_SIZE),
					CellPair("base_expr", base_expr->Trim()), CellPair("size", upper),
					CellPair("sub_tree", expr) ));
				retnode["options"]->Add(cell);
				expr = gcnew StringCell();
				base_expr = "";
				currCategory = "CREATE_DEF";
			}
			else if (currCategory == "INDEX_TYPE" || currCategory == "TYPE_OPTION")
			{
				// BTREE or HASH
				expr->Add(GetReservedType(trim));
				if (currCategory == "INDEX_TYPE")
				{
					cell = CellPairs(( CellPair("expr_type", Expressions::INDEX_TYPE),
						CellPair("base_expr", base_expr->Trim()), CellPair("using", upper),
						CellPair("sub_tree", expr) ));
					retnode["index-type"] = cell;
				}
				else
				{
					cell = CellPairs(( CellPair("expr_type", Expressions::INDEX_TYPE),
						CellPair("base_expr", base_expr->Trim()), CellPair("using", upper),
						CellPair("sub_tree", expr) ));
					retnode["options"]->Add(cell);
				}

				expr = gcnew StringCell();
				base_expr = "";
				currCategory = "CREATE_DEF";
			}
			else if (currCategory == "LOCK_OPTION")
			{
				// DEFAULT|NONE|SHARED|EXCLUSIVE
				expr->Add(GetReservedType(trim));
				cell = CellPairs(( CellPair("expr_type", Expressions::INDEX_LOCK),
					CellPair("base_expr", base_expr->Trim()), CellPair("lock", upper),
					CellPair("sub_tree", expr) ));
				retnode["options"]->Add(cell);

				expr = gcnew StringCell();
				base_expr = "";
				currCategory = "CREATE_DEF";
			}
			else if (currCategory == "ALGORITHM_OPTION")
			{
				// DEFAULT|INPLACE|COPY
				expr->Add(GetReservedType(trim));
				cell = CellPairs(( CellPair("expr_type", Expressions::INDEX_ALGORITHM),
					CellPair("base_expr", base_expr->Trim()), CellPair("algorithm", upper),
					CellPair("sub_tree", expr) ));
				retnode["options"]->Add(cell);

				expr = gcnew StringCell();
				base_expr = "";
				currCategory = "CREATE_DEF";
			}
		}

		prevCategory = currCategory;
		currCategory = "";
	}

	if (retnode["options"]->Count == 0)
	{
		retnode["options"] = NullCell;
	}


	root[_Key] = retnode;

	return root;
}

}