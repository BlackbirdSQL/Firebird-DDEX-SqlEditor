// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ResultPane.StatisticsGridsCollection

using System.Collections;
using BlackbirdSql.Common.Model.QueryExecution;

namespace BlackbirdSql.Common.Controls.ResultsPane;


public class StatisticsGridsCollection : CollectionBase
{
	public StatisticsDlgGridControl this[int index]
	{
		get
		{
			return (StatisticsDlgGridControl)List[index];
		}
		set
		{
			List[index] = value;
		}
	}

	public StatisticsGridsCollection()
	{
	}

	public StatisticsGridsCollection(StatisticsGridsCollection value)
	{
		AddRange(value);
	}

	public StatisticsGridsCollection(StatisticsDlgGridControl[] value)
	{
		AddRange(value);
	}

	public int Add(StatisticsDlgGridControl node)
	{
		return List.Add(node);
	}

	public void AddRange(StatisticsDlgGridControl[] nodes)
	{
		for (int i = 0; i < nodes.Length; i++)
		{
			Add(nodes[i]);
		}
	}

	public void AddRange(StatisticsGridsCollection value)
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

	public void CopyTo(StatisticsDlgGridControl[] array, int index)
	{
		List.CopyTo(array, index);
	}

	public int IndexOf(StatisticsDlgGridControl node)
	{
		return List.IndexOf(node);
	}

	public void Insert(int index, StatisticsDlgGridControl node)
	{
		List.Insert(index, node);
	}

	public void Remove(StatisticsDlgGridControl node)
	{
		List.Remove(node);
	}
}
