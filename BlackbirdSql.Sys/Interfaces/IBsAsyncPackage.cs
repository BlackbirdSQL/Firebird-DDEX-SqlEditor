// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.ComponentModel.Design;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;



namespace BlackbirdSql.Sys.Interfaces;

[Guid(LibraryData.AsyncPackageGuid)]


public interface IBsAsyncPackage
{

	IBsPackageController ApcInstance { get; }

	IDisposable DisposableWaitCursor { get; set; }

	IVsSolution VsSolution { get; }


	IBsEventsManager EventsManager { get; }


	Microsoft.VisualStudio.OLE.Interop.IServiceProvider OleServiceProvider { get; }

	IAsyncServiceContainer ServiceContainer { get; }

	IServiceContainer SyncServiceContainer { get; }

	delegate void LoadSolutionOptionsDelegate(Stream stream);
	delegate void SaveSolutionOptionsDelegate(Stream stream);

	event LoadSolutionOptionsDelegate OnLoadSolutionOptionsEvent;
	event SaveSolutionOptionsDelegate OnSaveSolutionOptionsEvent;



	// IVsDataConnectionDialog CreateConnectionDialogHandler();

	Task<object> CreateServiceInstanceAsync(Type serviceType, CancellationToken token);

	Task FinalizeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress);

	TInterface GetService<TService, TInterface>() where TInterface : class;

	Task<TInterface> GetServiceAsync<TService, TInterface>() where TInterface : class;

	void SaveUserPreferences();

	object ServicesCreatorCallback(IServiceContainer container, Type serviceType);

	Task<object> ServicesCreatorCallbackAsync(IAsyncServiceContainer container, CancellationToken token, Type serviceType);

}