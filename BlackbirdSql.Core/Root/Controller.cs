// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using BlackbirdSql.Core.Ctl;
using BlackbirdSql.Core.Ctl.Interfaces;

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

	public static Type SchemaFactoryType => Instance.DdexPackage.SchemaFactoryType;
	public static bool InvariantResolved => DdexPackage.InvariantResolved;

	public static System.IServiceProvider ServiceProvider => (System.IServiceProvider)Instance.DdexPackage;

	public static IDisposable DisposableWaitCursor
	{
		get { return DdexPackage.DisposableWaitCursor; }
		set { DdexPackage.DisposableWaitCursor = value; }
	}

	public static IVsRunningDocumentTable DocTable => Instance.DocTable;

	public static Microsoft.VisualStudio.OLE.Interop.IServiceProvider OleServiceProvider
		=> Instance.DdexPackage.OleServiceProvider;

	public static IVsMonitorSelection SelectionMonitor => Instance.SelectionMonitor;

	public static bool SolutionValidating => Instance.SolutionValidating;

	public static IVsTaskStatusCenterService StatusCenterService => Instance.StatusCenterService;

	public static TInterface GetService<TService, TInterface>() where TInterface : class
		=> Instance.GetService<TService, TInterface>();

	public static TInterface GetService<TInterface>() where TInterface : class
		=> Instance.GetService<TInterface, TInterface>();

	public static async Task<TInterface> GetServiceAsync<TService, TInterface>() where TInterface : class
		=> await Instance.GetServiceAsync<TService, TInterface>();

	public static void ValidateSolution()
	{
		Instance.ValidateSolution();
	}

}