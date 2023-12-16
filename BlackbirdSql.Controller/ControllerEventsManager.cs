// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)


using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Ctl.Extensions;
using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.Core.Model;

using EnvDTE;
using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TaskStatusCenter;

using VSLangProj;


namespace BlackbirdSql.Controller;

[SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread",
	Justification = "Class is UIThread compliant.")]

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
public class ControllerEventsManager : AbstractEventsManager
{

	// ---------------------------------------------------------------------------------
	#region Constants - ControllerEventsManager
	// ---------------------------------------------------------------------------------


	private int _RefCnt = 0;

	// A sync call has taken over. Async is locked out or abort at the first opportunity.
	protected CancellationTokenSource _ValidationTokenSource = null;
	protected CancellationToken _ValidationToken;
	protected Task<bool> _ValidationTask;

	private _dispReferencesEvents_ReferenceAddedEventHandler _ReferenceAddedEventHandler = null;


	#endregion Constants




	// =========================================================================================================
	#region Property accessors - SolutionEventsManager
	// =========================================================================================================


	#endregion Property accessors





	// =========================================================================================================
	#region Constructors / Destructors - SolutionEventsManager
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// .ctor
	/// </summary>
	// ---------------------------------------------------------------------------------
	public ControllerEventsManager(IBPackageController controller) : base(controller)
	{
		_TaskHandlerTaskName = "Validation";
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// SolutionEventsManager destructor
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override void Dispose()
	{
		// Controller.OnAfterDocumentWindowHideEvent -= OnAfterDocumentWindowHide;
		// Controller.OnQueryCloseProjectEvent -= OnQueryCloseProject;
		Controller.OnLoadSolutionOptionsEvent -= OnLoadSolutionOptions;
		Controller.OnSaveSolutionOptionsEvent -= OnSaveSolutionOptions;
		Controller.OnAfterCloseSolutionEvent -= OnAfterCloseSolution;
		Controller.OnQueryCloseSolutionEvent -= OnQueryCloseSolution;
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Methods - SolutionEventsManager
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Checks if a project has FirebirdSql.Data.FirebirdClient configured in the app.config and configures it if it doesn't
	/// </summary>
	/// <param name="project"></param>
	/// <returns>true if completed successfully else false if there were errors.</returns>
	// ---------------------------------------------------------------------------------
	private bool ConfigureDbProvider(ProjectItem config, IBGlobalsAgent globals, bool validatingSolution, bool invalidate)
	{
		if (validatingSolution)
		{
			if (_TaskHandler.UserCancellation.IsCancellationRequested)
				_ValidationTokenSource.Cancel();
			if (_ValidationToken.IsCancellationRequested)
				return false;
		}

		if (!ThreadHelper.CheckAccess())
		{
			COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
			Diag.Dug(exc);
			throw exc;
		}

		if (globals.IsConfiguredDbProviderStatus)
			return true;

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
				modified = XmlParser.ConfigureDbProvider(path, SystemData.ProviderFactoryType);
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

		globals.IsValidatedDbProviderStatus = true;

		if (modified)
		{
			TaskHandlerProgress($">  {config.ContainingProject.Name} -> Updated App.config: DbProvider");
		}

		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Checks if a project has EntityFramework.Firebird configured in the app.config and configures it if it doesn't
	/// </summary>
	/// <param name="project"></param>
	/// <returns>true if completed successfully else false if there were errors.</returns>
	// ---------------------------------------------------------------------------------
	private bool ConfigureEntityFramework(ProjectItem config, IBGlobalsAgent globals, bool validatingSolution, bool invalidate)
	{
		if (validatingSolution)
		{
			if (_TaskHandler.UserCancellation.IsCancellationRequested)
				_ValidationTokenSource.Cancel();
			if (_ValidationToken.IsCancellationRequested)
				return false;
		}

		if (!ThreadHelper.CheckAccess())
		{
			COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
			Diag.Dug(exc);
			throw exc;
		}

		if (globals.IsConfiguredEFStatus)
			return true;


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
				modified = XmlParser.ConfigureEntityFramework(path, !globals.IsConfiguredDbProviderStatus, SystemData.ProviderFactoryType);
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

		globals.IsValidatedEFStatus = true;

		if (modified)
		{
			TaskHandlerProgress($">  {config.ContainingProject.Name} -> Updated App.config: DbProvider and EntityFramework");
		}


		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the app.config <see cref="ProjectItem"/> of a <see cref="Project"/>
	/// </summary>
	/// <param name="project"></param>
	/// <returns>true if app.config was updated else false</returns>
	// ---------------------------------------------------------------------------------
	private ProjectItem GetAppConfigProjectItem(Project project, bool createIfNotFound = false)
	{
		if (!ThreadHelper.CheckAccess())
		{
			COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
			Diag.Dug(exc);
			throw exc;
		}

		ProjectItem config = null;

		try
		{
			foreach (ProjectItem item in project.ProjectItems)
			{
				if (item.Name.ToLower() == "app.config")
				{
					config = item;
					break;
				}
			}
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			return null;
		}

		if (createIfNotFound && config == null)
		{
			FileInfo info = new FileInfo(project.FullName);
			string filename = info.Directory.FullName + "\\App.config";
			StreamWriter sw = File.CreateText(filename);

			string xml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<configuration>
</configuration>";

			sw.Write(xml);
			sw.Close();

			config = project.ProjectItems.AddFromFile(filename);
		}

		return config;

	}



	private string GetProjectPath(Project project)
	{
		if (project.Properties == null || project.Properties.Count == 0)
			return null;

		Property fullPath = null;
		Property filename = null;

		try
		{
			fullPath = project.Properties.Item("FullPath");
		}
		catch
		{
		}

		try
		{
			filename = project.Properties.Item("FileName");
		}
		catch
		{
		}

		if (fullPath == null || filename == null)
			return null;

		return $"{fullPath.Value}{filename.Value}";

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Hooks onto the controller's solution events and performs a initial solution
	/// validation.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override void Initialize()
	{
		// Controller.OnAfterDocumentWindowHideEvent += OnAfterDocumentWindowHide;
		// Controller.OnQueryCloseProjectEvent += OnQueryCloseProject;
		Controller.OnLoadSolutionOptionsEvent += OnLoadSolutionOptions;
		Controller.OnSaveSolutionOptionsEvent += OnSaveSolutionOptions;
		Controller.OnQueryCloseSolutionEvent += OnQueryCloseSolution;
		Controller.OnAfterCloseSolutionEvent += OnAfterCloseSolution;

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Identifies whether or not a project is a CPS project
	/// </summary>
	/// <param name="hierarchy"></param>
	/// <returns>true if project is CPS</returns>
	// ---------------------------------------------------------------------------------
	internal static bool IsCpsProject(IVsHierarchy hierarchy)
	{
		Requires.NotNull(hierarchy, "hierarchy");
		return hierarchy.IsCapabilityMatch("CPS");
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Checks wether the project is a valid executable output type that requires
	/// configuration of the app.config and updates the globals.
	/// </summary>
	/// <param name="project"></param>
	/// <returns>
	/// True if the project is a valid C#/VB executable project else false.
	/// </returns>
	/// <remarks>
	/// This method here to avoid 'not on main thread' in GlobalsAgent because in here
	/// we know we're on main thread.
	/// We're not going to worry about anything but C# and VB non=CSP projects
	/// </remarks>
	// ---------------------------------------------------------------------------------
	public bool IsValidExecutableProjectType(Project project, IBGlobalsAgent globals)
	{
		if (globals.IsValidatedStatus)
			return globals.IsValidStatus;

		// We're only supporting C# and VB projects for this - a dict list is at the end of this class
		if (project.Kind != "{F184B08F-C81C-45F6-A57F-5ABD9991F28F}"
			&& project.Kind != "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}")
		{
			globals.IsValidStatus = false;
			return false;
		}

		bool result = false;


		// Don't process CPS projects
		Controller.VsSolution.GetProjectOfUniqueName(project.UniqueName, out IVsHierarchy hierarchy);

		if (!IsCpsProject(hierarchy))
		{
			int outputType = int.MaxValue;

			if (project.Properties != null && project.Properties.Count > 0)
			{
				Property property = null;

				try
				{
					property = project.Properties.Item("OutputType");
				}
				catch
				{
					Tracer.Warning(GetType(), "IsValidExecutableProjectType()", "Project: {0}  has no OutputType property", project.Name);
				}

				if (property != null)
					outputType = (int)property.Value;
			}


			if (outputType < 2)
				result = true;
		}

		globals.IsValidStatus = result;

		return result;

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The _AsyncPayloadLauncher payload.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected async Task<bool> PayloadTaskAsync(int projectCount, CancellationToken asyncCancellationToken,
		CancellationToken userCancellationToken)
	{
		await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

		try
		{
			if (userCancellationToken.IsCancellationRequested || asyncCancellationToken.IsCancellationRequested)
				return false;

			Stopwatch stopwatch = new();

			TaskHandlerProgress(0, 0);
			int i = 0;

			foreach (Project project in ((Solution)GlobalsAgent.SolutionObject).Projects)
			{
				// Go to back of UI thread.
				RecursiveValidateSolutionProject(project, stopwatch);

				if (userCancellationToken.IsCancellationRequested || asyncCancellationToken.IsCancellationRequested)
					i = projectCount - 1;

				TaskHandlerProgress((i + 1) * 100 / projectCount, stopwatch.Elapsed.Milliseconds);

				if (userCancellationToken.IsCancellationRequested || asyncCancellationToken.IsCancellationRequested)
					break;

				i++;
			}

			if (userCancellationToken.IsCancellationRequested)
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
			return false;
		}

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
	private void RecursiveValidateProject(Project project, IBGlobalsAgent globals, bool validatingSolution)
	{
		if (validatingSolution)
		{
			if (_TaskHandler.UserCancellation.IsCancellationRequested)
				_ValidationTokenSource.Cancel();
			if (_ValidationToken.IsCancellationRequested)
				return;
		}

		if (!ThreadHelper.CheckAccess())
		{
			COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
			Diag.Dug(exc);
			throw exc;
		}

		ProjectItem config = null;

		// There's a dict list of these at the end of the class
		if (Kind(project.Kind) == "ProjectFolder")
		{
			if (project.ProjectItems != null && project.ProjectItems.Count > 0)
			{
				RecursiveValidateProject(project.ProjectItems, validatingSolution);
			}
		}
		else
		{
			bool failed = false;

			if (IsValidExecutableProjectType(project, globals))
			{
				if (GlobalsAgent.ValidateConfig)
				{
					bool isConfiguredEFStatus = globals.IsConfiguredEFStatus;
					bool isConfiguredDbProviderStatus = globals.IsConfiguredDbProviderStatus;

					if (!isConfiguredEFStatus || !isConfiguredDbProviderStatus)
					{
						VSProject projectObject = project.Object as VSProject;

						if (!isConfiguredEFStatus)
						{
							if (projectObject.References.Find(SystemData.EFProvider) != null)
							{
								isConfiguredEFStatus = true;
								isConfiguredDbProviderStatus = true;

								config ??= GetAppConfigProjectItem(project);
								if (config != null)
								{
									failed |= !ConfigureEntityFramework(config, globals, validatingSolution, false);
								}
								else
								{
									failed = true;
								}
							}
						}

						if (!isConfiguredDbProviderStatus)
						{
							if (projectObject.References.Find(SystemData.Invariant) != null)
							{
								isConfiguredDbProviderStatus = true;

								config ??= GetAppConfigProjectItem(project);
								if (config != null)
								{
									failed |= !ConfigureDbProvider(config, globals, validatingSolution, false);
								}
								else
								{
									failed = true;
								}
							}
						}

						if (!isConfiguredEFStatus || !isConfiguredDbProviderStatus)
						{
							AddReferenceAddedEventHandler(/*projectObject4 != null ? projectObject4.Events :*/ projectObject.Events);
						}


					}
				}

				// Why SolutionGlobals check ????????????????????????
				if (GlobalsAgent.ValidateEdmx && !GlobalsAgent.SolutionGlobals.IsValidStatus && !globals.IsUpdatedEdmxsStatus)
				{
					bool success = true;

					try
					{
						foreach (ProjectItem item in project.ProjectItems)
						{
							if (validatingSolution)
							{
								if (_TaskHandler.UserCancellation.IsCancellationRequested)
									_ValidationTokenSource.Cancel();
								if (_ValidationToken.IsCancellationRequested)
								{
									success = false;
									break;
								}
							}

							if (!RecursiveValidateProjectItemEdmx(item, validatingSolution))
								success = false;
						}
					}
					catch (Exception ex)
					{
						success = false;
						Diag.Dug(ex);
					}

					if (success)
						globals.IsUpdatedEdmxsStatus = true;
					else
						GlobalsAgent.SolutionGlobals.IsValidateFailedStatus = true;

				}

			}

			if (failed)
				GlobalsAgent.SolutionGlobals.IsValidateFailedStatus = true;
			else
				globals.IsScannedStatus = true;
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Recursively validates projects already opened before our package was sited
	/// This list is tertiary level projects from parent projects (solution folders)
	/// </summary>
	/// <param name="projects"></param>
	// ---------------------------------------------------------------------------------
	protected void RecursiveValidateProject(ProjectItems projectItems, bool validatingSolution)
	{
		if (!ThreadHelper.CheckAccess())
		{
			COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
			Diag.Dug(exc);
			throw exc;
		}

		foreach (ProjectItem projectItem in projectItems)
		{
			if (_TaskHandler != null && _TaskHandler.UserCancellation.IsCancellationRequested)
				break;

			if (projectItem.SubProject != null && projectItem.SubProject.Globals != null)
			{
				IBGlobalsAgent globals = new GlobalsAgent(GetProjectPath(projectItem.SubProject));

				if (!globals.IsScannedStatus)
				{
					RecursiveValidateProject(projectItem.SubProject, globals, validatingSolution);
					globals.Flush();
				}
			}
			else
			{
				// Diag.Trace(projectItem.Name + " projectItem.SubProject is null (Possible Unloaded project or document)");
			}
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
	bool RecursiveValidateProjectItemEdmx(ProjectItem item, bool validatingSolution)
	{
		if (_TaskHandler != null && _TaskHandler.UserCancellation.IsCancellationRequested)
			return false;

		if (!ThreadHelper.CheckAccess())
		{
			COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
			Diag.Dug(exc);
			throw exc;
		}

		if (Kind(item.Kind) == "PhysicalFolder")
		{
			bool success = true;

			foreach (ProjectItem subitem in item.ProjectItems)
			{
				if (validatingSolution)
				{
					if (_TaskHandler.UserCancellation.IsCancellationRequested)
						_ValidationTokenSource.Cancel();
					if (_ValidationToken.IsCancellationRequested)
					{
						success = false;
						break;
					}
				}

				if (!RecursiveValidateProjectItemEdmx(subitem, validatingSolution))
					success = false;
			}

			return success;
		}

		// Diag.Trace(item.ContainingProject.Name + " checking projectitem: " + item.Name + ":" + item.Kind);

		if (validatingSolution)
		{
			if (_TaskHandler.UserCancellation.IsCancellationRequested)
				_ValidationTokenSource.Cancel();
			if (_ValidationToken.IsCancellationRequested)
				return false;
		}

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
			Diag.Stack(item.ContainingProject.Name + ":" + item.Name + " has no link property");
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
			if (!UpdateEdmx(item, validatingSolution, false))
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
	/// Moves back onto the UI thread and validates the next top-level project.
	/// </summary>
	/// <param name="index">Index of the next project</param>
	// ---------------------------------------------------------------------------------
	protected bool RecursiveValidateSolutionProject(Project project, Stopwatch stopwatch)
	{

		if (project.Globals != null)
		{
			GlobalsAgent globals = new(GetProjectPath(project));

			if (!globals.IsScannedStatus)
			{
				stopwatch.Start();

				RecursiveValidateProject(project, globals, true);

				globals.Flush();
				stopwatch.Stop();
			}
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
	private bool UpdateEdmx(ProjectItem edmx, bool validatingSolution, bool invalidate)
	{
		if (validatingSolution)
		{
			if (_TaskHandler.UserCancellation.IsCancellationRequested)
				_ValidationTokenSource.Cancel();
			if (_ValidationToken.IsCancellationRequested)
				return false;
		}

		if (!ThreadHelper.CheckAccess())
		{
			COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
			Diag.Dug(exc);
			throw exc;
		}


		try
		{
			if (edmx.IsOpen)
			{
				Tracer.Warning(GetType(), "UpdateEdmx()", "{0}: edmx file is open: {1}", edmx.ContainingProject.Name, edmx.Name);
				return false;
			}

			if (edmx.FileCount == 0)
				return true;

			string path = edmx.FileNames[0];

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



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Recursively makes calls to <see cref="RecursiveValidateProjectAsync"/>, which is
	/// on the UI thread, from a thread in the thread pool. This is to ensure the UI
	/// thread is not locked up processing validations.
	/// </summary>
	/// <param name="projectCount"></param>
	// ---------------------------------------------------------------------------------
	protected async Task<bool> ValidateSolutionAsync(int projectCount)
	{
		if (projectCount == 0)
			return true;


		await Task.Run(() => UpdateStatusBar("BlackbirdSql validating solution"));


		TaskHandlerOptions options = default;
		options.Title = "BlackbirdSql Solution Validaton";
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

		Task<bool> payload() =>
			PayloadTaskAsync(projectCount, asyncCancellationToken, userCancellationToken);

		// Projects may have already been opened. They may be irrelevant eg. unloaded
		// project items or other non-project files, but we have to check anyway.
		// Performance is a priority here, not compact code, because we're synchronous on the main
		// thread, so we stay within the: 
		// Projects > Project > ProjectItems > SubProject > ProjectItems... structure.
		// We want to be in and out of here as fast as possible so every possible low overhead check
		// is done first to ensure that.
		// Start up the payload launcher with tracking.
		_ValidationTask = await Task.Factory.StartNew(payload, default, creationOptions, scheduler);


		_TaskHandler.RegisterTask(_ValidationTask);

		return true;
	}


	#endregion Methods





	// =========================================================================================================
	#region Asynchronous event handlers - SolutionEventsManager
	// =========================================================================================================


	/*
	/// <summary>
	/// Adds <see cref="OnReferenceAdded"> event handler to the global Project <see cref="_dispReferencesEvents_Event.ReferenceAdded"/> event
	/// </summary>
	/// <param name="dte"></param>
	void AddReferenceAddedEventHandler(DTE dte)
	{
		try
		{

			_ReferenceAddedEventHandler ??= new _dispReferencesEvents_ReferenceAddedEventHandler(OnReferenceAdded);


			Events2 dteEvents = dte.Events as Events2;

			_CSharpReferencesEvents ??= (ReferencesEvents)dteEvents.GetObject("CSharpReferencesEvents");
			_VBReferencesEvents ??= (ReferencesEvents)dteEvents.GetObject("VBReferencesEvents");

			_CSharpReferencesEvents.ReferenceAdded += _ReferenceAddedEventHandler;
			_VBReferencesEvents.ReferenceAdded += _ReferenceAddedEventHandler;

			// Diag.Trace("Added _ReferenceAddedEventHandler for DTE");
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;
		}
	}
	*/



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Adds <see cref="OnReferenceAdded"> event handler to the Project <see cref="_dispReferencesEvents_Event.ReferenceAdded"/> event
	/// </summary>
	/// <param name="dte"></param>
	// ---------------------------------------------------------------------------------
	private void AddReferenceAddedEventHandler(VSProjectEvents events)
	{
		try
		{
			_ReferenceAddedEventHandler ??= new _dispReferencesEvents_ReferenceAddedEventHandler(OnReferenceAdded);

			events.ReferencesEvents.ReferenceAdded += _ReferenceAddedEventHandler;

			// Diag.Trace("Added _ReferenceAddedEventHandler");
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs asynchronous operations on <see cref="OnReferenceAdded(Reference)"/>
	/// </summary>
	/// <param name="reference"></param>
	/// <returns></returns>
	// ---------------------------------------------------------------------------------
	private async Task HandleReferenceAddedAsync(Reference reference)
	{
		await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(); // Debug

		if (!GlobalsAgent.ValidateConfig || reference.Type != prjReferenceType.prjReferenceTypeAssembly
			|| (reference.Name.ToLower() != SystemData.EFProvider.ToLower()
			&& reference.Name.ToLower() != SystemData.Invariant.ToLower()))
		{
			return;
		}

		// await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

		// There simply is no other event that is fired when a project object is complete so we're recycling
		// the referenceadded event to the back of the UI thread queue until it's available.
		// The solution object adopts a fire and forget stragedy when opening projects so it also doesn't keep track.
		// This issue only occurs when a project was opened and our package was given the ide context but not yet sited.
		// It's a single digit recycle count so it's low overhead.
		if (reference.ContainingProject.Properties == null)
		{
			if (++_RefCnt < 1000)
			{
				// Diag.Trace("RECYCLING HandleReferenceAddedAsync for Project: " + reference.ContainingProject.Name + " for Reference: " + reference.Name);

				_ = Task.Delay(200).ContinueWith(_ => HandleReferenceAddedAsync(reference), TaskScheduler.Current);
			}
			return;
		}


		// If anything gets through things are still happening so we can reset and allow references with incomplete project objects
		// to continue recycling
		_RefCnt = 0;

		ProjectItem config = null;
		IBGlobalsAgent globals = null;

		if (reference.Name.ToLower() == SystemData.EFProvider.ToLower()
			|| reference.Name.ToLower() == SystemData.Invariant.ToLower())
		{
			globals = new GlobalsAgent(GetProjectPath(reference.ContainingProject));
		}

		if (reference.Name.ToLower() == SystemData.EFProvider.ToLower() && !globals.IsConfiguredEFStatus)
		{
			// Diag.Trace("HandleReferenceAddedAsync is through for Project: " + reference.ContainingProject.Name + " for Reference: " + reference.Name);

			// Check if is configured again if queue was not clear and there was a Wait()
			if (ClearValidationQueue() || !globals.IsConfiguredEFStatus)
			{
				config ??= GetAppConfigProjectItem(reference.ContainingProject);

				if (config != null)
				{
					if (!ConfigureEntityFramework(config, globals, false, false))
						GlobalsAgent.SolutionGlobals.IsValidateFailedStatus = true;
				}
				else
				{
					GlobalsAgent.SolutionGlobals.IsValidateFailedStatus = true;
				}
			}
		}
		else if (reference.Name.ToLower() == SystemData.Invariant.ToLower() && !globals.IsConfiguredDbProviderStatus)
		{
			// Diag.Trace("HandleReferenceAddedAsync is through for Project: " + reference.ContainingProject.Name + " for Reference: " + reference.Name);

			// Check if is configured again if queue was not clear and there was a Wait()
			if (ClearValidationQueue() || !globals.IsConfiguredDbProviderStatus)
			{
				config ??= GetAppConfigProjectItem(reference.ContainingProject);

				if (config != null)
				{
					if (!ConfigureDbProvider(config, globals, false, false))
						GlobalsAgent.SolutionGlobals.IsValidateFailedStatus = true;
				}
				else
				{
					GlobalsAgent.SolutionGlobals.IsValidateFailedStatus = true;
				}
			}
		}

		globals?.Flush();
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs asychronous operations on <see cref="IVsSolutionEvents.OnAfterOpenProject"/>
	/// </summary>
	/// <param name="project"></param>
	/// <returns></returns>
	// ---------------------------------------------------------------------------------
	private async Task HandleAfterOpenProjectAsync(Project project)
	{
		await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

		// Recycle until project object is complete if necessary
		if (project.Properties == null)
		{
			if (++_RefCnt < 1000)
			{
				// Diag.Trace("RECYCLING Project: " + project.Name + " for AfterOpenProject");

				_ = Task.Delay(200).ContinueWith(_ => HandleAfterOpenProjectAsync(project), TaskScheduler.Current);
			}
			return;
		}

		ClearValidationQueue();

		// Diag.Trace("Project opened");
		// If anything gets through things are still happening so we can reset and allow events with incomplete project objects
		// to continue recycling
		_RefCnt = 0;

		IBGlobalsAgent globals = new GlobalsAgent(GetProjectPath(project));

		if (globals.IsScannedStatus || !IsValidExecutableProjectType(project, globals)
			|| (globals.IsConfiguredDbProviderStatus && globals.IsConfiguredEFStatus && globals.IsUpdatedEdmxsStatus))
		{
			// Diag.Trace("Project no validation required");
			return;
		}

		RecursiveValidateProject(project, globals, false);
		globals.Flush();

	}


	#endregion Asynchronous event handlers





	// =========================================================================================================
	#region IVs Events Implementation and Event handling - SolutionEventsManager
	// =========================================================================================================


	// Events that we handle are listed first




	// ---------------------------------------------------------------------------------
	/// <summary>
	/// /// Event handler for the Project
	/// <see cref="_dispReferencesEvents_Event.ReferenceAdded"/> event
	/// </summary>
	/// <param name="reference"></param>
	// ---------------------------------------------------------------------------------
	private void OnReferenceAdded(Reference reference) => _ = HandleReferenceAddedAsync(reference);



	//	[SuppressMessage("Usage", "VSTHRD104:Offer async methods")]
	public void OnLoadSolutionOptions(Stream stream)
	{

		GlobalsAgent.SolutionGlobals?.Dispose();
		GlobalsAgent.SolutionGlobals = new GlobalsAgent(Controller.Dte.Solution, stream);

		// This is a once off procedure for solutions and their projects. (ie. Once validated always validated)
		// We're going to check each project that gets loaded (or has a reference added) if it
		// references EntityFramework.Firebird.dll else FirebirdSql.Data.FirebirdClient.dll.
		// If it is we'll check the app.config DbProvider and EntityFramework sections and update if necessary.
		// We also check (once and only once) within a project for any Firebird edmxs with legacy settings and update
		// those, because they cannot work with newer versions of EntityFramework.Firebird.
		// (This validation can be disabled in Visual Studio's options.)

		if ((GlobalsAgent.ValidateConfig || GlobalsAgent.ValidateEdmx) && !GlobalsAgent.SolutionGlobals.IsValidatedStatus)
		{
			if (!ThreadHelper.CheckAccess())
			{
				_ = Task.Run(OnLoadSolutionOptionsAsync);
				return;
			}

			OnLoadSolutionOptionsImpl();
		}
	}

	private async Task<bool> OnLoadSolutionOptionsAsync()
	{
		await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
		OnLoadSolutionOptionsImpl();

		return true;
	}


	private void OnLoadSolutionOptionsImpl()
	{
		// On main thread

		if (((Solution)GlobalsAgent.SolutionObject).Projects == null
			&& ((Solution)GlobalsAgent.SolutionObject).Projects.Count == 0)
		{
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
		int projectCount = ((Solution)GlobalsAgent.SolutionObject).Projects.Count;
		_ = Task.Run(() => ValidateSolutionAsync(projectCount));

	}

	public void OnSaveSolutionOptions(Stream stream)
	{
		_ValidationTokenSource?.Cancel();

		if (GlobalsAgent.SolutionGlobals == null)
			return;

		if (GlobalsAgent.SolutionGlobals.IsValidateFailedStatus)
		{
			GlobalsAgent.SolutionGlobals.ClearValidateStatus();
		}
		else if (!GlobalsAgent.SolutionGlobals.IsValidatedStatus)
		{
			GlobalsAgent.SolutionGlobals.IsValidStatus = true;
		}

		GlobalsAgent.SolutionGlobals.Flush(stream);
		GlobalsAgent.SolutionGlobals.Dispose();
		GlobalsAgent.SolutionGlobals = null;
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Event handler for the <see cref="IVsSolutionEvents"/> AfterCloseSolution event
	/// </summary>
	/// <param name="pUnkReserved"></param>
	/// <returns></returns>
	// ---------------------------------------------------------------------------------
	public int OnAfterCloseSolution(object pUnkReserved)
	{
		// Reset configured connections registration.
		ConnectionLocator.Reset();
		// Reset the unique database connection DatasetKeys.
		CsbAgent.Reset();

		return VSConstants.S_OK;

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Event handler for <see cref="IVsSolutionEvents"/> AfterOpenProject event
	/// </summary>
	/// <param name="pHierarchy"></param>
	/// <param name="fAdded"></param>
	/// <returns></returns>
	// ---------------------------------------------------------------------------------
	public int OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
	{
		if (!ThreadHelper.CheckAccess())
		{
			COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
			Diag.Dug(exc);
			throw exc;
		}

		// Get the root (project) node. 
		var itemid = VSConstants.VSITEMID_ROOT;

		pHierarchy.GetProperty(itemid, (int)__VSHPROPID.VSHPROPID_ExtObject, out object objProj);


		if (objProj is not Project project)
		{
			return VSConstants.S_OK;
		}
		else if (Kind(project.Kind) != "VbProject"
			&& Kind(project.Kind) != "C#Project")
		{
			return VSConstants.S_OK;
		}

		_ = HandleAfterOpenProjectAsync(project);

		return VSConstants.S_OK;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Event handler for the <see cref="IVsSolutionEvents"/> QueryCloseSolution event
	/// </summary>
	/// <param name="pUnkReserved"></param>
	/// <param name="pfCancel"></param>
	/// <returns></returns>
	// ---------------------------------------------------------------------------------
	public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel) => VSConstants.S_OK;


	#endregion IVs Events Implementation and Event handling





	// =========================================================================================================
	#region Utility Methods and Dictionaries - SolutionEventsManager
	// =========================================================================================================



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Ensures the validation queue is cleared out before passing control back to any
	/// event handler request.
	/// </summary>
	/// <returns>
	/// True if the queue was clear else false if there was a Wait() for the queue to
	/// clear
	/// </returns>
	// ---------------------------------------------------------------------------------
	[SuppressMessage("Usage", "VSTHRD002:Avoid problematic synchronous waits",
		Justification = "Code logic ensures a deadlock cannot occur")]
	public bool ClearValidationQueue()
	{
		if (_ValidationTask == null || _ValidationTask.IsCompleted)
			return true;


		// Notwithstanding that the validation process is pretty fast, unless the user
		// requested it, it makes no sense to cancel the validation process because it
		// is most likely the ClearValidationQueue() call has come from a project or
		// reference load that was late and should have been included in the validation
		// process anyway. So we're taking out the auto token cancel here.
		// Just Wait()...
		// _ValidationTokenSource.Cancel();
		_ValidationTask.Wait();

		return false;
	}


	#endregion Utility Methods and Dictionaries

}