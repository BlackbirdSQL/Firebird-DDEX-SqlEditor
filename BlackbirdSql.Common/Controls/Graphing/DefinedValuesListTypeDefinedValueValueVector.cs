// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.DefinedValuesListTypeDefinedValueValueVector
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;
using BlackbirdSql.Common.Controls.Graphing.Gram;

namespace BlackbirdSql.Common.Controls.Graphing;

[Serializable]
[GeneratedCode("xsd", "4.8.3928.0")]
[DebuggerStepThrough]
[DesignerCategory("code")]
[XmlType(AnonymousType = true, Namespace = LibraryData.C_ShowPlanNamespace)]
public class DefinedValuesListTypeDefinedValueValueVector
{
	private ColumnReferenceType[] columnReferenceField;

	[XmlElement("ColumnReference")]
	public ColumnReferenceType[] ColumnReference
	{
		get
		{
			return columnReferenceField;
		}
		set
		{
			columnReferenceField = value;
		}
	}
}
