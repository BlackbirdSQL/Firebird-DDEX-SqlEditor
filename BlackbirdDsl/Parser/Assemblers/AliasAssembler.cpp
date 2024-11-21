#include "pch.h"
#include "AliasAssembler.h"



namespace BlackbirdDsl {
namespace Assemblers {


SysStr^ AliasAssembler::Assemble()
{
	if (!HasAlias)
		return "";

	SysStr^ sql = "";

	StringCell^ cell = _RootNode["alias", "as"];

	if (!IsNullPtr(cell) && !cell->IsNull)
	{
		sql += " AS";
	}

	sql += " " + _RootNode["alias", "name"];

	return sql;
}

}
}