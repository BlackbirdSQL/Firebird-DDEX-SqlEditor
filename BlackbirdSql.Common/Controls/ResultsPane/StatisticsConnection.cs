// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ResultPane.StatisticsConnection

using System.Collections;

using BlackbirdSql.Common.Model.QueryExecution;

using FirebirdSql.Data.FirebirdClient;



namespace BlackbirdSql.Common.Controls.ResultsPane;


public class StatisticsConnection : CollectionBase
{
	private readonly FbConnection m_connection;

	public FbConnection InternalConnection => m_connection;

	public StatisticsTry this[int index]
	{
		get
		{
			return (StatisticsTry)List[index];
		}
		set
		{
			List[index] = value;
		}
	}

	public StatisticsConnection(FbConnection connection)
	{
		m_connection = connection;
	}

	public StatisticsConnection(StatisticsConnection value)
	{
		AddRange(value);
	}

	public StatisticsConnection(StatisticsTry[] value)
	{
		AddRange(value);
	}

	public int Add(StatisticsTry node)
	{
		return List.Add(node);
	}

	public void AddRange(StatisticsTry[] nodes)
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

	public void CopyTo(StatisticsTry[] array, int index)
	{
		List.CopyTo(array, index);
	}

	public int IndexOf(StatisticsTry node)
	{
		return List.IndexOf(node);
	}

	public void Insert(int index, StatisticsTry node)
	{
		List.Insert(index, node);
	}

	public void Remove(StatisticsTry node)
	{
		List.Remove(node);
	}
}
