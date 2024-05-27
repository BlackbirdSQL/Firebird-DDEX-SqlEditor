
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection;
using BlackbirdSql.Sys;
using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.Data.Isql;



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
		// Catalog, Schema and TableType are no real restrictions
		// if (!name.EndsWith("Catalog") && !name.EndsWith("Schema") && name != "TableType")
		// {
		var pname = string.Format("@p{0}", index++);

		FbParameter parm = ((FbCommand)@this).Parameters.Add(pname, FbDbType.VarChar, 255);
		parm.Value = value;

		return index;
		// }

		// return -1;
	}


	/// <summary>
	/// Creates a Firebird data adapter using a DbCommand.
	/// </summary>
	public DbDataAdapter CreateDbDataAdapter_(DbCommand @this)
	{
		return new FbDataAdapter((FbCommand)@this);
	}


}
