
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BlackbirdSql.Core;
using BlackbirdSql.EditorExtension;
using BlackbirdSql.Sys.Interfaces;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;


namespace BlackbirdSql.Controller;

// =========================================================================================================
//										ControllerPackage Class 
//
/// <summary>
/// BlackbirdSql.Data.Ddex DDEX 2.0 <see cref="IVsPackage"/> controller class implementation. Implements
/// support for SolutionOption, IVsSolution, IVsRunningDocumentTable events through the PackageController.
/// </summary>
/// <remarks>
/// This is a multi-Extension class implementation of <see cref="IBsAsyncPackage"/>.
/// The current package hieararchy is BlackbirdSqlDdexExtension > <see cref="ControllerPackage"/> >
/// <see cref="EditorExtension.EditorExtensionPackage"/> > <see cref="AbstractCorePackage"/>.
/// </remarks>
// =========================================================================================================
public abstract class ControllerPackage : EditorExtensionPackage
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - ControllerPackage
	// ---------------------------------------------------------------------------------


	/// <summary>
	/// ControllerPackage .ctor
	/// </summary>
	protected ControllerPackage() : base()
	{
		// Enable solution open/close event handling.
		AddOptionKey(SystemData.C_SolutionKey);

		// Diag.DebugTrace($"Called AddOptionKey():  Key: {SystemData.C_SolutionKey}.");

		// Create the Controller Events Manager. 
		_EventsManager = ControllerEventsManager.CreateInstance(_ApcInstance);
	}



	/// <summary>
	/// ControllerPackage static .ctor
	/// </summary>
	static ControllerPackage()
	{
		RegisterAssemblies();
	}



	/// <summary>
	/// Instance disposal.
	/// </summary>
	protected override void Dispose(bool disposing)
	{
		try
		{
			ApcManager.ShutdownDte();

			_ApcInstance?.Dispose();
			_ApcInstance = null;
			_EventsManager?.Dispose();
			_EventsManager = null;
		}
		catch (Exception ex)
		{
			Diag.Ex(ex);
		}

		base.Dispose(disposing);
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - ControllerPackage
	// =========================================================================================================


	private ControllerEventsManager _EventsManager;


	#endregion Fields





	// =========================================================================================================
	#region Property accessors - ControllerPackage
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Accessor to the events manager at this level of the <see cref="IBsAsyncPackage"/>
	/// class hierarchy.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public new IBsEventsManager EventsManager => _EventsManager;



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Accessor to the <see cref="IBsPackageController"/> singleton instance
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override IBsPackageController ApcInstance => _ApcInstance;


	#endregion Property accessors





	// =========================================================================================================
	#region Method Implementations - ControllerPackage
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Final asynchronous initialization tasks for the package that must occur after
	/// all descendents and ancestors have completed their InitializeAsync() tasks.
	/// It is the final descendent package class's responsibility to initiate the call
	/// to FinalizeAsync.
	/// </summary>
	public override async Task FinalizeAsync(CancellationToken cancelToken, IProgress<ServiceProgressData> progress)
	{
		Diag.ThrowIfNotOnUIThread();

		if (cancelToken.Cancelled() || ApcManager.IdeShutdownState)
			return;


		ProgressAsync(progress, "Finalizing ApcManager...").Forget();

		ProgressAsync(progress, "Finalizing ApcManager. Advising Events...").Forget();

		// Second try.
		await _ApcInstance.AdviseUnsafeEventsAsync();
		await _ApcInstance.RegisterProjectEventHandlersAsync();

		ProgressAsync(progress, "Finalizing ApcManager. Advising Events... Done.").Forget();

		await base.FinalizeAsync(cancelToken, progress);



		// If we get here and the Rct is not loaded/loading it means "no solution".
		if (!RctManager.Loading)
		{
			ProgressAsync(progress, "Finalizing ApcManager. Loading Running Connection Table...").Forget();

			RctManager.LoadConfiguredConnections();

			ProgressAsync(progress, "Finalizing ApcManager. Loading Running Connection Table... Done.").Forget();
		}



		ProgressAsync(progress, "Finalizing ApcManager... Done.").Forget();
		
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Creates a service instance of the specified type if this class has access to the
	/// final class type of the service being added.
	/// The class requiring and adding the service may not necessarily be the class that
	/// creates an instance of the service.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override async Task<TInterface> GetLocalServiceInstanceAsync<TService, TInterface>(CancellationToken token)
		 where TInterface : class
	{
		Type serviceType = typeof(TService);

		if (serviceType == typeof(IBsPackageController))
			return PackageController.Instance as TInterface;

		return await base.GetLocalServiceInstanceAsync<TService, TInterface>(token);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Asynchronous initialization of the package. The class must register services it
	/// requires using the ServicesCreatorCallback method.
	/// </summary>
	protected override async Task InitializeAsync(CancellationToken cancelToken, IProgress<ServiceProgressData> progress)
	{

		if (cancelToken.Cancelled() || ApcManager.IdeShutdownState)
			return;


		ProgressAsync(progress, "Initializing ApcManager...").Forget();

		await base.InitializeAsync(cancelToken, progress);



		ProgressAsync(progress, "ApcManager requesting Event propogatation...").Forget();

		// First try.
		Task.Run(ApcInstance.AdviseUnsafeEventsAsync).Forget();
		Task.Run(ApcInstance.RegisterProjectEventHandlersAsync).Forget();

		ProgressAsync(progress, "ApcManager requesting Event propogation... Done.").Forget();



		ProgressAsync(progress, "ApcManager loading service...").Forget();

		AddService(typeof(IBsPackageController), ServicesCreatorCallbackAsync, promote: false);

		ProgressAsync(progress, "ApcManager loading service... Done.").Forget();



		ProgressAsync(progress, "Initializing ApcManager ... Done.").Forget();

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
		if (serviceType == typeof(IBsPackageController))
			return await GetLocalServiceInstanceAsync<IBsPackageController, IBsPackageController>(token);

		return await base.ServicesCreatorCallbackAsync(container, token, serviceType);
	}


	#endregion Method Implementations





	// =========================================================================================================
	#region Methods - ControllerPackage
	// =========================================================================================================


	protected override IBsPackageController CreateController()
	{
		return PackageController.CreateInstance(this);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Adds this assembly to CurrentDomain.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private static void RegisterAssemblies()
	{
		AppDomain.CurrentDomain.AssemblyResolve += (_, args) =>
		{
			if (args.Name == typeof(ControllerPackage).Assembly.FullName)
				return typeof(ControllerPackage).Assembly;

			return null;
		};
	}


	#endregion Methods




	// =========================================================================================================
	#region Event handlers - ControllerPackage
	// =========================================================================================================



	protected override void OnLoadOptions(string key, Stream stream)
	{
		// Diag.DebugTrace($"OnLoadOptions():  Key: {key}.");

		// If this is called early we have to initialize user option push notifications
		// and environment events synchronously.
		// PropagateSettings();

		ApcInstance.AdviseUnsafeEventsAsyeu();
		ApcInstance.RegisterProjectEventHandlersAsyeu();

		// Diag.DebugTrace($"OnLoadOptions():  Invoking.");

		if (key == SystemData.C_SolutionKey)
			_OnLoadSolutionOptionsEvent?.Invoke(stream);
		else
			base.OnLoadOptions(key, stream);
	}


	protected override void OnSaveOptions(string key, Stream stream)
	{
		if (key == SystemData.C_SolutionKey)
			_OnSaveSolutionOptionsEvent?.Invoke(stream);
		else
			base.OnSaveOptions(key, stream);
	}


	#endregion Event handlers


}
