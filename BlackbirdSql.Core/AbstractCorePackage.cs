
using System;
using System.ComponentModel.Design;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BlackbirdSql.Core.Ctl;
using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Core.Properties;
using BlackbirdSql.Data;
using BlackbirdSql.Sys;
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
			UriParser.Register(new SqlStyleUriParser(SystemData.UriParserOptions), DbNative.Protocol, 0);
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
				DemandLoadPackage(SystemData.AsyncPackageGuid, out _);
			return (AbstractCorePackage)_Instance;
		}
	}


	static AbstractCorePackage()
	{
		RegisterDataServices();
	}

	private static void RegisterDataServices()
	{
		DbNative.CsbType = typeof(Csb);
		DbNative.DatabaseEngineSvc ??= DatabaseEngineService.CreateInstance();
		DbNative.DatabaseInfoSvc = DatabaseInfoService.CreateInstance();
		DbNative.DbCommandSvc = DbCommandService.CreateInstance();
		DbNative.DbConnectionSvc = DbConnectionService.CreateInstance();
		DbNative.DbExceptionSvc = DbExceptionService.CreateInstance();
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

	protected IBPackageController _ApcInstance = null;
	private IDisposable _DisposableWaitCursor;
	protected IVsSolution _VsSolution = null;
	protected bool _Initialized = false;

	protected IBAsyncPackage.LoadSolutionOptionsDelegate _OnLoadSolutionOptionsEvent;
	protected IBAsyncPackage.SaveSolutionOptionsDelegate _OnSaveSolutionOptionsEvent;

	#endregion Fields and Constants





	// =========================================================================================================
	#region Property accessors - AbstractCorePackage
	// =========================================================================================================


	public static IBsNativeDatabaseEngine DatabaseEngineSvc => DbNative.DatabaseEngineSvc ??= DatabaseEngineService.CreateInstance();

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
			object service = DbNative.DatabaseEngineSvc;
			return service;
		}
		else if (serviceType == typeof(SBsNativeDatabaseInfo))
		{
			object service = DbNative.DatabaseInfoSvc;
			return service;
		}
		else if (serviceType == typeof(SBsNativeDbCommand))
		{
			object service = DbNative.DbCommandSvc;
			return service;
		}
		else if (serviceType == typeof(SBsNativeDbConnection))
		{
			object service = DbNative.DbConnectionSvc;
			return service;
		}
		else if (serviceType == typeof(SBsNativeDbException))
		{
			object service = DbNative.DbExceptionSvc;
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
	/// Implementation of the <see cref="IBAsyncPackage"/> ServicesCreatorCallback
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
	/// Implementation of the <see cref="IBAsyncPackage"/> ServicesCreatorCallbackAsync
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



	protected abstract IBPackageController CreateController();



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



	protected void Progress(IProgress<ServiceProgressData> progress, string message)
	{
		ServiceProgressData progressData = new("Loading BlackbirdSql", message, _InitializationSeed++, C_ProgressTotal);
		progress.Report(progressData);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Starts up extension user options push notifications. Only the final class in
	/// the <see cref="IBAsyncPackage"/> class hierarchy should implement the method.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected abstract void PropagateSettings();



	public virtual void SaveUserPreferences()
	{
	}

	#endregion Methods


}
