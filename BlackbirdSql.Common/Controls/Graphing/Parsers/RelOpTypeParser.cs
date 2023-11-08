// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.RelOpTypeParser
using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using BlackbirdSql.Common.Controls.Graphing.ComponentModel;
using BlackbirdSql.Common.Controls.Graphing.Enums;
using BlackbirdSql.Common.Controls.Graphing.Gram;
using BlackbirdSql.Common.Properties;

namespace BlackbirdSql.Common.Controls.Graphing.Parsers;

internal sealed class RelOpTypeParser : AbstractXmlPlanParser
{
	private const string C_OPERATION_INDEX_DELETE = "IndexDelete";

	private const string C_OPERATION_CLUSTERED_INDEX_DELETE = "ClusteredIndexDelete";

	private const string C_OPERATION_COLUMNSTORE_INDEX_DELETE = "ColumnstoreIndexDelete";

	private const string C_OPERATION_INDEX_INSERT = "IndexInsert";

	private const string C_OPERATION_CLUSTERED_INDEX_INSERT = "ClusteredIndexInsert";

	private const string C_OPERATION_COLUMNSTORE_INDEX_INSERT = "ColumnstoreIndexInsert";

	private const string C_OPERATION_INDEX_MERGE = "IndexMerge";

	private const string C_OPERATION_CLUSTERED_INDEX_MERGE = "ClusteredIndexMerge";

	private const string C_OPERATION_COLUMNSTORE_INDEX_MERGE = "ColumnstoreIndexMerge";

	private const string C_OPERATION_INDEX_SCAN = "IndexScan";

	private const string C_OPERATION_CLUSTERED_INDEX_SCAN = "ClusteredIndexScan";

	private const string C_OPERATION_COLUMNSTORE_INDEX_SCAN = "ColumnstoreIndexScan";

	private const string C_OPERATION_INDEX_UPDATE = "IndexUpdate";

	private const string C_OPERATION_CLUSTERED_INDEX_UPDATE = "ClusteredIndexUpdate";

	private const string C_OPERATION_COLUMNSTORE_INDEX_UPDATE = "ColumnstoreIndexUpdate";

	private const string C_OBJECT_NODE = "Object";

	private const string C_STORAGE_PROPERTY = "Storage";

	private static RelOpTypeParser relOpTypeParser;

	public static RelOpTypeParser Instance
	{
		get
		{
			relOpTypeParser ??= new RelOpTypeParser();
			return relOpTypeParser;
		}
	}

	public override Node GetCurrentNode(object item, object parentItem, Node parentNode, NodeBuilderContext context)
	{
		return AbstractXmlPlanParser.NewNode(context);
	}

	public override IEnumerable GetChildren(object parsedItem)
	{
		RelOpType relOpType = parsedItem as RelOpType;
		if (relOpType.Item != null)
		{
			yield return relOpType.Item;
		}
	}

	protected override Operation GetNodeOperation(Node node)
	{
		object obj = node["PhysicalOp"];
		object obj2 = node["LogicalOp"];
		if (obj == null || obj2 == null)
		{
			throw new FormatException(ControlsResources.UnknownExecutionPlanSource);
		}
		string text = obj.ToString();
		string text2 = obj2.ToString();
		object obj3 = node["Lookup"];
		if (obj3 != null && obj3 is bool && Convert.ToBoolean(obj3))
		{
			if (string.Compare(text, "ClusteredIndexSeek", StringComparison.OrdinalIgnoreCase) == 0)
			{
				text = "KeyLookup";
			}
			if (string.Compare(text2, "ClusteredIndexSeek", StringComparison.OrdinalIgnoreCase) == 0)
			{
				text2 = "KeyLookup";
			}
		}
		if (string.Compare(text, C_OPERATION_INDEX_SCAN, StringComparison.OrdinalIgnoreCase) == 0 || string.Compare(text, C_OPERATION_CLUSTERED_INDEX_SCAN, StringComparison.OrdinalIgnoreCase) == 0)
		{
			object obj4 = node[C_STORAGE_PROPERTY];
			if (obj4 != null && obj4.Equals(EnStorageType.ColumnStore))
			{
				text = C_OPERATION_COLUMNSTORE_INDEX_SCAN;
			}
		}
		else
		{
			ExpandableObjectWrapper expandableObjectWrapper = (ExpandableObjectWrapper)node[C_OBJECT_NODE];
			if (expandableObjectWrapper != null)
			{
				PropertyValue propertyValue = (PropertyValue)expandableObjectWrapper.Properties[C_STORAGE_PROPERTY];
				if (propertyValue != null && propertyValue.Value.Equals(EnStorageType.ColumnStore))
				{
					if (string.Compare(text, C_OPERATION_INDEX_DELETE, StringComparison.OrdinalIgnoreCase) == 0 || string.Compare(text, C_OPERATION_CLUSTERED_INDEX_DELETE, StringComparison.OrdinalIgnoreCase) == 0)
					{
						text = C_OPERATION_COLUMNSTORE_INDEX_DELETE;
					}
					else if (string.Compare(text, C_OPERATION_INDEX_INSERT, StringComparison.OrdinalIgnoreCase) == 0 || string.Compare(text, C_OPERATION_CLUSTERED_INDEX_INSERT, StringComparison.OrdinalIgnoreCase) == 0)
					{
						text = C_OPERATION_COLUMNSTORE_INDEX_INSERT;
					}
					else if (string.Compare(text, C_OPERATION_INDEX_MERGE, StringComparison.OrdinalIgnoreCase) == 0 || string.Compare(text, C_OPERATION_CLUSTERED_INDEX_MERGE, StringComparison.OrdinalIgnoreCase) == 0)
					{
						text = C_OPERATION_COLUMNSTORE_INDEX_MERGE;
					}
					else if (string.Compare(text, C_OPERATION_INDEX_UPDATE, StringComparison.OrdinalIgnoreCase) == 0 || string.Compare(text, C_OPERATION_CLUSTERED_INDEX_UPDATE, StringComparison.OrdinalIgnoreCase) == 0)
					{
						text = C_OPERATION_COLUMNSTORE_INDEX_UPDATE;
					}
				}
			}
		}
		Operation physicalOperation = OperationTable.GetPhysicalOperation(text);
		Operation logicalOperation = OperationTable.GetLogicalOperation(text2);
		Operation result = ((logicalOperation != null && logicalOperation.Image != null && logicalOperation.Description != null) ? logicalOperation : physicalOperation);
		node.LogicalOpUnlocName = text2;
		node.PhysicalOpUnlocName = text;
		node["PhysicalOp"] = physicalOperation.DisplayName;
		node["LogicalOp"] = logicalOperation.DisplayName;
		return result;
	}

	protected override double GetNodeSubtreeCost(Node node)
	{
		object obj = node["PDWAccumulativeCost"] ?? node["EstimatedTotalSubtreeCost"];
		if (obj == null)
		{
			return 0.0;
		}
		return Convert.ToDouble(obj, CultureInfo.CurrentCulture);
	}

	public override void ParseProperties(object parsedItem, PropertyDescriptorCollection targetPropertyBag, NodeBuilderContext context)
	{
		base.ParseProperties(parsedItem, targetPropertyBag, context);
		RelOpType relOpType = parsedItem as RelOpType;
		if (relOpType.RunTimeInformation != null && relOpType.RunTimeInformation.Length != 0)
		{
			RunTimeCounters runTimeCounters = new RunTimeCounters();
			RunTimeCounters runTimeCounters2 = new RunTimeCounters();
			RunTimeCounters runTimeCounters3 = new RunTimeCounters();
			RunTimeCounters runTimeCounters4 = new RunTimeCounters();
			RunTimeCounters runTimeCounters5 = new RunTimeCounters();
			RunTimeCounters runTimeCounters6 = new RunTimeCounters();
			RunTimeCounters runTimeCounters7 = new RunTimeCounters();
			RunTimeCounters runTimeCounters8 = new RunTimeCounters
			{
				DisplayTotalCounters = false
			};
			RunTimeCounters runTimeCounters9 = new RunTimeCounters();
			RunTimeCounters runTimeCounters10 = new RunTimeCounters();
			RunTimeCounters runTimeCounters11 = new RunTimeCounters();
			RunTimeCounters runTimeCounters12 = new RunTimeCounters();
			RunTimeCounters runTimeCounters13 = new RunTimeCounters();
			RunTimeCounters runTimeCounters14 = new RunTimeCounters();
			RunTimeCounters runTimeCounters15 = new RunTimeCounters();
			RunTimeCounters runTimeCounters16 = new RunTimeCounters();
			RunTimeCounters runTimeCounters17 = new RunTimeCounters();
			RunTimeCounters runTimeCounters18 = new RunTimeCounters();
			RunTimeCounters runTimeCounters19 = new RunTimeCounters();
			RunTimeCounters runTimeCounters20 = new RunTimeCounters();
			RunTimeCounters runTimeCounters21 = new MemGrantRunTimeCounters();
			RunTimeCounters runTimeCounters22 = new MemGrantRunTimeCounters();
			RunTimeCounters runTimeCounters23 = new RunTimeCounters();
			RunTimeCounters runTimeCounters24 = new RunTimeCounters();
			RunTimeCounters runTimeCounters25 = new RunTimeCounters();
			RunTimeCounters runTimeCounters26 = new RunTimeCounters();
			RunTimeCounters runTimeCounters27 = new RunTimeCounters();
			ExpandableObjectWrapper expandableObjectWrapper = new ExpandableObjectWrapper();
			ExpandableObjectWrapper expandableObjectWrapper2 = new ExpandableObjectWrapper();
			ExpandableObjectWrapper expandableObjectWrapper3 = new ExpandableObjectWrapper();
			string value = string.Empty;
			string value2 = string.Empty;
			bool flag = false;
			RunTimeInformationTypeRunTimeCountersPerThread[] runTimeInformation = relOpType.RunTimeInformation;
			foreach (RunTimeInformationTypeRunTimeCountersPerThread runTimeInformationTypeRunTimeCountersPerThread in runTimeInformation)
			{
				if (runTimeInformationTypeRunTimeCountersPerThread.BrickIdSpecified)
				{
					runTimeCounters.AddCounter(runTimeInformationTypeRunTimeCountersPerThread.Thread, runTimeInformationTypeRunTimeCountersPerThread.BrickId, runTimeInformationTypeRunTimeCountersPerThread.ActualRows);
					runTimeCounters4.AddCounter(runTimeInformationTypeRunTimeCountersPerThread.Thread, runTimeInformationTypeRunTimeCountersPerThread.BrickId, runTimeInformationTypeRunTimeCountersPerThread.ActualRebinds);
					runTimeCounters5.AddCounter(runTimeInformationTypeRunTimeCountersPerThread.Thread, runTimeInformationTypeRunTimeCountersPerThread.BrickId, runTimeInformationTypeRunTimeCountersPerThread.ActualRewinds);
					runTimeCounters6.AddCounter(runTimeInformationTypeRunTimeCountersPerThread.Thread, runTimeInformationTypeRunTimeCountersPerThread.BrickId, runTimeInformationTypeRunTimeCountersPerThread.ActualExecutions);
					runTimeCounters7.AddCounter(runTimeInformationTypeRunTimeCountersPerThread.Thread, runTimeInformationTypeRunTimeCountersPerThread.BrickId, runTimeInformationTypeRunTimeCountersPerThread.ActualLocallyAggregatedRows);
					if (runTimeInformationTypeRunTimeCountersPerThread.ActualElapsedmsSpecified)
					{
						runTimeCounters8.AddCounter(runTimeInformationTypeRunTimeCountersPerThread.Thread, runTimeInformationTypeRunTimeCountersPerThread.BrickId, runTimeInformationTypeRunTimeCountersPerThread.ActualElapsedms);
					}
					if (runTimeInformationTypeRunTimeCountersPerThread.ActualCPUmsSpecified)
					{
						runTimeCounters9.AddCounter(runTimeInformationTypeRunTimeCountersPerThread.Thread, runTimeInformationTypeRunTimeCountersPerThread.BrickId, runTimeInformationTypeRunTimeCountersPerThread.ActualCPUms);
					}
					if (runTimeInformationTypeRunTimeCountersPerThread.ActualScansSpecified)
					{
						runTimeCounters10.AddCounter(runTimeInformationTypeRunTimeCountersPerThread.Thread, runTimeInformationTypeRunTimeCountersPerThread.BrickId, runTimeInformationTypeRunTimeCountersPerThread.ActualScans);
					}
					if (runTimeInformationTypeRunTimeCountersPerThread.ActualLogicalReadsSpecified)
					{
						runTimeCounters11.AddCounter(runTimeInformationTypeRunTimeCountersPerThread.Thread, runTimeInformationTypeRunTimeCountersPerThread.BrickId, runTimeInformationTypeRunTimeCountersPerThread.ActualLogicalReads);
					}
					if (runTimeInformationTypeRunTimeCountersPerThread.ActualPhysicalReadsSpecified)
					{
						runTimeCounters12.AddCounter(runTimeInformationTypeRunTimeCountersPerThread.Thread, runTimeInformationTypeRunTimeCountersPerThread.BrickId, runTimeInformationTypeRunTimeCountersPerThread.ActualPhysicalReads);
					}
					if (runTimeInformationTypeRunTimeCountersPerThread.ActualPageServerReadsSpecified)
					{
						runTimeCounters13.AddCounter(runTimeInformationTypeRunTimeCountersPerThread.Thread, runTimeInformationTypeRunTimeCountersPerThread.BrickId, runTimeInformationTypeRunTimeCountersPerThread.ActualPageServerReads);
					}
					if (runTimeInformationTypeRunTimeCountersPerThread.ActualReadAheadsSpecified)
					{
						runTimeCounters14.AddCounter(runTimeInformationTypeRunTimeCountersPerThread.Thread, runTimeInformationTypeRunTimeCountersPerThread.BrickId, runTimeInformationTypeRunTimeCountersPerThread.ActualReadAheads);
					}
					if (runTimeInformationTypeRunTimeCountersPerThread.ActualPageServerReadAheadsSpecified)
					{
						runTimeCounters15.AddCounter(runTimeInformationTypeRunTimeCountersPerThread.Thread, runTimeInformationTypeRunTimeCountersPerThread.BrickId, runTimeInformationTypeRunTimeCountersPerThread.ActualPageServerReadAheads);
					}
					if (runTimeInformationTypeRunTimeCountersPerThread.ActualLobLogicalReadsSpecified)
					{
						runTimeCounters16.AddCounter(runTimeInformationTypeRunTimeCountersPerThread.Thread, runTimeInformationTypeRunTimeCountersPerThread.BrickId, runTimeInformationTypeRunTimeCountersPerThread.ActualLobLogicalReads);
					}
					if (runTimeInformationTypeRunTimeCountersPerThread.ActualLobPhysicalReadsSpecified)
					{
						runTimeCounters17.AddCounter(runTimeInformationTypeRunTimeCountersPerThread.Thread, runTimeInformationTypeRunTimeCountersPerThread.BrickId, runTimeInformationTypeRunTimeCountersPerThread.ActualLobPhysicalReads);
					}
					if (runTimeInformationTypeRunTimeCountersPerThread.ActualLobPageServerReadsSpecified)
					{
						runTimeCounters18.AddCounter(runTimeInformationTypeRunTimeCountersPerThread.Thread, runTimeInformationTypeRunTimeCountersPerThread.BrickId, runTimeInformationTypeRunTimeCountersPerThread.ActualLobPageServerReads);
					}
					if (runTimeInformationTypeRunTimeCountersPerThread.ActualLobReadAheadsSpecified)
					{
						runTimeCounters19.AddCounter(runTimeInformationTypeRunTimeCountersPerThread.Thread, runTimeInformationTypeRunTimeCountersPerThread.BrickId, runTimeInformationTypeRunTimeCountersPerThread.ActualLobReadAheads);
					}
					if (runTimeInformationTypeRunTimeCountersPerThread.ActualLobPageServerReadAheadsSpecified)
					{
						runTimeCounters20.AddCounter(runTimeInformationTypeRunTimeCountersPerThread.Thread, runTimeInformationTypeRunTimeCountersPerThread.BrickId, runTimeInformationTypeRunTimeCountersPerThread.ActualLobPageServerReadAheads);
					}
					if (runTimeInformationTypeRunTimeCountersPerThread.ActualRowsReadSpecified)
					{
						runTimeCounters2.AddCounter(runTimeInformationTypeRunTimeCountersPerThread.Thread, runTimeInformationTypeRunTimeCountersPerThread.BrickId, runTimeInformationTypeRunTimeCountersPerThread.ActualRowsRead);
					}
					if (runTimeInformationTypeRunTimeCountersPerThread.BatchesSpecified)
					{
						runTimeCounters3.AddCounter(runTimeInformationTypeRunTimeCountersPerThread.Thread, runTimeInformationTypeRunTimeCountersPerThread.BrickId, runTimeInformationTypeRunTimeCountersPerThread.Batches);
					}
					if (runTimeInformationTypeRunTimeCountersPerThread.HpcRowCountSpecified)
					{
						runTimeCounters25.AddCounter(runTimeInformationTypeRunTimeCountersPerThread.Thread, runTimeInformationTypeRunTimeCountersPerThread.BrickId, runTimeInformationTypeRunTimeCountersPerThread.HpcRowCount);
					}
					if (runTimeInformationTypeRunTimeCountersPerThread.HpcKernelElapsedUsSpecified)
					{
						runTimeCounters24.AddCounter(runTimeInformationTypeRunTimeCountersPerThread.Thread, runTimeInformationTypeRunTimeCountersPerThread.BrickId, runTimeInformationTypeRunTimeCountersPerThread.HpcKernelElapsedUs);
					}
					if (runTimeInformationTypeRunTimeCountersPerThread.HpcHostToDeviceBytesSpecified)
					{
						runTimeCounters26.AddCounter(runTimeInformationTypeRunTimeCountersPerThread.Thread, runTimeInformationTypeRunTimeCountersPerThread.BrickId, runTimeInformationTypeRunTimeCountersPerThread.HpcHostToDeviceBytes);
					}
					if (runTimeInformationTypeRunTimeCountersPerThread.HpcDeviceToHostBytesSpecified)
					{
						runTimeCounters27.AddCounter(runTimeInformationTypeRunTimeCountersPerThread.Thread, runTimeInformationTypeRunTimeCountersPerThread.BrickId, runTimeInformationTypeRunTimeCountersPerThread.HpcDeviceToHostBytes);
					}
					if (runTimeInformationTypeRunTimeCountersPerThread.InputMemoryGrantSpecified)
					{
						runTimeCounters21.AddCounter(runTimeInformationTypeRunTimeCountersPerThread.Thread, runTimeInformationTypeRunTimeCountersPerThread.BrickId, runTimeInformationTypeRunTimeCountersPerThread.InputMemoryGrant);
					}
					if (runTimeInformationTypeRunTimeCountersPerThread.OutputMemoryGrantSpecified)
					{
						runTimeCounters22.AddCounter(runTimeInformationTypeRunTimeCountersPerThread.Thread, runTimeInformationTypeRunTimeCountersPerThread.BrickId, runTimeInformationTypeRunTimeCountersPerThread.OutputMemoryGrant);
					}
					if (runTimeInformationTypeRunTimeCountersPerThread.UsedMemoryGrantSpecified)
					{
						runTimeCounters23.AddCounter(runTimeInformationTypeRunTimeCountersPerThread.Thread, runTimeInformationTypeRunTimeCountersPerThread.BrickId, runTimeInformationTypeRunTimeCountersPerThread.UsedMemoryGrant);
					}
				}
				else
				{
					runTimeCounters.AddCounter(runTimeInformationTypeRunTimeCountersPerThread.Thread, runTimeInformationTypeRunTimeCountersPerThread.ActualRows);
					runTimeCounters4.AddCounter(runTimeInformationTypeRunTimeCountersPerThread.Thread, runTimeInformationTypeRunTimeCountersPerThread.ActualRebinds);
					runTimeCounters5.AddCounter(runTimeInformationTypeRunTimeCountersPerThread.Thread, runTimeInformationTypeRunTimeCountersPerThread.ActualRewinds);
					runTimeCounters6.AddCounter(runTimeInformationTypeRunTimeCountersPerThread.Thread, runTimeInformationTypeRunTimeCountersPerThread.ActualExecutions);
					runTimeCounters7.AddCounter(runTimeInformationTypeRunTimeCountersPerThread.Thread, runTimeInformationTypeRunTimeCountersPerThread.ActualLocallyAggregatedRows);
					if (runTimeInformationTypeRunTimeCountersPerThread.ActualElapsedmsSpecified)
					{
						runTimeCounters8.AddCounter(runTimeInformationTypeRunTimeCountersPerThread.Thread, runTimeInformationTypeRunTimeCountersPerThread.ActualElapsedms);
					}
					if (runTimeInformationTypeRunTimeCountersPerThread.ActualCPUmsSpecified)
					{
						runTimeCounters9.AddCounter(runTimeInformationTypeRunTimeCountersPerThread.Thread, runTimeInformationTypeRunTimeCountersPerThread.ActualCPUms);
					}
					if (runTimeInformationTypeRunTimeCountersPerThread.ActualScansSpecified)
					{
						runTimeCounters10.AddCounter(runTimeInformationTypeRunTimeCountersPerThread.Thread, runTimeInformationTypeRunTimeCountersPerThread.ActualScans);
					}
					if (runTimeInformationTypeRunTimeCountersPerThread.ActualLogicalReadsSpecified)
					{
						runTimeCounters11.AddCounter(runTimeInformationTypeRunTimeCountersPerThread.Thread, runTimeInformationTypeRunTimeCountersPerThread.ActualLogicalReads);
					}
					if (runTimeInformationTypeRunTimeCountersPerThread.ActualPhysicalReadsSpecified)
					{
						runTimeCounters12.AddCounter(runTimeInformationTypeRunTimeCountersPerThread.Thread, runTimeInformationTypeRunTimeCountersPerThread.ActualPhysicalReads);
					}
					if (runTimeInformationTypeRunTimeCountersPerThread.ActualPageServerReadsSpecified)
					{
						runTimeCounters13.AddCounter(runTimeInformationTypeRunTimeCountersPerThread.Thread, runTimeInformationTypeRunTimeCountersPerThread.ActualPageServerReads);
					}
					if (runTimeInformationTypeRunTimeCountersPerThread.ActualReadAheadsSpecified)
					{
						runTimeCounters14.AddCounter(runTimeInformationTypeRunTimeCountersPerThread.Thread, runTimeInformationTypeRunTimeCountersPerThread.ActualReadAheads);
					}
					if (runTimeInformationTypeRunTimeCountersPerThread.ActualPageServerReadAheadsSpecified)
					{
						runTimeCounters15.AddCounter(runTimeInformationTypeRunTimeCountersPerThread.Thread, runTimeInformationTypeRunTimeCountersPerThread.ActualPageServerReadAheads);
					}
					if (runTimeInformationTypeRunTimeCountersPerThread.ActualLobLogicalReadsSpecified)
					{
						runTimeCounters16.AddCounter(runTimeInformationTypeRunTimeCountersPerThread.Thread, runTimeInformationTypeRunTimeCountersPerThread.ActualLobLogicalReads);
					}
					if (runTimeInformationTypeRunTimeCountersPerThread.ActualLobPhysicalReadsSpecified)
					{
						runTimeCounters17.AddCounter(runTimeInformationTypeRunTimeCountersPerThread.Thread, runTimeInformationTypeRunTimeCountersPerThread.ActualLobPhysicalReads);
					}
					if (runTimeInformationTypeRunTimeCountersPerThread.ActualLobPageServerReadsSpecified)
					{
						runTimeCounters18.AddCounter(runTimeInformationTypeRunTimeCountersPerThread.Thread, runTimeInformationTypeRunTimeCountersPerThread.ActualLobPageServerReads);
					}
					if (runTimeInformationTypeRunTimeCountersPerThread.ActualLobReadAheadsSpecified)
					{
						runTimeCounters19.AddCounter(runTimeInformationTypeRunTimeCountersPerThread.Thread, runTimeInformationTypeRunTimeCountersPerThread.ActualLobReadAheads);
					}
					if (runTimeInformationTypeRunTimeCountersPerThread.ActualLobPageServerReadAheadsSpecified)
					{
						runTimeCounters20.AddCounter(runTimeInformationTypeRunTimeCountersPerThread.Thread, runTimeInformationTypeRunTimeCountersPerThread.ActualLobPageServerReadAheads);
					}
					if (runTimeInformationTypeRunTimeCountersPerThread.ActualRowsReadSpecified)
					{
						runTimeCounters2.AddCounter(runTimeInformationTypeRunTimeCountersPerThread.Thread, runTimeInformationTypeRunTimeCountersPerThread.ActualRowsRead);
					}
					if (runTimeInformationTypeRunTimeCountersPerThread.BatchesSpecified)
					{
						runTimeCounters3.AddCounter(runTimeInformationTypeRunTimeCountersPerThread.Thread, runTimeInformationTypeRunTimeCountersPerThread.Batches);
					}
					if (runTimeInformationTypeRunTimeCountersPerThread.HpcRowCountSpecified)
					{
						runTimeCounters25.AddCounter(runTimeInformationTypeRunTimeCountersPerThread.Thread, runTimeInformationTypeRunTimeCountersPerThread.HpcRowCount);
					}
					if (runTimeInformationTypeRunTimeCountersPerThread.HpcKernelElapsedUsSpecified)
					{
						runTimeCounters24.AddCounter(runTimeInformationTypeRunTimeCountersPerThread.Thread, runTimeInformationTypeRunTimeCountersPerThread.HpcKernelElapsedUs);
					}
					if (runTimeInformationTypeRunTimeCountersPerThread.HpcHostToDeviceBytesSpecified)
					{
						runTimeCounters26.AddCounter(runTimeInformationTypeRunTimeCountersPerThread.Thread, runTimeInformationTypeRunTimeCountersPerThread.HpcHostToDeviceBytes);
					}
					if (runTimeInformationTypeRunTimeCountersPerThread.HpcDeviceToHostBytesSpecified)
					{
						runTimeCounters27.AddCounter(runTimeInformationTypeRunTimeCountersPerThread.Thread, runTimeInformationTypeRunTimeCountersPerThread.HpcDeviceToHostBytes);
					}
					if (runTimeInformationTypeRunTimeCountersPerThread.InputMemoryGrantSpecified)
					{
						runTimeCounters21.AddCounter(runTimeInformationTypeRunTimeCountersPerThread.Thread, runTimeInformationTypeRunTimeCountersPerThread.InputMemoryGrant);
					}
					if (runTimeInformationTypeRunTimeCountersPerThread.OutputMemoryGrantSpecified)
					{
						runTimeCounters22.AddCounter(runTimeInformationTypeRunTimeCountersPerThread.Thread, runTimeInformationTypeRunTimeCountersPerThread.OutputMemoryGrant);
					}
					if (runTimeInformationTypeRunTimeCountersPerThread.UsedMemoryGrantSpecified)
					{
						runTimeCounters23.AddCounter(runTimeInformationTypeRunTimeCountersPerThread.Thread, runTimeInformationTypeRunTimeCountersPerThread.UsedMemoryGrant);
					}
				}
				if (runTimeInformationTypeRunTimeCountersPerThread.ActualExecutions != 0)
				{
					value = Enum.GetName(typeof(EnExecutionModeType), runTimeInformationTypeRunTimeCountersPerThread.ActualExecutionMode);
				}
				if (runTimeInformationTypeRunTimeCountersPerThread.ActualJoinTypeSpecified)
				{
					value2 = Enum.GetName(typeof(EnPhysicalOpType), runTimeInformationTypeRunTimeCountersPerThread.ActualJoinType);
				}
				if (runTimeInformationTypeRunTimeCountersPerThread.IsInterleavedExecuted)
				{
					flag = true;
				}
			}
			if (flag)
			{
				targetPropertyBag.Add(PropertyFactory.CreateProperty("IsInterleavedExecuted", flag));
			}
			targetPropertyBag.Add(PropertyFactory.CreateProperty("ActualRows", runTimeCounters));
			targetPropertyBag.Add(PropertyFactory.CreateProperty("ActualBatches", runTimeCounters3));
			targetPropertyBag.Add(PropertyFactory.CreateProperty("ActualRebinds", runTimeCounters4));
			targetPropertyBag.Add(PropertyFactory.CreateProperty("ActualRewinds", runTimeCounters5));
			targetPropertyBag.Add(PropertyFactory.CreateProperty("ActualExecutions", runTimeCounters6));
			if (runTimeCounters2.TotalCounters != 0)
			{
				targetPropertyBag.Add(PropertyFactory.CreateProperty("ActualRowsRead", runTimeCounters2));
			}
			if (runTimeCounters7.TotalCounters != 0)
			{
				targetPropertyBag.Add(PropertyFactory.CreateProperty("ActualLocallyAggregatedRows", runTimeCounters7));
			}
			if (runTimeCounters25.TotalCounters != 0)
			{
				targetPropertyBag.Add(PropertyFactory.CreateProperty("HpcRowCount", runTimeCounters25));
			}
			if (runTimeCounters24.TotalCounters != 0)
			{
				targetPropertyBag.Add(PropertyFactory.CreateProperty("HpcKernelElapsedUs", runTimeCounters24));
			}
			if (runTimeCounters26.TotalCounters != 0)
			{
				targetPropertyBag.Add(PropertyFactory.CreateProperty("HpcHostToDeviceBytes", runTimeCounters26));
			}
			if (runTimeCounters27.TotalCounters != 0)
			{
				targetPropertyBag.Add(PropertyFactory.CreateProperty("HpcDeviceToHostBytes", runTimeCounters27));
			}
			if (!string.IsNullOrEmpty(value))
			{
				targetPropertyBag.Add(PropertyFactory.CreateProperty("ActualExecutionMode", value));
			}
			if (!string.IsNullOrEmpty(value2))
			{
				targetPropertyBag.Add(PropertyFactory.CreateProperty("ActualJoinType", value2));
			}
			if (runTimeCounters8.NumOfCounters > 0)
			{
				expandableObjectWrapper.Properties.Add(PropertyFactory.CreateProperty("ActualElapsedms", runTimeCounters8));
			}
			if (runTimeCounters9.NumOfCounters > 0)
			{
				expandableObjectWrapper.Properties.Add(PropertyFactory.CreateProperty("ActualCPUms", runTimeCounters9));
			}
			if (expandableObjectWrapper.Properties.Count > 0)
			{
				targetPropertyBag.Add(PropertyFactory.CreateProperty("ActualTimeStatistics", expandableObjectWrapper));
			}
			if (runTimeCounters10.NumOfCounters > 0)
			{
				expandableObjectWrapper2.Properties.Add(PropertyFactory.CreateProperty("ActualScans", runTimeCounters10));
			}
			if (runTimeCounters11.NumOfCounters > 0)
			{
				expandableObjectWrapper2.Properties.Add(PropertyFactory.CreateProperty("ActualLogicalReads", runTimeCounters11));
			}
			if (runTimeCounters12.NumOfCounters > 0)
			{
				expandableObjectWrapper2.Properties.Add(PropertyFactory.CreateProperty("ActualPhysicalReads", runTimeCounters12));
			}
			if (runTimeCounters13.NumOfCounters > 0)
			{
				expandableObjectWrapper2.Properties.Add(PropertyFactory.CreateProperty("ActualPageServerReads", runTimeCounters13));
			}
			if (runTimeCounters14.NumOfCounters > 0)
			{
				expandableObjectWrapper2.Properties.Add(PropertyFactory.CreateProperty("ActualReadAheads", runTimeCounters14));
			}
			if (runTimeCounters15.NumOfCounters > 0)
			{
				expandableObjectWrapper2.Properties.Add(PropertyFactory.CreateProperty("ActualPageServerReadAheads", runTimeCounters15));
			}
			if (runTimeCounters16.NumOfCounters > 0)
			{
				expandableObjectWrapper2.Properties.Add(PropertyFactory.CreateProperty("ActualLobLogicalReads", runTimeCounters16));
			}
			if (runTimeCounters17.NumOfCounters > 0)
			{
				expandableObjectWrapper2.Properties.Add(PropertyFactory.CreateProperty("ActualLobPhysicalReads", runTimeCounters17));
			}
			if (runTimeCounters18.NumOfCounters > 0)
			{
				expandableObjectWrapper2.Properties.Add(PropertyFactory.CreateProperty("ActualLobPageServerReads", runTimeCounters18));
			}
			if (runTimeCounters19.NumOfCounters > 0)
			{
				expandableObjectWrapper2.Properties.Add(PropertyFactory.CreateProperty("ActualLobReadAheads", runTimeCounters19));
			}
			if (runTimeCounters20.NumOfCounters > 0)
			{
				expandableObjectWrapper2.Properties.Add(PropertyFactory.CreateProperty("ActualLobPageServerReadAheads", runTimeCounters20));
			}
			if (expandableObjectWrapper2.Properties.Count > 0)
			{
				targetPropertyBag.Add(PropertyFactory.CreateProperty("ActualIOStatistics", expandableObjectWrapper2));
			}
			if (runTimeCounters21.NumOfCounters > 0)
			{
				expandableObjectWrapper3.Properties.Add(PropertyFactory.CreateProperty("InputMemoryGrant", runTimeCounters21));
			}
			if (runTimeCounters22.NumOfCounters > 0)
			{
				expandableObjectWrapper3.Properties.Add(PropertyFactory.CreateProperty("OutputMemoryGrant", runTimeCounters22));
			}
			if (runTimeCounters23.NumOfCounters > 0)
			{
				expandableObjectWrapper3.Properties.Add(PropertyFactory.CreateProperty("UsedMemoryGrant", runTimeCounters23));
			}
			if (expandableObjectWrapper3.Properties.Count > 0)
			{
				targetPropertyBag.Add(PropertyFactory.CreateProperty("ActualMemoryGrantStats", expandableObjectWrapper3));
			}
		}
		if (relOpType.RunTimePartitionSummary != null && relOpType.RunTimePartitionSummary.PartitionsAccessed != null)
		{
			RunTimePartitionSummaryTypePartitionsAccessed partitionsAccessed = relOpType.RunTimePartitionSummary.PartitionsAccessed;
			targetPropertyBag.Add(PropertyFactory.CreateProperty("PartitionCount", partitionsAccessed.PartitionCount));
			if (partitionsAccessed.PartitionRange != null && partitionsAccessed.PartitionRange.Length != 0)
			{
				targetPropertyBag.Add(PropertyFactory.CreateProperty("PartitionsAccessed", GetPartitionRangeString(partitionsAccessed.PartitionRange)));
			}
		}
	}

	private static string GetPartitionRangeString(RunTimePartitionSummaryTypePartitionsAccessedPartitionRange[] ranges)
	{
		StringBuilder stringBuilder = new StringBuilder();
		string listSeparator = CultureInfo.CurrentCulture.TextInfo.ListSeparator;
		for (int i = 0; i < ranges.Length; i++)
		{
			if (i != 0)
			{
				stringBuilder.Append(listSeparator);
			}
			RunTimePartitionSummaryTypePartitionsAccessedPartitionRange runTimePartitionSummaryTypePartitionsAccessedPartitionRange = ranges[i];
			if (runTimePartitionSummaryTypePartitionsAccessedPartitionRange.Start == runTimePartitionSummaryTypePartitionsAccessedPartitionRange.End)
			{
				stringBuilder.Append(runTimePartitionSummaryTypePartitionsAccessedPartitionRange.Start);
			}
			else
			{
				stringBuilder.AppendFormat(CultureInfo.CurrentCulture, "{0}..{1}", runTimePartitionSummaryTypePartitionsAccessedPartitionRange.Start, runTimePartitionSummaryTypePartitionsAccessedPartitionRange.End);
			}
		}
		return stringBuilder.ToString();
	}

	private RelOpTypeParser()
	{
	}
}
