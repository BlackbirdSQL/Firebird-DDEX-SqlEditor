#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Threading;
using BlackbirdSql.Shared.Model.QueryExecution;




namespace BlackbirdSql.Shared.Events;


public class QESQLBatchNewResultSetEventArgs : EventArgs
{
	private readonly QEResultSet _ResultSet;
	private readonly CancellationToken _CancelToken;

	public QEResultSet ResultSet => _ResultSet;
	public CancellationToken CancelToken => _CancelToken;

	public QESQLBatchNewResultSetEventArgs(QEResultSet resultSet, CancellationToken cancelToken)
	{
		_ResultSet = resultSet;
		_CancelToken = cancelToken;
	}
}
