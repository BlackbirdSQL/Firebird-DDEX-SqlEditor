
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Ctl.Extensions;
using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.Core.Model.Enums;
using BlackbirdSql.Core.Properties;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;
using Microsoft.VisualStudio.Shell;

using CoreConstants = BlackbirdSql.Core.Ctl.CoreConstants;


namespace BlackbirdSql.Core.Model;

// =========================================================================================================
//											RctManager Class
//
/// <summary>
/// Manages the RunningConnectionTable.
/// </summary>
/// <remarks>
/// Clients should always interface with the Rct through this agent.
/// This class's singleton instance should remain active throughout the ide session.
/// </remarks>
// =========================================================================================================
public sealed class RctManager : IDisposable
{


	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - AbstractRunningConnectionTable
	// ---------------------------------------------------------------------------------


	/// <summary>
	/// Singleton .ctor.
	/// </summary>
	private RctManager()
	{
		if (_Instance != null)
		{
			TypeAccessException ex = new(Resources.ExceptionDuplicateSingletonInstances.FmtRes(GetType().FullName));
			Diag.Dug(ex);
			throw ex;
		}

		_Instance = this;
	}

	/// <summary>
	/// Gets the instance of the RctManager for this session.
	/// We do not auto-create to avoid instantiation confusion.
	/// Use CreateInstance() to instantiate.
	/// </summary>
	public static RctManager Instance => _Instance;


	/// <summary>
	/// Creates the singleton instance of the RctManager for this session.
	/// Instantiation must always occur here and not by the Instance accessor to avoid
	/// confusion.
	/// </summary>
	public static RctManager CreateInstance() => new RctManager();



	/// <summary>
	/// Disposal of Rct at the end of an IDE session.
	/// </summary>
	public void Dispose()
	{
		Delete();
	}


	#endregion Constructors / Destructors




	// =========================================================================================================
	#region Fields and Constants - RctManager
	// =========================================================================================================


	private static RctManager _Instance;
	private RunningConnectionTable _Rct;
	private static readonly string _Scheme = $"{CsbAgent.C_Scheme}://";
	private static string _UnadvisedConnectionString = null;

	private static char _EdmDatasetGlyph = '\0';
	private static char _ProjectDatasetGlyph = '\0';


	#endregion Fields and Constants




	// =========================================================================================================
	#region Property accessors - RctManager
	// =========================================================================================================


	public static bool Available => _Instance != null && _Instance._Rct != null
		&& !_Instance._Rct.ShutdownState && !_Instance._Rct.Loading;

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The table of all registered databases of the current data provider located in
	/// Server Explorer, FlameRobin and the Solution's Project settings, and any
	/// volatile unique connections defined in SqlEditor.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static DataTable Databases => _Instance == null ? null : Rct?.Databases;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The sorted and filtered view of the Databases table containing only
	/// DataSources/Servers.
	/// </summary>
	/// <returns>
	/// The populated <see cref="DataTable"/> that can be used together with
	/// <see cref="Databases"/> in a 1-n scenario. <see cref="ErmBindingSource"/> for
	/// an example.
	/// </returns>
	// ---------------------------------------------------------------------------------
	public static DataTable DataSources => _Instance == null ? null : Rct?.DataSources;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The glyph used to identify connections derived from Project EDM connections.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static char EdmDatasetGlyph
	{
		get
		{
			if (_EdmDatasetGlyph == '\0')
				_EdmDatasetGlyph = Properties.Resources.RunningConnectionTableEdmDataset[0];

			return _EdmDatasetGlyph;
		}
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns true if the Rct is physically in a loading state and both sync and
	/// async tasks are not in Inactive or Shutdown states.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static bool Loading => _Instance != null && _Instance._Rct != null && _Instance._Rct.Loading;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The glyph used to identify connections derived from Project connections
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static char ProjectDatasetGlyph
	{
		get
		{
			if (_ProjectDatasetGlyph == '\0')
				_ProjectDatasetGlyph = Resources.RunningConnectionTableProjectDatasetId[0];

			return _ProjectDatasetGlyph;
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns the Rct else null if the rct is in a shutdown state.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private static RunningConnectionTable Rct
	{
		get
		{
			if (_Instance == null)
				return null;

			if (_Instance._Rct == null)
			{
				LoadConfiguredConnections(false);
			}
			else if (Loading)
			{
				_Instance._Rct.WaitForSyncLoad();
				_Instance._Rct.WaitForAsyncLoad();
			}

			return _Instance._Rct;
		}
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns an IEnumerable of the registered datasets else null if the rct is in a
	/// shutdown state.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static IEnumerable<string> RegisteredDatasets => Rct?.RegisteredDatasets;



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns the sequential seed of the last attempt to modify the
	/// <see cref="RunningConnectionTable"/>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static long Seed => RunningConnectionTable.Seed;



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns the seed of the last connection registered or updated.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static bool ShutdownState => (_Instance == null || _Instance._Rct == null || _Instance._Rct.ShutdownState);


	#endregion Property accessors




	// =========================================================================================================
	#region Methods - RctManager
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Clones and returns a CsbAgent instance from a registered connection using a
	/// ConnectionInfo object else null if the rct is in a shutdown state.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static CsbAgent CloneRegistered(IBPropertyAgent connectionInfo)
	{
		if (connectionInfo == null)
		{
			ArgumentNullException ex = new(nameof(connectionInfo));
			Diag.Dug(ex);
			throw ex;
		}

		if (Rct == null)
			return null;

		CsbAgent csa = new(connectionInfo);
		string connectionUrl = csa.SafeDatasetMoniker;


		if (!Rct.TryGetHybridRowValue(connectionUrl, out DataRow row))
		{
			KeyNotFoundException ex = new KeyNotFoundException($"Connection url not found in RunningconnectionTable for key: {connectionUrl}");
			Diag.Dug(ex);
			throw ex;
		}


		return new CsbAgent((string)row[CoreConstants.C_KeyExConnectionString]);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Clones and returns instance from a registered connection using an IDbConnection
	/// else null if the rct is in a shutdown state..
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static CsbAgent CloneRegistered(IDbConnection connection)
	{
		if (connection == null)
		{
			ArgumentNullException ex = new(nameof(connection));
			Diag.Dug(ex);
			throw ex;
		}

		if (Rct == null)
			return null;

		if (!Rct.TryGetHybridRowValue(connection.ConnectionString, out DataRow row))
		{
			KeyNotFoundException ex = new KeyNotFoundException($"Connection url not found in RunningconnectionTable for ConnectionString: {connection.ConnectionString}");
			Diag.Dug(ex);
			throw ex;
		}


		return new CsbAgent((string)row[CoreConstants.C_KeyExConnectionString]);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Clones and returns an instance from a registered connection using a Server
	/// Explorer node else null if the rct is in a shutdown state.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static CsbAgent CloneRegistered(IVsDataExplorerNode node)
	{
		if (node == null)
		{
			ArgumentNullException ex = new(nameof(node));
			Diag.Dug(ex);
			throw ex;
		}

		if (Rct == null)
			return null;

		CsbAgent csa = new(node);
		string connectionUrl = csa.SafeDatasetMoniker;


		if (!Rct.TryGetHybridRowValue(connectionUrl, out DataRow row))
		{
			KeyNotFoundException ex = new KeyNotFoundException($"Connection url not found in RunningconnectionTable for key: {connectionUrl}");
			Diag.Dug(ex);
			throw ex;
		}


		return new CsbAgent((string)row[CoreConstants.C_KeyExConnectionString]);
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Clones and returns an instance from a registered connection using either a
	/// DatasetKey or ConnectionString else null if the rct is in a shutdown state.
	/// </summary>
	/// <param name="connectionKey">
	/// The DatasetKey or ConnectionString.
	/// </param>
	// ---------------------------------------------------------------------------------
	public static CsbAgent CloneRegistered(string hybridKey)
	{
		if (hybridKey == null)
		{
			ArgumentNullException ex = new(nameof(hybridKey));
			Diag.Dug(ex);
			throw ex;
		}

		if (Rct == null)
			return null;

		if (!Rct.TryGetHybridRowValue(hybridKey, out DataRow row))
			return null;

		return new CsbAgent((string)row[CoreConstants.C_KeyExConnectionString]);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Clones a CsbAgent instance from a registered connection using an
	/// IDbConnection , else null if the rct is in a shutdown state.
	/// Finally registers the csa for validity state checks.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static CsbAgent CloneVolatile(IDbConnection connection)
	{
		if (connection == null)
		{
			ArgumentNullException ex = new(nameof(connection));
			Diag.Dug(ex);
			throw ex;
		}

		if (Rct == null)
			return null;


		CsbAgent csa = CloneRegistered(connection);
		csa.RegisterValidationState(connection.ConnectionString);

		return csa;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Resets the underlying Running Connection Table in preparation for and ide
	/// shutdown or solution unload.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static void Delete()
	{
		if (_Instance != null && _Instance._Rct != null)
		{
			_Instance._Rct.Dispose();
			_Instance._Rct = null;
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the SE connection key given the ConnectionUrl or DatasetKey
	/// or DatsetKey synonym.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static string GetConnectionKey(string connectionValue)
	{
		if (Rct == null)
			return null;

		if (!Rct.TryGetHybridRowValue(connectionValue, out DataRow row))
			return null;

		object @object = row[CoreConstants.C_KeyExConnectionKey];

		return (@object == DBNull.Value || @object == null) ? null : (string)@object;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets a connection DatasetKey given the ConnectionUrl or DatasetKey
	/// or DatsetKey synonym.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static string GetDatasetKey(string connectionValue)
	{
		if (Rct == null)
			return null;

		if (!Rct.TryGetHybridRowValue(connectionValue, out DataRow row))
			return null;

		object @object = row[CoreConstants.C_KeyExDatasetKey];

		return (@object == DBNull.Value || @object == null) ? null : (string)@object;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Creates and returns an instance from a registered connection using a
	/// ConnectionString else registers a new connection if none exists else null if the
	/// rct is in a shutdown state..
	/// </summary>
	// ---------------------------------------------------------------------------------
	private static CsbAgent EnsureInstance(string connectionString, EnConnectionSource source)
	{
		if (connectionString == null)
		{
			ArgumentNullException ex = new(nameof(connectionString));
			Diag.Dug(ex);
			throw ex;
		}

		if (ShutdownState)
			return null;


		if (Rct.TryGetHybridRowValue(connectionString, out DataRow row))
			return new CsbAgent((string)row[CoreConstants.C_KeyExConnectionString]);


		// Calls to this method expect a registered connection. If it doesn't exist it means
		// we're creating a new configured connection.

		CsbAgent csa = new(connectionString);
		string datasetId = csa.DatasetId;

		if (string.IsNullOrWhiteSpace(datasetId))
			datasetId = csa.Dataset;

		// If the proposed key matches the proposed generated one, drop it.

		if (csa.ContainsKey(CoreConstants.C_KeyExConnectionName)
			&& !string.IsNullOrWhiteSpace(csa.ConnectionName)
			&& CsbAgent.C_DatasetKeyFmt.FmtRes(csa.DataSource, datasetId) == csa.ConnectionName)
		{
			csa.Remove(CoreConstants.C_KeyExConnectionName);
		}

		Rct.RegisterUniqueConnection(csa.ConnectionName, datasetId, source, ref csa);

		return csa;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Creates and returns an instance from a registered connection using a
	/// ConnectionString else registers a new node connection if none exists else null
	/// if the rct is in a shutdown state.
	/// Finally registers the csa for validity state checks.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static CsbAgent EnsureVolatileInstance(IDbConnection connection, EnConnectionSource source)
	{
		if (connection == null)
		{
			ArgumentNullException ex = new(nameof(connection));
			Diag.Dug(ex);
			throw ex;
		}

		if (Rct == null)
			return null;


		CsbAgent csa;


		if (Rct.TryGetHybridRowValue(connection.ConnectionString, out DataRow row))
		{
			csa = new((string)row[CoreConstants.C_KeyExConnectionString]);
			csa.RegisterValidationState(connection.ConnectionString);

			// Tracer.Trace(typeof(CsbAgent), "EnsureVolatileInstance()", "Found registered DataRow for dbConnectionString: {0}\nrow ConnectionString: {1}\ncsa.ConnectionString: {2}.", connection.ConnectionString, (string)row[CoreConstants.C_KeyExConnectionString], csa.ConnectionString);

			return csa;
		}

		// New registration.

		// Tracer.Trace(typeof(CsbAgent), "EnsureVolatileInstance()", "Could NOT find registered DataRow for dbConnectionString: {0}.", connection.ConnectionString);

		csa = new(connection);
		string datasetId = csa.DatasetId;

		if (string.IsNullOrWhiteSpace(datasetId))
			datasetId = csa.Dataset;

		if (!string.IsNullOrWhiteSpace(csa.ConnectionName)
			&& CsbAgent.C_DatasetKeyFmt.FmtRes(csa.DataSource, datasetId) == csa.ConnectionName)
		{
			csa.ConnectionName = CoreConstants.C_DefaultExConnectionName;
		}

		if (Rct.RegisterUniqueConnection(csa.ConnectionName, datasetId, source, ref csa))
		{
			csa.RegisterValidationState(connection.ConnectionString);
		}


		return csa;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Finds an explorer connection given a ConnectionString.
	/// </summary>
	/// <returns>
	/// The tuple (label, explorerConnection)
	/// </returns>
	// ---------------------------------------------------------------------------------
	private static (string, IVsDataExplorerConnection) FindServerExplorerConnection(string connectionString)
	{
		IVsDataExplorerConnectionManager manager =
			(Controller.OleServiceProvider.QueryService<IVsDataExplorerConnectionManager>()
			as IVsDataExplorerConnectionManager)
			?? throw Diag.ExceptionService(typeof(IVsDataExplorerConnectionManager));

		Guid clsidProvider = new(SystemData.ProviderGuid);
		string connectionUrl = CsbAgent.CreateConnectionUrl(connectionString);


		CsbAgent csa;

		foreach (KeyValuePair<string, IVsDataExplorerConnection> pair in manager.Connections)
		{
			if (pair.Value.Provider != clsidProvider)
				continue;

			csa = new(DataProtection.DecryptString(pair.Value.EncryptedConnectionString));

			if (csa.SafeDatasetMoniker == connectionUrl)
			{

				// Tracer.Trace(typeof(RctManager), "FindServerExplorerConnection()", "Found pair.Key: {0}: pair.Value.DisplayName: {1}", pair.Key, pair.Value.DisplayName);
				return (pair.Key, pair.Value);
			}
		}

		return (null, null);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Invalidates the Rct for active static CsbAgents so that they can preform
	/// validation checkS.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static void Invalidate()
	{
		if (Rct == null)
			return;

		Rct.Invalidate();

		// Tracer.Trace(typeof(RctManager), "Invalidate()", "New seed: {0}", Seed);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Loads ServerExplorer, FlameRobin and Application connection configurations from
	/// OnSolutionLoad, package initialization, ServerExplorer or any other applicable
	/// activation event.
	/// </summary>
	/// <returns>
	/// True if the load succeeded or connections were already loaded else false if the
	/// rct is in a shutdown state.
	/// </returns>
	/// <remarks>
	/// This is the entry point for loading configured connections and is a deadlock
	/// Bermuda Triangle :), because there are 3 basic events that can activate it.
	/// 1. ServerExplorer before the extension has fully completed async initialization.
	/// 2. UICONTEXT.SolutionExists also before async initialization.
	/// 3. Extension async initialization.
	/// 4. Any of 1, 2 or 3 after LoadConfiguredConnections() is already activated.
	/// The only way to control this is to create a managed awaitable.
	/// </remarks>
	// ---------------------------------------------------------------------------------
	public static bool LoadConfiguredConnections(bool initializer)
	{
		// Tracer.Trace(typeof(RctManager), "LoadConfiguredConnections()", "Instance._Rct == null: {0}", Instance._Rct == null);

		if (_Instance == null)
			return false;

		if (!Loading && Instance._Rct == null)
		{
			Instance._Rct = RunningConnectionTable.CreateInstance();
			Instance._Rct.LoadConfiguredConnections();
			if (initializer)
				return true;

			Instance._Rct.WaitForSyncLoad();
			initializer = true;
		}
		else if (initializer)
		{
			return false;
		}
		else if (Loading)
		{
			if (initializer)
				return false;

			Instance._Rct.WaitForSyncLoad();
		}


		// If we initialized then UIThread means something otherwise check async anyway.
		if (!initializer || !ThreadHelper.CheckAccess())
		{
			Instance._Rct.WaitForAsyncLoad();
		}

		return Instance._Rct != null;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Loads a project's App.Config Settings and EDM connections.
	/// </summary>
	/// <param name="probject">The <see cref="EnvDTE.Project"/>.</param>
	/// <remarks>
	/// This method only applies to projects late loaded or added after a solution has
	/// completed loading.
	/// </remarks>
	// ---------------------------------------------------------------------------------
	public static bool LoadProjectConnections(object probject)
	{
		// Tracer.Trace(typeof(RctManager), "LoadProjectConnections()");

		if (_Instance == null || Instance._Rct == null || ShutdownState)
			return false;

		return Instance._Rct.AsyncLoadConfiguredConnections(probject);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Modifies the name and connection information of Server Explorers internal
	/// IVsDataExplorerConnectionManager connection entry.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private static bool ModifyServerExplorerConnection(IVsDataExplorerConnection explorerConnection,
		ref CsbAgent csa, bool modifyExplorerConnection)
	{
		// Tracer.Trace(typeof(RctManager), "ModifyServerExplorerConnection()", "csa.ConnectionString: {0}, modifyExplorerConnection: {1}.", csa.ConnectionString, modifyExplorerConnection);

		CsbAgent seCsa = new(DataProtection.DecryptString(explorerConnection.Connection.EncryptedConnectionString));
		string connectionKey = explorerConnection.ConnectionKey();

		if (string.IsNullOrWhiteSpace(csa.ConnectionKey) || csa.ConnectionKey != connectionKey)
		{
			csa.ConnectionKey = connectionKey;
		}


		// Sanity check. Should already be done.
		// Perform a deep validation of the updated csa to ensure an update
		// is in fact required.
		bool updateRequired = !CsbAgent.AreEquivalent(csa, seCsa, CsbAgent.DescriberKeys);

		if (explorerConnection.DisplayName.Equals(csa.DatasetKey))
		{
			if (!updateRequired)
			{
				explorerConnection.ConnectionNode.Select();
				return false;
			}
		}
		else
		{
			Rct?.DisableEvents();
			explorerConnection.DisplayName = csa.DatasetKey;
			Rct?.EnableEvents();

			if (!updateRequired)
				return true;
		}

		if (!modifyExplorerConnection)
			return false;

		// An update is required...


		Rct?.DisableEvents();
		explorerConnection.Connection.EncryptedConnectionString = DataProtection.EncryptString(csa.ConnectionString);
		explorerConnection.ConnectionNode.Select();
		Rct?.EnableEvents();

		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Prevents name mangling of the DataSource name. Returns a uniformly cased
	/// Server/DataSource name of the provided name.
	/// To prevent name mangling of server names, the case of the first connection
	/// discovered for a server name is the case that will be used for all future
	/// connections for that server.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static string RegisterServer(string serverName, int port)
	{
		if (ShutdownState || _Instance == null || _Instance._Rct == null)
		{
			InvalidOperationException ex = new("RctManager.RegisterServer called before LoadConfiguredConnections().");
			Diag.Dug(ex);
			throw ex;
		}

		return _Instance._Rct.RegisterServer(serverName, port);
	}


	// ---------------------------------------------------------------------------------
	// Perform a single pass check to ensure an SE connection has had it's events
	// advised. This will occur if the SE was adding a new conection and we
	// registered it with the Rct before it was registered with Server Explorer.
	// ---------------------------------------------------------------------------------
	public static void RegisterUnadvisedConnection(IVsDataExplorerConnection explorerConnection)
	{
		if (_UnadvisedConnectionString == null)
			return;

		if (_UnadvisedConnectionString == DataProtection.DecryptString(explorerConnection.EncryptedConnectionString))
		{
			Rct.AdviseEvents(explorerConnection);
		}

		_UnadvisedConnectionString = null;
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Stores the ConnectionString of a connection added by the SE. The connection
	/// events cannot be linked until the SE has registered it in it's own tables.
	/// The stored value wil be retrieved as a single pass in IVsDataViewSupport
	/// and linked using <see cref="RegisterUnadvisedConnection"/>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static void StoreUnadvisedConnection(string connectionString)
	{
		_UnadvisedConnectionString = connectionString;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Lists all entries in the RunningConnectionTable to debug out. The command will
	/// appear on any Sited node of the SE on debug builds.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static void TraceRct()
	{
		try
		{
			string str = "\nRegistered Datasets: ";
			object datasetKey;

			foreach (DataRow row in Rct.InternalConnectionsTable.Rows)
			{
				datasetKey = row[CoreConstants.C_KeyExDatasetKey];

				if (datasetKey == DBNull.Value || string.IsNullOrWhiteSpace((string)datasetKey))
					continue;
				str += "\n--------------------------------------------------------------------------------------";
				str += "\n--------------------------------------------------------------------------------------";
				str += $"\nDATASETKEY: {((string)row[CoreConstants.C_KeyExDatasetKey])}, ConnectionUrl: {((string)row[CoreConstants.C_KeyExConnectionUrl])}";
				str += "\n\t------------------------------------------";
				str += "\n\t";

				foreach (DataColumn col in Rct.Databases.Columns)
				{
					if (col.ColumnName == CoreConstants.C_KeyExDatasetKey || col.ColumnName == CoreConstants.C_KeyExConnectionUrl || col.ColumnName == CoreConstants.C_KeyExConnectionString)
					{
						continue;
					}
					str += $"{col.ColumnName}: {(row[col.ColumnName] == null ? "null" : (row[col.ColumnName] == DBNull.Value ? "DBNull" : row[col.ColumnName].ToString()))}, ";
				}
				str += "\n\t------------------------------------------";
				str += $"\n\tConnectionString: {((string)row[CoreConstants.C_KeyExConnectionString])}";
			}
			Tracer.Information(typeof(RctManager), "TraceRct()", "{0}", str);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Updates a connection string with the registration properties of it's unique
	/// registered connection, if it exists.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static string UpdateConnectionFromRegistration(IDbConnection connection)
	{
		if (connection == null)
		{
			ArgumentNullException ex = new(nameof(connection));
			Diag.Dug(ex);
			throw ex;
		}

		string connectionString = connection.ConnectionString;

		if (ShutdownState)
			return connectionString;


		CsbAgent csa = new(connectionString);


		if (!Rct.TryGetHybridRowValue(csa.SafeDatasetMoniker, out DataRow row))
			return connectionString;

		object colObject = row[CoreConstants.C_KeyExDatasetKey];
		if (colObject != null && colObject != DBNull.Value)
			csa.DatasetKey = (string)colObject;

		colObject = row[CoreConstants.C_KeyExConnectionKey];
		if (colObject != null && colObject != DBNull.Value)
			csa.ConnectionKey = (string)colObject;

		colObject = row[CoreConstants.C_KeyExConnectionName];
		if (colObject != null && colObject != DBNull.Value)
			csa.ConnectionName = (string)colObject;

		if (!string.IsNullOrEmpty(csa.ConnectionName))
			colObject = null;
		else
			colObject = row[CoreConstants.C_KeyExDatasetId];
		if (colObject != null && colObject != DBNull.Value)
			csa.DatasetId = (string)colObject;

		colObject = row[CoreConstants.C_KeyExConnectionSource];
		if (colObject != null && colObject != DBNull.Value)
			csa.ConnectionSource = (EnConnectionSource)(int)colObject;

		return csa.ConnectionString;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Updates an existing registered connection using the provided connection string.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static CsbAgent UpdateOrRegisterConnection(string connectionString,
		EnConnectionSource source, bool addExplorerConnection, bool modifyExplorerConnection)
	{
		if (Rct == null)
			return null;


		CsbAgent csa = Rct.UpdateRegisteredConnection(connectionString, source, false);


		// If it's null force a creation.
		csa ??= EnsureInstance(connectionString, source);

		// Update the SE.
		UpdateServerExplorer(ref csa, addExplorerConnection, modifyExplorerConnection);

		return csa;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Updates the Server Explorer tables when a connection has been added or modified.
	/// </summary>
	/// <remarks>
	/// There is a caveat to a requested addExplorerConnection. If the SE is adding a
	/// connection, we will be unable to update it here because it will not yet exist
	/// in the SE connection table.
	/// In that case the <see cref="RctManager"/> will tag it for updating using
	/// <see cref="RctManager.StoreUnadvisedConnection"/>.
	/// </remarks>
	// ---------------------------------------------------------------------------------
	private static bool UpdateServerExplorer(ref CsbAgent csa,
		bool addExplorerConnection, bool modifyExplorerConnection)
	{
		// Tracer.Trace(typeof(RctManager), "UpdateServerExplorer()", "csa.ConnectionString: {0}, addServerExplorerConnection: {1}, modifyExplorerConnection: {2}.", csa.ConnectionString, addExplorerConnection, modifyExplorerConnection);

		csa.ConnectionSource = EnConnectionSource.ServerExplorer;

		(_, IVsDataExplorerConnection explorerConnection) = FindServerExplorerConnection(csa.ConnectionString);

		if (explorerConnection != null)
			return ModifyServerExplorerConnection(explorerConnection, ref csa, modifyExplorerConnection);

		if (!addExplorerConnection)
			return false;


		csa.ConnectionKey = csa.DatasetKey;

		IVsDataExplorerConnectionManager manager =
			(Controller.OleServiceProvider.QueryService<IVsDataExplorerConnectionManager>()
			as IVsDataExplorerConnectionManager)
			?? throw Diag.ExceptionService(typeof(IVsDataExplorerConnectionManager));

		Rct.DisableEvents();

		explorerConnection = manager.AddConnection(csa.DatasetKey, new(SystemData.ProviderGuid), DataProtection.EncryptString(csa.ConnectionString), true);

		Rct.AdviseEvents(explorerConnection);

		explorerConnection.ConnectionNode.Select();

		try
		{
			explorerConnection.Connection.EnsureConnected();
			explorerConnection.ConnectionNode.Refresh();
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
		}

		Rct.EnableEvents();

		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Validates IVsDataExplorerConnection to establish if it's DisplayName matches
	/// a proposed ConnectionName and updates the Server Explorer internal table if it
	/// doesn't.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static void ValidateAndUpdateExplorerConnectionRename(IVsDataExplorerConnection explorerConnection,
		string proposedConnectionName)
	{
		if (Rct == null)
			return;

		string connectionString = DataProtection.DecryptString(explorerConnection.EncryptedConnectionString);
		CsbAgent csa = new(connectionString);

		if (proposedConnectionName == csa.DatasetKey)
			return;

		string msg;
		string caption;

		if (proposedConnectionName.StartsWith(_Scheme))
		{
			Rct.DisableEvents();
			explorerConnection.DisplayName = csa.DatasetKey;
			Rct.EnableEvents();

			caption = "Invalid connection name";
			msg = $"Connection names beginning with '{_Scheme}' are reserved for connection monikers. The ConnectionName '{proposedConnectionName}' is not permitted.";
			Cmd.ShowMessage(msg, caption, MessageBoxButtons.OK);

			return;
		}


		string connectionUrl = csa.SafeDatasetMoniker;
		string proposedDatasetId = csa.DatasetId;
		string dataSource = csa.DataSource;
		string dataset = csa.Dataset;


		// Check whether the connection name will change.
		(_, string uniqueDatasetKey, string uniqueConnectionName, string uniqueDatasetId, _) =
			Rct.GenerateUniqueDatasetKey(proposedConnectionName, proposedDatasetId, dataSource, dataset, connectionUrl, connectionUrl);

		if (!string.IsNullOrEmpty(uniqueConnectionName))
		{
			caption = "ConnectionName conflict";
			msg = $"The Name '{proposedConnectionName}' is already allocated to another connection.\nUse '{uniqueConnectionName}' instead?\n\n(Note: The RunningConnectionTable keeps track of all connections and connection names, even if they are deleted or renamed in Server Explorer, so that windows that may be using them can still be serviced. Reload the solution to refresh the Rct.) ";

			if (msg != null && Cmd.ShowMessage(msg, caption, MessageBoxButtons.YesNo) == DialogResult.No)
			{
				Rct.DisableEvents();
				explorerConnection.DisplayName = GetDatasetKey(connectionUrl);
				Rct.EnableEvents();

				return;
			}
		}


		// At this point we're good to go.
		csa.DatasetKey = uniqueDatasetKey;

		if (uniqueConnectionName == string.Empty)
			csa.Remove(CoreConstants.C_KeyExConnectionName);
		else if (uniqueConnectionName != null)
			csa.ConnectionName = uniqueConnectionName;
		else
			csa.ConnectionName = proposedConnectionName;

		if (uniqueDatasetId == string.Empty)
			csa.Remove(CoreConstants.C_KeyExDatasetId);
		else if (uniqueDatasetId != null)
			csa.DatasetId = uniqueDatasetId;

		UpdateOrRegisterConnection(csa.ConnectionString, EnConnectionSource.ServerExplorer, false, true);

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Validates an IVsDataConnectionProperties Site to ensure it contains no
	/// unauthorised, invalid or redundant registration keys.
	/// This is to ensure redundant properties do not appear in future connection
	/// dialogs and that settings are valid for both the SE and BlackbirdSql connection
	/// tables.
	/// </summary>
	/// <returns>
	/// A tuple where... 
	/// Item1 (rSuccess): true if validation was successful else false if add or modify
	/// should be cancelled.
	/// Item2 (rAddInternally): true if connectionUrl (connection) has
	/// changed and requires adding a new connection internally.
	/// Item3 (rModifyInternally): true if connectionUrl changed and will now
	/// require internally modifying
	/// another connection.
	/// </returns>
	/// <param name="site">
	/// The IVsDataConnectionProperties Site (usually the Site of an
	/// IVsDataConnectionUIControl control.
	/// </param>
	/// <param name="source">
	/// The source requesting the validation.
	/// </param>
	/// <param name="insertMode">
	/// Boolean indicating wehther or not a connection is being added or modified.
	/// </param>
	/// <param name="originalConnectionString">
	/// The original ConnectionString before any editing of the Site took place.
	/// </param>
	// ---------------------------------------------------------------------------------
	public static (bool, bool, bool) ValidateSiteProperties(IVsDataConnectionProperties site, EnConnectionSource source,
		bool insertMode, string originalConnectionString)
	{
		bool rSuccess = true;
		bool rAddInternally = false;
		bool rModifyInternally = false;

		if (Rct == null)
			return (rSuccess, rAddInternally, rModifyInternally);

		string originalConnectionUrl = null;

		if (originalConnectionString != null)
			originalConnectionUrl = CsbAgent.CreateConnectionUrl(originalConnectionString);


		string proposedConnectionName = site.ContainsKey(CoreConstants.C_KeyExConnectionName)
			? (string)site[CoreConstants.C_KeyExConnectionName] : null;

		string msg = null;
		string caption = null;

		if (proposedConnectionName != null && proposedConnectionName.StartsWith(_Scheme))
		{
			caption = "Invalid connection name";
			msg = $"Connection names beginning with '{_Scheme}' are reserved for connection monikers. The ConnectionName '{proposedConnectionName}' is not permitted.";

			Cmd.ShowMessage(msg, caption, MessageBoxButtons.OK);

			rSuccess = false;
			return (rSuccess, rAddInternally, rModifyInternally);
		}

		string proposedDatasetId = site.ContainsKey(CoreConstants.C_KeyExDatasetId)
			? (string)site[CoreConstants.C_KeyExDatasetId] : null;

		string dataSource = (string)site[CoreConstants.C_KeyDataSource];
		string dataset = (string)site[CoreConstants.C_KeyExDataset];

		string connectionUrl = (site as IBDataConnectionProperties).Csa.SafeDatasetMoniker;

		// Check whether the connection name will change.
		(bool createNew, string uniqueDatasetKey, string uniqueConnectionName, string uniqueDatasetId, string changedTarget) =
			Rct.GenerateUniqueDatasetKey(proposedConnectionName, proposedDatasetId, dataSource, dataset, connectionUrl, originalConnectionUrl);


		if (!string.IsNullOrEmpty(uniqueConnectionName))
		{
			if (createNew)
			{
				caption = "New connection ConnectionName conflict";
				msg = $"The settings provided will create a new connection. The Proposed Connection Name '{proposedConnectionName}' is already allocated.\nUse '{uniqueConnectionName}' instead?";
			}
			else if (originalConnectionUrl == null || originalConnectionUrl == connectionUrl)
			{
				caption = "ConnectionName conflict";
				msg = $"The Proposed Connection Name '{proposedConnectionName}' is already allocated to another connection.\nUse '{uniqueConnectionName}' instead?";
			}
			else
			{
				caption = "Connection change ConnectionName conflict";
				msg = $"The modifed settings are allocated to connection '{changedTarget}', and will be applied to it instead.. The Proposed Connection Name '{proposedConnectionName}' is already allocated.\nUse '{uniqueConnectionName}' instead?";
			}
		}
		else if (!string.IsNullOrEmpty(uniqueDatasetId))
		{
			if (createNew)
			{
				caption = "New connection Database name conflict";
				msg = $"The settings provided will create a new connection. The Proposed Database Name '{proposedDatasetId}' is already allocated.\nUse '{uniqueDatasetId}' instead?";
			}
			else if (originalConnectionUrl == null || originalConnectionUrl == connectionUrl)
			{
				caption = "Database name conflict";
				msg = $"The Proposed Database Name '{proposedDatasetId}' is already allocated to another connection.\nUse '{uniqueDatasetId}' instead?";
			}
			else
			{
				caption = "Connection change Database name conflict";
				msg = $"The modifed settings are allocated to connection '{changedTarget}', and will be applied to it instead. The Proposed Database Name '{proposedDatasetId}' is already allocated.\nUse '{uniqueDatasetId}' instead?";
			}
		}
		else if (changedTarget != null)
		{
			// The target connection will change.
			caption = "Connection changed";
			msg = $"The modifed settings are allocated to connection '{changedTarget}', and will be applied to it instead. Continue?";
		}
		else if (createNew)
		{
			caption = "New connection";
			msg = $"The settings provided will create a new connection. Continue?";
		}

		if (msg != null && Cmd.ShowMessage(msg, caption, MessageBoxButtons.YesNo) == DialogResult.No)
			return (false, false, false);



		// At this point we're good to go.


		if (!site.ContainsKey(CoreConstants.C_KeyExDatasetKey)
			|| (string)site[CoreConstants.C_KeyExDatasetKey] != uniqueDatasetKey)
		{
			site[CoreConstants.C_KeyExDatasetKey] = uniqueDatasetKey;
		}

		if (uniqueConnectionName == string.Empty)
			site.Remove(CoreConstants.C_KeyExConnectionName);
		else if (uniqueConnectionName != null)
		{
			site[CoreConstants.C_KeyExConnectionName] = uniqueConnectionName;
		}

		if (uniqueDatasetId == string.Empty)
			site.Remove(CoreConstants.C_KeyExDatasetId);
		else if (uniqueDatasetId != null)
			site[CoreConstants.C_KeyExDatasetId] = uniqueDatasetId;


		// Establish the connection owner.
		// if the explorer connection exists or if it's the source it automatically is the owner.
		string connectionKey = site.ConnectionKey();

		if (connectionKey == null && source == EnConnectionSource.ServerExplorer)
			connectionKey = uniqueDatasetKey;

		// Tracer.Trace(typeof(RctManager), "ValidateSiteProperties()", "Retrieved ConnectionKey: {0}.", connectionKey ?? "Null");

		EnConnectionSource enConnectionSource = (EnConnectionSource)site[CoreConstants.C_KeyExConnectionSource];

		if (connectionKey != null)
		{
			// If the SE connection exists then it is the owner and we have to set the SE ConnectionKey
			// and make the SE the owner.
			string strValue = ((string)site[CoreConstants.C_KeyExConnectionKey]).Trim();

			if (strValue != connectionKey)
				site[CoreConstants.C_KeyExConnectionKey] = connectionKey;

			if (enConnectionSource != EnConnectionSource.ServerExplorer)
			{
				enConnectionSource = EnConnectionSource.ServerExplorer;
				site[CoreConstants.C_KeyExConnectionSource] = enConnectionSource;
			}
		}

		if (!insertMode && createNew)
			rAddInternally = true;

		rModifyInternally = changedTarget != null || (!rAddInternally && source != EnConnectionSource.ServerExplorer);

		rSuccess = true;

		return (rSuccess, rAddInternally, rModifyInternally);

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Verifies wether or not a source has update rights over a peristent and/or
	/// volatile stored connection, given the source and the owning source of the
	/// connecton.
	/// <summary>
	// ---------------------------------------------------------------------------------
	public static bool VerifyUpdateRights(EnConnectionSource updater,
		EnConnectionSource owner)
	{
		return RunningConnectionTable.VerifyUpdateRights(updater, owner);
	}


	#endregion Methods





	// =========================================================================================================
	#region Event handling - RctManager
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Insurance for removing an SE connection whose OnExplorerConnectionNodeRemoving event may not be fired.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static void OnExplorerConnectionClose(object sender, IVsDataExplorerConnection explorerConnection)
	{
		// Tracer.Trace(GetType(), "OnNodeRemoving()", "Node: {0}.", e.Node != null ? e.Node.ToString() : "null");
		if (ShutdownState || _Instance == null || _Instance._Rct == null
			|| _Instance._Rct.Loading || explorerConnection.ConnectionNode == null)
		{
			return;
		}

		DataExplorerNodeEventArgs eventArgs = new(explorerConnection.ConnectionNode);
		Rct.OnExplorerConnectionNodeRemoving(sender, eventArgs);

		IVsDataConnection site = explorerConnection.Connection;

		if (site != null)
			LinkageParser.DisposeInstance(site, true);
	}


	#endregion Event handling

}
