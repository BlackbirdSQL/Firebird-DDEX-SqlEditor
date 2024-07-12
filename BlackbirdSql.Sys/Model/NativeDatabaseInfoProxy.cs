using System.Data;
using System.Data.Common;



namespace BlackbirdSql.Sys.Model;


public class NativeDatabaseInfoProxy
{

	public NativeDatabaseInfoProxy(DbConnection connection)
	{
		_NativeObject = NativeDb.DbConnectionSvc.CreateDatabaseInfoObject(connection);
	}

	public NativeDatabaseInfoProxy(IDbConnection connection)
	{
		_NativeObject = NativeDb.DbConnectionSvc.CreateDatabaseInfoObject((DbConnection)connection);
	}



	private readonly object _NativeObject = null;

	public object NativeObject => _NativeObject;
}
