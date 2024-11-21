
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Enums;
using BlackbirdSql.Core.Extensions;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Core.Properties;
using BlackbirdSql.Sys.Enums;
using BlackbirdSql.Sys.Extensions;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;



namespace BlackbirdSql;


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
public sealed class RctManager : RunningConnectionTable
{


	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - RctManager
	// ---------------------------------------------------------------------------------


	/// <summary>
	/// Singleton .ctor.
	/// </summary>
	private RctManager() : base()
	{
		Diag.ThrowIfInstanceExists(_Instance);
	}

	private static RctManager Instance => (RctManager)_Instance;


	/// <summary>
	/// Gets the instance of the RctManager for this session.
	/// We do not auto-create to avoid instantiation confusion.
	/// Use CreateInstance() to instantiate.
	/// </summary>
	private static RctManager EnsuredInstance
	{
		get
		{
			if (ApcManager.IdeShutdownState)
			{
				if (_Instance != null)
					Delete();
				return null;
			}

			if (_Instance != null && Loaded)
				return (RctManager)_Instance;

			_Instance ??= new RctManager();

			if (!Loaded || Loading)
				ResolveDeadlocksAndEnsureLoaded(false);

			return Loaded ? (RctManager)_Instance : null;

		}
	}


	public static void Delete()
	{
		// Evs.Trace(typeof(RctManager), nameof(Delete));

		((RctManager)_Instance)?.Dispose(true);
		_Instance = null;
	}

	/// <summary>
	/// Disposal of Rct at the end of an IDE session.
	/// </summary>
	public override void Dispose()
	{
		Dispose(true);
	}



	/// <summary>
	/// Clears the Rct of non-persistent connections.
	/// </summary>
	public static void ClearVolatileConnections()
	{
		((RctManager)_Instance)?.InternalClearVolatileConnections();
	}


	protected override void Dispose(bool disposing) => base.Dispose(disposing);




	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Constants - RctManager
	// =========================================================================================================


	private const string C_Wizard = "{C99AEA30-8E36-4515-B76F-496F5A48A6AA}";
	/*
	private const string C_SolutionExplorer = "{3AE79031-E1BC-11D0-8F78-00A0C9110057}";
	private const string C_XsdDatasetDesigner = "{9DDABE98-1D02-11D3-89A1-00C04F688DDE}";
	private const string C_XsdDatasetDesigner2 = "{74946810-37A0-11D2-A273-00C04F8EF4FF}";
	*/


	#endregion Constants





	// =========================================================================================================
	#region Fields - RctManager
	// =========================================================================================================


	private static int _InitializeServerExplorerModelsEvsIndex = -1;
	private static int _InitializingExplorerModelsCardinal = 0;
	private static bool _ServerExplorerModelsInitialized = false;

	private static int _EventConnectionDialogCardinal = 0;
	private static bool _SessionConnectionSourceActive = false;


	private static readonly string _S_SEToolWindow = VSConstants.StandardToolWindows.ServerExplorer.ToString("B").ToUpper();
	private static readonly string _S_VsMdPropertyBrowser = VSConstants.StandardToolWindows.VSMDPropertyBrowser.ToString("B").ToUpper();
	private static readonly string _S_VsTextBuffer = VS.CLSID_VsTextBuffer.ToString("B").ToUpper();
	/*
	private static readonly string _S_DataSourceToolWindow = VSConstants.StandardToolWindows.DataSource.ToString("B").ToUpper();
	private static readonly string _S_ErrorList = VSConstants.StandardToolWindows.ErrorList.ToString("B").ToUpper();
	private static readonly string _S_OutputToolWindow = VSConstants.StandardToolWindows.Output.ToString("B").ToUpper();
	private static readonly string _S_VsTextBuffer = VS.CLSID_VsTextBuffer.ToString("B").ToUpper();
	*/


	#endregion Fields




	// =========================================================================================================
	#region Property accessors - RctManager
	// =========================================================================================================


	public static bool InitializingExplorerModels => _InitializingExplorerModelsCardinal > 0;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The table of all registered databases of the current data provider located in
	/// Server Explorer, FlameRobin and the Solution's Project settings, and any
	/// volatile unique connections defined in SqlEditor.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static DataTable Databases => EnsuredInstance?.InternalDatabases;


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
	public static DataTable Servers => EnsuredInstance?.InternalServers;



	public static string GlyphFormat = Resources.RctGlyphFormat;

	public static bool Loaded => _Instance != null && ((RctManager)_Instance).InternalLoaded;

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns true if the Rct is physically in a loading state and both sync and
	/// async tasks are not in Inactive or Shutdown states.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static bool Loading => _Instance != null && ((RctManager)_Instance).InternalLoading;



	private bool InternalLoaded => _InternalLoaded;

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns an IEnumerable of the registered datasets else null if the rct is in a
	/// shutdown state.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static IEnumerable<string> AdornedQualifiedNames
	{
		get
		{
			if (!Loaded && !ExplorerHasConnections)
				return [];
			return EnsuredInstance?.InternalAdornedQualifiedNames ?? [];
		}
	}


	public static IEnumerable<string> AdornedQualifiedTitles
	{
		get
		{
			if (!Loaded && !ExplorerHasConnections)
				return [];
			return EnsuredInstance?.InternalAdornedQualifiedTitles ?? [];
		}
	}


	public static bool ExplorerHasConnections
	{
		get
		{
			return (ApcManager.ExplorerConnectionManager?.Connections?.Count ?? 0) > 0;
		}
	}

	public static bool SessionConnectionSourceActive
	{
		get { return _SessionConnectionSourceActive; }
		set { _SessionConnectionSourceActive = value; }
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns the sequential stamp of the last attempt to modify the
	/// <see cref="RunningConnectionTable"/> and is used to perform drift detection
	/// to ensure uniformity of connections and <see cref="Csb"/>s globally across
	/// the extension.
	/// </summary>
	/// <remarks>
	/// Objects that use connections or <see cref="Csb"/>s record the stamp at the
	/// time their connection or Csa is created.
	/// Whenever the connection or Csa is accessed it must perform drift detection and
	/// compare it's saved stamp value against the Rct's stamp. If they differ it must
	/// check if it's connection requires updating or Csa requires renewing and also
	/// always update it's stamp to the most recent value irrelevant of any updates.
	/// If a Csa requires drift detection the built-in stamp drift detection should be
	/// enabled and used.
	/// </remarks>
	// ---------------------------------------------------------------------------------
	public static long Stamp => _Stamp;



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns the global shutdown state.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static bool ShutdownState => ApcManager.IdeShutdownState
		|| (Instance?.InternalShutdownState ?? false);


	public static char EdmDatasetGlyph => SystemData.C_EdmDatasetGlyph;
	public static char ProjectDatasetGlyph => SystemData.C_ProjectDatasetGlyph;
	public static char SessionDatasetGlyph => SystemData.C_SessionDatasetGlyph;
	public static char UtilityDatasetGlyph => SystemData.C_UtilityDatasetGlyph;

	public static char EdmTitleGlyph => SystemData.C_EdmTitleGlyph;
	public static char ProjectTitleGlyph => SystemData.C_ProjectTitleGlyph;
	public static char SessionTitleGlyph => SystemData.C_SessionTitleGlyph;
	public static char UtilityTitleGlyph => SystemData.C_UtilityTitleGlyph;


	#endregion Property accessors




	// =========================================================================================================
	#region Methods - RctManager
	// =========================================================================================================



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Clones and returns a Csb instance from a registered connection using an
	/// existing Csb object else null if the rct is in a shutdown state.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static Csb CloneRegistered(IBsCsb csb)
	{
		if (csb == null)
			return null;

		if (EnsuredInstance == null)
			return null;

		string connectionUrl = csb.Moniker;


		if (!Instance.TryGetHybridRowValue(connectionUrl, EnRctKeyType.ConnectionUrl, out DataRow row))
		{
			KeyNotFoundException ex = new KeyNotFoundException($"Connection url not found in RunningconnectionTable for key: {connectionUrl}");
			Diag.Ex(ex);
			throw ex;
		}


		return new Csb((string)row[CoreConstants.C_KeyExConnectionString]);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Clones and returns instance from a registered connection using an IDbConnection
	/// else null if the rct is in a shutdown state..
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static Csb CloneRegistered(IDbConnection connection)
	{
		if (connection == null)
		{
			ArgumentNullException ex = new(nameof(connection));
			Diag.Ex(ex);
			throw ex;
		}

		if (EnsuredInstance == null)
			return null;

		if (!Instance.TryGetHybridRowValue(connection.ConnectionString, EnRctKeyType.ConnectionString, out DataRow row))
		{
			KeyNotFoundException ex = new KeyNotFoundException($"Connection url not found in RunningconnectionTable for ConnectionString: {connection.ConnectionString}");
			Diag.Ex(ex);
			throw ex;
		}


		return new Csb((string)row[CoreConstants.C_KeyExConnectionString]);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Clones and returns instance from a registered connection using an IDbConnection
	/// else null if the rct is in a shutdown state..
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static Csb CloneRegistered(IVsDataExplorerConnection root)
	{
		if (root == null)
		{
			ArgumentNullException ex = new(nameof(root));
			Diag.Ex(ex);
			throw ex;
		}

		if (EnsuredInstance == null)
			return null;

		if (!Instance.TryGetHybridRowValue(root.DecryptedConnectionString(), EnRctKeyType.ConnectionString, out DataRow row))
		{
			KeyNotFoundException ex = new KeyNotFoundException($"Connection url not found in RunningconnectionTable for ConnectionString: {root.DecryptedConnectionString()}.");
			Diag.Ex(ex);
			throw ex;
		}


		return new Csb((string)row[CoreConstants.C_KeyExConnectionString]);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Clones and returns an instance from a registered connection using a Server
	/// Explorer node else null if the rct is in a shutdown state.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static Csb CloneRegistered(IVsDataExplorerNode node)
	{
		if (node == null)
		{
			ArgumentNullException ex = new(nameof(node));
			Diag.Ex(ex);
			throw ex;
		}

		if (EnsuredInstance == null)
			return null;

		Csb csa = new(node, false);

		return CloneRegistered(csa);
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
	public static Csb CloneRegistered(string hybridKey, EnRctKeyType keyType)
	{
		if (hybridKey == null)
		{
			ArgumentNullException ex = new(nameof(hybridKey));
			Diag.Ex(ex);
			throw ex;
		}

		if (EnsuredInstance == null)
			return null;

		if (!Instance.TryGetHybridRowValue(hybridKey, keyType, out DataRow row))
			return null;

		return new Csb((string)row[CoreConstants.C_KeyExConnectionString]);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Clones a Csb instance from a registered connection using an
	/// IBsCsb, else null if the rct is in a shutdown state.
	/// Finally registers the csa for validity state checks.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static Csb CloneVolatile(IBsCsb csb)
	{
		if (csb == null)
		{
			ArgumentNullException ex = new(nameof(csb));
			Diag.Ex(ex);
			throw ex;
		}

		if (EnsuredInstance == null)
			return null;

		return CloneRegistered(csb);
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Clones a Csb instance from a registered connection using an
	/// IDbConnection , else null if the rct is in a shutdown state.
	/// Finally registers the csa for validity state checks.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static Csb CloneVolatile(IDbConnection connection)
	{
		if (connection == null)
		{
			ArgumentNullException ex = new(nameof(connection));
			Diag.Ex(ex);
			throw ex;
		}

		if (EnsuredInstance == null)
			return null;

		return CloneRegistered(connection);
	}






	// -------------------------------------------------------------------------------------------
	/// <summary>
	/// Increments the <see cref="_EventsCardinal"/> counter when execution enters an external
	/// event handler to prevent recursion.
	/// </summary>
	/// <returns>
	/// Returns false if an event has already been entered else true if it is safe to enter.
	/// </returns>
	// -------------------------------------------------------------------------------------------
	public static bool ExternalEventSinkEnter(bool test = false, bool force = false) =>
		RctEventSink.EventEnter(test, force);



	// ---------------------------------------------------------------------------------------
	/// <summary>
	/// Decrements the RctEventSink Event counter that was previously
	/// incremented by <see cref="ExternalEventSinkEnter"/>.
	/// </summary>
	// ---------------------------------------------------------------------------------------
	public static void ExternalEventSinkExit() => RctEventSink.EventExit();



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the current connection dialog that is active else EnConnectionSource.None.
	/// This is messy but there really is no other way of doing this. Also attempting
	/// to do this on the UI thread only is impossible without creating deadlocks.
	/// We're only peeking at the DTE values and that doesn't seem to cause any issues
	/// that cannot be handled gracefully.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static EnConnectionSource ConnectionSource => GetConnectionSource();



	private static readonly Dictionary<string, string> _ActiveWindowObjectKinds = new()
	{
		{ "SEToolWindow", _S_SEToolWindow },
		{ "VsMdPropertyBrowser", _S_VsMdPropertyBrowser },
		{ "VsTextBuffer", _S_VsTextBuffer },
		{ "Wizard", C_Wizard },
		{ "DataSourceToolWindow", VSConstants.StandardToolWindows.DataSource.ToString("B") },
		{ "OutputToolWindow", VSConstants.StandardToolWindows.Output.ToString("B") },
		{ "SolutionExplorer", "{3AE79031-E1BC-11D0-8F78-00A0C9110057}" },
		{ "ErrorList", VSConstants.StandardToolWindows.ErrorList.ToString("B") },
		{ "XsdDatsetDesigner", "{9DDABE98-1D02-11D3-89A1-00C04F688DDE}" }
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Updates a connection string with the registration properties of it's unique
	/// registered connection, if it exists.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static string AdornConnectionStringFromRegistration(string connectionString)
	{
		if (connectionString == null)
			return "";

		if (ShutdownState || EnsuredInstance == null)
			return connectionString;


		Csb csa = new(connectionString);


		if (!Instance.TryGetHybridRowValue(csa.Moniker, EnRctKeyType.ConnectionUrl, out DataRow row))
			return connectionString;

		object colObject = row[CoreConstants.C_KeyExDatasetKey];
		if (!Cmd.IsNullValue(colObject))
			csa.DatasetKey = (string)colObject;

		colObject = row[CoreConstants.C_KeyExConnectionKey];
		if (!Cmd.IsNullValue(colObject))
			csa.ConnectionKey = (string)colObject;

		colObject = row[SysConstants.C_KeyExConnectionName];
		if (!Cmd.IsNullValue(colObject))
			csa.ConnectionName = (string)colObject;

		if (!string.IsNullOrEmpty(csa.ConnectionName))
			colObject = null;
		else
			colObject = row[SysConstants.C_KeyExDatasetName];
		if (!Cmd.IsNullValue(colObject))
			csa.DatasetName = (string)colObject;

		colObject = row[CoreConstants.C_KeyExConnectionSource];
		if (!Cmd.IsNullValue(colObject))
			csa.ConnectionSource = (EnConnectionSource)(int)colObject;

		return csa.ConnectionString;
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Updates a connection string with the registration properties of it's unique
	/// registered connection, if it exists.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static string AdornConnectionStringFromRegistration(IDbConnection connection)
	{
		if (connection == null)
		{
			ArgumentNullException ex = new(nameof(connection));
			Diag.Ex(ex);
			throw ex;
		}

		string connectionString;

		try
		{
			connectionString = connection.ConnectionString;
		}
		catch (Exception ex)
		{
			Diag.Expected(ex);
			return "";
		}

		return AdornConnectionStringFromRegistration(connectionString);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets (on the ui thread) the current connection dialog that is active else
	/// EnConnectionSource.None.
	/// This is messy but there really is no other way of doing this.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private static EnConnectionSource GetConnectionSource()
	{
		EnConnectionSource connectionSource = GetConnectionSourceImpl();

		// Evs.Trace(typeof(RctManager), nameof(GetConnectionSource), "ConnectionSource: {0}", connectionSource);

		return connectionSource;
	}




	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets (on the ui thread) the current connection dialog that is active else
	/// EnConnectionSource.None.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private static EnConnectionSource GetConnectionSourceImpl()
	{
		// Evs.Trace(typeof(RctManager), nameof(GetConnectionSourceImpl));

		// Definitely None.
		if (ApcManager.IdeShutdownState)
			return EnConnectionSource.Unknown;

		// Definitely Session.
		if (_SessionConnectionSourceActive)
			return EnConnectionSource.Session;


		// Definitely ServerExplorer.
		if (InitializingExplorerModels)
			return EnConnectionSource.ServerExplorer;

		// We are not in the connection dialog so the ConnectionSource is not needed.
		if (EventConnectionDialogEnter(true))
			return EnConnectionSource.Unknown;


		// We're just peeking.
		// Diag.ThrowIfNotOnUIThread();

		// At this point we're definitely in the connection dialog and we can't be guessing...
		//
		// 1. ServerExplorer: We're in the SE tool window. This has to be known or we have a bug.
		// 2. Session: We're in a Session dialog. This we will know because we would have launched it.
		// 3. Application: We're editing settings. This we will know because the settings window will
		//		be active and it will be in the parent hierarchy of the focused control.
		// 4. EntityDataModel: We're in the .edmx wizard or creating a datasource from the Project menu
		//		or some unknown extension.
		//		Applies to all other cases.In this case our active window could be anything.


		EnConnectionSource result;

		string objectKind = ApcManager.ActiveWindowObjectKind;
		// string objectType = ApcManager.ActiveWindowObjectType;

		// string info = $"\n\tConnection Dialog: ActiveWindowObjectType: {objectType}, ActiveWindowType: {ApcManager.ActiveWindowType}, ActiveWindowObjectKind: {KindName(objectKind)}|{objectKind}.";


		// Definitely ServerExplorer
		if (objectKind == _S_SEToolWindow)
		{
			result = EnConnectionSource.ServerExplorer;
			// Evs.Debug(typeof(RctManager), "GetConnectionSourceImpl()", $"ConnectionSource: {result}.{info}");
			return result;
		}


		// Most likely Application.
		if (objectKind == _S_VsMdPropertyBrowser)
		{
			result = EnConnectionSource.Application;
			// Evs.Debug(typeof(RctManager), "GetConnectionSourceImpl()", $"ConnectionSource: {result}.{info}");
			return result;
		}

		// Definitely EntityDataModel 
		if (objectKind == C_Wizard)
		{
			result = EnConnectionSource.EntityDataModel;
			// Evs.Debug(typeof(RctManager), "GetConnectionSourceImpl()", $"ConnectionSource: {result}.{info}");
			return result;
		}


		string extension = ApcManager.ActiveDocumentExtension;
		bool isSettings = !string.IsNullOrEmpty(extension) && extension.Equals(".settings", StringComparison.InvariantCultureIgnoreCase);

		if (isSettings)
		{
			// Hack: The ConnectionSource is 'Application' only if the ActiveWindow is
			// within the parent hierarchy of the focused control.

			isSettings = false;

			IntPtr activeHwnd = ApcManager.ActiveWindowHandle;
			// IntPtr fgHwnd = Sys.Native.GetForegroundWindow();
			IntPtr focusHwnd = Sys.Native.GetFocus();
			IntPtr parentHwnd = IntPtr.Zero;

			// string parentHwnds = "";

			if (focusHwnd != IntPtr.Zero)
				parentHwnd = Sys.Native.GetParent(focusHwnd);

			while (parentHwnd != IntPtr.Zero)
			{
				// if (parentHwnds != "")
				//	parentHwnds += ":";

				// parentHwnds += parentHwnd;

				if (parentHwnd == activeHwnd)
				{
					// parentHwnds += "[Active]";
					isSettings = true;
					break;
				}

				// if (parentHwnd == fgHwnd)
				//	parentHwnds += "[Fg]";
				// if (parentHwnd == focusHwnd)
				//	parentHwnds += "[Focus]";

				IntPtr pparentHwnd = Sys.Native.GetParent(parentHwnd);

				if (pparentHwnd == IntPtr.Zero || pparentHwnd == parentHwnd)
					break;

				parentHwnd = pparentHwnd;
			}

			// info = $"\n\tConnection Dialog: ActiveWindowObjectType: {objectType}, ActiveWindowType: {ApcManager.ActiveWindowType}, ActiveWindowObjectKind: {KindName(objectKind)}|{objectKind}, ActiveDocumentExtension: {extension}, ActiveHwnd: {activeHwnd}, FgHwnd: {fgHwnd}, FocusHwnd: {focusHwnd}, ParentHwnds: {parentHwnds}.";
		}

		result = isSettings ? EnConnectionSource.Application : (objectKind == _S_VsTextBuffer
				? EnConnectionSource.DataSource : EnConnectionSource.EntityDataModel);

		// Probably nothing
		if (objectKind == "" /* && objectType == "" */)
		{
			Diag.Ex(new ApplicationException($"ConnectionSource not discovered. Using: {result.ToString().ToUpper()}. ActiveWindowType: {ApcManager.ActiveWindowType}. ActiveWindowObjectType is null, ActiveWindowObjectKind is null."));
		}

		// Evs.Debug(typeof(RctManager), "GetConnectionSourceImpl()", $"ConnectionSource: {result}.{info}");

		return result;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets a connection string given the ConnectionUrl.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static string GetConnectionString(string connectionUrl)
	{
		if (EnsuredInstance == null)
			return null;

		if (!Instance.TryGetHybridRowValue(connectionUrl, EnRctKeyType.ConnectionUrl, out DataRow row))
			return null;

		object @object = row[CoreConstants.C_KeyExConnectionString];

		return @object == DBNull.Value ? null : @object?.ToString();
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets a connection DatasetKey given the ConnectionUrl.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private static string GetDatasetKey(string connectionUrl)
	{
		if (EnsuredInstance == null)
			return null;

		if (!Instance.TryGetHybridRowValue(connectionUrl, EnRctKeyType.ConnectionUrl, out DataRow row))
			return null;

		object @object = row[CoreConstants.C_KeyExDatasetKey];

		return @object == DBNull.Value ? null : @object?.ToString();
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Creates and returns an instance from a registered connection using a
	/// ConnectionString else registers a new connection if none exists else null if the
	/// rct is in a shutdown state..
	/// </summary>
	// ---------------------------------------------------------------------------------
	private static Csb EnsureInstance(string connectionString, EnConnectionSource source)
	{
		if (connectionString == null)
		{
			ArgumentNullException ex = new(nameof(connectionString));
			Diag.Ex(ex);
			throw ex;
		}

		if (ShutdownState || EnsuredInstance == null)
			return null;


		if (Instance.TryGetHybridRowValue(connectionString, EnRctKeyType.ConnectionString, out DataRow row))
			return new Csb((string)row[CoreConstants.C_KeyExConnectionString]);

		// Calls to this method expect a registered connection. If it doesn't exist it means
		// we're creating a new configured connection.

		Csb csa = new(connectionString);
		string datasetName = csa.DisplayDatasetName;


		// If the proposed key matches the proposed generated one, drop it.

		if (csa.ContainsKey(SysConstants.C_KeyExConnectionName)
			&& !string.IsNullOrWhiteSpace(csa.ConnectionName)
			&& (SysConstants.S_DatasetKeyFormat.Fmt(csa.ServerName, datasetName) == csa.ConnectionName
			|| SysConstants.S_DatasetKeyAlternateFormat.Fmt(csa.ServerName, datasetName) == csa.ConnectionName))
		{
			// csa.Remove(SysConstants.C_KeyExConnectionName);
		}

		Instance.RegisterUniqueConnection(csa.ConnectionName, datasetName, source, ref csa);

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
	public static Csb EnsureVolatileInstance(IDbConnection connection)
	{
		if (connection == null)
		{
			ArgumentNullException ex = new(nameof(connection));
			Diag.Ex(ex);
			throw ex;
		}

		if (EnsuredInstance == null)
			return null;


		Csb csa;


		if (Instance.TryGetHybridRowValue(connection.ConnectionString, EnRctKeyType.ConnectionString, out DataRow row))
		{
			return new((string)row[CoreConstants.C_KeyExConnectionString]);
		}

		// New registration.

		// Evs.Trace(typeof(Csb), nameof(EnsureVolatileInstance), "Could NOT find registered DataRow for dbConnectionString: {0}.", connection.ConnectionString);

		csa = new(connection);
		string datasetName = csa.DisplayDatasetName;


		if (!string.IsNullOrWhiteSpace(csa.ConnectionName)
			&& (SysConstants.S_DatasetKeyFormat.Fmt(csa.ServerName, datasetName) == csa.ConnectionName
			|| SysConstants.S_DatasetKeyAlternateFormat.Fmt(csa.ServerName, datasetName) == csa.ConnectionName))
		{
			csa.ConnectionName = SysConstants.C_DefaultExConnectionName;
		}

		if (Instance.RegisterUniqueConnection(csa.ConnectionName, datasetName, ConnectionSource, ref csa))
		{
			csa.RefreshDriftDetectionState();
		}


		return csa;
	}



	public static EnConnectionSource GetRegisteredConnectionSource(string connectionUrl)
	{
		if (connectionUrl == null)
		{
			ArgumentNullException ex = new(nameof(connectionUrl));
			Diag.Ex(ex);
			throw ex;
		}

		if (EnsuredInstance == null)
			return EnConnectionSource.Undefined;

		if (!Instance.TryGetHybridRowValue(connectionUrl, EnRctKeyType.ConnectionUrl, out DataRow row))
		{
			return EnConnectionSource.Undefined;
		}

		return (EnConnectionSource)(int)row[CoreConstants.C_KeyExConnectionSource];
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the registered unmangled , uniformly cased Server/DataSource name of the
	/// provided name if it exists else returns the provided serverName.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static (string, int) GetRegisteredServer(string serverName)
	{
		if (ShutdownState || !(Instance?.InternalLoaded ?? false))
			return (serverName, -1);

		return Instance.InternalGetRegisteredServer(serverName);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the registered unmangled , uniformly cased Server/DataSource name of the
	/// provided name if it exists else returns the provided serverName.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static (string, int) GetRegisteredServer(string serverName, int port)
	{
		if (ShutdownState || !(Instance?.InternalLoaded ?? false))
			return (serverName, port);

		return Instance.InternalGetRegisteredServer(serverName, port);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// [Launch async]: Task to perform explorer connection advise events.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private static bool InitializeServerExplorerModelsAsyin()
	{
		// Evs.Trace(typeof(RctManager), nameof(AsyncInitializeServerExplorerModels));

		if (_ServerExplorerModelsInitialized)
			return false;

		_ServerExplorerModelsInitialized = true;

		// Sanity checks.

		// The following for brevity.
		CancellationToken cancelToken = default;

		async Task<bool> payloadAsync() => await InitializeServerExplorerModelsAsync(cancelToken);

		// Evs.Debug(GetType(), nameof(AsyncInitializeServerExplorerModels), "Queueing InitializeServerExplorerModelsAsync.");

		// Run on new thread in thread pool.
		// Fire and forget.

		_ = Task.Factory.StartNew(payloadAsync, default, TaskCreationOptions.PreferFairness, TaskScheduler.Default);

		// Evs.Debug(GetType(), nameof(AsyncInitializeServerExplorerModels), "Queued InitializeServerExplorerModelsAsync.");

		return true;
	}



	public static bool IsRegistered(IVsDataExplorerConnection root)
	{
		if (root == null)
		{
			ArgumentNullException ex = new(nameof(root));
			Diag.Ex(ex);
			throw ex;
		}

		if (EnsuredInstance == null)
			return false;

		if (!Instance.TryGetHybridRowValue(root.DecryptedConnectionString(), EnRctKeyType.ConnectionString, out _))
		{
			return false;
		}

		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Invalidates the Rct for active static CsbAgents so that they can preform
	/// validation checkS.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static void Invalidate()
	{
		if (_Instance == null)
			return;


		Instance.InternalInvalidate();

		// Evs.Trace(typeof(RctManager), nameof(Invalidate), "New stamp: {0}", Stamp);
	}


	// [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used for debugging")]
	private static string KindName(string kind)
	{
		foreach (KeyValuePair<string, string> pair in _ActiveWindowObjectKinds)
		{
			if (kind.Equals(pair.Value, StringComparison.OrdinalIgnoreCase))
				return pair.Key;
		}

		return "Unknown";

	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// [Async on UI thread]: Loads a project's App.Config Settings and EDM connections.
	/// </summary>
	/// <param name="probject">The <see cref="EnvDTE.Project"/>.</param>
	/// <remarks>
	/// This method only applies to projects late loaded or added after a solution has
	/// completed loading.
	/// </remarks>
	// ---------------------------------------------------------------------------------
	public static bool LoadApplicationConnectionsAsyui(object probject)
	{
		// Evs.Trace(typeof(RctManager), nameof(LoadProjectConnections));

		if (!(Instance?.InternalLoaded ?? false) || ShutdownState)
			return false;

		return EnsuredInstance.InternalLoadApplicationConnectionsAsyui(probject);
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
		return ResolveDeadlocksAndEnsureLoaded(true);
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
		return ResolveDeadlocksAndEnsureLoaded(false);
	}






	// ---------------------------------------------------------------------------------
	/// <summary>
	/// [Async launch]: Loads server explorer models on thread pool.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private static async Task<bool> InitializeServerExplorerModelsAsync(CancellationToken cancelToken)
	{
		if (cancelToken.Cancelled() || ApcManager.IdeShutdownState)
			return false;

		bool result = false;

		_InitializingExplorerModelsCardinal++;

		_InitializeServerExplorerModelsEvsIndex = Evs.Start(typeof(RctManager),
			nameof(InitializeServerExplorerModelsAsync), "", nameof(InitializeServerExplorerModelsAsync),
			_InitializeServerExplorerModelsEvsIndex);

		try
		{

			IVsDataExplorerConnectionManager manager = ApcManager.ExplorerConnectionManager;

			Guid clsidProvider = new(SystemData.C_ProviderGuid);
			IVsDataExplorerConnection explorerConnection;

			foreach (KeyValuePair<string, IVsDataExplorerConnection> pair in manager.Connections)
			{
				if (!(clsidProvider == pair.Value.Provider))
					continue;

				explorerConnection = pair.Value;

				RctEventSink.InitializeServerExplorerModel(explorerConnection);
			}

			result = true;
		}
		catch (Exception ex)
		{
			Diag.Ex(ex);
		}
		finally
		{
			_InitializingExplorerModelsCardinal--;

			Evs.Stop(typeof(RctManager), nameof(InitializeServerExplorerModelsAsync), "",
				nameof(InitializeServerExplorerModelsAsync), _InitializeServerExplorerModelsEvsIndex);
		}


		DbProviderFactoriesEx.InvalidatedProviderFactoryRecovery();

		return await Task.FromResult(result);

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Modifies the name and connection information of Server Explorer's internal
	/// IVsDataExplorerConnectionManager connection entry.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private static bool ModifyServerExplorerConnection(IVsDataExplorerConnection explorerConnection,
		ref Csb csa, bool modifyExplorerConnection)
	{
		// Evs.Trace(typeof(RctManager), nameof(ModifyServerExplorerConnection),
		//	"csa.ConnectionString: {0}, se.DecryptedConnectionString: {1}, modifyExplorerConnection: {2}.",
		//	csa.ConnectionString, explorerConnection.Connection.DecryptedConnectionString(), modifyExplorerConnection);

		EnsuredInstance?.EventEnter(false, true);

		// finally is unreliable.
		try
		{

			Csb seCsa = new(explorerConnection.Connection.DecryptedConnectionString(), false);
			string connectionKey = explorerConnection.GetConnectionKey(true);

			if (string.IsNullOrWhiteSpace(csa.ConnectionKey) || csa.ConnectionKey != connectionKey)
			{
				csa.ConnectionKey = connectionKey;
			}


			// Sanity check. Should already be done.
			// Perform a deep validation of the updated csa to ensure an update
			// is in fact required.
			bool updateRequired = !AbstractCsb.AreEquivalent(csa, seCsa, Csb.DescriberKeys, true);

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
				explorerConnection.DisplayName = csa.DatasetKey;

				if (!updateRequired)
					return true;
			}

			if (!modifyExplorerConnection)
				return false;

			// An update is required...

			explorerConnection.Connection.EncryptedConnectionString = DataProtection.EncryptString(csa.ConnectionString);
			explorerConnection.ConnectionNode.Select();

			return true;
		}
		catch
		{
			return false;
		}
		finally
		{
			Instance?.EventExit();
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
		if (ShutdownState || !(Instance?.InternalLoaded ?? false))
			return serverName;

		return Instance.InternalRegisterServer(serverName, port);
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
	private static bool ResolveDeadlocksAndEnsureLoaded(bool asynchronous)
	{
		// Evs.Trace(typeof(RctManager), nameof(ResolveDeadlocksAndLoad), "Instance.Initialized: {0}", Instance._Rct == null);
		if (ApcManager.IdeShutdownState)
			return false;

		InitializeServerExplorerModelsAsyin();

		_Instance ??= new RctManager();

		RctManager instance = (RctManager)_Instance;

		return instance.InternalResolveDeadlocksAndEnsureLoaded(asynchronous);
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

			foreach (DataRow row in EnsuredInstance.InternalConnectionsTable.Rows)
			{
				datasetKey = row[CoreConstants.C_KeyExDatasetKey];

				if (datasetKey == DBNull.Value || string.IsNullOrWhiteSpace((string)datasetKey))
					continue;
				str += "\n--------------------------------------------------------------------------------------";
				str += $"\nDATASETKEY: {(string)row[CoreConstants.C_KeyExDatasetKey]}, ConnectionUrl: {(string)row[CoreConstants.C_KeyExConnectionUrl]}";
				str += "\n\t------------------------------------------";
				str += "\n\t";

				foreach (DataColumn col in Instance.InternalDatabases.Columns)
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

			foreach (KeyValuePair<string, int> pair in Instance)
			{
				str += $"\n\t{pair.Key}: {pair.Value}";
			}

			Evs.Info(typeof(RctManager), "TraceRct()", str);
		}
		catch (Exception ex)
		{
			Diag.Ex(ex);
			throw ex;
		}
	}





	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Updates an existing registered connection using the provided connection string.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static Csb UpdateOrRegisterConnection(string connectionString,
		EnConnectionSource source, bool addExplorerConnection, bool modifyExplorerConnection)
	{
		if (EnsuredInstance == null)
			return null;


		Csb csa = Instance.InternalUpdateRegisteredConnection(connectionString, source, false);


		// If it's null force a creation.
		csa ??= EnsureInstance(connectionString, source);

		// Update the SE.
		UpdateServerExplorer(ref csa, addExplorerConnection, modifyExplorerConnection);
		return csa;
	}


	public static Csb UpdateRegisteredConnection(string connectionString, EnConnectionSource source, bool forceOwnership)
	{
		if (!Loaded)
			return null;

		return Instance.InternalUpdateRegisteredConnection(connectionString, source, forceOwnership);
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
	private static bool UpdateServerExplorer(ref Csb csa,
		bool addExplorerConnection, bool modifyExplorerConnection)
	{
		// Evs.Trace(typeof(RctManager), nameof(UpdateServerExplorer), "csa.ConnectionString: {0}, createServerExplorerConnection: {1}, modifyExplorerConnection: {2}.", csa.ConnectionString, addExplorerConnection, modifyExplorerConnection);

		csa.ConnectionSource = EnConnectionSource.ServerExplorer;

		IVsDataExplorerConnectionManager manager = ApcManager.ExplorerConnectionManager;

		(_, IVsDataExplorerConnection explorerConnection) = manager.SearchExplorerConnectionEntry(csa.ConnectionString, false);

		if (explorerConnection != null)
			return ModifyServerExplorerConnection(explorerConnection, ref csa, modifyExplorerConnection);

		if (!addExplorerConnection)
			return false;


		csa.ConnectionKey = csa.DatasetKey;

		EnsuredInstance.EventEnter(false, true);

		try
		{
			explorerConnection = manager.AddConnection(csa.DatasetKey, new(SystemData.C_ProviderGuid), csa.ConnectionString, false);

			RctEventSink.InitializeServerExplorerModel(explorerConnection);

			explorerConnection.ConnectionNode.Select();
		}
		finally
		{
			Instance.EventExit();
		}

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
		string proposedConnectionName, Csb csa)
	{
		if (Instance == null)
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
		// connection, and we lose the DatasetName if it existed.
		// A node changed event is raised which we pick up here, and we perform a
		// reverse repair.

		if (csa.ContainsKey(CoreConstants.C_KeyExEdmx) || csa.ContainsKey(CoreConstants.C_KeyExEdmu))
		{
			if (csa.ContainsKey(CoreConstants.C_KeyExEdmu))
			{
				// Clear out any UIHierarchyMarshaler or DataSources ToolWindow ConnectionString identifiers.
				csa.Remove(CoreConstants.C_KeyExEdmu);
				csa.Remove(CoreConstants.C_KeyExEdmx);

				// Evs.Trace(typeof(RctManager), nameof(ValidateAndUpdateExplorerConnectionRename), "\nEDMU repairString: {0}.", csa.ConnectionString);

				UpdateOrRegisterConnection(csa.ConnectionString, EnConnectionSource.ServerExplorer, false, true);
			}
			else
			{
				string storedConnectionString = GetConnectionString(csa.Moniker);

				if (storedConnectionString == null)
				{
					ApplicationException ex = new($"ExplorerConnection rename failed. Possibly corrupted by the EDMX wizard. Proposed connection name: {proposedConnectionName}, Explorer connection string: {csa.ConnectionString}.");
					Diag.Ex(ex);
					throw ex;
				}

				// Evs.Trace(typeof(RctManager), nameof(ValidateAndUpdateExplorerConnectionRename), "EDMX repairString: {0}.", storedConnectionString);

				UpdateOrRegisterConnection(storedConnectionString, EnConnectionSource.ServerExplorer, false, true);
			}

			return;

		}

		if (proposedConnectionName == csa.DatasetKey)
			return;


		// Evs.Trace(typeof(RctManager), nameof(ValidateAndUpdateExplorerConnectionRename), "proposedConnectionName: {0}, connectionString: {1}.", proposedConnectionName, csa.ConnectionString);

		string msg;
		string caption;

		// Sanity check.
		if (proposedConnectionName.StartsWith(NativeDb.Scheme))
		{
			if (csa.ContainsKey(CoreConstants.C_DefaultExDatasetKey))
			{
				Instance.EventEnter(false, true);
				explorerConnection.DisplayName = csa.DatasetKey;
				Instance.EventExit();

				caption = ControlsResources.RctManager_CaptionInvalidConnectionName;
				msg = ControlsResources.RctManager_TextInvalidConnectionName.Fmt(NativeDb.Scheme, proposedConnectionName);
				MessageCtl.ShowX(msg, caption, MessageBoxButtons.OK);

				return;
			}

			proposedConnectionName = null;
		}


		bool createServerExplorerConnection = false;
		string connectionUrl = csa.Moniker;
		string proposedDatasetName = csa.DatasetName;
		string dataSource = csa.ServerName;
		string dataset = csa.Dataset;


		// Check whether the connection name will change.
		Instance.GenerateUniqueDatasetKey(EnConnectionSource.ServerExplorer, ref proposedConnectionName,
			ref proposedDatasetName, dataSource, dataset, connectionUrl, connectionUrl,
			ref createServerExplorerConnection, out _, out _, out string uniqueDatasetKey,
			out string uniqueConnectionName, out string uniqueDatasetName);

		// Evs.Trace(typeof(RctManager), nameof(ValidateAndUpdateExplorerConnectionRename), "GenerateUniqueDatasetKey results: proposedConnectionName: {0}, proposedDatasetName: {1}, dataSource: {2}, dataset: {3}, uniqueDatasetKey: {4}, uniqueConnectionName: {5}, uniqueDatasetName: {6}.",
		//	proposedConnectionName, proposedDatasetName, dataSource, dataset, uniqueDatasetKey ?? "Null", uniqueConnectionName ?? "Null", uniqueDatasetName ?? "Null");

		if (!string.IsNullOrEmpty(uniqueConnectionName))
		{
			caption = ControlsResources.RctManager_CaptionConnectionNameConflict;
			msg = ControlsResources.RctManager_TextConnectionNameConflictLong.Fmt(proposedConnectionName, uniqueConnectionName);

			if (MessageCtl.ShowX(msg, caption, MessageBoxButtons.YesNo) == DialogResult.No)
			{
				Instance.EventEnter(false, true);
				explorerConnection.DisplayName = GetDatasetKey(connectionUrl);
				Instance.EventExit();

				return;
			}
		}


		// At this point we're good to go.
		csa.DatasetKey = uniqueDatasetKey;

		if (uniqueConnectionName == null && proposedConnectionName == null)
			csa.Remove(SysConstants.C_KeyExConnectionName);
		else if (!string.IsNullOrEmpty(uniqueConnectionName))
			csa.ConnectionName = uniqueConnectionName;
		else
			csa.ConnectionName = proposedConnectionName;

		if (uniqueDatasetName == null && proposedDatasetName == null)
			csa.Remove(SysConstants.C_KeyExDatasetName);
		else if (!string.IsNullOrEmpty(uniqueDatasetName))
			csa.DatasetName = uniqueDatasetName;
		else
			csa.DatasetName = proposedDatasetName;


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
	/// Item1 (retSuccess): true if validation was successful else false if add or modify
	/// should be cancelled.
	/// Item2 (retAddInternally): true if connectionUrl (connection) has
	/// changed and requires adding a new connection internally.
	/// Item3 (retModifyInternally): true if connectionUrl changed and will now
	/// require internally modifying another connection.
	/// </returns>
	/// <param name="site">
	/// The IVsDataConnectionProperties Site (usually the Site of an
	/// IVsDataConnectionUIControl control.
	/// </param>
	/// <param name="connectionSource">
	/// The source requesting the validation.
	/// </param>
	/// <param name="serverExplorerInsertMode">
	/// Boolean indicating wehther or not a connection is being added or modified.
	/// </param>
	/// <param name="createServerExplorerConnection">
	/// If the ConnectionSource is Session, indicates wether or not the connection can be added as an SE Connection
	/// if it does not exist.
	/// </param>
	/// <param name="storedConnectionString">
	/// The original ConnectionString before any editing of the Site took place else
	/// null on a new connection.
	/// </param>
	// ---------------------------------------------------------------------------------
	public static (bool, bool, bool) ValidateSiteProperties(IVsDataConnectionProperties site, EnConnectionSource connectionSource,
		bool serverExplorerInsertMode, bool createServerExplorerConnection, string storedConnectionString)
	{
		bool retSuccess = true;
		bool retAddInternally = false;
		bool retModifyInternally = false;

		if (Instance == null)
			return (retSuccess, retAddInternally, retModifyInternally);

		string storedConnectionUrl = null;
		string storedDatasetKey = null;

		if (storedConnectionString != null)
		{
			storedConnectionUrl = Csb.CreateConnectionUrl(storedConnectionString);
			storedDatasetKey = GetDatasetKey(storedConnectionUrl);
		}


		string proposedConnectionName = site.ContainsKey(SysConstants.C_KeyExConnectionName)
			? (string)site[SysConstants.C_KeyExConnectionName] : null;
		if (Cmd.IsNullValueOrEmpty(proposedConnectionName))
			proposedConnectionName = null;

		string msg = null;
		string caption = null;

		if (!string.IsNullOrWhiteSpace(proposedConnectionName) && proposedConnectionName.StartsWith(NativeDb.Scheme))
		{
			caption = ControlsResources.RctManager_CaptionInvalidConnectionName;
			msg = ControlsResources.RctManager_TextInvalidConnectionName.Fmt(NativeDb.Scheme, proposedConnectionName);

			MessageCtl.ShowX(msg, caption, MessageBoxButtons.OK);

			retSuccess = false;
			return (retSuccess, retAddInternally, retModifyInternally);
		}

		string proposedDatasetName = site.ContainsKey(SysConstants.C_KeyExDatasetName)
			? (string)site[SysConstants.C_KeyExDatasetName] : null;
		if (Cmd.IsNullValueOrEmpty(proposedDatasetName))
			proposedDatasetName = null;

		string dataSource = (string)site[SysConstants.C_KeyDataSource];
		int port = site.ContainsKey(SysConstants.C_KeyPort) ? Convert.ToInt32(site[SysConstants.C_KeyPort]) : SysConstants.C_DefaultPort;
		string database = (string)site[SysConstants.C_KeyDatabase];
		string dataset;

		try
		{
			dataset = Cmd.GetFileNameWithoutExtension(database);
		}
		catch (Exception ex)
		{
			Diag.Ex(ex, $"Database path: {database}.");
			throw;
		}


		string connectionUrl = (site as IBsDataConnectionProperties).Csa.LiveDatasetMoniker;

		// Evs.Trace(typeof(RctManager), nameof(ValidateSiteProperties), "connectionSource: {0}, serverExplorerInsertMode: {1}, proposedConnectionName: {2}, proposedDatasetName: {3}, dataSource: {4}, dataset: {5}.",
		//	connectionSource, serverExplorerInsertMode, proposedConnectionName, proposedDatasetName, dataSource, dataset);

		if (!string.IsNullOrEmpty(dataSource))
			dataSource = GetRegisteredServer(dataSource).Item1;

		// Validate the proposed names.
		bool rctExists = Instance.GenerateUniqueDatasetKey(connectionSource, ref proposedConnectionName, ref proposedDatasetName,
			dataSource, dataset, connectionUrl, storedConnectionUrl, ref createServerExplorerConnection,
			out EnConnectionSource storedConnectionSource,
			out string changedTargetDatasetKey, out string uniqueDatasetKey, out string uniqueConnectionName,
			out string uniqueDatasetName);

		/*
		// Evs.Trace(typeof(RctManager), nameof(ValidateSiteProperties),
			"GenerateUniqueDatasetKey results: proposedConnectionName: {0}, proposedDatasetName: {1}, dataSource: {2}, dataset: {3}, createnew: {4}, storedConnectionSource: {5}, changedTargetDatasetKey: {6}, uniqueDatasetKey : {7}, uniqueConnectionName: {8}, uniqueDatasetName: {9}.",
			proposedConnectionName, proposedDatasetName, dataSource, dataset, createNew, storedConnectionSource,
			changedTargetDatasetKey ?? "Null", uniqueDatasetKey ?? "Null",
			uniqueConnectionName == null ? "Null" : (uniqueConnectionName == "" ? """" : uniqueConnectionName),
			uniqueDatasetName == null ? "Null" : (uniqueDatasetName == "" ? """" : uniqueDatasetName));
		*/





		#region ---------------- User Prompt Section -----------------



		if (!string.IsNullOrEmpty(uniqueConnectionName) && !string.IsNullOrEmpty(proposedConnectionName))
		{
			// Handle all cases where there's a connection name conflict.

				// The settings provided will create a new Session connection as well as a new SE connection with a connection name conflict.
			if (createServerExplorerConnection && !serverExplorerInsertMode && connectionSource == EnConnectionSource.Session)
			{
				caption = ControlsResources.RctManager_CaptionNewConnectionNameConflict;
				msg = ControlsResources.RctManager_TextNewSessionSEConnectionNameConflict.Fmt(proposedConnectionName, uniqueConnectionName);
			}
			// The settings provided will create a new SE connection with a connection name conflict.
			else if (createServerExplorerConnection && !serverExplorerInsertMode)
			{
				caption = ControlsResources.RctManager_CaptionNewConnectionNameConflict;
				msg = ControlsResources.RctManager_TextNewSEConnectionNameConflict.Fmt(proposedConnectionName, uniqueConnectionName);
			}
			// The settings provided will create a new Session connection with a connection name conflict.
			else if (!rctExists && !serverExplorerInsertMode)
			{
				caption = ControlsResources.RctManager_CaptionNewConnectionNameConflict;
				msg = ControlsResources.RctManager_TextNewSessionConnectionNameConflict.Fmt(proposedConnectionName, uniqueConnectionName);
			}
			// The settings provided will switch connections with a connection name conflict.
			else if (changedTargetDatasetKey != null)
			{
				caption = ControlsResources.RctManager_CaptionConnectionChangeNameConflict;
				msg = ControlsResources.RctManager_TextConnectionChangeNameConflict.Fmt(changedTargetDatasetKey, proposedConnectionName, uniqueConnectionName);
			}
			// The settings provided will cause a connection name conflict.
			else
			{
				caption = ControlsResources.RctManager_CaptionConnectionNameConflict;
				msg = ControlsResources.RctManager_TextConnectionNameConflict.Fmt(proposedConnectionName, uniqueConnectionName);
			}
		}
		else if (!string.IsNullOrEmpty(uniqueDatasetName) && !string.IsNullOrEmpty(proposedDatasetName))
		{
			// Handle all cases where there's a DatasetName conflict.

			// The settings provided will create a new Session connection as well as a new SE connection with a DatasetName conflict.
			if (createServerExplorerConnection && !serverExplorerInsertMode && connectionSource == EnConnectionSource.Session)
			{
				caption = ControlsResources.RctManager_CaptionNewConnectionDatabaseNameConflict;
				msg = ControlsResources.RctManager_TextNewSessionSEConnectionDatabaseNameConflict.Fmt(proposedDatasetName, uniqueDatasetName);
			}
			// The settings provided will create a new SE connection with a DatasetName conflict.
			else if (createServerExplorerConnection && !serverExplorerInsertMode)
			{
				caption = ControlsResources.RctManager_CaptionNewConnectionDatabaseNameConflict;
				msg = ControlsResources.RctManager_TextNewSEConnectionDatabaseNameConflict.Fmt(proposedDatasetName, uniqueDatasetName);
			}
			// The settings provided will create a new Session connection with a DatasetName conflict.
			else if (!rctExists && !serverExplorerInsertMode)
			{
				caption = ControlsResources.RctManager_CaptionNewConnectionDatabaseNameConflict;
				msg = ControlsResources.RctManager_TextNewSessionConnectionDatabaseNameConflict.Fmt(proposedDatasetName, uniqueDatasetName);
			}
			// The settings provided will switch connections with a DatasetName conflict.
			else if (changedTargetDatasetKey != null)
			{
				caption = ControlsResources.RctManager_CaptionConnectionChangeDatabaseNameConflict;
				msg = ControlsResources.RctManager_TextConnectionChangeDatabaseNameConflict.Fmt(changedTargetDatasetKey, proposedDatasetName, uniqueDatasetName);
			}
			// The settings provided will cause a DatasetName conflict.
			else
			{
				caption = ControlsResources.RctManager_CaptionDatabaseNameConflict;
				msg = ControlsResources.RctManager_TextDatabaseNameConflict.Fmt(proposedDatasetName, uniqueDatasetName);
			}
		}
		// Handle all cases where there is no conflict.
		// The settings provided will create a new SE connection within a Session connection dialog.
		else if (createServerExplorerConnection && !serverExplorerInsertMode && connectionSource == EnConnectionSource.Session)
		{
			caption = ControlsResources.RctManager_CaptionNewConnection;
			msg = ControlsResources.RctManager_TextNewSessionSEConnection;
		}
		// The settings provided will create a new SE connection.
		else if (createServerExplorerConnection && !serverExplorerInsertMode)
		{
			caption = ControlsResources.RctManager_CaptionNewConnection;
			msg = ControlsResources.RctManager_TextNewSEConnection;
		}
		// The settings provided will create a new Session connection.
		else if (!rctExists && !serverExplorerInsertMode)
		{
			caption = ControlsResources.RctManager_CaptionNewConnection;
			msg = ControlsResources.RctManager_TextNewSessionConnection;
		}
		// The settings provided will switch connections.
		else if (changedTargetDatasetKey != null)
		{
			// The target connection will change.
			caption = ControlsResources.RctManager_CaptionConnectionChanged;
			msg = ControlsResources.RctManager_TextConnectionChanged.Fmt(changedTargetDatasetKey);
		}
		// If it's an SE connection and it's not the SE modifying warn if the connection name is being modified.
		else if (connectionSource != EnConnectionSource.ServerExplorer
			&& storedConnectionSource == EnConnectionSource.ServerExplorer &&
			(uniqueConnectionName == "" || uniqueDatasetName == ""))
		{
			// The target connection name will change.
			caption = ControlsResources.RctManager_CaptionConnectionModified;
			msg = ControlsResources.RctManager_TextSEConnectionNameChange.Fmt(storedDatasetKey);
		}
		// If it's an SE connection and it's not the SE modifying warn that modified settings will be
		// applied to the connection.
		else if (connectionSource != EnConnectionSource.ServerExplorer
			&& storedConnectionSource == EnConnectionSource.ServerExplorer)
		{
			caption = ControlsResources.RctManager_CaptionConnectionModified;
			msg = ControlsResources.RctManager_TextSEConnectionModified.Fmt(storedDatasetKey);
		}



		if (msg != null && MessageCtl.ShowX(msg, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
			return (false, false, false);



		#endregion ---------------- User Prompt Section -----------------


		// At this point we're good to go.

		// Clean up the site properties.
		if (!site.ContainsKey(CoreConstants.C_KeyExDatasetKey)
			|| (string)site[CoreConstants.C_KeyExDatasetKey] != uniqueDatasetKey)
		{
			site[CoreConstants.C_KeyExDatasetKey] = uniqueDatasetKey;
		}

		if (uniqueConnectionName == null && proposedConnectionName == null)
			site.Remove(SysConstants.C_KeyExConnectionName);
		else if (!string.IsNullOrEmpty(uniqueConnectionName))
			site[SysConstants.C_KeyExConnectionName] = uniqueConnectionName;
		else
			site[SysConstants.C_KeyExConnectionName] = proposedConnectionName;

		if (uniqueDatasetName == null && proposedDatasetName == null)
			site.Remove(SysConstants.C_KeyExDatasetName);
		else if (!string.IsNullOrEmpty(uniqueDatasetName))
			site[SysConstants.C_KeyExDatasetName] = uniqueDatasetName;
		else
			site[SysConstants.C_KeyExDatasetName] = proposedDatasetName;


		// Establish the connection owner.
		// if the explorer connection exists or if it's the source (or EntityDataModel) it automatically is the owner.
		string connectionKey = site.FindConnectionKey();

		if (connectionKey == null && (connectionSource == EnConnectionSource.ServerExplorer
			|| connectionSource == EnConnectionSource.EntityDataModel
			|| connectionSource == EnConnectionSource.DataSource))
		{
			connectionKey = uniqueDatasetKey;
		}

		// Evs.Trace(typeof(RctManager), nameof(ValidateSiteProperties), "Retrieved ConnectionKey: {0}.", connectionKey ?? "Null");

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

		if (!serverExplorerInsertMode && createServerExplorerConnection)
			retAddInternally = true;

		retModifyInternally = changedTargetDatasetKey != null || (!retAddInternally
			&& connectionSource != EnConnectionSource.ServerExplorer && connectionSource != EnConnectionSource.EntityDataModel
			&& connectionSource != EnConnectionSource.DataSource);

		// Tag the site as being updated by the edmx wizard if it's not being done internally, which will
		// use IVsDataConnectionUIProperties.Parse().
		// We do this because the wizard will attempt to rename the connection and we'll pick it up in
		// the rct on an IVsDataExplorerConnection.NodeChanged event, and reverse the rename and drop the tag.
		if ((connectionSource == EnConnectionSource.EntityDataModel || connectionSource == EnConnectionSource.DataSource)
			&& !retAddInternally && !retModifyInternally)
		{
			site["edmu"] = true;
		}


		retSuccess = true;

		return (retSuccess, retAddInternally, retModifyInternally);

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Scans an App.Config for a configured connection.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static bool VerifyAppConfigConnectionExists(string connectionUrl)
	{
		if (connectionUrl == null)
		{
			ArgumentNullException ex = new(nameof(connectionUrl));
			Diag.Ex(ex);
			throw ex;
		}

		Diag.ThrowIfNotOnUIThread();

		Project project = ApcManager.ActiveProject;

		if (project == null)
			return false;

		Evs.Trace(typeof(RctManager), nameof(VerifyAppConfigConnectionExists), $"Project: {project.Name}, Kind: {project}.");

		// Evs.Debug(typeof(RctManager), nameof(VerifyAppConfigConnectionExists), $"Active project: {project.Name}.");

		ProjectItem config = project.GetAppConfig();

		if (config == null)
			return false;

		string xmlPath;

		try
		{
			if (config.FileCount == 0)
				return false;

			xmlPath = config.FileNames[0];
		}
		catch (Exception ex)
		{
			Diag.Ex(ex);
			return false;
		}



		XmlDocument xmlDoc = new XmlDocument();

		try
		{
			xmlDoc.Load(xmlPath);
		}
		catch (Exception ex)
		{
			Diag.Ex(ex);
			return false;
		}

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

			xmlNode = xmlRoot.SelectSingleNode("//confBlackbirdNs:connectionStrings", xmlNs);

			if (xmlNode == null)
				return false;

			xmlParent = xmlNode;


			string testConnectionUrl;

			XmlNodeList xmlNodes = xmlParent.SelectNodes($"confBlackbirdNs:add[@providerName='{NativeDb.Invariant}']", xmlNs);


			if (xmlNodes.Count > 0)
			{
				foreach (XmlNode connectionNode in xmlNodes)
				{
					testConnectionUrl = Csb.CreateConnectionUrl(connectionNode.Attributes["connectionString"].Value);

					if (connectionUrl == testConnectionUrl)
						return true;
				}
			}

			/*
			string connectionString;
			DbConnectionStringBuilder csb;


			xmlNodes = xmlParent.SelectNodes($"confBlackbirdNs:add[@providerName='System.Data.EntityClient']", xmlNs);

			foreach (XmlNode connectionNode in xmlNodes)
			{
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

				testConnectionUrl = Csb.CreateConnectionUrl(connectionString);

				if (connectionUrl == testConnectionUrl)
					return true;

			}
			*/
		}
		catch (Exception ex)
		{
			Diag.Ex(ex);
			throw;
		}

		return false;
		

	}



	#endregion Methods





	// =========================================================================================================
	#region Event handling - RctManager
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Increments the <see cref="_EventConnectionDialogCardinal"/> counter when
	/// execution enters a Connection dialog to identify the ConnectionSource.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static bool EventConnectionDialogEnter(bool test = false, bool force = false)
	{
		lock (_LockGlobal)
		{
			if (_EventConnectionDialogCardinal != 0 && !force)
				return false;

			if (!test)
				_EventConnectionDialogCardinal++;
		}

		return true;
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Decrements the <see cref="_EventConnectionDialogCardinal"/> counter that was
	/// previously
	/// incremented by <see cref="EventConnectionDialogEnter"/>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static void EventConnectionDialogExit()
	{
		// Evs.Trace(typeof(RctManager), nameof(EventConnectionDialogExit));

		lock (_LockGlobal)
		{
			if (_EventConnectionDialogCardinal <= 0)
			{
				ApplicationException ex = new($"Attempt to exit Validation event when not in a Validation event. _EventValidationCardinal: {_EventConnectionDialogCardinal}");
				Diag.Ex(ex);
				throw ex;
			}

			_EventConnectionDialogCardinal--;
		}
	}



	public static void NotifyInitializedServerExplorerModel(object sender, DataExplorerNodeEventArgs e)
	{
		RctEventSink.NotifyInitializedServerExplorerModel(sender, e);
	}


	#endregion Event handling

}
