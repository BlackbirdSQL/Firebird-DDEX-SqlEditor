// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.DefinedValuesListTypeDefinedValue
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;
using BlackbirdSql.Shared.Controls.Graphing.Gram;

namespace BlackbirdSql.Shared.Controls.Graphing;

[Serializable]
[GeneratedCode("xsd", "4.8.3928.0")]
[DebuggerStepThrough]
[DesignerCategory("code")]
[XmlType(AnonymousType = true, Namespace = LibraryData.C_ShowPlanNamespace)]
public class DefinedValuesListTypeDefinedValue
{
	private object itemField;

	private object[] itemsField;

	[XmlElement("ColumnReference", typeof(ColumnReferenceType), Order = 0)]
	[XmlElement("ValueVector", typeof(DefinedValuesListTypeDefinedValueValueVector), Order = 0)]
	public object Item
	{
		get
		{
			return itemField;
		}
		set
		{
			itemField = value;
		}
	}

	[XmlElement("ColumnReference", typeof(ColumnReferenceType), Order = 1)]
	[XmlElement("ScalarOperator", typeof(ScalarType), Order = 1)]
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
