using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using EnvDTE;
using Microsoft.VisualStudio.Data.Services.SupportEntities;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using static BlackbirdSql.Core.Ctl.CommandProviders.CommandProperties;

namespace BlackbirdSql.Core.Ctl.Interfaces;

[Guid(SystemData.PackageControllerGuid)]
public interface IBPackageController : IVsSolutionEvents3, /* IVsSolutionEvents2, IVsSolutionEvents, */
	IVsSelectionEvents, IVsRunningDocTableEvents, IVsRunningDocTableEvents4, IDisposable
{

	// Rdt Event Delegates

	delegate int AfterDocumentWindowHideDelegate(uint docCookie, IVsWindowFrame pFrame);
	delegate int AfterSaveDelegate(uint docCookie);
	delegate int BeforeDocumentWindowShowDelegate(uint docCookie, int fFirstShow, IVsWindowFrame pFrame);
	delegate int BeforeLastDocumentUnlockDelegate(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining,
		uint dwEditLocksRemaining);

	// Solution Event Delegates
	delegate int AfterOpenProjectDelegate(IVsHierarchy pHierarchy, int fAdded);
	delegate int QueryCloseProjectDelegate(IVsHierarchy hierarchy, int removing, ref int cancel);
	delegate int QueryCloseSolutionDelegate(object pUnkReserved, ref int pfCancel);

	// Selection Event Delegates
	delegate int CmdUIContextChangedDelegate(uint dwCmdUICookie, int fActive);
	delegate int ElementValueChangedDelegate(uint elementid, object varValueOld, object varValueNew);
	delegate int SelectionChangedDelegate(IVsHierarchy pHierOld, uint itemidOld, IVsMultiItemSelect pMISOld,
		ISelectionContainer pSCOld, IVsHierarchy pHierNew, uint itemidNew, IVsMultiItemSelect pMISNew,
		ISelectionContainer pSCNew);

	// Custom EventDelegates
	delegate int NewQueryRequestedDelegate(IVsDataViewHierarchy site, EnNodeSystemType nodeSystemType);


	// Solution events
	event AfterOpenProjectDelegate OnAfterOpenProjectEvent;
	event QueryCloseProjectDelegate OnQueryCloseProjectEvent;
	event QueryCloseSolutionDelegate OnQueryCloseSolutionEvent;

	// Rdt events
	event AfterDocumentWindowHideDelegate OnAfterDocumentWindowHideEvent;
	event AfterSaveDelegate OnAfterSaveEvent;
	event BeforeDocumentWindowShowDelegate OnBeforeDocumentWindowShowEvent;
	event BeforeLastDocumentUnlockDelegate OnBeforeLastDocumentUnlockEvent;

	// Selection Events
	event CmdUIContextChangedDelegate OnCmdUIContextChangedEvent;
	event ElementValueChangedDelegate OnElementValueChangedEvent;
	event SelectionChangedDelegate OnSelectionChangedEvent;

	// Custom Events
	event NewQueryRequestedDelegate OnNewQueryRequestedEvent;

	string UserDataDirectory { get; }

	IBAsyncPackage DdexPackage { get; }

	IVsRunningDocumentTable DocTable { get; }

	DTE Dte { get; }

	IVsSolution DteSolution { get; }

	public bool IsToolboxInitialized { get; }


	bool IsCmdLineBuild { get; }


	IVsUIHierarchy MiscHierarchy { get; }

	IVsMonitorSelection SelectionMonitor { get; }

	void RegisterEventsManager(IBEventsManager manager);


	IAsyncServiceContainer Services { get; }

	Globals SolutionGlobals { get; }

	uint ToolboxCmdUICookie { get; }

	IBGlobalsAgent Uig { get; }



	object PackageLock { get; }




	void AdviseEvents();

	void DeregisterMiscHierarchy();

	void RegisterMiscHierarchy(IVsUIHierarchy hierarchy);

	int OnNewQueryRequested(IVsDataViewHierarchy site, EnNodeSystemType nodeSystemType);

}
