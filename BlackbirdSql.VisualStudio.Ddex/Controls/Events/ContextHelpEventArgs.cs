// Microsoft.Data.ConnectionUI.Dialog, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.Data.ConnectionUI.ContextHelpEventArgs
using System.Drawing;
using System.Windows.Forms;
using BlackbirdSql.VisualStudio.Ddex.Controls.Enums;


namespace BlackbirdSql.VisualStudio.Ddex.Controls.Events;

public class ContextHelpEventArgs : HelpEventArgs
{
	private readonly EnDataConnectionDlgContext _context;

	public EnDataConnectionDlgContext Context => _context;

	public ContextHelpEventArgs(EnDataConnectionDlgContext context, Point mousePos)
		: base(mousePos)
	{
		_context = context;
	}
}
