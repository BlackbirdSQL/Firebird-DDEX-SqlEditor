// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.MergeColumns


using BlackbirdSql.Shared.Controls.Graphing.Gram;

namespace BlackbirdSql.Shared.Controls.Graphing;

internal sealed class MergeColumns
{
	private readonly ColumnReferenceType[] innerSideJoinColumnsField;

	private readonly ColumnReferenceType[] outerSideJoinColumnsField;

	internal ColumnReferenceType[] InnerSideJoinColumns => innerSideJoinColumnsField;

	internal ColumnReferenceType[] OuterSideJoinColumns => outerSideJoinColumnsField;

	public MergeColumns(MergeType mergeType)
	{
		innerSideJoinColumnsField = mergeType.InnerSideJoinColumns;
		outerSideJoinColumnsField = mergeType.OuterSideJoinColumns;
	}
}
