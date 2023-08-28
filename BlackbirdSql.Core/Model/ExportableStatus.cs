// Microsoft.SqlServer.ConnectionDlg.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.Core.Extensibility.ExportableStatus


namespace BlackbirdSql.Core.Model;

public class ExportableStatus
{
	public bool LoadingFailed { get; set; }

	public string ErrorMessage { get; set; }

	public string InfoLink { get; set; }
}
