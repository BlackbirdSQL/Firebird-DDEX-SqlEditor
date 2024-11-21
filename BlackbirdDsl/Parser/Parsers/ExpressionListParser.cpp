#include "pch.h"
#include "ExpressionListParser.h"

#include "Scanner.h"
#include "ExpressionToken.h"
#include "Parser.h"
#include "ExtensionMembers.h"



namespace BlackbirdDsl {




StringCell^ ExpressionListParser::Parse(StringCell^ root)
{
	bool skipNext = false;
	SysStr^ upper;
	SysStr^ token;

	ExpressionToken^ retnode = gcnew ExpressionToken();

	ExpressionToken^ prev = gcnew ExpressionToken();
	ExpressionToken^ curr = gcnew ExpressionToken();
	ExpressionToken^ tmpToken;
	ExpressionToken^ matchNode;
	ExpressionToken^ localExpr;
	ExpressionToken^ next;

	StringCell^ localTokenList = nullptr;
	StringCell^ tmpExprList;
	StringCell^ localExprList;

	for each(ReplicaKeyPair(StringCell^) rootPair in root->ReplicaKeyEnumerator)
	{
		if (IsCommentToken(rootPair.Value))
		{
			retnode->Add(CreateCommentToken(rootPair.Value));
			continue;
		}

		if (rootPair.Key.Named)
			tmpToken = gcnew ExpressionToken(rootPair.Value, rootPair.Key.Segment);
		else
			tmpToken = gcnew ExpressionToken(rootPair.Value);


		if (curr->IsWhitespaceToken)
			continue;

		if (skipNext)
		{
			// skip the next non-whitespace token
			skipNext = false;
			continue;
		}

		// is it a subquery? 
		if (curr->IsSubQueryToken)
		{

			curr->SubTree = (gcnew Parser(_Options))->Execute(RemoveParenthesis(curr->TrimToken));
			curr->TokenType = Expressions::SUBQUERY;

		}
		else if(curr->IsEnclosedWithinParenthesis)
		{
			// is it an in-list? 
			localTokenList = Scanner::Split(RemoveParenthesis(curr->TrimToken));

			if (prev->UpperToken == "IN") {

				for (int i = 0; i < localTokenList->Count; i++)
				{
					tmpToken = gcnew ExpressionToken(localTokenList[i]);

					if (tmpToken->IsCommaToken)
					{
						localTokenList->RemoveAt(i);
						i--;
					}

				}

				curr->SubTree = Parse(localTokenList);
				curr->TokenType = Expressions::IN_LIST;
			}
			else if (prev->UpperToken == "AGAINST")
			{
				matchNode = gcnew ExpressionToken();

				for each(ReplicaKeyPair(StringCell^) localPair in localTokenList->ReplicaKeyEnumerator)
				{
					if (localPair.Key.Named)
						tmpToken = gcnew ExpressionToken(localPair.Value, localPair.Key.Segment);
					else
						tmpToken = gcnew ExpressionToken(localPair.Value);


					upper = tmpToken->UpperToken;

					if (upper == "WITH")
					{
						matchNode = "WITH QUERY EXPANSION";
					}
					else if (upper == "IN")
					{
						matchNode = "IN BOOLEAN MODE";
					}

					if (matchNode == true)
						localTokenList->RemoveAt(localPair.Key.Ordinal);
				}

				tmpToken = (ExpressionToken^)Parse(localTokenList);

				if (matchNode == true)
				{
					matchNode = gcnew ExpressionToken(matchNode);
					matchNode->TokenType = Expressions::MATCH_MODE;
					tmpToken->Add(matchNode->ToCellPairs());
				}

				curr->SubTree = tmpToken;
				curr->TokenType = Expressions::MATCH_ARGUMENTS;
				prev->TokenType = Expressions::SIMPLE_FUNCTION;

			}
			else if(prev->IsColumnReference || prev->IsFunction || prev->IsAggregateFunction
				|| prev->IsCustomFunction)
			{

				// if we have a colref followed by a parenthesis pair,
				// it isn"t a colref, it is a user-function

				// TO DO: this should be a method, because we need the same code
				// below for unspecified tokens (expressions).

				localExpr = gcnew ExpressionToken();
				tmpExprList = gcnew StringCell();

				for each (ReplicaKeyPair(StringCell^) localPair in localTokenList->ReplicaKeyEnumerator)
				{
					if (localPair.Key.Named)
						tmpToken = gcnew ExpressionToken(localPair.Value, localPair.Key.Segment);
					else
						tmpToken = gcnew ExpressionToken(localPair.Value);

					if (!tmpToken->IsCommaToken)
					{
						localExpr->AddToken(localPair.Value);
						tmpExprList->Add(localPair.Value);
					}
					else
					{
						// an expression could have multiple parts split by operands
						// if we have a comma, it is a split-point for expressions
						localExprList = Parse(tmpExprList);

						if (localExprList->Count > 1)
						{
							localExpr->SubTree = localExprList;
							localExpr->TokenType = Expressions::EXPRESSION;
							localExprList["alias"] = false;
							localExprList = (gcnew StringCell(-1, localExprList));
						}

						if (IsNullPtr(curr->SubTree))
						{
							if (!localExprList->IsUnpopulated)
								curr->SubTree = localExprList;
						}
						else
						{
							tmpExprList = curr->SubTree;
							tmpExprList->Merge(localExprList);
							curr->SubTree = tmpExprList;
						}

						tmpExprList = NullCell;
						localExpr = gcnew ExpressionToken();
					}
				}

				localExprList = Parse(tmpExprList);

				if (localExprList->Count > 1)
				{
					localExpr->SubTree = localExprList;
					localExpr->TokenType = Expressions::EXPRESSION;
					localExprList = localExpr->ToCellPairs();
					localExprList["alias"] = false;
				}

				if (IsNullPtr(curr->SubTree))
				{
					if (!localExprList->IsUnpopulated)
						curr->SubTree = localExprList;
				}
				else
				{
					tmpExprList = curr->SubTree;
					tmpExprList->Merge(localExprList);
					curr->SubTree = tmpExprList;
				}

				prev->SubTree = curr->SubTree;

				if (prev->IsColumnReference)
				{
					if (Str::IsCustomFunction(prev->UpperToken))
						prev->TokenType = Expressions::CUSTOM_FUNCTION;
					else
						prev->TokenType = Expressions::SIMPLE_FUNCTION;

					prev->NoQuotes = nullptr;
				}

				retnode->ArrayPop();
				curr = prev;
			}

			// we have parenthesis, but it seems to be an expression
			if (curr->IsUnspecified)
			{
				tmpExprList = gcnew StringCell(localTokenList->Values);
				localExprList = Parse(tmpExprList);

				curr->TokenType = Expressions::BRACKET_EXPRESSION;
				if (IsNullPtr(curr->SubTree))
				{
					if (!localExprList->IsUnpopulated)
					{
						curr->SubTree = localExprList;
					}
				}
				else {
					tmpExprList = curr->SubTree;
					tmpExprList->Merge(localExprList);
					curr->SubTree = tmpExprList;
				}
			}

		}
		else if(curr->IsVariableToken)
		{

			// # a variable
			// # it can be quoted

			curr->TokenType = GetVariableType(curr->UpperToken);
			curr->SubTree = false;
			curr->NoQuotes = curr->Token->Trim()->Trim('@');

		}
		else
		{
			upper = curr->UpperToken;
			// it is either an operator, a colref or a constant 
			if (upper == "*")
			{
				curr->SubTree = false; // o subtree

				// single or first element of expression list -> all-column-alias
				if (retnode->IsUnpopulated)
				{
					curr->TokenType = Expressions::COLREF;
				}
				else
				{

					// if the last token is colref, const or expression
					// then * is an operator
					// but if the previous colref ends with a dot, the * is the all-columns-alias
					if (!prev->IsColumnReference && !prev->IsConstant && !prev->IsExpression
						&& !prev->IsBracketExpression && !prev->IsAggregateFunction
						&& !prev->IsVariable && !prev->IsFunction)
					{
						curr->TokenType = Expressions::COLREF;
					}
					else
					{
						if (prev->IsColumnReference && prev->EndsWith("."))
						{
							prev->AddToken("*"); // tablealias dot *
							continue; // skip the current token
						}

						curr->TokenType = Expressions::OPERATOR;
					}
				}
			}
			else if (upper == ":=" || upper == "AND" || upper == "&&" || upper == "BETWEEN" || upper == "BINARY"
				|| upper == "&" || upper == "~" || upper == "|" || upper == "^" || upper == "DIV" || upper == "/"
				|| upper == "<=>" || upper == "=" || upper == ">=" || upper == ">" || upper == "IS" || upper == "NOT"
				|| upper == "<<" || upper == "<=" || upper == "<" || upper == "LIKE" || upper == "%" || upper == "!="
				|| upper == "<>" || upper == "REGEXP" || upper == "!" || upper == "||" || upper == "OR"
				|| upper == ">>" || upper == "RLIKE" || upper == "SOUNDS" || upper == "XOR" || upper == "IN")
			{
				curr->SubTree = false;
				curr->TokenType = Expressions::OPERATOR;
			}
			else if (upper == "NULL")
			{
				curr->SubTree = false;
				curr->TokenType = Expressions::CONSTANT;
			}
			else if (upper == "-" || upper == "+")
			{
				// differ between preceding sign and operator
				curr->SubTree = false;

				if (prev->IsColumnReference || prev->IsFunction || prev->IsAggregateFunction
					|| prev->IsConstant || prev->IsSubQuery || prev->IsExpression
					|| prev->IsBracketExpression || prev->IsVariable || prev->IsCustomFunction)
				{
					curr->TokenType = Expressions::OPERATOR;
				}
				else
				{
					curr->TokenType = Expressions::SIGN;
				}
			}
			else
			{
				curr->SubTree = false;
				token = curr->Token[0];

				if (token == "'")
				{
					// it is a string literal
					curr->TokenType = Expressions::CONSTANT;
				}
				else if (token == "\"")
				{
					if ((Options & EnParserOptions::ANSI_QUOTES) == EnParserOptions::NONE)
					{
						// If we"re not using ANSI quotes, this is a string literal.
						curr->TokenType = Expressions::CONSTANT;
					}
					// Otherwise continue to the next case
				}
				else if (token == "`")
				{
					// it is an escaped colum name
					curr->TokenType = Expressions::COLREF;
					curr->NoQuotes = curr->Token;
				}
				else
				{
					if (Str::IsNumeric(curr->Token))
					{
						if (prev->IsSign)
						{
							prev->AddToken(curr->Token); // it is a negative numeric constant
							prev->TokenType = Expressions::CONSTANT;
							continue;
							// skip current token
						}
						else
						{
							curr->TokenType = Expressions::CONSTANT;
						}
					}
					else
					{
						curr->TokenType = Expressions::COLREF;
						curr->NoQuotes = curr->Token;
					}
				}
			}
		}

		// is a reserved word? 
		if (!curr->IsOperator && !curr->IsInList && !curr->IsFunction && !curr->IsAggregateFunction
			&& !curr->IsCustomFunction && Str::IsReserved(curr->UpperToken))
		{
			next = (!root->IsNull[rootPair.Key.Ordinal + 1] ? (gcnew ExpressionToken(root[rootPair.Key.Ordinal + 1])) : (gcnew ExpressionToken()));

			bool isEnclosedWithinParenthesis = next->IsEnclosedWithinParenthesis;
			if (isEnclosedWithinParenthesis && Str::IsCustomFunction(curr->UpperToken))
			{
				curr->TokenType = Expressions::CUSTOM_FUNCTION;
				curr->NoQuotes = nullptr;

			}
			else if (isEnclosedWithinParenthesis && Str::IsAggregateFunction(curr->UpperToken))
			{
				curr->TokenType = Expressions::AGGREGATE_FUNCTION;
				curr->NoQuotes = nullptr;

			}
			else if (curr->UpperToken == "NULL")
			{
				// it is a reserved word, but we would like to set it as constant
				curr->TokenType = Expressions::CONSTANT;

			}
			else
			{
				if (isEnclosedWithinParenthesis && Str::IsParameterizedFunction(curr->UpperToken))
				{
					// issue 60: check functions with parameters
					// -> colref (we check parameters later)
					// -> if there is no parameter, we leave the colref
					curr->TokenType = Expressions::COLREF;

				}
				else if (isEnclosedWithinParenthesis && Str::IsFunction(curr->UpperToken)) {
					curr->TokenType = Expressions::SIMPLE_FUNCTION;
					curr->NoQuotes = nullptr;

				}
				else if(!isEnclosedWithinParenthesis && Str::IsFunction(curr->UpperToken))
				{
					// Colname using function name.
					curr->TokenType = Expressions::COLREF;
				}
				else
				{
					curr->TokenType = Expressions::RESERVED;
					curr->NoQuotes = nullptr;
				}
			}
		}

		// issue 94, INTERVAL 1 MONTH
		if (curr->IsConstant && Str::IsParameterizedFunction(prev->UpperToken))
		{
			prev->TokenType = Expressions::RESERVED;
			prev->NoQuotes = nullptr;
		}

		if (prev->IsConstant && Str::IsParameterizedFunction(curr->UpperToken))
		{
			curr->TokenType = Expressions::RESERVED;
			curr->NoQuotes = nullptr;
		}

		if (curr->IsUnspecified)
		{
			curr->TokenType = Expressions::EXPRESSION;
			curr->NoQuotes = nullptr;
			curr->SubTree = Parse(Scanner::Split(curr->TrimToken));
		}

		retnode->Add(curr);
		prev = curr;

	} // end of for-loop

	return retnode->ToCellPairs();

}

}