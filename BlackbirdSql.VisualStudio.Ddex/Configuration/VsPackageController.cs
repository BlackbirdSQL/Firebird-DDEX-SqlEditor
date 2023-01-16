
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;

using EnvDTE;
using EnvDTE80;
using VSLangProj;

using BlackbirdSql.Common;


namespace BlackbirdSql.VisualStudio.Ddex.Configuration;



// ---------------------------------------------------------------------------------------------------
//
//								VsPackageController Class
//
// ---------------------------------------------------------------------------------------------------


/// <summary>
/// Controls package events and settings
/// </summary>
/// <remarks>
/// Also updates the app.config for EntityFramework and updates existing .edmx models using a legacy provider
/// Also ensures we never do validations of app.config and .edmx models twice
/// </remarks>
internal class VsPackageController : IVsSolutionEvents, IDisposable
{
	#region Private Variables


	static VsPackageController _Instance = null;
	static AsyncPackage _Parent = null;

	bool _ValidateConfig = false;
	bool _ValidateEdmx = false;
	int _RefCnt = 0;


	private DTE _Dte = null;
	private IVsSolution _Solution = null;
	private uint _HSolutionEvents = uint.MaxValue;

	ReferencesEvents _CSharpReferencesEvents = null;
	ReferencesEvents _VBReferencesEvents = null;

	private _dispReferencesEvents_ReferenceAddedEventHandler _GlobalReferenceAddedEventHandler = null;


	#endregion



	// ---------------------------------------------------------------------------------------------------
	//
	#region Constants - VsPackageController
	//
	// ---------------------------------------------------------------------------------------------------

	// The [Project][Solution].Globals global is set to transitory during debug because there seems no way to delete it for testing
	// other than programmatically. It's a single int32 using binary bitwise for the different status settings
#if DEBUG
	const bool G_Persistent = false;
	const string G_Key = "GlobalBlackbirdTransitory"; // For debug
#else
	const bool G_Persistent		= true;
	const string G_Key			= "GlobalBlackbirdPersistent";
#endif

	/// <summary>
	/// For Projects: has been validated (Once it's been validated it's always been validated)
	/// For Solutions: has been loaded and in a validation state if G_ProjectValue is false else validated
	/// </summary>
	const int G_Validated = 1;
	/// <summary>
	/// For Projects: Validated project is a valid executable C#/VB app. (Once [in]valid always [in]valid)
	/// Off: Solution has been loaded and is in a validation state. On: Validated
	/// (Only applicable if G_Validated is set)
	/// </summary>	
	const int G_Valid = 2;
	/// <summary>
	/// The app.config has EF configured and is good to go. (Once successfully configured always configured)
	/// </summary>
	const int G_EFConfigured = 4;
	/// <summary>
	/// Existing legacy edmx's have been updated and are good to go. (Once all successfully updated always updated)
	/// </summary>
	const int G_EdmxsUpdated = 8;
	/// <summary>
	///  If at any point in project validation there was a fail, this is set to true on the solution and the solution Globals
	///  is set to zero
	/// </summary>
	const int G_ValidateFailed = 16;

	const int S_OK = VSConstants.S_OK;


	#endregion



	// ---------------------------------------------------------------------------------------------------
	//
	#region Constructors / Destructors - VsPackageController
	//
	// ---------------------------------------------------------------------------------------------------


	/// <summary>
	/// Private Constructor
	/// </summary>
	private VsPackageController(AsyncPackage parent = null)
	{
		_Parent = parent;

		try
		{
			_ValidateConfig = VsGeneralOptionModel.Instance.ValidateConfig;
			_ValidateEdmx = VsGeneralOptionModel.Instance.ValidateEdmx;

#if DEBUG
			Diag.EnableTrace = VsDebugOptionModel.Instance.EnableTrace;
			Diag.EnableDiagnostics = VsDebugOptionModel.Instance.EnableDiagnostics;
#else
			Diag.EnableTrace = false;
			Diag.EnableDiagnostics = false;
#endif
			Diag.EnableWriteLog = VsGeneralOptionModel.Instance.EnableWriteLog;
			Diag.LogFile = VsGeneralOptionModel.Instance.LogFile;

			VsGeneralOptionModel.Saved += OnGeneralSettingsSaved;
			VsDebugOptionModel.Saved += OnDebugSettingsSaved;

		}
		catch (Exception ex)
		{
			Diag.Dug(ex, "Failed to retrieve Package settings");
		}

	}



	/// <summary>
	/// Gets the singleton VsPackageController instance
	/// </summary>
	public static VsPackageController GetInstance(AsyncPackage parent = null)
	{
		_Instance ??= new(parent);

		if (parent != null && _Parent == null)
			_Parent = parent;

		return _Instance;
	}



	public void Dispose()
	{
		// We should already be on UI thread. Callers must ensure this can never happen
		ThreadHelper.ThrowIfNotOnUIThread();

		UnadviseSolutionEvents(true);
	}


	#endregion



	// ---------------------------------------------------------------------------------------------------
	//
	#region Methods - VsPackageController
	//
	// ---------------------------------------------------------------------------------------------------


	/// <summary>
	/// Fires onload events for projects already loaded and then hooks onto solution events
	/// </summary>
	public void AdviseSolutionEvents(DTE dte, IVsSolution solution)
	{
		// We should already be on UI thread. Callers must ensure this can never happen
		ThreadHelper.ThrowIfNotOnUIThread();


		_Dte = dte;
		_Solution = solution;

		// Sanity check. Disable events if enabled
		UnadviseSolutionEvents(false);


		// We're going to check each project that gets loaded (or has a reference added) if it
		// references EntityFramework.Firebird.dll.
		// If it is we'll check the app.config EntityFramework/providers section for EntityFramework.Firebird
		// and hook into the resolver if it's not there as well as configure the app.config if the VS Option is set


		// Enable solution event capture
		_Solution.AdviseSolutionEvents(this, out _HSolutionEvents);

		// Raise open project event handler for projects that are already loaded
		// This can happen on IDE startup and a solution was opened with some projects
		// loaded before we were given access to the IDE context
		if ((_ValidateConfig || _ValidateEdmx) && _Dte.Solution != null)
		{
			if (_Dte.Solution.Projects != null && _Dte.Solution.Projects.Count > 0)
			{
				// We only ever go through this once so if a solution was previously in a validation state
				// it is now validated
				if (!IsValidStatus(_Dte.Solution.Globals))
				{
					if (IsValidatedStatus(_Dte.Solution.Globals))
					{
						SetIsValidStatus(_Dte.Solution.Globals, true);
					}
					else
					{
						try
						{
							// Projects may have already been opened. They may be irrelevant eg. unloaded
							// project items or other non-project files, but we have to check anyway.
							// Performance is a priority here, not compact code, because we're on the main
							// thread, so we stay within the: 
							// Projects > Project > ProjectItems > SubProject > ProjectItems... structure
							RecursiveValidateProject(_Dte.Solution.Projects);
						}
						catch (Exception ex)
						{
							Diag.Dug(ex);
							throw;
						}
					}
				}
			}
			else
			{
				Diag.Trace("_Dte.Solution.Projects is null - No projects loaded yet.");
			}
		}
		else
		{
			if (_ValidateConfig || _ValidateEdmx)
				Diag.Trace("_Dte.Solution is null");
		}

		// Add ReferencesEvents event handling to C# and VB projects
		AddReferenceAddedEventHandler(_Dte);

	}



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

		if (disposing)
			_Solution = null;
	}



	/// <summary>
	/// Recursively validates projects already opened before our package was sited
	/// This list is the initial top level project list from the parent solution
	/// </summary>
	/// <param name="projects"></param>
	private void RecursiveValidateProject(Projects projects)
	{
		// We should already be on UI thread. Callers must ensure this can never happen
		ThreadHelper.ThrowIfNotOnUIThread();

		foreach (Project project in projects)
			RecursiveValidateProject(project);
	}



	/// <summary>
	/// Recursively validates projects already opened before our package was sited
	/// This list is tertiary level projects from parent projects
	/// </summary>
	/// <param name="projects"></param>
	private void RecursiveValidateProject(ProjectItems projectItems)
	{
		// We should already be on UI thread. Callers must ensure this can never happen
		ThreadHelper.ThrowIfNotOnUIThread();

		foreach (ProjectItem projectItem in projectItems)
		{
			if (projectItem.SubProject != null)
				RecursiveValidateProject(projectItem.SubProject);
			else
				Diag.Trace(projectItem.Name + " projectItem.SubProject is null (Possible Unloaded project or document)");
		}
	}



	/// <summary>
	/// Recursively validates a project already opened before our package was sited
	/// </summary>
	/// <param name="projects"></param>
	/// <remarks>
	/// If the project is valid and has EntityFramework referenced the app.config is checked.
	/// If it doesn't an OnReferenceAdded event is attached.
	/// Updates legacy edmxs in the project
	/// If it's a folder project is checked for child projects
	/// </remarks>
	private void RecursiveValidateProject(Project project)
	{
		// We should already be on UI thread. Callers must ensure this can never happen
		ThreadHelper.ThrowIfNotOnUIThread();

		ProjectItem config;

		if (project.Kind == "{66A26720-8FB5-11D2-AA7E-00C04F688DDE}")
		{
			if (project.ProjectItems != null && project.ProjectItems.Count > 0)
			{
				Diag.Trace("Recursing SolutionFolder: " + project.Name);
				RecursiveValidateProject(project.ProjectItems);
			}
			else
			{
				Diag.Trace("No items in SolutionFolder: " + project.Name);
			}
		}
		else
		{
			Diag.Trace("Recursive validate project: " + project.Name);


			if (IsValidExecutableProjectType(project))
			{
				bool success = false;

				if (_ValidateConfig && !IsConfiguredEFStatus(project))
				{
					Diag.Trace(project.Name + ": Checking for " + SystemData.EFProvider + " reference");

					VSProject projectObject = project.Object as VSProject;

					foreach (Reference reference in projectObject.References)
					{
						if (reference.Type == prjReferenceType.prjReferenceTypeAssembly
							&& reference.Name.ToLower() == SystemData.EFProvider.ToLower())
						{
							Diag.Trace(project.Name + ": FOUND " + SystemData.EFProvider + " REFERENCE");
							success = true;

							config = GetAppConfigProjectItem(project);
							if (config != null)
							{
								if (!ConfigureEntityFramework(config, false))
									SetValidateFailedStatus(_Dte.Solution);
							}

							break;
						}
					}

					if (!success)
						Diag.Trace(project.Name + ": " + SystemData.EFProvider + " not found");
				}


				if (_ValidateEdmx && !IsValidStatus(_Dte.Solution.Globals) && !IsUpdatedEdmxsStatus(project))
				{
					Diag.Trace(project.Name + ": Updating edmxs");

					success = true;

					try
					{
						foreach (ProjectItem item in project.ProjectItems)
						{
							if (!RecursiveValidateProjectItem(item))
								success = false;
						}
					}
					catch (Exception ex)
					{
						success = false;
						Diag.Dug(ex);
					}

					if (success)
						SetIsUpdatedEdmxsStatus(project);
					else
						SetValidateFailedStatus(_Dte.Solution);

					Diag.Trace(project.Name + ": Edmxs updates " + (success ? "successful" : "failed"));
				}

			}

		}
	}



	/// <summary>
	/// Checks if a project item is an edmx and calls UpdateEdmx() if it is.
	/// If it's a project folder recursively calls RecursiveValidateProjectItem()
	/// </summary>
	/// <param name="item"></param>
	/// <returns>true if completed successfully else false if there were errors.</returns>
	bool RecursiveValidateProjectItem(ProjectItem item)
	{
		ThreadHelper.ThrowIfNotOnUIThread();


		if (item.Kind == "{6BB5F8EF-4483-11D3-8BCF-00C04F8EC28C}")
		{
			bool success = true;

			foreach (ProjectItem subitem in item.ProjectItems)
			{
				if (!RecursiveValidateProjectItem(subitem))
					success = false;
			}

			return success;
		}

		Diag.Trace(item.ContainingProject.Name + " checking projectitem: " + item.Name + ":" + item.Kind);

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
			if (!UpdateEdmx(item, false))
				return false;
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			return false;
		}

		return true;

	}



	/// <summary>
	/// Checks wether the project is a valid executable output type that requires configuration of the app.config
	/// </summary>
	/// <param name="project"></param>
	/// <returns>true if the project is a valid C#/VB executable project else false</returns>
	/// <remarks>
	/// We're not going to worry about anything but C# and VB projects
	/// </remarks>
	bool IsValidExecutableProjectType(Project project)
	{
		// We should already be on UI thread. Callers must ensure this can never happen
		try
		{
			ThreadHelper.ThrowIfNotOnUIThread();
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			return false;
		}

		if (IsValidatedStatus(project.Globals))
			return IsValidStatus(project.Globals);

		// We're only supporting C# and VB projects for this - a dict list is at the end of this class
		if (project.Kind != "{F184B08F-C81C-45F6-A57F-5ABD9991F28F}"
			&& project.Kind != "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}")
		{
			SetIsValidStatus(project.Globals, false);
			return false;
		}

		int outputType = int.MaxValue;

		if (project.Properties != null && project.Properties.Count > 0)
		{
			Property property = project.Properties.Item("OutputType");
			if (property != null)
				outputType = (int)property.Value;
		}

		Diag.Trace(project.Name + " output type is " + outputType);


		bool result = false;

		if (outputType < 2)
			result = true;

		SetIsValidStatus(project.Globals, result);

		return result;

	}





	/// <summary>
	/// Checks if a project has Firebird EntityFramework configured in the app.config and configures it if it doesn't
	/// </summary>
	/// <param name="project"></param>
	/// <returns>true if app.config was updated else false</returns>
	private ProjectItem GetAppConfigProjectItem(Project project)
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
			Diag.Trace(project.Name + ": Getting app.config");

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

			if (config == null)
			{
				Diag.Dug(true, project.Name + ": app.config is null");
				return null;
			}

		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			return null;
		}

		return config;

	}




	/// <summary>
	/// Checks if a project has Firebird EntityFramework configured in the app.config and configures it if it doesn't
	/// </summary>
	/// <param name="project"></param>
	/// <returns>true if completed successfully else false if there were errors.</returns>
	bool ConfigureEntityFramework(ProjectItem config, bool invalidate)
	{

		ThreadHelper.ThrowIfNotOnUIThread();

		if (IsConfiguredEFStatus(config.ContainingProject))
			return true;

		bool modified;


		try
		{
			Diag.Trace(config.ContainingProject.Name + " Checking app.config EF: Config IsOpen: " + config.IsOpen + " - IsDirty: " + config.IsDirty);

			if (config.IsOpen)
			{
				System.IO.IOException ex = new System.IO.IOException(config.ContainingProject.Name + " File is open: app.config");
				Diag.Dug(ex);
				return false;
			}

			if (config.FileCount == 0)
			{
				Diag.Dug(true, config.ContainingProject.Name + ": No app.config files exist");
				return false;
			}

			string path = config.FileNames[0];

			try
			{
				modified = DbXmlUpdater.ConfigureEntityFramework(path);
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

		SetIsValidatedAppConfigStatus(config.ContainingProject);

		if (modified)
			Diag.Trace(config.ContainingProject.Name + ": App.config was modified");


		return true;
	}




	/// <summary>
	/// Checks if an edmx is using the Legacy provider and updates it to the current provider if it is
	/// </summary>
	/// <param name="project"></param>
	/// <returns>true if completed successfully else false if there were errors.</returns>
	bool UpdateEdmx(ProjectItem edmx, bool invalidate)
	{
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
			Diag.Trace(edmx.ContainingProject.Name + "." + edmx.Name + ": Checking edmx: Edmx IsOpen: " + edmx.IsOpen + " - IsDirty: " + edmx.IsDirty);

			if (edmx.IsOpen)
			{
				System.IO.IOException ex = new System.IO.IOException(edmx.ContainingProject.Name + "." + edmx.Name + ": File is open");
				Diag.Dug(ex);
				return false;
			}

			if (edmx.FileCount == 0)
			{
				Diag.Dug(true, edmx.ContainingProject.Name + "." + edmx.Name + ": No files found");
				return true;
			}

			string path = edmx.FileNames[0];
			Diag.Trace(edmx.ContainingProject.Name + "." + edmx.Name + ": config file path: " + path);

			if (!DbXmlUpdater.UpdateEdmx(path))
				return true;

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


		Diag.Trace(edmx.ContainingProject.Name + "." + edmx.Name + ": was checked");


		return true;

	}





	void AddReferenceAddedEventHandler(DTE dte)
	{
		// We should already be on UI thread. Callers must ensure this can never happen
		ThreadHelper.ThrowIfNotOnUIThread();

		try
		{

			_GlobalReferenceAddedEventHandler ??= new _dispReferencesEvents_ReferenceAddedEventHandler(OnGlobalReferenceAdded);


			Events2 dteEvents = dte.Events as Events2;

			_CSharpReferencesEvents ??= (ReferencesEvents)dteEvents.GetObject("CSharpReferencesEvents");
			_VBReferencesEvents ??= (ReferencesEvents)dteEvents.GetObject("VBReferencesEvents");

			_CSharpReferencesEvents.ReferenceAdded += _GlobalReferenceAddedEventHandler;
			_VBReferencesEvents.ReferenceAdded += _GlobalReferenceAddedEventHandler;

			Diag.Trace("Added _GlobalReferenceAddedEventHandler");
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;
		}
	}


	#endregion




	#region Asynchronous Event Handlers

	async Task HandleReferenceAddedAsync(Reference reference)
	{
		if (!_ValidateConfig || reference.Type != prjReferenceType.prjReferenceTypeAssembly
			|| reference.Name.ToLower() != SystemData.EFProvider.ToLower())
		{
			return;
		}

		await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

		// There simply is no other event that is fired when a project object is complete so we're recycling
		// the referenceadded event to the back of the UI thread queue until it's available.
		// The solution object adopts a fire and forget stragedy when opening projects so it also doesn't keep track.
		// This issue only occurs when a project is opened and our package has already been sited and given
		// the ide context. It's a single digit recycle count so it's low overhead.
		if (reference.ContainingProject.Properties == null)
		{
			if (++_RefCnt < 1000)
			{
				Diag.Trace("RECYCLING HandleReferenceAddedAsync for Project: " + reference.ContainingProject.Name + " for GlobalReference: " + reference.Name);

				_ = Task.Delay(100).ContinueWith(_ => HandleReferenceAddedAsync(reference), TaskScheduler.Current);
			}
			return;
		}

		Diag.Trace("HandleReferenceAddedAsync is through for Project: " + reference.ContainingProject.Name + " for GlobalReference: " + reference.Name);

		if (!IsValidStatus(_Dte.Solution.Globals) && IsValidatedStatus(_Dte.Solution.Globals))
			SetIsValidStatus(_Dte.Solution.Globals, true);

		// If anything gets through things are still happening so we can reset and allow references with incomplete project objects
		// to continue recycling
		_RefCnt = 0;

		ProjectItem config;

		if (!IsConfiguredEFStatus(reference.ContainingProject) && IsValidExecutableProjectType(reference.ContainingProject))
		{
			config = GetAppConfigProjectItem(reference.ContainingProject);

			if (config != null)
			{
				if (!ConfigureEntityFramework(config, false))
					SetValidateFailedStatus(_Dte.Solution);
			}
		}
	}





	async Task HandleAfterOpenProjectAsync(Project project)
	{
		await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();


		// Recycle until project object is complete if necessary
		if (project.Properties == null)
		{
			if (++_RefCnt < 1000)
			{
				Diag.Trace("RECYCLING Project: " + project.Name + " for AfterOpenProject");

				_ = Task.Delay(100).ContinueWith(_ => HandleAfterOpenProjectAsync(project), TaskScheduler.Current);
			}
			return;
		}

		if (!IsValidStatus(_Dte.Solution.Globals) && IsValidatedStatus(_Dte.Solution.Globals))
			SetIsValidStatus(_Dte.Solution.Globals, true);

		// If anything gets through things are still happening so we can reset and allow events with incomplete project objects
		// to continue recycling
		_RefCnt = 0;


		if (!IsValidExecutableProjectType(project) || (IsConfiguredEFStatus(project) && IsUpdatedEdmxsStatus(project)))
			return;


		RecursiveValidateProject(project);

	}


	#endregion




	#region IVsSolutionEvents Implementation and Event handling

	// Events that we handle are listed first

	void OnDebugSettingsSaved(VsDebugOptionModel e)
	{
		Diag.EnableTrace = e.EnableTrace;
		Diag.EnableDiagnostics = e.EnableDiagnostics;
	}

	void OnGeneralSettingsSaved(VsGeneralOptionModel e)
	{
		Diag.EnableWriteLog = e.EnableWriteLog;
		Diag.LogFile = e.LogFile;

		_ValidateConfig = e.ValidateConfig;
		_ValidateEdmx = e.ValidateEdmx;
	}


	void OnGlobalReferenceAdded(Reference reference) => _ = HandleReferenceAddedAsync(reference);

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


		// VSITEMID_ROOT gets the current project. 
		// Alternativly, you might have another item id.
		var itemid = VSConstants.VSITEMID_ROOT;

		pHierarchy.GetProperty(itemid, (int)__VSHPROPID.VSHPROPID_ExtObject, out object objProj);

		// var projectItem = objProj as EnvDTE.ProjectItem;
		// ... or ...

		if (objProj is not Project project)
		{
			Diag.Dug(true, "AfterOpenProject: Could not get project object property from hierarchy: " + pHierarchy.ToString());
			return S_OK;
		}
		else if (project.Kind != "{F184B08F-C81C-45F6-A57F-5ABD9991F28F}"
			&& project.Kind != "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}")
		{
			return S_OK;
		}

		_ = HandleAfterOpenProjectAsync(project);

		return S_OK;
	}


	int IVsSolutionEvents.OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
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

		if (_Dte.Solution == null)
		{
			Diag.Dug(true, "Solution is null");
			return S_OK;
		}

		if (IsValidateFailedStatus(_Dte.Solution))
		{
			Diag.Trace("There was a failed solution validation. Clearing solution status flags");
			ClearValidateStatus(_Dte.Solution);
		}
		else if (!IsValidatedStatus(_Dte.Solution.Globals))
		{
			Diag.Trace("The solution has no validation status set. Setting validated to on and valid to off");
			SetIsValidStatus(_Dte.Solution.Globals, false);
		}
		else
		{
			Diag.Trace("The solution has it's validated flag set. Doing nothing.");
		}


		return S_OK;
	}


	// Unhandled events follow

	int IVsSolutionEvents.OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy) => S_OK;
	int IVsSolutionEvents.OnAfterOpenSolution(object pUnkReserved, int fNewSolution) => S_OK;
	int IVsSolutionEvents.OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel) => S_OK;
	int IVsSolutionEvents.OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved) => S_OK;
	int IVsSolutionEvents.OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel) => S_OK;
	int IVsSolutionEvents.OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy) => S_OK;
	int IVsSolutionEvents.OnBeforeCloseSolution(object pUnkReserved) => S_OK;
	int IVsSolutionEvents.OnAfterCloseSolution(object pUnkReserved) => S_OK;


	#endregion




	#region Utility Methods and Dictionaries


	/// <summary>
	/// Sets a status indicator tagging a solution as previously validated or validated and valid or a project as having been validated and if it is
	/// a valid C#/VB executable
	/// </summary>
	/// <param name="global"></param>
	/// <param name="valid"></param>
	/// <returns>True if the operation was successful else False</returns>
	bool SetIsValidStatus(Globals global, bool valid)
	{
		// Should never happen
		ThreadHelper.ThrowIfNotOnUIThread();

		bool exists = false;
		int value = 0;
		string str;

		Diag.Dug("Setting IsValid to " + valid.ToString());

		try
		{
			if (global == null)
			{
				Diag.Dug(true, "Globals is null");
				return false;
			}


			if (global.get_VariableExists(G_Key))
			{
				str = (string)global[G_Key];
				value = str == "" ? 0 : int.Parse(str);
				exists = true;
			}

			value |= G_Validated;

			if (valid)
				value |= G_Valid;
			else
				value &= (~G_Valid);

			global[G_Key] = value.ToString();
			if (!exists)
				global.set_VariablePersists(G_Key, G_Persistent);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			return false;
		}


		return true;
	}



	/// <summary>
	/// Sets status indicator tagging a project's app.config as having been validated
	/// </summary>
	/// <param name="project"></param>
	/// <param name="valid"></param>
	/// <returns>True if the operation was successful else False</returns>
	bool SetIsValidatedAppConfigStatus(Project project)
	{
		ThreadHelper.ThrowIfNotOnUIThread();

		bool exists = false;
		int value = 0;
		string str;

		try
		{
			if (project.Globals == null)
			{
				Diag.Dug(true, project.Name + ": Globals is null");
				return false;
			}


			if (project.Globals.get_VariableExists(G_Key))
			{
				str = (string)project.Globals[G_Key];
				value = str == "" ? 0 : int.Parse(str);
				exists = true;
			}

			value |= G_EFConfigured;


			project.Globals[G_Key] = value.ToString();
			if (!exists)
				project.Globals.set_VariablePersists(G_Key, G_Persistent);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			return false;
		}


		return true;
	}



	/// <summary>
	/// Sets non-persistent status indicator tagging a project's existing edmx's as having been validated/upgraded
	/// from legacy provider settings
	/// </summary>
	/// <param name="project"></param>
	/// <param name="valid"></param>
	/// <returns>True if the operation was successful else False</returns>
	bool SetIsUpdatedEdmxsStatus(Project project)
	{
		ThreadHelper.ThrowIfNotOnUIThread();

		bool exists = false;
		int value = 0;
		string str;

		try
		{
			if (project.Globals == null)
			{
				Diag.Dug(true, project.Name + ": Project.Globals is null");
				return false;
			}


			if (project.Globals.get_VariableExists(G_Key))
			{
				str = (string)project.Globals[G_Key];
				value = str == "" ? 0 : int.Parse(str);
				exists = true;
			}

			value |= G_EdmxsUpdated;


			project.Globals[G_Key] = value.ToString();
			if (!exists)
				project.Globals.set_VariablePersists(G_Key, G_Persistent);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			return false;
		}


		return true;
	}


	/// <summary>
	/// Clears the status indicator of a solution.
	/// </summary>
	/// <param name="solution"></param>
	/// <returns>True if the operation was successful else False</returns>
	bool ClearValidateStatus(Solution solution)
	{

		ThreadHelper.ThrowIfNotOnUIThread();

		try
		{
			if (solution.Globals == null)
			{
				Diag.Dug(true, solution.FullName + ": Solution.Globals is null");
				return false;
			}

			if (!solution.Globals.get_VariableExists(G_Key))
			{
				return true;
			}

			solution.Globals[G_Key] = 0.ToString();
			solution.Globals.set_VariablePersists(G_Key, G_Persistent);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			return false;
		}


		return true;
	}



	/// <summary>
	/// Sets validation to failed for a solution. Validation will then be reset on solution close
	/// </summary>
	/// <param name="solution"></param>
	/// <returns>True if the operation was successful else False</returns>
	bool SetValidateFailedStatus(Solution solution)
	{
		Diag.Dug("Setting ValidateFailed");

		ThreadHelper.ThrowIfNotOnUIThread();

		bool exists = false;
		int value = 0;
		string str;

		try
		{
			if (solution.Globals == null)
			{
				Diag.Dug(true, solution.FullName + ": Solution.Globals is null");
				return false;
			}


			if (solution.Globals.get_VariableExists(G_Key))
			{
				str = (string)solution.Globals[G_Key];
				value = str == "" ? 0 : int.Parse(str);
				exists = true;
			}

			value |= G_ValidateFailed;


			solution.Globals[G_Key] = value.ToString();
			if (!exists)
				solution.Globals.set_VariablePersists(G_Key, G_Persistent);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			return false;
		}


		return true;
	}


	/// <summary>
	/// Verifies whether or not a solution is in a validation state (or previously validated) or a project has been validated as being valid or not
	/// </summary>
	/// <param name="global"></param>
	/// <returns></returns>
	bool IsValidatedStatus(Globals global)
	{
		ThreadHelper.ThrowIfNotOnUIThread();

		int value;
		string str;

		try
		{
			if (global == null)
			{
				Diag.Dug(true, "Globals is null");
				return false;
			}


			if (global.get_VariableExists(G_Key))
			{
				str = (string)global[G_Key];
				value = str == "" ? 0 : int.Parse(str);

				return (value & G_Validated) == G_Validated;
			}

		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			return false;
		}

		return false;
	}

	/// <summary>
	/// Verifies whether or not a solution has been validated or a project is a valid C#/VB executable. See remarks.
	/// </summary>
	/// <param name="global"></param>
	/// <returns></returns>
	/// <remarks>
	/// Callers must call IsValidatedProjectStatus() before checking if a project is valid otherwise this indicator will be meaningless
	/// </remarks>
	bool IsValidStatus(Globals global)
	{
		ThreadHelper.ThrowIfNotOnUIThread();

		int value;
		string str;

		try
		{
			if (global == null)
			{
				Diag.Dug(true, "Globals is null");
				return false;
			}

			if (global.get_VariableExists(G_Key))
			{
				str = (string)global[G_Key];
				value = str == "" ? 0 : int.Parse(str);

				return (value & G_Valid) == G_Valid;
			}

		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			return false;
		}

		return false;
	}



	/// <summary>
	/// Cheecks whether at any point a solution vqalidation failed
	/// </summary>
	/// <param name="solution"></param>
	/// <returns></returns>
	bool IsValidateFailedStatus(Solution solution)
	{
		ThreadHelper.ThrowIfNotOnUIThread();

		int value;
		string str;

		try
		{
			if (solution.Globals == null)
			{
				Diag.Dug(true, solution.FullName + ": Solution.Globals is null");
				return false;
			}

			if (solution.Globals.get_VariableExists(G_Key))
			{
				str = (string)solution.Globals[G_Key];
				value = str == "" ? 0 : int.Parse(str);

				return (value & G_ValidateFailed) == G_ValidateFailed;
			}

		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			return false;
		}

		return false;
	}


	/// <summary>
	/// Verifies whether or not a project's App.config was validated for EntityFramework.Firebird
	/// </summary>
	/// <param name="project"></param>
	/// <returns></returns>
	bool IsConfiguredEFStatus(Project project)
	{
		ThreadHelper.ThrowIfNotOnUIThread();

		int value;
		string str;

		try
		{
			if (project.Globals == null)
			{
				Diag.Dug(true, project.Name + ": Project.Globals is null");
				return false;
			}

			if (project.Globals.get_VariableExists(G_Key))
			{
				str = (string)project.Globals[G_Key];
				value = str == "" ? 0 : int.Parse(str);

				return (value & G_EFConfigured) == G_EFConfigured;
			}

		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			return false;
		}

		return false;
	}



	/// <summary>
	/// Verifies whether or not a project's existing edmx models were updated from using legacy data providers to current
	/// Firebird Client and EntityFramework providers.
	/// </summary>
	/// <param name="project"></param>
	/// <returns></returns>
	bool IsUpdatedEdmxsStatus(Project project)
	{
		ThreadHelper.ThrowIfNotOnUIThread();

		int value;
		string str;

		try
		{
			if (project.Globals == null)
			{
				Diag.Dug(true, project.Name + ": Project.Globals is null");
				return false;
			}

			if (project.Globals.get_VariableExists(G_Key))
			{
				str = (string)project.Globals[G_Key];
				value = str == "" ? 0 : int.Parse(str);

				return (value & G_EdmxsUpdated) == G_EdmxsUpdated;
			}

		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			return false;
		}

		return false;
	}



	protected string GetKindName(string kind)
	{
		if (!ProjectGuids.TryGetValue(kind, out string name))
			name = kind;

		return name;
	}


	readonly Dictionary<string, string> ProjectGuids = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
	{
		{ "{06A35CCD-C46D-44D5-987B-CF40FF872267}", "DeploymentMergeModule" },
		{ "{14822709-B5A1-4724-98CA-57A101D1B079}", "Workflow(C#)" },
		{ "{20D4826A-C6FA-45DB-90F4-C717570B9F32}", "Legacy(2003)SmartDevice(C#)" },
		{ "{2150E333-8FDC-42A3-9474-1A3956D46DE8}", "SolutionFolder" },
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
		{ "{66A2671F-8FB5-11D2-AA7E-00C04F688DDE}", "MiscItem" },
		{ "{66A26720-8FB5-11D2-AA7E-00C04F688DDE}", "SolutionFolder" },
		{ "{66A26722-8FB5-11D2-AA7E-00C04F688DDE}", "SolutionItem" },
		{ "{67294A52-A4F0-11D2-AA88-00C04F688DDE}", "UnloadedProject" },
		{ "{6BB5F8EE-4483-11D3-8BCF-00C04F8EC28C}", "ProjectFile" },
		{ "{6BB5F8EF-4483-11D3-8BCF-00C04F8EC28C}", "ProjectFolder" },
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
		{ "{F184B08F-C81C-45F6-A57F-5ABD9991F28F}", "VbProject" },
		{ "{F2A71F9B-5D33-465A-A702-920D77279786}", "F#Project" },
		{ "{F5B4F3BC-B597-4E2B-B552-EF5D8A32436F}", "MonoTouchBinding" },
		{ "{F85E285D-A4E0-4152-9332-AB1D724D3325}", "ASP.NET(MVC2.0)" },
		{ "{F8810EC1-6754-47FC-A15F-DFABD2E3FA90}", "SharePointWorkflow" },
		{ "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}", "C#Project" },
		{ "{FBB4BD86-BF63-432a-A6FB-6CF3A1288F83}", "InstallShieldLimitedEdition" },
		{ ".nuget", "SettingsNuget" }
	};


	#endregion
}