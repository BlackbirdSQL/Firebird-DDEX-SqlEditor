﻿using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using BlackbirdSql.Core.Model.Enums;
using EnvDTE;
using Microsoft.VisualStudio.Data.Services.SupportEntities;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TaskStatusCenter;


namespace BlackbirdSql.Core.Ctl.Interfaces;

[Guid(SystemData.PackageControllerGuid)]
public interface IBPackageController : IVsSolutionEvents3, // IVsSolutionEvents2, IVsSolutionEvents, */
	IVsSelectionEvents, IVsRunningDocTableEvents, IVsRunningDocTableEvents4, IDisposable
{

	// Rdt Event Delegates

	delegate int AfterAttributeChangeDelegate(uint docCookie, uint grfAttribs);
	delegate int AfterDocumentWindowHideDelegate(uint docCookie, IVsWindowFrame pFrame);
	delegate int AfterSaveDelegate(uint docCookie);
	delegate int BeforeDocumentWindowShowDelegate(uint docCookie, int fFirstShow, IVsWindowFrame pFrame);
	delegate int BeforeLastDocumentUnlockDelegate(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining,
		uint dwEditLocksRemaining);

	// Solution Event Delegates
	delegate int AfterOpenProjectDelegate(Project project, int fAdded);
	delegate void LoadSolutionOptionsDelegate(Stream stream);
	delegate void SaveSolutionOptionsDelegate(Stream stream);
	delegate int AfterCloseSolutionDelegate(object pUnkReserved);
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

	event LoadSolutionOptionsDelegate OnLoadSolutionOptionsEvent;
	event SaveSolutionOptionsDelegate OnSaveSolutionOptionsEvent;

	event AfterCloseSolutionDelegate OnAfterCloseSolutionEvent;
	event QueryCloseProjectDelegate OnQueryCloseProjectEvent;
	event QueryCloseSolutionDelegate OnQueryCloseSolutionEvent;

	// Rdt events
	event AfterAttributeChangeDelegate OnAfterAttributeChangeEvent;
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

	IBAsyncPackage DdexPackage { get; set; }

	IVsRunningDocumentTable DocTable { get; }

	DTE Dte { get; }

	object SolutionObject { get; }

	bool SolutionValidating {  get; }

	IVsSolution VsSolution { get; }

	public bool IsToolboxInitialized { get; }


	bool IsCmdLineBuild { get; }


	IVsUIHierarchy MiscHierarchy { get; }

	IVsMonitorSelection SelectionMonitor { get; }

	IVsTaskStatusCenterService StatusCenterService { get; }


	void RegisterEventsManager(IBEventsManager manager);


	IAsyncServiceContainer Services { get; }


	uint ToolboxCmdUICookie { get; }

	object LockGlobal { get; }




	abstract bool AdviseEvents();
	Task<bool> AdviseEventsAsync();

	void DeregisterMiscHierarchy();

	void EnsureMonitorSelection();

	TInterface GetService<TService, TInterface>() where TInterface : class;

	Task<TInterface> GetServiceAsync<TService, TInterface>() where TInterface : class;

	void RegisterMiscHierarchy(IVsUIHierarchy hierarchy);

	void ValidateSolution();

	void OnLoadSolutionOptions(Stream stream);


	int OnNewQueryRequested(IVsDataViewHierarchy site, EnNodeSystemType nodeSystemType);
	void OnSaveSolutionOptions(Stream stream);

}
