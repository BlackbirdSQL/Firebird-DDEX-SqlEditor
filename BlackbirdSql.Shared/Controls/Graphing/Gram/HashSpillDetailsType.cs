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
public class HashSpillDetailsType
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
	public ulong GrantedMemoryKb
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
	public bool GrantedMemoryKbSpecified
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
	public ulong UsedMemoryKb
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
	public bool UsedMemoryKbSpecified
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
	public ulong WritesToTempDb
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
	public bool WritesToTempDbSpecified
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
	public ulong ReadsFromTempDb
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
	public bool ReadsFromTempDbSpecified
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
