// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution.QESQLBatchNewResultSetEventHandler
// Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution.QESQLBatchNewResultSetEventArgs

using System;
using System.Threading;
using System.Threading.Tasks;
using BlackbirdSql.Shared.Model.QueryExecution;



namespace BlackbirdSql.Shared.Events;


public delegate Task<bool> BatchNewResultSetEventHandler(object sender, BatchNewResultSetEventArgs args);


public class BatchNewResultSetEventArgs : EventArgs
{
	private readonly QEResultSet _ResultSet;
	private readonly CancellationToken _CancelToken;

	public QEResultSet ResultSet => _ResultSet;
	public CancellationToken CancelToken => _CancelToken;

	public BatchNewResultSetEventArgs(QEResultSet resultSet, CancellationToken cancelToken)
	{
		_ResultSet = resultSet;
		_CancelToken = cancelToken;
	}
}
