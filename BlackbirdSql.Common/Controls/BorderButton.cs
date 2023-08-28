// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.WPF.Framework.BorderButton

using System.Windows.Automation.Peers;
using System.Windows.Controls;




namespace BlackbirdSql.Common.Controls;


public class BorderButton : Border
{
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new BorderButtonAutomationPeer(this);
	}
}
