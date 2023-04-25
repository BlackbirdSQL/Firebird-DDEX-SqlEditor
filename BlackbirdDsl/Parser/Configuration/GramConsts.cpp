#include "pch.h"
#include "GramConsts.h"


namespace BlackbirdDsl {

void DslParsers::Initialize()
{
	int i = 0;
	_Tags = gcnew Dictionary<SysStr^, int>;

	_Tags->Add("BRACKET", BRACKET);
	_Tags->Add("CREATE", CREATE);
	_Tags->Add("TABLE", TABLE);
	_Tags->Add("INDEX", INDEX);
	_Tags->Add("EXPLAIN", EXPLAIN);
	_Tags->Add("DESCRIBE", DESCRIBE);
	_Tags->Add("DESC", DESC);
	_Tags->Add("SELECT", SELECT);
	_Tags->Add("FROM", FROM);
	_Tags->Add("USING", USING);
	_Tags->Add("UPDATE", UPDATE);
	_Tags->Add("GROUP", GROUP);
	_Tags->Add("ORDER", ORDER);
	_Tags->Add("LIMIT", LIMIT);
	_Tags->Add("WHERE", WHERE);
	_Tags->Add("HAVING", HAVING);
	_Tags->Add("SET", SET);
	_Tags->Add("DUPLICATE", DUPLICATE);
	_Tags->Add("INSERT", INSERT);
	_Tags->Add("REPLACE", REPLACE);
	_Tags->Add("DELETE", DELETE_);
	_Tags->Add("VALUES", VALUES);
	_Tags->Add("INTO", INTO);
	_Tags->Add("DROP", DROP);
	_Tags->Add("RENAME", RENAME);
	_Tags->Add("SHOW", SHOW);
	_Tags->Add("OPTIONS", OPTIONS);
	_Tags->Add("WITH", WITH);
}

}