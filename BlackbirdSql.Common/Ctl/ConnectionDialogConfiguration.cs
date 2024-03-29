﻿// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.ConnectionDialogConfiguration

using BlackbirdSql.Common.Ctl.Enums;

namespace BlackbirdSql.Common.Ctl;


public sealed class ConnectionDialogConfiguration
{
	public bool IsConnectMode { get; set; }

	public EnConnectionDialogTab InitialTab { get; set; }

	public ConnectionDialogConfiguration()
	{
		IsConnectMode = true;
	}
}
