// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.NodeCollection<T>
using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.AnalysisServices.Graphing;


namespace BlackbirdSql.Common.Controls.Graphing;

public class NodeCollection<T> : ICollection<T>, IEnumerable<T>, IEnumerable where T : Microsoft.AnalysisServices.Graphing.Node
{
	private readonly T node;

	public T Owner => node;

	public T this[int index]
	{
		get
		{
			int num = 0;
			INodeEnumerator children = ((INode)node).Children;
			while (children.MoveNext())
			{
				if (num++ == index)
				{
					return (T)children.Current;
				}
			}
			throw new ArgumentOutOfRangeException("index");
		}
	}

	public int Count => node.CnodeChildren();

	public virtual bool IsReadOnly => true;

	public NodeCollection(T node)
	{
		this.node = node;
	}

	public T GetPrevious(T item)
	{
		INode node = null;
		INodeEnumerator children = ((INode)this.node).Children;
		while (children.MoveNext())
		{
			if (children.Current.Equals(item))
			{
				return (T)node;
			}
			node = children.Current;
		}
		return null;
	}

	public T GetNext(T item)
	{
		INodeEnumerator children = ((INode)node).Children;
		while (children.MoveNext())
		{
			if (children.Current.Equals(item) && children.MoveNext())
			{
				return (T)children.Current;
			}
		}
		return null;
	}

	public IEnumerator<T> GetEnumerator()
	{
		INodeEnumerator enumerator = ((INode)node).Children;
		while (enumerator.MoveNext())
		{
			yield return (T)enumerator.Current;
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		INodeEnumerator enumerator = ((INode)node).Children;
		while (enumerator.MoveNext())
		{
			yield return (T)enumerator.Current;
		}
	}

	public virtual void Add(T child)
	{
		throw new NotSupportedException();
	}

	public bool Contains(T item)
	{
		INodeEnumerator children = ((INode)node).Children;
		while (children.MoveNext())
		{
			if (children.Current.Equals(item))
			{
				return true;
			}
		}
		return false;
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		INodeEnumerator children = ((INode)node).Children;
		while (children.MoveNext())
		{
			array[arrayIndex++] = (T)children.Current;
		}
	}

	public bool Remove(T item)
	{
		throw new NotSupportedException();
	}

	public void Clear()
	{
		throw new NotSupportedException();
	}
}
