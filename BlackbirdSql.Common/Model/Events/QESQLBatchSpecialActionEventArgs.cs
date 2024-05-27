// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution.QESQLBatchSpecialActionEventArgs
using System;
using System.Data;
using BlackbirdSql.Common.Model.Enums;
using BlackbirdSql.Common.Model.QueryExecution;

namespace BlackbirdSql.Common.Model.Events;

public sealed class QESQLBatchSpecialActionEventArgs : EventArgs
{
	private readonly EnSqlSpecialActions m_action;

	private readonly QESQLBatch m_batch;

	private readonly IDataReader m_dr;

	public EnSqlSpecialActions Action => m_action;

	public QESQLBatch Batch => m_batch;

	public IDataReader DataReader => m_dr;

	private QESQLBatchSpecialActionEventArgs()
	{
	}

	public QESQLBatchSpecialActionEventArgs(EnSqlSpecialActions action, QESQLBatch batch, IDataReader dr)
	{
		m_action = action;
		m_batch = batch;
		m_dr = dr;
	}
}
