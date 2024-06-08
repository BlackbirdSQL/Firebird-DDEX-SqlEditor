#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;

// using Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution;
// using Ns = Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution;


// namespace Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution
namespace BlackbirdSql.Shared.Events
{
	public class QESQLBatchMessageEventArgs : EventArgs
	{
		private readonly string m_strMsg = string.Empty;

		private readonly string m_strDetailedMsg = string.Empty;

		public string Message => m_strMsg;

		public string DetailedMessage => m_strDetailedMsg;

		protected QESQLBatchMessageEventArgs()
		{
		}

		public QESQLBatchMessageEventArgs(string msg)
		{
			m_strMsg = msg;
		}

		public QESQLBatchMessageEventArgs(string detailedMsg, string msg)
		{
			m_strMsg = msg;
			m_strDetailedMsg = detailedMsg;
		}
	}
}
