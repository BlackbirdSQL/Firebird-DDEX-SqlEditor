#region Assembly Microsoft.SqlServer.GridControl, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLCommon\Microsoft.SqlServer.GridControl.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System.Drawing;
using BlackbirdSql.Shared.Enums;

// namespace Microsoft.SqlServer.Management.UI.Grid
namespace BlackbirdSql.Shared.Controls.Grid
{
	public sealed class HitTestInfo
	{
		private readonly EnHitTestResult result;

		private readonly long rowIndex;

		private readonly int columnIndex;

		private Rectangle areaRectangle;

		public EnHitTestResult HitTestResult => result;

		public long RowIndex => rowIndex;

		public int ColumnIndex => columnIndex;

		public Rectangle AreaRectangle => areaRectangle;

		private HitTestInfo()
		{
		}

		public HitTestInfo(EnHitTestResult result, long rowIndex, int columnIndex, Rectangle areaRectangle)
		{
			this.result = result;
			this.rowIndex = rowIndex;
			this.columnIndex = columnIndex;
			this.areaRectangle = areaRectangle;
		}
	}
}
