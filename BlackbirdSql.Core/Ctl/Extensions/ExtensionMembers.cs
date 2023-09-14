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


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Converts a string to title case and strips out spaces. This provides a
	/// readable/usable column name format for connection property descriptors.
	/// </summary>
	/// <param name="value"></param>
	/// <returns>
	/// The title case string without spaces else returns the original if the string
	/// is null
	/// </returns>
	// ---------------------------------------------------------------------------------
	public static string DescriptorCase(this string value)
	{
		if (string.IsNullOrEmpty(value))
			return value;

		return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value).Replace(" ", "");
	}

	public static bool EmptyOrEqual(this string value1, string value2)
	{
		if (string.IsNullOrEmpty(value1) && string.IsNullOrEmpty(value2))
		{
			return true;
		}

		return value1 == value2;
	}

	public static string FormatForStatus(this TimeSpan value)
	{
		return new TimeSpan(value.Days, value.Hours, value.Minutes, value.Seconds, 0).ToString();
	}

	public static string FormatForStatus(this long ticks)
	{
		TimeSpan value = new(ticks);

		return value.FormatForStatus();
	}

	public static string FormatForStats(this TimeSpan value)
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

	public static string FormatForStats(this long ticks)
	{
		TimeSpan value = new(ticks);

		return value.FormatForStats();
	}

	public static (string, float) ByteSizeFormat(this long value, int decimalPlaces = 3)
	{
		string str;
		float newValue;

		if (value < 0)
		{
			(str, newValue) = (-value).ByteSizeFormat(decimalPlaces);
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


	public static (string, float) ByteSizeFormat(this float value, int decimalPlaces = 3)
	{
		string str;
		float newValue;

		if (value < 0)
		{
			(str, newValue) = (-value).ByteSizeFormat(decimalPlaces);
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

	public static (string, float) SISizeFormat(this long value, int maxDigits = 4, int decimalPlaces = 3)
	{
		string str;
		float newValue;

		if (value < 0)
		{
			(str, newValue) = (-value).SISizeFormat(maxDigits, decimalPlaces);
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

	public static (string, float) SISizeFormat(this float value, int maxDigits = 4, int decimalPlaces = 3)
	{
		string str;
		float newValue;

		if (value < 0)
		{
			(str, newValue) = (-value).SISizeFormat(maxDigits, decimalPlaces);
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
	/// <param name="descriptor"></param>
	/// <param name="connectionProperties"></param>
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



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs an extended get of a connection descriptor's value
	/// if it's underlying connection properties object implements the dynamic
	/// <see cref="ICustomTypeDescriptor"/> interface.
	/// </summary>
	/// <param name="descriptor"></param>
	/// <param name="connectionProperties"></param>
	/// <returns>The descriptor's current value</returns>
	// ---------------------------------------------------------------------------------
	public static object GetValueX(this PropertyDescriptor descriptor, IBPropertyAgent connectionProperties)
	{
		object component = connectionProperties;

		if (connectionProperties is ICustomTypeDescriptor customTypeDescriptor)
		{
			component = customTypeDescriptor.GetPropertyOwner(descriptor);
		}

		return descriptor.GetValue(component);
	}



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


	public static bool IsLocalAccount(this string account, StringComparison stringComparison = StringComparison.InvariantCultureIgnoreCase)
	{
		/*
		if (!account.IsDataLakeLocalAccount(stringComparison))
		{
			return account.IsCosmosLocalAccount(stringComparison);
		}
		*/
		return true;
	}


	internal static bool IsSqlException(this Exception ex)
	{
		return ex.IsExceptionType(typeof(FbException));
	}

	public static string QuoteNameIfNeed(this string name)
	{
		if (!string.IsNullOrEmpty(name))
		{
			return "[" + name.Replace("]", "]]") + "]";
		}

		return string.Empty;
	}



	public static string Res(this string value, object arg0)
	{
		return string.Format(CultureInfo.CurrentCulture, value, arg0);

	}

	public static string Res(this string value, object arg0, object arg1)
	{
		return string.Format(CultureInfo.CurrentCulture, value, arg0, arg1);

	}

	public static string Res(this string value, object arg0, object arg1, object arg2)
	{
		return string.Format(CultureInfo.CurrentCulture, value, arg0, arg1, arg2);

	}

	public static string Res(this string value, object arg0, object arg1, object arg2, object arg3)
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

	public static string SecureStringToString(this SecureString secureString)
	{
		return new string(secureString.SecureStringToCharArray());
	}

	public static SecureString StringToSecureString(this string unsecureString)
	{
		return unsecureString.ToCharArray().CharArrayToSecureString();
	}

	private static char[] SecureStringToCharArray(this SecureString secureString)
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


	public static DateTime ToDateTime(this long timestamp)
	{
		DateTimeOffset offset = DateTimeOffset.FromUnixTimeMilliseconds(timestamp);
		return offset.UtcDateTime;
	}


	public static long ToUnixMilliseconds(this DateTime value)
	{
		return ((DateTimeOffset)value).ToUnixTimeMilliseconds();
	}



	private static SecureString CharArrayToSecureString(this IEnumerable<char> charArray)
	{
		SecureString secureString = new SecureString();
		foreach (char item in charArray)
		{
			secureString.AppendChar(item);
		}

		secureString.MakeReadOnly();
		return secureString;
	}

	public static string GetServer(this FbException exception)
	{
		if (exception == null)
			return null;

		if (exception.Data.Contains("Server"))
			return (string)exception.Data["Server"];

		return null;
	}

	public static void SetServer(this FbException exception, string value)
	{
		if (exception == null)
			return;

		exception.Data["Server"] = value;
	}

	public static int GetNumber(this FbException exception)
	{
		if (exception == null)
			return -1;

		if (exception.Errors.Count <= 0)
			return -1;

		return exception.Errors.ElementAt(0).Number;
	}

	public static byte GetClass(this FbException exception)
	{
		if (exception == null)
			return 0;

		if (exception.Errors.Count <= 0)
			return 0;

		return exception.Errors.ElementAt(0).Class;
	}

	public static int GetLineNumber(this FbException exception)
	{
		if (exception == null)
			return -1;

		if (exception.Errors.Count <= 0)
			return -1;

		return exception.Errors.ElementAt(0).LineNumber;
	}

	public static string GetState(this FbException exception)
	{
		if (exception == null)
			return null;


		return exception.SQLSTATE;
	}

	public static string GetProcedure(this FbException exception)
	{
		if (exception == null)
			return null;


		return exception.TargetSite.Name;
	}

}
