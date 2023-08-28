// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.VSIntegration.SqlEditorPackage


using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data.Common;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using BlackbirdSql.Common;
using BlackbirdSql.Common.Config;
using BlackbirdSql.Common.Controls;
using BlackbirdSql.Common.Ctl;
using BlackbirdSql.Common.Ctl.Commands;
using BlackbirdSql.Common.Enums;
using BlackbirdSql.Common.Events;
using BlackbirdSql.Common.Interfaces;
using BlackbirdSql.Common.Model;
using BlackbirdSql.Common.Model.Interfaces;
using BlackbirdSql.Common.Model.QueryExecution;
using BlackbirdSql.Common.Properties;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Core.Providers;
using BlackbirdSql.EditorExtension.Config;
using BlackbirdSql.Wpf.Controls;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

using Cmd = BlackbirdSql.Common.Cmd;
using Native = BlackbirdSql.Common.Native;
using Tracer = BlackbirdSql.Core.Diagnostics.Tracer;
using MonikerAgent = BlackbirdSql.Core.Model.MonikerAgent;


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
[ProvideOptionPage(typeof(SqlResultsGeneralOptionsPage), "BlackbirdSql Editor", "Query Results", 14, 15, true)]
[ProvideOptionPage(typeof(SqlResultsToTextOptionsPage), "BlackbirdSql Editor", "Results To Text", 14, 18, true)]
[ProvideOptionPage(typeof(SqlResultsToGridOptionsPage), "BlackbirdSql Editor", "Results To Grid", 14, 19, true)]
[ProvideOptionPage(typeof(SqlExecutionGeneralOptionsPage), "BlackbirdSql Editor", "Query Execution", 13, 15, true)]
[ProvideOptionPage(typeof(SqlExecutionAdvancedOptionsPage), "BlackbirdSql Editor", "Advanced Query Execution", 13, 16, true)]
[ProvideOptionPage(typeof(SqlExecutionAnsiOptionsPage), "BlackbirdSql Editor", "ANSI Query Execution", 13, 17, true)]
[ProvideOptionPage(typeof(SqlEditorGeneralSettingsDialogPage), "BlackbirdSql Editor", "General", 11, 20, true)]
[ProvideOptionPage(typeof(SqlEditorTabAndStatusBarSettingsDialogPage), "BlackbirdSql Editor", "Editor Tab and Status Bar", 11, 21, true)]

[VsProvideEditorFactory(typeof(EditorFactoryWithoutEncoding), 111, false, DefaultName = "BlackbirdSql Editor",
	CommonPhysicalViewAttributes = (int)__VSPHYSICALVIEWATTRIBUTES.PVA_SupportsPreview,
	TrustLevel = __VSEDITORTRUSTLEVEL.ETL_AlwaysTrusted)]
[ProvideEditorExtension(typeof(EditorFactoryWithoutEncoding), MonikerAgent.C_SqlExtension, 110,
	DefaultName = "BlackbirdSql Editor", NameResourceID = 111, RegisterFactory = true)]
[ProvideEditorLogicalView(typeof(EditorFactoryWithoutEncoding), VSConstants.LOGVIEWID.Debugging_string)]
[ProvideEditorLogicalView(typeof(EditorFactoryWithoutEncoding), VSConstants.LOGVIEWID.Code_string)]
[ProvideEditorLogicalView(typeof(EditorFactoryWithoutEncoding), VSConstants.LOGVIEWID.Designer_string)]
[ProvideEditorLogicalView(typeof(EditorFactoryWithoutEncoding), VSConstants.LOGVIEWID.TextView_string)]
[VsProvideFileExtensionMapping(typeof(EditorFactoryWithoutEncoding), "BlackbirdSql Editor", 111, 100)]

[VsProvideEditorFactory(typeof(EditorFactoryWithEncoding), 112, false, DefaultName = "BlackbirdSql Editor with Encoding",
	CommonPhysicalViewAttributes = (int)__VSPHYSICALVIEWATTRIBUTES.PVA_SupportsPreview,
	TrustLevel = __VSEDITORTRUSTLEVEL.ETL_AlwaysTrusted)]
[ProvideEditorExtension(typeof(EditorFactoryWithEncoding), MonikerAgent.C_SqlExtension, 100,
	DefaultName = "BlackbirdSql Editor with Encoding", NameResourceID = 112, RegisterFactory = true)]
[ProvideEditorLogicalView(typeof(EditorFactoryWithEncoding), VSConstants.LOGVIEWID.Debugging_string)]
[ProvideEditorLogicalView(typeof(EditorFactoryWithEncoding), VSConstants.LOGVIEWID.Code_string)]
[ProvideEditorLogicalView(typeof(EditorFactoryWithEncoding), VSConstants.LOGVIEWID.Designer_string)]
[ProvideEditorLogicalView(typeof(EditorFactoryWithEncoding), VSConstants.LOGVIEWID.TextView_string)]
[VsProvideFileExtensionMapping(typeof(EditorFactoryWithEncoding), "BlackbirdSql Editor with Encoding", 112, 96)]

[VsProvideEditorFactory(typeof(SqlResultsEditorFactory), 113, false, DefaultName = "BlackbirdSql Results", CommonPhysicalViewAttributes = 0)]
[ProvideEditorLogicalView(typeof(SqlResultsEditorFactory), VSConstants.LOGVIEWID.Debugging_string)]
[ProvideEditorLogicalView(typeof(SqlResultsEditorFactory), VSConstants.LOGVIEWID.Code_string)]
[ProvideEditorLogicalView(typeof(SqlResultsEditorFactory), VSConstants.LOGVIEWID.Designer_string)]
[ProvideEditorLogicalView(typeof(SqlResultsEditorFactory), VSConstants.LOGVIEWID.TextView_string)]


[ProvideMenuResource("Menus.ctmenu", 1)]


#endregion Class Attributes





// =========================================================================================================
#region							EditorExtensionAsyncPackage Class Declaration
// =========================================================================================================


public abstract class EditorExtensionAsyncPackage : AbstractAsyncPackage, IBEditorPackage, IVsTextMarkerTypeProvider, Microsoft.VisualStudio.OLE.Interop.IServiceProvider, IVsFontAndColorDefaultsProvider, IVsBroadcastMessageEvents
{

	#region Variables - EditorExtensionAsyncPackage


	// private static readonly string _TName = typeof(EditorExtensionAsyncPackage).Name;


	private uint _VsBroadcastMessageEventsCookie;

	private EditorEventsManager _EventsManager;


	private EditorFactoryWithoutEncoding _SqlEditorFactory;

	private EditorFactoryWithEncoding _SqlEditorFactoryWithEncoding;

	private SqlResultsEditorFactory _SqlResultsEditorFactory;


	private uint _MarkerServiceCookie;

	private uint _FontAndColorServiceCookie;


	#endregion Variables





	// =========================================================================================================
	#region Property accessors - EditorExtensionAsyncPackage
	// =========================================================================================================


	public Dictionary<object, AuxiliaryDocData> DocDataToEditorStatus { get; set; }

	public ISqlEditorWindowPane LastFocusedSqlEditor { get; set; }

	public Dictionary<object, AuxiliaryDocData> DocDataEditors => DocDataToEditorStatus;


	public bool EnableSpatialResultsTab { get; set; }



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Accessor to this package's <see cref="IBLanguageService"/> else null.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public virtual IBLanguageService LanguageService => throw new NotImplementedException();


	public override IBEventsManager EventsManager => _EventsManager ??= new EditorEventsManager(Controller);


	#endregion Property accessors





	// =========================================================================================================
	#region Constructors / Destructors - EditorExtensionAsyncPackage
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// EditorExtensionAsyncPackage package .ctor
	/// </summary>
	// ---------------------------------------------------------------------------------
	public EditorExtensionAsyncPackage() : base()
	{
		Tracer.Trace(GetType(), "ctor()", "", null);
		DocDataToEditorStatus = new Dictionary<object, AuxiliaryDocData>();
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the singleton AbstractPackageController instance
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static EditorExtensionAsyncPackage Instance
	{
		get
		{
			if (_Instance == null)
			{
				NullReferenceException ex = new("Cannot instantiate EditorExtensionAsyncPackage from abstract ancestor");
				Diag.Dug(ex);
				throw ex;
			}

			return (EditorExtensionAsyncPackage)_Instance;
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// EditorExtensionAsyncPackage package disposal
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			Tracer.Trace(GetType(), "Dispose()", "", null);


			if (GetGlobalService(typeof(SVsShell)) is IVsShell vsShell)
			{
				Native.ThrowOnFailure(vsShell.UnadviseBroadcastMessages(_VsBroadcastMessageEventsCookie));
			}

			_EventsManager?.Dispose();



			if (Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SProfferService)) is IProfferService profferService)
			{
				Native.ThrowOnFailure(profferService.RevokeService(_MarkerServiceCookie));
				Native.ThrowOnFailure(profferService.RevokeService(_FontAndColorServiceCookie));
			}
		}
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Method Implementations - EditorExtensionAsyncPackage
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Asynchronous initialization of the package. The class must register services it
	/// requires using the ServicesCreatorCallback method.
	/// </summary>
	protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
	{
		await base.InitializeAsync(cancellationToken, progress);

		// Services.AddService(typeof(IBProviderObjectFactory), ServicesCreatorCallbackAsync, promote: true);

		/*
		_ = ((Action)delegate
		{
			ThreadHelper.Generic.Invoke(delegate
			{
				try
				{
					DemandLoadPackage("9A62B3CA-5BDF-47CB-A406-3CEB946F1DDF");
				}
				catch (Exception)
				{
				}
			});
		}).BeginInvoke(null, null);
		*/

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
		if (cancellationToken.IsCancellationRequested)
			return;

		// await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
		await base.FinalizeAsync(cancellationToken, progress);

		if (await GetServiceAsync(typeof(IProfferService)) is not IProfferService obj)
			return;

		Guid rguidMarkerService = LibraryData.CLSID_EditorMarkerService;
		Native.ThrowOnFailure(
			obj.ProfferService(ref rguidMarkerService, this, out _MarkerServiceCookie),
			(string)null);

		Guid rguidService = LibraryData.CLSID_FontAndColorService;
		Native.ThrowOnFailure(obj.ProfferService(ref rguidService, this, out _FontAndColorServiceCookie));

		Services.AddService(typeof(IBDesignerExplorerServices), ServicesCreatorCallbackAsync, promote: true);
		Services.AddService(typeof(IBDesignerOnlineServices), ServicesCreatorCallbackAsync, promote: true);

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

		Controller.RegisterEventsManager(EventsManager);

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
			object service;
			try
			{
				service = new DesignerExplorerServices();
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw ex;
			}

			if (service == null)
			{
				TypeAccessException ex = new(serviceType.FullName);
				Diag.Dug(ex);
				throw ex;
			}

			return service;
		}
		else if (serviceType == typeof(IBDesignerOnlineServices))
		{
			object service;
			try
			{
				service = new DesignerOnlineServices();
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw ex;
			}

			if (service == null)
			{
				TypeAccessException ex = new(serviceType.FullName);
				Diag.Dug(ex);
				throw ex;
			}

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

		if (serviceType == typeof(IBDesignerExplorerServices) || serviceType == typeof(IBDesignerOnlineServices))
			return await CreateServiceInstanceAsync(serviceType, token);


		return await base.ServicesCreatorCallbackAsync(container, token, serviceType);
	}



	private static void InitializeTabbedEditorToolbarHandlerManager()
	{
		TabbedEditorToolbarHandlerManager toolbarMgr = AbstractTabbedEditorPane.ToolbarManager;
		if (toolbarMgr != null)
		{
			Guid clsid = LibraryData.CLSID_SqlEditorCommandSet;

			toolbarMgr.AddMapping(typeof(SqlEditorTabbedEditorPane),
				new SqlEditorToolbarCommandHandler<SqlEditorSqlDatabaseCommand>(clsid, (uint)SqlEditorCmdSet.CmbIdSQLDatabases));
			toolbarMgr.AddMapping(typeof(SqlEditorTabbedEditorPane),
				new SqlEditorToolbarCommandHandler<SqlEditorSqlDatabaseListCommand>(clsid, (uint)SqlEditorCmdSet.CmbIdSQLDatabasesGetList));
			toolbarMgr.AddMapping(typeof(SqlEditorTabbedEditorPane),
				new SqlEditorToolbarCommandHandler<SqlEditorExecuteQueryCommand>(clsid, (uint)SqlEditorCmdSet.CmdIdExecuteQuery));
			toolbarMgr.AddMapping(typeof(SqlEditorTabbedEditorPane),
				new SqlEditorToolbarCommandHandler<SqlEditorParseQueryCommand>(clsid, (uint)SqlEditorCmdSet.CmdIdParseQuery));
			toolbarMgr.AddMapping(typeof(SqlEditorTabbedEditorPane),
				new SqlEditorToolbarCommandHandler<SqlEditorCancelQueryCommand>(clsid, (uint)SqlEditorCmdSet.CmdIdCancelQuery));
			toolbarMgr.AddMapping(typeof(SqlEditorTabbedEditorPane),
				new SqlEditorToolbarCommandHandler<SqlEditorConnectCommand>(clsid, (uint)SqlEditorCmdSet.CmdIdConnect));
			toolbarMgr.AddMapping(typeof(SqlEditorTabbedEditorPane),
				new SqlEditorToolbarCommandHandler<SqlEditorDisconnectCommand>(clsid, (uint)SqlEditorCmdSet.CmdIdDisconnect));
			toolbarMgr.AddMapping(typeof(SqlEditorTabbedEditorPane),
				new SqlEditorToolbarCommandHandler<SqlEditorDisconnectAllQueriesCommand>(clsid, (uint)SqlEditorCmdSet.CmdIdDisconnectAllQueries));
			toolbarMgr.AddMapping(typeof(SqlEditorTabbedEditorPane),
				new SqlEditorToolbarCommandHandler<SqlEditorChangeConnectionCommand>(clsid, (uint)SqlEditorCmdSet.CmdIdChangeConnection));
			toolbarMgr.AddMapping(typeof(SqlEditorTabbedEditorPane),
				new SqlEditorToolbarCommandHandler<SqlEditorShowEstimatedPlanCommand>(clsid, (uint)SqlEditorCmdSet.CmdIdShowEstimatedPlan));
			toolbarMgr.AddMapping(typeof(SqlEditorTabbedEditorPane),
				new SqlEditorToolbarCommandHandler<SqlEditorCloneQueryWindowCommand>(clsid, (uint)SqlEditorCmdSet.CmdIdCloneQuery));
			toolbarMgr.AddMapping(typeof(SqlEditorTabbedEditorPane),
				new SqlEditorToolbarCommandHandler<SqlEditorToggleSqlCmdModeCommand>(clsid, (uint)SqlEditorCmdSet.CmdIdToggleSQLCMDMode));
			toolbarMgr.AddMapping(typeof(SqlEditorTabbedEditorPane),
				new SqlEditorToolbarCommandHandler<SqlEditorToggleExecutionPlanCommand>(clsid, (uint)SqlEditorCmdSet.CmdIdToggleExecutionPlan));
		}
	}



	private async Task DefineCommandsAsync()
	{
		OleMenuCommandService commandService = await GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;

		if (await GetServiceAsync(typeof(IMenuCommandService)) is OleMenuCommandService oleMenuCommandService)
		{
			Guid clsid = LibraryData.CLSID_SqlEditorCommandSet;

			CommandID id = new CommandID(clsid, (int)SqlEditorCmdSet.CmdIdNewQueryConnection);
			OleMenuCommand oleMenuCommand = new OleMenuCommand(OnNewQueryConnection, id);
			oleMenuCommand.BeforeQueryStatus += EnableCommand;
			oleMenuCommandService.AddCommand(oleMenuCommand);
			CommandID id2 = new CommandID(clsid, (int)SqlEditorCmdSet.CmdidCycleToNextTab);
			OleMenuCommand oleMenuCommand2 = new OleMenuCommand(CycleToNextEditorTab, id2);
			oleMenuCommand2.BeforeQueryStatus += EnableCommand;
			oleMenuCommandService.AddCommand(oleMenuCommand2);
			CommandID id3 = new CommandID(clsid, (int)SqlEditorCmdSet.CmdidCycleToPrevious);
			OleMenuCommand oleMenuCommand3 = new OleMenuCommand(CycleToPreviousEditorTab, id3);
			oleMenuCommand3.BeforeQueryStatus += EnableCommand;
			oleMenuCommandService.AddCommand(oleMenuCommand3);
		}
		else
		{
			ServiceUnavailableException ex = new(typeof(IMenuCommandService));
			Diag.Dug(ex);
		}
	}



	private void OnNewQueryConnection(object sender, EventArgs e)
	{
		using (DpiAwareness.EnterDpiScope(DpiAwarenessContext.SystemAware))
		{
			Cmd.OpenNewMiscellaneousSqlFile(new ServiceProvider(Instance.OleServiceProvider));
			ISqlEditorWindowPane lastFocusedSqlEditor = Instance.LastFocusedSqlEditor;
			if (lastFocusedSqlEditor != null)
			{
				new SqlEditorConnectCommand(lastFocusedSqlEditor).Exec((uint)OLECMDEXECOPT.OLECMDEXECOPT_DODEFAULT, IntPtr.Zero, IntPtr.Zero);
				GetAuxiliaryDocData(lastFocusedSqlEditor.DocData).IsQueryWindow = true;
			}
		}
	}



	private void EnableCommand(object sender, EventArgs e)
	{
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
		IntPtr intPtr = IntPtr.Zero;
		try
		{
			intPtr = Marshal.GetIUnknownForObject(serviceObject);
			Marshal.QueryInterface(intPtr, ref interfaceGuid, out service);
		}
		finally
		{
			if (intPtr != IntPtr.Zero)
			{
				Marshal.Release(intPtr);
			}
		}
	}



	int IVsTextMarkerTypeProvider.GetTextMarkerType(ref Guid markerGuid, out IVsPackageDefinedTextMarkerType vsTextMarker)
	{
		if (markerGuid == VS.CLSID_TSqlEditorMessageErrorMarker)
		{
			vsTextMarker = new VsTextMarker((uint)MARKERVISUAL.MV_COLOR_ALWAYS, COLORINDEX.CI_RED, COLORINDEX.CI_USERTEXT_BK, SharedResx.ErrorMarkerDisplayName, SharedResx.ErrorMarkerDescription, "Error Message");
			return VSConstants.S_OK;
		}

		vsTextMarker = null;
		return VSConstants.E_INVALIDARG;
	}



	public bool HasAnyAuxillaryDocData()
	{
		lock (Controller.PackageLock)
		{
			return DocDataToEditorStatus.Count > 0;
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
		if (rguidCategory == VS.CLSID_FontAndColorsSqlResultsTextCategory)
		{
			ppObj = FontAndColorProviderTextResults.Instance;
		}
		else if (rguidCategory == VS.CLSID_FontAndColorsSqlResultsGridCategory)
		{
			ppObj = FontAndColorProviderGridResults.Instance;
		}
		else if (rguidCategory == VS.CLSID_FontAndColorsSqlResultsExecutionPlanCategory)
		{
			ppObj = FontAndColorProviderExecutionPlan.Instance;
		}

		return VSConstants.S_OK;
	}



	private void OnPowerBroadcast(IntPtr wParam, IntPtr lParam)
	{
		if ((int)wParam != 4)
		{
			return;
		}

		foreach (AuxiliaryDocData value in DocDataToEditorStatus.Values)
		{
			QueryExecutor queryExecutor = value.QueryExecutor;
			if (queryExecutor != null && queryExecutor.IsConnected)
			{
				queryExecutor.ConnectionStrategy.Connection?.Close();
			}
		}
	}



	public AuxiliaryDocData GetAuxiliaryDocData(object docData)
	{
		if (docData == null)
		{
			return null;
		}

		lock (Controller.PackageLock)
		{
			DocDataToEditorStatus.TryGetValue(docData, out AuxiliaryDocData value);
			return value;
		}
	}



	public void EnsureAuxilliaryDocData(IVsHierarchy hierarchy, string documentMoniker, object docData)
	{
		lock (Controller.PackageLock)
		{
			if (!Instance.DocDataToEditorStatus.TryGetValue(docData, out AuxiliaryDocData value))
			{
				value = new AuxiliaryDocData(docData);
				// hierarchy.GetSite(out Microsoft.VisualStudio.OLE.Interop.IServiceProvider ppSP);

				/*
				_ = value.Strategy = new ServiceProvider(ppSP).GetService(typeof(ISqlEditorStrategyProvider))
					is ISqlEditorStrategyProvider sqlEditorStrategyProvider
					? sqlEditorStrategyProvider.CreateEditorStrategy(documentMoniker, value)
					: new DefaultSqlEditorStrategy();
				*/

				DbConnectionStringBuilder csb = value.GetUserDataCsb();
				_ = value.Strategy = new DefaultSqlEditorStrategy(csb);

				if (value.Strategy.IsDw)
				{
					value.IntellisenseEnabled = false;
				}

				Instance.DocDataToEditorStatus.Add(docData, value);
			}
		}
	}



	public bool ContainsEditorStatus(object docData)
	{
		if (docData == null)
		{
			return false;
		}

		lock (Controller.PackageLock)
		{
			if (DocDataToEditorStatus.ContainsKey(docData))
			{
				return true;
			}
		}

		return false;
	}



	public void RemoveEditorStatus(object docData)
	{
		lock (Controller.PackageLock)
		{
			if (DocDataToEditorStatus.TryGetValue(docData, out AuxiliaryDocData value))
			{
				DocDataToEditorStatus.Remove(docData);
				value?.Dispose();
			}
		}
	}



	public void SetSqlEditorStrategyForDocument(object docData, ISqlEditorStrategy strategy)
	{
		lock (Controller.PackageLock)
		{
			if (!DocDataToEditorStatus.TryGetValue(docData, out var value))
			{
				ArgumentException ex = new("No auxillary information for DocData");
				Diag.Dug(ex);
				throw ex;
			}

			value.Strategy = strategy;
		}
	}

	public bool? ShowConnectionDialogFrame(IntPtr parent, IBDependencyManager dependencyManager, EventsChannel channel,
		UIConnectionInfo ci, VerifyConnectionDelegate verifierDelegate, ConnectionDialogConfiguration config,
		ref UIConnectionInfo uIConnectionInfo)
	{ 
		ConnectionDialogFrame connectionDialogFrame = new ConnectionDialogFrame(dependencyManager, channel, ci, verifierDelegate, config);

		connectionDialogFrame.ShowModal(parent);
		uIConnectionInfo = connectionDialogFrame.UIConnectionInfo;
		connectionDialogFrame.ViewModel.CloseSections();

		return connectionDialogFrame.DialogResult;
	}

	#endregion Method Implementations

}

#endregion Class Declaration
