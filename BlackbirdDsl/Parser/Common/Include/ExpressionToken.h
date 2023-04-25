#pragma once
#include "pch.h"
#include "StringCell.h"
#include "DslOptions.h"
#include "GramConsts.h"
#include "RegexLexer.h"
#include "AbstractParser.h"

using namespace C5;


#define TokenPair KeyValuePair<System::String^, ExpressionToken^>
#define TokenPairs gcnew array<KeyValuePair<System::String^, ExpressionToken^>>
#define NullToken gcnew ExpressionToken()


namespace BlackbirdDsl {


public ref class ExpressionToken : public StringCell
{

public:

	property bool IsWhitespaceToken
	{
		bool get() { return (TrimToken == ""); }
	};

	property bool IsCommaToken
	{
		bool get() { return (TrimToken == ","); }
	};

	property bool IsVariableToken
	{
		bool get() { return !IsNullPtr(UpperToken) && UpperToken->Value[0] == '@'; }
	};

	property bool IsSubQueryToken
	{
		bool get() { return RegexLexer::SubQueryIsMatch(TrimToken); }
	};

	property bool IsExpression
	{
		bool get() { return TokenType == Expressions::EXPRESSION; }
	};

	property bool IsBracketExpression
	{
		bool get() { return TokenType == Expressions::BRACKET_EXPRESSION; }
	};

	property bool IsOperator
	{
		bool get() { return TokenType == Expressions::OPERATOR; }
	};

	property bool IsInList
	{
		bool get() { return TokenType == Expressions::IN_LIST; }
	};

	property bool IsFunction
	{
		bool get() { return TokenType == Expressions::SIMPLE_FUNCTION; }
	};

	property bool IsUnspecified
	{
		bool get() { return (IsNullPtr(TokenType) || TokenType->IsNull); }
	};

	property bool IsVariable
	{
		bool get()
		{
			return TokenType == Expressions::GLOBAL_VARIABLE || TokenType == Expressions::LOCAL_VARIABLE
				|| TokenType == Expressions::USER_VARIABLE;
		}
	};

	property bool IsAggregateFunction
	{
		bool get() { return TokenType == Expressions::AGGREGATE_FUNCTION; }
	};

	property bool IsCustomFunction
	{
		bool get() { return TokenType == Expressions::CUSTOM_FUNCTION; }
	};

	property bool IsColumnReference
	{
		bool get() { return TokenType == Expressions::COLREF; }
	};

	property bool IsConstant
	{
		bool get() { return TokenType == Expressions::CONSTANT; }
	};

	property bool IsSign
	{
		bool get() { return TokenType == Expressions::SIGN; }
	};

	property bool IsSubQuery
	{
		bool get() { return TokenType == Expressions::SUBQUERY; }
	};

	property bool IsEnclosedWithinParenthesis
	{
		bool get()
		{
			return (!SysStr::IsNullOrEmpty(UpperToken) && ((SysStr^)UpperToken)->StartsWith("(") && ((SysStr^)UpperToken)->EndsWith(")"));
		}
	};


	property StringCell^ KeyToken
	{
		StringCell^ get()
		{
			return StringCell::default["_KeyToken_"];
		}
		void set(StringCell^ value)
		{
			StringCell::default["_KeyToken_"] = value;
		}
	};


	property StringCell^ KeyToken[int]
	{
		StringCell ^ get(int index)
		{
			return StringCell::default["_KeyToken_", index];
		}
		void set(int index, StringCell ^ value)
		{
			StringCell::default["_KeyToken_", index] = value;
		}
	};


	property StringCell^ Expression
	{
		StringCell^ get()
		{
			return StringCell::default["_Expression_"];
		}
		void set(StringCell^ value)
		{
			StringCell::default["_Expression_"] = value;
		}
	};


	property StringCell^ Expression[int]
	{
		StringCell ^ get(int index)
		{
			return StringCell::default["_Expression_", index];
		}
		void set(int index, StringCell ^ value)
		{
			StringCell::default["_Expression_", index] = value;
		}
	};


	property StringCell^ SubTree
	{
		StringCell^ get()
		{
			return StringCell::default["_SubTree_"];
		}
		void set(StringCell^ value)
		{
			StringCell::default["_SubTree_"] = value;
		}
	};


	property StringCell^ SubTree[int]
	{
		StringCell ^ get(int index)
		{
			return StringCell::default["_SubTree_", index];
		}
		void set(int index, StringCell ^ value)
		{
			StringCell::default["_SubTree_", index] = value;
		}
	};



	property StringCell^ UpperToken
	{
		StringCell^ get()
		{
			return StringCell::default["_UpperToken_"];
		}
		void set(StringCell^ value)
		{
			StringCell::default["_UpperToken_"] = value;
		}
	};


	property StringCell^ UpperToken[int]
	{
		StringCell ^ get(int index)
		{
			return StringCell::default["_UpperToken_", index];
		}
		void set(int index, StringCell ^ value)
		{
			StringCell::default["_UpperToken_", index] = value;
		}
	};


	property StringCell^ TrimToken
	{
		StringCell ^ get()
		{
			return StringCell::default["_TrimToken_"];
		}
		void set(StringCell ^ value)
		{
			StringCell::default["_TrimToken_"] = value;
		}
	};


	property StringCell^ TrimToken[int]
	{
		StringCell ^ get(int index)
		{
			return StringCell::default["_TrimToken_", index];
		}
		void set(int index, StringCell ^ value)
		{
			StringCell::default["_TrimToken_", index] = value;
		}
	};

	property StringCell^ Token
	{
		StringCell^ get()
		{
			return StringCell::default["_Token_"];
		}
		void set(StringCell^ value)
		{
			StringCell::default["_Token_"] = value;
		}
	};


	property StringCell^ Token[int]
	{
		StringCell ^ get(int index)
		{
			return StringCell::default["_Token_", index];
		}
		void set(int index, StringCell ^ value)
		{
			StringCell::default["_Token_", index] = value;
		}
	};

	property StringCell^ TokenType
	{
		StringCell^ get()
		{
			return StringCell::default["_TokenType_"];
		}
		void set(StringCell^ value)
		{
			StringCell::default["_TokenType_"] = value;
		}
	};


	property StringCell^ TokenType[int]
	{
		StringCell ^ get(int index)
		{
			return StringCell::default["_TokenType_", index];
		}
		void set(int index, StringCell ^ value)
		{
			StringCell::default["_TokenType_", index] = value;
		}
	};



	property StringCell^ NoQuotes
	{
		StringCell^ get()
		{
			return StringCell::default["_NoQuotes_"];
		}
		void set(StringCell^ value)
		{
			if (!IsNullPtr(value) && !value->IsNull)
				Remove("_NoQuotes_");
			else
				StringCell::default["_NoQuotes_"] = AbstractParser::ExtractQuotesPairs(value);
		}
	};


	property StringCell^ NoQuotes[int]
	{
		StringCell ^ get(int index)
		{
			return StringCell::default["_NoQuotes_", index];
		}
		void set(int index, StringCell ^ value)
		{
			if (IsNullPtr(value) || value->IsNull)
			{
				if (!IsNullPtr(NoQuotes[index]))
				{
					NoQuotes->RemoveAt(index);
					if (NoQuotes->Count == 0)
						Remove("_NoQuotes_");
				}
				return;
			}
			StringCell::default["_NoQuotes_", index] = AbstractParser::ExtractQuotesPairs(value);
		}
	};


	static operator ExpressionToken^ (PCYSTR rhs)
	{
		ExpressionToken^ cell = gcnew ExpressionToken();
		cell->LocalString = (gcnew SysStr(rhs));
		return cell;
	};

	static operator ExpressionToken^ (const StringCell^ rhs)
	{
		ExpressionToken^ cell = gcnew ExpressionToken();

		((StringCell^)rhs)->Clone(cell);
		return cell;
	};



	static bool operator==(ExpressionToken^ lhs, bool rhs)
	{
		return StringCell::operator==(lhs, rhs);
	};
	static bool operator==(bool lhs, ExpressionToken^ rhs)
	{
		return StringCell::operator==(lhs, rhs);
	};



private:

	// We want control over what may be instantiated with this class

	ExpressionToken(int capacity) : StringCell(capacity) {};

	ExpressionToken(int capacity, StringCell^ element) : StringCell(capacity, element) {};

	ExpressionToken(ICollection<StringCell^>^ collection) : StringCell(collection) {};

	ExpressionToken(ICollection<SysStr^>^ collection) : StringCell(collection) {};

	ExpressionToken(System::Collections::ICollection^ collection, int start, bool excludeNullOrEmpty)
		: StringCell(collection, start, excludeNullOrEmpty) {};

	ExpressionToken(ICollection<KeyValuePair<SysStr^, StringCell^>>^ collection) : StringCell(collection) {};

	ExpressionToken(SysStr^ value) : StringCell(value) {};

	ExpressionToken(System::Int32^ value) : StringCell(value) {};

	ExpressionToken(System::Double^ value) : StringCell(value) {};

	ExpressionToken(PCXSTR value) : StringCell(value) {};

	ExpressionToken(PCYSTR value) : StringCell(value) {};

	ExpressionToken(SysObj^ value) : StringCell(value) {};


public:

	ExpressionToken() : StringCell()
	{
	};

	ExpressionToken(SysStr^ key, StringCell^ token) : StringCell(key, token)
	{
	}

	ExpressionToken(StringCell^ token, SysStr ^ key) : StringCell()
	{
		Expression = "";
		KeyToken = key;
		TrimToken = token->Trim();
		UpperToken = token->Trim()->ToUpper();

		Token = token;
		TokenType = NullCell;
		NoQuotes = NullCell;
		SubTree = NullCell;
	};

	ExpressionToken(ExpressionToken^ token, SysStr^ key) : ExpressionToken((StringCell^)token, key)
	{
	}

	ExpressionToken(StringCell^ token) : StringCell()
	{
		Expression = "";
		KeyToken = 0;
		TrimToken = token->Trim();
		UpperToken = token->Trim()->ToUpper();

		Token = token;
		TokenType = NullCell;
		NoQuotes = NullCell;
		SubTree = NullCell;
	};

	ExpressionToken(ExpressionToken^ token) : ExpressionToken((StringCell^)token)
	{
	}

	/*
	ExpressionToken(ICollection<KeyValuePair<SysStr^, StringCell^>>^ collection) : StringCell(collection)
	{
	};
	*/



	void AddToken(StringCell^ value);

	bool EndsWith(SysStr^ needle);

	StringCell^ ToCellPairs();



};

}