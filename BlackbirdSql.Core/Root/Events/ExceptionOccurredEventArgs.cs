// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.ExceptionOccurredEventArgs

using System;
using System.Collections.Generic;



namespace BlackbirdSql.Core.Events;

public class ExceptionOccurredEventArgs : EventArgs
{
	public IEnumerable<Exception> Exceptions { get; private set; }

	// public EnFirewallRuleSource Source { get; set; }

	public ExceptionOccurredEventArgs(IEnumerable<Exception> exceptions /*, EnFirewallRuleSource source */)
	{
		Exceptions = exceptions;
		// Source = source;
	}
}
