#region Assembly Microsoft.SqlServer.GridControl, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLCommon\Microsoft.SqlServer.GridControl.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Collections.Generic;
using BlackbirdSql.Core;

// namespace Microsoft.SqlServer.Management.UI.Grid
namespace BlackbirdSql.Common.Controls.Grid
{
	public class GridColumnMapper<T>
	{
		public delegate TResult Func<out TResult, in T1>(T1 i);

		private readonly Dictionary<int, T> columnIndexes = [];

		public T this[int index] => columnIndexes[index];

		public GridColumnMapper(int n, Func<T, int> incrFunc)
		{
			for (int i = 0; i < n; i++)
			{
				columnIndexes.Add(i, incrFunc(i));
			}
		}

		public void ShiftColumnIndexes(int oldIndex, int newIndex)
		{
			if (oldIndex < 0 || oldIndex >= columnIndexes.Count)
			{
				ArgumentOutOfRangeException ex = new("oldIndex");
				Diag.Dug(ex);
				throw ex;
			}

			if (newIndex < 0 || newIndex >= columnIndexes.Count)
			{
				ArgumentOutOfRangeException ex = new("newIndex");
				Diag.Dug(ex);
				throw ex;
			}

			T value = columnIndexes[oldIndex];
			int num = oldIndex < newIndex ? 1 : -1;
			int num2 = Math.Abs(newIndex - oldIndex);
			for (int i = 0; i < num2; i++)
			{
				int num3 = oldIndex + i * num;
				columnIndexes[num3] = columnIndexes[num3 + num];
			}

			columnIndexes[newIndex] = value;
		}
	}
}
