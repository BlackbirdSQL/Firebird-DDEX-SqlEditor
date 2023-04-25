#include "pch.h"
#include "TAliasAssembler.h"



namespace BlackbirdDsl {
namespace Assemblers {


SysStr^ TAliasAssembler::Assemble()
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