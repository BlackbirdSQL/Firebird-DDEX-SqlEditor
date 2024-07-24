// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution.QESQLBatchSpecialActionEventArgs

using System;
using System.Data;
using BlackbirdSql.Shared.Ctl.QueryExecution;
using BlackbirdSql.Shared.Enums;



namespace BlackbirdSql.Shared.Events;


public delegate void BatchSpecialActionEventHandler(object sender, BatchSpecialActionEventArgs args);


public sealed class BatchSpecialActionEventArgs : EventArgs
{
	private readonly EnSpecialActions m_action;

	private readonly QESQLBatch m_batch;

	private readonly IDataReader m_dr;

	public EnSpecialActions Action => m_action;

	public QESQLBatch Batch => m_batch;

	public IDataReader DataReader => m_dr;

	private BatchSpecialActionEventArgs()
	{
	}

	public BatchSpecialActionEventArgs(EnSpecialActions action, QESQLBatch batch, IDataReader dr)
	{
		m_action = action;
		m_batch = batch;
		m_dr = dr;
	}
}
