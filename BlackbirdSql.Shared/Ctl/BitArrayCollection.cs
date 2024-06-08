// Microsoft.SqlServer.DlgGrid, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.BitArrayCollection

using System.Collections;

// using Microsoft.SqlServer.Management.UI.Grid;




namespace BlackbirdSql.Shared.Ctl;


public class BitArrayCollection : CollectionBase
{
	public BitArray this[int index]
	{
		get
		{
			return (BitArray)List[index];
		}
		set
		{
			List[index] = value;
		}
	}

	public BitArrayCollection()
	{
	}

	public BitArrayCollection(BitArrayCollection value)
	{
		AddRange(value);
	}

	public BitArrayCollection(BitArray[] value)
	{
		AddRange(value);
	}

	public int Add(BitArray cell)
	{
		return List.Add(cell);
	}

	public void AddRange(BitArray[] cells)
	{
		for (int i = 0; i < cells.Length; i++)
		{
			Add(cells[i]);
		}
	}

	public void AddRange(BitArrayCollection value)
	{
		for (int i = 0; i < value.Count; i++)
		{
			Add(value[i]);
		}
	}

	public bool Contains(BitArray array)
	{
		return List.Contains(array);
	}

	public void CopyTo(BitArray[] array, int index)
	{
		List.CopyTo(array, index);
	}

	public int IndexOf(BitArray array)
	{
		return List.IndexOf(array);
	}

	public void Insert(int index, BitArray array)
	{
		List.Insert(index, array);
	}

	public void Remove(BitArray array)
	{
		List.Remove(array);
	}
}
