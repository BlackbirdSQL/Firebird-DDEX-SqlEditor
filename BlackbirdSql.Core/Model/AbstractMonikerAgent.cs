
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using BlackbirdSql.Core.Ctl;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Ctl.Enums;
using BlackbirdSql.Core.Ctl.Extensions;
using BlackbirdSql.Core.Ctl.Interfaces;

using FirebirdSql.Data.FirebirdClient;

using Microsoft.VisualStudio.Data.Services;


namespace BlackbirdSql.Core.Model;

// =========================================================================================================
//
//											AbstractMonikerAgent Class
//
/// <summary>
/// This is the base class of MonikerAgent and it's descendents. MonikerAgent centrally controls moniker
/// naming for the extension. AbstractMonikerAgent contains the logic that centrally controls the allocation
/// of unique connection DatasetKey names. See remarks.
/// </summary>
/// <remarks>
/// A BlackbirdSql connection is uniquely identifiable on it's DatasetKey, Server(DisplayMember), where
/// DisplayMember is the connection name in FlameRobin or the Dataset if the connection cannot be derived
/// from FlameRobin. A Dataset name is the database path's stripped file name. If the DatasetKey is not
/// unique across an ide session, duplicate DisplayMember names will be numerically suffixed beginning
/// with 2. Connections are considered equivalent if the connection equivalency parameters match. To keep
/// things simple the PropertySet describers have DataSource/Server, Database path, UserID, Role and
/// NoDatabaseTriggers set as equivalency parameters. No further distinction takes place.
/// </remarks>
// =========================================================================================================
public abstract class AbstractMonikerAgent
{

	// ---------------------------------------------------------------------------------
	#region Constants - AbstractMonikerAgent
	// ---------------------------------------------------------------------------------

	private const string C_Scheme = "fbsql";
	protected const string C_DatasetKeyFmt = "{0} ({1})";
	private const char C_CompositeSeparator = '.';


	#endregion Constants




	// =========================================================================================================
	#region Variables - AbstractMonikerAgent
	// =========================================================================================================


	protected static IDictionary<string, string> _SDatasetKeys;
	protected static IDictionary<string, string> _SConnectionMonikers;
	protected static IDictionary<string, string> _SPasswords;

	private string _Explorer = "";
	private string _DatasetKey = null;
	private string _Server = "";
	private int _Port = CoreConstants.C_DefaultPortNumber;
	EnDbServerType _ServerType = CoreConstants.C_DefaultServerType;
	private string _Database = "";
	private string _DisplayMember = null;
	private string _Dataset = null;
	private string _User = "";
	private string _Password = "";
	private string _Role = "";
	private string _Charset = ModelConstants.C_DefaultCharacterSet;
	private int _Dialect = ModelConstants.C_DefaultDialect;
	private bool _NoTriggers = ModelConstants.C_DefaultNoDbTriggers;
	private EnModelObjectType _ObjectType;
	protected bool _Alternate = false;
	private string _ObjectName = "";

	private string _SafeDatasetMoniker = null;
	private string _UnsafeDatasetMoniker = null;
	private string _DatabaseMoniker = null;

	#endregion Variables




	// =========================================================================================================
	#region Property accessors - AbstractMonikerAgent
	// =========================================================================================================


	/// <summary>
	/// Contains the list of registered connection dastasetKeys. This accessor may only be referenced inside of
	/// RegisterUniqueConnectionDatsetKey().
	/// </summary>
	protected static IDictionary<string, string> SDatasetKeys => _SDatasetKeys ??= new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
	protected static IDictionary<string, string> SConnectionMonikers => _SConnectionMonikers ??= new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
	protected static IDictionary<string, string> SPasswords => _SPasswords ??= new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

	public string Explorer => _Explorer;

	public string Server => _Server;

	public int Port => _Port;

	public EnDbServerType ServerType => _ServerType;

	public string Database => _Database;

	public string Dataset => _Dataset ??= (string.IsNullOrWhiteSpace(_Database) ? "" : Path.GetFileNameWithoutExtension(_Database));


	public string DatasetKey => _DatasetKey ??= RegisterUniqueConnectionDatsetKey(false);

	public string DisplayMember
	{
		get
		{
			if (!string.IsNullOrWhiteSpace(_DatasetKey))
				return _DisplayMember;

			RegisterUniqueConnectionDatsetKey(false);

			return _DisplayMember;
		}
	}


	public string User => _User;

	public string Role => _Role;

	public string Charset => _Charset;

	public int Dialect => _Dialect;

	public bool NoTriggers => _NoTriggers;

	protected string Password => _Password;


	public EnModelObjectType ObjectType => _ObjectType;


	protected string ObjectName => _ObjectName;

	/// <summary>
	/// Returns the unique dataset connection url in the form
	/// fbsql://user@server/database_lc_serialized/role_lc.noTriggersTrueFalse/
	/// </summary>
	public string SafeDatasetMoniker => _SafeDatasetMoniker ??= BuildUniqueConnectionUrl(true);
	public string UnsafeDatasetMoniker => _UnsafeDatasetMoniker ??= BuildUniqueConnectionUrl(false);


	/// <summary>
	/// Returns the unique document moniker url prefix in the form
	/// fbsql://server/database_lc_serialized//
	/// </summary>
	protected string DatabaseMoniker => _DatabaseMoniker ??= BuildUniqueDatabaseUrl();


	#endregion Property accessors




	// =========================================================================================================
	#region Constructors / Destructors - AbstractMonikerAgent
	// =========================================================================================================


	public AbstractMonikerAgent()
	{
	}


	public AbstractMonikerAgent(string fbSqlUrl)
	{
		Parse(fbSqlUrl);
	}


	public AbstractMonikerAgent(IDbConnection connection)
	{
		Parse(connection);
	}


	public AbstractMonikerAgent(IBPropertyAgent ci)
	{
		Parse(ci);
	}


	public AbstractMonikerAgent(IVsDataExplorerNode node)
	{
		Extract(node);
	}



	public AbstractMonikerAgent(string server, int port, EnDbServerType serverType, string database, string user, string password, string role,
			string charset, int dialect, bool noTriggers, EnModelObjectType objectType, IList<string> identifierList)
	{
		Initialize(null, null, server, port, serverType, database, user, password, role, charset, dialect,
			noTriggers, EnModelObjectType.Database, null); ;
	}



	public AbstractMonikerAgent(string displayMember, string server, string database,
			EnModelObjectType objectType, object[] identifier)
	{
		Initialize(displayMember, null, server, CoreConstants.C_DefaultPortNumber, CoreConstants.C_DefaultServerType,
			database, string.Empty, string.Empty, string.Empty, ModelConstants.C_DefaultCharacterSet,
			ModelConstants.C_DefaultDialect, false, objectType, identifier);
	}


	/// <summary>
	/// .ctor for use only by XmlParser for registering FlameRobin datasetKeys.
	/// </summary>
	protected AbstractMonikerAgent(string displayMember, string server, int port, EnDbServerType serverType, string database, string user,
				string password, string role, string charset, int dialect, bool noTriggers)
	{
		Initialize(displayMember, null, server, port, serverType, database, user, password, role, charset, dialect,
			noTriggers, EnModelObjectType.Database, null);
	}

	public AbstractMonikerAgent(string displayMember, string dataset, string server, int port, EnDbServerType serverType, string database, string user,
			string password, string role, string charset, int dialect, EnModelObjectType objectType, object[] identifier)
	{
		Initialize(displayMember, dataset, server, port, serverType, database, user, password, role, charset, dialect, false, objectType, identifier);
	}
		public AbstractMonikerAgent(string displayMember, string dataset, string server, int port, EnDbServerType serverType, string database, string user,
			string password, string role, string charset, int dialect, bool noTriggers, EnModelObjectType objectType, object[] identifier)
	{
		Initialize(displayMember, dataset, server, port, serverType, database, user, password, role, charset, dialect, noTriggers, objectType, identifier);
	}


	private void Initialize(string displayMember, string dataset, string server, int port, EnDbServerType serverType, string database, string user,
		string password, string role, string charset, int dialect, bool noTriggers, EnModelObjectType objectType, object[] identifier)
	{
		_DisplayMember = displayMember;
		_Dataset = dataset;
		_Server = server;
		_Port = port;
		_ServerType = serverType;
		_Database = database;
		_User = user;
		_Password = password;
		_Role = role;
		_Charset = charset;
		_Dialect = dialect;
		_NoTriggers = noTriggers;


		_ObjectType = objectType;
		_ObjectName = "";

		if (identifier != null && identifier.Length > 0)
		{
			_ObjectName = identifier[0] != null ? identifier[0].ToString() : "";
			for (int i = 1; i < identifier.Length; i++)
			{
				_ObjectName += C_CompositeSeparator
					+ (identifier[i] != null ? identifier[i].ToString() : "");
			}
		}

	}


	#endregion Property Constructors / Destructors



	// =========================================================================================================
	#region Methods - AbstractMonikerAgent
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Extracts moniker information from a Server Explorer node.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void Extract(IVsDataExplorerNode node)
	{
		Tracer.Trace(GetType(), "Extract(IVsDataExplorerNode)");

		IVsDataObject @nodeObj = node.Object;

		if (@nodeObj == null)
		{
			ArgumentNullException ex = new($"{node.Name} Object is null");
			Diag.Dug(ex);
			return;
		}

		EnModelObjectType objType = nodeObj.Type.ToModelObjectType();
		if (objType == EnModelObjectType.AlterDatabase)
			objType = EnModelObjectType.Database;

		Tracer.Trace(GetType(), "Extract(IVsDataExplorerNode)", "Node type is {0}.", objType);

		IVsDataObject @dbObj;

		if (objType == EnModelObjectType.Database || objType == EnModelObjectType.AlterDatabase)
		{
			@dbObj = @nodeObj;
		}
		else
		{
			@dbObj = node.ExplorerConnection.ConnectionNode.Object;
			_Explorer = node.ExplorerConnection.DisplayName;
		}

		if (@dbObj != null && @nodeObj != null)
		{
			_DatasetKey = (string)@dbObj.Properties[ModelConstants.C_KeyNodeDatasetKey];
			_Server = (string)@dbObj.Properties[ModelConstants.C_KeyNodeDataSource];
			_Port = (int)@dbObj.Properties[ModelConstants.C_KeyNodePort];
			_ServerType = (EnDbServerType)@dbObj.Properties[ModelConstants.C_KeyNodeServerType];
			_Database = (string)@dbObj.Properties[ModelConstants.C_KeyNodeDatabase];
			_DisplayMember = (string)@dbObj.Properties[ModelConstants.C_KeyNodeDisplayMember];
			_User = (string)@dbObj.Properties[ModelConstants.C_KeyNodeUserId];
			_Password = (string)@dbObj.Properties[ModelConstants.C_KeyNodePassword];
			_Role = (string)@dbObj.Properties[ModelConstants.C_KeyNodeRole];
			_NoTriggers = (bool)@dbObj.Properties[ModelConstants.C_KeyNodeNoDbTriggers];

			_ObjectType = objType;

			if (objType != EnModelObjectType.Database)
				_ObjectName = @nodeObj.Identifier.ToString(DataObjectIdentifierFormat.None);
		}
	}

	protected static IDictionary<string, string> LoadConfiguredConnections()
	{
		if (_SDatasetKeys == null)
			_ = XmlParser.Databases;

		return _SDatasetKeys;
	}


	protected static IDictionary<string, string> LoadConfiguredConnectionMonikers()
	{
		if (_SDatasetKeys == null)
			_ = XmlParser.Databases;

		return _SConnectionMonikers;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Parse a moniker url of the form:
	/// fbsql://[username[:password]@]server/serializedDatabase/[role.noTriggersTrueFalse]/[objectType/id0.id1.id2]
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void Parse(string fbSqlUrl)
	{
		UriBuilder urlb = new UriBuilder(fbSqlUrl);

		Tracer.Trace(GetType(), "Parse(fbSqlUrl)");

		_DisplayMember = null;
		_Server = "";
		_Dataset = null;
		_Database = "";
		_User = "";
		_Password = "";
		_ObjectType = EnModelObjectType.Unknown;
		_ObjectName = "";
		_Role = "";
		_NoTriggers = false;

		// fbsql://[username[:password]@]server/serializedDatabase/[role.noTriggersTrueFalse]/[objectType/id0.id1.id2]

		if (urlb.Scheme == C_Scheme)
		{
			_Server = urlb.Host;
			_Port = urlb.Port;
			_User = urlb.UserName;
			_Password = urlb.Password;

			string[] arr = urlb.Path.Split('/');

			if (arr.Length > 0)
			{
				_Database = StringUtils.Deserialize64(arr[0]);

				if (arr.Length > 1)
				{
					string[] equivalancies = arr[1].Split(C_CompositeSeparator);

					if (equivalancies.Length > 0)
						_Role = equivalancies[0];
					if (equivalancies.Length > 1)
						_Charset = equivalancies[1];
					if (equivalancies.Length > 2)
						_Dialect = Convert.ToInt32(equivalancies[2]);
					if (equivalancies.Length > 3)
						_NoTriggers = equivalancies[3].ToLowerInvariant() == "true";

					if (arr.Length > 2)
					{
						_ObjectType = arr[2].ToModelObjectType();

						if ((int)_ObjectType > 20)
						{
							_Alternate = true;
							_ObjectType -= 20;
						}

						if (arr.Length > 3)
							_ObjectName = arr[3];
					}

				}
			}

		}
	}



	// ---------------------------------------------------------------------------------
	// ---------------------------------------------------------------------------------
	public void Parse(IDbConnection connection)
	{
		Tracer.Trace(GetType(), "Parse(IBPropertyAgent)");

		DbConnection conn = connection as DbConnection;
		_Server = conn.DataSource;
		_Database = conn.Database;

		FbConnectionStringBuilder csb = new(connection.ConnectionString);

		_Port = csb.Port;
		_User = csb.UserID;
		_Password = csb.Password;
		_Role = csb.Role;
		_Charset = csb.Charset;
		_Dialect = csb.Dialect;
		_NoTriggers = csb.NoDatabaseTriggers;

		_ObjectName = "";
		_ObjectType = EnModelObjectType.Database;
	}



	// ---------------------------------------------------------------------------------
	// ---------------------------------------------------------------------------------
	public void Parse(IBPropertyAgent ci)
	{
		Tracer.Trace(GetType(), "Parse(IBPropertyAgent)");

		_Server = string.IsNullOrEmpty((string)ci["DataSource"]) ? "localhost" : (string)ci["DataSource"];
		_Port = (int)ci["Port"];
		_Database = (string)ci["Database"];
		_User = (string)(ci["UserID"] ?? string.Empty);
		_Password = (string)(ci["Password"] ?? string.Empty);

		_Dataset = (string)ci["Dataset"];
		_DisplayMember = (string)ci["DisplayMember"];
		_Charset = (string)ci["Charset"];
		_Dialect = (int)ci["Dialect"];
		_NoTriggers = (bool)ci["NoDbTriggers"];

		_ObjectName = "";
		_ObjectType = EnModelObjectType.Unknown;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Builds a uniquely identifiable connection url. Connection urls are used for
	/// uniquely naming connections and are unique to equivalent connections according
	/// to describer equivalency in the CorePropertySet and ModelPropertySet static
	/// classes.
	/// </summary>
	/// <returns>
	/// The unique connection url in format:
	/// fbsql://user@server:port/database_lc_serialized/role_lc.charset_uc.dialect.noTriggersTrueFalse/
	/// </returns>
	// ---------------------------------------------------------------------------------
	private string BuildUniqueConnectionUrl(bool safeUrl)
	{
		// We'll use UriBuilder for the url.

		if (string.IsNullOrWhiteSpace(_Server) || string.IsNullOrWhiteSpace(_Database)
			|| string.IsNullOrWhiteSpace(_User))
		{
			return null;
		}

		UriBuilder urlb = new()
		{
			Scheme = C_Scheme,
			Host = _Server,
			UserName = _User,
			Port = _Port,
			Password = safeUrl ? string.Empty : _Password
		};


		// Append the serialized database path and dot separated equivalency connection properties as the url path.

		// Serialize the db path.
		string str = StringUtils.Serialize64(_Database);
		// string str = JsonConvert.SerializeObject(_Database.ToLowerInvariant());
		// string str = JsonSerializer.Serialize(_Database.ToLowerInvariant(), typeof(string));

		Tracer.Trace(GetType(), "BuildUniqueConnectionUrl()", "Serialized dbpath: {0}", str);

		StringBuilder stringBuilder = new(str);
		stringBuilder.Append("/");

		// Append equivalency properties composite as defined in the Describers in
		// CorePropertySet and ModelPropertySet.

		if (!string.IsNullOrWhiteSpace(_Role) ||
			!_Charset.Equals(ModelConstants.C_DefaultCharacterSet)
			|| _Dialect != ModelConstants.C_DefaultDialect || _NoTriggers)
		{
			stringBuilder.Append(_Role.ToLowerInvariant());
			stringBuilder.Append(C_CompositeSeparator);
			stringBuilder.Append(_Charset.ToUpperInvariant());
			stringBuilder.Append(C_CompositeSeparator);
			stringBuilder.Append(_Dialect.ToString());
			stringBuilder.Append(C_CompositeSeparator);
			stringBuilder.Append(_NoTriggers.ToString().ToLowerInvariant());
		}

		stringBuilder.Append("/");

		urlb.Path = stringBuilder.ToString();

		string result = urlb.Uri.ToString();

		Tracer.Trace(GetType(), "BuildUniqueConnectionUrl()", "Url: {0}", result);

		// We have a unique connection url
		return result;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Builds a uniquely identifiable database url. Database urls are used for
	/// uniquely naming document monikers and are unique to a database / database path.
	/// </summary>
	/// <returns>
	/// The unique database url in format:
	/// fbsql://server/database_lc_serialized//
	/// </returns>
	// ---------------------------------------------------------------------------------
	protected string BuildUniqueDatabaseUrl()
	{
		// We'll use UriBuilder for the url.

		UriBuilder urlb = new()
		{
			Scheme = C_Scheme,
			Host = _Server,
		};


		// Append the serialized database path.
		
		// Serialize the db path.
		string str = StringUtils.Serialize64(_Database);
		// string str = string.IsNullOrEmpty(_Database) ? "" : JsonConvert.SerializeObject(_Database.ToLowerInvariant());

		Tracer.Trace(GetType(), "BuildUniqueDatabaseUrl()", "Serialized dbpath: {0}", str);

		StringBuilder stringBuilder = new(str);
		stringBuilder.Append("//");

		urlb.Path = stringBuilder.ToString();

		string result = urlb.Uri.ToString();

		Tracer.Trace(GetType(), "BuildUniqueDatabaseUrl()", "Url: {0}", result);

		// We have a unique connection url
		return result;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Builds a uniquely identifiable connection url and registers it's unique
	/// DatasetKey in the form Server(DisplayMember).
	/// Connection datasets are used for uniquely naming connections and are unique to
	/// equivalent connections according to describer equivalency in the CorePropertySet
	/// and ModelPropertySet static classes.
	/// </summary>
	/// <param name="initializating">
	/// True if the call is being made by XmlParser for registering FlameRobin databases
	/// else false.
	/// </param>
	/// <returns>
	/// The unique connection DatsetKey.
	/// </returns>
	/// <remarks>
	/// SConnectionMonikers
	/// key: fbsql://server/database_lc_serialized/role_lc.noTriggersTrueFalse/
	/// value: DatasetKey in form Server(uniqueDisplayMember).
	/// SDatasetKeys
	/// key: DatasetKey in form server(uniqueDisplayMember).
	/// value: fbsql://server/database_lc_serialized/role_lc.noTriggersTrueFalse/
	/// </remarks>
	// ---------------------------------------------------------------------------------
	protected string RegisterUniqueConnectionDatsetKey(bool initializating)
	{
		Tracer.Trace(GetType(), "RegisterUniqueDataset()", "Database: {0}.", _Database);

		// If this is not an XmlParser initialization then that has to be done first.
		if (!initializating)
			LoadConfiguredConnections();

		if (!string.IsNullOrWhiteSpace(_DatasetKey))
			return _DatasetKey;

		if (string.IsNullOrWhiteSpace(_Server) || string.IsNullOrWhiteSpace(_Database)
			|| string.IsNullOrWhiteSpace(_User))
		{
			return "";
		}

		string server = Server;
		string connectionUrl = BuildUniqueConnectionUrl(true);

		string displayMember = !string.IsNullOrWhiteSpace(_DisplayMember) ? _DisplayMember : Dataset;
		string uniqueDisplayMember = displayMember;
		string uniqueDatasetKey = C_DatasetKeyFmt.FmtRes(server, uniqueDisplayMember);

		// If the unique url exists, use its datasetkey. Otherwise create a new one,
		// numbering it if duplicates exist.
		if (_SConnectionMonikers == null || !SConnectionMonikers.TryGetValue(connectionUrl, out uniqueDatasetKey))
		{
			for (int i = 1; i <= SDatasetKeys.Count; i++)
			{
				if (i > 1)
					uniqueDisplayMember = displayMember + i;

				uniqueDatasetKey = C_DatasetKeyFmt.FmtRes(server, uniqueDisplayMember);

				if (!_SDatasetKeys.ContainsKey(uniqueDatasetKey))
					break;
			}
			SConnectionMonikers.Add(connectionUrl, uniqueDatasetKey);
			SDatasetKeys.Add(uniqueDatasetKey, connectionUrl);
			SPasswords.Add(uniqueDatasetKey, Password);
			Tracer.Trace(GetType(), "RegisterUniqueDataset()", "ADDED uniqueDatasetKey: {0} uniqueDisplayMember: {1}.", uniqueDatasetKey, uniqueDisplayMember);
		}
		else
		{
			// Keeping it simple. No need for regex.
			// {0} ({1})
			// MMEI-LT01 (MMEI DEMO)

			// MMEI-LT01 ({1})
			string str = C_DatasetKeyFmt.FmtRes(server, "{1}");
			int pos = str.IndexOf("{1}");
			// pos = 11.
			// len = 21 - 11 - 9 + 5 + 3 = 9
			int len = uniqueDatasetKey.Length - pos - C_DatasetKeyFmt.Length + C_DatasetKeyFmt.IndexOf("{1}") + 3;

			uniqueDisplayMember = uniqueDatasetKey.Substring(pos, len);
			Tracer.Trace(GetType(), "RegisterUniqueDataset()", "FOUND uniqueDatasetKey: {0} uniqueDisplayMember: {1}.", uniqueDatasetKey, uniqueDisplayMember);

		}

		_DisplayMember = uniqueDisplayMember;
		_DatasetKey = uniqueDatasetKey;

		Tracer.Trace(GetType(), "RegisterUniqueDataset()", "DisplayMember: {0}, DataSetKey: {1}, Url: {2}", _DisplayMember, _DatasetKey, connectionUrl);


		return uniqueDatasetKey;
	}




	// ---------------------------------------------------------------------------------
	// ---------------------------------------------------------------------------------
	public DbConnectionStringBuilder ToCsb()
	{
		Tracer.Trace(GetType(), "ToCsb()");

		FbConnectionStringBuilder csb = new()
		{
			DataSource = _Server,
			Database = _Database,
			UserID = _User,
			Password = _Password,
		};

		if (_Port != CoreConstants.C_DefaultPortNumber)
			csb.Port = _Port;
		if (!string.IsNullOrWhiteSpace(_Role))
			csb.Role = _Role;
		if (!string.IsNullOrWhiteSpace(_Charset)
			&& !_Charset.Equals(ModelConstants.C_DefaultCharacterSet, StringComparison.InvariantCultureIgnoreCase))
		{
			csb.Charset = _Charset;
		}
		if (_Dialect != ModelConstants.C_DefaultDialect)
			csb.Dialect = _Dialect;
		if (_NoTriggers)
			csb.NoDatabaseTriggers = _NoTriggers;

		csb.Add("DatasetKey", DatasetKey);
		csb.Add("Dataset", Dataset);
		csb.Add("DisplayMember", DisplayMember);

		return csb;

	}


	#endregion Methods

}
