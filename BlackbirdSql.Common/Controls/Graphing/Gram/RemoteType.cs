// Warning: Some assembly references could not be resolved automatically. This might lead to incorrect decompilation of some parts,
// for ex. property getter/setter access. To get optimal decompilation results, please manually add the missing references to the list of loaded assemblies.
// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.RemoteType
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace BlackbirdSql.Common.Controls.Graphing.Gram;

[Serializable]
[XmlInclude(typeof(RemoteQueryType))]
[XmlInclude(typeof(PutType))]
[XmlInclude(typeof(RemoteModifyType))]
[XmlInclude(typeof(RemoteFetchType))]
[XmlInclude(typeof(RemoteRangeType))]
[GeneratedCode("xsd", "4.8.3928.0")]
[DebuggerStepThrough]
[DesignerCategory("code")]
[XmlType(Namespace = "http://schemas.microsoft.com/sqlserver/2004/07/showplan")]
public class RemoteType : RelOpBaseType
{
	private string remoteDestinationField;

	private string remoteSourceField;

	private string remoteObjectField;

	[XmlAttribute]
	public string RemoteDestination
	{
		get
		{
			return remoteDestinationField;
		}
		set
		{
			remoteDestinationField = value;
		}
	}

	[XmlAttribute]
	public string RemoteSource
	{
		get
		{
			return remoteSourceField;
		}
		set
		{
			remoteSourceField = value;
		}
	}

	[XmlAttribute]
	public string RemoteObject
	{
		get
		{
			return remoteObjectField;
		}
		set
		{
			remoteObjectField = value;
		}
	}
}
