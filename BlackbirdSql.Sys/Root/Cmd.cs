
using System;
using System.IO;
using System.Security;
using System.Windows;
using System.Windows.Media;
using BlackbirdSql.Sys.Enums;
using Microsoft.VisualStudio;



namespace BlackbirdSql;


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
	#region Property Accessors - Cmd
	// ---------------------------------------------------------------------------------


	/// <summary>
	/// Always true without warning message.
	/// </summary>
	public static bool ToBeCompleted => true;


	#endregion Property accessors





	// =========================================================================================================
	#region Static Methods - Cmd
	// =========================================================================================================


	/// <summary>
	/// <see cref="ErrorHandler.ThrowOnFailure"/> token.
	/// </summary>
	public static int ___(int hr) => ErrorHandler.ThrowOnFailure(hr);



	// CanonicalizeDirectoryName
	public static string CanonicalizeDirectoryName(string fullPathDirName)
	{
		if (string.IsNullOrEmpty(fullPathDirName))
		{
			ArgumentNullException ex = new("fullPathDirName");
			Diag.Dug(ex);
			throw ex;
		}

		return CanonicalizeFileNameOrDirectoryImpl(fullPathDirName, pathIsDir: true);
	}


	// CanonicalizeFileNameOrDirectoryImpl
	private static string CanonicalizeFileNameOrDirectoryImpl(string path, bool pathIsDir)
	{
		if (path.StartsWith("FBSQL::", StringComparison.OrdinalIgnoreCase) || path.StartsWith("FBSQLCLR::", StringComparison.OrdinalIgnoreCase))
		{
			return path;
		}

		path = Path.GetFullPath(path);
		path = path.ToUpperInvariant();
		if (pathIsDir)
		{
			return EnsureNoBackslash(path);
		}

		return path;
	}


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



	public static System.Windows.Media.Color CombineColors(System.Windows.Media.Color c1, int a1, System.Windows.Media.Color c2, int a2)
	{
		return System.Windows.Media.Color.FromArgb((byte)((c1.A * a1 + c2.A * a2) / 100), (byte)((c1.R * a1 + c2.R * a2) / 100), (byte)((c1.G * a1 + c2.G * a2) / 100), (byte)((c1.B * a1 + c2.B * a2) / 100));
	}



	public static System.Drawing.Color ConvertColor(System.Windows.Media.Color wpfColor)
	{
		return System.Drawing.Color.FromArgb(255, wpfColor.R, wpfColor.G, wpfColor.B);
	}



	// EnsureNoBackslash
	public static string EnsureNoBackslash(string fullPath)
	{
		string result = fullPath;
		if (!string.IsNullOrEmpty(fullPath) && fullPath.Length > 1 && (fullPath[^1] == '\\' || fullPath[^1] == '/'))
		{
			result = fullPath[..^1];
		}

		return result;
	}



	public static UIElement FindVisualParent(Type typeOfParent, UIElement element, Type stopOnType = null)
	{
		for (UIElement uIElement = element; uIElement != null; uIElement = VisualTreeHelper.GetParent(uIElement) as UIElement)
		{
			if (typeOfParent.IsAssignableFrom(uIElement.GetType()))
			{
				return uIElement;
			}
			if (stopOnType != null && stopOnType.IsAssignableFrom(uIElement.GetType()))
			{
				return null;
			}
		}
		return null;
	}

	public static T FindVisualParent<T>(UIElement element, Type stopOnType = null) where T : UIElement
	{
		for (UIElement uIElement = element; uIElement != null; uIElement = VisualTreeHelper.GetParent(uIElement) as UIElement)
		{
			if (uIElement is T result)
			{
				return result;
			}
			if (stopOnType != null && stopOnType.IsAssignableFrom(uIElement.GetType()))
			{
				return null;
			}
		}
		return null;
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



	/// <summary>
	/// Checks for null and DBNull.
	/// </summary>
	public static bool IsNullValue(object value) => value == null || value == DBNull.Value;

	/// <summary>
	/// Checks for null and DBNull and string.Empty.
	/// </summary>
	public static bool IsNullValueOrEmpty(object value) => value == null || value == DBNull.Value || value.ToString() == string.Empty;



	// IsSamePath
	public static bool IsSamePath(string file1, string file2)
	{
		if (file1 == null || file1.Length == 0)
		{
			if (file2 != null)
				return file2.Length == 0;

			return true;
		}

		try
		{
			if (!Uri.TryCreate(file1, UriKind.Absolute, out var result)
				|| !Uri.TryCreate(file2, UriKind.Absolute, out var result2))
			{
				return false;
			}

			if (result != null && result.IsFile && result2 != null && result2.IsFile)
			{
				try
				{
					string strA = CanonicalizeDirectoryName(result.LocalPath);
					string strB = CanonicalizeDirectoryName(result2.LocalPath);

					return string.Compare(strA, strB, StringComparison.OrdinalIgnoreCase) == 0;
				}
				catch (PathTooLongException)
				{
					return false;
				}
				catch (ArgumentException)
				{
					return false;
				}
				catch (SecurityException)
				{
					return false;
				}
				catch (NotSupportedException)
				{
					return false;
				}
			}

			return file1 == file2;
		}
		catch (UriFormatException ex5)
		{
			Tracer.Warning(typeof(Cmd), "IsSamePath()", "IsSamePath exception: {0}", ex5.Message);
		}

		return false;
	}


	/// <summary>
	/// Performs a null equality check on with DBNull treated as null.
	/// </summary>
	public static EnNullEquality NullEquality(object lhs, object rhs)
	{
		if (Cmd.IsNullValue(lhs) && Cmd.IsNullValue(rhs)) return EnNullEquality.Equal;

		if (Cmd.IsNullValue(lhs) || Cmd.IsNullValue(rhs)) return EnNullEquality.UnEqual;

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
