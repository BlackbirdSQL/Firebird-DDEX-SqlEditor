// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.MergeColumns


using BlackbirdSql.Common.Controls.Graphing.Gram;

namespace BlackbirdSql.Common.Controls.Graphing;

public sealed class MergeColumns
{
	private readonly ColumnReferenceType[] innerSideJoinColumnsField;

	private readonly ColumnReferenceType[] outerSideJoinColumnsField;

	public ColumnReferenceType[] InnerSideJoinColumns => innerSideJoinColumnsField;

	public ColumnReferenceType[] OuterSideJoinColumns => outerSideJoinColumnsField;

	public MergeColumns(MergeType mergeType)
	{
		innerSideJoinColumnsField = mergeType.InnerSideJoinColumns;
		outerSideJoinColumnsField = mergeType.OuterSideJoinColumns;
	}
}
