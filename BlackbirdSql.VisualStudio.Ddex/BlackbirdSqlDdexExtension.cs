// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)


using System;
using System.Data.Common;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using BlackbirdSql.Controller;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl;
using BlackbirdSql.Core.Ctl.ComponentModel;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Ctl.Events;
using BlackbirdSql.Core.Ctl.Extensions;
using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.Core.Model.Interfaces;
using BlackbirdSql.VisualStudio.Ddex.Controls.DataTools;
using BlackbirdSql.VisualStudio.Ddex.Ctl;
using BlackbirdSql.VisualStudio.Ddex.Ctl.ComponentModel;
using BlackbirdSql.VisualStudio.Ddex.Ctl.Config;
using BlackbirdSql.VisualStudio.Ddex.Ctl.Interfaces;
using BlackbirdSql.VisualStudio.Ddex.Model;
using BlackbirdSql.VisualStudio.Ddex.Properties;

using FirebirdSql.Data.FirebirdClient;

using Microsoft.VisualStudio;

using Microsoft.VisualStudio.Data.Core;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

using Task = System.Threading.Tasks.Task;


namespace BlackbirdSql.VisualStudio.Ddex;

// =========================================================================================================
//										BlackbirdSqlDdexExtension Class 
//
/// <summary>
/// BlackbirdSql.Data.Ddex DDEX 2.0 <see cref="IVsPackage"/> class implementation.
/// </summary>
/// <remarks>
/// Implements the package exposed by this assembly and registers itself with the shell.
/// This is a multi-Extension class implementation of <see cref="IBAsyncPackage"/>.
/// The current package hieararchy is BlackbirdSqlDdexExtension > <see cref="ControllerAsyncPackage"/> >
/// <see cref="EditorExtension.EditorExtensionAsyncPackage"/> > <see cref="AbstractAsyncPackage"/>.
/// </remarks>
// =========================================================================================================



// ---------------------------------------------------------------------------------------------------------
#region							BlackbirdSqlDdexExtension Class Attributes
// ---------------------------------------------------------------------------------------------------------


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
// [ProvideAutoLoad(VSConstants.UICONTEXT.ShellInitialized_string, PackageAutoLoadFlags.BackgroundLoad)]

[ProvideAutoLoad(SystemData.UIContextGuid, PackageAutoLoadFlags.BackgroundLoad)]
[ProvideUIContextRule(SystemData.UIContextGuid,
	name: "BlackbirdSql UIContext Autoload",
	expression: "(ShellInit | SolutionModal)",
	termNames: ["ShellInit", "SolutionModal"],
	termValues: [VSConstants.UICONTEXT.ShellInitialized_string, VSConstants.UICONTEXT.SolutionOpening_string])]

// Not used
// [ProvideMenuResource(1000, 1)] TBC

// Register the DDEX as a data provider
[VsPackageRegistration]

// Register services
[ProvideService(typeof(IBPackageController), IsAsyncQueryable = true,
	ServiceName = PackageData.PackageControllerServiceName)]
[ProvideService(typeof(IBProviderObjectFactory), IsAsyncQueryable = true,
	ServiceName = PackageData.ProviderObjectFactoryServiceName)]


// Implement Visual studio options/settings
[VsProvideOptionPage(typeof(SettingsProvider.GeneralSettingsPage), SettingsProvider.CategoryName,
	SettingsProvider.SubCategoryName, SettingsProvider.GeneralSettingsPageName, 200, 201, 221)]
[VsProvideOptionPage(typeof(SettingsProvider.DebugSettingsPage), SettingsProvider.CategoryName,
	SettingsProvider.SubCategoryName, SettingsProvider.DebugSettingsPageName, 200, 201, 222)]
[VsProvideOptionPage(typeof(SettingsProvider.EquivalencySettingsPage), SettingsProvider.CategoryName,
	SettingsProvider.SubCategoryName, SettingsProvider.EquivalencySettingsPageName, 200, 201, 223)]


#endregion Class Attributes




// =========================================================================================================
#region							BlackbirdSqlDdexExtension Class Declaration
//
// =========================================================================================================
[Guid(SystemData.PackageGuid)]
public sealed class BlackbirdSqlDdexExtension : ControllerAsyncPackage
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - BlackbirdSqlDdexExtension
	// ---------------------------------------------------------------------------------


	/// <summary>
	/// BlackbirdSqlDdexExtension package .ctor
	/// </summary>
	public BlackbirdSqlDdexExtension() : base()
	{
	}


	/// <summary>
	/// BlackbirdSqlDdexExtension package disposal 
	/// </summary>
	/// <param name="disposing"></param>
	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}



	static BlackbirdSqlDdexExtension()
	{
		CacheInvariant();
	}


	#endregion Constructors / Destructors




	// =========================================================================================================
	#region Property accessors - BlackbirdSqlDdexExtension
	// =========================================================================================================


	public override bool InvariantResolved => _InvariantResolved;

	/// <summary>
	/// Accessor to user options at this level of the <see cref="IBAsyncPackage"/> class hierarchy.
	/// </summary>
	private PersistentSettings ExtensionSettings => (PersistentSettings)PersistentSettings.Instance;


	#endregion Property accessors




	// =========================================================================================================
	#region Method Implementations - BlackbirdSqlDdexExtension
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Adds FirebirdClient to assembly cache
	/// </summary>
	// ---------------------------------------------------------------------------------
	private static void CacheInvariant()
	{
		if (!DbProviderFactoriesEx.AddAssemblyToCache(SystemData.Invariant, 
			Resources.Provider_ShortDisplayName, Resources.Provider_DisplayName,
			typeof(FirebirdClientFactory).AssemblyQualifiedName))
		{
			_InvariantResolved = true;
			return;
		}

		AppDomain.CurrentDomain.AssemblyResolve += (_, args) =>
		{
			if (args.Name == typeof(FirebirdClientFactory).Assembly.FullName)
			{
				// if (!_InvariantResolved)
				//	Tracer.Trace(typeof(BlackbirdSqlDdexExtension), "CacheInvariant()", "Assembly resolved: {0}.", args.Name);

				_InvariantResolved = true;
				return typeof(FirebirdClientFactory).Assembly;
			}

			return null;
		};

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets a service interface from this service provider.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override TInterface GetService<TService, TInterface>()
	{
		TInterface instance = null;
		Type type = typeof(TService);

		ThreadHelper.JoinableTaskFactory.Run(async delegate
		{
			instance = await CreateServiceInstanceAsync(type, default) as TInterface;
		});


		// Tracer.Trace(GetType(), "GetService()", "TService: {0}, TInterface: {1}, instance: {2}", type, typeof(TInterface), instance );

		instance ??= ((AsyncPackage)this).GetService<TService, TInterface>();

		return instance;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets a service interface from this service provider asynchronously.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override Task<TInterface> GetServiceAsync<TService, TInterface>()
	{
		Type type = typeof(TService);

		Task<object> task = CreateServiceInstanceAsync(type, default);

		if (task == null || task == Task.FromResult<object>(null))
			return ((AsyncPackage)this).GetServiceAsync<TService, TInterface>();

		return task as Task<TInterface>;
	}




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

		// Moved to main thread
		// Services.AddService(typeof(IBProviderObjectFactory), ServicesCreatorCallbackAsync, promote: true);


		// Perform any final initialization tasks.
		// It is the final descendent package class's responsibility to initiate the call to FinalizeAsync.
		await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);


		// We are the final descendent package class so call FinalizeAsync().
		await FinalizeAsync(cancellationToken, progress);


	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Final asynchronous initialization tasks for the package that must occur after
	/// all descendents and ancestors have completed their InitializeAsync() tasks.
	/// It is the final descendent package class's responsibility to initiate the call
	/// to FinalizeAsync.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override async Task FinalizeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
	{
		Diag.ThrowIfNotOnUIThread();

		if (cancellationToken.IsCancellationRequested)
			return;

		// Load all packages settings models and propogate throughout the extension hierarchy.
		PropagateSettings();

		// Add provider object and schema factories
		ServiceContainer.AddService(typeof(IBProviderObjectFactory), ServicesCreatorCallbackAsync, promote: true);
		ServiceContainer.AddService(typeof(IBProviderSchemaFactory), ServicesCreatorCallbackAsync, promote: true);
		ServiceContainer.AddService(typeof(IVsDataConnectionDialog), ServicesCreatorCallbackAsync, promote: true);

		// Descendents have completed their final async initialization now we perform ours.
		await base.FinalizeAsync(cancellationToken, progress);

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Creates a service instance of the specified type if this class has access to the
	/// final class type of the service being added.
	/// The class requiring and adding the service may not necessarily be the class that
	/// creates an instance of the service.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override async Task<object> CreateServiceInstanceAsync(Type serviceType, CancellationToken token)
	{
		if (serviceType == null)
		{
			ArgumentNullException ex = new("serviceType");
			Diag.Dug(ex);
			throw ex;
		}

		if (serviceType == typeof(IBProviderObjectFactory) || serviceType == typeof(IVsDataProviderObjectFactory))
		{
			return new TProviderObjectFactory();
		}
		if (serviceType == typeof(IBProviderSchemaFactory))
		{
			return new DslProviderSchemaFactory();
		}
		if (serviceType == typeof(IVsDataConnectionDialog))
		{
			return new TDataConnectionDlgHandler();
		}


		return await base.CreateServiceInstanceAsync(serviceType, token);

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Initializes and configures a service of the specified type that is used by this
	/// Package.
	/// Configuration is performed by the class requiring the service.
	/// The actual instance creation of the service is the responsibility of the class
	/// Package that has access to the final descendent class of the Service.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override async Task<object> ServicesCreatorCallbackAsync(IAsyncServiceContainer container, CancellationToken token, Type serviceType)
	{
		if (serviceType == typeof(IBProviderObjectFactory) || serviceType == typeof(IBProviderSchemaFactory))
			return await CreateServiceInstanceAsync(serviceType, token);


		return await base.ServicesCreatorCallbackAsync(container, token, serviceType);
	}


	#endregion Method Implementations




	// =========================================================================================================
	#region Methods - BlackbirdSqlDdexExtension
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Starts up extension user options push notifications. Only the final class in
	/// the <see cref="IBAsyncPackage"/> class hierarchy should implement the method.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected override void PropagateSettings()
	{
		if (_Initialized)
			return;

		_Initialized = true;

		PropagateSettingsEventArgs e = new();

		ExtensionSettings.PopulateSettingsEventArgs(ref e);
		ExtensionSettings.PropagateSettings(e);
		ExtensionSettings.RegisterSettingsEventHandlers(ExtensionSettings.OnSettingsSaved);
	}


	#endregion Methods




	// =========================================================================================================
	#region Event handlers - BlackbirdSqlDdexExtension
	// =========================================================================================================


	#endregion Event handlers

}


#endregion Class Declaration
