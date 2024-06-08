#region Assembly Microsoft.SqlServer.GridControl, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLCommon\Microsoft.SqlServer.GridControl.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Collections;
using System.Drawing;

// namespace Microsoft.SqlServer.Management.UI.Grid
namespace BlackbirdSql.Shared.Controls.Grid
{
	public class GridColumnCollection : CollectionBase, IDisposable
	{
		public AbstractGridColumn this[int index]
		{
			get
			{
				return (AbstractGridColumn)List[index];
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
			foreach (AbstractGridColumn item in List)
			{
				item.Dispose();
			}

			Clear();
		}

		public void SetRTL(bool bRightToLeft)
		{
			foreach (AbstractGridColumn item in List)
			{
				item.SetRTL(bRightToLeft);
			}
		}

		public void ProcessNewGridFont(Font newFont)
		{
			foreach (AbstractGridColumn item in List)
			{
				item.ProcessNewGridFont(newFont);
			}
		}

		public GridColumnCollection(GridColumnCollection value)
		{
			AddRange(value);
		}

		public GridColumnCollection(AbstractGridColumn[] value)
		{
			AddRange(value);
		}

		public int Add(AbstractGridColumn node)
		{
			return List.Add(node);
		}

		public void AddRange(AbstractGridColumn[] nodes)
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

		public bool Contains(AbstractGridColumn node)
		{
			return List.Contains(node);
		}

		public void CopyTo(AbstractGridColumn[] array, int index)
		{
			List.CopyTo(array, index);
		}

		public int IndexOf(AbstractGridColumn node)
		{
			return List.IndexOf(node);
		}

		public void Insert(int index, AbstractGridColumn node)
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

		public void Remove(AbstractGridColumn node)
		{
			List.Remove(node);
		}

		public void Move(int fromIndex, int toIndex)
		{
			AbstractGridColumn value = this[fromIndex];
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
