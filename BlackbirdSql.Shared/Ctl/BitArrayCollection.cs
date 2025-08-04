// Microsoft.SqlServer.DlgGrid, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.BitArrayCollection

using System.Collections;



namespace BlackbirdSql.Shared.Ctl;


internal class BitArrayCollection : CollectionBase
{
	internal BitArray this[int index]
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

	internal int Add(BitArray cell)
	{
		return List.Add(cell);
	}

	internal void AddRange(BitArray[] cells)
	{
		for (int i = 0; i < cells.Length; i++)
		{
			Add(cells[i]);
		}
	}

	internal void AddRange(BitArrayCollection value)
	{
		for (int i = 0; i < value.Count; i++)
		{
			Add(value[i]);
		}
	}

	internal bool Contains(BitArray array)
	{
		return List.Contains(array);
	}

	internal void CopyTo(BitArray[] array, int index)
	{
		List.CopyTo(array, index);
	}

	internal int IndexOf(BitArray array)
	{
		return List.IndexOf(array);
	}

	internal void Insert(int index, BitArray array)
	{
		List.Insert(index, array);
	}

	internal void Remove(BitArray array)
	{
		List.Remove(array);
	}
}
