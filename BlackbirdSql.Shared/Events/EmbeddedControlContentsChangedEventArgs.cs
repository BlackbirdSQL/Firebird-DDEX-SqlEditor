// Microsoft.SqlServer.GridControl, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.EmbeddedControlContentsChangedEventArgs

using System;
using BlackbirdSql.Shared.Interfaces;



namespace BlackbirdSql.Shared.Events;

public class EmbeddedControlContentsChangedEventArgs : EventArgs
{
	private readonly IBGridEmbeddedControl m_EmbeddedControl;

	public IBGridEmbeddedControl EmbeddedControl => m_EmbeddedControl;

	public EmbeddedControlContentsChangedEventArgs(IBGridEmbeddedControl embCtrl)
	{
		m_EmbeddedControl = embCtrl;
	}
}
