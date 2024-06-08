// Microsoft.SqlServer.DlgGrid, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.GridCellCollection

using System.Collections;

// using Microsoft.SqlServer.Management.UI.Grid;




namespace BlackbirdSql.Shared.Controls.Grid;


public class GridCellCollection : CollectionBase
{
	public GridCell this[int index]
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

	public int Add(GridCell cell)
	{
		return List.Add(cell);
	}

	public void AddRange(GridCell[] cells)
	{
		for (int i = 0; i < cells.Length; i++)
		{
			Add(cells[i]);
		}
	}

	public void AddRange(GridCellCollection value)
	{
		for (int i = 0; i < value.Count; i++)
		{
			Add(value[i]);
		}
	}

	public bool Contains(GridCell node)
	{
		return List.Contains(node);
	}

	public void CopyTo(GridCell[] array, int index)
	{
		List.CopyTo(array, index);
	}

	public int IndexOf(GridCell cell)
	{
		return List.IndexOf(cell);
	}

	public void Insert(int index, GridCell cell)
	{
		List.Insert(index, cell);
	}

	public void Remove(GridCell cell)
	{
		List.Remove(cell);
	}
}
