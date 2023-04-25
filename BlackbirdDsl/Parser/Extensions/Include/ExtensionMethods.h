#pragma once
#include "pch.h"
#include "CPentaCommon.h"
#include "Gram.h"




namespace BlackbirdDsl {


// [Extension]
public ref class Str abstract sealed
{


public:



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// IsNumeric
	/// </summary>
	// ---------------------------------------------------------------------------------
	// [ExtensionAttribute]
	static bool IsNumeric(SysStr^ value)
	{
		if (SysStr::IsNullOrEmpty(value))
		{
			return false;
		}
		else
		{
			System::Double^ outval = gcnew System::Double(0.0);
			return System::Double::TryParse(value, *outval);
		}
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// IsAggregateFunction
	/// </summary>
	// ---------------------------------------------------------------------------------
	// [ExtensionAttribute]
	static bool IsAggregateFunction(SysStr^ value)
	{
		if (SysStr::IsNullOrEmpty(value))
			return false;

		return Gram::IsAggregateFunction(value);
	}




	// ---------------------------------------------------------------------------------
	/// <summary>
	/// IsReserved
	/// </summary>
	// ---------------------------------------------------------------------------------
	// [ExtensionAttribute]
	static bool IsReserved(SysStr^ value)
	{
		if (SysStr::IsNullOrEmpty(value))
			return false;

		return Gram::IsReserved(value);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// IsReserved
	/// </summary>
	// ---------------------------------------------------------------------------------
	// [ExtensionAttribute]
	static bool IsFunction(SysStr^ value)
	{
		if (SysStr::IsNullOrEmpty(value))
			return false;

		return Gram::IsFunction(value);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// IsReserved
	/// </summary>
	// ---------------------------------------------------------------------------------
	// [ExtensionAttribute]
	static bool IsParameterizedFunction(SysStr^ value)
	{
		if (SysStr::IsNullOrEmpty(value))
			return false;

		return Gram::IsParameterizedFunction(value);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// IsReserved
	/// </summary>
	// ---------------------------------------------------------------------------------
	// [ExtensionAttribute]
	static bool IsCustomFunction(SysStr^ value)
	{
		if (SysStr::IsNullOrEmpty(value))
			return false;

		return Gram::IsCustomFunction(value);
	}



};

}