
using System;
using System.Threading;
using System.Threading.Tasks;
using BlackbirdSql.Core.Ctl.Events;
using BlackbirdSql.Core.Ctl.Interfaces;
using EnvDTE;
using Microsoft.VisualStudio.Data.Services.SupportEntities;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using static BlackbirdSql.Core.Ctl.CommandProviders.CommandProperties;

namespace BlackbirdSql.Core.Ctl;

public abstract class AbstractAsyncPackage : AsyncPackage, IBAsyncPackage
{

	#region Variables - AbstractAsyncPackage


	protected static Package _Instance = null;
	protected IBPackageController _Controller;

	protected DTE _Dte = null;
	protected IVsSolution _DteSolution = null;
	protected IVsRunningDocumentTable _DocTable = null;

	protected System.Reflection.Assembly _InvariantAssembly = null;



	#endregion Variables





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


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Accessor to the <see cref="IVsRunningDocumentTable"/> instance
	/// </summary>
	public virtual IVsRunningDocumentTable DocTable
	{
		get
		{
#pragma warning disable VSTHRD010 // Invoke single-threaded types on Main thread - We're safe here
			_DocTable ??= GetDocTable();
#pragma warning restore VSTHRD010 // Invoke single-threaded types on Main thread

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
#pragma warning disable VSTHRD010 // Invoke single-threaded types on Main thread - We're safe here
			_Dte ??= GetDte();
#pragma warning restore VSTHRD010 // Invoke single-threaded types on Main thread

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
	public virtual IVsSolution DteSolution
	{
		get
		{
#pragma warning disable VSTHRD010 // Invoke single-threaded types on Main thread - We're safe here
			_DteSolution ??= GetSolution();
#pragma warning restore VSTHRD010 // Invoke single-threaded types on Main thread

			// If it's null there's an issue. Possibly we've come in too early
			if (_DteSolution == null)
			{
				NullReferenceException ex = new("SVsSolution is null");
				Diag.Dug(ex);
			}

			return _DteSolution;

		}
	}


	public abstract IBEventsManager EventsManager { get; }



	public Microsoft.VisualStudio.OLE.Interop.IServiceProvider OleServiceProvider
	{
		get
		{
			if (GetService(typeof(Microsoft.VisualStudio.OLE.Interop.IServiceProvider))
				is not Microsoft.VisualStudio.OLE.Interop.IServiceProvider provider)
			{
				InvalidOperationException ex = new("OLE.Interop.IServiceProvider service not found");
				Diag.Dug(ex);
				throw ex;
			}

			return provider;
		}
	}

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Accessor to the 'this' cast as the <see cref="IAsyncServiceContainer"/>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public virtual IAsyncServiceContainer Services => this;


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
			InvalidOperationException ex = new("Attempt to create duplicate Ddex extension package instances");
			Diag.Dug(ex);
			throw ex;
		}

		Diag.Context = "IDE";
		_Instance = this;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// AbstractAsyncPackage package destructor 
	/// </summary>
	/// <param name="disposing"></param>
	// ---------------------------------------------------------------------------------
	protected override void Dispose(bool disposing)
	{
		ThreadHelper.ThrowIfNotOnUIThread();

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
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
	public virtual async Task FinalizeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
	{
		// Sample format of FinalizeAsync in descendents

		// if (cancellationToken.IsCancellationRequested)
		//	return;

		// await base.FinalizeAsync(cancellationToken, progress);


		// Sample add service call

		// ServiceContainer.AddService(typeof(ICustomService), ServiceCreatorCallbackMethod, promote: true);
	}
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously





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
	/// Populates the GlobalEventArgs with all options available to the package. The
	/// final package class must implement this method.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public abstract GlobalEventArgs PopulateOptionsEventArgs();



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Populates the GlobalEventArgs with the options available to the package for the
	/// specified options group.
	/// The final package class must implement this method.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public abstract GlobalEventArgs PopulateOptionsEventArgs(string group);



	public abstract void RegisterOptionsEventHandlers(IBAsyncPackage.SettingsSavedDelegate onOptionsSettingSaved);



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



	#endregion Method Implementations





	// =========================================================================================================
	#region Methods - AbstractAsyncPackage
	// =========================================================================================================



	private IVsRunningDocumentTable GetDocTable()
	{
		ThreadHelper.ThrowIfNotOnUIThread();
		try
		{
			if (GetService(typeof(SVsRunningDocumentTable)) is not IVsRunningDocumentTable service)
				throw new InvalidOperationException("IBDesignerOnlineServices service not found");
			return service;
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			return null;
		}

	}

	private DTE GetDte()
	{
		ThreadHelper.ThrowIfNotOnUIThread();
		try
		{
			return GetService(typeof(DTE)) is not DTE service ? throw new InvalidOperationException("DTE service not found") : service;
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			return null;
		}
	}



	private IVsSolution GetSolution()
	{
		ThreadHelper.ThrowIfNotOnUIThread();
		try
		{
			if (GetService(typeof(SVsSolution)) is not IVsSolution service)
				throw new InvalidOperationException("IVsSolution service not found");
			return service;
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			return null;
		}

	}



	protected static T UiInvoke<T>(Func<T> function, int uiInvokeTimeoutSeconds = 5)
	{
		// If we’re already on the UI thread, just execute the method directly.
		if (ThreadHelper.CheckAccess())
		{
			return function();
		}

		T result = default;
		// Prefer BeginInvoke over Invoke since BeginInvoke is potentially saver than Invoke.
		using (ManualResetEventSlim eventHandle = new ManualResetEventSlim(false))
		{
#pragma warning disable VSTHRD001 // Avoid legacy thread switching APIs
			ThreadHelper.Generic.BeginInvoke(() => { result = function(); eventHandle.Set(); });
#pragma warning restore VSTHRD001 // Avoid legacy thread switching APIs
			// Wait for the invoke to complete.
			var success = eventHandle.Wait(TimeSpan.FromSeconds(uiInvokeTimeoutSeconds));
			// If the operation timed out, fail.
			if (!success)
			{
				TimeoutException ex = new();
				Diag.Dug(ex);
				throw ex;
			}

			return result;
		}

	}





	#endregion Methods

}
