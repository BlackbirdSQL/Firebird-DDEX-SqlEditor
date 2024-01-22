// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.VSIntegration.SqlEditorPackage
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using BlackbirdSql.Common;
using BlackbirdSql.Common.Controls;
using BlackbirdSql.Common.Ctl;
using BlackbirdSql.Common.Ctl.Commands;
using BlackbirdSql.Common.Ctl.ComponentModel;
using BlackbirdSql.Common.Ctl.Events;
using BlackbirdSql.Common.Ctl.Interfaces;
using BlackbirdSql.Common.Model;
using BlackbirdSql.Common.Model.QueryExecution;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl;
using BlackbirdSql.Core.Ctl.ComponentModel;
using BlackbirdSql.Core.Ctl.Enums;
using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.Core.Model;
using BlackbirdSql.EditorExtension.Controls.Config;
using BlackbirdSql.EditorExtension.Ctl;
using BlackbirdSql.EditorExtension.Ctl.Config;
using BlackbirdSql.EditorExtension.Properties;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

using Cmd = BlackbirdSql.Common.Cmd;
using MonikerAgent = BlackbirdSql.Core.Model.MonikerAgent;
using Native = BlackbirdSql.Common.Native;
using Tracer = BlackbirdSql.Core.Ctl.Diagnostics.Tracer;


namespace BlackbirdSql.EditorExtension;

// =========================================================================================================
//										EditorExtensionAsyncPackage Class 
//
/// <summary>
/// BlackbirdSql Editor Extension <see cref="AsyncPackage"/> class implementation
/// </summary>
// =========================================================================================================



// ---------------------------------------------------------------------------------------------------------
#region							EditorExtensionAsyncPackage Class Attributes
// ---------------------------------------------------------------------------------------------------------


// [ProvideLoadKey("Standard", "##VERSION.MAJOR.MINOR.BUILD.REVISION##", "Microsoft SQL Server Data Tools - TSql Editor", "Microsoft Corporation", 101)]
// [Guid(SystemData.PackageGuid)]


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


[VsProvideEditorFactory(typeof(EditorFactoryWithoutEncoding), 311, false, DefaultName = "BlackbirdSql Editor",
	CommonPhysicalViewAttributes = (int)__VSPHYSICALVIEWATTRIBUTES.PVA_SupportsPreview,
	TrustLevel = __VSEDITORTRUSTLEVEL.ETL_AlwaysTrusted)]
[ProvideEditorExtension(typeof(EditorFactoryWithoutEncoding), MonikerAgent.C_SqlExtension, 110,
	DefaultName = "BlackbirdSql Editor", NameResourceID = 311, RegisterFactory = true)]
[ProvideEditorLogicalView(typeof(EditorFactoryWithoutEncoding), VSConstants.LOGVIEWID.Debugging_string)]
[ProvideEditorLogicalView(typeof(EditorFactoryWithoutEncoding), VSConstants.LOGVIEWID.Code_string)]
[ProvideEditorLogicalView(typeof(EditorFactoryWithoutEncoding), VSConstants.LOGVIEWID.Designer_string)]
[ProvideEditorLogicalView(typeof(EditorFactoryWithoutEncoding), VSConstants.LOGVIEWID.TextView_string)]
[VsProvideFileExtensionMapping(typeof(EditorFactoryWithoutEncoding), "BlackbirdSql Editor", 311, 100)]

[VsProvideEditorFactory(typeof(EditorFactoryWithEncoding), 312, false,
	DefaultName = "BlackbirdSql Editor with Encoding",
	CommonPhysicalViewAttributes = (int)__VSPHYSICALVIEWATTRIBUTES.PVA_SupportsPreview,
	TrustLevel = __VSEDITORTRUSTLEVEL.ETL_AlwaysTrusted)]
[ProvideEditorExtension(typeof(EditorFactoryWithEncoding), MonikerAgent.C_SqlExtension, 100,
	DefaultName = "BlackbirdSql Editor with Encoding", NameResourceID = 312, RegisterFactory = true)]
[ProvideEditorLogicalView(typeof(EditorFactoryWithEncoding), VSConstants.LOGVIEWID.Debugging_string)]
[ProvideEditorLogicalView(typeof(EditorFactoryWithEncoding), VSConstants.LOGVIEWID.Code_string)]
[ProvideEditorLogicalView(typeof(EditorFactoryWithEncoding), VSConstants.LOGVIEWID.Designer_string)]
[ProvideEditorLogicalView(typeof(EditorFactoryWithEncoding), VSConstants.LOGVIEWID.TextView_string)]
[VsProvideFileExtensionMapping(typeof(EditorFactoryWithEncoding), "BlackbirdSql Editor with Encoding", 312, 96)]

[VsProvideEditorFactory(typeof(SqlResultsEditorFactory), 313, false,
	DefaultName = "BlackbirdSql Results", CommonPhysicalViewAttributes = 0)]
[ProvideEditorLogicalView(typeof(SqlResultsEditorFactory), VSConstants.LOGVIEWID.Debugging_string)]
[ProvideEditorLogicalView(typeof(SqlResultsEditorFactory), VSConstants.LOGVIEWID.Code_string)]
[ProvideEditorLogicalView(typeof(SqlResultsEditorFactory), VSConstants.LOGVIEWID.Designer_string)]
[ProvideEditorLogicalView(typeof(SqlResultsEditorFactory), VSConstants.LOGVIEWID.TextView_string)]


[ProvideMenuResource("Menus.ctmenu", 1)]


#endregion Class Attributes





// =========================================================================================================
#region							EditorExtensionAsyncPackage Class Declaration
// =========================================================================================================
public abstract class EditorExtensionAsyncPackage : AbstractAsyncPackage, IBEditorPackage,
	IVsTextMarkerTypeProvider, Microsoft.VisualStudio.OLE.Interop.IServiceProvider,
	IVsFontAndColorDefaultsProvider, IVsBroadcastMessageEvents
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - EditorExtensionAsyncPackage
	// ---------------------------------------------------------------------------------


	public EditorExtensionAsyncPackage() : base()
	{
		_EventsManager = EditorEventsManager.CreateInstance(_Controller);
	}


	/// <summary>
	/// Gets the singleton AbstractPackageController instance
	/// </summary>
	public static EditorExtensionAsyncPackage Instance
	{
		get
		{
			if (_Instance == null)
			{
				TypeAccessException ex = new(Resources.ErrCannotInstantiateFromAbstractAncestor);
				Diag.Dug(ex);
				throw ex;
			}

			return (EditorExtensionAsyncPackage)_Instance;
		}
	}


	/// <summary>
	/// EditorExtensionAsyncPackage package disposal
	/// </summary>
	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			// Tracer.Trace(GetType(), "Dispose()", "", null);

			if (ThreadHelper.CheckAccess() && GetGlobalService(typeof(SVsShell)) is IVsShell vsShell)
			{
				Native.ThrowOnFailure(vsShell.UnadviseBroadcastMessages(_VsBroadcastMessageEventsCookie));
			}

			_EventsManager?.Dispose();

			if (!ThreadHelper.CheckAccess())
				return;

			if (GetGlobalService(typeof(SProfferService)) is IProfferService profferService)
			{
				Native.ThrowOnFailure(profferService.RevokeService(_MarkerServiceCookie));
				Native.ThrowOnFailure(profferService.RevokeService(_FontAndColorServiceCookie));
			}
		}
	}


	#endregion Additional Constructors / Destructors




	// =========================================================================================================
	#region Fields - EditorExtensionAsyncPackage
	// =========================================================================================================


	// private static readonly string _TName = typeof(EditorExtensionAsyncPackage).Name;

	private readonly EditorEventsManager _EventsManager;
	private uint _VsBroadcastMessageEventsCookie;
	private EditorFactoryWithoutEncoding _SqlEditorFactory;
	private EditorFactoryWithEncoding _SqlEditorFactoryWithEncoding;
	private SqlResultsEditorFactory _SqlResultsEditorFactory;
	private uint _MarkerServiceCookie;
	private uint _FontAndColorServiceCookie;


	#endregion Fields





	// =========================================================================================================
	#region Property accessors - EditorExtensionAsyncPackage
	// =========================================================================================================


	private Dictionary<object, AuxiliaryDocData> _DocDataEditors = null;

	public IBSqlEditorWindowPane LastFocusedSqlEditor { get; set; }


	public Dictionary<object, AuxiliaryDocData> DocDataEditors => _DocDataEditors ??= [];


	public bool EnableSpatialResultsTab { get; set; }


	public override IBEventsManager EventsManager => _EventsManager;



	#endregion Property accessors




	// =========================================================================================================
	#region Methods and Implementations - EditorExtensionAsyncPackage
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Asynchronous initialization of the package. The class must register services it
	/// requires using the ServicesCreatorCallback method.
	/// </summary>
	protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
	{
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

		if (cancellationToken.IsCancellationRequested)
			return;

		await base.FinalizeAsync(cancellationToken, progress);

		if (await GetServiceAsync(typeof(IProfferService)) is not IProfferService obj)
			throw Diag.ExceptionService(typeof(OleMenuCommandService));

		Guid rguidMarkerService = LibraryData.CLSID_EditorMarkerService;
		Native.ThrowOnFailure(
			obj.ProfferService(ref rguidMarkerService, this, out _MarkerServiceCookie),
			(string)null);

		Guid rguidService = LibraryData.CLSID_FontAndColorService;
		Native.ThrowOnFailure(obj.ProfferService(ref rguidService, this, out _FontAndColorServiceCookie));


		ServiceContainer.AddService(typeof(IBDesignerExplorerServices), ServicesCreatorCallbackAsync, promote: true);
		ServiceContainer.AddService(typeof(IBDesignerOnlineServices), ServicesCreatorCallbackAsync, promote: true);
		// Services.AddService(typeof(ISqlEditorStrategyProvider), ServicesCreatorCallbackAsync, promote: true);

		_SqlEditorFactory = new EditorFactoryWithoutEncoding();
		_SqlEditorFactoryWithEncoding = new EditorFactoryWithEncoding();
		_SqlResultsEditorFactory = new SqlResultsEditorFactory();

		RegisterEditorFactory(_SqlEditorFactory);
		RegisterEditorFactory(_SqlEditorFactoryWithEncoding);
		RegisterEditorFactory(_SqlResultsEditorFactory);


		Native.ThrowOnFailure(
			(GetGlobalService(typeof(SVsShell)) as IVsShell).AdviseBroadcastMessages(this, out _VsBroadcastMessageEventsCookie),
			(string)null);


		InitializeTabbedEditorToolbarHandlerManager();

		await DefineCommandsAsync();

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
		else if (serviceType == typeof(IBDesignerOnlineServices))
		{
			object service = new DesignerOnlineServices()
				?? throw Diag.ExceptionService(serviceType);

			return service;
		}
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
			|| serviceType == typeof(IBDesignerOnlineServices))
		{
			return await CreateServiceInstanceAsync(serviceType, token);
		}


		return await base.ServicesCreatorCallbackAsync(container, token, serviceType);
	}



	private static void InitializeTabbedEditorToolbarHandlerManager()
	{
		TabbedEditorToolbarHandlerManager toolbarMgr = AbstractTabbedEditorPane.ToolbarManager;
		if (toolbarMgr != null)
		{
			Guid clsid = LibraryData.CLSID_CommandSet;

			toolbarMgr.AddMapping(typeof(SqlEditorTabbedEditorPane),
				new SqlEditorToolbarCommandHandler<SqlEditorDatabaseCommand>(clsid, (uint)EnCommandSet.CmbIdSqlDatabases));
			toolbarMgr.AddMapping(typeof(SqlEditorTabbedEditorPane),
				new SqlEditorToolbarCommandHandler<SqlEditorDatabaseListCommand>(clsid, (uint)EnCommandSet.CmbIdSqlDatabasesGetList));
			toolbarMgr.AddMapping(typeof(SqlEditorTabbedEditorPane),
				new SqlEditorToolbarCommandHandler<SqlEditorExecuteQueryCommand>(clsid, (uint)EnCommandSet.CmdIdExecuteQuery));
			toolbarMgr.AddMapping(typeof(SqlEditorTabbedEditorPane),
				new SqlEditorToolbarCommandHandler<SqlEditorParseQueryCommand>(clsid, (uint)EnCommandSet.CmdIdParseQuery));
			toolbarMgr.AddMapping(typeof(SqlEditorTabbedEditorPane),
				new SqlEditorToolbarCommandHandler<SqlEditorCancelQueryCommand>(clsid, (uint)EnCommandSet.CmdIdCancelQuery));
			toolbarMgr.AddMapping(typeof(SqlEditorTabbedEditorPane),
				new SqlEditorToolbarCommandHandler<SqlEditorConnectCommand>(clsid, (uint)EnCommandSet.CmdIdConnect));
			toolbarMgr.AddMapping(typeof(SqlEditorTabbedEditorPane),
				new SqlEditorToolbarCommandHandler<SqlEditorDisconnectCommand>(clsid, (uint)EnCommandSet.CmdIdDisconnect));
			toolbarMgr.AddMapping(typeof(SqlEditorTabbedEditorPane),
				new SqlEditorToolbarCommandHandler<SqlEditorDisconnectAllQueriesCommand>(clsid, (uint)EnCommandSet.CmdIdDisconnectAllQueries));
			toolbarMgr.AddMapping(typeof(SqlEditorTabbedEditorPane),
				new SqlEditorToolbarCommandHandler<SqlEditorChangeConnectionCommand>(clsid, (uint)EnCommandSet.CmdIdChangeConnection));
			toolbarMgr.AddMapping(typeof(SqlEditorTabbedEditorPane),
				new SqlEditorToolbarCommandHandler<SqlEditorShowEstimatedPlanCommand>(clsid, (uint)EnCommandSet.CmdIdShowEstimatedPlan));
			toolbarMgr.AddMapping(typeof(SqlEditorTabbedEditorPane),
				new SqlEditorToolbarCommandHandler<SqlEditorCloneQueryWindowCommand>(clsid, (uint)EnCommandSet.CmdIdCloneQuery));
			toolbarMgr.AddMapping(typeof(SqlEditorTabbedEditorPane),
				new SqlEditorToolbarCommandHandler<SqlEditorToggleSqlCmdModeCommand>(clsid, (uint)EnCommandSet.CmdIdToggleSQLCMDMode));
			toolbarMgr.AddMapping(typeof(SqlEditorTabbedEditorPane),
				new SqlEditorToolbarCommandHandler<SqlEditorToggleExecutionPlanCommand>(clsid, (uint)EnCommandSet.CmdIdToggleExecutionPlan));
			toolbarMgr.AddMapping(typeof(SqlEditorTabbedEditorPane),
				new SqlEditorToolbarCommandHandler<SqlEditorToggleClientStatisticsCommand>(clsid, (uint)EnCommandSet.CmdIdToggleClientStatistics));
			toolbarMgr.AddMapping(typeof(SqlEditorTabbedEditorPane),
				new SqlEditorToolbarCommandHandler<SqlEditorNewQueryCommand>(clsid, (uint)EnCommandSet.CmdIdNewSqlQuery));
		}
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



	private void OnNewSqlQuery(object sender, EventArgs e)
	{
		// Tracer.Trace(GetType(), "OnNewSqlQuery()");

		using (DpiAwareness.EnterDpiScope(DpiAwarenessContext.SystemAware))
		{
			Cmd.OpenNewMiscellaneousSqlFile(new ServiceProvider(Instance.OleServiceProvider));
			IBSqlEditorWindowPane lastFocusedSqlEditor = Instance.LastFocusedSqlEditor;
			if (lastFocusedSqlEditor != null)
			{
				new SqlEditorNewQueryCommand(lastFocusedSqlEditor).Exec((uint)OLECMDEXECOPT.OLECMDEXECOPT_DODEFAULT, IntPtr.Zero, IntPtr.Zero);
				GetAuxiliaryDocData(lastFocusedSqlEditor.DocData).IsQueryWindow = true;
			}
		}
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



	private void CycleToNextEditorTab(object sender, EventArgs e)
	{
		LastFocusedSqlEditor?.ActivateNextTab();
	}



	private void CycleToPreviousEditorTab(object sender, EventArgs e)
	{
		LastFocusedSqlEditor?.ActivatePreviousTab();
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
		lock (Controller.LockGlobal)
		{
			return _DocDataEditors != null && _DocDataEditors.Count > 0;
		}
	}


	int IVsBroadcastMessageEvents.OnBroadcastMessage(uint message, IntPtr wParam, IntPtr lParam)
	{
		if (message == 536)
		{
			OnPowerBroadcast(wParam, lParam);
		}

		return VSConstants.S_OK;
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



	private void OnPowerBroadcast(IntPtr wParam, IntPtr lParam)
	{
		if ((int)wParam != 4 || _DocDataEditors == null)
		{
			return;
		}

		foreach (AuxiliaryDocData value in _DocDataEditors.Values)
		{
			QueryManager qryMgr = value.QryMgr;
			if (qryMgr != null && qryMgr.IsConnected)
			{
				qryMgr.ConnectionStrategy.Connection?.Close();
			}
		}
	}



	public AuxiliaryDocData GetAuxiliaryDocData(object docData)
	{
		lock (Controller.LockGlobal)
		{
			if (docData == null || _DocDataEditors == null)
			{
				return null;
			}

			_DocDataEditors.TryGetValue(docData, out AuxiliaryDocData value);
			return value;
		}
	}



	public void EnsureAuxilliaryDocData(IVsHierarchy hierarchy, string documentMoniker, object docData)
	{
		lock (Controller.LockGlobal)
		{
			if (_DocDataEditors == null || !_DocDataEditors.TryGetValue(docData, out AuxiliaryDocData value))
			{
				value = new AuxiliaryDocData(docData);
				// hierarchy.GetSite(out Microsoft.VisualStudio.OLE.Interop.IServiceProvider ppSP);

				// Not point looking because This will always be null for us
				IBSqlEditorStrategyProvider sqlEditorStrategyProvider = null;
				// ISqlEditorStrategyProvider sqlEditorStrategyProvider = new ServiceProvider(ppSP).GetService(typeof(ISqlEditorStrategyProvider)) as ISqlEditorStrategyProvider;

				// We always use DefaultSqlEditorStrategy and use the csb passed via userdata that was
				// contructed from the SE node
				IBSqlEditorStrategy sqlEditorStrategy = (sqlEditorStrategyProvider == null
					? new DefaultSqlEditorStrategy(value.GetUserDataCsb())
					: sqlEditorStrategyProvider.CreateEditorStrategy(documentMoniker, value));

				value.Strategy = sqlEditorStrategy;
				if (value.Strategy.IsDw)
					value.IntellisenseEnabled = false;

				DocDataEditors.Add(docData, value);
			}
		}
	}



	public bool ContainsEditorStatus(object docData)
	{
		lock (Controller.LockGlobal)
		{
			if (docData == null || _DocDataEditors == null)
			{
				return false;
			}

			if (_DocDataEditors == null)
				return false;

			if (_DocDataEditors.ContainsKey(docData))
				return true;
		}

		return false;
	}



	public void RemoveEditorStatus(object docData)
	{
		lock (Controller.LockGlobal)
		{
			if (_DocDataEditors == null)
				return;

			if (_DocDataEditors.TryGetValue(docData, out AuxiliaryDocData value))
			{
				_DocDataEditors.Remove(docData);
				value?.Dispose();
			}
		}
	}



	public void SetSqlEditorStrategyForDocument(object docData, IBSqlEditorStrategy strategy)
	{
		lock (Controller.LockGlobal)
		{
			if (_DocDataEditors == null || !_DocDataEditors.TryGetValue(docData, out var value))
			{
				ArgumentException ex = new("No auxillary information for DocData");
				Diag.Dug(ex);
				throw ex;
			}

			value.Strategy = strategy;
		}
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
				result = FormUtilities.ShowDialog(dlg);
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


	#endregion Methods and Implementations




	// =========================================================================================================
	#region Event handlers - BlackbirdSqlDdexExtension
	// =========================================================================================================


	#endregion Event handlers


}

#endregion Class Declaration
