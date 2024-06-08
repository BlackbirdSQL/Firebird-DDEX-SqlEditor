// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.BatchHashTableBuildType
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
public class BatchHashTableBuildType : RelOpBaseType
{
	private RelOpType relOpField;

	private bool bitmapCreatorField;

	private bool bitmapCreatorFieldSpecified;

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
