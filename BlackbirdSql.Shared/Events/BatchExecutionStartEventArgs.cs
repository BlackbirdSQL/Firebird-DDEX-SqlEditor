// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution.QESQLStartingBatchEventHandler
// Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution.QESQLStartingBatchEventArgs

using System;
using BlackbirdSql.Shared.Ctl.QueryExecution;



namespace BlackbirdSql.Shared.Events;


public delegate void BatchExecutionStartEventHandler(object sender, BatchExecutionStartEventArgs args);


public class BatchExecutionStartEventArgs : EventArgs
{
	protected int m_lineNum = -1;

	protected QESQLBatch m_batch;

	public int BatchStartLineNumber => m_lineNum;

	public QESQLBatch Batch => m_batch;

	protected BatchExecutionStartEventArgs()
	{
	}

	public BatchExecutionStartEventArgs(int batchStartLineNum, QESQLBatch batch)
	{
		m_lineNum = batchStartLineNum;
		m_batch = batch;
	}
}
