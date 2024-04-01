// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.HashType
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
[XmlType(Namespace = LibraryData.C_ShowPlanNamespace)]
public class HashType : RelOpBaseType
{
	private ColumnReferenceType[] hashKeysBuildField;

	private ColumnReferenceType[] hashKeysProbeField;

	private ScalarExpressionType buildResidualField;

	private ScalarExpressionType probeResidualField;

	private StarJoinInfoType starJoinInfoField;

	private RelOpType[] relOpField;

	private bool bitmapCreatorField;

	private bool bitmapCreatorFieldSpecified;

	[XmlArrayItem("ColumnReference", IsNullable = false)]
	public ColumnReferenceType[] HashKeysBuild
	{
		get
		{
			return hashKeysBuildField;
		}
		set
		{
			hashKeysBuildField = value;
		}
	}

	[XmlArrayItem("ColumnReference", IsNullable = false)]
	public ColumnReferenceType[] HashKeysProbe
	{
		get
		{
			return hashKeysProbeField;
		}
		set
		{
			hashKeysProbeField = value;
		}
	}

	public ScalarExpressionType BuildResidual
	{
		get
		{
			return buildResidualField;
		}
		set
		{
			buildResidualField = value;
		}
	}

	public ScalarExpressionType ProbeResidual
	{
		get
		{
			return probeResidualField;
		}
		set
		{
			probeResidualField = value;
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
	public bool BitmapCreator
	{
		get
		{
			return bitmapCreatorField;
		}
		set
		{
			bitmapCreatorField = value;
		}
	}

	[XmlIgnore]
	public bool BitmapCreatorSpecified
	{
		get
		{
			return bitmapCreatorFieldSpecified;
		}
		set
		{
			bitmapCreatorFieldSpecified = value;
		}
	}
}
