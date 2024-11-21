#include "pch.h"
#include "Scanner.h"

#include "ExtensionMembers.h"
#include "RegexLexer.h"





namespace BlackbirdDsl {



void Scanner::ConcatNegativeNumbers(StringCell^ tokens)
{

	bool possibleSign = true;
	XCHAR ch;
	int i = 0;
	StringCell^ cell;

	while (i < tokens->Count)
	{
		cell = tokens[i];

		// a sign is also possible on the first position of the tokenlist
		if (possibleSign)
		{
			if (cell == "-" || cell == "+")
			{
				if (i + 1 < tokens->Count && !tokens->IsNull[i + 1])
				{
					tokens[i + 1] = tokens + tokens[i + 1];
					tokens->RemoveAt(i);
				}
			}

			possibleSign = false;

			continue;
		}

		// TO DO: we can have sign of a number after "(" and ",", are others possible?
		ch = cell->Value[cell->Length - 1];

		if (ch == ',' || ch == '(')
			possibleSign = true;

		i++;
	}

	// itemsTracked = *tokens;
	// return tokens;
}



void Scanner::ConcatScientificNotations(StringCell^ tokens)
{
	bool scientific = false;
	int i = 0;
	SysStr^ item;


	while (i < tokens->Count)
	{
		item = tokens[i];


		if (scientific)
		{
			if (item == "-" || item == "+")
			{
				if (i > 0)
				{
					tokens[i - 1] += item;
					tokens->RemoveAt(i);
					if (i < tokens->Count)
					{
						tokens[i - 1] += tokens[i];
						tokens->RemoveAt(i);
						i--;
					}
					i--;

				}

			}
			else if (Str::IsNumeric(item))
			{
				if (i > 0)
				{
					tokens[i - 1] += item;
					tokens->RemoveAt(i);
					i--;
				}
			}
			scientific = false;

			i++;
			continue;
		}

		if (toupper(item[item->Length - 1]) == 'E')
		{
			scientific = item->Length > 1 && Str::IsNumeric(item->Substring(0, item->Length - 2));
		}

		i++;
	}

	// itemsTracked = *tokens;
	// return tokens;
}



void Scanner::ConcatUserDefinedVariables(StringCell^ tokens)
{
	int i = 0;
	int userdef = -1;
	SysStr^ item;


	while (i < tokens->Count)
	{
		item = tokens[i];


		if (userdef != -1)
		{
			tokens[userdef] += item;
			tokens->RemoveAt(i);
			i--;
			if (item != "@")
				userdef = -1;
		}

		if (userdef == -1 && item == "@")
			userdef = i;

		i++;
	}

	// return tokens;
}

void Scanner::ConcatComments(StringCell^ tokens) {

	int i = 0;

	int commentOffset = -1;

	List<SysStr^>^ backTicks = gcnew List<SysStr^>(4);

	bool inString = false;
	bool inLine = false;

	SysStr^ lastBackTick = "";

	SysStr^ token;

	while (i < tokens->Count)
	{
		token = tokens[i];


		/*
		 * Check to see if we're inside a value (i.e. back ticks).
		 * If so inline comments are not valid.
		 */
		if (commentOffset == -1 && IsBackTick(token))
		{
			if (backTicks->Count != 0)
			{
				lastBackTick = ArrayPop(backTicks);

				if (lastBackTick != token)
				{
					backTicks->Add(lastBackTick); // Re-add last back tick
					backTicks->Add(token);
				}
			}
			else
			{
				backTicks->Add(token);
			}
		}

		if (commentOffset == -1 && (token == "\"" || token == "'"))
		{
			inString = !inString;
		}

		if (!inString)
		{
			if (commentOffset != -1)
			{
				if (inLine && (token == "\n" || token == "\r\n"))
				{
					commentOffset = -1;
				}
				else
				{
					tokens->RemoveAt(i);
					i--;
					tokens[commentOffset] += token;
				}

				if (!inLine && (token == "*/"))
				{
					commentOffset = -1;
				}
			}

			if (commentOffset == -1 && token == "--" && backTicks->Count == 0)
			{
				commentOffset = i;
				inLine = true;
			}

			if (commentOffset == -1 && token->Length > 0 && token[0] == '#' && backTicks->Count == 0)
			{
				commentOffset = i;
				inLine = true;
			}

			if (commentOffset == -1 && token == "/*")
			{
				commentOffset = i;
				inLine = false;
			}
		}

		i++;
	}

}






void Scanner::BalanceBackticks(StringCell^ tokens)
{
	int i = 0;
	int cnt = tokens->Count;

	SysStr^ token;

	while (i < cnt)
	{
		token = tokens[i];


		if (IsBackTick(token))
		{
			cnt = BalanceCharacter(tokens, i, token);
		}

		i++;
	}
}



// backticks are not balanced within one token, so we have
// to re-combine some tokens
int Scanner::BalanceCharacter(StringCell^ tokens, int idx, SysStr^ str)
{
	int i = idx + 1;
	SysStr^ item;


	while (i < tokens->Count)
	{
		item = tokens[i];


		tokens[idx] += item;
		tokens->RemoveAt(i);
		i--;

		if (item == str)
			break;

		i++;
	}

	return tokens->Count;
}



/**
 * This function concats some tokens to a column reference.
 * There are two different cases:
 *
 * 1. If the current token ends with a dot, we will add the next token
 * 2. If the next token starts with a dot, we will add it to the previous token
 *
 */
void Scanner::ConcatColReferences(StringCell^ tokens)
{
	int i = 0, k, len;
	SysStr^ item, ^ itemk;


	while (i < tokens->Count)
	{
		item = tokens[i];


		if (item-> Length > 0 && item[0] == '.')
		{

			// concat the previous tokens, till the token has been changed
			k = i - 1;
			len = item->Length;

			while (k >= 0 && len == item->Length)
			{
				itemk = tokens[k];


				tokens[i] = item = itemk + item;
				tokens->RemoveAt(k);

				k--;
			}
		}

		if (item->EndsWith(".") && !Str::IsNumeric(item))
		{
			// concat the next tokens, till the token has been changed
			k = i + 1;
			len = item->Length;

			while (k < tokens->Count && len == item->Length)
			{
				itemk = tokens[k];

				item += itemk;
				tokens[i] = item;

				tokens->RemoveAt(k);
			}
		}

		i++;
	}

	// return tokens;
}



void Scanner::ConcatEscapeSequences(StringCell^ tokens)
{
	int i = 0;
	SysStr^ item;


	while (i < tokens->Count)
	{
		item = tokens[i];


		if (item->EndsWith("\\"))
		{
			i++;
			if (i == tokens->Count)
				break;

			item = tokens[i];

			tokens[i - 1] += item;
			tokens->RemoveAt(i);
			i--;
		}
		i++;
	}

	// return tokens;
}



void Scanner::BalanceParenthesis(StringCell^ tokens)
{
	int i = 0, n, cnt;
	SysStr^ item;


	while (i < tokens->Count)
	{
		item = tokens[i];


		if (item != "(")
		{
			i++;
			continue;
		}

		cnt = 1;

		for (n = i + 1; n < tokens->Count; n++)
		{
			item = tokens[n];

			if (item == "(")
				cnt++;

			if (item == ")")
				cnt--;

			tokens[i] += item;

			tokens->RemoveAt(n);
			n--;

			if (cnt == 0)
			{
				n++;
				break;
			}
		}

		i = n;
	}

	// return tokens;
}



StringCell^ Scanner::Split(SysStr^ sql)
{
	if (SysStr::IsNullOrEmpty(sql))
	{
		System::ArgumentNullException^ ex = gcnew System::ArgumentNullException("Argument 'sql' is null or empty");
		Diag::Ex(ex);
		throw ex;
	}


	System::Collections::ICollection^ statements = RegexLexer::SplitSql(sql);

	StringCell^ tokens = gcnew StringCell(statements, 0, true);

	ConcatComments(tokens);
	ConcatEscapeSequences(tokens);
	BalanceBackticks(tokens);
	ConcatColReferences(tokens);
	BalanceParenthesis(tokens);
	ConcatUserDefinedVariables(tokens);
	ConcatScientificNotations(tokens);
	ConcatNegativeNumbers(tokens);

	return tokens;
}



}