// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.ThreadReservationType
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
public class ThreadReservationType
{
	private int nodeIdField;

	private bool nodeIdFieldSpecified;

	private int reservedThreadsField;

	[XmlAttribute]
	public int NodeId
	{
		get
		{
			return nodeIdField;
		}
		set
		{
			nodeIdField = value;
		}
	}

	[XmlIgnore]
	public bool NodeIdSpecified
	{
		get
		{
			return nodeIdFieldSpecified;
		}
		set
		{
			nodeIdFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public int ReservedThreads
	{
		get
		{
			return reservedThreadsField;
		}
		set
		{
			reservedThreadsField = value;
		}
	}
}
