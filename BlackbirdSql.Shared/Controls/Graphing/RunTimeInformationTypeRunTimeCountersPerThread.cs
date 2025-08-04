// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.RunTimeInformationTypeRunTimeCountersPerThread
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;
using BlackbirdSql.Shared.Controls.Graphing.Enums;

namespace BlackbirdSql.Shared.Controls.Graphing;

[Serializable]
[GeneratedCode("xsd", "4.8.3928.0")]
[DebuggerStepThrough]
[DesignerCategory("code")]
[XmlType(AnonymousType = true, Namespace = LibraryData.C_ShowPlanNamespace)]
internal class RunTimeInformationTypeRunTimeCountersPerThread
{
	private int threadField;

	private int brickIdField;

	private bool brickIdFieldSpecified;

	private ulong actualRebindsField;

	private bool actualRebindsFieldSpecified;

	private ulong actualRewindsField;

	private bool actualRewindsFieldSpecified;

	private ulong actualRowsField;

	private ulong actualRowsReadField;

	private bool actualRowsReadFieldSpecified;

	private ulong batchesField;

	private bool batchesFieldSpecified;

	private ulong actualEndOfScansField;

	private ulong actualExecutionsField;

	private EnExecutionModeType actualExecutionModeField;

	private bool actualExecutionModeFieldSpecified;

	private ulong taskAddrField;

	private bool taskAddrFieldSpecified;

	private ulong schedulerIdField;

	private bool schedulerIdFieldSpecified;

	private ulong firstActiveTimeField;

	private bool firstActiveTimeFieldSpecified;

	private ulong lastActiveTimeField;

	private bool lastActiveTimeFieldSpecified;

	private ulong openTimeField;

	private bool openTimeFieldSpecified;

	private ulong firstRowTimeField;

	private bool firstRowTimeFieldSpecified;

	private ulong lastRowTimeField;

	private bool lastRowTimeFieldSpecified;

	private ulong closeTimeField;

	private bool closeTimeFieldSpecified;

	private ulong actualElapsedmsField;

	private bool actualElapsedmsFieldSpecified;

	private ulong actualCPUmsField;

	private bool actualCPUmsFieldSpecified;

	private ulong actualScansField;

	private bool actualScansFieldSpecified;

	private ulong actualLogicalReadsField;

	private bool actualLogicalReadsFieldSpecified;

	private ulong actualPhysicalReadsField;

	private bool actualPhysicalReadsFieldSpecified;

	private ulong actualPageServerReadsField;

	private bool actualPageServerReadsFieldSpecified;

	private ulong actualReadAheadsField;

	private bool actualReadAheadsFieldSpecified;

	private ulong actualPageServerReadAheadsField;

	private bool actualPageServerReadAheadsFieldSpecified;

	private ulong actualLobLogicalReadsField;

	private bool actualLobLogicalReadsFieldSpecified;

	private ulong actualLobPhysicalReadsField;

	private bool actualLobPhysicalReadsFieldSpecified;

	private ulong actualLobPageServerReadsField;

	private bool actualLobPageServerReadsFieldSpecified;

	private ulong actualLobReadAheadsField;

	private bool actualLobReadAheadsFieldSpecified;

	private ulong actualLobPageServerReadAheadsField;

	private bool actualLobPageServerReadAheadsFieldSpecified;

	private int segmentReadsField;

	private bool segmentReadsFieldSpecified;

	private int segmentSkipsField;

	private bool segmentSkipsFieldSpecified;

	private ulong actualLocallyAggregatedRowsField;

	private bool actualLocallyAggregatedRowsFieldSpecified;

	private ulong inputMemoryGrantField;

	private bool inputMemoryGrantFieldSpecified;

	private ulong outputMemoryGrantField;

	private bool outputMemoryGrantFieldSpecified;

	private ulong usedMemoryGrantField;

	private bool usedMemoryGrantFieldSpecified;

	private bool isInterleavedExecutedField;

	private bool isInterleavedExecutedFieldSpecified;

	private EnPhysicalOpType actualJoinTypeField;

	private bool actualJoinTypeFieldSpecified;

	private ulong hpcRowCountField;

	private bool hpcRowCountFieldSpecified;

	private ulong hpcKernelElapsedUsField;

	private bool hpcKernelElapsedUsFieldSpecified;

	private ulong hpcHostToDeviceBytesField;

	private bool hpcHostToDeviceBytesFieldSpecified;

	private ulong hpcDeviceToHostBytesField;

	private bool hpcDeviceToHostBytesFieldSpecified;

	private ulong actualPageServerPushedPageIDsField;

	private bool actualPageServerPushedPageIDsFieldSpecified;

	private ulong actualPageServerRowsReturnedField;

	private bool actualPageServerRowsReturnedFieldSpecified;

	private ulong actualPageServerRowsReadField;

	private bool actualPageServerRowsReadFieldSpecified;

	private ulong actualPageServerPushedReadsField;

	private bool actualPageServerPushedReadsFieldSpecified;

	[XmlAttribute]
	internal int Thread
	{
		get
		{
			return threadField;
		}
		set
		{
			threadField = value;
		}
	}

	[XmlAttribute]
	internal int BrickId
	{
		get
		{
			return brickIdField;
		}
		set
		{
			brickIdField = value;
		}
	}

	[XmlIgnore]
	internal bool BrickIdSpecified
	{
		get
		{
			return brickIdFieldSpecified;
		}
		set
		{
			brickIdFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal ulong ActualRebinds
	{
		get
		{
			return actualRebindsField;
		}
		set
		{
			actualRebindsField = value;
		}
	}

	[XmlIgnore]
	internal bool ActualRebindsSpecified
	{
		get
		{
			return actualRebindsFieldSpecified;
		}
		set
		{
			actualRebindsFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal ulong ActualRewinds
	{
		get
		{
			return actualRewindsField;
		}
		set
		{
			actualRewindsField = value;
		}
	}

	[XmlIgnore]
	internal bool ActualRewindsSpecified
	{
		get
		{
			return actualRewindsFieldSpecified;
		}
		set
		{
			actualRewindsFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal ulong ActualRows
	{
		get
		{
			return actualRowsField;
		}
		set
		{
			actualRowsField = value;
		}
	}

	[XmlAttribute]
	internal ulong ActualRowsRead
	{
		get
		{
			return actualRowsReadField;
		}
		set
		{
			actualRowsReadField = value;
		}
	}

	[XmlIgnore]
	internal bool ActualRowsReadSpecified
	{
		get
		{
			return actualRowsReadFieldSpecified;
		}
		set
		{
			actualRowsReadFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal ulong Batches
	{
		get
		{
			return batchesField;
		}
		set
		{
			batchesField = value;
		}
	}

	[XmlIgnore]
	internal bool BatchesSpecified
	{
		get
		{
			return batchesFieldSpecified;
		}
		set
		{
			batchesFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal ulong ActualEndOfScans
	{
		get
		{
			return actualEndOfScansField;
		}
		set
		{
			actualEndOfScansField = value;
		}
	}

	[XmlAttribute]
	internal ulong ActualExecutions
	{
		get
		{
			return actualExecutionsField;
		}
		set
		{
			actualExecutionsField = value;
		}
	}

	[XmlAttribute]
	internal EnExecutionModeType ActualExecutionMode
	{
		get
		{
			return actualExecutionModeField;
		}
		set
		{
			actualExecutionModeField = value;
		}
	}

	[XmlIgnore]
	internal bool ActualExecutionModeSpecified
	{
		get
		{
			return actualExecutionModeFieldSpecified;
		}
		set
		{
			actualExecutionModeFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal ulong TaskAddr
	{
		get
		{
			return taskAddrField;
		}
		set
		{
			taskAddrField = value;
		}
	}

	[XmlIgnore]
	internal bool TaskAddrSpecified
	{
		get
		{
			return taskAddrFieldSpecified;
		}
		set
		{
			taskAddrFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal ulong SchedulerId
	{
		get
		{
			return schedulerIdField;
		}
		set
		{
			schedulerIdField = value;
		}
	}

	[XmlIgnore]
	internal bool SchedulerIdSpecified
	{
		get
		{
			return schedulerIdFieldSpecified;
		}
		set
		{
			schedulerIdFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal ulong FirstActiveTime
	{
		get
		{
			return firstActiveTimeField;
		}
		set
		{
			firstActiveTimeField = value;
		}
	}

	[XmlIgnore]
	internal bool FirstActiveTimeSpecified
	{
		get
		{
			return firstActiveTimeFieldSpecified;
		}
		set
		{
			firstActiveTimeFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal ulong LastActiveTime
	{
		get
		{
			return lastActiveTimeField;
		}
		set
		{
			lastActiveTimeField = value;
		}
	}

	[XmlIgnore]
	internal bool LastActiveTimeSpecified
	{
		get
		{
			return lastActiveTimeFieldSpecified;
		}
		set
		{
			lastActiveTimeFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal ulong OpenTime
	{
		get
		{
			return openTimeField;
		}
		set
		{
			openTimeField = value;
		}
	}

	[XmlIgnore]
	internal bool OpenTimeSpecified
	{
		get
		{
			return openTimeFieldSpecified;
		}
		set
		{
			openTimeFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal ulong FirstRowTime
	{
		get
		{
			return firstRowTimeField;
		}
		set
		{
			firstRowTimeField = value;
		}
	}

	[XmlIgnore]
	internal bool FirstRowTimeSpecified
	{
		get
		{
			return firstRowTimeFieldSpecified;
		}
		set
		{
			firstRowTimeFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal ulong LastRowTime
	{
		get
		{
			return lastRowTimeField;
		}
		set
		{
			lastRowTimeField = value;
		}
	}

	[XmlIgnore]
	internal bool LastRowTimeSpecified
	{
		get
		{
			return lastRowTimeFieldSpecified;
		}
		set
		{
			lastRowTimeFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal ulong CloseTime
	{
		get
		{
			return closeTimeField;
		}
		set
		{
			closeTimeField = value;
		}
	}

	[XmlIgnore]
	internal bool CloseTimeSpecified
	{
		get
		{
			return closeTimeFieldSpecified;
		}
		set
		{
			closeTimeFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal ulong ActualElapsedms
	{
		get
		{
			return actualElapsedmsField;
		}
		set
		{
			actualElapsedmsField = value;
		}
	}

	[XmlIgnore]
	internal bool ActualElapsedmsSpecified
	{
		get
		{
			return actualElapsedmsFieldSpecified;
		}
		set
		{
			actualElapsedmsFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal ulong ActualCPUms
	{
		get
		{
			return actualCPUmsField;
		}
		set
		{
			actualCPUmsField = value;
		}
	}

	[XmlIgnore]
	internal bool ActualCPUmsSpecified
	{
		get
		{
			return actualCPUmsFieldSpecified;
		}
		set
		{
			actualCPUmsFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal ulong ActualScans
	{
		get
		{
			return actualScansField;
		}
		set
		{
			actualScansField = value;
		}
	}

	[XmlIgnore]
	internal bool ActualScansSpecified
	{
		get
		{
			return actualScansFieldSpecified;
		}
		set
		{
			actualScansFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal ulong ActualLogicalReads
	{
		get
		{
			return actualLogicalReadsField;
		}
		set
		{
			actualLogicalReadsField = value;
		}
	}

	[XmlIgnore]
	internal bool ActualLogicalReadsSpecified
	{
		get
		{
			return actualLogicalReadsFieldSpecified;
		}
		set
		{
			actualLogicalReadsFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal ulong ActualPhysicalReads
	{
		get
		{
			return actualPhysicalReadsField;
		}
		set
		{
			actualPhysicalReadsField = value;
		}
	}

	[XmlIgnore]
	internal bool ActualPhysicalReadsSpecified
	{
		get
		{
			return actualPhysicalReadsFieldSpecified;
		}
		set
		{
			actualPhysicalReadsFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal ulong ActualPageServerReads
	{
		get
		{
			return actualPageServerReadsField;
		}
		set
		{
			actualPageServerReadsField = value;
		}
	}

	[XmlIgnore]
	internal bool ActualPageServerReadsSpecified
	{
		get
		{
			return actualPageServerReadsFieldSpecified;
		}
		set
		{
			actualPageServerReadsFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal ulong ActualReadAheads
	{
		get
		{
			return actualReadAheadsField;
		}
		set
		{
			actualReadAheadsField = value;
		}
	}

	[XmlIgnore]
	internal bool ActualReadAheadsSpecified
	{
		get
		{
			return actualReadAheadsFieldSpecified;
		}
		set
		{
			actualReadAheadsFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal ulong ActualPageServerReadAheads
	{
		get
		{
			return actualPageServerReadAheadsField;
		}
		set
		{
			actualPageServerReadAheadsField = value;
		}
	}

	[XmlIgnore]
	internal bool ActualPageServerReadAheadsSpecified
	{
		get
		{
			return actualPageServerReadAheadsFieldSpecified;
		}
		set
		{
			actualPageServerReadAheadsFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal ulong ActualLobLogicalReads
	{
		get
		{
			return actualLobLogicalReadsField;
		}
		set
		{
			actualLobLogicalReadsField = value;
		}
	}

	[XmlIgnore]
	internal bool ActualLobLogicalReadsSpecified
	{
		get
		{
			return actualLobLogicalReadsFieldSpecified;
		}
		set
		{
			actualLobLogicalReadsFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal ulong ActualLobPhysicalReads
	{
		get
		{
			return actualLobPhysicalReadsField;
		}
		set
		{
			actualLobPhysicalReadsField = value;
		}
	}

	[XmlIgnore]
	internal bool ActualLobPhysicalReadsSpecified
	{
		get
		{
			return actualLobPhysicalReadsFieldSpecified;
		}
		set
		{
			actualLobPhysicalReadsFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal ulong ActualLobPageServerReads
	{
		get
		{
			return actualLobPageServerReadsField;
		}
		set
		{
			actualLobPageServerReadsField = value;
		}
	}

	[XmlIgnore]
	internal bool ActualLobPageServerReadsSpecified
	{
		get
		{
			return actualLobPageServerReadsFieldSpecified;
		}
		set
		{
			actualLobPageServerReadsFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal ulong ActualLobReadAheads
	{
		get
		{
			return actualLobReadAheadsField;
		}
		set
		{
			actualLobReadAheadsField = value;
		}
	}

	[XmlIgnore]
	internal bool ActualLobReadAheadsSpecified
	{
		get
		{
			return actualLobReadAheadsFieldSpecified;
		}
		set
		{
			actualLobReadAheadsFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal ulong ActualLobPageServerReadAheads
	{
		get
		{
			return actualLobPageServerReadAheadsField;
		}
		set
		{
			actualLobPageServerReadAheadsField = value;
		}
	}

	[XmlIgnore]
	internal bool ActualLobPageServerReadAheadsSpecified
	{
		get
		{
			return actualLobPageServerReadAheadsFieldSpecified;
		}
		set
		{
			actualLobPageServerReadAheadsFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal int SegmentReads
	{
		get
		{
			return segmentReadsField;
		}
		set
		{
			segmentReadsField = value;
		}
	}

	[XmlIgnore]
	internal bool SegmentReadsSpecified
	{
		get
		{
			return segmentReadsFieldSpecified;
		}
		set
		{
			segmentReadsFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal int SegmentSkips
	{
		get
		{
			return segmentSkipsField;
		}
		set
		{
			segmentSkipsField = value;
		}
	}

	[XmlIgnore]
	internal bool SegmentSkipsSpecified
	{
		get
		{
			return segmentSkipsFieldSpecified;
		}
		set
		{
			segmentSkipsFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal ulong ActualLocallyAggregatedRows
	{
		get
		{
			return actualLocallyAggregatedRowsField;
		}
		set
		{
			actualLocallyAggregatedRowsField = value;
		}
	}

	[XmlIgnore]
	internal bool ActualLocallyAggregatedRowsSpecified
	{
		get
		{
			return actualLocallyAggregatedRowsFieldSpecified;
		}
		set
		{
			actualLocallyAggregatedRowsFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal ulong InputMemoryGrant
	{
		get
		{
			return inputMemoryGrantField;
		}
		set
		{
			inputMemoryGrantField = value;
		}
	}

	[XmlIgnore]
	internal bool InputMemoryGrantSpecified
	{
		get
		{
			return inputMemoryGrantFieldSpecified;
		}
		set
		{
			inputMemoryGrantFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal ulong OutputMemoryGrant
	{
		get
		{
			return outputMemoryGrantField;
		}
		set
		{
			outputMemoryGrantField = value;
		}
	}

	[XmlIgnore]
	internal bool OutputMemoryGrantSpecified
	{
		get
		{
			return outputMemoryGrantFieldSpecified;
		}
		set
		{
			outputMemoryGrantFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal ulong UsedMemoryGrant
	{
		get
		{
			return usedMemoryGrantField;
		}
		set
		{
			usedMemoryGrantField = value;
		}
	}

	[XmlIgnore]
	internal bool UsedMemoryGrantSpecified
	{
		get
		{
			return usedMemoryGrantFieldSpecified;
		}
		set
		{
			usedMemoryGrantFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal bool IsInterleavedExecuted
	{
		get
		{
			return isInterleavedExecutedField;
		}
		set
		{
			isInterleavedExecutedField = value;
		}
	}

	[XmlIgnore]
	internal bool IsInterleavedExecutedSpecified
	{
		get
		{
			return isInterleavedExecutedFieldSpecified;
		}
		set
		{
			isInterleavedExecutedFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal EnPhysicalOpType ActualJoinType
	{
		get
		{
			return actualJoinTypeField;
		}
		set
		{
			actualJoinTypeField = value;
		}
	}

	[XmlIgnore]
	internal bool ActualJoinTypeSpecified
	{
		get
		{
			return actualJoinTypeFieldSpecified;
		}
		set
		{
			actualJoinTypeFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal ulong HpcRowCount
	{
		get
		{
			return hpcRowCountField;
		}
		set
		{
			hpcRowCountField = value;
		}
	}

	[XmlIgnore]
	internal bool HpcRowCountSpecified
	{
		get
		{
			return hpcRowCountFieldSpecified;
		}
		set
		{
			hpcRowCountFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal ulong HpcKernelElapsedUs
	{
		get
		{
			return hpcKernelElapsedUsField;
		}
		set
		{
			hpcKernelElapsedUsField = value;
		}
	}

	[XmlIgnore]
	internal bool HpcKernelElapsedUsSpecified
	{
		get
		{
			return hpcKernelElapsedUsFieldSpecified;
		}
		set
		{
			hpcKernelElapsedUsFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal ulong HpcHostToDeviceBytes
	{
		get
		{
			return hpcHostToDeviceBytesField;
		}
		set
		{
			hpcHostToDeviceBytesField = value;
		}
	}

	[XmlIgnore]
	internal bool HpcHostToDeviceBytesSpecified
	{
		get
		{
			return hpcHostToDeviceBytesFieldSpecified;
		}
		set
		{
			hpcHostToDeviceBytesFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal ulong HpcDeviceToHostBytes
	{
		get
		{
			return hpcDeviceToHostBytesField;
		}
		set
		{
			hpcDeviceToHostBytesField = value;
		}
	}

	[XmlIgnore]
	internal bool HpcDeviceToHostBytesSpecified
	{
		get
		{
			return hpcDeviceToHostBytesFieldSpecified;
		}
		set
		{
			hpcDeviceToHostBytesFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal ulong ActualPageServerPushedPageIDs
	{
		get
		{
			return actualPageServerPushedPageIDsField;
		}
		set
		{
			actualPageServerPushedPageIDsField = value;
		}
	}

	[XmlIgnore]
	internal bool ActualPageServerPushedPageIDsSpecified
	{
		get
		{
			return actualPageServerPushedPageIDsFieldSpecified;
		}
		set
		{
			actualPageServerPushedPageIDsFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal ulong ActualPageServerRowsReturned
	{
		get
		{
			return actualPageServerRowsReturnedField;
		}
		set
		{
			actualPageServerRowsReturnedField = value;
		}
	}

	[XmlIgnore]
	internal bool ActualPageServerRowsReturnedSpecified
	{
		get
		{
			return actualPageServerRowsReturnedFieldSpecified;
		}
		set
		{
			actualPageServerRowsReturnedFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal ulong ActualPageServerRowsRead
	{
		get
		{
			return actualPageServerRowsReadField;
		}
		set
		{
			actualPageServerRowsReadField = value;
		}
	}

	[XmlIgnore]
	internal bool ActualPageServerRowsReadSpecified
	{
		get
		{
			return actualPageServerRowsReadFieldSpecified;
		}
		set
		{
			actualPageServerRowsReadFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal ulong ActualPageServerPushedReads
	{
		get
		{
			return actualPageServerPushedReadsField;
		}
		set
		{
			actualPageServerPushedReadsField = value;
		}
	}

	[XmlIgnore]
	internal bool ActualPageServerPushedReadsSpecified
	{
		get
		{
			return actualPageServerPushedReadsFieldSpecified;
		}
		set
		{
			actualPageServerPushedReadsFieldSpecified = value;
		}
	}
}
