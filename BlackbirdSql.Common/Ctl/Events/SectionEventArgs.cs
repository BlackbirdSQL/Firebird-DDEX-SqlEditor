// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.WPF.Framework.SectionEventArgs

using System;
using System.ComponentModel;
using System.Diagnostics;
using BlackbirdSql.Common.Ctl;
using BlackbirdSql.Common.Ctl.Interfaces;
using BlackbirdSql.Core.Ctl.Enums;
using BlackbirdSql.Core.Ctl.Diagnostics;

namespace BlackbirdSql.Common.Ctl.Events;


[EditorBrowsable(EditorBrowsableState.Never)]
public class SectionEventArgs : EventArgs
{
	public IBSection Section => SectionHost.Section;

	public SectionHost SectionHost { get; private set; }

	public SectionEventArgs(SectionHost sectionHost)
	{
		UiTracer.TraceSource.AssertTraceEvent(sectionHost != null, TraceEventType.Error, EnUiTraceId.UiInfra, "sectionHost != null");
		SectionHost = sectionHost;
	}
}
