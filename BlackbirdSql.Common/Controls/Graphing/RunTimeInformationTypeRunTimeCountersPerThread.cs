// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.RunTimeInformationTypeRunTimeCountersPerThread
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;
using BlackbirdSql.Common.Controls.Graphing.Enums;

namespace BlackbirdSql.Common.Controls.Graphing;

[Serializable]
[GeneratedCode("xsd", "4.8.3928.0")]
[DebuggerStepThrough]
[DesignerCategory("code")]
[XmlType(AnonymousType = true, Namespace = LibraryData.C_ShowPlanNamespace)]
public class RunTimeInformationTypeRunTimeCountersPerThread
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
	public int Thread
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
	public int BrickId
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
	public bool BrickIdSpecified
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
	public ulong ActualRebinds
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
	public bool ActualRebindsSpecified
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
	public ulong ActualRewinds
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
	public bool ActualRewindsSpecified
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
	public ulong ActualRows
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
	public ulong ActualRowsRead
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
	public bool ActualRowsReadSpecified
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
	public ulong Batches
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
	public bool BatchesSpecified
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
	public ulong ActualEndOfScans
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
	public ulong ActualExecutions
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
	public EnExecutionModeType ActualExecutionMode
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
	public bool ActualExecutionModeSpecified
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
	public ulong TaskAddr
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
	public bool TaskAddrSpecified
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
	public ulong SchedulerId
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
	public bool SchedulerIdSpecified
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
	public ulong FirstActiveTime
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
	public bool FirstActiveTimeSpecified
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
	public ulong LastActiveTime
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
	public bool LastActiveTimeSpecified
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
	public ulong OpenTime
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
	public bool OpenTimeSpecified
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
	public ulong FirstRowTime
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
	public bool FirstRowTimeSpecified
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
	public ulong LastRowTime
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
	public bool LastRowTimeSpecified
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
	public ulong CloseTime
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
	public bool CloseTimeSpecified
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
	public ulong ActualElapsedms
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
	public bool ActualElapsedmsSpecified
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
	public ulong ActualCPUms
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
	public bool ActualCPUmsSpecified
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
	public ulong ActualScans
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
	public bool ActualScansSpecified
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
	public ulong ActualLogicalReads
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
	public bool ActualLogicalReadsSpecified
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
	public ulong ActualPhysicalReads
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
	public bool ActualPhysicalReadsSpecified
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
	public ulong ActualPageServerReads
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
	public bool ActualPageServerReadsSpecified
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
	public ulong ActualReadAheads
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
	public bool ActualReadAheadsSpecified
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
	public ulong ActualPageServerReadAheads
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
	public bool ActualPageServerReadAheadsSpecified
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
	public ulong ActualLobLogicalReads
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
	public bool ActualLobLogicalReadsSpecified
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
	public ulong ActualLobPhysicalReads
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
	public bool ActualLobPhysicalReadsSpecified
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
	public ulong ActualLobPageServerReads
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
	public bool ActualLobPageServerReadsSpecified
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
	public ulong ActualLobReadAheads
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
	public bool ActualLobReadAheadsSpecified
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
	public ulong ActualLobPageServerReadAheads
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
	public bool ActualLobPageServerReadAheadsSpecified
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
	public int SegmentReads
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
	public bool SegmentReadsSpecified
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
	public int SegmentSkips
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
	public bool SegmentSkipsSpecified
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
	public ulong ActualLocallyAggregatedRows
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
	public bool ActualLocallyAggregatedRowsSpecified
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
	public ulong InputMemoryGrant
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
	public bool InputMemoryGrantSpecified
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
	public ulong OutputMemoryGrant
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
	public bool OutputMemoryGrantSpecified
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
	public ulong UsedMemoryGrant
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
	public bool UsedMemoryGrantSpecified
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
	public bool IsInterleavedExecuted
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
	public bool IsInterleavedExecutedSpecified
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
	public EnPhysicalOpType ActualJoinType
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
	public bool ActualJoinTypeSpecified
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
	public ulong HpcRowCount
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
	public bool HpcRowCountSpecified
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
	public ulong HpcKernelElapsedUs
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
	public bool HpcKernelElapsedUsSpecified
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
	public ulong HpcHostToDeviceBytes
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
	public bool HpcHostToDeviceBytesSpecified
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
	public ulong HpcDeviceToHostBytes
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
	public bool HpcDeviceToHostBytesSpecified
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
	public ulong ActualPageServerPushedPageIDs
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
	public bool ActualPageServerPushedPageIDsSpecified
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
	public ulong ActualPageServerRowsReturned
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
	public bool ActualPageServerRowsReturnedSpecified
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
	public ulong ActualPageServerRowsRead
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
	public bool ActualPageServerRowsReadSpecified
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
	public ulong ActualPageServerPushedReads
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
	public bool ActualPageServerPushedReadsSpecified
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
