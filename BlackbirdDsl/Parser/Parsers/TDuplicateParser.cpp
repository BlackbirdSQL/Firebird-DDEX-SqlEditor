#include "pch.h"
#include "TDuplicateParser.h"




namespace BlackbirdDsl {




StringCell^ TDuplicateParser::Parse(StringCell^ root)
{
	StringCell^ parserNode = root[_Key];

	if (parserNode->Count == 0)
	{
		root->Remove(_Key);
		return root;
	}







	root["ON DUPLICATE KEY UPDATE"] = parserNode;
	root->Remove(_Key);

	return root;
}

}