// Microsoft.SqlServer.ConnectionDlg.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.Core.ServerTypes


namespace BlackbirdSql.Core.Ctl.Enums;

public enum EnEngineType
{
	Unknown = 0,
	LocalClassicServer,
	LocalSuperClassic,
	LocalSuperServer,
	ClassicServer,
	SuperClassic,
	SuperServer,
	EmbeddedDatabase
}

