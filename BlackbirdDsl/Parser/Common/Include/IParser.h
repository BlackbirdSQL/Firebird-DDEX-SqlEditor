#pragma once
#include "pch.h"
#include "StringCell.h"


using namespace C5;


namespace BlackbirdDsl {



public interface class IParser
{
public:

	StringCell^ Execute(SysStr^ sql);

	StringCell^ Execute(StringCell^ node);

	StringCell^ Parse(StringCell^ root);

};

}