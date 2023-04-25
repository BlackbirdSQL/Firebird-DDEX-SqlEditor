#pragma once
#include "pch.h"
#include "CPentaCommon.h"
#include "GramConsts.h"


using namespace System::Collections::Generic;



namespace BlackbirdDsl {


/// <summary>
/// FlagsTokenCategory
/// </summary>
[System::Flags]
enum struct FlagsTokenCategory
{
	UNRESERVED = 0,
	RESERVED = 1,
	SEPARATOR = 2,
	OPERATOR = 4,
	BLOCK = 8,
	COLNAME_KEYWORD = 16,
	FUNCTION = 32 | RESERVED,
	PARAM_FUNCTION = 64 | FUNCTION,
	AGGR_FUNCTION = 128 | PARAM_FUNCTION,
	CUSTOM_FUNCTION = 256 | PARAM_FUNCTION
};


ref class Gram abstract sealed
{

public:


	/// <summary>
	/// PG_KEYWORD [scan.c 5517]
	/// </summary>

	ref struct Token
	{
	public:
		int Value;
		FlagsTokenCategory Category;
		bool IsBareLabel;
		int Index;

		Token(int value, FlagsTokenCategory category, bool isBareLabel, int index)
		{
			Value = value;
			Category = category;
			IsBareLabel = isBareLabel;
			Index = index;
		}
	};


private:



	static List<SysStr^>^ _BackTrackingTypes = nullptr;

	/// <summary>
	/// _Tokens
	/// </summary>
	static IDictionary<SysStr^, Token^>^ _Tokens = nullptr;

public:

	static const bool BARE_LABEL = true;
	static const bool AS_LABEL = false;


	static SysStr^ OperatorSymbols = "\t\n\r ,()_'\"?@0123456789";
	static SysStr^ InlineSymbols = "\t\n\r ,()<>*+-/|&=!;";



	static property List<SysStr^>^ BackTrackingTypes
	{
		List<SysStr^>^ get();
	}

	static property IDictionary<SysStr^, Token^>^ Tokens
	{
		IDictionary<SysStr^, Token^>^ get();
	}


	static bool IsReserved(SysStr^ key)
	{
		Token^ token;

		if (!Tokens->TryGetValue(key, token))
			return false;

		return (((int)token->Category & (int)FlagsTokenCategory::RESERVED) > 0);
	}

	static bool IsFunction(SysStr^ key)
	{
		Token^ token;

		if (!Tokens->TryGetValue(key, token))
			return false;

		return (((int)token->Category & (int)FlagsTokenCategory::FUNCTION) > 0);
	}

	static bool IsParameterizedFunction(SysStr^ key)
	{
		Token^ token;

		if (!Tokens->TryGetValue(key, token))
			return false;

		return (((int)token->Category & (int)FlagsTokenCategory::PARAM_FUNCTION) > 0);
	}

	static bool IsAggregateFunction(SysStr^ key)
	{
		Token^ token;

		if (!Tokens->TryGetValue(key, token))
			return false;

		return (((int)token->Category & (int)FlagsTokenCategory::AGGR_FUNCTION) > 0);
	}

	static bool IsCustomFunction(SysStr^ key)
	{
		Token^ token;

		if (!Tokens->TryGetValue(key, token))
			return false;

		return (((int)token->Category & (int)FlagsTokenCategory::CUSTOM_FUNCTION) > 0);
	}



};

}