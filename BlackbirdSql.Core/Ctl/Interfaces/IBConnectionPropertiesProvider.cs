// Microsoft.SqlServer.ConnectionDlg.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.Core.IConnectionPropertiesProvider

namespace BlackbirdSql.Core.Ctl.Interfaces;

public interface IConnectionPropertiesProvider : IBExportable
{
	IBPropertyAgent GetConnectionProperties(string connectionString);
}
