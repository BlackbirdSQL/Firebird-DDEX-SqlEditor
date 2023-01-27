/*
 *  Visual Studio DDEX 2.0 Provider for FirebirdClient (BlackbirdSql)
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
 * DBProviderFactory (FirebirdSql.Data.FirebirdClient.FirebirdClientFactory) lost in a tertiary thread.
 * 
 * We're going to try SwitchToMainThreadAsync here.
 * If it still loads erratically we'll have to go synchronous and comment this whole class out
 * 
 * If the load fails in server explorer it can be refreshed once loading is complete.
 * 
 */

using System;
using System.Runtime.InteropServices;
using System.Threading;

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

using EnvDTE;

using BlackbirdSql.Common;
using BlackbirdSql.VisualStudio.Ddex.Configuration;

using Task = System.Threading.Tasks.Task;
using System.Data.Common.BlackbirdSql;
using FirebirdSql.Data.FirebirdClient;

namespace BlackbirdSql.VisualStudio.Ddex
{



	// ---------------------------------------------------------------------------------------------------
	//
	//								BlackbirdSqlDdexPackage Class
	//
	// ---------------------------------------------------------------------------------------------------




	/// <summary>
	/// BlackbirdSql.Data.Ddex DDEX 2.0 package class
	/// </summary>
	/// <remarks>
	/// This is the class that implements the package exposed by this assembly.
	///
	/// The minimum requirement for a class to be considered a valid package for Visual Studio
	/// is to implement the IVsPackage interface and register itself with the shell.
	/// This package uses the helper classes defined inside the Managed Package Framework (MPF)
	/// to do it: it derives from the Package class that provides the implementation of the 
	/// IVsPackage interface and uses the registration attributes defined in the framework to 
	/// register itself and its components with the shell.
	/// </remarks>




	// ---------------------------------------------------------------------------------------------------
	//
	#region Package attributes - BlackbirdSqlDdexPackage
	//
	// ---------------------------------------------------------------------------------------------------



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


	#endregion



	// ---------------------------------------------------------------------------------------------------
	//
	#region Class declaration - BlackbirdSqlDdexPackage
	//
	// ---------------------------------------------------------------------------------------------------




	public sealed class BlackbirdSqlDdexPackage : AsyncPackage
	{
		private DTE _Dte = null;
		private IVsSolution _Solution = null;
		private VsPackageController _Controller;



		// ---------------------------------------------------------------------------------------------------
		//
		#region Property accessors - BlackbirdSqlDdexPackage
		//
		// ---------------------------------------------------------------------------------------------------



		/// <summary>
		/// Accessor to the <see cref="VsPackageController"/> singleton instance
		/// </summary>
		VsPackageController Controller
		{
			get
			{
				return _Controller ??= VsPackageController.GetInstance(_Dte, _Solution);
			}
		}


		#endregion



		// ---------------------------------------------------------------------------------------------------
		//
		#region Constructors / Destructors - BlackbirdSqlDdexPackage
		//
		// ---------------------------------------------------------------------------------------------------



		/// <summary>
		/// BlackbirdSqlDdexPackage package .ctor
		/// </summary>
		public BlackbirdSqlDdexPackage()
		{
		}



		protected override void Dispose(bool disposing)
		{
			try
			{
				ThreadHelper.ThrowIfNotOnUIThread();
				_Controller?.Dispose();
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
			}

			base.Dispose(disposing);
		}

		#endregion




		// ---------------------------------------------------------------------------------------------------
		//
		#region Package methods - BlackbirdSqlDdexPackage
		//
		// ---------------------------------------------------------------------------------------------------



		/// <summary>
		/// Initialization of the package; this method is called right after the package is sited, so this is the place
		/// where you can put all the initilaization code that rely on services provided by VisualStudio.
		/// </summary>
		protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
		{
			await base.InitializeAsync(cancellationToken, progress);


			AddService
			(
				typeof(IProviderObjectFactory),
				(_, _, _) => Task.FromResult<object>(new ProviderObjectFactory()),
				promote: true
			);

			
			_ = AdviseSolutionEventsAsync();


			// Adding FirebirdClient to assembly cache asynchronously
			if (DbProviderFactoriesEx.AddAssemblyToCache(typeof(FirebirdClientFactory),
				Properties.Resources.Provider_ShortDisplayName, Properties.Resources.Provider_DisplayName))
			{
				// Diag.Trace("DbProviderFactory added to assembly cache");

				AppDomain.CurrentDomain.AssemblyResolve += (_, args) =>
				{
					var assembly = typeof(FirebirdClientFactory).Assembly;

					if (args.Name == assembly.FullName)
						Diag.Dug(true, "Dsl Provider Factory failed to load: " + assembly.FullName);

					return args.Name == assembly.FullName ? assembly : null;
				};

			}
			else
			{
				Diag.Trace("DbProviderFactory not added to assembly cache during package registration. Factory already cached");
			}



		}



		/// <summary>
		/// Enables solution and project event handling
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


			// Get the solution service
			_Solution = await GetServiceAsync(typeof(SVsSolution)).ConfigureAwait(false) as IVsSolution;

			// If it's null there's an issue. Possibly we've come in too early
			if (_Solution == null)
			{
				NullReferenceException ex = new NullReferenceException("SVsSolution is null");
				Diag.Dug(ex);
				return;
			}

			Controller.AdviseSolutionEvents();
		}


		#endregion

	}


	#endregion Class Declaration
}