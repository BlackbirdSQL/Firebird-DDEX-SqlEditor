
using System;
using System.Globalization;
using System.Text;
using BlackbirdSql.Core.Ctl;
using BlackbirdSql.Core.Ctl.Enums;
using BlackbirdSql.Core.Ctl.Extensions;
using FirebirdSql.Data.FirebirdClient;




namespace BlackbirdSql.Core;


// =========================================================================================================
//												Cmd Class
//
/// <summary>
/// Central location for implementation of utility static methods. 
/// </summary>
// =========================================================================================================

public abstract class Cmd
{


	// ---------------------------------------------------------------------------------
	#region Constants - Cmd
	// ---------------------------------------------------------------------------------



	#endregion Constants





	// =========================================================================================================
	#region Variables - Cmd
	// =========================================================================================================



	#endregion Variables





	// =========================================================================================================
	#region Static Methods - Cmd
	// =========================================================================================================





	// CheckForEmptyString
	public static void CheckForEmptyString(string variable, string variableName)
	{
		CheckForNullReference(variable, variableName);
		CheckForNullReference(variableName, "variableName");
		if (variable.Length == 0)
		{
			ArgumentNullException ex = new("variableName");
			Diag.Dug(ex);
			throw ex;
		}
	}



	// CheckForNull
	public static void CheckForNull(object var, string varName)
	{
		if (var == null)
		{
			ArgumentNullException ex = new(varName);
			Diag.Dug(ex);
			throw ex;
		}
	}



	// CheckForNullReference
	public static void CheckForNullReference(object variable, string variableName)
	{
		if (variableName == null)
		{
			ArgumentNullException ex = new("variableName");
			Diag.Dug(ex);
			throw ex;
		}

		if (variable == null)
		{
			ArgumentNullException ex = new(variableName);
			Diag.Dug(ex);
			throw ex;
		}
	}



	// CheckStringForNullOrEmpty
	public static void CheckStringForNullOrEmpty(string stringVar, string stringVarName)
	{
		CheckStringForNullOrEmpty(stringVar, stringVarName, trim: false);
	}

	// CheckStringForNullOrEmpty
	public static void CheckStringForNullOrEmpty(string stringVar, string stringVarName, bool trim)
	{
		CheckForNull(stringVar, stringVarName);

		if (trim)
			stringVar = stringVar.Trim();

		if (stringVar.Length == 0)
		{
			ArgumentException ex = new("EmptyStringNotAllowed", stringVarName);
			Diag.Dug(ex);
			throw ex;
		}
	}



	// Failed
	public static bool Failed(int hr)
	{
		return hr < 0;
	}




	// GetDetailedException
	public static string GetDetailedException(Exception e)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (e != null)
		{
			if (e is FbException ex)
			{
				stringBuilder.AppendLine(HashLog.Format(CultureInfo.CurrentCulture, Properties.Resources.ClassNumber, ex.GetProcedure(), ex.GetErrorCode()));
			}

			stringBuilder.AppendLine(e.Message);
		}

		return stringBuilder.ToString();
	}



	// IsCriticalException
	public static bool IsCriticalException(Exception ex)
	{

		if (ex is NullReferenceException || ex is OutOfMemoryException || ex is System.Threading.ThreadAbortException)
		{
			return true;
		}

		if (ex.InnerException != null)
			return IsCriticalException(ex.InnerException);

		return false;
	}


	// NullEquality
	public static EnNullEquality NullEquality(object lhs, object rhs)
	{
		if ((lhs == null || lhs == DBNull.Value) && (rhs == null || rhs == DBNull.Value)) return EnNullEquality.Equal;

		if (lhs == null || lhs == DBNull.Value || rhs == null || rhs == DBNull.Value) return EnNullEquality.UnEqual;

		return EnNullEquality.NotNulls;

	}


	/*
	// UseRandomizedHashing
	public static bool UseRandomizedHashing()
	{
		return Native.InternalUseRandomizedHashing();
	}
	*/


	#endregion Static Methods

}
