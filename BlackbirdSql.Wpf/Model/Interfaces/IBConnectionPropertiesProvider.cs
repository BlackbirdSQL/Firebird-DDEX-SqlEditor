// Microsoft.SqlServer.ConnectionDlg.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.Core.IConnectionPropertiesProvider

using BlackbirdSql.Core.Ctl.Interfaces;

namespace BlackbirdSql.Wpf.Model.Interfaces;

public interface IBConnectionPropertiesProvider : IBExportable
{
	IBPropertyAgent GetConnectionProperties(string connectionString);
}
