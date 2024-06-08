// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.ItemChoiceType
using System;
using System.CodeDom.Compiler;
using System.Xml.Serialization;


namespace BlackbirdSql.Shared.Controls.Graphing.Enums;

[Serializable]
[GeneratedCode("xsd", "4.8.3928.0")]
[XmlType(Namespace = LibraryData.C_ShowPlanNamespace, IncludeInSchema = false)]
public enum EnItemChoiceType
{
	AdaptiveJoin,
	Apply,
	Assert,
	BatchHashTableBuild,
	Bitmap,
	Collapse,
	ComputeScalar,
	Concat,
	ConstTableGet,
	ConstantScan,
	CreateIndex,
	Delete,
	DeletedScan,
	ExtExtractScan,
	Extension,
	ExternalSelect,
	Filter,
	ForeignKeyReferencesCheck,
	GbAgg,
	GbApply,
	Generic,
	Get,
	Hash,
	IndexScan,
	Insert,
	InsertedScan,
	Join,
	LocalCube,
	LogRowScan,
	Merge,
	MergeInterval,
	Move,
	NestedLoops,
	OnlineIndex,
	Parallelism,
	ParameterTableScan,
	PrintDataflow,
	Project,
	Put,
	RemoteFetch,
	RemoteModify,
	RemoteQuery,
	RemoteRange,
	RemoteScan,
	RowCountSpool,
	ScalarInsert,
	Segment,
	Sequence,
	SequenceProject,
	SimpleUpdate,
	Sort,
	Split,
	Spool,
	StreamAggregate,
	Switch,
	TableScan,
	TableValuedFunction,
	Top,
	TopSort,
	Union,
	UnionAll,
	Update,
	WindowAggregate,
	WindowSpool,
	XcsScan
}
