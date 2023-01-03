//
// BlackbirdSql - ConnectionParameters have been seperated into their own static for brevity
//
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
using System.Data;
using System.Data.Common;
using System.ComponentModel;

using BlackbirdSql.Common;



namespace BlackbirdSql.Data.Common;

public class ConnectionStringBuilder : DbConnectionStringBuilder
{
	#region Properties

	[Category("Security")]
	[DisplayName("User ID")]
	[Description("Indicates the User ID to be used when connecting to the data source.")]
	[DefaultValue(ConnectionParameters.DefaultValueUserId)]
	public string UserID
	{
		get { return DbConnectionString.GetString(GetKey(ConnectionParameters.DefaultKeyUserId), base.TryGetValue, ConnectionParameters.DefaultValueUserId); }
		set { SetValue(ConnectionParameters.DefaultKeyUserId, value); }
	}

	[Category("Security")]
	[DisplayName("Password")]
	[Description("Indicates the password to be used when connecting to the data source.")]
	[PasswordPropertyText(true)]
	[DefaultValue(ConnectionParameters.DefaultValuePassword)]
	public string Password
	{
		get { return DbConnectionString.GetString(GetKey(ConnectionParameters.DefaultKeyPassword), base.TryGetValue, ConnectionParameters.DefaultValuePassword); }
		set { SetValue(ConnectionParameters.DefaultKeyPassword, value); }
	}

	[Category("Source")]
	[DisplayName("Data Source")]
	[Description("The name of the Firebird server to which to connect.")]
	[DefaultValue(ConnectionParameters.DefaultValueDataSource)]
	public string DataSource
	{
		get { return DbConnectionString.GetString(GetKey(ConnectionParameters.DefaultKeyDataSource), base.TryGetValue, ConnectionParameters.DefaultValueDataSource); }
		set { SetValue(ConnectionParameters.DefaultKeyDataSource, value); }
	}

	[Category("Source")]
	[DisplayName("Initial Catalog")]
	[Description("The name of the actual database or the database to be used when a connection is open. It is normally the path to an .FDB file or an alias.")]
	[DefaultValue(ConnectionParameters.DefaultValueCatalog)]
	public string Database
	{
		get { return DbConnectionString.GetString(GetKey(ConnectionParameters.DefaultKeyCatalog), base.TryGetValue, ConnectionParameters.DefaultValueCatalog); }
		set { SetValue(ConnectionParameters.DefaultKeyCatalog, value); }
	}

	[Category("Source")]
	[DisplayName("Port Number")]
	[Description("Port to use for TCP/IP connections")]
	[DefaultValue(ConnectionParameters.DefaultValuePortNumber)]
	public int Port
	{
		get { return DbConnectionString.GetInt32(GetKey(ConnectionParameters.DefaultKeyPortNumber), base.TryGetValue, ConnectionParameters.DefaultValuePortNumber); }
		set { SetValue(ConnectionParameters.DefaultKeyPortNumber, value); }
	}

	[Category("Advanced")]
	[DisplayName("Packet Size")]
	[Description("The size (in bytes) of network packets. PacketSize may be in the range 512-32767 bytes.")]
	[DefaultValue(ConnectionParameters.DefaultValuePacketSize)]
	public int PacketSize
	{
		get { return DbConnectionString.GetInt32(GetKey(ConnectionParameters.DefaultKeyPacketSize), base.TryGetValue, ConnectionParameters.DefaultValuePacketSize); }
		set { SetValue(ConnectionParameters.DefaultKeyPacketSize, value); }
	}

	[Category("Security")]
	[DisplayName("Role Name")]
	[Description("The user role.")]
	[DefaultValue(ConnectionParameters.DefaultValueRoleName)]
	public string Role
	{
		get { return DbConnectionString.GetString(GetKey(ConnectionParameters.DefaultKeyRoleName), base.TryGetValue, ConnectionParameters.DefaultValueRoleName); }
		set { SetValue(ConnectionParameters.DefaultKeyRoleName, value); }
	}

	[Category("Advanced")]
	[DisplayName("Dialect")]
	[Description("The database SQL dialect.")]
	[DefaultValue(ConnectionParameters.DefaultValueDialect)]
	public int Dialect
	{
		get { return DbConnectionString.GetInt32(GetKey(ConnectionParameters.DefaultKeyDialect), base.TryGetValue, ConnectionParameters.DefaultValueDialect); }
		set { SetValue(ConnectionParameters.DefaultKeyDialect, value); }
	}

	[Category("Advanced")]
	[DisplayName("Character Set")]
	[Description("The connection character set encoding.")]
	[DefaultValue(ConnectionParameters.DefaultValueCharacterSet)]
	public string Charset
	{
		get { return DbConnectionString.GetString(GetKey(ConnectionParameters.DefaultKeyCharacterSet), base.TryGetValue, ConnectionParameters.DefaultValueCharacterSet); }
		set { SetValue(ConnectionParameters.DefaultKeyCharacterSet, value); }
	}

	[Category("Connection")]
	[DisplayName("Connection Timeout")]
	[Description("The time (in seconds) to wait for a connection to open.")]
	[DefaultValue(ConnectionParameters.DefaultValueConnectionTimeout)]
	public int ConnectionTimeout
	{
		get { return DbConnectionString.GetInt32(GetKey(ConnectionParameters.DefaultKeyConnectionTimeout), base.TryGetValue, ConnectionParameters.DefaultValueConnectionTimeout); }
		set { SetValue(ConnectionParameters.DefaultKeyConnectionTimeout, value); }
	}

	[Category("Pooling")]
	[DisplayName("Pooling")]
	[Description("When true the connection is grabbed from a pool or, if necessary, created and added to the appropriate pool.")]
	[DefaultValue(ConnectionParameters.DefaultValuePooling)]
	public bool Pooling
	{
		get { return DbConnectionString.GetBoolean(GetKey(ConnectionParameters.DefaultKeyPooling), base.TryGetValue, ConnectionParameters.DefaultValuePooling); }
		set { SetValue(ConnectionParameters.DefaultKeyPooling, value); }
	}

	[Category("Connection")]
	[DisplayName("Connection LifeTime")]
	[Description("When a connection is returned to the pool, its creation time is compared with the current time, and the connection is destroyed if that time span (in seconds) exceeds the value specified by connection lifetime.")]
	[DefaultValue(ConnectionParameters.DefaultValueConnectionLifetime)]
	public int ConnectionLifeTime
	{
		get { return DbConnectionString.GetInt32(GetKey(ConnectionParameters.DefaultKeyConnectionLifetime), base.TryGetValue, ConnectionParameters.DefaultValueConnectionLifetime); }
		set { SetValue(ConnectionParameters.DefaultKeyConnectionLifetime, value); }
	}

	[Category("Pooling")]
	[DisplayName("Min Pool Size")]
	[Description("The minimun number of connections allowed in the pool.")]
	[DefaultValue(ConnectionParameters.DefaultValueMinPoolSize)]
	public int MinPoolSize
	{
		get { return DbConnectionString.GetInt32(GetKey(ConnectionParameters.DefaultKeyMinPoolSize), base.TryGetValue, ConnectionParameters.DefaultValueMinPoolSize); }
		set { SetValue(ConnectionParameters.DefaultKeyMinPoolSize, value); }
	}

	[Category("Pooling")]
	[DisplayName("Max Pool Size")]
	[Description("The maximum number of connections allowed in the pool.")]
	[DefaultValue(ConnectionParameters.DefaultValueMaxPoolSize)]
	public int MaxPoolSize
	{
		get { return DbConnectionString.GetInt32(GetKey(ConnectionParameters.DefaultKeyMaxPoolSize), base.TryGetValue, ConnectionParameters.DefaultValueMaxPoolSize); }
		set { SetValue(ConnectionParameters.DefaultKeyMaxPoolSize, value); }
	}

	[Category("Advanced")]
	[DisplayName("Fetch Size")]
	[Description("The maximum number of rows to be fetched in a single call to read into the internal row buffer.")]
	[DefaultValue(ConnectionParameters.DefaultValueFetchSize)]
	public int FetchSize
	{
		get { return DbConnectionString.GetInt32(GetKey(ConnectionParameters.DefaultKeyFetchSize), base.TryGetValue, ConnectionParameters.DefaultValueFetchSize); }
		set { SetValue(ConnectionParameters.DefaultKeyFetchSize, value); }
	}

	[Category("Source")]
	[DisplayName("Server Type")]
	[Description("The type of server used.")]
	[DefaultValue(ConnectionParameters.DefaultValueServerType)]
	public ServerType ServerType
	{
		get { return GetServerType(ConnectionParameters.DefaultKeyServerType, ConnectionParameters.DefaultValueServerType); }
		set { SetValue(ConnectionParameters.DefaultKeyServerType, value); }
	}

	[Category("Advanced")]
	[DisplayName("Isolation Level")]
	[Description("The default Isolation Level for implicit transactions.")]
	[DefaultValue(ConnectionParameters.DefaultValueIsolationLevel)]
	public IsolationLevel IsolationLevel
	{
		get { return GetIsolationLevel(ConnectionParameters.DefaultKeyIsolationLevel, ConnectionParameters.DefaultValueIsolationLevel); }
		set { SetValue(ConnectionParameters.DefaultKeyIsolationLevel, value); }
	}

	[Category("Advanced")]
	[DisplayName("Records Affected")]
	[Description("Get the number of rows affected by a command when true.")]
	[DefaultValue(ConnectionParameters.DefaultValueRecordsAffected)]
	public bool ReturnRecordsAffected
	{
		get { return DbConnectionString.GetBoolean(GetKey(ConnectionParameters.DefaultKeyRecordsAffected), base.TryGetValue, ConnectionParameters.DefaultValueRecordsAffected); }
		set { SetValue(ConnectionParameters.DefaultKeyRecordsAffected, value); }
	}

	[Category("Pooling")]
	[DisplayName("Enlist")]
	[Description("If true, enlists the connections in the current transaction.")]
	[DefaultValue(ConnectionParameters.DefaultValuePooling)]
	public bool Enlist
	{
		get { return DbConnectionString.GetBoolean(GetKey(ConnectionParameters.DefaultKeyEnlist), base.TryGetValue, ConnectionParameters.DefaultValueEnlist); }
		set { SetValue(ConnectionParameters.DefaultKeyEnlist, value); }
	}

	[Category("Advanced")]
	[DisplayName("Client Library")]
	[Description("Client library for Firebird Embedded.")]
	[DefaultValue(ConnectionParameters.DefaultValueClientLibrary)]
	public string ClientLibrary
	{
		get { return DbConnectionString.GetString(GetKey(ConnectionParameters.DefaultKeyClientLibrary), base.TryGetValue, ConnectionParameters.DefaultValueClientLibrary); }
		set { SetValue(ConnectionParameters.DefaultKeyClientLibrary, value); }
	}

	[Category("Advanced")]
	[DisplayName("Cache Pages")]
	[Description("How many cache buffers to use for this session.")]
	[DefaultValue(ConnectionParameters.DefaultValueDbCachePages)]
	public int DbCachePages
	{
		get { return DbConnectionString.GetInt32(GetKey(ConnectionParameters.DefaultKeyDbCachePages), base.TryGetValue, ConnectionParameters.DefaultValueDbCachePages); }
		set { SetValue(ConnectionParameters.DefaultKeyDbCachePages, value); }
	}

	[Category("Advanced")]
	[DisplayName("No Db Triggers")]
	[Description("Disables database triggers for this connection.")]
	[DefaultValue(ConnectionParameters.DefaultValueNoDbTriggers)]
	public bool NoDatabaseTriggers
	{
		get { return DbConnectionString.GetBoolean(GetKey(ConnectionParameters.DefaultKeyNoDbTriggers), base.TryGetValue, ConnectionParameters.DefaultValueNoDbTriggers); }
		set { SetValue(ConnectionParameters.DefaultKeyNoDbTriggers, value); }
	}

	[Category("Advanced")]
	[DisplayName("No Garbage Collect")]
	[Description("If true, disables sweeping the database upon attachment.")]
	[DefaultValue(ConnectionParameters.DefaultValueNoGarbageCollect)]
	public bool NoGarbageCollect
	{
		get { return DbConnectionString.GetBoolean(GetKey(ConnectionParameters.DefaultKeyNoGarbageCollect), base.TryGetValue, ConnectionParameters.DefaultValueNoGarbageCollect); }
		set { SetValue(ConnectionParameters.DefaultKeyNoGarbageCollect, value); }
	}

	[Category("Advanced")]
	[DisplayName("Compression")]
	[Description("Enables or disables wire compression.")]
	[DefaultValue(ConnectionParameters.DefaultValueCompression)]
	public bool Compression
	{
		get { return DbConnectionString.GetBoolean(GetKey(ConnectionParameters.DefaultKeyCompression), base.TryGetValue, ConnectionParameters.DefaultValueCompression); }
		set { SetValue(ConnectionParameters.DefaultKeyCompression, value); }
	}

	[Category("Advanced")]
	[DisplayName("Crypt Key")]
	[Description("Key used for database decryption.")]
	[DefaultValue(ConnectionParameters.DefaultValueCryptKey)]
	public byte[] CryptKey
	{
		get { return GetBytes(ConnectionParameters.DefaultKeyCryptKey, ConnectionParameters.DefaultValueCryptKey); }
		set { SetValue(ConnectionParameters.DefaultKeyCryptKey, value); }
	}

	[Category("Advanced")]
	[DisplayName("Wire Crypt")]
	[Description("Selection for wire encryption.")]
	[DefaultValue(ConnectionParameters.DefaultValueWireCrypt)]
	public WireCrypt WireCrypt
	{
		get { return GetWireCrypt(ConnectionParameters.DefaultKeyWireCrypt, ConnectionParameters.DefaultValueWireCrypt); }
		set { SetValue(ConnectionParameters.DefaultKeyWireCrypt, value); }
	}

	[Category("Advanced")]
	[DisplayName("Application Name")]
	[Description("The name of the application making the connection.")]
	[DefaultValue(ConnectionParameters.DefaultValueApplicationName)]
	public string ApplicationName
	{
		get { return DbConnectionString.GetString(GetKey(ConnectionParameters.DefaultKeyApplicationName), base.TryGetValue, ConnectionParameters.DefaultValueApplicationName); }
		set { SetValue(ConnectionParameters.DefaultKeyApplicationName, value); }
	}

	[Category("Advanced")]
	[DisplayName("Command Timeout")]
	[Description("The time (in seconds) for command execution.")]
	[DefaultValue(ConnectionParameters.DefaultValueCommandTimeout)]
	public int CommandTimeout
	{
		get { return DbConnectionString.GetInt32(GetKey(ConnectionParameters.DefaultKeyCommandTimeout), base.TryGetValue, ConnectionParameters.DefaultValueCommandTimeout); }
		set { SetValue(ConnectionParameters.DefaultKeyCommandTimeout, value); }
	}

	[Category("Advanced")]
	[DisplayName("Parallel Workers")]
	[Description("Number of parallel workers to use for certain operations in Firebird.")]
	[DefaultValue(ConnectionParameters.DefaultValueParallelWorkers)]
	public int ParallelWorkers
	{
		get { return DbConnectionString.GetInt32(GetKey(ConnectionParameters.DefaultKeyParallelWorkers), base.TryGetValue, ConnectionParameters.DefaultValueParallelWorkers); }
		set { SetValue(ConnectionParameters.DefaultKeyParallelWorkers, value); }
	}

	#endregion

	#region Constructors

	public ConnectionStringBuilder()
	{
		Diag.Dug();
	}

	public ConnectionStringBuilder(string connectionString) : this()
	{
		Diag.Dug("Connection string: " + connectionString);

		ConnectionString = connectionString;
	}

	#endregion

	#region Private methods

	private ServerType GetServerType(string keyword, ServerType defaultValue)
	{
		Diag.Dug();

		var key = GetKey(keyword);
		if (!TryGetValue(key, out var value))
			return defaultValue;
		switch (value)
		{
			case ServerType fbServerType:
				return fbServerType;
			case string s when Enum.TryParse<ServerType>(s, true, out var enumResult):
				return enumResult;
			default:
				return DbConnectionString.GetServerType(key, base.TryGetValue, defaultValue);
		}
	}

	private IsolationLevel GetIsolationLevel(string keyword, IsolationLevel defaultValue)
	{
		Diag.Dug();

		var key = GetKey(keyword);
		if (!TryGetValue(key, out var value))
			return defaultValue;

		switch (value)
		{
			case IsolationLevel isolationLevel:
				return isolationLevel;
			case string s when Enum.TryParse<IsolationLevel>(s, true, out var enumResult):
				return enumResult;
			default:
				return DbConnectionString.GetIsolationLevel(key, base.TryGetValue, defaultValue);
		}
	}

	private WireCrypt GetWireCrypt(string keyword, WireCrypt defaultValue)
	{
		Diag.Dug();

		var key = GetKey(keyword);
		if (!TryGetValue(key, out var value))
			return defaultValue;
		switch (value)
		{
			case WireCrypt fbWireCrypt:
				return fbWireCrypt;
			case string s when Enum.TryParse<WireCrypt>(s, true, out var enumResult):
				return enumResult;
			default:
				return DbConnectionString.GetWireCrypt(key, base.TryGetValue, defaultValue);
		}
	}

	private byte[] GetBytes(string keyword, byte[] defaultValue)
	{
		Diag.Dug();

		var key = GetKey(keyword);

		if (key == null)
			return defaultValue;

		if (!TryGetValue(key, out var value))
			return defaultValue;
		switch (value)
		{
			case byte[] bytes:
				return bytes;
			case string s:
				return Convert.FromBase64String(s);
			default:
				return defaultValue;
		}
	}

	private void SetValue<T>(string keyword, T value)
	{
		Diag.Dug();

		var key = GetKey(keyword);
		if (value is byte[] bytes)
		{
			this[key] = Convert.ToBase64String(bytes);
		}
		else
		{
			this[key] = value;
		}
	}

	private string GetKey(string keyword)
	{
		Diag.Dug();

		string synonymKey = ConnectionParameters.Synonyms[keyword];

		foreach (string key in Keys)
		{
			if (ConnectionParameters.Synonyms.ContainsKey(key) && ConnectionParameters.Synonyms[key] == synonymKey)
			{
				synonymKey = key;
				break;
			}
		}
		return synonymKey;
	}

	#endregion
}
