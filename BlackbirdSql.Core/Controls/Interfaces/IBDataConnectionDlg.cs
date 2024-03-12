using System;


namespace BlackbirdSql.Core.Controls.Interfaces;

public interface IBDataConnectionDlg
{
	bool UpdateServerExplorer {  get; }

	event EventHandler UpdateServerExplorerChangedEvent;
}
