#include "pch.h"
#include "UnionParser.h"

#include "RegexLexer.h"
#include "Parser.h"
#include "SqlParser.h"



namespace BlackbirdDsl {






StringCell^ UnionParser::Parse(StringCell^ root)
{

	// Sometimes the parser needs to skip ahead until a particular
	// token is found
	SysStr^ skipUntilToken = nullptr;

	// This is the last type of union used (UNION or UNION ALL)
	// indicates (a) presence of at least one union in this query
	// (b) the type of union if this is the first or last query
	SysStr^ unionType = nullptr;

	// Sometimes a "query" consists of more than one query (like a UNION query)
	// his array holds all the queries
	StringCell^ output = gcnew StringCell();
	StringCell^ nodes = gcnew StringCell();
	StringCell^ node;

	SysStr^ str;
	SysStr^ search;

	int len = root->Count;

	for (int index = 0; index < len; index++)
	{
		node = root[index];
		str = node->Trim();

		// overread all tokens till that given token
		if (skipUntilToken != nullptr)
		{
			if (SysStr::IsNullOrEmpty(str))
				continue; // read the next token

			if (str->ToUpper() == skipUntilToken)
			{
				skipUntilToken = nullptr;
				continue; // read the next token
			}
		}

		if (str->ToUpper() != "UNION")
		{
			nodes->Add(node); // here we get empty tokens, if we remove these, we get problems in parse_sql()
			continue;
		}

		unionType = "UNION";


		// we are looking for an ALL token right after UNION
		for (int i = index + 1; i < len; ++i)
		{
			search = root[i]->Trim();

			if (SysStr::IsNullOrEmpty(search))
				continue;

			if (search->ToUpper() != "ALL")
				break;

			// the other for-loop should overread till "ALL"
			skipUntilToken = "ALL";
			unionType = "UNION ALL";
		}

		// store the tokens related to the unionType
		if (nodes->Count > 0)
		{

			output->Add(unionType, nodes);
			nodes = gcnew StringCell();
		}
	}


	// the query tokens after the last UNION or UNION ALL
	// or we don't have an UNION/UNION ALL
	if (nodes->IsCollection)
	{
		if (unionType != nullptr)
		{
			SplitRemainder(unionType, output, nodes);
		}
		else
		{
			output->Add(nodes);
		}
	}

	ParseMySql(output);

	return output;

}



/**
 * MySQL supports a special form of UNION:
 * (select ...)
 * union
 * (select ...)
 *
 * This function handles this query syntax. Only one such subquery
 * is supported in each UNION block. (select)(select)union(select) is not legal.
 * The extra queries will be silently ignored.
 */

void UnionParser::ParseMySql(StringCell^ root)
{
	SysStr^ str;
	StringCell^ node;
	StringCell^ unionNode;


	for each(SysStr^ unionType in UnionTypes)
	{
		if (!root->TryGetValue(unionType, unionNode))
			continue;


		if (IsNullPtr(unionNode) || !unionNode->IsCollection)
			continue;



		for each(ReplicaKeyPair(StringCell^) pair in unionNode->ReplicaKeyEnumerator)
		{
			for each(StringCell^ cell in pair.Value->Enumerator)
			{
				str = cell->Trim();


				if (SysStr::IsNullOrEmpty(str))
					continue;

				// starts with "(select"

				if (RegexLexer::SelectIsMatch(str))
				{
					str = RemoveParenthesis(str);

					node = (gcnew Parser(_Options))->Execute(str);

					if (pair.Key.Named)
						unionNode[pair.Key.Segment] = node;
					else
						unionNode[pair.Key.Ordinal] = node;

					break;
				}
				

				node = ParserInstance(SqlParser)->Parse(pair.Value);

				if (pair.Key.Named)
					unionNode[pair.Key.Segment] = node;
				else
					unionNode[pair.Key.Ordinal] = node;

				break;
			}
		}
	}

}




/**
 * Moves the final union query into a separate output, so the remainder (such as ORDER BY) can
 * be processed separately.
 */
void UnionParser::SplitRemainder(SysStr^ unionType, StringCell^ root, StringCell^ remainder)
{
	StringCell^ finalQueryNodes = gcnew StringCell();

	//If this token contains a matching pair of brackets at the start and end, use it as the final query

	bool finalQueryFound = false;
	SysStr^ str;

	if (remainder->Count == 1)
	{
		if (!remainder[0]->IsUnpopulated)
		{
			str = remainder[0]->Trim();

			if (str->StartsWith("(") && str->EndsWith(")"))
			{
				root->Add(unionType, remainder);
				finalQueryFound = true;
			}
		}
	}


	if (!finalQueryFound)
	{
		StringCell^ node;

		for (int i = 0; i < remainder->Count; i++)
		{
			node = remainder[i];

			if (node->ToUpper == "ORDER")
			{
				break;
			}
			else
			{
				finalQueryNodes->Add(node);
				remainder->RemoveAt(i);
				i--;
			}
		}
	}



	if (finalQueryNodes->Implode()->Trim() != "")
	{
		root->Add(unionType, finalQueryNodes);
	}

	str = remainder->Implode()->Trim();

	if (str != "")
	{
		root->Add(ParserInstance(Parser)->Execute(str));
	}

}

}