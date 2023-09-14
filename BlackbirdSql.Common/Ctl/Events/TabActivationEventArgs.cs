// Microsoft.VisualStudio.Data.Tools.Design.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Design.Core.Controls.TabbedEditor.TabActivationEventArgs
using System;

namespace BlackbirdSql.Common.Ctl.Events;

public class TabActivationEventArgs : EventArgs
{
	private Guid _logicalView;

	private bool? _showAtTop;

	public bool? ShowAtTop
	{
		get
		{
			return _showAtTop;
		}
		set
		{
			_showAtTop = value;
		}
	}

	public Guid LogicalView => _logicalView;

	public TabActivationEventArgs(Guid logicalView)
	{
		_logicalView = logicalView;
	}

	public TabActivationEventArgs(Guid logicalView, bool? showAtTop)
	{
		_logicalView = logicalView;
		ShowAtTop = showAtTop;
	}
}