// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;

using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl;
using BlackbirdSql.Core.Ctl.Interfaces;

using Microsoft.VisualStudio.Shell;


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
internal class PackageController : AbstractPackageController
{


	public override IBGlobalsAgent Uig => GlobalsAgent.Instance;

	// =========================================================================================================
	#region Constructors / Destructors - PackageController
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Private singleton .ctor
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected PackageController(IBAsyncPackage package)
		: base(package)
	{
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets or creates the singleton PackageController instance. This may only be
	/// called from <see cref="ControllerAsyncPackage.CreateServiceInstanceAsync"/>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static IBPackageController CreateInstance(IBAsyncPackage package)
	{
		return _Instance ??= new PackageController(package);
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Methods - PackageController
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Enables solution and running document table event handling
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override void AdviseEvents()
	{
		if (_EventsAdvised)
			return;

		_EventsAdvised = true;

		// We should already be on UI thread. Callers must ensure this can never happen
		ThreadHelper.ThrowIfNotOnUIThread();

		// Sanity check. Disable events if enabled
		UnadviseEvents(false);


		// This is a once off procedure for solutions and their projects. (ie. Once validated always validated)
		// We're going to check each project that gets loaded (or has a reference added) if it
		// references EntityFramework.Firebird.dll else FirebirdSql.Data.FirebirdClient.dll.
		// If it is we'll check the app.config DbProvider and EntityFramework sections and update if necessary.
		// We also check (once and only once) within a project for any Firebird edmxs with legacy settings and update
		// those, because they cannot work with newer versions of EntityFramework.Firebird.
		// (This validation can be disabled in Visual Studio's options.)


		// Enable solution event capture
		DteSolution.AdviseSolutionEvents(this, out _HSolutionEvents);

		try
		{
			DocTable.AdviseRunningDocTableEvents(this, out _HDocTableEvents);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
		}


		// Raise open project event handler for projects that are already loaded
		// This can happen on IDE startup and a solution was opened with some projects
		// loaded before our package was sited.
		if (Dte == null || Dte.Solution == null || Dte.Solution.Globals == null)
		{
			InvalidOperationException ex = new("DTE.Solution is invalid");
			Diag.Dug(ex);
		}

		RegisterEventsManager(((ControllerAsyncPackage)DdexPackage).EventsManager);

	}


	#endregion Methods


}