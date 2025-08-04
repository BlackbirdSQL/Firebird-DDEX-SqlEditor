
using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Data.ConnectionUI;
using BlackbirdSql.Core.Events;

namespace BlackbirdSql.Core.Interfaces;


// =========================================================================================================
//
//									IBsConnectionCsb Interface
//
// =========================================================================================================
public interface IBsConnectionCsb : IDataConnectionProperties, IBsCsb
{
	long ConnectionId { get; }
	DbConnection DataConnection { get; }
	DbTransaction DataTransaction { get; }
	bool HasTransactions { get; }
	bool PeekTransactions { get; }
	internal Version ServerVersion { get; set; }
	ConnectionState State { get; }



	event ConnectionChangedDelegate ConnectionChangedEvent;



	bool BeginTransaction(IsolationLevel isolationLevel);
	bool CloseConnection();
	DbCommand CreateCommand(string cmd = null);

	/// <summary>
	/// Creates a new data connection. If a connection already exists, disposes of the
	/// connection.
	/// Always use this method to create connections because it invokes
	/// ConnectionChangedEvent.
	/// </summary>
	void CreateDataConnection();

	void DisposeConnection();
	void DisposeTransaction();

	/// <summary>
	/// Opens or verifies a connection. If no connection exists returns false.
	/// Throws an exception on failure.
	/// Always use this method to open connections because it disposes of the connection
	/// and invokes ConnectionChangedEvent on failure.
	/// Do not call before ensuring IsComplete.
	/// </summary>
	(bool, bool) OpenOrVerifyConnection();


	/// <summary>
	/// Opens or verifies a connection. The Connection must exists.
	/// Throws an exception on failure.
	/// Always use this method to open connections because it disposes of the connection
	/// and invokes ConnectionChangedEvent on failure.
	/// Do not call before ensuring IsComplete.
	/// The cardinal must be either zero, if no keepalive reads are required, or an
	/// incremental value that can be used to execute a select statement unique from
	/// the previous call to this method.
	/// </summary>
	/// <returns>
	/// Boolean tuple with Item1: True if open / verification succeeded and
	/// Item2: HasTransactions.
	/// </returns>
	Task<(bool, bool)> OpenOrVerifyConnectionAsync(CancellationToken cancelToken);
}
