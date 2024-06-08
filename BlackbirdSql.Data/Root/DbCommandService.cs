using System.Data.Common;
using BlackbirdSql.Sys.Interfaces;
using FirebirdSql.Data.FirebirdClient;



namespace BlackbirdSql.Data;


// =========================================================================================================
//											Firebird DatabaseEngineService Class
//
/// <summary>
/// Central class for database specific class methods. The intention is ultimately provide these as a
/// service so that BlackbirdSql can provide support for additional database engines.
/// </summary>
// =========================================================================================================
public class DbCommandService : SBsNativeDbCommand, IBsNativeDbCommand
{
	private DbCommandService()
	{
	}

	public static DbCommandService CreateInstance() => new();


	/// <summary>
	/// Adds a parameter suffixed with an index to DbCommand.Parameters.
	/// </summary>
	/// <returns>The index of the parameter in DbCommand.Parameters else -1.</returns>
	public int AddParameter(DbCommand @this, string name, int index, object value)
	{
		return ((FbCommand)@this).AddParameter(name, index, value);
	}


}
