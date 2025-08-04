// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BlackbirdSql.Controller.Ctl.Config;
using BlackbirdSql.Controller.Properties;
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
internal sealed class ControllerEventsManager : AbstractEventsManager
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
	internal static IBsEventsManager Instance => _Instance ??
		throw Diag.ExceptionInstance(typeof(ControllerEventsManager));


	/// <summary>
	/// Creates the singleton instance of the Events Manager for this assembly.
	/// Instantiation must always occur here and not by the Instance accessor to avoid
	/// confusion.
	/// </summary>
	internal static ControllerEventsManager CreateInstance(IBsPackageController controller) =>
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

		/*
		Controller.OnProjectInitializedEvent -= OnProjectInitialized;
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
		Evs.Trace(GetType(), nameof(Initialize));

		_TaskHandlerTaskName = "Validation";

		Controller.OnLoadSolutionOptionsEvent += OnLoadSolutionOptions;
		Controller.OnAfterOpenProjectEvent += OnAfterOpenProject;
		Controller.OnAfterOpenSolutionEvent += OnAfterOpenSolution;
		Controller.OnAfterCloseSolutionEvent += OnAfterCloseSolution;
		Controller.OnBeforeDocumentWindowShowEvent += OnBeforeDocumentWindowShow;
		Controller.OnQueryCloseProjectEvent += OnQueryCloseProject;

		Controller.OnBuildDoneEvent += OnBuildDone;

		/*
		Controller.OnProjectInitializedEvent += OnProjectInitialized;
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


	internal static bool SolutionValidating => _EventValidationCardinal > 0;


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - ControllerEventsManager
	// =========================================================================================================


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
					if (!Cmd.GetExtension(pair.Value.Item2).Equals(".edmx", StringComparison.OrdinalIgnoreCase))
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
					if (Cmd.GetExtension(pair.Value.Item2).Equals(".edmx", StringComparison.OrdinalIgnoreCase))
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
	/// Moves back onto the UI thread and updates the IDE task handler progress bar
	/// with project update information.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override bool TaskHandlerProgress(string text)
	{
		if (_ProgressData.PercentComplete == null)
		{
			_ProgressData.PercentComplete = 0;
			if (_TaskHandler != null)
				Diag.TaskHandlerProgress(this, ControlsResources.ValidateSolution_Started);
		}

		Diag.TaskHandlerProgress(this, text);

		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Moves back onto the UI thread and updates the IDE task handler progress bar.
	/// </summary>
	/// <param name="progress">The % completion of TaskHandlerTaskName.</param>
	/// <param name="elapsed">The time taken to complete the stage.</param>
	// ---------------------------------------------------------------------------------
	public override bool TaskHandlerProgress(int progress, int elapsed)
	{
		bool completed = false;
		string text;

		if (progress == 0)
		{
			text = ControlsResources.ValidateSolution_Started;
		}
		else if (progress == 100)
		{
			completed = true;
			text = ControlsResources.ValidateSolution_Completed.Fmt(elapsed);
		}
		else if (_TaskHandler.UserCancellation.Cancelled())
		{
			completed = true;
			text = ControlsResources.ValidateSolution_Cancelled.Fmt(progress, elapsed);
		}
		else
		{
			text = ControlsResources.ValidateSolution_Progress.Fmt(progress, elapsed);
		}

		_ProgressData.PercentComplete = progress;

		Diag.TaskHandlerProgress(this, text, completed);

		return true;

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
		await Task.Run(() => UpdateStatusBar(ControlsResources.ValidateSolution_StatusBarStart.Fmt(solutionName)));


		TaskHandlerOptions options = default;
		options.Title = ControlsResources.ValidateSolution_Title.Fmt(solutionName);
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
			Diag.Ex(ex);
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
				IOException ex = new (Resources.ExceptionAppConfigOpen.Fmt(config.ContainingProject.Name));
				Diag.Ex(ex);
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
				Diag.Ex(ex);
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
					Diag.Ex(ex);
				}
			}
		}
		catch (Exception ex)
		{
			Diag.Ex(ex);
			return false;
		}


		if (modified)
			TaskHandlerProgress(ControlsResources.ValidateSolution_UpdatedAppConfigDbProvider.Fmt(config.ContainingProject.Name));

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
				Evs.Warning(GetType(), nameof(ValidateSolutionConfigureEntityFramework),
					ControlsResources.ValidateSolution_WarningAppConfigOpen.Fmt(config.ContainingProject.Name));
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
				Diag.Ex(ex);
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
					Diag.Ex(ex);
				}
			}
		}
		catch (Exception ex)
		{
			Diag.Ex(ex);
			return false;
		}


		if (modified)
			TaskHandlerProgress(ControlsResources.ValidateSoluton_UpdatedAppConfigDbProviderEF.Fmt(config.ContainingProject.Name));


		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// [Launch ensure UI thread]: Validates and updates a solution projects the
	/// app.config invariant and Entity Framework settings.
	/// </summary>
	// ---------------------------------------------------------------------------------
	internal void ValidateSolutionAsyeu(Stream stream = null)
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
		string solutionName = Cmd.GetFileNameWithoutExtension(solution.FileName);

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
				UpdateStatusBar(ControlsResources.ValidateSolution_StatusBarCancelled, true);
				return false;
			}

			UpdateStatusBar(ControlsResources.ValidateSolution_StatusBarCompleted.Fmt(stopwatch.ElapsedMilliseconds), true);

			// _TaskHandler = null;
			// _ProgressData = default;

			return true;

		}
		catch (Exception ex)
		{
			Diag.Ex(ex);
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
			Diag.Ex(ex);
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

		// Evs.Debug(GetType(), "ValidateSolutionRecursiveProjectItemEdmx()",
		//	$"Project: {item.ContainingProject.Name} checking projectitem: {item.Name}, kind: {item.Kind}.");

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
			Diag.Ex(ex, Resources.LabelColonSeparated.Fmt(item.ContainingProject.Name, item.Name));
			return false;
		}

		Property link;

		try
		{
			link = item.Properties.Item("IsLink");
		}
		catch
		{
			Diag.StackException(Resources.ExceptionProjectItemNoLinkProperty.Fmt(item.ContainingProject.Name, item.Name));
			return false;
		}

		try
		{
			if (link == null || (bool)link.Value == true)
				return true;
		}
		catch (Exception ex)
		{
			Diag.Ex(ex, Resources.LabelColonSeparated.Fmt(item.ContainingProject.Name, item.Name));
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
			Diag.Ex(ex);
			return false;
		}

		try
		{
			if (!ValidateSolutionUpdateEdmx(item, false))
				return false;
		}
		catch (Exception ex)
		{
			Diag.Ex(ex);
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
				{
					Evs.Warning(GetType(), nameof(ValidateSolutionUpdateEdmx),
						ControlsResources.ValidateSolution_WarningEdmxOpen.Fmt(edmx.ContainingProject.Name, edmx.Name));
				}
				return false;
			}


			if (!XmlParser.UpdateEdmx(path))
				return true;
			else
				TaskHandlerProgress(ControlsResources.ValidateSoluton_UpdatedEdmxLegacyFlag.Fmt(edmx.ContainingProject.Name, edmx.Name));


			if (!invalidate)
				return true;

			try
			{
				edmx.ContainingProject.IsDirty = true;
			}
			catch (Exception ex)
			{
				Diag.Ex(ex);
			}
		}
		catch (Exception ex)
		{
			Diag.Ex(ex);
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
				ApplicationException ex = new(Resources.ExceptionEventValidationExitError.Fmt(_EventValidationCardinal));
				Diag.Ex(ex);
				throw ex;
			}

			_EventValidationCardinal--;
		}
	}



	private void OnLoadSolutionOptions(Stream stream)
	{
		Evs.Trace(GetType(), nameof(OnLoadSolutionOptions));

		// Register configured connections.
		// Check for loading here otherwise an exception will be thrown.
		if (!RctManager.Loading)
		{
			RctManager.ClearVolatileConnections();
			RctManager.LoadConfiguredConnections();
		}

		NativeDb.ReindexEntityFrameworkAssembliesAsyui();
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
		Evs.Trace(GetType(), nameof(OnAfterCloseSolution));

		// Reset configured connections registration and the unique database connection
		// DatasetKeys for rebuild on next soluton load.
		RctManager.ClearVolatileConnections();

		return VSConstants.S_OK;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Event handler for <see cref="IVsSolutionEvents"/> AfterOpenProject event
	/// </summary>
	// ---------------------------------------------------------------------------------
	private int OnAfterOpenProject(Project project, int fAdded)
	{
		if (project.EditableObject() == null)
			return VSConstants.S_OK;

		Evs.Trace(GetType(), nameof(OnAfterOpenProject), Resources.LabelProjectName.Fmt(project?.Name));

		NativeDb.ReindexEntityFrameworkAssembliesAsyui(project);

		RctManager.LoadApplicationConnectionsAsyui(project);

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
		// Evs.Trace(GetType(), nameof(OnAssemblyObsolete));

		NativeDb.ReindexEntityFrameworkAssembliesAsyui();
	}
	*/


	private int OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame)
	{
		Evs.Trace(GetType(), nameof(OnBeforeDocumentWindowShow));

		Diag.ThrowIfNotOnUIThread();

		if (!fFirstShow.AsBool())
			return VSConstants.S_OK;

		RunningDocumentInfo docInfo;

		try
		{
			docInfo = RdtManager.GetDocumentInfo(docCookie);

			string ext = Cmd.GetExtension(docInfo.Moniker);

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

		NativeDb.ReindexEntityFrameworkAssembliesAsyui(project);

		return VSConstants.S_OK;
	}



	private void OnBuildDone(vsBuildScope Scope, vsBuildAction Action)
	{
		Evs.Trace(GetType(), nameof(OnBuildDone));

		NativeDb.ReindexEntityFrameworkAssembliesAsyui();
	}


	private int OnQueryCloseProject(IVsHierarchy hierarchy, int removing, ref int cancel)
	{
		Evs.Trace(GetType(), nameof(OnQueryCloseProject));

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
		// Evs.Trace(GetType(), nameof(OnDesignTimeOutputDeleted), $"bstrOutputMoniker: {bstrOutputMoniker}.");

		NativeDb.ReindexEntityFrameworkAssembliesAsyui();
	}



	void OnDesignTimeOutputDirty(string bstrOutputMoniker)
	{
		// Evs.Trace(GetType(), nameof(OnDesignTimeOutputDirty), $"bstrOutputMoniker: {bstrOutputMoniker}.");

		NativeDb.ReindexEntityFrameworkAssembliesAsyui();
	}



	private void OnProjectInitialized(Project project)
	{
		// Evs.Trace(GetType(), nameof(OnProjectInitialized), $"Project: {project?.Name}.");

		NativeDb.ReindexEntityFrameworkAssembliesAsyui(project);
	}



	private void OnProjectItemAdded(ProjectItem projectItem)
	{
		// Evs.Trace(GetType(), nameof(OnProjectItemAdded),
			$"Added Project: {projectItem.ContainingProject?.Name}, ProjectItem: {projectItem.Name}.");

		if (!projectItem.ContainingProject.IsEditable())
			return;

		NativeDb.ReindexEntityFrameworkAssembliesAsyui(projectItem.ContainingProject);
	}


	
	private void OnProjectItemRemoved(ProjectItem projectItem)
	{
		// Evs.Trace(GetType(), nameof(OnProjectItemRemoved),
			$"Removed Project: {projectItem.ContainingProject?.Name}, ProjectItem: {projectItem.Name}.");

		NativeDb.ReindexEntityFrameworkAssembliesAsyui(projectItem.ContainingProject);
	}



	private void OnProjectItemRenamed(ProjectItem projectItem, string oldName)
	{
		// Evs.Trace(GetType(), nameof(OnProjectItemRenamed),
			$"Renamed Project: {projectItem.ContainingProject?.Name}, ProjectItem: {projectItem.Name}.");

		NativeDb.ReindexEntityFrameworkAssembliesAsyui(projectItem.ContainingProject);
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Event handler for the Project
	/// <see cref="_dispReferencesEvents_Event.ReferenceAdded"/> event
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void OnReferenceAdded(Reference reference)
	{
		// Evs.Trace(GetType(), nameof(OnReferenceAdded),
			$"Project: {reference.ContainingProject?.Name}, Reference: {reference.Name}.");

		NativeDb.ReindexEntityFrameworkAssembliesAsyui(reference.ContainingProject);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Event handler for the Project
	/// <see cref="_dispReferencesEvents_Event.ReferenceChanged"/> event
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void OnReferenceChanged(Reference reference)
	{
		// Evs.Trace(GetType(), nameof(OnReferenceChanged), $"Project: {reference?.ContainingProject?.Name}.");

		NativeDb.ReindexEntityFrameworkAssembliesAsyui(reference.ContainingProject);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Event handler for the Project
	/// <see cref="_dispReferencesEvents_Event.ReferenceRemoved"/> event
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void OnReferenceRemoved(Reference reference)
	{
		// Evs.Trace(GetType(), nameof(OnReferenceRemoved), $"Project: {reference?.ContainingProject?.Name}.");

		NativeDb.ReindexEntityFrameworkAssembliesAsyui(reference.ContainingProject);
	}
	*/


	#endregion Events and Event handling


}