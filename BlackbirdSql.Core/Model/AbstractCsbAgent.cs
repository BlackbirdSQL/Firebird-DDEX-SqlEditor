
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using BlackbirdSql.Core.Ctl;
using BlackbirdSql.Core.Ctl.ComponentModel;
using BlackbirdSql.Core.Ctl.Config;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.Core.Model.Enums;

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

	public AbstractCsbAgent(string connectionString, bool validateServerName) : base(connectionString)
	{
		if (validateServerName)
			ValidateServerName();
	}



	public AbstractCsbAgent(IDbConnection connection, bool validateServerName) : base(connection.ConnectionString)
	{
		if (validateServerName)
			ValidateServerName();
	}

	public AbstractCsbAgent(IBPropertyAgent ci, bool validateServerName) : base()
	{
		Parse(ci);

		if (validateServerName)
			ValidateServerName();
	}


	protected AbstractCsbAgent(IVsDataExplorerNode node, bool validateServerName) : base()
	{
		Extract(node);
		if (validateServerName)
			ValidateServerName();
	}


	/// <summary>
	/// .ctor for use only by AbstruseCsbAgent for registering FlameRobin datasetKeys.
	/// </summary>
	public AbstractCsbAgent(string server, int port, string database, string user,
				string password, string charset) : base()
	{
		Initialize(server, port, C_DefaultServerType, database, user, password,
			C_DefaultRole, charset, C_DefaultDialect, C_DefaultNoDatabaseTriggers);
		ValidateServerName();
	}


	private void Initialize(string server, int port, FbServerType serverType, string database, string user,
		string password, string role, string charset, int dialect, bool noTriggers)
	{
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


	#endregion Constructors / Destructors




	// =====================================================================================================
	#region Constants - AbstractCsbAgent
	// =====================================================================================================


	#endregion Constants




	// =====================================================================================================
	#region Fields - AbstractCsbAgent
	// =====================================================================================================


	private string _SafeDatasetMoniker = null;
	private string _UnsafeDatasetMoniker = null;

	// A private 'this' object lock
	private readonly object _LockLocal = new();

	// A protected 'this' object lock
	protected readonly object _LockObject = new();

	private bool _IndexActive = false;





	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Describer collection of uniform properties used by the FbConnectionStringBuilder
	/// wrapper, CsbAgent, as well as PropertyAgent and it's descendent SqlEditor
	/// ConnectionInfo and Dispatcher connection classes, and also the SE root nodes,
	/// Root and Database.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected static readonly DescriberDictionary Describers = new(
		[
			new Describer(C_KeyExDatasetKey, typeof(string), C_DefaultExDatasetKey),
			new Describer(C_KeyExConnectionKey, typeof(string), C_DefaultExConnectionKey),
			new Describer(C_KeyExDatasetId, typeof(string), C_DefaultExDatasetId),
			new Describer(C_KeyExDataset, typeof(string), C_DefaultExDataset),
			new Describer(C_KeyExConnectionName, typeof(string), C_DefaultExConnectionName),
			new Describer(C_KeyExConnectionSource, typeof(EnConnectionSource), C_DefaultExConnectionSource),

			new Describer(C_KeyDataSource, C_KeyFbDataSource, typeof(string), C_DefaultDataSource, true, false, true, true), // *
			new Describer(C_KeyPort, C_KeyFbPort, typeof(int), C_DefaultPort, true, false), // *
			new Describer(C_KeyServerType, C_KeyFbServerType, typeof(FbServerType), C_DefaultServerType, true, false), // *
			new Describer(C_KeyDatabase, C_KeyFbDatabase, typeof(string), C_DefaultDatabase, true, false, true, true), // *
			new Describer(C_KeyUserID, C_KeyFbUserID, typeof(string), C_DefaultUserID, true, false, true, true), // *
			new Describer(C_KeyPassword, C_KeyFbPassword, typeof(string), C_DefaultPassword, true, false, false, true),

			new Describer(C_KeyRole, C_KeyFbRole, typeof(string), C_DefaultRole, true, false), // *
			new Describer(C_KeyDialect, C_KeyFbDialect, typeof(int), C_DefaultDialect, true, false), // *
			new Describer(C_KeyCharset, C_KeyFbCharset, typeof(string), C_DefaultCharset, true, false), // *
			new Describer(C_KeyNoDatabaseTriggers, C_KeyFbNoDatabaseTriggers, typeof(bool), C_DefaultNoDatabaseTriggers, true), // *
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
			StringPair("server", C_KeyDataSource),
			StringPair("host", C_KeyDataSource),
			StringPair("uid", C_KeyUserID),
			StringPair("user", C_KeyUserID),
			StringPair("username", C_KeyUserID),
			StringPair("user name", C_KeyUserID),
			StringPair("userpassword", C_KeyPassword),
			StringPair("user password", C_KeyPassword),
			StringPair("no triggers", C_KeyNoDatabaseTriggers),
			StringPair("nodbtriggers", C_KeyNoDatabaseTriggers),
			StringPair("no dbtriggers", C_KeyNoDatabaseTriggers),
			StringPair("no database triggers", C_KeyNoDatabaseTriggers),
			StringPair("timeout", C_KeyConnectionTimeout),
			StringPair("db cache pages", C_KeyDbCachePages),
			StringPair("cachepages", C_KeyDbCachePages),
			StringPair("pagebuffers", C_KeyDbCachePages),
			StringPair("page buffers", C_KeyDbCachePages),
			StringPair("wire compression", C_KeyCompression),
			StringPair("app", C_KeyApplicationName),
			StringPair("parallel", C_KeyParallelWorkers)
		]
	);


	#endregion Fields




	// =====================================================================================================
	#region Property accessors - AbstractCsbAgent
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
				throw new ArgumentNullException(nameof(key));

			lock (_LockObject)
			{
				if (_IndexActive)
					return base[key];

				try
				{

					_IndexActive = true;

					object result;

					switch (key)
					{
						case C_KeyExDatasetKey:
							result = DatasetKey;
							break;
						case C_KeyExConnectionName:
							result = ConnectionName;
							break;
						case C_KeyExDatasetId:
							result = DatasetId;
							break;
						case C_KeyExDataset:
							result = Dataset;
							break;
						case C_KeyExConnectionSource:
							result = ConnectionSource;
							break;
						case C_KeyExClientVersion:
							result = ClientVersion;
							break;
						case C_KeyExMemoryUsage:
							result = MemoryUsage;
							break;
						case C_KeyExActiveUsers:
							result = ActiveUsers;
							break;
						case C_KeyExConnectionKey:
							result = ConnectionKey;
							break;
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
							result = ServerType;
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
							result = IsolationLevel;
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
							result = WireCrypt;
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
							result = base[key];
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
			if (key == null)
				throw new ArgumentNullException(nameof(key));

			lock (_LockObject)
			{
				if (_IndexActive)
				{
					base[key] = value;
					return;
				}

				try
				{
					if (value == DBNull.Value)
						value = null;

					_IndexActive = true;

					switch (key)
					{
						case C_KeyExDatasetKey:
							DatasetKey = (string)value;
							break;
						case C_KeyExConnectionName:
							ConnectionName = (string)value;
							break;
						case C_KeyExDatasetId:
							DatasetId = (string)value;
							break;
						case C_KeyExDataset:
							break;
						case C_KeyExConnectionSource:
							if (value is EnConnectionSource source)
								ConnectionSource = source;
							else if (value is string s && Enum.TryParse<EnConnectionSource>(s, true, out EnConnectionSource enumResult))
								ConnectionSource = enumResult;
							else
								ConnectionSource = (EnConnectionSource)value;
							break;
						case C_KeyExClientVersion:
							ClientVersion = (Version)value;
							break;
						case C_KeyExMemoryUsage:
							MemoryUsage = (string)value;
							break;
						case C_KeyExActiveUsers:
							ActiveUsers = Convert.ToInt32(value);
							break;
						case C_KeyExConnectionKey:
							ConnectionKey = (string)value;
							break;
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
							else if (value is string s && Enum.TryParse<FbServerType>(s, true, out FbServerType enumResult))
								ServerType = enumResult;
							else
								ServerType = (FbServerType)value;
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
							else if (value is string s && Enum.TryParse<IsolationLevel>(s, true, out IsolationLevel enumResult))
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
							else if (value is string s && Enum.TryParse<FbWireCrypt>(s, true, out FbWireCrypt enumResult))
								WireCrypt = enumResult;
							else
								WireCrypt = (FbWireCrypt)value;
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
							base[key] = value;
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
	/// The original proposed DatasetKey supplied by a preconfigured connection string or connection dialog.
	/// If no proposed key is specified the auto-generated DatasetKey will be used on registration.
	/// </summary>
	[GlobalizedCategory("PropertyCategoryIdentifiers")]
	[GlobalizedDisplayName("PropertyDisplayConnectionName")]
	[GlobalizedDescription("PropertyDescriptionConnectionName")]
	[ReadOnly(true)]
	[DefaultValue(C_DefaultExConnectionName)]
	public string ConnectionName
	{
		get
		{
			if (TryGetValue(C_KeyExConnectionName, out object value))
				return (string)value;

			return C_DefaultExConnectionName;
		}
		set
		{
			this[C_KeyExConnectionName] = value;
		}
	}



	/// <summary>
	/// The server scope unique name for a connection configuration database used in DatasetKey
	/// 'Server (DatasetId)'.
	/// Clients must always register a csb using one of the register methods before attempting to
	/// access DatasetKey or DatasetId.
	/// </summary>
	[GlobalizedCategory("PropertyCategoryIdentifiers")]
	[GlobalizedDisplayName("PropertyDisplayDatasetId")]
	[GlobalizedDescription("PropertyDescriptionDatasetId")]
	[ReadOnly(true)]
	[DefaultValue(C_DefaultExDatasetId)]
	public string DatasetId
	{
		get
		{
			if (TryGetValue(C_KeyExDatasetId, out object value))
				return (string)value;

			return C_DefaultExDatasetId;
		}
		set
		{
			this[C_KeyExDatasetId] = value;
		}
	}


	/// <summary>
	/// The unique key for an ide session connection configuration in the form 'Server (DatasetId)'.
	/// Clients must always register a csb using one of the register methods before attempting to
	/// access DatasetKey or DatasetId.
	/// </summary>
	[GlobalizedCategory("PropertyCategoryIdentifiers")]
	[GlobalizedDisplayName("PropertyDisplayDatasetKey")]
	[GlobalizedDescription("PropertyDescriptionDatasetKey")]
	[ReadOnly(true)]
	[DefaultValue(C_DefaultExDatasetKey)]
	public string DatasetKey
	{
		get
		{
			if (TryGetValue(C_KeyExDatasetKey, out object value))
				return (string)value;

			return C_DefaultExDatasetKey;
		}
		set
		{
			this[C_KeyExDatasetKey] = value;
		}
	}



	/// <summary>
	/// The internal Server Explorer connection id for Server Explorer connections.
	/// </summary>
	[GlobalizedCategory("PropertyCategoryIdentifiers")]
	[GlobalizedDisplayName("PropertyDisplayConnectionKey")]
	[GlobalizedDescription("PropertyDescriptionConnectionKey")]
	[ReadOnly(true)]
	[DefaultValue(C_DefaultExConnectionKey)]
	public string ConnectionKey
	{
		get
		{
			if (TryGetValue(C_KeyExConnectionKey, out object value))
				return (string)value;

			return C_DefaultExConnectionKey;
		}
		set
		{
			this[C_KeyExConnectionKey] = value;
		}
	}



	[GlobalizedCategory("PropertyCategoryIdentifiers")]
	[GlobalizedDisplayName("PropertyDisplayDataset")]
	[GlobalizedDescription("PropertyDescriptionDataset")]
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
	/// The original proposed DatasetKey supplied by a preconfigured connection string or connection dialog.
	/// If no proposed key is specified the auto-generated DatasetKey will be used on registration.
	/// </summary>
	[GlobalizedCategory("PropertyCategoryIdentifiers")]
	[GlobalizedDisplayName("PropertyDisplayConnectionSource")]
	[GlobalizedDescription("PropertyDescriptionConnectionSource")]
	[ReadOnly(true)]
	[DefaultValue(C_DefaultExConnectionSource)]
	public EnConnectionSource ConnectionSource
	{
		get
		{
			if (TryGetValue(C_KeyExConnectionSource, out object value))
			{
				switch (value)
				{
					case EnConnectionSource source:
						return source;
					case string s when Enum.TryParse<EnConnectionSource>(s, true, out EnConnectionSource enumResult):
						return enumResult;
					default:
						return (EnConnectionSource)value;
				}
			}

			return C_DefaultExConnectionSource;
		}
		set
		{
			this[C_KeyExConnectionSource] = value;
		}
	}


	/// <summary>
	/// The server memory usage.
	/// </summary>
	[Browsable(false)]
	[GlobalizedCategory("PropertyCategoryExtended")]
	[GlobalizedDisplayName("PropertyDisplayClientVersion")]
	[GlobalizedDescription("PropertyDescriptionClientVersion")]
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
	[GlobalizedCategory("PropertyCategoryExtended")]
	[GlobalizedDisplayName("PropertyDisplayMemoryUsage")]
	[GlobalizedDescription("PropertyDescriptionMemoryUsage")]
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
	[GlobalizedCategory("PropertyCategoryExtended")]
	[GlobalizedDisplayName("PropertyDisplayActiveUsers")]
	[GlobalizedDescription("PropertyDescriptionActiveUsers")]
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


	[Browsable (false)]
	public string DisplayName
	{
		get
		{
			string retval = DatasetId;
			if (string.IsNullOrWhiteSpace(retval))
				retval = Dataset;
			if (!string.IsNullOrWhiteSpace(ConnectionName))
				retval = ConnectionName + " | " + retval;
			return retval;
		}
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Builds and returns a uniquely identifiable connection url. Connection urls
	/// are used for uniquely naming connections and are unique to equivalent
	/// connections according to describer equivalency.
	/// </summary>
	/// <returns>
	/// The unique connection url in format:
	/// fbsql://user_uc@server:port/Serilize64(databasepath_lc)/[Serilize64(newline_delimited_equivalencykeys)]/
	/// </returns>
	// ---------------------------------------------------------------------------------
	[Browsable(false)]
	public string DatasetMoniker
	{
		get
		{
			_SafeDatasetMoniker = BuildUniqueConnectionUrl(true);
			return _SafeDatasetMoniker;
		}
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns a stored uniquely identifiable connection url. Connection urls are used
	/// for uniquely naming connections and are unique to equivalent connections
	/// according to describer equivalency.
	/// </summary>
	/// <returns>
	/// The unique connection url in format:
	/// fbsql://user_uc@server:port/Serilize64(databasepath_lc)/[Serilize64(newline_delimited_equivalencykeys)]/
	/// </returns>
	// ---------------------------------------------------------------------------------
	[Browsable(false)]
	public string SafeDatasetMoniker => _SafeDatasetMoniker ??= BuildUniqueConnectionUrl(true);


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns a stored uniquely identifiable connection url. Connection urls are used
	/// for uniquely naming connections and are unique to equivalent connections
	/// according to describer equivalency.
	/// </summary>
	/// <returns>
	/// The unique connection url in format:
	/// fbsql://user_uc@server:port/Serilize64(databasepath_lc)/[Serilize64(newline_delimited_equivalencykeys)]/
	/// </returns>
	// ---------------------------------------------------------------------------------
	[Browsable(false)]
	public string UnsafeDatasetMoniker => _UnsafeDatasetMoniker ??= BuildUniqueConnectionUrl(false);


	#endregion Property accessors





	// =====================================================================================================
	#region Overloaded Property accessors - AbstractCsbAgent
	// =====================================================================================================


	// TBC


	#endregion Overloaded Property accessors





	// =====================================================================================================
	#region Methods - AbstractCsbAgent
	// =====================================================================================================



	public new void Add(string keyword, object value)
	{
		string key = keyword;
		Describer describer = Describers[keyword];

		if (describer != null)
			key = describer.Key;

		this[key] = value;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Checks whether or not connection property/parameter equivalency objects are
	/// equivalent
	/// </summary>
	/// <remarks>
	/// We consider connections equivalent if they will produce the same results. The connection properties
	/// that determine this equivalency are defined in <see cref="CoreProperties.EquivalencyKeys"/>.
	/// </remarks>
	public static bool AreEquivalent(DbConnectionStringBuilder csb1, DbConnectionStringBuilder csb2)
	{
		// Tracer.Trace(typeof(AbstractCsbAgent), "AreEquivalent(DbConnectionStringBuilder, DbConnectionStringBuilder)");

		return AreEquivalent(csb1, csb2, Describers.EquivalencyKeys);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Checks whether or not the emumerator property/parameter objects are equivalent.
	/// </summary>
	public static bool AreEquivalent(DbConnectionStringBuilder csb1, DbConnectionStringBuilder csb2, IEnumerable<Describer> enumerator, bool deep = false)
	{
		// Tracer.Trace(typeof(AbstractCsbAgent), "AreEquivalent(DbConnectionStringBuilder, DbConnectionStringBuilder)");

		object value1, value2;

		CsbAgent csa1 = (CsbAgent)csb1;
		CsbAgent csa2 = (CsbAgent)csb2;



		try
		{
			// Keep it simple. Loop thorugh each connection string and compare
			// If it's not in the other or null use default

			foreach (Describer describer in enumerator)
			{
				value1 = csa1[describer.Name];
				value2 = csa2[describer.Name];

				if (!AreEquivalent(describer.Name, value1, value2))
				{
					// Tracer.Trace(typeof(AbstractCsbAgent), "AreEquivalent(DbConnectionStringBuilder, DbConnectionStringBuilder)",
					//	"Connection parameter '{0}' mismatch: '{1}' : '{2}.",
					//	describer.Name, value1 != null ? value1.ToString() : "null", value2 != null ? value2.ToString() : "null");
					return false;
				}
			}
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			return false;
		}


		if (deep)
		{
			Describer describer;

			foreach (KeyValuePair<string, object> pair in csa1)
			{
				describer = Describers[pair.Key];

				if (describer != null)
					continue;

				if (!csa2.ContainsKey(pair.Key))
					return false;

				value1 = csa1[pair.Key];
				value2 = csa2[pair.Key];

				if (!AreEquivalent(pair.Key, value1, value2))
					return false;
			}

			foreach (KeyValuePair<string, object> pair in csa2)
			{
				describer = Describers[pair.Key];

				if (describer != null)
					continue;

				if (!csa1.ContainsKey(pair.Key))
					return false;
			}
		}

		// Tracer.Trace(typeof(TConnectionEquivalencyComparer),
		// 	"TConnectionEquivalencyComparer.AreEquivalent(IDictionary, IDictionary)", "Connections are equivalent");

		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Checks whether or not connection property/parameter equivalency objects of
	/// a connection string are equivalent.
	/// </summary>
	/// <remarks>
	/// We consider connections equivalent if they will produce the same results. The connection properties
	/// that determine this equivalency are defined in <see cref="CoreProperties.EquivalencyKeys"/>.
	/// </remarks>
	public static bool AreEquivalent(string connectionString1, string connectionString2)
	{
		// Tracer.Trace(typeof(AbstractCsbAgent), "AreEquivalent(DbConnectionStringBuilder, DbConnectionStringBuilder)");

		return AreEquivalent(connectionString1, connectionString2, Describers.EquivalencyKeys);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Checks whether or not the emumerator property/parameter objects of a connection
	/// string are equivalent.
	/// </summary>
	public static bool AreEquivalent(string connectionString1, string connectionString2, IEnumerable<Describer> enumerator)
	{
		CsbAgent csa1 = new(connectionString1, false);
		CsbAgent csa2 = new(connectionString2, false);

		return AreEquivalent(csa1, csa2, enumerator);
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
	public static bool AreEquivalent(string key, object value1, object value2)
	{
		// Tracer.Trace();

		if (value1 == null)
		{
			if (value2 == null)
				return true;

			return false;
		}

		if (value2 == null)
			return false;


		string text1;
		string text2;

		if (value1 is byte[] bytes1)
			text1 = Encoding.Default.GetString(bytes1);
		else
			text1 = value1.ToString().Trim();

		if (value2 is byte[] bytes2)
			text2 = Encoding.Default.GetString(bytes2);
		else
			text2 = value2.ToString().Trim();

		if (!string.Equals(text1, text2, StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}

		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Builds a uniquely identifiable connection url. Connection urls are used for
	/// uniquely naming connections and are unique to equivalent connections according
	/// to describer equivalency.
	/// </summary>
	/// <returns>
	/// The unique connection url in format:
	/// fbsql://user_uc@server:port/Serilize64(databasepath_lc)/[Serilize64(newline_delimited_equivalencykeys)]/
	/// </returns>
	// ---------------------------------------------------------------------------------
	private string BuildUniqueConnectionUrl(bool safeUrl)
	{
		return BuildUniqueConnectionUrl(DataSource, Database, this, safeUrl);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Builds a uniquely identifiable connection url. Connection urls are used for
	/// uniquely naming connections and are unique to equivalent connections according
	/// to describer equivalency.
	/// </summary>
	/// <returns>
	/// The unique connection url in format:
	/// fbsql://user_uc@server:port/Serilize64(databasepath_lc)/[Serilize64(newline_delimited_equivalencykeys)]/
	/// </returns>
	// ---------------------------------------------------------------------------------
	protected static string BuildUniqueConnectionUrl(string datasource, string database,
		AbstractCsbAgent csa, bool safeUrl)
	{
		// We'll use UriBuilder for the url.

		if (string.IsNullOrWhiteSpace(datasource) || string.IsNullOrWhiteSpace(database)
			|| string.IsNullOrWhiteSpace(csa.UserID))
		{
			return null;
		}

		UriBuilder urlb = new()
		{
			Scheme = SystemData.Protocol,
			Host = datasource.ToLowerInvariant(),
			UserName = csa.UserID.ToLowerInvariant(),
			Port = csa.Port,
			Password = safeUrl ? string.Empty : csa.Password.ToLowerInvariant()
		};

		// Append the serialized database path and dot separated equivalency connection properties as the url path.

		// Serialize the db path.
		StringBuilder stringBuilder = new(StringUtils.Serialize64(csa.Database.ToLowerInvariant()));

		stringBuilder.Append(SystemData.UnixFieldSeparator);


		// Append equivalency properties composite as defined in the Describers Colleciton.

		int i;
		bool hasEquivalencies = false;
		string key;

		for (i = 0; i < PersistentSettings.EquivalencyKeys.Length; i++)
		{
			key = PersistentSettings.EquivalencyKeys[i];

			if (key == "DataSource" || key == "Port" || key == "Database" || key == "UserID")
				continue;

			if (csa.ContainsKey(key) && !Describers[key].DefaultEqualsOrEmptyString(csa[key]))
			{
				hasEquivalencies = true;
				break;
			}
		}

		if (hasEquivalencies)
		{
			object value;
			string stringValue;
			StringBuilder sb = new();

			for (i = 0; i < PersistentSettings.EquivalencyKeys.Length; i++)
			{
				key = PersistentSettings.EquivalencyKeys[i];

				if (key == "DataSource" || key == "Port" || key == "Database" || key == "UserID")
					continue;

				value = csa[key];

				if (value == null)
					stringValue = string.Empty;
				else if (value.GetType() == typeof(byte[]))
					stringValue = Encoding.Default.GetString((byte[])value);
				else
					stringValue = value.ToString().ToLowerInvariant();

				sb.Append(stringValue);

				if (i < (PersistentSettings.EquivalencyKeys.Length - 1))
				{
					sb.Append('\n');
				}

			}

			stringBuilder.Append(StringUtils.Serialize64(sb.ToString()));
		}


		stringBuilder.Append(SystemData.UnixFieldSeparator);

		urlb.Path = stringBuilder.ToString();

		string result = urlb.Uri.ToString();

		// Tracer.Trace(typeof(AbstractCsbAgent), "BuildUniqueConnectionUrl()", "Url: {0}", testurlb.Uri.ToString());

		// We have a unique connection url
		return result;

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



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Extracts dataset information from a Server Explorer node.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void Extract(IVsDataExplorerNode node)
	{
		// Tracer.Trace(GetType(), "Extract(IVsDataExplorerNode)");
		IVsDataExplorerNode connectionNode = node.ExplorerConnection.ConnectionNode;

		IVsDataObject @object = connectionNode.Object;

		if (@object == null)
		{
			COMException ex = new($"Connection node object for node {node.ExplorerConnection.DisplayName} is null");
			Diag.Dug(ex);
			return;
		}


		// Tracer.Trace(GetType(), "Extract(IVsDataExplorerNode)", "Node type is {0}.", objType);

		object value;
		object readOnly;
		PropertyDescriptorCollection descriptors = TypeDescriptor.GetProperties(this);



		foreach (KeyValuePair<string, Describer> pair in CsbAgent.Describers)
		{
			try
			{
				value = @object.Properties[pair.Key];

				if (value == null || value == DBNull.Value)
					continue;

				readOnly = Reflect.GetAttributeValue(descriptors[pair.Key], typeof(ReadOnlyAttribute), "isReadOnly");

				if (readOnly != null && (bool)readOnly)
					continue;


				if (pair.Value.DefaultEquals(value))
					continue;


				this[pair.Key] = value;
			}
			catch (Exception ex)
			{
				Diag.Dug(ex, $"Error retrieving node {node.Name} property: {pair.Key}");
				throw;
			}
		}
		// Tracer.Trace(GetType(), "Extract()", "node.ExplorerConnection./DisplayName: {0}, node.ExplorerConnection.ConnectionNode.Name: {1}, dbObj.Name: {2}.", node.ExplorerConnection.DisplayName, node.ExplorerConnection.ConnectionNode.Name, dbObj.Name);

		ConnectionKey = connectionNode.GetConnectionKey();
		if (ConnectionKey == null)
		{
			COMException ex = new($"ConnectionKey for explorer connection {node.ExplorerConnection.DisplayName} for node {node.Name} is null");
			Diag.Dug(ex);
			throw ex;
		}

		ConnectionSource = EnConnectionSource.ServerExplorer;

		if (!string.IsNullOrWhiteSpace(DatasetKey) && connectionNode.ExplorerConnection.DisplayName != DatasetKey)
		{
			// There has been a name change.
			DatasetKey = connectionNode.ExplorerConnection.DisplayName;
		}

	}



	// ---------------------------------------------------------------------------------
	// ---------------------------------------------------------------------------------
	protected abstract void Parse(IBPropertyAgent ci);



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



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Converts a key and value to a KeyValuePair<string, string>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private static KeyValuePair<string, string> StringPair(string key, string value)
	{
		return new KeyValuePair<string, string>(key, value);
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



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Validates the DataSource for case name mangling.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void ValidateServerName()
	{
		string dataSource, server;

		if (ContainsKey(C_KeyDataSource) && !string.IsNullOrWhiteSpace(dataSource = DataSource)
			&& !RctManager.ShutdownState && !dataSource.Equals(server = RctManager.RegisterServer(dataSource, Port)))
		{
			DataSource = server;
		}
	}





	/// <summary>
	/// Updates descriptor collection readonly on dataset key properties for application
	/// connection sources.
	/// </summary>
	public static bool UpdateDatasetKeysReadOnlyAttribute(ref PropertyDescriptorCollection descriptors, bool readOnly)
	{
		if (descriptors == null || descriptors.Count == 0)
			return false;

		bool value;
		bool updated = false;
		string[] descriptorList = [CoreConstants.C_KeyExConnectionName, CoreConstants.C_KeyExDatasetId];

		try
		{
			foreach (string name in descriptorList)
			{
				PropertyDescriptor descriptor = descriptors.Find(name, false);

				if (descriptor == null)
					continue;

				if (descriptor.Attributes[typeof(ReadOnlyAttribute)] is not ReadOnlyAttribute attr)
					throw new IndexOutOfRangeException($"ReadOnlyAttribute not found in PropertyDescriptor for {name}.");

				FieldInfo fieldInfo = Reflect.GetFieldInfo(attr, "isReadOnly", BindingFlags.NonPublic | BindingFlags.Instance);

				value = readOnly;

				if ((bool)Reflect.GetFieldInfoValue(attr, fieldInfo) != value)
				{
					// Tracer.Trace(typeof(AbstractCsbAgent), "UpdatePropertiesReadOnlyAttribute()", "Setting ReadOnlyAttribute for PropertyDescriptor {0} to readonly: {1}.", name, readOnly);

					updated = true;
					Reflect.SetFieldInfoValue(attr, fieldInfo, value);
				}
			}
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
		}

		return updated;
	}


	#endregion Methods

}
