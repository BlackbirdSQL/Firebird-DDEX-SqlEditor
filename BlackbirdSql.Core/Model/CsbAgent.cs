
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
			Tracer.Trace(GetType(), "ConvertTo()", "context: {0}, value type: {1}, dest type: {2}", context, value.GetType(), destinationType);

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


	[Browsable(false)]
	public static IDictionary<string, string> RegisteredConnectionMonikers => _SConnectionMonikers ?? LoadConfiguredConnectionMonikers();

	public static new DescriberDictionary Describers => AbstractCsbAgent.Describers;
	public static IDictionary<string, Describer> Synonyms => Describers.Synonyms;

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

	/// <summary>
	/// .ctor for use only by ConnectionLocator for registering solution datasetKeys.
	/// </summary>
	protected CsbAgent(string datasetId, string connectionString) : base(datasetId, connectionString)
	{
	}


	public CsbAgent(IDbConnection connection) : base(connection)
	{
	}


	public CsbAgent(IBPropertyAgent ci) : base(ci)
	{
	}


	public CsbAgent(IVsDataExplorerNode node) : base(node)
	{
	}


	/// <summary>
	/// .ctor for use only by ConnectionLocator for registering FlameRobin datasetKeys.
	/// </summary>
	protected CsbAgent(string datasetId, string server, int port, string database,
		string user, string password, string charset)
		: base(datasetId, server, port, database, user, password, charset)
	{
	}


	/// <summary>
	/// Reserved for the registration of FlameRobin datasetKeys.
	/// </summary>
	public static CsbAgent CreateRegisteredDataset(string datasetId, string server, int port, string database, string user,
		string password, string charset)
	{
		// Tracer.Trace(typeof(CsbAgent), "RegisterDatasetKey(datasetId, server, port, database, user, password, charset)", "datasetId: {0}, server: {1}, port: {2}, database: {3}, user: {4}, password: {5}, charset: {6}", datasetId, server, port, database, user, password, charset);

		CsbAgent csa = new(datasetId, server, port, database, user, password, charset);
		(string datasetKey, _) = csa.RegisterUniqueConnectionDatasetKey(true);

		if (string.IsNullOrWhiteSpace(datasetKey))
			return null;

		return csa;
	}


	/// <summary>
	/// Reserved for the registration of solution datasetKeys.
	/// </summary>
	public static CsbAgent CreateRegisteredDataset(string datasetId, string connectionString)
	{
		// Tracer.Trace(typeof(CsbAgent), "RegisterDatasetKey(datasetId, connectionString)");

		CsbAgent csa = new(datasetId, connectionString);
		(string datasetKey, _) = csa.RegisterUniqueConnectionDatasetKey(true);

		if (string.IsNullOrWhiteSpace(datasetKey))
			return null;


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
