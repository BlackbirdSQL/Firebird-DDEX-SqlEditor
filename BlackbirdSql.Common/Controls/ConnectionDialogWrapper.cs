// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.ConnectionDialogWrapper

using System;
using System.Data;
using System.Diagnostics;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Diagnostics;
using BlackbirdSql.Core.Diagnostics.Enums;
using BlackbirdSql.Core.Events;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Common.Ctl;
using BlackbirdSql.Common.Events;
using BlackbirdSql.Common.Enums;
using BlackbirdSql.Common.Interfaces;

namespace BlackbirdSql.Common.Controls;

public class ConnectionDialogWrapper : IDisposable
{
	private IDbConnection _Connection;

	private readonly EventsChannel _Channel;

	private readonly IBDependencyManager _DependencyManager;

	// private IConnectionStringManager _connectionStringManager;

	public VerifyConnectionDelegate ConnectionVerifier { get; set; }

	public string EncryptedConnectionString { get; private set; }

	public Traceable Trace { get; set; }


	public ConnectionDialogWrapper()
	{
		_DependencyManager = ConnectionDialogService.Instance.DependencyManager;
		Trace = new Traceable(_DependencyManager);
		_Connection = null;
		_Channel = new EventsChannel();
		_Channel.MakeConnectionCompleted += GetConnectionObject;
		// ServiceManager<IConnectionStringManager> serviceManager = new ServiceManager<IConnectionStringManager>(_DependencyManager);
		// _connectionStringManager = serviceManager.GetService(null);
		// Trace.AssertTraceEvent(_connectionStringManager != null, TraceEventType.Error, EnUiTraceId.UiInfra, "Cannot load IConnectionStringManager");
	}

	public void Dispose()
	{
		_Channel.MakeConnectionCompleted -= GetConnectionObject;
		GC.SuppressFinalize(this);
	}

	public bool? ShowDialogValidateConnection(IntPtr parent, UIConnectionInfo ci, out IDbConnection connection)
	{
		ShowDialog(parent, ci, out connection, out var result, new ConnectionDialogConfiguration
		{
			IsConnectMode = true,
			InitialTab = EnConnectionDialogTab.Browse
		});
		return result;
	}

	public bool? ShowDialog(IntPtr parent, UIConnectionInfo ci, ConnectionDialogConfiguration config)
	{
		ShowDialog(parent, ci, out var connection, out var result, config);
		if (connection != null)
		{
			EncryptedConnectionString ??= connection.ConnectionString; // _connectionStringManager.EncryptConnectionString(connection.ConnectionString);

			connection.Dispose();
		}

		return result;
	}

	private UIConnectionInfo ShowDialog(IntPtr parent, UIConnectionInfo ci, out IDbConnection connection, out bool? result, ConnectionDialogConfiguration config)
	{
		UIConnectionInfo uIConnectionInfo = ci;
		BlackbirdSql.Core.Cmd.CheckForNull(ci, "uiConnectionInfo");
		BlackbirdSql.Core.Cmd.CheckForNull(parent, "parent");

		IBEditorPackage editorPackage = (IBEditorPackage)Controller.Instance.DdexPackage;

		result = editorPackage.ShowConnectionDialogFrame(parent, _DependencyManager, _Channel, ci, ConnectionVerifier, config, ref uIConnectionInfo);

		connection = _Connection;

		return uIConnectionInfo;
	}

	private void GetConnectionObject(object sender, MakeConnectionCompletedEventArgs e)
	{
		Trace.AssertTraceEvent(e != null, TraceEventType.Error, EnUiTraceId.Connection, "event argument is null");
		if (e != null)
		{
			_Connection = e.Connection;
			if (e.ConnectionString != null)
			{
				EncryptedConnectionString = e.ConnectionString; // _connectionStringManager.EncryptConnectionString(e.ConnectionString);
			}
		}
	}
}
