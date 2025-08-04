// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.MemoryGrantType
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
internal class MemoryGrantType
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
	internal ulong SerialRequiredMemory
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
	internal ulong SerialDesiredMemory
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
	internal ulong RequiredMemory
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
	internal bool RequiredMemorySpecified
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
	internal ulong DesiredMemory
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
	internal bool DesiredMemorySpecified
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
	internal ulong RequestedMemory
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
	internal bool RequestedMemorySpecified
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
	internal ulong GrantWaitTime
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
	internal bool GrantWaitTimeSpecified
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
	internal ulong GrantedMemory
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
	internal bool GrantedMemorySpecified
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
	internal ulong MaxUsedMemory
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
	internal bool MaxUsedMemorySpecified
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
	internal ulong MaxQueryMemory
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
	internal bool MaxQueryMemorySpecified
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
	internal ulong LastRequestedMemory
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
	internal bool LastRequestedMemorySpecified
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
	internal EnMemoryGrantFeedbackInfoType IsMemoryGrantFeedbackAdjusted
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
	internal bool IsMemoryGrantFeedbackAdjustedSpecified
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
