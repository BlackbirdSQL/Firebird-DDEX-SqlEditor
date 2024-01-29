
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using BlackbirdSql.Core.Ctl;
using BlackbirdSql.Core.Ctl.Config;
using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.Core.Model.Enums;
using FirebirdSql.Data.FirebirdClient;
using Microsoft.VisualStudio.Data.Services;

using static BlackbirdSql.Core.Ctl.CoreConstants;


namespace BlackbirdSql.Core.Model;

// =========================================================================================================
//
//											CsbAgent Class
//
/// <summary>
/// CsbAgent is the overlord superclass centrally controlling unique connections according to connection
/// equivalency keys.
/// It works in parallel with <see cref="IVsDataExplorerConnectionManager"/> and is a wrapper for
/// <see cref="FbConnectionStringBuilder"/>.
/// The class hierarachy is CsbAgent > <see cref="AbstractCsbAgent"/> > <see cref="AbstruseCsbAgent"/> >
/// FbConnectionStringBuilder.
/// CsbAgent brings some sanity to DBConnectionStringBuilder naming by making property names the primary
/// property name for index accessors, TryGetValue() and Contains() as well as improved support for
/// synonyms using the <see cref="DescriberDictionary"/> <see cref="Describers"/>. See remarks.
/// </summary>
/// <remarks>
/// A BlackbirdSql connection is uniquely identifiable on it's generated
/// <see cref="AbstractCsbAgent.DatasetKey"/> 'Server (DatasetId)', and also it's proposed name,
/// <see cref="AbstractCsbAgent.ConnectionName"/>, if that was provided.
/// A <see cref="AbstractCsbAgent.DatasetId"/> is the proposed name of the database which defaults to
/// <see cref="AbstractCsbAgent.Dataset"/>.
/// If the DatasetKey is not unique across an ide session, then either ConnectionName (if specified) or
/// DatasetId will be numerically suffixed beginning with '_2'. Connections are considered equivalent
/// based on the configured <see cref="PersistentSettings.EquivalencyKeys"/>.
/// Whenever a new instance of an existing unique connection is requested, the stored connection's
/// redundant properties will be updated if connection properties or a connection string are provided.
/// </remarks>
// =========================================================================================================
public class CsbAgent : AbstractCsbAgent, ICloneable
{


	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - CsbAgent
	// ---------------------------------------------------------------------------------


	public CsbAgent() : base()
	{
		_Id = RctManager.Seed;
	}


	public CsbAgent(string connectionString) : base(connectionString)
	{
		_Id = RctManager.Seed;
	}


	public CsbAgent(IDbConnection connection) : base(connection)
	{
		_Id = RctManager.Seed;
	}


	public CsbAgent(IBPropertyAgent ci) : base(ci)
	{
		_Id = RctManager.Seed;
	}


	public CsbAgent(IVsDataExplorerNode node) : base(node)
	{
		_Id = RctManager.Seed;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// .ctor for use only by AbstruseCsbAgent for registering FlameRobin datasetKeys.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public CsbAgent(string server, int port, string database,
			string user, string password, string charset)
		: base(server, port, database, user, password, charset)
	{
	}


	public object Clone()
	{
		CsbAgent clone = [];

		foreach (Describer describer in DescriberKeys)
		{
			if (ContainsKey(describer.Name))
			{
				clone[describer.Name] = this[describer.Name];
			}

			clone._EquivalencyConnectionString = _EquivalencyConnectionString;
			clone._EquivalencyMoniker = _EquivalencyMoniker;
			clone._Id = _Id;
		}

		return clone;
	}

	#endregion Constructors / Destructors




	// =========================================================================================================
	#region Fields - CsbAgent
	// =========================================================================================================


	private string _EquivalencyConnectionString = null;
	private string _EquivalencyMoniker = null;
	private long _Id = -1;


	#endregion Fields




	// =========================================================================================================
	#region Property accessors - CsbAgent
	// =========================================================================================================




	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Describer collection of uniform properties used by the FbConnectionStringBuilder
	/// wrapper, CsbAgent, as well as PropertyAgent and it's descendent SqlEditor
	/// ConnectionInfo and Dispatcher connection classes, and also the SE root nodes,
	/// Root and Database.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static new DescriberDictionary Describers => AbstractCsbAgent.Describers;

	public static IEnumerable<Describer> AdvancedKeys => Describers.AdvancedKeys;
	public static IEnumerable<Describer> ConnectionKeys => Describers.ConnectionKeys;
	public static IEnumerable<Describer> DescriberKeys => Describers.DescriberKeys;
	public static IEnumerable<Describer> EquivalencyKeys => Describers.EquivalencyKeys;
	public static IEnumerable<Describer> MandatoryKeys => Describers.MandatoryKeys;
	public static IEnumerable<Describer> PublicMandatoryKeys => Describers.PublicMandatoryKeys;
	public static IEnumerable<Describer> WeakEquivalencyKeys => Describers.WeakEquivalencyKeys;

	[Browsable(false)]
	public long Id => _Id;

	#endregion Property accessors




	// =========================================================================================================
	#region Methods - CsbAgent
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Applies required or removes redundant ConnectionName and DatasetId properties after
	/// a validation call to <see cref="ValidateKeys"/>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public void ApplyValidKeys(string connectionName, string datasetId, bool clearInvalidDatasetKey)
	{
		if (connectionName == string.Empty)
			Remove(C_KeyExConnectionName);
		else if (connectionName != null)
			ConnectionName = connectionName;

		if (datasetId == string.Empty)
			Remove(C_KeyExDatasetId);
		else if (datasetId != null)
			DatasetId = datasetId;

		if (!clearInvalidDatasetKey || !ContainsKey(C_KeyExDatasetKey))
			return;

		// At this point a DatasetKey exists and must be removed if it does not match a proposed
		// key (clearInvalidDatasetKey == true).

		string datasetKey = DatasetKey;
		connectionName = ConnectionName;
		datasetId = DatasetId;

		if (!string.IsNullOrWhiteSpace(connectionName))
		{
			if (!connectionName.Equals(datasetKey))
				Remove(C_KeyExDatasetKey);
		}
		else if (!string.IsNullOrWhiteSpace(datasetId))
		{
			string derivedConnectionName = SystemData.DatasetKeyFmt.FmtRes(DataSource, datasetId);

			if (!derivedConnectionName.Equals(datasetKey))
				Remove(C_KeyExDatasetKey);
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Creates a uniquely identifiable connection url. Connection urls are used for
	/// uniquely naming connections and are unique to equivalent connections according
	/// to describer equivalency.
	/// </summary>
	/// <returns>
	/// The unique connection url in format:
	/// fbsql://user_uc@server:port/Serilize64(databasepath_lc)/[Serilize64(newline_delimited_equivalencykeys)]/
	/// </returns>
	// ---------------------------------------------------------------------------------
	public static string CreateConnectionUrl(IDbConnection connection)
	{
		return (new CsbAgent(connection.ConnectionString)).SafeDatasetMoniker;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Creates a uniquely identifiable connection url. Connection urls are used for
	/// uniquely naming connections and are unique to equivalent connections according
	/// to describer equivalency.
	/// </summary>
	/// <returns>
	/// The unique connection url in format:
	/// fbsql://user_uc@server:port/Serilize64(databasepath_lc)/[Serilize64(newline_delimited_equivalencykeys)]/
	/// </returns>
	// ---------------------------------------------------------------------------------
	public static string CreateConnectionUrl(string connectionString)
	{
		if (string.IsNullOrWhiteSpace(connectionString))
			return connectionString;

		return (new CsbAgent(connectionString)).SafeDatasetMoniker;
	}


	public override int GetHashCode()
	{
		return base.GetHashCode();
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs a stored equivalency and validity check against connections. Many
	/// commands as well as PropertyWindows use CsbAgent to do status validations and
	/// updates on pulsed events.
	/// This can result in a very high volume of calls, so the agent stores the
	/// <see cref="RctManager.Seed"/>, database moniker and connection string by a call
	/// to <see cref="RegisterValidationState"/>, for low overhead responses when the
	/// connection has not changed.
	/// </summary>
	/// <param name="obj"></param>
	/// <returns></returns>
	// ---------------------------------------------------------------------------------
	public bool Invalidated(IDbConnection connection)
	{
		if (connection == null) return true;

		if (_EquivalencyConnectionString == null)
		{
			NullReferenceException ex = new($"CsbAgent validation requested before the validity state was registered with RegisterValidityState().");
			Diag.Dug(ex);
			throw ex;
		}

		// Before constructing anything this equivalency checks the RctManager seed and compares
		// the connection strings.
		// This will be fast low-overhead call and suitable for operations like QueryStatus()
		// event handling.
		return _Id != RctManager.Seed || _EquivalencyConnectionString != connection.ConnectionString
			|| !SafeDatasetMoniker.Equals(_EquivalencyMoniker, StringComparison.InvariantCulture);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Parses a ConnectionInfo object.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected override void Parse(IBPropertyAgent ci)
	{
		// Tracer.Trace(GetType(), "Parse(IBPropertyAgent)");

		DatasetKey = (string)ci[CoreConstants.C_KeyExDatasetKey];
		ConnectionKey = (string)ci[CoreConstants.C_KeyExConnectionKey];
		ConnectionSource = (EnConnectionSource)ci[CoreConstants.C_KeyExConnectionSource];
		ConnectionName = (string)ci[CoreConstants.C_KeyExConnectionName];
		DatasetId = (string)ci[CoreConstants.C_KeyExDatasetId];

		DataSource = string.IsNullOrEmpty((string)ci[CoreConstants.C_KeyDataSource]) ? "localhost" : (string)ci[CoreConstants.C_KeyDataSource];
		Port = Convert.ToInt32(ci[CoreConstants.C_KeyPort]);
		ServerType = (FbServerType)ci[CoreConstants.C_KeyServerType];
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






	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Registers the state of the <see cref="RunningConnectionTable"/> and connection
	/// data for future validity checks by <see cref="Invalidated(IDbConnection)"/>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public void RegisterValidationState(string dbConnectionString)
	{
		string invalidArgument = string.IsNullOrWhiteSpace(dbConnectionString) ? nameof(dbConnectionString) : null;

		if (invalidArgument == null && string.IsNullOrWhiteSpace(DataSource))
			invalidArgument = "DataSource";

		if (invalidArgument == null && string.IsNullOrWhiteSpace(Database))
			invalidArgument = "Database";

		if (invalidArgument == null && string.IsNullOrWhiteSpace(UserID))
			invalidArgument = "UserID";

		if (invalidArgument != null)
		{
			ArgumentNullException ex = new($"{invalidArgument} is null or empty.");
			Diag.Dug(ex);
			throw ex;
		}

		UpdateValidationRctState();

		_EquivalencyMoniker = BuildUniqueConnectionUrl(DataSource, Database, this, true);
		_EquivalencyConnectionString = dbConnectionString;
	}




	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Updates the csa with the state of the <see cref="RunningConnectionTable"/> for
	/// future validity checks by <see cref="Invalidated(IDbConnection)"/>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public void UpdateValidationRctState()
	{
		_Id = RctManager.Seed;
	}




	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Validates the csa for redundant or required registration properties.
	/// Determines if the ConnectionName (proposed DatsetKey) and DatasetId (proposed
	/// database name) are required in the csa.
	/// This cleanup ensures that proposed keys do not appear in the connection string
	/// if they will have no impact on the final DatsetKey. 
	/// </summary>
	/// <param name="applyValidation">
	/// If true will call <see cref="ApplyValidKeys"/>
	/// after validation
	/// </param>
	/// <param name="clearInvalidDatasetKey">
	/// If true will clear the DatasetKey if it does not match the proposed ConnectionName
	/// or datasetId. Only applies if 'applyValidation' is set to true.
	/// </param>
	/// <returns>
	/// Returns a tuple (ConnectionName, DatsetId). The contents of each reflects the
	/// result:
	/// 1. If a property is to be deleted, string.Empty is returned indicating
	/// it exists but is not required.
	/// 2. If a property was already correctly deleted or is valid, no action is
	/// required and null is returned.
	/// 3. If the property has a new value, the new value is returned.
	/// </returns>
	// ---------------------------------------------------------------------------------
	public (string, string) ValidateKeys(bool applyValidation, bool clearInvalidDatasetKey)
	{
		// First the DatasetId. If it's equal to the Dataset we clear it because, by
		// default the trimmed filepath (Dataset) can be used.

		string dataset = Dataset;
		string datasetId = ContainsKey(C_KeyExDatasetId) ? DatasetId : null;

		if (datasetId != null)
		{
			if (string.IsNullOrWhiteSpace(datasetId))
			{
				// DatasetId exists and is invalid (empty). Delete it.
				datasetId = string.Empty;
			}
			else
			{
				// If the DatasetId is equal to the Dataset it's not needed. Delete it.
				if (datasetId == dataset)
					datasetId = string.Empty;
			}
		}

		// Now that the datasetId is established, we can determined its default derived value
		// and the default derived value of the datasetKey.
		string derivedDatasetId = string.IsNullOrEmpty(datasetId) ? dataset : datasetId;
		string derivedConnectionName = SystemData.DatasetKeyFmt.FmtRes(DataSource, derivedDatasetId);
		string derivedAlternateConnectionName = SystemData.DatasetKeyAlternateFmt.FmtRes(DataSource, derivedDatasetId);


		// Now the proposed DatasetKey, ConnectionName. If it exists and is equal to the derived
		// Datsetkey, it's also not needed.

		string connectionName = ContainsKey(C_KeyExConnectionName) ? ConnectionName : null;

		if (connectionName != null)
		{
			if (string.IsNullOrWhiteSpace(connectionName))
			{
				// ConnectionName exists and is invalid (empty). Delete it.
				connectionName = string.Empty;
			}
			else
			{
				// If the ConnectionName (proposed DatsetKey) is equal to the default
				// derived datasetkey it also won't be needed, so delete it,
				// else the ConnectionName still exists and is the determinant, so
				// any existing proposed DatasetId is not required.
				if (connectionName == derivedConnectionName || connectionName == derivedAlternateConnectionName)
					connectionName = string.Empty;
				else if (!string.IsNullOrWhiteSpace(datasetId))
					datasetId = string.Empty;
			}
		}

		// Finally determine if the final ConnectionName and DatsetId are equal to
		// the values in the csa if the final keys exist.
		if (!string.IsNullOrEmpty(connectionName) && ContainsKey(C_KeyExConnectionName)
			&& connectionName == ConnectionName)
		{
			connectionName = null;
		}

		if (!string.IsNullOrEmpty(datasetId) && ContainsKey(C_KeyExDatasetId)
			&& datasetId == DatasetId)
		{
			datasetId = null;
		}


		// Simultaneously update the csa if requested.
		if (applyValidation)
			ApplyValidKeys(connectionName, datasetId, clearInvalidDatasetKey);

		return (connectionName, datasetId);

	}


	#endregion Methods




	// =========================================================================================================
	#region Sub-Classes - CsbAgent
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Provides a type converter to convert collection objects in the advanced
	/// properties PropertyGrid.
	/// </summary>
	// ---------------------------------------------------------------------------------
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


	#endregion Sub-Classes

}
