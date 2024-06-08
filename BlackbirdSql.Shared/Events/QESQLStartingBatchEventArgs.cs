#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using BlackbirdSql.Shared.Ctl.QueryExecution;

namespace BlackbirdSql.Shared.Events;


public class QESQLStartingBatchEventArgs : EventArgs
{
	protected int m_lineNum = -1;

	protected QESQLBatch m_batch;

	public int BatchStartLineNumber => m_lineNum;

	public QESQLBatch Batch => m_batch;

	protected QESQLStartingBatchEventArgs()
	{
	}

	public QESQLStartingBatchEventArgs(int batchStartLineNum, QESQLBatch batch)
	{
		m_lineNum = batchStartLineNum;
		m_batch = batch;
	}
}
