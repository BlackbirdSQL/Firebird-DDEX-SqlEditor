
using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using BlackbirdSql.Sys.Interfaces;
using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.Data.Services;



namespace BlackbirdSql.Data;


// =========================================================================================================
//											Firebird DatabaseEngineService Class
//
/// <summary>
/// Central class for database specific class methods. The intention is ultimately provide these as a
/// service so that BlackbirdSql can provide support for additional database engines.
/// </summary>
// =========================================================================================================
public class DbConnectionService : SBsNativeDbConnection, IBsNativeDbConnection
{
	private DbConnectionService()
	{
	}

	public static IBsNativeDbConnection EnsureInstance() => _Instance ??= new DbConnectionService();


	public static IBsNativeDbConnection _Instance = null;


	public object CreateDatabaseInfoObject(DbConnection @this)
	{
		FbDatabaseInfo info = new((FbConnection)@this);

		return info;
	}

	public DbCommand CreateDbCommand(DbConnection @this,  string cmdText = null, object transaction = null)
	{
		return new FbCommand(cmdText, (FbConnection)@this, (FbTransaction)transaction);
	}

	public string GetDataSource(DbConnection @this)
	{
		return ((FbConnection)@this).DataSource;
	}


	/// <summary>
	/// Parses and converts a server version string to it's Version format.
	/// </summary>
	public string GetDataSourceVersion(IDbConnection @this)
	{
		return "Firebird " + FbServerProperties.ParseServerVersion(((FbConnection)@this).ServerVersion);
	}


	public int GetPacketSize(DbConnection @this)
	{
		return ((FbConnection)@this).PacketSize;
	}


	public async Task<DataTable> GetSchemaAsync(DbConnection @this, string collectionName,
		string[] restrictions, CancellationToken cancellationToken)
	{
		if (@this is not FbConnection connection)
			return null;

		return await connection.GetSchemaAsync(collectionName, restrictions, cancellationToken);
	}


	/// <summary>
	/// Parses and converts a server version string to it's Version format.
	/// </summary>
	public Version GetVersion(IDbConnection @this)
	{
		if (@this is not FbConnection connection)
			return new();

		return connection.GetVersion();
	}


}
