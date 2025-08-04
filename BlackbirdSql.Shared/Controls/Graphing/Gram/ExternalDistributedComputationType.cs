// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.ExternalDistributedComputationType
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
internal class ExternalDistributedComputationType
{
	private StmtSimpleType[] itemsField;

	private string edcShowplanXmlField;

	[XmlElement("StmtSimple")]
	internal StmtSimpleType[] Items
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

	[XmlAttribute]
	internal string EdcShowplanXml
	{
		get
		{
			return edcShowplanXmlField;
		}
		set
		{
			edcShowplanXmlField = value;
		}
	}
}
