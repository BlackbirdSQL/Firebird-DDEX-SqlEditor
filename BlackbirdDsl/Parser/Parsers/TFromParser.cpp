#include "pch.h"
#include "TFromParser.h"

#include "Parser.h"
#include "UnionParser.h"
#include "Scanner.h"
#include "WColumnListParser.h"
#include "WExpressionListParser.h"
#include "RegexLexer.h"




namespace BlackbirdDsl {



StringCell^ TFromParser::InitParseInfo(StringCell^ parseInfo)
{
	// first init
	if (parseInfo->IsNull)
	{
		parseInfo = CellPairs((CellPair("join_type", ""), CellPair("saved_join_type", "JOIN")));

	}
	// loop init
	return CellPairs(( CellPair("expression", ""), CellPair("token_count", 0),
		CellPair("table", ""), CellPair("no_quotes", ""), CellPair("alias", false),
		CellPair("hints", NullCell), CellPair("join_type", ""), CellPair("next_join_type", ""),
		CellPair("saved_join_type", parseInfo["saved_join_type"]), CellPair("ref_type", false),
		CellPair("ref_expr", false), CellPair("base_expr", false), CellPair("sub_tree", false),
		CellPair("subquery", "")));
}




StringCell^ TFromParser::ParseFromExpression(StringCell^ parseInfo)
{
	StringCell^ retnode = NullCell;
	StringCell^ unionQueries;
	StringCell^ subTree;
	StringCell^ unparsed;
	StringCell^ ref;
	StringCell^ cell;


	if (parseInfo->IsNullOrEmpty["hints"])
	{
		parseInfo["hints"] = false;
	}


	// exchange the join types (join_type is save now, saved_join_type holds the next one)
	parseInfo["join_type"] = parseInfo["saved_join_type"]; // initialized with JOIN
	parseInfo["saved_join_type"] = (parseInfo["next_join_type"] ? parseInfo["next_join_type"] : "JOIN");

	// we have a reg_expr, so we have to parse it
	if (!parseInfo->IsUnpopulated["ref_expr"])
	{
		unparsed = Scanner::Split(parseInfo["ref_expr"]->Trim());

		// here we can get a comma separated list
		for each(StringCell^ v in unparsed->Enumerator)
		{
			if (IsCommaToken(v))
				v = "";
		}

		if (parseInfo["ref_type"] == "USING")
		{
			// unparsed has only one entry, the column list
			cell = (gcnew WColumnListParser(Options))->Execute(RemoveParenthesis(unparsed[0]));

			ref = gcnew StringCell(-1, CellPairs(( CellPair("expr_type", Expressions::COLUMN_LIST),
				CellPair("base_expr", unparsed[0]), CellPair("sub_tree", cell) )));
		}
		else
		{
			ref = ParserInstance(WExpressionListParser)->Parse(unparsed);
		}

		parseInfo["ref_expr"] = (ref->IsNullOrEmpty ? NullCell : ref);
	}

	// there is an expression, we have to parse it
	if (!parseInfo->IsNullOrEmpty["table"] && parseInfo["table"]->Trim()->StartsWith("("))
	{
		parseInfo["expression"] = RemoveParenthesis(parseInfo["table"]);

		if (RegexLexer::SubTreeIsMatch(parseInfo["expression"]))
		{
			parseInfo["sub_tree"] = (gcnew Parser(Options))->Execute(parseInfo["expression"]);

			retnode["expr_type"] = Expressions::SUBQUERY;
		}
		else
		{
			unionQueries = (gcnew UnionParser(Options))->Execute(parseInfo["expression"]);

			// If there was no UNION or UNION ALL in the query, then the query is
			// stored at queries[0].
			if (!unionQueries->IsNull && !IsUnion(unionQueries))
			{

				cell = Parse(gcnew StringCell(_Key, unionQueries[0]));
				subTree = cell[_Key];
			}
			else
			{
				subTree = unionQueries;
			}

			parseInfo["sub_tree"] = subTree;
			retnode["expr_type"] = Expressions::TABLE_EXPRESSION;
		}
	}
	else
	{
		retnode["expr_type"] = Expressions::TABLE;
		retnode["table"] = parseInfo["table"];
		retnode["no_quotes"] = ExtractQuotesPairs(parseInfo["table"]);
	}

	retnode["alias"] = parseInfo["alias"];
	retnode["hints"] = parseInfo["hints"];
	retnode["join_type"] = parseInfo["join_type"];
	retnode["ref_type"] = parseInfo["ref_type"];
	retnode["ref_clause"] = parseInfo["ref_expr"];
	retnode["base_expr"] = parseInfo["expression"]->Trim();
	retnode["sub_tree"] = parseInfo["sub_tree"];

	return retnode;

}





StringCell^ TFromParser::Parse(StringCell^ root)
{
	StringCell^ parserNode = root[_Key];

	if (parserNode->Count == 0)
	{
		return root;
	}


	StringCell^ parseInfo = InitParseInfo(NullCell);

	StringCell^ retnode = NullCell;
	StringCell^ prevToken;
	StringCell^ cell;

	SysStr^ tokenCategory = "";
	SysStr^ upper;
	SysStr^ str;
	SysStr^ str2;
	SysStr^ str3;


	bool skipNext = false;
	bool breaker = false;
	int i = 0;
	int n, curHint;

	for each (StringCell^ token in parserNode->Enumerator)
	{
		if (skipNext && !token->IsUnpopulated)
		{
			parseInfo["token_count"]++;
			skipNext = false;
			continue;
		}
		else
		{
			if (skipNext) 
				continue;
		}

		if (IsCommentToken(token))
		{
			retnode->Add(CreateCommentToken(token));
			continue;
		}

		upper = token->Trim()->ToUpper();

		if (upper == "CROSS" || upper == "," || upper == "INNER" || upper == "STRAIGHT_JOIN")
		{
		}
		if (upper == "OUTER" || upper == "JOIN")
		{
			if (tokenCategory == "LEFT" || tokenCategory == "RIGHT" || tokenCategory == "NATURAL")
			{
				tokenCategory = "";
				parseInfo["next_join_type"] = prevToken->Trim()->ToUpper(); // it seems to be a join
			}
			else if (tokenCategory == "IDX_HINT")
			{
				parseInfo["expression"] += token;

				if (!parseInfo->IsNull["ref_type"])
				{
					// all after ON / USING
					parseInfo["ref_expr"] += token;
				}
			}
		}
		else if (upper == "LEFT" || upper == "RIGHT" || upper == "NATURAL")
		{
			tokenCategory = upper;
			prevToken = token;
			i++;
			continue;
		}
		else
		{
			breaker = false;
			if (tokenCategory == "LEFT" || tokenCategory == "RIGHT")
			{
				if (upper == "")
				{
					prevToken += token;
					breaker = true;
				}
				else
				{
					tokenCategory = "";     // it seems to be a function
					parseInfo["expression"] += prevToken;
					if (!parseInfo->IsNull["ref_type"])
					{
						// all after ON / USING
						parseInfo["ref_expr"] += prevToken;
					}
					prevToken = "";
				}
			}

			if (!breaker)
			{
				parseInfo["expression"] += token;
				if (!parseInfo->IsNull["ref_type"])
				{
					// all after ON / USING
					parseInfo["ref_expr"] += token;
				}
			}
		}

		if (upper == "")
		{
			i++;
			continue;
		}

		if (upper == "AS")
		{
			parseInfo["alias"] = CellPairs((CellPair("as", true), CellPair("name", ""), CellPair("base_expr", token)));
			parseInfo["token_count"]++;

			n = 1;
			str = "";
			str2 = "";

			while (str == "" && parserNode->Count > i + n)
			{
				str3 = parserNode[i + n];
				str2 += (str3 == "" ? " " : str3);

				str = str3->Trim();
				++n;
			}
			if (str2 != "")
				parseInfo["alias", "base_expr"] += str2;

			parseInfo["alias", "name"] = str;
			parseInfo["alias", "no_quotes"] = ExtractQuotesPairs(str);
			parseInfo["alias", "base_expr"]->Trimmed();
		}
		else if (upper == "IGNORE" || upper == "USE" || upper == "FORCE")
		{
			tokenCategory = "IDX_HINT";
			parseInfo["hints"]->Add("hint_type", upper);
			continue;
		}
		else if (upper == "KEY" || upper == "INDEX")
		{
			if (tokenCategory == "CREATE")
			{
				tokenCategory = upper; // TO DO: what is it for a statement?
				continue;
			}
			if (tokenCategory == "IDX_HINT")
			{
				curHint = parseInfo["hints"]->Count - 1;
				if (IsNullPtr(parseInfo["hints", curHint]))
					cell = NullCell;
				else
					cell = parseInfo["hints", curHint];
				cell["hint_type"] += (" " + upper);
				continue;
			}
		}
		else if (upper == "USING" || upper == "ON" || upper == "CROSS" || upper == "INNER"
			|| upper == "OUTER" || upper == "NATURAL")
		{
			if (upper == "USING" || upper == "ON")
			{
				parseInfo["ref_type"] = upper;
				parseInfo["ref_expr"] = "";
			}
			parseInfo["token_count"]++;
		}
		else if (upper == "FOR")
		{
			if (tokenCategory == "IDX_HINT")
			{
				curHint = parseInfo["hints"]->Count - 1;
				if (IsNullPtr(parseInfo["hints", curHint]))
					cell = NullCell;
				else
					cell = parseInfo["hints", curHint];
				cell["hint_type"] += (" " + upper);
				continue;
			}

			parseInfo["token_count"]++;
			skipNext = true;
		}
		else if (upper == "STRAIGHT_JOIN")
		{
			parseInfo["next_join_type"] = "STRAIGHT_JOIN";
			if (parseInfo->ContainsKey("subquery"))
			{
				cell = Scanner::Split(RemoveParenthesis(parseInfo["subquery"]));
				parseInfo["sub_tree"] = Parse(gcnew StringCell(_Key, cell));
				parseInfo["expression"] = parseInfo["subquery"];
			}

			retnode->Add(ParseFromExpression(parseInfo));
			parseInfo = InitParseInfo(parseInfo);
		}
		else if (upper == ",")
		{
			parseInfo["next_join_type"] = "CROSS";
		}
		else if (upper == "JOIN")
		{
			if (tokenCategory == "IDX_HINT")
			{
				curHint = parseInfo["hints"]->Count - 1;
				if (IsNullPtr(parseInfo["hints", curHint]))
					cell = NullCell;
				else
					cell = parseInfo["hints", curHint];
				cell["hint_type"] += (" " + upper);
				continue;
			}

			if (!IsNullPtr(parseInfo["subquery"]))
			{
				cell = Scanner::Split(RemoveParenthesis(parseInfo["subquery"]));
				parseInfo["sub_tree"] = Parse(gcnew StringCell(_Key, cell));
				parseInfo["expression"] = parseInfo["subquery"];
			}

			retnode->Add(ParseFromExpression(parseInfo));
			parseInfo = InitParseInfo(parseInfo);
		}
		if (upper == "GROUP BY")
		{
			if (tokenCategory == "IDX_HINT")
			{
				curHint = parseInfo["hints"]->Count - 1;
				if (IsNullPtr(parseInfo["hints", curHint]))
					cell = NullCell;
				else
					cell = parseInfo["hints", curHint];
				cell["hint_type"] += (" " + upper);
				continue;
			}
		}
		else
		{
			// TO DO: enhance it, so we can have base_expr to calculate the position of the keywords
			// build a subtree under "hints"
			if (tokenCategory == "IDX_HINT")
			{
				tokenCategory = "";

				curHint = parseInfo["hints"]->Count - 1;
				if (IsNullPtr(parseInfo["hints", curHint]))
					cell = NullCell;
				else
					cell = parseInfo["hints", curHint];
				cell["hint_list"] = token;
			}
			else if (parseInfo["token_count"] == 0)
			{
				if (parseInfo["table"] == "")
				{
					parseInfo["table"] = token;
					parseInfo["no_quotes"] = ExtractQuotesPairs(token);
				}
			}
			else if (parseInfo["token_count"] == 1)
			{
				parseInfo["alias"] = CellPairs((CellPair("as", NullCell), CellPair("name", token->Trim()),
					CellPair("no_quotes", ExtractQuotesPairs(token)), CellPair("base_expr", token->Trim())));
			}
			parseInfo["token_count"]++;
		}
		i++;
	}

	retnode->Add(ParseFromExpression(parseInfo));


	root[_Key] = retnode;

	return root;
}

}