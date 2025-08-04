// Warning: Some assembly references could not be resolved automatically. This might lead to incorrect decompilation of some parts,
// for ex. property getter/setter access. To get optimal decompilation results, please manually add the missing references to the list of loaded assemblies.
// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.StmtCondType
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
internal class StmtCondType : AbstractStmtInfoType
{
	private StmtCondTypeCondition conditionField;

	private StmtCondTypeThen thenField;

	private StmtCondTypeElse elseField;

	internal StmtCondTypeCondition Condition
	{
		get
		{
			return conditionField;
		}
		set
		{
			conditionField = value;
		}
	}

	internal StmtCondTypeThen Then
	{
		get
		{
			return thenField;
		}
		set
		{
			thenField = value;
		}
	}

	internal StmtCondTypeElse Else
	{
		get
		{
			return elseField;
		}
		set
		{
			elseField = value;
		}
	}
}
