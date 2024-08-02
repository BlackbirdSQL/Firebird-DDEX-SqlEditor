// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution.QESQLBatchExecutedEventHandler
// Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution.QESQLBatchExecutedEventArgs

using BlackbirdSql.Sys.Enums;
using BlackbirdSql.Core.Enums;
using BlackbirdSql.Shared.Ctl.QueryExecution;

namespace BlackbirdSql.Shared.Events;


public delegate void BatchExecutionCompletedEventHandler(object sender, BatchExecutionCompletedEventArgs args);


public class BatchExecutionCompletedEventArgs : QueryExecutionCompletedEventArgs
{
	public BatchExecutionCompletedEventArgs(EnScriptExecutionResult res, bool syncCancel,
			QESQLBatch batch, EnSqlExecutionType executionType)
		: base(res, syncCancel, executionType)
	{
		Batch = batch;
	}

	public QESQLBatch Batch { get; private set; }
}
