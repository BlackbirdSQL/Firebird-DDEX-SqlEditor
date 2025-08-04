// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.PhysicalOpType
using System;
using System.CodeDom.Compiler;
using System.Xml.Serialization;


namespace BlackbirdSql.Shared.Controls.Graphing.Enums;

[Serializable]
[GeneratedCode("xsd", "4.8.3928.0")]
[XmlType(Namespace = LibraryData.C_ShowPlanNamespace)]
internal enum EnPhysicalOpType
{
	[XmlEnum("Adaptive Join")]
	AdaptiveJoin,
	Apply,
	Assert,
	[XmlEnum("Batch Hash Table Build")]
	BatchHashTableBuild,
	Bitmap,
	Broadcast,
	[XmlEnum("Clustered Index Delete")]
	ClusteredIndexDelete,
	[XmlEnum("Clustered Index Insert")]
	ClusteredIndexInsert,
	[XmlEnum("Clustered Index Scan")]
	ClusteredIndexScan,
	[XmlEnum("Clustered Index Seek")]
	ClusteredIndexSeek,
	[XmlEnum("Clustered Index Update")]
	ClusteredIndexUpdate,
	[XmlEnum("Clustered Index Merge")]
	ClusteredIndexMerge,
	[XmlEnum("Clustered Update")]
	ClusteredUpdate,
	Collapse,
	[XmlEnum("Columnstore Index Delete")]
	ColumnstoreIndexDelete,
	[XmlEnum("Columnstore Index Insert")]
	ColumnstoreIndexInsert,
	[XmlEnum("Columnstore Index Merge")]
	ColumnstoreIndexMerge,
	[XmlEnum("Columnstore Index Scan")]
	ColumnstoreIndexScan,
	[XmlEnum("Columnstore Index Update")]
	ColumnstoreIndexUpdate,
	[XmlEnum("Compute Scalar")]
	ComputeScalar,
	[XmlEnum("Compute To Control Node")]
	ComputeToControlNode,
	Concatenation,
	[XmlEnum("Constant Scan")]
	ConstantScan,
	[XmlEnum("Constant Table Get")]
	ConstantTableGet,
	[XmlEnum("Control To Compute Nodes")]
	ControlToComputeNodes,
	Delete,
	[XmlEnum("Deleted Scan")]
	DeletedScan,
	[XmlEnum("External Broadcast")]
	ExternalBroadcast,
	[XmlEnum("External Extraction Scan")]
	ExternalExtractionScan,
	[XmlEnum("External Local Streaming")]
	ExternalLocalStreaming,
	[XmlEnum("External Round Robin")]
	ExternalRoundRobin,
	[XmlEnum("External Select")]
	ExternalSelect,
	[XmlEnum("External Shuffle")]
	ExternalShuffle,
	Filter,
	[XmlEnum("Foreign Key References Check")]
	ForeignKeyReferencesCheck,
	GbAgg,
	GbApply,
	Get,
	Generic,
	[XmlEnum("Hash Match")]
	HashMatch,
	[XmlEnum("Index Delete")]
	IndexDelete,
	[XmlEnum("Index Insert")]
	IndexInsert,
	[XmlEnum("Index Scan")]
	IndexScan,
	Insert,
	Join,
	[XmlEnum("Index Seek")]
	IndexSeek,
	[XmlEnum("Index Spool")]
	IndexSpool,
	[XmlEnum("Index Update")]
	IndexUpdate,
	[XmlEnum("Inserted Scan")]
	InsertedScan,
	LocalCube,
	[XmlEnum("Log Row Scan")]
	LogRowScan,
	[XmlEnum("Merge Interval")]
	MergeInterval,
	[XmlEnum("Merge Join")]
	MergeJoin,
	[XmlEnum("Nested Loops")]
	NestedLoops,
	[XmlEnum("Online Index Insert")]
	OnlineIndexInsert,
	Parallelism,
	[XmlEnum("Parameter Table Scan")]
	ParameterTableScan,
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
	[XmlEnum("RID Lookup")]
	RIDLookup,
	[XmlEnum("Row Count Spool")]
	RowCountSpool,
	Segment,
	Sequence,
	[XmlEnum("Sequence Project")]
	SequenceProject,
	Shuffle,
	[XmlEnum("Single Source Round Robin Move")]
	SingleSourceRoundRobinMove,
	Sort,
	Split,
	[XmlEnum("Stream Aggregate")]
	StreamAggregate,
	Switch,
	[XmlEnum("Table Delete")]
	TableDelete,
	[XmlEnum("Table Insert")]
	TableInsert,
	[XmlEnum("Table Merge")]
	TableMerge,
	[XmlEnum("Table Scan")]
	TableScan,
	[XmlEnum("Table Spool")]
	TableSpool,
	[XmlEnum("Table Update")]
	TableUpdate,
	[XmlEnum("Table-valued function")]
	Tablevaluedfunction,
	Top,
	Trim,
	UDX,
	Union,
	[XmlEnum("Union All")]
	UnionAll,
	[XmlEnum("Window Aggregate")]
	WindowAggregate,
	[XmlEnum("Window Spool")]
	WindowSpool,
	[XmlEnum("Key Lookup")]
	KeyLookup,
	[XmlEnum("Extensible Column Store Scan")]
	ExtensibleColumnStoreScan
}
