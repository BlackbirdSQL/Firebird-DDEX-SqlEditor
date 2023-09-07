// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.MemoryGrantType
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
public class MemoryGrantType
{
	private ulong serialRequiredMemoryField;

	private ulong serialDesiredMemoryField;

	private ulong requiredMemoryField;

	private bool requiredMemoryFieldSpecified;

	private ulong desiredMemoryField;

	private bool desiredMemoryFieldSpecified;

	private ulong requestedMemoryField;

	private bool requestedMemoryFieldSpecified;

	private ulong grantWaitTimeField;

	private bool grantWaitTimeFieldSpecified;

	private ulong grantedMemoryField;

	private bool grantedMemoryFieldSpecified;

	private ulong maxUsedMemoryField;

	private bool maxUsedMemoryFieldSpecified;

	private ulong maxQueryMemoryField;

	private bool maxQueryMemoryFieldSpecified;

	private ulong lastRequestedMemoryField;

	private bool lastRequestedMemoryFieldSpecified;

	private EnMemoryGrantFeedbackInfoType isMemoryGrantFeedbackAdjustedField;

	private bool isMemoryGrantFeedbackAdjustedFieldSpecified;

	[XmlAttribute]
	public ulong SerialRequiredMemory
	{
		get
		{
			return serialRequiredMemoryField;
		}
		set
		{
			serialRequiredMemoryField = value;
		}
	}

	[XmlAttribute]
	public ulong SerialDesiredMemory
	{
		get
		{
			return serialDesiredMemoryField;
		}
		set
		{
			serialDesiredMemoryField = value;
		}
	}

	[XmlAttribute]
	public ulong RequiredMemory
	{
		get
		{
			return requiredMemoryField;
		}
		set
		{
			requiredMemoryField = value;
		}
	}

	[XmlIgnore]
	public bool RequiredMemorySpecified
	{
		get
		{
			return requiredMemoryFieldSpecified;
		}
		set
		{
			requiredMemoryFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public ulong DesiredMemory
	{
		get
		{
			return desiredMemoryField;
		}
		set
		{
			desiredMemoryField = value;
		}
	}

	[XmlIgnore]
	public bool DesiredMemorySpecified
	{
		get
		{
			return desiredMemoryFieldSpecified;
		}
		set
		{
			desiredMemoryFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public ulong RequestedMemory
	{
		get
		{
			return requestedMemoryField;
		}
		set
		{
			requestedMemoryField = value;
		}
	}

	[XmlIgnore]
	public bool RequestedMemorySpecified
	{
		get
		{
			return requestedMemoryFieldSpecified;
		}
		set
		{
			requestedMemoryFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public ulong GrantWaitTime
	{
		get
		{
			return grantWaitTimeField;
		}
		set
		{
			grantWaitTimeField = value;
		}
	}

	[XmlIgnore]
	public bool GrantWaitTimeSpecified
	{
		get
		{
			return grantWaitTimeFieldSpecified;
		}
		set
		{
			grantWaitTimeFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public ulong GrantedMemory
	{
		get
		{
			return grantedMemoryField;
		}
		set
		{
			grantedMemoryField = value;
		}
	}

	[XmlIgnore]
	public bool GrantedMemorySpecified
	{
		get
		{
			return grantedMemoryFieldSpecified;
		}
		set
		{
			grantedMemoryFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public ulong MaxUsedMemory
	{
		get
		{
			return maxUsedMemoryField;
		}
		set
		{
			maxUsedMemoryField = value;
		}
	}

	[XmlIgnore]
	public bool MaxUsedMemorySpecified
	{
		get
		{
			return maxUsedMemoryFieldSpecified;
		}
		set
		{
			maxUsedMemoryFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public ulong MaxQueryMemory
	{
		get
		{
			return maxQueryMemoryField;
		}
		set
		{
			maxQueryMemoryField = value;
		}
	}

	[XmlIgnore]
	public bool MaxQueryMemorySpecified
	{
		get
		{
			return maxQueryMemoryFieldSpecified;
		}
		set
		{
			maxQueryMemoryFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public ulong LastRequestedMemory
	{
		get
		{
			return lastRequestedMemoryField;
		}
		set
		{
			lastRequestedMemoryField = value;
		}
	}

	[XmlIgnore]
	public bool LastRequestedMemorySpecified
	{
		get
		{
			return lastRequestedMemoryFieldSpecified;
		}
		set
		{
			lastRequestedMemoryFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public EnMemoryGrantFeedbackInfoType IsMemoryGrantFeedbackAdjusted
	{
		get
		{
			return isMemoryGrantFeedbackAdjustedField;
		}
		set
		{
			isMemoryGrantFeedbackAdjustedField = value;
		}
	}

	[XmlIgnore]
	public bool IsMemoryGrantFeedbackAdjustedSpecified
	{
		get
		{
			return isMemoryGrantFeedbackAdjustedFieldSpecified;
		}
		set
		{
			isMemoryGrantFeedbackAdjustedFieldSpecified = value;
		}
	}
}
