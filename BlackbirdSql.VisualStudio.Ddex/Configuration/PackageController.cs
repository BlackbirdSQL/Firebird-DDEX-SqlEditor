﻿// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TaskStatusCenter;
using Microsoft.VisualStudio.Threading;

using EnvDTE;
using VSLangProj;

using FirebirdSql.Data.FirebirdClient;

using BlackbirdSql.Common;
using BlackbirdSql.Common.Extensions;
using BlackbirdSql.Common.Providers;



namespace BlackbirdSql.VisualStudio.Ddex.Configuration;



// =========================================================================================================
//										VsPackageController Class
//
/// <summary>
/// Manages package events and settings
/// </summary>
/// <remarks>
/// Also updates the app.config for DbProvider and EntityFramework and updates existing .edmx models that
/// are using a legacy provider.
/// Also ensures we never do validations of a solution and project app.config and .edmx models twice.
/// </remarks>
// =========================================================================================================
internal class PackageController : IVsSolutionEvents, IPackageController, ITaskHandlerClient, IDisposable
{

	// ---------------------------------------------------------------------------------
	#region Variables
	// ---------------------------------------------------------------------------------


	private static PackageController _Instance = null;
	private static IVsUIHierarchy _MiscHierarchy = null;

	int _RefCnt = 0;

	private readonly Package _DdexPackage;
	private readonly DTE _Dte = null;
	private readonly IVsSolution _Solution = null;
	private uint _HSolutionEvents = uint.MaxValue;
	private VsGlobalsAgent _Uig;

	private string _ApplicationDataDirectory = null;


	private _dispReferencesEvents_ReferenceAddedEventHandler _ReferenceAddedEventHandler = null;

	protected TaskProgressData _ProgressData = default;
	protected ITaskHandler _TaskHandler = null;


	// A sync call has taken over. Async is locked out or abort at the first opportunity.
	protected CancellationTokenSource _ValidationTokenSource = null;
	protected CancellationToken _ValidationToken;


	protected Task<bool> _ValidationTask;

	#endregion Variables





	// =========================================================================================================
	#region Constants - VsPackageController
	// =========================================================================================================


	const int S_OK = VSConstants.S_OK;


	#endregion Constants





	// =========================================================================================================
	#region Property Accessors - VsPackageController
	// =========================================================================================================



	public string ApplicationDataDirectory
	{
		get
		{
			ThreadHelper.ThrowIfNotOnUIThread();
			if (_ApplicationDataDirectory == null)
			{
				try
				{
					IVsShell service = DdexPackage.GetService<SVsShell, IVsShell>();
					if (service == null)
					{
						ArgumentException ex = new("Could not get service <SVsShell, IVsShell>");
						Diag.Dug(ex);
						throw ex;
					}

					NativeMethods.WrapComCall(service.GetProperty((int)__VSSPROPID.VSSPROPID_AppDataDir, out object pvar));
					_ApplicationDataDirectory = pvar as string;
				}
				catch (Exception ex)
				{
					Diag.Dug(ex);
					throw ex;
				}
			}

			return _ApplicationDataDirectory;
		}
	}



	public Package DdexPackage
	{
		get
		{
			return _DdexPackage;
		}
	}

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Accessor to the singleton <see cref="VsGlobalsAgent"/> instance
	/// </summary>
	// ---------------------------------------------------------------------------------
	VsGlobalsAgent Uig
	{
		get
		{
			return _Uig ??= VsGlobalsAgent.GetInstance(_Dte);
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Accessor to <see cref="_Solution.Globals"/>
	/// </summary>
	// ---------------------------------------------------------------------------------
	Globals SolutionGlobals
	{
		get
		{
#pragma warning disable VSTHRD010 // Invoke single-threaded types on Main thread
			return _Dte.Solution.Globals;
#pragma warning restore VSTHRD010 // Invoke single-threaded types on Main thread
		}
	}


	#endregion Property Accessors





	// =========================================================================================================
	#region Constructors / Destructors - VsPackageController
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Private singleton .ctor
	/// </summary>
	// ---------------------------------------------------------------------------------
	private PackageController(Package package, DTE dte, IVsSolution solution)
	{
		_DdexPackage = package;
		_Dte = dte;
		_Solution = solution;

		_ValidationTask = null;

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the singleton VsPackageController instance
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static PackageController Instance()
	{
		return _Instance;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets or creates the singleton VsPackageController instance
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static PackageController Instance(Package package, DTE dte, IVsSolution solution)
	{
		return _Instance ??= new(package, dte, solution);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// VsPackageController destructor
	/// </summary>
	// ---------------------------------------------------------------------------------
	public void Dispose()
	{
		// We should already be on UI thread. Callers must ensure this can never happen
		ThreadHelper.ThrowIfNotOnUIThread();

		UnadviseSolutionEvents(true);
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Methods - VsPackageController
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Imitates OnOpen events for projects already loaded and then hooks onto solution events
	/// </summary>
	// ---------------------------------------------------------------------------------
	public void AdviseSolutionEvents()
	{
		if (_ReferenceAddedEventHandler != null)
			return;

		// We should already be on UI thread. Callers must ensure this can never happen
		ThreadHelper.ThrowIfNotOnUIThread();

		// Diag.Trace();


		// Sanity check. Disable events if enabled
		UnadviseSolutionEvents(false);


		// This is a once off procedure for solutions and their projects. (ie. Once validated always validated)
		// We're going to check each project that gets loaded (or has a reference added) if it
		// references EntityFramework.Firebird.dll else FirebirdSql.Data.FirebirdClient.dll.
		// If it is we'll check the app.config DbProvider and EntityFramework sections and update if necessary.
		// We also check (once and only once) within a project for any Firebird edmxs with legacy settings and update
		// those, because they cannot work with newer versions of EntityFramework.Firebird.
		// (This validation can be disabled in Visual Studio's options.)


		// Enable solution event capture
		_Solution.AdviseSolutionEvents(this, out _HSolutionEvents);


		// Raise open project event handler for projects that are already loaded
		// This can happen on IDE startup and a solution was opened with some projects
		// loaded before our package was sited.
		if (_Dte == null || _Dte.Solution == null || _Dte.Solution.Globals == null)
		{
			Diag.Dug(true, "DTE.Solution is invalid");
		}
		else if ((Uig.ValidateConfig || Uig.ValidateEdmx) && _Dte.Solution.Projects != null
			&& _Dte.Solution.Projects.Count > 0)
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
				int projectCount = _Dte.Solution.Projects.Count;
				_ = Task.Run(() => ValidateSolutionAsync(projectCount));
			}
		}

		// Add ReferencesEvents event handling to C# and VB projects
		/// AddReferenceAddedEventHandler(_Dte);

		// Diag.Trace("Package Controller is ready");

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Disables solution event invocation
	/// </summary>
	/// <param name="disposing"></param>
	// ---------------------------------------------------------------------------------
	public void UnadviseSolutionEvents(bool disposing)
	{
		// We should already be on UI thread. Callers must ensure this can never happen
		ThreadHelper.ThrowIfNotOnUIThread();

		if (_Solution == null)
			return;

		if (_HSolutionEvents != uint.MaxValue)
		{
			_Solution.UnadviseSolutionEvents(_HSolutionEvents);
			_HSolutionEvents = uint.MaxValue;
		}

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


		UpdateStatusBar("BlackbirdSql validating solution");


		IVsTaskStatusCenterService tsc = await ServiceProvider.GetGlobalServiceAsync<SVsTaskStatusCenterService,
			IVsTaskStatusCenterService>(swallowExceptions: false);

		TaskHandlerOptions options = default;
		options.Title = "BlackbirdSql Solution Validaton";
		options.ActionsAfterCompletion = CompletionActions.None;

		_ProgressData = default;
		_ProgressData.CanBeCanceled = true;

		_TaskHandler = tsc.PreRegister(options, _ProgressData);


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
		foreach (Project proj in _Dte.Solution.Projects)
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
		// We should already be on UI thread. Callers must ensure this can never happen
		ThreadHelper.ThrowIfNotOnUIThread();

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

		ThreadHelper.ThrowIfNotOnUIThread();


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
			Diag.Dug(true, item.ContainingProject.Name + ":" + item.Name + " has no link property");
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

		// We should already be on UI thread. Callers must ensure this can never happen
		try
		{
			ThreadHelper.ThrowIfNotOnUIThread();
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;
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

			if (Uig.IsValidExecutableProjectType(_Solution, project))
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
		// We should already be on UI thread. Callers must ensure this can never happen
		try
		{
			ThreadHelper.ThrowIfNotOnUIThread();
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			return null;
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

		if (config == null)
			Diag.Dug(true, project.Name + ": app.config is null");

		return config;

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Checks if a project has FirebirdSql.Data.FirebirdClient configured in the app.config and configures it if it doesn't
	/// </summary>
	/// <param name="project"></param>
	/// <returns>true if completed successfully else false if there were errors.</returns>
	// ---------------------------------------------------------------------------------
	bool ConfigureDbProvider(ProjectItem config, bool validatingSolution, bool invalidate)
	{
		if (validatingSolution)
		{
			if (_TaskHandler.UserCancellation.IsCancellationRequested)
				_ValidationTokenSource.Cancel();
			if (_ValidationToken.IsCancellationRequested)
				return false;
		}

		ThreadHelper.ThrowIfNotOnUIThread();

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
				modified = XmlParser.ConfigureDbProvider(path, typeof(FirebirdClientFactory));
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
	bool ConfigureEntityFramework(ProjectItem config, bool validatingSolution, bool invalidate)
	{
		if (validatingSolution)
		{
			if (_TaskHandler.UserCancellation.IsCancellationRequested)
				_ValidationTokenSource.Cancel();
			if (_ValidationToken.IsCancellationRequested)
				return false;
		}

		ThreadHelper.ThrowIfNotOnUIThread();

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
				modified = XmlParser.ConfigureEntityFramework(path, !Uig.IsConfiguredDbProviderStatus(config.ContainingProject), typeof(FirebirdClientFactory));
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
	bool UpdateEdmx(ProjectItem edmx, bool validatingSolution, bool invalidate)
	{
		if (validatingSolution)
		{
			if (_TaskHandler.UserCancellation.IsCancellationRequested)
				_ValidationTokenSource.Cancel();
			if (_ValidationToken.IsCancellationRequested)
				return false;
		}

		try
		{
			ThreadHelper.ThrowIfNotOnUIThread();
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			return false;
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
		// We should already be on UI thread. Callers must ensure this can never happen
		ThreadHelper.ThrowIfNotOnUIThread();

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
	void AddReferenceAddedEventHandler(VSProjectEvents events)
	{
		// We should already be on UI thread. Callers must ensure this can never happen
		ThreadHelper.ThrowIfNotOnUIThread();

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
	#region Asynchronous event handlers - VsPackageController
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs asynchronous operations on <see cref="OnReferenceAdded(Reference)"/>
	/// </summary>
	/// <param name="reference"></param>
	/// <returns></returns>
	// ---------------------------------------------------------------------------------
	async Task HandleReferenceAddedAsync(Reference reference)
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


		// This condition can only be true if a solution was closed after successfully being validated which means we can sign it off
		// as valid. That means no more recursive updates on edmx's or app.config. Only adding dll references can trigger an app.config update
		// if it wasn't previously done.
		// if (Uig.IsValidatedStatus(SolutionGlobals))
		//	Uig.SetIsValidStatus(SolutionGlobals, true);

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
	async Task HandleAfterOpenProjectAsync(Project project)
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

		// This condition can only be true if a solution was closed after successfully being validated which means we can sign it off
		// as valid. That means no more recursive updates on edmx's or app.config. Only adding dll references can trigger an app.config update
		// if it wasn't previously done.
		// if (Uig.IsValidatedStatus(SolutionGlobals))
		//	Uig.SetIsValidStatus(SolutionGlobals, true);

		ClearValidationQueue();

		// Diag.Trace("Project opened");
		// If anything gets through things are still happening so we can reset and allow events with incomplete project objects
		// to continue recycling
		_RefCnt = 0;

		if (Uig.IsScannedStatus(project) || !Uig.IsValidExecutableProjectType(_Solution, project)
			|| (Uig.IsConfiguredDbProviderStatus(project) && Uig.IsConfiguredEFStatus(project) && Uig.IsUpdatedEdmxsStatus(project)))
		{
			// Diag.Trace("Project no validation required");
			return;
		}

		RecursiveValidateProject(project, false);

	}


	#endregion Asynchronous event handlers





	// =========================================================================================================
	#region IVsSolutionEvents Implementation and Event handling - VsPackageController
	// =========================================================================================================


	// Events that we handle are listed first

	int HandleTemporarySqlItems(IVsHierarchy pHierarchy, int fRemoving)
	{
		ThreadHelper.ThrowIfNotOnUIThread();

		if (_MiscHierarchy == null || !_MiscHierarchy.Equals(pHierarchy))
			return S_OK;

		IVsUIHierarchy miscHierarchy = _MiscHierarchy;
		_MiscHierarchy = null;

		string path;
		var itemid = VSConstants.VSITEMID_ROOT;
		object objProj;


		try
		{
			miscHierarchy.GetProperty(itemid, (int)__VSHPROPID.VSHPROPID_ExtObject, out objProj);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}

		if (objProj == null || objProj is not Project project)
		{
			ArgumentException ex = new($"Could not get hierarchy root for {miscHierarchy}");
			Diag.Dug(ex);
			throw ex;
		}


		if (project.ProjectItems == null || project.ProjectItems.Count == 0)
			return S_OK;

		string sqlDataDirectory = null;

		foreach (ProjectItem projectItem in project.ProjectItems)
		{
			// Diag.Trace("Validating project item: " + projectItem.Name + ":  Kind: " + Kind(projectItem.Kind) + " FileCount: "
			//	+ projectItem.FileCount);



			if (projectItem.FileCount == 0 || Kind(projectItem.Kind) != "MiscItem")
			{
				continue;
			}

			// FileNames is 1 based indexing - How/Why???
			path = projectItem.FileNames[1];

			sqlDataDirectory ??= SqlMonikerHelper.GetTemporaryDataDirectory(ApplicationDataDirectory);



			if (path.StartsWith(sqlDataDirectory + Path.DirectorySeparatorChar,
				StringComparison.InvariantCultureIgnoreCase))
			{
				try
				{
					projectItem.Remove();
				}
				catch (Exception ex)
				{
					Diag.Dug(ex);
					try
					{
						projectItem.Delete();
					}
					catch (Exception exd)
					{
						Diag.Dug(exd);
					}
				}
				try
				{
					File.Delete(path);
				}
				catch (Exception ex)
				{
					Diag.Dug(ex);
				}
			}
		}


		if (sqlDataDirectory != null)
		{
			try
			{

				DirectoryInfo dir = new DirectoryInfo(sqlDataDirectory);
				if (dir.GetFiles("*.*", SearchOption.AllDirectories).Length == 0)
					dir.Delete(true);
			}
			catch { }
		}
		// Diag.Trace();

		return S_OK;
	}

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// /// Event handler for the Project <see cref="_dispReferencesEvents_Event.ReferenceAdded"/> event
	/// </summary>
	/// <param name="reference"></param>
	// ---------------------------------------------------------------------------------
	void OnReferenceAdded(Reference reference) => _ = HandleReferenceAddedAsync(reference);



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Event handler for <see cref="IVsSolutionEvents"/> AfterOpenProject event
	/// </summary>
	/// <param name="pHierarchy"></param>
	/// <param name="fAdded"></param>
	/// <returns></returns>
	// ---------------------------------------------------------------------------------
	int IVsSolutionEvents.OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
	{
		try
		{
			ThreadHelper.ThrowIfNotOnUIThread();
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			return S_OK;
		}

		if (_Dte == null || _Dte.Solution == null || _Dte.Solution.Globals == null)
		{
			Diag.Dug(true, "DTE.Solution is invalid");
			return S_OK;
		}

		// VSITEMID_ROOT gets the current project. 
		// Alternativly, you might have another item id.
		var itemid = VSConstants.VSITEMID_ROOT;

		pHierarchy.GetProperty(itemid, (int)__VSHPROPID.VSHPROPID_ExtObject, out object objProj);

		// var projectItem = objProj as EnvDTE.ProjectItem;
		// ... or ...

		if (objProj is not Project project)
		{
			// Diag.Dug(true, "AfterOpenProject: Possible VS project. Could not get project object property from hierarchy: " + pHierarchy.ToString());
			return S_OK;
		}
		else if (Kind(project.Kind) != "VbProject"
			&& Kind(project.Kind) != "C#Project")
		{
			return S_OK;
		}

		_ = HandleAfterOpenProjectAsync(project);

		return S_OK;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Event handler for the <see cref="IVsSolutionEvents"/> QueryCloseSolution event
	/// </summary>
	/// <param name="pUnkReserved"></param>
	/// <param name="pfCancel"></param>
	/// <returns></returns>
	// ---------------------------------------------------------------------------------
	int IVsSolutionEvents.OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
	{
		_ValidationTokenSource?.Cancel();

		ThreadHelper.ThrowIfNotOnUIThread();

		if (_Dte == null || _Dte.Solution == null || _Dte.Solution.Globals == null)
		{
			Diag.Dug(true, "DTE.Solution is invalid");
			return S_OK;
		}

		if (Uig.IsValidateFailedStatus)
		{
			// Diag.Trace("There was a failed solution validation. Clearing solution status flags");
			Uig.ClearValidateStatus();
		}
		else if (!Uig.IsValidatedStatus(SolutionGlobals))
		{
			// No -> Diag.Trace("The solution has no validation status set. Setting validated to on and valid to off");
			Uig.SetIsValidStatus(SolutionGlobals, true);
		}
		else
		{
			// Diag.Trace("The solution has it's validated flag set. Doing nothing.");
		}

		return S_OK;

	}


	int IVsSolutionEvents.OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
	{
		ThreadHelper.ThrowIfNotOnUIThread();
		return HandleTemporarySqlItems(pHierarchy, fRemoving);
	}





	// Unhandled events follow

	int IVsSolutionEvents.OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy) => S_OK;
	int IVsSolutionEvents.OnAfterOpenSolution(object pUnkReserved, int fNewSolution) => S_OK;
	int IVsSolutionEvents.OnAfterCloseSolution(object pUnkReserved) => S_OK;
	int IVsSolutionEvents.OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved) => S_OK;
	int IVsSolutionEvents.OnBeforeCloseSolution(object pUnkReserved) => S_OK;
	int IVsSolutionEvents.OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy) => S_OK;
	int IVsSolutionEvents.OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel) => S_OK;


	#endregion IVsSolutionEvents Implementation and Event handling





	// =========================================================================================================
	#region Utility Methods and Dictionaries - VsPackageController
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

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Updates the IDE status bar.
	/// </summary>
	/// <remarks>
	/// The bar is only updated at the start of an Execute and end of an Execute. 
	/// </remarks>
	// ---------------------------------------------------------------------------------
	protected bool UpdateStatusBar(string message, bool complete = false)
	{
		return Diag.UpdateStatusBar(message, complete);
	}

	public virtual void RegisterMiscHierarchy(IVsUIHierarchy hierarchy)
	{
		_MiscHierarchy = hierarchy;
	}

	public virtual ITaskHandler GetTaskHandler()
	{
		return _TaskHandler;
	}

	public virtual TaskProgressData GetProgressData()
	{
		return _ProgressData;
	}




	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Moves back onto the UI thread and updates the IDE task handler progress bar
	/// with project update information.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected bool TaskHandlerProgress(string text)
	{
		if (_ProgressData.PercentComplete == null)
		{
			_ProgressData.PercentComplete = 0;
			if (_TaskHandler != null)
				Diag.TaskHandlerProgress(this, "Started");
		}

		Diag.TaskHandlerProgress(this, text);

		return true;
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Moves back onto the UI thread and updates the IDE task handler progress bar.
	/// </summary>
	/// <param name="progress">The % completion of the linkage build.</param>
	/// <param name="elapsed">The time taken to complete the stage.</param>
	// ---------------------------------------------------------------------------------
	protected bool TaskHandlerProgress(int progress, int elapsed)
	{
		bool completed = false;
		string text;

		if (progress == 0)
		{
			text = "Started";
		}
		else if (progress == 100)
		{
			completed = true;
			text = $"Completed. Validation took {elapsed}ms.";
		}
		else if (_TaskHandler.UserCancellation.IsCancellationRequested)
		{
			completed = true;
			text = $"Cancelled. {progress}% completed. Validation took {elapsed}ms.";
		}
		else
		{
			text = $"{progress}% completed after {elapsed}ms.";
		}

		_ProgressData.PercentComplete = progress;

		Diag.TaskHandlerProgress(this, text, completed);

		return true;

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the descriptive name for a DTE object Kind guid string
	/// </summary>
	/// <param name="kind"></param>
	/// <returns>The descriptive name from <see cref="ProjectGuids"/></returns>
	// ---------------------------------------------------------------------------------
	protected string Kind(string kind)
	{
		if (!ProjectGuids.TryGetValue(kind, out string name))
			name = kind;

		return name;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// DTE object Kind guid string descriptive name dictionary
	/// </summary>
	// ---------------------------------------------------------------------------------
	readonly Dictionary<string, string> ProjectGuids = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
	{
		{ "{2150E333-8FDC-42A3-9474-1A3956D46DE8}", "SolutionFolder" },
		{ "{66A26720-8FB5-11D2-AA7E-00C04F688DDE}", "ProjectFolder" },
		{ "{6BB5F8EF-4483-11D3-8BCF-00C04F8EC28C}", "PhysicalFolder" },
		{ "{66A2671F-8FB5-11D2-AA7E-00C04F688DDE}", "MiscItem" },
		{ "{F184B08F-C81C-45F6-A57F-5ABD9991F28F}", "VbProject" },
		{ "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}", "C#Project" }
		/* The above the only one's we care about ATM
		{ "{06A35CCD-C46D-44D5-987B-CF40FF872267}", "DeploymentMergeModule" },
		{ "{14822709-B5A1-4724-98CA-57A101D1B079}", "Workflow(C#)" },
		{ "{20D4826A-C6FA-45DB-90F4-C717570B9F32}", "Legacy(2003)SmartDevice(C#)" },
		{ "{262852C6-CD72-467D-83FE-5EEB1973A190}", "JsProject" },
		{ "{2DF5C3F4-5A5F-47a9-8E94-23B4456F55E2}", "XNA(XBox)" },
		{ "{32F31D43-81CC-4C15-9DE6-3FC5453562B6}", "WorkflowFoundation" },
		{ "{349C5851-65DF-11DA-9384-00065B846F21}", "WebApplicationProject" },
		{ "{3AC096D0-A1C2-E12C-1390-A8335801FDAB}", "Test" },
		{ "{3AE79031-E1BC-11D0-8F78-00A0C9110057}", "SolutionExplorer"},
		{ "{3D9AD99F-2412-4246-B90B-4EAA41C64699}", "WCF" },
		{ "{3EA9E505-35AC-4774-B492-AD1749C4943A}", "DeploymentCab" },
		{ "{4B160523-D178-4405-B438-79FB67C8D499}", "NomadVSProject" },
		{ "{4D628B5B-2FBC-4AA6-8C16-197242AEB884}", "SmartDevice(C#)" },
		{ "{4F174C21-8C12-11D0-8340-0000F80270F8}", "Db(other" },
		{ "{54435603-DBB4-11D2-8724-00A0C9A8B90C}", "VS2015InstallerProjectExtension" },
		{ "{593B0543-81F6-4436-BA1E-4747859CAAE2}", "SharePoint(C#)" },
		{ "{603C0E0B-DB56-11DC-BE95-000D561079B0}", "ASP.NET(MVC 1.0)" },
		{ "{60DC8134-EBA5-43B8-BCC9-BB4BC16C2548}", "WPF" },
		{ "{68B1623D-7FB9-47D8-8664-7ECEA3297D4F}", "SmartDevice(VB.NET)" },
		{ "{66A2671D-8FB5-11D2-AA7E-00C04F688DDE}", "MiscProject" },
		{ "{66A26722-8FB5-11D2-AA7E-00C04F688DDE}", "SolutionItem" },
		{ "{67294A52-A4F0-11D2-AA88-00C04F688DDE}", "UnloadedProject" },
		{ "{6BB5F8EE-4483-11D3-8BCF-00C04F8EC28C}", "ProjectFile" },
		{ "{6BC8ED88-2882-458C-8E55-DFD12B67127B}", "MonoTouch" },
		{ "{6D335F3A-9D43-41b4-9D22-F6F17C4BE596}", "XNA(Win)" },
		{ "{76F1466A-8B6D-4E39-A767-685A06062A39}", "WinPhone8/8.1(Blank/Hub/Webview)" },
		{ "{786C830F-07A1-408B-BD7F-6EE04809D6DB}", "PortableClassLibrary" },
		{ "{8BB2217D-0F2D-49D1-97BC-3654ED321F3B}", "ASP.NET5" },
		{ "{8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942}", "C++Project" },
		{ "{930C7802-8A8C-48F9-8165-68863BCCD9DD}", "WixProject" },
		{ "{978C614F-708E-4E1A-B201-565925725DBA}", "DeploymentSetup" },
		{ "{A1591282-1198-4647-A2B1-27E5FF5F6F3B}", "Silverlight" },
		{ "{A5A43C5B-DE2A-4C0C-9213-0A381AF9435A}", "UniversalWinClassLib" },
		{ "{A860303F-1F3F-4691-B57E-529FC101A107}", "VSToolsApplications(VSTA)" },
		{ "{A9ACE9BB-CECE-4E62-9AA4-C7E7C5BD2124}", "Database" },
		{ "{AB322303-2255-48EF-A496-5904EB18DA55}", "DeploymentSmartDeviceCab" },
		{ "{B69E3092-B931-443C-ABE7-7E7B65F2A37F}", "MicroFW" },
		{ "{BAA0C2D2-18E2-41B9-852F-F413020CAA33}", "VSToolsOffice(VSTO)" },
		{ "{BBD0F5D1-1CC4-42fd-BA4C-A96779C64378}", "SynergexProject" },
		{ "{BC8A1FFA-BEE3-4634-8014-F334798102B3}", "WinStoreProject" },
		{ "{BF6F8E12-879D-49E7-ADF0-5503146B24B8}", "C#Dynamics2012AX-AOT" },
		{ "{C089C8C0-30E0-4E22-80C0-CE093F111A43}", "WinPhone8/8.1App(C#)" },
		{ "{C252FEB5-A946-4202-B1D4-9916A0590387}", "VisualDbTools" },
		{ "{CB4CE8C6-1BDB-4DC7-A4D3-65A1999772F8}", "Legacy(2003)SmartDevice(VB.NET)" },
		{ "{D399B71A-8929-442a-A9AC-8BEC78BB2433}", "XNA(Zune)" },
		{ "{D59BE175-2ED0-4C54-BE3D-CDAA9F3214C8}", "Workflow(VB.NET)" },
		{ "{DB03555F-0C8B-43BE-9FF9-57896B3C5E56}", "WinPhone8/8.1App(VB.NET)" },
		{ "{E24C65DC-7377-472B-9ABA-BC803B73C61A}", "WebSiteProject" },
		{ "{E3E379DF-F4C6-4180-9B81-6769533ABE47}", "ASP.NET(MVC4.0)" },
		{ "{E53F8FEA-EAE0-44A6-8774-FFD645390401}", "ASP.NET(MVC3.0)" },
		{ "{E6FDF86B-F3D1-11D4-8576-0002A516ECE8}", "J#" },
		{ "{EC05E597-79D4-47f3-ADA0-324C4F7C7484}", "SharePoint(VB.NET)" },
		{ "{ECD6D718-D1CF-4119-97F3-97C25A0DFBF9}", "LightSwitchProject" },
		{ "{edcc3b85-0bad-11db-bc1a-00112fde8b61}", "NemerleProject" },
		{ "{EFBA0AD7-5A72-4C68-AF49-83D382785DCF}", "Xamarin.Android/MonoAndroid" },
		{ "{F135691A-BF7E-435D-8960-F99683D2D49C}", "DistributedSystem" },
		{ "{F2A71F9B-5D33-465A-A702-920D77279786}", "F#Project" },
		{ "{F5B4F3BC-B597-4E2B-B552-EF5D8A32436F}", "MonoTouchBinding" },
		{ "{F85E285D-A4E0-4152-9332-AB1D724D3325}", "ASP.NET(MVC2.0)" },
		{ "{F8810EC1-6754-47FC-A15F-DFABD2E3FA90}", "SharePointWorkflow" },
		{ "{FBB4BD86-BF63-432a-A6FB-6CF3A1288F83}", "InstallShieldLimitedEdition" },
		{ ".nuget", "SettingsNuget" }
		*/
	};


	#endregion Utility Methods and Dictionaries

}