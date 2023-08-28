// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.ConnectionsLoadedEventArgs

using System;

using BlackbirdSql.Core.Interfaces;



namespace BlackbirdSql.Core.Events;


public class ConnectionsLoadedEventArgs : EventArgs
{
	public IBServerDefinition ServerDefinition { get; private set; }

	public int NumberOfConnections { get; private set; }

	public ConnectionsLoadedEventArgs(IBServerDefinition serverDefinition, int numberOfConnections)
	{
		ServerDefinition = serverDefinition;
		NumberOfConnections = numberOfConnections;
	}
}
