// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.CodeDom;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using BlackbirdSql.Core.Ctl;
using BlackbirdSql.Core.Ctl.Extensions;
using BlackbirdSql.Core.Ctl.Interfaces;
using Microsoft.ServiceHub.Framework;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.RpcContracts.FileSystem;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TaskStatusCenter;

namespace BlackbirdSql.Core;


// =========================================================================================================
//										Controller Class
//
/// <summary>
/// Placeholder for AbstractPackageController static members for consistency.
/// </summary>
// =========================================================================================================
[SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread",
	Justification = "Only checking for null")]
internal static class Controller
{
	public static string ActiveWindowObjectKind
	{
		get
		{
			EnvDTE.Window window;

			// On shutdown an exception will be thrown here and the extension
			// will be in a shutdown state.
			try
			{
				window = Instance.Dte?.ActiveWindow;
			}
			catch (InvalidCastException ex)
			{
				if (ex.HResult != VSConstants.E_NOINTERFACE)
					throw;

				Instance.ShutdownDte();

				return null;
			}
			catch
			{
				window = null;
			}


			if (window == null)
				return null;

			return window.ObjectKind;
		}
	}

	public static string ActiveWindowObjectType
	{
		get
		{
			EnvDTE.Window window;

			// On shutdown an exception will be thrown here and the extension
			// will be in a shutdown state.
			try
			{
				window = Instance.Dte?.ActiveWindow;
			}
			catch (InvalidCastException ex)
			{
				if (ex.HResult != VSConstants.E_NOINTERFACE)
					throw;

				Instance.ShutdownDte();

				return null;
			}
			catch
			{
				window = null;
			}

			if (window == null)
				return null;

			object @object = window.Object;
			if (@object == null)
				return null;


			return @object.GetType().FullName;
		}
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the singleton AbstractPackageController instance
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static IBPackageController Instance => AbstractPackageController.Instance;

	public static bool CanValidateSolution => Instance.Dte.Solution != null
		&& Instance.Dte.Solution.Projects != null
		&& Instance.Dte.Solution.Projects.Count > 0;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Accessor to the singleton package instance as a service container.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static IAsyncServiceContainer Services => (IAsyncServiceContainer)Instance.DdexPackage;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the singleton DdexPackage instance
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static IBAsyncPackage DdexPackage => Instance.DdexPackage;

	public static IFileSystemProvider FileSystemBrokeredService => Instance.DdexPackage.FileSystemBrokeredService;

	public static Type SchemaFactoryType => Instance.DdexPackage.SchemaFactoryType;

	public static bool ShutdownState => Instance.ShutdownState;

	public static bool InvariantResolved => DdexPackage.InvariantResolved;

	public static System.IServiceProvider ServiceProvider => (System.IServiceProvider)Instance.DdexPackage;

	public static IVsDataExplorerConnectionManager ExplorerConnectionManager
	{
		get
		{
			IVsDataExplorerConnectionManager manager =
			(Controller.OleServiceProvider.QueryService<IVsDataExplorerConnectionManager>()
				as IVsDataExplorerConnectionManager)
			?? throw Diag.ExceptionService(typeof(IVsDataExplorerConnectionManager));

			return manager;
		}
	}

	public static ServiceRpcDescriptor FileSystemRpcDescriptor2 =>
		Instance.FileSystemRpcDescriptor2;

	public static IDisposable DisposableWaitCursor
	{
		get { return DdexPackage.DisposableWaitCursor; }
		set { DdexPackage.DisposableWaitCursor = value; }
	}

	public static Microsoft.VisualStudio.OLE.Interop.IServiceProvider OleServiceProvider
		=> Instance.DdexPackage.OleServiceProvider;

	public static IVsMonitorSelection SelectionMonitor => Instance.SelectionMonitor;

	public static bool SolutionValidating => Instance.SolutionValidating;

	public static TInterface EnsureService<TService, TInterface>() where TInterface : class
		=> Instance.EnsureService<TService, TInterface>();

	public static TInterface EnsureService<TInterface>() where TInterface : class
	{
		TInterface @interface = Instance.GetService<TInterface, TInterface>();
		Diag.ThrowIfServiceUnavailable(@interface, typeof(TInterface));

		return @interface;
	}

	public static async Task<TInterface> EnsureServiceAsync<TService, TInterface>() where TInterface : class
	{
		TInterface @interface = await Instance.GetServiceAsync<TService, TInterface>(); 
		Diag.ThrowIfServiceUnavailable(@interface, typeof(TInterface));

		return @interface;
	}


	public static TInterface GetService<TService, TInterface>() where TInterface : class
		=> Instance.GetService<TService, TInterface>();

	public static TInterface GetService<TInterface>() where TInterface : class
		=> Instance.GetService<TInterface, TInterface>();

	public static async Task<TInterface> GetServiceAsync<TService, TInterface>() where TInterface : class
		=> await Instance.GetServiceAsync<TService, TInterface>();

	public static async Task<IVsTaskStatusCenterService> GetStatusCenterServiceAsync()
		=> await Instance.GetStatusCenterServiceAsync();

	public static void ValidateSolution()
	{
		Instance.ValidateSolution();
	}

}