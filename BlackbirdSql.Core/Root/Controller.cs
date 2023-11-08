// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

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
internal static class Controller
{

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the singleton AbstractPackageController instance
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static IBPackageController Instance => AbstractPackageController.Instance;


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


	public static IVsRunningDocumentTable DocTable => Instance.DocTable;

	public static Microsoft.VisualStudio.OLE.Interop.IServiceProvider OleServiceProvider
		=> Instance.DdexPackage.OleServiceProvider;

	public static IVsMonitorSelection SelectionMonitor => Instance.SelectionMonitor;

	public static IVsTaskStatusCenterService StatusCenterService => Instance.StatusCenterService;

	public static TInterface GetService<TService, TInterface>() where TInterface : class
		=> Instance.GetService<TService, TInterface>();

	public static async Task<TInterface> GetServiceAsync<TService, TInterface>() where TInterface : class
		=> await Instance.GetServiceAsync<TService, TInterface>();


}