// Microsoft.SqlServer.GridControl, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.EmbeddedControlContentsChangedEventHandler
// Microsoft.SqlServer.Management.UI.Grid.EmbeddedControlContentsChangedEventArgs

using System;
using BlackbirdSql.Shared.Interfaces;



namespace BlackbirdSql.Shared.Events;


public delegate void EmbeddedControlContentsChangedEventHandler(object sender, EmbeddedControlContentsChangedEventArgs args);


public class EmbeddedControlContentsChangedEventArgs : EventArgs
{
	private readonly IBsGridEmbeddedControl m_EmbeddedControl;

	public IBsGridEmbeddedControl EmbeddedControl => m_EmbeddedControl;

	public EmbeddedControlContentsChangedEventArgs(IBsGridEmbeddedControl embCtrl)
	{
		m_EmbeddedControl = embCtrl;
	}
}
