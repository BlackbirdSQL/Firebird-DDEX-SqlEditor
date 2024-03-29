﻿
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Windows.Forms;
using BlackbirdSql.Core.Controls;
using BlackbirdSql.Core.Ctl.Config;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Ctl.Extensions;
using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Core.Model.Enums;
using BlackbirdSql.Core.Properties;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;
using Microsoft.VisualStudio.Shell;

using CoreConstants = BlackbirdSql.Core.Ctl.CoreConstants;



namespace BlackbirdSql.Core;


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
	private static string _UnadvisedConnectionString = null;
	private static bool _AdvisingExplorerEvents = false;
	private static readonly char _EdmGlyph = '\u26ee';
	private static readonly char _ProjectDatasetGlyph = '\u2699';
	private static readonly char _UtilityDatasetGlyph = '\u058e';


	#endregion Fields and Constants




	// =========================================================================================================
	#region Property accessors - RctManager
	// =========================================================================================================


	public static bool Available => _Instance != null && _Instance._Rct != null
		&& !_Instance._Rct.ShutdownState && !_Instance._Rct.Loading;

	public static bool AdvisingExplorerEvents
	{
		get { return _AdvisingExplorerEvents; }
		set { _AdvisingExplorerEvents = value; }
	}

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
	public static char EdmGlyph => _EdmGlyph;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns true if a connection dialog exists and has been activated through a
	/// UIHierarchy marshaler or the DataSources Explorer else false.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static bool IsEdmConnectionSource =>
		GetConnectionSource() == EnConnectionSource.EntityDataModel;


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
	public static char ProjectDatasetGlyph => _ProjectDatasetGlyph;



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

			if (_Instance._Rct == null || Loading)
				EnsureLoaded();

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
	/// <see cref="RunningConnectionTable"/> and is used to ensure uniformity of
	/// connections and <see cref="CsbAgent"/>s globally across the extension.
	/// </summary>
	/// <remarks>
	/// Objects that use connections or <see cref="CsbAgent"/>s record the seed at the
	/// time their connection or Csa is created.
	/// Whenever the connection or Csa is accessed it must compare it's saved seed value
	/// against the Rct's seed. If they differ it must check if it's connection requires
	/// updating or Csa requires renewing and also always update it's seed irrelevant of
	/// any updates.
	/// If a Csa requires tracking the built-in seed tracking should be enabled and
	/// used.
	/// </remarks>
	// ---------------------------------------------------------------------------------
	public static long Seed => RunningConnectionTable.Seed;



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns the seed of the last connection registered or updated.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static bool ShutdownState => _Instance != null && _Instance._Rct != null && _Instance._Rct.ShutdownState;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The glyph used to identify connections derived from External utility (Firebird)
	/// connections.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static char UtilityDatasetGlyph => _UtilityDatasetGlyph;


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

		CsbAgent csa = new(connectionInfo, false);
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

		CsbAgent csa = new(node, false);
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
	public static string GetStoredConnectionKey(string connectionValue)
	{
		if (Rct == null)
			return null;

		if (!Rct.TryGetHybridRowValue(connectionValue, out DataRow row))
			return null;

		object @object = row[CoreConstants.C_KeyExConnectionKey];

		return @object == DBNull.Value || @object == null ? null : (string)@object;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the current connection dialog that is active else EnConnectionSource.None.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static EnConnectionSource GetConnectionSource()
	{
		// Definitely None
		if (ApcManager.IdeShutdownState)
			return EnConnectionSource.None;

		// Definitely ServerExplorer
		if (RctManager.AdvisingExplorerEvents)
			return EnConnectionSource.ServerExplorer;

		/*
		 * We're just peeking.
		 * 
		if (!ThreadHelper.CheckAccess())
		{
			// Fire and wait.

			EnConnectionSource result = ThreadHelper.JoinableTaskFactory.Run(async delegate
			{
				await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
				return GetConnectionSourceImpl(caller);
			});

			return result;
		}
		*/

		return GetConnectionSourceImpl();
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets (on the ui thread) the current connection dialog that is active else
	/// EnConnectionSource.None.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private static EnConnectionSource GetConnectionSourceImpl()
	{
		// We're just peeking.
		// Diag.ThrowIfNotOnUIThread();

		string objectKind = ApcManager.ActiveWindowObjectKind;

		// Probably nothing
		if (objectKind == null)
		{
			// Tracer.Trace(typeof(UnsafeCmd), $"GetConnectionSource({caller})", "\nProbably EnConnectionSource.None: ActiveWindowObjectKind is null.");
			return EnConnectionSource.None;
		}

		string seGuid = VSConstants.StandardToolWindows.ServerExplorer.ToString("B", CultureInfo.InvariantCulture);

		// Definitely ServerExplorer
		if (objectKind != null && objectKind.Equals(seGuid, StringComparison.InvariantCultureIgnoreCase))
		{
			// Tracer.Trace(typeof(UnsafeCmd), "GetConnectionSource()", "\nDefinitely EnConnectionSource.ServerExplorer: ActiveWindowObjectKind is ServerExplorer ToolWindow.");
			return EnConnectionSource.ServerExplorer;
		}

		string datasourceGuid = VSConstants.StandardToolWindows.DataSource.ToString("B", CultureInfo.InvariantCulture);

		// Definitely EntityDataModel 
		if (objectKind != null && objectKind.Equals(datasourceGuid, StringComparison.InvariantCultureIgnoreCase))
		{
			// Tracer.Trace(typeof(UnsafeCmd), "GetConnectionSource()", "\nDefinitely EnConnectionSource.EntityDataModel: ActiveWindowObjectKind is DataSource ToolWindow.");
			return EnConnectionSource.EntityDataModel;
		}


		string objectType = ApcManager.ActiveWindowObjectType;

		// Probably nothing
		if (objectType == null)
		{
			// Tracer.Trace(typeof(UnsafeCmd), "GetConnectionSource()", "\nProbably EnConnectionSource.None: ActiveWindowObjectType is null, ObjectKind: {0}.", objectKind);
			return EnConnectionSource.None;
		}

		// Definitely Session
		if (objectType.StartsWith("BlackbirdSql.", StringComparison.InvariantCultureIgnoreCase))
		{
			// Tracer.Trace(typeof(UnsafeCmd), "GetConnectionSource()", "\nDefinitely EnConnectionSource.Session: ActiveWindowObjectType StartsWith 'BlackbirdSql'.");
			return EnConnectionSource.Session;
		}

		// Most likely Application.
		if (objectType.Equals("System.ComponentModel.Design.DesignerHost", StringComparison.InvariantCultureIgnoreCase))
		{
			// Tracer.Trace(typeof(UnsafeCmd), "GetConnectionSource()", "\nLikely EnConnectionSource.Application: ActiveWindowObjectType is ComponentModel.Design.DesignerHost.");
			return EnConnectionSource.Application;
		}

		// Most likely EntityDataModel or some other design model document initialized from
		// Solution Explorer that opens the connection dialog.
		// (Removed solution explorer as the kind to include other possible hierarchy launch locations.)
		if (objectType.Equals("Microsoft.VisualStudio.PlatformUI.UIHierarchyMarshaler", StringComparison.InvariantCultureIgnoreCase))
		{
			// Tracer.Trace(typeof(UnsafeCmd), "GetConnectionSource()", "\nLikely EnConnectionSource.EntityDataModel: ActiveWindowObjectType is Microsoft.VisualStudio.PlatformUI.UIHierarchyMarshaler.");
			return EnConnectionSource.EntityDataModel;
		}


		// No known connection source
		// Tracer.Trace(typeof(UnsafeCmd), "GetConnectionSource()", "No known ConnectionSource. ObjectType: {0}, ObjectKind: {1}.", objectType, objectKind);

		return EnConnectionSource.None;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets a connection string given the ConnectionUrl or DatasetKey
	/// or DatsetKey synonym.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static string GetConnectionString(string connectionValue)
	{
		if (Rct == null)
			return null;

		if (!Rct.TryGetHybridRowValue(connectionValue, out DataRow row))
			return null;

		object @object = row[CoreConstants.C_KeyExConnectionString];

		return @object == DBNull.Value || @object == null ? null : (string)@object;
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

		return @object == DBNull.Value || @object == null ? null : (string)@object;
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
			&& (SystemData.DatasetKeyFmt.FmtRes(csa.DataSource, datasetId) == csa.ConnectionName
			|| SystemData.DatasetKeyAlternateFmt.FmtRes(csa.DataSource, datasetId) == csa.ConnectionName))
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
			&& (SystemData.DatasetKeyFmt.FmtRes(csa.DataSource, datasetId) == csa.ConnectionName
			|| SystemData.DatasetKeyAlternateFmt.FmtRes(csa.DataSource, datasetId) == csa.ConnectionName))
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
	/// Initiates loading of the RunningConnectionTable.
	/// Loads ServerExplorer, FlameRobin and Application connection configurations from
	/// OnSolutionLoad, package initialization, ServerExplorer or any other applicable
	/// activation event.
	/// </summary>
	/// <returns>
	/// True if the load succeeded or connections were already loaded else false if the
	/// rct is in a shutdown state.
	/// </returns>
	// ---------------------------------------------------------------------------------
	public static bool LoadConfiguredConnections()
	{
		return ResolveDeadlocks(true);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Ensures the rct is loaded, waiting or synchronoulsy loading if required.
	/// </summary>
	/// <returns>
	/// True if the load succeeded or connections were already loaded else false if the
	/// rct is in a shutdown state.
	/// </returns>
	// ---------------------------------------------------------------------------------
	public static bool EnsureLoaded()
	{
		return ResolveDeadlocks(false);
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Resolves red zone deadlocks then loads the Rct if neccesary.
	/// </summary>
	/// <param name="asynchronous" >
	/// True is the caller is making an async request to ensure the rct is loaded or in
	/// a loaing state, else False if the loaded rct is required.
	/// </param>
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
	/// The only way to control this is to create a managed awaitable or sync process to
	/// take over loading because switching between the ui and threadpool occurs
	/// several times during the load.
	/// </remarks>
	// ---------------------------------------------------------------------------------
	private static bool ResolveDeadlocks(bool asynchronous)
	{
		// Tracer.Trace(typeof(RctManager), "LoadConfiguredConnections()", "Instance._Rct == null: {0}", Instance._Rct == null);

		// Shutdown state.
		if (_Instance == null)
			return false;

		// Rct has not been initialized.
		if (!Loading && Instance._Rct == null)
		{
			Instance._Rct = RunningConnectionTable.CreateInstance();
			Instance._Rct.LoadConfiguredConnections();

			if (asynchronous)
				return true;

			if (PersistentSettings.IncludeAppConnections && !ThreadHelper.CheckAccess())
				Instance._Rct.WaitForAsyncLoad();

			return true;
		}
		// Aynchronous request. Rct has been initialized or is loading, so just exit.
		else if (asynchronous)
		{
			return false;
		}
		// Synchronous request and loading - deadlock red zone.
		else if (Loading)
		{
			// If sync loads are still active we're safe and can wait because no switching
			// of threads takes place here.
			Instance._Rct.WaitForSyncLoad();

			// If we're not on the main thread or the async payload is not in a
			// pending state we can safely wait.
			if (!ThreadHelper.CheckAccess() || !Instance._Rct.AsyncPending)
			{
				Instance._Rct.WaitForAsyncLoad();
			}
			else
			{
				// Tracer.Trace(typeof(RctManager), "LoadConfiguredConnections()", "Cancelling Async and switching to sync");

				// We're on the main thread and async is pending. This is a deadlock red zone.
				// Send the async payload a cancel notification and execute the cancelled
				// async task on the main thread.
				Instance._Rct.LoadUnsafeConfiguredConnectionsSync();
			}
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

		if (!PersistentSettings.IncludeAppConnections || _Instance == null || Instance._Rct == null || ShutdownState)
			return false;

		return Instance._Rct.AsyncLoadConfiguredConnections(probject);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Modifies the name and connection information of Server Explorer's internal
	/// IVsDataExplorerConnectionManager connection entry.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private static bool ModifyServerExplorerConnection(IVsDataExplorerConnection explorerConnection,
		ref CsbAgent csa, bool modifyExplorerConnection)
	{
		// Tracer.Trace(typeof(RctManager), "ModifyServerExplorerConnection()", "csa.ConnectionString: {0}, modifyExplorerConnection: {1}.", csa.ConnectionString, modifyExplorerConnection);

		Rct?.DisableEvents();

		// finally is unreliable.
		try
		{

			CsbAgent seCsa = new(explorerConnection.Connection.DecryptedConnectionString(), false);
			string connectionKey = explorerConnection.GetConnectionKey(true);

			if (string.IsNullOrWhiteSpace(csa.ConnectionKey) || csa.ConnectionKey != connectionKey)
			{
				csa.ConnectionKey = connectionKey;
			}


			// Sanity check. Should already be done.
			// Perform a deep validation of the updated csa to ensure an update
			// is in fact required.
			bool updateRequired = !AbstractCsbAgent.AreEquivalent(csa, seCsa, CsbAgent.DescriberKeys, true);

			if (explorerConnection.DisplayName.Equals(csa.DatasetKey))
			{
				if (!updateRequired)
				{
					explorerConnection.ConnectionNode.Select();
					Rct?.EnableEvents();
					return false;
				}
			}
			else
			{
				explorerConnection.DisplayName = csa.DatasetKey;

				if (!updateRequired)
				{
					Rct?.EnableEvents();
					return true;
				}
			}

			if (!modifyExplorerConnection)
			{
				Rct?.EnableEvents();
				return false;
			}

			// An update is required...


			explorerConnection.Connection.SetConnectionString(csa.ConnectionString);
			explorerConnection.ConnectionNode.Select();

			Rct?.EnableEvents();

			return true;
		}
		catch
		{
			Rct?.EnableEvents();
			return false;
		}
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
			return serverName;

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

		try
		{
			if (_UnadvisedConnectionString == explorerConnection.DecryptedConnectionString())
			{
				Rct.AdviseEvents(explorerConnection);
			}
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			_UnadvisedConnectionString = null;
			throw;
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
				str += $"\nDATASETKEY: {(string)row[CoreConstants.C_KeyExDatasetKey]}, ConnectionUrl: {(string)row[CoreConstants.C_KeyExConnectionUrl]}";
				str += "\n\t------------------------------------------";
				str += "\n\t";

				foreach (DataColumn col in Rct.Databases.Columns)
				{
					if (col.ColumnName == CoreConstants.C_KeyExDatasetKey || col.ColumnName == CoreConstants.C_KeyExConnectionUrl || col.ColumnName == CoreConstants.C_KeyExConnectionString)
					{
						continue;
					}
					str += $"{col.ColumnName}: {(row[col.ColumnName] == null ? "null" : row[col.ColumnName] == DBNull.Value ? "DBNull" : row[col.ColumnName].ToString())}, ";
				}
				str += "\n\t------------------------------------------";
				str += $"\n\tConnectionString: {(string)row[CoreConstants.C_KeyExConnectionString]}";
			}

			str += "\n--------------------------------------------------------------------------------------";
			str += "\n--------------------------------------------------------------------------------------";
			str += $"\nDATASETKEY INDEXES:";

			foreach (KeyValuePair<string, int> pair in Rct)
			{
				str += $"\n\t{pair.Key}: {pair.Value}";
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
	/// <see cref="StoreUnadvisedConnection"/>.
	/// </remarks>
	// ---------------------------------------------------------------------------------
	private static bool UpdateServerExplorer(ref CsbAgent csa,
		bool addExplorerConnection, bool modifyExplorerConnection)
	{
		// Tracer.Trace(typeof(RctManager), "UpdateServerExplorer()", "csa.ConnectionString: {0}, addServerExplorerConnection: {1}, modifyExplorerConnection: {2}.", csa.ConnectionString, addExplorerConnection, modifyExplorerConnection);

		csa.ConnectionSource = EnConnectionSource.ServerExplorer;

		IVsDataExplorerConnectionManager manager = ApcManager.ExplorerConnectionManager;

		(_, IVsDataExplorerConnection explorerConnection) = manager.SearchExplorerConnectionEntry(csa.ConnectionString, false);

		if (explorerConnection != null)
			return ModifyServerExplorerConnection(explorerConnection, ref csa, modifyExplorerConnection);

		if (!addExplorerConnection)
			return false;


		csa.ConnectionKey = csa.DatasetKey;

		Rct.DisableEvents();

		explorerConnection = manager.AddConnection(csa.DatasetKey, new(SystemData.ProviderGuid), csa.ConnectionString, false);

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
	/// Validates IVsDataExplorerConnection to establish if it's stored DatasetKey
	/// matches the DisplayName (proposed ConnectionName) and updates the Server
	/// Explorer internal table if it doesn't. Also checks if a wizard spawned from a
	/// UIHierarchyMarshaler has corrupted the root node.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static void ValidateAndUpdateExplorerConnectionRename(IVsDataExplorerConnection explorerConnection,
		string proposedConnectionName, CsbAgent csa)
	{
		if (Rct == null)
			return;


		// Wizards spawned from a UIHierarchyMarshaler will likely misbehave and corrupt
		// our connection node if it uses a 'New Connection' dialog.
		// We tag the connection string in our IVsDataConnectionProperties and
		// IVsDataConnectionUIProperties implementations with "edmx" and "edmu"
		// respectively, to identify it as a connection created or selected in a dialog
		// spawned from UIHierarchyMarshaler or DataSources ToolWindow.
		// If the connection doesn't exist in the SE we will have created it on the fly
		// to prevent the wizard throwing an exception.
		// Whether created or selected, the wizard will misbehave and rename the
		// connection, and we lose the DatasetId if it existed.
		// A node changed event is raised which we pick up here, and we perform a
		// reverse repair.

		if (csa.ContainsKey("edmx") || csa.ContainsKey("edmu"))
		{
			if (csa.ContainsKey("edmu"))
			{
				// Clear out any UIHierarchyMarshaler or DataSources ToolWindow ConnectionString identifiers.
				csa.Remove("edmu");
				csa.Remove("edmx");

				// Tracer.Trace(typeof(RctManager), "ValidateAndUpdateExplorerConnectionRename()", "\nEDMU repairString: {0}.", csa.ConnectionString);

				UpdateOrRegisterConnection(csa.ConnectionString, EnConnectionSource.ServerExplorer, false, true);
			}
			else
			{
				string storedConnectionString = GetConnectionString(csa.SafeDatasetMoniker);

				if (storedConnectionString == null)
				{
					ApplicationException ex = new($"ExplorerConnection rename failed. Possibly corrupted by the EDMX wizard. Proposed connection name: {proposedConnectionName}, Explorer connection string: {csa.ConnectionString}.");
					Diag.Dug(ex);
					throw ex;
				}

				// Tracer.Trace(typeof(RctManager), "ValidateAndUpdateExplorerConnectionRename()", "EDMX repairString: {0}.", storedConnectionString);

				UpdateOrRegisterConnection(storedConnectionString, EnConnectionSource.ServerExplorer, false, true);
			}

			return;

		}

		if (proposedConnectionName == csa.DatasetKey)
			return;


		// Tracer.Trace(typeof(RctManager), "ValidateAndUpdateExplorerConnectionRename()", "proposedConnectionName: {0}, connectionString: {1}.", proposedConnectionName, csa.ConnectionString);

		string msg;
		string caption;

		// Sanity check.
		if (proposedConnectionName.StartsWith(SystemData.Scheme))
		{
			if (csa.ContainsKey(CoreConstants.C_DefaultExDatasetKey))
			{
				Rct.DisableEvents();
				explorerConnection.DisplayName = csa.DatasetKey;
				Rct.EnableEvents();

				caption = ControlsResources.RctManager_CaptionInvalidConnectionName;
				msg = ControlsResources.RctManager_TextInvalidConnectionName.FmtRes(SystemData.Scheme, proposedConnectionName);
				MessageCtl.ShowEx(msg, caption, MessageBoxButtons.OK);

				return;
			}

			proposedConnectionName = null;
		}


		string connectionUrl = csa.SafeDatasetMoniker;
		string proposedDatasetId = csa.DatasetId;
		string dataSource = csa.DataSource;
		string dataset = csa.Dataset;


		// Check whether the connection name will change.
		Rct.GenerateUniqueDatasetKey(proposedConnectionName, proposedDatasetId, dataSource, dataset,
			connectionUrl, connectionUrl, out _, out _, out string uniqueDatasetKey,
			out string uniqueConnectionName, out string uniqueDatasetId);

		// Tracer.Trace(typeof(RctManager), "ValidateAndUpdateExplorerConnectionRename()", "GenerateUniqueDatasetKey results: proposedConnectionName: {0}, proposedDatasetId: {1}, dataSource: {2}, dataset: {3}, uniqueDatasetKey: {4}, uniqueConnectionName: {5}, uniqueDatasetId: {6}.",
		//	proposedConnectionName, proposedDatasetId, dataSource, dataset, uniqueDatasetKey ?? "Null", uniqueConnectionName ?? "Null", uniqueDatasetId ?? "Null");

		if (!string.IsNullOrEmpty(uniqueConnectionName))
		{
			caption = ControlsResources.RctManager_CaptionConnectionNameConflict;
			msg = ControlsResources.RctManager_TextConnectionNameConflictLong.FmtRes(proposedConnectionName, uniqueConnectionName);

			if (MessageCtl.ShowEx(msg, caption, MessageBoxButtons.YesNo) == DialogResult.No)
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
		else
			csa.DatasetId = proposedDatasetId;



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
	/// <param name="serverExplorerInsertMode">
	/// Boolean indicating wehther or not a connection is being added or modified.
	/// </param>
	/// <param name="originalConnectionString">
	/// The original ConnectionString before any editing of the Site took place else
	/// null on a new connection.
	/// </param>
	// ---------------------------------------------------------------------------------
	public static (bool, bool, bool) ValidateSiteProperties(IVsDataConnectionProperties site, EnConnectionSource connectionSource,
		bool serverExplorerInsertMode, string originalConnectionString)
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

		if (!string.IsNullOrWhiteSpace(proposedConnectionName) && proposedConnectionName.StartsWith(SystemData.Scheme))
		{
			caption = ControlsResources.RctManager_CaptionInvalidConnectionName;
			msg = ControlsResources.RctManager_TextInvalidConnectionName.FmtRes(SystemData.Scheme, proposedConnectionName);

			MessageCtl.ShowEx(msg, caption, MessageBoxButtons.OK);

			rSuccess = false;
			return (rSuccess, rAddInternally, rModifyInternally);
		}

		string proposedDatasetId = site.ContainsKey(CoreConstants.C_KeyExDatasetId)
			? (string)site[CoreConstants.C_KeyExDatasetId] : null;

		string dataSource = (string)site[CoreConstants.C_KeyDataSource];
		string dataset = (string)site[CoreConstants.C_KeyExDataset];

		string connectionUrl = (site as IBDataConnectionProperties).Csa.DatasetMoniker;

		// Validate the proposed names.
		bool createNew = Rct.GenerateUniqueDatasetKey(proposedConnectionName, proposedDatasetId,
			dataSource, dataset, connectionUrl, originalConnectionUrl, out EnConnectionSource storedConnectionSource,
			out string changedTargetDatasetKey, out string uniqueDatasetKey, out string uniqueConnectionName,
			out string uniqueDatasetId);

		// Tracer.Trace(typeof(RctManager), "ValidateSiteProperties()", "GenerateUniqueDatasetKey results: proposedConnectionName: {0}, proposedDatasetId: {1}, dataSource: {2}, dataset: {3}, createnew: {4}, storedConnectionSource: {5}, changedTargetDatasetKey: {6}, uniqueDatasetKey : {7}, uniqueConnectionName: {8}, uniqueDatasetId: {9}.",
		//	proposedConnectionName, proposedDatasetId, dataSource, dataset, createNew, storedConnectionSource, changedTargetDatasetKey ?? "Null", uniqueDatasetKey ?? "Null", uniqueConnectionName ?? "Null", uniqueDatasetId ?? "Null");

		// If we're in the EDM and the stored connection source is not ServerExplorer we have to create it in the SE to get past the EDM bug.
		// Also, if we're in the SE and the stored connection source is not ServerExplorer we have to create it in the SE.
		if (!createNew && storedConnectionSource != EnConnectionSource.ServerExplorer
			&& (connectionSource == EnConnectionSource.ServerExplorer || connectionSource == EnConnectionSource.EntityDataModel))
		{
			createNew = true;
		}



		#region ---------------- User Prompt Section -----------------



		if (!string.IsNullOrEmpty(uniqueConnectionName) && !string.IsNullOrEmpty(proposedConnectionName))
		{
			// Handle all cases where there's a connection name conflict.

			// The settings provided will create a new SE connection with a connection name conflict.
			if (createNew && !serverExplorerInsertMode && (connectionSource == EnConnectionSource.ServerExplorer
				|| connectionSource == EnConnectionSource.EntityDataModel))
			{
				caption = ControlsResources.RctManager_CaptionNewConnectionNameConflict;
				msg = ControlsResources.RctManager_TextNewSEConnectionNameConflict.FmtRes(proposedConnectionName, uniqueConnectionName);
			}
			// The settings provided will create a new Session connection as well as a new SE connection with a connection name conflict.
			else if (createNew && !serverExplorerInsertMode)
			{
				caption = ControlsResources.RctManager_CaptionNewConnectionNameConflict;
				msg = ControlsResources.RctManager_TextNewConnectionNameConflict.FmtRes(proposedConnectionName, uniqueConnectionName);
			}
			// The settings provided will switch connections with a connection name conflict.
			else if (changedTargetDatasetKey != null)
			{
				caption = ControlsResources.RctManager_CaptionConnectionChangeNameConflict;
				msg = ControlsResources.RctManager_TextConnectionChangeNameConflict.FmtRes(changedTargetDatasetKey, proposedConnectionName, uniqueConnectionName);
			}
			// The settings provided will cause a connection name conflict.
			else
			{
				caption = ControlsResources.RctManager_CaptionConnectionNameConflict;
				msg = ControlsResources.RctManager_TextConnectionNameConflict.FmtRes(proposedConnectionName, uniqueConnectionName);
			}
		}
		else if (!string.IsNullOrEmpty(uniqueDatasetId) && !string.IsNullOrEmpty(proposedDatasetId))
		{
			// Handle all cases where there's a DatasetId conflict.

			// The settings provided will create a new SE connection with a DatasetId conflict.
			if (createNew && !serverExplorerInsertMode && (connectionSource == EnConnectionSource.ServerExplorer
				|| connectionSource == EnConnectionSource.EntityDataModel))
			{
				caption = ControlsResources.RctManager_CaptionNewConnectionDatabaseNameConflict;
				msg = ControlsResources.RctManager_TextNewSEConnectionDatabaseNameConflict.FmtRes(proposedDatasetId, uniqueDatasetId);
			}
			// The settings provided will create a new Session connection as well as a new SE connection with a DatasetId conflict.
			else if (createNew && !serverExplorerInsertMode)
			{
				caption = ControlsResources.RctManager_CaptionNewConnectionDatabaseNameConflict;
				msg = ControlsResources.RctManager_TextNewConnectionDatabaseNameConflict.FmtRes(proposedDatasetId, uniqueDatasetId);
			}
			// The settings provided will switch connections with a DatasetId conflict.
			else if (changedTargetDatasetKey != null)
			{
				caption = ControlsResources.RctManager_CaptionConnectionChangeDatabaseNameConflict;
				msg = ControlsResources.RctManager_TextConnectionChangeDatabaseNameConflict.FmtRes(changedTargetDatasetKey, proposedDatasetId, uniqueDatasetId);
			}
			// The settings provided will cause a DatasetId conflict.
			else
			{
				caption = ControlsResources.RctManager_CaptionDatabaseNameConflict;
				msg = ControlsResources.RctManager_TextDatabaseNameConflict.FmtRes(proposedDatasetId, uniqueDatasetId);
			}
		}
		// Handle all case where there is no conflict.
		// The settings provided will create a new SE connection.
		else if (createNew && !serverExplorerInsertMode &&
			(connectionSource == EnConnectionSource.ServerExplorer || connectionSource == EnConnectionSource.EntityDataModel))
		{
			caption = ControlsResources.RctManager_CaptionNewConnection;
			msg = ControlsResources.RctManager_TextNewSEConnection;
		}
		// The settings provided will create a new Session connection as well as a new SE connection.
		else if (createNew && !serverExplorerInsertMode)
		{
			caption = ControlsResources.RctManager_CaptionNewConnection;
			msg = ControlsResources.RctManager_TextNewConnection;
		}
		// The settings provided will switch connections.
		else if (changedTargetDatasetKey != null)
		{
			// The target connection will change.
			caption = ControlsResources.RctManager_CaptionConnectionChanged;
			msg = ControlsResources.RctManager_TextConnectionChanged.FmtRes(changedTargetDatasetKey);
		}

		if (msg != null && MessageCtl.ShowEx(msg, caption, MessageBoxButtons.YesNo) == DialogResult.No)
			return (false, false, false);



		#endregion ---------------- User Prompt Section -----------------



		// At this point we're good to go.

		// Clean up the site properties.
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
		// if the explorer connection exists or if it's the source (or EntityDataModel) it automatically is the owner.
		string connectionKey = site.FindConnectionKey();

		if (connectionKey == null && (connectionSource == EnConnectionSource.ServerExplorer
			|| connectionSource == EnConnectionSource.EntityDataModel))
		{
			connectionKey = uniqueDatasetKey;
		}

		// Tracer.Trace(typeof(RctManager), "ValidateSiteProperties()", "Retrieved ConnectionKey: {0}.", connectionKey ?? "Null");

		if (connectionKey != null)
		{
			// If the SE connection exists then it is the owner and we have to set the SE ConnectionKey
			// and make the SE the owner.
			string strValue = ((string)site[CoreConstants.C_KeyExConnectionKey]).Trim();

			if (strValue != connectionKey)
				site[CoreConstants.C_KeyExConnectionKey] = connectionKey;

			EnConnectionSource siteConnectionSource = (EnConnectionSource)site[CoreConstants.C_KeyExConnectionSource];

			if (siteConnectionSource != EnConnectionSource.ServerExplorer)
				site[CoreConstants.C_KeyExConnectionSource] = EnConnectionSource.ServerExplorer;
		}

		if (!serverExplorerInsertMode && createNew)
			rAddInternally = true;

		rModifyInternally = changedTargetDatasetKey != null || !rAddInternally
			&& connectionSource != EnConnectionSource.ServerExplorer && connectionSource != EnConnectionSource.EntityDataModel;

		// Tag the site as being updated by the edmx wizard if it's not being done internally, which will
		// use IVsDataConnectionUIProperties.Parse().
		// We do this because the wizard will attempt to rename the connection and we'll pick it up in
		// the rct on an IVsDataExplorerConnection.NodeChanged event, and reverse the rename and drop the tag.
		if (connectionSource == EnConnectionSource.EntityDataModel && !rAddInternally && !rModifyInternally)
			site["edmu"] = true;


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
		return AbstractRunningConnectionTable.VerifyUpdateRights(updater, owner);
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
