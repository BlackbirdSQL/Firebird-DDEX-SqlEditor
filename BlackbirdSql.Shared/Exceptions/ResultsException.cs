// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution.QESQLBatchConsumerException

using System;
using System.Runtime.Serialization;

// using Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution;




namespace BlackbirdSql.Shared.Exceptions;


[Serializable]
public class ResultsException : ApplicationException
{
	public ResultsException(string errorString) : base(errorString)
	{
	}

	protected ResultsException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
