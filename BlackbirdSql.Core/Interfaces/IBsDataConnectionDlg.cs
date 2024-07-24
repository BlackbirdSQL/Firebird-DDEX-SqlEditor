using System;


namespace BlackbirdSql.Core.Interfaces;

public interface IBsDataConnectionDlg
{
	bool UpdateServerExplorer { get; }

	event EventHandler UpdateServerExplorerChangedEvent;
}
