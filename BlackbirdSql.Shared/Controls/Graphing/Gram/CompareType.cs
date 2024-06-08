// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.CompareType
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
public class CompareType
{
	private ScalarType[] scalarOperatorField;

	private EnCompareOpType compareOpField;

	[XmlElement("ScalarOperator")]
	public ScalarType[] ScalarOperator
	{
		get
		{
			return scalarOperatorField;
		}
		set
		{
			scalarOperatorField = value;
		}
	}

	[XmlAttribute]
	public EnCompareOpType CompareOp
	{
		get
		{
			return compareOpField;
		}
		set
		{
			compareOpField = value;
		}
	}
}
