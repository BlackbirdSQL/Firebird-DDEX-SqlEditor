// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.QueryPlanType
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;


namespace BlackbirdSql.Common.Controls.Graphing.Gram;

[Serializable]
[GeneratedCode("xsd", "4.8.3928.0")]
[DebuggerStepThrough]
[DesignerCategory("code")]
[XmlType(Namespace = LibraryData.C_ShowPlanNamespace)]
public class QueryPlanType
{
	private InternalInfoType internalInfoField;

	private OptimizationReplayType optimizationReplayField;

	private ThreadStatType threadStatField;

	private MissingIndexGroupType[] missingIndexesField;

	private GuessedSelectivityType guessedSelectivityField;

	private UnmatchedIndexesType unmatchedIndexesField;

	private WarningsType warningsField;

	private MemoryGrantType memoryGrantInfoField;

	private OptimizerHardwareDependentPropertiesType optimizerHardwareDependentPropertiesField;

	private StatsInfoType[] optimizerStatsUsageField;

	private TraceFlagListType[] traceFlagsField;

	private WaitStatType[] waitStatsField;

	private QueryExecTimeType queryTimeStatsField;

	private RelOpType relOpField;

	private ColumnReferenceType[] parameterListField;

	private int degreeOfParallelismField;

	private bool degreeOfParallelismFieldSpecified;

	private int effectiveDegreeOfParallelismField;

	private bool effectiveDegreeOfParallelismFieldSpecified;

	private string nonParallelPlanReasonField;

	private ulong memoryGrantField;

	private bool memoryGrantFieldSpecified;

	private ulong cachedPlanSizeField;

	private bool cachedPlanSizeFieldSpecified;

	private ulong compileTimeField;

	private bool compileTimeFieldSpecified;

	private ulong compileCPUField;

	private bool compileCPUFieldSpecified;

	private ulong compileMemoryField;

	private bool compileMemoryFieldSpecified;

	private bool usePlanField;

	private bool usePlanFieldSpecified;

	private bool containsInterleavedExecutionCandidatesField;

	private bool containsInterleavedExecutionCandidatesFieldSpecified;

	private bool containsInlineScalarTsqlUdfsField;

	private bool containsInlineScalarTsqlUdfsFieldSpecified;

	private int queryVariantIDField;

	private bool queryVariantIDFieldSpecified;

	private string dispatcherPlanHandleField;

	private bool exclusiveProfileTimeActiveField;

	private bool exclusiveProfileTimeActiveFieldSpecified;

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

	public OptimizationReplayType OptimizationReplay
	{
		get
		{
			return optimizationReplayField;
		}
		set
		{
			optimizationReplayField = value;
		}
	}

	public ThreadStatType ThreadStat
	{
		get
		{
			return threadStatField;
		}
		set
		{
			threadStatField = value;
		}
	}

	[XmlArrayItem("MissingIndexGroup", IsNullable = false)]
	public MissingIndexGroupType[] MissingIndexes
	{
		get
		{
			return missingIndexesField;
		}
		set
		{
			missingIndexesField = value;
		}
	}

	public GuessedSelectivityType GuessedSelectivity
	{
		get
		{
			return guessedSelectivityField;
		}
		set
		{
			guessedSelectivityField = value;
		}
	}

	public UnmatchedIndexesType UnmatchedIndexes
	{
		get
		{
			return unmatchedIndexesField;
		}
		set
		{
			unmatchedIndexesField = value;
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

	public MemoryGrantType MemoryGrantInfo
	{
		get
		{
			return memoryGrantInfoField;
		}
		set
		{
			memoryGrantInfoField = value;
		}
	}

	public OptimizerHardwareDependentPropertiesType OptimizerHardwareDependentProperties
	{
		get
		{
			return optimizerHardwareDependentPropertiesField;
		}
		set
		{
			optimizerHardwareDependentPropertiesField = value;
		}
	}

	[XmlArrayItem("StatisticsInfo", IsNullable = false)]
	public StatsInfoType[] OptimizerStatsUsage
	{
		get
		{
			return optimizerStatsUsageField;
		}
		set
		{
			optimizerStatsUsageField = value;
		}
	}

	[XmlElement("TraceFlags")]
	public TraceFlagListType[] TraceFlags
	{
		get
		{
			return traceFlagsField;
		}
		set
		{
			traceFlagsField = value;
		}
	}

	[XmlArrayItem("Wait", IsNullable = false)]
	public WaitStatType[] WaitStats
	{
		get
		{
			return waitStatsField;
		}
		set
		{
			waitStatsField = value;
		}
	}

	public QueryExecTimeType QueryTimeStats
	{
		get
		{
			return queryTimeStatsField;
		}
		set
		{
			queryTimeStatsField = value;
		}
	}

	public RelOpType RelOp
	{
		get
		{
			return relOpField;
		}
		set
		{
			relOpField = value;
		}
	}

	[XmlArrayItem("ColumnReference", IsNullable = false)]
	public ColumnReferenceType[] ParameterList
	{
		get
		{
			return parameterListField;
		}
		set
		{
			parameterListField = value;
		}
	}

	[XmlAttribute]
	public int DegreeOfParallelism
	{
		get
		{
			return degreeOfParallelismField;
		}
		set
		{
			degreeOfParallelismField = value;
		}
	}

	[XmlIgnore]
	public bool DegreeOfParallelismSpecified
	{
		get
		{
			return degreeOfParallelismFieldSpecified;
		}
		set
		{
			degreeOfParallelismFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public int EffectiveDegreeOfParallelism
	{
		get
		{
			return effectiveDegreeOfParallelismField;
		}
		set
		{
			effectiveDegreeOfParallelismField = value;
		}
	}

	[XmlIgnore]
	public bool EffectiveDegreeOfParallelismSpecified
	{
		get
		{
			return effectiveDegreeOfParallelismFieldSpecified;
		}
		set
		{
			effectiveDegreeOfParallelismFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public string NonParallelPlanReason
	{
		get
		{
			return nonParallelPlanReasonField;
		}
		set
		{
			nonParallelPlanReasonField = value;
		}
	}

	[XmlAttribute]
	public ulong MemoryGrant
	{
		get
		{
			return memoryGrantField;
		}
		set
		{
			memoryGrantField = value;
		}
	}

	[XmlIgnore]
	public bool MemoryGrantSpecified
	{
		get
		{
			return memoryGrantFieldSpecified;
		}
		set
		{
			memoryGrantFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public ulong CachedPlanSize
	{
		get
		{
			return cachedPlanSizeField;
		}
		set
		{
			cachedPlanSizeField = value;
		}
	}

	[XmlIgnore]
	public bool CachedPlanSizeSpecified
	{
		get
		{
			return cachedPlanSizeFieldSpecified;
		}
		set
		{
			cachedPlanSizeFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public ulong CompileTime
	{
		get
		{
			return compileTimeField;
		}
		set
		{
			compileTimeField = value;
		}
	}

	[XmlIgnore]
	public bool CompileTimeSpecified
	{
		get
		{
			return compileTimeFieldSpecified;
		}
		set
		{
			compileTimeFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public ulong CompileCPU
	{
		get
		{
			return compileCPUField;
		}
		set
		{
			compileCPUField = value;
		}
	}

	[XmlIgnore]
	public bool CompileCPUSpecified
	{
		get
		{
			return compileCPUFieldSpecified;
		}
		set
		{
			compileCPUFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public ulong CompileMemory
	{
		get
		{
			return compileMemoryField;
		}
		set
		{
			compileMemoryField = value;
		}
	}

	[XmlIgnore]
	public bool CompileMemorySpecified
	{
		get
		{
			return compileMemoryFieldSpecified;
		}
		set
		{
			compileMemoryFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public bool UsePlan
	{
		get
		{
			return usePlanField;
		}
		set
		{
			usePlanField = value;
		}
	}

	[XmlIgnore]
	public bool UsePlanSpecified
	{
		get
		{
			return usePlanFieldSpecified;
		}
		set
		{
			usePlanFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public bool ContainsInterleavedExecutionCandidates
	{
		get
		{
			return containsInterleavedExecutionCandidatesField;
		}
		set
		{
			containsInterleavedExecutionCandidatesField = value;
		}
	}

	[XmlIgnore]
	public bool ContainsInterleavedExecutionCandidatesSpecified
	{
		get
		{
			return containsInterleavedExecutionCandidatesFieldSpecified;
		}
		set
		{
			containsInterleavedExecutionCandidatesFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public bool ContainsInlineScalarTsqlUdfs
	{
		get
		{
			return containsInlineScalarTsqlUdfsField;
		}
		set
		{
			containsInlineScalarTsqlUdfsField = value;
		}
	}

	[XmlIgnore]
	public bool ContainsInlineScalarTsqlUdfsSpecified
	{
		get
		{
			return containsInlineScalarTsqlUdfsFieldSpecified;
		}
		set
		{
			containsInlineScalarTsqlUdfsFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public int QueryVariantID
	{
		get
		{
			return queryVariantIDField;
		}
		set
		{
			queryVariantIDField = value;
		}
	}

	[XmlIgnore]
	public bool QueryVariantIDSpecified
	{
		get
		{
			return queryVariantIDFieldSpecified;
		}
		set
		{
			queryVariantIDFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public string DispatcherPlanHandle
	{
		get
		{
			return dispatcherPlanHandleField;
		}
		set
		{
			dispatcherPlanHandleField = value;
		}
	}

	[XmlAttribute]
	public bool ExclusiveProfileTimeActive
	{
		get
		{
			return exclusiveProfileTimeActiveField;
		}
		set
		{
			exclusiveProfileTimeActiveField = value;
		}
	}

	[XmlIgnore]
	public bool ExclusiveProfileTimeActiveSpecified
	{
		get
		{
			return exclusiveProfileTimeActiveFieldSpecified;
		}
		set
		{
			exclusiveProfileTimeActiveFieldSpecified = value;
		}
	}
}
