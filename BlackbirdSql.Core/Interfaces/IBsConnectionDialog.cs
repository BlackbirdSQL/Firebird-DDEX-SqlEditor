using System;
using System.Windows.Forms.Design;


namespace BlackbirdSql.Core.Interfaces;

public interface IBsConnectionDialog
{
	bool UpdateServerExplorer { get; }

	event EventHandler UpdateServerExplorerChangedEvent;

	void ShowError(string title, Exception ex);
	void ShowError(IUIService uiService, string title, Exception ex);
}
