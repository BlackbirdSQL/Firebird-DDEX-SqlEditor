// Microsoft.SqlServer.DlgGrid, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.GridCellCollection

using System.Collections;



namespace BlackbirdSql.Shared.Controls.Grid;


internal class GridCellCollection : CollectionBase
{
	internal GridCell this[int index]
	{
		get
		{
			return (GridCell)List[index];
		}
		set
		{
			List[index] = value;
		}
	}

	public GridCellCollection()
	{
	}

	public GridCellCollection(GridCellCollection value)
	{
		AddRange(value);
	}

	public GridCellCollection(GridCell[] value)
	{
		AddRange(value);
	}

	internal int Add(GridCell cell)
	{
		return List.Add(cell);
	}

	internal void AddRange(GridCell[] cells)
	{
		for (int i = 0; i < cells.Length; i++)
		{
			Add(cells[i]);
		}
	}

	internal void AddRange(GridCellCollection value)
	{
		for (int i = 0; i < value.Count; i++)
		{
			Add(value[i]);
		}
	}

	internal bool Contains(GridCell node)
	{
		return List.Contains(node);
	}

	internal void CopyTo(GridCell[] array, int index)
	{
		List.CopyTo(array, index);
	}

	internal int IndexOf(GridCell cell)
	{
		return List.IndexOf(cell);
	}

	internal void Insert(int index, GridCell cell)
	{
		List.Insert(index, cell);
	}

	internal void Remove(GridCell cell)
	{
		List.Remove(cell);
	}
}
