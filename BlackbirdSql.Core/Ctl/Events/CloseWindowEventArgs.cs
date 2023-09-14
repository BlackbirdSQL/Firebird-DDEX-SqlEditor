// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.CloseWindowEventArgs

using System;


namespace BlackbirdSql.Core.Ctl.Events;

public class CloseWindowEventArgs : EventArgs
{
	public bool Success { get; private set; }

	public CloseWindowEventArgs(bool success)
	{
		Success = success;
	}
}
