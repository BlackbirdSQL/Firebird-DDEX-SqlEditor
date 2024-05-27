using System.Data;
using System.Data.Common;



namespace BlackbirdSql.Sys;


public class NativeDatabaseInfoProxy
{
	private readonly object _NativeObject = null;

	public object NativeObject => _NativeObject;

	public NativeDatabaseInfoProxy(DbConnection connection)
	{
		_NativeObject = DbNative.DbConnectionSvc.CreateDatabaseInfoObject(connection);
	}

	public NativeDatabaseInfoProxy(IDbConnection connection)
	{
		_NativeObject = DbNative.DbConnectionSvc.CreateDatabaseInfoObject((DbConnection)connection); 
	}
}
