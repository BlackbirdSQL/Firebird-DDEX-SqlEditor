#include "pch.h"
#include "TDropParser.h"




namespace BlackbirdDsl {




StringCell^ TDropParser::Parse(StringCell^ root)
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