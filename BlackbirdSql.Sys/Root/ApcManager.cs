// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using BlackbirdSql.Sys;
using BlackbirdSql.Sys.Interfaces;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TaskStatusCenter;



namespace BlackbirdSql;

[SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification = "<Pending>")]



// =========================================================================================================
//											ApcManager Class
//
/// <summary>
/// Provides application-wide static member access to the PackageController singleton instance.
/// </summary>
// =========================================================================================================
public static class ApcManager
{
	public static string ActiveWindowObjectKind
	{
		get
		{
			/*
			 * Deprecated. We're just peeking.
			 * 
			// Fire and wait.
			if (!ThreadHelper.CheckAccess())
			{
				string result = ThreadHelper.JoinableTaskFactory.Run(async delegate
				{
					await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
					return GetActiveWindowObjectKind();
				});

				return result;
			}

			return GetActiveWindowObjectKind();
			*/

			// We're just peeking at stored values.
			// Diag.ThrowIfNotOnUIThread();

			if (IdeShutdownState)
				return null;

			EnvDTE.Window window = null;

			try
			{
				window = Instance.Dte?.ActiveWindow;
			}
			catch (InvalidCastException ex)
			{
				if (ex.HResult == VSConstants.E_NOINTERFACE)
				{
					ResetDte();
					return null;
				}
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
			/*
			 * Deprecated. We're just peeking.
			 * 
			// Fire and wait.
			if (!ThreadHelper.CheckAccess())
			{
				string result = ThreadHelper.JoinableTaskFactory.Run(async delegate
				{
					await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
					return GetActiveWindowObjectType();
				});

				return result;
			}
			*/

			// We're just peeking at stored values.
			// Diag.ThrowIfNotOnUIThread();

			if (IdeShutdownState)
				return null;

			EnvDTE.Window window = null;

			try
			{
				window = Instance.Dte?.ActiveWindow;
			}
			catch (InvalidCastException ex)
			{
				if (ex.HResult == VSConstants.E_NOINTERFACE)
				{
					ResetDte();
					return null;
				}
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
	public static IBsPackageController Instance => AbstrusePackageController.Instance;

	public static bool CanValidateSolution => Instance.Dte.Solution != null
		&& Instance.Dte.Solution.Projects != null
		&& Instance.Dte.Solution.Projects.Count > 0;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Accessor to the singleton package instance as a service container.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static IAsyncServiceContainer Services => (IAsyncServiceContainer)Instance.PackageInstance;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the singleton PackageInstance instance
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static IBsAsyncPackage PackageInstance => Instance.PackageInstance;

	public static bool IdeShutdownState => AbstrusePackageController.IdeShutdownState;

	public static System.IServiceProvider ServiceProvider => (System.IServiceProvider)Instance.PackageInstance;

	public static IVsTaskStatusCenterService StatusCenterService => Instance.StatusCenterService;

	public static IVsDataExplorerConnectionManager ExplorerConnectionManager
	{
		get
		{
			IVsDataExplorerConnectionManager manager =
			(ApcManager.OleServiceProvider.QueryService<IVsDataExplorerConnectionManager>()
				as IVsDataExplorerConnectionManager)
			?? throw Diag.ExceptionService(typeof(IVsDataExplorerConnectionManager));

			return manager;
		}
	}

	public static IDisposable DisposableWaitCursor
	{
		get { return PackageInstance.DisposableWaitCursor; }
		set { PackageInstance.DisposableWaitCursor = value; }
	}

	public static Microsoft.VisualStudio.OLE.Interop.IServiceProvider OleServiceProvider
		=> Instance.PackageInstance.OleServiceProvider;

	public static IVsMonitorSelection SelectionMonitor => Instance.SelectionMonitor;

	public static bool SolutionValidating => Instance.SolutionValidating;


	public static string CreateConnectionUrl(IDbConnection connection)
		=> Instance.CreateConnectionUrl(connection);

	public static string CreateConnectionUrl(string connectionString)
		=> Instance.CreateConnectionUrl(connectionString);

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

	public static string GetRegisterConnectionDatasetKey(IDbConnection connection)
		=> Instance.GetRegisterConnectionDatasetKey(connection);

	public static void InvalidateRctManager() => Instance.InvalidateRctManager();

	public static bool IsConnectionEquivalency(string connectionString1, string connectionString2)
			=> Instance.IsConnectionEquivalency(connectionString1, connectionString2);

	public static bool IsWeakConnectionEquivalency(string connectionString1, string connectionString2)
		=> Instance.IsWeakConnectionEquivalency(connectionString1, connectionString2);

	public static void ResetDte() => AbstrusePackageController.ResetDte();


	public static void ValidateSolution() => Instance.ValidateSolution();

}