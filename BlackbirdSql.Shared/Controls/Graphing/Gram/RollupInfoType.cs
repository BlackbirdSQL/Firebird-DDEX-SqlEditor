// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.RollupInfoType
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
public class RollupInfoType
{
	private RollupLevelType[] rollupLevelField;

	private int highestLevelField;

	[XmlElement("RollupLevel")]
	public RollupLevelType[] RollupLevel
	{
		get
		{
			return rollupLevelField;
		}
		set
		{
			rollupLevelField = value;
		}
	}

	[XmlAttribute]
	public int HighestLevel
	{
		get
		{
			return highestLevelField;
		}
		set
		{
			highestLevelField = value;
		}
	}
}
