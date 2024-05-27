// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.ConnectionsLoadedEventArgs

using System;
using BlackbirdSql.Sys;


namespace BlackbirdSql.Core.Ctl.Events;

public class ConnectionsLoadedEventArgs : EventArgs
{
	public EnEngineType ServerEngine { get; private set; }

	public int NumberOfConnections { get; private set; }

	public ConnectionsLoadedEventArgs(EnEngineType serverEngine, int numberOfConnections)
	{
		ServerEngine = serverEngine;
		NumberOfConnections = numberOfConnections;
	}
}
