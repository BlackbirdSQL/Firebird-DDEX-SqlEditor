#include "pch.h"
#include "TUsingParser.h"




namespace BlackbirdDsl {




StringCell^ TUsingParser::Parse(StringCell^ root)
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