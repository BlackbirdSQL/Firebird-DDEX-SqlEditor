
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Globalization;
using BlackbirdSql.Core.Ctl;
using BlackbirdSql.Core.Ctl.ComponentModel;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Ctl.Interfaces;

using FirebirdSql.Data.FirebirdClient;

using Microsoft.VisualStudio.Data.Services;


namespace BlackbirdSql.Core.Model;

// =========================================================================================================
//
//											CsbAgent Class
//
/// <summary>
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
/// things simple the PropertySet describers have DataSource/Server, Database path, UserID, Role and
/// NoDatabaseTriggers set as equivalency parameters. No further distinction takes place. Property
/// equivalency keys are defined in the CorePropertySet and ModelPropertySet describer collections.
/// </remarks>
// =========================================================================================================
public class CsbAgent : AbstractCsbAgent
{

	/// <summary>Provides a type converter to convert collection objects in the advanced properties PropertyGrid.</summary>
	public class CsbConverter : CollectionConverter
	{
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			// Tracer.Trace(GetType(), "ConvertTo()", "context: {0}, value type: {1}, dest type: {2}", context, value.GetType(), destinationType);

			return base.ConvertTo(context, culture, value, destinationType);
		}

		public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
		{
			return TypeDescriptor.GetProperties(typeof(CsbAgent), attributes);
		}

		public override bool GetPropertiesSupported(ITypeDescriptorContext context)
		{
			return true;
		}

		public CsbConverter()
		{
		}
	}



	// ---------------------------------------------------------------------------------
	#region Constants - CsbAgent
	// ---------------------------------------------------------------------------------

	public const string C_SqlExtension = ".fbsql";

	protected const string C_ServiceFolder = "ServerxExplorer";
	protected const string C_TempSqlFolder = "SqlTemporaryFiles";


	#endregion Constants




	// =========================================================================================================
	#region Variables - CsbAgent
	// =========================================================================================================


	#endregion Variables




	// =========================================================================================================
	#region Property accessors - CsbAgent
	// =========================================================================================================


	[Browsable(false)]
	public static IDictionary<string, string> RegisteredDatasets => _SDatasetKeys ?? LoadConfiguredConnections();

	public static new DescriberDictionary Describers => AbstractCsbAgent.Describers;

	public static int Id => _SConnectionMonikers.Count;


	#endregion Property accessors




	// =========================================================================================================
	#region Constructors / Destructors - CsbAgent
	// =========================================================================================================


	public CsbAgent() : base()
	{
	}


	public CsbAgent(string connectionString) : base(connectionString)
	{
	}


	public CsbAgent(IDbConnection connection) : base(connection)
	{
	}


	private CsbAgent(IBPropertyAgent ci) : base(ci)
	{
	}


	public CsbAgent(IVsDataExplorerNode node) : base(node)
	{
	}


	/// <summary>
	/// .ctor for use only by ConnectionLocator for registering FlameRobin datasetKeys.
	/// </summary>
	private CsbAgent(string server, int port, string database,
		string user, string password, string charset)
		: base(server, port, database, user, password, charset)
	{
	}


	/// <summary>
	/// Creates an instance from a registered connection using a ConnectionInfo object.
	/// </summary>
	public static CsbAgent CreateInstance(IBPropertyAgent connectioInfo)
	{
		if (connectioInfo == null)
		{
			ArgumentNullException ex = new(nameof(connectioInfo));
			Diag.Dug(ex);
			throw ex;
		}

		LoadConfiguredConnections();

		CsbAgent csa = new(connectioInfo);
		string connectionUrl = csa.SafeDatasetMoniker;

		if (!_SConnectionMonikers.TryGetValue(connectionUrl, out (string, string, string, string) datasetInfo))
		{
			KeyNotFoundException ex = new KeyNotFoundException($"Connection url not found in SConnectionMonikers for key: {connectionUrl}");
			Diag.Dug(ex);
			throw ex;
		}


		return new CsbAgent(datasetInfo.Item4);
	}


	/// <summary>
	/// Creates an instance from a registered connection using an IDbConnection.
	/// </summary>
	public static CsbAgent CreateInstance(IDbConnection connection)
	{
		if (connection == null)
		{
			ArgumentNullException ex = new(nameof(connection));
			Diag.Dug(ex);
			throw ex;
		}

		LoadConfiguredConnections();

		CsbAgent csa = new(connection);
		string connectionUrl = csa.SafeDatasetMoniker;

		if (!_SConnectionMonikers.TryGetValue(connectionUrl, out (string, string, string, string) datasetInfo))
		{
			KeyNotFoundException ex = new KeyNotFoundException($"Connection url not found in SConnectionMonikers for key: {connectionUrl}");
			Diag.Dug(ex);
			throw ex;
		}


		return new CsbAgent(datasetInfo.Item4);
	}



	/// <summary>
	/// Creates an instance from a registered connection using a Server Explorer
	/// node.
	/// </summary>
	public static CsbAgent CreateInstance(IVsDataExplorerNode node)
	{
		if (node == null)
		{
			ArgumentNullException ex = new(nameof(node));
			Diag.Dug(ex);
			throw ex;
		}

		LoadConfiguredConnections();

		CsbAgent csa = new(node);
		string connectionUrl = csa.SafeDatasetMoniker;

		if (!_SConnectionMonikers.TryGetValue(connectionUrl, out (string, string, string, string) datasetInfo))
		{
			KeyNotFoundException ex = new KeyNotFoundException($"Connection url not found in SConnectionMonikers for key: {connectionUrl}");
			Diag.Dug(ex);
			throw ex;
		}


		return new CsbAgent(datasetInfo.Item4);
	}


	/// <summary>
	/// Creates an instance from a registered connection using either a DatasetKey or
	/// ConnectionString.
	/// </summary>
	/// <param name="connectionKey">
	/// The DatasetKey or ConnectionString.
	/// </param>
	public static CsbAgent CreateInstance(string connectionKey)
	{
		if (connectionKey == null)
		{
			ArgumentNullException ex = new(nameof(connectionKey));
			Diag.Dug(ex);
			throw ex;
		}

		LoadConfiguredConnections();

		(string, string, string, string) datasetInfo;

		if (connectionKey.StartsWith("data source=", StringComparison.InvariantCultureIgnoreCase) || connectionKey.Contains(";data source="))
		{
			CsbAgent csa = new(connectionKey);
			string connectionUrl = csa.SafeDatasetMoniker;

			if (!_SConnectionMonikers.TryGetValue(connectionUrl, out datasetInfo))
				return null;
		}
		else
		{

			datasetInfo = GetDatasetConnectionInfo(connectionKey, false);

			if (datasetInfo.Item1 == null)
				return null;
		}

		return new CsbAgent(datasetInfo.Item4);
	}


	/// <summary>
	/// Reserved for the registration of FlameRobin datasetKeys.
	/// </summary>
	public static CsbAgent CreateRegisteredDataset(string proposedDatasetId, string server, int port, string database, string user,
		string password, string charset)
	{
		// Tracer.Trace(typeof(CsbAgent), "RegisterDatasetKey(datasetId, server, port, database, user, password, charset)", "datasetId: {0}, server: {1}, port: {2}, database: {3}, user: {4}, password: {5}, charset: {6}", datasetId, server, port, database, user, password, charset);

		CsbAgent csa = new(server, port, database, user, password, charset);
		string datasetKey = csa.RegisterUniqueConnectionDatasetKey(null, proposedDatasetId);

		if (string.IsNullOrWhiteSpace(datasetKey))
			return null;

		return csa;
	}


	/// <summary>
	/// Reserved for the registration of server explorer connection nodes and solution
	/// project preconfigured connections.
	/// </summary>
	public static CsbAgent CreateRegisteredDataset(string proposedDatasetKey, string proposedDatasetId, CsbAgent csa)
	{
		// Tracer.Trace(typeof(CsbAgent), "RegisterDatasetKey(datasetId, connectionString)");

		string datasetKey = csa.RegisterUniqueConnectionDatasetKey(proposedDatasetKey, proposedDatasetId);

		if (string.IsNullOrWhiteSpace(datasetKey))
			return null;


		return csa;
	}



	/// <summary>
	/// Creates an instance from a registered connection using a ConnectionString else
	/// registers a new connection if none exists.
	/// </summary>
	public static CsbAgent EnsureInstance(IVsDataExplorerNode node)
	{
		if (node == null)
		{
			ArgumentNullException ex = new(nameof(node));
			Diag.Dug(ex);
			throw ex;
		}

		LoadConfiguredConnections();

		CsbAgent csa = CreateInstance(node);

		if (csa != null)
			return csa;

		csa = new(node);

		lock (_LockClass)
			csa.DataSource = ConnectionLocator.RegisterServerName(csa.DataSource);

		string datasetId = csa.DatasetId;
		if (string.IsNullOrWhiteSpace(datasetId))
			datasetId = csa.Dataset;

		csa = CsbAgent.CreateRegisteredDataset(node.ExplorerConnection.DisplayName, datasetId, csa);

		lock (_LockClass)
			ConnectionLocator.AddRegisteredDataset(csa);

		return csa;
	}


	/// <summary>
	/// Creates an instance from a registered connection using a ConnectionString else
	/// registers a new connection if none exists.
	/// </summary>
	public static CsbAgent EnsureInstance(string connectionString)
	{
		if (connectionString == null)
		{
			ArgumentNullException ex = new(nameof(connectionString));
			Diag.Dug(ex);
			throw ex;
		}

		LoadConfiguredConnections();

		CsbAgent csa = CreateInstance(connectionString);

		if (csa != null)
			return csa;

		csa = new(connectionString);

		lock (_LockClass)
			csa.DataSource = ConnectionLocator.RegisterServerName(csa.DataSource);

		string datasetId = csa.DatasetId;
		if (string.IsNullOrWhiteSpace(datasetId))
			datasetId = csa.Dataset;

		csa = CsbAgent.CreateRegisteredDataset(csa.ExternalKey, datasetId, csa);

		lock (_LockClass)
			ConnectionLocator.AddRegisteredDataset(csa);

		return csa;
	}

	#endregion Property Constructors / Destructors




	// =========================================================================================================
	#region Methods - CsbAgent
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Checks whether or not connection property/parameter objects are equivalent
	/// </summary>
	/// <remarks>
	/// We consider connections equivalent if they will produce the same results. The connection properties
	/// that determine this equivalency are defined in <see cref="CoreProperties.EquivalencyKeys"/>.
	/// </remarks>
	public static new bool AreEquivalent(DbConnectionStringBuilder csb1, DbConnectionStringBuilder csb2)
	{
		return AbstractCsbAgent.AreEquivalent(csb1, csb2);
	}


	// ---------------------------------------------------------------------------------
	// ---------------------------------------------------------------------------------
	protected override void Parse(IBPropertyAgent ci)
	{
		// Tracer.Trace(GetType(), "Parse(IBPropertyAgent)");

		DatasetKey = (string)ci[CoreConstants.C_KeyExDatasetKey];
		ExternalKey = (string)ci[CoreConstants.C_KeyExExternalKey];
		DatasetId = (string)ci[CoreConstants.C_KeyExDatasetId];

		DataSource = string.IsNullOrEmpty((string)ci[CoreConstants.C_KeyDataSource]) ? "localhost" : (string)ci[CoreConstants.C_KeyDataSource];
		Port = Convert.ToInt32(ci[CoreConstants.C_KeyPort]);
		ServerType = (FbServerType)Convert.ToInt32(ci[CoreConstants.C_KeyServerType]);
		Database = (string)ci[CoreConstants.C_KeyDatabase];
		UserID = (string)(ci[CoreConstants.C_KeyUserID] ?? string.Empty);
		Password = (string)(ci[CoreConstants.C_KeyPassword] ?? string.Empty);

		Role = (string)ci[ModelConstants.C_KeyRole];
		Charset = (string)ci[ModelConstants.C_KeyCharset];
		Dialect = Convert.ToInt32(ci[ModelConstants.C_KeyDialect]);
		NoDatabaseTriggers = (bool)ci[ModelConstants.C_KeyNoDatabaseTriggers];

		PacketSize = Convert.ToInt32(ci[ModelConstants.C_KeyPacketSize]);
		ConnectionTimeout = Convert.ToInt32(ci[ModelConstants.C_KeyConnectionTimeout]);
		Pooling = (bool)ci[ModelConstants.C_KeyPooling];
		ConnectionLifeTime = Convert.ToInt32(ci[ModelConstants.C_KeyConnectionLifeTime]);
		MinPoolSize = Convert.ToInt32(ci[ModelConstants.C_KeyMinPoolSize]);
		MaxPoolSize = Convert.ToInt32(ci[ModelConstants.C_KeyMaxPoolSize]);
		FetchSize = Convert.ToInt32(ci[ModelConstants.C_KeyFetchSize]);
		IsolationLevel = (IsolationLevel)Convert.ToInt32(ci[ModelConstants.C_KeyIsolationLevel]);
		ReturnRecordsAffected = (bool)ci[ModelConstants.C_KeyReturnRecordsAffected];
		Enlist = (bool)ci[ModelConstants.C_KeyEnlist];
		ClientLibrary = (string)ci[ModelConstants.C_KeyClientLibrary];
		DbCachePages = Convert.ToInt32(ci[ModelConstants.C_KeyDbCachePages]);
		NoGarbageCollect = (bool)ci[ModelConstants.C_KeyNoGarbageCollect];
		Compression = (bool)ci[ModelConstants.C_KeyCompression];
		CryptKey = (byte[])ci[ModelConstants.C_KeyCryptKey];
		WireCrypt = (FbWireCrypt)Convert.ToInt32(ci[ModelConstants.C_KeyWireCrypt]);
		ApplicationName = (string)ci[ModelConstants.C_KeyApplicationName];
		CommandTimeout = Convert.ToInt32(ci[ModelConstants.C_KeyCommandTimeout]);
		ParallelWorkers = Convert.ToInt32(ci[ModelConstants.C_KeyParallelWorkers]);
	}

	#endregion Methods

}
