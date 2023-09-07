// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.OrderByTypeOrderByColumn
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
[XmlType(AnonymousType = true, Namespace = "http://schemas.microsoft.com/sqlserver/2004/07/showplan")]
public class OrderByTypeOrderByColumn
{
	private ColumnReferenceType columnReferenceField;

	private bool ascendingField;

	public ColumnReferenceType ColumnReference
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

	[XmlAttribute]
	public bool Ascending
	{
		get
		{
			return ascendingField;
		}
		set
		{
			ascendingField = value;
		}
	}
}
