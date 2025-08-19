#include "pch.h"
#include "AbstractParser.h"

#include "EnParserOptions.h"
#include "Scanner.h"

namespace BlackbirdDsl {



StringCell^ AbstractParser::Execute(SysStr^ sql)
{
	_Sql = sql;

	StringCell^ tokens = Scanner::Split(sql);

	return Execute(tokens);

}

StringCell^ AbstractParser::Execute(StringCell^ node)
{
	if (node->Count == 0)
		return NullCell;

	StringCell^ root;

	if (_Key != nullptr)
		root = gcnew StringCell(_Key, node);
	else
		root = node;


	StringCell^ output = Parse(root);

	if (_Key != nullptr)
		return output[_Key];

	return output;
}


/*
 * Revokes the quoting characters from an expression
 * Possibilities:
 *   `a`
 *   'a'
 *   "a"
 *   `a`.`b`
 *   `a.b`
 *   a.`b`
 *   `a`.b
 * It is also possible to have escaped quoting characters
 * within an expression part:
 *   `a``b` => a`b
 * And you can use whitespace between the parts:
 *   a  .  `b` => [a,b]
 */
StringCell^ AbstractParser::ExtractQuotesPairs(SysStr^ sql)
{
	sql = sql->Trim();

	XCHAR quote = '\0';
	XCHAR chr;

	int start = 0;
	int i = 0;
	int len = sql->Length;

	SysStr^ sub;

	System::IntPtr intptr = Marshal::StringToBSTR(sql);
	PXSTR str = (PXSTR)intptr.ToPointer();


	StringCell^ nodes = gcnew StringCell();


	while (i < len) {

		chr = str[i];


		switch (chr)
		{
			case '`':
			case '"':
			case '\'':
				if (quote == '\0')
				{
					// start
					quote = chr;
					start = i + 1;
					break;
				}

				if (chr != quote)
					break;

				if (i + 1 < len && quote == str[i + 1])
				{
					// escaped
					i++;
					break;
				}
				// end

				sub = sql->Substring(start, i - start);

				switch (quote)
				{
				case '`':
					nodes += sub->Replace(Quote::GRAVES, Quote::GRAVE);
					break;
				case '\'':
					nodes += sub->Replace(Quote::SINGLES, Quote::SINGLE);
					break;
				default:
					nodes += sub->Replace(Quote::DOUBLES, Quote::DOUBLE);
					break;
				}

				start = i + 1;
				quote = '\0';

				break;

			case '.':
				if (quote == '\0')
				{
					// we have found a separator
					if (i > start)
					{
						sub = sql->Substring(start, i - start)->Trim();
						if (sub != "")
							nodes += sub;
					}
					start = i + 1;
				}

				break;

			default:
				// ignore
				break;
		}

		i++;
	}

	delete str;

	if (quote == '\0' && start < len && i > start)
	{
		sub = sql->Substring(start, i - start)->Trim();
		if (sub != "")
			nodes += sub;
	}

	StringCell^ retnode = gcnew StringCell();

	retnode["delim"] = nodes->Count == 1 ? gcnew StringCell() : gcnew StringCell(".");
	retnode["parts"] = nodes;

	return retnode;
}



/**
 * This method removes parenthesis from start of the given string.
 * It removes also the associated closing parenthesis.
 */
SysStr^ AbstractParser::RemoveParenthesis(SysStr^ expression)
{
	bool changed = false;
	int i = 0;
	int start = 0;
	int parenthesis;
	int parenthesisRemoved = 0;

	expression = expression->Trim();
	int len = expression->Length;

	if (len == 0)
		return "";


	System::IntPtr intptr = Marshal::StringToBSTR(expression);
	PXSTR str = (PXSTR)intptr.ToPointer();


	if (str[0] == '(')
	{
		// remove only one parenthesis pair now!
		i++;
		start++;
		changed = true;
		parenthesisRemoved++;
	}


	parenthesis = parenthesisRemoved;

	// Whether a string was opened or not, and with which character it was open (' or ")
	XCHAR stringOpened = '\0';

	while (i < len)
	{

		if (str[i] == '\\')
		{
			i += 2; // an escape character, the next character is irrelevant
			continue;
		}

		if (str[i] == '\'')
		{
			if (stringOpened == '\0')
			{
				stringOpened = '\'';
			}
			else if (stringOpened == '\'')
			{
				stringOpened = '\0';
			}
		}

		if (str[i] == '"')
		{
			if (stringOpened == '\0')
			{
				stringOpened = '"';
			}
			else if (stringOpened == '"')
			{
				stringOpened = '\0';
			}
		}

		if (stringOpened == '\0' && str[i] == '(')
		{
			parenthesis++;
		}

		if (stringOpened == '\0' && str[i] == ')')
		{
			if (parenthesis == parenthesisRemoved)
			{
				str[i] = ' ';
				changed = true;
				parenthesisRemoved--;
			}

			parenthesis--;
		}

		i++;
	}


	if (changed)
	{
		while (start < len && str[start] == ' ')
			start++;

		while (len > 0 && str[len - 1] == ' ')
		{
			str[len - 1] = '\0';
			len--;
		}

		expression = gcnew SysStr(str + start);
	}


	return expression;

}



SysStr^ AbstractParser::GetVariableType(SysStr^ expression)
{
	// $expression must contain only upper-case characters
	if (expression->Substring(1, 1) != "@")
	{
		return Expressions::USER_VARIABLE;
	}

	SysStr^ type = expression->Substring(2, expression->IndexOf(L'.', 2));

	if (type == "GLOBAL")
		type = Expressions::GLOBAL_VARIABLE;
	else if (type == "LOCAL")
		type = Expressions::LOCAL_VARIABLE;
	else
		type = Expressions::SESSION_VARIABLE;

	return type;
}




StringCell^ AbstractParser::CreateCommentToken(SysStr^ expression)
{
	StringCell^ retnode = NullCell;

	retnode["expr_type"] = Expressions::COMMENT;
	retnode["value"] = expression;

	return retnode;
}

/**
 * translates an array of objects into an associative array - ??? ??? this is indexed not an associative
 */
 /* Not sure about this
 public function toArray($tokenList) {
	 $expr = array();
	 foreach($tokenList as $token) {
		 if ($token instanceof \PHPSQLParser\utils\ExpressionToken) {
			 $expr[] = $token->toArray();
		 }
		 else {
			 $expr[] = $token;
		 }
	 }
	 return $expr;
 }
 */


 /*
 protected function array_insert_after($array, $key, $entry) {
	 $idx = array_search($key, array_keys($array));
	 $array = array_slice($array, 0, $idx + 1, true) + $entry
		 + array_slice($array, $idx + 1, count($array) - 1, true);
	 return $array;
 }
 */


}