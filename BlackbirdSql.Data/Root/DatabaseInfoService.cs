
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using BlackbirdSql.Data.Model;
using BlackbirdSql.Sys.Interfaces;
using BlackbirdSql.Sys.Model;
using FirebirdSql.Data.FirebirdClient;



namespace BlackbirdSql.Data;


// =========================================================================================================
//											Firebird DatabaseInfoService Class
//
/// <summary>
/// Central class for database specific class methods. The intention is ultimately provide these as a
/// service so that BlackbirdSql can provide support for additional database engines.
/// </summary>
// =========================================================================================================
internal class DatabaseInfoService : SBsNativeDatabaseInfo, IBsNativeDatabaseInfo
{
	private DatabaseInfoService()
	{
	}



	internal static IBsNativeDatabaseInfo EnsureInstance() => _Instance ??= new DatabaseInfoService();


	internal static IBsNativeDatabaseInfo _Instance = null;




	public long GetActiveTransactionsCount(NativeDatabaseInfoProxy @this)
	{
		return ((FbDatabaseInfo)@this.NativeObject).GetActiveTransactionsCount();
	}

	public List<string> GetActiveUsers(NativeDatabaseInfoProxy @this)
	{
		return ((FbDatabaseInfo)@this.NativeObject).GetActiveUsers();
	}



	public long GetAllocationPages(NativeDatabaseInfoProxy @this)
	{
		return ((FbDatabaseInfo)@this.NativeObject).GetAllocationPages();
	}

	public long GetCurrentMemory(NativeDatabaseInfoProxy @this)
	{
		return ((FbDatabaseInfo)@this.NativeObject).GetCurrentMemory();
	}

	public long GetDatabaseSizeInPages(NativeDatabaseInfoProxy @this)
	{
		return ((FbDatabaseInfo)@this.NativeObject).GetDatabaseSizeInPages();
	}

	public long GetDeleteCount(NativeDatabaseInfoProxy @this)
	{
		return (long)Reflect.InvokeGenericMethod<long>(@this.NativeObject, "GetValue", BindingFlags.Default, [(byte)IscCodes.isc_info_delete_count]);
	}

	public List<(long, long)> GetTablesDatabaseInfo(NativeDatabaseInfoProxy @this)
	{
		FbDatabaseInfo dbInfo = (FbDatabaseInfo)@this.NativeObject;

		FbConnection connection = dbInfo.Connection;
		if (connection == null)
		{
			ArgumentNullException ex = new("DatabaseInfo connection is null.");
			Diag.Ex(ex);
			throw ex;
		}

		object innerConnection = Reflect.GetPropertyValue(connection, "InnerConnection");
		if (innerConnection == null)
		{
			ArgumentNullException ex = new("DatabaseInfo InnerConnection is null.");
			Diag.Ex(ex);
			throw ex;
		}

		object database = Reflect.GetPropertyValue(innerConnection, "Database", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (database == null)
		{
			ArgumentNullException ex = new("DatabaseInfo InnerConnection.Database is null.");
			Diag.Ex(ex);
			throw ex;
		}


		byte[] iscValues =
			[
				Convert.ToByte(IscCodes.isc_info_insert_count), // 25
				Convert.ToByte(IscCodes.isc_info_update_count), // 26
				Convert.ToByte(IscCodes.isc_info_delete_count), // 27
				Convert.ToByte(IscCodes.isc_info_read_idx_count), // 24
				Convert.ToByte(IscCodes.isc_info_read_seq_count), // 23
				Convert.ToByte(IscCodes.isc_info_expunge_count), // 30
				Convert.ToByte(IscCodes.isc_info_purge_count) // 29
			];

		List<(long, long)> results = [];

		ulong pairCount;
		int offset;
		uint dataLen;
		ulong result;


		foreach (byte brequest in iscValues)
		{
			byte[] requestBuffer = [brequest, Convert.ToByte(IscCodes.isc_info_end)];
			byte[] responseBuffer = new byte[IscCodes.DEFAULT_MAX_BUFFER_SIZE];

			Reflect.InvokeMethodBaseType(database, "DatabaseInfo", 4,
				BindingFlags.Default, [requestBuffer, responseBuffer, responseBuffer.Length]);

			dataLen = BitConverter.ToUInt16(responseBuffer, 1);
			pairCount = 0;
			result = 0;

			if (dataLen == 0)
			{
				// Evs.Trace(typeof(DbNativeExtensionMembers), nameof(GetDatabaseInfo), "Request: {0}, Entities affected: {1}, Value: {2}.", brequest, pairCount, result);

				results.Add(((long, long))(pairCount, result));

				continue;
			}

			pairCount = dataLen / 6;
			offset = 0;

			for (ulong i = 0; i < pairCount; i++)
			{
				offset = 5 + ((int)i * 6);
				result += BitConverter.ToUInt32(responseBuffer, offset);
			}

			results.Add(((long, long))(pairCount, result));

			// Evs.Trace(typeof(DbNativeExtensionMembers), nameof(GetDatabaseInfo), "Request: {0}, Entities affected: {1}, Value: {2}.", brequest, pairCount, result);
		}

		return results;
	}


	public long GetExpungeCount(NativeDatabaseInfoProxy @this)
	{
		return ((FbDatabaseInfo)@this.NativeObject).GetExpungeCount();
	}

	public long GetServerCacheReadsCount(NativeDatabaseInfoProxy @this)
	{
		return ((FbDatabaseInfo)@this.NativeObject).GetFetches();
	}


	public long GetInsertCount(NativeDatabaseInfoProxy @this)
	{
		return ((FbDatabaseInfo)@this.NativeObject).GetInsertCount();
	}

	public long GetServerCacheWritesCount(NativeDatabaseInfoProxy @this)
	{
		return ((FbDatabaseInfo)@this.NativeObject).GetMarks();
	}


	public long GetMaxMemory(NativeDatabaseInfoProxy @this)
	{
		return ((FbDatabaseInfo)@this.NativeObject).GetMaxMemory();
	}

	public long GetPageSize(NativeDatabaseInfoProxy @this)
	{
		return ((FbDatabaseInfo)@this.NativeObject).GetPageSize();
	}


	public long GetNumBuffers(NativeDatabaseInfoProxy @this)
	{
		return ((FbDatabaseInfo)@this.NativeObject).GetNumBuffers();
	}


	public long GetPurgeCount(NativeDatabaseInfoProxy @this)
	{
		return (long)Reflect.InvokeGenericMethod<long>(@this.NativeObject, "GetValue", BindingFlags.Default, [(byte)IscCodes.isc_info_purge_count]);
	}


	public long GetReadIdxCount(NativeDatabaseInfoProxy @this)
	{
		return ((FbDatabaseInfo)@this.NativeObject).GetReadIdxCount();
	}


	public long GetReads(NativeDatabaseInfoProxy @this)
	{
		return ((FbDatabaseInfo)@this.NativeObject).GetReads();
	}


	public long GetReadSeqCount(NativeDatabaseInfoProxy @this)
	{
		return ((FbDatabaseInfo)@this.NativeObject).GetReadSeqCount();
	}


	public long GetUpdateCount(NativeDatabaseInfoProxy @this)
	{
		return ((FbDatabaseInfo)@this.NativeObject).GetUpdateCount();
	}


	public long GetWrites(NativeDatabaseInfoProxy @this)
	{
		return ((FbDatabaseInfo)@this.NativeObject).GetWrites();
	}

}
