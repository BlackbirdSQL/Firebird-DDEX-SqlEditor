#pragma once
#include "pch.h"
#include "IParser.h"
#include "GramConsts.h"
#include "EnParserOptions.h"


using namespace C5;




namespace BlackbirdDsl {


#define ParserInstance(__PARSER__) (gcnew __PARSER__(Options))



public ref class AbstractParser abstract : public IParser
{

protected:

	EnParserOptions _Options;

	SysStr^ _Sql;
	SysStr^ _Key = nullptr;


public:

	property bool ConsistentSubtrees
	{
		bool get() { return ((_Options & EnParserOptions::CONSISTENT_SUBTREES) != EnParserOptions::NONE); }
	};

	property bool AnsiQuotes
	{
		bool get() { return ((_Options & EnParserOptions::ANSI_QUOTES) != EnParserOptions::NONE); }
	};

	property bool OffsetCapture
	{
		bool get() { return ((_Options & EnParserOptions::OFFSET_CAPTURE) != EnParserOptions::NONE); }
	};


	property bool TokenizeOnly
	{
		bool get() { return ((_Options & EnParserOptions::TOKENIZE_ONLY) != EnParserOptions::NONE); }
	};




public:

	property SysStr^ Sql
	{
		SysStr^ get() { return _Sql; }
		void set(SysStr^ value) { _Sql = value; }
	};

	property EnParserOptions Options
	{
		EnParserOptions get() { return _Options; }
	};

	static StringCell^ ExtractQuotesPairs(SysStr^ sql);



	AbstractParser()
	{
		_Options = EnParserOptions::NONE;
	};

	AbstractParser(EnParserOptions options)
	{
		_Options = options;
	};


	virtual StringCell^ Execute(SysStr^ sql);

	virtual StringCell^ Execute(StringCell^ node);

	virtual StringCell^ Parse(StringCell^ root) abstract;




protected:

	SysStr^ RemoveParenthesis(SysStr^ expression);


	SysStr^ GetVariableType(SysStr^ expression);


	bool IsCommaToken(StringCell^ token)
	{
		return (token->Trimmed() == ",");
	};

	bool IsWhitespaceToken(StringCell^ token)
	{
		return (token->Trimmed() == "");
	};

	bool IsCommentToken(StringCell^ token)
	{

		return (token->StorageObject != nullptr && (token->TransientString->StartsWith("--") || token->TransientString->StartsWith("/*")));
	};

	bool IsColumnReference(StringCell^ token)
	{
		return (token["expr_type"] == Expressions::COLREF);
	};

	bool IsReserved(StringCell^ token)
	{
		return (token["expr_type"] == Expressions::RESERVED);
	};

	bool IsConstant(StringCell^ token)
	{
		return (token["expr_type"] == Expressions::CONSTANT);
	};

	bool IsAggregateFunction(StringCell^ token)
	{
		return (token["expr_type"] == Expressions::AGGREGATE_FUNCTION);
	};

	bool IsCustomFunction(StringCell^ token)
	{
		return (token["expr_type"] == Expressions::CUSTOM_FUNCTION);
	};

	bool IsFunction(StringCell^ token)
	{
		return (token["expr_type"] == Expressions::SIMPLE_FUNCTION);
	};

	bool IsExpression(StringCell^ token)
	{
		return (token["expr_type"] == Expressions::EXPRESSION);
	};

	bool IsBracketExpression(StringCell^ token)
	{
		return (token["expr_type"] == Expressions::BRACKET_EXPRESSION);
	};

	bool IsSubQuery(StringCell^ token)
	{
		return (token["expr_type"] == Expressions::SUBQUERY);
	};

	bool IsComment(StringCell^ token)
	{
		return (token["expr_type"] == Expressions::COMMENT);
	};

	bool IsUnion(StringCell^ token)
	{
		if (IsNullPtr(token) || token->Count == 0)
			return false;

		return token->ContainsKey("UNION") || token->ContainsKey("UNION ALL");
	}


	StringCell^ CreateCommentToken(SysStr^ expression);

	/* Not sure about this
	List<StringCell^>^ ToArray($tokenList)
	{
		$expr = array();
		foreach($tokenList as $token) {
			if ($token instanceof \PHPSQLParser\utils\ExpressionToken) {
				$expr[] = $token->toArray();
			}
			else {
				$expr[] = $token;
			}
		}
		return $expr;
	}
	*/

};

}