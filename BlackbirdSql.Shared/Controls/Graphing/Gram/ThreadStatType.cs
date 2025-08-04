// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.ThreadStatType
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
internal class ThreadStatType
{
	private ThreadReservationType[] threadReservationField;

	private int branchesField;

	private int usedThreadsField;

	private bool usedThreadsFieldSpecified;

	[XmlElement("ThreadReservation")]
	internal ThreadReservationType[] ThreadReservation
	{
		get
		{
			return threadReservationField;
		}
		set
		{
			threadReservationField = value;
		}
	}

	[XmlAttribute]
	internal int Branches
	{
		get
		{
			return branchesField;
		}
		set
		{
			branchesField = value;
		}
	}

	[XmlAttribute]
	internal int UsedThreads
	{
		get
		{
			return usedThreadsField;
		}
		set
		{
			usedThreadsField = value;
		}
	}

	[XmlIgnore]
	internal bool UsedThreadsSpecified
	{
		get
		{
			return usedThreadsFieldSpecified;
		}
		set
		{
			usedThreadsFieldSpecified = value;
		}
	}
}
