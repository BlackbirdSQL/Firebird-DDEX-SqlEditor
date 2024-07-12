// Microsoft.Data.ConnectionUI.Dialog, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.Data.ConnectionUI.ContextHelpEventArgs
using System.Drawing;
using System.Windows.Forms;
using BlackbirdSql.VisualStudio.Ddex.Enums;



namespace BlackbirdSql.VisualStudio.Ddex.Events;


public class ContextHelpEventArgs(EnDataConnectionDlgContext context, Point mousePos) : HelpEventArgs(mousePos)
{
	private readonly EnDataConnectionDlgContext _Context = context;


	public EnDataConnectionDlgContext Context => _Context;
}
