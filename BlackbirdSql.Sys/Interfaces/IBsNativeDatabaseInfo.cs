// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Runtime.InteropServices;
using BlackbirdSql.Sys.Model;



namespace BlackbirdSql.Sys.Interfaces;

[Guid(LibraryData.C_NativeDatabaseInfoServiceGuid)]


// =========================================================================================================
//										IBsNativeDatabaseInfo Interface
//
/// <summary>
/// Interface for native DatabaseInfo extension methods service.
/// </summary>
// =========================================================================================================
internal interface IBsNativeDatabaseInfo
{
	long GetActiveTransactionsCount(NativeDatabaseInfoProxy @this);
	List<string> GetActiveUsers(NativeDatabaseInfoProxy @this);
	long GetAllocationPages(NativeDatabaseInfoProxy @this);
	long GetCurrentMemory(NativeDatabaseInfoProxy @this);
	long GetDatabaseSizeInPages(NativeDatabaseInfoProxy @this);
	long GetDeleteCount(NativeDatabaseInfoProxy @this);
	List<(long, long)> GetTablesDatabaseInfo(NativeDatabaseInfoProxy @this);
	long GetExpungeCount(NativeDatabaseInfoProxy @this);
	long GetServerCacheReadsCount(NativeDatabaseInfoProxy @this);
	long GetInsertCount(NativeDatabaseInfoProxy @this);
	long GetServerCacheWritesCount(NativeDatabaseInfoProxy @this);
	long GetMaxMemory(NativeDatabaseInfoProxy @this);
	long GetPageSize(NativeDatabaseInfoProxy @this);
	long GetNumBuffers(NativeDatabaseInfoProxy @this);
	long GetPurgeCount(NativeDatabaseInfoProxy @this);
	long GetReadIdxCount(NativeDatabaseInfoProxy @this);
	long GetReads(NativeDatabaseInfoProxy @this);
	long GetReadSeqCount(NativeDatabaseInfoProxy @this);
	long GetUpdateCount(NativeDatabaseInfoProxy @this);
	long GetWrites(NativeDatabaseInfoProxy @this);
}