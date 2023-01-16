/*
 *  Visual Studio DDEX Provider for FirebirdClient (BlackbirdSql)
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. You may obtain a copy of the License at 
 *     http://www.blackbirdsql.org/index.php?op=doc&id=idpl
 *
 *     Software distributed under the License is distributed on 
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *     express or implied.  See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2023 GA Christos
 *  All Rights Reserved.
 *   
 *  Contributors:
 *    GA Christos
 */

/*
 * 
 * Auto load is time critical with this package. 
 * For example when an edmx is brought into context we can't have registering of the
 * DBProviderFactory (BlackbirdSql.FirebirdClientFactory) lost in a tertiary thread.
 * 
 * We're going to try SwitchToMainThreadAsync here.
 * If it still loads erratically we'll have to go synchronous and comment this whole class out
 * 
 * If the load fails in server explorer it can be refreshed once tloading is complete.
 * 
 */

using System;
using System.Runtime.InteropServices;
using System.Threading;

using EnvDTE;

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

using BlackbirdSql.Common;
using BlackbirdSql.VisualStudio.Ddex.Configuration;

using Task = System.Threading.Tasks.Task;
using EnvDTE80;
using System.IO.Packaging;

namespace BlackbirdSql.VisualStudio.Ddex
{

	/// <summary>
	/// This is the class that implements the package exposed by this assembly.
	///
	/// The minimum requirement for a class to be considered a valid package for Visual Studio
	/// is to implement the IVsPackage interface and register itself with the shell.
	/// This package uses the helper classes defined inside the Managed Package Framework (MPF)
	/// to do it: it derives from the Package class that provides the implementation of the 
	/// IVsPackage interface and uses the registration attributes defined in the framework to 
	/// register itself and its components with the shell.
	/// </summary>

	[Guid(PackageData.PackageGuid)]

	// This attribute tells the registration utility (regpkg.exe) that this class needs
	// to be registered as package.
	[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]

	// A Visual Studio component can be registered under different regitry roots; for instance
	// when you debug your package you want to register it in the experimental hive. This
	// attribute specifies the registry root to use if no one is provided to regpkg.exe with
	// the /root switch.
	// [DefaultRegistryRoot("Software\\Microsoft\\VisualStudio\\15.0")]
	// This attribute is used to register the information needed to show the this package
	// in the Help/About dialog of Visual Studio.
	[InstalledProductRegistration("#100", "#102", "1.0", IconResourceID = 400)]

	// In order be loaded inside Visual Studio in a machine that has not the VS SDK installed, 
	// package needs to have a valid load key (it can be requested at 
	// http://msdn.microsoft.com/vstudio/extend/). This attributes tells the shell that this 
	// package has a load key embedded in its resources.
	// [ProvideLoadKey("Standard", "1.0", "DDEX Provider for BlackbirdClient", "..", 106)]

	// We start loading as soon as the VS shell is available
	[ProvideAutoLoad(PackageData.ShellInitializedContextRuleGuid, PackageAutoLoadFlags.BackgroundLoad)]
	// [ProvideMenuResource(1000, 1)] TBC
	[ProvideService(typeof(IProviderObjectFactory), IsAsyncQueryable = true, ServiceName = PackageData.ServiceName)]

	[VsPackageRegistration]

	// Visual studio Options implementation
	[ProvideOptionPage(typeof(VsOptionsProvider.GeneralOptionPage), VsOptionsProvider.OptionPageCategory,
		VsOptionsProvider.GeneralOptionPageName, 0, 0, true, SupportsProfiles = true)]
#if DEBUG
	[ProvideOptionPage(typeof(UIDebugOptionDialogPage), VsOptionsProvider.OptionPageCategory,
		VsOptionsProvider.DebugOptionPageName, 0, 0, true, SupportsProfiles = true)]
#endif



	public sealed class BlackbirdSqlDdexPackage : AsyncPackage
	{
		private DTE _Dte = null;
		private IVsSolution _Solution = null;
		readonly VsPackageController _Controller;



		#region · Constructors / Destructors ·

		/// <summary>
		/// Package constructor
		/// </summary>
		public BlackbirdSqlDdexPackage()
		{
			_Controller = VsPackageController.GetInstance(this);
		}



		protected override void Dispose(bool disposing)
		{
			try
			{
				ThreadHelper.ThrowIfNotOnUIThread();
				_Controller.Dispose();
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
			}

			base.Dispose(disposing);
		}

		#endregion



		#region · Package Methods ·


		/// <summary>
		/// Initialization of the package; this method is called right after the package is sited, so this is the place
		/// where you can put all the initilaization code that rely on services provided by VisualStudio.
		/// </summary>
		protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
		{
			await base.InitializeAsync(cancellationToken, progress);

			Diag.Trace();


			AddService
			(
				typeof(IProviderObjectFactory),
				(_, _, _) => Task.FromResult<object>(new ProviderObjectFactory()),
				promote: true
			);

			Diag.Trace("IProviderObjectFactory service added");

			// Taken DbProviderFactoriesEx.AddAssemblyToCache() out of here and into ProviderObjectFactory()


			// We're switching to main thread because this is loading too slowly and unavailable in the server
			// explorer while background tasks are still loading
			// We'll see  if it helps (Nope... if anything seems to be worse)

			
			// await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

			_ = AdviseSolutionEventsAsync();


			Diag.Trace("Package initialization complete");

		}



		/// <summary>
		/// Fires onload events for projects already loaded and then hooks onto solution events
		/// </summary>
		private async Task AdviseSolutionEventsAsync()
		{
			await JoinableTaskFactory.SwitchToMainThreadAsync(DisposalToken);

			try
			{
				_Dte = await GetServiceAsync(typeof(DTE)).ConfigureAwait(false) as DTE;
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				return;
			}

			if (_Dte == null)
			{
				NullReferenceException ex = new NullReferenceException("DTE is null");
				Diag.Dug(ex);
				return;
			}

			Diag.Trace("DTE is loaded");

			// Get the solution service
			_Solution = await GetServiceAsync(typeof(SVsSolution)).ConfigureAwait(false) as IVsSolution;

			// If it's null there's an issue. Possibly we've come in too early
			if (_Solution == null)
			{
				NullReferenceException ex = new NullReferenceException("SVsSolution is null");
				Diag.Dug(ex);
				return;
			}

			_Controller?.AdviseSolutionEvents(_Dte, _Solution);
		}


		#endregion

	}
}