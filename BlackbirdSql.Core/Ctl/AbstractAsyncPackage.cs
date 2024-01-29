using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Core.Properties;
using EnvDTE;
using FirebirdSql.Data.FirebirdClient;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;


namespace BlackbirdSql.Core.Ctl;

public abstract class AbstractAsyncPackage : AsyncPackage, IBAsyncPackage
{

	#region Fields - AbstractAsyncPackage



	protected static Package _Instance = null;
	protected IBPackageController _Controller = null;
	private IDisposable _DisposableWaitCursor;
	protected DTE _Dte = null;
	protected IVsSolution _VsSolution = null;
	protected IVsRunningDocumentTable _DocTable = null;
	protected bool _Initialized = false;
	protected static bool _InvariantResolved = false;

	protected IBAsyncPackage.LoadSolutionOptionsDelegate _OnLoadSolutionOptionsEvent;
	protected IBAsyncPackage.SaveSolutionOptionsDelegate _OnSaveSolutionOptionsEvent;

	#endregion Fields





	// =========================================================================================================
	#region Property accessors - AbstractAsyncPackage
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Accessor to the <see cref="IBPackageController"/> singleton instance
	/// </summary>
	// ---------------------------------------------------------------------------------
	public virtual IBPackageController Controller => _Controller
		??= (IBPackageController)GetGlobalService(typeof(IBPackageController));


	public IDisposable DisposableWaitCursor
	{
		get { return _DisposableWaitCursor; }
		set { _DisposableWaitCursor = value; }
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Accessor to the <see cref="IVsRunningDocumentTable"/> instance
	/// </summary>
	public virtual IVsRunningDocumentTable DocTable
	{
		get
		{
			_DocTable ??= GetDocTable();

			// If it's null there's an issue. Possibly we've come in too early
			if (_DocTable == null)
			{
				NullReferenceException ex = new("SVsRunningDocumentTable is null");
				Diag.Dug(ex);
			}

			return _DocTable;

		}
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Accessor to the <see cref="DTE"/> instance.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public virtual DTE Dte
	{
		get
		{
			_Dte ??= GetDte();

			// If it's null there's an issue. Possibly we've come in too early
			if (_Dte == null)
			{
				NullReferenceException ex = new("DTE is null");
				Diag.Dug(ex);
			}

			return _Dte;
		}
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

	public abstract bool InvariantResolved { get; }


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
	#region Constructors / Destructors - AbstractAsyncPackage
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// AbstractAsyncPackage package .ctor
	/// </summary>
	// ---------------------------------------------------------------------------------
	public AbstractAsyncPackage()
	{
		if (_Instance != null)
		{
			TypeAccessException ex = new(Resources.ExceptionDuplicateSingletonInstances.FmtRes("Ddex extension package instances"));
			Diag.Dug(ex);
			throw ex;
		}

		Diag.Context = "IDE";
		_Instance = this;

		_Controller = CreateController();

		RctManager.CreateInstance();
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// AbstractAsyncPackage package destructor 
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
	#region Method Implementations - AbstractAsyncPackage
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
		if (cancellationToken.IsCancellationRequested)
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
	#region Methods - AbstractAsyncPackage
	// =========================================================================================================


	protected abstract IBPackageController CreateController();


	private IVsRunningDocumentTable GetDocTable()
	{
		if (GetService(typeof(SVsRunningDocumentTable)) is not IVsRunningDocumentTable service)
		{
			Diag.ExceptionService(typeof(IVsRunningDocumentTable));
			return null;
		}
		return service;
	}

	private DTE GetDte()
	{
		try
		{
			return GetService(typeof(DTE)) is not DTE service ? throw new ServiceUnavailableException(typeof(DTE)) : service;
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			return null;
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
