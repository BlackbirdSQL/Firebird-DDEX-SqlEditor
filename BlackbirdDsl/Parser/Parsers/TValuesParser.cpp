#include "pch.h"
#include "TValuesParser.h"




namespace BlackbirdDsl {




StringCell^ TValuesParser::Parse(StringCell^ root)
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