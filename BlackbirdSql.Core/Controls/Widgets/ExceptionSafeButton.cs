// Microsoft.Data.ConnectionUI.Dialog, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.Data.ConnectionUI.ExceptionSafeButton
using System;
using System.Windows.Forms;
using BlackbirdSql.Core.Interfaces;


namespace BlackbirdSql.Core.Controls.Widgets;


internal class ExceptionSafeButton : Button
{
	protected override void OnClick(EventArgs e)
	{
		try
		{
			base.OnClick(e);
		}
		catch (Exception ex)
		{
			IBsConnectionDialog dataConnectionDialog = (IBsConnectionDialog)FindForm();
			dataConnectionDialog.ShowError(null, ex);
			((Form)dataConnectionDialog).DialogResult = DialogResult.None;
		}
	}

	internal void ClearHandlers()
	{
		base.Events.Dispose();
	}
}
