// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution.QESQLBatchMessageEventHandler
// Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution.QESQLBatchMessageEventArgs

using System;



namespace BlackbirdSql.Shared.Events;


public delegate void BatchMessageEventHandler(object sender, BatchMessageEventArgs args);


public class BatchMessageEventArgs : EventArgs
{
	private readonly string _Message = "";

	private readonly string _DetailedMessage = "";

	public string Message => _Message;

	public string DetailedMessage => _DetailedMessage;

	protected BatchMessageEventArgs()
	{
	}

	public BatchMessageEventArgs(string message)
	{
		_Message = message;
	}

	public BatchMessageEventArgs(string detailedMessage, string message)
	{
		_DetailedMessage = detailedMessage;
		_Message = message;
	}
}
