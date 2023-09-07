// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.ThreadStatType
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
public class ThreadStatType
{
	private ThreadReservationType[] threadReservationField;

	private int branchesField;

	private int usedThreadsField;

	private bool usedThreadsFieldSpecified;

	[XmlElement("ThreadReservation")]
	public ThreadReservationType[] ThreadReservation
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
	public int Branches
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
	public int UsedThreads
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
	public bool UsedThreadsSpecified
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
