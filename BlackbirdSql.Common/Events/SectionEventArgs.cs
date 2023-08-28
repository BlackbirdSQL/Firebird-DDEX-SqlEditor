// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.WPF.Framework.SectionEventArgs

using System;
using System.ComponentModel;
using System.Diagnostics;

using BlackbirdSql.Core.Diagnostics;
using BlackbirdSql.Core.Diagnostics.Enums;
using BlackbirdSql.Common.Ctl;
using BlackbirdSql.Common.Interfaces;

namespace BlackbirdSql.Common.Events;


[EditorBrowsable(EditorBrowsableState.Never)]
public class SectionEventArgs : EventArgs
{
	public ISection Section => SectionHost.Section;

	public SectionHost SectionHost { get; private set; }

	public SectionEventArgs(SectionHost sectionHost)
	{
		UiTracer.TraceSource.AssertTraceEvent(sectionHost != null, TraceEventType.Error, EnUiTraceId.UiInfra, "sectionHost != null");
		SectionHost = sectionHost;
	}
}
