
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using BlackbirdSql.Core.Ctl;
using BlackbirdSql.Core.Ctl.Config;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Ctl.Extensions;
using BlackbirdSql.Core.Model.Enums;
using BlackbirdSql.Core.Model.Interfaces;
using BlackbirdSql.Core.Properties;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Shell;

using static BlackbirdSql.Core.Ctl.CoreConstants;


namespace BlackbirdSql.Core.Model;

// =========================================================================================================
//									AbstruseRunningConnectionTable Class
//
/// <summary>
/// Holds a database of volatile and non-volatile configured connections for a session.
/// Access to this class set should be through the <see cref="RctManager"/> only.
/// The Rct classes do not manage access availibility and deadlock prevention. That is the responsibility
/// of the <see cref="RctManager"/>.
/// </summary>
/// <remarks>
/// This abstruse class deals specifically deals with the initial loading of preconfigured connections.
/// The final class <see cref="RunningConnectionTable"/> deals specifically with IDictionary and DataTable
/// accessor handling of registered connection configurations.
/// Connections are distinct by their equivalency connection properties as defined in the BlackbirdSql user
/// options. No further distinction takes place.
/// The SE and SqlEditor(Session) are now fully synchronized. If SqlEditor creates a new connection,
/// it will be added to the SE unless 'Add New Connections To Server Explorer' is unchecked.
/// If the DatasetKey for a connection is changed, the old name becomes a synonym for the duration of the
/// session.
/// Once a synonym is added it cannot be used by another unique connection within the same
/// Solution/Application session.
/// Deleting an SE connection will convert it to a volatile session connection
/// in the Rct.
/// </remarks>
// =========================================================================================================
[SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread")]
[SuppressMessage("Usage", "VSTHRD102:Implement internal logic asynchronously")]
public abstract class AbstruseRunningConnectionTable : PublicDictionary<string, int>, IBRunningConnectionTable
{


	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - AbstruseRunningConnectionTable
	// ---------------------------------------------------------------------------------


	protected AbstruseRunningConnectionTable() : base(StringComparer.OrdinalIgnoreCase)
	{
		if (_Instance != null)
		{
			TypeAccessException ex = new(Resources.ExceptionDuplicateSingletonInstances.FmtRes(GetType().FullName));
			Diag.Dug(ex);
			throw ex;
		}

		_Instance = this;
	}


	public virtual void Dispose()
	{
		Dispose(true);

	}


	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			_Instance = null;
			_Seed++;

			bool launchersActive = false;

			lock (_LockObject)
			{
				UnloadServerExplorerConfiguredConnections();

				if (_SyncPayloadLauncherLaunchState != EnLauncherPayloadLaunchState.Inactive
					&& _SyncPayloadLauncherLaunchState != EnLauncherPayloadLaunchState.Shutdown)
				{
					launchersActive = true;
					_SyncPayloadLauncherTokenSource?.Cancel();
				}
				if (_AsyncPayloadLauncherLaunchState != EnLauncherPayloadLaunchState.Inactive
					&& _AsyncPayloadLauncherLaunchState != EnLauncherPayloadLaunchState.Shutdown)
				{
					launchersActive = true;
					_AsyncPayloadLauncherTokenSource?.Cancel();
				}
			}

			if (launchersActive)
				System.Threading.Thread.Sleep(50);

			launchersActive = false;

			lock (_LockObject)
			{
				if (_SyncPayloadLauncherLaunchState != EnLauncherPayloadLaunchState.Inactive)
				{
					launchersActive = true;
					ClearSyncPayloadLauncher();
					_SyncPayloadLauncherLaunchState = EnLauncherPayloadLaunchState.Shutdown;
				}

				if (_AsyncPayloadLauncherLaunchState != EnLauncherPayloadLaunchState.Inactive)
				{
					launchersActive = true;
					ClearAsyncPayloadLauncher();
					_AsyncPayloadLauncherLaunchState = EnLauncherPayloadLaunchState.Shutdown;
				}


				_HasLocal = false;
				_DataSources = null;
				_Databases = null;
				_InternalConnectionsTable = null;
				_RegisteredServerNames = null;
				Clear();
			}

			if (launchersActive)
				System.Threading.Thread.Sleep(50);

			_SyncPayloadLauncherLaunchState = EnLauncherPayloadLaunchState.Shutdown;
			_AsyncPayloadLauncherLaunchState = EnLauncherPayloadLaunchState.Shutdown;

		}
	}


	#endregion Constructors / Destructors




	// =========================================================================================================
	#region Fields and Constants - AbstruseRunningConnectionTable
	// =========================================================================================================


	private int _EventsCardinal = 0;
	private bool _HasLocal = false;
	protected static IBRunningConnectionTable _Instance;
	protected int _LoadDataCardinal = 0;
	private readonly object _LockObject = new();
	private int _LoadingSyncCardinal = 0;
	private int _LoadingAsyncCardinal = 0;
	protected static string _Scheme = $"{CsbAgent.C_Scheme}://";

	protected DataTable _Databases = null, _DataSources = null;
	protected DataTable _InternalConnectionsTable = null;
	private readonly IList<object> _Probjects = new List<object>();
	private IList<string> _RegisteredServerNames = new List<string>();

	/// <summary>
	/// Must be incremented on every possible update and registration.
	/// </summary>
	protected static long _Seed = -1;


	// Task handling fields.

	private CancellationToken _AsyncPayloadLauncherToken;
	private CancellationTokenSource _AsyncPayloadLauncherTokenSource = null;
	private CancellationToken _SyncPayloadLauncherToken;
	private CancellationTokenSource _SyncPayloadLauncherTokenSource = null;

	/// <summary>
	/// The async connection loader launcher task if it exists.
	/// </summary>
	private Task<bool> _AsyncPayloadLauncher;

	/// <summary>
	/// The async connection loader launcher process state, 0 = No process,
	/// 1 = launch queued, 2 = launch and/or payload Active.
	/// </summary>
	protected EnLauncherPayloadLaunchState _AsyncPayloadLauncherLaunchState = EnLauncherPayloadLaunchState.Inactive;

	/// <summary>
	/// The sync connection loader launcher task if it exists.
	/// </summary>
	private Task<bool> _SyncPayloadLauncher;

	/// <summary>
	/// The sync connection loader launcher process state, 0 = No process,
	/// 1 = launch queued, 2 = launch and/or payload Active.
	/// </summary>
	protected EnLauncherPayloadLaunchState _SyncPayloadLauncherLaunchState = EnLauncherPayloadLaunchState.Inactive;


	#endregion Fields and Constants




	// =========================================================================================================
	#region Property accessors - AbstruseRunningConnectionTable
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns true if the Async task is activ e else false.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private bool AsyncLoading => _AsyncPayloadLauncherLaunchState != EnLauncherPayloadLaunchState.Inactive
			&& _AsyncPayloadLauncherLaunchState != EnLauncherPayloadLaunchState.Shutdown
			&& _AsyncPayloadLauncher != null && !_AsyncPayloadLauncher.IsCompleted
			&& !_AsyncPayloadLauncherToken.IsCancellationRequested;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// This internal table storing all registered connections and datasources/servers.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public DataTable InternalConnectionsTable => _InternalConnectionsTable;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns true if the Rct is physically in a loading state and both sync and
	/// async tasks are not in Inactive or Shutdown states.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public bool Loading => _LoadingSyncCardinal > 0 || _LoadingAsyncCardinal > 0;


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - AbstruseRunningConnectionTable
	// =========================================================================================================


	private bool AdviseServerExplorerConnectionsEvents()
	{
		// Tracer.Trace(GetType(), "LoadServerExplorerConfiguredConnections()", "Executing Siting connections");

		IVsDataExplorerConnectionManager manager =
			(Controller.OleServiceProvider.QueryService<IVsDataExplorerConnectionManager>()
			as IVsDataExplorerConnectionManager)
		?? throw Diag.ExceptionService(typeof(IVsDataExplorerConnectionManager));

		Guid clsidProvider = new(SystemData.ProviderGuid);
		IVsDataExplorerConnection explorerConnection;


		foreach (KeyValuePair<string, IVsDataExplorerConnection> pair in manager.Connections)
		{
			if (pair.Value.Provider != clsidProvider)
				continue;

			explorerConnection = pair.Value;

			try
			{
				object viewSupport = Reflect.GetPropertyValue(explorerConnection, "ViewSupport",
					System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

				if (viewSupport == null)
				{
					COMException ex = new("IVsDataExplorerConnection ViewSupport is not ready");
					throw ex;
				}

				AdviseEvents(explorerConnection);
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
			}
		}

		return true;

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Appends a single row to the internal connections datatable and resets the
	/// exposed Databases and DataSources derived tables.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected bool AppendSingleConnectionRow(DataRow row)
	{
		// Tracer.Trace(GetType(), "AppendSingleConnectionRow()", "Adding row: {0}", row["DatasetKey"]);

		if (_Instance == null)
			return false;

		/*
		string str = "\nAdding registered Row: ";
		foreach (DataColumn col in _InternalConnectionsTable.Columns)
			str += col.ColumnName + ": " + row[col.ColumnName] + ", ";
		*/
		// Tracer.Trace(GetType(), "AppendSingleConnectionRow()", str);

		BeginLoadData(true);

		try
		{

			Invalidate();

			_InternalConnectionsTable.Rows.Add(row);

			return true;
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;
		}
		finally
		{
			EndLoadData();
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Launches an async task to perform explorer cnnection advise events if no async
	/// load occured.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public bool AsyncAdviseServerExplorerConnectionsEvents()
	{
		// Tracer.Trace(GetType(), "AsyncLoadConfiguredConnections()", "Probject? {0}, _LoadingAsyncCardinal: {1}.",
		//	probject == null ? "probject == null" : "probject != null", _LoadingAsyncCardinal);

		// Sanity checks.

		lock (_LockObject)
		{
			if (_InternalConnectionsTable == null)
				return false;
		}

		if (_AsyncPayloadLauncherLaunchState != EnLauncherPayloadLaunchState.Inactive)
		{
			// We create an awaiter which blocks entry so this should never happen.
			COMException exc = new($"Recursive call: Async Launch State: {_AsyncPayloadLauncherLaunchState}.", VSConstants.RPC_E_CANTCALLOUT_AGAIN);
			Diag.Dug(exc);
			throw exc;
		}


		// Prep the launcher.
		PrepAsyncPayloadLauncher();

		// The following for brevity.
		CancellationToken cancellationToken = _AsyncPayloadLauncherToken;
		TaskCreationOptions creationOptions = TaskCreationOptions.PreferFairness
			| TaskCreationOptions.AttachedToParent;
		TaskScheduler scheduler = TaskScheduler.Default;
		// For brevity.

		// For brevity. Create the payload.
		bool payload() =>
			PayloadAdviseServerExplorerConnectionsEvents();


		// Start up the payload launcher with tracking.
		// Tracer.Trace(GetType(), "AsyncAdviseServerExplorerConnectionsEvents()", "Calling startnew.");

		_AsyncPayloadLauncher?.Dispose();
		_AsyncPayloadLauncher = Task.Factory.StartNew(payload, default, creationOptions, scheduler);

		// Tracer.Trace(GetType(), "AsyncAdviseServerExplorerConnectionsEvents()", "Done called startnew.");

		// We may do this for the async task.
		// _TaskHandler.RegisterTask(_ASyncPayloadLauncher);


		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Launches an async task to perform unsafe loading of solution connections which
	/// require the DTE on the UI thread. To keep things tight this method will throw an exception if
	/// it's already on the UI thread and should not have been called.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public bool AsyncLoadConfiguredConnections(object probject)
	{
		// Tracer.Trace(GetType(), "AsyncLoadConfiguredConnections()", "Probject? {0}, _LoadingAsyncCardinal: {1}.",
		//	probject == null ? "probject == null" : "probject != null", _LoadingAsyncCardinal);

		if (!PersistentSettings.IncludeAppConnections)
			return false;

		// Sanity checks.

		lock (_LockObject)
		{
			if (_InternalConnectionsTable == null)
				return false;

			if (_LoadingAsyncCardinal > 0 && probject != null)
			{
				_Probjects.Add(probject);
				return true;
			}
		}

		if (_AsyncPayloadLauncherLaunchState != EnLauncherPayloadLaunchState.Inactive)
		{
			// We create an awaiter which blocks entry so this should never happen.
			COMException exc = new($"Recursive call: Async Launch State: {_AsyncPayloadLauncherLaunchState}.", VSConstants.RPC_E_CANTCALLOUT_AGAIN);
			Diag.Dug(exc);
			throw exc;
		}

		if (probject == null)
			Diag.ThrowIfOnUIThread();


		// Prep the launcher.
		PrepAsyncPayloadLauncher();

		// The following for brevity.
		CancellationToken cancellationToken = _AsyncPayloadLauncherToken;
		TaskCreationOptions creationOptions = TaskCreationOptions.PreferFairness
			| TaskCreationOptions.AttachedToParent;
		TaskScheduler scheduler = TaskScheduler.Default;
		// For brevity.

		// For brevity. Create the payload.
		bool payload() =>
			PayloadLoadUnsafeConfiguredConnections(cancellationToken, probject);


		// Start up the payload launcher with tracking.
		// Tracer.Trace(GetType(), "AsyncLoadConfiguredConnections()", "Calling startnew.");

		_AsyncPayloadLauncher?.Dispose();
		_AsyncPayloadLauncher = Task.Factory.StartNew(payload, default, creationOptions, scheduler);

		// Tracer.Trace(GetType(), "AsyncLoadConfiguredConnections()", "Done called startnew.");

		// We may do this for the async task.
		// _TaskHandler.RegisterTask(_ASyncPayloadLauncher);


		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Increments the load data cardinal and optionally clears internal tables.
	/// After increment and once the cardinal reaches zero using
	/// <see cref="EndLoadData"/> changes will be accepted in preparation for
	/// reconstructing public Rct tables.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected void BeginLoadData(bool resetTables)
	{
		if (resetTables)
		{
			if (_Databases != null)
				_Databases = null;

			if (_DataSources != null)
				_DataSources = null;
		}

		if (_LoadDataCardinal == 0)
			_InternalConnectionsTable?.BeginLoadData();

		_LoadDataCardinal++;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Clears the async payload launcher variables.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void ClearAsyncPayloadLauncher()
	{
		// Tracer.Trace(GetType(), "ClearAsyncPayloadLauncher()");

		lock (_LockObject)
		{
			_AsyncPayloadLauncherToken = default;
			_AsyncPayloadLauncherTokenSource?.Dispose();
			_AsyncPayloadLauncherTokenSource = null;
			_LoadingAsyncCardinal--;
			_AsyncPayloadLauncherLaunchState = EnLauncherPayloadLaunchState.Inactive;
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Clears the sync payload launcher variables except for the launch state.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void ClearSyncPayloadLauncher()
	{
		lock (_LockObject)
		{
			_SyncPayloadLauncher?.Dispose();
			_SyncPayloadLauncher = null;
			_SyncPayloadLauncherTokenSource?.Dispose();
			_SyncPayloadLauncherTokenSource = null;
			_SyncPayloadLauncherToken = default;
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Determines whether the rct contains an entry for the provided unique
	/// ConnectionUrl.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private bool ContainsConnectionUrlKey(string key)
	{
		if (key == null)
			return false;

		IEnumerable<DataRow> rows = _InternalConnectionsTable.Select().Where(x => key.Equals(x[CoreConstants.C_KeyExConnectionUrl]));

		return rows.Count() > 0;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Creates and initializes a configured connection row.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private DataRow CreateDataRow(CsbAgent csa = null)
	{
		DataRow row = _InternalConnectionsTable.NewRow();

		row["Id"] = _InternalConnectionsTable.Rows.Count;
		row[C_KeyExConnectionUrl] = "";

		UpdateDataRowFromCsa(row, csa);

		row.AcceptChanges();

		return row;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Creates the internal Rct DataTable.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected DataTable CreateDataTable()
	{
		DataTable dataTable = new();

		dataTable.Columns.Add("Id", typeof(int));
		dataTable.Columns.Add("Orderer", typeof(int));
		dataTable.Columns.Add(C_KeyExConnectionUrl, typeof(string));
		dataTable.Columns.Add("DataSourceLc", typeof(string));
		dataTable.Columns.Add("Name", typeof(string));
		dataTable.Columns.Add("DatabaseLc", typeof(string));
		dataTable.Columns.Add("DisplayName", typeof(string));

		foreach (Describer describer in CsbAgent.DescriberKeys)
			dataTable.Columns.Add(describer.Name, describer.DataType);

		dataTable.Columns.Add(C_KeyExConnectionString, typeof(string));

		dataTable.AcceptChanges();

		dataTable.PrimaryKey = [dataTable.Columns["Id"]];

		dataTable.AcceptChanges();

		return dataTable;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Generates a unique DatasetKey or DatasetId from the proposedConnectionName
	/// or proposedDatasetId. At most one may be specified, the other null. If both are
	/// null the readonly Dataset property will be used.
	/// <seealso cref="AbstractRunningConnectionTable.GenerateUniqueDatasetKey"/>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public abstract (bool, string, string, string, string) GenerateUniqueDatasetKey(string proposedConnectionName,
		string proposedDatasetId, string dataSource, string dataset, string connectionUrl, string originalConnectionUrl);



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Decrements the load data cardinal incremented by <see cref="BeginLoadData"/>
	/// and accepts changes once the cardinal reaches zero in preparation for
	/// reconstructing public Rct tables.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected void EndLoadData()
	{
		_LoadDataCardinal--;

		if (_LoadDataCardinal < 0)
		{
			InvalidOperationException ex = new($"EndLoadData() over called {-_LoadDataCardinal} times.");
			Diag.Dug(ex);
			return;
		}

		try
		{
			_InternalConnectionsTable?.AcceptChanges();

			if (_LoadDataCardinal == 0)
			{

				_InternalConnectionsTable?.EndLoadData();
			}
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
		}

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Increments the Rct seed after changes to the internal tables.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public long Invalidate()
	{
		return ++_Seed;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Loads ServerExplorer, FlameRobin and Application connection configurations from
	/// OnSolutionLoad, package initialization, ServerExplorer or any other applicable
	/// activation event.
	/// </summary>
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
	public bool LoadConfiguredConnections()
	{
		// Tracer.Trace(GetType(), "LoadConfiguredConnections()");

		if (Loading)
			return false;

		if (_Databases != null)
			return true;


		bool result = false;

		_LoadingSyncCardinal++;

		try
		{

			// There are a possible 2 tasks that will be launched.
			// 1. The synchronous task for loading ServerExplorer and FlameRobin connections
			//		and which does not care which thread we are on.
			// 2. A possible async task to load Application connections and which uses the
			//		dte, so will be created IF (1) is not on the UI thread.
			// For (1) we create a dummy awaitable which will be cancelled once (1) is complete.
			// If (2) is required we launch an async task which we fire and forget.

			CancellationToken cancellationToken = SyncEnter();


			// Load the SE and FlameRobin connections and possibly application connections.
			result = LoadSafeConfiguredConnections(cancellationToken);


			// Load the Application connections asynchronously if we're not on the main thread
			// with a fire and forget async task.

			if (PersistentSettings.IncludeAppConnections && !ThreadHelper.CheckAccess())
				AsyncLoadConfiguredConnections(null);
			else
				AsyncAdviseServerExplorerConnectionsEvents();

		}
		finally
		{
			if (_LoadingSyncCardinal == 1)
			{
				if (!SyncExit())
					result = false;
			}

			_LoadingSyncCardinal--;
		}

		return result;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Loads ServerExplorer and FlameRobin configured connections (and possibly
	/// Application connections) and performs registration synchronously.
	/// This is the first phase of loading. We don't care which thread we're on but
	/// it's initiated either on UICONTEXT.ShellInitialized and launched asynchronously
	/// or else on UICONTEXT.SolutionExists.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private bool LoadSafeConfiguredConnections(CancellationToken cancellationToken)
	{
		// Tracer.Trace(GetType(), "LoadSafeConfiguredConnections()");

		_InternalConnectionsTable = CreateDataTable();

		BeginLoadData(true);

		try
		{

			LoadServerExplorerConfiguredConnections();

			if (_Instance == null)
				return false;

			LoadUtilityConfiguredConnections();

			if (_Instance == null)
				return false;

			if (PersistentSettings.IncludeAppConnections && ThreadHelper.CheckAccess())
			{
				LoadSolutionConfiguredConnections(_SyncPayloadLauncherToken, true, null);
				if (_Instance == null)
					return false;
			}

			// Add a ghost row to the datasources list
			// This will be the default datasource row so that anything else
			// selected will generate a CurrentChanged event.
			DataRow row = CreateDataRow();

			row["DataSourceLc"] = "";
			row["Port"] = 0;
			row["Name"] = "";
			row["DatabaseLc"] = "";
			row["Orderer"] = 0;

			// string str = "AddGhostRow: ";
			// foreach (DataColumn col in _InternalConnectionsTable.Columns)
			// str += col.ColumnName + ": " + row[col.ColumnName] + ", ";
			// Tracer.Trace(GetType(), "LoadSafeConfiguredConnections()", str);


			AppendSingleConnectionRow(row);


			// Add a Clear/Reset dummy row for the datasources list
			// If selected will invoke a form reset the move the cursor back to the ghost row.
			row = CreateDataRow();

			row["Orderer"] = 1;
			row["DataSource"] = Resources.ErmBindingSource_Reset;
			row["DataSourceLc"] = Resources.ErmBindingSource_Reset.ToLowerInvariant();
			row["Name"] = "";
			row["Port"] = 0;
			row["DatabaseLc"] = "";

			// str = "AddResetRow: ";
			// foreach (DataColumn col in _InternalConnectionsTable.Columns)
			// str += col.ColumnName + ": " + row[col.ColumnName] + ", ";
			// Tracer.Trace(GetType(), "LoadSafeConfiguredConnections()", str);

			AppendSingleConnectionRow(row);


			// Add at least one row, that will be the ghost row, for localhost. 
			if (!_HasLocal)
			{
				row = CreateDataRow();

				row["Orderer"] = 2;
				row["DataSource"] = "localhost";
				row["DataSourceLc"] = "localhost";
				row["Name"] = "";
				row["DatabaseLc"] = "";

				// str = "AddLocalHostGhostRow: ";
				// foreach (DataColumn col in _InternalConnectionsTable.Columns)
				// str += col.ColumnName + ": " + row[col.ColumnName] + ", ";
				// Tracer.Trace(GetType(), "LoadSafeConfiguredConnections()", str);

				AppendSingleConnectionRow(row);
			}

			return true;
		}
		finally
		{
			EndLoadData();
		}

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Loads server explorer configured connections from the
	/// <see cref="IVsDataExplorerConnectionManager"/>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void LoadServerExplorerConfiguredConnections()
	{
		// Tracer.Trace(GetType(), "LoadServerExplorerConfiguredConnections()");

		IVsDataExplorerConnectionManager manager =
			(Controller.OleServiceProvider.QueryService<IVsDataExplorerConnectionManager>()
			as IVsDataExplorerConnectionManager)
			?? throw Diag.ExceptionService(typeof(IVsDataExplorerConnectionManager));

		Guid clsidProvider = new(SystemData.ProviderGuid);

		foreach (KeyValuePair<string, IVsDataExplorerConnection> pair in manager.Connections)
		{
			if (pair.Value.Provider != clsidProvider)
				continue;

			try
			{
				LoadServerExplorerConfiguredConnectionImpl(pair.Value.ConnectionNode.Name,
					DataProtection.DecryptString(pair.Value.EncryptedConnectionString));
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw;
			}
		}

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Loads a connection retrieved from the configured Server Explorer connections
	/// in the <see cref="IVsDataExplorerConnectionManager"/>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void LoadServerExplorerConfiguredConnectionImpl(string connectionName, string connectionString)
	{

		// Tracer.Trace(GetType(), "LoadServerExplorerConfiguredConnectionsImpl()", "Connection name: {0}", connectionName);

		string datasetId;
		CsbAgent csa;
		DataRow row;

		csa = new(connectionString)
		{
			ConnectionKey = connectionName
		};


		// Tracer.Trace(GetType(), "RegisterAppConnectionStrings()", "Updated csb datasource: {0}, serverName: {1}, connectionstring: {2}.", datasource, serverName, csa.ConnectionString);

		datasetId = csa.DatasetId;

		if (string.IsNullOrWhiteSpace(datasetId))
			datasetId = csa.Dataset;

		BeginLoadData(true);

		try
		{
			// The datasetId may not be unique at this juncture and already registered.
			row = RegisterUniqueConnectionImpl(connectionName, datasetId,
				EnConnectionSource.ServerExplorer, ref csa);

			if (row == null)
				return;

			// string str = "AddSERow: ";
			// foreach (DataColumn col in _InternalConnectionsTable.Columns)
			// str += col.ColumnName + ": " + row[col.ColumnName] + ", ";
			// Tracer.Trace(GetType(), "LoadServerExplorerConfiguredConnectionsImpl()", str);


			if (_Instance == null)
				return;


			// Tracer.Trace(GetType(), "LoadServerExplorerConfiguredConnectionImpl()", "Adding row: {0}", row["DatasetKey"]);
			AppendSingleConnectionRow(row);
		}
		finally
		{
			EndLoadData();
		}

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Recursively loads unique connections configured in the App.configs of a
	/// Solution's projects.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void LoadSolutionConfiguredConnections(CancellationToken cancellationToken, bool isSync, object probject)
	{
		// Tracer.Trace(GetType(), "LoadSolutionConfiguredConnections()");

		Diag.ThrowIfNotOnUIThread();


		if (probject == null && Controller.Instance.Dte.Solution.Projects.Count == 0)
			return;

		int count;
		Project current;

		lock (_LockObject)
		{
			if (probject != null)
			{
				_Probjects.Add(probject);
			}
			else
			{
				foreach (Project project in Controller.Instance.Dte.Solution.Projects)
					_Probjects.Add(project);
			}

			count = _Probjects.Count;
			current = (Project)_Probjects[0];
		}

		while (count > 0)
		{
			// Tracer.Trace(GetType(), "LoadSolutionConfiguredConnections()", "Scanning project: {0}.", project.Name);
			try
			{
				RecursiveScanProject(current);
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw ex;
			}

			if ((!isSync && (_AsyncPayloadLauncher == null || _AsyncPayloadLauncher.IsCompleted))
				|| (isSync && (_SyncPayloadLauncher == null || _SyncPayloadLauncher.IsCompleted))
				|| cancellationToken.IsCancellationRequested)
			{
				lock (_LockObject)
					_Probjects.Clear();
				return;
			}

			// Tracer.Trace(GetType(), "LoadSolutionConfiguredConnections()", "Completed Scanning project: {0}.", project.Name);

			lock (_LockObject)
			{
				_Probjects.RemoveAt(0);

				count = _Probjects.Count;

				if (count == 0)
					break;

				current = (Project)_Probjects[0];
			}
		}
		
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Loads FlameRobin configured connections.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void LoadUtilityConfiguredConnections()
	{
		// Tracer.Trace(GetType(), "LoadUtilityConfiguredConnections()");

		DataRow row;

		string xmlPath = SystemData.UtilityConfigurationPath;

		if (!File.Exists(xmlPath))
			return;

		BeginLoadData(true);

		try
		{
			XmlDocument xmlDoc = new XmlDocument();

			xmlDoc.Load(xmlPath);

			XmlNode xmlRoot = xmlDoc.DocumentElement;
			/* XmlNamespaceManager xmlNs = new XmlNamespaceManager(xmlDoc.NameTable);


			if (!xmlNs.HasNamespace("confBlackbirdNs"))
			{
				xmlNs.AddNamespace("confBlackbirdNs", xmlRoot.NamespaceURI);
			}
			*/
			XmlNodeList xmlServers, xmlDatabases;
			XmlNode xmlNode = null;
			CsbAgent csa;
			int port;
			string serverName, datasource, authentication, user, password, path, charset;
			string datasetId;

			xmlServers = xmlRoot.SelectNodes("//server");


			foreach (XmlNode xmlServer in xmlServers)
			{
				if ((xmlNode = xmlServer.SelectSingleNode("name")) == null)
					continue;
				serverName = xmlNode.InnerText.Trim();

				if ((xmlNode = xmlServer.SelectSingleNode("host")) == null)
					continue;
				datasource = xmlNode.InnerText.Trim();


				if ((xmlNode = xmlServer.SelectSingleNode("port")) == null)
					continue;
				port = Convert.ToInt32(xmlNode.InnerText.Trim());

				if (port == 0)
					port = CoreConstants.C_DefaultPort;


				// To keep uniformity of server names, the case of the first connection
				// discovered for a server name is the case that will be used for all
				// connections for that server.
				serverName = RegisterServer(datasource, port);

				xmlDatabases = xmlServer.SelectNodes("database");

				foreach (XmlNode xmlDatabase in xmlDatabases)
				{
					if ((xmlNode = xmlDatabase.SelectSingleNode("name")) == null)
						continue;


					datasetId = xmlNode.InnerText.Trim();

					if ((xmlNode = xmlDatabase.SelectSingleNode("path")) == null)
						continue;

					path = xmlNode.InnerText.Trim();

					if ((xmlNode = xmlDatabase.SelectSingleNode("charset")) == null)
						continue;

					charset = xmlNode.InnerText.Trim();

					user = "";
					password = "";

					if ((xmlNode = xmlDatabase.SelectSingleNode("authentication")) == null)
						authentication = "trusted";
					else
						authentication = xmlNode.InnerText.Trim();

					if (authentication != "trusted")
					{
						if ((xmlNode = xmlDatabase.SelectSingleNode("username")) != null)
						{
							user = xmlNode.InnerText.Trim();

							if (authentication == "pwd"
								&& (xmlNode = xmlDatabase.SelectSingleNode("password")) != null)
							{
								password = xmlNode.InnerText.Trim();
							}
						}

					}

					// Tracer.Trace(GetType(), "LoadConfiguredConnectionsImpl()", "Calling RegisterDatasetKey for datasetId: {0}.", datasetId);

					// The datasetId may not be unique at this juncture and already registered.
					csa = new(serverName, port, path, user, password, charset);

					row = RegisterUniqueConnectionImpl(null, datasetId,
						EnConnectionSource.ExternalUtility, ref csa);

					if (row == null)
						continue;


					if (_Instance == null)
						return;

					// Tracer.Trace(GetType(), "LoadUtilityConfiguredConnections()", "Adding row: {0}", row["DatasetKey"]);
					AppendSingleConnectionRow(row);
				}
			}

		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
		}
		finally
		{
			EndLoadData();
		}

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Loads explorer advise connections on thread pool.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private bool PayloadAdviseServerExplorerConnectionsEvents()
	{
		// Tracer.Trace(GetType(), "PayloadAdviseServerExplorerConnectionsEvents()");

		// Sanity check.
		Diag.ThrowIfOnUIThread();

		bool result = true;

		try
		{
			_AsyncPayloadLauncherLaunchState = EnLauncherPayloadLaunchState.Launching;
			AdviseServerExplorerConnectionsEvents();
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			ClearAsyncPayloadLauncher();
			throw ex;
		}

		ClearAsyncPayloadLauncher();

		return result;

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Searches for application configured connections and performs registration
	/// asynchronously.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private bool PayloadLoadUnsafeConfiguredConnections(CancellationToken cancellationToken, object probject)
	{
		return ThreadHelper.JoinableTaskFactory.Run(() => PayloadLoadUnsafeConfiguredConnectionsAsync(cancellationToken, probject));
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Searches for application configured connections and performs registration
	/// asynchronously.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private async Task<bool> PayloadLoadUnsafeConfiguredConnectionsAsync(CancellationToken cancellationToken, object probject)
	{
		// Tracer.Trace(GetType(), "PayloadLoadUnsafeConfiguredConnectionsAsync()");

		bool result = true;

		try
		{

			if (cancellationToken.IsCancellationRequested)
			{
				result = false;
			}
			else
			{

				_AsyncPayloadLauncherLaunchState = EnLauncherPayloadLaunchState.Launching;

				// We must be off of the ui thread here
				Diag.ThrowIfOnUIThread();


				AdviseServerExplorerConnectionsEvents();


				// Now onto main thread.
				await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();


				if (probject == null && (Controller.Instance.Dte == null || Controller.Instance.Dte.Solution == null
					|| Controller.Instance.Dte.Solution.Projects == null))
				{
					COMException exc = new("DTE.Solution.Projects is not available", VSConstants.RPC_E_INVALID_DATA);
					Diag.Dug(exc);
					throw exc;
				}

				// Build an object list of top level projects for the solution, then launch a non-block thread on
				// the thread pool that will recursively call an async method that switches back to the ui
				// thread to register connections for each project.


				// Tracer.Trace(GetType(), "PayloadLoadUnsafeConfiguredConnectionsAsync()", "Calling LoadSolutionConfiguredConnections(), _AsyncPayloadLauncherToken.IsCancellationRequested: {0}.", _AsyncPayloadLauncherToken.IsCancellationRequested);


				BeginLoadData(true);

				try
				{
					LoadSolutionConfiguredConnections(_AsyncPayloadLauncherToken, false, probject);

					if (_Instance == null || _AsyncPayloadLauncherLaunchState == EnLauncherPayloadLaunchState.Shutdown)
						result = false;

					// finally{} just will not work here. This must have something to do with how
					// this async task was formed.
				}
				catch
				{
					EndLoadData();
					throw;
				}

				EndLoadData();
			}

			// finally{} just will not work here. This must have something to do with how
			// this async task was formed.
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			ClearAsyncPayloadLauncher();
			throw ex;
		}

		ClearAsyncPayloadLauncher();

		return result;

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Executes the sync configuration loader's wait task.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private bool PayloadSyncWaiter(CancellationToken cancellationToken)
	{
		// Tracer.Trace(GetType(), "SyncPayloadTask()");

		if (_SyncPayloadLauncher == null || cancellationToken.IsCancellationRequested)
			return false;

		_SyncPayloadLauncherLaunchState = EnLauncherPayloadLaunchState.Launching;

		while (!cancellationToken.IsCancellationRequested)
		{
			System.Threading.Thread.Sleep(50);
		}

		if (_SyncPayloadLauncherLaunchState == EnLauncherPayloadLaunchState.Shutdown)
			return false;

		_SyncPayloadLauncherLaunchState = EnLauncherPayloadLaunchState.Inactive;

		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Preps the async payload launcher variables for launch.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void PrepAsyncPayloadLauncher()
	{
		lock (_LockObject)
		{
			_LoadingAsyncCardinal++;
			_AsyncPayloadLauncherLaunchState = EnLauncherPayloadLaunchState.Pending;
			_AsyncPayloadLauncherToken = default;
			_AsyncPayloadLauncherTokenSource?.Dispose();
			_AsyncPayloadLauncherTokenSource = new();
			_AsyncPayloadLauncherToken = _AsyncPayloadLauncherTokenSource.Token;
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Preps the sync payload launcher variables for launch.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void PrepSyncPayloadLauncher()
	{
		lock (_LockObject)
		{
			_SyncPayloadLauncherLaunchState = EnLauncherPayloadLaunchState.Pending;
			_SyncPayloadLauncherTokenSource?.Dispose();
			_SyncPayloadLauncherTokenSource = new();
			_SyncPayloadLauncherToken = _SyncPayloadLauncherTokenSource.Token;
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Recursively scans a project already opened before our package was sited for
	/// configured connections in the App.Config.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void RecursiveScanProject(Project project)
	{
		Diag.ThrowIfNotOnUIThread();

		ProjectItem config = null;

		// There's a dict list of these at the end of the class
		if (UnsafeCmd.Kind(project.Kind) == "ProjectFolder")
		{
			if (project.ProjectItems != null && project.ProjectItems.Count > 0)
			{
				// Diag.Trace("Recursing ProjectFolder: " + project.Name);
				RecursiveScanProject(project.ProjectItems);
			}
			/*
			else
			{
				// Diag.Trace("No items in ProjectFolder: " + project.Name);
			}
			*/
		}
		else
		{
			// Diag.Trace("Recursive validate project: " + project.Name);

			if (UnsafeCmd.IsValidExecutableProjectType(project, false))
			{

				// VSProject projectObject = project.Object as VSProject;


				config ??= UnsafeCmd.GetAppConfigProjectItem(project);
				if (config != null)
					ScanAppConfig(config);
			}

		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Recursively scans projects already opened before our package was sited for
	/// configured connections in the App.Config.
	/// This list is tertiary level projects from parent projects (solution folders).
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void RecursiveScanProject(ProjectItems projectItems)
	{
		Diag.ThrowIfNotOnUIThread();

		foreach (ProjectItem projectItem in projectItems)
		{
			if (projectItem.SubProject != null)
			{
				RecursiveScanProject(projectItem.SubProject);
			}
			else
			{
				// Diag.Trace(projectItem.Name + " projectItem.SubProject is null (Possible Unloaded project or document)");
			}
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Scans an App.Config for configured connections and loads them into the rct.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void RegisterAppConnectionStrings(string projectName, string xmlPath)
	{
		// Tracer.Trace(GetType(), "RegisterAppConnectionStrings()");

		XmlDocument xmlDoc = new XmlDocument();

		try
		{
			xmlDoc.Load(xmlPath);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			return;
		}

		BeginLoadData(true);

		try
		{
			XmlNode xmlRoot = xmlDoc.DocumentElement;
			XmlNamespaceManager xmlNs = new XmlNamespaceManager(xmlDoc.NameTable);

			if (!xmlNs.HasNamespace("confBlackbirdNs"))
			{
				xmlNs.AddNamespace("confBlackbirdNs", xmlRoot.NamespaceURI);
			}


			// For anyone watching, you have to denote your private namespace after every forwardslash in
			// the markup tree.
			// Q? Does this mean you can use different namespaces within the selection string?
			XmlNode xmlNode = null, xmlParent;
			string datasetId;
			string[] arr;
			CsbAgent csa;
			DataRow row;

			xmlNode = xmlRoot.SelectSingleNode("//confBlackbirdNs:connectionStrings", xmlNs);

			if (xmlNode == null)
				return;

			xmlParent = xmlNode;


			XmlNodeList xmlNodes = xmlParent.SelectNodes($"confBlackbirdNs:add[@providerName='System.Data.EntityClient']", xmlNs);

			if (xmlNodes.Count > 0)
			{

				string name;
				string connectionString;
				DbConnectionStringBuilder csb;

				foreach (XmlNode connectionNode in xmlNodes)
				{
					name = connectionNode.Attributes["name"].Value;
					datasetId = Resources.RunningConnectionTableEdmDataset.FmtRes(projectName, name);

					csb = new()
					{
						ConnectionString = connectionNode.Attributes["connectionString"].Value
					};

					if (!csb.ContainsKey("provider") || !((string)csb["provider"]).Equals(SystemData.Invariant, StringComparison.InvariantCultureIgnoreCase))
						continue;

					connectionString = csb.ContainsKey("provider connection string")
						? (string)csb["provider connection string"] : null;

					if (string.IsNullOrWhiteSpace(connectionString))
						continue;

					// Tracer.Trace(GetType(), "RegisterAppConnectionStrings()", "Entity connectionString: {0}", connectionString);

					csa = new(connectionString);


					// The datasetId may not be unique at this juncture and already registered.
					row = RegisterUniqueConnectionImpl(null, datasetId,
						EnConnectionSource.EntityDataModel, ref csa);

					if (row == null)
						continue;

					// string str = "AddAppDbRow: ";
					// foreach (DataColumn col in _InternalConnectionsTable.Columns)
					// str += col.ColumnName + ": " + row[col.ColumnName] + ", ";
					// Tracer.Trace(GetType(), "RegisterAppConnectionStrings()", str);


					if (_Instance == null)
						return;

					// Tracer.Trace(GetType(), "RegisterAppConnectionStrings()", "Adding row: {0}", row["DatasetKey"]);
					AppendSingleConnectionRow(row);

				}
			}




			xmlNodes = xmlParent.SelectNodes($"confBlackbirdNs:add[@providerName='{SystemData.Invariant}']", xmlNs);

			if (xmlNodes.Count == 0)
				return;

			foreach (XmlNode connectionNode in xmlNodes)
			{
				arr = connectionNode.Attributes["name"].Value.Split('.');
				datasetId = Resources.RunningConnectionTableProjectDatasetId.FmtRes(projectName, arr[^1]);

				csa = new(connectionNode.Attributes["connectionString"].Value);


				// Tracer.Trace(GetType(), "RegisterAppConnectionStrings()", "datasource: {0}, dataset: {1}, serverName: {2}, ConnectionName: {3}, datasetId: {4}, Connectionstring: {5}, storedConnectionString: {6}.", datasource, csa.Dataset, serverName, csa.ConnectionName, datasetId, csa.ConnectionString, connectionNode.Attributes["connectionString"].Value);

				// The datasetId may not be unique at this juncture and already registered.
				row = RegisterUniqueConnectionImpl(null, datasetId,
					EnConnectionSource.Application, ref csa);

				if (row == null)
					continue;

				// string str = "AddAppDbRow: ";
				// foreach (DataColumn col in _InternalConnectionsTable.Columns)
				// str += col.ColumnName + ": " + row[col.ColumnName] + ", ";
				// Tracer.Trace(GetType(), "RegisterAppConnectionStrings()", str);


				if (_Instance == null)
					return;

				// Tracer.Trace(GetType(), "RegisterAppConnectionStrings()", "Adding row: {0}", row["DatasetKey"]);
				AppendSingleConnectionRow(row);

			}


		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;
		}
		finally
		{
			EndLoadData();
		}

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns a uniformly cased Server/DataSource name of the provided name.
	/// To maintain uniformity of server names, the case of the first connection
	/// discovered for a server name is the case that will be used for all future
	/// connections for that server.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public string RegisterServer(string serverName, int port)
	{

		if (serverName.ToLowerInvariant() == "localhost")
		{
			serverName = "localhost";
			_HasLocal = true;
		}

		string search = serverName.ToLower();

		DataRow[] rows = _InternalConnectionsTable.Select()
				.Where(x => search.Equals(x["DataSourceLc"])
					&& string.IsNullOrWhiteSpace((string)x["DatabaseLc"])).ToArray();

		if (rows.Length > 0)
			return (string)rows[0]["DataSource"];

		BeginLoadData(true);

		try
		{

			DataRow row = CreateDataRow();

			row["DataSource"] = serverName;
			row["DataSourceLc"] = serverName.ToLower();
			row["Port"] = port;
			row["Name"] = "";
			row["DatabaseLc"] = "";

			if (serverName == "localhost")
				row["Orderer"] = 2;
			else
				row["Orderer"] = 3;

			// string str = "AddServerRow: ";
			// foreach (DataColumn col in _InternalConnectionsTable.Columns)
			//	str += col.ColumnName + ": " + row[col.ColumnName] + ", ";
			// Tracer.Trace(GetType(), "AddServerRow()", str);

			AppendSingleConnectionRow(row);

			return serverName;

		}
		finally
		{
			EndLoadData();
		}

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Builds a uniquely identifiable connection url and registers it's unique
	/// DatasetKey in the form 'Server (DatasetId)' or the supplied proposed DatasetKey.
	/// The minimum property requirement for a dataset to be registered is DataSource,
	/// Database and UserID.
	/// </summary>
	/// <returns>
	/// True if successful else false.
	/// </returns>
	// ---------------------------------------------------------------------------------
	public bool RegisterUniqueConnection(string proposedDatasetKey,
		string proposedDatasetId, EnConnectionSource source, ref CsbAgent csa)
	{

		if (_Instance == null)
			return false;

		BeginLoadData(true);

		try
		{
			DataRow row = RegisterUniqueConnectionImpl(proposedDatasetKey, proposedDatasetId, source, ref csa);

			if (row == null)
				return false;

			AppendSingleConnectionRow(row);
			csa.UpdateValidationRctState();
			return true;
		}
		finally
		{
			EndLoadData();
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Builds a uniquely identifiable connection url and registers it's unique
	/// DatasetKey in the form 'Server (DatasetId)' or the supplied proposed DatasetKey.
	/// The minimum property requirement for a dataset to be registered is DataSource,
	/// Database and UserID.
	/// </summary>
	/// <returns>
	/// The new DataRow of the unique connection.
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
	private DataRow RegisterUniqueConnectionImpl(string proposedConnectionName,
		string proposedDatasetId, EnConnectionSource source, ref CsbAgent csa)
	{
		// Tracer.Trace(GetType(), "RegisterUniqueConnectionDatsetKey()");

		if (string.IsNullOrWhiteSpace(proposedDatasetId))
		{
			ArgumentNullException ex = new ArgumentNullException("proposedDatasetId may not be null.");
			Diag.Dug(ex);
			throw ex;
		}


		// The only way to get or register a connection configuration is through a
		// unique connection url, which requires at a minimum DataSource, Database and
		// UserID.

		string connectionUrl = csa.SafeDatasetMoniker;

		// Sanity check.
		if (connectionUrl == null)
			return null;

		// If the unique url exists return null to indicate "not done", otherwise create a new one,
		// numbering it's datasetid and or datasetkey if duplicates exist.
		if (ContainsConnectionUrlKey(connectionUrl))
			return null;

		// Also register it with the next available DatasetKey and ConnectionName.

		BeginLoadData(true);

		try
		{
			// Sanity checks.

			// Take the glyph out of Application and EntityDataModel source dataset ids if the
			// new source is not Application or EntityDataModel.
			// Globalized values must always have the glyph on the left irrespective of ltr or rtl.
			if (source != EnConnectionSource.Application && source != EnConnectionSource.EntityDataModel
				&& (proposedDatasetId[0] == RctManager.EdmDatasetGlyph
				|| proposedDatasetId[0] == RctManager.ProjectDatasetGlyph))
			{
				proposedDatasetId = proposedDatasetId[1..];
			}

			(bool newConnection, string uniqueDatasetKey, string uniqueConnectionName, string uniqueDatasetId, _) =
				GenerateUniqueDatasetKey(proposedConnectionName, proposedDatasetId, csa.DataSource, csa.Dataset, connectionUrl, null);

			csa.DatasetKey = uniqueDatasetKey;

			if (uniqueConnectionName == string.Empty)
				csa.Remove(CoreConstants.C_KeyExConnectionName);
			else if (uniqueConnectionName != null)
				csa.ConnectionName = uniqueConnectionName;
			else if (proposedConnectionName != null)
				csa.ConnectionName = proposedConnectionName;

			if (uniqueDatasetId == string.Empty)
				csa.Remove(CoreConstants.C_KeyExDatasetId);
			else if (uniqueDatasetId != null)
				csa.DatasetId = uniqueDatasetId;
			else if (proposedDatasetId != null)
				csa.DatasetId = proposedDatasetId;

			int id = _InternalConnectionsTable.Rows.Count;

			// Add the generated DatasetKey to the list of internal unique keys (synonyms).
			Add(uniqueDatasetKey, id);



			// Sanity check
			csa.ConnectionSource = (EnConnectionSource)(int)source;
			if (source == EnConnectionSource.ServerExplorer && string.IsNullOrWhiteSpace(csa.ConnectionKey))
			{
				csa.ConnectionKey = csa.DatasetKey;
			}


			// Legacy removal
			csa.Remove("externalkey");


			// Tracer.Trace(GetType(), "RegisterUniqueDatasetKey()", "\nADDED uniqueDatasetKey: {0}, source: {1}, dataset: {2}, connectionName: {3}, uniqueDatasetId: {4}, \n\tConnectionString: {5}.", uniqueDatasetKey, source, Dataset, proposedDatasetKey, proposedDatasetId, ConnectionString);

			DataRow row = CreateDataRow(csa);

			row["Name"] = csa.DatasetId;
			row["DataSourceLc"] = csa.DataSource.ToLower();
			row["DatabaseLc"] = csa.Database.ToLower();
			row[C_KeyExConnectionUrl] = connectionUrl;
			row[C_KeyExConnectionString] = csa.ConnectionString;


			if (csa.DataSource.ToLowerInvariant() == "localhost")
				row["Orderer"] = 2;
			else
				row["Orderer"] = 3;

			return row;
		}
		finally
		{
			EndLoadData();
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Removes a connection given the ConnectionUrl or DatasetKey
	/// or DatsetKey synonym.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override bool Remove(string key)
	{
		if (key == null || _InternalConnectionsTable == null)
			return false;


		if (!TryGetHybridInternalRowValue(key, out DataRow row))
			return false;


		int id = Convert.ToInt32(row["Id"]);

		foreach (KeyValuePair<string, int> pair in this)
		{
			if (pair.Value == id)
				RemoveEntry(pair.Key);
		}

		BeginLoadData(true);

		_InternalConnectionsTable.Rows.Remove(row);

		Invalidate();

		EndLoadData();

		return true;

	}

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Checks if the app.config exists and then executes a scan.
	/// </summary>
	/// <param name="project"></param>
	/// <returns>
	/// true if completed successfully else false if there were errors.
	/// </returns>
	// ---------------------------------------------------------------------------------
	private bool ScanAppConfig(ProjectItem appConfig)
	{
		Diag.ThrowIfNotOnUIThread();

		try
		{
			if (appConfig.FileCount == 0)
				return false;

			string configFile = appConfig.FileNames[0];

			RegisterAppConnectionStrings(appConfig.ContainingProject.Name, configFile);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			return false;
		}


		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Preps and enters/launches the sync Rct loading task.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private CancellationToken SyncEnter()
	{
		// Prep the launcher.
		PrepSyncPayloadLauncher();


		// The following for brevity.
		CancellationToken cancellationToken = _SyncPayloadLauncherToken;
		TaskCreationOptions creationOptions = TaskCreationOptions.PreferFairness | TaskCreationOptions.AttachedToParent;
		TaskScheduler scheduler = TaskScheduler.Default;

		// For brevity. Create the payload.
		bool payload() =>
			PayloadSyncWaiter(cancellationToken);

		// Start up the payload launcher with tracking.
		_SyncPayloadLauncher = Task.Factory.StartNew(payload, default, creationOptions, scheduler);

		// We're not doing this for a sync task.
		// _TaskHandler.RegisterTask(_SyncPayloadLauncher);

		// Tracer.Trace(GetType(), "LoadConfiguredConnections()", "SyncPayloadTask created. Calling LoadConfiguredConnectionsImpl()");

		return cancellationToken;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs a graceful termination of the sync Rct loading task.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private bool SyncExit()
	{
		_SyncPayloadLauncherTokenSource.Cancel();

		System.Threading.Thread.Sleep(50);

		// Kill SyncPayloadTask.

		// Tracer.Trace(GetType(), "LoadConfiguredConnections()", "Killing SyncPayloadTask. Loading sync done. State: {0}",
		//	_SyncPayloadLauncherLaunchState);

		ClearSyncPayloadLauncher();


		if (_SyncPayloadLauncherLaunchState == EnLauncherPayloadLaunchState.Shutdown)
		{
			_LoadingSyncCardinal = 0;
			return false;
		}

		_SyncPayloadLauncherLaunchState = EnLauncherPayloadLaunchState.Inactive;

		return true;

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Get the internal connection data row given the ConnectionUrl, ConnectionString,
	/// DatasetKey or DatasetKey synonym.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected bool TryGetHybridInternalRowValue(string hybridKey, out DataRow value)
	{
		if (hybridKey == null || _InternalConnectionsTable == null)
		{
			value = null;
			return false;
		}

		bool isConnectionUrl = hybridKey.StartsWith(_Scheme);

		if (!isConnectionUrl &&
			(hybridKey.StartsWith("data source=", StringComparison.InvariantCultureIgnoreCase)
			|| hybridKey.ToLowerInvariant().Contains(";data source=")))
		{
			hybridKey = CsbAgent.CreateConnectionUrl(hybridKey);
			isConnectionUrl = true;
		}

		if (isConnectionUrl)
		{
			DataRow[] rows = _InternalConnectionsTable.Select().Where(x => hybridKey.Equals(x[CoreConstants.C_KeyExConnectionUrl])).ToArray();

			value = rows.Length > 0 ? rows[0] : null;
		}
		else
		{
			if (TryGetEntry(hybridKey, out int id))
			{
				value = _InternalConnectionsTable.Rows.Find(id);
			}
			else
			{
				value = null;
			}
		}

		return value != null;

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// De-links server explorer configured connections from the Rct on Dispose.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void UnloadServerExplorerConfiguredConnections()
	{
		// Tracer.Trace(GetType(), "UnloadServerExplorerConfiguredConnections()");

		IVsDataExplorerConnectionManager manager =
			(Controller.OleServiceProvider.QueryService<IVsDataExplorerConnectionManager>()
			as IVsDataExplorerConnectionManager)
			?? throw Diag.ExceptionService(typeof(IVsDataExplorerConnectionManager));

		Guid clsidProvider = new(SystemData.ProviderGuid);


		foreach (KeyValuePair<string, IVsDataExplorerConnection> pair in manager.Connections)
		{
			if (pair.Value.Provider != clsidProvider)
				continue;

			UnadviseEvents(pair.Value);
		}

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Updates an rct row given a csa. csa may be null.
	/// </summary>
	/// <returns>True if the row was updated else false.</returns>
	// ---------------------------------------------------------------------------------
	protected bool UpdateDataRowFromCsa(DataRow row, CsbAgent csa = null)
	{
		bool updated = false;
		object csaValue;
		object rowValue;
		object originalRowValue;
		string displayName;

		try
		{
			row[C_KeyExDisplayName] = DBNull.Value;

			foreach (Describer describer in CsbAgent.DescriberKeys)
			{
				csaValue = csa != null && csa.ContainsKey(describer.Name)
					? csa[describer.Name]
					: describer.DefaultValue;

				if (csa != null && describer.Name == C_KeyExDatasetId)
				{
					if (csaValue == describer.DefaultValue)
						csaValue = csa.Dataset;

					// Update the database (datasetId) dropdown field.
					// If there's a connection name the datasetId will mean
					// nothing without the connection name and may produce duuplicates,
					// so prefix it with a qualifier.
					if (!string.IsNullOrWhiteSpace(csa.ConnectionName))
						displayName = csa.ConnectionName + " | " + csaValue;
					else
						displayName = (string)csaValue;

					if (!updated)
					{
						BeginLoadData(true);
						row.BeginEdit();
					}

					updated = true;
					row[C_KeyExDisplayName] = displayName;
				}

				originalRowValue = row[describer.Name];
				rowValue = originalRowValue == DBNull.Value ? null : row[describer.Name];


				if (csaValue != null)
				{
					if (rowValue == null || !rowValue.Equals(csaValue))
					{
						if (!updated)
						{
							BeginLoadData(true);
							row.BeginEdit();
						}
						updated = true;
						row[describer.Name] = csaValue;
					}
					continue;
				}

				// The csa value is null.

				if (rowValue != null)
				{
					if (!updated)
					{
						BeginLoadData(true);
						row.BeginEdit();
					}
					updated = true;
					row[describer.Name] = DBNull.Value;
					continue;
				}

				// Row value is null. Finally check if it's original value is null.
				if (originalRowValue == null)
				{
					if (!updated)
					{
						BeginLoadData(true);
						row.BeginEdit();
					}
					updated = true;
					row[describer.Name] = DBNull.Value;
				}
			}

			return updated;
		}
		finally
		{
			if (updated)
			{
				row.EndEdit();
				row.AcceptChanges();
				EndLoadData();
			}
		}
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Attempts to update a registered connection if it exists else returns null.
	/// <seealso cref="AbstractRunningConnectionTable.UpdateRegisteredConnection"/>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public abstract CsbAgent UpdateRegisteredConnection(string connectionString,
		EnConnectionSource source, bool forceOwnership);




	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The awaiter for unsafe loading of solution connections that use the DTE and were
	/// launched by <see cref="AsyncLoadConfiguredConnections"/> .
	/// </summary>
	/// <remarks>
	/// This may take some time if the ide is starting up, followed in quick succession
	/// with opening a solution and expanding an SE node.
	/// </remarks>
	// ---------------------------------------------------------------------------------
	public void WaitForAsyncLoad()
	{
		// Tracer.Trace(GetType(), "WaitForAsyncLoadConfiguredConnections()");

		int waitTime = 0;

		while (AsyncLoading)
		{
			if (waitTime >= 15000)
			{
				TimeoutException ex = new($"Timed out waiting for AsyncLoadConfiguredConnections() to complete. Timeout (ms): {waitTime}.");
				Diag.Dug(ex);
				throw ex;
			}

			// Tracer.Trace(GetType(), "WaitForAsyncLoadConfiguredConnections()", "WAITING");

			try
			{
				_AsyncPayloadLauncher.Wait(100, _AsyncPayloadLauncherToken);
			}
			catch { }

			waitTime += 100;
		}

		// Tracer.Trace(GetType(), "WaitForAsyncLoadConfiguredConnections()", "DONE WAITING");

	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The dummy awaiter for loading of safe preconfigured connections that was
	/// launched in <see cref="SyncPayloadTask"/>.
	/// </summary>
	/// This may take some time if the ide is starting up, followed in quick succession
	/// with opening a solution and expanding an SE node.
	// ---------------------------------------------------------------------------------
	public bool WaitForSyncLoad()
	{
		// Tracer.Trace(GetType(), "WaitForAsyncLoadConfiguredConnections()");

		if (!Loading)
			return true;

		if (_Databases != null)
			return true;

		int waitTime = 0;

		while (_SyncPayloadLauncherLaunchState != EnLauncherPayloadLaunchState.Inactive
			&& _SyncPayloadLauncherLaunchState != EnLauncherPayloadLaunchState.Shutdown
			&& _SyncPayloadLauncher != null && !_SyncPayloadLauncher.IsCompleted
			&& !_SyncPayloadLauncherToken.IsCancellationRequested)
		{
			if (waitTime >= 15000)
			{
				TimeoutException ex = new($"Timed out waiting for LoadConfiguredConnections() to complete. Timeout (ms): {waitTime}.");
				Diag.Dug(ex);
				throw ex;
			}

			try
			{
				_SyncPayloadLauncher.Wait(100, _SyncPayloadLauncherToken);
			}
			catch { }

			waitTime += 100;
		}

		if (_SyncPayloadLauncherLaunchState == EnLauncherPayloadLaunchState.Shutdown)
			return false;


		return true;
	}



	#endregion Methods





	// =========================================================================================================
	#region Event Handling - AbstruseRunningConnectionTable
	// =========================================================================================================




	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Attaches to a Server Explorer connection's events.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public void AdviseEvents(IVsDataExplorerConnection explorerConnection)
	{
		explorerConnection.NodeChanged += OnExplorerConnectionNodeChanged;
		explorerConnection.NodeRemoving += OnExplorerConnectionNodeRemoving;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Increments the <see cref="EventsDisabled"/> counter when execution updates the
	/// SE to prevent recursion.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public void DisableEvents()
	{
		_EventsCardinal++;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Decrements the <see cref="EventsDisabled"/> counter that was previously
	/// incremented by <see cref="DisableEvents"/>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public void EnableEvents()
	{
		if (_EventsCardinal == 0)
			Diag.Dug(new InvalidOperationException(Resources.ExceptionEventsAlreadyEnabled));
		else
			_EventsCardinal--;
	}



	/// <summary>
	/// Node renames occur here.
	/// </summary>
	private void OnExplorerConnectionNodeChanged(object sender, DataExplorerNodeEventArgs e)
	{
		if (_Instance != null && _EventsCardinal != 0)
			return;

		// Tracer.Trace(GetType(), "OnExplorerConnectionNodeChanged()");

		if (e.Node.IsExpanding || e.Node.IsRefreshing || e.Node.ExplorerConnection == null
			|| e.Node != e.Node.ExplorerConnection.ConnectionNode)
		{
			return;
		}

		if (_Instance == null)
		{
			UnadviseEvents(e.Node.ExplorerConnection);
			return;
		}

		IVsDataObject @object = e.Node.Object;

		string datasetKey = @object != null && @object.Properties != null
			&& @object.Properties.ContainsKey(CoreConstants.C_KeyExDatasetKey)
			? (string)@object.Properties[CoreConstants.C_KeyExDatasetKey] : null;

		if (datasetKey != null && datasetKey == e.Node.ExplorerConnection.DisplayName)
			return;

		// Tracer.Trace(GetType(), "OnExplorerConnectionNodeChanged()", "Renaming");


		RctManager.ValidateAndUpdateExplorerConnectionRename(e.Node.ExplorerConnection, e.Node.ExplorerConnection.DisplayName);
	}


	public void OnExplorerConnectionNodeRemoving(object sender, DataExplorerNodeEventArgs e)
	{
		if (_Instance != null && _EventsCardinal != 0)
			return;

		// Tracer.Trace(GetType(), "OnExplorerConnectionNodeRemoving()", "Sender type: {0}.", sender.GetType().FullName);

		if (e.Node == null || e.Node.ExplorerConnection == null)
			return;

		try
		{
			UnadviseEvents(e.Node.ExplorerConnection);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
		}

		if (_Instance == null)
			return;

		UpdateRegisteredConnection(DataProtection.DecryptString(e.Node.ExplorerConnection.EncryptedConnectionString),
			EnConnectionSource.Session, true);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Detaches from a Server Explorer connection's events.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void UnadviseEvents(IVsDataExplorerConnection explorerConnection)
	{
		explorerConnection.NodeChanged -= OnExplorerConnectionNodeChanged;
		explorerConnection.NodeRemoving -= OnExplorerConnectionNodeRemoving;
	}


	#endregion Event Handling


}
