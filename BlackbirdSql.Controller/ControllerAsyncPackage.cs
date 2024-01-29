
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.Core.Model;
using BlackbirdSql.EditorExtension;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;


namespace BlackbirdSql.Controller;

// =========================================================================================================
//										ControllerAsyncPackage Class 
//
/// <summary>
/// BlackbirdSql.Data.Ddex DDEX 2.0 <see cref="IVsPackage"/> controller class implementation. Implements
/// support for SolutionOption, IVsSolution, IVsRunningDocumentTable events through the PackageController.
/// </summary>
/// <remarks>
/// This is a multi-Extension class implementation of <see cref="IBAsyncPackage"/>.
/// The current package hieararchy is BlackbirdSqlDdexExtension > <see cref="ControllerAsyncPackage"/> >
/// <see cref="EditorExtension.EditorExtensionAsyncPackage"/> > <see cref="AbstractAsyncPackage"/>.
/// </remarks>
// =========================================================================================================
public abstract class ControllerAsyncPackage : EditorExtensionAsyncPackage
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - ControllerAsyncPackage
	// ---------------------------------------------------------------------------------


	/// <summary>
	/// AbstractAsyncPackage package .ctor
	/// </summary>
	public ControllerAsyncPackage() : base()
	{
		// Enable solution open/close event handling.
		AddOptionKey(GlobalsAgent.C_PersistentKey);

		// Create the Controller.
		// Create the Controller Events Manager. 
		_EventsManager = ControllerEventsManager.CreateInstance(_Controller);

	}


	/// <summary>
	/// Instance disposal.
	/// </summary>
	protected override void Dispose(bool disposing)
	{
		try
		{
			_Controller?.Dispose();
			_EventsManager?.Dispose();
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
		}

		base.Dispose(disposing);
	}


	#endregion Constructors / Destructors




	// =========================================================================================================
	#region Fields - ControllerAsyncPackage
	// =========================================================================================================


	private readonly ControllerEventsManager _EventsManager;


	#endregion Fields




	// =========================================================================================================
	#region Property accessors - ControllerAsyncPackage
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
	public override IBPackageController Controller => _Controller;


	#endregion Property accessors




	// =========================================================================================================
	#region Method Implementations - ControllerAsyncPackage
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Asynchronous initialization of the package. The class must register services it
	/// requires using the ServicesCreatorCallback method.
	/// </summary>
	protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
	{
		if (cancellationToken.IsCancellationRequested)
			return;

		// First try.
		await Controller.AdviseEventsAsync();


		await base.InitializeAsync(cancellationToken, progress);

		ServiceContainer.AddService(typeof(IBPackageController), ServicesCreatorCallbackAsync, promote: true);

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

		if (cancellationToken.IsCancellationRequested)
			return;

		// Second try.
		_Controller.AdviseEvents();

		await base.FinalizeAsync(cancellationToken, progress);

		// If we get here and the Rct is not loaded/loading it means "no solution".
		if (!RctManager.Loading)
			RctManager.LoadConfiguredConnections(true);
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
	#region Methods - ControllerAsyncPackage
	// =========================================================================================================


	protected override IBPackageController CreateController()
	{
		return PackageController.CreateInstance(this);
	}

	#endregion Methods




	// =========================================================================================================
	#region Event handlers - ControllerAsyncPackage
	// =========================================================================================================



	protected override void OnLoadOptions(string key, Stream stream)
	{
		// If this is called early we have to initialize user option push notifications
		// and environment events synchronously.
		PropagateSettings();
		Controller.AdviseEvents();

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
