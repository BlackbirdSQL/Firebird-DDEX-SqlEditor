#include "pch.h"
#include "WPartitionOptionsParser.h"




namespace BlackbirdDsl {




StringCell^ WPartitionOptionsParser::Parse(StringCell^ root)
{
	if (root->Count == 0)
		return root;

	List<SysStr^>^ keys = (List<SysStr^>^)root->Keys;





	return root;
}

}