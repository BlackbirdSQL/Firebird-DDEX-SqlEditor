// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;



namespace BlackbirdSql.Sys.Interfaces;

[Guid(LibraryData.C_NativeDbExceptionServiceGuid)]


// =========================================================================================================
//										IBsNativeDbException Interface
//
/// <summary>
/// Interface for native DbException extension methods service.
/// </summary>
// =========================================================================================================
public interface IBsNativeDbException
{
	byte GetErrorClass_(object error);
	ICollection<object> GetErrorEnumerator_(IList<object> errors);
	int GetErrorLineNumber_(object error);
	string GetErrorMessage_(object error);
	int GetErrorNumber_(object error);

	/// <summary>
	/// Gets the class byte value from a native database exception.
	/// </summary>
	byte GetExceptionClass_(Exception exception);

	int GetExceptionErrorCode_(Exception @this);
	IList<object> GetExceptionErrors_(Exception exception);

	/// <summary>
	/// Gets the source line number of a native database exception.
	/// </summary>
	int GetExceptionLineNumber_(Exception exception);

	/// <summary>
	/// Gets the method name of a native database exception.
	/// </summary>
	string GetExceptionProcedure_(Exception exception);

	/// <summary>
	/// Gets the connection name or datasetName from a native database exception.
	/// </summary>
	string GetExceptionDatabase_(Exception exception);

	/// <summary>
	/// Gets the server name from a native database exception.
	/// </summary>
	string GetExceptionServer_(Exception exception);

	/// <summary>
	/// Returns the sql exception state of a native database exception.
	/// </summary>
	string GetExceptionState_(Exception @this);

	/// <summary>
	/// Checks if an exception contains a native database exception.
	/// </summary>
	bool HasSqlException_(Exception exception);

	/// <summary>
	/// Checks if an exception is a database network exception.
	/// </summary>
	bool IsDbNetException_(Exception @this);

	/// <summary>
	/// Checks if an exception is a native database exception.
	/// </summary>
	bool IsSqlException_(Exception exception);

	/// <summary>
	/// Sets the database name in a native database exception.
	/// </summary>
	void SetExceptionDatabase_(Exception exception, string value);

	/// <summary>
	/// Sets the server name in a native database exception.
	/// </summary>
	void SetExceptionServer_(Exception exception, string value);

}