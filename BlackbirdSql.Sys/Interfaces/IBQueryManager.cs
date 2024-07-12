// Microsoft.SqlServer.BatchParser, Version=16.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// ManagedBatchParser.IBatchSource

using System;
using System.Data;


namespace BlackbirdSql.Sys.Interfaces;

public interface IBQueryManager : IDisposable
{
	IDbConnection Connection { get; }

	long ExecutionTimeout { get; }
	bool IsWithActualPlan { get; }
	bool IsWithClientStats { get; }
	DateTime? QueryExecutionStartTime { get; set; }
	DateTime? QueryExecutionEndTime { get; set; }

	IsolationLevel TtsIsolationLevel { get; }


	IDbTransaction Transaction { get; }

	void BeginTransaction();

	void CommitTransaction();

	void RollbackTransaction();

	void DisposeTransaction(bool force);
}
