#include "pch.h"
#include "TRenameParser.h"




namespace BlackbirdDsl {




StringCell^ TRenameParser::Parse(StringCell^ root)
{
	StringCell^ parserNode = root[_Key];

	if (parserNode->Count == 0)
	{
		root->Remove(_Key);
		return root;
	}





	root[_Key] = parserNode;

	return root;
}

}