// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.RelOpType
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;
using BlackbirdSql.Shared.Controls.Graphing.Enums;



namespace BlackbirdSql.Shared.Controls.Graphing.Gram;

[Serializable]
[GeneratedCode("xsd", "4.8.3928.0")]
[DebuggerStepThrough]
[DesignerCategory("code")]
[XmlType(Namespace = LibraryData.C_ShowPlanNamespace)]
internal class RelOpType
{
	private ColumnReferenceType[] outputListField;

	private WarningsType warningsField;

	private MemoryFractionsType memoryFractionsField;

	private RunTimeInformationTypeRunTimeCountersPerThread[] runTimeInformationField;

	private RunTimePartitionSummaryType runTimePartitionSummaryField;

	private InternalInfoType internalInfoField;

	private RelOpBaseType itemField;

	private EnItemChoiceType itemElementNameField;

	private double avgRowSizeField;

	private double estimateCPUField;

	private double estimateIOField;

	private double estimateRebindsField;

	private double estimateRewindsField;

	private EnExecutionModeType estimatedExecutionModeField;

	private bool estimatedExecutionModeFieldSpecified;

	private bool groupExecutedField;

	private bool groupExecutedFieldSpecified;

	private double estimateRowsField;

	private double estimateRowsWithoutRowGoalField;

	private bool estimateRowsWithoutRowGoalFieldSpecified;

	private double estimatedRowsReadField;

	private bool estimatedRowsReadFieldSpecified;

	private EnLogicalOpType logicalOpField;

	private int nodeIdField;

	private bool nodeIdFieldSpecified;

	private bool parallelField;

	private bool remoteDataAccessField;

	private bool remoteDataAccessFieldSpecified;

	private bool partitionedField;

	private bool partitionedFieldSpecified;

	private EnPhysicalOpType physicalOpField;

	private bool isAdaptiveField;

	private bool isAdaptiveFieldSpecified;

	private double adaptiveThresholdRowsField;

	private bool adaptiveThresholdRowsFieldSpecified;

	private double estimatedTotalSubtreeCostField;

	private double tableCardinalityField;

	private bool tableCardinalityFieldSpecified;

	private ulong statsCollectionIdField;

	private bool statsCollectionIdFieldSpecified;

	private EnPhysicalOpType estimatedJoinTypeField;

	private bool estimatedJoinTypeFieldSpecified;

	private string hyperScaleOptimizedQueryProcessingField;

	private string hyperScaleOptimizedQueryProcessingUnusedReasonField;

	private double pDWAccumulativeCostField;

	private bool pDWAccumulativeCostFieldSpecified;

	[XmlArrayItem("ColumnReference", IsNullable = false)]
	internal ColumnReferenceType[] OutputList
	{
		get
		{
			return outputListField;
		}
		set
		{
			outputListField = value;
		}
	}

	internal WarningsType Warnings
	{
		get
		{
			return warningsField;
		}
		set
		{
			warningsField = value;
		}
	}

	internal MemoryFractionsType MemoryFractions
	{
		get
		{
			return memoryFractionsField;
		}
		set
		{
			memoryFractionsField = value;
		}
	}

	[XmlArrayItem("RunTimeCountersPerThread", IsNullable = false)]
	internal RunTimeInformationTypeRunTimeCountersPerThread[] RunTimeInformation
	{
		get
		{
			return runTimeInformationField;
		}
		set
		{
			runTimeInformationField = value;
		}
	}

	internal RunTimePartitionSummaryType RunTimePartitionSummary
	{
		get
		{
			return runTimePartitionSummaryField;
		}
		set
		{
			runTimePartitionSummaryField = value;
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

	[XmlElement("AdaptiveJoin", typeof(AdaptiveJoinType))]
	[XmlElement("Apply", typeof(JoinType))]
	[XmlElement("Assert", typeof(FilterType))]
	[XmlElement("BatchHashTableBuild", typeof(BatchHashTableBuildType))]
	[XmlElement("Bitmap", typeof(BitmapType))]
	[XmlElement("Collapse", typeof(CollapseType))]
	[XmlElement("ComputeScalar", typeof(ComputeScalarType))]
	[XmlElement("Concat", typeof(ConcatType))]
	[XmlElement("ConstTableGet", typeof(GetType))]
	[XmlElement("ConstantScan", typeof(ConstantScanType))]
	[XmlElement("CreateIndex", typeof(CreateIndexType))]
	[XmlElement("Delete", typeof(DMLOpType))]
	[XmlElement("DeletedScan", typeof(RowsetType))]
	[XmlElement("ExtExtractScan", typeof(RemoteType))]
	[XmlElement("Extension", typeof(UDXType))]
	[XmlElement("ExternalSelect", typeof(ExternalSelectType))]
	[XmlElement("Filter", typeof(FilterType))]
	[XmlElement("ForeignKeyReferencesCheck", typeof(ForeignKeyReferencesCheckType))]
	[XmlElement("GbAgg", typeof(GbAggType))]
	[XmlElement("GbApply", typeof(GbApplyType))]
	[XmlElement("Generic", typeof(GenericType))]
	[XmlElement("Get", typeof(GetType))]
	[XmlElement("Hash", typeof(HashType))]
	[XmlElement("IndexScan", typeof(IndexScanType))]
	[XmlElement("Insert", typeof(DMLOpType))]
	[XmlElement("InsertedScan", typeof(RowsetType))]
	[XmlElement("Join", typeof(JoinType))]
	[XmlElement("LocalCube", typeof(LocalCubeType))]
	[XmlElement("LogRowScan", typeof(RelOpBaseType))]
	[XmlElement("Merge", typeof(MergeType))]
	[XmlElement("MergeInterval", typeof(SimpleIteratorOneChildType))]
	[XmlElement("Move", typeof(MoveType))]
	[XmlElement("NestedLoops", typeof(NestedLoopsType))]
	[XmlElement("OnlineIndex", typeof(CreateIndexType))]
	[XmlElement("Parallelism", typeof(ParallelismType))]
	[XmlElement("ParameterTableScan", typeof(RelOpBaseType))]
	[XmlElement("PrintDataflow", typeof(RelOpBaseType))]
	[XmlElement("Project", typeof(ProjectType))]
	[XmlElement("Put", typeof(PutType))]
	[XmlElement("RemoteFetch", typeof(RemoteFetchType))]
	[XmlElement("RemoteModify", typeof(RemoteModifyType))]
	[XmlElement("RemoteQuery", typeof(RemoteQueryType))]
	[XmlElement("RemoteRange", typeof(RemoteRangeType))]
	[XmlElement("RemoteScan", typeof(RemoteType))]
	[XmlElement("RowCountSpool", typeof(SpoolType))]
	[XmlElement("ScalarInsert", typeof(ScalarInsertType))]
	[XmlElement("Segment", typeof(SegmentType))]
	[XmlElement("Sequence", typeof(SequenceType))]
	[XmlElement("SequenceProject", typeof(ComputeScalarType))]
	[XmlElement("SimpleUpdate", typeof(SimpleUpdateType))]
	[XmlElement("Sort", typeof(SortType))]
	[XmlElement("Split", typeof(SplitType))]
	[XmlElement("Spool", typeof(SpoolType))]
	[XmlElement("StreamAggregate", typeof(StreamAggregateType))]
	[XmlElement("Switch", typeof(SwitchType))]
	[XmlElement("TableScan", typeof(TableScanType))]
	[XmlElement("TableValuedFunction", typeof(TableValuedFunctionType))]
	[XmlElement("Top", typeof(TopType))]
	[XmlElement("TopSort", typeof(TopSortType))]
	[XmlElement("Union", typeof(ConcatType))]
	[XmlElement("UnionAll", typeof(ConcatType))]
	[XmlElement("Update", typeof(UpdateType))]
	[XmlElement("WindowAggregate", typeof(WindowAggregateType))]
	[XmlElement("WindowSpool", typeof(WindowType))]
	[XmlElement("XcsScan", typeof(XcsScanType))]
	[XmlChoiceIdentifier("ItemElementName")]
	internal RelOpBaseType Item
	{
		get
		{
			return itemField;
		}
		set
		{
			itemField = value;
		}
	}

	[XmlIgnore]
	internal EnItemChoiceType ItemElementName
	{
		get
		{
			return itemElementNameField;
		}
		set
		{
			itemElementNameField = value;
		}
	}

	[XmlAttribute]
	internal double AvgRowSize
	{
		get
		{
			return avgRowSizeField;
		}
		set
		{
			avgRowSizeField = value;
		}
	}

	[XmlAttribute]
	internal double EstimateCPU
	{
		get
		{
			return estimateCPUField;
		}
		set
		{
			estimateCPUField = value;
		}
	}

	[XmlAttribute]
	internal double EstimateIO
	{
		get
		{
			return estimateIOField;
		}
		set
		{
			estimateIOField = value;
		}
	}

	[XmlAttribute]
	internal double EstimateRebinds
	{
		get
		{
			return estimateRebindsField;
		}
		set
		{
			estimateRebindsField = value;
		}
	}

	[XmlAttribute]
	internal double EstimateRewinds
	{
		get
		{
			return estimateRewindsField;
		}
		set
		{
			estimateRewindsField = value;
		}
	}

	[XmlAttribute]
	internal EnExecutionModeType EstimatedExecutionMode
	{
		get
		{
			return estimatedExecutionModeField;
		}
		set
		{
			estimatedExecutionModeField = value;
		}
	}

	[XmlIgnore]
	internal bool EstimatedExecutionModeSpecified
	{
		get
		{
			return estimatedExecutionModeFieldSpecified;
		}
		set
		{
			estimatedExecutionModeFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal bool GroupExecuted
	{
		get
		{
			return groupExecutedField;
		}
		set
		{
			groupExecutedField = value;
		}
	}

	[XmlIgnore]
	internal bool GroupExecutedSpecified
	{
		get
		{
			return groupExecutedFieldSpecified;
		}
		set
		{
			groupExecutedFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal double EstimateRows
	{
		get
		{
			return estimateRowsField;
		}
		set
		{
			estimateRowsField = value;
		}
	}

	[XmlAttribute]
	internal double EstimateRowsWithoutRowGoal
	{
		get
		{
			return estimateRowsWithoutRowGoalField;
		}
		set
		{
			estimateRowsWithoutRowGoalField = value;
		}
	}

	[XmlIgnore]
	internal bool EstimateRowsWithoutRowGoalSpecified
	{
		get
		{
			return estimateRowsWithoutRowGoalFieldSpecified;
		}
		set
		{
			estimateRowsWithoutRowGoalFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal double EstimatedRowsRead
	{
		get
		{
			return estimatedRowsReadField;
		}
		set
		{
			estimatedRowsReadField = value;
		}
	}

	[XmlIgnore]
	internal bool EstimatedRowsReadSpecified
	{
		get
		{
			return estimatedRowsReadFieldSpecified;
		}
		set
		{
			estimatedRowsReadFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal EnLogicalOpType LogicalOp
	{
		get
		{
			return logicalOpField;
		}
		set
		{
			logicalOpField = value;
		}
	}

	[XmlAttribute]
	internal int NodeId
	{
		get
		{
			return nodeIdField;
		}
		set
		{
			nodeIdField = value;
		}
	}

	[XmlIgnore]
	internal bool NodeIdSpecified
	{
		get
		{
			return nodeIdFieldSpecified;
		}
		set
		{
			nodeIdFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal bool Parallel
	{
		get
		{
			return parallelField;
		}
		set
		{
			parallelField = value;
		}
	}

	[XmlAttribute]
	internal bool RemoteDataAccess
	{
		get
		{
			return remoteDataAccessField;
		}
		set
		{
			remoteDataAccessField = value;
		}
	}

	[XmlIgnore]
	internal bool RemoteDataAccessSpecified
	{
		get
		{
			return remoteDataAccessFieldSpecified;
		}
		set
		{
			remoteDataAccessFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal bool Partitioned
	{
		get
		{
			return partitionedField;
		}
		set
		{
			partitionedField = value;
		}
	}

	[XmlIgnore]
	internal bool PartitionedSpecified
	{
		get
		{
			return partitionedFieldSpecified;
		}
		set
		{
			partitionedFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal EnPhysicalOpType PhysicalOp
	{
		get
		{
			return physicalOpField;
		}
		set
		{
			physicalOpField = value;
		}
	}

	[XmlAttribute]
	internal bool IsAdaptive
	{
		get
		{
			return isAdaptiveField;
		}
		set
		{
			isAdaptiveField = value;
		}
	}

	[XmlIgnore]
	internal bool IsAdaptiveSpecified
	{
		get
		{
			return isAdaptiveFieldSpecified;
		}
		set
		{
			isAdaptiveFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal double AdaptiveThresholdRows
	{
		get
		{
			return adaptiveThresholdRowsField;
		}
		set
		{
			adaptiveThresholdRowsField = value;
		}
	}

	[XmlIgnore]
	internal bool AdaptiveThresholdRowsSpecified
	{
		get
		{
			return adaptiveThresholdRowsFieldSpecified;
		}
		set
		{
			adaptiveThresholdRowsFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal double EstimatedTotalSubtreeCost
	{
		get
		{
			return estimatedTotalSubtreeCostField;
		}
		set
		{
			estimatedTotalSubtreeCostField = value;
		}
	}

	[XmlAttribute]
	internal double TableCardinality
	{
		get
		{
			return tableCardinalityField;
		}
		set
		{
			tableCardinalityField = value;
		}
	}

	[XmlIgnore]
	internal bool TableCardinalitySpecified
	{
		get
		{
			return tableCardinalityFieldSpecified;
		}
		set
		{
			tableCardinalityFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal ulong StatsCollectionId
	{
		get
		{
			return statsCollectionIdField;
		}
		set
		{
			statsCollectionIdField = value;
		}
	}

	[XmlIgnore]
	internal bool StatsCollectionIdSpecified
	{
		get
		{
			return statsCollectionIdFieldSpecified;
		}
		set
		{
			statsCollectionIdFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal EnPhysicalOpType EstimatedJoinType
	{
		get
		{
			return estimatedJoinTypeField;
		}
		set
		{
			estimatedJoinTypeField = value;
		}
	}

	[XmlIgnore]
	internal bool EstimatedJoinTypeSpecified
	{
		get
		{
			return estimatedJoinTypeFieldSpecified;
		}
		set
		{
			estimatedJoinTypeFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal string HyperScaleOptimizedQueryProcessing
	{
		get
		{
			return hyperScaleOptimizedQueryProcessingField;
		}
		set
		{
			hyperScaleOptimizedQueryProcessingField = value;
		}
	}

	[XmlAttribute]
	internal string HyperScaleOptimizedQueryProcessingUnusedReason
	{
		get
		{
			return hyperScaleOptimizedQueryProcessingUnusedReasonField;
		}
		set
		{
			hyperScaleOptimizedQueryProcessingUnusedReasonField = value;
		}
	}

	[XmlAttribute]
	internal double PDWAccumulativeCost
	{
		get
		{
			return pDWAccumulativeCostField;
		}
		set
		{
			pDWAccumulativeCostField = value;
		}
	}

	[XmlIgnore]
	internal bool PDWAccumulativeCostSpecified
	{
		get
		{
			return pDWAccumulativeCostFieldSpecified;
		}
		set
		{
			pDWAccumulativeCostFieldSpecified = value;
		}
	}
}
