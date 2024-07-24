// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution.QESQLBatchErrorMessageEventHandler
// Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution.QESQLBatchErrorMessageEventArgs

using System;
using BlackbirdSql.Shared.Interfaces;



namespace BlackbirdSql.Shared.Events;


public delegate void BatchErrorMessageEventHandler(object sender, BatchErrorMessageEventArgs args);


public class BatchErrorMessageEventArgs : EventArgs
{
	private readonly string _DetailedMessage = "";

	private readonly string _DescriptionMessage = "";

	private readonly int _Line = -1;

	private readonly IBsTextSpan _TextSpan;

	public string DetailedMessage => _DetailedMessage;

	public string DescriptionMessage => _DescriptionMessage;

	public int Line => _Line;

	public IBsTextSpan TextSpan => _TextSpan;

	protected BatchErrorMessageEventArgs()
	{
	}

	public BatchErrorMessageEventArgs(string detailedMessage, string description)
	{
		_DetailedMessage = detailedMessage;
		_DescriptionMessage = description;
	}

	public BatchErrorMessageEventArgs(string detailedMessage, string description, int line, IBsTextSpan textSpan)
	{
		_DetailedMessage = detailedMessage;
		_DescriptionMessage = description;
		_Line = line;
		_TextSpan = textSpan;
	}
}
