// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BlackbirdSql.Controller.Properties;
using BlackbirdSql.Core;
using BlackbirdSql.Sys.Interfaces;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;


namespace BlackbirdSql.Controller;

// =========================================================================================================
//										PackageController Class
//
/// <summary>
/// Manages package events and settings.
/// </summary>
/// <remarks>
/// Also updates the app.config for DbProvider and EntityFramework and updates existing .edmx models that
/// are using a legacy provider.
/// Also ensures we never do validations of a solution and project app.config and .edmx models twice.
/// Aslo performs cleanups of any sql editor documents that may be left dangling on solution close.
/// </remarks>
// =========================================================================================================
public sealed class PackageController : AbstractPackageController
{

	// =========================================================================================================
	#region Constructors / Destructors - PackageController
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Private singleton .ctor
	/// </summary>
	// ---------------------------------------------------------------------------------
	private PackageController(IBsAsyncPackage ddex)
		: base(ddex)
	{
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets or creates the singleton PackageController instance. This should only
	/// be called in the .ctor of <see cref="AbstractCorePackage"/>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static IBsPackageController CreateInstance(IBsAsyncPackage ddex) =>
		new PackageController(ddex);



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the singleton PackageController instance..
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static new IBsPackageController Instance =>
		_Instance ?? throw Diag.ExceptionInstance(typeof(IBsPackageController));


	protected override void Dispose(bool disposing)
	{
		if (!disposing)
			return;

		base.Dispose(disposing);

	}




	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - PackageController
	// =========================================================================================================


	private static bool _EventsAdvisedUnsafe = false;


	#endregion Fields





	// =========================================================================================================
	#region Property Accessors - PackageController
	// =========================================================================================================


	public ControllerPackage ControllerPackage => (ControllerPackage)PackageInstance;

	public override bool SolutionValidating => ControllerEventsManager.SolutionValidating;



	#endregion Property Accessors




	// =========================================================================================================
	#region Methods - PackageController
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Enables unsafe solution, running document table and selection monitor event handling
	/// on the UI and safe solution OnLoadOptions and OnSaveOptions event handling.
	/// Only calls unsafe if on ui thread. Does not attempt switch.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override async Task<bool> AdviseUnsafeEventsAsync()
	{
		lock (_LockObject)
		{
			if (_EventsAdvisedUnsafe)
				return false;
		}


		// Check we're on ui thread for async advising of unsafe events.

		if (!ThreadHelper.CheckAccess())
		{
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

			lock (_LockObject)
			{
				if (_EventsAdvisedUnsafe)
					return false;
			}
		}

		return AdviseUnsafeEventsImpl();
	}



	private bool AdviseUnsafeEventsImpl()
	{
		Diag.ThrowIfNotOnUIThread();

		lock (_LockObject)
		{
			if (_EventsAdvisedUnsafe)
				return false;
		}


		// Sanity check. Disable events if enabled
		// UnadviseEvents(false);

		if (Dte == null)
			Diag.ThrowException(new NullReferenceException(Resources.ExceptionDteIsNull));

		// If it's null there's an issue. Possibly we've come in too early
		if (VsSolution == null)
			Diag.ThrowException(new NullReferenceException(Resources.ExceptionSVsSolutionIsNull));

		if (!RdtManager.ServiceAvailable)
			Diag.ThrowException(new NullReferenceException(Resources.ExceptionIVsRunningDocumentTableIsNull));

		// Tracer.Trace(GetType(), "AdviseUnsafeEventsImpl()", "ProjectItemsEvents.");

		if (!_EventsAdvisedUnsafe)
		{
			_EventsAdvisedUnsafe = true;


			// Tracer.Trace(GetType(), "AdviseUnsafeEventsImpl()", "SolutionEvents.");

			// Enable solution event capture
			VsSolution.AdviseSolutionEvents(this, out _HSolutionEvents);


			// Enable rdt events.
			try
			{
				RdtManager.AdviseRunningDocTableEvents(this, out _HDocTableEvents);
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
			}
		}

		return true;
	}


	public override async Task<bool> RegisterProjectEventHandlersAsync()
	{
		if (!EventProjectRegistrationEnter(true))
			return false;


		if (!ThreadHelper.CheckAccess())
		{
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

			if (!EventProjectRegistrationEnter(true))
				return false;
		}

		return RegisterProjectEventHandlersImpl();
	}


	private bool RegisterProjectEventHandlersImpl()
	{
		if (!EventProjectRegistrationEnter())
			return false;


		try
		{
			List<Project> projects = UnsafeCmd.RecursiveGetDesignTimeProjects();

			// Tracer.Trace(GetType(), "UiRegisterProjectEventHandlers()", "Adding event handlers for {0} EF projects", projects.Count);
			foreach (Project project in projects)
				AddProjectEventHandlers(project);
		}
		finally
		{
			EventProjectRegistrationExit();
		}

		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Enables unsafe solution, running document table and selection monitor event handling
	/// on the UI and safe solution OnLoadOptions and OnSaveOptions event handling.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override bool UiAdviseUnsafeEvents()
	{
		lock (_LockObject)
		{
			if (_EventsAdvisedUnsafe)
				return false;
		}


		// Ensure UI thread call is made for unsafe events.

		if (!ThreadHelper.CheckAccess())
		{
			// Fire and forget async
			Task.Run(AdviseUnsafeEventsAsync).Forget();
			return true;
		}

		return AdviseUnsafeEventsImpl();
	}



	public override void UiRegisterProjectEventHandlers()
	{
		if (!EventProjectRegistrationEnter(true))
			return;

		if (!ThreadHelper.CheckAccess())
		{
			// Fire and forget async
			Task.Run(RegisterProjectEventHandlersAsync).Forget();
			return;
		}


		RegisterProjectEventHandlersImpl();

		return;
	}



	public override void ValidateSolution()
	{
		((ControllerEventsManager)ControllerPackage.EventsManager).ValidateSolution();
	}

	#endregion Methods


}