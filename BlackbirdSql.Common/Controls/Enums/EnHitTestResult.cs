// Microsoft.SqlServer.GridControl, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.HitTestResult


namespace BlackbirdSql.Common.Controls.Enums;

public enum EnHitTestResult
{
	Nothing,
	ColumnOnly,
	RowOnly,
	ColumnResize,
	HeaderButton,
	TextCell,
	ButtonCell,
	BitmapCell,
	HyperlinkCell,
	CustomCell
}
