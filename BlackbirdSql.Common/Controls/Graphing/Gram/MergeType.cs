// Warning: Some assembly references could not be resolved automatically. This might lead to incorrect decompilation of some parts,
// for ex. property getter/setter access. To get optimal decompilation results, please manually add the missing references to the list of loaded assemblies.
// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.MergeType
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace BlackbirdSql.Common.Controls.Graphing.Gram;

[Serializable]
[GeneratedCode("xsd", "4.8.3928.0")]
[DebuggerStepThrough]
[DesignerCategory("code")]
[XmlType(Namespace = "http://schemas.microsoft.com/sqlserver/2004/07/showplan")]
public class MergeType : RelOpBaseType
{
	private ColumnReferenceType[] innerSideJoinColumnsField;

	private ColumnReferenceType[] outerSideJoinColumnsField;

	private ScalarExpressionType residualField;

	private ScalarExpressionType passThruField;

	private StarJoinInfoType starJoinInfoField;

	private RelOpType[] relOpField;

	private bool manyToManyField;

	private bool manyToManyFieldSpecified;

	[XmlArrayItem("ColumnReference", IsNullable = false)]
	public ColumnReferenceType[] InnerSideJoinColumns
	{
		get
		{
			return innerSideJoinColumnsField;
		}
		set
		{
			innerSideJoinColumnsField = value;
		}
	}

	[XmlArrayItem("ColumnReference", IsNullable = false)]
	public ColumnReferenceType[] OuterSideJoinColumns
	{
		get
		{
			return outerSideJoinColumnsField;
		}
		set
		{
			outerSideJoinColumnsField = value;
		}
	}

	public ScalarExpressionType Residual
	{
		get
		{
			return residualField;
		}
		set
		{
			residualField = value;
		}
	}

	public ScalarExpressionType PassThru
	{
		get
		{
			return passThruField;
		}
		set
		{
			passThruField = value;
		}
	}

	public StarJoinInfoType StarJoinInfo
	{
		get
		{
			return starJoinInfoField;
		}
		set
		{
			starJoinInfoField = value;
		}
	}

	[XmlElement("RelOp")]
	public RelOpType[] RelOp
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

	[XmlAttribute]
	public bool ManyToMany
	{
		get
		{
			return manyToManyField;
		}
		set
		{
			manyToManyField = value;
		}
	}

	[XmlIgnore]
	public bool ManyToManySpecified
	{
		get
		{
			return manyToManyFieldSpecified;
		}
		set
		{
			manyToManyFieldSpecified = value;
		}
	}
}
