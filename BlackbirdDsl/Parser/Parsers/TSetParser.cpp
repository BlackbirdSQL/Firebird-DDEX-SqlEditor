#include "pch.h"
#include "TSetParser.h"




namespace BlackbirdDsl {




StringCell^ TSetParser::Parse(StringCell^ root)
{
	StringCell^ parserNode = root[_Key];

	if (parserNode->Count == 0)
	{
		root->Remove(_Key);
		return root;
	}

	bool isUpdate = !root->IsUnpopulated["UPDATE"];




	root[_Key] = parserNode;

	return root;
}

}