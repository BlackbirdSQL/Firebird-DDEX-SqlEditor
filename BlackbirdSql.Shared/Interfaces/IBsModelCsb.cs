using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using BlackbirdSql.Core.Events;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Shared.Enums;



namespace BlackbirdSql.Shared.Interfaces;


// =========================================================================================================
//
//										IBsModelCsb Interface
//
// =========================================================================================================
public interface IBsModelCsb : IBsConnectionCsb
{
	EnCreationFlags CreationFlags { get; set; }


	/// <summary>
	/// Returns the connection using the latest available properties. If it's properties
	/// are outdated, closes the connection and applies the latest properties without
	/// reopening. Returns null if no connection exists. If a Close() fails, disposes of
	/// the connection.
	/// </summary>
	DbConnection LiveConnection { get; }
}
