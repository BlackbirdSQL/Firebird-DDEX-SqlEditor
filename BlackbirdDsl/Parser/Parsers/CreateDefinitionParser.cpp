#include "pch.h"
#include "CreateDefinitionParser.h"




namespace BlackbirdDsl {




StringCell^ CreateDefinitionParser::Parse(StringCell^ root)
{
	if (root->Count == 0)
		return root;

	List<SysStr^>^ keys = (List<SysStr^>^)root->Keys;





	return root;
}

}