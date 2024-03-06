// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ResultPane.StatisticsConnection

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using BlackbirdSql.Common.Model.QueryExecution;
using BlackbirdSql.Core;

using FirebirdSql.Data.FirebirdClient;


namespace BlackbirdSql.Common.Controls.ResultsPanels;

public class StatisticsConnection : CollectionBase
{
	public const int C_CategoryCount = 4;


	private readonly FbConnection _InternalConnection;

	private StatisticsSnapshotAgent _StatisticsConnectionBase = null;


	public FbConnection InternalConnection => _InternalConnection;

	public StatisticsSnapshotAgent StatisticsConnectionBase => _StatisticsConnectionBase;


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

	public void SetStatisticsBase(StatisticsSnapshotAgent agent)
	{
		_StatisticsConnectionBase = agent;
	}


	public void ResetStatistics()
	{
		_StatisticsConnectionBase = null;
	}
}
