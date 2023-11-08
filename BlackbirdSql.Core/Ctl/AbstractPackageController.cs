// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using BlackbirdSql.Core.Ctl.Interfaces;
using EnvDTE;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Data.Services.SupportEntities;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TaskStatusCenter;
using static BlackbirdSql.Core.Ctl.CommandProviders.CommandProperties;


namespace BlackbirdSql.Core.Ctl;

[SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread",
	Justification = "Class is UIThread compliant.")]

// =========================================================================================================
//										AbstractPackageController Class
//
/// <summary>
/// Manages package events and settings. This is the PackageController base class.
/// </summary>
/// <remarks>
/// Also updates the app.config for DbProvider and EntityFramework and updates existing .edmx models that
/// are using a legacy provider.
/// Also ensures we never do validations of a solution and project app.config and .edmx models twice.
/// Aslo performs cleanups of any sql editor documents that may be left dangling on solution close.
/// </remarks>
// =========================================================================================================
internal abstract class AbstractPackageController : IBPackageController
{

	// ---------------------------------------------------------------------------------
	#region Variables
	// ---------------------------------------------------------------------------------

	protected static bool _EventsAdvised = false;

	protected static IBPackageController _Instance = null;
	protected static List<IBEventsManager> _EventsManagers = null;

	// A package instance global lock
	private readonly object _LockGlobal = new object();

	private static IVsUIHierarchy _MiscHierarchy = null;


	private readonly IBAsyncPackage _DdexPackage;

	protected IVsMonitorSelection _MonitorSelection = null;
	private IVsTaskStatusCenterService _StatusCenterService = null;

	protected uint _HSolutionEvents = uint.MaxValue;
	protected uint _HDocTableEvents = uint.MaxValue;
	protected uint _HSelectionEvents = uint.MaxValue;

	protected uint _ToolboxCmdUICookie;

	protected IBGlobalsAgent _Uig;

	private string _UserDataDirectory = null;



	private IBPackageController.AfterDocumentWindowHideDelegate _OnAfterDocumentWindowHideEvent;
	private IBPackageController.AfterSaveDelegate _OnAfterSaveEvent;
	private IBPackageController.BeforeDocumentWindowShowDelegate _OnBeforeDocumentWindowShowEvent;
	private IBPackageController.BeforeLastDocumentUnlockDelegate _OnBeforeLastDocumentUnlockEvent;

	private IBPackageController.AfterOpenProjectDelegate _OnAfterOpenProjectEvent;
	private IBPackageController.QueryCloseProjectDelegate _OnQueryCloseProjectEvent;
	private IBPackageController.QueryCloseSolutionDelegate _OnQueryCloseSolutionEvent;

	// Selection Events Delegates
	private IBPackageController.SelectionChangedDelegate _OnSelectionChangedEvent;
	private IBPackageController.ElementValueChangedDelegate _OnElementValueChangedEvent;
	private IBPackageController.CmdUIContextChangedDelegate _OnCmdUIContextChangedEvent;

	// Custom events Delegates
	private IBPackageController.NewQueryRequestedDelegate _OnNewQueryRequestedEvent;


	#endregion Variables





	// =========================================================================================================
	#region Property Accessors - AbstractPackageController
	// =========================================================================================================


	public string UserDataDirectory => _UserDataDirectory ??=
		Environment.GetFolderPath(Environment.SpecialFolder.Personal);



	/// <summary>
	/// Accessor to the <see cref="IVsRunningDocTableEvents.OnAfterDocumentWindowHide"/> event.
	/// </summary>
	event IBPackageController.AfterDocumentWindowHideDelegate IBPackageController.OnAfterDocumentWindowHideEvent
	{
		add { _OnAfterDocumentWindowHideEvent += value; }
		remove { _OnAfterDocumentWindowHideEvent -= value; }
	}


	/// <summary>
	/// Accessor to the <see cref="IVsSolutionEvents.OnAfterOpenProject"/> event.
	/// </summary>
	event IBPackageController.AfterOpenProjectDelegate IBPackageController.OnAfterOpenProjectEvent
	{
		add { _OnAfterOpenProjectEvent += value; }
		remove { _OnAfterOpenProjectEvent -= value; }
	}


	/// <summary>
	/// Accessor to the <see cref="IVsRunningDocTableEvents.OnAfterSave"/> event.
	/// </summary>
	event IBPackageController.AfterSaveDelegate IBPackageController.OnAfterSaveEvent
	{
		add { _OnAfterSaveEvent += value; }
		remove { _OnAfterSaveEvent -= value; }
	}


	/// <summary>
	/// Accessor to the <see cref="IVsRunningDocTableEvents.OnBeforeDocumentWindowShow"/> event.
	/// </summary>
	event IBPackageController.BeforeDocumentWindowShowDelegate IBPackageController.OnBeforeDocumentWindowShowEvent
	{
		add { _OnBeforeDocumentWindowShowEvent += value; }
		remove { _OnBeforeDocumentWindowShowEvent -= value; }
	}


	/// <summary>
	/// Accessor to the <see cref="IVsRunningDocTableEvents.OnBeforeLastDocumentUnlock"/> event.
	/// </summary>
	event IBPackageController.BeforeLastDocumentUnlockDelegate IBPackageController.OnBeforeLastDocumentUnlockEvent
	{
		add { _OnBeforeLastDocumentUnlockEvent += value; }
		remove { _OnBeforeLastDocumentUnlockEvent -= value; }
	}


	/// <summary>
	/// Accessor to the <see cref="IVsSelectionEvents.OnCmdUIContextChanged"/> event.
	/// </summary>
	event IBPackageController.CmdUIContextChangedDelegate IBPackageController.OnCmdUIContextChangedEvent
	{
		add { EnsureMonitorSelection(); _OnCmdUIContextChangedEvent += value; }
		remove { _OnCmdUIContextChangedEvent -= value; }
	}


	/// <summary>
	/// Accessor to the <see cref="IVsSelectionEvents.OnElementValueChangedEvent"/> event.
	/// </summary>
	event IBPackageController.ElementValueChangedDelegate IBPackageController.OnElementValueChangedEvent
	{
		add { EnsureMonitorSelection(); _OnElementValueChangedEvent += value; }
		remove { _OnElementValueChangedEvent -= value; }
	}


	/// <summary>
	/// Accessor to the <see cref="IVsSolutionEvents.OnQueryCloseProject"/> event.
	/// </summary>
	event IBPackageController.QueryCloseProjectDelegate IBPackageController.OnQueryCloseProjectEvent
	{
		add { _OnQueryCloseProjectEvent += value; }
		remove { _OnQueryCloseProjectEvent -= value; }
	}


	/// <summary>
	/// Accessor to the <see cref="IVsSolutionEvents.OnQueryCloseSolutionEvent"/> event.
	/// </summary>
	event IBPackageController.QueryCloseSolutionDelegate IBPackageController.OnQueryCloseSolutionEvent
	{
		add { _OnQueryCloseSolutionEvent += value; }
		remove { _OnQueryCloseSolutionEvent -= value; }
	}


	/// <summary>
	/// Accessor to the <see cref="IVsSelectionEvents.OnSelectionChangedEvent"/> event.
	/// </summary>
	event IBPackageController.SelectionChangedDelegate IBPackageController.OnSelectionChangedEvent
	{
		add { EnsureMonitorSelection(); _OnSelectionChangedEvent += value; }
		remove { _OnSelectionChangedEvent -= value; }
	}


	/// <summary>
	/// Accessor to the <see cref="IVsSelectionEvents.OnSelectionChangedEvent"/> event.
	/// </summary>
	event IBPackageController.NewQueryRequestedDelegate IBPackageController.OnNewQueryRequestedEvent
	{
		add { _OnNewQueryRequestedEvent += value; }
		remove { _OnNewQueryRequestedEvent -= value; }
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Accessor to the singleton <see cref="GlobalsAgent"/> instance
	/// </summary>
	// ---------------------------------------------------------------------------------
	public abstract IBGlobalsAgent Uig { get; }


	public IBAsyncPackage DdexPackage => _DdexPackage;


	public IVsRunningDocumentTable DocTable => _DdexPackage.DocTable;


	public DTE Dte => _DdexPackage.Dte;


	public IVsSolution DteSolution => _DdexPackage.DteSolution;

	public bool IsCmdLineBuild
	{
		get
		{
			if (!ThreadHelper.CheckAccess())
			{
				COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
				Diag.Dug(exc);
				throw exc;
			}

			try
			{
				if (Package.GetGlobalService(typeof(SVsShell)) is IVsShell vsShell)
				{
					vsShell.GetProperty((int)__VSSPROPID.VSSPROPID_IsInCommandLineMode, out var pvar);
					if (pvar is bool bvar)
						return bvar;
				}
			}
			catch (COMException)
			{
				return false;
			}

			return false;
		}
	}


	public bool IsToolboxInitialized
	{
		get
		{
			if (SelectionMonitor != null)
			{
				if (!ThreadHelper.CheckAccess())
				{
					COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
					Diag.Dug(exc);
					throw exc;
				}

				Native.ThrowOnFailure(SelectionMonitor.IsCmdUIContextActive(ToolboxCmdUICookie, out int pfActive));

				return pfActive != 0;
			}
			return true;
		}
	}



	public IVsUIHierarchy MiscHierarchy => _MiscHierarchy;

	public IVsMonitorSelection SelectionMonitor
	{
		get
		{
			if (_MonitorSelection == null)
				EnsureMonitorSelection();
			return _MonitorSelection;
		}
	}


	public IAsyncServiceContainer Services => (IAsyncServiceContainer)_DdexPackage;


	/// <summary>
	/// Struggling to resolve deadlocking on GetService[Async] calls forLinkageParserl, so we're going
	/// to maintain instances of certain services.
	/// </summary>
	/// <remarks>
	/// Deadlock debug notes:
	/// 1. Caveat: Both GetService() and GetServiceAsynce seem to require the UIThread at some point.
	/// 2. The LinkageParser can switch between the UIThread and pool multiple times during the lifetime of
	/// a task. By default building the linkage is async, but if a Linkage result set/schema is required
	/// we complete the build process on the UIThread.
	/// 3. A launcher task using Run or StartNew can be in a prelaunch phase where it cannot be notified of
	/// it's cancellation.
	/// The reproduceable: Suppose a ServerExplorer tree is expanded on it's Sequences node and the user
	/// refreshes. The entire visible tree structure is refreshed. Any existing Linkage tables are disposed
	/// of and an async rebuild is launched.
	/// Once ServerExplorer reaches the expanded Sequences node, it will request the Sequences, which are held
	/// in a linkage table, which is currently being built async.
	/// So we notify the async task of cancellation and wait. The async task performs a cleanup and notifies
	/// the sync task that it is handing over the linkage process.
	/// This all sounds very beautiful and easy enough, but there are some major caveats...
	/// 1. The async task launcher task (StartNew etc) may be in prelaunch, a phase in which it is inaccessible.
	/// The timeslice on this phase is substantial, so during for example a refresh, it's a given that a sync
	/// task will attempt to cancel an inacessible launcher, which is busy doing some of it's "launch stuff"
	/// on the very UIThread the sync task has now blocked.
	/// 2. A sync or async task has called GetService[Async], and at this juncture another task on the UIThread
	/// may have sent out a cancellation request. GetService will deadlock behind it on the UI.
	/// GetService[Async] also uses a substantial timeslice relatively speaking, so this deadlock will
	/// happen more often than not.
	/// 3. There has been a call to the Tracer, Diag class, TaskHandler or output window. GetService is gonna
	/// happen somewhere in that logic, and it's a deadlock.
	/// Solution: 1 has been resolved some time back, but with 2 and 3 we have no access whatsoever.
	/// So the easiest solution is to store those service instances once they've been created, eliminating
	/// the need for GetService during processing/linkage/building.
	/// </remarks>

	public IVsTaskStatusCenterService StatusCenterService => _StatusCenterService ??=
		ThreadHelper.JoinableTaskFactory.Run(new Func<Task<IVsTaskStatusCenterService>>(GetStatusCenterServiceAsync));
		


	public uint ToolboxCmdUICookie
	{
		get
		{
			if (_ToolboxCmdUICookie == 0 && SelectionMonitor != null)
			{
				if (!ThreadHelper.CheckAccess())
				{
					COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
					Diag.Dug(exc);
					throw exc;
				}

				Guid rguidCmdUI = new Guid(VSConstants.UICONTEXT.ToolboxInitialized_string);
				SelectionMonitor.GetCmdUIContextCookie(ref rguidCmdUI, out _ToolboxCmdUICookie);
			}
			return _ToolboxCmdUICookie;
		}
	}


	/// <summary>
	/// The extension wide package instance lock.
	/// </summary>
	public object LockGlobal => _LockGlobal;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Accessor to <see cref="_Solution.Globals"/>
	/// </summary>
	// ---------------------------------------------------------------------------------
	public Globals SolutionGlobals =>
		(Dte != null && Dte.Solution != null) ? Dte.Solution.Globals : null;


	#endregion Property Accessors





	// =========================================================================================================
	#region Constructors / Destructors - AbstractPackageController
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Private singleton .ctor
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected AbstractPackageController(IBAsyncPackage package)
	{
		if (_Instance != null)
		{
			ArgumentException ex = new("Singleton PackageController instance already created");
			Diag.Dug(ex);
			throw ex;
		}

		_DdexPackage = package;
	}

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Private .ctor purely for calling instance methods intended to be static from
	/// a dummy instance of this class.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected AbstractPackageController()
	{
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the singleton AbstractPackageController instance
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static IBPackageController Instance
	{
		get
		{
			if (_Instance == null)
			{
				_ = (IBPackageController)Package.GetGlobalService(typeof(IBPackageController));

				if (_Instance == null)
				{
					NullReferenceException ex = new("Cannot instantiate PackageController service from abstract ancestor");
					Diag.Dug(ex);
					throw ex;
				}
			}

			return _Instance;
		}
	}





	// ---------------------------------------------------------------------------------
	/// <summary>
	/// AbstractPackageController disposal
	/// </summary>
	// ---------------------------------------------------------------------------------
	public virtual void Dispose()
	{
		UnadviseEvents(true);

		if (_EventsManagers != null)
		{
			foreach (IBEventsManager manager in _EventsManagers)
				manager.Dispose();
		}
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Methods - AbstractPackageController
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Enables solution and running document table event handling
	/// </summary>
	// ---------------------------------------------------------------------------------
	public abstract void AdviseEvents();




	// ---------------------------------------------------------------------------------
	/// <summary>
	/// IBPackageController service callback to deregister the solution's MiscProject.
	/// </summary>
	/// <param name="hierarchy"></param>
	// ---------------------------------------------------------------------------------
	public virtual void DeregisterMiscHierarchy()
	{
		_MiscHierarchy = null;
	}



	protected void EnsureMonitorSelection()
	{
		if (_MonitorSelection != null)
			return;

		if (!ThreadHelper.CheckAccess())
		{
			COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
			Diag.Dug(exc);
			throw exc;
		}

		_MonitorSelection = Package.GetGlobalService(typeof(IVsMonitorSelection)) as IVsMonitorSelection;

		if (_MonitorSelection != null && !IsCmdLineBuild)
			Native.ThrowOnFailure(_MonitorSelection.AdviseSelectionEvents(this, out _HSelectionEvents));


		return;

	}




	public TInterface GetService<TService, TInterface>() where TInterface : class
		=> ((AsyncPackage)DdexPackage).GetService<TService, TInterface>();



	public async Task<TInterface> GetServiceAsync<TService, TInterface>() where TInterface : class
		=> await ((AsyncPackage)DdexPackage).GetServiceAsync<TService, TInterface>();



	private async Task<IVsTaskStatusCenterService> GetStatusCenterServiceAsync()
	{
		return await ServiceProvider.GetGlobalServiceAsync<SVsTaskStatusCenterService,
						IVsTaskStatusCenterService>(swallowExceptions: false);
	}



	public void RegisterEventsManager(IBEventsManager manager)
	{
		_EventsManagers ??= new List<IBEventsManager>();

		_EventsManagers.Add(manager);

		manager.Initialize();
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// IBPackageController service callback to register the solution's MiscProject for
	/// an sql editor document cleanup on shutdown.
	/// </summary>
	/// <param name="hierarchy"></param>
	// ---------------------------------------------------------------------------------
	public virtual void RegisterMiscHierarchy(IVsUIHierarchy hierarchy)
	{
		_MiscHierarchy = hierarchy;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Disables solution event invocation
	/// </summary>
	/// <param name="disposing"></param>
	// ---------------------------------------------------------------------------------
	public void UnadviseEvents(bool disposing)
	{
		_ = disposing;

		if (DteSolution != null && _HSolutionEvents != uint.MaxValue)
			DteSolution.UnadviseSolutionEvents(_HSolutionEvents);

		if (DocTable != null && _HDocTableEvents != uint.MaxValue)
			DocTable.UnadviseRunningDocTableEvents(_HDocTableEvents);

		if (_MonitorSelection != null && _HSelectionEvents != uint.MaxValue)
			_MonitorSelection.UnadviseSelectionEvents(_HSelectionEvents);

		_HSolutionEvents = uint.MaxValue;
		_HDocTableEvents = uint.MaxValue;
		_MonitorSelection = null;
		_HSelectionEvents = uint.MaxValue;

	}



	public int OnNewQueryRequested(IVsDataViewHierarchy site, EnNodeSystemType nodeSystemType) => _OnNewQueryRequestedEvent != null
	? _OnNewQueryRequestedEvent(site, nodeSystemType) : VSConstants.E_NOTIMPL;


	#endregion Methods





	// =========================================================================================================
	#region IVsSolutionEvents Implementation and Event handling - AbstractPackageController
	// =========================================================================================================


	// Events that we handle are listed first

	public int OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded) => _OnAfterOpenProjectEvent != null
		? _OnAfterOpenProjectEvent(pHierarchy, fAdded) : VSConstants.E_NOTIMPL;

	public int OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel) => _OnQueryCloseProjectEvent != null
		? _OnQueryCloseProjectEvent(pHierarchy, fRemoving, ref pfCancel) : VSConstants.E_NOTIMPL;

	public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel) => _OnQueryCloseSolutionEvent != null
		? _OnQueryCloseSolutionEvent(pUnkReserved, ref pfCancel) : VSConstants.E_NOTIMPL;


	// Unhandled events follow

	public int OnAfterClosingChildren(IVsHierarchy hierarchy) => VSConstants.E_NOTIMPL;
	public int OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy) => VSConstants.E_NOTIMPL;
	public int OnAfterMergeSolution(object pUnkReserved) => VSConstants.E_NOTIMPL;
	public int OnAfterOpeningChildren(IVsHierarchy hierarchy) => VSConstants.E_NOTIMPL;
	public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution) => VSConstants.E_NOTIMPL;
	public int OnAfterCloseSolution(object pUnkReserved) => VSConstants.E_NOTIMPL;
	public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved) => VSConstants.E_NOTIMPL;
	public int OnBeforeCloseSolution(object pUnkReserved) => VSConstants.E_NOTIMPL;
	public int OnBeforeClosingChildren(IVsHierarchy hierarchy) => VSConstants.E_NOTIMPL;
	public int OnBeforeOpeningChildren(IVsHierarchy hierarchy) => VSConstants.E_NOTIMPL;
	public int OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy) => VSConstants.E_NOTIMPL;
	public int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel) => VSConstants.E_NOTIMPL;


	#endregion IVsSolutionEvents Implementation and Event handling





	// =========================================================================================================
	#region IVsRunningDocTableEvents Implementation and Event handling - AbstractPackageController
	// =========================================================================================================


	// Events that we handle are listed first


	public int OnAfterDocumentWindowHide(uint docCookie, IVsWindowFrame pFrame) => _OnAfterDocumentWindowHideEvent != null
		? _OnAfterDocumentWindowHideEvent(docCookie, pFrame) : VSConstants.E_NOTIMPL;

	public int OnAfterSave(uint docCookie) => _OnAfterSaveEvent != null ? _OnAfterSaveEvent(docCookie) : VSConstants.E_NOTIMPL;

	public int OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame)
		=> _OnBeforeDocumentWindowShowEvent != null
		? _OnBeforeDocumentWindowShowEvent(docCookie, fFirstShow, pFrame)
		: VSConstants.E_NOTIMPL;

	public int OnBeforeLastDocumentUnlock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining,
		uint dwEditLocksRemaining)
		=> _OnBeforeLastDocumentUnlockEvent != null
		? _OnBeforeLastDocumentUnlockEvent(docCookie, dwRDTLockType, dwReadLocksRemaining, dwEditLocksRemaining)
		: VSConstants.E_NOTIMPL;




	// Unhandled events follow

	public int OnAfterAttributeChange(uint docCookie, uint grfAttribs) => VSConstants.E_NOTIMPL;
	public int OnAfterFirstDocumentLock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining,
		uint dwEditLocksRemaining) => VSConstants.E_NOTIMPL;
	public int OnAfterLastDocumentUnlock(IVsHierarchy pHier, uint itemid, string pszMkDocument,
		int fClosedWithoutSaving) => VSConstants.E_NOTIMPL;
	public int OnAfterSaveAll() => VSConstants.E_NOTIMPL;
	public int OnBeforeFirstDocumentLock(IVsHierarchy pHier, uint itemid, string pszMkDocument) => VSConstants.E_NOTIMPL;


	#endregion IVsRunningDocTableEvents Implementation and Event handling





	// =========================================================================================================
	#region IVsSelectionEvents Implementation and Event handling - AbstractPackageController
	// =========================================================================================================


	public int OnCmdUIContextChanged(uint dwCmdUICookie, int fActive)
		=> _OnCmdUIContextChangedEvent != null
		? _OnCmdUIContextChangedEvent(dwCmdUICookie, fActive)
		: VSConstants.E_NOTIMPL;

	public int OnElementValueChanged(uint elementid, object varValueOld, object varValueNew)
		=> _OnElementValueChangedEvent != null
		? _OnElementValueChangedEvent(elementid, varValueOld, varValueNew)
		: VSConstants.E_NOTIMPL;

	public int OnSelectionChanged(IVsHierarchy pHierOld, uint itemidOld, IVsMultiItemSelect pMISOld,
		ISelectionContainer pSCOld, IVsHierarchy pHierNew, uint itemidNew,
		IVsMultiItemSelect pMISNew, ISelectionContainer pSCNew)
		=> _OnSelectionChangedEvent != null
		? _OnSelectionChangedEvent(pHierOld, itemidOld, pMISOld, pSCOld, pHierNew, itemidNew, pMISNew, pSCNew)
		: VSConstants.E_NOTIMPL;


	#endregion IVsSelectionEvents Implementation and Event handling


}