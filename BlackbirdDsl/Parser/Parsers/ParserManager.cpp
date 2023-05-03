#include "pch.h"
#include "ParserManager.h"

#include "GramConsts.h"
#include "TBracketParser.h"
#include "TCreateParser.h"
#include "TTableParser.h"
#include "TIndexParser.h"
#include "TExplainParser.h"
#include "TDescribeParser.h"
#include "TDescParser.h"
#include "TSelectParser.h"
#include "TFromParser.h"
#include "TUsingParser.h"
#include "TUpdateParser.h"
#include "TGroupByParser.h"
#include "TOrderByParser.h"
#include "TLimitParser.h"
#include "TWhereParser.h"
#include "THavingParser.h"
#include "TSetParser.h"
#include "TDuplicateParser.h"
#include "TInsertParser.h"
#include "TReplaceParser.h"
#include "TDeleteParser.h"
#include "TValuesParser.h"
#include "TIntoParser.h"
#include "TDropParser.h"
#include "TRenameParser.h"
#include "TShowParser.h"
#include "TOptionsParser.h"
#include "TWithParser.h"






namespace BlackbirdDsl {



IParser^ ParserManager::GetParser(SysStr^ parserType, DslOptions options)
{
	int tag = DslParsers::Tag[parserType];

	switch (tag)
	{
	case DslParsers::BRACKET:
		return gcnew TBracketParser(options);
	case DslParsers::CREATE:
		return gcnew TCreateParser(options);
	case DslParsers::TABLE:
		return gcnew TTableParser(options);
	case DslParsers::INDEX:
		return gcnew TIndexParser(options);
	case DslParsers::EXPLAIN:
		return gcnew TExplainParser(options);
	case DslParsers::DESCRIBE:
		return gcnew TDescribeParser(options);
	case DslParsers::DESC:
		return gcnew TDescParser(options);
	case DslParsers::SELECT:
		return gcnew TSelectParser(options);
	case DslParsers::FROM:
		return gcnew TFromParser(options);
	case DslParsers::USING:
		return gcnew TUsingParser(options);
	case DslParsers::UPDATE:
		return gcnew TUpdateParser(options);
	case DslParsers::GROUP:
		return gcnew TGroupByParser(options);
	case DslParsers::ORDER:
		return gcnew TOrderByParser(options);
	case DslParsers::LIMIT:
		return gcnew TLimitParser(options);
	case DslParsers::WHERE:
		return gcnew TWhereParser(options);
	case DslParsers::HAVING:
		return gcnew THavingParser(options);
	case DslParsers::SET:
		return gcnew TSetParser(options);
	case DslParsers::DUPLICATE:
		return gcnew TDuplicateParser(options);
	case DslParsers::INSERT:
		return gcnew TInsertParser(options);
	case DslParsers::REPLACE:
		return gcnew TReplaceParser(options);
	case DslParsers::DELETE_:
		return gcnew TDeleteParser(options);
	case DslParsers::VALUES:
		return gcnew TValuesParser(options);
	case DslParsers::INTO:
		return gcnew TIntoParser(options);
	case DslParsers::DROP:
		return gcnew TDropParser(options);
	case DslParsers::RENAME:
		return gcnew TRenameParser(options);
	case DslParsers::SHOW:
		return gcnew TShowParser(options);
	case DslParsers::OPTIONS:
		return gcnew TOptionsParser(options);
	case DslParsers::WITH:
		return gcnew TWithParser(options);
	default:
		return nullptr;
	}

}


StringCell^ ParserManager::Parse(StringCell^ root, DslOptions options)
{
	return root;
	if (IsNullPtr(root))
		return root;

	for each (SysStr ^ key in DslParsers::Keys)
	{
		if (!root->IsUnpopulated[key])
			root = GetParser(key, options)->Parse(root);
	}

	return root;
}


}