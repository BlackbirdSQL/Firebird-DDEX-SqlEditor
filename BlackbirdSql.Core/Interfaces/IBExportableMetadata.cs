// Microsoft.SqlServer.ConnectionDlg.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.Core.Extensibility.IExportableMetadata


namespace BlackbirdSql.Core.Interfaces;

public interface IBExportableMetadata : IBStandardMetadata, IBServerDefinition
{
	int Priority { get; }
}
