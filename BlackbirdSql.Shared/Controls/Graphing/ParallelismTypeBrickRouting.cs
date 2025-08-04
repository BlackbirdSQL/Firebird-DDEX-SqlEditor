// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.ParallelismTypeBrickRouting
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
internal class ParallelismTypeBrickRouting
{
	private ObjectType objectField;

	private SingleColumnReferenceType fragmentIdColumnField;

	internal ObjectType Object
	{
		get
		{
			return objectField;
		}
		set
		{
			objectField = value;
		}
	}

	internal SingleColumnReferenceType FragmentIdColumn
	{
		get
		{
			return fragmentIdColumnField;
		}
		set
		{
			fragmentIdColumnField = value;
		}
	}
}
