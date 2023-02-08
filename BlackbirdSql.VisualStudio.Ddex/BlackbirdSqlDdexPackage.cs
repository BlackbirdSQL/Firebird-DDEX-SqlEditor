//
// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)
//

using System;
using System.Data.Common;
using System.Runtime.InteropServices;
using System.Threading;

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

using EnvDTE;

using BlackbirdSql.Common;
using BlackbirdSql.Common.Extensions;
using BlackbirdSql.VisualStudio.Ddex.Configuration;
using FirebirdSql.Data.FirebirdClient;

using Task = System.Threading.Tasks.Task;



namespace BlackbirdSql.VisualStudio.Ddex
{

	// =========================================================================================================
	//										BlackbirdSqlDdexPackage Class 
	//
	/// <summary>
	/// BlackbirdSql.Data.Ddex DDEX 2.0 <see cref="IVsPackage"/> class implementation
	/// </summary>
	/// <remarks>
	/// Implements the package exposed by this assembly and registers itself with the shell.
	/// </remarks>
	// =========================================================================================================



	// ---------------------------------------------------------------------------------------------------------
	#region							BlackbirdSqlDdexPackage Class Attributes
	// ---------------------------------------------------------------------------------------------------------

	[Guid(PackageData.PackageGuid)]

	// Register this class as a VS package.
	[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]

	// A Visual Studio component can be registered under different regitry roots; for instance
	// when you debug your package you want to register it in the experimental hive. This
	// attribute specifies the registry root to use if no one is provided to regpkg.exe with
	// the /root switch.
	// [DefaultRegistryRoot("Software\\Microsoft\\VisualStudio\\15.0")]

	// 'Help About' registration
	[InstalledProductRegistration("#100", "#102", "1.0", IconResourceID = 400)]

	// Valid load key for devices without the VS SDK installed. (Possibly deprecated)
	// Load key request site (http://msdn.microsoft.com/vstudio/extend/) no longer exists.
	// [ProvideLoadKey("Standard", "1.0", "DDEX Provider for BlackbirdClient", "..", 999)]

	// We start loading as soon as the VS shell is available.
	[ProvideAutoLoad(PackageData.ShellInitializedContextRuleGuid, PackageAutoLoadFlags.BackgroundLoad)]

	// Not used
	// [ProvideMenuResource(1000, 1)] TBC

	// Register the service under the Services registry key
	[ProvideService(typeof(IProviderObjectFactory), IsAsyncQueryable = true, ServiceName = PackageData.ServiceName)]

	// Register the DDEX as a data provider
	[VsPackageRegistration]

	// Implement Visual studio options/settings
	[ProvideOptionPage(typeof(VsOptionsProvider.GeneralOptionPage), VsOptionsProvider.OptionPageCategory,
		VsOptionsProvider.GeneralOptionPageName, 0, 0, true, SupportsProfiles = true)]
#if DEBUG
	[ProvideOptionPage(typeof(UIDebugOptionsDialogPage), VsOptionsProvider.OptionPageCategory,
		VsOptionsProvider.DebugOptionPageName, 0, 0, true, SupportsProfiles = true)]
#endif


	#endregion Class Attributes





	// =========================================================================================================
	#region							BlackbirdSqlDdexPackage Class Declaration
	// =========================================================================================================


	public sealed class BlackbirdSqlDdexPackage : AsyncPackage
	{

		#region Variables - BlackbirdSqlDdexPackage


		private DTE _Dte = null;
		private IVsSolution _Solution = null;
		private VsPackageController _Controller;

		private System.Reflection.Assembly _InvariantAssembly = null;


		#endregion Variables





		// =========================================================================================================
		#region Property accessors - BlackbirdSqlDdexPackage
		// =========================================================================================================


		// ---------------------------------------------------------------------------------
		/// <summary>
		/// Accessor to the <see cref="VsPackageController"/> singleton instance
		/// </summary>
		// ---------------------------------------------------------------------------------
		VsPackageController Controller
		{
			get
			{
				return _Controller ??= VsPackageController.GetInstance(_Dte, _Solution);
			}
		}


		#endregion Property accessors





		// =========================================================================================================
		#region Constructors / Destructors - BlackbirdSqlDdexPackage
		// =========================================================================================================


		// ---------------------------------------------------------------------------------
		/// <summary>
		/// BlackbirdSqlDdexPackage package .ctor
		/// </summary>
		// ---------------------------------------------------------------------------------
		public BlackbirdSqlDdexPackage()
		{
			Diag.Context = "IDE";
		}



		// ---------------------------------------------------------------------------------
		/// <summary>
		/// BlackbirdSqlDdexPackage package destructor 
		/// </summary>
		/// <param name="disposing"></param>
		// ---------------------------------------------------------------------------------
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


		#endregion Constructors / Destructors





		// =========================================================================================================
		#region Method Implementations - BlackbirdSqlDdexPackage
		// =========================================================================================================



		// ---------------------------------------------------------------------------------
		/// <summary>
		/// Asynchronous initialization of the package
		/// </summary>
		/// <remarks>
		/// Auto load is time critical with this package.
		/// For example when an edmx is brought into context we can't have registering of the
		/// <see cref="DbProviderFactory"/> (<see cref="FirebirdClientFactory"/>) lost in a tertiary thread.
		/// We're going to try SwitchToMainThreadAsync here.
		/// If it still loads erratically we'll have to consider goin synchronous and commentting this whole class out.
		/// </remarks>
		// ---------------------------------------------------------------------------------
		protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
		{
			await base.InitializeAsync(cancellationToken, progress);

			// Diag.Trace();

			AddService
			(
				typeof(IProviderObjectFactory),
				(_, _, _) => Task.FromResult<object>(new TProviderObjectFactory()),
				promote: true
			);

			_ = AdviseSolutionEventsAsync();


			// Add FirebirdClient to assembly cache asynchronously
			if (_InvariantAssembly == null && DbProviderFactoriesEx.AddAssemblyToCache2(typeof(FirebirdClientFactory),
				Properties.Resources.Provider_ShortDisplayName, Properties.Resources.Provider_DisplayName))
			{
				// Diag.Trace("DbProviderFactory added to assembly cache");

				_InvariantAssembly = typeof(FirebirdClientFactory).Assembly;

				AppDomain.CurrentDomain.AssemblyResolve += (_, args) =>
				{
					if (args.Name == _InvariantAssembly.FullName)
					{
						// Diag.Dug(true, "Dsl Provider Factory failed to load: " + _InvariantAssembly.FullName);
						return _InvariantAssembly;
					}

					return null;
				};
				/*
				AppDomain.CurrentDomain.AssemblyLoad += (_, args) =>
				{
					if (args.LoadedAssembly.FullName == _InvariantAssembly.FullName)
						Diag.Trace("Dsl Provider Factory loaded: " + _InvariantAssembly.FullName);
				};
				*/

			}
			else
			{
				Diag.Trace("DbProviderFactory not added to assembly cache during package registration. Factory already cached");
			}

		}


		#endregion Method Implementations





		// =========================================================================================================
		#region Methods - BlackbirdSqlDdexPackage
		// =========================================================================================================



		// ---------------------------------------------------------------------------------
		/// <summary>
		/// Enables solution and project event handling
		/// </summary>
		// ---------------------------------------------------------------------------------
		private async Task AdviseSolutionEventsAsync()
		{
			await JoinableTaskFactory.SwitchToMainThreadAsync(DisposalToken);
			// Diag.Trace();

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


		#endregion Methods

	}


	#endregion Class Declaration

}