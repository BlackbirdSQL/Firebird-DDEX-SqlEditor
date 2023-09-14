// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.MakeConnectionCompletedEventArgs

using System;
using System.Data;

namespace BlackbirdSql.Core.Ctl.Events;

public class MakeConnectionCompletedEventArgs : EventArgs
{
	public IDbConnection Connection { get; private set; }

	public string ConnectionString { get; private set; }

	public MakeConnectionCompletedEventArgs(IDbConnection connection, string connectionString)
	{
		Connection = connection;
		ConnectionString = connectionString;
	}
}
