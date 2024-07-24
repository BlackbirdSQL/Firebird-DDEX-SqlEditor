// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution.QEOLESQLErrorMessageEventHandler
// Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution.QEOLESQLErrorMessageEventArgs

using BlackbirdSql.Shared.Enums;



namespace BlackbirdSql.Shared.Events;


public delegate void ErrorMessageEventHandler(object sender, ErrorMessageEventArgs args);


public class ErrorMessageEventArgs : BatchErrorMessageEventArgs
{
	private readonly EnQESQLScriptProcessingMessageType m_msgType;

	public EnQESQLScriptProcessingMessageType MessageType => m_msgType;

	public ErrorMessageEventArgs(string errorLine, string msg, EnQESQLScriptProcessingMessageType msgType)
		: base(errorLine, msg)
	{
		m_msgType = msgType;
	}
}
