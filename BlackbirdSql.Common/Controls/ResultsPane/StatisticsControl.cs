// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ResultPane.StatisticsControl

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using BlackbirdSql.Common.Model.QueryExecution;
using BlackbirdSql.Core;


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

	public void RetrieveStatisticsIfNeeded(QueryManager qryMgr, IDbCommand command, long recordCount, long recordsAffected, DateTime executionEndTime)
	{
		IEnumerator enumerator = GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				StatisticsConnection conn = (StatisticsConnection)enumerator.Current;

				// if (!conn.Loaded)
				conn.Load(qryMgr, command, recordCount, recordsAffected, executionEndTime);


				Dictionary<string, object> stats = new(25)
				{
					{ "IduRowCount", conn.IduRowCount },
					{ "InsRowCount", conn.InsRowCount },
					{ "UpdRowCount", conn.UpdRowCount },
					{ "DelRowCount", conn.DelRowCount },
					{ "SelectRowCount", conn.SelectRowCount },
					{ "Transactions", conn.Transactions },
					{ "ServerRoundtrips", conn.ServerRoundtrips },
					{ "BufferCount", conn.BufferCount },
					{ "ReadCount", conn.ReadCount },
					{ "WriteCount", conn.WriteCount },
					{ "ReadIdxCount", conn.ReadIdxCount },
					{ "ReadSeqCount", conn.ReadSeqCount },
					{ "PurgeCount", conn.PurgeCount },
					{ "ExpungeCount", conn.ExpungeCount },
					{ "Marks", conn.Marks },
					{ "PacketSize", conn.PacketSize },
					{ "ExecutionStartTimeEpoch", conn.ExecutionStartTimeEpoch },
					{ "ExecutionEndTimeEpoch", conn.ExecutionEndTimeEpoch },
					{ "ExecutionTimeTicks", conn.ExecutionTimeTicks },
					{ "AllocationPages", conn.AllocationPages },
					{ "CurrentMemory", conn.CurrentMemory },
					{ "MaxMemory", conn.MaxMemory },
					{ "DatabaseSizeInPages", conn.DatabaseSizeInPages },
					{ "PageSize", conn.PageSize },
					{ "ActiveUserCount", conn.ActiveUserCount }
				};


				conn.Insert(0, new StatisticsSnapshot(stats));

				// statisticsConnection.InternalConnection.ResetStatistics();
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
