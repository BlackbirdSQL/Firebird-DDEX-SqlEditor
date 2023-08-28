// Microsoft.SqlServer.Data.Tools.ExceptionMessageBox, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.Data.Tools.ExceptionMessageBox.CopyToClipboardEventArgs

using System;

namespace BlackbirdSql.Core.Events
{
	public sealed class CopyToClipboardEventArgs : EventArgs
	{
		private readonly string m_clipboardText;

		private bool m_eventHandled;

		public string ClipboardText => m_clipboardText;

		public bool EventHandled
		{
			get
			{
				return m_eventHandled;
			}
			set
			{
				m_eventHandled = value;
			}
		}

		public CopyToClipboardEventArgs(string clipboardText)
		{
			m_clipboardText = clipboardText;
			m_eventHandled = false;
		}

		public CopyToClipboardEventArgs()
		{
			m_clipboardText = string.Empty;
			m_eventHandled = false;
		}
	}
}
