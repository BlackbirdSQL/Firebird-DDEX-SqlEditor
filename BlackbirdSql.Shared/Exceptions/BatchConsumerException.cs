// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution.QESQLBatchConsumerException

using System;
using System.Runtime.Serialization;

// using Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution;




namespace BlackbirdSql.Shared.Exceptions;


[Serializable]
public class BatchConsumerException : ApplicationException
{
	public enum EnErrorType
	{
		CannotShowMoreResults
	}

	private readonly EnErrorType extraInfo;

	public EnErrorType ExtraInfo => extraInfo;

	public BatchConsumerException(string errorString, EnErrorType extraInfo)
		: base(errorString)
	{
		this.extraInfo = extraInfo;
	}

	protected BatchConsumerException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
