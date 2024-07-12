// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using BlackbirdSql.Sys.Enums;
using BlackbirdSql.Sys.Interfaces;
using BlackbirdSql.Sys.Properties;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Design;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TaskStatusCenter;
using VSLangProj;



namespace BlackbirdSql.Sys;


// =========================================================================================================
//										AbstrusePackageController Class
//
/// <summary>
/// Manages package events and settings. This is the PackageController base class. This abstract class deals
/// specifically with event handlers.
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


		if (!ThreadHelper.CheckAccess())
		{
			// Fire and forget

			_ = Task.Factory.StartNew(
				async () =>
				{
					await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
					Initialize();
				},
				default, TaskCreationOptions.PreferFairness, TaskScheduler.Default);
		}
		else
		{
			Initialize();
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
				if (IdeShutdownState)
					return null;

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



	protected abstract void Initialize();


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// AbstrusePackageController disposal
	/// </summary>
	// ---------------------------------------------------------------------------------
	public virtual void Dispose()
	{
		Dispose(true);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposing)
			return;

		UnadviseEvents(true);
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - AbstrusePackageController
	// =========================================================================================================


	private static bool _IdeShutdownState = false;

	private int _EventProjectCardinal = -2;
	private static int _EventRdtCardinal = 0;

	private static bool _SolutionLoaded = false;


	private int _RefLoadCnt = 0;
	private int _RefOpenCnt = 0;

	protected static IBsPackageController _Instance = null;

	// A package controller instance lock
	protected readonly object _LockObject = new object();


	private IDictionary<VSProject, ReferencesEvents> _ProjectReferencesEvents = null;
	private IDictionary<VSProject, BuildManagerEvents> _ProjectBuildManagerEvents = null;


	private _dispReferencesEvents_ReferenceAddedEventHandler _ReferenceAddedEventHandler = null;
	private _dispReferencesEvents_ReferenceRemovedEventHandler _ReferenceRemovedEventHandler = null;
	private _dispReferencesEvents_ReferenceChangedEventHandler _ReferenceChangedEventHandler = null;
	private _dispBuildManagerEvents_DesignTimeOutputDeletedEventHandler _DesignTimeOutputDeletedEventHandler = null;
	private _dispBuildManagerEvents_DesignTimeOutputDirtyEventHandler _DesignTimeOutputDirtyEventHandler = null;


	// System Events Delegates
	protected IBsPackageController.InitializeDelegate _OnInitializeEvent;
	private IBsPackageController.AssemblyObsoleteDelegate _OnAssemblyObsoleteEvent;
	private IBsPackageController.BuildDoneDelegate _OnBuildDoneEvent;


	private IBsPackageController.AfterAttributeChangeDelegate _OnAfterAttributeChangeEvent;
	private IBsPackageController.AfterAttributeChangeExDelegate _OnAfterAttributeChangeExEvent;
	private IBsPackageController.AfterDocumentWindowHideDelegate _OnAfterDocumentWindowHideEvent;
	private IBsPackageController.AfterLastDocumentUnlockDelegate _OnAfterLastDocumentUnlockEvent;
	private IBsPackageController.AfterSaveDelegate _OnAfterSaveEvent;
	private IBsPackageController.AfterSaveAsyncDelegate _OnAfterSaveAsyncEvent;
	private IBsPackageController.BeforeDocumentWindowShowDelegate _OnBeforeDocumentWindowShowEvent;
	private IBsPackageController.BeforeLastDocumentUnlockDelegate _OnBeforeLastDocumentUnlockEvent;
	private IBsPackageController.BeforeSaveDelegate _OnBeforeSaveEvent;
	private IBsPackageController.BeforeSaveAsyncDelegate _OnBeforeSaveAsyncEvent;

	private IBsPackageController.AfterCloseSolutionDelegate _OnAfterCloseSolutionEvent;
	private IBsPackageController.AfterLoadProjectDelegate _OnAfterLoadProjectEvent;
	private IBsPackageController.AfterOpenProjectDelegate _OnAfterOpenProjectEvent;
	private IBsPackageController.AfterOpenSolutionDelegate _OnAfterOpenSolutionEvent;
	private IBsPackageController.BeforeCloseProjectDelegate _OnBeforeCloseProjectEvent;
	private IBsPackageController.BeforeCloseSolutionDelegate _OnBeforeCloseSolutionEvent;
	private IBsPackageController.LoadSolutionOptionsDelegate _OnLoadSolutionOptionsEvent;
	private IBsPackageController.SaveSolutionOptionsDelegate _OnSaveSolutionOptionsEvent;
	private IBsPackageController.QueryCloseProjectDelegate _OnQueryCloseProjectEvent;
	private IBsPackageController.QueryCloseSolutionDelegate _OnQueryCloseSolutionEvent;

	// Selection Events Delegates
	private IBsPackageController.SelectionChangedDelegate _OnSelectionChangedEvent;
	private IBsPackageController.ElementValueChangedDelegate _OnElementValueChangedEvent;
	private IBsPackageController.CmdUIContextChangedDelegate _OnCmdUIContextChangedEvent;

	// Project Events Delegates
	private IBsPackageController.DesignTimeOutputDeletedDelegate _OnDesignTimeOutputDeletedEvent;
	private IBsPackageController.DesignTimeOutputDirtyDelegate _OnDesignTimeOutputDirtyEvent;

	private IBsPackageController.ProjectInitializedDelegate _OnProjectInitializedEvent;

	private IBsPackageController.ProjectItemAddedDelegate _OnProjectItemAddedEvent;
	private IBsPackageController.ProjectItemRemovedDelegate _OnProjectItemRemovedEvent;
	private IBsPackageController.ProjectItemRenamedDelegate _OnProjectItemRenamedEvent;

	private IBsPackageController.ReferenceAddedDelegate _OnReferenceAddedEvent;
	private IBsPackageController.ReferenceChangedDelegate _OnReferenceChangedEvent;
	private IBsPackageController.ReferenceRemovedDelegate _OnReferenceRemovedEvent;


	// Custom events Delegates
	private IBsPackageController.NewQueryRequestedDelegate _OnNewQueryRequestedEvent;


	#endregion Fields





	// =========================================================================================================
	#region Property Accessors - AbstrusePackageController
	// =========================================================================================================


	public static bool IdeShutdownState => _IdeShutdownState;

	public abstract DTE Dte { get; }
	public abstract DTE2 Dte2 { get; }
	public abstract string ProviderGuid { get; }
	public abstract bool SolutionValidating { get; }
	public abstract bool IsCmdLineBuild { get; }
	public abstract bool IsToolboxInitialized { get; }
	public abstract IBsAsyncPackage PackageInstance { get; }
	public abstract IVsMonitorSelection SelectionMonitor { get; }
	public abstract IAsyncServiceContainer Services { get; }
	public abstract bool SolutionClosing { get; }
	public abstract Solution SolutionObject { get; }
	public abstract Projects SolutionProjects { get; }
	public abstract IVsTaskStatusCenterService StatusCenterService { get; }
	public abstract uint ToolboxCmdUICookie { get; }


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
	/// Accessor to the <see cref="IVsSolutionEvents.OnAfterLoadProject"/> event.
	/// </summary>
	event IBsPackageController.AfterLoadProjectDelegate IBsPackageController.OnAfterLoadProjectEvent
	{
		add { _OnAfterLoadProjectEvent += value; }
		remove { _OnAfterLoadProjectEvent -= value; }
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
	/// Accessor to the <see cref="IVsRunningDocTableEvents4.OnAfterLastDocumentUnlock"/> event.
	/// </summary>
	event IBsPackageController.AfterLastDocumentUnlockDelegate IBsPackageController.OnAfterLastDocumentUnlockEvent
	{
		add { _OnAfterLastDocumentUnlockEvent += value; }
		remove { _OnAfterLastDocumentUnlockEvent -= value; }
	}


	/// <summary>
	/// Accessor to the <see cref="IVsSolutionEvents.OnAfterOpenSolution"/> event.
	/// </summary>
	event IBsPackageController.AfterOpenSolutionDelegate IBsPackageController.OnAfterOpenSolutionEvent
	{
		add { _OnAfterOpenSolutionEvent += value; }
		remove { _OnAfterOpenSolutionEvent -= value; }
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
	/// Accessor to the <see cref="OnInitialize"/> event.
	/// </summary>
	event IBsPackageController.InitializeDelegate IBsPackageController.OnInitializeEvent
	{
		add { _OnInitializeEvent += value; }
		remove { _OnInitializeEvent -= value; }
	}


	/// <summary>
	/// Accessor to the <see cref="DynamicTypeService.AssemblyObsolete"/> event.
	/// </summary>
	event IBsPackageController.AssemblyObsoleteDelegate IBsPackageController.OnAssemblyObsoleteEvent
	{
		add { _OnAssemblyObsoleteEvent += value; }
		remove { _OnAssemblyObsoleteEvent -= value; }
	}


	/// <summary>
	/// Accessor to the <see cref="BuildEvents.OnBuildDone"/> event.
	/// </summary>
	event IBsPackageController.BuildDoneDelegate IBsPackageController.OnBuildDoneEvent
	{
		add { _OnBuildDoneEvent += value; }
		remove { _OnBuildDoneEvent -= value; }
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
	/// Accessor to the <see cref="BuildManagerEvents.DesignTimeOutputDeleted"/> event.
	/// </summary>
	event IBsPackageController.DesignTimeOutputDeletedDelegate IBsPackageController.OnDesignTimeOutputDeletedEvent
	{
		add { _OnDesignTimeOutputDeletedEvent += value; }
		remove { _OnDesignTimeOutputDeletedEvent -= value; }
	}


	/// <summary>
	/// Accessor to the <see cref="BuildManagerEvents.DesignTimeOutputDirty"/> event.
	/// </summary>
	event IBsPackageController.DesignTimeOutputDirtyDelegate IBsPackageController.OnDesignTimeOutputDirtyEvent
	{
		add { _OnDesignTimeOutputDirtyEvent += value; }
		remove { _OnDesignTimeOutputDirtyEvent -= value; }
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
	/// Accessor to the <see cref="OnProjectInitializedEvent"/> event. This event is guaranteed to be
	/// fired once, but only once, for all Visible design time projects excluding the Misc project,
	/// irrelevant of whether or not the extension package was initialized and active at the time the
	/// project was loaded or opened.
	/// </summary>
	event IBsPackageController.ProjectInitializedDelegate IBsPackageController.OnProjectInitializedEvent
	{
		add { _OnProjectInitializedEvent += value; }
		remove { _OnProjectInitializedEvent -= value; }
	}


	/// <summary>
	/// Accessor to the <see cref="ProjectItemsEvents.ItemAdded"/> event.
	/// </summary>
	event IBsPackageController.ProjectItemAddedDelegate IBsPackageController.OnProjectItemAddedEvent
	{
		add { _OnProjectItemAddedEvent += value; }
		remove { _OnProjectItemAddedEvent -= value; }
	}


	/// <summary>
	/// Accessor to the <see cref="ProjectItemsEvents.ItemRemoved"/> event.
	/// </summary>
	event IBsPackageController.ProjectItemRemovedDelegate IBsPackageController.OnProjectItemRemovedEvent
	{
		add { _OnProjectItemRemovedEvent += value; }
		remove { _OnProjectItemRemovedEvent -= value; }
	}


	/// <summary>
	/// Accessor to the <see cref="ProjectItemsEvents.ItemRenamed"/> event.
	/// </summary>
	event IBsPackageController.ProjectItemRenamedDelegate IBsPackageController.OnProjectItemRenamedEvent
	{
		add { _OnProjectItemRenamedEvent += value; }
		remove { _OnProjectItemRenamedEvent -= value; }
	}


	/// <summary>
	/// Accessor to the <see cref="_dispReferencesEvents_Event.ReferenceAdded"/> event.
	/// </summary>
	event IBsPackageController.ReferenceAddedDelegate IBsPackageController.OnReferenceAddedEvent
	{
		add { _OnReferenceAddedEvent += value; }
		remove { _OnReferenceAddedEvent -= value; }
	}


	/// <summary>
	/// Accessor to the <see cref="_dispReferencesEvents_Event.ReferenceChanged"/> event.
	/// </summary>
	event IBsPackageController.ReferenceChangedDelegate IBsPackageController.OnReferenceChangedEvent
	{
		add { _OnReferenceChangedEvent += value; }
		remove { _OnReferenceChangedEvent -= value; }
	}


	/// <summary>
	/// Accessor to the <see cref="_dispReferencesEvents_Event.ReferenceRemoved"/> event.
	/// </summary>
	event IBsPackageController.ReferenceRemovedDelegate IBsPackageController.OnReferenceRemovedEvent
	{
		add { _OnReferenceRemovedEvent += value; }
		remove { _OnReferenceRemovedEvent -= value; }
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


	/// <summary>
	/// <see cref="ErrorHandler.ThrowOnFailure"/> token.
	/// </summary>
	protected static int ___(int hr) => ErrorHandler.ThrowOnFailure(hr);



	/// <summary>
	/// <see cref="ErrorHandler.Succeeded"/> token.
	/// </summary>
	protected static bool __(int hr) => ErrorHandler.Succeeded(hr);


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Adds event handlers for project events that reset the ITypeResolutionService
	/// cache, so that we can rebuild the type references to the
	/// EntityFramework.FirebirdClient assembly that ships with the extension.
	/// This is to prevent invalid cast exceptions where project EntityFramework
	/// versions differ.
	/// </summary>
	/// <remarks>
	/// There are too many caveats in establicsing if a project's target frameworks
	/// include .NetFramework, so all we care about is that it's a user project and it's
	/// Object property is of type VSProject. The cache for a project will only be
	/// updated if it references EntityFramework.Firebird.
	/// </remarks>
	// ---------------------------------------------------------------------------------
	[SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification = "Caller must check")]
	private void AddProjectEventHandlers(Project project)
	{
		// Tracer.Trace(GetType(), "AddProjectEventHandlers()", "CALLING ReindexEntityFrameworkAssemblies() for project: {0}.", project.Name);

		if (!project.IsEditable())
			return;

		VSProject projectObject = project.Object as VSProject;

		if (_ProjectReferencesEvents != null && _ProjectReferencesEvents.Count != 0 && _ProjectReferencesEvents.ContainsKey(projectObject))
			return;

		try
		{
			_ReferenceAddedEventHandler ??= new _dispReferencesEvents_ReferenceAddedEventHandler(OnReferenceAdded);
			_ReferenceRemovedEventHandler ??= new _dispReferencesEvents_ReferenceRemovedEventHandler(OnReferenceRemoved);
			_ReferenceChangedEventHandler ??= new _dispReferencesEvents_ReferenceChangedEventHandler(OnReferenceChanged);
			_DesignTimeOutputDeletedEventHandler ??= new _dispBuildManagerEvents_DesignTimeOutputDeletedEventHandler(OnDesignTimeOutputDeleted);
			_DesignTimeOutputDirtyEventHandler ??= new _dispBuildManagerEvents_DesignTimeOutputDirtyEventHandler(OnDesignTimeOutputDirty);

			// Reference and BuildManager events get lost if we don't maintain a reference to them.

			_ProjectReferencesEvents ??= new Dictionary<VSProject, ReferencesEvents>();
			_ProjectBuildManagerEvents ??= new Dictionary<VSProject, BuildManagerEvents>();

			ReferencesEvents referenceEvents = projectObject.Events.ReferencesEvents;
			_ProjectReferencesEvents[projectObject] = referenceEvents;

			BuildManagerEvents buildManagerEvents = projectObject.Events.BuildManagerEvents;
			_ProjectBuildManagerEvents[projectObject] = buildManagerEvents;


			referenceEvents.ReferenceAdded += _ReferenceAddedEventHandler;
			referenceEvents.ReferenceRemoved += _ReferenceRemovedEventHandler;
			referenceEvents.ReferenceChanged += _ReferenceChangedEventHandler;
			buildManagerEvents.DesignTimeOutputDeleted += _DesignTimeOutputDeletedEventHandler;
			buildManagerEvents.DesignTimeOutputDirty += _DesignTimeOutputDirtyEventHandler;

			_OnProjectInitializedEvent?.Invoke(project);


			// Tracer.Trace(GetType(), "AddProjectEventHandlers()", "Added event handlers for project: {0}.", project.Name);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;
		}
	}


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



	public abstract string CreateConnectionUrl(string connectionString);
	public abstract string GetRegisterConnectionDatasetKey(IVsDataExplorerConnection root);
	public abstract void InvalidateRctManager();
	public abstract bool IsConnectionEquivalency(string connectionString1, string connectionString2);
	public abstract bool IsWeakConnectionEquivalency(string connectionString1, string connectionString2);
	public abstract TInterface EnsureService<TService, TInterface>() where TInterface : class;



	private bool EventProjectEnter(bool increment = true, bool force = false)
	{
		lock (_LockObject)
		{
			// Tracer.Trace(GetType(), "EventProjectEnter()", "_EventProjectCardinal: {0}, increment: {1}.", _EventProjectCardinal, increment);

			if ((_EventProjectCardinal != 0 && !force) || SolutionObject == null)
				return false;

			if (increment)
				_EventProjectCardinal++;
		}

		return true;
	}



	private void EventProjectExit()
	{
		lock (_LockObject)
		{
			// Tracer.Trace(GetType(), "EventProjectExit()", "_EventProjectCardinal: {0}.", _EventProjectCardinal);

			if (_EventProjectCardinal <= 0)
			{
				ApplicationException ex = new($"Attempt to exit project event when not in a project event. _EventProjectCardinal: {_EventProjectCardinal}");
				Diag.Dug(ex);
				throw ex;
			}
			_EventProjectCardinal--;
		}
	}


	protected bool EventProjectRegistrationEnter(bool increment = true)
	{
		lock (_LockObject)
		{
			// Tracer.Trace(GetType(), "EventProjectRegisterEnter()", "_EventProjectCardinal: {0}, increment: {1}.", _EventProjectCardinal, increment);

			if (_EventProjectCardinal != -2 || SolutionObject == null)
				return false;

			if (increment)
				_EventProjectCardinal++;
		}

		return true;
	}



	protected void EventProjectRegistrationExit()
	{
		lock (_LockObject)
		{
			// Tracer.Trace(GetType(), "EventProjectRegisterExit()", "_EventProjectCardinal: {0}.", _EventProjectCardinal);

			if (_EventProjectCardinal != -1)
			{
				ApplicationException ex = new("Attempt to exit project event registration when events were not being registered.");
				Diag.Dug(ex);
				throw ex;
			}
			_EventProjectCardinal = 0;
		}
	}


	protected void EventProjectDeregister()
	{
		// Tracer.Trace(GetType(), "EventProjectDeregister()", "_EventProjectCardinal: {0}.", _EventProjectCardinal);

		lock (_LockObject)
			_EventProjectCardinal = -2;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Increments the <see cref="_EventRdtCardinal"/> counter when execution enters
	/// an Rdt event handler to prevent recursion
	/// </summary>
	// ---------------------------------------------------------------------------------
	public bool EventRdtEnter(bool increment = true, bool force = false)
	{
		lock (_LockObject)
		{
			if ((_EventRdtCardinal > 0 || IdeShutdownState) && !force)
				return false;

			if (increment)
				_EventRdtCardinal++;
		}

		return true;
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Decrements the <see cref="RdtEventsDisabled"/> counter that was previously
	/// incremented by <see cref="DisableRdtEvents"/>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public void EventRdtExit()
	{
		lock (_LockObject)
		{
			if (_EventRdtCardinal <= 0)
			{
				ApplicationException ex = new($"Attempt to exit RDT event when not in an Rdt event. _EventRdtCardinal: {_EventRdtCardinal}");
				Diag.Dug(ex);
				throw ex;
			}
			_EventRdtCardinal--;
		}
	}



	public TInterface GetService<TInterface>() where TInterface : class
		=> GetService<TInterface, TInterface>();


	public TInterface GetService<TService, TInterface>() where TInterface : class
		=> PackageInstance.GetService<TService, TInterface>();


	public async Task<TInterface> GetServiceAsync<TInterface>() where TInterface : class
		=> await GetServiceAsync<TInterface, TInterface>();


	public async Task<TInterface> GetServiceAsync<TService, TInterface>() where TInterface : class
		=> await PackageInstance.GetServiceAsync<TService, TInterface>();


	protected abstract void InternalShutdownDte();


	protected void RegisterProjectEventHandlers()
	{
		if (!EventProjectRegistrationEnter())
			return;

		if (!ThreadHelper.CheckAccess())
		{
			// Fire and forget async

			_ = Task.Factory.StartNew(
				async () =>
				{
					await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

					try
					{
						List<Project> projects = UnsafeCmd.RecursiveGetDesignTimeProjects();

						// Tracer.Trace(GetType(), "AdviseEvents()", "Adding event handlers for {0} EF projects", projects.Count);
						foreach (Project project in projects)
							AddProjectEventHandlers(project);
					}
					finally
					{
						EventProjectRegistrationExit();
					}
				},
				default, TaskCreationOptions.PreferFairness, TaskScheduler.Default);
		}
		else
		{
			try
			{
				List<Project> projects = UnsafeCmd.RecursiveGetDesignTimeProjects();

				// Tracer.Trace(GetType(), "AdviseEvents()", "Adding event handlers for {0} EF projects", projects.Count);
				foreach (Project project in projects)
					AddProjectEventHandlers(project);
			}
			finally
			{
				EventProjectRegistrationExit();
			}
		}
	}



	public static void ShutdownDte()
	{
		((AbstrusePackageController)_Instance)?.InternalShutdownDte();

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
	#region General and Type Resolution Event handling - AbstrusePackageController
	// =========================================================================================================


	protected void OnAssemblyObsolete(object sender, AssemblyObsoleteEventArgs e) =>
		_OnAssemblyObsoleteEvent?.Invoke(sender, e);



	protected void OnBeginShutdown() => ShutdownDte();



	protected void OnBuildDone(vsBuildScope scope, vsBuildAction action) =>
		_OnBuildDoneEvent?.Invoke(scope, action);



	private void OnDesignTimeOutputDeleted(string bstrOutputMoniker) =>
		_OnDesignTimeOutputDeletedEvent?.Invoke(bstrOutputMoniker);



	void OnDesignTimeOutputDirty(string bstrOutputMoniker) =>
		_OnDesignTimeOutputDirtyEvent?.Invoke(bstrOutputMoniker);



	public int OnNewQueryRequested(IVsDataViewHierarchy site, EnNodeSystemType nodeSystemType) =>
		_OnNewQueryRequestedEvent?.Invoke(site, nodeSystemType) ?? VSConstants.E_NOTIMPL;



	[SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification = "Caller must check")]
	protected void OnProjectItemAdded(ProjectItem projectItem)
	{
		// Tracer.Trace(typeof(AbstrusePackageController), "OnProjectItemAdded()", projectItem.ContainingProject.Name);

		if (_OnProjectItemAddedEvent == null || !projectItem.ContainingProject.IsEditable())
		{
			return;
		}

		_OnProjectItemAddedEvent.Invoke(projectItem);
	}



	[SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification = "Caller must check")]
	protected void OnProjectItemRemoved(ProjectItem projectItem)
	{
		// Tracer.Trace(typeof(AbstrusePackageController), "OnProjectItemRemoved()", projectItem.ContainingProject.Name);

		if (_OnProjectItemRemovedEvent == null || !projectItem.ContainingProject.IsEditable())
		{
			return;
		}

		_OnProjectItemRemovedEvent.Invoke(projectItem);
	}




	[SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification = "Caller must check")]
	protected void OnProjectItemRenamed(ProjectItem projectItem, string oldName)
	{
		// Tracer.Trace(typeof(AbstrusePackageController), "OnProjectItemRenamed()", projectItem.ContainingProject.Name);

		if (_OnProjectItemRenamedEvent == null || !projectItem.ContainingProject.IsEditable())
		{
			return;
		}

		_OnProjectItemRenamedEvent.Invoke(projectItem, oldName);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Event handler for the Project
	/// <see cref="_dispReferencesEvents_Event.ReferenceAdded"/> event
	/// </summary>
	/// <param name="reference"></param>
	// ---------------------------------------------------------------------------------
	private void OnReferenceAdded(Reference reference)
	{
		// Tracer.Trace(typeof(AbstrusePackageController), "OnReferenceAdded()", reference.ContainingProject.Name);

		if (_OnReferenceAddedEvent == null || !reference.ContainingProject.IsEditable())
		{
			return;
		}

		_OnReferenceAddedEvent.Invoke(reference);
	}



	private void OnReferenceChanged(Reference reference)
	{
		if (_OnReferenceChangedEvent == null || !reference.ContainingProject.IsEditable())
		{
			return;
		}

		_OnReferenceChangedEvent.Invoke(reference);
	}



	private void OnReferenceRemoved(Reference reference)
	{
		// Tracer.Trace(typeof(AbstrusePackageController), "OnReferenceRemoved()", reference.ContainingProject.Name);

		if (_OnReferenceRemovedEvent == null || !reference.ContainingProject.IsEditable())
		{
			return;
		}

		_OnReferenceRemovedEvent.Invoke(reference);
	}


	#endregion General and Type Resolution Event handling





	// =========================================================================================================
	#region IVsSolutionEvents/2/3 Implementation and Event handling - AbstrusePackageController
	// =========================================================================================================


	// Events that we handle are listed first





	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Implements <see cref="IVsSolutionEvents.OnAfterCloseSolution"/>,
	/// <see cref="IVsSolutionEvents2.OnAfterCloseSolution"/> and
	/// <see cref="IVsSolutionEvents3.OnAfterCloseSolution"/>
	/// </summary>
	// ---------------------------------------------------------------------------------
	public int OnAfterCloseSolution(object pUnkReserved)
	{
		// Tracer.Trace(typeof(AbstrusePackageController), "OnAfterCloseSolution()");

		_SolutionLoaded = false;
		_ProjectReferencesEvents = null;
		_ProjectBuildManagerEvents = null;

		EventProjectDeregister();

		return _OnAfterCloseSolutionEvent?.Invoke(pUnkReserved) ?? VSConstants.S_OK;
	}



	[SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification = "Caller must check")]
	public int OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
	{
		// Tracer.Trace(typeof(AbstrusePackageController), "OnAfterLoadProject()");

		Project project = pRealHierarchy.ToEditableProject();

		if (project == null)
			return VSConstants.S_OK;

		if (project.Properties == null)
		{
			_ = Task.Factory.StartNew(async () => await OnAfterLoadProjectAsync(project),
				default, TaskCreationOptions.PreferFairness, TaskScheduler.Default);

			return VSConstants.S_OK;
		}



		if (_ProjectReferencesEvents == null || _ProjectReferencesEvents.Count == 0
			|| !_ProjectReferencesEvents.ContainsKey(project.Object as VSProject))
		{
			_ = Task.Factory.StartNew(
				async () =>
				{
					await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

					if (EventProjectEnter(true, true))
					{
						try
						{
							AddProjectEventHandlers(project);
						}
						finally
						{
							EventProjectExit();
						}
					}

					_OnAfterLoadProjectEvent?.Invoke(project);
				},
				default, TaskCreationOptions.PreferFairness, TaskScheduler.Default);

			return VSConstants.S_OK;
		}


		return _OnAfterLoadProjectEvent?.Invoke(project) ?? VSConstants.S_OK;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Ensure project is fully loaded.
	/// </summary>
	/// <param name="project"></param>
	/// <returns></returns>
	// ---------------------------------------------------------------------------------
	private async Task OnAfterLoadProjectAsync(Project project)
	{
		await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

		// Recycle until project object is complete if necessary
		if (project.Properties == null)
		{
			if (++_RefLoadCnt < 100)
			{
				await Task.Delay(50);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
				ThreadHelper.JoinableTaskFactory.RunAsync(() => OnAfterLoadProjectAsync(project));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

				return;
			}

			Diag.Dug(new ApplicationException($"Time out waiting for project properties to load: {project.Name}."));

			return;
		}

		// Tracer.Trace("Project opened");
		// If anything gets through things are still happening so we can reset and allow events with incomplete project objects
		// to continue recycling
		_RefLoadCnt = 0;



		if (EventProjectEnter(true, true))
		{
			try
			{
				AddProjectEventHandlers(project);
			}
			finally
			{
				EventProjectExit();
			}
		}

		_OnAfterLoadProjectEvent?.Invoke(project);
	}



	/// <summary>
	/// Implements <see cref="IVsSolutionEvents.OnAfterOpenProject"/>,
	/// <see cref="IVsSolutionEvents2.OnAfterOpenProject"/> and <see cref="IVsSolutionEvents3.OnAfterOpenProject"/>
	/// </summary>
	[SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification = "Caller must check")]
	public int OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
	{
		// Tracer.Trace(typeof(AbstrusePackageController), "OnAfterOpenProject()");

		Project project = pHierarchy.ToEditableProject();

		if (project == null)
			return VSConstants.S_OK;

		if (project.Properties == null)
		{
			_ = Task.Factory.StartNew(async () => await OnAfterOpenProjectAsync(project, fAdded),
				default, TaskCreationOptions.PreferFairness, TaskScheduler.Default);

			return VSConstants.S_OK;
		}


		if (_ProjectReferencesEvents == null || _ProjectReferencesEvents.Count == 0
			|| !_ProjectReferencesEvents.ContainsKey(project.Object as VSProject))
		{
			_ = Task.Factory.StartNew(
				async () =>
				{
					await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

					if (EventProjectEnter(true, true))
					{
						try
						{
							AddProjectEventHandlers(project);
						}
						finally
						{
							EventProjectExit();
						}
					}

					_OnAfterOpenProjectEvent?.Invoke(project, fAdded);
				},
				default, TaskCreationOptions.PreferFairness, TaskScheduler.Default);

			return VSConstants.S_OK;
		}


		return _OnAfterOpenProjectEvent?.Invoke(project, fAdded) ?? VSConstants.S_OK;

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
			if (++_RefOpenCnt < 100)
			{
				await Task.Delay(50);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
				ThreadHelper.JoinableTaskFactory.RunAsync(() => OnAfterOpenProjectAsync(project, fAdded));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

				return;
			}

			Diag.Dug(new ApplicationException($"Time out waiting for project properties to load: {project.Name}."));

			return;
		}

		// Tracer.Trace("Project opened");
		// If anything gets through things are still happening so we can reset and allow events with incomplete project objects
		// to continue recycling
		_RefOpenCnt = 0;

		if (EventProjectEnter(true, true))
		{
			try
			{
				AddProjectEventHandlers(project);
			}
			finally
			{
				EventProjectExit();
			}
		}

		_OnAfterOpenProjectEvent?.Invoke(project, fAdded);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Implements <see cref="IVsSolutionEvents.OnAfterOpenSolution"/>,
	/// <see cref="IVsSolutionEvents2.OnAfterOpenSolution"/> and
	/// <see cref="IVsSolutionEvents3.OnAfterOpenSolution"/>
	/// </summary>
	// ---------------------------------------------------------------------------------
	public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution) =>
		_OnAfterOpenSolutionEvent?.Invoke(pUnkReserved, fNewSolution) ?? VSConstants.E_NOTIMPL;



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Implements <see cref="IVsSolutionEvents.OnBeforeCloseProject"/>,
	/// <see cref="IVsSolutionEvents2.OnBeforeCloseProject"/>
	/// and <see cref="IVsSolutionEvents3.OnBeforeCloseProject"/>
	/// </summary>
	// ---------------------------------------------------------------------------------
	[SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification = "Caller must check")]
	public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
	{
		// Tracer.Trace(typeof(AbstrusePackageController), "OnBeforeCloseProject()", "Guid: {0}.", pHierarchy.ProjectKind());

		if (!pHierarchy.IsDesignTimeProject())
			return VSConstants.S_OK;

		// Tracer.Trace(GetType(), "OnBeforeCloseProject()", pHierarchy.ToProject().Name);

		_OnBeforeCloseProjectEvent?.Invoke(pHierarchy, fRemoved);

		if (_ProjectReferencesEvents == null || _ProjectReferencesEvents.Count == 0)
			return VSConstants.S_OK;

		Project project = pHierarchy.ToEditableProject();

		if (project == null)
			return VSConstants.S_OK;

		_ProjectReferencesEvents.Remove(project.Object as VSProject);
		_ProjectBuildManagerEvents.Remove(project.Object as VSProject);

		return VSConstants.S_OK;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Implements <see cref="IVsSolutionEvents.OnBeforeCloseSolution"/>,
	/// <see cref="IVsSolutionEvents2.OnBeforeCloseSolution"/> and
	/// <see cref="IVsSolutionEvents3.OnBeforeCloseSolution"/>
	/// </summary>
	// ---------------------------------------------------------------------------------
	public int OnBeforeCloseSolution(object pUnkReserved) => 
		_OnBeforeCloseSolutionEvent?.Invoke(pUnkReserved) ?? VSConstants.E_NOTIMPL;




	[SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification = "Caller must check")]
	public int OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
	{
		// Tracer.Trace(typeof(AbstrusePackageController), "OnBeforeUnloadProject()");

		if (_ProjectReferencesEvents == null || _ProjectReferencesEvents.Count == 0)
			return VSConstants.S_OK;

		Project project = pRealHierarchy.ToEditableProject();

		if (project == null)
			return VSConstants.S_OK;

		_ProjectReferencesEvents.Remove(project.Object as VSProject);
		_ProjectBuildManagerEvents.Remove(project.Object as VSProject);

		return VSConstants.S_OK;
	}



	public void OnLoadSolutionOptions(Stream stream)
	{
		// Tracer.Trace(GetType(), "OnLoadSolutionOptions()");

		RegisterProjectEventHandlers();


		if (_SolutionLoaded)
			return;

		_SolutionLoaded = true;

		_OnLoadSolutionOptionsEvent?.Invoke(stream);

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Implements <see cref="IVsSolutionEvents.OnQueryCloseProject"/>,
	/// <see cref="IVsSolutionEvents2.OnQueryCloseProject"/> and
	/// <see cref="IVsSolutionEvents3.OnQueryCloseProject"/>
	/// </summary>
	// ---------------------------------------------------------------------------------
	public int OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
	{
		// Tracer.Trace(typeof(AbstrusePackageController), "OnQueryCloseProject()", "Guid: {0}.", pHierarchy.ProjectKind());

		if (_OnQueryCloseProjectEvent == null || !pHierarchy.IsDesignTimeProject())
		{
			return VSConstants.S_OK;
		}

		return _OnQueryCloseProjectEvent(pHierarchy, fRemoving, ref pfCancel);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Implements <see cref="IVsSolutionEvents.OnQueryCloseSolution"/>,
	/// <see cref="IVsSolutionEvents2.OnQueryCloseSolution"/> and
	/// <see cref="IVsSolutionEvents3.OnQueryCloseSolution"/>
	/// </summary>
	// ---------------------------------------------------------------------------------
	public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel) => 
		_OnQueryCloseSolutionEvent?.Invoke(pUnkReserved, ref pfCancel) ?? VSConstants.E_NOTIMPL;




	public void OnSaveSolutionOptions(Stream stream) =>
		_OnSaveSolutionOptionsEvent?.Invoke(stream);





	// Unhandled events follow

	int IVsSolutionEvents3.OnAfterClosingChildren(IVsHierarchy hierarchy) => VSConstants.E_NOTIMPL;
	public int OnAfterMergeSolution(object pUnkReserved) => VSConstants.E_NOTIMPL;
	int IVsSolutionEvents3.OnAfterOpeningChildren(IVsHierarchy hierarchy) => VSConstants.E_NOTIMPL;
	int IVsSolutionEvents3.OnBeforeClosingChildren(IVsHierarchy hierarchy) => VSConstants.E_NOTIMPL;
	int IVsSolutionEvents3.OnBeforeOpeningChildren(IVsHierarchy hierarchy) => VSConstants.E_NOTIMPL;
	public int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel) => VSConstants.E_NOTIMPL;


	#endregion IVsSolutionEvents Implementation and Event handling





	// =========================================================================================================
	#region IVsRunningDocTableEvents Implementation and Event handling - AbstrusePackageController
	// =========================================================================================================


	// Events that we handle are listed first


	public int OnAfterAttributeChange(uint docCookie, uint grfAttribs) => 
		_OnAfterAttributeChangeEvent?.Invoke(docCookie, grfAttribs) ?? VSConstants.S_OK;


	public int OnAfterAttributeChangeEx(uint docCookie, uint grfAttribs, IVsHierarchy pHierOld, uint itemidOld,
		string pszMkDocumentOld, IVsHierarchy pHierNew, uint itemidNew, string pszMkDocumentNew)
	{
		if (_OnAfterAttributeChangeExEvent == null)
			return VSConstants.S_OK;

		// Fire and wait.

		if (!ThreadHelper.CheckAccess())
		{
			return ThreadHelper.JoinableTaskFactory.Run(async delegate
			{
				await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

				return _OnAfterAttributeChangeExEvent(docCookie, grfAttribs, pHierOld, itemidOld,
					pszMkDocumentOld, pHierNew, itemidNew, pszMkDocumentNew);
			});
		}

		return _OnAfterAttributeChangeExEvent(docCookie, grfAttribs, pHierOld, itemidOld,
			pszMkDocumentOld, pHierNew, itemidNew, pszMkDocumentNew);
	}



	public int OnAfterDocumentWindowHide(uint docCookie, IVsWindowFrame pFrame)
	{
		if (_OnAfterDocumentWindowHideEvent == null)
			return VSConstants.S_OK;

		// Fire and wait.

		if (!ThreadHelper.CheckAccess())
		{
			return ThreadHelper.JoinableTaskFactory.Run(async delegate
			{
				await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

				return _OnAfterDocumentWindowHideEvent(docCookie, pFrame);
			});
		}

		return _OnAfterDocumentWindowHideEvent(docCookie, pFrame); 
	}



	public int OnAfterSave(uint docCookie) => 
		_OnAfterSaveEvent?.Invoke(docCookie) ?? VSConstants.E_NOTIMPL;



	int IVsRunningDocTableEvents4.OnAfterLastDocumentUnlock(IVsHierarchy pHier, uint itemid, string pszMkDocument,
			int fClosedWithoutSaving) => 
		_OnAfterLastDocumentUnlockEvent?.Invoke(pHier, itemid, pszMkDocument, fClosedWithoutSaving) ?? VSConstants.E_NOTIMPL;



	public IVsTask OnAfterSaveAsync(uint cookie, uint flags) => _OnAfterSaveAsyncEvent?.Invoke(cookie, flags);




	public int OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame) =>
		_OnBeforeDocumentWindowShowEvent?.Invoke(docCookie, fFirstShow, pFrame) ?? VSConstants.E_NOTIMPL;




	public int OnBeforeLastDocumentUnlock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining,
			uint dwEditLocksRemaining) =>
		_OnBeforeLastDocumentUnlockEvent?.Invoke(docCookie, dwRDTLockType, dwReadLocksRemaining, dwEditLocksRemaining)
			?? VSConstants.E_NOTIMPL;



	int IVsRunningDocTableEvents3.OnBeforeSave(uint docCookie) =>
		_OnBeforeSaveEvent?.Invoke(docCookie) ?? VSConstants.E_NOTIMPL;



	public IVsTask OnBeforeSaveAsync(uint cookie, uint flags, IVsTask saveTask) =>
		_OnBeforeSaveAsyncEvent?.Invoke(cookie, flags, saveTask);



	// Unhandled events follow

	public int OnAfterFirstDocumentLock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining,
		uint dwEditLocksRemaining) => VSConstants.E_NOTIMPL;
	int IVsRunningDocTableEvents4.OnAfterSaveAll() => VSConstants.E_NOTIMPL;
	int IVsRunningDocTableEvents4.OnBeforeFirstDocumentLock(IVsHierarchy pHier, uint itemid, string pszMkDocument) => VSConstants.E_NOTIMPL;


	#endregion IVsRunningDocTableEvents Implementation and Event handling





	// =========================================================================================================
	#region IVsSelectionEvents Implementation and Event handling - AbstrusePackageController
	// =========================================================================================================


	public int OnCmdUIContextChanged(uint dwCmdUICookie, int fActive) => _OnCmdUIContextChangedEvent != null
		? _OnCmdUIContextChangedEvent(dwCmdUICookie, fActive) : VSConstants.S_OK;


	public int OnElementValueChanged(uint elementid, object varValueOld, object varValueNew)
	{
		if (_OnElementValueChangedEvent == null)
			return VSConstants.S_OK;

		// Fire and wait.

		if (!ThreadHelper.CheckAccess())
		{
			return ThreadHelper.JoinableTaskFactory.Run(async delegate
			{
				await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

				return _OnElementValueChangedEvent(elementid, varValueOld, varValueNew);
			});
		}

		return _OnElementValueChangedEvent(elementid, varValueOld, varValueNew);
	}



	public int OnSelectionChanged(IVsHierarchy pHierOld, uint itemidOld, IVsMultiItemSelect pMISOld,
			ISelectionContainer pSCOld, IVsHierarchy pHierNew, uint itemidNew,
			IVsMultiItemSelect pMISNew, ISelectionContainer pSCNew) =>
		_OnSelectionChangedEvent?.Invoke(pHierOld, itemidOld, pMISOld, pSCOld, pHierNew, itemidNew, pMISNew, pSCNew)
			?? VSConstants.E_NOTIMPL;


	#endregion IVsSelectionEvents Implementation and Event handling


}
