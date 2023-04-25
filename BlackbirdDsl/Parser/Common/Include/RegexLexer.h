#pragma once
#include "pch.h"
#include "CPentaCommon.h"


namespace BlackbirdDsl {


ref class RegexLexer
{
protected:
	static SysObj^ _RegexSelectIsMatch = nullptr;
	static SysObj^ _RegexSubQueryIsMatch = nullptr;
	static SysObj^ _RegexSubTreeIsMatch = nullptr;
	static SysObj^ _RegexSplitSql = nullptr;

public:

	static bool SelectIsMatch(SysStr^ sql);

	static bool SubQueryIsMatch(SysStr^ sql);

	static bool SubTreeIsMatch(SysStr^ sql);

	static System::Collections::ICollection^ SplitSql(SysStr^ sql);

};


}