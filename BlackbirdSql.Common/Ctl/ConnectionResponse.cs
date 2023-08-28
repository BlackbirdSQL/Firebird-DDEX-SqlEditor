// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.ConnectionResponse
using System;
using System.Data;


namespace BlackbirdSql.Common.Ctl;


public class ConnectionResponse
{
	public bool Success { get; set; }

	public Exception Exception { get; set; }

	public IDbConnection Connection { get; set; }
}
