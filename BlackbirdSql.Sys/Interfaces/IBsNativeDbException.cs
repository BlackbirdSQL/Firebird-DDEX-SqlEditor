// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;



namespace BlackbirdSql.Sys.Interfaces;

[Guid(LibraryData.NativeDbExceptionServiceGuid)]


// =========================================================================================================
//										IBsNativeDbException Interface
//
/// <summary>
/// Interface for native DbException extension methods service.
/// </summary>
// =========================================================================================================
public interface IBsNativeDbException
{
	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the error number from a native database exception.
	/// </summary>
	// ---------------------------------------------------------------------------------
	int GetErrorCode(Exception exception);


	IList<object> GetErrors(Exception exception);

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the class byte value from a native database exception.
	/// </summary>
	// ---------------------------------------------------------------------------------
	byte GetClass(Exception exception);



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the source line number of a native database exception.
	/// </summary>
	// ---------------------------------------------------------------------------------
	int GetLineNumber(Exception exception);



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the method name of a native database exception.
	/// </summary>
	// ---------------------------------------------------------------------------------
	string GetProcedure(Exception exception);




	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the server name from a native database exception.
	/// </summary>
	// ---------------------------------------------------------------------------------
	string GetServer(Exception exception);


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns the sql exception state of a native database exception.
	/// </summary>
	// ---------------------------------------------------------------------------------
	string GetState(Exception @this);


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Checks if an exception contains a native database exception.
	/// </summary>
	// ---------------------------------------------------------------------------------
	bool HasSqlException(Exception exception);

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Checks if an exception is a native database exception.
	/// </summary>
	// ---------------------------------------------------------------------------------
	bool IsSqlException(Exception exception);




	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Sets the server name in a native database exception.
	/// </summary>
	// ---------------------------------------------------------------------------------
	void SetServer(Exception exception, string value);

}