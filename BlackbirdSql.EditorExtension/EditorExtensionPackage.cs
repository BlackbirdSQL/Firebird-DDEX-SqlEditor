// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.VSIntegration.SqlEditorPackage

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BlackbirdSql.Core.Ctl.CommandProviders;
using BlackbirdSql.Core.Ctl.ComponentModel;
using BlackbirdSql.Core.Enums;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.EditorExtension.Controls.Config;
using BlackbirdSql.EditorExtension.Ctl;
using BlackbirdSql.EditorExtension.Ctl.Config;
using BlackbirdSql.EditorExtension.Properties;
using BlackbirdSql.LanguageExtension;
using BlackbirdSql.Shared.Controls;
using BlackbirdSql.Shared.Ctl;
using BlackbirdSql.Shared.Ctl.Commands;
using BlackbirdSql.Shared.Ctl.ComponentModel;
using BlackbirdSql.Shared.Enums;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Shared.Model;
using BlackbirdSql.Sys;
using BlackbirdSql.Sys.Interfaces;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Threading;

using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;



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


[VsProvideEditorFactory(typeof(EditorFactory), 311, false, DefaultName = PackageData.C_ServiceName,
	CommonPhysicalViewAttributes = (int)__VSPHYSICALVIEWATTRIBUTES.PVA_SupportsPreview,
	TrustLevel = __VSEDITORTRUSTLEVEL.ETL_AlwaysTrusted)]
[ProvideEditorExtension(typeof(EditorFactory), LanguageExtension.PackageData.C_Extension, 110,
	DefaultName = PackageData.C_ServiceName, NameResourceID = 311, RegisterFactory = true)]
// [ProvideEditorLogicalView(typeof(EditorFactoryWithoutEncoding), VSConstants.LOGVIEWID.Debugging_string)]
[ProvideEditorLogicalView(typeof(EditorFactory), VSConstants.LOGVIEWID.Code_string)]
[ProvideEditorLogicalView(typeof(EditorFactory), VSConstants.LOGVIEWID.Designer_string)]
[ProvideEditorLogicalView(typeof(EditorFactory), VSConstants.LOGVIEWID.TextView_string)]
[VsProvideFileExtensionMapping(typeof(EditorFactory), PackageData.C_ServiceName, 311, 100)]

[VsProvideEditorFactory(typeof(EditorFactoryEncoded), 317, false,
	DefaultName = $"{PackageData.C_ServiceName} with Encoding",
	CommonPhysicalViewAttributes = (int)__VSPHYSICALVIEWATTRIBUTES.PVA_SupportsPreview,
	TrustLevel = __VSEDITORTRUSTLEVEL.ETL_AlwaysTrusted)]
[ProvideEditorExtension(typeof(EditorFactoryEncoded), LanguageExtension.PackageData.C_Extension, 100,
	DefaultName = $"{PackageData.C_ServiceName} with Encoding", NameResourceID = 317, RegisterFactory = true)]
// [ProvideEditorLogicalView(typeof(EditorFactoryWithEncoding), VSConstants.LOGVIEWID.Debugging_string)]
[ProvideEditorLogicalView(typeof(EditorFactoryEncoded), VSConstants.LOGVIEWID.Code_string)]
[ProvideEditorLogicalView(typeof(EditorFactoryEncoded), VSConstants.LOGVIEWID.Designer_string)]
[ProvideEditorLogicalView(typeof(EditorFactoryEncoded), VSConstants.LOGVIEWID.TextView_string)]
[VsProvideFileExtensionMapping(typeof(EditorFactoryEncoded), $"{PackageData.C_ServiceName} with Encoding", 317, 96)]

[VsProvideEditorFactory(typeof(EditorFactoryResults), 312, false,
	DefaultName = "BlackbirdSql Results", CommonPhysicalViewAttributes = 0)]
// [ProvideEditorLogicalView(typeof(EditorFactoryResults), VSConstants.LOGVIEWID.Debugging_string)]
[ProvideEditorLogicalView(typeof(EditorFactoryResults), VSConstants.LOGVIEWID.Code_string)]
[ProvideEditorLogicalView(typeof(EditorFactoryResults), VSConstants.LOGVIEWID.Designer_string)]
[ProvideEditorLogicalView(typeof(EditorFactoryResults), VSConstants.LOGVIEWID.TextView_string)]


#endregion Class Attributes



// =========================================================================================================
#region							EditorExtensionPackage Class Declaration
// =========================================================================================================
public abstract class EditorExtensionPackage : LanguageExtensionPackage, IBsEditorPackage,
	IVsTextMarkerTypeProvider, IVsFontAndColorDefaultsProvider, IVsBroadcastMessageEvents,
	IOleServiceProvider
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - EditorExtensionPackage
	// ---------------------------------------------------------------------------------


	protected EditorExtensionPackage() : base()
	{
		_EventsManager = EditorEventsManager.CreateInstance(_ApcInstance);
	}


	/// <summary>
	/// EditorExtensionPackage static .ctor
	/// </summary>
	static EditorExtensionPackage()
	{
		RegisterAssemblies();
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


	private Dictionary<object, AuxilliaryDocData> _AuxDocDataTable = null;

	private object _CurrentDocData = null;
	private AuxilliaryDocData _CurrentAuxilliaryDocData = null;

	// private static readonly string _TName = typeof(EditorExtensionPackage).Name;

	private readonly EditorEventsManager _EventsManager;
	private uint _BroadcastMessageEventsCookie;
	private EditorFactory _SqlEditorFactory;
	private EditorFactoryEncoded _SqlEditorFactoryWithEncoding;
	private EditorFactoryResults _SqlResultsEditorFactory;
	private uint _MarkerServiceCookie;
	private uint _FontAndColorServiceCookie;


	#endregion Constants & Fields





	// =========================================================================================================
	#region Property accessors - EditorExtensionPackage
	// =========================================================================================================

	/*
	[Import]
	private JoinableTaskContext? JoinableTaskContext { get; set; }
	*/

	public Dictionary<object, AuxilliaryDocData> AuxDocDataTable => _AuxDocDataTable ??= [];

	public bool EnableSpatialResultsTab { get; set; }

	public new IBsEventsManager EventsManager => _EventsManager;

	public bool HasAuxillaryDocData => (_AuxDocDataTable?.Count ?? 0) > 0;

	public IBsTabbedEditorPane CurrentTabbedEditor { get; set; }

	public object LockLocal => _LockLocal;


	#endregion Property accessors





	// =========================================================================================================
	#region Methods Implementations - EditorExtensionPackage
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Final asynchronous initialization tasks for the package that must occur after
	/// all descendents and ancestors have completed their InitializeAsync() tasks.
	/// It is the final descendent package class's responsibility to initiate the call
	/// to FinalizeAsync.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override async Task FinalizeAsync(CancellationToken cancelToken, IProgress<ServiceProgressData> progress)
	{
		Diag.ThrowIfNotOnUIThread();

		if (cancelToken.Cancelled() || ApcManager.IdeShutdownState)
			return;


		ProgressAsync(progress, "Finalizing Editor...").Forget();


		ProgressAsync(progress, "Finalizing Editor. Registering Editor factories...").Forget();

		_SqlEditorFactory = new EditorFactory();
		_SqlEditorFactoryWithEncoding = new EditorFactoryEncoded();
		_SqlResultsEditorFactory = new EditorFactoryResults();

		RegisterEditorFactory(_SqlEditorFactory);
		RegisterEditorFactory(_SqlEditorFactoryWithEncoding);
		RegisterEditorFactory(_SqlResultsEditorFactory);

		ProgressAsync(progress, "Finalizing Editor. Registering Editor factories... Done.").Forget();


		ProgressAsync(progress, "Finalizing Editor. Advising Broadcast messages....").Forget();

		___((GetGlobalService(typeof(SVsShell)) as IVsShell).AdviseBroadcastMessages(this, out _BroadcastMessageEventsCookie));

		ProgressAsync(progress, "Finalizing Editor. Advising Broadcast messages... Done.").Forget();

		await base.FinalizeAsync(cancelToken, progress);


		ProgressAsync(progress, "Finalizing Editor. Registering OLE commands...").Forget();

		// This just will not fire. There is somethig in the extension's package hierarchy that
		// is causing this to fail, so handling SplitNext and SplitPrev in TabbedEditor.
		// await RegisterOleCommandsAsync();

		ProgressAsync(progress, "Finalizing Editor. Registering OLE commands... Done.").Forget();


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
	public override async Task<TInterface> GetLocalServiceInstanceAsync<TService, TInterface>(CancellationToken token)
		 where TInterface : class
	{
		Type serviceType = typeof(TService);

		if (serviceType == typeof(IBsDesignerExplorerServices))
			return new DesignerExplorerServices() as TInterface;

		/*
		else if (serviceType == typeof(IBDesignerOnlineServices))
			return new DesignerOnlineServices() as TInterface;
		*/

		return await base.GetLocalServiceInstanceAsync<TService, TInterface>(token);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Asynchronous initialization of the package. The class must register services it
	/// requires using the ServicesCreatorCallback method.
	/// </summary>
	protected override async Task InitializeAsync(CancellationToken cancelToken, IProgress<ServiceProgressData> progress)
	{
		ProgressAsync(progress, "InitialIzing Editor ...").Forget();


		await base.InitializeAsync(cancelToken, progress);

		ProgressAsync(progress, "Initializing Editor. Proffering services...").Forget();

		if (await GetServiceAsync(typeof(IProfferService)) is not IProfferService profferSvc)
			throw Diag.ExceptionService(typeof(IProfferService));

		Guid rguidMarkerService = PackageData.CLSID_EditorMarkerService;
		___(profferSvc.ProfferService(ref rguidMarkerService, this, out _MarkerServiceCookie));

		Guid rguidService = PackageData.CLSID_FontAndColorService;
		___(profferSvc.ProfferService(ref rguidService, this, out _FontAndColorServiceCookie));

		ProgressAsync(progress, "Initializing Editor. Proffering services... Done.").Forget();

		ProgressAsync(progress, "Initializing Editor. Registering Designer Explorer services...").Forget();

		AddService(typeof(IBsDesignerExplorerServices), ServicesCreatorCallbackAsync, promote: false);
		// AddService(typeof(IBDesignerOnlineServices), ServicesCreatorCallbackAsync, promote: false);
		// AddService(typeof(ISqlEditorStrategyProvider), ServicesCreatorCallbackAsync, promote: false);

		ProgressAsync(progress, "Initializing Editor. Registering Designer Explorer services... Done.").Forget();

		ProgressAsync(progress, "Initializing Editor. Initializing Tabbed Toolbar manager...").Forget();

		InitializeTabbedEditorToolbarHandlerManager();

		ProgressAsync(progress, "Initializing  Editor. Initializing Tabbed Toolbar manager... Done.").Forget();

		ProgressAsync(progress, "Initializing Editor ... Done.").Forget();

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

		if (serviceType == typeof(IBsDesignerExplorerServices))
			return await GetLocalServiceInstanceAsync<IBsDesignerExplorerServices, IBsDesignerExplorerServices>(token);

		/*
		else if (serviceType == typeof(IBsDesignerOnlineServices))
			return await GetLocalServiceInstanceAsync<IBsDesignerOnlineServices, IBsDesignerOnlineServices>(token);
		*/

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
			if (docData == null || _AuxDocDataTable == null)
				return false;

			if (_CurrentDocData != null && object.ReferenceEquals(docData, _CurrentDocData))
				return true;

			if (AuxilliaryDocData.GetUserDataAuxilliaryDocData(docData) != null)
				return true;

			// Sanity check
			if (_AuxDocDataTable.TryGetValue(docData, out AuxilliaryDocData value))
			{
				AuxilliaryDocData.SetUserDataAuxilliaryDocData(docData, value);
				return true;
			}
		}

		return false;
	}



	public bool EnsureAuxilliaryDocData(IVsHierarchy hierarchy, string documentMoniker, object docData,
		bool isClone, out AuxilliaryDocData auxDocData)
	{
		// Tracer.Trace(GetType(), "EnsureAuxilliaryDocData()",
		//	"AuxDocData does not exist. Clone? {0}, documentMoniker: {1}.", isClone, documentMoniker);

		lock (_LockLocal)
		{
			auxDocData = GetAuxilliaryDocData(docData);

			if (auxDocData != null)
			{
				if (isClone)
					auxDocData.CloneAdd();
				return false;
			}


			// Accessing the stack and pop.
			string inflightMoniker = RdtManager.PopInflightMonikerStack;


			IBsModelCsb csa = null;

			if (inflightMoniker != null && RdtManager.InflightMonikerCsbTable.TryGetValue(inflightMoniker, out IBsCsb csb))
			{
				csa = csb as IBsModelCsb;
				RdtManager.InflightMonikerCsbTable[inflightMoniker] = null;
			}


			uint cookie = 0;

			if (inflightMoniker == null && !string.IsNullOrEmpty(documentMoniker))
			{
				try
				{
					cookie = RdtManager.GetRdtCookie(documentMoniker);
				}
				catch { }
			}


			//Tracer.Trace(GetType(), "EnsureAuxilliaryDocData()",
			//	"fileCookie: {0}, monikerCookie: {1}, filePath: {2}, inflightMoniker: {3}, documentMoniker: {4}.",
			//	fileCookie, cookie, filePath, inflightMoniker, documentMoniker);



			auxDocData ??= new AuxilliaryDocData(this, cookie, documentMoniker, inflightMoniker, docData);


			// No point looking because This will always be null for us

			// IBSqlEditorStrategyProvider sqlEditorStrategyProvider = null;
			// ISqlEditorStrategyProvider sqlEditorStrategyProvider = new ServiceProvider(ppSP).GetService(typeof(ISqlEditorStrategyProvider)) as ISqlEditorStrategyProvider;

			// We always use DefaultConnectionStrategy and use the csb passed via userdata that was
			// constructed from the SE node

			EnCreationFlags creationFlags = EnCreationFlags.None;

			if (csa?.ContainsKey(SharedConstants.C_KeyExCreationFlags) ?? false)
			{
				creationFlags = csa.CreationFlags;
				csa.Remove(SharedConstants.C_KeyExCreationFlags);
			}



			bool createConnection = (creationFlags & EnCreationFlags.CreateConnection) > 0;

			auxDocData.CreateQueryManager(createConnection ? csa : null, cookie);


			AuxDocDataTable.Add(docData, auxDocData);

			AuxilliaryDocData.SetUserDataAuxilliaryDocData(docData, auxDocData);

			// Set as the current for performance.
			_CurrentDocData = docData;
			_CurrentAuxilliaryDocData = auxDocData;

			// True if query must be executed.

			return (creationFlags & EnCreationFlags.AutoExecute) > 0;
		}
	}



	public AuxilliaryDocData GetAuxilliaryDocData(object docData)
	{
		lock (_LockLocal)
		{
			if (docData == null || _AuxDocDataTable == null)
				return null;

			if (_CurrentDocData != null && object.ReferenceEquals(docData, _CurrentDocData))
				return _CurrentAuxilliaryDocData;

			AuxilliaryDocData value = AuxilliaryDocData.GetUserDataAuxilliaryDocData(docData);

			if (value == null)
			{
				// Sanity check
				if (_AuxDocDataTable.TryGetValue(docData, out value))
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



	private static void InitializeTabbedEditorToolbarHandlerManager()
	{
		CommandMapper cmdMapper = AbstractTabbedEditorPane.CmdMapper;

		if (cmdMapper == null)
			return;

		Guid clsid = CommandProperties.ClsidCommandSet;

		cmdMapper.AddMapping(typeof(TabbedEditorPane),
			new CommandHandler<CommandDatabaseSelect>(clsid, (uint)EnCommandSet.CmbIdDatabaseSelect));
		cmdMapper.AddMapping(typeof(TabbedEditorPane),
			new CommandHandler<CommandDatabaseList>(clsid, (uint)EnCommandSet.CmbIdDatabaseList));
		cmdMapper.AddMapping(typeof(TabbedEditorPane),
			new CommandHandler<CommandExecuteQuery>(clsid, (uint)EnCommandSet.CmdIdExecuteQuery));
		cmdMapper.AddMapping(typeof(TabbedEditorPane),
			new CommandHandler<CommandCancelQuery>(clsid, (uint)EnCommandSet.CmdIdCancelQuery));
		cmdMapper.AddMapping(typeof(TabbedEditorPane),
			new CommandHandler<CommandConnect>(clsid, (uint)EnCommandSet.CmdIdConnect));
		cmdMapper.AddMapping(typeof(TabbedEditorPane),
			new CommandHandler<CommandDisconnect>(clsid, (uint)EnCommandSet.CmdIdDisconnect));
		cmdMapper.AddMapping(typeof(TabbedEditorPane),
			new CommandHandler<CommandDisconnectAllQueries>(clsid, (uint)EnCommandSet.CmdIdDisconnectAllQueries));
		cmdMapper.AddMapping(typeof(TabbedEditorPane),
			new CommandHandler<CommandModifyConnection>(clsid, (uint)EnCommandSet.CmdIdModifyConnection));
		cmdMapper.AddMapping(typeof(TabbedEditorPane),
			new CommandHandler<CommandShowEstimatedPlan>(clsid, (uint)EnCommandSet.CmdIdShowEstimatedPlan));
		cmdMapper.AddMapping(typeof(TabbedEditorPane),
			new CommandHandler<CommandCloneQueryWindow>(clsid, (uint)EnCommandSet.CmdIdCloneQuery));
		cmdMapper.AddMapping(typeof(TabbedEditorPane),
			new CommandHandler<CommandToggleExecutionPlan>(clsid, (uint)EnCommandSet.CmdIdToggleExecutionPlan));
		cmdMapper.AddMapping(typeof(TabbedEditorPane),
			new CommandHandler<CommandToggleClientStatistics>(clsid, (uint)EnCommandSet.CmdIdToggleClientStatistics));
		cmdMapper.AddMapping(typeof(TabbedEditorPane),
			new CommandHandler<CommandNewQuery>(clsid, (uint)EnCommandSet.CmdIdNewQuery));
		cmdMapper.AddMapping(typeof(TabbedEditorPane),
			new CommandHandler<CommandTransactionCommit>(clsid, (uint)EnCommandSet.CmdIdTransactionCommit));
		cmdMapper.AddMapping(typeof(TabbedEditorPane),
			new CommandHandler<CommandTransactionRollback>(clsid, (uint)EnCommandSet.CmdIdTransactionRollback));
		cmdMapper.AddMapping(typeof(TabbedEditorPane),
			new CommandHandler<CommandToggleTTS>(clsid, (uint)EnCommandSet.CmdIdToggleTTS));
	}



	int IOleServiceProvider.QueryService(ref Guid serviceGuid, ref Guid interfaceGuid, out IntPtr service)
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

		object serviceObject = ((Package)this).QueryService(serviceGuid);

		if (serviceObject == null)
		{
			service = IntPtr.Zero;
			return VSConstants.E_NOINTERFACE;
		}

		GetServicePointer(interfaceGuid, serviceObject, out service);

		return VSConstants.S_OK;

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Adds this assembly to CurrentDomain.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private static void RegisterAssemblies()
	{
		AppDomain.CurrentDomain.AssemblyResolve += (_, args) =>
		{
			if (args.Name == typeof(EditorExtensionPackage).Assembly.FullName)
				return typeof(EditorExtensionPackage).Assembly;

			if (args.Name == typeof(TabbedEditorPane).Assembly.FullName)
				return typeof(TabbedEditorPane).Assembly;

			return null;
		};
	}



	public void RemoveAuxilliaryDocData(object docData)
	{

		lock (_LockLocal)
		{
			if (_AuxDocDataTable == null)
				return;

			if (_AuxDocDataTable.TryGetValue(docData, out AuxilliaryDocData auxDocData))
			{
				if (auxDocData.IsVirtualWindow)
					auxDocData.IsVirtualWindow = false;

				if (_CurrentDocData != null && ReferenceEquals(docData, _CurrentDocData))
				{
					_CurrentDocData = null;
					_CurrentAuxilliaryDocData = null;
				}

				AuxilliaryDocData.SetUserDataAuxilliaryDocData(docData, null);

				_AuxDocDataTable.Remove(docData);
				auxDocData?.Dispose();
			}
		}
	}



	public DialogResult ShowExecutionSettingsDialog(AuxilliaryDocData auxDocData,
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


	#endregion Methods and Implementations




	// =========================================================================================================
	#region Event handlers - EditorExtensionPackage
	// =========================================================================================================


	/// <summary>
	/// Deprecated.
	/// </summary>
	protected void OnBeforeQueryStatus(object sender, EventArgs e)
	{
		// Diag.DebugTrace("OnBeforeQueryStatus()");

		if (sender is OleMenuCommand oleMenuCommand)
		{
			oleMenuCommand.Enabled = true;
			oleMenuCommand.Visible = true;
		}
	}



	int IVsBroadcastMessageEvents.OnBroadcastMessage(uint message, IntPtr wParam, IntPtr lParam)
	{
		if (message == Native.WM_POWERBROADCAST)
			OnPowerBroadcast(wParam, lParam);

		return VSConstants.S_OK;
	}



	/// <summary>
	/// Deprecated.
	/// </summary>
	protected void OnCycleToNextEditorTab(object sender, EventArgs e)
	{
		// Diag.DebugTrace("OnCycleToNextEditorTab()");

		CurrentTabbedEditor?.ActivateNextTab();
	}



	/// <summary>
	/// Deprecated.
	/// </summary>
	protected void OnCycleToPreviousEditorTab(object sender, EventArgs e)
	{
		// Diag.DebugTrace("OnCycleToPreviousEditorTab()");

		CurrentTabbedEditor?.ActivatePreviousTab();
	}



	/// <summary>
	/// Deprecated.
	/// </summary>
	protected void OnNewQuery(object sender, EventArgs e)
	{
		// Diag.DebugTrace("OnNewQuery()");

		using (Microsoft.VisualStudio.Utilities.DpiAwareness.EnterDpiScope(Microsoft.VisualStudio.Utilities.DpiAwarenessContext.SystemAware))
		{
			DesignerExplorerServices.OpenNewMiscellaneousSqlFile(Resources.NewQueryBaseName, "");
			IBsTabbedEditorPane tabbedEditor = CurrentTabbedEditor;

			if (tabbedEditor != null)
				new CommandNewQuery(tabbedEditor).Exec((uint)OLECMDEXECOPT.OLECMDEXECOPT_DODEFAULT, IntPtr.Zero, IntPtr.Zero);
		}
	}



	private void OnPowerBroadcast(IntPtr wParam, IntPtr lParam)
	{
		// Tracer.Trace(GetType(), "OnPowerBroadcast()");

		if ((int)wParam != Native.PBT_APMSUSPEND || _AuxDocDataTable == null
			|| _AuxDocDataTable.Count == 0)
		{
			return;
		}

		foreach (AuxilliaryDocData value in _AuxDocDataTable.Values)
		{
			value?.QryMgr?.Strategy?.CloseConnection();
		}
	}


	#endregion Event handlers


}

#endregion Class Declaration
