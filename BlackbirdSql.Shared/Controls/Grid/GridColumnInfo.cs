// Microsoft.SqlServer.GridControl, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.GridColumnInfo

using System.Drawing;
using System.Windows.Forms;
using BlackbirdSql.Shared.Enums;



namespace BlackbirdSql.Shared.Controls.Grid;


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
