// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.OperationTable
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Serialization;
using BlackbirdSql.Shared.Controls.Graphing.Enums;


namespace BlackbirdSql.Shared.Controls.Graphing;

internal static class OperationTable
{
	private static readonly Dictionary<string, Operation> PhysicalOperations;

	private static readonly Dictionary<string, Operation> LogicalOperations;

	private static readonly Dictionary<string, Operation> Statements;

	private static readonly Dictionary<string, Operation> CursorTypes;

	public static Operation GetStatement(string statementTypeName)
	{
		if (!Statements.TryGetValue(statementTypeName, out var value))
		{
			return Operation.CreateUnknown(statementTypeName, "Language_construct_catch_all.ico");
		}
		return value;
	}

	public static Operation GetCursorType(string cursorTypeName)
	{
		if (!CursorTypes.TryGetValue(cursorTypeName, out var value))
		{
			cursorTypeName = GetNameFromXmlEnumAttribute(cursorTypeName, typeof(EnCursorType));
			return Operation.CreateUnknown(cursorTypeName, "Cursor_catch_all_32x.ico");
		}
		return value;
	}

	public static Operation GetPhysicalOperation(string operationType)
	{
		if (!PhysicalOperations.TryGetValue(operationType, out var value))
		{
			operationType = GetNameFromXmlEnumAttribute(operationType, typeof(EnPhysicalOpType));
			return Operation.CreateUnknown(operationType, "Iterator_catch_all.ico");
		}
		return value;
	}

	public static Operation GetLogicalOperation(string operationType)
	{
		if (!LogicalOperations.TryGetValue(operationType, out var value))
		{
			operationType = GetNameFromXmlEnumAttribute(operationType, typeof(EnLogicalOpType));
			return new Operation(null, operationType);
		}
		return value;
	}

	public static Operation GetUdf()
	{
		return new Operation(null, "Udf", null, "Language_construct_catch_all.ico", "");
	}

	public static Operation GetStoredProc()
	{
		return new Operation(null, "StoredProc", null, "Language_construct_catch_all.ico", "");
	}

	static OperationTable()
	{
		PhysicalOperations = DictionaryFromList(new Operation[116]
		{
			new Operation("AdaptiveJoin", "AdaptiveJoin", "AdaptiveJoinDescription", "Adaptive_Join_32x.ico", "sql13.swb.showplan.adaptivejoin.f1"),
			new Operation("Assert", "Assert", "AssertDescription", "Assert_32x.ico", "sql13.swb.showplan.assert.f1"),
			new Operation("Bitmap", "Bitmap", "BitmapDescription", "Bitmap_32x.ico", ""),
			new Operation("ClusteredIndexDelete", "ClusteredIndexDelete", "ClusteredIndexDeleteDescription", "Clustered_index_delete_32x.ico", "sql13.swb.showplan.clusteredindexdelete.f1"),
			new Operation("ClusteredIndexInsert", "ClusteredIndexInsert", "ClusteredIndexInsertDescription", "Clustered_index_insert_32x.ico", "sql13.swb.showplan.clusteredindexinsert.f1"),
			new Operation("ClusteredIndexScan", "ClusteredIndexScan", "ClusteredIndexScanDescription", "Clustered_index_scan_32x.ico", "sql13.swb.showplan.clusteredindexscan.f1"),
			new Operation("ClusteredIndexSeek", "ClusteredIndexSeek", "ClusteredIndexSeekDescription", "Clustered_index_seek_32x.ico", "sql13.swb.showplan.clusteredindexseek.f1"),
			new Operation("ClusteredIndexUpdate", "ClusteredIndexUpdate", "ClusteredIndexUpdateDescription", "Clustered_index_update_32x.ico", "sql13.swb.showplan.clusteredindexupdate.f1"),
			new Operation("ClusteredIndexMerge", "ClusteredIndexMerge", "ClusteredIndexMergeDescription", "Clustered_index_merge_32x.ico", "sql13.swb.showplan.clusteredindexmerge.f1"),
			new Operation("ClusteredUpdate", "ClusteredUpdate", "ClusteredUpdateDescription", "Clustered_update_32x.ico", ""),
			new Operation("Collapse", "Collapse", "CollapseDescription", "Collapse_32x.ico", "sql13.swb.showplan.collapse.f1"),
			new Operation("ComputeScalar", "ComputeScalar", "ComputeScalarDescription", "Compute_scalar_32x.ico", "sql13.swb.showplan.computescalar.f1"),
			new Operation("Concatenation", "Concatenation", "ConcatenationDescription", "Concatenation_32x.ico", "sql13.swb.showplan.concatenation.f1"),
			new Operation("ConstantScan", "ConstantScan", "ConstantScanDescription", "Constant_scan_32x.ico", "sql13.swb.showplan.constantscan.f1"),
			new Operation("DeletedScan", "DeletedScan", "DeletedScanDescription", "Deleted_scan_32x.ico", "sql13.swb.showplan.deletedscan.f1"),
			new Operation("Filter", "Filter", "FilterDescription", "Filter_32x.ico", "sql13.swb.showplan.filter.f1"),
			new Operation("HashMatch", "HashMatch", "HashMatchDescription", "Hash_match_32x.ico", "sql13.swb.showplan.hashmatch.f1"),
			new Operation("IndexDelete", "IndexDelete", "IndexDeleteDescription", "Nonclust_index_delete_32x.ico", "sql13.swb.showplan.indexdelete.f1"),
			new Operation("IndexInsert", "IndexInsert", "IndexInsertDescription", "Nonclust_index_insert_32x.ico", "sql13.swb.showplan.indexinsert.f1"),
			new Operation("IndexScan", "IndexScan", "IndexScanDescription", "Nonclust_index_scan_32x.ico", "sql13.swb.showplan.indexscan.f1"),
			new Operation("ColumnstoreIndexDelete", "ColumnstoreIndexDelete", "ColumnstoreIndexDeleteDescription", "Columnstore_index_delete_32x.ico", "sql13.swb.showplan.columnstoreindexdelete.f1"),
			new Operation("ColumnstoreIndexInsert", "ColumnstoreIndexInsert", "ColumnstoreIndexInsertDescription", "Columnstore_index_insert_32x.ico", "sql13.swb.showplan.columnstoreindexinsert.f1"),
			new Operation("ColumnstoreIndexMerge", "ColumnstoreIndexMerge", "ColumnstoreIndexMergeDescription", "Columnstore_index_merge_32x.ico", "sql13.swb.showplan.columnstoreindexmerge.f1"),
			new Operation("ColumnstoreIndexScan", "ColumnstoreIndexScan", "ColumnstoreIndexScanDescription", "Columnstore_index_scan_32x.ico", "sql13.swb.showplan.columnstoreindexscan.f1"),
			new Operation("ColumnstoreIndexUpdate", "ColumnstoreIndexUpdate", "ColumnstoreIndexUpdateDescription", "Columnstore_index_update_32x.ico", "sql13.swb.showplan.columnstoreindexupdate.f1"),
			new Operation("IndexSeek", "IndexSeek", "IndexSeekDescription", "Nonclust_index_seek_32x.ico", "sql13.swb.showplan.indexseek.f1"),
			new Operation("IndexSpool", "IndexSpool", "IndexSpoolDescription", "Nonclust_index_spool_32x.ico", "sql13.swb.showplan.indexspool.f1"),
			new Operation("IndexUpdate", "IndexUpdate", "IndexUpdateDescription", "Nonclust_index_update_32x.ico", "sql13.swb.showplan.indexupdate.f1"),
			new Operation("InsertedScan", "InsertedScan", "InsertedScanDescription", "Inserted_scan_32x.ico", "sql13.swb.showplan.insertedscan.f1"),
			new Operation("LogRowScan", "LogRowScan", "LogRowScanDescription", "Log_row_scan_32x.ico", "sql13.swb.showplan.logrowscan.f1"),
			new Operation("MergeInterval", "MergeInterval", "MergeIntervalDescription", "Merge_interval_32x.ico", ""),
			new Operation("MergeJoin", "MergeJoin", "MergeJoinDescription", "Merge_join_32x.ico", "sql13.swb.showplan.mergejoin.f1"),
			new Operation("NestedLoops", "NestedLoops", "NestedLoopsDescription", "Nested_loops_32x.ico", "sql13.swb.showplan.nestedloops.f1"),
			new Operation("Parallelism", "Parallelism", "ParallelismDescription", "Parallelism_32x.ico", "sql13.swb.showplan.parallelism.f1"),
			new Operation("ParameterTableScan", "ParameterTableScan", "ParameterTableScanDescription", "Parameter_table_scan_32x.ico", "sql13.swb.showplan.parametertablescan.f1"),
			new Operation("Print", "Print", "PrintDescription", "Print.ico", "sql13.swb.showplan.print.f1"),
			new Operation("Put", "Put", "PutDescription", "Put_32x.ico", "sql13.swb.showplan.put.f1"),
			new Operation("Rank", "Rank", "RankDescription", "Rank_32x.ico", "sql13.swb.showplan.rank.f1"),
			new Operation("ForeignKeyReferencesCheck", "ForeignKeyReferencesCheck", "ForeignKeyReferencesCheckDescription", "Referential_Integrity_32x.ico", "sql13.swb.showplan.ForeignKeyReferencesCheck.f1"),
			new Operation("RemoteDelete", "RemoteDelete", "RemoteDeleteDescription", "Remote_delete_32x.ico", "sql13.swb.showplan.remotedelete.f1"),
			new Operation("RemoteIndexScan", "RemoteIndexScan", "RemoteIndexScanDescription", "Remote_index_scan_32x.ico", "sql13.swb.showplan.remoteindexscan.f1"),
			new Operation("RemoteIndexSeek", "RemoteIndexSeek", "RemoteIndexSeekDescription", "Remote_index_seek_32x.ico", "sql13.swb.showplan.remoteindexseek.f1"),
			new Operation("RemoteInsert", "RemoteInsert", "RemoteInsertDescription", "Remote_insert_32x.ico", "sql13.swb.showplan.remoteinsert.f1"),
			new Operation("RemoteQuery", "RemoteQuery", "RemoteQueryDescription", "Remote_query_32x.ico", "sql13.swb.showplan.remotequery.f1"),
			new Operation("RemoteScan", "RemoteScan", "RemoteScanDescription", "Remote_scan_32x.ico", "sql13.swb.showplan.remotescan.f1"),
			new Operation("RemoteUpdate", "RemoteUpdate", "RemoteUpdateDescription", "Remote_update_32x.ico", "sql13.swb.showplan.remoteupdate.f1"),
			new Operation("RIDLookup", "RIDLookup", "RIDLookupDescription", "RID_clustered_locate_32x.ico", ""),
			new Operation("RowCountSpool", "RowCountSpool", "RowCountSpoolDescription", "Remote_count_spool_32x.ico", "sql13.swb.showplan.rowcountspool.f1"),
			new Operation("Segment", "Segment", "SegmentDescription", "Segment_32x.ico", ""),
			new Operation("Sequence", "Sequence", "SequenceDescription", "Sequence_32x.ico", "sql13.swb.showplan.sequence.f1"),
			new Operation("SequenceProject", "SequenceProject", "SequenceProjectDescription", "Sequence_project_32x.ico", ""),
			new Operation("Sort", "Sort", "SortDescription", "Sort_32x.ico", "sql13.swb.showplan.sort.f1"),
			new Operation("Split", "Split", "SplitDescription", "Split_32x.ico", ""),
			new Operation("StreamAggregate", "StreamAggregate", "StreamAggregateDescription", "Stream_aggregate_32x.ico", "sql13.swb.showplan.streamaggregate.f1"),
			new Operation("Switch", "Switch", "SwitchDescription", "Switch_32x.ico", ""),
			new Operation("Tablevaluedfunction", "TableValueFunction", "TableValueFunctionDescription", "Table_value_function_32x.ico", ""),
			new Operation("TableDelete", "TableDelete", "TableDeleteDescription", "Table_delete_32x.ico", "sql13.swb.showplan.tabledelete.f1"),
			new Operation("TableInsert", "TableInsert", "TableInsertDescription", "Table_insert_32x.ico", "sql13.swb.showplan.tableinsert.f1"),
			new Operation("TableScan", "TableScan", "TableScanDescription", "Table_scan_32x.ico", "sql13.swb.showplan.tablescan.f1"),
			new Operation("TableSpool", "TableSpool", "TableSpoolDescription", "Table_spool_32x.ico", "sql13.swb.showplan.tablespool.f1"),
			new Operation("TableUpdate", "TableUpdate", "TableUpdateDescription", "Table_update_32x.ico", "sql13.swb.showplan.tableupdate.f1"),
			new Operation("TableMerge", "TableMerge", "TableMergeDescription", "Table_merge_32x.ico", "sql13.swb.showplan.tablemerge.f1"),
			new Operation("TFP", "TFP", "TFPDescription", "Predict_32x.ico", ""),
			new Operation("Top", "Top", "TopDescription", "Top_32x.ico", "sql13.swb.showplan.top.f1"),
			new Operation("UDX", "UDX", "UDXDescription", "UDX_32x.ico", ""),
			new Operation("BatchHashTableBuild", "BatchHashTableBuild", "BatchHashTableBuildDescription", "BatchHashTableBuild_32x.ico", "sql13.swb.showplan.buildhash.f1"),
			new Operation("WindowSpool", "Window", "WindowDescription", "Table_spool_32x.ico", ""),
			new Operation("WindowAggregate", "WindowAggregate", "WindowAggregateDescription", "Window_aggregate_32x.ico", ""),
			new Operation("FetchQuery", "FetchQuery", "FetchQueryDescription", "Fetch_query_32x.ico", ""),
			new Operation("PopulateQuery", "PopulationQuery", "PopulationQueryDescription", "Population_query_32x.ico", ""),
			new Operation("RefreshQuery", "RefreshQuery", "RefreshQueryDescription", "Refresh_query_32x.ico", ""),
			new Operation("Result", "Result", "ResultDescription", "Result_32x.ico", "sql13.swb.showplan.result.f1"),
			new Operation("Aggregate", "Aggregate", "AggregateDescription", "Aggregate_32x.ico", "sql13.swb.showplan.aggregate.f1"),
			new Operation("Assign", "Assign", "AssignDescription", "Assign_32x.ico", "sql13.swb.showplan.assign.f1"),
			new Operation("ArithmeticExpression", "ArithmeticExpression", "ArithmeticExpressionDescription", "Arithmetic_expression_32x.ico", "sql13.swb.showplan.arithmeticexpression.f1"),
			new Operation("BookmarkLookup", "BookmarkLookup", "BookmarkLookupDescription", "Bookmark_lookup_32x.ico", "sql13.swb.showplan.bookmarklookup.f1"),
			new Operation("Convert", "Convert", "ConvertDescription", "Convert_32x.ico", "sql13.swb.showplan.convert.f1"),
			new Operation("Declare", "Declare", "DeclareDescription", "Declare_32x.ico", "sql13.swb.showplan.declare.f1"),
			new Operation("Delete", "Delete", "DeleteDescription", "Delete_32x.ico", "sql13.swb.showplan.delete.f1"),
			new Operation("Dynamic", "Dynamic", "DynamicDescription", "Dynamic_32x.ico", "sql13.swb.showplan.dynamic.f1"),
			new Operation("HashMatchRoot", "HashMatchRoot", "HashMatchRootDescription", "Hash_match_root_32x.ico", "sql13.swb.showplan.hashmatchroot.f1"),
			new Operation("HashMatchTeam", "HashMatchTeam", "HashMatchTeamDescription", "Hash_match_team_32x.ico", "sql13.swb.showplan.hashmatchteam.f1"),
			new Operation("If", "If", "IfDescription", "If_32x.ico", "sql13.swb.showplan.if.f1"),
			new Operation("Insert", "Insert", "InsertDescription", "Insert_32x.ico", "sql13.swb.showplan.insert.f1"),
			new Operation("Intrinsic", "Intrinsic", "IntrinsicDescription", "Intrinsic_32x.ico", "sql13.swb.showplan.intrinsic.f1"),
			new Operation("Keyset", "Keyset", "KeysetDescription", "Keyset_32x.ico", "sql13.swb.showplan.keyset.f1"),
			new Operation("Locate", "Locate", "LocateDescription", "RID_nonclustered_locate_32x.ico", "sql13.swb.showplan.locate.f1"),
			new Operation("PopulationQuery", "PopulationQuery", "PopulationQueryDescription", "Population_query_32x.ico", "sql13.swb.showplan.populationquery.f1"),
			new Operation("SetFunction", "SetFunction", "SetFunctionDescription", "Set_function_32x.ico", "sql13.swb.showplan.setfunction.f1"),
			new Operation("Snapshot", "Snapshot", "SnapshotDescription", "Snapshot_32x.ico", "sql13.swb.showplan.snapshot.f1"),
			new Operation("Spool", "Spool", "SpoolDescription", "Spool_32x.ico", "sql13.swb.showplan.spool.f1"),
			new Operation("TSQL", "SQL", "SQLDescription", "SQL_32x.ico", "sql13.swb.showplan.sql.f1"),
			new Operation("Update", "Update", "UpdateDescription", "Update_32x.ico", "sql13.swb.showplan.update.f1"),
			new Operation("KeyLookup", "KeyLookup", "KeyLookupDescription", "Bookmark_lookup_32x.ico", "sql13.swb.showplan.keylookup.f1"),
			new Operation("Apply", "Apply", "ApplyDescription", "Apply_32x.ico", ""),
			new Operation("Broadcast", "Broadcast", "BroadcastDescription", "Broadcast_32x.ico", ""),
			new Operation("ComputeToControlNode", "ComputeToControlNode", "ComputeToControlNodeDescription", "Compute_to_control_32x.ico", ""),
			new Operation("ConstTableGet", "ConstTableGet", "ConstTableGetDescription", "Const_table_get_32x.ico", ""),
			new Operation("ControlToComputeNodes", "ControlToComputeNodes", "ControlToComputeNodesDescription", "Control_to_compute_32x.ico", ""),
			new Operation("ExternalBroadcast", "ExternalBroadcast", "ExternalBroadcastDescription", "External_broadcast_32x.ico", ""),
			new Operation("ExternalExport", "ExternalExport", "ExternalExportDescription", "External_export_32x.ico", ""),
			new Operation("ExternalLocalStreaming", "ExternalLocalStreaming", "ExternalLocalStreamingDescription", "External_local_streaming_32x.ico", ""),
			new Operation("ExternalRoundRobin", "ExternalRoundRobin", "ExternalRoundRobinDescription", "External_round_robin_32x.ico", ""),
			new Operation("ExternalShuffle", "ExternalShuffle", "ExternalShuffleDescription", "External_shuffle_32x.ico", ""),
			new Operation("Get", "Get", "GetDescription", "Get_32x.ico", ""),
			new Operation("GbApply", "GbApply", "GbApplyDescription", "Apply_32x.ico", ""),
			new Operation("GbAgg", "GbAgg", "GbAggDescription", "Group_by_aggregate_32x.ico", ""),
			new Operation("Join", "Join", "JoinDescription", "Join_32x.ico", ""),
			new Operation("LocalCube", "LocalCube", "LocalCubeDescription", "Intrinsic_32x.ico", ""),
			new Operation("Project", "Project", "ProjectDescription", "Project_32x.ico", ""),
			new Operation("Shuffle", "Shuffle", "ShuffleDescription", "Shuffle_32x.ico", ""),
			new Operation("SingleSourceRoundRobin", "SingleSourceRoundRobin", "SingleSourceRoundRobinDescription", "Single_source_round_robin_32x.ico", ""),
			new Operation("SingleSourceShuffle", "SingleSourceShuffle", "SingleSourceShuffleDescription", "Single_source_shuffle_32x.ico", ""),
			new Operation("Trim", "Trim", "TrimDescription", "Trim_32x.ico", ""),
			new Operation("Union", "Union", "UnionDescription", "Union_32x.ico", ""),
			new Operation("UnionAll", "UnionAll", "UnionAllDescription", "Union_all_32x.ico", "")
		});
		LogicalOperations = DictionaryFromList(new Operation[79]
		{
			new Operation("Aggregate", "LogicalOpAggregate"),
			new Operation("AntiDiff", "LogicalOpAntiDiff"),
			new Operation("Assert", "LogicalOpAssert"),
			new Operation("BitmapCreate", "LogicalOpBitmapCreate"),
			new Operation("ClusteredIndexScan", "LogicalOpClusteredIndexScan"),
			new Operation("ClusteredIndexSeek", "LogicalOpClusteredIndexSeek"),
			new Operation("ClusteredUpdate", "LogicalOpClusteredUpdate"),
			new Operation("Collapse", "LogicalOpCollapse"),
			new Operation("ComputeScalar", "LogicalOpComputeScalar"),
			new Operation("Concatenation", "LogicalOpConcatenation"),
			new Operation("ConstantScan", "LogicalOpConstantScan"),
			new Operation("CrossJoin", "LogicalOpCrossJoin"),
			new Operation("Delete", "LogicalOpDelete"),
			new Operation("DeletedScan", "LogicalOpDeletedScan"),
			new Operation("DistinctSort", "LogicalOpDistinctSort"),
			new Operation("Distinct", "LogicalOpDistinct"),
			new Operation("DistributeStreams", "LogicalOpDistributeStreams", "DistributeStreamsDescription", "Parallelism_distribute.ico", ""),
			new Operation("EagerSpool", "LogicalOpEagerSpool"),
			new Operation("Filter", "LogicalOpFilter"),
			new Operation("FlowDistinct", "LogicalOpFlowDistinct"),
			new Operation("FullOuterJoin", "LogicalOpFullOuterJoin"),
			new Operation("GatherStreams", "LogicalOpGatherStreams", "GatherStreamsDescription", "Parallelism_32x.ico", ""),
			new Operation("IndexScan", "LogicalOpIndexScan"),
			new Operation("IndexSeek", "LogicalOpIndexSeek"),
			new Operation("InnerApply", "LogicalOpInnerApply"),
			new Operation("InnerJoin", "LogicalOpInnerJoin"),
			new Operation("Insert", "LogicalOpInsert"),
			new Operation("InsertedScan", "LogicalOpInsertedScan"),
			new Operation("IntersectAll", "LogicalOpIntersectAll"),
			new Operation("Intersect", "LogicalOpIntersect"),
			new Operation("KeyLookup", "LogicalKeyLookup"),
			new Operation("LazySpool", "LogicalOpLazySpool"),
			new Operation("LeftAntiSemiApply", "LogicalOpLeftAntiSemiApply"),
			new Operation("LeftAntiSemiJoin", "LogicalOpLeftAntiSemiJoin"),
			new Operation("LeftDiffAll", "LogicalOpLeftDiffAll"),
			new Operation("LeftDiff", "LogicalOpLeftDiff"),
			new Operation("LeftOuterApply", "LogicalOpLeftOuterApply"),
			new Operation("LeftOuterJoin", "LogicalOpLeftOuterJoin"),
			new Operation("LeftSemiApply", "LogicalOpLeftSemiApply"),
			new Operation("LeftSemiJoin", "LogicalOpLeftSemiJoin"),
			new Operation("LogRowScan", "LogicalOpLogRowScan"),
			new Operation("MergeInterval", "LogicalOpMergeInterval"),
			new Operation("ParameterTableScan", "LogicalOpParameterTableScan"),
			new Operation("PartialAggregate", "LogicalOpPartialAggregate"),
			new Operation("Print", "LogicalOpPrint"),
			new Operation("Put", "LogicalOpPut"),
			new Operation("Rank", "LogicalOpRank"),
			new Operation("ForeignKeyReferencesCheck", "LogicalOpForeignKeyReferencesCheck"),
			new Operation("RemoteDelete", "LogicalOpRemoteDelete"),
			new Operation("RemoteIndexScan", "LogicalOpRemoteIndexScan"),
			new Operation("RemoteIndexSeek", "LogicalOpRemoteIndexSeek"),
			new Operation("RemoteInsert", "LogicalOpRemoteInsert"),
			new Operation("RemoteQuery", "LogicalOpRemoteQuery"),
			new Operation("RemoteScan", "LogicalOpRemoteScan"),
			new Operation("RemoteUpdate", "LogicalOpRemoteUpdate"),
			new Operation("RepartitionStreams", "LogicalOpRepartitionStreams", "RepartitionStreamsDescription", "Parallelism_repartition.ico", ""),
			new Operation("RIDLookup", "LogicalOpRIDLookup"),
			new Operation("RightAntiSemiJoin", "LogicalOpRightAntiSemiJoin"),
			new Operation("RightDiffAll", "LogicalOpRightDiffAll"),
			new Operation("RightDiff", "LogicalOpRightDiff"),
			new Operation("RightOuterJoin", "LogicalOpRightOuterJoin"),
			new Operation("RightSemiJoin", "LogicalOpRightSemiJoin"),
			new Operation("Segment", "LogicalOpSegment"),
			new Operation("Sequence", "LogicalOpSequence"),
			new Operation("Sort", "LogicalOpSort"),
			new Operation("Split", "LogicalOpSplit"),
			new Operation("Switch", "LogicalOpSwitch"),
			new Operation("Tablevaluedfunction", "LogicalOpTableValuedFunction"),
			new Operation("TableScan", "LogicalOpTableScan"),
			new Operation("Top", "LogicalOpTop"),
			new Operation("TopNSort", "LogicalOpTopNSort"),
			new Operation("UDX", "LogicalOpUDX"),
			new Operation("Union", "LogicalOpUnion"),
			new Operation("Update", "LogicalOpUpdate"),
			new Operation("Merge", "LogicalOpMerge"),
			new Operation("MergeStats", "LogicalOpMergeStats"),
			new Operation("LocalStats", "LogicalOpLocalStats"),
			new Operation("BatchHashTableBuild", "LogicalOpBatchHashTableBuild"),
			new Operation("WindowSpool", "LogicalOpWindow")
		});
		Statements = DictionaryFromList(new Operation[2]
		{
			new Operation("SELECT", null, null, "Result_32x.ico", ""),
			new Operation("COND", null, null, "If_32x.ico", "")
		});
		CursorTypes = DictionaryFromList(new Operation[4]
		{
			new Operation("Dynamic", "Dynamic", "DynamicDescription", "Dynamic_32x.ico", ""),
			new Operation("FastForward", "FastForward", "FastForwardDescription", "Cursor_catch_all_32x.ico", ""),
			new Operation("Keyset", "Keyset", "KeysetDescription", "Keyset_32x.ico", ""),
			new Operation("Snapshot", "Snapshot", "SnapshotDescription", "Snapshot_32x.ico", "")
		});
	}

	private static Dictionary<string, Operation> DictionaryFromList(Operation[] list)
	{
		Dictionary<string, Operation> dictionary = new Dictionary<string, Operation>(list.Length);
		foreach (Operation operation in list)
		{
			dictionary.Add(operation.Name, operation);
		}
		return dictionary;
	}

	private static string GetNameFromXmlEnumAttribute(string enumMemberName, Type enumType)
	{
		MemberInfo[] members = enumType.GetMembers();
		foreach (MemberInfo memberInfo in members)
		{
			if (memberInfo.Name == enumMemberName)
			{
				object[] customAttributes = memberInfo.GetCustomAttributes(typeof(XmlEnumAttribute), inherit: true);
				int num = 0;
				if (num < customAttributes.Length)
				{
					return ((XmlEnumAttribute)customAttributes[num]).Name;
				}
				break;
			}
		}
		return enumMemberName;
	}
}
