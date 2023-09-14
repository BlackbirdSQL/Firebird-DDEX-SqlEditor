// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.SectionSaveContextEventArgs
using System;

namespace BlackbirdSql.Common.Ctl.Events;


public class SectionSaveContextEventArgs : EventArgs
{
	public object Context { get; set; }
}
