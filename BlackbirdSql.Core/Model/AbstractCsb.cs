
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using BlackbirdSql.Core.Ctl.ComponentModel;
using BlackbirdSql.Core.Properties;
using BlackbirdSql.Sys.Ctl;
using BlackbirdSql.Sys.Enums;
using BlackbirdSql.Sys.Model;
using Microsoft.VisualStudio.Data.Services;

using static BlackbirdSql.CoreConstants;
using static BlackbirdSql.SysConstants;



namespace BlackbirdSql.Core.Model;


// =========================================================================================================
//
//											AbstractCsb Class
//
/// <summary>
/// This is the base class of Csb and it's descendents.
/// Csb is a wrapper for FBConnectionStringBuilder. Csb brings some sanity to
/// DbConnectionStringBuilder naming by making property names the primary property name for index accessors,
/// TryGetValue() and Contains() as well as improved support for synonyms. Secondly, it centrally controls
/// unique connection DatasetKey naming according to connection equivalency keys. See remarks.
/// </summary>
/// <remarks>
/// A BlackbirdSql connection is uniquely identifiable on it's DatasetKey, 'Server (DatasetName)', where
/// DatasetName is the connection name in FlameRobin or the Dataset if the connection cannot be derived
/// from FlameRobin. A Dataset name is the database path's stripped file name. If the DatasetKey is not
/// unique across an ide session, duplicate DatasetNames will be numerically suffixed beginning
/// with 2, which means a connection's DatasetKey may differ from one ide session to another.
/// Connections are considered equivalent if the connection equivalency parameters match. To keep
/// things simple the describers have DataSource/Server, Database path, UserID, Role and
/// NoDatabaseTriggers set as equivalency parameters. No further distinction takes place. Property
/// equivalency keys are defined in the Describers collection.
/// </remarks>
// =========================================================================================================
public abstract class AbstractCsb : NativeDbCsbProxy
{


	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - AbstractCsb
	// ---------------------------------------------------------------------------------


	public AbstractCsb() : base()
	{
		_Id = _Seed++;
	}

	public AbstractCsb(string connectionString, bool validateServerName) : base(connectionString)
	{
		_Id = _Seed++;

		if (validateServerName)
			ValidateServerName();
	}



	public AbstractCsb(IDbConnection connection, bool validateServerName) : base(connection?.ConnectionString)
	{
		_Id = _Seed++;

		if (validateServerName)
			ValidateServerName();
	}

	public AbstractCsb(DbConnectionStringBuilder rhs, bool validateServerName) : base(rhs?.ConnectionString)
	{
		_Id = _Seed++;

		if (rhs == null)
			return;

		if (rhs is AbstractCsb csb)
		{
			_Moniker = csb._Moniker;
			_UnsafeMoniker = csb._UnsafeMoniker;
		}

		if (validateServerName)
			ValidateServerName();
	}


	protected AbstractCsb(IVsDataExplorerNode node, bool validateServerName) : base()
	{
		_Id = _Seed++;

		Extract(node);
		if (validateServerName)
			ValidateServerName();
	}


	/// <summary>
	/// .ctor for use only by AbstruseCsbAgent for registering FlameRobin datasetKeys.
	/// </summary>
	public AbstractCsb(string server, int port, string database, string user,
				string password, string charset) : base()
	{
		_Id = _Seed++;

		Initialize(server, port, C_DefaultServerType, database, user, password,
			C_DefaultRole, charset, C_DefaultDialect, C_DefaultNoDatabaseTriggers);
		ValidateServerName();
	}


	static AbstractCsb()
	{
		// Extension specific describers.

		Describers.AddRange(
		[
			new Describer(C_KeyExDatasetKey, typeof(string), C_DefaultExDatasetKey, D_Default),
			new Describer(C_KeyExConnectionKey, typeof(string), C_DefaultExConnectionKey, D_Default),
			new Describer(C_KeyExDatasetName, typeof(string), C_DefaultExDatasetName, D_Default),
			new Describer(C_KeyExDataset, typeof(string), C_DefaultExDataset, D_Default | D_Derived),

			new Describer(C_KeyExConnectionName, typeof(string), C_DefaultExConnectionName, D_Default),
			new Describer(C_KeyExConnectionSource, typeof(EnConnectionSource), C_DefaultExConnectionSource, D_Default),

			new Describer(C_KeyExClientVersion, typeof(Version), C_DefaultExClientVersion, D_Public | D_Derived),
			new Describer(C_KeyExMemoryUsage, typeof(string), C_DefaultExMemoryUsage, D_Public | D_Derived),
			new Describer(C_KeyExActiveUsers, typeof(int), C_DefaultExActiveUsers, D_Public | D_Derived),

			new Describer(C_KeyExServerVersion, typeof(Version), C_DefaultExServerVersion, D_Public | D_Derived),
			new Describer(C_KeyExPersistPassword, typeof(bool), C_DefaultExPersistPassword, D_Public | D_Derived),
			new Describer(C_KeyExEdmx, typeof(bool), D_Default),
			new Describer(C_KeyExEdmu, typeof(bool), D_Default)
		]);

		Describers.AddSynonym("datasetid", C_KeyExDatasetName);

		// Diag.DebugTrace("Added core describers");
	}


	private void Initialize(string server, int port, EnServerType serverType, string database, string user,
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
	#region Constants - AbstractCsb
	// =====================================================================================================


	#endregion Constants




	// =====================================================================================================
	#region Fields - AbstractCsb
	// =====================================================================================================


	private readonly long _Id = -1L;
	protected string _Moniker = null;
	private static long _Seed = 0L;
	protected string _UnsafeMoniker = null;


	#endregion Fields





	// =====================================================================================================
	#region Property accessors - AbstractCsb
	// =====================================================================================================


	public static new DescriberDictionary Describers => NativeDbCsbProxy.Describers;


	/// <summary>
	/// The original proposed DatasetKey supplied by a preconfigured connection string or connection dialog.
	/// If no proposed key is specified the auto-generated DatasetKey will be used on registration.
	/// </summary>
	[GlobalizedCategory("PropertyCategoryIdentifiers")]
	[GlobalizedDisplayName("PropertyDisplayConnectionName")]
	[GlobalizedDescription("PropertyDescriptionConnectionName")]
	[ReadOnly(false)]
	[DefaultValue(C_DefaultExConnectionName)]
	public string ConnectionName
	{
		get { return (string)GetValue(C_KeyExConnectionName); }
		set { SetValue(C_KeyExConnectionName, value); }
	}



	/// <summary>
	/// The server scope unique name for a connection configuration database used in DatasetKey
	/// 'Server (DatasetName)'.
	/// Clients must always register a csb using one of the register methods before attempting to
	/// access DatasetKey or DatasetName.
	/// </summary>
	[GlobalizedCategory("PropertyCategoryIdentifiers")]
	[GlobalizedDisplayName("PropertyDisplayDatasetName")]
	[GlobalizedDescription("PropertyDescriptionDatasetName")]
	[ReadOnly(false)]
	[DefaultValue(C_DefaultExDatasetName)]
	public string DatasetName
	{
		get { return (string)GetValue(C_KeyExDatasetName); }
		set { SetValue(C_KeyExDatasetName, value); }
	}


	/// <summary>
	/// The unique key for an ide session connection configuration in the form 'Server (DatasetName)'.
	/// Clients must always register a csb using one of the register methods before attempting to
	/// access DatasetKey or DatasetName.
	/// </summary>
	[GlobalizedCategory("PropertyCategoryIdentifiers")]
	[GlobalizedDisplayName("PropertyDisplayDatasetKey")]
	[GlobalizedDescription("PropertyDescriptionDatasetKey")]
	[ReadOnly(true)]
	[DefaultValue(C_DefaultExDatasetKey)]
	public string DatasetKey
	{
		get { return (string)GetValue(C_KeyExDatasetKey); }
		set { SetValue(C_KeyExDatasetKey, value); }
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
		get { return (string)GetValue(C_KeyExConnectionKey); }
		set { SetValue(C_KeyExConnectionKey, value); }
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
			try
			{
				return Cmd.GetFileNameWithoutExtension(Database);
			}
			catch (Exception ex)
			{
				Diag.Dug(ex, $"Database path: {Database}.");
				throw;
			}
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
		get { return (EnConnectionSource)GetValue(C_KeyExConnectionSource); }
		set { SetValue(C_KeyExConnectionSource, value); }
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
		get { return (Version)GetValue(C_KeyExClientVersion); }
		set { SetValue(C_KeyExClientVersion, value); }
	}


	[Browsable(false)]
	public long Id => _Id;



	/// <summary>
	/// The server memory usage.
	/// </summary>
	[Browsable(false)]
	[GlobalizedCategory("PropertyCategoryExtended")]
	[GlobalizedDisplayName("PropertyDisplayMemoryUsage")]
	[GlobalizedDescription("PropertyDescriptionMemoryUsage")]
	[ReadOnly(true)]
	[DefaultValue(C_DefaultExMemoryUsage)]
	public string MemoryUsage
	{
		get { return (string)GetValue(C_KeyExMemoryUsage); }
		set { SetValue(C_KeyExMemoryUsage, value); }
	}


	/// <summary>
	/// Server connected users.
	/// </summary>
	[Browsable(false)]
	[GlobalizedCategory("PropertyCategoryExtended")]
	[GlobalizedDisplayName("PropertyDisplayActiveUsers")]
	[GlobalizedDescription("PropertyDescriptionActiveUsers")]
	[ReadOnly(true)]
	[DefaultValue(C_DefaultExActiveUsers)]
	public int ActiveUsers
	{
		get { return (int)GetValue(C_KeyExActiveUsers); }
		set { SetValue(C_KeyExActiveUsers, value); }
	}


	/// <summary>
	/// The closest estimate to the SE display name.
	/// </summary>
	[Browsable(false)]
	public string ServerExplorerName
	{
		get
		{
			string retval = ConnectionName;
			if (string.IsNullOrWhiteSpace(retval))
				retval = DatasetKey;
			if (string.IsNullOrWhiteSpace(retval))
			{
				retval = DisplayDatasetName;

				if (!string.IsNullOrWhiteSpace(DataSource))
					retval = S_DatasetKeyFormat.FmtRes(DataSource, retval);
			}
			return retval;
		}
	}




	/// <summary>
	/// The derived unqualified DatasetName which is DatasetName else Dataset.
	/// </summary>
	[Browsable(false)]
	public string DisplayDatasetName
	{
		get
		{
			string retval = DatasetName;

			if (string.IsNullOrWhiteSpace(retval))
				retval = Dataset;

			return retval;
		}
	}


	/// <summary>
	/// The derived unqualified name which is the ConnectionName else DatasetName else Dataset.
	/// </summary>
	[Browsable(false)]
	public string DisplayName
	{
		get
		{
			string retval = ConnectionName;
			if (string.IsNullOrWhiteSpace(retval))
				retval = DisplayDatasetName;

			return retval;
		}
	}


	/// <summary>
	/// The DisplayName adorned with the ConnectionSource glyph
	/// </summary>
	[Browsable(false)]
	public string AdornedDisplayName
	{
		get
		{

			char glyph = '\0';

			if (ConnectionSource == EnConnectionSource.Session)
				glyph = RctManager.SessionDatasetGlyph;
			else if (ConnectionSource == EnConnectionSource.Application)
				glyph = RctManager.ProjectDatasetGlyph;
			else if (ConnectionSource == EnConnectionSource.EntityDataModel || ConnectionSource == EnConnectionSource.DataSource)
				glyph = RctManager.EdmDatasetGlyph;
			else if (ConnectionSource == EnConnectionSource.ExternalUtility)
				glyph = RctManager.UtilityDatasetGlyph;

			string retval = DisplayName;

			if (glyph != '\0')
				retval = Resources.RctGlyphFormat.FmtRes(glyph, retval);

			return retval;
		}
	}



	/// <summary>
	/// The DisplayName adorned with the ConnectionSource glyph
	/// for use in titles and captions
	/// </summary>
	[Browsable(false)]
	public string AdornedTitle
	{
		get
		{

			char glyph = '\0';

			if (ConnectionSource == EnConnectionSource.Session)
				glyph = RctManager.SessionTitleGlyph;
			else if (ConnectionSource == EnConnectionSource.Application)
				glyph = RctManager.ProjectTitleGlyph;
			else if (ConnectionSource == EnConnectionSource.EntityDataModel || ConnectionSource == EnConnectionSource.DataSource)
				glyph = RctManager.EdmTitleGlyph;
			else if (ConnectionSource == EnConnectionSource.ExternalUtility)
				glyph = RctManager.UtilityTitleGlyph;

			string retval = DisplayName;

			if (glyph != '\0')
				retval = Resources.RctGlyphFormat.FmtRes(glyph, retval);

			return retval;
		}
	}



	/// <summary>
	/// The DatasetKey if ConnectionName is null else the DisplayName qualified with the server name.
	/// </summary>
	[Browsable(false)]
	public string QualifiedName => string.IsNullOrEmpty(ConnectionName) ? DatasetKey : S_DatasetKeyFormat.FmtRes(DataSource, DisplayName);



	[Browsable(false)]
	public string AdornedQualifiedName
	{
		get
		{
			string retval = QualifiedName;

			char glyph = '\0';

			if (ConnectionSource == EnConnectionSource.Session)
				glyph = RctManager.SessionDatasetGlyph;
			else if (ConnectionSource == EnConnectionSource.Application)
				glyph = RctManager.ProjectDatasetGlyph;
			else if (ConnectionSource == EnConnectionSource.EntityDataModel || ConnectionSource == EnConnectionSource.DataSource)
				glyph = RctManager.EdmDatasetGlyph;
			else if (ConnectionSource == EnConnectionSource.ExternalUtility)
				glyph = RctManager.UtilityDatasetGlyph;

			if (glyph != '\0')
				retval = Resources.RctGlyphFormat.FmtRes(glyph, retval);

			return retval;
		}
	}


	[Browsable(false)]
	public string AdornedQualifiedTitle
	{
		get
		{
			string retval = QualifiedName;

			char glyph = '\0';
			string format = null;

			switch (ConnectionSource)
			{
				case EnConnectionSource.Session:
					glyph = RctManager.SessionTitleGlyph;
					format = Resources.RctGlyphFormat2.FmtRes(glyph, retval);
					break;
				case EnConnectionSource.Application:
					glyph = RctManager.ProjectTitleGlyph;
					format = Resources.RctGlyphFormat.FmtRes(glyph, retval);
					break;
				case EnConnectionSource.EntityDataModel:
				case EnConnectionSource.DataSource:
					glyph = RctManager.EdmTitleGlyph;
					format = Resources.RctGlyphFormat.FmtRes(glyph, retval);
					break;
				case EnConnectionSource.ExternalUtility:
					glyph = RctManager.UtilityTitleGlyph;
					format = Resources.RctGlyphFormat.FmtRes(glyph, retval);
					break;
				default:
					break;
			}

			if (glyph != '\0')
				retval = format.FmtRes(glyph, retval);

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
	public string LiveDatasetMoniker
	{
		get
		{
			_Moniker = BuildUniqueConnectionUrl(true);
			return _Moniker;
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
	public string Moniker => _Moniker ??= BuildUniqueConnectionUrl(true);


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
	public string UnsafeMoniker
	{
		get { return _UnsafeMoniker ??= BuildUniqueConnectionUrl(false); }
		set { _UnsafeMoniker = value; }
	}


	#endregion Property accessors





	// =====================================================================================================
	#region Overloaded Property accessors - AbstractCsb
	// =====================================================================================================


	#endregion Overloaded Property accessors





	// =====================================================================================================
	#region Methods - AbstractCsb
	// =====================================================================================================



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
		// Evs.Debug(typeof(AbstractCsb), "AreEquivalent(DbConnectionStringBuilder, DbConnectionStringBuilder)");

		return AreEquivalent(csb1, csb2, Describers.EquivalencyKeys);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Checks whether or not the emumerator property/parameter objects are equivalent.
	/// </summary>
	public static bool AreEquivalent(DbConnectionStringBuilder csb1, DbConnectionStringBuilder csb2, IEnumerable<Describer> enumerator, bool deep = false)
	{
		// Evs.Debug(typeof(AbstractCsb), "AreEquivalent(DbConnectionStringBuilder, DbConnectionStringBuilder)");

		object value1, value2;

		Csb csa1 = (Csb)csb1;
		Csb csa2 = (Csb)csb2;



		try
		{
			// Keep it simple. Loop thorugh each connection string and compare
			// If it's not in the other or null use default

			foreach (Describer describer in enumerator)
			{
				if (describer.Name == "ServerVersion")
					continue;

				value1 = csa1[describer.Name];
				value2 = csa2[describer.Name];

				if (!AreEquivalent(describer.Name, value1, value2))
				{
					// Evs.Debug(typeof(AbstractCsb), "AreEquivalent(DbConnectionStringBuilder, DbConnectionStringBuilder)",
					//	$"Connection parameter '{describer.Name}' mismatch: '{(value1?.ToString() ?? "null")}' " +
					//	$": '{(value2?.ToString() ?? "null")}.");

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

				if (describer.IsDerived || describer.IsExtended)
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

				if (describer.IsDerived || describer.IsExtended)
					continue;

				if (!csa1.ContainsKey(pair.Key))
					return false;
			}
		}

		// Evs.Debug(typeof(AbstractCsb), "AreEquivalent(DbConnectionStringBuilder, DbConnectionStringBuilder)",
		//	"Connections are equivalent");

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
		// Evs.Debug(typeof(AbstractCsb), "AreEquivalent(string, string)");

		return AreEquivalent(connectionString1, connectionString2, Describers.EquivalencyKeys);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Checks whether or not the emumerator property/parameter objects of a connection
	/// string are equivalent.
	/// </summary>
	public static bool AreEquivalent(string connectionString1, string connectionString2, IEnumerable<Describer> enumerator)
	{
		Csb csa1 = new(connectionString1, false);
		Csb csa2 = new(connectionString2, false);

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
		// Evs.Debug(typeof(AbstractCsb), "AreEquivalent(string, object, object)");

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
		AbstractCsb csa, bool safeUrl)
	{
		// We'll use UriBuilder for the url.

		if (string.IsNullOrWhiteSpace(datasource) || string.IsNullOrWhiteSpace(database)
			|| string.IsNullOrWhiteSpace(csa.UserID))
		{
			return null;
		}

		UriBuilder urlb = new()
		{
			Scheme = NativeDb.Protocol,
			Host = datasource.ToLowerInvariant(),
			UserName = csa.UserID.ToLowerInvariant(),
			Port = csa.Port,
			Password = safeUrl ? "" : csa.Password.ToLowerInvariant()
		};

		// Append the serialized database path and dot separated equivalency connection properties as the url path.

		// Serialize the db path.
		StringBuilder stringBuilder = new(Serialization.Serialize64(csa.Database.ToLowerInvariant()));

		stringBuilder.Append(SystemData.C_UnixFieldSeparator);


		// Append equivalency properties composite as defined in the Describers Colleciton.

		int i;
		bool hasEquivalencies = false;
		string key;

		for (i = 0; i < NativeDb.EquivalencyKeys.Length; i++)
		{
			key = NativeDb.EquivalencyKeys[i];

			if (key == C_KeyDataSource || key == C_KeyPort || key == C_KeyDatabase || key == C_KeyUserID)
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

			for (i = 0; i < NativeDb.EquivalencyKeys.Length; i++)
			{
				key = NativeDb.EquivalencyKeys[i];

				if (key == "DataSource" || key == "Port" || key == "Database" || key == "UserID")
					continue;

				value = csa[key];

				if (value == null)
					stringValue = "";
				else if (value.GetType() == typeof(byte[]))
					stringValue = Encoding.Default.GetString((byte[])value);
				else
					stringValue = value.ToString().ToLowerInvariant();

				sb.Append(stringValue);

				if (i < (NativeDb.EquivalencyKeys.Length - 1))
				{
					sb.Append('\n');
				}

			}

			stringBuilder.Append(Serialization.Serialize64(sb.ToString()));
		}


		stringBuilder.Append(SystemData.C_UnixFieldSeparator);

		urlb.Path = stringBuilder.ToString();

		string result = urlb.Uri.ToString();

		// Evs.Debug(typeof(AbstractCsb), "BuildUniqueConnectionUrl()", $"Url: {result}.");

		// We have a unique connection url
		return result;

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Extracts dataset information from a Server Explorer node.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void Extract(IVsDataExplorerNode node)
	{
		// Evs.Debug(GetType(), "Extract(IVsDataExplorerNode)");

		IVsDataExplorerNode connectionNode = node.ExplorerConnection.ConnectionNode;

		IVsDataObject @object = connectionNode.Object;

		if (@object == null)
		{
			ConnectionString = node.ExplorerConnection.DecryptedConnectionString();
			return;
		}


		// Evs.Debug(GetType(), "Extract(IVsDataExplorerNode)", $"Node type is {@object}.");

		object value;
		object readOnly;
		PropertyDescriptorCollection descriptors = TypeDescriptor.GetProperties(this);



		foreach (KeyValuePair<string, Describer> pair in Csb.Describers)
		{
			try
			{
				if (!@object.Properties.ContainsKey(pair.Key))
					continue;

				value = @object.Properties[pair.Key];

				if (Cmd.IsNullValue(value))
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

		// Evs.Debug(GetType(), "Extract()", $"node.ExplorerConnection.DisplayName: {node.ExplorerConnection.DisplayName}, " +
		//	$"node.ExplorerConnection.ConnectionNode.Name: {node.ExplorerConnection.ConnectionNode.Name}.");

		ConnectionKey = connectionNode.GetConnectionKey();
		if (ConnectionKey == null)
		{
			COMException ex = new($"ConnectionKey for explorer connection {node.ExplorerConnection.SafeName()} for node {node.Name} is null");
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
	/// <summary>
	/// Ensures a proposed name (ConnectionName or DatasetName), exists.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public void ValidateProposedName()
	{
		if (!string.IsNullOrWhiteSpace(ConnectionName) || !string.IsNullOrWhiteSpace(DatasetName)
			|| string.IsNullOrWhiteSpace(DisplayDatasetName))
		{
			return;
		}

		DatasetName = DisplayDatasetName;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Validates the DataSource for case name mangling.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public void ValidateServerName()
	{
		string dataSource, server;

		if (ContainsKey(C_KeyDataSource) && !string.IsNullOrWhiteSpace(dataSource = DataSource)
			&& !RctManager.ShutdownState && !dataSource.Equals(server = RctManager.RegisterServer(dataSource, Port)))
		{
			DataSource = server;
		}
	}



	/// <summary>
	/// Updates descriptor ReadonlyAttributes.
	/// </summary>
	public static bool UpdateDescriptorReadOnlyAttribute(ref PropertyDescriptorCollection descriptors, string[] readOnlyDescriptors, bool readOnly)
	{
		if (descriptors == null || descriptors.Count == 0)
			return false;

		bool value;
		bool updated = false;

		try
		{
			foreach (string name in readOnlyDescriptors)
			{
				PropertyDescriptor descriptor = descriptors.Find(name, true);

				if (descriptor == null)
					continue;

				if (descriptor.Attributes[typeof(ReadOnlyAttribute)] is not ReadOnlyAttribute attr)
					throw new IndexOutOfRangeException($"ReadOnlyAttribute not found in PropertyDescriptor for {name}.");

				FieldInfo fieldInfo = Reflect.GetFieldInfo(attr, "isReadOnly");

				value = readOnly;

				if ((bool)Reflect.GetFieldInfoValue(attr, fieldInfo) != value)
				{
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
