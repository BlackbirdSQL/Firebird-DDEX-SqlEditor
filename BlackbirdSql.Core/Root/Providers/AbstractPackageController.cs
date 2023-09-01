// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)


using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using BlackbirdSql.Core.Interfaces;

using EnvDTE;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;





namespace BlackbirdSql.Core.Providers;



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

	private readonly object _PackageLock = new object();

	private static IVsUIHierarchy _MiscHierarchy = null;


	private readonly IBAsyncPackage _DdexPackage;

	protected IVsMonitorSelection _MonitorSelection = null;

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


	#endregion Variables





	// =========================================================================================================
	#region Property Accessors - AbstractPackageController
	// =========================================================================================================


	public string UserDataDirectory => Get_ApplicationDataDirectory();



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

	public uint ToolboxCmdUICookie
	{
		get
		{
			if (_ToolboxCmdUICookie == 0 && SelectionMonitor != null)
			{
				Guid rguidCmdUI = new Guid(VSConstants.UICONTEXT.ToolboxInitialized_string);
				SelectionMonitor.GetCmdUIContextCookie(ref rguidCmdUI, out _ToolboxCmdUICookie);
			}
			return _ToolboxCmdUICookie;
		}
	}

	public object PackageLock => _PackageLock;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Accessor to <see cref="_Solution.Globals"/>
	/// </summary>
	// ---------------------------------------------------------------------------------
	public Globals SolutionGlobals => Dte.Solution.Globals;


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
		// We should already be on UI thread. Callers must ensure this can never happen
		ThreadHelper.ThrowIfNotOnUIThread();

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



	public string Get_ApplicationDataDirectory()
	{
		/*
		ThreadHelper.ThrowIfNotOnUIThread();
		if (_UserDataDirectory == null)
		{
			try
			{
				IVsShell service = ((AsyncPackage)DdexPackage).GetService<SVsShell, IVsShell>();
				if (service == null)
				{
					ArgumentException ex = new("Could not get service <SVsShell, IVsShell>");
					Diag.Dug(ex);
					throw ex;
				}

				Native.WrapComCall(service.GetProperty((int)__VSSPROPID.VSSPROPID_AppDataDir, out object pvar));
				_UserDataDirectory = pvar as string;
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw ex;
			}
		}
		*/

		return _UserDataDirectory ??= Environment.GetFolderPath(Environment.SpecialFolder.Personal);
	}


	protected void EnsureMonitorSelection()
	{
		if (_MonitorSelection != null)
			return;

		_MonitorSelection = Package.GetGlobalService(typeof(IVsMonitorSelection)) as IVsMonitorSelection;

		if (_MonitorSelection != null && !IsCmdLineBuild)
			Native.ThrowOnFailure(_MonitorSelection.AdviseSelectionEvents(this, out _HSelectionEvents));

		
		return;

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
		// We should already be on UI thread. Callers must ensure this can never happen
		ThreadHelper.ThrowIfNotOnUIThread();

		_ = disposing;

		if (DteSolution != null && _HSolutionEvents != uint.MaxValue)
		{
			DteSolution.UnadviseSolutionEvents(_HSolutionEvents);
			_HSolutionEvents = uint.MaxValue;
		}

		if (DocTable != null && _HDocTableEvents != uint.MaxValue)
		{
			DocTable.UnadviseRunningDocTableEvents(_HDocTableEvents);
			_HDocTableEvents = uint.MaxValue;
		}

		if (_MonitorSelection != null && _HSelectionEvents != uint.MaxValue)
		{
			_MonitorSelection.UnadviseSelectionEvents(_HSelectionEvents);
			_MonitorSelection = null;
			_HSelectionEvents = uint.MaxValue;
		}

	}


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