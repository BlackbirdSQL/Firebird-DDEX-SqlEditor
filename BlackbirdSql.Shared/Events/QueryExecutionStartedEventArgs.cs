
using System;
using System.Data;
using BlackbirdSql.Core.Enums;
using BlackbirdSql.Sys.Enums;



namespace BlackbirdSql.Shared.Events;


public delegate bool QueryExecutionStartedEventHandler(object sender, QueryExecutionStartedEventArgs args);


public class QueryExecutionStartedEventArgs : EventArgs
{

	public QueryExecutionStartedEventArgs(string queryText, EnSqlExecutionType executionType, IDbConnection connection)
	{
		QueryText = queryText;
		ExecutionType = executionType;
		Connection = connection;
	}

	public string QueryText { get; private set; }

	public EnSqlExecutionType ExecutionType { get; private set; }

	public IDbConnection Connection { get; private set; }
}
