// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.HashSpillDetailsType
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
internal class HashSpillDetailsType
{
	private ulong grantedMemoryKbField;

	private bool grantedMemoryKbFieldSpecified;

	private ulong usedMemoryKbField;

	private bool usedMemoryKbFieldSpecified;

	private ulong writesToTempDbField;

	private bool writesToTempDbFieldSpecified;

	private ulong readsFromTempDbField;

	private bool readsFromTempDbFieldSpecified;

	[XmlAttribute]
	internal ulong GrantedMemoryKb
	{
		get
		{
			return grantedMemoryKbField;
		}
		set
		{
			grantedMemoryKbField = value;
		}
	}

	[XmlIgnore]
	internal bool GrantedMemoryKbSpecified
	{
		get
		{
			return grantedMemoryKbFieldSpecified;
		}
		set
		{
			grantedMemoryKbFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal ulong UsedMemoryKb
	{
		get
		{
			return usedMemoryKbField;
		}
		set
		{
			usedMemoryKbField = value;
		}
	}

	[XmlIgnore]
	internal bool UsedMemoryKbSpecified
	{
		get
		{
			return usedMemoryKbFieldSpecified;
		}
		set
		{
			usedMemoryKbFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal ulong WritesToTempDb
	{
		get
		{
			return writesToTempDbField;
		}
		set
		{
			writesToTempDbField = value;
		}
	}

	[XmlIgnore]
	internal bool WritesToTempDbSpecified
	{
		get
		{
			return writesToTempDbFieldSpecified;
		}
		set
		{
			writesToTempDbFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal ulong ReadsFromTempDb
	{
		get
		{
			return readsFromTempDbField;
		}
		set
		{
			readsFromTempDbField = value;
		}
	}

	[XmlIgnore]
	internal bool ReadsFromTempDbSpecified
	{
		get
		{
			return readsFromTempDbFieldSpecified;
		}
		set
		{
			readsFromTempDbFieldSpecified = value;
		}
	}
}
