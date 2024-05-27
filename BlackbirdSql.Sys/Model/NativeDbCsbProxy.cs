
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;

using static BlackbirdSql.SysConstants;



namespace BlackbirdSql.Sys;


// =========================================================================================================
//
//									NativeDbCsbProxy Class
//
/// <summary>
/// Serves as a proxy class for the native DbConnectionStringBuilder.
/// Currently replicates FBConnectionStringBuilder.
/// </summary>
// =========================================================================================================
public class NativeDbCsbProxy : DbConnectionStringBuilder
{


	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - AbstractCsb
	// ---------------------------------------------------------------------------------

	public NativeDbCsbProxy()
	{ }

	public NativeDbCsbProxy(string connectionString)
		: this()
	{
		ConnectionString = connectionString;
	}


	#endregion Constructors / Destructors




	// =====================================================================================================
	#region Constants - AbstractCsb
	// =====================================================================================================


	#endregion Constants




	// =====================================================================================================
	#region Fields - AbstractCsb
	// =====================================================================================================


	// A protected 'this' object lock
	protected readonly object _LockObject = new();

	private static DescriberDictionary _Describers = null;


	#endregion Fields




	// =====================================================================================================
	#region Property accessors - AbstractCsb
	// =====================================================================================================



	/// <summary>
	/// Index accessor override to get back some uniformity in connection property naming.
	/// </summary>
	[Browsable(false)]
	public override object this[string key]
	{
		get
		{
			if (key == null)
				Diag.ThrowException(new ArgumentNullException(nameof(key)));


			lock (_LockObject)
				return GetValue(key);
		}
		set
		{
			if (key == null)
				throw new ArgumentNullException(nameof(key));

			lock (_LockObject)
				SetValue(key, value);
		}
	}


	public static DescriberDictionary Describers => _Describers ??= DbNative.Describers;


	[Category("Security")]
	[DisplayName("User ID")]
	[Description("Indicates the User ID to be used when connecting to the data source.")]
	[DefaultValue(C_DefaultUserID)]
	public string UserID
	{
		get { return (string)GetValue(C_KeyUserID); }
		set { SetValue(C_KeyUserID, value); }
	}

	[Category("Security")]
	[DisplayName("Password")]
	[Description("Indicates the password to be used when connecting to the data source.")]
	[PasswordPropertyText(true)]
	[DefaultValue(C_DefaultPassword)]
	public string Password
	{
		get { return (string)GetValue(C_KeyPassword); }
		set { SetValue(C_KeyPassword, value); }
	}

	[Category("Source")]
	[DisplayName("DataSource")]
	[Description("The name of the Database server to which to connect.")]
	[DefaultValue(C_DefaultDataSource)]
	public string DataSource
	{
		get { return (string)GetValue(C_KeyDataSource); }
		set { SetValue(C_KeyDataSource, value); }
	}

	[Category("Source")]
	[DisplayName("Database")]
	[Description("The name of the actual database or the database to be used when a connection is open. It is normally the path to an .FDB file or an alias.")]
	[DefaultValue(C_DefaultDatabase)]
	public string Database
	{
		get { return (string)GetValue(C_KeyDatabase); }
		set { SetValue(C_KeyDatabase, value); }
	}

	[Category("Source")]
	[DisplayName("Port")]
	[Description("Port to use for TCP/IP connections")]
	[DefaultValue(C_DefaultPort)]
	public int Port
	{
		get { return (int)GetValue(C_KeyPort); }
		set { SetValue(C_KeyPort, value); }
	}

	[Category("Advanced")]
	[DisplayName("PacketSize")]
	[Description("The size (in bytes) of network packets. PacketSize may be in the range 512-32767 bytes.")]
	[DefaultValue(C_DefaultPacketSize)]
	public int PacketSize
	{
		get { return (int)GetValue(C_KeyPacketSize); }
		set { SetValue(C_KeyPacketSize, value); }
	}

	[Category("Security")]
	[DisplayName("Role")]
	[Description("The user role.")]
	[DefaultValue(C_DefaultRole)]
	public string Role
	{
		get { return (string)GetValue(C_KeyRole); }
		set { SetValue(C_KeyRole, value); }
	}

	[Category("Advanced")]
	[DisplayName("Dialect")]
	[Description("The database SQL dialect.")]
	[DefaultValue(C_DefaultDialect)]
	public int Dialect
	{
		get { return (int)GetValue(C_KeyDialect); }
		set { SetValue(C_KeyDialect, value); }
	}

	[Category("Advanced")]
	[DisplayName("Character Set")]
	[Description("The connection character set encoding.")]
	[DefaultValue(C_DefaultCharset)]
	public string Charset
	{
		get { return (string)GetValue(C_KeyCharset); }
		set { SetValue(C_KeyCharset, value); }
	}

	[Category("Connection")]
	[DisplayName("Connection Timeout")]
	[Description("The time (in seconds) to wait for a connection to open.")]
	[DefaultValue(C_DefaultConnectionTimeout)]
	public int ConnectionTimeout
	{
		get { return (int)GetValue(C_KeyConnectionTimeout); }
		set { SetValue(C_KeyConnectionTimeout, value); }
	}

	[Category("Pooling")]
	[DisplayName("Pooling")]
	[Description("When true the connection is grabbed from a pool or, if necessary, created and added to the appropriate pool.")]
	[DefaultValue(C_DefaultPooling)]
	public bool Pooling
	{
		get { return (bool)GetValue(C_KeyPooling); }
		set { SetValue(C_KeyPooling, value); }
	}

	[Category("Connection")]
	[DisplayName("Connection LifeTime")]
	[Description("When a connection is returned to the pool, its creation time is compared with the current time, and the connection is destroyed if that time span (in seconds) exceeds the value specified by connection lifetime.")]
	[DefaultValue(C_DefaultConnectionLifeTime)]
	public int ConnectionLifeTime
	{
		get { return (int)GetValue(C_KeyConnectionLifeTime); }
		set { SetValue(C_KeyConnectionLifeTime, value); }
	}

	[Category("Pooling")]
	[DisplayName("MinPoolSize")]
	[Description("The minimun number of connections allowed in the pool.")]
	[DefaultValue(C_DefaultMinPoolSize)]
	public int MinPoolSize
	{
		get { return (int)GetValue(C_KeyMinPoolSize); }
		set { SetValue(C_KeyMinPoolSize, value); }
	}

	[Category("Pooling")]
	[DisplayName("MaxPoolSize")]
	[Description("The maximum number of connections allowed in the pool.")]
	[DefaultValue(C_DefaultMaxPoolSize)]
	public int MaxPoolSize
	{
		get { return (int)GetValue(C_KeyMaxPoolSize); }
		set { SetValue(C_KeyMaxPoolSize, value); }
	}

	[Category("Advanced")]
	[DisplayName("FetchSize")]
	[Description("The maximum number of rows to be fetched in a single call to read into the internal row buffer.")]
	[DefaultValue(C_DefaultFetchSize)]
	public int FetchSize
	{
		get { return (int)GetValue(C_KeyFetchSize); }
		set { SetValue(C_KeyFetchSize, value); }
	}

	[Category("Source")]
	[DisplayName("ServerType")]
	[Description("The type of server used.")]
	[DefaultValue(C_DefaultServerType)]
	public EnServerType ServerType
	{
		get { return (EnServerType)GetValue(C_KeyServerType); }
		set { SetValue(C_KeyServerType, value); }
	}

	[Category("Advanced")]
	[DisplayName("IsolationLevel")]
	[Description("The default Isolation Level for implicit transactions.")]
	[DefaultValue(C_DefaultIsolationLevel)]
	public IsolationLevel IsolationLevel
	{
		get { return (IsolationLevel)GetValue(C_KeyIsolationLevel); }
		set { SetValue(C_KeyIsolationLevel, value); }
	}

	[Category("Advanced")]
	[DisplayName("Records Affected")]
	[Description("Get the number of rows affected by a command when true.")]
	[DefaultValue(C_DefaultReturnRecordsAffected)]
	public bool ReturnRecordsAffected
	{
		get { return (bool)GetValue(C_KeyReturnRecordsAffected); }
		set { SetValue(C_KeyReturnRecordsAffected, value); }
	}

	[Category("Pooling")]
	[DisplayName("Enlist")]
	[Description("If true, enlists the connections in the current transaction.")]
	[DefaultValue(C_DefaultPooling)]
	public bool Enlist
	{
		get { return (bool)GetValue(C_KeyEnlist); }
		set { SetValue(C_KeyEnlist, value); }
	}

	[Category("Advanced")]
	[DisplayName("Client Library")]
	[Description("Client library for Embedded database.")]
	[DefaultValue(C_DefaultClientLibrary)]
	public string ClientLibrary
	{
		get { return (string)GetValue(C_KeyClientLibrary); }
		set { SetValue(C_KeyClientLibrary, value); }
	}

	[Category("Advanced")]
	[DisplayName("DB Cache Pages")]
	[Description("How many cache buffers to use for this session.")]
	[DefaultValue(C_DefaultDbCachePages)]
	public int DbCachePages
	{
		get { return (int)GetValue(C_KeyDbCachePages); }
		set { SetValue(C_KeyDbCachePages, value); }
	}

	[Category("Advanced")]
	[DisplayName("No Triggers")]
	[Description("Disables database triggers for this connection.")]
	[DefaultValue(C_DefaultNoDatabaseTriggers)]
	public bool NoDatabaseTriggers
	{
		get { return (bool)GetValue(C_KeyNoDatabaseTriggers); }
		set { SetValue(C_KeyNoDatabaseTriggers, value); }
	}

	[Category("Advanced")]
	[DisplayName("No Garbage Collect")]
	[Description("If true, disables sweeping the database upon attachment.")]
	[DefaultValue(C_DefaultNoGarbageCollect)]
	public bool NoGarbageCollect
	{
		get { return (bool)GetValue(C_KeyNoGarbageCollect); }
		set { SetValue(C_KeyNoGarbageCollect, value); }
	}

	[Category("Advanced")]
	[DisplayName("Compression")]
	[Description("Enables or disables wire compression.")]
	[DefaultValue(C_DefaultCompression)]
	public bool Compression
	{
		get { return (bool)GetValue(C_KeyCompression); }
		set { SetValue(C_KeyCompression, value); }
	}

	[Category("Advanced")]
	[DisplayName("Crypt Key")]
	[Description("Key used for database decryption.")]
	[DefaultValue(C_DefaultCryptKey)]
	public byte[] CryptKey
	{
		get { return (byte[])GetValue(C_KeyCryptKey); }
		set { SetValue(C_KeyCryptKey, value); }
	}

	[Category("Advanced")]
	[DisplayName("Wire Crypt")]
	[Description("Selection for wire encryption.")]
	[DefaultValue(C_DefaultWireCrypt)]
	public EnWireCrypt WireCrypt
	{
		get { return (EnWireCrypt)GetValue(C_KeyWireCrypt); }
		set { SetValue(C_KeyWireCrypt, value); }
	}

	[Category("Advanced")]
	[DisplayName("Application Name")]
	[Description("The name of the application making the connection.")]
	[DefaultValue(C_DefaultApplicationName)]
	public string ApplicationName
	{
		get { return (string)GetValue(C_KeyApplicationName); }
		set { SetValue(C_KeyApplicationName, value); }
	}

	[Category("Advanced")]
	[DisplayName("Command Timeout")]
	[Description("The time (in seconds) for command execution.")]
	[DefaultValue(C_DefaultCommandTimeout)]
	public int CommandTimeout
	{
		get { return (int)GetValue(C_KeyCommandTimeout); }
		set { SetValue(C_KeyCommandTimeout, value); }
	}

	[Category("Advanced")]
	[DisplayName("Parallel Workers")]
	[Description("Number of parallel workers to use for certain operations in the database.")]
	[DefaultValue(C_DefaultParallelWorkers)]
	public int ParallelWorkers
	{
		get { return (int)GetValue(C_KeyParallelWorkers); }
		set { SetValue(C_KeyParallelWorkers, value); }
	}


	#endregion Property accessors





	// =====================================================================================================
	#region Methods - AbstractCsb
	// =====================================================================================================


	public delegate bool TryGetValueDelegate(string key, out object value);


	public new void Add(string keyword, object value)
	{
		string key = keyword;
		Describer describer = _Describers[keyword];

		if (describer != null)
			key = describer.Key;

		this[key] = value;
	}



	public override bool ContainsKey(string keyword)
	{
		// Tracer.Trace(GetType(), "ContainsKey()", "key: {0}", keyword);
		if (base.ContainsKey(keyword))
			return true;

		IList<string> synonyms = _Describers.GetSynonyms(keyword);

		foreach (string synonym in synonyms)
		{
			if (base.ContainsKey(synonym))
				return true;
		}

		return false;
	}


	protected object GetValue(string synonym)
	{
		Describer describer = _Describers.GetSynonymDescriber(synonym);

		if (describer == null)
			Diag.ThrowException(new ArgumentException(nameof(synonym)), "Describer does not exist");

		string storageKey = describer.ConnectionStringKey;

		if (!TryGetValue(storageKey, out object value))
			return describer.DefaultValue;

		Type propertyType = describer.PropertyType;

		if (propertyType.IsSubclassOf(typeof(Enum)))
		{
			switch (value)
			{
				case Enum enumValue:
					return enumValue;
				case string stringValue:
					return Enum.Parse(propertyType, stringValue, true);
				default:
					return value;
			}
		}
		else if (propertyType == typeof(byte[]))
		{
			switch (value)
			{
				case byte[] bytesValue:
					return bytesValue;
				case string stringValue:
					return Convert.FromBase64String(stringValue);
				default:
					return value;
			}
		}
		else if (propertyType == typeof(int))
		{
			return Convert.ToInt32(value);
		}
		else if (propertyType == typeof(bool))
		{
			return Convert.ToBoolean(value);
		}
		else if (propertyType == typeof(Version))
		{
			switch (value)
			{
				case string stringValue:
					return new Version(stringValue);
				default:
					return value;
			}
		}
		else if (propertyType == typeof(string))
		{
			return Convert.ToString(value);
		}
		else
		{
			return value;
		}
	}


	public override bool Remove(string keyword)
	{
		if (base.Remove(keyword))
			return true;

		IList<string> synonyms = _Describers.GetSynonyms(keyword);

		foreach (string synonym in synonyms)
		{
			if (base.Remove(synonym))
				return true;
		}

		return false;

	}




	protected void SetValue(string synonym, object value)
	{
		Describer describer = _Describers.GetSynonymDescriber(synonym);

		if (describer == null)
			Diag.ThrowException(new ArgumentException(nameof(synonym)), $"Describer does not exist: {synonym}.");

		string storageKey = describer.ConnectionStringKey;
		object storedValue;

		Type propertyType = describer.PropertyType;

		if (propertyType.IsSubclassOf(typeof(Enum)))
		{
			if (value is Enum enumValue)
				storedValue = enumValue;
			else if (value is string stringValue)
				storedValue = Enum.Parse(propertyType, stringValue, true);
			else
				storedValue = value;
		}
		else if (propertyType == typeof(byte[]))
		{
			if (value is byte[] bytesValue)
				storedValue = Convert.ToBase64String(bytesValue);
			else
				storedValue = value;
		}
		else
		{
			storedValue = value;
		}

		if (storedValue == null || storedValue.Equals(describer.DefaultValue))
			Remove(storageKey);
		else
			base[storageKey] = storedValue;
	}



	public override bool TryGetValue(string keyword, out object value)
	{
		if (base.TryGetValue(keyword, out value))
			return true;

		IList<string> synonyms = Describers.GetSynonyms(keyword);

		foreach (string synonym in synonyms)
		{
			if (base.TryGetValue(synonym, out value))
				return true;
		}

		return false;
	}



	#endregion Methods


}
