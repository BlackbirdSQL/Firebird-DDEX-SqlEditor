// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.SelectedConnectionChangedEventArgs

using System;
using BlackbirdSql.Core.Ctl.Interfaces;


namespace BlackbirdSql.Core.Ctl.Events;

public class SelectedConnectionChangedEventArgs : EventArgs
{
	public IBPropertyAgent ConnectionInfo { get; private set; }

	public SelectedConnectionChangedEventArgs(IBPropertyAgent connectionInfo)
	{
		ConnectionInfo = connectionInfo;
	}
}
