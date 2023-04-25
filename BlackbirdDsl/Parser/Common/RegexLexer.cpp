#include "pch.h"
#include "RegexLexer.h"


#using <system.dll>
using namespace System::Text::RegularExpressions;




namespace BlackbirdDsl {


bool RegexLexer::SelectIsMatch(SysStr^ sql)
{
	if (_RegexSelectIsMatch == nullptr)
		_RegexSelectIsMatch = gcnew Regex("^\\(\\s*select\\s*", RegexOptions::Compiled | RegexOptions::Multiline | RegexOptions::IgnoreCase);


	Regex^ regex = (Regex^)_RegexSelectIsMatch;

	return regex->IsMatch(sql);
}


bool RegexLexer::SubQueryIsMatch(SysStr^ sql)
{
	if (_RegexSubQueryIsMatch == nullptr)
		_RegexSubQueryIsMatch = gcnew Regex("^\\(\\s*(-- [\\w\\s]+\\n)?\\s*SELECT", RegexOptions::Compiled | RegexOptions::Multiline | RegexOptions::IgnoreCase);


	Regex^ regex = (Regex^)_RegexSubQueryIsMatch;

	return regex->IsMatch(sql);
}


bool RegexLexer::SubTreeIsMatch(SysStr^ sql)
{
	if (_RegexSubTreeIsMatch == nullptr)
		_RegexSubTreeIsMatch = gcnew Regex("^\\s*(-- [\\w\\s]+\\n)?\\s*SELECT", RegexOptions::Compiled | RegexOptions::Multiline | RegexOptions::IgnoreCase);


	Regex^ regex = (Regex^)_RegexSubTreeIsMatch;

	return regex->IsMatch(sql);
}



System::Collections::ICollection^ RegexLexer::SplitSql(SysStr^ sql)
{
	if (_RegexSplitSql == nullptr)
		_RegexSplitSql = gcnew Regex("(\\<\\=\\>|\\r\\n|\\!\\=|\\>\\=|\\<\\=|\\<\\>|\\<\\<|\\>\\>|\\:\\=|\\\\|&&|\\|\\||\\:\\="
			+ "|/\\*|\\*/|\\-\\-|\\>|\\<|\\||\\=|\\^|\\(|\\)|\\t|\\n|'|\"|`|,|@|\\s|\\+|\\-|\\*|/|;)", RegexOptions::Compiled | RegexOptions::Multiline);

	Regex^ regex = (Regex^)_RegexSplitSql;

	return regex->Split(sql);

}

}