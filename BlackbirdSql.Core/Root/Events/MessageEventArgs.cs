// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.MessageEventArgs

using System;
using System.Windows;



namespace BlackbirdSql.Core.Events;

public class MessageEventArgs : EventArgs
{
	public string Message { get; private set; }

	public MessageBoxImage Icon { get; private set; }

	public string Title { get; private set; }

	public Exception Exception { get; private set; }

	public MessageEventArgs(string message, MessageBoxImage icon, string title)
	{
		Message = message;
		Icon = icon;
		Title = title;
	}

	public MessageEventArgs(string message, MessageBoxImage icon, string title, Exception ex)
		: this(message, icon, title)
	{
		Exception = ex;
	}
}
