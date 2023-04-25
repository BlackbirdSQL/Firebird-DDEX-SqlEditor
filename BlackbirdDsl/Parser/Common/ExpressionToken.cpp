#include "pch.h"
#include "ExpressionToken.h"

#include "AbstractParser.h"



namespace BlackbirdDsl {

// TODO: we could replace it with a constructor new ExpressionToken(this, "*")
void ExpressionToken::AddToken(StringCell^ value)
{
	Token->Add(value);
}


bool ExpressionToken::EndsWith(SysStr^ needle)
{
	if (needle->Length == 0)
		return true;

	if (IsNullPtr(Token) || Token->IsNullOrEmpty)
		return false;

	return (Token->ToString()->EndsWith(needle));
}


StringCell^ ExpressionToken::ToCellPairs()
{
	StringCell^ retnode = CellPairs((( CellPair("expr_type", TokenType),
		CellPair("base_expr", Token), CellPair("sub_tree", SubTree))));

	if (!IsNullPtr(NoQuotes) && !NoQuotes->IsUnpopulated)
		retnode["no_quotes"] = NoQuotes;

	return retnode;
}



}