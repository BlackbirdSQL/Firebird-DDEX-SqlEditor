
using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BlackbirdSql.Core.Ctl;
using BlackbirdSql.Core.Ctl.Config;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Core.Properties;
using BlackbirdSql.Data;
using BlackbirdSql.Sys;
using BlackbirdSql.Sys.Interfaces;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;



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
	protected AbstractCorePackage() : base()
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
	}



	/// <summary>
	/// AbstractCorePackage static .ctor
	/// </summary>
	static AbstractCorePackage()
	{
		RegisterAssemblies();
	}



	/// <summary>
	/// Gets the singleton Package instance
	/// </summary>
	public static AbstractCorePackage Instance
	{
		get
		{
			// if (_Instance == null)
			//	DemandLoadPackage(Sys.LibraryData.AsyncPackageGuid, out _);
			return (AbstractCorePackage)_Instance;
		}
	}


	public static void RegisterDataServices()
	{
		NativeDb.CsbType = typeof(Csb);
		NativeDb.DatabaseEngineSvc ??= DatabaseEngineService.EnsureInstance();
		NativeDb.ProviderSchemaFactorySvc ??= ProviderSchemaFactoryService.EnsureInstance();
		NativeDb.DatabaseInfoSvc ??= DatabaseInfoService.EnsureInstance();
		NativeDb.DbCommandSvc ??= DbCommandService.EnsureInstance();
		NativeDb.DbConnectionSvc ??= DbConnectionService.EnsureInstance();
		NativeDb.DbExceptionSvc ??= DbExceptionService.EnsureInstance();
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// AbstractCorePackage package destructor 
	/// </summary>
	/// <param name="disposing"></param>
	// ---------------------------------------------------------------------------------
	protected override void Dispose(bool disposing)
	{
		RctManager.Delete();

		base.Dispose(disposing);
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields and Constants - AbstractCorePackage
	// =========================================================================================================


	protected const int C_ProgressTotal = 50;

	protected static Package _Instance = null;
	// private int _InitializationSeed = 0;

	protected IBsPackageController _ApcInstance = null;
	private IDisposable _DisposableWaitCursor;
	private long _LoadStatisticsMainThreadStartTime = 0L;
	private long _LoadStatisticsMainThreadEndTime = 0L;
	private long _LoadStatisticsEndTime = 0L;
	private string _LoadStatisticsMsg = null;
	private bool _LoadStatisticsOutputFailed = false;
	protected static Stopwatch _S_Stopwatch = new();

	protected IVsSolution _VsSolution = null;

	protected IBsAsyncPackage.LoadSolutionOptionsDelegate _OnLoadSolutionOptionsEvent;
	protected IBsAsyncPackage.SaveSolutionOptionsDelegate _OnSaveSolutionOptionsEvent;

	protected enum _EnStatisticsStage
	{
		None,
		MainThreadLoadStart,
		MainThreadLoadEnd,
		InitializationEnd
	}
	#endregion Fields and Constants





	// =========================================================================================================
	#region Property accessors - AbstractCorePackage
	// =========================================================================================================


	// public static IBsNativeDatabaseEngine DatabaseEngineSvc => NativeDb.DatabaseEngineSvc ??= DatabaseEngineService.EnsureInstance();

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
				// If it's null there's an issue. Possibly we've come in too early

				if (GetService(typeof(SVsSolution)) is not IVsSolution service)
					Diag.ExceptionService(typeof(IVsSolution));
				else
					_VsSolution = service;
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


		ProgressAsync(progress, "Finalizing Core...").Forget();

		// Sample format of FinalizeAsync in descendents
		// if (cancellationToken.IsCancellationRequested)
		//	return;
		// await base.FinalizeAsync(cancellationToken, progress);

		// Sample add service call
		// ServiceContainer.AddService(typeof(ICustomService), ServiceCreatorCallbackMethod, promote: true);

		ProgressAsync(progress, "Finalizing Core... Done.").Forget();

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


		ProgressAsync(progress, "Initializing Core...").Forget();

		await base.InitializeAsync(cancellationToken, progress);

		ProgressAsync(progress, "Initializing Core... Done.").Forget();

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



	protected async Task ProgressAsync(IProgress<ServiceProgressData> progress, string message,
		_EnStatisticsStage stage = _EnStatisticsStage.None, long elapsed = 0L)
	{
		await Cmd.AwaitableAsync();

		switch (stage)
		{
			case _EnStatisticsStage.MainThreadLoadStart:
				_LoadStatisticsMainThreadStartTime = elapsed;
				break;
			case _EnStatisticsStage.MainThreadLoadEnd:
				_LoadStatisticsMainThreadEndTime = elapsed;
				break;
			case _EnStatisticsStage.InitializationEnd:
				_LoadStatisticsEndTime = elapsed;
				break;
			default:
				break;
		}

		// string thread = ThreadHelper.CheckAccess() ? "UI!!" : "Pool";
		// Diag.DebugTrace($"Loading BlackbirdSql [{thread}:{++_InitializationSeed}/{C_ProgressTotal}]: {message}");

		// ServiceProgressData progressData = new("Loading BlackbirdSql", message, _InitializationSeed++, C_ProgressTotal);
		// progress.Report(progressData);


		if (_LoadStatisticsMainThreadStartTime != 0L
			&& _LoadStatisticsMainThreadEndTime != 0L && _LoadStatisticsEndTime != 0L)
		{
			bool enableLoadStatistics = true;

#if !DEBUG
			enableLoadStatistics = PersistentSettings.EnableLoadStatistics;
#endif

			if (enableLoadStatistics)
			{
				long asyncInitTime = _LoadStatisticsMainThreadStartTime;
				long mainThreadSwitchTime = _LoadStatisticsMainThreadEndTime - _LoadStatisticsMainThreadStartTime;
				long mainThreadInitTime = _LoadStatisticsEndTime - _LoadStatisticsMainThreadEndTime;
				long totalTime = asyncInitTime + mainThreadInitTime;

				_LoadStatisticsMainThreadStartTime = 0L;
				_LoadStatisticsMainThreadEndTime = 0L;
				_LoadStatisticsEndTime = 0L;

				string outputMsg = Resources.LoadTimeStatistics.FmtRes(asyncInitTime.FmtStats(true), mainThreadInitTime.FmtStats(true),
					mainThreadSwitchTime.FmtStats(true), totalTime.FmtStats(true));

				if (_LoadStatisticsOutputFailed)
					Diag.OutputPaneWriteLineAsync(outputMsg, true).Forget();
				else
					_LoadStatisticsMsg = outputMsg;
			}
		}
	}


	public void OutputLoadStatistics()
	{
		if (_LoadStatisticsMsg == null)
		{
			_LoadStatisticsOutputFailed = true;
			return;
		}

		Diag.AsyuiOutputPaneWriteLine(_LoadStatisticsMsg, true);

		_LoadStatisticsMsg = null;
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
			if (args.Name == typeof(AbstractCorePackage).Assembly.FullName)
				return typeof(AbstractCorePackage).Assembly;

			if (args.Name == typeof(AbstrusePackageController).Assembly.FullName)
				return typeof(AbstrusePackageController).Assembly;

			if (args.Name == typeof(DatabaseEngineService).Assembly.FullName)
				return typeof(DatabaseEngineService).Assembly;

			return null;
		};
	}



	public virtual void SaveUserPreferences()
	{
	}

#endregion Methods


}
