// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using BlackbirdSql.Sys;
using BlackbirdSql.Sys.Interfaces;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TaskStatusCenter;

using static BlackbirdSql.Sys.Interfaces.IBsPackageController;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;



namespace BlackbirdSql;


// =========================================================================================================
//											ApcManager Class
//
/// <summary>
/// Provides application-wide static member access to the PackageController singleton instance.
/// </summary>
// =========================================================================================================
internal static class ApcManager
{

	[SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification = "We're only peeking.")]
	internal static string ActiveDocumentExtension
	{
		get
		{
			if (IdeShutdownState || SolutionClosing)
				return "";

			EnvDTE.Document document;

			try
			{
				document = Instance?.Dte?.ActiveDocument;
			}
			catch (InvalidCastException ex)
			{
				if (ex.HResult == VSConstants.E_NOINTERFACE)
					ShutdownDte();

				return "";
			}
			catch
			{
				return "";
			}

			if (document == null)
				return "";

			try
			{
				return Cmd.GetExtension(document.FullName);
			}
			catch
			{
				return "";
			}
		}
	}



	[SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification = "We're only peeking.")]
	internal static Project ActiveProject
	{
		get
		{
			if (IdeShutdownState || SolutionClosing)
				return null;

			DTE2 dte2 = Dte as DTE2;

			try
			{

				object activeSolutionProjects = dte2?.ActiveSolutionProjects;

				if (activeSolutionProjects != null && activeSolutionProjects is Array array && array.Length > 0)
				{
					object value = array.GetValue(0);

					if (value != null && value is Project project && !project.IsFolder())
					{
						return project;
					}
				}
			}
			catch (COMException)
			{
			}
			catch (Exception ex)
			{
				Diag.Ex(ex);
				return null;
			}

			if (dte2.ActiveDocument != null && dte2.ActiveDocument.ProjectItem != null
				&& dte2.ActiveDocument.ProjectItem.ContainingProject != null)
			{
				return dte2.ActiveDocument.ProjectItem.ContainingProject;
			}

			if (dte2.Solution.Projects.Count == 0)
				return null;

			Project	result = dte2.Solution.Projects.Item(1);

			if (result.IsFolder())
				return null;

			return result;
		}
	}



	internal static EnvDTE.Window ActiveWindow
	{
		get
		{
			if (IdeShutdownState || SolutionClosing)
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
	internal static IntPtr ActiveWindowHandle
	{
		get
		{
			try
			{
				EnvDTE.Window window = ActiveWindow;
				if (window == null)
					return IntPtr.Zero;

				return window.HWnd;
			}
			catch
			{
				return IntPtr.Zero;
			}
		}
	}


	[SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification = "We're only peeking.")]
	internal static string ActiveWindowObjectKind
	{
		get
		{
			try
			{
				EnvDTE.Window window = ActiveWindow;
				if (window == null)
					return "";

				string kind = window.ObjectKind;
				if (kind == null)
					return "";

				return kind.ToUpper();
			}
			catch
			{
				return "";
			}
		}
	}



	[SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification = "We're only peeking.")]
	internal static string ActiveWindowObjectType
	{
		get
		{
			try
			{
				EnvDTE.Window window = ActiveWindow;
				if (window == null)
					return "";

				if (window.Object == null)
					return "";

				return window.Object.GetType().FullName;
			}
			catch
			{
				return "";
			}
		}
	}


	internal static string ActiveWindowType => ActiveWindow?.GetType().FullName ?? "";


	[SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification = "We're only peeking.")]
	internal static bool CanValidateSolution => SolutionProjects != null && SolutionProjects.Count > 0;


	internal static DTE Dte => IdeShutdownState ? null : Instance.Dte;

	internal static bool IdeShutdownState => AbstrusePackageController.IdeShutdownState;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the singleton AbstractPackageController instance
	/// </summary>
	// ---------------------------------------------------------------------------------
	private static IBsPackageController Instance => AbstrusePackageController.Instance;


	internal static bool IsToolboxInitialized => !IdeShutdownState && Instance.IsToolboxInitialized;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the singleton PackageInstance instance
	/// </summary>
	// ---------------------------------------------------------------------------------
	internal static IBsAsyncPackage PackageInstance => Instance?.PackageInstance;


	internal static IServiceProvider ServiceProvider => (IServiceProvider)Instance?.PackageInstance;

	internal static Solution SolutionObject => IdeShutdownState ? null : Instance.SolutionObject;

	internal static Projects SolutionProjects => SolutionObject != null ? Instance.SolutionProjects : null;

	internal static IVsTaskStatusCenterService StatusCenterService => Instance?.StatusCenterService;

	internal static IVsDataExplorerConnectionManager ExplorerConnectionManager =>
		(OleServiceProvider?.QueryService<IVsDataExplorerConnectionManager>()
			as IVsDataExplorerConnectionManager)
		?? throw Diag.ExceptionService(typeof(IVsDataExplorerConnectionManager));

	internal static IDisposable DisposableWaitCursor
	{
		get { return PackageInstance.DisposableWaitCursor; }
		set { PackageInstance.DisposableWaitCursor = value; }
	}

	internal static IOleServiceProvider OleServiceProvider =>
		Instance?.PackageInstance?.OleServiceProvider;

	internal static string ProviderGuid => Instance?.ProviderGuid;

	internal static IVsMonitorSelection SelectionMonitor => Instance?.SelectionMonitor;

	/// <summary>
	/// Only functions if on main thread. Always returns false IfNotOnUIThread.
	/// </summary>
	internal static bool SolutionClosing => IdeShutdownState || Instance.SolutionClosing;

	internal static bool SolutionValidating => Instance != null && Instance.SolutionValidating;


	internal static string CreateConnectionUrl(string connectionString)
		=> Instance?.CreateConnectionUrl(connectionString);

	internal static string GetConnectionQualifiedName(string connectionString)
		=> Instance?.GetConnectionQualifiedName(connectionString);

	internal static TInterface EnsureService<TService, TInterface>() where TInterface : class
		=> Instance?.EnsureService<TService, TInterface>();

	internal static TInterface EnsureService<TInterface>() where TInterface : class
	{
		TInterface @interface = Instance?.GetService<TInterface, TInterface>();
		Diag.ThrowIfServiceUnavailable(@interface, typeof(TInterface));

		return @interface;
	}


	internal static async Task<TInterface> EnsureServiceAsync<TService, TInterface>() where TInterface : class =>
		await Instance?.GetServiceAsync<TService, TInterface>();


	internal static TInterface GetService<TService, TInterface>() where TInterface : class
		=> Instance?.GetService<TService, TInterface>();

	internal static TInterface GetService<TInterface>() where TInterface : class
		=> Instance?.GetService<TInterface, TInterface>();

	internal static async Task<TInterface> GetServiceAsync<TService, TInterface>() where TInterface : class
		=> await Instance?.GetServiceAsync<TService, TInterface>();

	internal static string GetRegisterConnectionDatasetKey(IVsDataExplorerConnection root)
		=> Instance?.GetRegisterConnectionDatasetKey(root);

	internal static void InitializeSettings() => Instance.InitializeSettings();

	internal static void InvalidateRctManager() => Instance?.InvalidateRctManager();

	internal static bool IsConnectionEquivalency(string connectionString1, string connectionString2)
			=> Instance != null && Instance.IsConnectionEquivalency(connectionString1, connectionString2);

	internal static bool IsWeakConnectionEquivalency(string connectionString1, string connectionString2)
		=> Instance != null && Instance.IsWeakConnectionEquivalency(connectionString1, connectionString2);



	internal static void ShutdownDte()
	{
		// Evs.Trace(typeof(AbstrusePackageController), nameof(ShutdownDte));

		AbstrusePackageController.ShutdownDte();
	}



	internal static void ValidateSolution() => Instance?.ValidateSolution();

	internal static event ElementValueChangedDelegate OnElementValueChangedEvent
	{
		add { if (Instance != null) Instance.OnElementValueChangedEvent += value; }
		remove { if (Instance != null) Instance.OnElementValueChangedEvent -= value; }
	}

}