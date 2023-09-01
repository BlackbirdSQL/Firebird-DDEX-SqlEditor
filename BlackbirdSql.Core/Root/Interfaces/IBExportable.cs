// Microsoft.SqlServer.ConnectionDlg.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.Core.Extensibility.IExportable

using BlackbirdSql.Core.Model;


namespace BlackbirdSql.Core.Interfaces;

public interface IBExportable
{
	IBExportableMetadata Metadata { get; set; }

	IBDependencyManager DependencyManager { get; set; }

	ExportableStatus Status { get; }
}
