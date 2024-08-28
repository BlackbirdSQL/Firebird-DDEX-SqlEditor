
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using BlackbirdSql.Data.Model;
using BlackbirdSql.Sys.Interfaces;
using FirebirdSql.Data.FirebirdClient;
using Microsoft.VisualStudio.RpcContracts;



namespace BlackbirdSql.Data;


// =========================================================================================================
//											Firebird DatabaseEngineService Class
//
/// <summary>
/// Central class for database specific class methods. The intention is ultimately provide these as a
/// service so that BlackbirdSql can provide support for additional database engines.
/// </summary>
// =========================================================================================================
public class DbExceptionService : SBsNativeDbException, IBsNativeDbException
{
	private DbExceptionService()
	{
	}

	public static IBsNativeDbException EnsureInstance() => _Instance ??= new DbExceptionService();


	public static IBsNativeDbException _Instance = null;



	public byte GetErrorClass_(object error)
	{
		if (error is FbError fbError)
			return fbError.Class;

		return 0;
	}



	public ICollection<object> GetErrorEnumerator_(IList<object> errors)
	{
		if (errors == null)
			return null;

		return errors;
	}



	public int GetErrorLineNumber_(object error)
	{
		if (error is FbError fbError)
			return fbError.LineNumber;

		return -1;
	}



	public string GetErrorMessage_(object error)
	{
		if (error is FbError fbError)
			return fbError.Message;

		return (string)Reflect.GetPropertyValue(error, "Message");
	}


	public int GetErrorNumber_(object error)
	{
		if (error is FbError fbError)
			return fbError.Number;

		return -1;
	}






	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the class byte value from a Firebird exception.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public byte GetExceptionClass_(Exception @this)
	{
		if (@this is not FbException exception)
			return 0;

		FbErrorCollection errors = exception.Errors;


		if (errors.Count <= 0)
			return 0;

		return errors.ElementAt(0).Class;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the error number from a Firebird exception.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public int GetExceptionErrorCode_(Exception @this)
	{
		int errorCode;
		int dbErrorCode = 0;

		if (@this is DbException dbException)
			dbErrorCode = dbException.ErrorCode;

		if (@this is not FbException exception)
		{
			// Tracer.Trace(GetType(), "GetErrorCode()", "exceptionType: {0}.", @this.GetType().Name);

			if (@this.GetType().Name != "IscException")
				return dbErrorCode;

			errorCode = (int)Reflect.GetPropertyValue(@this, "ErrorCode");

			if (errorCode == 0)
			{
				Reflect.InvokeMethod(@this, "BuildErrorCode");
				errorCode = (int)Reflect.GetPropertyValue(@this, "ErrorCode");
			}

			if (errorCode == 0)
				errorCode = dbErrorCode;

			return errorCode;
		}

		errorCode = exception.ErrorCode;

		if (errorCode != 0)
			return errorCode;

		FbErrorCollection errors = exception.Errors;

		if (errors == null || errors.Count == 0)
			return dbErrorCode;

		errorCode = errors.ElementAt(0).Number;

		return errorCode != 0 ? errorCode : dbErrorCode;
	}



	public IList<object> GetExceptionErrors_(Exception @this)
	{
		if (@this is not FbException exception)
		{
			// Tracer.Trace(GetType(), "GetErrors()", "exceptionType: {0}.", @this.GetType().Name);

			if (@this.GetType().Name != "IscException")
				return [];

			return (IList<object>)Reflect.GetPropertyValue(@this, "Errors");

		}

		FbErrorCollection errors = exception.Errors;

		IList<object> objects;

		if (errors.Count <= 0)
		{
			FbError error = Reflect.CreateInstance<FbError>([@this.Message, 0]);

			objects =
			[
				error
			];

			return objects;
		}

		// Convert the collection to a list of error objects. The BlackbirdSql extension
		// doesn't know about FbError.

		objects =
		[
			// Tracer.Trace(GetType(), "GetErrors()", "Error count: {0}.", errors.Count);
			.. errors,
		];

		return objects;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the source line number of a Firebird exception.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public int GetExceptionLineNumber_(Exception @this)
	{
		if (@this is not FbException exception)
			return -1;

		FbErrorCollection errors = exception.Errors;

		if (errors.Count <= 0)
			return -1;

		return errors.ElementAt(0).LineNumber;
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the method name of a Firebird excerption.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public string GetExceptionProcedure_(Exception @this)
	{
		if (@this is not FbException exception)
			return "";

		return exception.TargetSite?.Name ?? "";
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the connection name or datasetid from a Firebird exception.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public string GetExceptionDatabase_(Exception @this)
	{
		if (@this == null)
			return "";

		if (@this.Data.Contains("Database"))
			return (string)@this.Data["Database"];

		return "";
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the server name from a Firebird exception.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public string GetExceptionServer_(Exception @this)
	{
		if (@this == null)
			return "";

		if (@this.Data.Contains("Server"))
			return (string)@this.Data["Server"];

		return "";
	}

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns the sql exception state of a Firebird exception.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public string GetExceptionState_(Exception @this)
	{
		string result;

		if (@this is not FbException exception)
		{
			// Tracer.Trace(GetType(), "GetState()", "exceptionType: {0}.", @this.GetType().Name);
			if (@this.GetType().Name != "IscException")
				return "";

			result = (string)Reflect.GetPropertyValue(@this, "SQLSTATE");
		}
		else
		{
			result = exception.SQLSTATE;
		}


		if (result == null)
			return "";

		return result;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Checks if an exception or aggregate exception or it's inner exception
	/// is of a specific type.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public bool HasExceptionType<T>(Exception @this) where T : class
	{
		if (@this == null)
			return false;

		if (@this as T != null)
			return true;


		if (@this is AggregateException ex2)
		{
			if (ex2.InnerExceptions != null)
				return ex2.InnerExceptions.Any((inner) => HasExceptionType<T>(inner));

			return false;
		}

		Exception exi = @this;

		while (exi.InnerException != null)
		{
			if (exi.InnerException as T != null)
				return true;

			exi = exi.InnerException;
		}
		return false;
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Checks if an exception or aggregate exception or it's inner exception
	/// is of a specific type.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public bool HasExceptionType(Exception @this, string type)
	{
		if (@this == null)
			return false;

		if (@this.GetType().Name == type)
			return true;


		if (@this is AggregateException ex2)
		{
			if (ex2.InnerExceptions != null)
				return ex2.InnerExceptions.Any((inner) => HasExceptionType(inner, type));

			return false;
		}

		Exception exi = @this;

		while (exi.InnerException != null)
		{
			if (exi.InnerException.GetType().Name == type)
				return true;

			exi = exi.InnerException;
		}
		return false;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Checks if an exception is a Firebird exception.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public bool HasSqlException_(Exception @this)
	{
		if (HasExceptionType<FbException>(@this))
			return true;

		// Tracer.Trace(GetType(), "HasSqlException()", "exceptionType: {0}.", @this.GetType().Name);

		return HasExceptionType(@this, "IscException");

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Checks if an exception is a Firebird exception.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public bool IsSqlException_(Exception @this)
	{
		// Tracer.Trace(GetType(), "IsSqlException()", "exceptionType: {0}.", @this.GetType().Name);

		if (@this is FbException exception)
		{
			if (GetExceptionErrorCode_(exception) == IscCodes.isc_net_write_err)
				return false;

			return true;
		}

		return @this.GetType().Name == "IscException";
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Sets the database name in a Firebird exception.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public void SetExceptionDatabase_(Exception @this, string value)
	{
		if (@this == null)
			return;

		@this.Data["Database"] = value;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Sets the server name in a Firebird exception.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public void SetExceptionServer_(Exception @this, string value)
	{
		if (@this == null)
			return;

		@this.Data["Server"] = value;
	}
}
