// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.IO;
using System.Management.Instrumentation;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.VisualStudio.RpcContracts.FileSystem;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;


namespace BlackbirdSql.Core.Ctl.Interfaces;

[Guid(SystemData.AsyncPackageGuid)]
public interface IBAsyncPackage
{

	IBPackageController ApcInstance { get; }

	Type SchemaFactoryType { get; }

	IDisposable DisposableWaitCursor { get; set; }

	IVsSolution VsSolution { get; }


	IBEventsManager EventsManager { get; }

	
	Microsoft.VisualStudio.OLE.Interop.IServiceProvider OleServiceProvider { get; }

	IAsyncServiceContainer ServiceContainer { get; }

	IFileSystemProvider FileSystemBrokeredService { get; }

	delegate void LoadSolutionOptionsDelegate(Stream stream);
	delegate void SaveSolutionOptionsDelegate(Stream stream);

	event LoadSolutionOptionsDelegate OnLoadSolutionOptionsEvent;
	event SaveSolutionOptionsDelegate OnSaveSolutionOptionsEvent;



	// IVsDataConnectionDialog CreateConnectionDialogHandler();

	Task<object> CreateServiceInstanceAsync(Type serviceType, CancellationToken token);

	Task FinalizeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress);

	TInterface GetService<TService, TInterface>() where TInterface : class;

	Task<TInterface> GetServiceAsync<TService, TInterface>() where TInterface : class;

	Task<object> ServicesCreatorCallbackAsync(IAsyncServiceContainer container, CancellationToken token, Type serviceType);

}