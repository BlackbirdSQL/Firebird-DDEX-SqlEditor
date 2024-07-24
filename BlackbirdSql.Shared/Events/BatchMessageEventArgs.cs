// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution.QESQLBatchMessageEventHandler
// Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution.QESQLBatchMessageEventArgs

using System;



namespace BlackbirdSql.Shared.Events;


public delegate void BatchMessageEventHandler(object sender, BatchMessageEventArgs args);


public class BatchMessageEventArgs : EventArgs
{
	private readonly string m_strMsg = string.Empty;

	private readonly string m_strDetailedMsg = string.Empty;

	public string Message => m_strMsg;

	public string DetailedMessage => m_strDetailedMsg;

	protected BatchMessageEventArgs()
	{
	}

	public BatchMessageEventArgs(string msg)
	{
		m_strMsg = msg;
	}

	public BatchMessageEventArgs(string detailedMsg, string msg)
	{
		m_strMsg = msg;
		m_strDetailedMsg = detailedMsg;
	}
}
