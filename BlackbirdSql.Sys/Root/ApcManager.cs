// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using BlackbirdSql.Sys;
using BlackbirdSql.Sys.Interfaces;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TaskStatusCenter;

using static BlackbirdSql.Sys.Interfaces.IBsPackageController;



namespace BlackbirdSql;


// =========================================================================================================
//											ApcManager Class
//
/// <summary>
/// Provides application-wide static member access to the PackageController singleton instance.
/// </summary>
// =========================================================================================================
public static class ApcManager
{

	[SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification = "We're only peeking.")]
	public static string ActiveDocument
	{
		get
		{
			if (IdeShutdownState)
				return string.Empty;

			EnvDTE.Document document = null;

			try
			{
				document = Instance?.Dte?.ActiveDocument;
			}
			catch (InvalidCastException ex)
			{
				if (ex.HResult == VSConstants.E_NOINTERFACE)
				{
					ShutdownDte();
					return string.Empty;
				}
			}
			catch
			{
				return string.Empty;
			}

			return document == null ? string.Empty : document.FullName;
		}
	}



	[SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification = "We're only peeking.")]
	public static Project ActiveProject
	{
		get
		{
			if (IdeShutdownState)
				return null;

			Project result = null;

			try
			{
				object activeSolutionProjects = Dte.ActiveSolutionProjects;

				if (activeSolutionProjects != null && activeSolutionProjects is Array array && array.Length > 0)
				{
					object value = array.GetValue(0);

					if (value != null && value is Project project)
					{
						result = project;
					}
				}
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				return null;
			}

			return result;
		}
	}



	public static EnvDTE.Window ActiveWindow
	{
		get
		{
			if (IdeShutdownState)
				return null;

			EnvDTE.Window window = null;

			try
			{
				window = Instance?.Dte?.ActiveWindow;
			}
			catch (InvalidCastException ex)
			{
				if (ex.HResult == VSConstants.E_NOINTERFACE)
				{
					ShutdownDte();
					return null;
				}
			}
			catch
			{
				window = null;
			}

			return window;
		}
	}


	[SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification = "We're only peeking.")]
	public static string ActiveWindowObjectKind
	{
		get
		{
			try
			{
				EnvDTE.Window window = ActiveWindow;
				if (window == null)
					return string.Empty;

				string kind = window.ObjectKind;
				if (kind == null)
					return string.Empty;

				return kind;
			}
			catch
			{
				return string.Empty;
			}
		}
	}



	[SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification = "We're only peeking.")]
	public static string ActiveWindowObjectType
	{
		get
		{
			try
			{
				EnvDTE.Window window = ActiveWindow;
				if (window == null)
					return string.Empty;

				if (window.Object == null)
					return string.Empty;

				return window.Object.GetType().FullName;
			}
			catch
			{
				return string.Empty;
			}
		}
	}


	public static string ActiveWindowType => ActiveWindow?.GetType().FullName ?? string.Empty;


	[SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification = "We're only peeking.")]
	public static bool CanValidateSolution => SolutionProjects != null && SolutionProjects.Count > 0;


	public static DTE Dte => IdeShutdownState ? null : Instance.Dte;

	public static bool IdeShutdownState => AbstrusePackageController.IdeShutdownState;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the singleton AbstractPackageController instance
	/// </summary>
	// ---------------------------------------------------------------------------------
	private static IBsPackageController Instance => AbstrusePackageController.Instance;


	public static bool IsToolboxInitialized => !IdeShutdownState && Instance.IsToolboxInitialized;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the singleton PackageInstance instance
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static IBsAsyncPackage PackageInstance => Instance?.PackageInstance;


	public static System.IServiceProvider ServiceProvider => (System.IServiceProvider)Instance?.PackageInstance;

	public static Solution SolutionObject => IdeShutdownState ? null : Instance.SolutionObject;

	public static Projects SolutionProjects => SolutionObject != null ? Instance.SolutionProjects : null;

	public static IVsTaskStatusCenterService StatusCenterService => Instance?.StatusCenterService;

	public static IVsDataExplorerConnectionManager ExplorerConnectionManager
	{
		get
		{
			IVsDataExplorerConnectionManager manager =
				(OleServiceProvider?.QueryService<IVsDataExplorerConnectionManager>()
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
		=> Instance?.PackageInstance?.OleServiceProvider;

	public static string ProviderGuid => Instance?.ProviderGuid;

	public static IVsMonitorSelection SelectionMonitor => Instance?.SelectionMonitor;

	public static bool SolutionClosing => IdeShutdownState || Instance.SolutionClosing;

	public static bool SolutionValidating => Instance != null && Instance.SolutionValidating;


	public static string CreateConnectionUrl(string connectionString)
		=> Instance?.CreateConnectionUrl(connectionString);

	public static TInterface EnsureService<TService, TInterface>() where TInterface : class
		=> Instance?.EnsureService<TService, TInterface>();

	public static TInterface EnsureService<TInterface>() where TInterface : class
	{
		TInterface @interface = Instance?.GetService<TInterface, TInterface>();
		Diag.ThrowIfServiceUnavailable(@interface, typeof(TInterface));

		return @interface;
	}


	public static async Task<TInterface> EnsureServiceAsync<TService, TInterface>() where TInterface : class
	{
		TInterface @interface = await Instance?.GetServiceAsync<TService, TInterface>(); 
		Diag.ThrowIfServiceUnavailable(@interface, typeof(TInterface));

		return @interface;
	}




	public static TInterface GetService<TService, TInterface>() where TInterface : class
		=> Instance?.GetService<TService, TInterface>();

	public static TInterface GetService<TInterface>() where TInterface : class
		=> Instance?.GetService<TInterface, TInterface>();

	public static async Task<TInterface> GetServiceAsync<TService, TInterface>() where TInterface : class
		=> await Instance?.GetServiceAsync<TService, TInterface>();

	public static string GetRegisterConnectionDatasetKey(IVsDataExplorerConnection root)
		=> Instance?.GetRegisterConnectionDatasetKey(root);

	public static void InvalidateRctManager() => Instance?.InvalidateRctManager();

	public static bool IsConnectionEquivalency(string connectionString1, string connectionString2)
			=> Instance != null && Instance.IsConnectionEquivalency(connectionString1, connectionString2);

	public static bool IsWeakConnectionEquivalency(string connectionString1, string connectionString2)
		=> Instance != null && Instance.IsWeakConnectionEquivalency(connectionString1, connectionString2);



	public static void ShutdownDte()
	{
		// Tracer.Trace(typeof(AbstrusePackageController), "ShutdownDte()");

		AbstrusePackageController.ShutdownDte();
	}



	public static void ValidateSolution() => Instance?.ValidateSolution();

	public static event ElementValueChangedDelegate OnElementValueChangedEvent
	{
		add { if (Instance != null) Instance.OnElementValueChangedEvent += value; }
		remove { if (Instance != null) Instance.OnElementValueChangedEvent -= value; }
	}

}