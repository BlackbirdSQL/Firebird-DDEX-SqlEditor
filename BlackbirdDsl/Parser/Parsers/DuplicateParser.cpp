#include "pch.h"
#include "DuplicateParser.h"




namespace BlackbirdDsl {




StringCell^ DuplicateParser::Parse(StringCell^ root)
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