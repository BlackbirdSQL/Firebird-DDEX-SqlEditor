#include "pch.h"
#include "WCreateDefinitionParser.h"




namespace BlackbirdDsl {




StringCell^ WCreateDefinitionParser::Parse(StringCell^ root)
{
	if (root->Count == 0)
		return root;

	List<SysStr^>^ keys = (List<SysStr^>^)root->Keys;





	return root;
}

}