// Microsoft.SqlServer.GridControl, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.StandardKeyProcessingEventArgs

using System;
using System.Windows.Forms;


namespace BlackbirdSql.Common.Controls.Events;

public class StandardKeyProcessingEventArgs : EventArgs
{
	private readonly Keys m_Key;

	private readonly Keys m_Modifiers;

	private bool m_ShouldHandle = true;

	public Keys Key => m_Key;

	public Keys Modifiers => m_Modifiers;

	public bool ShouldHandle
	{
		get
		{
			return m_ShouldHandle;
		}
		set
		{
			m_ShouldHandle = value;
		}
	}

	public StandardKeyProcessingEventArgs(KeyEventArgs ke)
	{
		m_Key = ke.KeyCode;
		m_Modifiers = ke.Modifiers;
	}

	protected StandardKeyProcessingEventArgs()
	{
	}
}
