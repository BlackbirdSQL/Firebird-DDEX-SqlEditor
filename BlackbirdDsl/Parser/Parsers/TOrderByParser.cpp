#include "pch.h"
#include "TOrderByParser.h"




namespace BlackbirdDsl {




StringCell^ TOrderByParser::Parse(StringCell^ root)
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