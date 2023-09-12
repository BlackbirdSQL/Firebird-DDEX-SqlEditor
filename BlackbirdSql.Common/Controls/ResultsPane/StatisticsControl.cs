// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ResultPane.StatisticsControl

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

using BlackbirdSql.Common.Model.QueryExecution;
using BlackbirdSql.Core;
using Microsoft.SqlServer.Management.SqlParser.MetadataProvider;


namespace BlackbirdSql.Common.Controls.ResultsPane;

public class StatisticsControl : CollectionBase
{
	private string m_Text;


	public string Text
	{
		get
		{
			return m_Text;
		}
		set
		{
			m_Text = value;
		}
	}



	public StatisticsConnection this[int index]
	{
		get
		{
			return (StatisticsConnection)List[index];
		}
		set
		{
			List[index] = value;
		}
	}

	public StatisticsControl()
	{
	}


	public void LoadStatisticsSnapshotBase(QueryManager qryMgr)
	{
		try
		{
			StatisticsConnection conn = this[List.Count - 1];

			StatisticsSnapshotAgent agent = new(conn.InternalConnection);

			agent.Load(qryMgr, 0, 0, DateTime.Now);

			conn.SetStatisticsBase(agent);

		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}
	}



	public void RetrieveStatisticsIfNeeded(QueryManager qryMgr, long recordCount, long recordsAffected, DateTime executionEndTime)
	{
		IEnumerator enumerator = GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				StatisticsConnection conn = (StatisticsConnection)enumerator.Current;
				StatisticsSnapshotAgent agent = new(conn.InternalConnection, conn.StatisticsConnectionBase);

				agent.Load(qryMgr, recordCount, recordsAffected, executionEndTime);


				Dictionary<string, object> stats = new(25)
				{
					{ "IduRowCount", agent.IduRowCount },
					{ "InsRowCount", agent.InsRowCount },
					{ "UpdRowCount", agent.UpdRowCount },
					{ "DelRowCount", agent.DelRowCount },
					{ "SelectRowCount", agent.SelectRowCount },
					{ "Transactions", agent.Transactions },
					{ "ServerRoundtrips", agent.ServerRoundtrips },
					{ "BufferCount", agent.BufferCount },
					{ "ReadCount", agent.ReadCount },
					{ "WriteCount", agent.WriteCount },
					{ "ReadIdxCount", agent.ReadIdxCount },
					{ "ReadSeqCount", agent.ReadSeqCount },
					{ "PurgeCount", agent.PurgeCount },
					{ "ExpungeCount", agent.ExpungeCount },
					{ "Marks", agent.Marks },
					{ "PacketSize", agent.PacketSize },
					{ "ExecutionStartTimeEpoch", agent.ExecutionStartTimeEpoch },
					{ "ExecutionEndTimeEpoch", agent.ExecutionEndTimeEpoch },
					{ "ExecutionTimeTicks", agent.ExecutionTimeTicks },
					{ "AllocationPages", agent.AllocationPages },
					{ "CurrentMemory", agent.CurrentMemory },
					{ "MaxMemory", agent.MaxMemory },
					{ "DatabaseSizeInPages", agent.DatabaseSizeInPages },
					{ "PageSize", agent.PageSize },
					{ "ActiveUserCount", agent.ActiveUserCount }
				};


				conn.Insert(0, new StatisticsSnapshot(stats));

				conn.ResetStatistics();
			}
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}
		finally
		{
			if (enumerator is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}
	}

	public StatisticsControl(StatisticsControl value)
	{
		AddRange(value);
	}

	public StatisticsControl(StatisticsConnection[] value)
	{
		AddRange(value);
	}

	public int Add(StatisticsConnection node)
	{
		return List.Add(node);
	}

	public void AddRange(StatisticsConnection[] nodes)
	{
		for (int i = 0; i < nodes.Length; i++)
		{
			Add(nodes[i]);
		}
	}

	public void AddRange(StatisticsControl value)
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

	public void CopyTo(StatisticsConnection[] array, int index)
	{
		List.CopyTo(array, index);
	}

	public int IndexOf(StatisticsConnection node)
	{
		return List.IndexOf(node);
	}




	public void Insert(int index, StatisticsConnection node)
	{
		List.Insert(index, node);
	}

	public void Remove(StatisticsConnection node)
	{
		List.Remove(node);
	}
}
