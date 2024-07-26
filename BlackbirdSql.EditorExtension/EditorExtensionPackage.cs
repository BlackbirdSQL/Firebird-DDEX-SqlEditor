// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.VSIntegration.SqlEditorPackage

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl.CommandProviders;
using BlackbirdSql.Core.Ctl.ComponentModel;
using BlackbirdSql.Core.Enums;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Core.Model;
using BlackbirdSql.EditorExtension.Controls.Config;
using BlackbirdSql.EditorExtension.Ctl;
using BlackbirdSql.EditorExtension.Ctl.Config;
using BlackbirdSql.EditorExtension.Properties;
using BlackbirdSql.LanguageExtension;
using BlackbirdSql.Shared.Controls;
using BlackbirdSql.Shared.Ctl;
using BlackbirdSql.Shared.Ctl.Commands;
using BlackbirdSql.Shared.Ctl.ComponentModel;
using BlackbirdSql.Shared.Ctl.Config;
using BlackbirdSql.Shared.Ctl.QueryExecution;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Shared.Model;
using BlackbirdSql.Sys;
using BlackbirdSql.Sys.Enums;
using BlackbirdSql.Sys.Interfaces;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Threading;
using OleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;



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


[VsProvideEditorFactory(typeof(EditorFactoryWithoutEncoding), 311, false, DefaultName = PackageData.ServiceName,
	CommonPhysicalViewAttributes = (int)__VSPHYSICALVIEWATTRIBUTES.PVA_SupportsPreview,
	TrustLevel = __VSEDITORTRUSTLEVEL.ETL_AlwaysTrusted)]
[ProvideEditorExtension(typeof(EditorFactoryWithoutEncoding), LanguageExtension.PackageData.Extension, 110,
	DefaultName = PackageData.ServiceName, NameResourceID = 311, RegisterFactory = true)]
// [ProvideEditorLogicalView(typeof(EditorFactoryWithoutEncoding), VSConstants.LOGVIEWID.Debugging_string)]
[ProvideEditorLogicalView(typeof(EditorFactoryWithoutEncoding), VSConstants.LOGVIEWID.Code_string)]
[ProvideEditorLogicalView(typeof(EditorFactoryWithoutEncoding), VSConstants.LOGVIEWID.Designer_string)]
[ProvideEditorLogicalView(typeof(EditorFactoryWithoutEncoding), VSConstants.LOGVIEWID.TextView_string)]
[VsProvideFileExtensionMapping(typeof(EditorFactoryWithoutEncoding), PackageData.ServiceName, 311, 100)]

[VsProvideEditorFactory(typeof(EditorFactoryWithEncoding), 317, false,
	DefaultName = $"{PackageData.ServiceName} with Encoding",
	CommonPhysicalViewAttributes = (int)__VSPHYSICALVIEWATTRIBUTES.PVA_SupportsPreview,
	TrustLevel = __VSEDITORTRUSTLEVEL.ETL_AlwaysTrusted)]
[ProvideEditorExtension(typeof(EditorFactoryWithEncoding), LanguageExtension.PackageData.Extension, 100,
	DefaultName = $"{PackageData.ServiceName} with Encoding", NameResourceID = 317, RegisterFactory = true)]
// [ProvideEditorLogicalView(typeof(EditorFactoryWithEncoding), VSConstants.LOGVIEWID.Debugging_string)]
[ProvideEditorLogicalView(typeof(EditorFactoryWithEncoding), VSConstants.LOGVIEWID.Code_string)]
[ProvideEditorLogicalView(typeof(EditorFactoryWithEncoding), VSConstants.LOGVIEWID.Designer_string)]
[ProvideEditorLogicalView(typeof(EditorFactoryWithEncoding), VSConstants.LOGVIEWID.TextView_string)]
[VsProvideFileExtensionMapping(typeof(EditorFactoryWithEncoding), $"{PackageData.ServiceName} with Encoding", 317, 96)]

[VsProvideEditorFactory(typeof(ResultsEditorFactory), 312, false,
	DefaultName = "BlackbirdSql Results", CommonPhysicalViewAttributes = 0)]
// [ProvideEditorLogicalView(typeof(ResultsEditorFactory), VSConstants.LOGVIEWID.Debugging_string)]
[ProvideEditorLogicalView(typeof(ResultsEditorFactory), VSConstants.LOGVIEWID.Code_string)]
[ProvideEditorLogicalView(typeof(ResultsEditorFactory), VSConstants.LOGVIEWID.Designer_string)]
[ProvideEditorLogicalView(typeof(ResultsEditorFactory), VSConstants.LOGVIEWID.TextView_string)]



#endregion Class Attributes



// =========================================================================================================
#region							EditorExtensionPackage Class Declaration
// =========================================================================================================
public abstract class EditorExtensionPackage : LanguageExtensionPackage, IBsEditorPackage,
	IVsTextMarkerTypeProvider, IVsFontAndColorDefaultsProvider, IVsBroadcastMessageEvents,
	OleServiceProvider
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - EditorExtensionPackage
	// ---------------------------------------------------------------------------------


	protected EditorExtensionPackage() : base()
	{
		_EventsManager = EditorEventsManager.CreateInstance(_ApcInstance);
	}


	/// <summary>
	/// Gets the singleton Package instance
	/// </summary>
	public static new EditorExtensionPackage Instance
	{
		get
		{
			// if (_Instance == null)
			//	DemandLoadPackage(Sys.LibraryData.AsyncPackageGuid, out _);
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
			_CurrentDocData = null;
			_CurrentAuxilliaryDocData = null;

			// Tracer.Trace(GetType(), "Dispose()", "", null);

			if (ThreadHelper.CheckAccess() && GetGlobalService(typeof(SVsShell)) is IVsShell vsShell)
			{
				___(vsShell.UnadviseBroadcastMessages(_BroadcastMessageEventsCookie));
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


	#endregion Constructors / Destructors




	// =========================================================================================================
	#region Constants & Fields - EditorExtensionPackage
	// =========================================================================================================


	// A private 'this' object lock
	private readonly object _LockLocal = new object();


	private Dictionary<object, AuxilliaryDocData> _AuxilliaryDocDataTable = null;

	private object _CurrentDocData = null;
	private AuxilliaryDocData _CurrentAuxilliaryDocData = null;

	// private static readonly string _TName = typeof(EditorExtensionPackage).Name;

	private readonly EditorEventsManager _EventsManager;
	private uint _BroadcastMessageEventsCookie;
	private EditorFactoryWithoutEncoding _SqlEditorFactory;
	private EditorFactoryWithEncoding _SqlEditorFactoryWithEncoding;
	private ResultsEditorFactory _SqlResultsEditorFactory;
	private uint _MarkerServiceCookie;
	private uint _FontAndColorServiceCookie;


	#endregion Constants & Fields





	// =========================================================================================================
	#region Property accessors - EditorExtensionPackage
	// =========================================================================================================

	/*
	[Import]
#pragma warning disable CS8632 // The annotation for nullable reference types warning.
	private JoinableTaskContext? JoinableTaskContext { get; set; }
#pragma warning restore CS8632 // The annotation for nullable reference types warning.
	*/

	public IBsTabbedEditorWindowPane LastFocusedSqlEditor { get; set; }




	public Dictionary<object, AuxilliaryDocData> AuxilliaryDocDataTable => _AuxilliaryDocDataTable ??= [];


	public bool EnableSpatialResultsTab { get; set; }


	public override IBsEventsManager EventsManager => _EventsManager;

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
		ProgressAsync(progress, "InitialIzing Editor ...").Forget();

		await base.InitializeAsync(cancellationToken, progress);

		ProgressAsync(progress, "InitialIzing Editor. Proffering services...").Forget();

		if (await GetServiceAsync(typeof(IProfferService)) is not IProfferService profferSvc)
			throw Diag.ExceptionService(typeof(IProfferService));

		Guid rguidMarkerService = PackageData.CLSID_EditorMarkerService;
		___(profferSvc.ProfferService(ref rguidMarkerService, this, out _MarkerServiceCookie));

		Guid rguidService = PackageData.CLSID_FontAndColorService;
		___(profferSvc.ProfferService(ref rguidService, this, out _FontAndColorServiceCookie));

		ProgressAsync(progress, "InitialIzing Editor. Proffering services... Done.").Forget();

		ProgressAsync(progress, "InitialIzing Editor. Registering Designer Explorer services...").Forget();

		ServiceContainer.AddService(typeof(IBsDesignerExplorerServices), ServicesCreatorCallbackAsync, promote: true);
		// ServiceContainer.AddService(typeof(IBDesignerOnlineServices), ServicesCreatorCallbackAsync, promote: true);
		// Services.AddService(typeof(ISqlEditorStrategyProvider), ServicesCreatorCallbackAsync, promote: true);

		ProgressAsync(progress, "InitialIzing Editor. Registering Designer Explorer services... Done.").Forget();

		ProgressAsync(progress, "InitialIzing Editor. Initializing Tabbed Toolbar manager...").Forget();

		InitializeTabbedEditorToolbarHandlerManager();

		ProgressAsync(progress, "InitialIzing  Editor. Initializing Tabbed Toolbar manager... Done.").Forget();

		ProgressAsync(progress, "InitialIzing Editor ... Done.").Forget();

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


		ProgressAsync(progress, "Finalizing Editor...").Forget();

		ProgressAsync(progress, "Finalizing Editor. Registering Editor factories...").Forget();

		_SqlEditorFactory = new EditorFactoryWithoutEncoding();
		_SqlEditorFactoryWithEncoding = new EditorFactoryWithEncoding();
		_SqlResultsEditorFactory = new ResultsEditorFactory();

		RegisterEditorFactory(_SqlEditorFactory);
		RegisterEditorFactory(_SqlEditorFactoryWithEncoding);
		RegisterEditorFactory(_SqlResultsEditorFactory);

		ProgressAsync(progress, "Finalizing Editor. Registering Editor factories... Done.").Forget();


		ProgressAsync(progress, "Finalizing Editor. Registering OLE commands...").Forget();

		await RegisterOleCommandsAsync();

		ProgressAsync(progress, "Finalizing Editor. Registering OLE commands... Done.").Forget();


		ProgressAsync(progress, "Finalizing Editor. Advising Broadcast messages....").Forget();

		___((GetGlobalService(typeof(SVsShell)) as IVsShell).AdviseBroadcastMessages(this, out _BroadcastMessageEventsCookie));

		ProgressAsync(progress, "Finalizing Editor. Advising Broadcast messages... Done.").Forget();

		await base.FinalizeAsync(cancellationToken, progress);

		ProgressAsync(progress, "Finalizing Editor... Done.").Forget();

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
		else if (serviceType == typeof(IBsDesignerExplorerServices))
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

		if (serviceType == typeof(IBsDesignerExplorerServices)
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


	public bool AuxilliaryDocDataExists(object docData)
	{
		lock (_LockLocal)
		{
			if (docData == null || _AuxilliaryDocDataTable == null)
				return false;

			if (_CurrentDocData != null && object.ReferenceEquals(docData, _CurrentDocData))
				return true;

			if (AuxilliaryDocData.GetUserDataAuxilliaryDocData(docData) != null)
				return true;

			// Sanity check
			if (_AuxilliaryDocDataTable.TryGetValue(docData, out AuxilliaryDocData value))
			{
				AuxilliaryDocData.SetUserDataAuxilliaryDocData(docData, value);
				return true;
			}
		}

		return false;
	}



	public bool EnsureAuxilliaryDocData(IVsHierarchy hierarchy, string documentMoniker, object docData)
	{
		lock (_LockLocal)
		{
			if (AuxilliaryDocDataExists(docData))
				return false;


			// Accessing the stack and pop.
			string inflightMoniker = RdtManager.InflightMonikerStack;

			// Tracer.Trace(GetType(), "EnsureAuxilliaryDocData()", "inflightMoniker: {0}, documentMoniker: {1}.", inflightMoniker, documentMoniker);

			AuxilliaryDocData auxDocData = new AuxilliaryDocData(documentMoniker, inflightMoniker, docData);
			// hierarchy.GetSite(out Microsoft.VisualStudio.OLE.Interop.IServiceProvider ppSP);

			if (inflightMoniker != null && RdtManager.InflightMonikerCsbTable.TryGetValue(inflightMoniker, out object csaObject))
			{
				auxDocData.UserDataCsb = (System.Data.Common.DbConnectionStringBuilder)csaObject;
				RdtManager.InflightMonikerCsbTable[inflightMoniker] = null;
			}

			// No point looking because This will always be null for us

			// IBSqlEditorStrategyProvider sqlEditorStrategyProvider = null;
			// ISqlEditorStrategyProvider sqlEditorStrategyProvider = new ServiceProvider(ppSP).GetService(typeof(ISqlEditorStrategyProvider)) as ISqlEditorStrategyProvider;

			// We always use DefaultConnectionStrategy and use the csb passed via userdata that was
			// constructed from the SE node

			EnEditorCreationFlags creationFlags = EnEditorCreationFlags.None;

			if (auxDocData.UserDataCsb is Csb csa && csa.ContainsKey(SysConstants.C_KeyExCreationFlags))
			{
				creationFlags = (EnEditorCreationFlags)csa[SysConstants.C_KeyExCreationFlags];
				csa.Remove(SysConstants.C_KeyExCreationFlags);
			}


			auxDocData.StrategyFactory = new ConnectionStrategyFactory(auxDocData.UserDataCsb, creationFlags);
			_ = auxDocData.QryMgr;

			AuxilliaryDocDataTable.Add(docData, auxDocData);

			AuxilliaryDocData.SetUserDataAuxilliaryDocData(docData, auxDocData);

			// Set as the current for performance.
			_CurrentDocData = docData;
			_CurrentAuxilliaryDocData = auxDocData;

			// True if query must be executed.

			return (creationFlags & EnEditorCreationFlags.AutoExecute) > 0;
		}
	}



	public AuxilliaryDocData GetAuxilliaryDocData(object docData)
	{
		lock (_LockLocal)
		{
			if (docData == null || _AuxilliaryDocDataTable == null)
				return null;

			if (_CurrentDocData != null && object.ReferenceEquals(docData, _CurrentDocData))
				return _CurrentAuxilliaryDocData;

			AuxilliaryDocData value = AuxilliaryDocData.GetUserDataAuxilliaryDocData(docData);

			if (value == null)
			{
				// Sanity check
				if (_AuxilliaryDocDataTable.TryGetValue(docData, out value))
					AuxilliaryDocData.SetUserDataAuxilliaryDocData(docData, value);
			}

			if (value != null)
			{
				_CurrentDocData = docData;
				_CurrentAuxilliaryDocData = value;
			}

			return value;
		}
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
		if (markerGuid == VS.CLSID_TSqlEditorMessageErrorMarker)
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
			return _AuxilliaryDocDataTable != null && _AuxilliaryDocDataTable.Count > 0;
	}



	private static void InitializeTabbedEditorToolbarHandlerManager()
	{
		ToolbarCommandMapper toolbarMgr = AbstractTabbedEditorWindowPane.ToolbarManager;

		if (toolbarMgr == null)
			return;

		Guid clsid = CommandProperties.ClsidCommandSet;

		toolbarMgr.AddMapping(typeof(TabbedEditorWindowPane),
			new ToolbarCommandHandler<CommandDatabaseSelect>(clsid, (uint)EnCommandSet.CmbIdDatabaseSelect));
		toolbarMgr.AddMapping(typeof(TabbedEditorWindowPane),
			new ToolbarCommandHandler<CommandDatabaseList>(clsid, (uint)EnCommandSet.CmbIdDatabaseList));
		toolbarMgr.AddMapping(typeof(TabbedEditorWindowPane),
			new ToolbarCommandHandler<CommandExecuteQuery>(clsid, (uint)EnCommandSet.CmdIdExecuteQuery));
		toolbarMgr.AddMapping(typeof(TabbedEditorWindowPane),
			new ToolbarCommandHandler<CommandCancelQuery>(clsid, (uint)EnCommandSet.CmdIdCancelQuery));
		toolbarMgr.AddMapping(typeof(TabbedEditorWindowPane),
			new ToolbarCommandHandler<CommandConnect>(clsid, (uint)EnCommandSet.CmdIdConnect));
		toolbarMgr.AddMapping(typeof(TabbedEditorWindowPane),
			new ToolbarCommandHandler<CommandDisconnect>(clsid, (uint)EnCommandSet.CmdIdDisconnect));
		toolbarMgr.AddMapping(typeof(TabbedEditorWindowPane),
			new ToolbarCommandHandler<CommandDisconnectAllQueries>(clsid, (uint)EnCommandSet.CmdIdDisconnectAllQueries));
		toolbarMgr.AddMapping(typeof(TabbedEditorWindowPane),
			new ToolbarCommandHandler<CommandModifyConnection>(clsid, (uint)EnCommandSet.CmdIdModifyConnection));
		toolbarMgr.AddMapping(typeof(TabbedEditorWindowPane),
			new ToolbarCommandHandler<CommandShowEstimatedPlan>(clsid, (uint)EnCommandSet.CmdIdShowEstimatedPlan));
		toolbarMgr.AddMapping(typeof(TabbedEditorWindowPane),
			new ToolbarCommandHandler<CommandCloneQueryWindow>(clsid, (uint)EnCommandSet.CmdIdCloneQuery));
		toolbarMgr.AddMapping(typeof(TabbedEditorWindowPane),
			new ToolbarCommandHandler<CommandToggleExecutionPlan>(clsid, (uint)EnCommandSet.CmdIdToggleExecutionPlan));
		toolbarMgr.AddMapping(typeof(TabbedEditorWindowPane),
			new ToolbarCommandHandler<CommandToggleClientStatistics>(clsid, (uint)EnCommandSet.CmdIdToggleClientStatistics));
		toolbarMgr.AddMapping(typeof(TabbedEditorWindowPane),
			new ToolbarCommandHandler<CommandNewQuery>(clsid, (uint)EnCommandSet.CmdIdNewQuery));
		toolbarMgr.AddMapping(typeof(TabbedEditorWindowPane),
			new ToolbarCommandHandler<CommandTransactionCommit>(clsid, (uint)EnCommandSet.CmdIdTransactionCommit));
		toolbarMgr.AddMapping(typeof(TabbedEditorWindowPane),
			new ToolbarCommandHandler<CommandTransactionRollback>(clsid, (uint)EnCommandSet.CmdIdTransactionRollback));
		toolbarMgr.AddMapping(typeof(TabbedEditorWindowPane),
			new ToolbarCommandHandler<CommandToggleTTS>(clsid, (uint)EnCommandSet.CmdIdToggleTTS));
	}



	int OleServiceProvider.QueryService(ref Guid serviceGuid, ref Guid interfaceGuid, out IntPtr service)
	{
		if (interfaceGuid == typeof(IVsTextMarkerTypeProvider).GUID && serviceGuid == PackageData.CLSID_EditorMarkerService)
		{
			GetServicePointer(interfaceGuid, this, out service);
			return VSConstants.S_OK;
		}

		if (interfaceGuid == typeof(IVsFontAndColorDefaultsProvider).GUID && serviceGuid == PackageData.CLSID_FontAndColorService)
		{
			GetServicePointer(interfaceGuid, this, out service);
			return VSConstants.S_OK;
		}

		service = IntPtr.Zero;

		return VSConstants.E_NOINTERFACE;
	}



	private async Task RegisterOleCommandsAsync()
	{
		if (await GetServiceAsync(typeof(IMenuCommandService)) is not OleMenuCommandService oleMenuCommandSvc)
			throw Diag.ExceptionService(typeof(OleMenuCommandService));

		Guid clsid = CommandProperties.ClsidCommandSet;

		CommandID id = new CommandID(clsid, (int)EnCommandSet.CmdIdNewQuery);
		OleMenuCommand cmd = new(OnNewQuery, id);
		cmd.BeforeQueryStatus += OnBeforeQueryStatus;
		oleMenuCommandSvc.AddCommand(cmd);

		CommandID id2 = new CommandID(clsid, (int)EnCommandSet.CmdIdCycleToNextTab);
		OleMenuCommand cmd2 = new(OnCycleToNextEditorTab, id2);
		cmd2.BeforeQueryStatus += OnBeforeQueryStatus;
		oleMenuCommandSvc.AddCommand(cmd2);

		CommandID id3 = new CommandID(clsid, (int)EnCommandSet.CmdIdCycleToPrevious);
		OleMenuCommand cmd3 = new(OnCycleToPreviousEditorTab, id3);
		cmd3.BeforeQueryStatus += OnBeforeQueryStatus;
		oleMenuCommandSvc.AddCommand(cmd3);
	}



	public void RemoveAuxilliaryDocData(object docData)
	{
		lock (_LockLocal)
		{
			if (_AuxilliaryDocDataTable == null)
				return;

			if (_AuxilliaryDocDataTable.TryGetValue(docData, out AuxilliaryDocData auxDocData))
			{
				if (auxDocData.InflightMoniker != null)
					RdtManager.InflightMonikerCsbTable.Remove(auxDocData.InflightMoniker);

				if (_CurrentDocData != null && ReferenceEquals(docData, _CurrentDocData))
				{
					_CurrentDocData = null;
					_CurrentAuxilliaryDocData = null;
				}

				AuxilliaryDocData.SetUserDataAuxilliaryDocData(docData, null);

				_AuxilliaryDocDataTable.Remove(docData);
				auxDocData?.Dispose();
			}
		}
	}



	public DialogResult ShowExecutionSettingsDialogFrame(AuxilliaryDocData auxDocData,
		FormStartPosition startPosition)
	{
		// Tracer.Trace(GetType(), "ShowExecutionSettingsDialogFrame()");

		DialogResult result = DialogResult.Abort;

		QueryManager qryMgr = auxDocData.QryMgr;
		if (qryMgr == null)
			return DialogResult.Abort;

		using (LiveSettingsDialog dlg = new(qryMgr.LiveSettings))
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



	public bool TryGetTabbedEditorService(uint docCookie, bool activateIfOpen, out IBsWindowPaneServiceProvider tabbedEditorService)
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


		if (!__(hresult) || !pfOpen.AsBool() || ppWindowFrame == null)
		{
			Diag.Dug(new COMException($"Failed to find window frame for moniker {mkDocument} using document cookie {docCookie}."));
			tabbedEditorService = null;
			return false;
		}


		if (!__(ppWindowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out var pvar)))
		{
			Diag.Dug(new COMException($"Failed to get window frame DocView property for moniker {mkDocument} using document cookie {docCookie}."));
			tabbedEditorService = null;
			return false;
		}

		if (pvar is not IBsWindowPaneServiceProvider tabbedEditorPane)
		{
			Diag.Dug(new COMException($"Window frame DocView property is not of type IBsWindowPaneServiceProvider for moniker {mkDocument} using document cookie {docCookie}."));
			tabbedEditorService = null;
			return false;
		}


		tabbedEditorService = tabbedEditorPane;

		// Tracer.Trace(GetType(), "TryGetTabbedEditorService()", "DONE!!!");

		return true;
	}


	#endregion Methods and Implementations




	// =========================================================================================================
	#region Event handlers - EditorExtensionPackage
	// =========================================================================================================


	private void OnBeforeQueryStatus(object sender, EventArgs e)
	{
		if (sender is OleMenuCommand oleMenuCommand)
		{
			oleMenuCommand.Enabled = true;
			oleMenuCommand.Visible = true;
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



	private void OnCycleToNextEditorTab(object sender, EventArgs e)
	{
		// Tracer.Trace(GetType(), "OnCycleToNextEditorTab()");
		LastFocusedSqlEditor?.ActivateNextTab();
	}



	private void OnCycleToPreviousEditorTab(object sender, EventArgs e)
	{
		// Tracer.Trace(GetType(), "OnCycleToPreviousEditorTab()");
		LastFocusedSqlEditor?.ActivatePreviousTab();
	}



	private void OnNewQuery(object sender, EventArgs e)
	{
		// Tracer.Trace(GetType(), "OnNewQuery()");

		using (Microsoft.VisualStudio.Utilities.DpiAwareness.EnterDpiScope(Microsoft.VisualStudio.Utilities.DpiAwarenessContext.SystemAware))
		{
			DesignerExplorerServices.OpenNewMiscellaneousSqlFile(Resources.NewQueryBaseName, string.Empty);
			IBsTabbedEditorWindowPane lastFocusedSqlEditor = LastFocusedSqlEditor;
			if (lastFocusedSqlEditor != null)
			{
				new CommandNewQuery(lastFocusedSqlEditor).Exec((uint)OLECMDEXECOPT.OLECMDEXECOPT_DODEFAULT, IntPtr.Zero, IntPtr.Zero);
				GetAuxilliaryDocData(lastFocusedSqlEditor.DocData).IsVirtualWindow = true;
			}
		}
	}



	private void OnPowerBroadcast(IntPtr wParam, IntPtr lParam)
	{
		if ((int)wParam != 4 || _AuxilliaryDocDataTable == null)
		{
			return;
		}

		foreach (AuxilliaryDocData value in _AuxilliaryDocDataTable.Values)
		{
			QueryManager qryMgr = value.QryMgr;

			if (qryMgr != null && qryMgr.IsConnected)
				qryMgr.Strategy.CloseConnection();
		}
	}


	#endregion Event handlers


}

#endregion Class Declaration
