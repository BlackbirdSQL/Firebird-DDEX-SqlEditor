
using System;
using System.Data;



namespace BlackbirdSql.Core.Events;


public delegate void ConnectionChangedDelegate(object sender, ConnectionChangedEventArgs e);


public class ConnectionChangedEventArgs(IDbConnection currentConnection, IDbConnection previousConnection) : EventArgs
{
	public IDbConnection CurrentConnection { get; private set; } = currentConnection;
	public IDbConnection PreviousConnection { get; private set; } = previousConnection;
}
