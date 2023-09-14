// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.SectionInitializeEventArgs
using System;


namespace BlackbirdSql.Common.Ctl.Events;


public class SectionInitializeEventArgs : EventArgs
{
	public IServiceProvider ServiceProvider { get; private set; }

	public object Context { get; private set; }

	public SectionInitializeEventArgs(IServiceProvider serviceProvider, object context)
	{
		ServiceProvider = serviceProvider;
		Context = context;
	}
}
