// Microsoft.SqlServer.ConnectionDlg.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.Core.Extensibility.ExportableAttribute
using System;
using System.ComponentModel.Composition;

using BlackbirdSql.Core.Ctl.Enums;
using BlackbirdSql.Core.Ctl.Interfaces;


namespace BlackbirdSql.Core.Ctl.ComponentModel;

[MetadataAttribute]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ExportableAttribute : ExportAttribute, IBExportableMetadata, IBStandardMetadata, IBServerDefinition
{
	public string EngineProduct { get; private set; }

	public EnEngineType EngineType { get; private set; }

	public string Key { get; set; }

	public string Id { get; private set; }

	public string DisplayName { get; private set; }

	public int Priority { get; private set; }

	public ExportableAttribute(string engineProduct, EnEngineType engineType, Type type, string id, int priority = 0, string displayName = null)
		: base(type)
	{
		EngineProduct = engineProduct;
		EngineType = engineType;
		Id = id;
		DisplayName = displayName;
		Priority = priority;
	}

	public ExportableAttribute(Type type, string id, int priority = 0, string displayName = null)
		: this(string.Empty, EnEngineType.Unknown, type, id, priority, displayName)
	{
	}
}
