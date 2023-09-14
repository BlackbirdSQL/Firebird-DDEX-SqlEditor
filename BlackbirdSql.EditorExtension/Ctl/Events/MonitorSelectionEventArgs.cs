// Microsoft.VisualStudio.Data.Tools.Design.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Design.Core.Common.MonitorSelectionEventArgs

using System;

namespace BlackbirdSql.EditorExtension.Ctl.Events;


public class MonitorSelectionEventArgs : EventArgs
{
	public object OldValue { get; private set; }

	public object NewValue { get; private set; }

	public MonitorSelectionEventArgs(object oldValue, object newValue)
	{
		OldValue = oldValue;
		NewValue = newValue;
	}
}
