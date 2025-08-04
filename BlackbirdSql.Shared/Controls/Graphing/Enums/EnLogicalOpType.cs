// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.LogicalOpType
using System;
using System.CodeDom.Compiler;
using System.Xml.Serialization;


namespace BlackbirdSql.Shared.Controls.Graphing.Enums;

[Serializable]
[GeneratedCode("xsd", "4.8.3928.0")]
[XmlType(Namespace = LibraryData.C_ShowPlanNamespace)]
internal enum EnLogicalOpType
{
	Aggregate,
	[XmlEnum("Anti Diff")]
	AntiDiff,
	Assert,
	[XmlEnum("Async Concat")]
	AsyncConcat,
	[XmlEnum("Batch Hash Table Build")]
	BatchHashTableBuild,
	[XmlEnum("Bitmap Create")]
	BitmapCreate,
	[XmlEnum("Clustered Index Scan")]
	ClusteredIndexScan,
	[XmlEnum("Clustered Index Seek")]
	ClusteredIndexSeek,
	[XmlEnum("Clustered Update")]
	ClusteredUpdate,
	Collapse,
	[XmlEnum("Compute Scalar")]
	ComputeScalar,
	Concatenation,
	[XmlEnum("Constant Scan")]
	ConstantScan,
	[XmlEnum("Constant Table Get")]
	ConstantTableGet,
	[XmlEnum("Cross Join")]
	CrossJoin,
	Delete,
	[XmlEnum("Deleted Scan")]
	DeletedScan,
	[XmlEnum("Distinct Sort")]
	DistinctSort,
	Distinct,
	[XmlEnum("Distribute Streams")]
	DistributeStreams,
	[XmlEnum("Eager Spool")]
	EagerSpool,
	[XmlEnum("External Extraction Scan")]
	ExternalExtractionScan,
	[XmlEnum("External Select")]
	ExternalSelect,
	Filter,
	[XmlEnum("Flow Distinct")]
	FlowDistinct,
	[XmlEnum("Foreign Key References Check")]
	ForeignKeyReferencesCheck,
	[XmlEnum("Full Outer Join")]
	FullOuterJoin,
	[XmlEnum("Gather Streams")]
	GatherStreams,
	GbAgg,
	GbApply,
	Get,
	Generic,
	[XmlEnum("Inner Apply")]
	InnerApply,
	[XmlEnum("Index Scan")]
	IndexScan,
	[XmlEnum("Index Seek")]
	IndexSeek,
	[XmlEnum("Inner Join")]
	InnerJoin,
	Insert,
	[XmlEnum("Inserted Scan")]
	InsertedScan,
	Intersect,
	[XmlEnum("Intersect All")]
	IntersectAll,
	[XmlEnum("Lazy Spool")]
	LazySpool,
	[XmlEnum("Left Anti Semi Apply")]
	LeftAntiSemiApply,
	[XmlEnum("Left Semi Apply")]
	LeftSemiApply,
	[XmlEnum("Left Outer Apply")]
	LeftOuterApply,
	[XmlEnum("Left Anti Semi Join")]
	LeftAntiSemiJoin,
	[XmlEnum("Left Diff")]
	LeftDiff,
	[XmlEnum("Left Diff All")]
	LeftDiffAll,
	[XmlEnum("Left Outer Join")]
	LeftOuterJoin,
	[XmlEnum("Left Semi Join")]
	LeftSemiJoin,
	LocalCube,
	[XmlEnum("Log Row Scan")]
	LogRowScan,
	Merge,
	[XmlEnum("Merge Interval")]
	MergeInterval,
	[XmlEnum("Merge Stats")]
	MergeStats,
	Move,
	[XmlEnum("Parameter Table Scan")]
	ParameterTableScan,
	[XmlEnum("Partial Aggregate")]
	PartialAggregate,
	Print,
	Project,
	Put,
	Rank,
	[XmlEnum("Remote Delete")]
	RemoteDelete,
	[XmlEnum("Remote Index Scan")]
	RemoteIndexScan,
	[XmlEnum("Remote Index Seek")]
	RemoteIndexSeek,
	[XmlEnum("Remote Insert")]
	RemoteInsert,
	[XmlEnum("Remote Query")]
	RemoteQuery,
	[XmlEnum("Remote Scan")]
	RemoteScan,
	[XmlEnum("Remote Update")]
	RemoteUpdate,
	[XmlEnum("Repartition Streams")]
	RepartitionStreams,
	[XmlEnum("RID Lookup")]
	RIDLookup,
	[XmlEnum("Right Anti Semi Join")]
	RightAntiSemiJoin,
	[XmlEnum("Right Diff")]
	RightDiff,
	[XmlEnum("Right Diff All")]
	RightDiffAll,
	[XmlEnum("Right Outer Join")]
	RightOuterJoin,
	[XmlEnum("Right Semi Join")]
	RightSemiJoin,
	Segment,
	Sequence,
	Sort,
	Split,
	Switch,
	[XmlEnum("Table-valued function")]
	Tablevaluedfunction,
	[XmlEnum("Table Scan")]
	TableScan,
	Top,
	[XmlEnum("TopN Sort")]
	TopNSort,
	UDX,
	Union,
	[XmlEnum("Union All")]
	UnionAll,
	Update,
	[XmlEnum("Local Stats")]
	LocalStats,
	[XmlEnum("Window Spool")]
	WindowSpool,
	[XmlEnum("Window Aggregate")]
	WindowAggregate,
	[XmlEnum("Key Lookup")]
	KeyLookup,
	[XmlEnum("Extensible Column Store Scan")]
	ExtensibleColumnStoreScan
}
