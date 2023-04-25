#pragma once
#include "pch.h"



namespace BlackbirdDsl {


/// <summary>
/// FlagsOptions
/// </summary>
[System::Flags]
public enum struct FlagsOptions
{
	NONE = 0,
	CONSISTENT_SUBTREES = 1,
	ANSI_QUOTES = 2,
	OFFSET_CAPTURE = 4
};


ref class DslOptions abstract sealed
{

public:

	static bool ConsistentSubtrees(FlagsOptions options)
	{
		return ((int)options & (int)FlagsOptions::CONSISTENT_SUBTREES) > 0;
	}

	static bool AnsiQuotes(FlagsOptions options)
	{
		return ((int)options & (int)FlagsOptions::ANSI_QUOTES) > 0;
	}

	static bool OffsetCapture(FlagsOptions options)
	{
		return ((int)options & (int)FlagsOptions::OFFSET_CAPTURE) > 0;
	}

};

}