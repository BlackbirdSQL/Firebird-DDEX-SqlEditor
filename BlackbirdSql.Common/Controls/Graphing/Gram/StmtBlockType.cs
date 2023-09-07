// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.StmtBlockType
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
public class StmtBlockType
{
	private object[] itemsField;

	[XmlElement("ExternalDistributedComputation", typeof(ExternalDistributedComputationType))]
	[XmlElement("StmtCond", typeof(StmtCondType))]
	[XmlElement("StmtCursor", typeof(StmtCursorType))]
	[XmlElement("StmtReceive", typeof(StmtReceiveType))]
	[XmlElement("StmtSimple", typeof(StmtSimpleType))]
	[XmlElement("StmtUseDb", typeof(StmtUseDbType))]
	public object[] Items
	{
		get
		{
			return itemsField;
		}
		set
		{
			itemsField = value;
		}
	}
}
