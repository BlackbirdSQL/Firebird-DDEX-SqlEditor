// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Threading;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Data.Services;

namespace BlackbirdSql.Core.Ctl.Interfaces;

[Guid(SystemData.PackageGuid)]

public interface IBAsyncPackage
{

	IBPackageController Controller { get; }

	IDisposable DisposableWaitCursor { get; set; }

	IVsRunningDocumentTable DocTable { get; }

	DTE Dte { get; }

	IVsSolution DteSolution { get; }


	IBEventsManager EventsManager { get; }

	Microsoft.VisualStudio.OLE.Interop.IServiceProvider OleServiceProvider { get; }

	IAsyncServiceContainer ServiceContainer { get; }



	// IVsDataConnectionDialog CreateConnectionDialogHandler();

	Task<object> CreateServiceInstanceAsync(Type serviceType, CancellationToken token);

	Task FinalizeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress);

	TInterface GetService<TService, TInterface>() where TInterface : class;

	Task<TInterface> GetServiceAsync<TService, TInterface>() where TInterface : class;




Task<object> ServicesCreatorCallbackAsync(IAsyncServiceContainer container, CancellationToken token, Type serviceType);

}