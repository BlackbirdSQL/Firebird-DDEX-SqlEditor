#include "pch.h"
#include "SetParser.h"




namespace BlackbirdDsl {




StringCell^ SetParser::Parse(StringCell^ root)
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