
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using BlackbirdSql.Core.Ctl.Config;
using BlackbirdSql.Core.Enums;
using BlackbirdSql.Core.Properties;
using BlackbirdSql.Sys.Ctl;
using BlackbirdSql.Sys.Enums;
using BlackbirdSql.Sys.Extensions;
using BlackbirdSql.Sys.Interfaces;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Shell;

using static BlackbirdSql.CoreConstants;
using static BlackbirdSql.SysConstants;



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
public abstract class AbstruseRunningConnectionTable : PublicDictionary<string, int>, IBsRunningConnectionTable
{


	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - AbstruseRunningConnectionTable
	// ---------------------------------------------------------------------------------


	protected AbstruseRunningConnectionTable() : base(StringComparer.OrdinalIgnoreCase)
	{
	}


	public abstract void Dispose();


	protected virtual void Dispose(bool disposing)
	{
		if (!disposing)
			return;

		InternalInvalidate();

		bool launchersActive = false;


		lock (_LockObject)
		{
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
		{
			System.Threading.Thread.Sleep(50);
			System.Threading.Thread.Yield();
		}

		launchersActive = false;

		lock (_LockObject)
		{
			if (_SyncPayloadLauncherLaunchState != EnLauncherPayloadLaunchState.Inactive)
			{
				launchersActive = true;
				ClearSyncPayloadLauncher(false);
				_SyncPayloadLauncherLaunchState = EnLauncherPayloadLaunchState.Shutdown;
			}

			if (_AsyncPayloadLauncherLaunchState != EnLauncherPayloadLaunchState.Inactive)
			{
				launchersActive = true;
				ClearAsyncPayloadLauncher(false);
				_AsyncPayloadLauncherLaunchState = EnLauncherPayloadLaunchState.Shutdown;
			}


			_HasLocal = false;
			_InternalServers = null;
			_InternalDatabases = null;
			_InternalConnectionsTable = null;
			Clear();
		}

		if (launchersActive)
		{
			System.Threading.Thread.Sleep(50);
			System.Threading.Thread.Yield();
		}

		_SyncPayloadLauncherLaunchState = EnLauncherPayloadLaunchState.Shutdown;
		_AsyncPayloadLauncherLaunchState = EnLauncherPayloadLaunchState.Shutdown;

	}


	#endregion Constructors / Destructors




	// =========================================================================================================
	#region Fields and Constants - AbstruseRunningConnectionTable
	// =========================================================================================================


	private bool _HasLocal = false;
	private static int _Seed = -1;
	protected bool _InternalLoaded = false;
	protected static IBsRunningConnectionTable _Instance;
	protected int _LoadDataCardinal = 0;
	protected static readonly object _LockGlobal = new();
	protected readonly object _LockObject = new();
	private int _LoadingSyncCardinal = 0;
	private int _LoadingAsyncCardinal = 0;
	protected static string _Scheme = NativeDb.Scheme;

	protected DataTable _InternalDatabases = null, _InternalServers = null;
	protected DataTable _InternalConnectionsTable = null;


	private IList<object> _Probjects = null;

	/// <summary>
	/// Must be incremented on every possible update and registration.
	/// </summary>
	protected static long _Stamp = -1;


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
			&& !_AsyncPayloadLauncherToken.Cancelled();


	private bool AsyncPending => _AsyncPayloadLauncherLaunchState == EnLauncherPayloadLaunchState.Pending;



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns true if the Rct is physically in a loading state and both sync and
	/// async tasks are not in Inactive or Shutdown states.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected bool InternalLoading
	{
		get
		{
			lock (_LockObject)
			{
				return _LoadingSyncCardinal > 0 ||
					(_LoadingAsyncCardinal > 0 && !_AsyncPayloadLauncherToken.Cancelled());
			}
		}
	}


	private IList<object> Probjects => _Probjects ??= [];


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - AbstruseRunningConnectionTable
	// =========================================================================================================



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Appends a single row to the internal connections datatable and resets the
	/// exposed InternalDatabases and InternalServers derived tables.
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

			InternalInvalidate();

			lock (_LockObject)
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
	/// Increments the load data cardinal and optionally clears internal tables.
	/// After increment and once the cardinal reaches zero using
	/// <see cref="EndLoadData"/> changes will be accepted in preparation for
	/// reconstructing public Rct tables.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected void BeginLoadData(bool resetTables)
	{
		lock (_LockObject)
		{
			if (resetTables)
			{
				if (_InternalDatabases != null)
					_InternalDatabases = null;

				if (_InternalServers != null)
					_InternalServers = null;
			}

			if (_LoadDataCardinal == 0)
				_InternalConnectionsTable?.BeginLoadData();

			_LoadDataCardinal++;
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Clears the async payload launcher variables.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void ClearAsyncPayloadLauncher(bool disposeToken)
	{
		// Tracer.Trace(GetType(), "ClearAsyncPayloadLauncher()");

		lock (_LockObject)
		{
			if (disposeToken)
			{
				_AsyncPayloadLauncherToken = default;
				_AsyncPayloadLauncherTokenSource?.Dispose();
				_AsyncPayloadLauncherTokenSource = null;
			}

			_LoadingAsyncCardinal--;
			_AsyncPayloadLauncherLaunchState = EnLauncherPayloadLaunchState.Inactive;

			// Tracer.Trace(GetType(), "ClearAsyncPayloadLauncher()", "_LoadingAsyncCardinal is {0}.", _LoadingAsyncCardinal);
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Clears the sync payload launcher variables except for the launch state.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void ClearSyncPayloadLauncher(bool disposeToken)
	{
		lock (_LockObject)
		{
			// Fix as per MagicAndre1981.
			if (_SyncPayloadLauncher != null && (_SyncPayloadLauncher.IsCompleted
				|| _SyncPayloadLauncher.IsCanceled || _SyncPayloadLauncher.IsFaulted))
			{
				_SyncPayloadLauncher.Dispose();
			}

			_SyncPayloadLauncher = null;

			if (disposeToken)
			{
				_SyncPayloadLauncherTokenSource?.Dispose();
				_SyncPayloadLauncherTokenSource = null;
				_SyncPayloadLauncherToken = default;
			}
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

		lock (_LockObject)
		{
			IEnumerable<DataRow> rows = _InternalConnectionsTable.Select().Where(x => key.Equals(x[C_KeyExConnectionUrl]));

			return rows.Count() > 0;
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Creates and initializes a configured connection row.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private DataRow CreateDataRow(Csb csa = null)
	{
		DataRow row = null;

		lock (_LockObject)
		{
			row = _InternalConnectionsTable.NewRow();
			row["Id"] = ++_Seed;
		}

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
	private DataTable CreateDataTable()
	{
		DataTable dataTable = new();

		dataTable.Columns.Add("Id", typeof(int));
		dataTable.Columns.Add("Orderer", typeof(int));
		dataTable.Columns.Add(C_KeyExConnectionUrl, typeof(string));
		dataTable.Columns.Add("DataSourceLc", typeof(string));
		dataTable.Columns.Add("Name", typeof(string));
		dataTable.Columns.Add("DatabaseLc", typeof(string));
		dataTable.Columns.Add(C_KeyExAdornedQualifiedName, typeof(string));
		dataTable.Columns.Add(C_KeyExAdornedQualifiedTitle, typeof(string));
		dataTable.Columns.Add(C_KeyExAdornedDisplayName, typeof(string));

		foreach (Describer describer in Csb.DescriberKeys)
			dataTable.Columns.Add(describer.Name, describer.DataType);

		dataTable.Columns.Add(C_KeyExConnectionString, typeof(string));

		dataTable.AcceptChanges();

		dataTable.PrimaryKey = [dataTable.Columns["Id"]];

		dataTable.AcceptChanges();

		return dataTable;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Generates a unique DatasetKey (ConnectionName) or DatasetId (DatabaseName)
	/// from the proposedConnectionName or proposedDatasetId, usually supplied by a
	/// connection dialog's underlying site or csa, or from a connection rename.
	/// At most one should be specified. If both are specified there will be a
	/// redundancy check of the connection name otherwise the connection name takes
	/// precedence.
	/// If both are null the proposed derivedDatasetId will be derived from the dataSource.
	/// </summary>
	/// <param name="connectionSource">
	/// The ConnectionSource making the request.
	/// </param>
	/// <param name="proposedConnectionName">
	/// The proposed DatasetKey (ConnectionName) property else null.
	/// </param>
	/// <param name="proposedDatasetId">
	/// The proposed DatasetId (DatabaseName) property else null.
	/// </param>
	/// <param name="dataSource">
	/// The DataSource (server name) property to be used in constructing the DatasetKey.
	/// </param>
	/// <param name="dataset">
	/// The readonly Dataset property to be used in constructing a DatasetId if the
	/// proposed DatasetId is null.
	/// </param>
	/// <param name="connectionUrl">
	/// The readonly SafeDatasetMoniker property of the underlying csa of the caller.
	/// </param>
	/// <param name="storedConnectionUrl">
	/// If a stored connection is being modified, the connectionUrl of the stored
	/// connection, else null. If connectionUrl matches connectionUrl they will
	/// be considered equal and it will be ignored.
	/// </param>
	/// <param name="outStoredConnectionSource">
	/// Out | The ConnectionSource of connectionUrl if the connection exists in the rct
	/// else EnConnectionSource.None.
	/// </param>
	/// <param name="outChangedTargetDatasetKey">
	/// Out | If a connection is being modified (storedConnectionUrl is not null) and
	/// connectionUrl points to an existing connection, then the target has changed and
	/// outChangedTargetDatasetKey refers to the changed target's DatasetKey, else null.
	/// </param>
	/// <param name="outUniqueDatasetKey">
	/// Out | The final unique DatasetKey.
	/// </param>
	/// <param name="outUniqueConnectionName">
	/// Out | The unique resulting proposed ConnectionName. If null is returned then whatever was
	/// provided in proposedConnectionName is correct and remains as is. If "" is
	/// returned then whatever was provided in proposedConnectionName is good but changes the
	/// existing name. If a value is returned then proposedConnectionName was
	/// ambiguous and outUniqueConnectionName must be used in it's place.
	/// outUniqueConnectionName and outUniqueDatasetId are mutually exclusive.
	/// </param>
	/// <param name="outUniqueDatasetId">
	/// Out | The unique resulting proposed DatsetId. If null is returned then whatever was
	/// provided in proposedDatasetId is correct and remains as is. If "" is
	/// returned then whatever was provided in proposedDatasetId is good but changes the
	/// existing name. If a value is returned then proposedDatasetId was ambiguous and
	/// outUniqueDatasetId must be used in it's place. outUniqueConnectionName and
	/// outUniqueDatasetId are mutually exclusive.
	/// </param>
	/// <returns>
	/// A boolean indicating whether or not the provided arguments would cause a new
	/// connection to be registered in the rct. This only applies to registration in
	/// the rct and does not determine whether or not a new connection would be created
	/// in the SE. The caller must determine that.
	/// </returns>
	// ---------------------------------------------------------------------------------
	protected abstract bool GenerateUniqueDatasetKey(EnConnectionSource connectionSource,
		ref string proposedConnectionName, ref string proposedDatasetId, string dataSource,
		string dataset, string connectionUrl, string storedConnectionUrl,
		out EnConnectionSource outStoredConnectionSource,
		out string outChangedTargetDatasetKey, out string outUniqueDatasetKey,
		out string outUniqueConnectionName, out string outUniqueDatasetId);



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Decrements the load data cardinal incremented by <see cref="BeginLoadData"/>
	/// and accepts changes once the cardinal reaches zero in preparation for
	/// reconstructing public Rct tables.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected void EndLoadData()
	{
		lock (_LockObject)
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
					_InternalConnectionsTable?.EndLoadData();
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
			}
		}

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// [Async on UI thread]: Launches an async task to perform unsafe loading of
	/// solution connections which require the DTE on the UI thread. To keep things
	/// tight this method will throw an exception if it's already on the UI thread and
	/// should not have been called.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected bool InternalAsyuiLoadApplicationConnections(object probject)
	{
		// Tracer.Trace(GetType(), "InternalAsyuiLoadApplicationConnections()", "Probject? {0}, _LoadingAsyncCardinal: {1}.",
		//  	probject == null ? "probject == null" : "probject != null", _LoadingAsyncCardinal);

		if (!PersistentSettings.IncludeAppConnections)
			return false;


		// Sanity checks.

		lock (_LockObject)
		{
			if (_InternalConnectionsTable == null)
				return false;


			if (_LoadingAsyncCardinal > 0 && !_AsyncPayloadLauncherToken.Cancelled() && probject != null)
			{
				// Tracer.Trace(GetType(), "InternalAsyuiLoadApplicationConnections()", "Abort - is probject.");

				Probjects.Add(probject);

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
		TaskCreationOptions creationOptions = TaskCreationOptions.PreferFairness | TaskCreationOptions.AttachedToParent;
		CancellationToken cancelToken = _AsyncPayloadLauncherToken;

		// Fire and remember.

		if (_AsyncPayloadLauncher != null && _AsyncPayloadLauncher.IsCompleted)
			_AsyncPayloadLauncher?.Dispose();

		// Run on new thread in thread pool.
		// Fire and remember.

		async Task<bool> payloadAsync() => await InternalLoadApplicationConnectionsAsync(cancelToken, probject);


		// Tracer.Trace(GetType(), "InternalAsyuiLoadApplicationConnections()", "Queueing InternalLoadApplicationConnectionsAsync.");

		_AsyncPayloadLauncher = Task.Factory.StartNew(payloadAsync, default, creationOptions, TaskScheduler.Default).Unwrap();


		// Tracer.Trace(GetType(), "InternalAsyuiLoadApplicationConnections()", "Queued InternalLoadApplicationConnectionsAsync.");


		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Increments the Rct stamp after changes to the internal tables.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected long InternalInvalidate()
	{
		// Tracer.Trace(GetType(), "InternalInvalidate()");

		return ++_Stamp;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Recursively loads unique connections configured in the App.configs of a
	/// Solution's projects.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void InternalLoadApplicationConnections(CancellationToken cancelToken, bool isSync, object probject)
	{
		// Tracer.Trace(GetType(), "InternalLoadApplicationConnections()");

		Diag.ThrowIfNotOnUIThread();

		int count;
		Project current;

		lock (_LockObject)
		{
			if (probject != null)
			{
				Probjects.Add(probject);
			}
			else
			{
				foreach (Project project in UnsafeCmd.RecursiveGetDesignTimeProjects())
				{
					if (project.IsDesignTime())
						Probjects.Add(project);
				}
			}

			count = _Probjects != null ? _Probjects.Count : 0;
			current = count > 0 ? (Project)_Probjects[0] : null;
		}

		while (count > 0)
		{
			// Tracer.Trace(GetType(), "InternalLoadApplicationConnections()", "Scanning project: {0}.", project.Name);
			try
			{
				RecursiveScanProject(current);
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw ex;
			}

			if (!isSync && (_AsyncPayloadLauncher == null || cancelToken.Cancelled()))
			{
				lock (_LockObject)
					_Probjects.Clear();
				return;
			}

			// Tracer.Trace(GetType(), "InternalLoadApplicationConnections()", "Completed Scanning project: {0}.", project.Name);

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
	/// Searches for application configured connections and performs registration
	/// asynchronously.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private async Task<bool> InternalLoadApplicationConnectionsAsync(CancellationToken cancelToken, object probject)
	{
		// Tracer.Trace(GetType(), "InternalLoadApplicationConnectionsAsync()");

		bool result = true;

		try
		{

			if (cancelToken.Cancelled())
			{
				result = false;
			}
			else
			{

				// We must be off of the ui thread here
				Diag.ThrowIfOnUIThread();

				// Now onto main thread.
				await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

			}

			// Check again.
			if (cancelToken.Cancelled() || ApcManager.SolutionClosing)
			{
				result = false;
			}
			else
			{
				_AsyncPayloadLauncherLaunchState = EnLauncherPayloadLaunchState.Launching;

				// if (probject == null)
				//	AdviseServerExplorerConnectionsEvents();


				if (probject == null && ApcManager.SolutionProjects == null)
				{
					COMException exc = new("DTE.Solution.Projects is not available", VSConstants.RPC_E_INVALID_DATA);
					Diag.Dug(exc);
					throw exc;
				}

				// Build an object list of top level projects for the solution, then launch a non-block thread on
				// the thread pool that will recursively call an async method that switches back to the ui
				// thread to register connections for each project.


				// Tracer.Trace(GetType(), "InternalLoadApplicationConnectionsAsync()", "Calling InternalLoadApplicationConnections(), _AsyncPayloadLauncherToken.Cancelled(): {0}.", _AsyncPayloadLauncherToken.Cancelled());


				BeginLoadData(true);

				try
				{
					InternalLoadApplicationConnections(_AsyncPayloadLauncherToken, false, probject);

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
			throw;
		}
		finally
		{
			ClearAsyncPayloadLauncher(true);

			lock (_LockObject)
			{
				if (probject == null && _LoadingSyncCardinal == 0)
					_InternalLoaded = true;
			}
		}

		return result;

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Searches for application configured connections and performs registration
	/// synchronously.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected bool InternalLoadApplicationConnectionsSync()
	{
		// Tracer.Trace(GetType(), "InternalLoadApplicationConnectionsSync()");

		bool result = true;

		_AsyncPayloadLauncherTokenSource?.Cancel();

		if (ApcManager.SolutionClosing)
			return false;

		// We must be on the ui thread here
		Diag.ThrowIfNotOnUIThread();

		try
		{
			if (ApcManager.SolutionProjects == null)
			{
				COMException exc = new("DTE.Solution.Projects is not available", VSConstants.RPC_E_INVALID_DATA);
				Diag.Dug(exc);
				throw exc;
			}

			BeginLoadData(true);

			try
			{
				InternalLoadApplicationConnections(default, true, null);

				if (_Instance == null)
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
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}


		return result;
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
	protected bool InternalLoadConnections()
	{
		// Tracer.Trace(GetType(), "InternalLoadConnections()");

		if (InternalLoading)
			return false;

		// if (_InternalDatabases != null)
		//	return true;


		bool result = false;

		lock (_LockObject)
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

			CancellationToken cancelToken = SyncEnter();


			// Load the SE and FlameRobin connections and possibly application connections.
			result = InternalLoadConnectionsSync(cancelToken);

			// Load the Application connections asynchronously if we're not on the main thread
			// with a fire and forget async task.

			if (PersistentSettings.IncludeAppConnections && !ThreadHelper.CheckAccess())
				InternalAsyuiLoadApplicationConnections(null);
		}
		finally
		{
			if (!SyncExit())
				result = false;

			lock (_LockObject)
			{
				_LoadingSyncCardinal--;

				if (ThreadHelper.CheckAccess() || _LoadingAsyncCardinal == 0)
				{
					_InternalLoaded = true;
				}
			}
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
	private bool InternalLoadConnectionsSync(CancellationToken cancelToken)
	{
		// Tracer.Trace(GetType(), "InternalLoadConnectionsSync()");

		lock (_LockObject)
		{

			_InternalDatabases = null;
			_InternalServers = null;
			_InternalConnectionsTable ??= CreateDataTable();
		}


		BeginLoadData(true);

		try
		{
			if (RctEventSink.EventEnter(true))
				InternalLoadServerExplorerConnections();

			if (_Instance == null)
				return false;

			InternalLoadUtilityConnections();

			if (_Instance == null)
				return false;

			if (PersistentSettings.IncludeAppConnections && ThreadHelper.CheckAccess())
			{
				// Tracer.Trace(GetType(), "InternalLoadConnectionsSync()", "Loading Unsafe Solution Connections on Safe");

				InternalLoadApplicationConnections(_SyncPayloadLauncherToken, true, null);

				if (_Instance == null)
					return false;
			}

			// Add a ghost row to the datasources list
			// This will be the default datasource row so that anything else
			// selected will generate a CurrentChanged event.
			DataRow row = CreateDataRow();

			row["DataSourceLc"] = "";
			row["Port"] = 0;
			row["DatabaseLc"] = "";
			row["Orderer"] = 0;

			// string str = "AddGhostRow: ";
			// foreach (DataColumn col in _InternalConnectionsTable.Columns)
			// str += col.ColumnName + ": " + row[col.ColumnName] + ", ";
			// Tracer.Trace(GetType(), "InternalLoadConnectionsSync()", str);


			AppendSingleConnectionRow(row);


			// Add a Clear/Reset dummy row for the datasources list
			// If selected will invoke a form reset the move the cursor back to the ghost row.
			row = CreateDataRow();

			row["Orderer"] = 1;
			row["DataSource"] = Resources.ErmBindingSource_Reset;
			row["DataSourceLc"] = Resources.ErmBindingSource_Reset.ToLowerInvariant();
			row["Port"] = 0;
			row["DatabaseLc"] = "";

			// str = "AddResetRow: ";
			// foreach (DataColumn col in _InternalConnectionsTable.Columns)
			// str += col.ColumnName + ": " + row[col.ColumnName] + ", ";
			// Tracer.Trace(GetType(), "InternalLoadConnectionsSync()", str);

			AppendSingleConnectionRow(row);


			// Add at least one row, that will be the ghost row, for localhost. 
			if (!_HasLocal)
			{
				row = CreateDataRow();

				row["Orderer"] = 2;
				row["DataSource"] = "localhost";
				row["DataSourceLc"] = "localhost";
				row["DatabaseLc"] = "";

				// str = "AddLocalHostGhostRow: ";
				// foreach (DataColumn col in _InternalConnectionsTable.Columns)
				// str += col.ColumnName + ": " + row[col.ColumnName] + ", ";
				// Tracer.Trace(GetType(), "InternalLoadConnectionsSync()", str);

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
	private void InternalLoadServerExplorerConnections()
	{
		// Tracer.Trace(GetType(), "InternalLoadServerExplorerConnections()");

		RctEventSink.EventEnter(false, true);

		try
		{
			IVsDataExplorerConnectionManager manager = ApcManager.ExplorerConnectionManager;

			string connectionName;
			Guid clsidProvider = new(SystemData.C_ProviderGuid);


			foreach (KeyValuePair<string, IVsDataExplorerConnection> pair in manager.Connections)
			{
				if (!(clsidProvider == pair.Value.Provider))
					continue;


				connectionName = pair.Value.ConnectionNode.Name;


				if (connectionName.Equals("Database", StringComparison.OrdinalIgnoreCase))
					connectionName = pair.Key;

				try
				{
					InternalLoadServerExplorerConnection(connectionName, pair.Value.DecryptedConnectionString());
				}
				catch (Exception ex)
				{
					Diag.Dug(ex);
					throw;
				}
			}
		}
		finally
		{
			RctEventSink.EventExit();
		}

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Loads a connection retrieved from the configured Server Explorer connections
	/// in the <see cref="IVsDataExplorerConnectionManager"/>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void InternalLoadServerExplorerConnection(string connectionName, string connectionString)
	{

		// Tracer.Trace(GetType(), "InternalLoadServerExplorerConnection()", "Connection name: {0}", connectionName);

		string datasetId;
		Csb csa;
		DataRow row;

		csa = new(connectionString)
		{
			ConnectionKey = connectionName
		};

		if (connectionName.StartsWith("Database[\"") && connectionName.EndsWith("\"]"))
			connectionName = connectionName[10..^2];



		datasetId = csa.DatasetId;

		// If the connection name matches the datasetKey constructed from the datasetId, remove it.
		if (!string.IsNullOrEmpty(datasetId) && !csa.ContainsKey(C_KeyExConnectionName)
			&& connectionName == S_DatasetKeyFormat.FmtRes(csa.DataSource, datasetId))
		{
			connectionName = null;
		}

		if (!string.IsNullOrEmpty(connectionName) && !string.IsNullOrEmpty(datasetId))
			datasetId = null;

		BeginLoadData(true);

		try
		{
			// Tracer.Trace(GetType(), "InternalLoadServerExplorerConnection()",
			//	"ConnectionName: {0}, datasetId: {1}, csa: {2}.", connectionName, datasetId, csa.ConnectionString);

			// The datasetId may not be unique at this juncture and already registered.
			row = RegisterUniqueConnectionImpl(connectionName, datasetId,
				EnConnectionSource.ServerExplorer, ref csa);

			if (row == null)
				return;

			/*
			string str = "AddedSERow: ";
			foreach (DataColumn col in _InternalConnectionsTable.Columns)
				str += col.ColumnName + ": " + row[col.ColumnName] + ", ";

			// Tracer.Trace(GetType(), "InternalLoadServerExplorerConnection()", str);
			*/

			if (_Instance == null)
				return;


			// Tracer.Trace(GetType(), "InternalLoadServerExplorerConnection()", "Adding row: {0}", row["DatasetKey"]);
			AppendSingleConnectionRow(row);
		}
		finally
		{
			EndLoadData();
		}

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Loads FlameRobin configured connections.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void InternalLoadUtilityConnections()
	{
		// Tracer.Trace(GetType(), "InternalLoadUtilityConnections()");

		DataRow row;

		string xmlPath = NativeDb.ExternalUtilityConfigurationPath;

		if (string.IsNullOrEmpty(xmlPath) || !File.Exists(xmlPath))
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
			Csb csa;
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
					port = C_DefaultPort;


				// To keep uniformity of server names, the case of the first connection
				// discovered for a server name is the case that will be used for all
				// connections for that server.
				serverName = InternalRegisterServer(datasource, port);

				xmlDatabases = xmlServer.SelectNodes("database");

				foreach (XmlNode xmlDatabase in xmlDatabases)
				{
					if ((xmlNode = xmlDatabase.SelectSingleNode("name")) == null)
						continue;

					datasetId = xmlNode.InnerText.Trim();

					if ((xmlNode = xmlDatabase.SelectSingleNode("path")) == null)
						continue;

					path = Cmd.CleanPath(xmlNode.InnerText);

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

					datasetId = Resources.RctUtilityDatasetIdFormat.FmtRes(datasetId);


					// The datasetId may not be unique at this juncture and already registered.
					csa = new(serverName, port, path, user, password, charset);

					row = RegisterUniqueConnectionImpl(null, datasetId, EnConnectionSource.ExternalUtility, ref csa);

					if (row == null)
						continue;


					if (_Instance == null)
						return;

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

	protected bool InternalResetVolatile()
	{
		lock (_LockObject)
		{
			if (_InternalConnectionsTable == null || InternalLoading)
				return false;

			int orderer;
			int source;
			int seSource = (int)EnConnectionSource.ServerExplorer;
			int sessionSource = (int)EnConnectionSource.Session;

			const string orderKey = "Orderer";
			const string monikerKey = C_KeyExConnectionUrl;
			const string sourceKey = C_KeyExConnectionSource;

			DataRow[] rows = _InternalConnectionsTable.Select().Where
				(x =>
					((orderer = Convert.ToInt32(x[orderKey])) == 2 || orderer == 3)
					&& (source = Convert.ToInt32(x[sourceKey])) != seSource
					&& source != sessionSource
					&& !Cmd.IsNullValueOrEmpty(x[monikerKey])
				).ToArray();


			if (rows.Length == 0)
				return false;

			_InternalLoaded = false;


			BeginLoadData(true);

			try
			{
				foreach (DataRow row in rows)
				{
					int id = Convert.ToInt32(row["Id"]);

					foreach (KeyValuePair<string, int> pair in this)
					{
						if (pair.Value == id)
							RemoveEntry(pair.Key);
					}

					_InternalConnectionsTable.Rows.Remove(row);
				}

			}
			finally
			{
				EndLoadData();
			}

		}

		return true;
	}






	protected bool InternalResolveDeadlocksAndEnsureLoaded(bool asynchronous)
	{
		// Tracer.Trace(typeof(RctManager), "ResolveDeadlocksAndLoad()", "Instance.Initialized: {0}", Instance._Rct == null);


		if (!InternalLoading && !_InternalLoaded)
		{
			// Rct has not been initialized.

			InternalLoadConnections();

			if (asynchronous || !PersistentSettings.IncludeAppConnections || ThreadHelper.CheckAccess())
				return true;

			WaitForAsyncLoad();

			return true;
		}
		// Asynchronous request. Rct has been initialized or is loading, so just exit.
		else if (asynchronous)
		{
			return false;
		}
		// Synchronous request and loading - deadlock red zone.
		else if (InternalLoading)
		{
			// If sync loads are still active we're safe and can wait because no switching
			// of threads takes place here.

			WaitForSyncLoad();

			// If we're not on the main thread or the async payload is not in a
			// pending state we can safely wait.
			if (!ThreadHelper.CheckAccess() || !AsyncPending)
			{
				WaitForAsyncLoad();
			}
			else
			{
				// We're on the main thread and async is pending. This is a deadlock red zone.
				// Send the async payload a cancel notification and execute the cancelled
				// async task on the main thread.

				InternalLoadApplicationConnectionsSync();
			}
		}


		return _InternalLoaded;

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Get the internal connection data row given the ConnectionUrl, ConnectionString,
	/// DatasetKey or DatasetKey synonym.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected bool InternalTryGetHybridRowValue(string hybridKey, EnRctKeyType keyType, out DataRow value)
	{
		lock (_LockObject)
		{
			if (hybridKey == null || _InternalConnectionsTable == null)
			{
				value = null;
				return false;
			}
		}


		// Tracer.Trace(GetType(), "TryGetHybridRowValue()", "hybridKey: {0}", hybridKey);


		if (keyType == EnRctKeyType.ConnectionString)
		{
			Csb csa = new(hybridKey, false);
			hybridKey = csa.Moniker;
			keyType = EnRctKeyType.ConnectionUrl;
		}


		if (keyType == EnRctKeyType.ConnectionUrl)
		{
			lock (_LockObject)
			{
				DataRow[] rows = _InternalConnectionsTable.Select().Where(x => hybridKey.Equals(x[C_KeyExConnectionUrl])).ToArray();

				value = rows.Length > 0 ? rows[0] : null;
			}

			// if (value == null)
			//	Tracer.Trace(GetType(), "TryGetHybridRowValue()", "FAILED Final hybridKey: {0}", hybridKey);
		}
		else if (keyType == EnRctKeyType.AdornedQualifiedName)
		{
			lock (_LockObject)
			{
				DataRow[] rows = _InternalConnectionsTable.Select().Where(x => hybridKey.Equals(x[C_KeyExAdornedQualifiedName])).ToArray();

				value = rows.Length > 0 ? rows[0] : null;
			}

			// if (value == null)
			//	Tracer.Trace(GetType(), "TryGetHybridRowValue()", "FAILED Final hybridKey: {0}", hybridKey);
		}
		else if (keyType == EnRctKeyType.AdornedQualifiedTitle)
		{
			lock (_LockObject)
			{
				DataRow[] rows = _InternalConnectionsTable.Select().Where(x => hybridKey.Equals(x[C_KeyExAdornedQualifiedTitle])).ToArray();

				value = rows.Length > 0 ? rows[0] : null;
			}

			// if (value == null)
			//	Tracer.Trace(GetType(), "TryGetHybridRowValue()", "FAILED Final hybridKey: {0}", hybridKey);
		}
		else
		{
			if (TryGetEntry(hybridKey, out int id))
			{
				lock (_LockObject)
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
	/// Executes the sync configuration loader's wait task.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private bool PayloadSyncWaiter(CancellationToken cancelToken)
	{
		// Tracer.Trace(GetType(), "SyncPayloadTask()");

		if (_SyncPayloadLauncher == null || cancelToken.Cancelled())
			return false;

		_SyncPayloadLauncherLaunchState = EnLauncherPayloadLaunchState.Launching;

		int waitTime = 0;

		while (!cancelToken.Cancelled())
		{
			if (waitTime >= 15000)
			{
				TimeoutException ex = new($"Timed out waiting for SyncPayload Task to complete. Timeout (ms): {waitTime}.");
				Diag.Dug(ex);
				throw ex;
			}

			System.Threading.Thread.Sleep(50);
			System.Threading.Thread.Yield();

			waitTime += 50;
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

			// Tracer.Trace(GetType(), "PrepAsyncPayloadLauncher()", "_LoadingAsyncCardinal is {0}.", _LoadingAsyncCardinal);
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

		// Tracer.Trace("Recursive validate project: " + project.Name);

		config ??= project.GetAppConfig();

		if (config != null)
			ScanAppConfig(config);
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
			Csb csa;
			DataRow row;

			xmlNode = xmlRoot.SelectSingleNode("//confBlackbirdNs:connectionStrings", xmlNs);

			if (xmlNode == null)
				return;

			xmlParent = xmlNode;


			// Sort by datasetkey length first to try and avoid the long connection names created by edmx.

			int i = 0;
			string sortkey;
			List<string> sortlist = [];
			Dictionary<string, Csb> sortdict = [];


			XmlNodeList xmlNodes = xmlParent.SelectNodes($"confBlackbirdNs:add[@providerName='{NativeDb.Invariant}']", xmlNs);


			if (xmlNodes.Count > 0)
			{
				foreach (XmlNode connectionNode in xmlNodes)
				{
					arr = connectionNode.Attributes["name"].Value.Split('.');
					datasetId = Resources.RctProjectDatasetIdFormat.FmtRes(projectName, arr[^1]);

					csa = new(connectionNode.Attributes["connectionString"].Value);

					foreach (Describer describer in Csb.AdvancedKeys)
					{
						if (!describer.IsConnectionParameter)
							csa.Remove(describer.Key);
					}

					// Add in now to store. Will be retrieved and removed when we actually load.
					csa.DatasetId = datasetId;
					csa.ConnectionSource = EnConnectionSource.Application;

					sortkey = csa.DataSource + datasetId + "\n" + (i++).ToString("D4");
					sortlist.Add(sortkey);
					sortdict[sortkey] = csa;
				}
			}

			string name;
			string connectionString;
			DbConnectionStringBuilder csb;


			xmlNodes = xmlParent.SelectNodes($"confBlackbirdNs:add[@providerName='System.Data.EntityClient']", xmlNs);

			if (sortlist.Count == 0 && xmlNodes.Count == 0)
				return;

			foreach (XmlNode connectionNode in xmlNodes)
			{
				name = connectionNode.Attributes["name"].Value;
				datasetId = Resources.RctEdmDatasetFormat.FmtRes(projectName, name);

				csb = new()
				{
					ConnectionString = connectionNode.Attributes["connectionString"].Value
				};

				if (!csb.ContainsKey("provider")
					|| !((string)csb["provider"]).Equals(NativeDb.Invariant, StringComparison.InvariantCultureIgnoreCase))
				{
					continue;
				}

				connectionString = csb.ContainsKey("provider connection string")
					? (string)csb["provider connection string"] : null;

				if (string.IsNullOrWhiteSpace(connectionString))
					continue;

				// Tracer.Trace(GetType(), "RegisterAppConnectionStrings()", "Entity connectionString: {0}", connectionString);

				csa = new(connectionString);

				foreach (Describer describer in Csb.AdvancedKeys)
				{
					if (!describer.IsConnectionParameter)
						csa.Remove(describer.Key);
				}

				// Clear out any UIHierarchyMarshaler or DataSources ToolWindow ConnectionString identifiers.
				csa.Remove(C_KeyExEdmx);
				csa.Remove(C_KeyExEdmu);

				// Add in now to store. Will be retrieved and removed when we actually load.
				csa.DatasetId = datasetId;
				csa.ConnectionSource = EnConnectionSource.Application;

				sortkey = csa.DataSource + datasetId + "\n" + (i++).ToString("D4");
				sortlist.Add(sortkey);
				sortdict[sortkey] = csa;
			}


			// Sort as alpha then string length.

			sortlist.Sort(StringComparer.OrdinalIgnoreCase);
			sortlist.Sort((x, y) => x.Length.CompareTo(y.Length));

			EnConnectionSource connectionSource;

			foreach (string sortlistkey in sortlist)
			{
				csa = sortdict[sortlistkey];

				datasetId = csa.DatasetId;
				connectionSource = csa.ConnectionSource;

				csa.Remove(C_KeyExDatasetId);
				csa.Remove(C_KeyExConnectionSource);

				// Tracer.Trace(GetType(), "RegisterAppConnectionStrings()", "datasource: {0}, dataset: {1}, serverName: {2}, ConnectionName: {3}, datasetId: {4}, Connectionstring: {5}, storedConnectionString: {6}.", datasource, csa.Dataset, serverName, csa.ConnectionName, datasetId, csa.ConnectionString, connectionNode.Attributes["connectionString"].Value);

				// The datasetId may not be unique at this juncture and already registered.
				row = RegisterUniqueConnectionImpl(null, datasetId, connectionSource, ref csa);

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
	protected string InternalRegisterServer(string serverName, int port)
	{

		if (serverName.ToLowerInvariant() == "localhost")
		{
			serverName = "localhost";
			_HasLocal = true;
		}

		DataRow[] rows = null;
		string search = serverName.ToLower();

		lock (_LockObject)
		{
			rows = _InternalConnectionsTable.Select()
				.Where(x => search.Equals(x["DataSourceLc"])
					&& string.IsNullOrWhiteSpace((string)x["DatabaseLc"])
					&& !Cmd.IsNullValue(x[C_KeyPort])
					&& port == Convert.ToInt32(x[C_KeyPort])).ToArray();
		}

		if (rows.Length > 0)
			return (string)rows[0]["DataSource"];

		BeginLoadData(true);

		try
		{

			DataRow row = CreateDataRow();

			row["DataSource"] = serverName;
			row["DataSourceLc"] = serverName.ToLower();
			row["Port"] = port;
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
	protected bool RegisterUniqueConnection(string proposedDatasetKey,
		string proposedDatasetId, EnConnectionSource source, ref Csb csa)
	{

		if (_Instance == null)
			return false;

		if (source <= EnConnectionSource.Unknown)
			source = EnConnectionSource.Session;

		BeginLoadData(true);

		try
		{
			DataRow row = RegisterUniqueConnectionImpl(proposedDatasetKey, proposedDatasetId, source, ref csa);

			if (row == null)
				return false;

			AppendSingleConnectionRow(row);
			csa.RefreshDriftDetectionState();
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
		string proposedDatasetId, EnConnectionSource source, ref Csb csa)
	{
		// Tracer.Trace(GetType(), "RegisterUniqueConnectionDatsetKey()");

		if (string.IsNullOrWhiteSpace(proposedConnectionName) && string.IsNullOrWhiteSpace(proposedDatasetId))
		{
			ArgumentNullException ex = new ArgumentNullException("proposedConnectionName and proposedDatasetId may not both be null.");
			Diag.Dug(ex);
			throw ex;
		}

		if (!string.IsNullOrWhiteSpace(proposedConnectionName) && !string.IsNullOrWhiteSpace(proposedDatasetId))
		{
			ArgumentNullException ex = new ArgumentNullException("proposedConnectionName and proposedDatasetId may not both contain values.");
			Diag.Dug(ex);
			throw ex;
		}


		// The only way to get or register a connection configuration is through a
		// unique connection url, which requires at a minimum DataSource, Database and
		// UserID.

		string connectionUrl = csa.LiveDatasetMoniker;

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

			GenerateUniqueDatasetKey(source, ref proposedConnectionName, ref proposedDatasetId, csa.DataSource, csa.Dataset,
				connectionUrl, null, out _, out _, out string uniqueDatasetKey, out string uniqueConnectionName,
				out string uniqueDatasetId);

			csa.DatasetKey = uniqueDatasetKey;

			if (uniqueConnectionName == null && proposedConnectionName == null)
				csa.Remove(C_KeyExConnectionName);
			else if (!string.IsNullOrEmpty(uniqueConnectionName))
				csa.ConnectionName = uniqueConnectionName;
			else
				csa.ConnectionName = proposedConnectionName;

			if (uniqueDatasetId == null && proposedDatasetId == null)
				csa.Remove(C_KeyExDatasetId);
			else if (!string.IsNullOrEmpty(uniqueDatasetId))
				csa.DatasetId = uniqueDatasetId;
			else
				csa.DatasetId = proposedDatasetId;


			int id = 0;

			lock (_LockObject)
				id = _InternalConnectionsTable.Rows.Count;

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
		CancellationToken cancelToken = _SyncPayloadLauncherToken;
		TaskCreationOptions creationOptions = TaskCreationOptions.PreferFairness | TaskCreationOptions.AttachedToParent;
		TaskScheduler scheduler = TaskScheduler.Default;

		// For brevity. Create the payload.
		bool payload() =>
			PayloadSyncWaiter(cancelToken);

		// Start up the payload launcher with tracking.
		// Fire and remember - switch to thread pool

		_SyncPayloadLauncher = Task.Factory.StartNew(payload, default, creationOptions, scheduler);

		// We're not doing this for a sync task.
		// _TaskHandler.RegisterTask(_SyncPayloadLauncher);

		// Tracer.Trace(GetType(), "InternalLoadConnections()", "SyncPayloadTask created. Calling LoadConfiguredConnectionsImpl()");

		return cancelToken;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs a graceful termination of the sync Rct loading task.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private bool SyncExit()
	{
		_SyncPayloadLauncherTokenSource?.Cancel();

		System.Threading.Thread.Sleep(50);
		System.Threading.Thread.Yield();

		// Kill SyncPayloadTask.

		// Tracer.Trace(GetType(), "InternalLoadConnections()", "Killing SyncPayloadTask. Loading sync done. State: {0}",
		//	_SyncPayloadLauncherLaunchState);

		ClearSyncPayloadLauncher(false);


		if (_SyncPayloadLauncherLaunchState == EnLauncherPayloadLaunchState.Shutdown)
			return false;

		_SyncPayloadLauncherLaunchState = EnLauncherPayloadLaunchState.Inactive;

		return true;

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Updates an rct row given a csa. csa may be null.
	/// </summary>
	/// <returns>True if the row was updated else false.</returns>
	// ---------------------------------------------------------------------------------
	protected bool UpdateDataRowFromCsa(DataRow row, Csb csa = null)
	{
		bool updated = false;
		object csaValue;


		try
		{
			foreach (Describer describer in Csb.DescriberKeys)
			{
				csaValue = csa != null && csa.ContainsKey(describer.Name)
					? csa[describer.Name]
					: describer.DefaultValue;

				updated = UpdateDataColumn(row, describer.Name, csaValue, updated);
			}

			updated = UpdateDataColumn(row, C_KeyExAdornedQualifiedName, csa?.AdornedQualifiedName, updated);
			updated = UpdateDataColumn(row, C_KeyExAdornedQualifiedTitle, csa?.AdornedQualifiedTitle, updated);
			updated = UpdateDataColumn(row, C_KeyExAdornedDisplayName, csa?.AdornedDisplayName, updated);

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



	private bool UpdateDataColumn(DataRow row, string name, object newValue, bool updated)
	{
		object originalRowValue = row[name];
		object rowValue = originalRowValue == DBNull.Value ? null : row[name];


		if (newValue != null)
		{
			if (rowValue == null || !rowValue.Equals(newValue))
			{
				if (!updated)
				{
					BeginLoadData(true);
					row.BeginEdit();
				}
				updated = true;
				row[name] = newValue;
				if (name == C_KeyExDatasetId)
					row["Name"] = newValue;

			}
			return updated;
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
			row[name] = DBNull.Value;
			if (name == C_KeyExDatasetId)
				row["Name"] = "";
			return updated;
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
			row[name] = DBNull.Value;
			if (name == C_KeyExDatasetId)
				row["Name"] = "";
		}

		return updated;

	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The awaiter for unsafe loading of solution connections that use the DTE and were
	/// launched by <see cref="InternalAsyuiLoadApplicationConnections"/> .
	/// </summary>
	/// <remarks>
	/// This may take some time if the ide is starting up, followed in quick succession
	/// with opening a solution and expanding an SE node.
	/// </remarks>
	// ---------------------------------------------------------------------------------
	protected void WaitForAsyncLoad()
	{
		// Tracer.Trace(GetType(), "WaitForAsyncLoad()");

		int waitTime = 0;

		while (AsyncLoading)
		{
			if (waitTime >= 15000)
			{
				TimeoutException ex = new($"Timed out waiting for InternalAsyuiLoadApplicationConnections() to complete. Timeout (ms): {waitTime}.");
				Diag.Dug(ex);
				throw ex;
			}

			// Tracer.Trace(GetType(), "WaitForAsyncLoad()", "WAITING");

			try
			{
				if (_AsyncPayloadLauncher.Wait(100, _AsyncPayloadLauncherToken))
					break;
			}
			catch { }

			waitTime += 100;
		}

		// Tracer.Trace(GetType(), "WaitForAsyncLoad()", "DONE WAITING");

	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The dummy awaiter for loading of safe preconfigured connections that was
	/// launched in <see cref="SyncPayloadTask"/>.
	/// </summary>
	/// This may take some time if the ide is starting up, followed in quick succession
	/// with opening a solution and expanding an SE node.
	// ---------------------------------------------------------------------------------
	protected bool WaitForSyncLoad()
	{
		// Tracer.Trace(GetType(), "WaitForSyncLoad()");

		if (!InternalLoading)
			return true;

		if (_InternalDatabases != null)
			return true;

		int waitTime = 0;

		while (_SyncPayloadLauncherLaunchState != EnLauncherPayloadLaunchState.Inactive
			&& _SyncPayloadLauncherLaunchState != EnLauncherPayloadLaunchState.Shutdown
			&& _SyncPayloadLauncher != null && !_SyncPayloadLauncher.IsCompleted
			&& !_SyncPayloadLauncher.IsCanceled && !_SyncPayloadLauncher.IsFaulted
			&& _SyncPayloadLauncherTokenSource != null && !_SyncPayloadLauncherToken.Cancelled())
		{
			if (waitTime >= 15000)
			{
				TimeoutException ex = new($"Timed out waiting for InternalLoadConnections() to complete. Timeout (ms): {waitTime}.");
				Diag.Dug(ex);
				throw ex;
			}

			try
			{
				if (_SyncPayloadLauncher.Wait(100, _SyncPayloadLauncherToken))
					break;
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


	// Currently only used for debugging


#endregion Event Handling


}
