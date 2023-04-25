#include "pch.h"
#include "WSelectExpressionParser.h"

#include "WExpressionListParser.h"
#include "Scanner.h"




namespace BlackbirdDsl {





/*
* This fuction processes each SELECT clause.
* We determine what (if any) alias
* is provided, and we set the type of expression.
*/
StringCell^ WSelectExpressionParser::Parse(StringCell^ root)
{
	if (root->Count == 0)
		return NullCell;
	/*
	* Determine if there is an explicit alias after the AS clause.
	* If AS is found, then the next non-whitespace token is captured as the alias.
	* The root after (and including) the AS are removed.
	*/

	bool capture = false;


	StringCell^ baseExpr = "";
	StringCell^ stripped = NullCell;
	StringCell^ alias = NullCell;
	StringCell^ processed = NullCell;
	StringCell^ noQuotes = nullptr;

	StringCell^ token;
	StringCell^ last;
	StringCell^ prev;
	StringCell^ cell;

	StringCell^ retnode;

	SysStr^ upper;
	SysStr^ type;

	for (int i = 0; i < root->Count; ++i)
	{
		token = root[i];
		upper = token->ToUpper;

		if (upper == "AS")
		{
			alias = CellPairs((CellPair("as", true), CellPair("name", ""), CellPair("base_expr", token)));
			root[i] = "";
			capture = true;
			continue;
		}

		if (!IsWhitespaceToken(upper))
		{
			stripped->Add(token);
		}

		// we have an explicit AS, next one can be the alias
		// but also a comment!
		if (capture)
		{
			if (!IsWhitespaceToken(upper) && !IsCommentToken(upper))
			{
				alias["name"] += token;
				stripped->ArrayPop();
			}
			alias["base_expr"] += token;
			root[i] = "";
			continue;
		}

		baseExpr += token;
	}

	if (!alias->IsUnpopulated)
	{
		// remove quotation from the alias
		alias["no_quotes"] = ExtractQuotesPairs(alias["name"]);
		alias["name"]->Trimmed();
		alias["base_expr"]->Trimmed();
	}

	stripped = ParserInstance(WExpressionListParser)->Parse(stripped);

	// TODO: the last part can also be a comment, don"t use array_pop

	// we remove the last token, if it is a colref,
	// it can be an alias without an AS
	last = stripped->ArrayPop();
	if (!alias && IsColumnReference(last))
	{

		// TODO: it can be a comment, don"t use array_pop

		// check the token before the colref
		prev = stripped->ArrayPop();

		if (IsReserved(prev) || IsConstant(prev) || IsAggregateFunction(prev)
			|| IsFunction(prev) || IsExpression(prev) || IsSubQuery(prev)
			|| IsColumnReference(prev) || IsBracketExpression(prev) || IsCustomFunction(prev))
		{

			alias = CellPairs((CellPair("as", false), CellPair("name", last["base_expr"]->Trim()),
				CellPair("no_quotes", ExtractQuotesPairs(last["base_expr"])),
					CellPair("base_expr", last["base_expr"]->Trim())));
			// remove the last token
			root->ArrayPop();
		}
	}

	baseExpr = _Sql;

	// TODO: this is always done with stripped, how we do it twice?
	processed = ParserInstance(WExpressionListParser)->Parse(root);

	// if there is only one part, we copy the expr_type
	// in all other cases we use "EXPRESSION" as global type
	type = Expressions::EXPRESSION;

	if (processed->Count == 1)
	{
		if (!IsSubQuery(processed[0]))
		{
			cell = processed[0];
			type = cell["expr_type"];
			baseExpr = cell["base_expr"];
			noQuotes = !cell->IsUnpopulated["no_quotes"] ? cell["no_quotes"] : nullptr;
			processed = cell["sub_tree"]; // it can be FALSE
		}
	}

	retnode = CellPairs(( CellPair("expr_type", type), CellPair("alias", alias),
		CellPair("base_expr", baseExpr->Trim()),
		CellPair("sub_tree", (IsNullPtr(processed) || processed->IsUnpopulated) ? NullCell : processed)));

	if (!IsNullPtr(noQuotes))
		retnode["no_quotes"] = noQuotes;

	return retnode;
}

}