// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.VSIntegration.SqlEditorPackage

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BlackbirdSql.BrokeredServices;
using BlackbirdSql.BrokeredServices.ComponentModel;
using BlackbirdSql.Common;
using BlackbirdSql.Common.Controls;
using BlackbirdSql.Common.Controls.Interfaces;
using BlackbirdSql.Common.Ctl;
using BlackbirdSql.Common.Ctl.Commands;
using BlackbirdSql.Common.Ctl.ComponentModel;
using BlackbirdSql.Common.Ctl.Events;
using BlackbirdSql.Common.Ctl.Interfaces;
using BlackbirdSql.Common.Model;
using BlackbirdSql.Common.Model.QueryExecution;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Controls;
using BlackbirdSql.Core.Ctl.ComponentModel;
using BlackbirdSql.Core.Ctl.Enums;
using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.Core.Model;
using BlackbirdSql.EditorExtension.Controls.Config;
using BlackbirdSql.EditorExtension.Ctl;
using BlackbirdSql.EditorExtension.Ctl.Config;
using BlackbirdSql.EditorExtension.Properties;
using Microsoft;
using Microsoft.ServiceHub.Framework;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.RpcContracts.FileSystem;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.ServiceBroker;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Threading;

using Native = BlackbirdSql.Common.Native;



namespace BlackbirdSql.EditorExtension;


// =========================================================================================================
//										EditorExtensionPackage Class 
//
/// <summary>
/// BlackbirdSql Editor Extension <see cref="AsyncPackage"/> class implementation
/// </summary>
// =========================================================================================================



// ---------------------------------------------------------------------------------------------------------
#region							EditorExtensionPackage Class Attributes
// ---------------------------------------------------------------------------------------------------------


[VsProvideOptionPage(typeof(SettingsProvider.GeneralSettingsPage), SettingsProvider.CategoryName,
	SettingsProvider.SubCategoryName, SettingsProvider.GeneralSettingsPageName, 300, 301, 321)]
[VsProvideOptionPage(typeof(SettingsProvider.TabAndStatusBarSettingsPage), SettingsProvider.CategoryName,
	SettingsProvider.SubCategoryName, SettingsProvider.TabAndStatusBarSettingsPageName, 300, 301, 322)]

[VsProvideOptionPage(typeof(SettingsProvider.ExecutionSettingsPage), SettingsProvider.CategoryName,
	SettingsProvider.SubCategoryName, SettingsProvider.ExecutionSettingsPageName,
	SettingsProvider.ExecutionGeneralSettingsPageName, 300, 301, 323, 324)]
[VsProvideOptionPage(typeof(SettingsProvider.ExecutionAdvancedSettingsPage), SettingsProvider.CategoryName,
	SettingsProvider.SubCategoryName, SettingsProvider.ExecutionSettingsPageName,
	SettingsProvider.ExecutionAdvancedSettingsPageName, 300, 301, 323, 325)]
[VsProvideOptionPage(typeof(SettingsProvider.ResultsSettingsPage), SettingsProvider.CategoryName,
	SettingsProvider.SubCategoryName, SettingsProvider.ResultsSettingsPageName,
	SettingsProvider.ResultsGeneralSettingsPageName, 300, 301, 326, 327)]
[VsProvideOptionPage(typeof(SettingsProvider.ResultsGridSettingsPage), SettingsProvider.CategoryName,
	SettingsProvider.SubCategoryName, SettingsProvider.ResultsSettingsPageName,
	SettingsProvider.ResultsGridSettingsPageName, 300, 301, 326, 328)]
[VsProvideOptionPage(typeof(SettingsProvider.ResultsTextSettingsPage), SettingsProvider.CategoryName,
	SettingsProvider.SubCategoryName, SettingsProvider.ResultsSettingsPageName,
	SettingsProvider.ResultsTextSettingsPageName, 300, 301, 326, 329)]

[VsProvideFileSystemProvider(SystemData.Protocol, PackageData.FileSystemBrokeredServiceName,
	PackageData.ServiceVersion2, IsDisplayInfoProvider = true, IsRemoteProvider = true,
	Audience = (ServiceAudience.AllClientsIncludingGuests | ServiceAudience.PublicSdk | ServiceAudience.Process))]

[VsProvideEditorFactory(typeof(EditorFactoryWithoutEncoding), 311, false, DefaultName = PackageData.ServiceName,
	CommonPhysicalViewAttributes = (int)__VSPHYSICALVIEWATTRIBUTES.PVA_SupportsPreview,
	TrustLevel = __VSEDITORTRUSTLEVEL.ETL_AlwaysTrusted)]
[ProvideEditorExtension(typeof(EditorFactoryWithoutEncoding), SystemData.Extension, 110,
	DefaultName = PackageData.ServiceName, NameResourceID = 311, RegisterFactory = true)]
[ProvideEditorLogicalView(typeof(EditorFactoryWithoutEncoding), VSConstants.LOGVIEWID.Debugging_string)]
[ProvideEditorLogicalView(typeof(EditorFactoryWithoutEncoding), VSConstants.LOGVIEWID.Code_string)]
[ProvideEditorLogicalView(typeof(EditorFactoryWithoutEncoding), VSConstants.LOGVIEWID.Designer_string)]
[ProvideEditorLogicalView(typeof(EditorFactoryWithoutEncoding), VSConstants.LOGVIEWID.TextView_string)]
[VsProvideFileExtensionMapping(typeof(EditorFactoryWithoutEncoding), PackageData.ServiceName, 311, 100)]

[VsProvideEditorFactory(typeof(EditorFactoryWithEncoding), 312, false,
	DefaultName = $"{PackageData.ServiceName} with Encoding",
	CommonPhysicalViewAttributes = (int)__VSPHYSICALVIEWATTRIBUTES.PVA_SupportsPreview,
	TrustLevel = __VSEDITORTRUSTLEVEL.ETL_AlwaysTrusted)]
[ProvideEditorExtension(typeof(EditorFactoryWithEncoding), SystemData.Extension, 100,
	DefaultName = $"{PackageData.ServiceName} with Encoding", NameResourceID = 312, RegisterFactory = true)]
[ProvideEditorLogicalView(typeof(EditorFactoryWithEncoding), VSConstants.LOGVIEWID.Debugging_string)]
[ProvideEditorLogicalView(typeof(EditorFactoryWithEncoding), VSConstants.LOGVIEWID.Code_string)]
[ProvideEditorLogicalView(typeof(EditorFactoryWithEncoding), VSConstants.LOGVIEWID.Designer_string)]
[ProvideEditorLogicalView(typeof(EditorFactoryWithEncoding), VSConstants.LOGVIEWID.TextView_string)]
[VsProvideFileExtensionMapping(typeof(EditorFactoryWithEncoding), $"{PackageData.ServiceName} with Encoding", 312, 96)]

[VsProvideEditorFactory(typeof(SqlResultsEditorFactory), 313, false,
	DefaultName = "BlackbirdSql Results", CommonPhysicalViewAttributes = 0)]
[ProvideEditorLogicalView(typeof(SqlResultsEditorFactory), VSConstants.LOGVIEWID.Debugging_string)]
[ProvideEditorLogicalView(typeof(SqlResultsEditorFactory), VSConstants.LOGVIEWID.Code_string)]
[ProvideEditorLogicalView(typeof(SqlResultsEditorFactory), VSConstants.LOGVIEWID.Designer_string)]
[ProvideEditorLogicalView(typeof(SqlResultsEditorFactory), VSConstants.LOGVIEWID.TextView_string)]


[ProvideMenuResource("Menus.ctmenu", 1)]


#endregion Class Attributes



// =========================================================================================================
#region							EditorExtensionPackage Class Declaration
// =========================================================================================================
public abstract class EditorExtensionPackage : AbstractCorePackage, IBEditorPackage,
	IVsTextMarkerTypeProvider, Microsoft.VisualStudio.OLE.Interop.IServiceProvider,
	IVsFontAndColorDefaultsProvider, IVsBroadcastMessageEvents
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - EditorExtensionPackage
	// ---------------------------------------------------------------------------------


	public EditorExtensionPackage() : base()
	{
		_EventsManager = EditorEventsManager.CreateInstance(_ApcInstance);
	}


	/// <summary>
	/// Gets the singleton AbstractPackageController instance
	/// </summary>
	public static EditorExtensionPackage Instance
	{
		get
		{
			if (_Instance == null)
				DemandLoadPackage(SystemData.AsyncPackageGuid, out _);
			return (EditorExtensionPackage)_Instance;
		}
	}



	/// <summary>
	/// EditorExtensionPackage package disposal
	/// </summary>
	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			// Tracer.Trace(GetType(), "Dispose()", "", null);

			((IDisposable)_FileSystemBrokeredService)?.Dispose();

			if (ThreadHelper.CheckAccess() && GetGlobalService(typeof(SVsShell)) is IVsShell vsShell)
			{
				___(vsShell.UnadviseBroadcastMessages(_VsBroadcastMessageEventsCookie));
			}

			_EventsManager?.Dispose();

			if (!ThreadHelper.CheckAccess())
				return;

			if (GetGlobalService(typeof(SProfferService)) is IProfferService profferService)
			{
				___(profferService.RevokeService(_MarkerServiceCookie));
				___(profferService.RevokeService(_FontAndColorServiceCookie));
			}
		}
	}


	#endregion Additional Constructors / Destructors




	// =========================================================================================================
	#region Constants & Fields - EditorExtensionPackage
	// =========================================================================================================


	// A private 'this' object lock
	private readonly object _LockLocal = new object();

#pragma warning disable ISB001 // Dispose of proxies
	private IFileSystemProvider _FileSystemBrokeredService = null;
#pragma warning restore ISB001 // Dispose of proxies

	private Dictionary<object, AuxiliaryDocData> _AuxiliaryDocDataTable = null;


	// private static readonly string _TName = typeof(EditorExtensionPackage).Name;
	public const string BrokeredServiceName = PackageData.FileSystemBrokeredServiceName;
	public const string BrokeredServiceVersion = PackageData.ServiceVersion2;

	private readonly EditorEventsManager _EventsManager;
	private uint _VsBroadcastMessageEventsCookie;
	private EditorFactoryWithoutEncoding _SqlEditorFactory;
	private EditorFactoryWithEncoding _SqlEditorFactoryWithEncoding;
	private SqlResultsEditorFactory _SqlResultsEditorFactory;
	private uint _MarkerServiceCookie;
	private uint _FontAndColorServiceCookie;


	#endregion Constants & Fields





	// =========================================================================================================
	#region Property accessors - EditorExtensionPackage
	// =========================================================================================================


	[Import]
#pragma warning disable CS8632 // The annotation for nullable reference types warning.
	private JoinableTaskContext? JoinableTaskContext { get; set; }
#pragma warning restore CS8632 // The annotation for nullable reference types warning.

	public IBSqlEditorWindowPane LastFocusedSqlEditor { get; set; }


	public override IFileSystemProvider FileSystemBrokeredService
	{
		get
		{
			// Fire and wait
			if (_FileSystemBrokeredService != null)
				return _FileSystemBrokeredService;

			ThreadHelper.JoinableTaskFactory.Run(async delegate
			{
				IBrokeredServiceContainer brokeredServiceContainer =
					await GetServiceAsync<SVsBrokeredServiceContainer, IBrokeredServiceContainer>();

				Diag.ThrowIfServiceUnavailable(brokeredServiceContainer, typeof(IBrokeredServiceContainer));

				IServiceBroker serviceBroker = brokeredServiceContainer.GetFullAccessServiceBroker();

#pragma warning disable ISB001 // Dispose of proxies
				_FileSystemBrokeredService = await serviceBroker.GetProxyAsync<IFileSystemProvider>(PackageData.FileSystemRpcDescriptor2, default);
#pragma warning restore ISB001 // Dispose of proxies
			});

			return _FileSystemBrokeredService;
		}
	}

	private FbsqlPlusFileSystemProvider FileSystemProvider => (FbsqlPlusFileSystemProvider)FileSystemBrokeredService;

	public Dictionary<object, AuxiliaryDocData> AuxiliaryDocDataTable => _AuxiliaryDocDataTable ??= [];


	public bool EnableSpatialResultsTab { get; set; }


	public override IBEventsManager EventsManager => _EventsManager;

	public object LockLocal => _LockLocal;



	#endregion Property accessors




	// =========================================================================================================
	#region Methods Implementations - EditorExtensionPackage
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Asynchronous initialization of the package. The class must register services it
	/// requires using the ServicesCreatorCallback method.
	/// </summary>
	protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
	{
		ServiceProgressData progressData = new("Loading BlackbirdSql", "Loading FileSystemProvider", 1, 15);
		progress.Report(progressData);


		await RegisterFileSystemProviderAsync();


		progressData = new("Loading BlackbirdSql", "Done Loading FileSystemProvider", 2, 15);
		progress.Report(progressData);

		await base.InitializeAsync(cancellationToken, progress);
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Final asynchronous initialization tasks for the package that must occur after
	/// all descendents and ancestors have completed their InitializeAsync() tasks.
	/// It is the final descendent package class's responsibility to initiate the call
	/// to FinalizeAsync.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override async Task FinalizeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
	{
		Diag.ThrowIfNotOnUIThread();

		if (cancellationToken.IsCancellationRequested || ApcManager.IdeShutdownState)
			return;

		await base.FinalizeAsync(cancellationToken, progress);

		ServiceProgressData progressData = new("Loading BlackbirdSql", "Finalizing: Proffering Editor Services", 9, 12);
		progress.Report(progressData);


		if (await GetServiceAsync(typeof(IProfferService)) is not IProfferService profferSvc)
			throw Diag.ExceptionService(typeof(OleMenuCommandService));

		Guid rguidMarkerService = LibraryData.CLSID_EditorMarkerService;
		___(profferSvc.ProfferService(ref rguidMarkerService, this, out _MarkerServiceCookie));

		Guid rguidService = LibraryData.CLSID_FontAndColorService;
		___(profferSvc.ProfferService(ref rguidService, this, out _FontAndColorServiceCookie));


		ServiceContainer.AddService(typeof(IBDesignerExplorerServices), ServicesCreatorCallbackAsync, promote: true);
		// ServiceContainer.AddService(typeof(IBDesignerOnlineServices), ServicesCreatorCallbackAsync, promote: true);
		// Services.AddService(typeof(ISqlEditorStrategyProvider), ServicesCreatorCallbackAsync, promote: true);

		progressData = new("Loading BlackbirdSql", "Finalizing: Done Proffering Editor Services", 10, 15);
		progress.Report(progressData);

		progressData = new("Loading BlackbirdSql", "Finalizing: Registering Editor Factories", 10, 15);
		progress.Report(progressData);

		_SqlEditorFactory = new EditorFactoryWithoutEncoding();
		_SqlEditorFactoryWithEncoding = new EditorFactoryWithEncoding();
		_SqlResultsEditorFactory = new SqlResultsEditorFactory();

		RegisterEditorFactory(_SqlEditorFactory);
		RegisterEditorFactory(_SqlEditorFactoryWithEncoding);
		RegisterEditorFactory(_SqlResultsEditorFactory);


		___((GetGlobalService(typeof(SVsShell)) as IVsShell).AdviseBroadcastMessages(this, out _VsBroadcastMessageEventsCookie));

		progressData = new("Loading BlackbirdSql", "Finalizing: Done Registering Editor Factories", 11, 15);
		progress.Report(progressData);

		progressData = new("Loading BlackbirdSql", "Finalizing: Initializing Tabbed Toolbar Manager", 11, 15);
		progress.Report(progressData);

		InitializeTabbedEditorToolbarHandlerManager();

		progressData = new("Loading BlackbirdSql", "Finalizing: Done Initializing Tabbed Toolbar Manager", 12, 15);
		progress.Report(progressData);

		await DefineCommandsAsync();

		progressData = new("Loading BlackbirdSql", "Finalizing: Done Initializing Tabbed Toolbar Manager", 13, 15);
		progress.Report(progressData);

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Creates a service instance of the specified type if this class has access to the
	/// final class type of the service being added.
	/// The class requiring and adding the service may not necessarily be the class that
	/// creates an instance of the service.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override async Task<object> CreateServiceInstanceAsync(Type serviceType, CancellationToken token)
	{
		if (serviceType == null)
		{
			ArgumentNullException ex = new("serviceType");
			Diag.Dug(ex);
			throw ex;
		}
		else if (serviceType == typeof(IBDesignerExplorerServices))
		{
			object service = new DesignerExplorerServices()
				?? throw Diag.ExceptionService(serviceType);

			return service;
		}
		/*
		else if (serviceType == typeof(IBDesignerOnlineServices))
		{
			object service = new DesignerOnlineServices()
				?? throw Diag.ExceptionService(serviceType);

			return service;
		}
		*/
		else if (serviceType.IsInstanceOfType(this))
		{
			return this;
		}

		return await base.CreateServiceInstanceAsync(serviceType, token);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Initializes and configures a service of the specified type that is used by this
	/// Package.
	/// Configuration is performed by the class requiring the service.
	/// The actual instance creation of the service is the responsibility of the class
	/// Package that has access to the final descendent class of the Service.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override async Task<object> ServicesCreatorCallbackAsync(IAsyncServiceContainer container, CancellationToken token, Type serviceType)
	{

		if (serviceType == typeof(IBDesignerExplorerServices)
			/* || serviceType == typeof(IBDesignerOnlineServices) */)
		{
			return await CreateServiceInstanceAsync(serviceType, token);
		}


		return await base.ServicesCreatorCallbackAsync(container, token, serviceType);
	}


	#endregion Methods Implementations





	// =========================================================================================================
	#region Methods - EditorExtensionPackage
	// =========================================================================================================


	public bool ContainsEditorStatus(object docData)
	{
		lock (_LockLocal)
		{
			if (docData == null || _AuxiliaryDocDataTable == null)
			{
				return false;
			}

			if (_AuxiliaryDocDataTable == null)
				return false;

			if (_AuxiliaryDocDataTable.ContainsKey(docData))
				return true;
		}

		return false;
	}



	private void CycleToNextEditorTab(object sender, EventArgs e)
	{
		LastFocusedSqlEditor?.ActivateNextTab();
	}



	private void CycleToPreviousEditorTab(object sender, EventArgs e)
	{
		LastFocusedSqlEditor?.ActivatePreviousTab();
	}



	private async Task DefineCommandsAsync()
	{
		if (await GetServiceAsync(typeof(IMenuCommandService)) is not OleMenuCommandService oleMenuCommandService)
			throw Diag.ExceptionService(typeof(OleMenuCommandService));

		// Tracer.Trace(GetType(), "DefineCommandsAsync()", "OleCommandService class: {0}.", oleMenuCommandService.GetType().FullName);

		Guid clsid = LibraryData.CLSID_CommandSet;

		CommandID id = new CommandID(clsid, (int)EnCommandSet.CmdIdNewSqlQuery);
		OleMenuCommand oleMenuCommand = new OleMenuCommand(OnNewSqlQuery, id);
		oleMenuCommand.BeforeQueryStatus += EnableCommand;
		oleMenuCommandService.AddCommand(oleMenuCommand);
		CommandID id2 = new CommandID(clsid, (int)EnCommandSet.CmdIdCycleToNextTab);
		OleMenuCommand oleMenuCommand2 = new OleMenuCommand(CycleToNextEditorTab, id2);
		oleMenuCommand2.BeforeQueryStatus += EnableCommand;
		oleMenuCommandService.AddCommand(oleMenuCommand2);
		CommandID id3 = new CommandID(clsid, (int)EnCommandSet.CmdIdCycleToPrevious);
		OleMenuCommand oleMenuCommand3 = new OleMenuCommand(CycleToPreviousEditorTab, id3);
		oleMenuCommand3.BeforeQueryStatus += EnableCommand;
		oleMenuCommandService.AddCommand(oleMenuCommand3);
	}



	private void EnableCommand(object sender, EventArgs e)
	{
		// Tracer.Trace(GetType(), "EnableCommand()");

		if (sender is OleMenuCommand oleMenuCommand)
		{
			oleMenuCommand.Enabled = true;
			oleMenuCommand.Visible = true;
		}
	}





	public void EnsureAuxilliaryDocData(IVsHierarchy hierarchy, string documentMoniker, object docData)
	{
		lock (_LockLocal)
		{
			if (_AuxiliaryDocDataTable == null || !_AuxiliaryDocDataTable.TryGetValue(docData, out AuxiliaryDocData auxDocData))
			{
				// Accessing the stack and pop.
				string explorerMoniker = DesignerExplorerServices.ExplorerMonikerStack;

				// Tracer.Trace(GetType(), "EnsureAuxilliaryDocData()", "explorerMoniker: {0}, documentMoniker: {1}.", explorerMoniker, documentMoniker);

				auxDocData = new AuxiliaryDocData(documentMoniker, explorerMoniker, docData);
				// hierarchy.GetSite(out Microsoft.VisualStudio.OLE.Interop.IServiceProvider ppSP);

				if (explorerMoniker != null && DesignerExplorerServices.MonikerCsaTable.TryGetValue(explorerMoniker, out object csaObject))
				{
					auxDocData.SetUserDataCsb((System.Data.Common.DbConnectionStringBuilder)csaObject);
					DesignerExplorerServices.MonikerCsaTable[explorerMoniker] = null;
				}
				// Not point looking because This will always be null for us
				IBSqlEditorStrategyProvider sqlEditorStrategyProvider = null;
				// ISqlEditorStrategyProvider sqlEditorStrategyProvider = new ServiceProvider(ppSP).GetService(typeof(ISqlEditorStrategyProvider)) as ISqlEditorStrategyProvider;

				// We always use DefaultSqlEditorStrategy and use the csb passed via userdata that was
				// contructed from the SE node
				IBSqlEditorStrategy sqlEditorStrategy = (sqlEditorStrategyProvider == null
					? new DefaultSqlEditorStrategy(auxDocData.GetUserDataCsb())
					: sqlEditorStrategyProvider.CreateEditorStrategy(documentMoniker, auxDocData));

				auxDocData.Strategy = sqlEditorStrategy;
				if (auxDocData.Strategy.IsDw)
					auxDocData.IntellisenseEnabled = false;

				AuxiliaryDocDataTable.Add(docData, auxDocData);
			}
		}
	}



	public AuxiliaryDocData GetAuxiliaryDocData(object docData)
	{
		lock (_LockLocal)
		{
			if (docData == null || _AuxiliaryDocDataTable == null)
				return null;

			_AuxiliaryDocDataTable.TryGetValue(docData, out AuxiliaryDocData value);
			return value;
		}
	}



	int IVsFontAndColorDefaultsProvider.GetObject(ref Guid rguidCategory, out object ppObj)
	{
		ppObj = null;
		if (rguidCategory == Core.VS.CLSID_FontAndColorsSqlResultsTextCategory)
		{
			ppObj = FontAndColorProviderTextResults.Instance;
		}
		else if (rguidCategory == Core.VS.CLSID_FontAndColorsSqlResultsGridCategory)
		{
			ppObj = FontAndColorProviderGridResults.Instance;
		}
		else if (rguidCategory == Core.VS.CLSID_FontAndColorsSqlResultsExecutionPlanCategory)
		{
			ppObj = FontAndColorProviderExecutionPlan.Instance;
		}

		return VSConstants.S_OK;
	}



	private void GetServicePointer(Guid interfaceGuid, object serviceObject, out IntPtr service)
	{
		IntPtr intPtrUnknown = IntPtr.Zero;
		try
		{
			intPtrUnknown = Marshal.GetIUnknownForObject(serviceObject);
			Marshal.QueryInterface(intPtrUnknown, ref interfaceGuid, out service);
		}
		finally
		{
			if (intPtrUnknown != IntPtr.Zero)
			{
				Marshal.Release(intPtrUnknown);
			}
		}
	}



	int IVsTextMarkerTypeProvider.GetTextMarkerType(ref Guid markerGuid, out IVsPackageDefinedTextMarkerType vsTextMarker)
	{
		if (markerGuid == Core.VS.CLSID_TSqlEditorMessageErrorMarker)
		{
			vsTextMarker = new VsTextMarker((uint)MARKERVISUAL.MV_COLOR_ALWAYS, COLORINDEX.CI_RED, COLORINDEX.CI_USERTEXT_BK, Resources.ErrorMarkerDisplayName, Resources.ErrorMarkerDescription, "Error Message");
			return VSConstants.S_OK;
		}

		vsTextMarker = null;
		return VSConstants.E_INVALIDARG;
	}



	public bool HasAnyAuxillaryDocData()
	{
		lock (_LockLocal)
			return _AuxiliaryDocDataTable != null && _AuxiliaryDocDataTable.Count > 0;
	}



	private static void InitializeTabbedEditorToolbarHandlerManager()
	{
		TabbedEditorToolbarHandlerManager toolbarMgr = AbstractTabbedEditorWindowPane.ToolbarManager;
		if (toolbarMgr != null)
		{
			Guid clsid = LibraryData.CLSID_CommandSet;

			toolbarMgr.AddMapping(typeof(TabbedEditorWindowPane),
				new SqlEditorToolbarCommandHandler<SqlEditorDatabaseCommand>(clsid, (uint)EnCommandSet.CmbIdSqlDatabases));
			toolbarMgr.AddMapping(typeof(TabbedEditorWindowPane),
				new SqlEditorToolbarCommandHandler<SqlEditorDatabaseListCommand>(clsid, (uint)EnCommandSet.CmbIdSqlDatabasesGetList));
			toolbarMgr.AddMapping(typeof(TabbedEditorWindowPane),
				new SqlEditorToolbarCommandHandler<SqlEditorExecuteTtsQueryCommand>(clsid, (uint)EnCommandSet.CmdIdExecuteTtsQuery));
			toolbarMgr.AddMapping(typeof(TabbedEditorWindowPane),
				new SqlEditorToolbarCommandHandler<SqlEditorExecuteQueryCommand>(clsid, (uint)EnCommandSet.CmdIdExecuteQuery));
			toolbarMgr.AddMapping(typeof(TabbedEditorWindowPane),
				new SqlEditorToolbarCommandHandler<SqlEditorParseQueryCommand>(clsid, (uint)EnCommandSet.CmdIdParseQuery));
			toolbarMgr.AddMapping(typeof(TabbedEditorWindowPane),
				new SqlEditorToolbarCommandHandler<SqlEditorCancelQueryCommand>(clsid, (uint)EnCommandSet.CmdIdCancelQuery));
			toolbarMgr.AddMapping(typeof(TabbedEditorWindowPane),
				new SqlEditorToolbarCommandHandler<SqlEditorConnectCommand>(clsid, (uint)EnCommandSet.CmdIdConnect));
			toolbarMgr.AddMapping(typeof(TabbedEditorWindowPane),
				new SqlEditorToolbarCommandHandler<SqlEditorDisconnectCommand>(clsid, (uint)EnCommandSet.CmdIdDisconnect));
			toolbarMgr.AddMapping(typeof(TabbedEditorWindowPane),
				new SqlEditorToolbarCommandHandler<SqlEditorDisconnectAllQueriesCommand>(clsid, (uint)EnCommandSet.CmdIdDisconnectAllQueries));
			toolbarMgr.AddMapping(typeof(TabbedEditorWindowPane),
				new SqlEditorToolbarCommandHandler<SqlEditorChangeConnectionCommand>(clsid, (uint)EnCommandSet.CmdIdChangeConnection));
			toolbarMgr.AddMapping(typeof(TabbedEditorWindowPane),
				new SqlEditorToolbarCommandHandler<SqlEditorShowEstimatedPlanCommand>(clsid, (uint)EnCommandSet.CmdIdShowEstimatedPlan));
			toolbarMgr.AddMapping(typeof(TabbedEditorWindowPane),
				new SqlEditorToolbarCommandHandler<SqlEditorCloneQueryWindowCommand>(clsid, (uint)EnCommandSet.CmdIdCloneQuery));
			toolbarMgr.AddMapping(typeof(TabbedEditorWindowPane),
				new SqlEditorToolbarCommandHandler<SqlEditorToggleSqlCmdModeCommand>(clsid, (uint)EnCommandSet.CmdIdToggleSQLCMDMode));
			toolbarMgr.AddMapping(typeof(TabbedEditorWindowPane),
				new SqlEditorToolbarCommandHandler<SqlEditorToggleExecutionPlanCommand>(clsid, (uint)EnCommandSet.CmdIdToggleExecutionPlan));
			toolbarMgr.AddMapping(typeof(TabbedEditorWindowPane),
				new SqlEditorToolbarCommandHandler<SqlEditorToggleClientStatisticsCommand>(clsid, (uint)EnCommandSet.CmdIdToggleClientStatistics));
			toolbarMgr.AddMapping(typeof(TabbedEditorWindowPane),
				new SqlEditorToolbarCommandHandler<SqlEditorNewQueryCommand>(clsid, (uint)EnCommandSet.CmdIdNewSqlQuery));
			toolbarMgr.AddMapping(typeof(TabbedEditorWindowPane),
				new SqlEditorToolbarCommandHandler<SqlEditorTransactionCommitCommand>(clsid, (uint)EnCommandSet.CmdIdTransactionCommit));
			toolbarMgr.AddMapping(typeof(TabbedEditorWindowPane),
				new SqlEditorToolbarCommandHandler<SqlEditorTransactionRollbackCommand>(clsid, (uint)EnCommandSet.CmdIdTransactionRollback));
		}
	}



	int Microsoft.VisualStudio.OLE.Interop.IServiceProvider.QueryService(ref Guid serviceGuid, ref Guid interfaceGuid, out IntPtr service)
	{
		if (interfaceGuid == typeof(IVsTextMarkerTypeProvider).GUID && serviceGuid == LibraryData.CLSID_EditorMarkerService)
		{
			GetServicePointer(interfaceGuid, this, out service);
			return VSConstants.S_OK;
		}

		if (interfaceGuid == typeof(IVsFontAndColorDefaultsProvider).GUID && serviceGuid == LibraryData.CLSID_FontAndColorService)
		{
			GetServicePointer(interfaceGuid, this, out service);
			return VSConstants.S_OK;
		}

		if (interfaceGuid == typeof(IVsTextMarkerTypeProvider).GUID && serviceGuid == LibraryData.CLSID_EditorMarkerService)
		{
			GetServicePointer(interfaceGuid, this, out service);
			return VSConstants.S_OK;
		}

		service = (IntPtr)0;
		return VSConstants.E_NOINTERFACE;
	}



	private async Task<bool> RegisterFileSystemProviderAsync()
	{

		IComponentModel componentModelSvc = await GetServiceAsync<SComponentModel, IComponentModel>();
		Assumes.Present(componentModelSvc);
		componentModelSvc.DefaultCompositionService.SatisfyImportsOnce(this);


		IBrokeredServiceContainer brokeredServiceContainer = await GetServiceAsync<SVsBrokeredServiceContainer, IBrokeredServiceContainer>();
		Assumes.Present(brokeredServiceContainer);

		Assumes.Present(JoinableTaskContext);
		JoinableTaskContext joinableTaskContext2 = JoinableTaskContext;


		Func<Task<IVsAsyncFileChangeEx>> fileChangeSvc = GetServiceAsync<SVsFileChangeEx, IVsAsyncFileChangeEx>;
		Assumes.Present(fileChangeSvc);
		Func<Task<IVsAsyncFileChangeEx>> fileChangeSvc2 = fileChangeSvc;


		IDisposable disposable2 = brokeredServiceContainer.Proffer(PackageData.FileSystemRpcDescriptor2,
			(ServiceMoniker moniker, ServiceActivationOptions options, IServiceBroker broker, CancellationToken token) =>
				new ValueTask<object>(new FbsqlPlusFileSystemProvider(fileChangeSvc2, joinableTaskContext2, true)));


		return true;

	}



	public void RemoveEditorStatus(object docData)
	{
		lock (_LockLocal)
		{
			if (_AuxiliaryDocDataTable == null)
				return;

			if (_AuxiliaryDocDataTable.TryGetValue(docData, out AuxiliaryDocData auxDocData))
			{
				Guid clsidUserData = typeof(IVsUserData).GUID;
				IVsUserData vsUserData = docData as IVsUserData;

				vsUserData.GetData(ref clsidUserData, out object objData);

				string moniker = objData as string;

				if (_FileSystemBrokeredService != null
					&& FileSystemProvider.Unwatch(moniker)
					&& FileSystemProvider.WatchedFileSystemEntries.Count == 0)
				{
					FileSystemProvider.Dispose();
					_FileSystemBrokeredService = null;
				}


				if (auxDocData.ExplorerMoniker != null)
					DesignerExplorerServices.MonikerCsaTable.Remove(auxDocData.ExplorerMoniker);

				_AuxiliaryDocDataTable.Remove(docData);
				auxDocData?.Dispose();
			}
		}
	}



	public void SetSqlEditorStrategyForDocument(object docData, IBSqlEditorStrategy strategy)
	{
		lock (_LockLocal)
		{
			if (_AuxiliaryDocDataTable == null || !_AuxiliaryDocDataTable.TryGetValue(docData, out var value))
			{
				ArgumentException ex = new("No auxillary information for DocData");
				Diag.Dug(ex);
				throw ex;
			}

			value.Strategy = strategy;
		}
	}



	public bool? ShowConnectionDialogFrame(IntPtr parent, EventsChannel channel,
		ConnectionPropertyAgent ci, VerifyConnectionDelegate verifierDelegate, ConnectionDialogConfiguration config,
		ref ConnectionPropertyAgent connectionInfo)
	{
		// Tracer.Trace(GetType(), "ShowConnectionDialogFrame()");

		/*
		ConnectionDialogFrame connectionDialogFrame = new ConnectionDialogFrame(channel, ci, verifierDelegate, config);

		connectionDialogFrame.ShowModal(parent);
		connectionInfo = connectionDialogFrame.ConnectionInfo;
		connectionDialogFrame.ViewModel.CloseSections();

		return connectionDialogFrame.DialogResult;
		*/

		return false;
	}



	public DialogResult ShowExecutionSettingsDialogFrame(AuxiliaryDocData auxDocData,
		FormStartPosition startPosition)
	{
		// Tracer.Trace(GetType(), "ShowExecutionSettingsDialogFrame()");

		DialogResult result = DialogResult.Abort;

		QueryManager qryMgr = auxDocData.QryMgr;
		if (qryMgr == null)
			return DialogResult.Abort;

		using (CurrentWndOptionsDlg dlg = new(qryMgr.LiveSettings))
		{
			dlg.StartPosition = startPosition;

			try
			{
				result = FormHost.ShowDialog(dlg);
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw;
			}

			if (result == DialogResult.OK)
				auxDocData.UpdateLiveSettingsState(qryMgr.LiveSettings);
		}

		return result;
	}



	public bool TryGetTabbedEditorService(uint docCookie, bool activateIfOpen, out IBTabbedEditorService tabbedEditorService)
	{
		// Tracer.Trace(GetType(), "TryGetTabbedEditorService()", "ENTER!!!");

		IVsUIShellOpenDocument shellOpenDocumentSvc = ApcInstance.EnsureService<SVsUIShellOpenDocument, IVsUIShellOpenDocument>();

		
		Guid rguidEditorType = new(SystemData.MandatedSqlEditorFactoryGuid);
		uint[] pitemidOpen = new uint[1];
		IVsUIHierarchy pHierCaller = null;

		RunningDocumentInfo documentInfo;

		lock (RdtManager.LockGlobal)
			documentInfo = RdtManager.GetDocumentInfo(docCookie);

		string mkDocument = documentInfo.Moniker;

		if (!documentInfo.IsDocumentInitialized)
		{
			Diag.Dug(new COMException($"Document for moniker {mkDocument} using document cookie {docCookie} is not initialized."));
			tabbedEditorService = null;
			return false;
		}

		__VSIDOFLAGS openDocumentFlags = activateIfOpen ? __VSIDOFLAGS.IDO_ActivateIfOpen : 0;

		int hresult = shellOpenDocumentSvc.IsSpecificDocumentViewOpen(pHierCaller, uint.MaxValue,
			mkDocument, ref rguidEditorType, null, (uint)openDocumentFlags,
			out _, out pitemidOpen[0], out IVsWindowFrame ppWindowFrame, out int pfOpen);


		if (!ErrorHandler.Succeeded(hresult) || !pfOpen.AsBool() || ppWindowFrame == null)
		{
			Diag.Dug(new COMException($"Failed to find window frame for moniker {mkDocument} using document cookie {docCookie}."));
			tabbedEditorService = null;
			return false;
		}


		if (!Native.Succeeded(ppWindowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out var pvar)))
		{
			Diag.Dug(new COMException($"Failed to get window frame DocView property for moniker {mkDocument} using document cookie {docCookie}."));
			tabbedEditorService = null;
			return false;
		}

		if (pvar is not IBTabbedEditorService tabbedEditorPane)
		{
			Diag.Dug(new COMException($"Window frame DocView property is not of type IBTabbedEditorService for moniker {mkDocument} using document cookie {docCookie}."));
			tabbedEditorService = null;
			return false;
		}


		tabbedEditorService = tabbedEditorPane;

		// Tracer.Trace(GetType(), "TryGetTabbedEditorService()", "DONE!!!");

		return true;
	}


	#endregion Methods and Implementations




	// =========================================================================================================
	#region Event handlers - BlackbirdSqlDdexExtension
	// =========================================================================================================


	int IVsBroadcastMessageEvents.OnBroadcastMessage(uint message, IntPtr wParam, IntPtr lParam)
	{
		if (message == 536)
		{
			OnPowerBroadcast(wParam, lParam);
		}

		return VSConstants.S_OK;
	}



	private void OnNewSqlQuery(object sender, EventArgs e)
	{
		// Tracer.Trace(GetType(), "OnNewSqlQuery()");

		using (Microsoft.VisualStudio.Utilities.DpiAwareness.EnterDpiScope(Microsoft.VisualStudio.Utilities.DpiAwarenessContext.SystemAware))
		{
			DesignerExplorerServices.OpenNewMiscellaneousSqlFile();
			IBSqlEditorWindowPane lastFocusedSqlEditor = Instance.LastFocusedSqlEditor;
			if (lastFocusedSqlEditor != null)
			{
				new SqlEditorNewQueryCommand(lastFocusedSqlEditor).Exec((uint)OLECMDEXECOPT.OLECMDEXECOPT_DODEFAULT, IntPtr.Zero, IntPtr.Zero);
				GetAuxiliaryDocData(lastFocusedSqlEditor.DocData).IsVirtualWindow = true;
			}
		}
	}



	private void OnPowerBroadcast(IntPtr wParam, IntPtr lParam)
	{
		if ((int)wParam != 4 || _AuxiliaryDocDataTable == null)
		{
			return;
		}

		foreach (AuxiliaryDocData value in _AuxiliaryDocDataTable.Values)
		{
			QueryManager qryMgr = value.QryMgr;
			if (qryMgr != null && qryMgr.IsConnected)
			{
				qryMgr.ConnectionStrategy.Transaction?.Dispose();
				qryMgr.ConnectionStrategy.Transaction = null;
				qryMgr.ConnectionStrategy.Connection?.Close();
			}
		}
	}


	#endregion Event handlers


}

#endregion Class Declaration
