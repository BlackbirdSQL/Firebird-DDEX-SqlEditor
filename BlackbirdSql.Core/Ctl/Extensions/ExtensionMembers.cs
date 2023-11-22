// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Forms;
using BlackbirdSql.Core.Ctl.Interfaces;
using FirebirdSql.Data.FirebirdClient;

using Microsoft.VisualStudio.Data.Services.SupportEntities;


namespace BlackbirdSql.Core.Ctl.Extensions;

// =========================================================================================================
//											ExtensionMembers Class
//
/// <summary>
/// Central class for package external class extension methods. 
/// </summary>
// =========================================================================================================
static class ExtensionMembers
{
	private static readonly string[] S_ByteSizeSuffixes =
		{ "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
	private static readonly string[] S_SIExponents =
		{ null, "\x00b9", "\x00b2", "\x00b3", "\x2074", "\x2075", "\x2076", "\x2077", "\x2078", "\x2079",
			"\x00b9\x2070", "\x00b9\x00b9", "\x00b9\x00b2", "\x00b9\x00b3", "\x00b9\x2074", "\x00b9\x2075",
			"\x00b9\x2076", "\x00b9\x2077", "\x00b9\x2078", "\x00b9\x2079", "\x00b2\x2070", "\x00b2\x00b9",
			"\x00b2\x00b2", "\x00b2\x00b3", "\x00b2\x2074", "\x00b2\x2075", "\x00b2\x2076", "\x00b2\x2077",
			"\x00b2\x2078", "\x00b2\x2079"};



	/// <summary>
	/// Formats time span for display in an sql window statusbar.
	/// </summary>
	public static string FmtSqlStatus(this TimeSpan value)
	{
		return new TimeSpan(value.Days, value.Hours, value.Minutes, value.Seconds, 0).ToString();
	}


	/// <summary>
	/// Formats time ticks for display in an sql window statusbar.
	/// </summary>
	public static string FmtSqlStatus(this long ticks)
	{
		TimeSpan value = new(ticks);

		return value.FmtSqlStatus();
	}


	/// <summary>
	/// Formats time span for display in sql statistics output.
	/// </summary>
	public static string FmtSqlStats(this TimeSpan value)
	{
		string empty = value.ToString();
		if (!string.IsNullOrEmpty(empty))
		{
			string numberDecimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
			int num = empty.LastIndexOf(numberDecimalSeparator, StringComparison.Ordinal);
			if (num != -1)
			{
				int num2 = num + 4;
				if (num2 <= empty.Length)
				{
					empty = empty[..num2];
				}
			}
		}

		return empty;
	}

	/// <summary>
	/// Formats time span ticks for display in sql statistics output.
	/// </summary>
	public static string FmtSqlStats(this long ticks)
	{
		TimeSpan value = new(ticks);

		return value.FmtSqlStats();
	}


	/// <summary>
	/// Format a long byte size down to it's byte unit by decimal places.
	/// </summary>
	public static (string, float) FmtByteSize(this long value, int decimalPlaces = 3)
	{
		string str;
		float newValue;

		if (value < 0)
		{
			(str, newValue) = (-value).FmtByteSize(decimalPlaces);
			str = "-" + str;
			return (str, -newValue);
		}

		int i = 0;
		newValue = value;

		while (value > 999999L && Math.Round(newValue, decimalPlaces) >= 1000)
		{
			newValue /= 1024;
			i++;

			if (S_ByteSizeSuffixes.Length <= i + 1)
				break;
		}

		if (decimalPlaces != 0 && Math.Round(newValue, 0) == Math.Round(newValue, decimalPlaces))
			decimalPlaces = 0;

		return (string.Format("{0:n" + decimalPlaces + "} {1}", newValue, S_ByteSizeSuffixes[i]), newValue);
	}



	/// <summary>
	/// Format a float byte size down to it's byte unit by decimal places.
	/// </summary>
	public static (string, float) FmtByteSize(this float value, int decimalPlaces = 3)
	{
		string str;
		float newValue;

		if (value < 0)
		{
			(str, newValue) = (-value).FmtByteSize(decimalPlaces);
			str = "-" + str;
			return (str, -newValue);
		}

		int i = 0;
		newValue = value;

		while (value > 999999.999 && Math.Round(newValue, decimalPlaces) >= 1000)
		{
			newValue /= 1024;
			i++;

			if (S_ByteSizeSuffixes.Length <= i + 1)
				break;
		}

		if (decimalPlaces != 0 && Math.Round(newValue, 0) == Math.Round(newValue, decimalPlaces))
			decimalPlaces = 0;

		return (string.Format("{0:n" + decimalPlaces + "} {1}", newValue, S_ByteSizeSuffixes[i]), newValue);
	}


	/// <summary>
	/// Formats a long into superscipt exponential notation by decimal places
	/// </summary>
	public static (string, float) FmtExpSize(this long value, int maxDigits = 4, int decimalPlaces = 3)
	{
		string str;
		float newValue;

		if (value < 0)
		{
			(str, newValue) = (-value).FmtExpSize(maxDigits, decimalPlaces);
			str = "-" + str;
			return (str, -newValue);
		}

		int i = 0;
		newValue = value;
		while (Math.Floor(newValue) >= Math.Pow(10, maxDigits))
		{
			newValue /= 10;
			i++;

			if (S_SIExponents.Length <= i + 1)
				break;
		}

		if (i <= 1)
			return (value.ToString(), value);

		if (decimalPlaces != 0 && Math.Round(newValue, 0) == Math.Round(newValue, decimalPlaces))
			decimalPlaces = 0;

		return (string.Format("{0:n" + decimalPlaces + "} (10{1})", newValue, S_SIExponents[i]), newValue);

	}

	/// <summary>
	/// Formats a float into superscipt exponential notation by decimal places
	/// </summary>
	public static (string, float) FmtExpSize(this float value, int maxDigits = 4, int decimalPlaces = 3)
	{
		string str;
		float newValue;

		if (value < 0)
		{
			(str, newValue) = (-value).FmtExpSize(maxDigits, decimalPlaces);
			str = "-" + str;
			return (str, -newValue);
		}

		int i = 0;
		newValue = (float)value;
		while (Math.Floor(newValue) >= Math.Pow(10, maxDigits))
		{
			newValue /= 10;
			i++;

			if (S_SIExponents.Length <= i + 1)
				break;
		}

		if (i <= 1)
			return (value.ToString(), value);

		if (decimalPlaces != 0 && Math.Round(newValue, 0) == Math.Round(newValue, decimalPlaces))
			decimalPlaces = 0;

		return (string.Format("{0:n" + decimalPlaces + "} (10{1})", newValue, S_SIExponents[i]), newValue);

	}


	/// <summary>
	/// Concatenates exception message with inner exception.
	/// </summary>
	internal static string GetExceptionMessage(this Exception ex)
	{
		string text = string.Empty;
		if (ex != null)
		{
			text = ex.Message;
			if (ex.InnerException != null)
			{
				text = text + " " + ex.InnerException.Message;
			}
		}
		return text;
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs an extended get of a connection descriptor's value
	/// if it's underlying connection properties object implements the dynamic
	/// <see cref="ICustomTypeDescriptor"/> interface.
	/// </summary>
	/// <returns>The descriptor's current value</returns>
	// ---------------------------------------------------------------------------------
	public static object GetValueX(this PropertyDescriptor descriptor, IVsDataConnectionProperties connectionProperties)
	{
		object component = connectionProperties;

		if (connectionProperties is ICustomTypeDescriptor customTypeDescriptor)
		{
			component = customTypeDescriptor.GetPropertyOwner(descriptor);
		}

		return descriptor.GetValue(component);
	}



	/// <summary>
	/// Checks if an exception or aggregate exception or it's inner exception
	/// is of a specific type.
	/// </summary>
	internal static bool IsExceptionType(this Exception ex, Type type)
	{
		if (ex == null)
		{
			return false;
		}
		if (ex is AggregateException ex2)
		{
			if (ex2.InnerExceptions != null)
			{
				return ex2.InnerExceptions.Any((inner) => inner.IsExceptionType(type));
			}
			return false;
		}
		if (ex.GetType() == type || ex.InnerException != null && ex.InnerException.IsExceptionType(type))
		{
			return true;
		}
		return false;
	}


	/// <summary>
	/// Checks if an exception is a Firebird exception.
	/// </summary>
	internal static bool IsSqlException(this Exception ex)
	{
		return ex.IsExceptionType(typeof(FbException));
	}



	/// <summary>
	/// Formats a resource string format string given arguments.
	/// </summary>
	public static string FmtRes(this string value, object arg0)
	{
		return string.Format(CultureInfo.CurrentCulture, value, arg0);

	}

	/// <summary>
	/// Formats a resource string format string given arguments.
	/// </summary>
	public static string FmtRes(this string value, object arg0, object arg1)
	{
		return string.Format(CultureInfo.CurrentCulture, value, arg0, arg1);

	}

	/// <summary>
	/// Formats a resource string format string given arguments.
	/// </summary>
	public static string FmtRes(this string value, object arg0, object arg1, object arg2)
	{
		return string.Format(CultureInfo.CurrentCulture, value, arg0, arg1, arg2);

	}

	/// <summary>
	/// Formats a resource string format string given arguments.
	/// </summary>
	public static string FmtRes(this string value, object arg0, object arg1, object arg2, object arg3)
	{
		return string.Format(CultureInfo.CurrentCulture, value, arg0, arg1, arg2, arg3);

	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Mimicks the search performed by assignment to <see cref="ListControl.SelectedValue"/> where no <see cref="ListControl.ValueMember"/> exists.
	/// </summary>
	/// <param name="comboBox"></param>
	/// <param name="value"></param>
	/// <returns>The new found <see cref="ComboBox.Items"/> value at the new <see cref="ComboBox.SelectedIndex"/> else null</returns>
	/// <remarks>
	/// The method returns the assigned object if found so that inline assignments in series can also cast the object.
	/// eg.string str = (string)combo.SetSelectedValueX(obj);</remarks>
	// ---------------------------------------------------------------------------------
	public static object SetSelectedValueX(this ComboBox comboBox, object value)
	{
		if (value == null)
		{
			comboBox.SelectedIndex = -1;
			return null;
		}

		int result;

		if (value is string str)
			result = comboBox.FindStringExact(str);
		else
			result = comboBox.FindStringExact(value.ToString());

		comboBox.SelectedIndex = result;

		return result == -1 ? null : value;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Allows casting of <see cref="ComboBox.SelectedIndex"/> when doing multiple assignments in series.
	/// eg. string x = cbo.SetSelectedIndexX(index).ToString();
	/// </summary>
	/// <param name="comboBox"></param>
	/// <param name="index"></param>
	/// <returns>The new value of <see cref="ComboBox.SelectedIndex"/> else -1.</returns>
	// ---------------------------------------------------------------------------------
	public static int SetSelectedIndexX(this ComboBox comboBox, int index)
	{
		comboBox.SelectedIndex = index;

		return comboBox.SelectedIndex;
	}


	/// <summary>
	/// Converts a secure string to it's readable string.
	/// </summary>
	public static string ToReadable(this SecureString secureString)
	{
		return new string(secureString.ToCharArray());
	}


	/// <summary>
	/// Converts a string to a SecureString.
	/// </summary>
	/// <param name="unsecureString"></param>
	/// <returns></returns>
	public static SecureString ToSecure(this string unsecureString)
	{
		return unsecureString.ToCharArray().ToSecure();
	}


	/// <summary>
	/// Converts a SecureString to a char array.
	/// </summary>
	private static char[] ToCharArray(this SecureString secureString)
	{
		char[] array = new char[secureString.Length];
		IntPtr intPtr = Marshal.SecureStringToGlobalAllocUnicode(secureString);
		try
		{
			Marshal.Copy(intPtr, array, 0, secureString.Length);
			return array;
		}
		finally
		{
			Marshal.ZeroFreeGlobalAllocUnicode(intPtr);
		}
	}


	/// <summary>
	/// Converts a long timestamp to a UTC DateTime.
	/// </summary>
	public static DateTime ToUtcDateTime(this long timestamp)
	{
		DateTimeOffset offset = DateTimeOffset.FromUnixTimeMilliseconds(timestamp);
		return offset.UtcDateTime;
	}


	/// <summary>
	/// Converts a DateTime to it's long unix milliseconds timestamp.
	/// </summary>
	public static long UnixMilliseconds(this DateTime value)
	{
		return ((DateTimeOffset)value).ToUnixTimeMilliseconds();
	}


	/// <summary>
	/// Converts an IEnumerable char array to a SecureString.
	/// </summary>
	private static SecureString ToSecure(this IEnumerable<char> charArray)
	{
		SecureString secureString = new SecureString();
		foreach (char item in charArray)
		{
			secureString.AppendChar(item);
		}

		secureString.MakeReadOnly();
		return secureString;
	}


	/// <summary>
	/// Gets the server name from a Firebird exception.
	/// </summary>
	public static string GetServer(this FbException exception)
	{
		if (exception == null)
			return null;

		if (exception.Data.Contains("Server"))
			return (string)exception.Data["Server"];

		return null;
	}



	/// <summary>
	/// Sets the server name in a Firebird exception.
	/// </summary>
	public static void SetServer(this FbException exception, string value)
	{
		if (exception == null)
			return;

		exception.Data["Server"] = value;
	}


	/// <summary>
	/// Gets the error number from a Firebird exception.
	/// </summary>
	public static int GetErrorCode(this FbException exception)
	{
		if (exception == null)
			return -1;

		if (exception.Errors.Count <= 0)
			return -1;

		return exception.Errors.ElementAt(0).Number;
	}


	/// <summary>
	/// Gets the class byte value from a Firebird exception.
	/// </summary>
	public static byte GetClass(this FbException exception)
	{
		if (exception == null)
			return 0;

		if (exception.Errors.Count <= 0)
			return 0;

		return exception.Errors.ElementAt(0).Class;
	}

	/// <summary>
	/// Gets the source line number of a Firebird exception.
	/// </summary>
	public static int GetLineNumber(this FbException exception)
	{
		if (exception == null)
			return -1;

		if (exception.Errors.Count <= 0)
			return -1;

		return exception.Errors.ElementAt(0).LineNumber;
	}


	/// <summary>
	/// Returns the sql exception state of a Firebird exception.
	/// </summary>
	public static string GetState(this FbException exception)
	{
		if (exception == null)
			return null;


		return exception.SQLSTATE;
	}

	/// <summary>
	/// Gets the method name of a Firebird excerption.
	/// </summary>
	public static string GetProcedure(this FbException exception)
	{
		if (exception == null)
			return null;


		return exception.TargetSite.Name;
	}

}
