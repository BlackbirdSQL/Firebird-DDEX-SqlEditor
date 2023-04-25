#include "pch.h"
#include "TLimitParser.h"




namespace BlackbirdDsl {




StringCell^ TLimitParser::Parse(StringCell^ root)
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