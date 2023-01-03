/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/BlackbirdSQL/NETProvider/master/license.txt.
 *
 *    Software distributed under the License is distributed on
 *    an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *    express or implied. See the License for the specific
 *    language governing rights and limitations under the License.
 *
 *    All Rights Reserved.
 */

//$OriginalAuthors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using BlackbirdSql.Common;



namespace BlackbirdSql.Data.Common;

internal sealed class DbConnectionString
{

	#region Fields

	private Dictionary<string, object> _options;

	#endregion

	#region Properties

	public string UserID => GetString(ConnectionParameters.DefaultKeyUserId, _options.TryGetValue);
	public string Password => GetString(ConnectionParameters.DefaultKeyPassword, _options.TryGetValue);
	public string DataSource => GetString(ConnectionParameters.DefaultKeyDataSource, _options.TryGetValue);
	public int Port => GetInt32(ConnectionParameters.DefaultKeyPortNumber, _options.TryGetValue);
	public string Database => ExpandDataDirectory(GetString(ConnectionParameters.DefaultKeyCatalog, _options.TryGetValue));
	public int PacketSize => GetInt32(ConnectionParameters.DefaultKeyPacketSize, _options.TryGetValue);
	public string Role => GetString(ConnectionParameters.DefaultKeyRoleName, _options.TryGetValue);
	public short Dialect => GetInt16(ConnectionParameters.DefaultKeyDialect, _options.TryGetValue);
	public string Charset => GetString(ConnectionParameters.DefaultKeyCharacterSet, _options.TryGetValue);
	public int ConnectionTimeout => GetInt32(ConnectionParameters.DefaultKeyConnectionTimeout, _options.TryGetValue);
	public bool Pooling => GetBoolean(ConnectionParameters.DefaultKeyPooling, _options.TryGetValue);
	public long ConnectionLifetime => GetInt64(ConnectionParameters.DefaultKeyConnectionLifetime, _options.TryGetValue);
	public int MinPoolSize => GetInt32(ConnectionParameters.DefaultKeyMinPoolSize, _options.TryGetValue);
	public int MaxPoolSize => GetInt32(ConnectionParameters.DefaultKeyMaxPoolSize, _options.TryGetValue);
	public int FetchSize => GetInt32(ConnectionParameters.DefaultKeyFetchSize, _options.TryGetValue);
	public ServerType ServerType => GetServerType(ConnectionParameters.DefaultKeyServerType, _options.TryGetValue);
	public IsolationLevel IsolationLevel => GetIsolationLevel(ConnectionParameters.DefaultKeyIsolationLevel, _options.TryGetValue);
	public bool ReturnRecordsAffected => GetBoolean(ConnectionParameters.DefaultKeyRecordsAffected, _options.TryGetValue);
	public bool Enlist => GetBoolean(ConnectionParameters.DefaultKeyEnlist, _options.TryGetValue);
	public string ClientLibrary => GetString(ConnectionParameters.DefaultKeyClientLibrary, _options.TryGetValue);
	public int DbCachePages => GetInt32(ConnectionParameters.DefaultKeyDbCachePages, _options.TryGetValue);
	public bool NoDatabaseTriggers => GetBoolean(ConnectionParameters.DefaultKeyNoDbTriggers, _options.TryGetValue);
	public bool NoGarbageCollect => GetBoolean(ConnectionParameters.DefaultKeyNoGarbageCollect, _options.TryGetValue);
	public bool Compression => GetBoolean(ConnectionParameters.DefaultKeyCompression, _options.TryGetValue);
	public byte[] CryptKey => GetBytes(ConnectionParameters.DefaultKeyCryptKey, _options.TryGetValue);
	public WireCrypt WireCrypt => GetWireCrypt(ConnectionParameters.DefaultKeyWireCrypt, _options.TryGetValue);
	public string ApplicationName => GetString(ConnectionParameters.DefaultKeyApplicationName, _options.TryGetValue);
	public int CommandTimeout => GetInt32(ConnectionParameters.DefaultKeyCommandTimeout, _options.TryGetValue);
	public int ParallelWorkers => GetInt32(ConnectionParameters.DefaultKeyParallelWorkers, _options.TryGetValue);

	#endregion

	#region Internal Properties
	internal string NormalizedConnectionString
	{
		get { return string.Join(";", _options.OrderBy(x => x.Key, StringComparer.Ordinal).Where(x => x.Value != null).Select(x => string.Format("{0}={1}", x.Key, WrapValueIfNeeded(x.Value.ToString())))); }
	}
	#endregion

	#region Constructors

	public DbConnectionString()
	{
		SetDefaultOptions();
	}

	public DbConnectionString(string connectionString)
		: this()
	{
		Load(connectionString);
	}

	#endregion

	#region Methods

	public void Validate()
	{
		if (
			(string.IsNullOrEmpty(Database)) ||
			(string.IsNullOrEmpty(DataSource) && ServerType != ServerType.Embedded) ||
			(string.IsNullOrEmpty(Charset))
		   )
		{
			Diag.Dug(true, "An invalid connection string argument has been supplied or a required connection string argument has not been supplied.");
			throw new ArgumentException("An invalid connection string argument has been supplied or a required connection string argument has not been supplied.");
		}
		if (Port <= 0 || Port > 65535)
		{
			Diag.Dug(true, "Incorrect port.");
			throw new ArgumentException("Incorrect port.");
		}
		if (MinPoolSize > MaxPoolSize)
		{
			Diag.Dug(true, "Incorrect pool size.");
			throw new ArgumentException("Incorrect pool size.");
		}
		if (Dialect < 1 || Dialect > 3)
		{
			Diag.Dug(true, "Incorrect database dialect it should be 1, 2, or 3.");
			throw new ArgumentException("Incorrect database dialect it should be 1, 2, or 3.");
		}
		if (PacketSize < 512 || PacketSize > 32767)
		{
			Diag.Dug(true, "'Packet Size' value of {0} is not valid.{1}The value should be an integer >= 512 and <= 32767.");
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "'Packet Size' value of {0} is not valid.{1}The value should be an integer >= 512 and <= 32767.", PacketSize, Environment.NewLine));
		}
		if (DbCachePages < 0)
		{
			Diag.Dug(true, "" + string.Format(CultureInfo.CurrentCulture, "'Cache Pages' value of {0} is not valid.{1}The value should be an integer >= 0.", DbCachePages, Environment.NewLine));
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "'Cache Pages' value of {0} is not valid.{1}The value should be an integer >= 0.", DbCachePages, Environment.NewLine));
		}
		if (Pooling && NoDatabaseTriggers)
		{
			Diag.Dug(true, "Cannot use Pooling and NoDatabaseTriggers together.");
			throw new ArgumentException("Cannot use Pooling and NoDatabaseTriggers together.");
		}
		if (ParallelWorkers < 0)
		{
			Diag.Dug(true, string.Format(CultureInfo.CurrentCulture, "'Parallel Workers' value of {0} is not valid.{1}The value should be an integer >= 0.", ParallelWorkers, Environment.NewLine));
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "'Parallel Workers' value of {0} is not valid.{1}The value should be an integer >= 0.", ParallelWorkers, Environment.NewLine));
		}
	}

	#endregion

	#region Private Methods

	private void Load(string connectionString)
	{
		const string KeyPairsRegex = "(([\\w\\s\\d]*)\\s*?=\\s*?\"([^\"]*)\"|([\\w\\s\\d]*)\\s*?=\\s*?'([^']*)'|([\\w\\s\\d]*)\\s*?=\\s*?([^\"';][^;]*))";

		if (!string.IsNullOrEmpty(connectionString))
		{
			var keyPairs = Regex.Matches(connectionString, KeyPairsRegex);

			foreach (Match keyPair in keyPairs)
			{
				if (keyPair.Groups.Count == 8)
				{
					var values = new string[]
					{
							(keyPair.Groups[2].Success ? keyPair.Groups[2].Value
								: keyPair.Groups[4].Success ? keyPair.Groups[4].Value
									: keyPair.Groups[6].Success ? keyPair.Groups[6].Value
										: string.Empty)
							.Trim().ToLowerInvariant(),
							(keyPair.Groups[3].Success ? keyPair.Groups[3].Value
								: keyPair.Groups[5].Success ? keyPair.Groups[5].Value
									: keyPair.Groups[7].Success ? keyPair.Groups[7].Value
										: string.Empty)
							.Trim()
					};

					if (values.Length == 2 && !string.IsNullOrEmpty(values[0]) && !string.IsNullOrEmpty(values[1]))
					{
						if (ConnectionParameters.Synonyms.TryGetValue(values[0], out var key))
						{
							switch (key)
							{
								case ConnectionParameters.DefaultKeyServerType:
									_options[key] = ParseEnum<ServerType>(values[1], ConnectionParameters.DefaultKeyServerType);
									break;
								case ConnectionParameters.DefaultKeyIsolationLevel:
									_options[key] = ParseEnum<IsolationLevel>(values[1], ConnectionParameters.DefaultKeyIsolationLevel);
									break;
								case ConnectionParameters.DefaultKeyCryptKey:
									var cryptKey = default(byte[]);
									try
									{
										cryptKey = Convert.FromBase64String(values[1]);
									}
									catch
									{
										throw NotSupported(ConnectionParameters.DefaultKeyCryptKey);
									}
									_options[key] = cryptKey;
									break;
								case ConnectionParameters.DefaultKeyWireCrypt:
									_options[key] = ParseEnum<WireCrypt>(values[1], ConnectionParameters.DefaultKeyWireCrypt);
									break;
								default:
									_options[key] = values[1];
									break;
							}
						}
					}
				}
			}

			if (!string.IsNullOrEmpty(Database))
			{
				ParseConnectionInfo(Database);
			}
		}
	}

	private void SetDefaultOptions()
	{
		_options = new Dictionary<string, object>(ConnectionParameters.DefaultValues);
	}

	// it is expected the hostname do be at least 2 characters to prevent possible ambiguity (DNET-892)
	private void ParseConnectionInfo(string connectionInfo)
	{
		connectionInfo = connectionInfo.Trim();

		{
			// URL style inet://[hostv6]:port/database
			var match = Regex.Match(connectionInfo, "^inet://\\[(?<host>[A-Za-z0-9:]{2,})\\]:(?<port>\\d+)/(?<database>.+)$");
			if (match.Success)
			{
				_options[ConnectionParameters.DefaultKeyCatalog] = match.Groups["database"].Value;
				_options[ConnectionParameters.DefaultKeyDataSource] = match.Groups["host"].Value;
				_options[ConnectionParameters.DefaultKeyPortNumber] = int.Parse(match.Groups["port"].Value, CultureInfo.InvariantCulture);
				return;
			}
		}
		{
			// URL style inet://host:port/database
			var match = Regex.Match(connectionInfo, "^inet://(?<host>[A-Za-z0-9\\.-]{2,}):(?<port>\\d+)/(?<database>.+)$");
			if (match.Success)
			{
				_options[ConnectionParameters.DefaultKeyCatalog] = match.Groups["database"].Value;
				_options[ConnectionParameters.DefaultKeyDataSource] = match.Groups["host"].Value;
				_options[ConnectionParameters.DefaultKeyPortNumber] = int.Parse(match.Groups["port"].Value, CultureInfo.InvariantCulture);
				return;
			}
		}
		{
			// URL style inet://host/database
			var match = Regex.Match(connectionInfo, "^inet://(?<host>[A-Za-z0-9\\.:-]{2,})/(?<database>.+)$");
			if (match.Success)
			{
				_options[ConnectionParameters.DefaultKeyCatalog] = match.Groups["database"].Value;
				_options[ConnectionParameters.DefaultKeyDataSource] = match.Groups["host"].Value;
				return;
			}
		}
		{
			// URL style inet:///database
			var match = Regex.Match(connectionInfo, "^inet:///(?<database>.+)$");
			if (match.Success)
			{
				_options[ConnectionParameters.DefaultKeyCatalog] = match.Groups["database"].Value;
				_options[ConnectionParameters.DefaultKeyDataSource] = "localhost";
				return;
			}
		}
		{
			// new style //[hostv6]:port/database
			var match = Regex.Match(connectionInfo, "^//\\[(?<host>[A-Za-z0-9:]{2,})\\]:(?<port>\\d+)/(?<database>.+)$");
			if (match.Success)
			{
				_options[ConnectionParameters.DefaultKeyCatalog] = match.Groups["database"].Value;
				_options[ConnectionParameters.DefaultKeyDataSource] = match.Groups["host"].Value;
				_options[ConnectionParameters.DefaultKeyPortNumber] = int.Parse(match.Groups["port"].Value, CultureInfo.InvariantCulture);
				return;
			}
		}
		{
			// new style //host:port/database
			var match = Regex.Match(connectionInfo, "^//(?<host>[A-Za-z0-9\\.-]{2,}):(?<port>\\d+)/(?<database>.+)$");
			if (match.Success)
			{
				_options[ConnectionParameters.DefaultKeyCatalog] = match.Groups["database"].Value;
				_options[ConnectionParameters.DefaultKeyDataSource] = match.Groups["host"].Value;
				_options[ConnectionParameters.DefaultKeyPortNumber] = int.Parse(match.Groups["port"].Value, CultureInfo.InvariantCulture);
				return;
			}
		}
		{
			// new style //host/database
			var match = Regex.Match(connectionInfo, "^//(?<host>[A-Za-z0-9\\.:-]{2,})/(?<database>.+)$");
			if (match.Success)
			{
				_options[ConnectionParameters.DefaultKeyCatalog] = match.Groups["database"].Value;
				_options[ConnectionParameters.DefaultKeyDataSource] = match.Groups["host"].Value;
				return;
			}
		}
		{
			// old style host:X:\database
			var match = Regex.Match(connectionInfo, "^(?<host>[A-Za-z0-9\\.:-]{2,}):(?<database>[A-Za-z]:\\\\.+)$");
			if (match.Success)
			{
				_options[ConnectionParameters.DefaultKeyCatalog] = match.Groups["database"].Value;
				_options[ConnectionParameters.DefaultKeyDataSource] = match.Groups["host"].Value;
				return;
			}
		}
		{
			// old style host/port:database
			var match = Regex.Match(connectionInfo, "^(?<host>[A-Za-z0-9\\.:-]{2,})/(?<port>\\d+):(?<database>.+)$");
			if (match.Success)
			{
				_options[ConnectionParameters.DefaultKeyCatalog] = match.Groups["database"].Value;
				_options[ConnectionParameters.DefaultKeyDataSource] = match.Groups["host"].Value;
				_options[ConnectionParameters.DefaultKeyPortNumber] = int.Parse(match.Groups["port"].Value, CultureInfo.InvariantCulture);
				return;
			}
		}
		{
			// old style host:database
			var match = Regex.Match(connectionInfo, "^(?<host>[A-Za-z0-9\\.:-]{2,}):(?<database>.+)$");
			if (match.Success)
			{
				_options[ConnectionParameters.DefaultKeyCatalog] = match.Groups["database"].Value;
				_options[ConnectionParameters.DefaultKeyDataSource] = match.Groups["host"].Value;
				return;
			}
		}

		_options[ConnectionParameters.DefaultKeyCatalog] = connectionInfo;
	}

	#endregion

	#region Internal Static Methods

	internal delegate bool TryGetValueDelegate(string key, out object value);

	internal static short GetInt16(string key, TryGetValueDelegate tryGetValue, short defaultValue = default)
	{
		return tryGetValue(key, out var value)
			? Convert.ToInt16(value, CultureInfo.InvariantCulture)
			: defaultValue;
	}

	internal static int GetInt32(string key, TryGetValueDelegate tryGetValue, int defaultValue = default)
	{
		return tryGetValue(key, out var value)
			? Convert.ToInt32(value, CultureInfo.InvariantCulture)
			: defaultValue;
	}

	internal static long GetInt64(string key, TryGetValueDelegate tryGetValue, long defaultValue = default)
	{
		return tryGetValue(key, out var value)
			? Convert.ToInt64(value, CultureInfo.InvariantCulture)
			: defaultValue;
	}

	internal static string GetString(string key, TryGetValueDelegate tryGetValue, string defaultValue = default)
	{
		return tryGetValue(key, out var value)
			? Convert.ToString(value, CultureInfo.InvariantCulture)
			: defaultValue;
	}

	internal static bool GetBoolean(string key, TryGetValueDelegate tryGetValue, bool defaultValue = default)
	{
		return tryGetValue(key, out var value)
			? Convert.ToBoolean(value, CultureInfo.InvariantCulture)
			: defaultValue;
	}

	internal static byte[] GetBytes(string key, TryGetValueDelegate tryGetValue, byte[] defaultValue = default)
	{
		return tryGetValue(key, out var value)
			? (byte[])value
			: defaultValue;
	}

	internal static ServerType GetServerType(string key, TryGetValueDelegate tryGetValue, ServerType defaultValue = ServerType.Default)
	{
		return tryGetValue(key, out var value)
			? (ServerType)value
			: defaultValue;
	}

	internal static IsolationLevel GetIsolationLevel(string key, TryGetValueDelegate tryGetValue, IsolationLevel defaultValue = default)
	{
		return tryGetValue(key, out var value)
			? (IsolationLevel)value
			: defaultValue;
	}

	internal static WireCrypt GetWireCrypt(string key, TryGetValueDelegate tryGetValue, WireCrypt defaultValue = default)
	{
		return tryGetValue(key, out var value)
			? (WireCrypt)value
			: defaultValue;
	}

	#endregion

	#region Private Static Methods

	private static string ExpandDataDirectory(string s)
	{
		const string DataDirectoryKeyword = "|DataDirectory|";
		if (s == null)
			return s;

		var dataDirectoryLocation = (string)AppDomain.CurrentDomain.GetData("DataDirectory") ?? string.Empty;
		var pattern = string.Format("{0}{1}?", Regex.Escape(DataDirectoryKeyword), Regex.Escape(Path.DirectorySeparatorChar.ToString()));
		return Regex.Replace(s, pattern, dataDirectoryLocation + Path.DirectorySeparatorChar, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
	}

	private static T ParseEnum<T>(string value, string name) where T : struct
	{
		if (!Enum.TryParse<T>(value, true, out var result))
			throw NotSupported(name);
		return result;
	}

	private static Exception NotSupported(string name) => new NotSupportedException($"Not supported '{name}'.");

	private static string WrapValueIfNeeded(string value)
	{
		if (value != null && value.Contains(';'))
			return "'" + value + "'";
		return value;
	}

	#endregion
}
