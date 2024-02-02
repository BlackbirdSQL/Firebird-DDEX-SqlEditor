// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ResultPane.StatisticsConnection

using System;
using System.Collections.Generic;
using BlackbirdSql.Common.Model.QueryExecution;
using BlackbirdSql.Core;
using FirebirdSql.Data.FirebirdClient;


namespace BlackbirdSql.Common.Controls.ResultsPane;

public class StatisticsSnapshotAgent(FbConnection connection, StatisticsSnapshotAgent statisticsSnapShotBase = null)
{
	private readonly FbConnection _InternalConnection = connection;
	private readonly StatisticsSnapshotAgent _StatisticsSnapShotBase = statisticsSnapShotBase;


	// QueryProfileStatistics
	private long _IduRowCount = -1L;
	private long _InsRowCount = 0L;
	private long _UpdRowCount = 0L;
	private long _DelRowCount = 0L;
	private long _SelectRowCount = 0L;
	private long _Transactions = 0L;

	// NetworkStatistics
	private long _ServerRoundtrips = 0L;
	private long _BufferCount = 0L;
	private long _ReadCount = 0L;
	private long _WriteCount = 0L;
	private long _ReadIdxCount = 0L;
	private long _ReadSeqCount = 0L;
	private long _PurgeCount = 0L;
	private long _ExpungeCount = 0L;
	private long _Marks = 0L;
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



	public FbConnection InternalConnection => _InternalConnection;



	// QueryProfileStatistics
	public long IduRowCount => (_InsRowCount + _UpdRowCount + _DelRowCount) > 0
		? _InsRowCount + _UpdRowCount + _DelRowCount
		: _IduRowCount;
	public long InsRowCount => _InsRowCount;
	public long UpdRowCount => _UpdRowCount;
	public long DelRowCount => _DelRowCount;
	public long SelectRowCount => _SelectRowCount;
	public long Transactions => _Transactions;

	// NetworkStatistics
	public long ServerRoundtrips => _ServerRoundtrips;
	public long BufferCount => _BufferCount;
	public long ReadCount => _ReadCount;
	public long WriteCount => _WriteCount;
	public long ReadIdxCount => _ReadIdxCount;
	public long ReadSeqCount => _ReadSeqCount;
	public long PurgeCount => _PurgeCount;
	public long ExpungeCount => _ExpungeCount;
	public long Marks => _Marks;
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

	public void Load(QueryManager qryMgr, long rowCount, long recordsAffected, DateTime executionEndTime)
	{
		_SelectRowCount = rowCount;
		_IduRowCount = recordsAffected;



		try
		{
			FbDatabaseInfo info = new(_InternalConnection);

			_InsRowCount = info.GetInsertCount();
			_UpdRowCount = info.GetUpdateCount();
			_DelRowCount = info.GetDeleteCount();
			_Transactions = info.GetActiveTransactionsCount();

			// NetworkStatistics
			_ServerRoundtrips = info.GetFetches() - (_StatisticsSnapShotBase != null ? _StatisticsSnapShotBase._ServerRoundtrips : 0);
			_BufferCount = info.GetNumBuffers();
			_ReadCount = info.GetReads() - (_StatisticsSnapShotBase != null ? _StatisticsSnapShotBase._ReadCount : 0);
			_WriteCount = info.GetWrites() - (_StatisticsSnapShotBase != null ? _StatisticsSnapShotBase._WriteCount : 0);

			_ReadIdxCount = info.GetReadIdxCount() - (_StatisticsSnapShotBase != null ? _StatisticsSnapShotBase._ReadIdxCount : 0);
			_ReadSeqCount = info.GetReadSeqCount() - (_StatisticsSnapShotBase != null ? _StatisticsSnapShotBase._ReadSeqCount : 0);
			// _ReadIdxCount = 0 - (_StatisticsSnapShotBase != null ? _StatisticsSnapShotBase._ReadIdxCount : 0);
			// _ReadSeqCount = 0 - (_StatisticsSnapShotBase != null ? _StatisticsSnapShotBase._ReadSeqCount : 0);

			_PurgeCount = info.GetPurgeCount() - (_StatisticsSnapShotBase != null ? _StatisticsSnapShotBase._PurgeCount : 0);
			_ExpungeCount = info.GetExpungeCount() - (_StatisticsSnapShotBase != null ? _StatisticsSnapShotBase._ExpungeCount : 0);
			_Marks = info.GetMarks() - (_StatisticsSnapShotBase != null ? _StatisticsSnapShotBase._Marks : 0);
			_PacketSize = _InternalConnection.PacketSize;

			// TimeStatistics
			_ExecutionEndTime = executionEndTime;
			_ExecutionStartTime = qryMgr.QueryExecutionStartTime ?? DateTime.Now;

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
			Diag.Dug(ex);
			throw ex;
		}

	}

	public static T ConvertValue<T>(object value) => value is IConvertible ? (T)Convert.ChangeType(value, typeof(T)) : (T)value;

}
