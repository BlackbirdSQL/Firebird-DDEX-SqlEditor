
using System;
using System.Data;
using System.Data.Common;
using Microsoft.Data.ConnectionUI;

namespace BlackbirdSql.Core.Interfaces;


// =========================================================================================================
//
//									IBsConnectionCsb Interface
//
// =========================================================================================================
public interface IBsConnectionCsb : IDataConnectionProperties, IBsCsb
{
	DbConnection DataConnection { get; }
	DbTransaction DataTransaction { get; }
	bool HasTransactions { get; }
	bool PeekTransactions { get; }

	public Version ServerVersion { get; set; }
	ConnectionState State { get; }


	bool BeginTransaction(IsolationLevel isolationLevel);
	bool CloseConnection();
	DbCommand CreateCommand(string cmd = null);
	void DisposeConnection();
	void DisposeTransaction();
}
