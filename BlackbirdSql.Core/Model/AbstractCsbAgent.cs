
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq.Expressions;
using System.Text;

using BlackbirdSql.Core.Ctl;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Ctl.Extensions;
using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.Core.Model.Enums;
using BlackbirdSql.Core.Properties;

using FirebirdSql.Data.FirebirdClient;

using Microsoft.VisualStudio.Data.Services;

using static BlackbirdSql.Core.Ctl.CoreConstants;
using static BlackbirdSql.Core.Model.ModelConstants;


namespace BlackbirdSql.Core.Model;

// =========================================================================================================
//
//											AbstractCsbAgent Class
//
/// <summary>
/// This is the base class of CsbAgent and it's descendents.
/// CsbAgent is a wrapper for FBConnectionStringBuilder. CsbAgent brings some sanity to
/// DBConnectionStringBuilder naming by making property names the primary property name for index accessors,
/// TryGetValue() and Contains() as well as improved support for synonyms. Secondly, it centrally controls
/// unique connection DatasetKey naming according to connection equivalency keys. See remarks.
/// </summary>
/// <remarks>
/// A BlackbirdSql connection is uniquely identifiable on it's DatasetKey, 'Server (DatasetId)', where
/// DatasetId is the connection name in FlameRobin or the Dataset if the connection cannot be derived
/// from FlameRobin. A Dataset name is the database path's stripped file name. If the DatasetKey is not
/// unique across an ide session, duplicate DatasetId names will be numerically suffixed beginning
/// with 2, which means a connection's DatasetKey may differ from one ide session to another.
/// Connections are considered equivalent if the connection equivalency parameters match. To keep
/// things simple the describers have DataSource/Server, Database path, UserID, Role and
/// NoDatabaseTriggers set as equivalency parameters. No further distinction takes place. Property
/// equivalency keys are defined in the Describers collection.
/// </remarks>
// =========================================================================================================
public abstract class AbstractCsbAgent : FbConnectionStringBuilder
{


	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - AbstractCsbAgent
	// ---------------------------------------------------------------------------------


	public AbstractCsbAgent() : base()
	{
	}

	public AbstractCsbAgent(string connectionString) : base(connectionString)
	{
	}

	/// <summary>
	/// .ctor for use only by ConnectionLocator for registering solution datasetKeys.
	/// </summary>
	protected AbstractCsbAgent(string datasetId, string connectionString) : base(connectionString)
	{
		DatasetId = datasetId;
	}


	public AbstractCsbAgent(IDbConnection connection) : base(connection.ConnectionString)
	{
	}


	public AbstractCsbAgent(IBPropertyAgent ci) : base()
	{
		Parse(ci);
	}


	public AbstractCsbAgent(IVsDataExplorerNode node) : base()
	{
		Extract(node);
	}


	/// <summary>
	/// .ctor for use only by ConnectionLocator for registering FlameRobin datasetKeys.
	/// </summary>
	protected AbstractCsbAgent(string datasetId, string server, int port, string database, string user,
				string password, string charset)
	{
		Initialize(datasetId, server, port, C_DefaultServerType, database, user, password,
			C_DefaultRole, charset, C_DefaultDialect, C_DefaultNoDatabaseTriggers);
	}


	private void Initialize(string datasetId, string server, int port, FbServerType serverType, string database, string user,
		string password, string role, string charset, int dialect, bool noTriggers)
	{
		DatasetId = datasetId;
		DataSource = server;
		Port = port;
		ServerType = serverType;
		Database = database;
		UserID = user;
		Password = password;
		Role = role;
		Charset = charset;
		Dialect = dialect;
		NoDatabaseTriggers = noTriggers;
	}


	#endregion Property Constructors / Destructors




	// =====================================================================================================
	#region Constants - AbstractCsbAgent
	// =====================================================================================================

	private const string C_Scheme = "fbsql";
	protected const string C_DatasetKeyFmt = "{0} ({1})";
	private const char C_CompositeSeparator = '.';


	#endregion Constants




	// =====================================================================================================
	#region Variables - AbstractCsbAgent
	// =====================================================================================================


	protected static IDictionary<string, string> _SDatasetKeys;
	protected static IDictionary<string, string> _SConnectionMonikers;

	private string _SafeDatasetMoniker = null;
	private string _UnsafeDatasetMoniker = null;

	// A private 'this' object lock
	private readonly object _LockLocal = new object();
	private bool _IndexActive = false;

	private string _EquivalencyConnectionString = null;
	private string _EquivalencyMoniker = null;



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Describer collection of uniform properties used by the FbConnectionStringBuilder
	/// wrapper, CsbAgent, as well as PropertyAgent and it's descendent SqlEditor
	/// ConnectionInfo and Dispatcher connection classes, and also the SE root nodes,
	/// Root and Database.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static readonly DescriberDictionary Describers = new(
		[
			new Describer(C_KeyExDatasetKey, typeof(string), C_DefaultExDatasetKey),
			new Describer(C_KeyExDatasetId, typeof(string), C_DefaultExDatasetId),
			new Describer(C_KeyExDataset, typeof(string), C_DefaultExDataset),

			new Describer(C_KeyDataSource, C_KeyFbDataSource, typeof(string), C_DefaultDataSource, true, false, true, true, true), // *
			new Describer(C_KeyPort, C_KeyFbPort, typeof(int), C_DefaultPort, true, false, true, false, true), // *
			new Describer(C_KeyServerType, C_KeyFbServerType, typeof(FbServerType), C_DefaultServerType, true, false, true, false, true), // *
			new Describer(C_KeyDatabase, C_KeyFbDatabase, typeof(string), C_DefaultDatabase, true, false, true, true, true), // *
			new Describer(C_KeyUserID ,C_KeyFbUserID, typeof(string), C_DefaultUserID, true, false, true, true, true), // *
			new Describer(C_KeyPassword, C_KeyFbPassword, typeof(string), C_DefaultPassword, true, false, false, true),

			new Describer(C_KeyRole, C_KeyFbRole, typeof(string), C_DefaultRole, true, false, true, false, true), // *
			new Describer(C_KeyDialect, C_KeyFbDialect, typeof(int), C_DefaultDialect, true, false, true, false, true), // *
			new Describer(C_KeyCharset, C_KeyFbCharset, typeof(string), C_DefaultCharset, true, false, true, false, true), // *
			new Describer(C_KeyNoDatabaseTriggers, C_KeyFbNoDatabaseTriggers, typeof(bool), C_DefaultNoDatabaseTriggers, true, true, true, false, true), // *
			new Describer(C_KeyPacketSize, C_KeyFbPacketSize, typeof(int), C_DefaultPacketSize, true),
			new Describer(C_KeyConnectionTimeout, C_KeyFbConnectionTimeout, typeof(int), C_DefaultConnectionTimeout, true),
			new Describer(C_KeyPooling, C_KeyFbPooling, typeof(bool), C_DefaultPooling, true),
			new Describer(C_KeyConnectionLifeTime, C_KeyFbConnectionLifeTime, typeof(int), C_DefaultConnectionLifeTime, true),
			new Describer(C_KeyMinPoolSize, C_KeyFbMinPoolSize, typeof(int), C_DefaultMinPoolSize, true),
			new Describer(C_KeyMaxPoolSize, C_KeyFbMaxPoolSize, typeof(int), C_DefaultMaxPoolSize, true),
			new Describer(C_KeyFetchSize, C_KeyFbFetchSize, typeof(int), C_DefaultFetchSize, true),
			new Describer(C_KeyIsolationLevel, C_KeyFbIsolationLevel, typeof(IsolationLevel), C_DefaultIsolationLevel, true),
			new Describer(C_KeyReturnRecordsAffected, C_KeyFbReturnRecordsAffected, typeof(bool), C_DefaultReturnRecordsAffected, true),
			new Describer(C_KeyEnlist, C_KeyFbEnlist, typeof(bool), C_DefaultEnlist, true),
			new Describer(C_KeyClientLibrary, C_KeyFbClientLibrary, typeof(string), C_DefaultClientLibrary, true),
			new Describer(C_KeyDbCachePages, C_KeyFbDbCachePages, typeof(int), C_DefaultDbCachePages, true),
			new Describer(C_KeyNoGarbageCollect, C_KeyFbNoGarbageCollect, typeof(bool), C_DefaultNoGarbageCollect, true),
			new Describer(C_KeyCompression, C_KeyFbCompression, typeof(bool), C_DefaultCompression, true),
			new Describer(C_KeyCryptKey, C_KeyFbCryptKey, typeof(byte[]), C_DefaultCryptKey, true),
			new Describer(C_KeyWireCrypt, C_KeyFbWireCrypt, typeof(FbWireCrypt), C_DefaultWireCrypt, true),
			new Describer(C_KeyApplicationName, C_KeyFbApplicationName, typeof(string), C_DefaultApplicationName, true),
			new Describer(C_KeyCommandTimeout, C_KeyFbCommandTimeout, typeof(int), C_DefaultCommandTimeout, true),
			new Describer(C_KeyParallelWorkers, C_KeyFbParallelWorkers, typeof(int), C_DefaultParallelWorkers, true),

			new Describer(C_KeyExClientVersion, typeof(Version), C_DefaultExClientVersion, false, false),
			new Describer(C_KeyExMemoryUsage, typeof(string), C_DefaultExMemoryUsage, false, false),
			new Describer(C_KeyExActiveUsers, typeof(int), C_DefaultExActiveUsers, false, false)
		],
		[
			StringPair( "server", C_KeyDataSource ),
			StringPair( "host", C_KeyDataSource ),
			StringPair( "uid", C_KeyUserID),
			StringPair( "user", C_KeyUserID),
			StringPair( "username", C_KeyUserID),
			StringPair( "user name", C_KeyUserID),
			StringPair( "userpassword", C_KeyPassword),
			StringPair( "user password", C_KeyPassword),
			StringPair( "no triggers", C_KeyNoDatabaseTriggers ),
			StringPair( "nodbtriggers", C_KeyNoDatabaseTriggers ),
			StringPair( "no dbtriggers", C_KeyNoDatabaseTriggers ),
			StringPair( "no database triggers", C_KeyNoDatabaseTriggers ),
			StringPair( "timeout", C_KeyConnectionTimeout ),
			StringPair( "db cache pages", C_KeyDbCachePages ),
			StringPair( "cachepages", C_KeyDbCachePages ),
			StringPair( "pagebuffers", C_KeyDbCachePages ),
			StringPair( "page buffers", C_KeyDbCachePages ),
			StringPair( "wire compression", C_KeyCompression ),
			StringPair( "app", C_KeyApplicationName ),
			StringPair( "parallel", C_KeyParallelWorkers )
		]
	);


	#endregion Variables




	// =====================================================================================================
	#region Property accessors - AbstractCsbAgent
	// =====================================================================================================


	/// <summary>
	/// Index accessor override to get back some uniformity in connection property naming.
	/// </summary>
	[Browsable(false)]
	public override object this[string keyword]
	{
		get
		{
			lock (_LockLocal)
			{
					if (_IndexActive)
						return base[keyword];

				try
				{

					_IndexActive = true;

					object result;

					switch (keyword)
					{
						case C_KeyDataSource:
							// case C_KeyFbDataSource:
							result = DataSource;
							break;
						case C_KeyPort:
							// case C_KeyFbPort:
							result = Port;
							break;
						case C_KeyServerType:
							// case C_KeyFbServerType:
							result = (int)ServerType;
							break;
						case C_KeyDatabase:
							// case C_KeyFbDatabase:
							result = Database;
							break;
						case C_KeyUserID:
						// case C_KeyFbUserID:
						case "User ID":
							result = UserID;
							break;
						case C_KeyPassword:
							// case C_KeyFbPassword:
							result = Password;
							break;
						case C_KeyRole:
							// case C_KeyFbRole:
							result = Role;
							break;
						case C_KeyDialect:
							// case C_KeyFbDialect:
							result = Dialect;
							break;
						case C_KeyCharset:
						// case C_KeyFbCharset:
						case "Character Set":
							result = Charset;
							break;
						case C_KeyNoDatabaseTriggers:
						// case C_KeyFbNoDatabaseTriggers:
						case "No Triggers":
							result = NoDatabaseTriggers;
							break;
						case C_KeyPacketSize:
							// case C_KeyFbPacketSize:
							result = PacketSize;
							break;
						case C_KeyConnectionTimeout:
						// case C_KeyFbConnectionTimeout:
						case "Connection Timeout":
							result = ConnectionTimeout;
							break;
						case C_KeyPooling:
							//  case C_KeyFbPooling:
							result = Pooling;
							break;
						case C_KeyConnectionLifeTime:
						//  case C_KeyFbConnectionLifeTime:
						case "Connection LifeTime":
							result = ConnectionLifeTime;
							break;
						case C_KeyMinPoolSize:
							//  case C_KeyFbMinPoolSize:
							result = MinPoolSize;
							break;
						case C_KeyMaxPoolSize:
							//  case C_KeyFbMaxPoolSize:
							result = MaxPoolSize;
							break;
						case C_KeyFetchSize:
							//  case C_KeyFbFetchSize:
							result = FetchSize;
							break;
						case C_KeyIsolationLevel:
							//  case C_KeyFbIsolationLevel:
							result = (int)IsolationLevel;
							break;
						case C_KeyReturnRecordsAffected:
						//  case C_KeyFbReturnRecordsAffected:
						case "Records Affected":
							result = ReturnRecordsAffected;
							break;
						case C_KeyEnlist:
							//  case C_KeyFbEnlist:
							result = Enlist;
							break;
						case C_KeyClientLibrary:
						//  case C_KeyFbClientLibrary:
						case "Client Library":
							result = ClientLibrary;
							break;
						case C_KeyDbCachePages:
						//  case C_KeyFbDbCachePages:
						case "DB Cache Pages":
							result = DbCachePages;
							break;
						case C_KeyNoGarbageCollect:
						//  case C_KeyFbNoGarbageCollect:
						case "No Garbage Collect":
							result = NoGarbageCollect;
							break;
						case C_KeyCompression:
							//  case C_KeyFbCompression:
							result = Compression;
							break;
						case C_KeyCryptKey:
						//  case C_KeyFbCryptKey:
						case "Crypt Key":
							result = CryptKey;
							break;
						case C_KeyWireCrypt:
						//  case C_KeyFbWireCrypt:
						case "Wire Crypt":
							result = (int)WireCrypt;
							break;
						case C_KeyApplicationName:
						//  case C_KeyFbApplicationName:
						case "Application Name":
							result = ApplicationName;
							break;
						case C_KeyCommandTimeout:
						//  case C_KeyFbCommandTimeout:
						case "Command Timeout":
							result = CommandTimeout;
							break;
						case C_KeyParallelWorkers:
						//  case C_KeyFbParallelWorkers:
						case "Parallel Workers":
							result = ParallelWorkers;
							break;
						default:
							result = base[keyword];
							break;
					}

					return result;
				}
				catch (Exception ex)
				{
					Diag.Dug(ex);
					throw;
				}
				finally
				{
					_IndexActive = false;
				}
			}
		}
		set
		{
			lock (_LockLocal)
			{
				if (_IndexActive)
				{
					base[keyword] = value;
					return;
				}

				try
				{
					if (value == DBNull.Value)
						value = null;

					_IndexActive = true;

					switch (keyword)
					{
						case C_KeyDataSource:
							//  case C_KeyFbDataSource:
							DataSource = (string)value;
							break;
						case C_KeyPort:
							//  case C_KeyFbPort:
							Port = Convert.ToInt32(value);
							break;
						case C_KeyServerType:
							//  case C_KeyFbServerType:
							if (value is FbServerType fbServerType)
								ServerType = fbServerType;
							else if (value is string s && Enum.TryParse<FbServerType>(s, true, out var enumResult))
								ServerType = enumResult;
							else
								ServerType = (FbServerType)Convert.ToInt32(value);
							break;
						case C_KeyDatabase:
							//  case C_KeyFbDatabase:
							Database = (string)value;
							break;
						case C_KeyUserID:
						//  case C_KeyFbUserID:
						case "User ID":
							UserID = (string)value;
							break;
						case C_KeyPassword:
							//  case C_KeyFbPassword:
							Password = (string)value;
							break;
						case C_KeyRole:
							//  case C_KeyFbRole:
							Role = (string)value;
							break;
						case C_KeyDialect:
							//  case C_KeyFbDialect:
							Dialect = Convert.ToInt32(value);
							break;
						case C_KeyCharset:
						//  case C_KeyFbCharset:
						case "Character Set":
							Charset = (string)value;
							break;
						case C_KeyNoDatabaseTriggers:
						//  case C_KeyFbNoDatabaseTriggers:
						case "No Triggers":
							NoDatabaseTriggers = Convert.ToBoolean(value);
							break;
						case C_KeyPacketSize:
							//  case C_KeyFbPacketSize:
							PacketSize = Convert.ToInt32(value);
							break;
						case C_KeyConnectionTimeout:
						//  case C_KeyFbConnectionTimeout:
						case "Connection Timeout":
							ConnectionTimeout = Convert.ToInt32(value);
							break;
						case C_KeyPooling:
							//  case C_KeyFbPooling:
							Pooling = Convert.ToBoolean(value);
							break;
						case C_KeyConnectionLifeTime:
						//  case C_KeyFbConnectionLifeTime:
						case "Connection LifeTime":
							ConnectionLifeTime = Convert.ToInt32(value);
							break;
						case C_KeyMinPoolSize:
							//  case C_KeyFbMinPoolSize:
							MinPoolSize = Convert.ToInt32(value);
							break;
						case C_KeyMaxPoolSize:
							//  case C_KeyFbMaxPoolSize:
							MaxPoolSize = Convert.ToInt32(value);
							break;
						case C_KeyFetchSize:
							//  case C_KeyFbFetchSize:
							FetchSize = Convert.ToInt32(value);
							break;
						case C_KeyIsolationLevel:
							//  case C_KeyFbIsolationLevel:
							if (value is IsolationLevel isolationLevel)
								IsolationLevel = isolationLevel;
							else if (value is string s && Enum.TryParse<IsolationLevel>(s, true, out var enumResult))
								IsolationLevel = enumResult;
							else
								IsolationLevel = (IsolationLevel)Convert.ToInt32(value);
							break;
						case C_KeyReturnRecordsAffected:
						//  case C_KeyFbReturnRecordsAffected:
						case "Records Affected":
							ReturnRecordsAffected = Convert.ToBoolean(value);
							break;
						case C_KeyEnlist:
							//  case C_KeyFbEnlist:
							Enlist = Convert.ToBoolean(value);
							break;
						case C_KeyClientLibrary:
						//  case C_KeyFbClientLibrary:
						case "Client Library":
							ClientLibrary = (string)value;
							break;
						case C_KeyDbCachePages:
						//  case C_KeyFbDbCachePages:
						case "DB Cache Pages":
							DbCachePages = Convert.ToInt32(value);
							break;
						case C_KeyNoGarbageCollect:
						//  case C_KeyFbNoGarbageCollect:
						case "No Garbage Collect":
							NoGarbageCollect = Convert.ToBoolean(value);
							break;
						case C_KeyCompression:
							//  case C_KeyFbCompression:
							Compression = Convert.ToBoolean(value);
							break;
						case C_KeyCryptKey:
						//  case C_KeyFbCryptKey:
						case "Crypt Key":
							CryptKey = (byte[])value;
							break;
						case C_KeyWireCrypt:
						//  case C_KeyFbWireCrypt:
						case "Wire Crypt":
							if (value is FbWireCrypt wireCrypt)
								WireCrypt = wireCrypt;
							else if (value is string s && Enum.TryParse<FbWireCrypt>(s, true, out var enumResult))
								WireCrypt = enumResult;
							else
								WireCrypt = (FbWireCrypt)Convert.ToInt32(value);
							break;
						case C_KeyApplicationName:
						//  case C_KeyFbApplicationName:
						case "Application Name":
							ApplicationName = (string)value;
							break;
						case C_KeyCommandTimeout:
						//  case C_KeyFbCommandTimeout:
						case "Command Timeout":
							CommandTimeout = Convert.ToInt32(value);
							break;
						case C_KeyParallelWorkers:
						//  case C_KeyFbParallelWorkers:
						case "Parallel Workers":
							ParallelWorkers = Convert.ToInt32(value);
							break;
						default:
							base[keyword] = value;
							break;
					}
				}
				catch (Exception ex)
				{
					Diag.Dug(ex);
					throw;
				}
				finally
				{
					_IndexActive = false;
				}

			}
		}
	}


	/// <summary>
	/// Contains the list of registered connection dastasetKeys. This accessor may only be referenced inside of
	/// RegisterUniqueConnectionDatsetKey().
	/// </summary>
	protected static IDictionary<string, string> SDatasetKeys => _SDatasetKeys ??= new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
	protected static IDictionary<string, string> SConnectionMonikers => _SConnectionMonikers ??= new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

	[Category("Extended")]
	[DisplayName("Dataset")]
	[Description("The short name (file name without extension) of the database file")]
	[ReadOnly(true)]
	[DefaultValue(C_DefaultExDataset)]
	public string Dataset
	{
		get
		{
			// Tracer.Trace(GetType(), "Dataset", "Database: {0}, Path.GetFileNameWithoutExtension(Database): {1}.",
			//	Database, string.IsNullOrWhiteSpace(Database) ? "" : Path.GetFileNameWithoutExtension(Database));
			return (string.IsNullOrWhiteSpace(Database) ? "" : Path.GetFileNameWithoutExtension(Database));
		}
	}


	/// <summary>
	/// The unique key for an ide session connection configuration in the form 'Server (DatasetId)'.
	/// Clients must always register a csb using one of the register methods before attempting to
	/// access DatasetKey or DatasetId.
	/// </summary>
	[Category("Extended")]
	[DisplayName("DatasetKey")]
	[Description("The unique key for an ide session connection configuration in the form 'Server (DatasetId)'.")]
	[ReadOnly(true)]
	[DefaultValue(C_DefaultExDatasetKey)]
	public string DatasetKey
	{
		get
		{
			if (TryGetValue("DatasetKey", out object value))
				return (string)value;

			return C_DefaultExDatasetKey;
		}
		set
		{
			this["DatasetKey"] = value;
		}
	}


	/// <summary>
	/// The server scope unique name for a connection configuration database used in DatasetKey
	/// 'Server (DatasetId)'.
	/// Clients must always register a csb using one of the register methods before attempting to
	/// access DatasetKey or DatasetId.
	/// </summary>
	[Category("Extended")]
	[DisplayName("DatasetId")]
	[Description("The server scope unique name for a connection configuration database used in DatasetKey 'Server (DatasetId)'.")]
	[ReadOnly(true)]
	[DefaultValue(C_DefaultExDatasetId)]
	public string DatasetId
	{
		get
		{
			if (TryGetValue("DatasetId", out object value))
				return (string)value;

			return C_DefaultExDatasetId;
		}
		set
		{
			this["DatasetId"] = value;
		}
	}


	/// <summary>
	/// The server memory usage.
	/// </summary>
	[Browsable(false)]
	[Category("Extended")]
	[DisplayName("Client version")]
	[Description("The FirbirdSql.Data.Firebird library version.")]
	[ReadOnly(true)]
	[DefaultValue(C_DefaultExClientVersion)]
	public Version ClientVersion
	{
		get
		{
			if (TryGetValue(C_KeyExClientVersion, out object value))
				return (Version)value;

			return null;
		}
		set
		{
			this[C_KeyExClientVersion] = value;
		}
	}

	/// <summary>
	/// The server memory usage.
	/// </summary>
	[Browsable(false)]
	[Category("Extended")]
	[DisplayName("Memory usage")]
	[Description("Applicable to live connections only. The server memory usage.")]
	[ReadOnly(true)]
	[DefaultValue("")]
	public string MemoryUsage
	{
		get
		{
			if (TryGetValue(C_KeyExMemoryUsage, out object value))
				return (string)value;

			return "";
		}
		set
		{
			this[C_KeyExMemoryUsage] = value;
		}
	}


	/// <summary>
	/// Server connected users.
	/// </summary>
	[Browsable(false)]
	[Category("Extended")]
	[DisplayName("Server connected users.")]
	[Description("Applicable to live connections only. The number of connected users (Firebird 3 only).")]
	[ReadOnly(true)]
	[DefaultValue(0)]
	public int ActiveUsers
	{
		get
		{
			if (TryGetValue(C_KeyExActiveUsers, out object value))
				return Convert.ToInt32(value);

			return 0;
		}
		set
		{
			this[C_KeyExActiveUsers] = value;
		}
	}


	[Browsable(false)]
	public ICollection ConnectionKeys
	{
		get
		{
			ICollection<string> collection = (ICollection<string>)base.Keys;
			IEnumerator<string> enumerator = collection.GetEnumerator();
			object[] array = new object[collection.Count];
			for (int i = 0; i < array.Length; i++)
			{
				enumerator.MoveNext();
				array[i] = enumerator.Current;
			}

			return new ReadOnlyCollection<object>(array);
		}
	}



	/// <summary>
	/// The connection string parameters property name list.
	/// </summary>
	[Browsable(false)]
	public ICollection PropertyKeys
	{
		get
		{
			ICollection<string> collection = (ICollection<string>)base.Keys;
			IEnumerator<string> enumerator = collection.GetEnumerator();
			object[] array = new object[collection.Count];
			for (int i = 0; i < array.Length; i++)
			{
				enumerator.MoveNext();
				array[i] = Describers[enumerator.Current].Key;
			}

			return new ReadOnlyCollection<object>(array);
		}
	}



	/// <summary>
	/// Returns the unique dataset connection url in the form
	/// fbsql://user@server/database_lc_serialized/[role_uc.charset_uc.dialect.noTriggersTrueFalse]/
	/// </summary>
	[Browsable(false)]
	public string SafeDatasetMoniker => _SafeDatasetMoniker ??= BuildUniqueConnectionUrl(true);

	[Browsable(false)]
	public string UnsafeDatasetMoniker => _UnsafeDatasetMoniker ??= BuildUniqueConnectionUrl(false);


	#endregion Property accessors




	// =====================================================================================================
	#region Methods - AbstractCsbAgent
	// =====================================================================================================



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Checks whether or not connection property/parameter objects are equivalent
	/// </summary>
	/// <remarks>
	/// We consider connections equivalent if they will produce the same results. The connection properties
	/// that determine this equivalency are defined in <see cref="CoreProperties.EquivalencyKeys"/>.
	/// </remarks>
	public static bool AreEquivalent(DbConnectionStringBuilder csb1, DbConnectionStringBuilder csb2)
	{
		// Tracer.Trace(typeof(AbstractCsbAgent), "AreEquivalent(DbConnectionStringBuilder, DbConnectionStringBuilder)");

		int equivalencyValueCount = 0;
		int equivalencyKeyCount = Describers.EquivalencyCount;
		object value1, value2;
		Describer describer;

		try
		{
			// Keep it simple. Loop thorugh each connection string and compare
			// If it's not in the other or null use default

			foreach (KeyValuePair<string, object> param in csb1)
			{
				// If all equivalency keys have been checked, break
				if (equivalencyValueCount == equivalencyKeyCount)
					break;

				// Get the correct key for the parameter in connection 1
				if ((describer = Describers[param.Key]) == null)
				{
					ArgumentException ex = new(Resources.ExceptionParameterDescriberNotFound.FmtRes(param.Key));
					Diag.Dug(ex);
					throw ex;
				}

				// Exclude non-applicable connection values.
				// Typically we may require a password and if it's already in, for example, the SE we have rights to it.
				// There would be no point ignoring that password just because some spurious value differs. For example 'Connection Lifetime'.

				if (!describer.IsEquivalency)
					continue;

				equivalencyValueCount++;

				// For both connections we set the value to default if it's null or doesn't exist
				if (param.Value != null)
					value1 = param.Value;
				else
					value1 = describer.DefaultValue;

				// We can't do a straight lookup on the second string because it may be a synonym so we have to loop
				// through the parameters, find the real key, and use that

				value2 = FindKeyValueInConnection(describer, csb2);

				value2 ??= describer.DefaultValue;

				if (!AreEquivalent(describer.DerivedConnectionParameter, value1, value2))
				{
					// Tracer.Trace(typeof(AbstractCsbCount), "AreEquivalent(DbConnectionStringBuilder, DbConnectionStringBuilder)",
					// 	"Connection parameter '{0}' mismatch: '{1}' : '{2}.",
					//	param.Key, value1 != null ? value1.ToString() : "null", value2 != null ? value2.ToString() : "null");
					return false;
				}
			}

			if (equivalencyValueCount < equivalencyKeyCount)
			{

				foreach (KeyValuePair<string, object> param in csb2)
				{
					// If all equivalency keys have been checked, break
					if (equivalencyValueCount == equivalencyKeyCount)
						break;

					// Get the correct key for the parameter in connection 2
					if ((describer = Describers[param.Key]) == null)
					{
						ArgumentException ex = new($"Could not locate Describer for connection parameter '{param.Key}'.");
						Diag.Dug(ex);
						throw ex;
					}



					// Exclude non-applicable connection values.
					// Typically we may require a password and if it's already in, for example, the SE we have rights to it.
					// There would be no point ignoring that password just because some spurious value differs. For example 'Connection Lifetime'. 

					if (!describer.IsEquivalency)
						continue;

					equivalencyValueCount++;

					// For both connections we set the value to default if it's null or doesn't exist
					if (param.Value != null)
						value2 = param.Value;
					else
						value2 = describer.DefaultValue;

					// We can't do a straight lookup on the first connection because it may be a synonym so we have to loop
					// through the parameters, find the real key, and use that
					value1 = FindKeyValueInConnection(describer, csb1);

					value1 ??= describer.DefaultValue;

					if (!AreEquivalent(describer.DerivedConnectionParameter, value2, value1))
					{
						// Tracer.Trace(typeof(AbstractCsbCount), "AreEquivalent(DbConnectionStringBuilder, DbConnectionStringBuilder)",
						//	"Connection2 parameter '{0}' mismatch: '{1}' : '{2}.",
						//	param.Key, value2 != null ? value2.ToString() : "null", value1 != null ? value1.ToString() : "null");
						return false;
					}
					// Diag.Trace("Connection2 parameter '" + key + "' equivalent: '" + (value2 != null ? value2.ToString() : "null") + "' : '" + (value1 != null ? value1.ToString() : "null"));
				}
			}

		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			return false;
		}

		// Tracer.Trace(typeof(TConnectionEquivalencyComparer),
		// 	"TConnectionEquivalencyComparer.AreEquivalent(IDictionary, IDictionary)", "Connections are equivalent");

		return true;
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs an equivalency comparison of to values of the connection
	/// property/parameter 'key'.
	/// </summary>
	/// <param name="key"></param>
	/// <param name="value1"></param>
	/// <param name="value2"></param>
	/// <returns>true if equivalent else false</returns>
	// ---------------------------------------------------------------------------------
	protected static bool AreEquivalent(string key, object value1, object value2)
	{
		// Diag.Trace();
		string text1 = value1 as string;
		string text2 = value2 as string;

		if (!string.Equals(text1, text2, StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}

		return true;
	}


	public override bool ContainsKey(string keyword)
	{
		// Tracer.Trace(GetType(), "ContainsKey()", "key: {0}", keyword);
		if (base.ContainsKey(keyword))
			return true;

		IList<string> synonyms = Describers.GetSynonyms(keyword);

		foreach (string synonym in synonyms)
		{
			if (base.ContainsKey(synonym))
				return true;
		}

		return false;
	}

	public new void Add(string keyword, object value)
	{
		Describer describer = Describers[keyword];

		if (describer == null)
		{
			KeyNotFoundException ex = new($"Describer key: {keyword}.");
			Diag.Dug(ex);
			throw ex;
		}

		this[describer.Key] = value;
	}






	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Finds the value of the connection property/parameter 'key' in a connection
	/// properties list given that the property key used in the list may be a synonym.
	/// </summary>
	/// <param name="key"></param>
	/// <param name="connectionProperties"></param>
	/// <returns></returns>
	/// <exception cref="ArgumentException"></exception>
	// ---------------------------------------------------------------------------------
	private static object FindKeyValueInConnection(Describer describer, DbConnectionStringBuilder csb)
	{
		if (csb.TryGetValue(describer.Name, out object value))
			return value;

		IList<string> synonyms = Describers.GetSynonyms(describer.Name);

		foreach (string synonym in synonyms)
		{
			if (csb.TryGetValue(synonym, out value))
				return value;
		}

		return null;
	}


	public static object ParseEnum(Type enumType, object enumValue)
	{
		object value = null;

		if (enumValue != null)
		{
			if (enumValue.GetType().IsSubclassOf(typeof(Enum)) || enumValue.GetType() == typeof(int))
				value = Convert.ToInt32(enumValue);
			else if (enumValue is string s)
				value = Convert.ToInt32(Enum.Parse(enumType, s, true));
		}
		return value;
	}


	public override bool Remove(string keyword)
	{
		if (base.Remove(keyword))
			return true;

		IList<string> synonyms = Describers.GetSynonyms(keyword);

		foreach (string synonym in synonyms)
		{
			if (base.Remove(synonym))
				return true;
		}

		return false;

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


	/// <summary>
	/// Performs a stored equivalency check against connections. Many commands as well as
	/// PropertyWindows use CsbAgent to do status validations and updates on pulsed events.
	/// This can result in a very high volume of calls, so the agent stores the document
	/// moniker and connection strings for low overhead respnses when the connection has not
	/// changed.
	/// </summary>
	/// <param name="obj"></param>
	/// <returns></returns>
	public override bool Equals(object obj)
	{
		if (obj is not IDbConnection connection)
			return base.Equals(obj);

		if (connection == null) return false;

		if (_EquivalencyConnectionString != null
			&& _EquivalencyConnectionString == connection.ConnectionString)
		{
			return SafeDatasetMoniker.Equals(_EquivalencyMoniker, StringComparison.InvariantCulture);
		}

		string datasetMoniker = BuildUniqueConnectionUrl(connection);

		_EquivalencyConnectionString = connection.ConnectionString;
		_EquivalencyMoniker = datasetMoniker;

		return SafeDatasetMoniker.Equals(datasetMoniker, StringComparison.InvariantCulture);
	}


	public override int GetHashCode()
	{
		return base.GetHashCode();
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Builds a uniquely identifiable lc connection url. Connection urls are used for
	/// uniquely naming connections and are unique to equivalent connections according
	/// to describer equivalency.
	/// </summary>
	/// <returns>
	/// The unique connection url in format:
	/// fbsql://user_uc@server:port/database_lc_serialized/[role_uc.charset_uc.dialect.noTriggersTrueFalse]/
	/// </returns>
	// ---------------------------------------------------------------------------------
	private string BuildUniqueConnectionUrl(bool safeUrl)
	{
		// We'll use UriBuilder for the url.

		if (string.IsNullOrWhiteSpace(DataSource) || string.IsNullOrWhiteSpace(Database)
			|| string.IsNullOrWhiteSpace(UserID))
		{
			return null;
		}

		UriBuilder urlb = new()
		{
			Scheme = C_Scheme,
			Host = DataSource.ToLowerInvariant(),
			UserName = UserID.ToLowerInvariant(),
			Port = Port,
			Password = safeUrl ? string.Empty : Password.ToLowerInvariant()
		};


		// Append the serialized database path and dot separated equivalency connection properties as the url path.

		// Serialize the db path.
		string str = StringUtils.Serialize64(Database.ToLowerInvariant());

		// Tracer.Trace(GetType(), "BuildUniqueConnectionUrl(IDbConnection)", "database.ToLc: {0}, serialized: {1}.", Database.ToLowerInvariant(), str);

		// string str = JsonConvert.SerializeObject(_Database.ToLowerInvariant());
		// string str = JsonSerializer.Serialize(_Database.ToLowerInvariant(), typeof(string));

		// Tracer.Trace(GetType(), "BuildUniqueConnectionUrl()", "Serialized dbpath: {0}", str);

		StringBuilder stringBuilder = new(str);
		stringBuilder.Append("/");

		// Append equivalency properties composite as defined in the Describers Colleciton.

		if (!string.IsNullOrWhiteSpace(Role) ||
			!Charset.Equals(C_DefaultCharset)
			|| Dialect != C_DefaultDialect || NoDatabaseTriggers)
		{
			stringBuilder.Append(Role.ToLowerInvariant());
			stringBuilder.Append(C_CompositeSeparator);
			stringBuilder.Append(Charset.ToLowerInvariant());
			stringBuilder.Append(C_CompositeSeparator);
			stringBuilder.Append(Dialect.ToString());
			stringBuilder.Append(C_CompositeSeparator);
			stringBuilder.Append(NoDatabaseTriggers.ToString().ToLowerInvariant());
		}

		stringBuilder.Append("/");

		urlb.Path = stringBuilder.ToString();

		string result = urlb.Uri.ToString();

		// Tracer.Trace(GetType(), "BuildUniqueConnectionUrl(IDbConnection)", "Url: {0}", result);

		// We have a unique connection url
		return result;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Builds a uniquely identifiable lc connection url given a connection..
	/// </summary>
	/// <returns>
	/// The unique connection url in format:
	/// fbsql://user_uc@server:port/database_lc_serialized/[role_uc.charset_uc.dialect.noTriggersTrueFalse]/
	/// </returns>
	// ---------------------------------------------------------------------------------
	public static string BuildUniqueConnectionUrl(IDbConnection connection)
	{
		// We'll use UriBuilder for the url.

		DbConnection conn = connection as DbConnection;

		string server = conn.DataSource;
		string database = conn.Database;

		if (string.IsNullOrWhiteSpace(server) || string.IsNullOrWhiteSpace(database))
			return null;

		FbConnectionStringBuilder csb = new(connection.ConnectionString);

		int port = csb.Port;
		string user = csb.UserID;
		// string password = csb.Password;
		string role = csb.Role;
		string charset = csb.Charset;
		int dialect = csb.Dialect;
		bool noTriggers = csb.NoDatabaseTriggers;


		UriBuilder urlb = new()
		{
			Scheme = C_Scheme,
			Host = server.ToLowerInvariant(),
			UserName = user.ToLowerInvariant(),
			Port = port,
		};

		string str = StringUtils.Serialize64(database.ToLowerInvariant());

		StringBuilder stringBuilder = new(str);
		stringBuilder.Append("/");

		if (!string.IsNullOrWhiteSpace(role) ||
			!charset.Equals(C_DefaultCharset)
			|| dialect != C_DefaultDialect || noTriggers)
		{
			stringBuilder.Append(role.ToLowerInvariant());
			stringBuilder.Append(C_CompositeSeparator);
			stringBuilder.Append(charset.ToLowerInvariant());
			stringBuilder.Append(C_CompositeSeparator);
			stringBuilder.Append(dialect.ToString());
			stringBuilder.Append(C_CompositeSeparator);
			stringBuilder.Append(noTriggers.ToString().ToLowerInvariant());
		}

		stringBuilder.Append("/");

		urlb.Path = stringBuilder.ToString();

		return urlb.Uri.ToString();

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Extracts dataset information from a Server Explorer node.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void Extract(IVsDataExplorerNode node)
	{
		// Tracer.Trace(GetType(), "Extract(IVsDataExplorerNode)");

		IVsDataObject @nodeObj = node.Object;

		if (@nodeObj == null)
		{
			ArgumentNullException ex = new($"{node.Name} Object is null");
			Diag.Dug(ex);
			return;
		}

		EnModelObjectType objType = node.ModelObjectType();


		// Tracer.Trace(GetType(), "Extract(IVsDataExplorerNode)", "Node type is {0}.", objType);

		IVsDataObject @dbObj;

		if (objType == EnModelObjectType.Database)
			@dbObj = @nodeObj;
		else
			@dbObj = node.ExplorerConnection.ConnectionNode.Object;

		if (@dbObj != null)
		{
			foreach (KeyValuePair<string, Describer> pair in CsbAgent.Describers)
			{
				try
				{
					if (@dbObj.Properties[pair.Key] == DBNull.Value)
						continue;

					object value = @dbObj.Properties[pair.Key];

					if (pair.Value.DefaultEquals(value))
						continue;

					this[pair.Key] = value;
				}
				catch (Exception ex)
				{
					Diag.Dug(ex, $"Node property: {pair.Key}");
					throw;
				}
			}
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets a registered database connection configuration.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected (string, string) GetConnectionDatasetKey(bool autoCreate)
	{
		// Tracer.Trace(GetType(), "GetConnectionDatsetKey()", "AutoCreate: {0}.", autoCreate);

		LoadConfiguredConnections();


		if (string.IsNullOrWhiteSpace(DataSource) || string.IsNullOrWhiteSpace(Database))
		{
			return (null, null);
		}

		string connectionUrl = BuildUniqueConnectionUrl(true);

		// If the unique url exists, use its datasetkey. Otherwise create a new one,
		// numbering it if duplicates exist.
		if (_SConnectionMonikers == null || !SConnectionMonikers.TryGetValue(connectionUrl, out string connectionString))
		{
			if (autoCreate)
				return RegisterUniqueConnectionDatasetKey(false);

			return (null, null);
		}

		// Tracer.Trace(GetType(), "GetConnectionDatsetKey()", "connectionParameters: {0}.", connectionParameters);

		// connectionParameters form:
		// Server(uniqueDatasetId) \n User \n Password \n Port \n dbPath \n Role
		// \n Charset \n Dialect \n NoDatabaseTriggers

		FbConnectionStringBuilder csb = new(connectionString);

		string uniqueDatasetKey = (string)csb["DatasetKey"];
		string uniqueDatasetId = (string)csb["DatasetId"];

		return (uniqueDatasetKey, uniqueDatasetId);
	}



	public static string GetDatasetConnectionString(string datasetKey)
	{
		return GetDatasetConnectionString(datasetKey, false);
	}



	private static string GetDatasetConnectionString(string datasetKey, bool initializing)
	{
		if (!initializing)
			LoadConfiguredConnections();

		if (_SDatasetKeys == null || !_SDatasetKeys.TryGetValue(datasetKey, out string connectionUrl))
			return null;

		if (!_SConnectionMonikers.TryGetValue(connectionUrl, out string connectionString))
		{
			ArgumentException ex = new($"Connection string for DatasetKey {datasetKey} connectionUrl {connectionUrl} was not found.");
			Diag.Dug(ex);
			throw ex;
		}

		return connectionString;
	}


	protected static IDictionary<string, string> LoadConfiguredConnections()
	{
		if (_SDatasetKeys == null)
			ConnectionLocator.LoadConfiguredConnections();


		return _SDatasetKeys;
	}


	protected static IDictionary<string, string> LoadConfiguredConnectionMonikers()
	{
		if (_SDatasetKeys == null)
			ConnectionLocator.LoadConfiguredConnections();

		return _SConnectionMonikers;
	}




	// ---------------------------------------------------------------------------------
	// ---------------------------------------------------------------------------------
	protected abstract void Parse(IBPropertyAgent ci);



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Registers a dataset configuration for this csb if it does not exist or gets the
	/// dataset key and dataset id if already registered.
	///	Clients must always register a csb using one of the register methods before
	///	attempting to access DatasetKey or DatasetId.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public void RegisterDataset()
	{
		RegisterUniqueConnectionDatasetKey(false);
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Builds a uniquely identifiable connection url and registers it's unique
	/// DatasetKey in the form Server (DatasetId). The minimum property requirement for
	/// a dataset to be registered is DataSource, Database and UserID.
	/// </summary>
	/// <param name="initializating">
	/// True if the call is being made by ConnectionLocator for registering FlameRobin
	/// or Solution preconfigured connections else false.
	/// </param>
	/// <returns>
	/// The unique connection DatsetKey.
	/// </returns>
	/// <remarks>
	/// Connection datasets are used for uniquely naming connections and are unique to
	/// equivalent connections according to describer equivalency as defined in the Describers
	/// collection.
	/// Dictionary tables updated:
	/// SConnectionMonikers
	/// key: fbsql://user@server:port//database_lc_serialized/[role_lc.charset_uc.dialect.noTriggersTrueFalse]/
	/// value: Connection info in form Server(uniqueDatasetId) \n User \n Password \n Port \n dbPath \n
	/// Role \n Charset \n Dialect \n NoDatabaseTriggers \.
	/// We don't use the values in the url key because they're not in the original case.
	/// SDatasetKeys
	/// key: DatasetKey in form server(uniqueDatasetId).
	/// value: The SConnectionMonikers url key.
	/// </remarks>
	// ---------------------------------------------------------------------------------
	protected (string, string) RegisterUniqueConnectionDatasetKey(bool initializing = false)
	{
		// Tracer.Trace(GetType(), "RegisterUniqueConnectionDatsetKey()");

		// If this is not a ConnectionLocator initialization then that has to be done first.
		if (!initializing)
			LoadConfiguredConnections();

		string connectionString;
		// If there is a DatasetKey then this csa has been registered and all that remains
		// is to load any additional non-default properties from the register and return.
		if (!string.IsNullOrWhiteSpace(DatasetKey))
		{
			connectionString = GetDatasetConnectionString(DatasetKey, initializing);

			if (connectionString != null)
			{
				UpdateProperties(connectionString);
				return (DatasetKey, DatasetId);
			}
		}


		// At this point the only way to get or register a connection configuration
		// is through a unique connection url, which requires at a minimum DataSource,
		// Database and UserID.

		string connectionUrl = BuildUniqueConnectionUrl(true);

		if (connectionUrl == null)
			return (null, null);

		string uniqueDatasetId = null;
		string uniqueDatasetKey = null;

		// If the unique url exists, use its datasetkey. Otherwise create a new one,
		// numbering it if duplicates exist.
		if (_SConnectionMonikers == null || !SConnectionMonikers.TryGetValue(connectionUrl, out connectionString))
		{
			// Unique connection url for the connection is not registered.
			// Register it with the next available DatasetKey.

			// Set the base for naming the DatasetId part of DatasetKey.
			string datasetId = string.IsNullOrWhiteSpace(DatasetId) ? Dataset : DatasetId;

			// Establish a unique key using i as the suffix.
			// This loop will execute at least once.
			for (int i = 0; i <= SDatasetKeys.Count; i++)
			{
				uniqueDatasetId = datasetId + (i == 0 ? "" : $"_{i + 1}");
				uniqueDatasetKey = C_DatasetKeyFmt.FmtRes(DataSource, uniqueDatasetId);

				if (!_SDatasetKeys.ContainsKey(uniqueDatasetKey))
					break;
			}

			this["DatasetKey"] = uniqueDatasetKey;
			this["DatasetId"] = uniqueDatasetId;

			SConnectionMonikers.Add(connectionUrl, ConnectionString);
			SDatasetKeys.Add(uniqueDatasetKey, connectionUrl);
			// Tracer.Trace(GetType(), "RegisterUniqueDataset()", "ADDED uniqueDatasetKey: {0}, uniqueDatasetId: {1}, connectionUrl: {2}, ConnectionString: {3}.", uniqueDatasetKey, uniqueDatasetId, connectionUrl, ConnectionString);
		}
		else
		{
			if (initializing)
				return (null, null);
			// connectionParameters form:
			// Server(uniqueDatasetId) \n User \n Password \n Port \n dbPath \n Role
			// \n Charset \n Dialect \n NoDatabaseTriggers

			UpdateProperties(connectionString);
			uniqueDatasetKey = DatasetKey;
			uniqueDatasetId = DatasetId;

			// Tracer.Trace(GetType(), "RegisterUniqueDataset()", "FOUND uniqueDatasetKey: {0} uniqueDatasetId: {1}.", uniqueDatasetKey, uniqueDatasetId);
		}


		return (uniqueDatasetKey, uniqueDatasetId);
	}



	public static void Reset()
	{
		_SDatasetKeys = null;
		_SConnectionMonikers = null;
	}




	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Converts a key and value to a KeyValuePair<string, string>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static KeyValuePair<string, string> StringPair(string key, string value)
	{
		return new KeyValuePair<string, string>(key, value);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Updates any unset properties with properties in connectionString.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void UpdateProperties(string connectionString)
	{
		CsbAgent csa = new(connectionString);

		foreach (string key in CsbAgent.Describers.Keys)
		{
			if (!csa.ContainsKey(key) || ContainsKey(key))
				continue;

			this[key] = csa[key];
		}
	}


	#endregion Methods

}
