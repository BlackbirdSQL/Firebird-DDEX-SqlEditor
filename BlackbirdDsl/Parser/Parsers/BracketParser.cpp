#include "pch.h"
#include "BracketParser.h"

#include "Parser.h"






namespace BlackbirdDsl {



StringCell^ BracketParser::GetRemainingNodes(StringCell^ parserNode)
{
	if (IsNullPtr(parserNode) || parserNode->IsUnpopulated)
		return nullptr;

	StringCell^ retNode = gcnew StringCell(parserNode->Count);



	for each(ReplicaKeyPair(StringCell^) pair in parserNode->ReplicaKeyEnumerator)
	{

		if (pair.Key.Named)
		{
			if (pair.Key.Segment == "BRACKET" || pair.Key.Segment == "SELECT" || pair.Key.Segment == "FROM")
				continue;
			retNode->Add(pair.Key.Segment, pair.Value);
		}
		else
		{
			retNode->Add(pair.Value);
		}
	}

	return retNode;
}



StringCell^ BracketParser::Parse(StringCell^ root)
{
	StringCell^ parserNode = root[_Key];

	if (parserNode->Count == 0)
	{
		root->Remove(_Key);
		return root;
	}

	SysStr^ expression = parserNode[0];
	SysStr^ sanitized = RemoveParenthesis(parserNode[0]);

	parserNode = (gcnew Parser(_Options))->Execute(sanitized);

	StringCell^ remainingPairs = GetRemainingNodes(parserNode);

	if (!parserNode->IsNull[_Key])
		parserNode = parserNode[_Key];

	StringCell^ cell;

	if (!parserNode->IsNull["SELECT"])
	{
		cell = CellPairs(( CellPair("expr_type", Expressions::QUERY),
			CellPair("base_expr", sanitized), CellPair("sub_tree", parserNode) ));
		parserNode = cell;
	}

	cell = CellPairs(( CellPair("expr_type", Expressions::BRACKET_EXPRESSION),
		CellPair("base_expr", expression), CellPair("sub_tree", parserNode) ));

	parserNode = gcnew StringCell();
	parserNode += cell;


	if (!remainingPairs->IsUnpopulated)
	{
		for each (ReplicaKeyPair(StringCell^) pair in remainingPairs->ReplicaKeyEnumerator)
		{
			if (pair.Key.Named)
				parserNode += gcnew StringCell(pair.Key.Segment, pair.Value);
			else
				parserNode += gcnew StringCell(pair.Value);
		}
	}

	if (!parserNode->IsUnpopulated)
	{
		StringCell^ remainingExpressions = parserNode[0]["remaining_expressions"];

		if (!IsNullPtr(remainingExpressions) && remainingExpressions->Count > 0)
		{
			parserNode[0]->Remove("remaining_expressions");

			for each (ReplicaKeyPair(StringCell^) pair in remainingExpressions->ReplicaKeyEnumerator)
			{
				if (pair.Key.Named)
					parserNode[pair.Key.Segment] = pair.Value;
				else
					parserNode += pair.Value;
			}
		}
	}

	root[_Key] = parserNode;


	return root;

}

}