
using System;
using System.IO;
using System.Security;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using BlackbirdSql.Sys;
using BlackbirdSql.Sys.Enums;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;



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
	#region Constants - Cmd
	// ---------------------------------------------------------------------------------


	private static readonly char[] _S_InvalidPathChars = ['\a', '\b', '\f', '\n', '\r', '\t', '\v', '\0'];


	#endregion Constants





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



	/// <summary>
	/// Warning suppression. Returns true if on the main thread else false.
	/// </summary>
	public static async Task<bool> AwaitableAsync()
	{
		if (!ThreadHelper.CheckAccess())
			return false;

		// Warning suppression.
		await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

		return true;
	}



	public static async Task<bool> CheckAccessAsync()
	{
		if (!ThreadHelper.CheckAccess())
			return false;

		// Warning suppression.
		await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

		return true;
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


	public static string CleanPath(string path)
	{
		if (string.IsNullOrWhiteSpace(path))
			return "";

		path = string.Concat(path.Split(_S_InvalidPathChars));

		return string.Concat(path.Split(Path.GetInvalidPathChars())).Trim();
	}


	public static string GetExtension(string path) =>
		string.IsNullOrWhiteSpace(path) ? "" : Path.GetExtension(CleanPath(path));


	public static string GetDirectoryName(string path) =>
		string.IsNullOrWhiteSpace(path) ? "" : Path.GetDirectoryName(CleanPath(path));


	public static string GetFileName(string path) =>
		string.IsNullOrWhiteSpace(path) ? "" : Path.GetFileName(CleanPath(path));

	public static string GetFileNameWithoutExtension(string path) =>
		string.IsNullOrWhiteSpace(path) ? "" : Path.GetFileNameWithoutExtension(CleanPath(path));




	public static System.Windows.Media.Color CombineColors(System.Windows.Media.Color c1, int a1, System.Windows.Media.Color c2, int a2)
	{
		return System.Windows.Media.Color.FromArgb((byte)((c1.A * a1 + c2.A * a2) / 100), (byte)((c1.R * a1 + c2.R * a2) / 100), (byte)((c1.G * a1 + c2.G * a2) / 100), (byte)((c1.B * a1 + c2.B * a2) / 100));
	}



	public static System.Drawing.Color ConvertColor(System.Windows.Media.Color wpfColor)
	{
		return System.Drawing.Color.FromArgb(255, wpfColor.R, wpfColor.G, wpfColor.B);
	}



	public static string CreateVsFilterString(string s)
	{
		if (s == null)
		{
			return null;
		}

		int length = s.Length;
		char[] array = new char[length + 1];
		s.CopyTo(0, array, 0, length);
		for (int i = 0; i < length; i++)
		{
			if (array[i] == '|')
			{
				array[i] = '\0';
			}
		}

		return new string(array);
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



	public static int GetExtraSizeForBorderStyle(BorderStyle border)
	{
		return border switch
		{
			BorderStyle.Fixed3D => SystemInformation.Border3DSize.Height * 2,
			BorderStyle.FixedSingle => SystemInformation.BorderSize.Height * 2,
			BorderStyle.None => 0,
			_ => 0,
		};
	}



	/// <summary>
	/// Shortens a moniker directory path to within maxLength and prefixes with '.'
	/// if the path was shortened. maxLength may be less than or equal to zero.
	/// </summary>
	public static string GetShortenedMonikerPath(string path, int maxLength)
	{
		string[] parts = path.Split([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar]);

		int i = parts.Length - 1;
		string result;

		result = parts[^1];

		while (i > 0 && parts[i - 1].Length + result.Length + 1 <= maxLength)
		{
			i--;
			result = Path.Combine(parts[i], result);
		}

		if (i > 0)
			result = Path.Combine(".", result);

		return result;
	}



	/// <summary>
	/// Gets the prefix of an identifier stripped of any unique suffix in the for '_999'.
	/// We're going to brute force this instead of a regex.
	/// </summary>
	public static string GetUniqueIdentifierPrefix(string identifier)
	{
		if (identifier == null || identifier.Length < 3)
			return identifier;

		int i;
		char c = '\0';

		for (i = identifier.Length - 1; i > 0 && Char.IsDigit(c = identifier[i]); i--) ;

		if (i < 1 || i == identifier.Length - 1 || c != '_')
			return identifier;

		return identifier[..i];
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
	/// Checks for null and DBNull and "".
	/// </summary>
	public static bool IsNullValueOrEmpty(object value) => value == null || value == DBNull.Value || value.ToString() == "";



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
			Evs.Warning(typeof(Cmd), "IsSamePath()", $"IsSamePath exception: {ex5.Message}");
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
