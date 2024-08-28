// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BlackbirdSql.Controller.Ctl.Config;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Extensions;
using BlackbirdSql.Sys.Interfaces;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TaskStatusCenter;
using VSLangProj;



namespace BlackbirdSql.Controller;


// =========================================================================================================
//											ControllerEventsManager Class
//
/// <summary>
/// Performs validations and updates of solutions and project items.
/// </summary>
/// <remarks>
/// Updates the app.config for DbProvider and EntityFramework and updates existing .edmx models that
/// are using a legacy provider.
/// Also ensures we never do validations of a solution and project app.config and .edmx models twice.
/// </remarks>
// =========================================================================================================
public sealed class ControllerEventsManager : AbstractEventsManager
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - ControllerEventsManager
	// ---------------------------------------------------------------------------------


	/// <summary>
	/// .ctor
	/// </summary>
	private ControllerEventsManager(IBsPackageController controller) : base(controller)
	{
	}


	/// <summary>
	/// Access to the static at the instance local level. This allows the base class to access and update
	/// the localized static instance.
	/// </summary>
	protected override IBsEventsManager InternalInstance
	{
		get { return _Instance; }
		set { _Instance = value; }
	}


	/// <summary>
	/// Gets the instance of the Events Manager for this assembly.
	/// We do not auto-create to avoid instantiation confusion.
	/// Use CreateInstance() to instantiate.
	/// </summary>
	public static IBsEventsManager Instance => _Instance ??
		throw Diag.ExceptionInstance(typeof(ControllerEventsManager));


	/// <summary>
	/// Creates the singleton instance of the Events Manager for this assembly.
	/// Instantiation must always occur here and not by the Instance accessor to avoid
	/// confusion.
	/// </summary>
	public static ControllerEventsManager CreateInstance(IBsPackageController controller) =>
		new(controller);


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// ControllerEventsManager destructor
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected override void Dispose(bool disposing)
	{
		Controller.OnLoadSolutionOptionsEvent -= OnLoadSolutionOptions;
		Controller.OnAfterOpenProjectEvent -= OnAfterOpenProject;
		Controller.OnAfterOpenSolutionEvent -= OnAfterOpenSolution;
		Controller.OnAfterCloseSolutionEvent -= OnAfterCloseSolution;
		Controller.OnBeforeDocumentWindowShowEvent -= OnBeforeDocumentWindowShow;
		Controller.OnQueryCloseProjectEvent -= OnQueryCloseProject;

		Controller.OnBuildDoneEvent -= OnBuildDone;
		Controller.OnProjectInitializedEvent -= OnProjectInitialized;

		/*
		Controller.OnAssemblyObsoleteEvent -= OnAssemblyObsolete;
		Controller.OnDesignTimeOutputDeletedEvent -= OnDesignTimeOutputDeleted;
		Controller.OnDesignTimeOutputDirtyEvent -= OnDesignTimeOutputDirty;
		Controller.OnProjectItemAddedEvent -= OnProjectItemAdded;
		Controller.OnProjectItemRemovedEvent -= OnProjectItemRemoved;
		Controller.OnProjectItemRenamedEvent -= OnProjectItemRenamed;
		Controller.OnReferenceAddedEvent -= OnReferenceAdded;
		Controller.OnReferenceChangedEvent -= OnReferenceChanged;
		Controller.OnReferenceRemovedEvent -= OnReferenceRemoved;
		Controller.OnCmdUIContextChangedEvent -= OnCmdUIContextChanged;
		*/
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Hooks onto the controller's solution events and performs a initial solution
	/// validation.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override void Initialize()
	{
		// Tracer.Trace(GetType(), "Initialize()");

		_TaskHandlerTaskName = "Validation";

		Controller.OnLoadSolutionOptionsEvent += OnLoadSolutionOptions;
		Controller.OnAfterOpenProjectEvent += OnAfterOpenProject;
		Controller.OnAfterOpenSolutionEvent += OnAfterOpenSolution;
		Controller.OnAfterCloseSolutionEvent += OnAfterCloseSolution;
		Controller.OnBeforeDocumentWindowShowEvent += OnBeforeDocumentWindowShow;
		Controller.OnQueryCloseProjectEvent += OnQueryCloseProject;

		Controller.OnBuildDoneEvent += OnBuildDone;
		Controller.OnProjectInitializedEvent += OnProjectInitialized;

		/*
		Controller.OnAssemblyObsoleteEvent += OnAssemblyObsolete;
		Controller.OnDesignTimeOutputDeletedEvent += OnDesignTimeOutputDeleted;
		Controller.OnDesignTimeOutputDirtyEvent += OnDesignTimeOutputDirty;
		Controller.OnProjectItemAddedEvent += OnProjectItemAdded;
		Controller.OnProjectItemRemovedEvent += OnProjectItemRemoved;
		Controller.OnProjectItemRenamedEvent += OnProjectItemRenamed;
		Controller.OnReferenceAddedEvent += OnReferenceAdded;
		Controller.OnReferenceRemovedEvent += OnReferenceRemoved;
		Controller.OnReferenceChangedEvent += OnReferenceChanged;
		Controller.OnCmdUIContextChangedEvent += OnCmdUIContextChanged;
		*/
	}


	#endregion Constructors / Destructors




	// =========================================================================================================
	#region Fields - ControllerEventsManager
	// =========================================================================================================


	private static IBsEventsManager _Instance;

	// private int _RefCnt = 0;

	// A sync call has taken over. Async is locked out or abort at the first opportunity.
	private CancellationTokenSource _ValidationTokenSource = null;
	private CancellationToken _ValidationToken;
	private Task<bool> _ValidationTask;

	private static int _EventValidationCardinal = 0;


	#endregion Fields





	// =========================================================================================================
	#region Property accessors - ControllerEventsManager
	// =========================================================================================================


	public static bool SolutionValidating => _EventValidationCardinal > 0;


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - ControllerEventsManager
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// [Launch ensure UI thread]: Validates and updates a solution projects the
	/// app.config invariant and Entity Framework settings.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public void AsyeuValidateSolution(Stream stream = null)
	{
		if (!EventValidationEnter())
			return;

		// We're going to check each project that gets loaded (or has a reference added) if it
		// references the database EntityFramework dll else the invariant dll.
		// If it is we'll check the app.config DbProvider and EntityFramework sections and update if necessary.
		// We also check (once and only once) within a project for any edmxs with legacy settings and update
		// those, because they cannot work with newer versions of EntityFramework.

		// Fire and wait.

		if (!ThreadHelper.CheckAccess())
		{
			bool result = Task.Run(async delegate
			{
				await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

				if (ApcManager.SolutionClosing)
					return true;

				ValidateSolutionImpl();

				return true;

			}).AwaiterResult();

			return;
		}

		ValidateSolutionImpl();
	}



	private void CloseOpenProjectModels(IVsHierarchy hierarchy)
	{
		if (hierarchy.IsMiscellaneous())
			return;

		if (!PersistentSettings.AutoCloseOffScreenEdmx && !PersistentSettings.AutoCloseXsdDatasets)
			return;


		List<string> extensions = [];

		if (PersistentSettings.AutoCloseOffScreenEdmx)
			extensions.Add(".edmx");
		if (PersistentSettings.AutoCloseXsdDatasets)
			extensions.Add(".xsd");

		IDictionary<ProjectItem, (uint, string)> openItems = UnsafeCmd.GetOpenProjectItems(hierarchy, [.. extensions]);

		bool closeEdmx = PersistentSettings.AutoCloseOffScreenEdmx;

		if (closeEdmx)
		{
			// This handles the edm sub-system bug where edmx models fail to load.
			// If any edmx model is onscreen, it will be onscreen the next time the project is loaded.
			// This will correctly initialize the edm sub-system so that any other hidden edmx models
			// will not fail to load.
			// If all edmx models in the rdt are offscreen we're going to close them, because opening
			// any of them with a tab switch the next time the project is loaded will cause the edm
			// sub-system to fail for the duratiuon of the ide session.

			foreach (KeyValuePair<ProjectItem, (uint, string)> pair in openItems)
			{
				if (pair.Key.IsDirty)
					continue;

				try
				{
					if (!Path.GetExtension(pair.Value.Item2).Equals(".edmx", StringComparison.OrdinalIgnoreCase))
						continue;
				}
				catch
				{
					continue;
				}

				if (RdtManager.WindowFrameIsOnScreen(pair.Value.Item2))
				{
					closeEdmx = false;
					break;
				}
			}

			if (!closeEdmx && !PersistentSettings.AutoCloseXsdDatasets)
				return;
		}


		foreach (KeyValuePair<ProjectItem, (uint, string)> pair in openItems)
		{
			if (pair.Key.IsDirty)
				continue;

			if (!closeEdmx && PersistentSettings.AutoCloseOffScreenEdmx)
			{
				try
				{
					if (Path.GetExtension(pair.Value.Item2).Equals(".edmx", StringComparison.OrdinalIgnoreCase))
					{
						continue;
					}
				}
				catch
				{
					continue;
				}
			}

			// RdtManager.HandsOffDocument(pair.Value.Item1, pair.Value.Item2);

			try
			{
				RdtManager.CloseDocument(__FRAMECLOSE.FRAMECLOSE_NoSave, pair.Value.Item1);
			}
			finally
			{
				// RdtManager.HandsOnDocument(pair.Value.Item1, pair.Value.Item2);
			}
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Recursively makes calls to <see cref="RecursiveValidateProjectAsync"/>, which is
	/// on the UI thread, from a thread in the thread pool. This is to ensure the UI
	/// thread is not locked up processing validations. (No longer does UI switching
	/// on a per project basis.)
	/// </summary>
	/// <param name="projectCount"></param>
	// ---------------------------------------------------------------------------------
	private async Task<bool> ValidateSolutionAsync(string solutionName)
	{
		await Task.Run(() => UpdateStatusBar("BlackbirdSql validating solution: " + solutionName));


		TaskHandlerOptions options = default;
		options.Title = $"BlackbirdSql Solution validation > {solutionName}";
		options.ActionsAfterCompletion = CompletionActions.None;

		_ProgressData = default;
		_ProgressData.CanBeCanceled = true;

		_TaskHandler = Controller.StatusCenterService.PreRegister(options, _ProgressData);


		_ValidationTokenSource?.Dispose();
		_ValidationTokenSource = new();
		_ValidationToken = _ValidationTokenSource.Token;

		// The following for brevity.
		CancellationToken asyncCancellationToken = _ValidationToken;
		CancellationToken userCancellationToken = _TaskHandler.UserCancellation;
		TaskCreationOptions creationOptions = TaskCreationOptions.PreferFairness
			| TaskCreationOptions.AttachedToParent;
		TaskScheduler scheduler = TaskScheduler.Default;

		Task<bool> payloadAsync() =>
			ValidateSolutionPayloadAsync(asyncCancellationToken, userCancellationToken);

		// Projects may have already been opened. They may be irrelevant eg. unloaded
		// project items or other non-project files, but we have to check anyway.
		// Performance is a priority here, not compact code, because we're synchronous on the main
		// thread, so we stay within the: 
		// Projects > Project > ProjectItems > SubProject > ProjectItems... structure.
		// We want to be in and out of here as fast as possible so every possible low overhead check
		// is done first to ensure that.
		// Start up the payload launcher with tracking.


		// Fire and remember

		_ValidationTask = await Task.Factory.StartNew(payloadAsync, default, creationOptions, scheduler);


		try
		{
			_TaskHandler.RegisterTask(_ValidationTask);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;
		}

		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Checks if a project has the invariant dll configured in the app.config and
	/// configures it if it doesn't
	/// </summary>
	/// <param name="project"></param>
	/// <returns>true if completed successfully else false if there were errors.</returns>
	// ---------------------------------------------------------------------------------
	private bool ValidateSolutionConfigureDbProvider(ProjectItem config, bool invalidate)
	{
		if (_TaskHandler.UserCancellation.Cancelled())
			_ValidationTokenSource?.Cancel();
		if (_ValidationToken.Cancelled())
			return false;


		Diag.ThrowIfNotOnUIThread();


		bool modified;

		try
		{
			if (config.IsOpen)
			{
				System.IO.IOException ex = new System.IO.IOException(config.ContainingProject.Name + " File is open: app.config");
				Diag.Dug(ex);
				return false;
			}

			if (config.FileCount == 0)
				return false;

			string path = config.FileNames[0];

			try
			{
				modified = XmlParser.ConfigureDbProvider(path, NativeDb.ClientFactoryType);
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				return false;
			}

			if (modified && invalidate)
			{
				try
				{
					config.ContainingProject.IsDirty = true;
				}
				catch (Exception ex)
				{
					Diag.Dug(ex);
				}
			}
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			return false;
		}


		if (modified)
			TaskHandlerProgress($">  {config.ContainingProject.Name} -> Updated App.config: DbProvider");

		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Checks if a project has the db engine EntityFramework (EntityFramework.Firebird
	/// for the Firebird port) configured in the app.config and configures it if it
	/// doesn't.
	/// </summary>
	/// <param name="project"></param>
	/// <returns>true if completed successfully else false if there were errors.</returns>
	// ---------------------------------------------------------------------------------
	private bool ValidateSolutionConfigureEntityFramework(ProjectItem config, bool invalidate)
	{
		if (_TaskHandler.UserCancellation.Cancelled())
			_ValidationTokenSource?.Cancel();
		if (_ValidationToken.Cancelled())
			return false;

		Diag.ThrowIfNotOnUIThread();


		bool modified;

		try
		{
			if (config.IsOpen)
			{
				Tracer.Warning(GetType(), "ConfigureEntityFramework()", "{0}: app.config file is open: app.config", config.ContainingProject.Name);
				return false;
			}

			if (config.FileCount == 0)
				return false;

			string path = config.FileNames[0];

			try
			{
				// If Entity framework must be configured then so must the client
				modified = XmlParser.ConfigureEntityFramework(path, true, NativeDb.ClientFactoryType);
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				return false;
			}

			if (modified && invalidate)
			{
				try
				{
					config.ContainingProject.IsDirty = true;
				}
				catch (Exception ex)
				{
					Diag.Dug(ex);
				}
			}
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			return false;
		}


		if (modified)
			TaskHandlerProgress($">  {config.ContainingProject.Name} -> Updated App.config: DbProvider and EntityFramework");


		return true;
	}



	private void ValidateSolutionImpl()
	{
		// On main thread

		Solution solution = ApcManager.SolutionObject;

		if (solution == null || ApcManager.SolutionProjects == null && ApcManager.SolutionProjects.Count == 0)
		{
			EventValidationExit();
			return;
		}

		// No projects loaded yet if Solution.Projects is null

		// Everything is synchronous within this particular call stack.
		// We cannot have projects being loaded while we're checking for any projects that may have been loaded.
		//
		// Also this condition is for a very particular set of circumstances:
		//		1. The developer started up a fresh IDE instance and went straight into opening a solution before
		//			the IDE shell was fully loaded.
		//			ie. Our package had been given the ide context but was not yet sited.
		//		2. Subsequent to installing our vsix, this particular solution had never before been opened.
		//			ie. it's a first for this solution since installing our extension.
		//		3. At least one of this solution's projects had been loaded before we were sited.
		//			iow. Any projects whose OnAfterOpen events have already been fired.

		// To keep the UI thread free and to allow it to perform status
		// updates we move off of the UI thread and then make calls back
		// to the UI thread for each top-level project validation.
		string solutionName = Path.GetFileNameWithoutExtension(solution.FileName);

		_ = Task.Run(() => ValidateSolutionAsync(solutionName));
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The _AsyncPayloadLauncher payload.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private async Task<bool> ValidateSolutionPayloadAsync(CancellationToken asyncCancellationToken,
		CancellationToken userCancellationToken)
	{
		await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

		List<Project> projects = UnsafeCmd.RecursiveGetDesignTimeProjects();

		try
		{
			if (projects.Count == 0 || userCancellationToken.Cancelled() || asyncCancellationToken.Cancelled()
				|| ApcManager.SolutionObject == null)
			{
				return false;
			}

			Stopwatch stopwatch = new();

			TaskHandlerProgress(0, 0);

			int i = 0;

			foreach (Project project in projects)
			{
				stopwatch.Start();

				ValidateSolutionRecursiveProject(project);

				stopwatch.Stop();


				if (userCancellationToken.Cancelled() || asyncCancellationToken.Cancelled())
					i = projects.Count - 1;

				TaskHandlerProgress((i + 1) * 100 / projects.Count, stopwatch.Elapsed.Milliseconds);

				if (userCancellationToken.Cancelled() || asyncCancellationToken.Cancelled())
					break;

				i++;
			}

			if (userCancellationToken.Cancelled())
			{
				UpdateStatusBar("Cancelled BlackbirdSql solution validation", true);
				return false;
			}

			UpdateStatusBar($"Completed BlackbirdSql solution validation in {stopwatch.ElapsedMilliseconds}ms", true);

			// _TaskHandler = null;
			// _ProgressData = default;

			return true;

		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
		}
		finally
		{
			EventValidationExit();
		}

		return false;

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Recursively validates a project already opened before our package was sited
	/// </summary>
	/// <param name="projects"></param>
	/// <remarks>
	/// If the project is valid and has EntityFramework referenced, the app.config is checked.
	/// If it doesn't an <see cref="OnGlobalReferenceAdded"/> event is attached.
	/// Updates legacy edmxs in the project
	/// If it's a folder project is checked for child projects
	/// </remarks>
	// ---------------------------------------------------------------------------------
	private void ValidateSolutionRecursiveProject(Project project)
	{
		if (_TaskHandler.UserCancellation.Cancelled())
			_ValidationTokenSource?.Cancel();
		if (_ValidationToken.Cancelled())
			return;

		Diag.ThrowIfNotOnUIThread();

		ProjectItem config = null;

		bool isConfiguredDbProviderStatus = false; // globals.IsConfiguredDbProviderStatus;

		VSProject projectObject = null;

		try
		{
			projectObject = project.Object as VSProject;
		}
		catch { };

		if (projectObject == null)
			return;

		Reference reference;

		try
		{
			if (projectObject.References == null)
				return;

			reference = projectObject.References.Find(NativeDb.EFProvider);
		}
		catch
		{
			return;
		}

		if (reference != null)
		{
			isConfiguredDbProviderStatus = true;

			config ??= project.GetAppConfig();

			if (config != null)
				ValidateSolutionConfigureEntityFramework(config, false);
		}

		if (!isConfiguredDbProviderStatus)
		{
			if (projectObject.References.Find(NativeDb.Invariant) != null)
			{
				config ??= project.GetAppConfig();
				if (config != null)
					ValidateSolutionConfigureDbProvider(config, false);
			}
		}

		try
		{
			foreach (ProjectItem item in project.ProjectItems)
			{
				if (_TaskHandler.UserCancellation.Cancelled())
					_ValidationTokenSource?.Cancel();

				if (_ValidationToken.Cancelled())
				{
					break;
				}

				ValidateSolutionRecursiveProjectItemEdmx(item);
			}
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
		}
	}




	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Checks if a project item is an edmx and calls <see cref="UpdateEdmx"/> if it is.
	/// If it's a project folder it recursively calls itself.
	/// </summary>
	/// <param name="item"></param>
	/// <returns>true if completed successfully else false if there were errors.</returns>
	// ---------------------------------------------------------------------------------
	private bool ValidateSolutionRecursiveProjectItemEdmx(ProjectItem item)
	{
		if (_TaskHandler != null && _TaskHandler.UserCancellation.Cancelled())
			return false;

		Diag.ThrowIfNotOnUIThread();

		if (UnsafeCmd.Kind(item.Kind) == "PhysicalFolder")
		{
			bool success = true;

			foreach (ProjectItem subitem in item.ProjectItems)
			{
				if (_TaskHandler.UserCancellation.Cancelled())
					_ValidationTokenSource?.Cancel();
				if (_ValidationToken.Cancelled())
				{
					success = false;
					break;
				}

				if (!ValidateSolutionRecursiveProjectItemEdmx(subitem))
					success = false;
			}

			return success;
		}

		// Tracer.Trace(item.ContainingProject.Name + " checking projectitem: " + item.Name + ":" + item.Kind);

		if (_TaskHandler.UserCancellation.Cancelled())
			_ValidationTokenSource?.Cancel();
		if (_ValidationToken.Cancelled())
			return false;

		try
		{
			if (item.FileCount < 1)
				return true;
		}
		catch (Exception ex)
		{
			Diag.Dug(ex, item.ContainingProject.Name + ":" + item.Name);
			return false;
		}

		Property link;

		try
		{
			link = item.Properties.Item("IsLink");
		}
		catch
		{
			Diag.StackException(item.ContainingProject.Name + ":" + item.Name + " has no link property");
			return false;
		}

		try
		{
			if (link == null || (bool)link.Value == true)
				return true;
		}
		catch (Exception ex)
		{
			Diag.Dug(ex, item.ContainingProject.Name + ":" + item.Name);
			return false;
		}

		try
		{
			FileInfo info = new FileInfo(item.FileNames[0]);
			if (info.Extension.ToLower() != ".edmx")
				return true;
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			return false;
		}

		try
		{
			if (!ValidateSolutionUpdateEdmx(item, false))
				return false;
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			return false;
		}

		return true;

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Checks if an edmx is using the Legacy provider and updates it to the current provider if it is.
	/// </summary>
	/// <param name="project"></param>
	/// <returns>true if completed successfully else false if there were errors.</returns>
	// ---------------------------------------------------------------------------------
	private bool ValidateSolutionUpdateEdmx(ProjectItem edmx, bool invalidate)
	{
		if (_TaskHandler.UserCancellation.Cancelled())
			_ValidationTokenSource?.Cancel();
		if (_ValidationToken.Cancelled())
			return false;

		Diag.ThrowIfNotOnUIThread();


		try
		{
			if (edmx.FileCount == 0)
				return true;

			string path = edmx.FileNames[0];

			if (edmx.IsOpen)
			{
				if (XmlParser.IsValidEdmx(path))
					Tracer.Warning(GetType(), "UpdateEdmx()", "{0}: edmx file is open: {1}", edmx.ContainingProject.Name, edmx.Name);
				return false;
			}


			if (!XmlParser.UpdateEdmx(path))
				return true;
			else
				TaskHandlerProgress($">  {edmx.ContainingProject.Name} -> Updated {edmx.Name}: Legacy flag");


			if (!invalidate)
				return true;

			try
			{
				edmx.ContainingProject.IsDirty = true;
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
			}
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			return false;
		}


		return true;

	}


	#endregion Methods





	// =========================================================================================================
	#region Events and Event handling - ControllerEventsManager
	// =========================================================================================================



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Increments the <see cref="_EventValidationCardinal"/> counter when execution
	/// enters a Validation event to prevent recursion.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private bool EventValidationEnter(bool test = false)
	{
		lock (_LockObject)
		{
			if (_EventValidationCardinal > 0)
				return false;
		}

		if (ApcManager.SolutionClosing)
			return false;

		lock (_LockObject)
		{
			if (!test)
				_EventValidationCardinal++;
		}

		return true;
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Decrements the <see cref="_EventValidationCardinal"/> counter that was previously
	/// incremented by <see cref="EventValidationEnter"/>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void EventValidationExit()
	{
		lock (_LockObject)
		{
			if (_EventValidationCardinal <= 0)
			{
				ApplicationException ex = new($"Attempt to exit Validation event when not in a Validation event. _EventValidationCardinal: {_EventValidationCardinal}");
				Diag.Dug(ex);
				throw ex;
			}

			_EventValidationCardinal--;
		}
	}



	private void OnLoadSolutionOptions(Stream stream)
	{
		// Diag.DebugTrace("OnLoadSolutionOptions()");

		// Register configured connections.
		// Check for loading here otherwise an exception will be thrown.
		if (!RctManager.Loading)
		{
			RctManager.ResetVolatile();
			RctManager.LoadConfiguredConnections();
		}

		NativeDb.AsyuiReindexEntityFrameworkAssemblies();
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Event handler for the <see cref="IVsSolutionEvents"/> AfterCloseSolution event
	/// </summary>
	/// <param name="pUnkReserved"></param>
	/// <returns></returns>
	// ---------------------------------------------------------------------------------
	private int OnAfterCloseSolution(object pUnkReserved)
	{
		// Tracer.Trace(GetType(), "OnAfterCloseSolution()");

		// Reset configured connections registration and the unique database connection
		// DatasetKeys for rebuild on next soluton load.
		RctManager.ResetVolatile();

		return VSConstants.S_OK;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Event handler for <see cref="IVsSolutionEvents"/> AfterOpenProject event
	/// </summary>
	// ---------------------------------------------------------------------------------
	private int OnAfterOpenProject(Project project, int fAdded)
	{
		// Tracer.Trace(GetType(), "OnAfterOpenProject()");

		if (project.EditableObject() == null)
			return VSConstants.S_OK;

		NativeDb.AsyuiReindexEntityFrameworkAssemblies(project);

		RctManager.AsyuiLoadApplicationConnections(project);

		return VSConstants.S_OK;
	}


	private int OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
	{
		PackageInstance.OutputLoadStatistics();
		return VSConstants.S_OK;
	}



	/*
	private void OnAssemblyObsolete(object sender, AssemblyObsoleteEventArgs e)
	{
		// Tracer.Trace(GetType(), "OnAssemblyObsolete()");

		NativeDb.AsyuiReindexEntityFrameworkAssemblies();
	}
	*/


	private int OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame)
	{
		Diag.ThrowIfNotOnUIThread();

		if (!fFirstShow.AsBool())
			return VSConstants.S_OK;

		RunningDocumentInfo docInfo;

		try
		{
			docInfo = RdtManager.GetDocumentInfo(docCookie);

			if (string.IsNullOrEmpty(docInfo.Moniker))
				return VSConstants.S_OK;

			string ext = Path.GetExtension(docInfo.Moniker);

			if (!ext.Equals(".edmx", StringComparison.CurrentCultureIgnoreCase))
				return VSConstants.S_OK;
		}
		catch
		{
			return VSConstants.S_OK;
		}

		Project project;

		try
		{
			project = docInfo.Hierarchy?.ToProject();
		}
		catch
		{
			return VSConstants.S_OK;
		}

		if (project == null)
			return VSConstants.S_OK;

		NativeDb.AsyuiReindexEntityFrameworkAssemblies(project);

		return VSConstants.S_OK;
	}



	private void OnBuildDone(vsBuildScope Scope, vsBuildAction Action)
	{
		// Tracer.Trace(GetType(), "OnBuildDone()");

		NativeDb.AsyuiReindexEntityFrameworkAssemblies();
	}


	private int OnQueryCloseProject(IVsHierarchy hierarchy, int removing, ref int cancel)
	{
		// Tracer.Trace(GetType(), "OnQueryCloseProject()");

		if (removing.AsBool())
			return VSConstants.S_OK;

		Controller.EventRdtEnter(false, true);

		try
		{
			CloseOpenProjectModels(hierarchy);
		}
		finally
		{
			Controller.EventRdtExit();
		}

		return VSConstants.S_OK;
	}



	/*
	private void OnDesignTimeOutputDeleted(string bstrOutputMoniker)
	{
		// Tracer.Trace(GetType(), "OnDesignTimeOutputDeleted()", "bstrOutputMoniker: {0}.", bstrOutputMoniker);

		NativeDb.AsyuiReindexEntityFrameworkAssemblies();
	}



	void OnDesignTimeOutputDirty(string bstrOutputMoniker)
	{
		// Tracer.Trace(GetType(), "OnDesignTimeOutputDirty()", "bstrOutputMoniker: {0}.", bstrOutputMoniker);

		NativeDb.AsyuiReindexEntityFrameworkAssemblies();
	}
	*/


	private void OnProjectInitialized(Project project)
	{
		// Tracer.Trace(GetType(), "OnProjectInitialized()", "Project: {0}.", project.Name);

		NativeDb.AsyuiReindexEntityFrameworkAssemblies(project);
	}


	/* 
	private void OnProjectItemAdded(ProjectItem projectItem)
	{
		// Tracer.Trace(GetType(), "OnProjectItemAdded()", "Added Project: {0}, ProjectItem: {1}.", projectItem.ContainingProject?.Name, projectItem.Name);

		if (!projectItem.ContainingProject.IsEditable())
			return;

		NativeDb.AsyuiReindexEntityFrameworkAssemblies(projectItem.ContainingProject);
	}


	
	private void OnProjectItemRemoved(ProjectItem projectItem)
	{
		// Tracer.Trace(GetType(), "OnProjectItemRemoved()", "Removed Project: {0}, ProjectItem: {1}.", projectItem.ContainingProject?.Name, projectItem.Name);

		NativeDb.AsyuiReindexEntityFrameworkAssemblies(projectItem.ContainingProject);
	}



	private void OnProjectItemRenamed(ProjectItem projectItem, string oldName)
	{
		// Tracer.Trace(GetType(), "OnProjectItemRenamed()", "Renamed Project: {0}, ProjectItem: {1}.", projectItem.ContainingProject?.Name, projectItem.Name);

		NativeDb.AsyuiReindexEntityFrameworkAssemblies(projectItem.ContainingProject);
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Event handler for the Project
	/// <see cref="_dispReferencesEvents_Event.ReferenceAdded"/> event
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void OnReferenceAdded(Reference reference)
	{
		// Tracer.Trace(GetType(), "OnReferenceAdded()", "Project: {0}, Reference: {1}.", reference.ContainingProject?.Name, reference.Name);

		NativeDb.AsyuiReindexEntityFrameworkAssemblies(reference.ContainingProject);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Event handler for the Project
	/// <see cref="_dispReferencesEvents_Event.ReferenceChanged"/> event
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void OnReferenceChanged(Reference reference)
	{
		// Tracer.Trace(GetType(), "OnReferenceChanged()", "Project: {0}.", reference.ContainingProject?.Name);

		NativeDb.AsyuiReindexEntityFrameworkAssemblies(reference.ContainingProject);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Event handler for the Project
	/// <see cref="_dispReferencesEvents_Event.ReferenceRemoved"/> event
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void OnReferenceRemoved(Reference reference)
	{
		// Tracer.Trace(GetType(), "OnReferenceRemoved()", "Project: {0}.", reference.ContainingProject?.Name);

		NativeDb.AsyuiReindexEntityFrameworkAssemblies(reference.ContainingProject);
	}
	*/


	#endregion Events and Event handling


}