using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BlackbirdSql.Core.Ctl;
using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.Core.Properties;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.RpcContracts.FileSystem;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;



namespace BlackbirdSql.Core;


// =========================================================================================================
//										AbstractCorePackage Class 
//
/// <summary>
/// BlackbirdSql <see cref="AsyncPackage"/> base class
/// </summary>
// =========================================================================================================
public abstract class AbstractCorePackage : AsyncPackage, IBAsyncPackage
{

	// -----------------------------------------------------------------------------------------------------
	#region Constructors / Destructors - AbstractCorePackage
	// -----------------------------------------------------------------------------------------------------


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// AbstractCorePackage package .ctor
	/// </summary>
	// ---------------------------------------------------------------------------------
	public AbstractCorePackage()
	{
		if (_Instance != null)
		{
			TypeAccessException ex = new(Resources.ExceptionDuplicateSingletonInstances.FmtRes("Ddex extension package instances"));
			Diag.Dug(ex);
			throw ex;
		}

		_Instance = this;

		Diag.Context = "IDE";

		try
		{
			UriParser.Register(new FbsqlStyleUriParser(SystemData.UriParserOptions), SystemData.Protocol, 0);
		}
		catch (Exception ex)
		{
			Diag.ThrowException(ex);
		}

		_ApcInstance = CreateController();

		RctManager.CreateInstance();
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// AbstractCorePackage package destructor 
	/// </summary>
	/// <param name="disposing"></param>
	// ---------------------------------------------------------------------------------
	protected override void Dispose(bool disposing)
	{
		RctManager.Instance?.Dispose();

		base.Dispose(disposing);
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - AbstractCorePackage
	// =========================================================================================================



	protected static Package _Instance = null;

	protected IBPackageController _ApcInstance = null;
	private IDisposable _DisposableWaitCursor;
	protected IVsSolution _VsSolution = null;
	protected bool _Initialized = false;

	protected IBAsyncPackage.LoadSolutionOptionsDelegate _OnLoadSolutionOptionsEvent;
	protected IBAsyncPackage.SaveSolutionOptionsDelegate _OnSaveSolutionOptionsEvent;

	#endregion Fields





	// =========================================================================================================
	#region Property accessors - AbstractCorePackage
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Accessor to the <see cref="IBPackageController"/> singleton instance
	/// </summary>
	// ---------------------------------------------------------------------------------
	public virtual IBPackageController ApcInstance => _ApcInstance
		??= (IBPackageController)GetGlobalService(typeof(IBPackageController));


	public IDisposable DisposableWaitCursor
	{
		get { return _DisposableWaitCursor; }
		set { _DisposableWaitCursor = value; }
	}






	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Accessor to the current <see cref="IVsSolution"/> instance.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public virtual IVsSolution VsSolution
	{
		get
		{
			if (_VsSolution == null)
			{
				if (GetService(typeof(SVsSolution)) is not IVsSolution service)
					Diag.ExceptionService(typeof(IVsSolution));
				else
					_VsSolution = service;

				// If it's null there's an issue. Possibly we've come in too early
				if (_VsSolution == null)
				{
					NullReferenceException ex = new("SVsSolution is null");
					Diag.Dug(ex);
				}
			}
			return _VsSolution;
		}
	}


	public abstract IBEventsManager EventsManager { get; }


	public abstract Type SchemaFactoryType { get; }


	public Microsoft.VisualStudio.OLE.Interop.IServiceProvider OleServiceProvider
	{
		get
		{
			if (GetService(typeof(Microsoft.VisualStudio.OLE.Interop.IServiceProvider))
				is not Microsoft.VisualStudio.OLE.Interop.IServiceProvider provider)
			{
				throw Diag.ExceptionService(typeof(Microsoft.VisualStudio.OLE.Interop.IServiceProvider));
			}

			return provider;
		}
	}

	public abstract IFileSystemProvider FileSystemBrokeredService { get; }

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Accessor to the 'this' cast as the <see cref="IAsyncServiceContainer"/>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public virtual IAsyncServiceContainer ServiceContainer => this;


	/// <summary>
	/// Accessor to the <see cref="Package.OnLoadOptions"/> event.
	/// </summary>
	event IBAsyncPackage.LoadSolutionOptionsDelegate IBAsyncPackage.OnLoadSolutionOptionsEvent
	{
		add { _OnLoadSolutionOptionsEvent += value; }
		remove { _OnLoadSolutionOptionsEvent -= value; }
	}

	/// <summary>
	/// Accessor to the <see cref="Package.OnLoadOptions"/> event.
	/// </summary>
	event IBAsyncPackage.SaveSolutionOptionsDelegate IBAsyncPackage.OnSaveSolutionOptionsEvent
	{
		add { _OnSaveSolutionOptionsEvent += value; }
		remove { _OnSaveSolutionOptionsEvent -= value; }
	}


	#endregion Property accessors





	// =========================================================================================================
	#region Method Implementations - AbstractCorePackage
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Creates a service instance of the specified type if this class has access to the
	/// final class type of the service being added.
	/// The class requiring and adding the service may not necessarily be the class that
	/// creates an instance of the service.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public virtual Task<object> CreateServiceInstanceAsync(Type serviceType, CancellationToken token) => Task.FromResult<object>(null);



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Final asynchronous initialization tasks for the package that must occur after
	/// all descendents and ancestors have completed their InitializeAsync() tasks.
	/// It is the final descendent package class's responsibility to initiate the call
	/// to FinalizeAsync.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public virtual async Task FinalizeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
	{
		await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);


		// Sample format of FinalizeAsync in descendents

		// if (cancellationToken.IsCancellationRequested)
		//	return;

		// await base.FinalizeAsync(cancellationToken, progress);


		// Sample add service call

		// ServiceContainer.AddService(typeof(ICustomService), ServiceCreatorCallbackMethod, promote: true);
	}

	public abstract TInterface GetService<TService, TInterface>() where TInterface : class;
	public abstract Task<TInterface> GetServiceAsync<TService, TInterface>() where TInterface : class;



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
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Implementation of the <see cref="IBAsyncPackage"/> ServicesCreatorCallback
	/// method.
	/// Initializes and configures a service of the specified type that is used by this
	/// Package.
	/// Configuration is performed by the class requiring the service.
	/// The actual instance creation of the service is the responsibility of the class
	/// Package that has access to the final descendent class of the Service.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public virtual Task<object> ServicesCreatorCallbackAsync(IAsyncServiceContainer container,
		CancellationToken token, Type serviceType) => Task.FromResult<object>(null);

	delegate void LoadSolutionOptionsDelegate(Stream stream);
	delegate void SaveSolutionOptionsDelegate(Stream stream);


	#endregion Method Implementations





	// =========================================================================================================
	#region Methods - AbstractCorePackage
	// =========================================================================================================


	protected abstract IBPackageController CreateController();


	/// <summary>
	/// ThrowOnFailure token
	/// </summary>
	protected static int ___(int hr) => ErrorHandler.ThrowOnFailure(hr);



	[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread")]
	protected static void DemandLoadPackage(string packageGuidString, out IVsPackage package, bool checkIfInstalled = false)
	{
		package = null;
		if (GetGlobalService(typeof(SVsShell)) is IVsShell vsShell)
		{
			Guid guidPackage = new Guid(packageGuidString);
			bool isInstalled = true;

			if (checkIfInstalled)
			{
				vsShell.IsPackageInstalled(ref guidPackage, out var pfInstalled);
				isInstalled = Convert.ToBoolean(pfInstalled);
			}

			if (isInstalled)
			{
				___(vsShell.LoadPackage(ref guidPackage, out package));
			}
		}
	}




	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Starts up extension user options push notifications. Only the final class in
	/// the <see cref="IBAsyncPackage"/> class hierarchy should implement the method.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected abstract void PropagateSettings();


	#endregion Methods


}
