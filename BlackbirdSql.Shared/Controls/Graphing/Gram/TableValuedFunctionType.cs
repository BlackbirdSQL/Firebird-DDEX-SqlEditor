// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.TableValuedFunctionType
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
internal class TableValuedFunctionType : RelOpBaseType
{
	private ObjectType objectField;

	private ScalarExpressionType predicateField;

	private RelOpType relOpField;

	private ScalarType[] parameterListField;

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

	internal ScalarExpressionType Predicate
	{
		get
		{
			return predicateField;
		}
		set
		{
			predicateField = value;
		}
	}

	internal RelOpType RelOp
	{
		get
		{
			return relOpField;
		}
		set
		{
			relOpField = value;
		}
	}

	[XmlArrayItem("ScalarOperator", IsNullable = false)]
	internal ScalarType[] ParameterList
	{
		get
		{
			return parameterListField;
		}
		set
		{
			parameterListField = value;
		}
	}
}
