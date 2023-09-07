// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.ColumnGroupType
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;
using BlackbirdSql.Common.Controls.Graphing.Enums;

namespace BlackbirdSql.Common.Controls.Graphing.Gram;

[Serializable]
[GeneratedCode("xsd", "4.8.3928.0")]
[DebuggerStepThrough]
[DesignerCategory("code")]
[XmlType(Namespace = LibraryData.C_ShowPlanNamespace)]
public class ColumnGroupType
{
	private ColumnType[] columnField;

	private EnColumnGroupTypeUsage usageField;

	[XmlElement("Column")]
	public ColumnType[] Column
	{
		get
		{
			return columnField;
		}
		set
		{
			columnField = value;
		}
	}

	[XmlAttribute]
	public EnColumnGroupTypeUsage Usage
	{
		get
		{
			return usageField;
		}
		set
		{
			usageField = value;
		}
	}
}
