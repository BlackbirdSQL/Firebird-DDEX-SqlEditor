// Microsoft.Data.Tools.Components, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.Data.Tools.Components.Diagnostics.SqlTraceId

namespace BlackbirdSql.Core.Ctl.Enums;

public enum EnSqlTraceId : uint
{
	CoreServices = 0u,
	TSqlModel = 1u,
	LanguageServices = 2u,
	VSShell = 3u,
	EntityDesigner = 4u,
	EntityDesigner_ForwardIntegration = 5u,
	EntityDesigner_GenerateFromModel = 6u,
	EntityDesigner_ModelChanges = 7u,
	EntityDesigner_OData = 8u,
	EntityDesigner_ReverseIntegration = 9u,
	TableDesigner = 10u,
	TableDesigner_ReadFromModel = 11u,
	TableDesigner_WriteToModel = 12u,
	AppDB = 13u,
	QueryResults = 14u,
	Debugger = 15u,
	SqlEditorAndLanguageServices = 16u,
	SchemaCompare = 17u,
	CommandlineTooling = 18u,
	DacApi = 19u,
	PdwExtensions = 20u,
	UnitTesting = 21u,
	Extensibility = 22u,
	DataCompare = 23u,
	Telemetry = 24u,
	ConnectionDialog = 25u,
	AlwaysEncryptedKeysDialog = 26u,
	TabEditor = 27u,
	InternalTest = 99u
}
