#include "pch.h"
#include "TSelectParser.h"

#include "Scanner.h"



namespace BlackbirdDsl {




StringCell^ TSelectParser::Parse(StringCell^ root)
{
	StringCell^ parserNode = root[_Key];

	if (parserNode->Count == 0)
	{
		root->Remove(_Key);
		return root;
	}

	StringCell^ expression = NullCell;
	StringCell^ retnode = NullCell;

	SysStr^ upper;


	for each(StringCell^ token in parserNode->Enumerator)
	{
		if (IsCommaToken(token))
		{
			expression = ((WSelectExpressionParser^)this)->Execute(expression->Trim());

			expression["delim"] = ",";
			retnode->Add(expression);
			expression = NullCell;
		}
		else if (IsCommentToken(token))
		{
			retnode->Add(CreateCommentToken(token));
		}
		else
		{
			upper = token->ToUpper;

			// add more SELECT options here
			if (upper == "DISTINCT" || upper == "DISTINCTROW" || upper == "HIGH_PRIORITY"
				|| upper == "SQL_CACHE" || upper == "SQL_NO_CACHE" || upper == "SQL_CALC_FOUND_ROWS"
				|| upper == "STRAIGHT_JOIN" || upper == "SQL_SMALL_RESULT" || upper == "SQL_BIG_RESULT"
				|| upper == "SQL_BUFFER_RESULT")
			{
				expression = ((WSelectExpressionParser^)this)->Execute(token->Trim());
				expression["delim"] = " ";
				retnode->Add(expression);
				expression = NullCell;
			}
			else
			{
				expression += token->ToString();
			}
		}
	}

	if (!expression->IsNullOrEmpty)
	{
		expression = ((WSelectExpressionParser^)this)->Execute(expression->Trim());
		expression["delim"] = false;
		retnode->Add(expression);
	}

	root[_Key] = parserNode;

	return root;
}

}