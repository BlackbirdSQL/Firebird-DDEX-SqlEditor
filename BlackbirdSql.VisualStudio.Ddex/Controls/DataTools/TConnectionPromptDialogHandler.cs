using System;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Sys.Enums;
using Microsoft.VisualStudio.Data.Core;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;
using Microsoft.VisualStudio.Shell;



namespace BlackbirdSql.VisualStudio.Ddex.Controls.DataTools;


public class TConnectionPromptDialogHandler : IBsDataConnectionPromptDialogHandler
{
	public TConnectionPromptDialogHandler()
	{

	}

	public void Dispose()
	{

	}


	private string _CompleteConnectionString = null;
	private string _PublicConnectionString = null;


	public string CompleteConnectionString => _CompleteConnectionString;

	public string PublicConnectionString
	{
		set { _PublicConnectionString = value; }
	}



	public bool ShowDialog()
	{
		IVsDataConnectionPromptDialog connectionPromptDialog = new TConnectionPromptDialog();

		using (connectionPromptDialog)
		{
			((IVsDataSiteableObject<IServiceProvider>)connectionPromptDialog).Site = ApcManager.ServiceProvider;
			IVsDataConnectionSupport connectionSupport = GetConnectionSupport(_PublicConnectionString);

			_CompleteConnectionString = connectionPromptDialog.ShowDialog(connectionSupport);
		}

		return !string.IsNullOrEmpty(_CompleteConnectionString);
	}

	private IVsDataConnectionSupport GetConnectionSupport(string connectionString)
	{
		IVsDataConnectionFactory factory = Package.GetGlobalService(typeof(IVsDataConnectionFactory)) as IVsDataConnectionFactory
			?? throw Diag.ExceptionService(typeof(IVsDataConnectionFactory));

		IVsDataConnectionSupport connectionSupport = null;
		Guid clsidProvider = new(SystemData.C_ProviderGuid);

		IVsDataConnection vsDataConnection = factory.CreateConnection(clsidProvider, connectionString, false);

		using (vsDataConnection)
		{
			connectionSupport = vsDataConnection.GetService(typeof(IVsDataConnectionSupport)) as IVsDataConnectionSupport
				?? throw Diag.ExceptionService(typeof(IVsDataConnectionSupport));

			((IBsDataConnectionSupport)connectionSupport).ConnectionSource = EnConnectionSource.Session;

			connectionSupport.ConnectionString = connectionString;
		}

		return connectionSupport;
	}

}
