// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.WPF.PlatformUI.DialogWindow

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;

using BlackbirdSql.Core.Diagnostics;
using BlackbirdSql.Core.Diagnostics.Enums;
using BlackbirdSql.Core.Events;
using BlackbirdSql.Common.Ctl;


namespace BlackbirdSql.Common.Controls;

public class DialogWindow : Window
{
	// private const int C_EngineMessagePasswordExpired = 18487;

	// private const int C_EngineMessagePasswordMustBeChanged = 18488;

	public bool? ShowModal(IntPtr parent)
	{
		return WindowHelper.ShowModal(this, parent);
	}

	public void ShowMessage(object sender, MessageEventArgs e)
	{
		UiTracer.TraceSource.AssertTraceEvent(e != null, TraceEventType.Error, EnUiTraceId.UiInfra, "e != null");
		if (e != null)
		{
			string message = e.Message;
			MessageBoxImage icon = e.Icon;
			string title = e.Title;
			UiTracer.TraceSource.AssertTraceEvent(message != null, TraceEventType.Error, EnUiTraceId.UiInfra, "message != null");
			UiTracer.TraceSource.AssertTraceEvent(title != null, TraceEventType.Error, EnUiTraceId.UiInfra, "title != null");
			if (e.Exception != null)
			{
				new NativeWindow().AssignHandle(new WindowInteropHelper(this).Handle);
				Cmd.ShowMessage(e.Exception, Properties.SharedResx.Error, MessageBoxButtons.OK, MessageBoxIcon.Hand, null);
			}
			else
			{
				System.Windows.MessageBox.Show(this, message, title, MessageBoxButton.OK, icon);
			}
		}
	}
}
