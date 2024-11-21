#include "pch.h"
#include "ReplaceParser.h"




namespace BlackbirdDsl {




StringCell^ ReplaceParser::Parse(StringCell^ root)
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