

using System;
using System.Threading;
using System.Threading.Tasks;

using BlackbirdSql.Core;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.EditorExtension;

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;




namespace BlackbirdSql.Controller;



// =========================================================================================================
//										ControllerAsyncPackage Class 
//
/// <summary>
/// BlackbirdSql.Data.Ddex DDEX 2.0 <see cref="IVsPackage"/> class implementation. Implements support for
/// IVsSolution, IVsRunningDocumentTable events through the PackageController.
/// </summary>
/// <remarks>
/// Implements the package exposed by this assembly and registers itself with the shell.
/// </remarks>
// =========================================================================================================
public abstract class ControllerAsyncPackage : EditorExtensionAsyncPackage
{

	#region Variables - ControllerAsyncPackage


	private ControllerEventsManager _EventsManager;


	#endregion Variables






	// =========================================================================================================
	#region Property accessors - ControllerAsyncPackage
	// =========================================================================================================


	public new IBEventsManager EventsManager => _EventsManager ??= new ControllerEventsManager(Controller);


	#endregion Property accessors





	// =========================================================================================================
	#region Constructors / Destructors - ControllerAsyncPackage
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// AbstractAsyncPackage package .ctor
	/// </summary>
	// ---------------------------------------------------------------------------------
	public ControllerAsyncPackage() : base()
	{
	}


	#endregion Constructors / Destructors





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

		await base.InitializeAsync(cancellationToken, progress);

		Services.AddService(typeof(IBPackageController), ServicesCreatorCallbackAsync, promote: true);

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
		if (cancellationToken.IsCancellationRequested)
			return;

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

		else if (serviceType == typeof(IBPackageController))
		{
			try
			{
				object service = PackageController.CreateInstance(this);
				return service ?? throw new TypeAccessException(serviceType.FullName);
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



	protected override void Dispose(bool disposing)
	{
		try
		{
			ThreadHelper.ThrowIfNotOnUIThread();
			((PackageController)_Controller)?.Dispose();
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
		}

		base.Dispose(disposing);
	}


	#endregion Method Implementations

}
