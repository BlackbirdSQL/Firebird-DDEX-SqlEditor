// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)


using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

using BlackbirdSql.Controller;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl.Events;
using BlackbirdSql.Core.Ctl.Extensions;
using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.VisualStudio.Ddex.Controls.Config;
using BlackbirdSql.VisualStudio.Ddex.Ctl;
using BlackbirdSql.VisualStudio.Ddex.Ctl.ComponentModel;
using BlackbirdSql.VisualStudio.Ddex.Ctl.Config;
using BlackbirdSql.VisualStudio.Ddex.Interfaces;
using BlackbirdSql.VisualStudio.Ddex.Properties;

using FirebirdSql.Data.FirebirdClient;

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

using Task = System.Threading.Tasks.Task;


namespace BlackbirdSql.VisualStudio.Ddex;

// =========================================================================================================
//										BlackbirdSqlDdexExtension Class 
//
/// <summary>
/// BlackbirdSql.Data.Ddex DDEX 2.0 <see cref="IVsPackage"/> class implementation
/// </summary>
/// <remarks>
/// Implements the package exposed by this assembly and registers itself with the shell.
/// </remarks>
// =========================================================================================================



// ---------------------------------------------------------------------------------------------------------
#region							BlackbirdSqlDdexExtension Class Attributes
// ---------------------------------------------------------------------------------------------------------

// Register this class as a VS package.
[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]

// A Visual Studio component can be registered under different regitry roots; for instance
// when you debug your package you want to register it in the experimental hive. This
// attribute specifies the registry root to use if no one is provided to regpkg.exe with
// the /root switch.
// [DefaultRegistryRoot("Software\\Microsoft\\VisualStudio\\15.0")]

// 'Help About' registration
[InstalledProductRegistration("#100", "#102", "1.0", IconResourceID = 400)]

// Valid load key for devices without the VS SDK installed. (Possibly deprecated)
// Load key request site (http://msdn.microsoft.com/vstudio/extend/) no longer exists.
// [ProvideLoadKey("Standard", "1.0", "DDEX Provider for BlackbirdClient", "..", 999)]

// We start loading as soon as the VS shell is available.
[ProvideAutoLoad(PackageData.ShellInitializedContextRuleGuid, PackageAutoLoadFlags.BackgroundLoad)]

// Not used
// [ProvideMenuResource(1000, 1)] TBC

// Register the DDEX as a data provider
[VsPackageRegistration]

// Register services
[ProvideService(typeof(IBPackageController), IsAsyncQueryable = true, ServiceName = PackageData.ControllerServiceName)]
[ProvideService(typeof(IBProviderObjectFactory), IsAsyncQueryable = true, ServiceName = PackageData.ObjectFactoryServiceName)]


// Implement Visual studio options/settings
[ProvideOptionPage(typeof(OptionsProvider.GeneralOptionPage), OptionsProvider.OptionPageCategory,
	OptionsProvider.GeneralOptionPageName, 0, 0, true, SupportsProfiles = true)]
#if DEBUG
[ProvideOptionPage(typeof(DebugOptionsDialogPage), OptionsProvider.OptionPageCategory,
	OptionsProvider.DebugOptionPageName, 0, 0, true, SupportsProfiles = true)]
#endif


#endregion Class Attributes





// =========================================================================================================
#region							BlackbirdSqlDdexExtension Class Declaration
// =========================================================================================================


public sealed class BlackbirdSqlDdexExtension : ControllerAsyncPackage
{



	#region Variables - BlackbirdSqlDdexExtension
	#endregion Variables





	// =========================================================================================================
	#region Property accessors - BlackbirdSqlDdexExtension
	// =========================================================================================================


	#endregion Property accessors





	// =========================================================================================================
	#region Constructors / Destructors - BlackbirdSqlDdexExtension
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// BlackbirdSqlDdexExtension package .ctor
	/// </summary>
	// ---------------------------------------------------------------------------------
	public BlackbirdSqlDdexExtension() : base()
	{
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// BlackbirdSqlDdexExtension package disposal 
	/// </summary>
	/// <param name="disposing"></param>
	// ---------------------------------------------------------------------------------
	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Method Implementations - BlackbirdSqlDdexExtension
	// =========================================================================================================



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Asynchronous initialization of the package
	/// </summary>
	/// <remarks>
	/// Auto load is time critical with this package.
	/// For example when an edmx is brought into context we can't have registering of the
	/// <see cref="DbProviderFactory"/> (<see cref="FirebirdClientFactory"/>) lost in a tertiary thread.
	/// We're going to try SwitchToMainThreadAsync here.
	/// If it still loads erratically we'll have to consider goin synchronous and commentting this whole class out.
	/// </remarks>
	// ---------------------------------------------------------------------------------
	protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
	{
		await base.InitializeAsync(cancellationToken, progress);

		// Moved to main thread
		// Services.AddService(typeof(IBProviderObjectFactory), ServicesCreatorCallbackAsync, promote: true);

		// Add FirebirdClient to assembly cache
		if (_InvariantAssembly == null && DbProviderFactoriesEx.AddAssemblyToCache(typeof(FirebirdClientFactory),
			Properties.Resources.Provider_ShortDisplayName, Properties.Resources.Provider_DisplayName))
		{
			_InvariantAssembly = typeof(FirebirdClientFactory).Assembly;

			AppDomain.CurrentDomain.AssemblyResolve += (_, args) =>
			{
				if (args.Name == _InvariantAssembly.FullName)
					return _InvariantAssembly;
				/*
				{
					// Diag.Trace("Firebird assembly name: " + _InvariantAssembly.FullName + " assembly resolved: " + args.Name);
					return _InvariantAssembly;
				}
				*/

				return null;
			};
		}


		// Perform any final initialization tasks.
		// It is the final descendent package class's responsibility to initiate the call to FinalizeAsync.
		await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

		await FinalizeAsync(cancellationToken, progress);


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

		// Add provider object factory
		Services.AddService(typeof(IBProviderObjectFactory), ServicesCreatorCallbackAsync, promote: true);


		_ = AdviseEventsAsync();

		await base.FinalizeAsync(cancellationToken, progress);

		// Descendents have completed their final async initialization now we perform ours.


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

		if (serviceType == typeof(IBProviderObjectFactory))
		{
			object service;
			try
			{
				service = new TProviderObjectFactory();
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw ex;
			}

			if (service == null)
			{
				ServiceUnavailableException ex = new(serviceType);
				Diag.Dug(ex);
				throw ex;
			}

			return service;
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
		if (serviceType == typeof(IBProviderObjectFactory))
			return await CreateServiceInstanceAsync(serviceType, token);


		return await base.ServicesCreatorCallbackAsync(container, token, serviceType);
	}


	#endregion Method Implementations





	// =========================================================================================================
	#region Methods - BlackbirdSqlDdexExtension
	// =========================================================================================================





	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Enables solution and running document table event handling
	/// </summary>
	// ---------------------------------------------------------------------------------
	private async Task AdviseEventsAsync()
	{
		await JoinableTaskFactory.SwitchToMainThreadAsync(DisposalToken);


		try
		{
			if (Dte == null)
				throw new NullReferenceException(Resources.ExceptionDteIsNull);

			// If it's null there's an issue. Possibly we've come in too early
			if (DteSolution == null)
				throw new NullReferenceException(Resources.ExceptionSVsSolutionIsNull);

			if (DocTable == null)
				throw new NullReferenceException(Resources.ExceptionIVsRunningDocumentTableIsNull);

			if (Controller == null)
				throw new NullReferenceException(Resources.ExceptionIBPackageControllerIsNull);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;
		}

		Controller.AdviseEvents();
	}



	public override void RegisterOptionsEventHandlers(IBAsyncPackage.SettingsSavedDelegate onSettingsSavedDelegate)
	{
		try
		{
			GeneralOptionModel.Saved += new Action<GeneralOptionModel>(onSettingsSavedDelegate);
			DebugOptionModel.Saved += new Action<DebugOptionModel>(onSettingsSavedDelegate);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex, Resources.ExceptionFailedToSubscribeOptionEvents);
		}
	}



	public override GlobalEventArgs PopulateOptionsEventArgs()
	{
		try
		{
			return new GlobalEventArgs(new KeyValuePair<string, object>[]
				{
					// General options
					GlobalEventArgs.ValuePair( "ShowDiagramPane", GeneralOptionModel.Instance.ShowDiagramPane ),
					GlobalEventArgs.ValuePair( "EnableDiagnostics", GeneralOptionModel.Instance.EnableDiagnostics ),
					GlobalEventArgs.ValuePair( "EnableTaskLog", GeneralOptionModel.Instance.EnableTaskLog ),
					GlobalEventArgs.ValuePair( "ValidateConfig", GeneralOptionModel.Instance.ValidateConfig ),
					GlobalEventArgs.ValuePair( "ValidateEdmx", GeneralOptionModel.Instance.ValidateEdmx ),
					// Debug options
					GlobalEventArgs.ValuePair( "PersistentValidation", DebugOptionModel.Instance.PersistentValidation ),
					GlobalEventArgs.ValuePair( "EnableTrace", DebugOptionModel.Instance.EnableTrace ),
					GlobalEventArgs.ValuePair( "EnableTracer", DebugOptionModel.Instance.EnableTracer ),
					GlobalEventArgs.ValuePair( "EnableDiagnosticsLog", DebugOptionModel.Instance.EnableDiagnosticsLog ),
					GlobalEventArgs.ValuePair( "LogFile", DebugOptionModel.Instance.LogFile ),
					GlobalEventArgs.ValuePair( "EnableFbDiagnostics", DebugOptionModel.Instance.EnableFbDiagnostics ),
					GlobalEventArgs.ValuePair( "FbLogFile", DebugOptionModel.Instance.FbLogFile )
				});
		}
		catch (Exception ex)
		{
			Diag.Dug(ex, Resources.ExceptionFailedToRetrievePackageSettings);
			throw ex;
		}


	}


	public override GlobalEventArgs PopulateOptionsEventArgs(string group)
	{
		if (group == "General")
		{
			return new GlobalEventArgs(group, new KeyValuePair<string, object>[]
				{
					// General options
					GlobalEventArgs.ValuePair( "ShowDiagramPane", GeneralOptionModel.Instance.ShowDiagramPane ),
					GlobalEventArgs.ValuePair( "EnableDiagnostics", GeneralOptionModel.Instance.EnableDiagnostics ),
					GlobalEventArgs.ValuePair( "EnableTaskLog", GeneralOptionModel.Instance.EnableTaskLog ),
					GlobalEventArgs.ValuePair( "ValidateConfig", GeneralOptionModel.Instance.ValidateConfig ),
					GlobalEventArgs.ValuePair( "ValidateEdmx", GeneralOptionModel.Instance.ValidateEdmx )
				});
		}
		else if (group == "Debug")
		{
			return new GlobalEventArgs(group, new KeyValuePair<string, object>[]
				{
					// Debug options
					GlobalEventArgs.ValuePair( "PersistentValidation", DebugOptionModel.Instance.PersistentValidation ),
					GlobalEventArgs.ValuePair( "EnableTrace", DebugOptionModel.Instance.EnableTrace ),
					GlobalEventArgs.ValuePair( "EnableTracer", DebugOptionModel.Instance.EnableTracer ),
					GlobalEventArgs.ValuePair( "EnableDiagnosticsLog", DebugOptionModel.Instance.EnableDiagnosticsLog ),
					GlobalEventArgs.ValuePair( "LogFile", DebugOptionModel.Instance.LogFile ),
					GlobalEventArgs.ValuePair( "EnableFbDiagnostics", DebugOptionModel.Instance.EnableFbDiagnostics ),
					GlobalEventArgs.ValuePair( "FbLogFile", DebugOptionModel.Instance.FbLogFile )
				});
		}

		return null;
	}


	#endregion Methods

}


#endregion Class Declaration
