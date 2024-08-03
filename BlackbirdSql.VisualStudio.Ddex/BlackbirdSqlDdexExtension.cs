// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Threading;
using System.Threading.Tasks;
using BlackbirdSql.Controller;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl.ComponentModel;
using BlackbirdSql.Core.Extensions;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.EditorExtension;
using BlackbirdSql.LanguageExtension;
using BlackbirdSql.Sys.Events;
using BlackbirdSql.Sys.Interfaces;
using BlackbirdSql.VisualStudio.Ddex.Controls.DataTools;
using BlackbirdSql.VisualStudio.Ddex.Ctl;
using BlackbirdSql.VisualStudio.Ddex.Ctl.ComponentModel;
using BlackbirdSql.VisualStudio.Ddex.Ctl.Config;
using BlackbirdSql.VisualStudio.Ddex.Interfaces;
using BlackbirdSql.VisualStudio.Ddex.Properties;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Data.Core;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;
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
/// This is a multi-Package daisy-chained class implementation of <see cref="IBsAsyncPackage"/>.
/// The current hierarchy is <see cref="BlackbirdSqlDdexExtension"/> >> <see cref="ControllerPackage"/>
/// >> <see cref="LanguageExtensionPackage"/> >> <see cref="EditorExtensionPackage"/>
/// >> <see cref="AbstractCorePackage"/>.
/// </remarks>
// =========================================================================================================



// ---------------------------------------------------------------------------------------------------------
#region							BlackbirdSqlDdexExtension Class Attributes
// ---------------------------------------------------------------------------------------------------------


// Register this class as a VS package.
[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]


// 'Help About' registration. productName & productDetails resource integers must be prefixed with #.
[InstalledProductRegistration("#100", "#102", ExtensionData.C_VsixVersion, IconResourceID = 400)]


// We start loading as soon as the VS shell is available.
// We may be loaded sooner than this UIContext if (on IDE startup) VS requests our editor on
// a document that was left open. In that case we use the static property accessor "Instance"
// to load our package ourselves.

[ProvideUIContextRule(SystemData.C_UIContextGuid,
	name: SystemData.C_UIContextName,
	expression: "(ShellInit | SolutionOpening | DesignMode | DataSourceWindowVisible | DataSourceWindowSupported)",
	termNames: ["ShellInit", "SolutionOpening", "DesignMode", "DataSourceWindowVisible", "DataSourceWindowSupported"],
	termValues: [VSConstants.UICONTEXT.ShellInitialized_string,
		VSConstants.UICONTEXT.SolutionOpening_string,
		VSConstants.UICONTEXT.DesignMode_string,
		UIContextGuids80.DataSourceWindowAutoVisible,
		UIContextGuids80.DataSourceWindowSupported])]


[ProvideAutoLoad(SystemData.C_UIContextGuid, PackageAutoLoadFlags.BackgroundLoad)]


[ProvideMenuResource("Menus.ctmenu", 1)]

// Register the DDEX as a data provider
[VsPackageRegistration]

// Register services
[ProvideService(typeof(IBsPackageController), IsAsyncQueryable = true,
	ServiceName = ExtensionData.C_PackageControllerServiceName)]
[ProvideService(typeof(IBsProviderObjectFactory), IsAsyncQueryable = true,
	ServiceName = ExtensionData.C_ProviderObjectFactoryServiceName)]

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
public sealed class BlackbirdSqlDdexExtension : ControllerPackage
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - BlackbirdSqlDdexExtension
	// ---------------------------------------------------------------------------------


	/// <summary>
	/// BlackbirdSqlDdexExtension default .ctor
	/// </summary>
	public BlackbirdSqlDdexExtension() : base()
	{
	}


	/// <summary>
	/// BlackbirdSqlDdexExtension static .ctor
	/// </summary>
	static BlackbirdSqlDdexExtension()
	{
		RegisterDataServices();
		RegisterAssemblies();
	}


	/// <summary>
	/// BlackbirdSqlDdexExtension package disposal 
	/// </summary>
	/// <param name="disposing"></param>
	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Property accessors - BlackbirdSqlDdexExtension
	// =========================================================================================================


	/// <summary>
	/// Performs a DemandLoadPackage() when we don't exist. This can occur where
	/// VS has requested our editor on an open document before we were loaded.
	/// </summary>
	public static new BlackbirdSqlDdexExtension Instance
	{
		get
		{
			// if (_Instance == null)
			//	DemandLoadPackage(Sys.LibraryData.AsyncPackageGuid, out _);
			return (BlackbirdSqlDdexExtension)_Instance;
		}
	}


	/// <summary>
	/// Accessor to user options at this level of the <see cref="IBsAsyncPackage"/> class hierarchy.
	/// </summary>
	private PersistentSettings ExtensionSettings => (PersistentSettings)PersistentSettings.Instance;


	#endregion Property accessors





	// =========================================================================================================
	#region Method Implementations - BlackbirdSqlDdexExtension
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets a service interface from this service provider.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override TInterface GetService<TService, TInterface>() where TInterface : class
	{
		TInterface instance = null;
		Type type = typeof(TService);

		// Fire and wait

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
	// [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD103:Call async methods when in an async method", Justification = "<Pending>")]
	public async override Task<TInterface> GetServiceAsync<TService, TInterface>()
	{
		Type type = typeof(TService);

		object svc = await CreateServiceInstanceAsync(type, default);


		svc ??= await ((AsyncPackage)this).GetServiceAsync<TService, TInterface>();

		return (TInterface)svc;
	}




	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Asynchronous initialization of the package
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
	{
		ProgressAsync(progress, "Initializing extension...").Forget();

		_S_Stopwatch.Start();

		ProgressAsync(progress, "Extension propagating user settings...").Forget();

		// Load all packages settings models and propogate throughout the extension hierarchy.
		PropagateSettings();

		ProgressAsync(progress, "Extension propagating user settings... Done.").Forget();

		await base.InitializeAsync(cancellationToken, progress);

		ProgressAsync(progress, "Extension registering DDEX Provider services...").Forget();

		if (ApcManager.IdeShutdownState)
			return;

		// Add provider object and schema factories
		ServiceContainer.AddService(typeof(IBsProviderObjectFactory), ServicesCreatorCallbackAsync, promote: true);
		ServiceContainer.AddService(typeof(IBsDataConnectionDlgHandler), ServicesCreatorCallbackAsync, promote: true);
		ServiceContainer.AddService(typeof(IBsDataConnectionPromptDialogHandler), ServicesCreatorCallbackAsync, promote: true);

		ProgressAsync(progress, "Extension registering DDEX Provider services... Done.").Forget();



		// Perform any final initialization tasks.
		// It is the final descendent package class's responsibility to initiate the call to FinalizeAsync.


		long elapsed = _S_Stopwatch.ElapsedTicks;


		ProgressAsync(progress, "Extension Initialization launching Finalization phase...",
			_EnStatisticsStage.MainThreadLoadStart, elapsed).Forget();

		// We are the final descendent package class so call FinalizeAsync().
		await FinalizeAsync(cancellationToken, progress);

		ProgressAsync(progress, "Extension Initialization complete... Loaded.").Forget();
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
		if (cancellationToken.IsCancellationRequested || ApcManager.IdeShutdownState)
			return;

		ProgressAsync(progress, "Finalizing phase launched. Switching to main thread...").Forget();

		await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

		long elapsed = _S_Stopwatch.ElapsedTicks;

		ProgressAsync(progress, "Finalizing phase launched. Switch to main thread... Done.",
			_EnStatisticsStage.MainThreadLoadEnd, elapsed).Forget();



		// Descendents have completed their final async initialization now we perform ours.
		await base.FinalizeAsync(cancellationToken, progress);

		elapsed = _S_Stopwatch.ElapsedTicks;
		_S_Stopwatch = null;

		ProgressAsync(progress, "Finalization phase completed.",
			_EnStatisticsStage.InitializationEnd, elapsed).Forget();

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

		if (serviceType == typeof(IBsProviderObjectFactory) || serviceType == typeof(IVsDataProviderObjectFactory))
		{
			return new TProviderObjectFactory();
		}
		if (serviceType == typeof(IBsDataConnectionDlgHandler))
		{
			return new TDataConnectionDlgHandler();
		}
		if (serviceType == typeof(IBsDataConnectionPromptDialogHandler))
		{
			return new TConnectionPromptDialogHandler();
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
		if (serviceType == typeof(IBsProviderObjectFactory))
			return await CreateServiceInstanceAsync(serviceType, token);


		return await base.ServicesCreatorCallbackAsync(container, token, serviceType);
	}


	#endregion Method Implementations




	// =========================================================================================================
	#region Methods - BlackbirdSqlDdexExtension
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Adds Database Client invariant to assembly factory register.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private static void RegisterAssemblies()
	{
		DbProviderFactoriesEx.RegisterAssemblyDirect(NativeDb.Invariant,
			Resources.Provider_ShortDisplayName, Resources.Provider_DisplayName,
			NativeDb.AssemblyQualifiedName);


		AppDomain.CurrentDomain.AssemblyResolve += (_, args) =>
		{

			if (args.Name == typeof(BlackbirdSqlDdexExtension).Assembly.FullName)
				return typeof(BlackbirdSqlDdexExtension).Assembly;

			if (NativeDb.MatchesInvariantAssembly(args.Name))
				return NativeDb.ClientFactoryAssembly;

			if (NativeDb.MatchesEntityFrameworkAssembly(args.Name))
				return NativeDb.EntityFrameworkAssembly;

			return null;
		};
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Starts up extension user options push notifications. Only the final class in
	/// the <see cref="IBsAsyncPackage"/> class hierarchy should implement the method.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected override void PropagateSettings()
	{
		if (_InitializedSettings)
			return;

		_InitializedSettings = true;

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
