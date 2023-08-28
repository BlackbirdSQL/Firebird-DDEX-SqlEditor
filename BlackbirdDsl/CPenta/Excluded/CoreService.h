#pragma once
#include "pch.h"
#include "CPentaCommon.h"

// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)



namespace C5 {




// =========================================================================================================
//											CoreService Class
//
/// Provides services not available in C#
// =========================================================================================================
public ref class CoreService abstract sealed
{

private:

	#pragma region Variables


public:


	#pragma endregion Variables




	// =========================================================================================================
	#pragma region Property Accessors - CoreService
	// =========================================================================================================



	#pragma endregion Property Accessors


	// =========================================================================================================
	#pragma region Constructors / Destructors - CoreService
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// Default .ctor.
	// ---------------------------------------------------------------------------------


	// ---------------------------------------------------------------------------------
	/// Destructor.
	// ---------------------------------------------------------------------------------


	#pragma endregion Constructors / Destructors





	// =========================================================================================================
	#pragma region Methods - CoreService
	// =========================================================================================================


public:

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns the IDE solution working folder.
	/// </summary>
	// ---------------------------------------------------------------------------------
	static SysStr^ GetWorkingFolder(bool isException, SysStr^ message)
	{
		SysStr^ str = gcnew SysStr("");

		Working
		return str;

	}


	#pragma endregion Methods

};

}
