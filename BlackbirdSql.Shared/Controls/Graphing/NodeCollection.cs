// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.NodeCollection<T>
using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.AnalysisServices.Graphing;


namespace BlackbirdSql.Shared.Controls.Graphing;

internal class NodeCollection<T> : ICollection<T>, IEnumerable<T>, IEnumerable where T : Microsoft.AnalysisServices.Graphing.Node
{
	private readonly T node;

	internal T Owner => node;

	internal T this[int index]
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

	internal int Count => node.CnodeChildren();

	internal virtual bool IsReadOnly => true;

	public NodeCollection(T node)
	{
		this.node = node;
	}

	internal T GetPrevious(T item)
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

	internal T GetNext(T item)
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

	internal IEnumerator<T> GetEnumerator()
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

	internal virtual void Add(T child)
	{
		throw new NotSupportedException();
	}

	internal bool Contains(T item)
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

	internal void CopyTo(T[] array, int arrayIndex)
	{
		INodeEnumerator children = ((INode)node).Children;
		while (children.MoveNext())
		{
			array[arrayIndex++] = (T)children.Current;
		}
	}

	internal bool Remove(T item)
	{
		throw new NotSupportedException();
	}

	internal void Clear()
	{
		throw new NotSupportedException();
	}
}
