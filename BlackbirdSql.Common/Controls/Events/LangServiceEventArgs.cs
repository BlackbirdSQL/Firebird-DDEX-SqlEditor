// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ResultPane.LangServiceEventArgs

using System;
using System.Runtime.InteropServices;


namespace BlackbirdSql.Common.Controls.Events;

[ComVisible(false)]
public class LangServiceEventArgs : EventArgs
{
	private Guid m_guid = Guid.Empty;

	public Guid ServiceGuid => m_guid;

	private LangServiceEventArgs()
	{
	}

	public LangServiceEventArgs(Guid svcGuid)
	{
		m_guid = svcGuid;
	}
}
