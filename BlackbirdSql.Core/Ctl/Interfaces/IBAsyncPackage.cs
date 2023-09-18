// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Threading;

using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using BlackbirdSql.Core.Ctl.Events;
using Microsoft.VisualStudio.Data.Services.SupportEntities;
using static BlackbirdSql.Core.Ctl.CommandProviders.CommandProperties;

namespace BlackbirdSql.Core.Ctl.Interfaces;

[Guid(SystemData.PackageGuid)]

public interface IBAsyncPackage
{

	delegate void SettingsSavedDelegate(object sender);


	IBPackageController Controller { get; }

	IVsRunningDocumentTable DocTable { get; }

	DTE Dte { get; }

	IVsSolution DteSolution { get; }


	IBEventsManager EventsManager { get; }

	Microsoft.VisualStudio.OLE.Interop.IServiceProvider OleServiceProvider { get; }

	IAsyncServiceContainer Services { get; }




	Task<object> CreateServiceInstanceAsync(Type serviceType, CancellationToken token);

	Task FinalizeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress);

	GlobalEventArgs PopulateOptionsEventArgs();

	GlobalEventArgs PopulateOptionsEventArgs(string group);


	void RegisterOptionsEventHandlers(SettingsSavedDelegate onSettingSaved);

	Task<object> ServicesCreatorCallbackAsync(IAsyncServiceContainer container, CancellationToken token, Type serviceType);

}