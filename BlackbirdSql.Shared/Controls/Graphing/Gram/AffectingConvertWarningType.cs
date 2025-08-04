// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.AffectingConvertWarningType
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;
using BlackbirdSql.Shared.Controls.Graphing.Enums;

namespace BlackbirdSql.Shared.Controls.Graphing.Gram;

[Serializable]
[GeneratedCode("xsd", "4.8.3928.0")]
[DebuggerStepThrough]
[DesignerCategory("code")]
[XmlType(Namespace = LibraryData.C_ShowPlanNamespace)]
internal class AffectingConvertWarningType
{
	private EnAffectingConvertWarningTypeConvertIssue convertIssueField;

	private string expressionField;

	[XmlAttribute]
	internal EnAffectingConvertWarningTypeConvertIssue ConvertIssue
	{
		get
		{
			return convertIssueField;
		}
		set
		{
			convertIssueField = value;
		}
	}

	[XmlAttribute]
	internal string Expression
	{
		get
		{
			return expressionField;
		}
		set
		{
			expressionField = value;
		}
	}
}
