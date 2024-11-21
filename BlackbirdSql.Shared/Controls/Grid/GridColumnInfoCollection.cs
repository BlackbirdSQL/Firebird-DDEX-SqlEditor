// Microsoft.SqlServer.GridControl, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.GridColumnInfoCollection

using System;
using System.Collections;
using BlackbirdSql.Shared.Properties;



namespace BlackbirdSql.Shared.Controls.Grid;


public class GridColumnInfoCollection : CollectionBase
{
	public GridColumnInfo this[int index]
	{
		get
		{
			return (GridColumnInfo)List[index];
		}
		set
		{
			InvalidOperationException ex = new(ControlsResources.ExGridColumnInfoCollectionIsReadOnly);
			Diag.Ex(ex);
			throw ex;
		}
	}

	public GridColumnInfoCollection()
	{
	}

	public GridColumnInfoCollection(GridColumnInfoCollection value)
	{
		AddRange(value);
	}

	public GridColumnInfoCollection(GridColumnInfo[] value)
	{
		AddRange(value);
	}

	public int Add(GridColumnInfo columnInfo)
	{
		return List.Add(columnInfo);
	}

	public void AddRange(GridColumnInfo[] columnInfos)
	{
		for (int i = 0; i < columnInfos.Length; i++)
		{
			Add(columnInfos[i]);
		}
	}

	public void AddRange(GridColumnInfoCollection value)
	{
		for (int i = 0; i < value.Count; i++)
		{
			Add(value[i]);
		}
	}

	public bool Contains(GridColumnInfo columnInfo)
	{
		return List.Contains(columnInfo);
	}

	public void CopyTo(GridColumnInfo[] array, int index)
	{
		List.CopyTo(array, index);
	}

	public int IndexOf(GridColumnInfo columnInfo)
	{
		return List.IndexOf(columnInfo);
	}

	public void Insert(int index, GridColumnInfo columnInfo)
	{
		List.Insert(index, columnInfo);
	}

	public void Remove(GridColumnInfo columnInfo)
	{
		List.Remove(columnInfo);
	}
}
