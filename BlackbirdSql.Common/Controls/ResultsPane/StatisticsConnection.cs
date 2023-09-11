// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ResultPane.StatisticsConnection

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Windows.Documents;
using BlackbirdSql.Common.Controls.Grid;
using BlackbirdSql.Common.Model.QueryExecution;
using BlackbirdSql.Common.Properties;
using BlackbirdSql.Core;
using FirebirdSql.Data.FirebirdClient;
using Newtonsoft.Json.Linq;

namespace BlackbirdSql.Common.Controls.ResultsPane;


public class StatisticsConnection : CollectionBase
{
	public const int C_CategoryCount = 4;


	private readonly FbConnection _InternalConnection;

	private bool _Loaded = false;

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
	public bool Loaded => _Loaded;


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
	public long ExecutionStartTimeEpoch => _ExecutionStartTime.ToUnixMilliseconds();
	public long ExecutionEndTimeEpoch => _ExecutionEndTime.ToUnixMilliseconds();
	public long ExecutionTimeTicks => (_ExecutionEndTime - _ExecutionStartTime).Ticks;

	// ServerStatistics
	public long AllocationPages => _AllocationPages;
	public long CurrentMemory => _CurrentMemory;
	public long MaxMemory => _MaxMemory;
	public long DatabaseSizeInPages => _DatabaseSizeInPages;
	public long PageSize => _PageSize;
	public List<string> ActiveUsers => _ActiveUsers;
	public long ActiveUserCount => _ActiveUsers == null ? 0L : _ActiveUsers.Count;



	public StatisticsSnapshot this[int index]
	{
		get
		{
			return (StatisticsSnapshot)List[index];
		}
		set
		{
			List[index] = value;
		}
	}

	public StatisticsConnection(FbConnection connection)
	{
		_InternalConnection = connection;
	}

	public void Load(QueryManager qryMgr, IDbCommand command, long rowCount, long recordsAffected, DateTime executionEndTime)
	{
		// if (_Loaded)
		//	return;

		try
		{
			FbDatabaseInfo info = new(_InternalConnection);

			// QueryProfileStatistics
			_IduRowCount = recordsAffected;
			_InsRowCount = info.GetInsertCount();
			_UpdRowCount = info.GetUpdateCount();
			_DelRowCount = info.GetDeleteCount();
			_SelectRowCount = rowCount;
			_Transactions = info.GetActiveTransactionsCount();

			// NetworkStatistics
			_ServerRoundtrips = info.GetFetches();
			_BufferCount = info.GetNumBuffers();
			_ReadCount = info.GetReads();
			_WriteCount = info.GetWrites();
			_ReadIdxCount = info.GetReadIdxCount();
			_ReadSeqCount = info.GetReadSeqCount();
			_PurgeCount = info.GetPurgeCount();
			_ExpungeCount = info.GetExpungeCount();
			_Marks = info.GetMarks();
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


			_Loaded = true;
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}

	}

	public StatisticsConnection(StatisticsConnection value)
	{
		AddRange(value);
	}

	public StatisticsConnection(StatisticsSnapshot[] value)
	{
		AddRange(value);
	}

	public int Add(StatisticsSnapshot node)
	{
		return List.Add(node);
	}

	public void AddRange(StatisticsSnapshot[] nodes)
	{
		for (int i = 0; i < nodes.Length; i++)
		{
			Add(nodes[i]);
		}
	}

	public void AddRange(StatisticsConnection value)
	{
		for (int i = 0; i < value.Count; i++)
		{
			Add(value[i]);
		}
	}

	public bool Contains(ResultSetAndGridContainer node)
	{
		return List.Contains(node);
	}

	public void CopyTo(StatisticsSnapshot[] array, int index)
	{
		List.CopyTo(array, index);
	}

	public int IndexOf(StatisticsSnapshot node)
	{
		return List.IndexOf(node);
	}


	public void Insert(int index, StatisticsSnapshot node)
	{
		List.Insert(index, node);
	}

	public void Remove(StatisticsSnapshot node)
	{
		List.Remove(node);
	}
}
