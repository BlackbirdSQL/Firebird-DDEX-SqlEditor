#pragma once
#include "pch.h"



namespace BlackbirdDsl {


/// <summary>
/// DslOptions
/// </summary>
public enum struct DslOptions
{
	NONE = 0,
	CONSISTENT_SUBTREES = 1,
	ANSI_QUOTES = 2,
	OFFSET_CAPTURE = 4,
	TOKENIZE_ONLY = 8
};



}