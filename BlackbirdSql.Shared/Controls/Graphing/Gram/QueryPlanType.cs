// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.QueryPlanType
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;


namespace BlackbirdSql.Shared.Controls.Graphing.Gram;

[Serializable]
[GeneratedCode("xsd", "4.8.3928.0")]
[DebuggerStepThrough]
[DesignerCategory("code")]
[XmlType(Namespace = LibraryData.C_ShowPlanNamespace)]
internal class QueryPlanType
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

	internal OptimizationReplayType OptimizationReplay
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

	internal ThreadStatType ThreadStat
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
	internal MissingIndexGroupType[] MissingIndexes
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

	internal GuessedSelectivityType GuessedSelectivity
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

	internal UnmatchedIndexesType UnmatchedIndexes
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

	internal MemoryGrantType MemoryGrantInfo
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

	internal OptimizerHardwareDependentPropertiesType OptimizerHardwareDependentProperties
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
	internal StatsInfoType[] OptimizerStatsUsage
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
	internal TraceFlagListType[] TraceFlags
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
	internal WaitStatType[] WaitStats
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

	internal QueryExecTimeType QueryTimeStats
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

	internal RelOpType RelOp
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
	internal ColumnReferenceType[] ParameterList
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
	internal int DegreeOfParallelism
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
	internal bool DegreeOfParallelismSpecified
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
	internal int EffectiveDegreeOfParallelism
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
	internal bool EffectiveDegreeOfParallelismSpecified
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
	internal string NonParallelPlanReason
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
	internal ulong MemoryGrant
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
	internal bool MemoryGrantSpecified
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
	internal ulong CachedPlanSize
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
	internal bool CachedPlanSizeSpecified
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
	internal ulong CompileTime
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
	internal bool CompileTimeSpecified
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
	internal ulong CompileCPU
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
	internal bool CompileCPUSpecified
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
	internal ulong CompileMemory
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
	internal bool CompileMemorySpecified
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
	internal bool UsePlan
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
	internal bool UsePlanSpecified
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
	internal bool ContainsInterleavedExecutionCandidates
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
	internal bool ContainsInterleavedExecutionCandidatesSpecified
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
	internal bool ContainsInlineScalarTsqlUdfs
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
	internal bool ContainsInlineScalarTsqlUdfsSpecified
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
	internal int QueryVariantID
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
	internal bool QueryVariantIDSpecified
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
	internal string DispatcherPlanHandle
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
	internal bool ExclusiveProfileTimeActive
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
	internal bool ExclusiveProfileTimeActiveSpecified
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
