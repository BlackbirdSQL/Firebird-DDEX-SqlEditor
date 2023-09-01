// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)


using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Core.Providers;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

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

	public static IVsMonitorSelection SelectionMonitor => Instance.SelectionMonitor;


}