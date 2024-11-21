#include "pch.h"
#include "ParserManager.h"

#include "GramConsts.h"
#include "BracketParser.h"
#include "CreateParser.h"
#include "TableParser.h"
#include "IndexParser.h"
#include "ExplainParser.h"
#include "DescribeParser.h"
#include "DescParser.h"
#include "SelectParser.h"
#include "FromParser.h"
#include "UsingParser.h"
#include "UpdateParser.h"
#include "GroupByParser.h"
#include "OrderByParser.h"
#include "LimitParser.h"
#include "WhereParser.h"
#include "HavingParser.h"
#include "SetParser.h"
#include "DuplicateParser.h"
#include "InsertParser.h"
#include "ReplaceParser.h"
#include "DeleteParser.h"
#include "ValuesParser.h"
#include "IntoParser.h"
#include "DropParser.h"
#include "RenameParser.h"
#include "ShowParser.h"
#include "OptionsParser.h"
#include "WithParser.h"






namespace BlackbirdDsl {



IParser^ ParserManager::GetParser(SysStr^ parserType, EnParserOptions options)
{
	int tag = DslParsers::Tag[parserType];

	switch (tag)
	{
	case DslParsers::BRACKET:
		return gcnew BracketParser(options);
	case DslParsers::CREATE:
		return gcnew CreateParser(options);
	case DslParsers::TABLE:
		return gcnew TableParser(options);
	case DslParsers::INDEX:
		return gcnew IndexParser(options);
	case DslParsers::EXPLAIN:
		return gcnew ExplainParser(options);
	case DslParsers::DESCRIBE:
		return gcnew DescribeParser(options);
	case DslParsers::DESC:
		return gcnew DescParser(options);
	case DslParsers::SELECT:
		return gcnew SelectParser(options);
	case DslParsers::FROM:
		return gcnew FromParser(options);
	case DslParsers::USING:
		return gcnew UsingParser(options);
	case DslParsers::UPDATE:
		return gcnew UpdateParser(options);
	case DslParsers::GROUP:
		return gcnew GroupByParser(options);
	case DslParsers::ORDER:
		return gcnew OrderByParser(options);
	case DslParsers::LIMIT:
		return gcnew LimitParser(options);
	case DslParsers::WHERE:
		return gcnew WhereParser(options);
	case DslParsers::HAVING:
		return gcnew HavingParser(options);
	case DslParsers::SET:
		return gcnew SetParser(options);
	case DslParsers::DUPLICATE:
		return gcnew DuplicateParser(options);
	case DslParsers::INSERT:
		return gcnew InsertParser(options);
	case DslParsers::REPLACE:
		return gcnew ReplaceParser(options);
	case DslParsers::DELETE_:
		return gcnew DeleteParser(options);
	case DslParsers::VALUES:
		return gcnew ValuesParser(options);
	case DslParsers::INTO:
		return gcnew IntoParser(options);
	case DslParsers::DROP:
		return gcnew DropParser(options);
	case DslParsers::RENAME:
		return gcnew RenameParser(options);
	case DslParsers::SHOW:
		return gcnew ShowParser(options);
	case DslParsers::OPTIONS:
		return gcnew OptionsParser(options);
	case DslParsers::WITH:
		return gcnew WithParser(options);
	default:
		return nullptr;
	}

}


StringCell^ ParserManager::Parse(StringCell^ root, EnParserOptions options)
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