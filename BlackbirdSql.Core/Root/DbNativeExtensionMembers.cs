// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using BlackbirdSql.Sys;



namespace BlackbirdSql.Core;


// =========================================================================================================
//											DbNativeExtensionMembers Class
//
/// <summary>
/// Central class for database specific class extension methods. The intention is to be able to swop this
/// out with any other database engine client.
/// </summary>
// =========================================================================================================
static class DbNativeExtensionMembers
{
	/// <summary>
	/// Adds a parameter suffixed with an index to DbCommand.Parameters.
	/// </summary>
	/// <returns>The index of the parameter in DbCommand.Parameters else -1.</returns>
	public static int AddParameter(this DbCommand @this, string name, int index, object value)
	{
		return DbNative.DbCommandSvc.AddParameter(@this, name, index, value);
	}


	public static bool Completed(this IDbTransaction @this)
	{
		return DbNative.TransactionCompleted(@this);
	}

	public static DbCommand CreateDatabaseCommand(this DbConnection @this, string cmd /*, transaction*/)
	{
		return DbNative.DbConnectionSvc.CreateDbCommand( @this, cmd );
	}

	public static object CreateDatabaseInfoObject(this DbConnection @this)
	{
		return DbNative.DbConnectionSvc.CreateDatabaseInfoObject(@this);
	}


	public static string GetDataSource(this DbConnection @this)
	{
		return DbNative.DbConnectionSvc.GetDataSource(@this);
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the error number from a native database exception.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static int GetErrorCode(this Exception @this)
	{
		return DbNative.DbExceptionSvc.GetErrorCode(@this);
	}


	public static IList<object> GetErrors(this Exception @this)
	{
		return DbNative.DbExceptionSvc.GetErrors(@this);
	}

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the class byte value from a native database exception.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static byte GetClass(this DbException @this)
	{
		return DbNative.DbExceptionSvc.GetClass(@this);
	}



	/// <summary>
	/// Parses and converts a server version string to it's Version format.
	/// </summary>
	public static string GetDataSourceVersion(this IDbConnection @this)
	{
		return DbNative.DbConnectionSvc.GetDataSourceVersion(@this);
	}



	public static long GetActiveTransactionsCount(this NativeDatabaseInfoProxy @this)
	{
		return DbNative.DatabaseInfoSvc.GetActiveTransactionsCount(@this);
	}



	public static List<string> GetActiveUsers(this NativeDatabaseInfoProxy @this)
	{
		return DbNative.DatabaseInfoSvc.GetActiveUsers(@this);
	}



	public static long GetAllocationPages(this NativeDatabaseInfoProxy @this)
	{
		return DbNative.DatabaseInfoSvc.GetAllocationPages(@this);
	}



	public static long GetCurrentMemory(this NativeDatabaseInfoProxy @this)
	{
		return DbNative.DatabaseInfoSvc.GetCurrentMemory(@this);
	}



	public static long GetDatabaseSizeInPages(this NativeDatabaseInfoProxy @this)
	{
		return DbNative.DatabaseInfoSvc.GetDatabaseSizeInPages(@this);
	}



	public static long GetDeleteCount(this NativeDatabaseInfoProxy @this)
	{
		return DbNative.DatabaseInfoSvc.GetDeleteCount(@this);
	}


	public static List<(long, long)> GetTablesDatabaseInfo(this NativeDatabaseInfoProxy @this)
	{
		return DbNative.DatabaseInfoSvc.GetTablesDatabaseInfo(@this);
	}



	public static long GetExpungeCount(this NativeDatabaseInfoProxy @this)
	{
		return DbNative.DatabaseInfoSvc.GetExpungeCount(@this);
	}



	public static long GetServerCacheReadsCount(this NativeDatabaseInfoProxy @this)
	{
		return DbNative.DatabaseInfoSvc.GetServerCacheReadsCount(@this);
	}



	public static long GetInsertCount(this NativeDatabaseInfoProxy @this)
	{
		return DbNative.DatabaseInfoSvc.GetInsertCount(@this);
	}



	public static long GetServerCacheWritesCount(this NativeDatabaseInfoProxy @this)
	{
		return DbNative.DatabaseInfoSvc.GetServerCacheWritesCount(@this);
	}



	public static long GetMaxMemory(this NativeDatabaseInfoProxy @this)
	{
		return DbNative.DatabaseInfoSvc.GetMaxMemory(@this);
	}



	public static long GetPageSize(this NativeDatabaseInfoProxy @this)
	{
		return DbNative.DatabaseInfoSvc.GetPageSize(@this);
	}


	public static long GetNumBuffers(this NativeDatabaseInfoProxy @this)
	{
		return DbNative.DatabaseInfoSvc.GetNumBuffers(@this);
	}



	public static long GetPurgeCount(this NativeDatabaseInfoProxy @this)
	{
		return DbNative.DatabaseInfoSvc.GetPurgeCount(@this);
	}



	public static long GetReadIdxCount(this NativeDatabaseInfoProxy @this)
	{
		return DbNative.DatabaseInfoSvc.GetReadIdxCount(@this);
	}



	public static long GetReads(this NativeDatabaseInfoProxy @this)
	{
		return DbNative.DatabaseInfoSvc.GetReads(@this);
	}



	public static long GetReadSeqCount(this NativeDatabaseInfoProxy @this)
	{
		return DbNative.DatabaseInfoSvc.GetReadSeqCount(@this);
	}


	public static string GetServerVersion(this DbConnection @this)
	{
		return DbNative.DatabaseInfoSvc.GetServerVersion(@this);
	}



	public static long GetUpdateCount(this NativeDatabaseInfoProxy @this)
	{
		return DbNative.DatabaseInfoSvc.GetUpdateCount(@this);
	}



	public static long GetWrites(this NativeDatabaseInfoProxy @this)
	{
		return DbNative.DatabaseInfoSvc.GetWrites(@this);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the source line number of a native database exception.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static int GetLineNumber(this DbException @this)
	{
		return DbNative.DbExceptionSvc.GetLineNumber(@this);
	}


	public static int GetPacketSize(this DbConnection @this)
	{
		return DbNative.DbConnectionSvc.GetPacketSize(@this);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the method name of a native database excerption.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static string GetProcedure(this DbException @this)
	{
		return DbNative.DbExceptionSvc.GetProcedure(@this);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the server name from a native database exception.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static string GetServer(this DbException @this)
	{
		return DbNative.DbExceptionSvc.GetServer(@this);
	}



	public static async Task<DataTable> GetSchemaAsync(this DbConnection @this, string collectionName,
		string[] restrictions, CancellationToken cancellationToken)
	{
		return await DbNative.DbConnectionSvc.GetSchemaAsync(@this, collectionName, restrictions, cancellationToken);
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns the sql exception state of a native database exception.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static string GetState(this DbException @this)
	{
		return DbNative.DbExceptionSvc.GetState(@this);
	}




	/// <summary>
	/// Parses and converts a server version string to it's Version format.
	/// </summary>
	public static Version GetVersion(this IDbConnection @this)
	{
		return DbNative.DbConnectionSvc.GetVersion(@this);
	}


	internal static bool HasTransactions(this IDbTransaction @this)
	{
		if (@this == null)
			return false;

		DbConnection connection = (DbConnection)@this.Connection;

		NativeDatabaseInfoProxy dbInfo = new(connection);

		return dbInfo.GetActiveTransactionsCount() > 0;
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Checks if an exception is a native database exception.
	/// </summary>
	// ---------------------------------------------------------------------------------
	internal static bool HasSqlException(this DbException @this)
	{
		return DbNative.DbExceptionSvc.HasSqlException(@this);
	}

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Checks if an exception is a native database exception.
	/// </summary>
	// ---------------------------------------------------------------------------------
	internal static bool IsSqlException(this DbException @this)
	{
		return DbNative.DbExceptionSvc.IsSqlException(@this);
	}




	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Sets the server name in a native database exception.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static void SetServer(this DbException @this, string value)
	{
		DbNative.DbExceptionSvc.SetServer(@this, value);
	}

}
