#include "pch.h"
#include "HavingParser.h"




namespace BlackbirdDsl {




StringCell^ HavingParser::Parse(StringCell^ root)
{
	StringCell^ parserNode = root[_Key];

	if (parserNode->Count == 0)
	{
		root->Remove(_Key);
		return root;
	}

	StringCell^ selectNode = (root->IsNullOrEmpty["SELECT"] ? (gcnew StringCell()) : root["SELECT"]);






	root[_Key] = parserNode;

	return root;
}

}