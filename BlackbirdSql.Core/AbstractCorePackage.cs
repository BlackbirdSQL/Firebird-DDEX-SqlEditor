
using System;
using System.ComponentModel.Design;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BlackbirdSql.Core.Ctl;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Core.Properties;
using BlackbirdSql.Data;
using BlackbirdSql.Sys.Interfaces;
using Microsoft.VisualStudio;
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
public abstract class AbstractCorePackage : AsyncPackage, IBsAsyncPackage
{

	// -----------------------------------------------------------------------------------------------------
	#region Constructors / Destructors - AbstractCorePackage
	// -----------------------------------------------------------------------------------------------------


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// AbstractCorePackage package .ctor
	/// </summary>
	// ---------------------------------------------------------------------------------
	public AbstractCorePackage() : base()
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
			UriParser.Register(new SqlStyleUriParser(SystemData.C_UriParserOptions), NativeDb.Protocol, 0);
		}
		catch (Exception ex)
		{
			Diag.ThrowException(ex);
		}

		_ApcInstance = CreateController();

		RctManager.CreateInstance();
	}



	/// <summary>
	/// Gets the singleton Package instance
	/// </summary>
	public static AbstractCorePackage Instance
	{
		get
		{
			if (_Instance == null)
				DemandLoadPackage(Sys.LibraryData.AsyncPackageGuid, out _);
			return (AbstractCorePackage)_Instance;
		}
	}


	static AbstractCorePackage()
	{
		RegisterDataServices();
	}

	private static void RegisterDataServices()
	{
		NativeDb.CsbType = typeof(Csb);
		NativeDb.DatabaseEngineSvc ??= DatabaseEngineService.CreateInstance();
		NativeDb.ProviderSchemaFactorySvc = ProviderSchemaFactoryService.CreateInstance();
		NativeDb.DatabaseInfoSvc = DatabaseInfoService.CreateInstance();
		NativeDb.DbCommandSvc = DbCommandService.CreateInstance();
		NativeDb.DbConnectionSvc = DbConnectionService.CreateInstance();
		NativeDb.DbExceptionSvc = DbExceptionService.CreateInstance();
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
	#region Fields and Constants - AbstractCorePackage
	// =========================================================================================================


	protected const int C_ProgressTotal = 43;

	protected static Package _Instance = null;
	protected int _InitializationSeed = 0;

	protected IBsPackageController _ApcInstance = null;
	private IDisposable _DisposableWaitCursor;
	protected IVsSolution _VsSolution = null;
	protected bool _Initialized = false;

	protected IBsAsyncPackage.LoadSolutionOptionsDelegate _OnLoadSolutionOptionsEvent;
	protected IBsAsyncPackage.SaveSolutionOptionsDelegate _OnSaveSolutionOptionsEvent;

	#endregion Fields and Constants





	// =========================================================================================================
	#region Property accessors - AbstractCorePackage
	// =========================================================================================================


	public static IBsNativeDatabaseEngine DatabaseEngineSvc => NativeDb.DatabaseEngineSvc ??= DatabaseEngineService.CreateInstance();

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Accessor to the <see cref="IBsPackageController"/> singleton instance
	/// </summary>
	// ---------------------------------------------------------------------------------
	public virtual IBsPackageController ApcInstance => _ApcInstance
		??= (IBsPackageController)GetGlobalService(typeof(IBsPackageController));


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


	public abstract IBsEventsManager EventsManager { get; }



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


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Accessor to the 'this' cast as the <see cref="IAsyncServiceContainer"/>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public virtual IAsyncServiceContainer ServiceContainer => this;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Accessor to the 'this' cast as the <see cref="IServiceContainer"/>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public virtual IServiceContainer SyncServiceContainer => this;


	/// <summary>
	/// Accessor to the <see cref="Package.OnLoadOptions"/> event.
	/// </summary>
	event IBsAsyncPackage.LoadSolutionOptionsDelegate IBsAsyncPackage.OnLoadSolutionOptionsEvent
	{
		add { _OnLoadSolutionOptionsEvent += value; }
		remove { _OnLoadSolutionOptionsEvent -= value; }
	}

	/// <summary>
	/// Accessor to the <see cref="Package.OnLoadOptions"/> event.
	/// </summary>
	event IBsAsyncPackage.SaveSolutionOptionsDelegate IBsAsyncPackage.OnSaveSolutionOptionsEvent
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
	public virtual async Task<object> CreateServiceInstanceAsync(Type serviceType, CancellationToken token)
	{
		if (serviceType == null)
		{
			ArgumentNullException ex = new("serviceType");
			Diag.Dug(ex);
			throw ex;
		}
		else if (serviceType == typeof(SBsNativeDatabaseEngine))
		{
			object service = NativeDb.DatabaseEngineSvc;
			return service;
		}
		else if (serviceType == typeof(SBsNativeDatabaseInfo))
		{
			object service = NativeDb.DatabaseInfoSvc;
			return service;
		}
		else if (serviceType == typeof(SBsNativeDbCommand))
		{
			object service = NativeDb.DbCommandSvc;
			return service;
		}
		else if (serviceType == typeof(SBsNativeDbConnection))
		{
			object service = NativeDb.DbConnectionSvc;
			return service;
		}
		else if (serviceType == typeof(SBsNativeDbException))
		{
			object service = NativeDb.DbExceptionSvc;
			return service;
		}
		/*
		else if (serviceType == typeof(IBDesignerOnlineServices))
		{
			object service = new DesignerOnlineServices()
				?? throw Diag.ExceptionService(serviceType);

			return service;
		}
		*/
		else if (serviceType.IsInstanceOfType(this))
		{
			return this;
		}

		return await Task.FromResult<object>(null); 
	}



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


		Progress(progress, "Finalizing Core initialization...");

		// Sample format of FinalizeAsync in descendents
		// if (cancellationToken.IsCancellationRequested)
		//	return;
		// await base.FinalizeAsync(cancellationToken, progress);

		// Sample add service call
		// ServiceContainer.AddService(typeof(ICustomService), ServiceCreatorCallbackMethod, promote: true);

		Progress(progress, "Finalizing Core initialization... Done.");

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


		Progress(progress, "Initializing Core...");

		await base.InitializeAsync(cancellationToken, progress);

		Progress(progress, "Initializing Core... Done.");

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Implementation of the <see cref="IBsAsyncPackage"/> ServicesCreatorCallback
	/// method.
	/// Initializes and configures a service of the specified type that is used by this
	/// Package.
	/// Configuration is performed by the class requiring the service.
	/// The actual instance creation of the service is the responsibility of the class
	/// Package that has access to the final descendent class of the Service.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public virtual object ServicesCreatorCallback(IServiceContainer container,
		Type serviceType) => null;



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Implementation of the <see cref="IBsAsyncPackage"/> ServicesCreatorCallbackAsync
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


	/// <summary>
	/// <see cref="ErrorHandler.ThrowOnFailure"/> token.
	/// </summary>
	protected static int ___(int hr) => ErrorHandler.ThrowOnFailure(hr);



	/// <summary>
	/// <see cref="ErrorHandler.Succeeded"/> token.
	/// </summary>
	protected static bool __(int hr) => ErrorHandler.Succeeded(hr);



	protected abstract IBsPackageController CreateController();



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



	protected void Progress(IProgress<ServiceProgressData> progress, string message)
	{
		ServiceProgressData progressData = new("Loading BlackbirdSql", message, _InitializationSeed++, C_ProgressTotal);
		progress.Report(progressData);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Starts up extension user options push notifications. Only the final class in
	/// the <see cref="IBsAsyncPackage"/> class hierarchy should implement the method.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected abstract void PropagateSettings();



	public virtual void SaveUserPreferences()
	{
	}

	#endregion Methods


}
