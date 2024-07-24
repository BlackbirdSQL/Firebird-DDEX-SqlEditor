
using System.Data;
using System.Data.Common;

namespace BlackbirdSql.Core.Interfaces;


// =========================================================================================================
//
//									IBsModelPropertyAgent Interface
//
// =========================================================================================================
public interface IBsModelPropertyAgent : IBsPropertyAgent
{
	int CommandTimeout { get; set; }
	int ConnectionLifeTime { get; set; }
	string ConnectionString { get; }
	IDbConnection DataConnection { get; }
	IDbTransaction DataTransaction { get; }
	bool HasTransactions { get; }
	string Moniker { get; }
	ConnectionState State { get; }


	void BeginTransaction(IsolationLevel isolationLevel);
	DbCommand CreateCommand(string cmd = null);
	void DisposeConnection();
	void DisposeTransaction(bool force);
}
