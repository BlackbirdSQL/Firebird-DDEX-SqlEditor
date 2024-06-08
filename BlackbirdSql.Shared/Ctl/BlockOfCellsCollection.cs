// Microsoft.SqlServer.GridControl, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.BlockOfCellsCollection

using System;
using System.Collections;
using BlackbirdSql.Shared.Controls.Grid;

namespace BlackbirdSql.Shared.Ctl;


public class BlockOfCellsCollection : CollectionBase, IDisposable
{
	public BlockOfCells this[int index]
	{
		get
		{
			return (BlockOfCells)List[index];
		}
		set
		{
			List[index] = value;
		}
	}

	public BlockOfCellsCollection()
	{
	}

	public void Dispose()
	{
		Clear();
	}

	public BlockOfCellsCollection(BlockOfCellsCollection value)
	{
		AddRange(value);
	}

	public BlockOfCellsCollection(BlockOfCells[] value)
	{
		AddRange(value);
	}

	public int Add(BlockOfCells node)
	{
		return List.Add(node);
	}

	public void AddRange(BlockOfCells[] nodes)
	{
		for (int i = 0; i < nodes.Length; i++)
		{
			Add(nodes[i]);
		}
	}

	public void AddRange(BlockOfCellsCollection value)
	{
		for (int i = 0; i < value.Count; i++)
		{
			Add(value[i]);
		}
	}

	public bool Contains(BlockOfCells node)
	{
		return List.Contains(node);
	}

	public void CopyTo(BlockOfCells[] array, int index)
	{
		List.CopyTo(array, index);
	}

	public int IndexOf(BlockOfCells node)
	{
		return List.IndexOf(node);
	}

	public void Insert(int index, BlockOfCells node)
	{
		List.Insert(index, node);
	}

	public void Remove(BlockOfCells node)
	{
		List.Remove(node);
	}
}
