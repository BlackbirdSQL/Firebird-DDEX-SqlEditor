// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using BlackbirdSql.Sys.Enums;
using BlackbirdSql.Sys.Interfaces;
using BlackbirdSql.Sys.Model;
using Microsoft.VisualStudio.Data.Services;



namespace BlackbirdSql;


// =========================================================================================================
//											DbNativeExtensionMembers Class
//
/// <summary>
/// Central class for database specific class extension methods. The intention is to be able to swop this
/// out with any other database engine client.
/// </summary>
// =========================================================================================================
public static class NativeDbExtensionMembers
{

	/// <summary>
	/// Adds a parameter suffixed with an index to DbCommand.Parameters.
	/// </summary>
	/// <returns>The index of the parameter in DbCommand.Parameters else -1.</returns>
	public static int AddParameter(this DbCommand @this, string name, int index, object value)
	{
		return NativeDb.DatabaseEngineSvc.AddCommandParameter_(@this, name, index, value);
	}

	public static bool Completed(this IDbTransaction @this)
	{
		return NativeDb.DatabaseEngineSvc.TransactionCompleted_(@this);
	}



	public static string GetDecoratedDdlSource(this IVsDataExplorerNode @this, EnModelTargetType targetType)
	{
		return NativeDb.GetDecoratedDdlSource(@this, targetType);
	}

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the error number from a native database exception.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static int GetErrorCode(this Exception @this)
	{
		return NativeDb.DbExceptionSvc.GetExceptionErrorCode_(@this);
	}


	public static IList<object> GetErrors(this Exception @this)
	{
		return NativeDb.DbExceptionSvc.GetExceptionErrors_(@this);
	}

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the class byte value from a native database exception.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static byte GetClass(this Exception @this)
	{
		return NativeDb.DbExceptionSvc.GetExceptionClass_(@this);
	}



	/// <summary>
	/// Gets the connection datasource.
	/// </summary>
	public static string GetDataSource(this IDbConnection @this)
	{
		return NativeDb.DatabaseEngineSvc.GetConnectionDataSource_(@this);
	}



	/// <summary>
	/// Parses and converts a server version string to it's Version format.
	/// </summary>
	public static string GetDataSourceVersion(this IDbConnection @this)
	{
		return NativeDb.DatabaseEngineSvc.GetConnectionDataSourceVersion_(@this);
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Disposes of a parser given an IVsDataConnection site.
	/// </summary>
	/// <param name="site">
	/// The IVsDataConnection explorer connection object
	/// </param>
	/// <param name="disposing">
	/// If disposing is set to true, then all parsers with weak equivalency will
	/// be tagged as intransient, meaning their trigger linkage databases cannot
	/// be copied to another parser with weak equivalency. 
	/// </param>
	/// <returns>True of the parser was found and disposed else false.</returns>
	// -------------------------------------------------------------------------
	public static bool DisposeLinkageParser(this IVsDataExplorerConnection @this, bool disposing)
	{
		return NativeDb.DatabaseEngineSvc.DisposeLinkageParserInstance_(@this, disposing);
	}

	public static void EnsureLinkageLoadingAsyin(this IVsDataExplorerConnection @this, int delay = 0, int multiplier = 1)
	{
		NativeDb.DatabaseEngineSvc.EnsureLinkageLoadingAsyin_(@this, delay, multiplier);
	}


	public static IBsNativeDbLinkageParser GetLinkageParser(this IVsDataExplorerConnection @this) => NativeDb.DatabaseEngineSvc.GetLinkageParserInstance_(@this);

	public static long GetActiveTransactionsCount(this NativeDatabaseInfoProxy @this)
	{
		return NativeDb.DatabaseInfoSvc.GetActiveTransactionsCount(@this);
	}



	public static List<string> GetActiveUsers(this NativeDatabaseInfoProxy @this)
	{
		return NativeDb.DatabaseInfoSvc.GetActiveUsers(@this);
	}



	public static long GetAllocationPages(this NativeDatabaseInfoProxy @this)
	{
		return NativeDb.DatabaseInfoSvc.GetAllocationPages(@this);
	}



	public static long GetCurrentMemory(this NativeDatabaseInfoProxy @this)
	{
		return NativeDb.DatabaseInfoSvc.GetCurrentMemory(@this);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the connection name or datasetName from a native database exception.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static string GetDatabase(this Exception @this)
	{
		return NativeDb.DbExceptionSvc.GetExceptionDatabase_(@this);
	}



	public static long GetDatabaseSizeInPages(this NativeDatabaseInfoProxy @this)
	{
		return NativeDb.DatabaseInfoSvc.GetDatabaseSizeInPages(@this);
	}



	public static long GetDeleteCount(this NativeDatabaseInfoProxy @this)
	{
		return NativeDb.DatabaseInfoSvc.GetDeleteCount(@this);
	}


	public static List<(long, long)> GetTablesDatabaseInfo(this NativeDatabaseInfoProxy @this)
	{
		return NativeDb.DatabaseInfoSvc.GetTablesDatabaseInfo(@this);
	}



	public static long GetExpungeCount(this NativeDatabaseInfoProxy @this)
	{
		return NativeDb.DatabaseInfoSvc.GetExpungeCount(@this);
	}



	public static long GetServerCacheReadsCount(this NativeDatabaseInfoProxy @this)
	{
		return NativeDb.DatabaseInfoSvc.GetServerCacheReadsCount(@this);
	}



	public static long GetInsertCount(this NativeDatabaseInfoProxy @this)
	{
		return NativeDb.DatabaseInfoSvc.GetInsertCount(@this);
	}


	public static DataTable GetSchemaEx(this IDbConnection @this, string collectionName, string[] restrictions)
	{
		return NativeDb.ProviderSchemaFactorySvc.GetSchema(@this, collectionName, restrictions);
	}

	public static long GetServerCacheWritesCount(this NativeDatabaseInfoProxy @this)
	{
		return NativeDb.DatabaseInfoSvc.GetServerCacheWritesCount(@this);
	}



	public static long GetMaxMemory(this NativeDatabaseInfoProxy @this)
	{
		return NativeDb.DatabaseInfoSvc.GetMaxMemory(@this);
	}



	public static long GetPageSize(this NativeDatabaseInfoProxy @this)
	{
		return NativeDb.DatabaseInfoSvc.GetPageSize(@this);
	}


	public static long GetNumBuffers(this NativeDatabaseInfoProxy @this)
	{
		return NativeDb.DatabaseInfoSvc.GetNumBuffers(@this);
	}



	public static long GetPurgeCount(this NativeDatabaseInfoProxy @this)
	{
		return NativeDb.DatabaseInfoSvc.GetPurgeCount(@this);
	}



	public static long GetReadIdxCount(this NativeDatabaseInfoProxy @this)
	{
		return NativeDb.DatabaseInfoSvc.GetReadIdxCount(@this);
	}



	public static long GetReads(this NativeDatabaseInfoProxy @this)
	{
		return NativeDb.DatabaseInfoSvc.GetReads(@this);
	}



	public static long GetReadSeqCount(this NativeDatabaseInfoProxy @this)
	{
		return NativeDb.DatabaseInfoSvc.GetReadSeqCount(@this);
	}




	public static long GetUpdateCount(this NativeDatabaseInfoProxy @this)
	{
		return NativeDb.DatabaseInfoSvc.GetUpdateCount(@this);
	}



	public static long GetWrites(this NativeDatabaseInfoProxy @this)
	{
		return NativeDb.DatabaseInfoSvc.GetWrites(@this);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the source line number of a native database exception.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static int GetLineNumber(this Exception @this)
	{
		return NativeDb.DbExceptionSvc.GetExceptionLineNumber_(@this);
	}


	public static int GetPacketSize(this DbConnection @this)
	{
		return NativeDb.DatabaseEngineSvc.GetConnectionPacketSize_(@this);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the method name of a native database excerption.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static string GetProcedure(this Exception @this)
	{
		return NativeDb.DbExceptionSvc.GetExceptionProcedure_(@this);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the server name from a native database exception.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static string GetServer(this Exception @this)
	{
		return NativeDb.DbExceptionSvc.GetExceptionServer_(@this);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns the sql exception state of a native database exception.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static string GetState(this Exception @this)
	{
		return NativeDb.DbExceptionSvc.GetExceptionState_(@this);
	}




	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Checks if an exception is a native database network exception.
	/// </summary>
	// ---------------------------------------------------------------------------------
	internal static bool IsDbNetException(this Exception @this)
	{
		return NativeDb.DbExceptionSvc.IsDbNetException_(@this);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Checks if an exception is a native database exception.
	/// </summary>
	// ---------------------------------------------------------------------------------
	internal static bool IsSqlException(this Exception @this)
	{
		return NativeDb.DbExceptionSvc.IsSqlException_(@this);
	}



	/// <summary>
	/// Parses and converts a server version string to it's Version format.
	/// </summary>
	public static Version ParseServerVersion(this IDbConnection @this)
	{
		return NativeDb.DatabaseEngineSvc.ParseConnectionServerVersion_(@this);
	}

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Opens or verifies a connection. The Connection must exists.
	/// Throws an exception on failure.
	/// Always use this method to open connections because it disposes of the connection
	/// and invokes ConnectionChangedEvent on failure.
	/// Do not call before ensuring IsComplete.
	/// </summary>
	/// <returns>Boolean tuple with Item1: IsOpen and Item2: HasTransactions.</returns>
	// ---------------------------------------------------------------------------------
	public static (bool, bool) OpenOrVerify(this IDbConnection @this)
	{
		return NativeDb.DatabaseEngineSvc.OpenOrVerifyConnection_(@this);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Opens or verifies a connection. The Connection must exists.
	/// Throws an exception on failure.
	/// Do not call before ensuring IsComplete.
	/// The cardinal must be either zero, if no keepalive reads are required, or an
	/// incremental value that can be used to execute a select statement unique from
	/// the previous call to this method.
	/// </summary>
	/// <returns>
	/// Boolean tuple with Item1: True if open / verification succeeded and
	/// Item2: HasTransactions.
	/// </returns>
	// ---------------------------------------------------------------------------------
	public static async Task<(bool, bool)> OpenOrVerifyAsync(this IDbConnection @this,
		IDbTransaction transaction, CancellationToken cancelToken)
	{
		return await NativeDb.DatabaseEngineSvc.OpenOrVerifyConnectionAsync_(@this,
			transaction, cancelToken);
	}


	internal static bool HasTransactions(this IDbTransaction @this)
	{
		return NativeDb.DatabaseEngineSvc.HasTransactions_(@this);
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Checks if an exception is a native database exception.
	/// </summary>
	// ---------------------------------------------------------------------------------
	internal static bool HasSqlException(this Exception @this)
	{
		return NativeDb.DbExceptionSvc.HasSqlException_(@this);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Sets the database name in a native database exception.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static void SetDatabase(this Exception @this, string value)
	{
		NativeDb.DbExceptionSvc.SetExceptionDatabase_(@this, value);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Sets the server name in a native database exception.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static void SetServer(this Exception @this, string value)
	{
		NativeDb.DbExceptionSvc.SetExceptionServer_(@this, value);
	}


	public static async Task<DataTable> GetSchemaTableAsync(this IDataReader @this, CancellationToken cancelToken)
	{
		return await NativeDb.DatabaseEngineSvc.ReaderGetSchemaTableAsync_(@this, cancelToken);
	}

	public static async Task<bool> CloseAsync(this IDataReader @this, CancellationToken cancelToken)
	{
		return await NativeDb.DatabaseEngineSvc.ReaderCloseAsync_(@this, cancelToken);
	}

	public static async Task<bool> NextResultAsync(this IDataReader @this, CancellationToken cancelToken)
	{
		return await NativeDb.DatabaseEngineSvc.ReaderNextResultAsync_(@this, cancelToken);
	}

	public static async Task<bool> ReadAsync(this IDataReader @this, CancellationToken cancelToken)
	{
		return await NativeDb.DatabaseEngineSvc.ReaderReadAsync_(@this, cancelToken);
	}

}
