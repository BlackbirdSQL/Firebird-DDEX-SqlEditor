using System;
using System.Windows.Forms;
using BlackbirdSql.Core.Controls.Events;


namespace BlackbirdSql.Core.Controls.Interfaces;

public interface IBDataConnectionDlg
{
	bool UpdateServerExplorer {  get; }

	event EventHandler UpdateServerExplorerChangedEvent;
}
