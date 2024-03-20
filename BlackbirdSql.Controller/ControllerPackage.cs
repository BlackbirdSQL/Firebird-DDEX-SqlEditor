
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl;
using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.EditorExtension;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;


namespace BlackbirdSql.Controller;

// =========================================================================================================
//										ControllerPackage Class 
//
/// <summary>
/// BlackbirdSql.Data.Ddex DDEX 2.0 <see cref="IVsPackage"/> controller class implementation. Implements
/// support for SolutionOption, IVsSolution, IVsRunningDocumentTable events through the PackageController.
/// </summary>
/// <remarks>
/// This is a multi-Extension class implementation of <see cref="IBAsyncPackage"/>.
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
	public ControllerPackage() : base()
	{
		// Enable solution open/close event handling.
		AddOptionKey(GlobalsAgent.C_PersistentKey);

		// Create the Controller.
		// Create the Controller Events Manager. 
		_EventsManager = ControllerEventsManager.CreateInstance(_ApcInstance);
	}


	/// <summary>
	/// Instance disposal.
	/// </summary>
	protected override void Dispose(bool disposing)
	{
		try
		{
			ApcManager.ResetDte();

			_ApcInstance?.Dispose();
			_ApcInstance = null;
			_EventsManager?.Dispose();
			_EventsManager = null;
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
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
	/// Accessor to the events manager at this level of the <see cref="IBAsyncPackage"/>
	/// class hierarchy.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public new IBEventsManager EventsManager => _EventsManager;



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Accessor to the <see cref="IBPackageController"/> singleton instance
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override IBPackageController ApcInstance => _ApcInstance;


	#endregion Property accessors




	// =========================================================================================================
	#region Method Implementations - ControllerPackage
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Asynchronous initialization of the package. The class must register services it
	/// requires using the ServicesCreatorCallback method.
	/// </summary>
	protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
	{
		if (cancellationToken.IsCancellationRequested || ApcManager.IdeShutdownState)
			return;

		await base.InitializeAsync(cancellationToken, progress);

		ServiceProgressData progressData = new("Loading BlackbirdSql", "Loading Controller Event Manager", 2, 15);
		progress.Report(progressData);

		// First try.
		await ApcInstance.AdviseEventsAsync();

		progressData = new("Loading BlackbirdSql", "Done Loading Controller Event Manager", 3, 15);
		progress.Report(progressData);



		progressData = new("Loading BlackbirdSql", "Loading Controller Service", 4, 15);
		progress.Report(progressData);

		ServiceContainer.AddService(typeof(IBPackageController), ServicesCreatorCallbackAsync, promote: true);

		progressData = new("Loading BlackbirdSql", "Done Loading Controller Service", 5, 15);
		progress.Report(progressData);

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Final asynchronous initialization tasks for the package that must occur after
	/// all descendents and ancestors have completed their InitializeAsync() tasks.
	/// It is the final descendent package class's responsibility to initiate the call
	/// to FinalizeAsync.
	/// </summary>
	public override async Task FinalizeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
	{
		Diag.ThrowIfNotOnUIThread();

		if (cancellationToken.IsCancellationRequested || ApcManager.IdeShutdownState)
			return;

		ServiceProgressData progressData = new("Loading BlackbirdSql", "Finalizing: Advising Controller Events", 8, 15);
		progress.Report(progressData);

		// Second try.
		_ApcInstance.AdviseEvents();

		progressData = new("Loading BlackbirdSql", "Finalizing: Done Advising Controller Events", 9, 15);
		progress.Report(progressData);

		await base.FinalizeAsync(cancellationToken, progress);


		// If we get here and the Rct is not loaded/loading it means "no solution".
		if (!RctManager.Loading)
		{
			progressData = new("Loading BlackbirdSql", "Finalizing: Loading Running Connection Table", 13, 15);
			progress.Report(progressData);

			RctManager.LoadConfiguredConnections();

			progressData = new("Loading BlackbirdSql", "Finalizing: Loaded Running Connection Table", 14, 15);
			progress.Report(progressData);
		}

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

		else if (serviceType == typeof(IBPackageController))
		{
			try
			{
				return PackageController.Instance;
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw ex;
			}
		}
		else if (serviceType.IsInstanceOfType(this))
		{
			return this;
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
		if (serviceType == typeof(IBPackageController))
		{
			return await CreateServiceInstanceAsync(serviceType, token);
		}

		return await base.ServicesCreatorCallbackAsync(container, token, serviceType);
	}


	#endregion Method Implementations




	// =========================================================================================================
	#region Methods - ControllerPackage
	// =========================================================================================================


	protected override IBPackageController CreateController()
	{
		return PackageController.CreateInstance(this);
	}

	#endregion Methods




	// =========================================================================================================
	#region Event handlers - ControllerPackage
	// =========================================================================================================



	protected override void OnLoadOptions(string key, Stream stream)
	{
		// If this is called early we have to initialize user option push notifications
		// and environment events synchronously.
		PropagateSettings();
		ApcInstance.AdviseEvents();

		if (key == GlobalsAgent.C_PersistentKey)
			_OnLoadSolutionOptionsEvent?.Invoke(stream);
		else
			base.OnLoadOptions(key, stream);
	}


	protected override void OnSaveOptions(string key, Stream stream)
	{
		if (key == GlobalsAgent.C_PersistentKey)
			_OnSaveSolutionOptionsEvent?.Invoke(stream);
		else
			base.OnSaveOptions(key, stream);
	}


	#endregion Event handlers


}
