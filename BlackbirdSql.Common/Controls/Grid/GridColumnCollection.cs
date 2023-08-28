#region Assembly Microsoft.SqlServer.GridControl, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLCommon\Microsoft.SqlServer.GridControl.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Collections;
using System.Drawing;

// namespace Microsoft.SqlServer.Management.UI.Grid
namespace BlackbirdSql.Common.Controls.Grid
{
	public class GridColumnCollection : CollectionBase, IDisposable
	{
		public GridColumn this[int index]
		{
			get
			{
				return (GridColumn)List[index];
			}
			set
			{
				List[index] = value;
			}
		}

		public GridColumnCollection()
		{
		}

		public void Dispose()
		{
			foreach (GridColumn item in List)
			{
				item.Dispose();
			}

			Clear();
		}

		public void SetRTL(bool bRightToLeft)
		{
			foreach (GridColumn item in List)
			{
				item.SetRTL(bRightToLeft);
			}
		}

		public void ProcessNewGridFont(Font newFont)
		{
			foreach (GridColumn item in List)
			{
				item.ProcessNewGridFont(newFont);
			}
		}

		public GridColumnCollection(GridColumnCollection value)
		{
			AddRange(value);
		}

		public GridColumnCollection(GridColumn[] value)
		{
			AddRange(value);
		}

		public int Add(GridColumn node)
		{
			return List.Add(node);
		}

		public void AddRange(GridColumn[] nodes)
		{
			for (int i = 0; i < nodes.Length; i++)
			{
				Add(nodes[i]);
			}
		}

		public void AddRange(GridColumnCollection value)
		{
			for (int i = 0; i < value.Count; i++)
			{
				Add(value[i]);
			}
		}

		public bool Contains(GridColumn node)
		{
			return List.Contains(node);
		}

		public void CopyTo(GridColumn[] array, int index)
		{
			List.CopyTo(array, index);
		}

		public int IndexOf(GridColumn node)
		{
			return List.IndexOf(node);
		}

		public void Insert(int index, GridColumn node)
		{
			for (int i = 0; i < Count; i++)
			{
				if (this[i].ColumnIndex >= index)
				{
					this[i].ColumnIndex++;
				}
			}

			List.Insert(index, node);
		}

		public void Remove(GridColumn node)
		{
			List.Remove(node);
		}

		public void Move(int fromIndex, int toIndex)
		{
			GridColumn value = this[fromIndex];
			RemoveAt(fromIndex);
			List.Insert(toIndex, value);
		}

		public void RemoveAtAndAdjust(int index)
		{
			int columnIndex = this[index].ColumnIndex;
			for (int i = 0; i < Count; i++)
			{
				if (this[i].ColumnIndex > columnIndex)
				{
					this[i].ColumnIndex--;
				}
			}

			RemoveAt(index);
		}
	}
}
