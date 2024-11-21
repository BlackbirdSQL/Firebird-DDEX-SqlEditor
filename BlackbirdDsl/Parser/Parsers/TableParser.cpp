#include "pch.h"
#include "TableParser.h"

#include "PartitionOptionsParser.h"
#include "CreateDefinitionParser.h"
#include "Scanner.h"




namespace BlackbirdDsl {



StringCell^ TableParser::GetReservedType(StringCell^ token)
{
	StringCell^ cell = CellPairs(( CellPair("expr_type", Expressions::RESERVED),
		CellPair("base_expr", token) ));

	return cell;
}



StringCell^ TableParser::GetConstantType(StringCell^ token)
{
	StringCell^ cell = CellPairs(( CellPair("expr_type", Expressions::CONSTANT),
		CellPair("base_expr", token) ));

	return cell;
}



StringCell^ TableParser::GetOperatorType(StringCell^ token)
{
	StringCell^ cell = CellPairs(( CellPair("expr_type", Expressions::OPERATOR),
		CellPair("base_expr", token) ));

	return cell;
}


void TableParser::Clear(StringCell^% expr, SysStr^% base_expr, SysStr^% category)
{
	expr = gcnew StringCell();
	base_expr = gcnew SysStr("");
	category = gcnew SysStr("");
}



StringCell^ TableParser::MoveLike(StringCell^ root)
{

	if (root->IsUnpopulated["TABLE"])
		return root;

	StringCell^ targetNode = root["TABLE"];

	if (targetNode->IsUnpopulated["like"])
		return root;

	root->InsertAfter("TABLE", "LIKE", targetNode["like"]);
	targetNode->Remove("like");

	return root;
}




StringCell^ TableParser::Parse(StringCell^ root)
{
	StringCell^ parserNode = root[_Key];

	if (parserNode->Count == 0)
	{
		root->Remove(_Key);
		return root;
	}


	SysStr^ currCategory = "TABLE_NAME";

	StringCell^ retnode = CellPairs(( CellPair("base_expr", ""),
		CellPair("name", ""), CellPair("no_quotes", ""), CellPair("create - def", ""),
		CellPair("options", gcnew StringCell()), CellPair("like", ""), CellPair("select-option", "") ));

	StringCell^ expr = gcnew StringCell();
	StringCell^ last;
	StringCell^ cell;
	StringCell^ coldef;
	StringCell^ unparsed;

	SysStr^ base_expr = "";
	SysStr^ trim;
	SysStr^ upper;
	// Note: Initialized due to warning.
	SysStr^ prevCategory = NullCell;

	int skip = 0;

	for each (ReplicaKeyPair(StringCell^) pair in parserNode->ReplicaKeyEnumerator)
	{
		trim = pair.Value->Trim();
		base_expr += pair.Value;

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

		if (upper == ",")
		{
			// it is possible to separate the table options with comma!
			if (prevCategory == "CREATE_DEF")
			{
				last = retnode["options"]->ArrayPop();
				last["delim"] = ",";
				retnode["options"]->Add(last);
				base_expr = "";
			}
			continue;

		}
		else if (upper == "UNION")
		{
			if (prevCategory == "CREATE_DEF")
			{
				expr->Add(GetReservedType(trim));
				currCategory = "UNION";
				continue;
			}
		}
		else if (upper == "LIKE")
		{
			// like without parenthesis
			if (prevCategory == "TABLE_NAME")
			{
				currCategory = upper;
				continue;
			}
		}
		else if (upper == "=")
		{
			// the optional operator
			if (prevCategory == "TABLE_OPTION")
			{
				expr->Add(GetOperatorType(trim));
				continue; // don"t change the category
			}
		}
		else if (upper == "CHARACTER")
		{
			if (prevCategory == "CREATE_DEF")
			{
				expr->Add(GetReservedType(trim));
				currCategory = "TABLE_OPTION";
			}
			if (prevCategory == "TABLE_OPTION")
			{
				// add it to the previous DEFAULT
				expr->Add(GetReservedType(trim));
				continue;
			}
		}
		else if (upper == "SET" || upper == "CHARSET")
		{
			if (prevCategory == "TABLE_OPTION")
			{
				// add it to a previous CHARACTER
				expr->Add(GetReservedType(trim));
				currCategory = "CHARSET";
				continue;
			}
		}
		else if (upper == "COLLATE")
		{
			if (prevCategory == "TABLE_OPTION" || prevCategory == "CREATE_DEF")
			{
				// add it to the previous DEFAULT
				expr->Add(GetReservedType(trim));
				currCategory = "COLLATE";
				continue;
			}
		}
		else if (upper == "DIRECTORY")
		{
			if (currCategory == "INDEX_DIRECTORY" || currCategory == "DATA_DIRECTORY")
			{
				// after INDEX or DATA
				expr->Add(GetReservedType(trim));
				continue;
			}
		}
		else if (upper == "INDEX")
		{
			if (prevCategory == "CREATE_DEF")
			{
				expr->Add(GetReservedType(trim));
				currCategory = "INDEX_DIRECTORY";
				continue;
			}
		}
		else if (upper == "DATA")
		{
			if (prevCategory == "CREATE_DEF")
			{
				expr->Add(GetReservedType(trim));
				currCategory = "DATA_DIRECTORY";
				continue;
			}
		}
		else if (upper == "INSERT_METHOD" || upper == "DELAY_KEY_WRITE" || upper == "ROW_FORMAT"
			|| upper == "PASSWORD" || upper == "MAX_ROWS" || upper == "MIN_ROWS" 
			|| upper == "PACK_KEYS" || upper == "CHECKSUM" || upper == "COMMENT"
			|| upper == "CONNECTION" || upper == "AUTO_INCREMENT" || upper == "AVG_ROW_LENGTH"
			|| upper == "ENGINE" || upper == "TYPE" || upper == "STATS_AUTO_RECALC"
			|| upper == "STATS_PERSISTENT" || upper == "KEY_BLOCK_SIZE")
		{
			if (prevCategory == "CREATE_DEF")
			{
				expr->Add(GetReservedType(trim));
				currCategory = prevCategory = "TABLE_OPTION";
				continue;
			}
		}
		else if (upper == "DYNAMIC" || upper == "FIXED" || upper == "COMPRESSED"
			|| upper == "REDUNDANT" || upper == "COMPACT" || upper == "NO"
			|| upper == "FIRST" || upper == "LAST" || upper == "DEFAULT")
		{
			if (prevCategory == "CREATE_DEF")
			{
				// DEFAULT before CHARACTER SET and COLLATE
				expr->Add(GetReservedType(trim));
				currCategory = "TABLE_OPTION";
			}
			else if (prevCategory == "TABLE_OPTION")
			{
				// all assignments with the keywords
				expr->Add(GetReservedType(trim));
				cell = CellPairs(( CellPair("expr_type", Expressions::EXPRESSION),
					CellPair("base_expr", base_expr->Trim()), CellPair("delim", " "),
					CellPair("sub_tree", expr) ));
				retnode["options"]->Add(cell);
				Clear(expr, base_expr, currCategory);
			}
		}
		else if (upper == "IGNORE" || upper == "REPLACE")
		{
			expr->Add(GetReservedType(trim));
			cell = CellPairs(( CellPair("base_expr", base_expr->Trim()),
				CellPair("duplicates", trim), CellPair("as", false), CellPair("sub_tree", expr) ));
			retnode["select-option"] = cell;
			continue;

		}
		else if (upper == "AS")
		{
			expr->Add(GetReservedType(trim));
			if (retnode->IsNull["select-option", "duplicates"])
			{
				retnode["select-option", "duplicates"] = "";
			}
			retnode["select-option", "as"] = true;
			retnode["select-option", "base_expr"] = base_expr->Trim();
			retnode["select-option", "sub_tree"] = expr;
			continue;

		}
		else if (upper == "PARTITION")
		{
			if (prevCategory == "CREATE_DEF")
			{
				cell = parserNode->ArraySlice(pair.Key.Ordinal - 1, -1, true);
				cell = ParserInstance(PartitionOptionsParser)->Parse(cell);

				skip = cell["last-parsed"] - pair.Key.Ordinal;
				retnode["partition-options"] = cell["partition-options"];
				continue;
			}
		}
		else
		{
			if (currCategory == "CHARSET")
			{
				// the charset name
				expr->Add(GetConstantType(trim));
				cell = CellPairs(( CellPair("expr_type", Expressions::CHARSET),
					CellPair("base_expr", base_expr->Trim()), CellPair("delim", " "),
					CellPair("sub_tree", expr) ));
				retnode["options"]->Add(cell);
				Clear(expr, base_expr, currCategory);

			}
			else if (currCategory == "COLLATE")
			{
				// the collate name
				expr->Add(GetConstantType(trim));
				cell = CellPairs(( CellPair("expr_type", Expressions::COLLATE),
					CellPair("base_expr", base_expr->Trim()), CellPair("delim", " "),
					CellPair("sub_tree", expr) ));
				retnode["options"]->Add(cell);
				Clear(expr, base_expr, currCategory);

			}
			else if (currCategory == "DATA_DIRECTORY")
			{
				// we have the directory name
				expr->Add(GetConstantType(trim));
				cell = CellPairs(( CellPair("expr_type", Expressions::DIRECTORY),
					CellPair("kind", "DATA"), CellPair("base_expr", base_expr->Trim()),
					CellPair("delim", " "), CellPair("sub_tree", expr) ));
				retnode["options"]->Add(cell);
				Clear(expr, base_expr, prevCategory);
				continue;

			}
			else if (currCategory == "INDEX_DIRECTORY")
			{
				// we have the directory name
				expr->Add(GetConstantType(trim));
				cell = CellPairs(( CellPair("expr_type", Expressions::DIRECTORY),
					CellPair("kind", "INDEX"), CellPair("base_expr", base_expr->Trim()),
					CellPair("delim", " "), CellPair("sub_tree", expr) ));
				retnode["options"]->Add(cell);
				Clear(expr, base_expr, prevCategory);
				continue;
			}
			else if (currCategory == "TABLE_NAME")
			{
				retnode["base_expr"] = trim;
				retnode["name"] = trim;
				retnode["no_quotes"] = ExtractQuotesPairs(trim);
				Clear(expr, base_expr, prevCategory);

			}
			else if (currCategory == "LIKE")
			{
				cell = CellPairs(( CellPair("expr_type", Expressions::TABLE),
					CellPair("table", trim), CellPair("base_expr", trim),
					CellPair("no_quotes", ExtractQuotesPairs(trim)) ));
				retnode["like"] = cell;
				Clear(expr, base_expr, currCategory);

			}
			else if (currCategory == "")
			{
				// after table name
				if (prevCategory == "TABLE_NAME" && upper[0] == '(' && upper->EndsWith(")"))
				{
					unparsed = Scanner::Split(RemoveParenthesis(trim));
					coldef = (gcnew CreateDefinitionParser(Options))->Execute(RemoveParenthesis(trim));
					cell = CellPairs(( CellPair("expr_type", Expressions::BRACKET_EXPRESSION),
						CellPair("base_expr", base_expr), CellPair("sub_tree", coldef["create-def"]) ));
					retnode["create-def"] = cell;
					expr = gcnew StringCell();
					base_expr = "";
					currCategory = "CREATE_DEF";
				}
			}
			else if (currCategory == "UNION")
			{
				// TO DO: this pair.Value starts and ends with parenthesis
				// and contains a list of table names (comma-separated)
				// split the pair.Value and add the list as subtree
				// we must change the DefaultProcessor

				unparsed = Scanner::Split(RemoveParenthesis(trim));
				cell = CellPairs(( CellPair("expr_type", Expressions::BRACKET_EXPRESSION),
					CellPair("base_expr", trim), CellPair("sub_tree", "***TODO***") ));
				expr->Add(cell);
				cell = CellPairs(( CellPair("expr_type", Expressions::UNION),
					CellPair("base_expr", base_expr->Trim()), CellPair("delim", " "),
					CellPair("sub_tree", expr) ));
				retnode["options"]->Add(cell);
				Clear(expr, base_expr, currCategory);
			}
			else
			{

				// strings and numeric constants
				expr->Add(GetConstantType(trim));
				cell = CellPairs(( CellPair("expr_type", Expressions::EXPRESSION),
					CellPair("base_expr", base_expr->Trim()), CellPair("delim", " "),
					CellPair("sub_tree", expr) ));
				retnode["options"]->Add(cell);
				Clear(expr, base_expr, currCategory);
			}
		}

		prevCategory = currCategory;
		currCategory = "";
	}

	if (retnode->IsUnpopulated["like"])
		retnode->Remove("like");

	if (retnode->IsUnpopulated["select-option"])
		retnode->Remove("select-option");

	if (retnode->IsUnpopulated["options"])
		retnode["options"] = "";


	MoveLike(root);

	root[_Key] = parserNode;

	return root;
}

}