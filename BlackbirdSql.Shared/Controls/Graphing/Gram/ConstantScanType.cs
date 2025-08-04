// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.ConstantScanType
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
internal class ConstantScanType : RelOpBaseType
{
	private ScalarType[][] valuesField;

	[XmlArrayItem("Row", IsNullable = false)]
	[XmlArrayItem("ScalarOperator", IsNullable = false, NestingLevel = 1)]
	internal ScalarType[][] Values
	{
		get
		{
			return valuesField;
		}
		set
		{
			valuesField = value;
		}
	}
}
