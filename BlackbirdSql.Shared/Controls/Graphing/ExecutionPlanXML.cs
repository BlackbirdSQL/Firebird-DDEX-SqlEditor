// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.ShowPlanXML
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
[XmlRoot(Namespace = LibraryData.C_ShowPlanNamespace, IsNullable = false)]
internal class ExecutionPlanXML
{
	private StmtBlockType[][] batchSequenceField;

	private string versionField;

	private string buildField;

	private bool clusteredModeField;

	private bool clusteredModeFieldSpecified;

	[XmlArrayItem("Batch", IsNullable = false)]
	[XmlArrayItem("Statements", IsNullable = false, NestingLevel = 1)]
	internal StmtBlockType[][] BatchSequence
	{
		get
		{
			return batchSequenceField;
		}
		set
		{
			batchSequenceField = value;
		}
	}

	[XmlAttribute]
	internal string Version
	{
		get
		{
			return versionField;
		}
		set
		{
			versionField = value;
		}
	}

	[XmlAttribute]
	internal string Build
	{
		get
		{
			return buildField;
		}
		set
		{
			buildField = value;
		}
	}

	[XmlAttribute]
	internal bool ClusteredMode
	{
		get
		{
			return clusteredModeField;
		}
		set
		{
			clusteredModeField = value;
		}
	}

	[XmlIgnore]
	internal bool ClusteredModeSpecified
	{
		get
		{
			return clusteredModeFieldSpecified;
		}
		set
		{
			clusteredModeFieldSpecified = value;
		}
	}
}
