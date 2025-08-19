// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BlackbirdSql.Sys.Properties;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Shell;



namespace BlackbirdSql;



// =========================================================================================================
//											ExtensionMembers Class
//
/// <summary>
/// Central class for package external class extension methods. 
/// </summary>
// =========================================================================================================
internal static partial class ExtensionMembers
{

	private static readonly string[] _S_ByteSizeSuffixes = [Resources.ByteSizeBytes, Resources.ByteSizeKB,
		Resources.ByteSizeMB, Resources.ByteSizeGB, Resources.ByteSizeTB, Resources.ByteSizePB,
		Resources.ByteSizeEB, Resources.ByteSizeZB, Resources.ByteSizeYB];

	private static readonly string[] _S_SIExponents = [null, "\x00b9", "\x00b2", "\x00b3", "\x2074",
		"\x2075", "\x2076", "\x2077", "\x2078", "\x2079", "\x00b9\x2070", "\x00b9\x00b9", "\x00b9\x00b2",
		"\x00b9\x00b3", "\x00b9\x2074", "\x00b9\x2075", "\x00b9\x2076", "\x00b9\x2077", "\x00b9\x2078",
		"\x00b9\x2079", "\x00b2\x2070", "\x00b2\x00b9", "\x00b2\x00b2", "\x00b2\x00b3", "\x00b2\x2074",
		"\x00b2\x2075", "\x00b2\x2076", "\x00b2\x2077", "\x00b2\x2078", "\x00b2\x2079"];




	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns the int boolean value.
	/// </summary>
	// ---------------------------------------------------------------------------------
	internal static bool AsBool(this int @this)
	{
		return @this != 0;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns the uint boolean value.
	/// </summary>
	// ---------------------------------------------------------------------------------
	internal static bool AsBool(this uint @this)
	{
		return @this != 0;
	}


	internal static int AsInt(this bool @this)
	{
		return @this ? 1 : 0;
	}
	internal static TResult AwaiterResult<TResult>(this Task<TResult> @this)
	{
		return @this.GetAwaiter().GetResult();
	}



	internal static bool Cancelled(this CancellationToken @this) => @this.IsCancellationRequested;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns the decrypted ConnectionString of an <see cref="IVsDataConnection"/>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	internal static string DecryptedConnectionString(this IVsDataConnection @this)
	{
		return DataProtection.DecryptString(@this.EncryptedConnectionString);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns the decrypted ConnectionString of an
	/// <see cref="IVsDataExplorerConnection"/>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	internal static string DecryptedConnectionString(this IVsDataExplorerConnection @this)
	{
		return DataProtection.DecryptString(@this.EncryptedConnectionString);
	}



	internal static IVsDataExplorerConnection ExplorerConnection(this IDbConnection @this, bool canBeWeakEquivalent = false)
	{
		IVsDataExplorerConnectionManager manager = ApcManager.ExplorerConnectionManager;

		(_, IVsDataExplorerConnection explorerConnection) = manager.SearchExplorerConnectionEntry(@this.ConnectionString, false, canBeWeakEquivalent);

		return explorerConnection;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Formats a resource string format string given arguments using CurrentCulture.
	/// </summary>
	// ---------------------------------------------------------------------------------
	internal static string Fmt(this string @this, params object[] args)
	{
		try
		{
			return string.Format(CultureInfo.CurrentCulture, @this, args);
		}
		catch (Exception ex)
		{
			string msg = $"Format failure: {@this}\nArgs: ";

			for (int i = 0; i < args.Length; i++)
			{
				try
				{
					msg += $"[{i}]: {args[i]}, ";
				}
				catch
				{
					msg += $"[{i}]: badarg, ";
				}
			}

			Diag.StackException(ex, msg);
		}

		return @this;
	}

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Formats a resource string format string given arguments using InvariantCulture.
	/// </summary>
	// ---------------------------------------------------------------------------------
	internal static string Fmti(this string @this, params object[] args)
	{
		try
		{
			return string.Format(CultureInfo.InvariantCulture, @this, args);
		}
		catch (Exception ex)
		{
			string msg = $"Format failure: {@this}\nArgs: ";

			for (int i = 0; i < args.Length; i++)
			{
				try
				{
					msg += $"[{i}]: {args[i]}, ";
				}
				catch
				{
					msg += $"[{i}]: badarg, ";
				}
			}

			Diag.StackException(ex, msg);
		}

		return @this;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Formats time span in the format 00:00:00:00.000. If trim is true, trims extraneous
	/// 00:.
	/// </summary>
	// ---------------------------------------------------------------------------------
	internal static string Fmt(this TimeSpan @this, bool trim = false)
	{
		string result = @this.ToString();

		if (!string.IsNullOrEmpty(result))
		{
			string numberDecimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
			int pos = result.LastIndexOf(numberDecimalSeparator, StringComparison.Ordinal);
			if (pos != -1)
			{
				int num2 = pos + 4;
				if (num2 <= result.Length)
				{
					result = result[..num2];
				}
			}

			if (trim)
			{
				while (result.StartsWith("00:"))
					result = result.TrimPrefix("00:");

				if (result.Length > 3 && result[0] == '0' && result[2] != ':')
					result = result[1..];
			}
		}

		return result;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Format a long byte size down to it's byte unit by decimal places.
	/// </summary>
	// ---------------------------------------------------------------------------------
	internal static (string, float) FmtByteSize(this long @this, int decimals = 3)
	{
		string str;
		float newValue;

		if (@this < 0)
		{
			(str, newValue) = (-@this).FmtByteSize(decimals);
			str = "-" + str;
			return (str, -newValue);
		}

		int i = 0;
		newValue = @this;

		while (@this > 999999L && Math.Round(newValue, decimals) >= 1000)
		{
			newValue /= 1024;
			i++;

			if (_S_ByteSizeSuffixes.Length <= i + 1)
				break;
		}

		if (decimals != 0 && Math.Round(newValue, 0) == Math.Round(newValue, decimals))
			decimals = 0;

		string format = Resources.FormatByteSize.Replace("{@decimals}", decimals.ToString());

		return (format.Fmt(newValue, _S_ByteSizeSuffixes[i]), newValue);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Format a float byte size down to it's byte unit by decimal places.
	/// </summary>
	// ---------------------------------------------------------------------------------
	internal static (string, float) FmtByteSize(this float @this, int decimals = 3)
	{
		string str;
		float newValue;

		if (@this < 0)
		{
			(str, newValue) = (-@this).FmtByteSize(decimals);
			str = "-" + str;
			return (str, -newValue);
		}

		int i = 0;
		newValue = @this;

		while (@this > 999999.999 && Math.Round(newValue, decimals) >= 1000)
		{
			newValue /= 1024;
			i++;

			if (_S_ByteSizeSuffixes.Length <= i + 1)
				break;
		}

		if (decimals != 0 && Math.Round(newValue, 0) == Math.Round(newValue, decimals))
			decimals = 0;

		string format = Resources.FormatByteSize.Replace("{@decimals}", decimals.ToString());

		return (format.Fmt(newValue, _S_ByteSizeSuffixes[i]), newValue);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Formats a long into superscipt exponential notation by decimal places
	/// </summary>
	// ---------------------------------------------------------------------------------
	internal static (string, float) FmtExpSize(this long @this, int maxDigits = 4, int decimals = 3)
	{
		string str;
		float newValue;

		if (@this < 0)
		{
			(str, newValue) = (-@this).FmtExpSize(maxDigits, decimals);
			str = "-" + str;
			return (str, -newValue);
		}

		int i = 0;
		newValue = @this;
		while (Math.Floor(newValue) >= Math.Pow(10, maxDigits))
		{
			newValue /= 10;
			i++;

			if (_S_SIExponents.Length <= i + 1)
				break;
		}

		if (i <= 1)
			return (@this.ToString(), @this);

		if (decimals != 0 && Math.Round(newValue, 0) == Math.Round(newValue, decimals))
			decimals = 0;

		string format = Resources.FormatExpByteSize.Replace("{@decimals}", decimals.ToString());

		return (format.Fmt(newValue, _S_SIExponents[i]), newValue);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Formats a float into superscipt exponential notation by decimal places
	/// </summary>
	// ---------------------------------------------------------------------------------
	internal static (string, float) FmtExpSize(this float @this, int maxDigits = 4, int decimals = 3)
	{
		string str;
		float newValue;

		if (@this < 0)
		{
			(str, newValue) = (-@this).FmtExpSize(maxDigits, decimals);
			str = "-" + str;
			return (str, -newValue);
		}

		int i = 0;
		newValue = (float)@this;
		while (Math.Floor(newValue) >= Math.Pow(10, maxDigits))
		{
			newValue /= 10;
			i++;

			if (_S_SIExponents.Length <= i + 1)
				break;
		}

		if (i <= 1)
			return (@this.ToString(), @this);

		if (decimals != 0 && Math.Round(newValue, 0) == Math.Round(newValue, decimals))
			decimals = 0;

		string format = Resources.FormatExpByteSize.Replace("{@decimals}", decimals.ToString());

		return (format.Fmt(newValue, _S_SIExponents[i]), newValue);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Formats time span for display in a window statusbar rounded to seconds.
	/// </summary>
	// ---------------------------------------------------------------------------------
	internal static string FmtRound(this TimeSpan @this)
	{
		return new TimeSpan(@this.Days, @this.Hours, @this.Minutes, @this.Seconds, 0).ToString();
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Checks if an exception or aggregate exception or it's inner exception
	/// is of a specific type.
	/// </summary>
	// ---------------------------------------------------------------------------------
	internal static bool HasExceptionType<T>(this Exception @this) where T : class
	{
		if (@this == null)
			return false;

		if (@this as T != null)
			return true;


		if (@this is AggregateException ex2)
		{
			if (ex2.InnerExceptions != null)
				return ex2.InnerExceptions.Any((inner) => inner.HasExceptionType<T>());

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
	/// Performs a safe search fir an ExplorerConnection entry from the ConnectionUrl
	/// of a provided ConnectionString.
	/// </summary>
	/// <returns>
	/// The tuple (label, explorerConnection)
	/// </returns>
	// ---------------------------------------------------------------------------------
	internal static (string, IVsDataExplorerConnection) SearchExplorerConnectionEntry(
		this IVsDataExplorerConnectionManager @this, string connectionString, bool encrypted, bool canBeWeakEquivalent = false)
	{
		string unencryptedConnectionString = encrypted ? DataProtection.DecryptString(connectionString) : connectionString;

		if (ApcManager.ProviderGuid == null)
			return (null, null);

		Guid clsidProvider = new(ApcManager.ProviderGuid);
		string connectionUrl = ApcManager.CreateConnectionUrl(unencryptedConnectionString);


		IDictionary<string, IVsDataExplorerConnection> connections = @this.Connections;


		foreach (KeyValuePair<string, IVsDataExplorerConnection> pair in connections)
		{
			if (!(clsidProvider == pair.Value.Provider))
				continue;

			if (ApcManager.CreateConnectionUrl(pair.Value.DecryptedConnectionString()) == connectionUrl)
				return (pair.Key, pair.Value);
		}

		if (!canBeWeakEquivalent)
			return (null, null);


		foreach (KeyValuePair<string, IVsDataExplorerConnection> pair in connections)
		{
			if (!(clsidProvider == pair.Value.Provider))
				continue;

			if (ApcManager.IsWeakConnectionEquivalency(unencryptedConnectionString, pair.Value.DecryptedConnectionString()))
				return (pair.Key, pair.Value);
		}

		return (null, null);
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
	internal static int SetSelectedIndexX(this ComboBox @this, int index)
	{
		@this.SelectedIndex = index;

		return @this.SelectedIndex;
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
	internal static object SetSelectedValueX(this ComboBox @this, object value)
	{
		if (value == null)
		{
			@this.SelectedIndex = -1;
			return null;
		}

		int result;

		if (value is string str)
			result = @this.FindStringExact(str);
		else
			result = @this.FindStringExact(value.ToString());

		@this.SelectedIndex = result;

		return result == -1 ? null : value;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Converts a long timestamp to a UTC DateTime.
	/// </summary>
	// ---------------------------------------------------------------------------------
	internal static DateTime ToUtcDateTime(this long @this)
	{
		DateTimeOffset offset = DateTimeOffset.FromUnixTimeMilliseconds(@this);
		return offset.UtcDateTime;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Converts a DateTime to it's long unix milliseconds timestamp.
	/// </summary>
	// ---------------------------------------------------------------------------------
	internal static long UnixMilliseconds(this DateTime @this)
	{
		return ((DateTimeOffset)@this).ToUnixTimeMilliseconds();
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Converts a DateTime to it's long unix seconds timestamp.
	/// </summary>
	// ---------------------------------------------------------------------------------
	internal static long UnixSeconds(this DateTime @this)
	{
		return ((DateTimeOffset)@this).ToUnixTimeSeconds();
	}


}
