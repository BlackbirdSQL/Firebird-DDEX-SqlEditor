
using System;
using System.Collections.Generic;
using System.Linq;
using BlackbirdSql.Sys.Interfaces;
using FirebirdSql.Data.FirebirdClient;



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


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the error number from a Firebird exception.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public int GetErrorCode(Exception @this)
	{
		FbErrorCollection errors;
		int errorCode;

		if (@this is not FbException exception)
		{
			// Tracer.Trace(GetType(), "GetErrorCode()", "exceptionType: {0}.", @this.GetType().Name);

			if (@this.GetType().Name != "IscException")
				return -1;

			errors = (FbErrorCollection)Reflect.GetPropertyValue(@this, "Errors");
			errorCode = (int)Reflect.GetPropertyValue(@this, "ErrorCode");
		}
		else
		{
			errors = exception.Errors;
			errorCode = exception.ErrorCode;
		}

		if (errors == null || errors.Count <= 0)
		{
			return errorCode;
		}

		return errors.ElementAt(0).Number;
	}

	public IList<object> GetErrors(Exception @this)
	{
		FbErrorCollection errors;

		if (@this is not FbException exception)
		{
			// Tracer.Trace(GetType(), "GetErrors()", "exceptionType: {0}.", @this.GetType().Name);

			if (@this.GetType().Name != "IscException")
				return [];
			errors = (FbErrorCollection)Reflect.GetPropertyValue(@this, "Errors");
		}
		else
		{
			errors = exception.Errors;
		}

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
	/// Gets the class byte value from a Firebird exception.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public byte GetClass(Exception @this)
	{
		FbErrorCollection errors;

		if (@this is not FbException exception)
		{
			// Tracer.Trace(GetType(), "GetClass()", "exceptionType: {0}.", @this.GetType().Name);

			if (@this.GetType().Name != "IscException")
				return 0;
			errors = (FbErrorCollection)Reflect.GetPropertyValue(@this, "Errors");
		}
		else
		{
			errors = exception.Errors;
		}


		if (errors.Count <= 0)
			return 0;

		return errors.ElementAt(0).Class;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the source line number of a Firebird exception.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public int GetLineNumber(Exception @this)
	{
		FbErrorCollection errors;

		if (@this is not FbException exception)
		{
			// Tracer.Trace(GetType(), "GetLineNumber()", "exceptionType: {0}.", @this.GetType().Name);

			if (@this.GetType().Name != "IscException")
				return -1;

			errors = (FbErrorCollection)Reflect.GetPropertyValue(@this, "Errors");
		}
		else
		{
			errors = exception.Errors;
		}

		if (errors.Count <= 0)
			return -1;

		return errors.ElementAt(0).LineNumber;
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the method name of a Firebird excerption.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public string GetProcedure(Exception @this)
	{
		if (@this is not FbException exception)
			return null;

		return exception.TargetSite.Name;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the server name from a Firebird exception.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public string GetServer(Exception @this)
	{
		if (@this == null)
			return null;

		if (@this.Data.Contains("Server"))
			return (string)@this.Data["Server"];

		return null;
	}

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns the sql exception state of a Firebird exception.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public string GetState(Exception @this)
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
	public bool HasSqlException(Exception @this)
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
	public bool IsSqlException(Exception @this)
	{
		// Tracer.Trace(GetType(), "IsSqlException()", "exceptionType: {0}.", @this.GetType().Name);

		if (@this is FbException exception)
		{
			if (GetErrorCode(exception) == 335544727)
				return false;

			return true;
		}

		return @this.GetType().Name == "IscException";
	}




	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Sets the server name in a Firebird exception.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public void SetServer(Exception @this, string value)
	{
		if (@this == null)
			return;

		@this.Data["Server"] = value;
	}

}
