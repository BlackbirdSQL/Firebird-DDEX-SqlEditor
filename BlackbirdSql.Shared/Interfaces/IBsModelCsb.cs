using System.Data.Common;
using System.Threading.Tasks;
using BlackbirdSql.Core.Events;
using BlackbirdSql.Core.Interfaces;



namespace BlackbirdSql.Shared.Interfaces;


// =========================================================================================================
//
//										IBsModelCsb Interface
//
// =========================================================================================================
public interface IBsModelCsb : IBsConnectionCsb
{
	long ConnectionId { get; }



	/// <summary>
	/// Returns the connection using the latest available properties. If it's properties
	/// are outdated, closes the connection and applies the latest properties without
	/// reopening. Returns null if no connection exists. If a Close() fails, disposes of
	/// the connection.
	/// </summary>
	DbConnection LiveConnection { get; }


	event ConnectionChangedDelegate ConnectionChangedEvent;



	/// <summary>
	/// Creates a new data connection. If a connection already exists, disposes of the
	/// connection.
	/// Always use this method to create connections because it invokes
	/// ConnectionChangedEvent.
	/// </summary>
	void CreateDataConnection();


	/// <summary>
	/// Opens or verifies a connection. If no connection exists returns false.
	/// Throws an exception on failure.
	/// Always use this method to open connections because it disposes of the connection
	/// and invokes ConnectionChangedEvent on failure.
	/// Do not call before ensuring IsComplete.
	/// </summary>
	bool OpenOrVerifyConnection();

	/// <summary>
	/// Opens or verifies a connection. If no connection exists returns false.
	/// Throws an exception on failure.
	/// Always use this method to open connections because it disposes of the connection
	/// and invokes ConnectionChangedEvent on failure.
	/// Do not call before ensuring IsComplete.
	/// </summary>
	Task<bool> OpenOrVerifyConnectionAsync();
}
