// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;



namespace BlackbirdSql.Sys.Interfaces;

[Guid(LibraryData.C_AsyncPackageGuid)]


public interface IBsAsyncPackage
{

	IBsPackageController ApcInstance { get; }
	IDisposable DisposableWaitCursor { get; set; }
	IVsSolution VsSolution { get; }
	IBsEventsManager EventsManager { get; }
	IOleServiceProvider OleServiceProvider { get; }



	delegate void LoadSolutionOptionsDelegate(Stream stream);
	delegate void SaveSolutionOptionsDelegate(Stream stream);

	event LoadSolutionOptionsDelegate OnLoadSolutionOptionsEvent;
	event SaveSolutionOptionsDelegate OnSaveSolutionOptionsEvent;



	// IVsDataConnectionDialog CreateConnectionDialogHandler();

	Task FinalizeAsync(CancellationToken cancelToken, IProgress<ServiceProgressData> progress);
	Task<TInterface> GetLocalServiceInstanceAsync<TService, TInterface>(CancellationToken token)
		where TInterface : class;
	TInterface GetService<TService, TInterface>() where TInterface : class;
	Task<TInterface> GetServiceAsync<TService, TInterface>() where TInterface : class;
	void OutputLoadStatistics();
	void SaveUserPreferences();
	Task<object> ServicesCreatorCallbackAsync(IAsyncServiceContainer container, CancellationToken token, Type serviceType);
}