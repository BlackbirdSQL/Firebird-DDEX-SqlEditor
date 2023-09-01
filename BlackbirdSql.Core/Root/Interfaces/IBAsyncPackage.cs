// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)


using System;
using System.Threading.Tasks;
using System.Threading;

using EnvDTE;

using BlackbirdSql.Core.Providers;

using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Operations;
using System.Runtime.InteropServices;




namespace BlackbirdSql.Core.Interfaces;


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