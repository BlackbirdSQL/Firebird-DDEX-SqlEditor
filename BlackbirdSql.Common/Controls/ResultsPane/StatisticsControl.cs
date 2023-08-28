// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ResultPane.StatisticsControl

using System.Collections;

using BlackbirdSql.Common.Model.QueryExecution;




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

	public void RetrieveStatisticsIfNeeded()
	{
		/*
		IEnumerator enumerator = GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				StatisticsConnection statisticsConnection = (StatisticsConnection)enumerator.Current;
				statisticsConnection.Insert(0, new StatisticsTry(statisticsConnection.InternalConnection.RetrieveStatistics()));
				statisticsConnection.InternalConnection.ResetStatistics();
			}
		}
		finally
		{
			IDisposable disposable = enumerator as IDisposable;
			if (disposable != null)
			{
				disposable.Dispose();
			}
		}
		*/
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
