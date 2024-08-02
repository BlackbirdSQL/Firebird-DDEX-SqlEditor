using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using BlackbirdSql.Sys.Enums;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Design;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TaskStatusCenter;
using VSLangProj;



namespace BlackbirdSql.Sys.Interfaces;

[Guid(LibraryData.PackageControllerGuid)]


#if ASYNCRDTEVENTS_ENABLED
public interface IBsPackageController : IVsSolutionEvents3,
	IVsSelectionEvents, IVsRunningDocTableEvents3, IVsRunningDocTableEvents4, IVsRunningDocTableEvents7, IDisposable
#else
public interface IBsPackageController : IVsSolutionEvents3, IVsSelectionEvents, IVsRunningDocTableEvents3,
	IVsRunningDocTableEvents4, IDisposable
#endif
{
	// System Event Delegates
	delegate void InitializeDelegate(object sender);
	delegate void AssemblyObsoleteDelegate(object sender, AssemblyObsoleteEventArgs e);
	delegate void BuildDoneDelegate(vsBuildScope Scope, vsBuildAction Action);


	// Rdt Event Delegates

	delegate int AfterAttributeChangeDelegate(uint docCookie, uint grfAttribs);
	delegate int AfterAttributeChangeExDelegate(uint docCookie, uint grfAttribs, IVsHierarchy pHierOld,
		uint itemidOld, string pszMkDocumentOld, IVsHierarchy pHierNew, uint itemidNew, string pszMkDocumentNew);

	delegate int AfterDocumentWindowHideDelegate(uint docCookie, IVsWindowFrame pFrame);
	delegate int AfterLastDocumentUnlockDelegate(IVsHierarchy pHier, uint itemid, string pszMkDocument,
		int fClosedWithoutSaving);
	delegate int AfterSaveDelegate(uint docCookie);
	delegate IVsTask AfterSaveAsyncDelegate(uint cookie, uint flags);

	delegate int BeforeDocumentWindowShowDelegate(uint docCookie, int fFirstShow, IVsWindowFrame pFrame);
	delegate int BeforeLastDocumentUnlockDelegate(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining,
		uint dwEditLocksRemaining);
	delegate int BeforeSaveDelegate(uint docCookie);
	delegate IVsTask BeforeSaveAsyncDelegate(uint cookie, uint flags, IVsTask saveTask);

	// Solution Event Delegates
	delegate int AfterLoadProjectDelegate(Project project);
	delegate int AfterMergeSolutionDelegate(object pUnkReserved);
	delegate int AfterOpenProjectDelegate(Project project, int fAdded);
	delegate int AfterOpenSolutionDelegate(object pUnkReserved, int fNewSolution);
	delegate void LoadSolutionOptionsDelegate(Stream stream);
	delegate void SaveSolutionOptionsDelegate(Stream stream);
	delegate int AfterCloseSolutionDelegate(object pUnkReserved);
	delegate int BeforeCloseProjectDelegate(IVsHierarchy pHierarchy, int fRemoved);
	delegate int BeforeCloseSolutionDelegate(object pUnkReserved);
	delegate int QueryCloseProjectDelegate(IVsHierarchy hierarchy, int removing, ref int cancel);
	delegate int QueryCloseSolutionDelegate(object pUnkReserved, ref int pfCancel);

	// Selection Event Delegates
	delegate int CmdUIContextChangedDelegate(uint dwCmdUICookie, int fActive);
	delegate int ElementValueChangedDelegate(uint elementid, object varValueOld, object varValueNew);
	delegate int SelectionChangedDelegate(IVsHierarchy pHierOld, uint itemidOld, IVsMultiItemSelect pMISOld,
		ISelectionContainer pSCOld, IVsHierarchy pHierNew, uint itemidNew, IVsMultiItemSelect pMISNew,
		ISelectionContainer pSCNew);

	// Project Event Delegates
	delegate void DesignTimeOutputDeletedDelegate(string bstrOutputMoniker);
	delegate void DesignTimeOutputDirtyDelegate(string bstrOutputMoniker);

	delegate void ProjectInitializedDelegate(Project project);

	delegate void ProjectItemAddedDelegate(ProjectItem projectItem);
	delegate void ProjectItemRemovedDelegate(ProjectItem projectItem);
	delegate void ProjectItemRenamedDelegate(ProjectItem projectItem, string oldName);

	delegate void ReferenceAddedDelegate(Reference reference);
	delegate void ReferenceChangedDelegate(Reference reference);
	delegate void ReferenceRemovedDelegate(Reference reference);

	// Custom EventDelegates
	delegate int NewQueryRequestedDelegate(IVsDataViewHierarchy site, EnNodeSystemType nodeSystemType);



	// System Events
	event AssemblyObsoleteDelegate OnAssemblyObsoleteEvent;
	event BuildDoneDelegate OnBuildDoneEvent;

	// Solution events
	event AfterLoadProjectDelegate OnAfterLoadProjectEvent;
	event AfterOpenProjectDelegate OnAfterOpenProjectEvent;
	event AfterOpenSolutionDelegate OnAfterOpenSolutionEvent;

	event LoadSolutionOptionsDelegate OnLoadSolutionOptionsEvent;
	event SaveSolutionOptionsDelegate OnSaveSolutionOptionsEvent;

	event AfterCloseSolutionDelegate OnAfterCloseSolutionEvent;
	event AfterMergeSolutionDelegate OnAfterMergeSolutionEvent;
	event BeforeCloseProjectDelegate OnBeforeCloseProjectEvent;
	event BeforeCloseSolutionDelegate OnBeforeCloseSolutionEvent;
	event QueryCloseProjectDelegate OnQueryCloseProjectEvent;
	event QueryCloseSolutionDelegate OnQueryCloseSolutionEvent;

	// Rdt events
	event AfterAttributeChangeDelegate OnAfterAttributeChangeEvent;
	event AfterAttributeChangeExDelegate OnAfterAttributeChangeExEvent;
	event AfterDocumentWindowHideDelegate OnAfterDocumentWindowHideEvent;
	event AfterLastDocumentUnlockDelegate OnAfterLastDocumentUnlockEvent;
	event AfterSaveDelegate OnAfterSaveEvent;
	event AfterSaveAsyncDelegate OnAfterSaveAsyncEvent;
	event BeforeDocumentWindowShowDelegate OnBeforeDocumentWindowShowEvent;
	event BeforeLastDocumentUnlockDelegate OnBeforeLastDocumentUnlockEvent;
	event BeforeSaveDelegate OnBeforeSaveEvent;
	event BeforeSaveAsyncDelegate OnBeforeSaveAsyncEvent;

	// Selection Events
	event CmdUIContextChangedDelegate OnCmdUIContextChangedEvent;
	event ElementValueChangedDelegate OnElementValueChangedEvent;
	event SelectionChangedDelegate OnSelectionChangedEvent;

	// Project Events
	event ProjectInitializedDelegate OnProjectInitializedEvent;

	event DesignTimeOutputDeletedDelegate OnDesignTimeOutputDeletedEvent;
	event DesignTimeOutputDirtyDelegate OnDesignTimeOutputDirtyEvent;

	event ProjectItemAddedDelegate OnProjectItemAddedEvent;
	event ProjectItemRemovedDelegate OnProjectItemRemovedEvent;
	event ProjectItemRenamedDelegate OnProjectItemRenamedEvent;

	event ReferenceAddedDelegate OnReferenceAddedEvent;
	event ReferenceChangedDelegate OnReferenceChangedEvent;
	event ReferenceRemovedDelegate OnReferenceRemovedEvent;


	// Custom Events
	event NewQueryRequestedDelegate OnNewQueryRequestedEvent;


	IBsAsyncPackage PackageInstance { get; }

	DTE Dte { get; }
	DTE2 Dte2 { get; }

	string ProviderGuid { get; }

	bool SolutionClosing { get; }
	Solution SolutionObject { get; }
	Projects SolutionProjects { get; }


	bool SolutionValidating { get; }

	public bool IsToolboxInitialized { get; }


	bool IsCmdLineBuild { get; }


	IVsMonitorSelection SelectionMonitor { get; }

	IVsTaskStatusCenterService StatusCenterService { get; }


	IAsyncServiceContainer Services { get; }


	uint ToolboxCmdUICookie { get; }


	abstract bool UiAdviseUnsafeEvents();
	Task<bool> AdviseUnsafeEventsAsync();
	void UiRegisterProjectEventHandlers();
	bool EventRdtEnter(bool test = false, bool force = false);
	void EventRdtExit();
	string CreateConnectionUrl(string connectionString);
	string GetConnectionQualifiedName(string connectionString);
	TInterface EnsureService<TService, TInterface>() where TInterface : class;
	TInterface GetService<TService, TInterface>() where TInterface : class;
	Task<TInterface> GetServiceAsync<TService, TInterface>() where TInterface : class;
	string GetRegisterConnectionDatasetKey(IVsDataExplorerConnection root);
	void InvalidateRctManager();
	bool IsConnectionEquivalency(string connectionString1, string connectionString2);
	bool IsWeakConnectionEquivalency(string connectionString1, string connectionString2);
	Task<bool> RegisterProjectEventHandlersAsync();
	void ValidateSolution();


	void OnLoadSolutionOptions(Stream stream);
	int OnNewQueryRequested(IVsDataViewHierarchy site, EnNodeSystemType nodeSystemType);
	void OnSaveSolutionOptions(Stream stream);

}
