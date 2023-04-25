#pragma once
#include "pch.h"
#include "StringCell.h"

using namespace C5;


namespace BlackbirdDsl {


/// <summary>
/// This class implements the calculator for the string positions of the
/// base_expr elements within the output of the PHPSQLParser.
/// </summary>
ref class OffsetCalculator abstract sealed
{

protected:
	static bool QuotedBefore(XCHAR c);
	static bool QuotedAfter(XCHAR c);

	static void PrintOffset(SysStr^ text, SysStr^ sql, int charPos, SysStr^ key, bool parsed, SysStr^ backtracking);


public:
	static StringCell^ SetOffsetsWithinSql(SysStr^ sql, StringCell^ node);

	static int FindOffsetWithinString(SysStr^ sql, SysStr^ value, SysStr^ exprType);

	static void LookForBaseExpression(SysStr^ sql, int* charPos, StringCell^ node, SysStr^ key, List<int>^ backtrackList);


};

}