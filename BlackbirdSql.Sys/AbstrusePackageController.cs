// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using BlackbirdSql.Sys.Enums;
using BlackbirdSql.Sys.Interfaces;
using BlackbirdSql.Sys.Properties;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Data.Services.SupportEntities;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TaskStatusCenter;



namespace BlackbirdSql.Sys;

[SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification = "<Pending>")]


// =========================================================================================================
//										AbstrusePackageController Class
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
public abstract class AbstrusePackageController : IBsPackageController
{


	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - AbstrusePackageController
	// ---------------------------------------------------------------------------------


	/// <summary>
	/// Private singleton .ctor
	/// </summary>
	protected AbstrusePackageController(IBsAsyncPackage ddex)
	{
		if (_Instance != null)
		{
			TypeAccessException ex = new(Resources.ExceptionDuplicateSingletonInstances.FmtRes(GetType().FullName));
			Diag.Dug(ex);
			throw ex;
		}


		_Instance = this;
		_PackageInstance = ddex;

		if (Package.GetGlobalService(typeof(DTE)) is DTE2 dte)
		{
			dte.Events.DTEEvents.OnBeginShutdown += OnBeginShutdown;
		}
		else
		{
			throw new NullReferenceException("DTE2 is not initialized");
		}

		ddex.OnLoadSolutionOptionsEvent += OnLoadSolutionOptions;
		ddex.OnSaveSolutionOptionsEvent += OnSaveSolutionOptions;

	}




	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the singleton AbstrusePackageController instance
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static IBsPackageController Instance
	{
		get
		{
			if (_Instance == null)
			{
				_ = (IBsPackageController)Package.GetGlobalService(typeof(IBsPackageController));

				if (_Instance == null)
				{
					TypeAccessException ex = new("Cannot instantiate PackageController service from abstract ancestor");
					Diag.Dug(ex);
					throw ex;
				}
			}

			return _Instance;
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// AbstrusePackageController disposal
	/// </summary>
	// ---------------------------------------------------------------------------------
	public virtual void Dispose()
	{
		_PackageInstance.OnLoadSolutionOptionsEvent -= OnLoadSolutionOptions;
		_PackageInstance.OnSaveSolutionOptionsEvent -= OnSaveSolutionOptions;

		UnadviseEvents(true);

		if (_EventsManagers != null)
		{
			foreach (IBsEventsManager manager in _EventsManagers)
				manager.Dispose();
		}

		
	}


	#endregion Constructors / Destructors




	// =========================================================================================================
	#region Fields - AbstrusePackageController
	// =========================================================================================================


	private DTE _Dte = null;
	private bool _ProjectEvents = false;
	private static bool _IdeShutdownState = false;
	private static int _RdtEventsCardinal = 0;

	protected static bool _EventsAdvisedUnsafe = false;
	protected static bool _SolutionLoaded = false;
	protected int _RefCnt = 0;

	protected static IBsPackageController _Instance = null;
	protected static List<IBsEventsManager> _EventsManagers = null;

	// A package instance global lock
	private readonly object _LockGlobal = new object();


	private readonly IBsAsyncPackage _PackageInstance;

	protected IVsMonitorSelection _MonitorSelection = null;
	private IVsTaskStatusCenterService _StatusCenterService = null;

	protected uint _HSolutionEvents = uint.MaxValue;
	protected uint _HDocTableEvents = uint.MaxValue;
	protected uint _HSelectionEvents = uint.MaxValue;

	protected uint _ToolboxCmdUICookie;

	private string _UserDataDirectory = null;


	private IBsPackageController.AfterAttributeChangeDelegate _OnAfterAttributeChangeEvent;
	private IBsPackageController.AfterAttributeChangeExDelegate _OnAfterAttributeChangeExEvent;
	private IBsPackageController.AfterDocumentWindowHideDelegate _OnAfterDocumentWindowHideEvent;
	private IBsPackageController.AfterSaveDelegate _OnAfterSaveEvent;
	private IBsPackageController.AfterSaveAsyncDelegate _OnAfterSaveAsyncEvent;
	private IBsPackageController.BeforeDocumentWindowShowDelegate _OnBeforeDocumentWindowShowEvent;
	private IBsPackageController.BeforeLastDocumentUnlockDelegate _OnBeforeLastDocumentUnlockEvent;
	private IBsPackageController.BeforeSaveDelegate _OnBeforeSaveEvent;
	private IBsPackageController.BeforeSaveAsyncDelegate _OnBeforeSaveAsyncEvent;

	private IBsPackageController.AfterOpenProjectDelegate _OnAfterOpenProjectEvent;
	private IBsPackageController.BeforeCloseProjectDelegate _OnBeforeCloseProjectEvent;
	private IBsPackageController.BeforeCloseSolutionDelegate _OnBeforeCloseSolutionEvent;
	private IBsPackageController.LoadSolutionOptionsDelegate _OnLoadSolutionOptionsEvent;
	private IBsPackageController.SaveSolutionOptionsDelegate _OnSaveSolutionOptionsEvent;
	private IBsPackageController.AfterCloseSolutionDelegate _OnAfterCloseSolutionEvent;
	private IBsPackageController.QueryCloseProjectDelegate _OnQueryCloseProjectEvent;
	private IBsPackageController.QueryCloseSolutionDelegate _OnQueryCloseSolutionEvent;

	// Selection Events Delegates
	private IBsPackageController.SelectionChangedDelegate _OnSelectionChangedEvent;
	private IBsPackageController.ElementValueChangedDelegate _OnElementValueChangedEvent;
	private IBsPackageController.CmdUIContextChangedDelegate _OnCmdUIContextChangedEvent;

	// Custom events Delegates
	private IBsPackageController.NewQueryRequestedDelegate _OnNewQueryRequestedEvent;


	#endregion Fields




	// =========================================================================================================
	#region Property Accessors - AbstrusePackageController
	// =========================================================================================================

	public string UserDataDirectory => _UserDataDirectory ??=
		Environment.GetFolderPath(Environment.SpecialFolder.Personal);

	public static bool IdeShutdownState => _IdeShutdownState;


	public IBsAsyncPackage PackageInstance => _PackageInstance;



	public DTE Dte
	{
		get
		{
			if (_IdeShutdownState)
				return null;

			if (_Dte != null)
				return _Dte;

			_Dte = PackageInstance.GetService<DTE, DTE>();

			if (_Dte == null)
				ResetDte();

			return _Dte;
		}
	}




	private bool RdtEventsEnabled => _RdtEventsCardinal == 0;

	public object SolutionObject => !_IdeShutdownState ? Dte.Solution : null;

	public abstract bool SolutionValidating { get; }


	public IVsSolution VsSolution => _PackageInstance.VsSolution;

	public bool IsCmdLineBuild
	{
		get
		{
			Diag.ThrowIfNotOnUIThread();

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
				Diag.ThrowIfNotOnUIThread();

				___(SelectionMonitor.IsCmdUIContextActive(ToolboxCmdUICookie, out int pfActive));

				return pfActive != 0;
			}
			return true;
		}
	}



	public IVsMonitorSelection SelectionMonitor
	{
		get
		{
			Diag.ThrowIfNotOnUIThread();

			if (_MonitorSelection == null)
				EnsureMonitorSelection();
			return _MonitorSelection;
		}
	}


	public IAsyncServiceContainer Services => (IAsyncServiceContainer)_PackageInstance;


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

	public IVsTaskStatusCenterService StatusCenterService => _StatusCenterService
		??= GetService<SVsTaskStatusCenterService, IVsTaskStatusCenterService>();
	
	//	ThreadHelper.JoinableTaskFactory.Run(new Func<Task<IVsTaskStatusCenterService>>(GetStatusCenterServiceAsync));



	public uint ToolboxCmdUICookie
	{
		get
		{
			if (_ToolboxCmdUICookie == 0 && SelectionMonitor != null)
			{
				Diag.ThrowIfNotOnUIThread();

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


	#endregion Property Accessors




	// =========================================================================================================
	#region Event Property Accessors - AbstrusePackageController
	// =========================================================================================================


	/// <summary>
	/// Accessor to the <see cref="IVsRunningDocTableEvents.OnAfterAttributeChange"/> event.
	/// </summary>
	event IBsPackageController.AfterAttributeChangeDelegate IBsPackageController.OnAfterAttributeChangeEvent
	{
		add { _OnAfterAttributeChangeEvent += value; }
		remove { _OnAfterAttributeChangeEvent -= value; }
	}


	/// <summary>
	/// Accessor to the <see cref="IVsRunningDocTableEvents3.OnAfterAttributeChangeEx"/> event.
	/// </summary>
	event IBsPackageController.AfterAttributeChangeExDelegate IBsPackageController.OnAfterAttributeChangeExEvent
	{
		add { _OnAfterAttributeChangeExEvent += value; }
		remove { _OnAfterAttributeChangeExEvent -= value; }
	}


	/// <summary>
	/// Accessor to the <see cref="IVsRunningDocTableEvents.OnAfterDocumentWindowHide"/> event.
	/// </summary>
	event IBsPackageController.AfterDocumentWindowHideDelegate IBsPackageController.OnAfterDocumentWindowHideEvent
	{
		add { _OnAfterDocumentWindowHideEvent += value; }
		remove { _OnAfterDocumentWindowHideEvent -= value; }
	}


	/// <summary>
	/// Accessor to the <see cref="IVsSolutionEvents.OnAfterOpenProject"/> event.
	/// </summary>
	event IBsPackageController.AfterOpenProjectDelegate IBsPackageController.OnAfterOpenProjectEvent
	{
		add { _OnAfterOpenProjectEvent += value; }
		remove { _OnAfterOpenProjectEvent -= value; }
	}


	/// <summary>
	/// Accessor to the <see cref="Package.OnLoadOptions"/> event.
	/// </summary>
	event IBsPackageController.LoadSolutionOptionsDelegate IBsPackageController.OnLoadSolutionOptionsEvent
	{
		add { _OnLoadSolutionOptionsEvent += value; }
		remove { _OnLoadSolutionOptionsEvent -= value; }
	}

	/// <summary>
	/// Accessor to the <see cref="Package.OnLoadOptions"/> event.
	/// </summary>
	event IBsPackageController.SaveSolutionOptionsDelegate IBsPackageController.OnSaveSolutionOptionsEvent
	{
		add { _OnSaveSolutionOptionsEvent += value; }
		remove { _OnSaveSolutionOptionsEvent -= value; }
	}


	/// <summary>
	/// Accessor to the <see cref="IVsSolutionEvents.OnAfterCloseSolution"/> event.
	/// </summary>
	event IBsPackageController.AfterCloseSolutionDelegate IBsPackageController.OnAfterCloseSolutionEvent
	{
		add { _OnAfterCloseSolutionEvent += value; }
		remove { _OnAfterCloseSolutionEvent -= value; }
	}


	/// <summary>
	/// Accessor to the <see cref="IVsRunningDocTableEvents.OnAfterSave"/> event.
	/// </summary>
	event IBsPackageController.AfterSaveDelegate IBsPackageController.OnAfterSaveEvent
	{
		add { _OnAfterSaveEvent += value; }
		remove { _OnAfterSaveEvent -= value; }
	}

	/// <summary>
	/// Accessor to the <see cref="IVsRunningDocTableEvents7.OnAfterSaveAsync"/> event.
	/// Comment out and remove IVsRunningDocTableEvents7 interface to enable OnAfterSave.
	/// </summary>
	event IBsPackageController.AfterSaveAsyncDelegate IBsPackageController.OnAfterSaveAsyncEvent
	{
		add { _OnAfterSaveAsyncEvent += value; }
		remove { _OnAfterSaveAsyncEvent -= value; }
	}


	/// <summary>
	/// Accessor to the <see cref="IVsRunningDocTableEvents.OnBeforeDocumentWindowShow"/> event.
	/// </summary>
	event IBsPackageController.BeforeDocumentWindowShowDelegate IBsPackageController.OnBeforeDocumentWindowShowEvent
	{
		add { _OnBeforeDocumentWindowShowEvent += value; }
		remove { _OnBeforeDocumentWindowShowEvent -= value; }
	}


	/// <summary>
	/// Accessor to the <see cref="IVsRunningDocTableEvents.OnBeforeLastDocumentUnlock"/> event.
	/// </summary>
	event IBsPackageController.BeforeLastDocumentUnlockDelegate IBsPackageController.OnBeforeLastDocumentUnlockEvent
	{
		add { _OnBeforeLastDocumentUnlockEvent += value; }
		remove { _OnBeforeLastDocumentUnlockEvent -= value; }
	}

	/// <summary>
	/// Accessor to the <see cref="IVsRunningDocTableEvents3.OnBeforeSave"/> event.
	/// </summary>
	event IBsPackageController.BeforeSaveDelegate IBsPackageController.OnBeforeSaveEvent
	{
		add { _OnBeforeSaveEvent += value; }
		remove { _OnBeforeSaveEvent -= value; }
	}

	/// <summary>
	/// Accessor to the <see cref="IVsRunningDocTableEvents7.OnBeforeSaveAsync"/> event.
	/// Comment out and remove IVsRunningDocTableEvents7 interface to enable OnAfterSave.
	/// </summary>
	event IBsPackageController.BeforeSaveAsyncDelegate IBsPackageController.OnBeforeSaveAsyncEvent
	{
		add { _OnBeforeSaveAsyncEvent += value; }
		remove { _OnBeforeSaveAsyncEvent -= value; }
	}


	/// <summary>
	/// Accessor to the <see cref="IVsSelectionEvents.OnCmdUIContextChanged"/> event.
	/// </summary>
	event IBsPackageController.CmdUIContextChangedDelegate IBsPackageController.OnCmdUIContextChangedEvent
	{
		add { _OnCmdUIContextChangedEvent += value; }
		remove { _OnCmdUIContextChangedEvent -= value; }
	}


	/// <summary>
	/// Accessor to the <see cref="IVsSelectionEvents.OnElementValueChangedEvent"/> event.
	/// </summary>
	event IBsPackageController.ElementValueChangedDelegate IBsPackageController.OnElementValueChangedEvent
	{
		add { _OnElementValueChangedEvent += value; }
		remove { _OnElementValueChangedEvent -= value; }
	}

	/// <summary>
	/// Accessor to the <see cref="IVsSolutionEvents.OnBeforeCloseProject"/> event.
	/// </summary>
	event IBsPackageController.BeforeCloseProjectDelegate IBsPackageController.OnBeforeCloseProjectEvent
	{
		add { _OnBeforeCloseProjectEvent += value; }
		remove { _OnBeforeCloseProjectEvent -= value; }
	}


	/// <summary>
	/// Accessor to the <see cref="IVsSolutionEvents.OnBeforeCloseSolutionEvent"/> event.
	/// </summary>
	event IBsPackageController.BeforeCloseSolutionDelegate IBsPackageController.OnBeforeCloseSolutionEvent
	{
		add { _OnBeforeCloseSolutionEvent += value; }
		remove { _OnBeforeCloseSolutionEvent -= value; }
	}


	/// <summary>
	/// Accessor to the <see cref="IVsSolutionEvents.OnQueryCloseProject"/> event.
	/// </summary>
	event IBsPackageController.QueryCloseProjectDelegate IBsPackageController.OnQueryCloseProjectEvent
	{
		add { _OnQueryCloseProjectEvent += value; }
		remove { _OnQueryCloseProjectEvent -= value; }
	}


	/// <summary>
	/// Accessor to the <see cref="IVsSolutionEvents.OnQueryCloseSolutionEvent"/> event.
	/// </summary>
	event IBsPackageController.QueryCloseSolutionDelegate IBsPackageController.OnQueryCloseSolutionEvent
	{
		add { _OnQueryCloseSolutionEvent += value; }
		remove { _OnQueryCloseSolutionEvent -= value; }
	}


	/// <summary>
	/// Accessor to the <see cref="IVsSelectionEvents.OnSelectionChangedEvent"/> event.
	/// </summary>
	event IBsPackageController.SelectionChangedDelegate IBsPackageController.OnSelectionChangedEvent
	{
		add { _OnSelectionChangedEvent += value; }
		remove { _OnSelectionChangedEvent -= value; }
	}


	/// <summary>
	/// Accessor to the <see cref="IVsSelectionEvents.OnSelectionChangedEvent"/> event.
	/// </summary>
	event IBsPackageController.NewQueryRequestedDelegate IBsPackageController.OnNewQueryRequestedEvent
	{
		add { _OnNewQueryRequestedEvent += value; }
		remove { _OnNewQueryRequestedEvent -= value; }
	}


	#endregion Event Property Accessors




	// =========================================================================================================
	#region Methods - AbstrusePackageController
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Enables solution and running document table event handling
	/// </summary>
	// ---------------------------------------------------------------------------------
	public abstract bool AdviseEvents();



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Asynchronously enables solution and running document table event handling
	/// </summary>
	// ---------------------------------------------------------------------------------
	public abstract Task<bool> AdviseEventsAsync();



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Enables UI thread event handling
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected abstract bool AdviseUnsafeEvents();




	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Increments the <see cref="RdtEventsDisabled"/> counter when execution enters
	/// an Rdt event handler to prevent recursion
	/// </summary>
	// ---------------------------------------------------------------------------------
	public void DisableRdtEvents()
	{
		lock (_LockGlobal)
			_RdtEventsCardinal++;
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Decrements the <see cref="RdtEventsDisabled"/> counter that was previously
	/// incremented by <see cref="DisableRdtEvents"/>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public void EnableRdtEvents()
	{
		lock (_LockGlobal)
		{
			if (_RdtEventsCardinal == 0)
				Diag.Dug(new InvalidOperationException(Resources.ExceptionEventsAlreadyEnabled));
			else
				_RdtEventsCardinal--;
		}
	}


	public abstract string CreateConnectionUrl(IDbConnection connection);
	public abstract string CreateConnectionUrl(string connectionString);
	public abstract string GetRegisterConnectionDatasetKey(IDbConnection connection);
	public abstract void InvalidateRctManager();
	public abstract bool IsConnectionEquivalency(string connectionString1, string connectionString2);
	public abstract bool IsWeakConnectionEquivalency(string connectionString1, string connectionString2);

	public void EnsureMonitorSelection()
	{
		if (_MonitorSelection != null)
			return;

		Diag.ThrowIfNotOnUIThread();

		_MonitorSelection = Package.GetGlobalService(typeof(IVsMonitorSelection)) as IVsMonitorSelection;

		if (_MonitorSelection != null && !IsCmdLineBuild)
			___(_MonitorSelection.AdviseSelectionEvents(this, out _HSelectionEvents));


		return;

	}

	public TInterface EnsureService<TService, TInterface>() where TInterface : class
	{
		TInterface @interface = PackageInstance.GetService<TService, TInterface>();
		Diag.ThrowIfServiceUnavailable(@interface, typeof(TInterface));

		return @interface;
	}


	/// <summary>
	/// <see cref="ErrorHandler.ThrowOnFailure"/> token.
	/// </summary>
	protected static int ___(int hr) => ErrorHandler.ThrowOnFailure(hr);


	public TInterface GetService<TInterface>() where TInterface : class
		=> GetService<TInterface, TInterface>();


	public TInterface GetService<TService, TInterface>() where TInterface : class
		=> PackageInstance.GetService<TService, TInterface>();


	public async Task<TInterface> GetServiceAsync<TInterface>() where TInterface : class
		=> await GetServiceAsync<TInterface, TInterface>();


	public async Task<TInterface> GetServiceAsync<TService, TInterface>() where TInterface : class
		=> await PackageInstance.GetServiceAsync<TService, TInterface>();



	public async Task<IVsTaskStatusCenterService> GetStatusCenterServiceAsync()
	{
		return await ServiceProvider.GetGlobalServiceAsync<SVsTaskStatusCenterService,
						IVsTaskStatusCenterService>(swallowExceptions: false);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Registers events managers for future disposal.
	/// </summary>
	/// <param name="manager"></param>
	// ---------------------------------------------------------------------------------
	public void RegisterEventsManager(IBsEventsManager manager)
	{
		_EventsManagers ??= [];
		_EventsManagers.Add(manager);
	}


	public static void ResetDte()
	{
		if (_Instance != null)
			((AbstrusePackageController)_Instance)._Dte = null;

		_IdeShutdownState = true;
	}

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Disables solution event invocation
	/// </summary>
	/// <param name="disposing"></param>
	// ---------------------------------------------------------------------------------
	public abstract void UnadviseEvents(bool disposing);




	public abstract void ValidateSolution();


	#endregion Methods





	// =========================================================================================================
	#region General Event handling - AbstrusePackageController
	// =========================================================================================================


	public int OnNewQueryRequested(IVsDataViewHierarchy site, EnNodeSystemType nodeSystemType) => _OnNewQueryRequestedEvent != null
	? _OnNewQueryRequestedEvent(site, nodeSystemType) : VSConstants.E_NOTIMPL;


	#endregion General Event handling





	// =========================================================================================================
	#region IVsSolutionEvents/2/3 Implementation and Event handling - AbstrusePackageController
	// =========================================================================================================


	// Events that we handle are listed first


	protected void OnProjectItemAdded(ProjectItem projectItem)
	{
		// Tracer.Trace(GetType(), "OnProjectItemAdded()", "Added misc ProjectItem: {0}.", projectItem.Name);
	}


	protected void OnProjectItemRemoved(ProjectItem projectItem)
	{
		// Tracer.Trace(GetType(), "OnProjectItemRemoved()", "Removed misc ProjectItem: {0}.", projectItem.Name);
	}

	protected void OnProjectItemRenamed(ProjectItem projectItem, string oldName)
	{
		// Tracer.Trace(GetType(), "OnProjectItemRenamed()", "Renamed misc ProjectItem: {0}, oldname: {1}.", projectItem.Name, oldName);
	}


	/// <summary>
	/// Implements <see cref="IVsSolutionEvents.OnAfterOpenProject"/>,
	/// <see cref="IVsSolutionEvents2.OnAfterOpenProject"/> and <see cref="IVsSolutionEvents3.OnAfterOpenProject"/>
	/// </summary>
	public int OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
	{
		if (!_ProjectEvents)
			return VSConstants.S_OK;

		if (_OnAfterOpenProjectEvent == null)
			return VSConstants.E_NOTIMPL;

		// Get the root (project) node. 
		var itemid = VSConstants.VSITEMID_ROOT;

		pHierarchy.GetProperty(itemid, (int)__VSHPROPID.VSHPROPID_ExtObject, out object objProj);


		if (objProj is not Project project)
			return VSConstants.S_OK;

		if (project.Properties == null)
			_ = OnAfterOpenProjectAsync(project, fAdded);
		else
			return _OnAfterOpenProjectEvent.Invoke(project, fAdded);

		return VSConstants.S_OK;
	}





	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Ensure project is fully loaded.
	/// </summary>
	/// <param name="project"></param>
	/// <returns></returns>
	// ---------------------------------------------------------------------------------
	private async Task OnAfterOpenProjectAsync(Project project, int fAdded)
	{
		await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

		// Recycle until project object is complete if necessary
		if (project.Properties == null)
		{
			if (++_RefCnt < 1000)
			{
				await Task.Delay(200);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
				ThreadHelper.JoinableTaskFactory.RunAsync(() => OnAfterOpenProjectAsync(project, fAdded));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			}
			return;
		}

		// Tracer.Trace("Project opened");
		// If anything gets through things are still happening so we can reset and allow events with incomplete project objects
		// to continue recycling
		_RefCnt = 0;

		_OnAfterOpenProjectEvent?.Invoke(project, fAdded);

		return;
	}


	protected void OnBeginShutdown()
	{
		ResetDte();
	}



	public void OnLoadSolutionOptions(Stream stream)
	{
		if (_SolutionLoaded)
			return;

		_SolutionLoaded = true;

		try
		{
			_OnLoadSolutionOptionsEvent?.Invoke(stream);
		}
		finally
		{
			_ProjectEvents = true;
		}

	}

	public void OnSaveSolutionOptions(Stream stream) =>
		_OnSaveSolutionOptionsEvent?.Invoke(stream);


	/// <summary>
	/// Implements <see cref="IVsSolutionEvents.OnAfterCloseSolution"/>,
	/// <see cref="IVsSolutionEvents2.OnAfterCloseSolution"/> and <see cref="IVsSolutionEvents3.OnAfterCloseSolution"/>
	/// </summary>
	public int OnAfterCloseSolution(object pUnkReserved)
	{
		_SolutionLoaded = false;
		_ProjectEvents = false;

		if (_OnAfterCloseSolutionEvent != null)
			return _OnAfterCloseSolutionEvent(pUnkReserved);
		else
			return VSConstants.E_NOTIMPL;
	}


	/// <summary>
	/// Implements <see cref="IVsSolutionEvents.OnBeforeCloseProject"/>,
	/// <see cref="IVsSolutionEvents2.OnBeforeCloseProject"/> and <see cref="IVsSolutionEvents3.OnBeforeCloseProject"/>
	/// </summary>
	public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved) =>
		ThreadHelper.CheckAccess() && _OnBeforeCloseProjectEvent != null
			? _OnBeforeCloseProjectEvent(pHierarchy, fRemoved) : VSConstants.E_NOTIMPL;


	/// <summary>
	/// Implements <see cref="IVsSolutionEvents.OnBeforeCloseSolution"/>,
	/// <see cref="IVsSolutionEvents2.OnBeforeCloseSolution"/> and <see cref="IVsSolutionEvents3.OnBeforeCloseSolution"/>
	/// </summary>
	public int OnBeforeCloseSolution(object pUnkReserved) => _OnBeforeCloseSolutionEvent != null
		? _OnBeforeCloseSolutionEvent(pUnkReserved) : VSConstants.E_NOTIMPL;


	/// <summary>
	/// Implements <see cref="IVsSolutionEvents.OnQueryCloseProject"/>,
	/// <see cref="IVsSolutionEvents2.OnQueryCloseProject"/> and <see cref="IVsSolutionEvents3.OnQueryCloseProject"/>
	/// </summary>
	public int OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel) => _OnQueryCloseProjectEvent != null
		? _OnQueryCloseProjectEvent(pHierarchy, fRemoving, ref pfCancel) : VSConstants.E_NOTIMPL;


	/// <summary>
	/// Implements <see cref="IVsSolutionEvents.OnQueryCloseSolution"/>,
	/// <see cref="IVsSolutionEvents2.OnQueryCloseSolution"/> and <see cref="IVsSolutionEvents3.OnQueryCloseSolution"/>
	/// </summary>
	public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel) => _OnQueryCloseSolutionEvent != null
		? _OnQueryCloseSolutionEvent(pUnkReserved, ref pfCancel) : VSConstants.E_NOTIMPL;


	// Unhandled events follow

	int IVsSolutionEvents3.OnAfterClosingChildren(IVsHierarchy hierarchy) => VSConstants.E_NOTIMPL;
	public int OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy) => VSConstants.E_NOTIMPL;
	public int OnAfterMergeSolution(object pUnkReserved) => VSConstants.E_NOTIMPL;
	int IVsSolutionEvents3.OnAfterOpeningChildren(IVsHierarchy hierarchy) => VSConstants.E_NOTIMPL;
	public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution) => VSConstants.E_NOTIMPL;
	int IVsSolutionEvents3.OnBeforeClosingChildren(IVsHierarchy hierarchy) => VSConstants.E_NOTIMPL;
	int IVsSolutionEvents3.OnBeforeOpeningChildren(IVsHierarchy hierarchy) => VSConstants.E_NOTIMPL;
	public int OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy) => VSConstants.E_NOTIMPL;
	public int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel) => VSConstants.E_NOTIMPL;


	#endregion IVsSolutionEvents Implementation and Event handling





	// =========================================================================================================
	#region IVsRunningDocTableEvents Implementation and Event handling - AbstrusePackageController
	// =========================================================================================================


	// Events that we handle are listed first


	public int OnAfterAttributeChange(uint docCookie, uint grfAttribs) =>
		_OnAfterAttributeChangeEvent != null && RdtEventsEnabled
		? _OnAfterAttributeChangeEvent(docCookie, grfAttribs) : VSConstants.E_NOTIMPL;


	public int OnAfterAttributeChangeEx(uint docCookie, uint grfAttribs, IVsHierarchy pHierOld, uint itemidOld,
		string pszMkDocumentOld, IVsHierarchy pHierNew, uint itemidNew, string pszMkDocumentNew)
	{
		if (_OnAfterAttributeChangeExEvent == null || !RdtEventsEnabled)
			return VSConstants.S_OK;

		// Fire and wait.

		if (!ThreadHelper.CheckAccess())
		{
			return ThreadHelper.JoinableTaskFactory.Run(async delegate
			{
				await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
				return OnAfterAttributeChangeExImpl(docCookie, grfAttribs, pHierOld, itemidOld,
					pszMkDocumentOld, pHierNew, itemidNew, pszMkDocumentNew);
			});
		}

		return OnAfterAttributeChangeExImpl(docCookie, grfAttribs, pHierOld, itemidOld,
			pszMkDocumentOld, pHierNew, itemidNew, pszMkDocumentNew);
	}

	private int OnAfterAttributeChangeExImpl(uint docCookie, uint grfAttribs, IVsHierarchy pHierOld, uint itemidOld,
		string pszMkDocumentOld, IVsHierarchy pHierNew, uint itemidNew, string pszMkDocumentNew) =>
			_OnAfterAttributeChangeExEvent(docCookie, grfAttribs, pHierOld, itemidOld,
					pszMkDocumentOld, pHierNew, itemidNew, pszMkDocumentNew);


	public int OnAfterDocumentWindowHide(uint docCookie, IVsWindowFrame pFrame)
	{
		if (_OnAfterDocumentWindowHideEvent == null || !RdtEventsEnabled)
			return VSConstants.S_OK;

		// Fire and wait.

		if (!ThreadHelper.CheckAccess())
		{
			return ThreadHelper.JoinableTaskFactory.Run(async delegate
			{
				await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
				return OnAfterDocumentWindowHideImpl(docCookie, pFrame);
			});
		}

		return OnAfterDocumentWindowHideImpl(docCookie, pFrame);
	}

	private int OnAfterDocumentWindowHideImpl(uint docCookie, IVsWindowFrame pFrame) =>
		_OnAfterDocumentWindowHideEvent(docCookie, pFrame);




	public int OnAfterSave(uint docCookie) =>
		_OnAfterSaveEvent != null && RdtEventsEnabled
		? _OnAfterSaveEvent(docCookie) : VSConstants.E_NOTIMPL;

	public IVsTask OnAfterSaveAsync(uint cookie, uint flags) =>
		_OnAfterSaveAsyncEvent != null && RdtEventsEnabled
		? _OnAfterSaveAsyncEvent(cookie, flags) : null;


	public int OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame)
	{
		if (_OnBeforeDocumentWindowShowEvent == null || !RdtEventsEnabled)
			return VSConstants.S_OK;

		// Fire and wait.

		if (!ThreadHelper.CheckAccess())
		{
			return ThreadHelper.JoinableTaskFactory.Run(async delegate
			{
				await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
				return OnBeforeDocumentWindowShowImpl(docCookie, fFirstShow, pFrame);
			});
		}

		return OnBeforeDocumentWindowShowImpl(docCookie, fFirstShow, pFrame);
	}

	private int OnBeforeDocumentWindowShowImpl(uint docCookie, int fFirstShow, IVsWindowFrame pFrame) =>
		_OnBeforeDocumentWindowShowEvent(docCookie, fFirstShow, pFrame);




	public int OnBeforeLastDocumentUnlock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining,
		uint dwEditLocksRemaining)
		=> _OnBeforeLastDocumentUnlockEvent != null && RdtEventsEnabled
		? _OnBeforeLastDocumentUnlockEvent(docCookie, dwRDTLockType, dwReadLocksRemaining, dwEditLocksRemaining)
		: VSConstants.E_NOTIMPL;

	int IVsRunningDocTableEvents3.OnBeforeSave(uint docCookie)
		=> _OnBeforeSaveEvent != null && RdtEventsEnabled
		? _OnBeforeSaveEvent(docCookie)
		: VSConstants.E_NOTIMPL;

	public IVsTask OnBeforeSaveAsync(uint cookie, uint flags, IVsTask saveTask) =>
		_OnBeforeSaveAsyncEvent != null && RdtEventsEnabled
		? _OnBeforeSaveAsyncEvent(cookie, flags, saveTask) : null;



	// Unhandled events follow

	public int OnAfterFirstDocumentLock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining,
		uint dwEditLocksRemaining) => VSConstants.E_NOTIMPL;
	int IVsRunningDocTableEvents4.OnAfterLastDocumentUnlock(IVsHierarchy pHier, uint itemid, string pszMkDocument,
		int fClosedWithoutSaving) => VSConstants.E_NOTIMPL;
	int IVsRunningDocTableEvents4.OnAfterSaveAll() => VSConstants.E_NOTIMPL;
	int IVsRunningDocTableEvents4.OnBeforeFirstDocumentLock(IVsHierarchy pHier, uint itemid, string pszMkDocument) => VSConstants.E_NOTIMPL;


	#endregion IVsRunningDocTableEvents Implementation and Event handling





	// =========================================================================================================
	#region IVsSelectionEvents Implementation and Event handling - AbstrusePackageController
	// =========================================================================================================


	public int OnCmdUIContextChanged(uint dwCmdUICookie, int fActive)
		=> _OnCmdUIContextChangedEvent != null
		? _OnCmdUIContextChangedEvent(dwCmdUICookie, fActive)
		: VSConstants.E_NOTIMPL;


	public int OnElementValueChanged(uint elementid, object varValueOld, object varValueNew)
	{
		if (_OnElementValueChangedEvent == null || !RdtEventsEnabled)
			return VSConstants.S_OK;

		// Fire and wait.

		if (!ThreadHelper.CheckAccess())
		{
			return ThreadHelper.JoinableTaskFactory.Run(async delegate
			{
				await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
				return OnElementValueChangedImpl(elementid, varValueOld, varValueNew);
			});
		}

		return OnElementValueChangedImpl(elementid, varValueOld, varValueNew);
	}

	private int OnElementValueChangedImpl(uint elementid, object varValueOld, object varValueNew) =>
		_OnElementValueChangedEvent(elementid, varValueOld, varValueNew);





	public int OnSelectionChanged(IVsHierarchy pHierOld, uint itemidOld, IVsMultiItemSelect pMISOld,
		ISelectionContainer pSCOld, IVsHierarchy pHierNew, uint itemidNew,
		IVsMultiItemSelect pMISNew, ISelectionContainer pSCNew)
		=> _OnSelectionChangedEvent != null
		? _OnSelectionChangedEvent(pHierOld, itemidOld, pMISOld, pSCOld, pHierNew, itemidNew, pMISNew, pSCNew)
		: VSConstants.E_NOTIMPL;


	#endregion IVsSelectionEvents Implementation and Event handling


}