#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System.Collections;

// using Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution;
// using Ns = Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution;


namespace BlackbirdSql.Common.Model.QueryExecution;
// namespace Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution


public class QESQLBatchCollection : CollectionBase
{
	public QESQLBatch this[int index]
	{
		get
		{
			return (QESQLBatch)List[index];
		}
		set
		{
			List[index] = value;
		}
	}

	public QESQLBatchCollection()
	{
	}

	public QESQLBatchCollection(QESQLBatchCollection value)
	{
		AddRange(value);
	}

	public QESQLBatchCollection(QESQLBatch[] value)
	{
		AddRange(value);
	}

	public int Add(QESQLBatch node)
	{
		return List.Add(node);
	}

	public void AddRange(QESQLBatch[] nodes)
	{
		for (int i = 0; i < nodes.Length; i++)
		{
			Add(nodes[i]);
		}
	}

	public void AddRange(QESQLBatchCollection value)
	{
		for (int i = 0; i < value.Count; i++)
		{
			Add(value[i]);
		}
	}

	public bool Contains(QESQLBatch node)
	{
		return List.Contains(node);
	}

	public void CopyTo(QESQLBatch[] array, int index)
	{
		List.CopyTo(array, index);
	}

	public int IndexOf(QESQLBatch node)
	{
		return List.IndexOf(node);
	}

	public void Insert(int index, QESQLBatch node)
	{
		List.Insert(index, node);
	}

	public void Remove(QESQLBatch node)
	{
		List.Remove(node);
	}
}
