#pragma once
#include "pch.h"
#include "StringCell.h"


using namespace System::Collections::Generic;
using namespace C5;



namespace BlackbirdDsl {


ref class Scanner  abstract sealed
{

protected:

	static bool IsBackTick(SysStr^ item)
	{
		return (item == "'" || item == "\"" || item == "`");
	}



	static SysStr^ ArrayPop(List<SysStr^>^ items)
	{

		if (items->Count == 0)
			return nullptr;

		SysStr^ retval = items[items->Count - 1];

		items->RemoveAt(items->Count - 1);

		return retval;
	}



	static void ConcatNegativeNumbers(StringCell^ tokens);

	static void ConcatScientificNotations(StringCell^ tokens);

	static void ConcatUserDefinedVariables(StringCell^ tokens);

	static void ConcatComments(StringCell^ tokens);

	static void BalanceBackticks(StringCell^ tokens);

	// backticks are not balanced within one token, so we have
	// to re-combine some tokens
	static int BalanceCharacter(StringCell^ tokens, int idx, SysStr^ str);

	/**
	 * This function concats some tokens to a column reference.
	 * There are two different cases:
	 *
	 * 1. If the current token ends with a dot, we will add the next token
	 * 2. If the next token starts with a dot, we will add it to the previous token
	 *
	 */
	static void ConcatColReferences(StringCell^ tokens);

	static void ConcatEscapeSequences(StringCell^ tokens);

	static void BalanceParenthesis(StringCell^ tokens);



public:

	static StringCell^ Split(SysStr^ sql);


};

}