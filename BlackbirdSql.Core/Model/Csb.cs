
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using BlackbirdSql.Core.Ctl.Config;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Sys;
using BlackbirdSql.Sys.Ctl;
using BlackbirdSql.Sys.Enums;
using Microsoft.VisualStudio.Data.Services;

using static BlackbirdSql.Sys.SysConstants;



namespace BlackbirdSql.Core.Model;


// =========================================================================================================
//
//											Csb Class
//
/// <summary>
/// Csb is the overlord superclass centrally controlling unique connections according to connection
/// equivalency keys.
/// It works in parallel with <see cref="IVsDataExplorerConnectionManager"/> and is a wrapper for
/// <see cref="FbConnectionStringBuilder"/>.
/// The class hierarachy is Csb > <see cref="AbstractCsb"/> > <see cref="AbstruseCsbAgent"/> >
/// FbConnectionStringBuilder.
/// Csb brings some sanity to DbConnectionStringBuilder naming by making property names the primary
/// property name for index accessors, TryGetValue() and Contains() as well as improved support for
/// synonyms using the <see cref="DescriberDictionary"/> <see cref="Describers"/>. See remarks.
/// </summary>
/// <remarks>
/// A BlackbirdSql connection is uniquely identifiable on it's generated
/// <see cref="AbstractCsb.DatasetKey"/> 'Server (DatasetId)', and also it's proposed name,
/// <see cref="AbstractCsb.ConnectionName"/>, if that was provided.
/// A <see cref="AbstractCsb.DatasetId"/> is the proposed name of the database which defaults to
/// <see cref="AbstractCsb.Dataset"/>.
/// If the DatasetKey is not unique across an ide session, then either ConnectionName (if specified) or
/// DatasetId will be numerically suffixed beginning with '_2'. Connections are considered equivalent
/// based on the configured <see cref="PersistentSettings.EquivalencyKeys"/>.
/// Whenever a new instance of an existing unique connection is requested, the stored connection's
/// redundant properties will be updated if connection properties or a connection string are provided.
/// </remarks>
// =========================================================================================================
public class Csb : AbstractCsb, ICloneable
{


	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - Csb
	// ---------------------------------------------------------------------------------


	public Csb() : base()
	{
		_Stamp = RctManager.Stamp;
	}


	/// <summary>
	/// .ctor from ConnectionString.
	/// </summary>
	/// <param name="connectionString">The db connection string.</param>
	/// <param name="validateServerName">
	/// If true will ensure against Case name mangling of the DataSource property.
	/// Typically Csa's only required for equivalency checks or ConnectionUrl monikers
	/// should set this argument to false.
	/// </param>
	public Csb(string connectionString, bool validateServerName = true) : base(connectionString, validateServerName)
	{
		_Stamp = RctManager.Stamp;
	}


	public Csb(IDbConnection connection, bool validateServerName = true) : base(connection, validateServerName)
	{
		_Stamp = RctManager.Stamp;
	}


	public Csb(IBsPropertyAgent ci, bool validateServerName = true) : base(ci, validateServerName)
	{
		_Stamp = RctManager.Stamp;
	}


	public Csb(IVsDataExplorerNode node, bool validateServerName = true) : base(node, validateServerName)
	{
		_Stamp = RctManager.Stamp;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// .ctor for use only by AbstruseCsbAgent for registering FlameRobin datasetKeys.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public Csb(string server, int port, string database,
			string user, string password, string charset)
		: base(server, port, database, user, password, charset)
	{
	}


	public object Clone()
	{
		Csb clone = [];

		foreach (Describer describer in DescriberKeys)
		{
			if (ContainsKey(describer.Name))
			{
				clone[describer.Name] = this[describer.Name];
			}

			clone._Stamp = _Stamp;
		}

		return clone;
	}


	#endregion Constructors / Destructors




	// =========================================================================================================
	#region Fields - Csb
	// =========================================================================================================


	private long _Stamp = -1;


	#endregion Fields




	// =========================================================================================================
	#region Property accessors - Csb
	// =========================================================================================================




	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Describer collection of uniform properties used by the FbConnectionStringBuilder
	/// wrapper, Csb, as well as PropertyAgent and it's descendent SqlEditor
	/// ConnectionInfo and Dispatcher connection classes, and also the SE root nodes,
	/// Root and Database.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static new DescriberDictionary Describers => AbstractCsb.Describers;


	/// <summary>
	/// Returns an enumerable of all connection describers of parameters that appear in the 'Advanced'
	/// dialog of a connection dialog.
	/// See the <seealso cref="DescriberKeys"/> enumerable for further information.
	/// </summary>
	public static IEnumerable<Describer> AdvancedKeys => Describers.AdvancedKeys;

	/// <summary>
	/// Returns an enumerable of all connection describers that are valid connection parameters
	/// for the underlying native database engine.
	/// See the <seealso cref="DescriberKeys"/> enumerable for further information.
	/// </summary>
	public static IEnumerable<Describer> ConnectionKeys => Describers.ConnectionKeys;

	/// <summary>
	/// Returns an enumerable of all connection key describers in the <see cref="DescriberDictionary"/>.
	/// A <see cref="Describer"/> is a detailed class equivalent of a descriptor. The database engine's
	/// <see cref="DescriberDictionary"/> is defined in the native database
	/// <see cref="IBsNativeDatabaseEngine"/> service.
	/// </summary>
	public static IEnumerable<Describer> DescriberKeys => Describers.DescriberKeys;

	/// <summary>
	/// Returns a <see cref="Describer"/> enumerable of all equivalency connection parameters as defined in the User
	/// Options. Equivalency parameters are connection parameters that could produce differing
	/// result sets. See the <seealso cref="DescriberKeys"/> enumerable for further information.
	/// </summary>
	public static IEnumerable<Describer> EquivalencyKeys => Describers.EquivalencyKeys;

	/// <summary>
	/// MandatoryKeys include the minimum set of connection parameters required to establish
	/// a connection including unsafe (non-public) parameters. eg. Passwords.
	/// See the <seealso cref="DescriberKeys"/> enumerable for further information.
	/// </summary>
	public static IEnumerable<Describer> MandatoryKeys => Describers.MandatoryKeys;

	/// <summary>
	/// PublicMandatoryKeys is a subset of <see cref="MandatoryKeys"/> that includes the minimum set
	/// of connection parameters required to establish a connection excluding unsafe (non-public)
	/// parameters. eg. Passwords.
	/// See the <seealso cref="DescriberKeys"/> enumerable for further information.
	/// </summary>
	public static IEnumerable<Describer> PublicMandatoryKeys => Describers.PublicMandatoryKeys;

	/// <summary>
	/// The weak equivalency keys enumerable is a subset of <see cref="EquivalencyKeys"/> and does
	/// not include the Application Name equivalency key if it has been included as an equivalency
	/// key in User options.
	/// This enumerable is used by the running connection table (Rct) and LinkageParser to identify
	/// equivalent connections that are differentiated by the application name parameter only, and
	/// are therefore functionally equivalent.
	/// </summary>
	public static IEnumerable<Describer> WeakEquivalencyKeys => Describers.WeakEquivalencyKeys;


	/// <summary>
	/// Performs a stored equivalency and validity check against connections. Many
	/// commands as well as PropertyWindows use Csb to do status validations and
	/// updates on pulsed events.
	/// This can result in a very high volume of calls, so the agent stores the
	/// <see cref="RctManager.Stamp"/> for low overhead responses when the connection
	/// has not changed.
	/// </summary>
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool IsInvalidated => _Stamp != RctManager.Stamp;


	/// <summary>
	/// Drift detection stamp of a csb.
	/// </summary>
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public long Stamp => _Stamp;

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool IsServerConnection => ServerType == (int)EnServerType.Default;


	#endregion Property accessors




	// =========================================================================================================
	#region Methods - Csb
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
			string derivedConnectionName = DatasetKeyFormat.FmtRes(DataSource, datasetId);

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
	public static string CreateConnectionUrl(IBsModelPropertyAgent connInfo)
	{
		return (new Csb(connInfo.ConnectionString, false)).SafeDatasetMoniker;
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
		return (new Csb(connection.ConnectionString, false)).SafeDatasetMoniker;
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
	public static string CreateConnectionUrl(IVsDataConnection site)
	{
		return (new Csb(site.DecryptedConnectionString(), false)).SafeDatasetMoniker;
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

		return new Csb(connectionString, false).SafeDatasetMoniker;
	}


	public override int GetHashCode()
	{
		return base.GetHashCode();
	}


	public static string GetServerExplorerName(string connectionString)
	{
		return new Csb(connectionString, false).ServerExplorerName;
	}


	/// <summary>
	/// Determines if the connection properties object is sufficiently complete,
	/// inclusive of password for connections other than Properties settings
	/// connection strings, in order to establish a database connection.
	/// </summary>
	public static bool IsComplete(string connectionString)
	{
			try
			{
				Csb csa = new(connectionString, false);

				foreach (Describer describer in Csb.MandatoryKeys)
				{
					object value = csa[describer.Key];

					if (string.IsNullOrEmpty((string)value))
						return false;
				}
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw;
			}

			return true;
	}



	public static bool IsWeakConnectionEquivalency(string connectionString1, string connectionString2)
		=> AreEquivalent(connectionString1, connectionString2, WeakEquivalencyKeys);



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Parses a ConnectionInfo object.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected override void Parse(IBsPropertyAgent ci)
	{
		// Tracer.Trace(GetType(), "Parse(IBsPropertyAgent)");

		foreach (Describer describer in DescriberKeys)
		{
			if (ci.Contains(describer.Key))
				this[describer.Key] = ci[describer.Key];
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Updates the csa with the state of the <see cref="RunningConnectionTable"/> stamp
	/// for future validity checks by <see cref="IsInvalidated"/>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public void RefreshDriftDetectionState()
	{
		_Stamp = RctManager.Stamp;
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
		string derivedConnectionName = DatasetKeyFormat.FmtRes(DataSource, derivedDatasetId);
		string derivedAlternateConnectionName = DatasetKeyAlternateFormat.FmtRes(DataSource, derivedDatasetId);


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
	#region Sub-Classes - Csb
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
			// Tracer.Trace(GetType(), "GetProperties()");

			return TypeDescriptor.GetProperties(typeof(Csb), attributes);
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
