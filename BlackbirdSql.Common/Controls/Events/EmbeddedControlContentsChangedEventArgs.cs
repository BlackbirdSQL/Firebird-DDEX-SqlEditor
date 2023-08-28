// Microsoft.SqlServer.GridControl, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.EmbeddedControlContentsChangedEventArgs

using System;
using BlackbirdSql.Common.Controls.Interfaces;



namespace BlackbirdSql.Common.Controls.Events;

public class EmbeddedControlContentsChangedEventArgs : EventArgs
{
	private readonly IGridEmbeddedControl m_EmbeddedControl;

	public IGridEmbeddedControl EmbeddedControl => m_EmbeddedControl;

	public EmbeddedControlContentsChangedEventArgs(IGridEmbeddedControl embCtrl)
	{
		m_EmbeddedControl = embCtrl;
	}
}
