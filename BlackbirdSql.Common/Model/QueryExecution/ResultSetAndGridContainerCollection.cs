// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution.ResultSetAndGridContainerCollection

using System.Collections;

// using Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution;




namespace BlackbirdSql.Common.Model.QueryExecution;


public class ResultSetAndGridContainerCollection : CollectionBase
{
	public ResultSetAndGridContainer this[int index]
	{
		get
		{
			return (ResultSetAndGridContainer)List[index];
		}
		set
		{
			List[index] = value;
		}
	}

	public ResultSetAndGridContainerCollection()
	{
	}

	public ResultSetAndGridContainerCollection(ResultSetAndGridContainerCollection value)
	{
		AddRange(value);
	}

	public ResultSetAndGridContainerCollection(ResultSetAndGridContainer[] value)
	{
		AddRange(value);
	}

	public int Add(ResultSetAndGridContainer node)
	{
		return List.Add(node);
	}

	public void AddRange(ResultSetAndGridContainer[] nodes)
	{
		for (int i = 0; i < nodes.Length; i++)
		{
			Add(nodes[i]);
		}
	}

	public void AddRange(ResultSetAndGridContainerCollection value)
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

	public void CopyTo(ResultSetAndGridContainer[] array, int index)
	{
		List.CopyTo(array, index);
	}

	public int IndexOf(ResultSetAndGridContainer node)
	{
		return List.IndexOf(node);
	}

	public void Insert(int index, ResultSetAndGridContainer node)
	{
		List.Insert(index, node);
	}

	public void Remove(ResultSetAndGridContainer node)
	{
		List.Remove(node);
	}
}
