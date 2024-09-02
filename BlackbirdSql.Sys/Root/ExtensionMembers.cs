// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BlackbirdSql.Sys.Enums;
using BlackbirdSql.Sys.Interfaces;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;
using Microsoft.VisualStudio.Shell;



namespace BlackbirdSql;

[SuppressMessage("Usage", "VSTHRD002:Avoid problematic synchronous waits", Justification = "For StartNew")]


// =========================================================================================================
//											ExtensionMembers Class
//
/// <summary>
/// Central class for package external class extension methods. 
/// </summary>
// =========================================================================================================
public static partial class ExtensionMembers
{

	private static readonly string[] S_ByteSizeSuffixes =
		["bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"];

	private static readonly string[] S_SIExponents =
		[null,
			"\x00b9",
			"\x00b2",
			"\x00b3",
			"\x2074",
			"\x2075",
			"\x2076",
			"\x2077",
			"\x2078",
			"\x2079",
			"\x00b9\x2070",
			"\x00b9\x00b9",
			"\x00b9\x00b2",
			"\x00b9\x00b3",
			"\x00b9\x2074",
			"\x00b9\x2075",
			"\x00b9\x2076",
			"\x00b9\x2077",
			"\x00b9\x2078",
			"\x00b9\x2079",
			"\x00b2\x2070",
			"\x00b2\x00b9",
			"\x00b2\x00b2",
			"\x00b2\x00b3",
			"\x00b2\x2074",
			"\x00b2\x2075",
			"\x00b2\x2076",
			"\x00b2\x2077",
			"\x00b2\x2078",
			"\x00b2\x2079"];





	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns the int boolean value.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static bool AsBool(this int @this)
	{
		return @this != 0;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns the uint boolean value.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static bool AsBool(this uint @this)
	{
		return @this != 0;
	}


	public static int AsInt(this bool @this)
	{
		return @this ? 1 : 0;
	}
	public static TResult AwaiterResult<TResult>(this Task<TResult> @this)
	{
		return @this.GetAwaiter().GetResult();
	}



	public static bool Cancelled(this CancellationToken @this) => @this.IsCancellationRequested;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns the decrypted ConnectionString of an <see cref="IVsDataConnection"/>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static string DecryptedConnectionString(this IVsDataConnection @this)
	{
		return DataProtection.DecryptString(@this.EncryptedConnectionString);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns the decrypted ConnectionString of an
	/// <see cref="IVsDataExplorerConnection"/>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static string DecryptedConnectionString(this IVsDataExplorerConnection @this)
	{
		return DataProtection.DecryptString(@this.EncryptedConnectionString);
	}



	public static IVsDataExplorerConnection ExplorerConnection(this IDbConnection @this, bool canBeWeakEquivalent = false)
	{
		IVsDataExplorerConnectionManager manager = ApcManager.ExplorerConnectionManager;

		(_, IVsDataExplorerConnection explorerConnection) = manager.SearchExplorerConnectionEntry(@this.ConnectionString, false, canBeWeakEquivalent);

		return explorerConnection;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Formats time span for display in statistics output.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static string FmtStats(this TimeSpan @this, bool trim = false)
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
	public static (string, float) FmtByteSize(this long @this, int decimalPlaces = 3)
	{
		string str;
		float newValue;

		if (@this < 0)
		{
			(str, newValue) = (-@this).FmtByteSize(decimalPlaces);
			str = "-" + str;
			return (str, -newValue);
		}

		int i = 0;
		newValue = @this;

		while (@this > 999999L && Math.Round(newValue, decimalPlaces) >= 1000)
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



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Format a float byte size down to it's byte unit by decimal places.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static (string, float) FmtByteSize(this float @this, int decimalPlaces = 3)
	{
		string str;
		float newValue;

		if (@this < 0)
		{
			(str, newValue) = (-@this).FmtByteSize(decimalPlaces);
			str = "-" + str;
			return (str, -newValue);
		}

		int i = 0;
		newValue = @this;

		while (@this > 999999.999 && Math.Round(newValue, decimalPlaces) >= 1000)
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



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Formats a long into superscipt exponential notation by decimal places
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static (string, float) FmtExpSize(this long @this, int maxDigits = 4, int decimalPlaces = 3)
	{
		string str;
		float newValue;

		if (@this < 0)
		{
			(str, newValue) = (-@this).FmtExpSize(maxDigits, decimalPlaces);
			str = "-" + str;
			return (str, -newValue);
		}

		int i = 0;
		newValue = @this;
		while (Math.Floor(newValue) >= Math.Pow(10, maxDigits))
		{
			newValue /= 10;
			i++;

			if (S_SIExponents.Length <= i + 1)
				break;
		}

		if (i <= 1)
			return (@this.ToString(), @this);

		if (decimalPlaces != 0 && Math.Round(newValue, 0) == Math.Round(newValue, decimalPlaces))
			decimalPlaces = 0;

		return (string.Format("{0:n" + decimalPlaces + "} (10{1})", newValue, S_SIExponents[i]), newValue);

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Formats a float into superscipt exponential notation by decimal places
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static (string, float) FmtExpSize(this float @this, int maxDigits = 4, int decimalPlaces = 3)
	{
		string str;
		float newValue;

		if (@this < 0)
		{
			(str, newValue) = (-@this).FmtExpSize(maxDigits, decimalPlaces);
			str = "-" + str;
			return (str, -newValue);
		}

		int i = 0;
		newValue = (float)@this;
		while (Math.Floor(newValue) >= Math.Pow(10, maxDigits))
		{
			newValue /= 10;
			i++;

			if (S_SIExponents.Length <= i + 1)
				break;
		}

		if (i <= 1)
			return (@this.ToString(), @this);

		if (decimalPlaces != 0 && Math.Round(newValue, 0) == Math.Round(newValue, decimalPlaces))
			decimalPlaces = 0;

		return (string.Format("{0:n" + decimalPlaces + "} (10{1})", newValue, S_SIExponents[i]), newValue);

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Formats a resource string format string given arguments.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static string FmtRes(this string @this, params object[] args)
	{
		return string.Format(CultureInfo.CurrentCulture, @this, args);

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Formats time span ticks for display in statistics output.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static string FmtStats(this long @this, bool trim = false)
	{
		TimeSpan value = new(@this);

		return value.FmtStats(trim);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Formats time span for display in a window statusbar.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static string FmtStatus(this TimeSpan @this)
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
	public static (string, IVsDataExplorerConnection) SearchExplorerConnectionEntry(
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
	/// Sets the encrypted ConnectionString of an <see cref="IVsDataConnection"/> using
	/// an unencrypted string.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static void SetConnectionString(this IVsDataConnection @this, string connectionString)
	{
		@this.EncryptedConnectionString = DataProtection.EncryptString(connectionString);
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
	public static int SetSelectedIndexX(this ComboBox @this, int index)
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
	public static object SetSelectedValueX(this ComboBox @this, object value)
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



	public static string ToUpper(this EnConnectionSource @this)
	{
		return @this.ToString().ToUpper();
	}

	public static string ToUpper(this Guid @this)
	{
		return @this.ToString().ToUpper();
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Converts a long timestamp to a UTC DateTime.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static DateTime ToUtcDateTime(this long @this)
	{
		DateTimeOffset offset = DateTimeOffset.FromUnixTimeMilliseconds(@this);
		return offset.UtcDateTime;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Converts a DateTime to it's long unix milliseconds timestamp.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static long UnixMilliseconds(this DateTime @this)
	{
		return ((DateTimeOffset)@this).ToUnixTimeMilliseconds();
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Converts a DateTime to it's long unix seconds timestamp.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static long UnixSeconds(this DateTime @this)
	{
		return ((DateTimeOffset)@this).ToUnixTimeSeconds();
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Validates the IVsDataConnectionProperties Site for redundant or required
	/// registration properties.
	/// Determines if the ConnectionName (proposed DatsetKey) and DatasetId (proposed
	/// database name) are required in the Site.
	/// This cleanup ensures that proposed keys do not appear in connection dialogs
	/// and strings if they will have no impact on the final DatsetKey. 
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static bool ValidateKeys(this IVsDataConnectionProperties @this)
	{
		bool modified = false;
		// First the DatasetId. If it's equal to the Dataset we clear it because, by
		// default the trimmed filepath (Dataset) can be used.

		string database = @this.ContainsKey(SysConstants.C_KeyDatabase)
			? (string)@this[SysConstants.C_KeyDatabase] : null;

		if (database != null && string.IsNullOrWhiteSpace(database))
		{
			modified = true;
			database = null;
			@this.Remove(SysConstants.C_KeyDatabase);
		}

		string dataset;

		try
		{
			dataset = database != null
				? Cmd.GetFileNameWithoutExtension(database) : null;
		}
		catch (Exception ex)
		{
			Diag.Dug(ex, $"Database path: {database}.");
			throw;
		}


		string datasetId = @this.ContainsKey(SysConstants.C_KeyExDatasetId)
			? (string)@this[SysConstants.C_KeyExDatasetId] : null;

		if (datasetId != null)
		{
			if (string.IsNullOrWhiteSpace(datasetId))
			{
				// DatasetId exists and is invalid (empty). Delete it.
				modified = true;
				datasetId = null;
				@this.Remove(SysConstants.C_KeyExDatasetId);
			}

			if (datasetId != null && dataset != null && datasetId == dataset)
			{
				// If the DatasetId is equal to the Dataset it's also not needed. Delete it.
				modified = true;
				datasetId = null;
				@this.Remove(SysConstants.C_KeyExDatasetId);
			}
		}

		// Now that the datasetId is established, we can determined its default derived value
		// and the default derived value of the datasetKey.
		string derivedDatasetId = datasetId ?? (dataset ?? null);

		string dataSource = @this.ContainsKey(SysConstants.C_KeyDataSource)
			? (string)@this[SysConstants.C_KeyDataSource] : null;

		if (dataSource != null && string.IsNullOrWhiteSpace(dataSource))
		{
			modified = true;
			dataSource = null;
			@this.Remove(SysConstants.C_KeyDataSource);
		}


		string derivedConnectionName = (dataSource != null && derivedDatasetId != null)
			? SysConstants.S_DatasetKeyFormat.FmtRes(dataSource, derivedDatasetId) : null;
		string derivedAlternateConnectionName = (dataSource != null && derivedDatasetId != null)
			? SysConstants.S_DatasetKeyAlternateFormat.FmtRes(dataSource, derivedDatasetId) : null;


		// Now the proposed DatasetKey, ConnectionName. If it exists and is equal to the derived
		// Datsetkey, it's also not needed.

		string connectionName = @this.ContainsKey(SysConstants.C_KeyExConnectionName)
			? (string)@this[SysConstants.C_KeyExConnectionName] : null;

		if (connectionName != null && string.IsNullOrWhiteSpace(connectionName))
		{
			modified = true;
			connectionName = null;
			@this.Remove(SysConstants.C_KeyExConnectionName);
		}

		if (connectionName != null)
		{
			// If the ConnectionName (proposed DatsetKey) is equal to the default
			// derived datasetkey it also won't be needed, so delete it,
			// else the ConnectionName still exists and is the determinant, so
			// any existing proposed DatasetId is not required.
			if (connectionName == derivedConnectionName || connectionName == derivedAlternateConnectionName)
			{
				modified = true;
				@this.Remove(SysConstants.C_KeyExConnectionName);
			}
			else if (datasetId != null)
			{
				// If ConnectionName exists the DatasetId is not needed. Delete it.
				modified = true;
				@this.Remove(SysConstants.C_KeyExDatasetId);
			}
		}

		return modified;

	}


}
