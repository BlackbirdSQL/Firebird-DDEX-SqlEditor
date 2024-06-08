#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion


#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using BlackbirdSql.Shared.Enums;

namespace BlackbirdSql.Shared.Events;


public class QEOLESQLErrorMessageEventArgs : QESQLBatchErrorMessageEventArgs
{
	private readonly EnQESQLScriptProcessingMessageType m_msgType;

	public EnQESQLScriptProcessingMessageType MessageType => m_msgType;

	public QEOLESQLErrorMessageEventArgs(string errorLine, string msg, EnQESQLScriptProcessingMessageType msgType)
		: base(errorLine, msg)
	{
		m_msgType = msgType;
	}
}
