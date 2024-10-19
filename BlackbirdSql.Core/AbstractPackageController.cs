// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using BlackbirdSql.Core.Ctl.Config;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Sys;
using BlackbirdSql.Sys.Interfaces;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Design;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TaskStatusCenter;

using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;



namespace BlackbirdSql.Core;


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
public abstract class AbstractPackageController : AbstrusePackageController
{


	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - AbstractPackageController
	// ---------------------------------------------------------------------------------


	/// <summary>
	/// Private singleton .ctor
	/// </summary>
	protected AbstractPackageController(IBsAsyncPackage ddex) : base(ddex)
	{
		_PackageInstance = ddex;
	}

	protected override void Dispose(bool disposing)
	{
		if (!disposing)
			return;

		base.Dispose(disposing);

		AbstractEventsManager.DisposeInstances();
	}


	protected override void Initialize()
	{
		_Dte = Package.GetGlobalService(typeof(DTE)) as DTE;

		if (_Dte is DTE { Events: not null } dte)
		{
			if (_DteEvents == null)
			{
				_DteEvents = dte.Events.DTEEvents;

				if (_DteEvents != null)
					_DteEvents.OnBeginShutdown += OnBeginShutdown;
			}

			if (_BuildEvents == null)
			{
				_BuildEvents = dte.Events.BuildEvents;

				if (_BuildEvents != null)
					_BuildEvents.OnBuildDone += OnBuildDone;
			}
		}

		if (_Dte is DTE2 { Events: not null } dte2)
		{
			if (_DynamicTypeService == null)
			{
				ServiceProvider serviceProvider = new((IOleServiceProvider)dte2);

				// Store a reference so that VS doesn't forget about us.
				_DynamicTypeService = serviceProvider.GetService(typeof(DynamicTypeService)) as DynamicTypeService;

				Diag.ThrowIfServiceUnavailable(_DynamicTypeService, typeof(DynamicTypeService));

				_DynamicTypeService.AssemblyObsolete += OnAssemblyObsolete;
			}

			if (_ProjectItemEvents == null)
			{
				// Evs.Debug(GetType(), "Initialize()", "_ProjectItemEvents registered.");

				// Store a reference so that VS doesn't forget about us.
				_ProjectItemEvents = ((Events2)dte2.Events).ProjectItemsEvents;

				if (_ProjectItemEvents != null)
				{
					_ProjectItemEvents.ItemRemoved += OnProjectItemRemoved;
					_ProjectItemEvents.ItemAdded += OnProjectItemAdded;
					_ProjectItemEvents.ItemRenamed += OnProjectItemRenamed;
				}
			}

		}
		else
		{
			throw new NullReferenceException("DTE2 is not initialized");
		}

	}


	#endregion Constructors / Destructors




	// =========================================================================================================
	#region Fields - AbstractPackageController
	// =========================================================================================================

	private readonly IBsAsyncPackage _PackageInstance;
	protected DTE _Dte = null;
	private IVsMonitorSelection _MonitorSelection = null;
	private IVsTaskStatusCenterService _StatusCenterService = null;

	private uint _ToolboxCmdUICookie;
	protected uint _HSolutionEvents = uint.MaxValue;
	protected uint _HDocTableEvents = uint.MaxValue;
	protected uint _HSelectionEvents = uint.MaxValue;

	private DTEEvents _DteEvents = null;
	private BuildEvents _BuildEvents = null;
	private ProjectItemsEvents _ProjectItemEvents = null;
	private DynamicTypeService _DynamicTypeService = null;


	#endregion Fields




	// =========================================================================================================
	#region Property Accessors - AbstractPackageController
	// =========================================================================================================


	public override DTE Dte
	{
		get
		{
			if (ApcManager.IdeShutdownState)
				return null;

			if (_Dte != null)
				return _Dte;

			_Dte = PackageInstance.GetService<_DTE, DTE>();

			if (_Dte == null)
				ShutdownDte();

			return _Dte;
		}
	}


	[SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification = "Caller must check")]
	public override DTE2 Dte2 => (DTE2)Dte;


	[SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification = "Caller must check")]
	public override bool IsCmdLineBuild
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


	[SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification = "Caller must check")]
	public override bool IsToolboxInitialized
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


	public override IBsAsyncPackage PackageInstance => _PackageInstance;


	public override string ProviderGuid => SystemData.C_ProviderGuid;


	public override IVsMonitorSelection SelectionMonitor
	{
		get
		{
			Diag.ThrowIfNotOnUIThread();

			if (_MonitorSelection == null)
				EnsureMonitorSelection();
			return _MonitorSelection;
		}
	}


	public override IAsyncServiceContainer Services => (IAsyncServiceContainer)_PackageInstance;


	[SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification = "<Pending>")]
	public override bool SolutionClosing
	{
		get
		{
			if (!ThreadHelper.CheckAccess())
				return false;

			try
			{
				if (!__(VsSolution.GetProperty((int)__VSPROPID.VSPROPID_SOLUTIONCLOSING, out object pvar)) || object.Equals(pvar, true))
				{
					return true;
				}
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				return true;
			}

			return false;
		}
	}


	[SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification = "Caller must check")]
	public override Solution SolutionObject => !IdeShutdownState ? Dte.Solution : null;


	public override Projects SolutionProjects => SolutionObject?.Projects;


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
	public override IVsTaskStatusCenterService StatusCenterService => _StatusCenterService
		??= GetService<SVsTaskStatusCenterService, IVsTaskStatusCenterService>();


	[SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification = "Caller must check")]
	public override uint ToolboxCmdUICookie
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


	protected override IVsSolution VsSolution => _PackageInstance.VsSolution;


	#endregion Property Accessors






	// =========================================================================================================
	#region Methods - AbstractPackageController
	// =========================================================================================================


	public override string CreateConnectionUrl(string connectionString) => Csb.CreateConnectionUrl(connectionString);

	public override string GetConnectionQualifiedName(string connectionString) => Csb.GetQualifiedName(connectionString);



	[SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification = "Caller must check")]
	private void EnsureMonitorSelection()
	{
		if (_MonitorSelection != null)
			return;

		Diag.ThrowIfNotOnUIThread();

		try
		{
			_MonitorSelection = Package.GetGlobalService(typeof(IVsMonitorSelection)) as IVsMonitorSelection;

			if (_MonitorSelection != null && !IsCmdLineBuild)
				___(_MonitorSelection.AdviseSelectionEvents(this, out _HSelectionEvents));
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
		}

		return;

	}



	public override TInterface EnsureService<TService, TInterface>() where TInterface : class
	{
		TInterface @interface = PackageInstance.GetService<TService, TInterface>();
		Diag.ThrowIfServiceUnavailable(@interface, typeof(TInterface));

		return @interface;
	}



	public override string GetRegisterConnectionDatasetKey(IVsDataExplorerConnection root)
	{
		Csb csa = RctManager.CloneRegistered(root);
		return csa == null ? CoreConstants.C_DefaultExDatasetKey : csa.DatasetKey;
	}



	public override void InitializeSettings() => _ = PersistentSettings.SettingsStore;


	protected override void InternalShutdownDte()
	{
		_Dte = null;
	}



	public override void InvalidateRctManager() => RctManager.Invalidate();



	public override bool IsConnectionEquivalency(string connectionString1, string connectionString2)
		=> Csb.AreEquivalent(connectionString1, connectionString2, Csb.ConnectionKeys);


	public override bool IsWeakConnectionEquivalency(string connectionString1, string connectionString2)
		=> Csb.AreEquivalent(connectionString1, connectionString2, Csb.WeakEquivalencyKeys);



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Disables solution event invocation
	/// </summary>
	/// <param name="disposing"></param>
	// ---------------------------------------------------------------------------------
	public override void UnadviseEvents(bool disposing)
	{
		// Diag.ThrowIfNotOnUIThread();

		if (!disposing)
			return;

		_PackageInstance.OnLoadSolutionOptionsEvent -= OnLoadSolutionOptions;
		_PackageInstance.OnSaveSolutionOptionsEvent -= OnSaveSolutionOptions;

		if (_DteEvents != null)
		{
			_DteEvents.OnBeginShutdown -= OnBeginShutdown;
			_DteEvents = null;
		}

		if (_BuildEvents != null)
		{
			_BuildEvents.OnBuildDone -= OnBuildDone;
			_BuildEvents = null;
		}

		if (_DynamicTypeService != null)
		{
			_DynamicTypeService.AssemblyObsolete -= OnAssemblyObsolete;
			_DynamicTypeService = null;
		}

		if (_ProjectItemEvents != null)
		{
			_ProjectItemEvents.ItemRemoved -= OnProjectItemRemoved;
			_ProjectItemEvents.ItemAdded -= OnProjectItemAdded;
			_ProjectItemEvents.ItemRenamed -= OnProjectItemRenamed;

			_ProjectItemEvents = null;
		}

		if (VsSolution != null && _HSolutionEvents != uint.MaxValue)
		{
			try
			{
				VsSolution.UnadviseSolutionEvents(_HSolutionEvents);
			}
			catch (Exception ex)
			{
				Diag.Debug(ex);
			}
		}


		try
		{
			if (RdtManager.ServiceAvailable && _HDocTableEvents != uint.MaxValue)
				RdtManager.UnadviseRunningDocTableEvents(_HDocTableEvents);
		}
		catch (Exception ex)
		{
			Diag.Debug(ex);
		}


		try
		{
			if (_MonitorSelection != null && _HSelectionEvents != uint.MaxValue)
				_MonitorSelection.UnadviseSelectionEvents(_HSelectionEvents);
		}
		catch (Exception ex)
		{
			Diag.Debug(ex);
		}


		_HSolutionEvents = uint.MaxValue;
		_HDocTableEvents = uint.MaxValue;
		_MonitorSelection = null;
		_HSelectionEvents = uint.MaxValue;

	}


	#endregion Methods


}