// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.ComputeScalarType
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
public class ComputeScalarType : RelOpBaseType
{
	private RelOpType relOpField;

	private bool computeSequenceField;

	private bool computeSequenceFieldSpecified;

	public RelOpType RelOp
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
	public bool ComputeSequence
	{
		get
		{
			return computeSequenceField;
		}
		set
		{
			computeSequenceField = value;
		}
	}

	[XmlIgnore]
	public bool ComputeSequenceSpecified
	{
		get
		{
			return computeSequenceFieldSpecified;
		}
		set
		{
			computeSequenceFieldSpecified = value;
		}
	}
}
