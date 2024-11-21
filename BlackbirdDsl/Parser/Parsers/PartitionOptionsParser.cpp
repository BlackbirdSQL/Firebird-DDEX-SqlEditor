#include "pch.h"
#include "PartitionOptionsParser.h"




namespace BlackbirdDsl {




StringCell^ PartitionOptionsParser::Parse(StringCell^ root)
{
	if (root->Count == 0)
		return root;

	List<SysStr^>^ keys = (List<SysStr^>^)root->Keys;





	return root;
}

}