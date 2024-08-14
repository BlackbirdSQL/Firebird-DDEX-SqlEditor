// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution.QESQLBatchExecutedEventHandler
// Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution.QESQLBatchExecutedEventArgs

using BlackbirdSql.Sys.Enums;
using BlackbirdSql.Core.Enums;
using BlackbirdSql.Shared.Ctl.QueryExecution;
using System.Threading;
using System.Threading.Tasks;

namespace BlackbirdSql.Shared.Events;


public delegate Task BatchExecutionCompletedEventHandler(object sender, BatchExecutionCompletedEventArgs args);


public class BatchExecutionCompletedEventArgs : ExecutionCompletedEventArgs
{
	public BatchExecutionCompletedEventArgs(EnScriptExecutionResult res, QESQLBatch batch,
			EnSqlExecutionType executionType, CancellationToken cancelToken, CancellationToken syncToken)
		: base(res, executionType, true, cancelToken, syncToken)
	{
		_Batch = batch;
	}

	private readonly QESQLBatch _Batch = null;

	public QESQLBatch Batch => _Batch;
}
