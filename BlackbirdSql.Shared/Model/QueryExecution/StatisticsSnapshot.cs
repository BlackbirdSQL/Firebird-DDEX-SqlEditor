// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ResultPane.StatisticsTry

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using BlackbirdSql.Shared.Events;
using BlackbirdSql.Sys.Model;
using Microsoft.VisualStudio.LanguageServer.Client;



namespace BlackbirdSql.Shared.Model.QueryExecution;



public class StatisticsSnapshot
{
	public StatisticsSnapshot(DbConnection connection, StatisticsSnapshot statisticsSnapShotBase = null)
	{
		_InternalConnection = connection;
		_StatisticsSnapShotBase = statisticsSnapShotBase;
	}



	private IDictionary _SnapshotData;

	private DateTime _TimeOfExecution;

	public IDictionary Snapshot => _SnapshotData;










	private readonly DbConnection _InternalConnection = null;
	private readonly StatisticsSnapshot _StatisticsSnapShotBase = null;


	// QueryProfileStatistics
	private long _SelectRowCount = 0L;
	private long _StatementCount = 0L;
	private long _InsRowEntities = -99L;
	private long _InsRowCount = -99L;
	private long _UpdRowEntities = -99L;
	private long _UpdRowCount = -99L;
	private long _DelRowEntities = -99L;
	private long _DelRowCount = -99L;
	private long _ReadIdxEntities = -99L;
	private long _ReadIdxCount = -99L;
	private long _ReadSeqEntities = -99L;
	private long _ReadSeqCount = -99L;
	private long _ExpungeEntities = -99L;
	private long _ExpungeCount = -99L;
	private long _PurgeEntities = -99L;
	private long _PurgeCount = -99L;

	private long _Transactions = 0L;

	// NetworkStatistics
	private long _ServerCacheReadCount = 0L;
	private long _ServerCacheWriteCount = 0L;
	private long _BufferCount = 0L;
	private long _ReadCount = 0L;
	private long _WriteCount = 0L;
	private long _PacketSize = 0L;

	// TimeStatistics
	private DateTime _ExecutionStartTime;
	private DateTime _ExecutionEndTime;

	// ServerStatistics
	private long _AllocationPages = 0L;
	private long _CurrentMemory = 0L;
	private long _MaxMemory = 0L;
	private long _DatabaseSizeInPages = 0L;
	private long _PageSize = 0L;
	private List<string> _ActiveUsers = null;



	public DbConnection InternalConnection => _InternalConnection;



	// QueryProfileStatistics
	public long IduRowCount => (_InsRowCount + _UpdRowCount + _DelRowCount) < 0 ? -99 : (_InsRowCount + _UpdRowCount + _DelRowCount);

	public long SelectRowCount => _SelectRowCount;
	public long StatementCount => _StatementCount;
	public long InsRowEntities => _InsRowEntities;
	public long InsRowCount => _InsRowCount;
	public long UpdRowEntities => _UpdRowEntities;
	public long UpdRowCount => _UpdRowCount;
	public long DelRowEntities => _DelRowEntities;
	public long DelRowCount => _DelRowCount;
	public long ReadIdxEntities => _ReadIdxEntities;
	public long ReadIdxCount => _ReadIdxCount;
	public long ReadSeqEntities => _ReadSeqEntities;
	public long ReadSeqCount => _ReadSeqCount;
	public long ExpungeEntities => _ExpungeEntities;
	public long ExpungeCount => _ExpungeCount;
	public long PurgeEntities => _PurgeEntities;
	public long PurgeCount => _PurgeCount;

	public long Transactions => _Transactions;

	// NetworkStatistics
	public long ServerCacheReadCount => _ServerCacheReadCount;
	public long ServerCacheWriteCount => _ServerCacheWriteCount;
	public long BufferCount => _BufferCount;
	public long ReadCount => _ReadCount;
	public long WriteCount => _WriteCount;
	public long PacketSize => _PacketSize;

	// TimeStatistics
	protected DateTime ExecutionStartTime => _ExecutionStartTime;
	protected DateTime ExecutionEndTime => _ExecutionEndTime;
	public long ExecutionStartTimeEpoch => _ExecutionStartTime.UnixMilliseconds();
	public long ExecutionEndTimeEpoch => _ExecutionEndTime.UnixMilliseconds();
	public long ExecutionTimeTicks => (_ExecutionEndTime - _ExecutionStartTime).Ticks;

	// ServerStatistics
	public long AllocationPages => _AllocationPages;
	public long CurrentMemory => _CurrentMemory;
	public long MaxMemory => _MaxMemory;
	public long DatabaseSizeInPages => _DatabaseSizeInPages;
	public long PageSize => _PageSize;
	public List<string> ActiveUsers => _ActiveUsers;
	public long ActiveUserCount => _ActiveUsers == null ? 0L : _ActiveUsers.Count;



	public DateTime TimeOfExecution => _TimeOfExecution;


	public void Load(QueryDataEventArgs args)
	{
		_SelectRowCount = args.TotalRowsSelected;
		_StatementCount = args.StatementCount;

		try
		{
			if (_InternalConnection.State != ConnectionState.Open)
			{
				try
				{
					// Evs.Debug(GetType(), "Load", $"DbConnection.Open:\nConnectionString: {_InternalConnection.ConnectionString}");
					_InternalConnection.OpenDb();
				}
				catch (Exception ex)
				{
					Diag.Expected(ex, $"\nConnectionString: {_InternalConnection?.ConnectionString}");
					throw;
				}
			}

			NativeDatabaseInfoProxy info = new(_InternalConnection);

			List<(long, long)> infoList = info.GetTablesDatabaseInfo();

			int i = 0;

			if (infoList != null)
			{

				_InsRowEntities = infoList[i].Item1 - (_StatisticsSnapShotBase != null ? _StatisticsSnapShotBase._InsRowEntities : 0);
				_InsRowCount = infoList[i++].Item2 - (_StatisticsSnapShotBase != null ? _StatisticsSnapShotBase._InsRowCount : 0);

				_UpdRowEntities = infoList[i].Item1 - (_StatisticsSnapShotBase != null ? _StatisticsSnapShotBase._UpdRowEntities : 0);
				_UpdRowCount = infoList[i++].Item2 - (_StatisticsSnapShotBase != null ? _StatisticsSnapShotBase._UpdRowCount : 0);

				_DelRowEntities = infoList[i].Item1 - (_StatisticsSnapShotBase != null ? _StatisticsSnapShotBase._DelRowEntities : 0);
				_DelRowCount = infoList[i++].Item2 - (_StatisticsSnapShotBase != null ? _StatisticsSnapShotBase._DelRowCount : 0);

				_ReadIdxEntities = infoList[i].Item1 - (_StatisticsSnapShotBase != null ? _StatisticsSnapShotBase._ReadIdxEntities : 0);
				_ReadIdxCount = infoList[i++].Item2 - (_StatisticsSnapShotBase != null ? _StatisticsSnapShotBase._ReadIdxCount : 0);

				_ReadSeqEntities = infoList[i].Item1 - (_StatisticsSnapShotBase != null ? _StatisticsSnapShotBase._ReadSeqEntities : 0);
				_ReadSeqCount = infoList[i++].Item2 - (_StatisticsSnapShotBase != null ? _StatisticsSnapShotBase._ReadSeqCount : 0);

				_ExpungeEntities = infoList[i].Item1 - (_StatisticsSnapShotBase != null ? _StatisticsSnapShotBase._ExpungeEntities : 0);
				_ExpungeCount = infoList[i++].Item2 - (_StatisticsSnapShotBase != null ? _StatisticsSnapShotBase._ExpungeCount : 0);

				_PurgeEntities = infoList[i].Item1 - (_StatisticsSnapShotBase != null ? _StatisticsSnapShotBase._PurgeEntities : 0);
				_PurgeCount = infoList[i++].Item2 - (_StatisticsSnapShotBase != null ? _StatisticsSnapShotBase._PurgeCount : 0);
			}

			_Transactions = info.GetActiveTransactionsCount(); // - (_StatisticsSnapShotBase != null ? _StatisticsSnapShotBase._Transactions : 0);


			// NetworkStatistics
			_ServerCacheReadCount = info.GetServerCacheReadsCount() - (_StatisticsSnapShotBase != null ? _StatisticsSnapShotBase._ServerCacheReadCount : 0);
			_ServerCacheWriteCount = info.GetServerCacheWritesCount() - (_StatisticsSnapShotBase != null ? _StatisticsSnapShotBase._ServerCacheWriteCount : 0);
			_BufferCount = info.GetNumBuffers();
			_ReadCount = info.GetReads() - (_StatisticsSnapShotBase != null ? _StatisticsSnapShotBase._ReadCount : 0);
			_WriteCount = info.GetWrites() - (_StatisticsSnapShotBase != null ? _StatisticsSnapShotBase._WriteCount : 0);

			_PacketSize = _InternalConnection.GetPacketSize();

			// TimeStatistics
			_ExecutionStartTime = args.ExecutionStartTime ?? DateTime.Now;
			_ExecutionEndTime = (DateTime)args.ExecutionEndTime;

			// ServerStatistics
			_AllocationPages = info.GetAllocationPages();
			_CurrentMemory = info.GetCurrentMemory();
			_MaxMemory = info.GetMaxMemory();
			_DatabaseSizeInPages = info.GetDatabaseSizeInPages();
			_PageSize = info.GetPageSize();
			_ActiveUsers = info.GetActiveUsers();
		}
		catch (Exception ex)
		{
			Diag.Ex(ex);
			throw ex;
		}


		Dictionary<string, object> stats = new(35)
		{
			{ "SelectRowCount", SelectRowCount },
			{ "StatementCount", StatementCount },
			{ "IduRowCount", IduRowCount },
			{ "InsRowEntities", InsRowEntities },
			{ "InsRowCount", InsRowCount },
			{ "UpdRowEntities", UpdRowEntities },
			{ "UpdRowCount", UpdRowCount },
			{ "DelRowEntities", DelRowEntities },
			{ "DelRowCount", DelRowCount },
			{ "ReadIdxEntities", ReadIdxEntities },
			{ "ReadIdxCount", ReadIdxCount },
			{ "ReadSeqEntities", ReadSeqEntities },
			{ "ReadSeqCount", ReadSeqCount },
			{ "ExpungeEntities", ExpungeEntities },
			{ "ExpungeCount", ExpungeCount },
			{ "PurgeEntities", PurgeEntities },
			{ "PurgeCount", PurgeCount },
			{ "Transactions", Transactions },
			{ "ServerCacheReadCount", ServerCacheReadCount },
			{ "ServerCacheWriteCount", ServerCacheWriteCount },
			{ "BufferCount", BufferCount },
			{ "ReadCount", ReadCount },
			{ "WriteCount", WriteCount },
			{ "PacketSize", PacketSize },
			{ "ExecutionStartTimeEpoch", ExecutionStartTimeEpoch },
			{ "ExecutionEndTimeEpoch", ExecutionEndTimeEpoch },
			{ "ExecutionTimeTicks", ExecutionTimeTicks },
			{ "AllocationPages", AllocationPages },
			{ "CurrentMemory", CurrentMemory },
			{ "MaxMemory", MaxMemory },
			{ "DatabaseSizeInPages", DatabaseSizeInPages },
			{ "PageSize", PageSize },
			{ "ActiveUserCount", ActiveUserCount }
		};

		_SnapshotData = stats;
		_TimeOfExecution = DateTime.Now;

	}

	public static T ConvertValue<T>(object value) => value is IConvertible ? (T)Convert.ChangeType(value, typeof(T)) : (T)value;


}
