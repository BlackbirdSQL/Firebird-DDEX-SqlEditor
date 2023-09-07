// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.RelOpType
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;
using BlackbirdSql.Common.Controls.Graphing.Enums;



namespace BlackbirdSql.Common.Controls.Graphing.Gram;

[Serializable]
[GeneratedCode("xsd", "4.8.3928.0")]
[DebuggerStepThrough]
[DesignerCategory("code")]
[XmlType(Namespace = LibraryData.C_ShowPlanNamespace)]
public class RelOpType
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
	public ColumnReferenceType[] OutputList
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

	public WarningsType Warnings
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

	public MemoryFractionsType MemoryFractions
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
	public RunTimeInformationTypeRunTimeCountersPerThread[] RunTimeInformation
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

	public RunTimePartitionSummaryType RunTimePartitionSummary
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

	public InternalInfoType InternalInfo
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
	public RelOpBaseType Item
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
	public EnItemChoiceType ItemElementName
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
	public double AvgRowSize
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
	public double EstimateCPU
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
	public double EstimateIO
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
	public double EstimateRebinds
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
	public double EstimateRewinds
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
	public EnExecutionModeType EstimatedExecutionMode
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
	public bool EstimatedExecutionModeSpecified
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
	public bool GroupExecuted
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
	public bool GroupExecutedSpecified
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
	public double EstimateRows
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
	public double EstimateRowsWithoutRowGoal
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
	public bool EstimateRowsWithoutRowGoalSpecified
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
	public double EstimatedRowsRead
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
	public bool EstimatedRowsReadSpecified
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
	public EnLogicalOpType LogicalOp
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
	public int NodeId
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
	public bool NodeIdSpecified
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
	public bool Parallel
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
	public bool RemoteDataAccess
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
	public bool RemoteDataAccessSpecified
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
	public bool Partitioned
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
	public bool PartitionedSpecified
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
	public EnPhysicalOpType PhysicalOp
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
	public bool IsAdaptive
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
	public bool IsAdaptiveSpecified
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
	public double AdaptiveThresholdRows
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
	public bool AdaptiveThresholdRowsSpecified
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
	public double EstimatedTotalSubtreeCost
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
	public double TableCardinality
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
	public bool TableCardinalitySpecified
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
	public ulong StatsCollectionId
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
	public bool StatsCollectionIdSpecified
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
	public EnPhysicalOpType EstimatedJoinType
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
	public bool EstimatedJoinTypeSpecified
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
	public string HyperScaleOptimizedQueryProcessing
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
	public string HyperScaleOptimizedQueryProcessingUnusedReason
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
	public double PDWAccumulativeCost
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
	public bool PDWAccumulativeCostSpecified
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
