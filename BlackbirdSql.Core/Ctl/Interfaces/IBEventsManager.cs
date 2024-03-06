// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using Microsoft.VisualStudio.Shell.Interop;


namespace BlackbirdSql.Core.Ctl.Interfaces;

// =========================================================================================================
//										IBEventsManager Interface
/// <summary>
/// Interface for Extension or Service EventsManagers derived from AbstractEventsManager.
/// </summary>
/// <remarks>
/// Each service or extension that handles ide events should derive an events manager from
/// AbstractEventsManager. All solution, rdt and selection events are routed through PackageController.
/// The events manager can handle an ide event by hooking onto Controller.On[event]. 
/// </remarks>
// =========================================================================================================
public interface IBEventsManager : IBTaskHandlerClient, IDisposable
{
	IBPackageController Controller { get; }
	IBAsyncPackage DdexPackage { get; }
	IVsMonitorSelection SelectionMonitor { get; }


	/// <summary>
	/// Initializes the event manager.
	/// </summary>
	/// <remarks>
	/// Example: <code>_Controller.OnExampleEvent += OnExample;</code>
	/// </remarks>
	void Initialize();
}