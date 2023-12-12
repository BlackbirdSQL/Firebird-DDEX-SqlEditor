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
using BlackbirdSql.Core.Ctl.Extensions;
using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.Core.Model;

using EnvDTE;

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
/// Performs validations and updates of solution project items.
/// </summary>
/// <remarks>
/// Updates the app.config for DbProvider and EntityFramework and updates existing .edmx models that
/// are using a legacy provider.
/// Also ensures we never do validations of a solution and project app.config and .edmx models twice.
/// </remarks>
// =========================================================================================================
public class ControllerEventsManager : AbstractEventsManager
{

	int _RefCnt = 0;

	// A sync call has taken over. Async is locked out or abort at the first opportunity.
	protected CancellationTokenSource _ValidationTokenSource = null;
	protected CancellationToken _ValidationToken;

	protected Task<bool> _ValidationTask;


	private _dispReferencesEvents_ReferenceAddedEventHandler _ReferenceAddedEventHandler = null;





	// =========================================================================================================
	#region Property accessors - SolutionEventsManager
	// =========================================================================================================

	protected ControllerAsyncPackage ControllerPackage => (ControllerAsyncPackage)DdexPackage;


	#endregion Property accessors





	// =========================================================================================================
	#region Constructors / Destructors - SolutionEventsManager
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Private singleton .ctor
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
		Controller.OnAfterCloseSolutionEvent -= OnAfterCloseSolution;
		Controller.OnQueryCloseSolutionEvent -= OnQueryCloseSolution;
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Methods - SolutionEventsManager
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Hooks onto the controller's solution events and performs a initial solution
	/// validation.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override void Initialize()
	{
		if (!ThreadHelper.CheckAccess())
		{
			COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
			Diag.Dug(exc);
			throw exc;
		}

		// Controller.OnAfterDocumentWindowHideEvent += OnAfterDocumentWindowHide;
		// Controller.OnQueryCloseProjectEvent += OnQueryCloseProject;
		Controller.OnQueryCloseSolutionEvent += OnQueryCloseSolution;
		Controller.OnAfterCloseSolutionEvent += OnAfterCloseSolution;

		// This is a once off procedure for solutions and their projects. (ie. Once validated always validated)
		// We're going to check each project that gets loaded (or has a reference added) if it
		// references EntityFramework.Firebird.dll else FirebirdSql.Data.FirebirdClient.dll.
		// If it is we'll check the app.config DbProvider and EntityFramework sections and update if necessary.
		// We also check (once and only once) within a project for any Firebird edmxs with legacy settings and update
		// those, because they cannot work with newer versions of EntityFramework.Firebird.
		// (This validation can be disabled in Visual Studio's options.)


		if ((Uig.ValidateConfig || Uig.ValidateEdmx) && Dte.Solution.Projects != null
			&& Dte.Solution.Projects.Count > 0)
		{
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


			// We only ever go through this once so if a solution was previously in a validation state
			// it is now validated
			/*
			if (Uig.IsValidatedStatus(SolutionGlobals))
			{
				Uig.SetIsValidStatus(SolutionGlobals, true);
			}
			else
			{
				int projectCount = _Dte.Solution.Projects.Count;
				_ = Task.Run(() => ValidateSolutionAsync(projectCount));
			}
			*/
			if (!Uig.IsValidatedStatus(SolutionGlobals))
			{
				// Diag.Trace("Validating solution");
				// To keep the UI thread free and to allow it to perform status
				// updates we move off of the UI thread and then make calls back
				// to the UI thread for each top-level project validation.
				int projectCount = Dte.Solution.Projects.Count;
				_ = Task.Run(() => ValidateSolutionAsync(projectCount));
			}
		}

		// Add ReferencesEvents event handling to C# and VB projects
		/// AddReferenceAddedEventHandler(_Dte);

		// Diag.Trace("Package Controller is ready");

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


		// Projects may have already been opened. They may be irrelevant eg. unloaded
		// project items or other non-project files, but we have to check anyway.
		// Performance is a priority here, not compact code, because we're synchronous on the main
		// thread, so we stay within the: 
		// Projects > Project > ProjectItems > SubProject > ProjectItems... structure.
		// We want to be in and out of here as fast as possible so every possible low overhead check
		// is done first to ensure that.
		_ValidationTask = Task.Factory.StartNew(() =>
		{
			Stopwatch stopwatch = new();

			TaskHandlerProgress(0, 0);

			for (int i = 0; i < projectCount; i++)
			{
				if (_TaskHandler.UserCancellation.IsCancellationRequested)
					_ValidationTokenSource.Cancel();
				if (_ValidationToken.IsCancellationRequested)
				{
					// TaskHandlerProgress(100, stopwatch.Elapsed.Milliseconds);
					break;
				}


				// Go to back of UI thread.
				if (!RecursiveValidateSolutionProjectAsync(i, stopwatch).Result)
					i = projectCount - 1;
				TaskHandlerProgress((i + 1) * 100 / projectCount, stopwatch.Elapsed.Milliseconds);
			}


			if (_TaskHandler.UserCancellation.IsCancellationRequested)
			{
				UpdateStatusBar("Cancelled BlackbirdSql solution validation", true);
				return false;
			}

			UpdateStatusBar($"Completed BlackbirdSql solution validation in {stopwatch.ElapsedMilliseconds}ms", true);



			// _TaskHandler = null;
			// _ProgressData = default;

			return true;
		},
		default, TaskCreationOptions.PreferFairness | TaskCreationOptions.AttachedToParent, TaskScheduler.Default);


		_TaskHandler.RegisterTask(_ValidationTask);

		return true;
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Moves back onto the UI thread and validates the next top-level project.
	/// </summary>
	/// <param name="index">Index of the next project</param>
	// ---------------------------------------------------------------------------------
	protected async Task<bool> RecursiveValidateSolutionProjectAsync(int index, System.Diagnostics.Stopwatch stopwatch)
	{
		await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

		int i = 0;
		Project project = null;

		// The enumerator is not accessable thru GetEnumerator()
		foreach (Project proj in Dte.Solution.Projects)
		{
			if (i == index)
			{
				project = proj;
				break;
			}
			i++;
		}
		if (project == null)
			return false;


		if (project.Globals != null && !Uig.IsScannedStatus(project))
		{
			stopwatch.Start();

			RecursiveValidateProject(project, true);

			stopwatch.Stop();
		}

		return true;
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
				if (!Uig.IsScannedStatus(projectItem.SubProject))
					RecursiveValidateProject(projectItem.SubProject, validatingSolution);
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
	bool RecursiveValidateProjectItem(ProjectItem item, bool validatingSolution)
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

				if (!RecursiveValidateProjectItem(subitem, validatingSolution))
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
	private void RecursiveValidateProject(Project project, bool validatingSolution)
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
				// Diag.Trace("Recursing ProjectFolder: " + project.Name);
				RecursiveValidateProject(project.ProjectItems, validatingSolution);
			}
			/*
			else
			{
				// Diag.Trace("No items in ProjectFolder: " + project.Name);
			}
			*/
		}
		else
		{
			// Diag.Trace("Recursive validate project: " + project.Name);

			bool failed = false;

			if (Uig.IsValidExecutableProjectType(DteSolution, project))
			{
				// Diag.Trace("ValidExecutable");
				if (Uig.ValidateConfig)
				{
					bool isConfiguredEFStatus = Uig.IsConfiguredEFStatus(project);
					bool isConfiguredDbProviderStatus = Uig.IsConfiguredDbProviderStatus(project);


					if (!isConfiguredEFStatus || !isConfiguredDbProviderStatus)
					{

						VSProject projectObject = project.Object as VSProject;

						if (!isConfiguredEFStatus)
						{
							if (projectObject.References.Find(SystemData.EFProvider) != null)
							{
								// Diag.Trace(project.Name + " FOUND REFERENCE:" + SystemData.EFProvider);

								isConfiguredEFStatus = true;
								isConfiguredDbProviderStatus = true;

								config ??= GetAppConfigProjectItem(project);
								if (config != null)
								{
									failed |= !ConfigureEntityFramework(config, validatingSolution, false);
								}
								else
								{
									failed = true;
								}
							}
							/* else if ((projectObject4 ??= project.Object as VSProject4) != null && projectObject4.PackageReferences != null)
							{
								if (projectObject4.PackageReferences.TryGetReference(SystemData.EFProvider, null,
									out string pkgVersion, out _, out _))
								{
									// A legacy CSProj must cast to VSProject4 to manipulate package references

									// Diag.Trace(project.Name + " FOUND PACKAGE REFERENCE:" + SystemData.EFProvider);

									isConfiguredEFStatus = true;
									isConfiguredDbProviderStatus = true;

									config ??= GetAppConfigProjectItem(project);
									if (config != null)
									{
										failed |= !ConfigureEntityFramework(config, false);
									}
									else
									{
										failed = true;
									}
								}
							}*/
						}

						if (!isConfiguredDbProviderStatus)
						{
							if (projectObject.References.Find(SystemData.Invariant) != null)
							{
								// Diag.Trace(project.Name + " FOUND REFERENCE:" + SystemData.Invariant);

								isConfiguredDbProviderStatus = true;

								config ??= GetAppConfigProjectItem(project);
								if (config != null)
								{
									failed |= !ConfigureDbProvider(config, validatingSolution, false);
								}
								else
								{
									failed = true;
								}
							}
							/* else if ((projectObject4 ??= project.Object as VSProject4) != null && projectObject4.PackageReferences != null)
							{

								if (projectObject4.PackageReferences.TryGetReference(SystemData.Invariant, null,
									out string pkgVersion, out _, out _))
								{
									// Diag.Trace(project.Name + " FOUND PACKAGE REFERENCE:" + SystemData.Invariant);

									isConfiguredDbProviderStatus = true;

									config ??= GetAppConfigProjectItem(project);
									if (config != null)
									{
										failed |= !ConfigureDbProvider(config, false);
									}
									else
									{
										failed = true;
									}
								}
							} */
						}

						if (!isConfiguredEFStatus || !isConfiguredDbProviderStatus)
						{
							AddReferenceAddedEventHandler(/*projectObject4 != null ? projectObject4.Events :*/ projectObject.Events);
						}


					}
				}



				if (Uig.ValidateEdmx && !Uig.IsValidStatus(SolutionGlobals) && !Uig.IsUpdatedEdmxsStatus(project))
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

							if (!RecursiveValidateProjectItem(item, validatingSolution))
								success = false;
						}
					}
					catch (Exception ex)
					{
						success = false;
						Diag.Dug(ex);
					}

					if (success)
						Uig.SetIsUpdatedEdmxsStatus(project);
					else
						Uig.IsValidateFailedStatus = true;

				}

			}



			if (failed)
				Uig.IsValidateFailedStatus = true;
			else
				Uig.SetIsScannedStatus(project);


		}
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

			// Diag.Trace("App.config is null. Added: " + filename);
		}

		// if (config == null)
			// Diag.Trace(project.Name + ": app.config is null");

		return config;

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Checks if a project has FirebirdSql.Data.FirebirdClient configured in the app.config and configures it if it doesn't
	/// </summary>
	/// <param name="project"></param>
	/// <returns>true if completed successfully else false if there were errors.</returns>
	// ---------------------------------------------------------------------------------
	private bool ConfigureDbProvider(ProjectItem config, bool validatingSolution, bool invalidate)
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

		if (Uig.IsConfiguredDbProviderStatus(config.ContainingProject))
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

		Uig.SetIsValidatedDbProviderStatus(config.ContainingProject);

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
	private bool ConfigureEntityFramework(ProjectItem config, bool validatingSolution, bool invalidate)
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

		if (Uig.IsConfiguredEFStatus(config.ContainingProject))
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
				// If Entity framework must be configured then so must the client
				modified = XmlParser.ConfigureEntityFramework(path, !Uig.IsConfiguredDbProviderStatus(config.ContainingProject), SystemData.ProviderFactoryType);
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

		Uig.SetIsValidatedEFStatus(config.ContainingProject);

		if (modified)
		{
			TaskHandlerProgress($">  {config.ContainingProject.Name} -> Updated App.config: DbProvider and EntityFramework");
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
				System.IO.IOException ex = new System.IO.IOException(edmx.ContainingProject.Name + "." + edmx.Name + ": File is open");
				Diag.Dug(ex);
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


	#endregion Methods





	// =========================================================================================================
	#region Asynchronous event handlers - SolutionEventsManager
	// =========================================================================================================


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

		if (!Uig.ValidateConfig || reference.Type != prjReferenceType.prjReferenceTypeAssembly
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

		if (reference.Name.ToLower() == SystemData.EFProvider.ToLower() && !Uig.IsConfiguredEFStatus(reference.ContainingProject))
		{
			// Diag.Trace("HandleReferenceAddedAsync is through for Project: " + reference.ContainingProject.Name + " for Reference: " + reference.Name);

			// Check if is configured again if queue was not clear and there was a Wait()
			if (ClearValidationQueue() || !Uig.IsConfiguredEFStatus(reference.ContainingProject))
			{
				config ??= GetAppConfigProjectItem(reference.ContainingProject);

				if (config != null)
				{
					if (!ConfigureEntityFramework(config, false, false))
						Uig.IsValidateFailedStatus = true;
				}
				else
				{
					Uig.IsValidateFailedStatus = true;
				}
			}
		}
		else if (reference.Name.ToLower() == SystemData.Invariant.ToLower() && !Uig.IsConfiguredDbProviderStatus(reference.ContainingProject))
		{
			// Diag.Trace("HandleReferenceAddedAsync is through for Project: " + reference.ContainingProject.Name + " for Reference: " + reference.Name);

			// Check if is configured again if queue was not clear and there was a Wait()
			if (ClearValidationQueue() || !Uig.IsConfiguredDbProviderStatus(reference.ContainingProject))
			{
				config ??= GetAppConfigProjectItem(reference.ContainingProject);

				if (config != null)
				{
					if (!ConfigureDbProvider(config, false, false))
						Uig.IsValidateFailedStatus = true;
				}
				else
				{
					Uig.IsValidateFailedStatus = true;
				}
			}
		}
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

		if (Uig.IsScannedStatus(project) || !Uig.IsValidExecutableProjectType(DteSolution, project)
			|| (Uig.IsConfiguredDbProviderStatus(project) && Uig.IsConfiguredEFStatus(project) && Uig.IsUpdatedEdmxsStatus(project)))
		{
			// Diag.Trace("Project no validation required");
			return;
		}

		RecursiveValidateProject(project, false);

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
			// Diag.Trace("AfterOpenProject: Possible VS project. Could not get project object property from hierarchy: " + pHierarchy.ToString());
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
	public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
	{
		_ValidationTokenSource?.Cancel();

		if (Uig.IsValidateFailedStatus)
		{
			// Diag.Trace("There was a failed solution validation. Clearing solution status flags");
			Uig.ClearValidateStatus();
		}
		else if (!Uig.IsValidatedStatus(SolutionGlobals))
		{
			Uig.SetIsValidStatus(SolutionGlobals, true);
		}
		else
		{
			// Diag.Trace("The solution has it's validated flag set. Doing nothing.");
		}

		return VSConstants.S_OK;

	}


	#endregion IVs Events Implementation and Event handling





	// =========================================================================================================
	#region Utility Methods and Dictionaries - SolutionEventsManager
	// =========================================================================================================


	// Deadlock warning message suppression
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD002:Avoid problematic synchronous waits",
		Justification = "Code logic ensures a deadlock cannot occur")]

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