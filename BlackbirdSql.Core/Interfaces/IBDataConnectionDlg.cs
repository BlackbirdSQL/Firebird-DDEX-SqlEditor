using System;


namespace BlackbirdSql.Core.Interfaces;

public interface IBDataConnectionDlg
{
	bool UpdateServerExplorer { get; }

	event EventHandler UpdateServerExplorerChangedEvent;
}
