// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.RelOpBaseType
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;


namespace BlackbirdSql.Shared.Controls.Graphing.Gram;

[Serializable]
[XmlInclude(typeof(GetType))]
[XmlInclude(typeof(DMLOpType))]
[XmlInclude(typeof(LocalCubeType))]
[XmlInclude(typeof(GbAggType))]
[XmlInclude(typeof(GbApplyType))]
[XmlInclude(typeof(JoinType))]
[XmlInclude(typeof(ProjectType))]
[XmlInclude(typeof(ExternalSelectType))]
[XmlInclude(typeof(MoveType))]
[XmlInclude(typeof(GenericType))]
[XmlInclude(typeof(RemoteType))]
[XmlInclude(typeof(RemoteQueryType))]
[XmlInclude(typeof(PutType))]
[XmlInclude(typeof(RemoteModifyType))]
[XmlInclude(typeof(RemoteFetchType))]
[XmlInclude(typeof(RemoteRangeType))]
[XmlInclude(typeof(BatchHashTableBuildType))]
[XmlInclude(typeof(SpoolType))]
[XmlInclude(typeof(WindowAggregateType))]
[XmlInclude(typeof(WindowType))]
[XmlInclude(typeof(UDXType))]
[XmlInclude(typeof(TopType))]
[XmlInclude(typeof(SplitType))]
[XmlInclude(typeof(SequenceType))]
[XmlInclude(typeof(SegmentType))]
[XmlInclude(typeof(NestedLoopsType))]
[XmlInclude(typeof(MergeType))]
[XmlInclude(typeof(ConcatType))]
[XmlInclude(typeof(SwitchType))]
[XmlInclude(typeof(CollapseType))]
[XmlInclude(typeof(BitmapType))]
[XmlInclude(typeof(SortType))]
[XmlInclude(typeof(TopSortType))]
[XmlInclude(typeof(StreamAggregateType))]
[XmlInclude(typeof(ParallelismType))]
[XmlInclude(typeof(ComputeScalarType))]
[XmlInclude(typeof(HashType))]
[XmlInclude(typeof(TableValuedFunctionType))]
[XmlInclude(typeof(ConstantScanType))]
[XmlInclude(typeof(FilterType))]
[XmlInclude(typeof(SimpleIteratorOneChildType))]
[XmlInclude(typeof(RowsetType))]
[XmlInclude(typeof(ScalarInsertType))]
[XmlInclude(typeof(CreateIndexType))]
[XmlInclude(typeof(UpdateType))]
[XmlInclude(typeof(SimpleUpdateType))]
[XmlInclude(typeof(IndexScanType))]
[XmlInclude(typeof(XcsScanType))]
[XmlInclude(typeof(TableScanType))]
[XmlInclude(typeof(ForeignKeyReferencesCheckType))]
[XmlInclude(typeof(AdaptiveJoinType))]
[GeneratedCode("xsd", "4.8.3928.0")]
[DebuggerStepThrough]
[DesignerCategory("code")]
[XmlType(Namespace = LibraryData.C_ShowPlanNamespace)]
internal class RelOpBaseType
{
	private DefinedValuesListTypeDefinedValue[] definedValuesField;

	private InternalInfoType internalInfoField;

	[XmlArrayItem("DefinedValue", IsNullable = false)]
	internal DefinedValuesListTypeDefinedValue[] DefinedValues
	{
		get
		{
			return definedValuesField;
		}
		set
		{
			definedValuesField = value;
		}
	}

	internal InternalInfoType InternalInfo
	{
		get
		{
			return internalInfoField;
		}
		set
		{
			internalInfoField = value;
		}
	}
}
