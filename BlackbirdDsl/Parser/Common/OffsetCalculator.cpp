#include "pch.h"
#include "OffsetCalculator.h"

#include "Gram.h"
#include <cctype>


namespace BlackbirdDsl {



void OffsetCalculator::PrintOffset(SysStr^ text, SysStr^ sql, int charPos, SysStr^ key, bool parsed, SysStr^ backtracking)
{
#ifdef _DEBUG
	SysStr^ spaces = "";

	SysStr^ holdem = sql->Substring(0, charPos) + "^" + sql->Substring(charPos);


	Diag::Dug(true, text + "  Key: " + key + "  Parsed: " + parsed + "  Back: " + backtracking + " "
		+ holdem);
#else // _DEBUG
	return;
#endif // else _DEBUG
}



StringCell^ OffsetCalculator::SetOffsetsWithinSql(SysStr^ sql, StringCell^ node)
{
	int charPos = 0;
	List<int>^ backtracking = gcnew List<int>;

	LookForBaseExpression(sql, &charPos, node, nullptr, backtracking);

	return node;
}


bool OffsetCalculator::QuotedBefore(XCHAR c)
{
	static XCHAR openChars[] = L"`(\0";

	bool result = false;
	int i = 0;


	while (!result && openChars[i] != '\0')
	{
		if (c == openChars[i])
			result = true;
	}

	return result;
}

bool OffsetCalculator::QuotedAfter(XCHAR c)
{
	static XCHAR closeChars[] = L"`)\0";

	bool result = false;
	int i = 0;


	while (!result && closeChars[i] != '\0')
	{
		if (c == closeChars[i])
			result = true;
	}

	return result;
}



int OffsetCalculator::FindOffsetWithinString(SysStr^ sql, SysStr^ value, SysStr^ exprType)
{
	if (value == nullptr || value == "")
		return -1;

	bool ok = false;
	bool quotedBefore, quotedAfter;
	int offset = 0;
	int pos;
	XCHAR before, after;



	exprType = exprType->ToLower();

	while (true)
	{
		pos = sql->IndexOf(value, offset);
		// error_log("pos:$pos value:$value sql:$sql");

		if (pos == -1)
			break;

		before = '\0';

		if (pos > 0)
			before = std::tolower(sql[pos - 1]);

		// if we have a quoted string, we every character is allowed after it
		// see issues 137 and 361


		quotedBefore = QuotedBefore((XCHAR)sql[pos]);
		quotedAfter = QuotedAfter((XCHAR)sql[pos + value->Length - 1]);

		after = '\0';

		if (sql->Length > pos + value->Length)
			after = (XCHAR)sql[pos + value->Length];

		// if we have an operator, it should be surrounded by
		// whitespace, comma, parenthesis, digit or letter, end_of_string
		// an operator should not be surrounded by another operator


		if (exprType == Operators::OPERATOR || exprType == Operators::COLUMNS)
		{
			ok = false;

			if (before == '\0' || (before >= 'a' && before <= 'z') || Gram::OperatorSymbols->IndexOf(before) != -1)
			{
				if (after == '\0' || (after >= 'a' && after <= 'z') || Gram::OperatorSymbols->IndexOf(after) != -1)
				{
					ok = true;
				}
			}

			if (!ok)
			{
				offset = pos + 1;
				continue;
			}

			break;
		}

		// in all other cases we accept
		// whitespace, comma, operators, parenthesis and end_of_string

		ok = false;

		if (before == '\0' || (quotedBefore && before >= 'a' && before <= 'z')
			|| Gram::InlineSymbols->IndexOf(before) != -1)
		{
			if (after == '\0' || (quotedAfter && after >= 'a' && after <= 'z')
				|| Gram::InlineSymbols->IndexOf(after) != -1)
			{
				ok = true;
			}
		}

		if (ok)
			break;

		offset = pos + 1;
	}

	return pos;
}



void OffsetCalculator::LookForBaseExpression(SysStr^ sql, int* charPos, StringCell^ node, SysStr^ key, List<int>^ backtrackList)
{
	bool isNumeric;
	int i, offset;
	System::Double* outval;

	if (key == nullptr)
	{
		isNumeric = true;
	}
	else
	{
		outval = new System::Double(0.0);
		isNumeric = System::Double::TryParse(key, *outval);
	}


	if (!isNumeric)
	{
		if ((key == "UNION" || key == "UNION ALL")
			|| (key == "select-option" && !node->IsNullOrEmpty) || (key == "alias" && !node->IsNullOrEmpty)
			|| (key == "expr_type" && Gram::BackTrackingTypes->Contains(node)))
		{
			// we hold the current position and come back after the next base_expr
			// we do this, because the next base_expr contains the complete expression/subquery/record
			// and we have to look into it too
			backtrackList->Add(*charPos);

		}
		else if ((key == "ref_clause" || key == "columns") && !node->IsNullOrEmpty)
		{
			// we hold the current position and come back after n base_expr(s)
			// there is an array of sub-elements before (!) the base_expr clause of the current element
			// so we go through the sub-elements and must come at the end

			backtrackList->Add(*charPos);

			for (i = 1; i < node->Count; i++)
			{
				backtrackList->Add(-1); // backtracking only after n base_expr!
			}
		}
		else if ((key == "sub_tree" && !node->IsNullOrEmpty) || (key == "options" && node->IsNullOrEmpty))
		{
			// we prevent wrong backtracking on subtrees (too much array_pop())
			// there is an array of sub-elements after(!) the base_expr clause of the current element
			// so we go through the sub-elements and must not come back at the end

			for (i = 1; i < node->Count; i++)
			{
				backtrackList->Add(-1);
			}
		}
		else if ((key == "TABLE") || (key == "create-def" && !node->IsNullOrEmpty))
		{
			// do nothing
		}
		else
		{
			// move the current pos after the keyword
			// SELECT, WHERE, INSERT etc.
			if (!SysStr::IsNullOrEmpty(key))
			{
				*charPos = sql->IndexOf(key, *charPos, System::StringComparison::OrdinalIgnoreCase);
				*charPos += key->Length;
			}
		}
	}

	if (!node->IsCollection)
		return;

	int oldPos;
	SysStr^ value, ^ subject, ^ type;

	for each (ReplicaKeyPair(StringCell^) pair in node->ReplicaKeyEnumerator)
	{
		if (pair.Key.Segment == "base_expr")
		{

			//$this->printPos("0", $sql, $charPos, $key, $value, $backtracking);

			subject = sql->Substring(*charPos);
			value = pair.Value;
			type = node["expr_type"];
			if (type == nullptr)
				type = "alias";

			offset = FindOffsetWithinString(subject, value, type);

			if (offset == -1)
			{
				KeyNotFoundException^ ex = gcnew KeyNotFoundException(pair.Value + ":" + subject);
				Diag::Dug(ex);
				throw ex;
			}

			node["position"] = gcnew StringCell(*charPos + offset);

			*charPos += offset + value->Length;

			//$this->printPos("1", $sql, $charPos, $key, $value, $backtracking);

			if (backtrackList->Count > 0)
			{
				oldPos = backtrackList[backtrackList->Count - 1];
				backtrackList->RemoveAt(backtrackList->Count - 1);
			}
			else
			{
				oldPos = -1;
			}

			if (oldPos != -1)
				*charPos = oldPos;

			//$this->printPos("2", $sql, $charPos, $key, $value, $backtracking);

		}
		else
		{
			LookForBaseExpression(sql, charPos, pair.Value, pair.Key.Segment, backtrackList);
			node[pair.Key.Ordinal] = pair.Value;
		}
	}
}




}