// Microsoft.SqlServer.BatchParser, Version=16.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// ManagedBatchParser.IBatchSource

using System;
using System.Data;


namespace BlackbirdSql.Sys.Interfaces;

public interface IBsQueryManager : IDisposable
{
	IDbConnection DataConnection { get; }
	ConnectionState DataConnectionState { get; }
	int StatementCount { get; }
	long ExecutionTimeout { get; }
	bool IsWithActualPlan { get; }
	bool IsWithClientStats { get; }
	DateTime? QueryExecutionStartTime { get; set; }
	DateTime? QueryExecutionEndTime { get; set; }

	IsolationLevel TtsIsolationLevel { get; }


	IDbTransaction Transaction { get; }

	void BeginTransaction();
	bool CommitTransactions(bool validate);
	void CloseConnection();
	void DisposeTransaction();
	bool RollbackTransactions(bool validate);
	void RaiseShowWindowFrame();
}
