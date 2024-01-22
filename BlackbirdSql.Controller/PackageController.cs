﻿// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using BlackbirdSql.Controller.Properties;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl;
using BlackbirdSql.Core.Ctl.Diagnostics;
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
	private PackageController(IBAsyncPackage ddex)
		: base(ddex)
	{
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets or creates the singleton PackageController instance. This should only
	/// be called in the .ctor of <see cref="AbstractAsyncPackage"/>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static IBPackageController CreateInstance(IBAsyncPackage ddex) =>
		new PackageController(ddex);



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the singleton PackageController instance..
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static new IBPackageController Instance =>
		_Instance ?? throw Diag.ExceptionInstance(typeof(IBPackageController));




	#endregion Constructors / Destructors




	// =========================================================================================================
	#region Property Accessors - PackageController
	// =========================================================================================================


	public ControllerAsyncPackage ControllerPackage => (ControllerAsyncPackage)DdexPackage;


	#endregion Property Accessors




	// =========================================================================================================
	#region Methods - PackageController
	// =========================================================================================================



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Enables unsafe solution, running document table and selection monitor event handling
	/// on the UI and safe solution OnLoadOptions and OnSaveOptions event handling.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override bool AdviseEvents()
	{

		if (_EventsAdvisedUnsafe)
			return false;

		_EventsAdvisedUnsafe = true;

		// Ensure UI thread call is made for unsafe events.

		if (!ThreadHelper.CheckAccess())
		{
			bool result = ThreadHelper.JoinableTaskFactory.Run(async delegate
			{
				await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
				return AdviseUnsafeEvents();
			});

			return result;
		}

		return AdviseUnsafeEvents();
	}





	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Enables unsafe solution, running document table and selection monitor event handling
	/// on the UI and safe solution OnLoadOptions and OnSaveOptions event handling.
	/// Only calls unsafe if on ui thread. Does not attempt switch.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override async Task<bool> AdviseEventsAsync()
	{
		if (_EventsAdvisedUnsafe)
			return false;


		// Check we're on ui thread for async advising of unsafe events.

		if (!ThreadHelper.CheckAccess())
			return false;

		// Warning suppression.
		await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

		_EventsAdvisedUnsafe = true;

		return AdviseUnsafeEvents();

	}



	protected override bool AdviseUnsafeEvents()
	{
		Diag.ThrowIfNotOnUIThread();


		// Sanity check. Disable events if enabled
		UnadviseEvents(false);

		try
		{
			if (Dte == null)
				throw new NullReferenceException(Resources.ExceptionDteIsNull);

			// If it's null there's an issue. Possibly we've come in too early
			if (VsSolution == null)
				throw new NullReferenceException(Resources.ExceptionSVsSolutionIsNull);

			if (DocTable == null)
				throw new NullReferenceException(Resources.ExceptionIVsRunningDocumentTableIsNull);

		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;
		}

		// Tracer.Trace(GetType(), "AdviseUnsafeEvents()", "AdviseSolutionEvents.");

		// Enable solution event capture
		VsSolution.AdviseSolutionEvents(this, out _HSolutionEvents);

		// Enable rdt events.
		try
		{
			DocTable.AdviseRunningDocTableEvents(this, out _HDocTableEvents);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
		}


		return true;
	}


	#endregion Methods


}