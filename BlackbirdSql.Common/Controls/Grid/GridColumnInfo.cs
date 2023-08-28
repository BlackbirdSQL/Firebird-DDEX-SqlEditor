#region Assembly Microsoft.SqlServer.GridControl, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLCommon\Microsoft.SqlServer.GridControl.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System.Drawing;
using System.Windows.Forms;
using BlackbirdSql.Common.Controls.Enums;

// namespace Microsoft.SqlServer.Management.UI.Grid
namespace BlackbirdSql.Common.Controls.Grid
{
	public sealed class GridColumnInfo
	{
		public int ColumnType = 1;

		public EnGridColumnHeaderType HeaderType;

		public HorizontalAlignment ColumnAlignment;

		public HorizontalAlignment HeaderAlignment;

		public EnTextBitmapLayout TextBmpHeaderLayout;

		public EnTextBitmapLayout TextBmpCellsLayout;

		public bool IsUserResizable = true;

		public bool IsHeaderMergedWithRight;

		public bool IsWithRightGridLine = true;

		public bool IsWithSelectionBackground = true;

		public EnGridColumnWidthType WidthType = EnGridColumnWidthType.InAverageFontChar;

		public int ColumnWidth = 20;

		public Color BackgroundColor = SystemColors.Window;

		public Color TextColor = SystemColors.WindowText;

		public float MergedHeaderResizeProportion;

		public bool IsHeaderClickable = true;

		public void SetBackgroundColor(Color bkColor)
		{
			BackgroundColor = bkColor;
		}

		public void SetTextColor(Color frColor)
		{
			TextColor = frColor;
		}
	}
}
