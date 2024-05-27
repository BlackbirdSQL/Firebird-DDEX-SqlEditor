// Microsoft.SqlServer.BatchParser, Version=16.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// ManagedBatchParser.IBatchSource

using System;
using System.Data;
using System.Data.Common;
using System.Runtime.InteropServices;
using System.Threading;



namespace BlackbirdSql.Sys;

[Guid(LibraryData.NativeDbStatementServiceGuid)]


public interface IBsNativeDbStatementWrapper : IDisposable
{
	DbCommand Command { get; }

	EnSqlStatementAction CurrentAction { get; }

	DbDataReader CurrentActionReader { get; }

	long ExecutionTimeout { get; }

	EnSqlExecutionType ExecutionType { get; }

	long RowsSelected { get; }

	long TotalRowsSelected { get; }

	bool IsSpecialAction { get; }

	string Script { get; }

	EnSqlStatementType StatementType { get; }

	IDbTransaction Transaction { get; }


	event EventHandler<StatementExecutionEventArgs> AfterExecutionEvent;


	void Cancel();

	void DisposeCommand();

	int AsyncExecute(bool autoCommit);

	bool AsyncNextResult();

	bool AsyncRead();

	void GeneratePlan();

	void UpdateRowsSelected(long rowsSelected);
}
