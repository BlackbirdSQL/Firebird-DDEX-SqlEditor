
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using BlackbirdSql.Sys.Enums;



namespace BlackbirdSql.Shared.Events;


public delegate Task<bool> ExecutionStartedEventHandler(object sender, ExecutionStartedEventArgs args);


public class ExecutionStartedEventArgs : EventArgs
{
	public ExecutionStartedEventArgs(string queryText, EnSqlExecutionType executionType, bool launched,
		IDbConnection connection, CancellationToken cancelToken, CancellationToken syncToken)
	{
		_QueryText = queryText;
		_ExecutionType = executionType;
		_Launched = launched;
		_Connection = connection;
		_CancelToken = cancelToken;
		_SyncToken = syncToken;
	}

	private readonly string _QueryText;
	private readonly EnSqlExecutionType _ExecutionType;
	private readonly bool _Launched;
	private readonly IDbConnection _Connection;
	private readonly CancellationToken _CancelToken;
	private readonly CancellationToken _SyncToken;
	private bool _Result = true;


	public string QueryText => _QueryText;
	public EnSqlExecutionType ExecutionType => _ExecutionType;
	public bool Launched => _Launched;
	public IDbConnection Connection => _Connection;
	public CancellationToken CancelToken => _CancelToken;
	public CancellationToken SyncToken => _SyncToken;


	public bool Result
	{
		get { return _Result; }
		set { _Result = value; }
	}
}
