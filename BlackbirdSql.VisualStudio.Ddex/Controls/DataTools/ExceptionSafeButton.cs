// Microsoft.Data.ConnectionUI.Dialog, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.Data.ConnectionUI.ExceptionSafeButton
using System;
using System.Windows.Forms;


namespace BlackbirdSql.VisualStudio.Ddex.Controls.DataTools;

public class ExceptionSafeButton : Button
{
	protected override void OnClick(EventArgs e)
	{
		try
		{
			base.OnClick(e);
		}
		catch (Exception ex)
		{
			VxbConnectionDlg dataConnectionDialog = (VxbConnectionDlg)FindForm();
			dataConnectionDialog.ShowError(null, ex);
			dataConnectionDialog.DialogResult = DialogResult.None;
		}
	}

	internal void ClearHandlers()
	{
		base.Events.Dispose();
	}
}
